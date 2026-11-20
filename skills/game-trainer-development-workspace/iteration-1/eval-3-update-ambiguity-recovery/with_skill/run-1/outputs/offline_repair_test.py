"""Standard-library offline repair protocol. No process attachment or OS memory API."""
import dataclasses
import hashlib
import json
import struct
import sys
import unittest
from datetime import datetime, timezone

PATTERN = (0x48, 0x8B, 0x0D, None, None, None, None, 0x48, 0x85, 0xC9,
           0x0F, 0x85, None, None, None, None)
PATCH = b'\x90' * 6  # replaces one complete synthetic near-JNE instruction
BASE = 0x140000000
TEXT = (0x100, 0x280)
DATA = (0x300, 0x500)
EVENTS = []


class Rejected(Exception):
    pass


def require(condition, reason):
    if not condition:
        raise Rejected(reason)


def digest(data):
    return hashlib.sha256(data).hexdigest()


@dataclasses.dataclass(frozen=True)
class Session:
    pid: int
    start: int
    base: int
    build: str
    epoch: int


class Image:
    def __init__(self, data, session):
        self.data = bytearray(data)
        self.session = session
        self.writes = 0
        self.fail_once = False

    def read(self, rva, length):
        require(length >= 0 and 0 <= rva <= len(self.data) - length, 'read_bounds')
        result = bytes(self.data[rva:rva + length])
        require(len(result) == length, 'short_read')
        return result

    def write(self, rva, data):
        self.read(rva, len(data))
        self.writes += 1
        count = len(data) - 1 if self.fail_once else len(data)
        self.fail_once = False
        self.data[rva:rva + count] = data[:count]
        return count


def decode_rip(image, hit):
    require(image.read(hit, 3) == b'\x48\x8b\x0d', 'unexpected_instruction')
    disp = struct.unpack('<i', image.read(hit + 3, 4))[0]
    absolute = image.session.base + hit + 7 + disp
    require(0 <= absolute <= 0xFFFFFFFFFFFFFFFF, 'target_overflow')
    return absolute - image.session.base


def semantic(image, hit):
    require(TEXT[0] <= hit and hit + len(PATTERN) <= TEXT[1], 'code_region')
    require(image.read(hit + 7, 5) == b'\x48\x85\xc9\x0f\x85', 'instruction_context')
    root = decode_rip(image, hit)
    require(DATA[0] <= root <= DATA[1] - 8 and root % 8 == 0, 'root_region')
    obj = struct.unpack('<Q', image.read(root, 8))[0] - image.session.base
    require(DATA[0] <= obj <= DATA[1] - 20 and obj % 8 == 0, 'object_region')
    magic, owner, hp, max_hp, count = struct.unpack('<4sIiiI', image.read(obj, 20))
    require(magic == b'ACTR', 'object_magic')
    require(owner == 7, 'object_owner')
    require(0 <= hp <= max_hp <= 10000 and max_hp > 0, 'health_semantics')
    require(1 <= count <= 16, 'object_count')
    return root, obj


@dataclasses.dataclass(frozen=True)
class Resolved:
    session: Session
    hit: int
    root: int
    obj: int
    original: bytes


def resolve(image, approved, budget=TEXT[1] - TEXT[0]):
    require(image.session.build in approved, 'unapproved_build')
    require(digest(image.data) == image.session.build, 'image_identity_mismatch')
    require(budget >= TEXT[1] - TEXT[0], 'scan_budget_incomplete')
    hits = [i for i in range(TEXT[0], TEXT[1] - len(PATTERN) + 1)
            if all(p is None or image.data[i + j] == p for j, p in enumerate(PATTERN))]
    valid, rejected = [], []
    for hit in hits:
        try:
            root, obj = semantic(image, hit)
            valid.append(Resolved(image.session, hit, root, obj, image.read(hit + 10, 6)))
        except Rejected as exc:
            rejected.append({'hit': hex(hit), 'reason': str(exc)})
    EVENTS.append({'build': image.session.build, 'raw_hits': list(map(hex, hits)),
                   'valid_hits': [hex(v.hit) for v in valid], 'rejected': rejected})
    require(bool(hits), 'signature_zero')
    require(bool(valid), 'no_semantic_candidate')
    require(len(valid) == 1, 'semantic_ambiguity')
    return valid[0]


