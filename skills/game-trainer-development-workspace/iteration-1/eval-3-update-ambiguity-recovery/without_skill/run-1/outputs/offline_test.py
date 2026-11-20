"""Synthetic byte-buffer protocol tests; no process APIs or third-party packages.
Run: python offline_test.py
"""
import hashlib
import json
import struct
import sys
import time
import unittest
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path


class ProtocolError(Exception):
    pass


@dataclass
class Image:
    build: str
    session: str
    base: int
    data: bytearray


@dataclass(frozen=True)
class Profile:
    build: str
    image_hash: str
    size: int
    code_start: int = 16
    code_end: int = 112
    target_start: int = 128
    target_end: int = 192


@dataclass(frozen=True)
class Lease:
    session: str
    build: str
    base: int
    rva: int
    original: bytes
    target_rva: int


# Synthetic x64 fixture: mov rax, qword ptr [rip+disp32]; test rax,rax.
# Only displacement bytes are wildcarded. Decoder handles this exact form only.
PREFIX = b'\x48\x8b\x05'
SUFFIX = b'\x48\x85\xc0'
PATCH = b'\x90' * 7


def decode(data, rva):
    if rva < 0 or rva + 10 > len(data):
        raise ProtocolError('instruction_out_of_bounds')
    if bytes(data[rva:rva + 3]) != PREFIX or bytes(data[rva + 7:rva + 10]) != SUFFIX:
        raise ProtocolError('unexpected_instruction')
    displacement = struct.unpack_from('<i', data, rva + 3)[0]
    return rva + 7 + displacement


def select(image, profile):
    if image.build != profile.build or len(image.data) != profile.size:
        raise ProtocolError('build_mismatch')
    if hashlib.sha256(image.data).hexdigest() != profile.image_hash:
        raise ProtocolError('image_hash_mismatch')
    raw = [rva for rva in range(profile.code_start, profile.code_end - 9)
           if bytes(image.data[rva:rva + 3]) == PREFIX
           and bytes(image.data[rva + 7:rva + 10]) == SUFFIX]
    valid = [rva for rva in raw
             if profile.target_start <= decode(image.data, rva) <= profile.target_end - 8]
    if len(valid) != 1:
        raise ProtocolError(f'candidate_count:{len(valid)};raw:{len(raw)}')
    rva = valid[0]
    return Lease(image.session, image.build, image.base, rva,
                 bytes(image.data[rva:rva + 7]), decode(image.data, rva))


class Controller:
    """Single-owner serialized buffer transaction, modeling a stopped target."""
    def __init__(self, image, lease):
        self.image = image
        self.lease = lease
        self.state = 'ready'
        self.events = []

    def validate_session(self):
        current = (self.image.session, self.image.build, self.image.base)
        saved = (self.lease.session, self.lease.build, self.lease.base)
        if current != saved:
            self.state = 'stale'
            raise ProtocolError('stale_session')
        if self.state == 'stale':
            raise ProtocolError('stale_session')
        if self.state == 'conflict':
            raise ProtocolError('conflict_requires_revalidation')

    def transition(self, enable):
        self.validate_session()
        rva = self.lease.rva
        current = bytes(self.image.data[rva:rva + 7])
        expected = PATCH if self.state == 'enabled' else self.lease.original
        if current != expected:
            self.state = 'conflict'
            raise ProtocolError('byte_conflict')
        if enable and self.state == 'enabled':
            return 'already_enabled'
        if not enable and self.state == 'ready':
            return 'already_disabled'
        if enable:
            if decode(self.image.data, rva) != self.lease.target_rva:
                raise ProtocolError('target_changed')
            replacement = PATCH
        else:
            replacement = self.lease.original
        self.image.data[rva:rva + 7] = replacement
        if bytes(self.image.data[rva:rva + 7]) != replacement:
            self.state = 'conflict'
            raise ProtocolError('readback_failed')
        self.state = 'enabled' if enable else 'ready'
        self.events.append({'operation': 'enable' if enable else 'disable',
                            'rva': rva, 'before': current.hex(), 'after': replacement.hex()})
        return self.state


def fixture(sites=(32,), targets=None, build='old', session='session-A', base=0x140000000):
    data = bytearray(b'\xcc' * 256)
    targets = targets if targets is not None else [160] * len(sites)
    for rva, target in zip(sites, targets):
        data[rva:rva + 10] = PREFIX + struct.pack('<i', target - (rva + 7)) + SUFFIX
    image = Image(build, session, base, data)
    return image, Profile(build, hashlib.sha256(data).hexdigest(), len(data))


class OfflineTests(unittest.TestCase):
    def test_old_build_unique(self):
        image, profile = fixture()
        lease = select(image, profile)
        self.assertEqual((lease.rva, lease.target_rva), (32, 160))
        self.assertEqual(image.base + lease.target_rva, 0x1400000A0)

    def test_zero_candidates_no_write(self):
        image, profile = fixture((), build='other')
        before = bytes(image.data)
        with self.assertRaisesRegex(ProtocolError, 'candidate_count:0;raw:0'):
            select(image, profile)
        self.assertEqual(bytes(image.data), before)

    def test_new_build_ambiguous_no_first_match(self):
        image, profile = fixture((32, 64), build='new')
        before = bytes(image.data)
        with self.assertRaisesRegex(ProtocolError, 'candidate_count:2;raw:2'):
            select(image, profile)
        self.assertEqual(bytes(image.data), before)

    def test_semantic_filter_resolves_one_candidate(self):
        image, profile = fixture((32, 64), (160, 240), build='new')
        self.assertEqual(select(image, profile).rva, 32)

    def test_all_targets_invalid(self):
        image, profile = fixture((32,), (240,))
        with self.assertRaisesRegex(ProtocolError, 'candidate_count:0;raw:1'):
            select(image, profile)

    def test_build_and_hash_are_required(self):
        image, profile = fixture()
        image.build = 'new'
        with self.assertRaisesRegex(ProtocolError, 'build_mismatch'):
            select(image, profile)
        image.build = 'old'
        image.data[200] ^= 1
        with self.assertRaisesRegex(ProtocolError, 'image_hash_mismatch'):
            select(image, profile)

    def test_signed_rip_and_expected_instruction(self):
        data = bytearray(256)
        data[96:106] = PREFIX + struct.pack('<i', -71) + SUFFIX
        self.assertEqual(decode(data, 96), 32)
        data[96] = 0x90
        with self.assertRaisesRegex(ProtocolError, 'unexpected_instruction'):
            decode(data, 96)
        with self.assertRaisesRegex(ProtocolError, 'instruction_out_of_bounds'):
            decode(data, 250)

    def test_enable_disable_exact_restoration(self):
        image, profile = fixture()
        before = bytes(image.data)
        controller = Controller(image, select(image, profile))
        self.assertEqual(controller.transition(False), 'already_disabled')
        self.assertEqual(controller.transition(True), 'enabled')
        self.assertEqual(bytes(image.data[32:39]), PATCH)
        self.assertEqual(controller.transition(True), 'already_enabled')
        self.assertEqual(controller.transition(False), 'ready')
        self.assertEqual(bytes(image.data), before)
        self.assertEqual([e['operation'] for e in controller.events], ['enable', 'disable'])

    def test_enable_conflict_does_not_write(self):
        image, profile = fixture()
        controller = Controller(image, select(image, profile))
        image.data[32] = 0x41
        before = bytes(image.data)
        with self.assertRaisesRegex(ProtocolError, 'byte_conflict'):
            controller.transition(True)
        self.assertEqual(bytes(image.data), before)

    def test_restore_conflict_preserves_other_component(self):
        image, profile = fixture()
        controller = Controller(image, select(image, profile))
        controller.transition(True)
        image.data[34] = 0x41
        before = bytes(image.data)
        with self.assertRaisesRegex(ProtocolError, 'byte_conflict'):
            controller.transition(False)
        self.assertEqual(bytes(image.data), before)
        self.assertEqual(controller.state, 'conflict')
        with self.assertRaisesRegex(ProtocolError, 'conflict_requires_revalidation'):
            controller.transition(True)

    def test_restart_same_address_invalidates_enable_and_restore(self):
        for enabled in (False, True):
            with self.subTest(enabled=enabled):
                image, profile = fixture()
                controller = Controller(image, select(image, profile))
                if enabled:
                    controller.transition(True)
                image.session = 'session-B'
                before = bytes(image.data)
                with self.assertRaisesRegex(ProtocolError, 'stale_session'):
                    controller.transition(not enabled)
                self.assertEqual(bytes(image.data), before)

    def test_module_reload_and_rebase_invalidate(self):
        for field, value in (('base', 0x150000000), ('build', 'new')):
            with self.subTest(field=field):
                image, profile = fixture()
                controller = Controller(image, select(image, profile))
                setattr(image, field, value)
                with self.assertRaisesRegex(ProtocolError, 'stale_session'):
                    controller.transition(True)

    def test_new_session_fresh_resolution(self):
        image, profile = fixture(session='session-B', base=0x150000000)
        controller = Controller(image, select(image, profile))
        self.assertEqual(controller.transition(True), 'enabled')
        self.assertEqual(controller.transition(False), 'ready')

    def test_context_instruction_change_blocks_enable(self):
        image, profile = fixture()
        controller = Controller(image, select(image, profile))
        image.data[39] = 0x90
        before = bytes(image.data)
        with self.assertRaisesRegex(ProtocolError, 'unexpected_instruction'):
            controller.transition(True)
        self.assertEqual(bytes(image.data), before)


if __name__ == '__main__':
    started_at = datetime.now(timezone.utc).isoformat()
    started = time.perf_counter()
    suite = unittest.defaultTestLoader.loadTestsFromTestCase(OfflineTests)
    result = unittest.TextTestRunner(stream=sys.stdout, verbosity=2).run(suite)
    summary = {'started_at_utc': started_at,
               'finished_at_utc': datetime.now(timezone.utc).isoformat(),
               'elapsed_seconds': time.perf_counter() - started,
               'tests_run': result.testsRun, 'failures': len(result.failures),
               'errors': len(result.errors), 'successful': result.wasSuccessful(),
               'python': sys.version, 'data_source': 'synthetic bytearray only',
               'real_process_attached': False, 'tokens': None,
               'tokens_note': 'not measured; no model token telemetry available'}
    Path(__file__).with_name('results.json').write_text(
        json.dumps(summary, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    raise SystemExit(0 if result.wasSuccessful() else 1)