class Controller:
    def __init__(self, image, resolved):
        self.image, self.resolved = image, resolved
        self.state = 'ready'
        self.owned = False

    def check_session(self):
        if self.image.session != self.resolved.session:
            self.state = 'stale'
            raise Rejected('stale_session')

    def enable(self):
        self.check_session()
        require(self.state in ('ready', 'enabled'), 'faulted_controller')
        rva = self.resolved.hit + 10
        current = self.image.read(rva, 6)
        if self.owned:
            if current != PATCH:
                self.state = 'conflict'
                raise Rejected('enable_conflict')
            return
        semantic(self.image, self.resolved.hit)  # revalidate even cached resolution
        require(current == self.resolved.original, 'original_mismatch')
        written = self.image.write(rva, PATCH)
        if written != len(PATCH) or self.image.read(rva, 6) != PATCH:
            expected_partial = PATCH[:written] + current[written:]
            if self.image.read(rva, 6) == expected_partial:
                self.image.write(rva, current)
                require(self.image.read(rva, 6) == current, 'rollback_failed')
            self.state = 'faulted'
            raise Rejected('install_failed')
        self.owned, self.state = True, 'enabled'

    def disable(self):
        self.check_session()
        require(self.state in ('ready', 'enabled'), 'faulted_controller')
        if not self.owned:
            return
        rva = self.resolved.hit + 10
        if self.image.read(rva, 6) != PATCH:
            self.state = 'conflict'
            raise Rejected('restore_conflict')
        written = self.image.write(rva, self.resolved.original)
        if written != 6 or self.image.read(rva, 6) != self.resolved.original:
            self.state = 'faulted'
            raise Rejected('restore_failed')
        self.owned, self.state = False, 'ready'


def fixture(mode='old', pid=11, start=100, base=BASE, epoch=1):
    data = bytearray(0x500)
    data[:4] = b'SIM\0'

    def candidate(hit, root, obj, owner):
        data[hit:hit + 16] = (b'\x48\x8b\x0d' + struct.pack('<i', root - hit - 7)
                                 + b'\x48\x85\xc9\x0f\x85\x11\0\0\0')
        struct.pack_into('<Q', data, root, base + obj)
        struct.pack_into('<4sIiiI', data, obj, b'ACTR', owner, 73, 100, 1)

    if mode == 'old':
        candidate(0x120, 0x300, 0x380, 7)
    elif mode in ('new', 'ambiguous'):
        candidate(0x120, 0x300, 0x380, 9 if mode == 'new' else 7)
        candidate(0x1A0, 0x308, 0x3C0, 7)
    elif mode == 'zero':
        candidate(0x120, 0x300, 0x380, 7)
        data[0x120] = 0x49
    else:
        raise ValueError(mode)
    session = Session(pid, start, base, digest(data), epoch)
    return Image(data, session)


def locate(image):
    return resolve(image, {image.session.build})


class RepairTests(unittest.TestCase):
    def test_old_unique_and_new_multi_choose_semantic_second(self):
        old, new = fixture(), fixture('new')
        self.assertEqual(locate(old).hit, 0x120)
        selected = locate(new)
        self.assertEqual((selected.hit, selected.root, selected.obj), (0x1A0, 0x308, 0x3C0))
        self.assertEqual(len(EVENTS[-1]['raw_hits']), 2)
        self.assertEqual(EVENTS[-1]['rejected'][0]['reason'], 'object_owner')

    def test_zero_and_unresolved_multi_stop(self):
        for mode, reason in [('zero', 'signature_zero'), ('ambiguous', 'semantic_ambiguity')]:
            image = fixture(mode)
            with self.assertRaisesRegex(Rejected, reason):
                locate(image)
            self.assertEqual(image.writes, 0)

    def test_unique_still_requires_semantics(self):
        image = fixture()
        struct.pack_into('<i', image.data, 0x388, 101)
        image.session = dataclasses.replace(image.session, build=digest(image.data))
        with self.assertRaisesRegex(Rejected, 'no_semantic_candidate'):
            locate(image)
        self.assertEqual(EVENTS[-1]['rejected'][0]['reason'], 'health_semantics')

    def test_signed_rip_and_expected_opcode(self):
        image = fixture()
        self.assertEqual(decode_rip(image, 0x120), 0x300)
        image.data[0x200:0x207] = b'\x48\x8b\x0d' + struct.pack('<i', 0x80 - 0x207)
        self.assertEqual(decode_rip(image, 0x200), 0x80)
        image.data[0x200] = 0x49
        with self.assertRaisesRegex(Rejected, 'unexpected_instruction'):
            decode_rip(image, 0x200)

    def test_rip_outside_region_is_rejected(self):
        image = fixture()
        struct.pack_into('<i', image.data, 0x123, 0x700 - 0x127)
        image.session = dataclasses.replace(image.session, build=digest(image.data))
        with self.assertRaisesRegex(Rejected, 'no_semantic_candidate'):
            locate(image)
        self.assertEqual(EVENTS[-1]['rejected'][0]['reason'], 'root_region')

    def test_build_gate_and_bounded_scan(self):
        image = fixture('new')
        with self.assertRaisesRegex(Rejected, 'unapproved_build'):
            resolve(image, {fixture().session.build})
        with self.assertRaisesRegex(Rejected, 'scan_budget_incomplete'):
            resolve(image, {image.session.build}, budget=1)
        image.data[0] ^= 1
        with self.assertRaisesRegex(Rejected, 'image_identity_mismatch'):
            locate(image)

    def test_stale_session_all_identity_dimensions_no_write(self):
        for field, value in [('pid', 12), ('start', 101), ('base', BASE + 0x10000),
                             ('build', 'different-build'), ('epoch', 2)]:
            with self.subTest(field=field):
                image = fixture()
                controller = Controller(image, locate(image))
                controller.enable()
                image.session = dataclasses.replace(image.session, **{field: value})
                before, writes = bytes(image.data), image.writes
                for action in (controller.enable, controller.disable):
                    with self.assertRaisesRegex(Rejected, 'stale_session'):
                        action()
                self.assertEqual(bytes(image.data), before)
                self.assertEqual(image.writes, writes)
                self.assertEqual(controller.state, 'stale')

    def test_normal_repeated_enable_disable_and_exit_cleanup(self):
        for session in [dict(), dict(pid=22, start=200, base=BASE + 0x200000, epoch=3)]:
            image = fixture('new', **session)
            before = bytes(image.data)
            controller = Controller(image, locate(image))
            for _ in range(2):
                controller.enable()
                writes = image.writes
                controller.enable()
                self.assertEqual(image.writes, writes)
                self.assertEqual(image.read(0x1AA, 6), PATCH)
                controller.disable()  # same close path used for normal tool exit
                controller.disable()
                self.assertEqual(bytes(image.data), before)
                self.assertEqual(controller.state, 'ready')

    def test_original_mismatch_cached_semantics_and_partial_rollback(self):
        image = fixture()
        controller = Controller(image, locate(image))
        image.data[0x12C] ^= 1  # branch displacement changed after resolution
        with self.assertRaisesRegex(Rejected, 'original_mismatch'):
            controller.enable()
        self.assertEqual(image.writes, 0)
        image = fixture()
        controller = Controller(image, locate(image))
        struct.pack_into('<I', image.data, 0x384, 9)
        with self.assertRaisesRegex(Rejected, 'object_owner'):
            controller.enable()
        self.assertEqual(image.writes, 0)
        image = fixture()
        controller = Controller(image, locate(image))
        before = bytes(image.data)
        image.fail_once = True
        with self.assertRaisesRegex(Rejected, 'install_failed'):
            controller.enable()
        self.assertEqual(bytes(image.data), before)
        self.assertFalse(controller.owned)
        self.assertEqual(controller.state, 'faulted')

    def test_restore_conflict_preserves_other_component(self):
        image = fixture('new')
        controller = Controller(image, locate(image))
        controller.enable()
        image.data[0x1AA] = 0xCC
        conflict, writes = bytes(image.data), image.writes
        with self.assertRaisesRegex(Rejected, 'restore_conflict'):
            controller.disable()
        self.assertEqual(bytes(image.data), conflict)
        self.assertEqual(image.writes, writes)
        self.assertEqual(controller.state, 'conflict')
        with self.assertRaisesRegex(Rejected, 'faulted_controller'):
            controller.enable()
        EVENTS.append({'restore_conflict': image.read(0x1AA, 6).hex(),
                       'preserved': bytes(image.data) == conflict})

    def test_read_bounds(self):
        image = fixture()
        for rva, size in [(0x4FF, 2), (-1, 1), (0, -1)]:
            with self.assertRaisesRegex(Rejected, 'read_bounds'):
                image.read(rva, size)
        self.assertEqual(image.read(0x4FF, 1), b'\0')


if __name__ == '__main__':
    print('started_utc=' + datetime.now(timezone.utc).isoformat(), flush=True)
    print('python=' + sys.version.replace('\n', ' '), flush=True)
    print('evidence_level=F; model=offline-bytearray; real_game=not_run', flush=True)
    suite = unittest.defaultTestLoader.loadTestsFromTestCase(RepairTests)
    result = unittest.TextTestRunner(stream=sys.stdout, verbosity=2).run(suite)
    print('observations=' + json.dumps(EVENTS, sort_keys=True), flush=True)
    print(json.dumps({'tests_run': result.testsRun, 'failures': len(result.failures),
                      'errors': len(result.errors), 'successful': result.wasSuccessful()}), flush=True)
    sys.exit(0 if result.wasSuccessful() else 1)
