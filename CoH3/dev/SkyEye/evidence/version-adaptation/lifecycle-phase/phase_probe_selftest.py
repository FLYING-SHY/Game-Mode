"""Isolated checks: sparse fake memory, fake clock, in-memory output; no Windows calls."""
import argparse
import hashlib
import io
import json
from pathlib import Path
import struct
import unittest
from unittest.mock import patch
import phase_probe as p

START = "2026-11-21T03:42:27.6605831Z"
START_FT = p.start_filetime(START)


class Clock:
    def __init__(self):
        self.now = 0.0

    def __call__(self):
        return self.now

    def sleep(self, seconds):
        self.now += seconds


class FakeMemory:
    base, image_size, pid = 0x140000000, p.IMAGE_SIZE, 123

    def __init__(self, clock=None):
        self.data, self.attempts, self.denied = {}, [], []
        self.clock = clock or Clock()
        self.closed, self.close_count, self.start = False, 0, START_FT
        self.exit_at = float("inf")

    def put(self, address, raw):
        self.data.update((address + i, value) for i, value in enumerate(raw))

    def pack(self, address, fmt, *values):
        self.put(address, struct.pack("<" + fmt, *values))

    def allocate(self, address, size):
        self.put(address, bytes(size))

    def read(self, address, size):
        self.attempts.append((address, size))
        if any(address < end and address + size > begin for begin, end in self.denied):
            raise OSError("fake read fault address=%#x size=%d" % (address, size))
        try:
            return bytes(self.data[address + i] for i in range(size))
        except KeyError:
            raise OSError("fake unmapped address=%#x size=%d" % (address, size)) from None

    def identity(self):
        return {"pid": self.pid, "startFiletime": self.start, "startUtc": p.filetime_utc(self.start),
                "stillActive": self.clock() < self.exit_at, "utc": p.utc_now()}

    def attest(self, tick):
        tick()
        return {"path": "FAKE_MEMORY_ONLY", "sha256": p.GAME_SHA256, "imageSize": self.image_size, "base": hex(self.base)}

    def close(self):
        self.closed = True
        self.close_count += 1


def fixture(clock=None):
    m = FakeMemory(clock)
    b, arena, world = m.base, 0x20000000, 0x20001000
    m.pack(b + p.ROLES["arena"][1], "Q", arena)
    m.pack(b + p.ROLES["world"][1], "I", 0x1000)
    m.pack(world + 252, "I", 0x2000)
    m.pack(arena + 0x2000 + 12, "II", 0x3000, 2)
    m.pack(arena + 0x3000, "II", 0x4000, 0x5000)
    m.pack(arena + 0x4000 + 8, "Q", 1000)
    m.pack(arena + 0x5000 + 8, "Q", (1 << 32) | 1001)
    owner, vector, elements, core, matrix, manager = [0x30000000 + i * 0x1000 for i in range(6)]
    m.pack(b + p.ROLES["cameraOwner"][1], "Q", owner)
    m.pack(owner + 184, "Q", vector)
    m.pack(vector, "QQ", elements, elements + 8)
    m.pack(elements, "Q", core + 40)
    m.allocate(core, 688)
    m.pack(core, "Q", b + p.ROLES["cameraCoreVtable"][1])
    m.pack(core + 416, "Q", matrix)
    m.pack(core + 680, "Q", manager)
    m.allocate(matrix, 816)
    view = [1., 0., 0., 0., 0., 1., 0., 0., 0., 0., 1., 0., 0., 0., 0., 1.]
    projection = [1., 0., 0., 0., 0., 1., 0., 0., 0., 0., 1., 1., 0., 0., -.1, 0.]
    m.pack(matrix + 88, "16f", *view)
    m.pack(matrix + 752, "16f", *projection)
    m.allocate(manager, 32)
    m.pack(manager, "QQ", b + p.ROLES["controllerVtable"][1], (1 << 32) | 1001)
    singleton, root, hud, mini, tactical, mini_template, tactical_template = [0x40000000 + i * 0x1000 for i in range(7)]
    m.pack(b + p.ROLES["uiSingleton"][1], "Q", singleton)
    m.pack(singleton + 136, "Q", root)
    for address, kind, parent in ((root, "root", 0), (hud, "hud", root), (mini, "minimap", hud),
                                  (tactical, "tactical", hud), (mini_template, "root", mini),
                                  (tactical_template, "root", tactical)):
        m.allocate(address, 576)
        m.pack(address, "Q", b + p.ROLES[kind + "Vtable"][1])
        m.pack(address + 136, "Q", parent)
        m.pack(address + 192, "I", 0x20)
        m.pack(address + 296, "2f", 1920., 1080.)
    m.pack(root + 200, "Q", hud)
    m.pack(hud + 200, "Q", mini)
    m.pack(hud + 472, "Q", tactical)
    m.pack(mini + 472, "Q", mini_template)
    m.pack(tactical + 528, "Q", tactical_template)
    return m


class OutputSink(io.StringIO):
    def close(self):
        self.saved = self.getvalue()
        super().close()


class Checks(unittest.TestCase):
    def reader(self, memory=None, clock=None):
        memory = memory or fixture(clock)
        return p.BoundedReader(memory, memory.clock), memory

    def test_utc_preserves_seventh_digit(self):
        self.assertEqual(p.filetime_utc(START_FT), START)
        self.assertEqual(p.start_filetime(START.replace("Z", "+00:00")), START_FT)
        self.assertEqual(p.start_filetime("2026-11-21T03:42:27.6605830Z"), START_FT - 1)
        for value in ("2026-11-21T03:42:27", "2026-11-21T03:42:27+08:00", "2026-11-21T03:42:27.12345678Z"):
            with self.assertRaises(ValueError):
                p.start_filetime(value)

    def test_source_pins_and_mapping(self):
        manifest = p.load_sources()
        self.assertEqual(len(manifest), 3)
        self.assertTrue(all(item["sha256"] == p.SOURCE_PINS[name] for name, item in manifest.items()))

    def test_invalid_span_never_reaches_backend(self):
        r, m = self.reader()
        for address, size in ((0, 8), (0xffff, 8), (p.USER_END - 1, 8), (0x10000, 0), (0x10000, 65537)):
            with self.assertRaises(ValueError):
                r.read(address, size, "invalid")
        self.assertEqual(m.attempts, [])
        self.assertEqual(r.calls, 0)
        with self.assertRaises(ValueError):
            p.relative(0x20000000, p.SENTINEL)

    def test_failed_attempt_is_charged(self):
        r, m = self.reader()
        with self.assertRaises(OSError):
            r.read(0x10000, 32, "unmapped")
        self.assertEqual((r.calls, r.requested, r.returned), (1, 32, 0))
        self.assertEqual(r.journal[-1]["status"], "failed")
        self.assertIn("0x10000", r.journal[-1]["error"]["message"])

    def test_short_attempt_is_charged(self):
        r, m = self.reader()
        with patch.object(m, "read", return_value=b"x"):
            with self.assertRaises(OSError):
                r.read(0x10000, 8, "short")
        self.assertEqual((r.calls, r.requested, r.returned), (1, 8, 1))
        self.assertEqual(r.journal[-1]["actualBytes"], 1)

    def test_read_and_byte_admission_caps(self):
        for kind in ("calls", "requested"):
            r, m = self.reader()
            setattr(r, kind, p.MAX_READS - p.RESERVE_READS if kind == "calls" else p.MAX_BYTES - p.RESERVE_BYTES)
            with self.assertRaises(p.BudgetExceeded):
                r.read(0x10000, 8, "capped")
            self.assertEqual(m.attempts, [])
        r, m = self.reader()
        r.finalizing, r.calls = True, p.MAX_READS
        with self.assertRaises(p.BudgetExceeded):
            r.read(0x10000, 8, "absoluteCap")
        self.assertEqual(m.attempts, [])

    def test_deadline_reserve_and_absolute_limit(self):
        clock = Clock()
        r, m = self.reader(clock=clock)
        clock.now = p.MAX_SECONDS - p.RESERVE_SECONDS
        with self.assertRaises(p.BudgetExceeded):
            r.tick()
        r.finalizing = True
        r.tick()
        clock.now = p.MAX_SECONDS
        with self.assertRaises(p.BudgetExceeded):
            r.tick()
        self.assertEqual(m.attempts, [])

    def test_hud_slice_releases_limit(self):
        r, m = self.reader()
        with self.assertRaises(p.SliceExceeded):
            with r.hud_slice():
                r.calls += 4500
                r.read(0x10000, 8, "slice")
        self.assertIsNone(r.soft)
        self.assertEqual(m.attempts, [])

    def test_complete_fake_observation(self):
        r, _ = self.reader()
        result = p.Observation(r).sample(0)
        self.assertTrue(result["sameProcessIdentity"])
        self.assertTrue(result["camera"]["sourceMatrixChecks"])
        self.assertEqual(result["camera"]["controller"]["localSlot"], 1)
        self.assertTrue(result["world"]["uniqueObservedLocalPlayer"])
        self.assertFalse(result["world"]["matchKeyCandidateIsProductSnapshot"])
        for name in ("minimap", "tactical"):
            chain = result["hud"]["maps"][name]
            self.assertEqual(chain["status"], "observed")
            self.assertEqual(chain["termination"], "null_parent")
            self.assertEqual(len(chain["nodes"]), 4)
            self.assertTrue(chain["sourceParentChainChecks"])
            self.assertFalse(chain["completeLayoutValidationPerformed"])
            self.assertFalse(chain["csharpTryReadExecuted"])
        self.assertTrue(result["stability"]["allObservedCheckpointsStable"])
        self.assertTrue(result["stability"]["stableWorldRootsAfterRead"])
        json.dumps(result, allow_nan=False)

    def test_nan_preserved_as_raw_without_json_nan(self):
        r, m = self.reader()
        m.pack(0x30004000 + 88, "f", float("nan"))
        out = p.Observation(r).observe("camera", p.Observation(r).camera)
        self.assertEqual(out["view"][0], "nan")
        self.assertFalse(out["finite"])
        self.assertFalse(out["sourceMatrixChecks"])
        json.dumps(out, allow_nan=False)

    def test_invalid_local_identity_never_produces_match_candidate(self):
        r, m = self.reader()
        m.pack(0x30005000 + 24, "B", 1)
        obs = p.Observation(r)
        camera = obs.observe("camera", obs.camera)
        out = obs.observe("world", lambda target: obs.world(target, camera))
        self.assertTrue(out["uniqueObservedLocalPlayer"])
        self.assertFalse(out["sourceLocalIdentityChecks"])
        self.assertNotIn("matchKeyCandidate", out)

    def test_actual_schedule_delay_is_reported(self):
        clock = Clock()
        r, _ = self.reader(clock=clock)
        clock.now = 2.25
        result = p.Observation(r).sample(1, 0.25)
        self.assertEqual(result["actualOffsetSeconds"], 2.0)
        self.assertEqual(result["scheduleDelaySeconds"], 1.0)

    def test_camera_vector_count_guard(self):
        r, m = self.reader()
        m.pack(0x30001000, "QQ", 0x30002000, 0x30002000 + 8200)
        obs = p.Observation(r)
        out = obs.observe("camera", obs.camera)
        self.assertEqual(out["status"], "not_observed")
        self.assertIn("Camera vector invalid", out["error"]["message"])
        self.assertEqual(out["candidates"], [])

    def test_player_count_guard_and_sentinel(self):
        for count in (0, 17):
            r, m = self.reader()
            m.pack(0x20002000 + 16, "I", count)
            obs = p.Observation(r)
            out = obs.observe("world", lambda target: obs.world(target, {}))
            self.assertEqual(out["playerCount"], count)
            self.assertEqual(out["status"], "not_observed")
            self.assertEqual(out["players"], [])
        r, m = self.reader()
        m.pack(m.base + p.ROLES["world"][1], "I", p.SENTINEL)
        obs = p.Observation(r)
        out = obs.observe("world", lambda target: obs.world(target, {}))
        self.assertEqual(out["worldOffset"], "0xffffffff")
        self.assertIn("sentinel", out["error"]["message"])

    def test_parent_cycle_is_finite(self):
        r, m = self.reader()
        m.pack(0x40003000 + 136, "Q", 0x40005000)
        obs = p.Observation(r)
        out = obs.observe("minimap", lambda target: obs.parent_chain(0x40003000, "minimap", target))
        self.assertEqual(out["termination"], "parent_cycle")
        self.assertEqual(len(out["nodes"]), 2)
        self.assertEqual(out["status"], "not_observed")

    def test_parent_limit_is_64(self):
        r, m = self.reader()
        previous = 0x40005000
        for index in range(65):
            node = 0x50000000 + index * 0x1000
            m.allocate(node, 344)
            m.pack(node, "Q", m.base + p.ROLES["hudVtable"][1])
            m.pack(previous + 136, "Q", node)
            previous = node
        obs = p.Observation(r)
        out = obs.observe("minimap", lambda target: obs.parent_chain(0x40003000, "minimap", target))
        self.assertEqual(out["termination"], "parent_limit")
        self.assertEqual(len(out["nodes"]), 64)

    def test_child_vector_over_count_not_followed(self):
        r, m = self.reader()
        root, vector, data = 0x40001000, 0x40007000, 0x60000000
        m.pack(root + 504, "Q", vector)
        m.allocate(vector, 128)
        m.pack(vector + 112, "Q", data)
        m.pack(vector + 124, "I", 513)
        obs = p.Observation(r)
        out = obs.observe("hud", obs.hud)
        self.assertTrue(any(item.get("count") == 513 for item in out["anomalies"]))
        self.assertFalse(any(address == data for address, _ in m.attempts))

    def test_missing_root_does_not_guess(self):
        r, m = self.reader()
        m.pack(0x40001000, "Q", 0xdeadbeef)
        obs = p.Observation(r)
        out = obs.observe("hud", obs.hud)
        self.assertEqual(out["status"], "not_observed")
        self.assertEqual(out["rootVtable"], "0xdeadbeef")
        self.assertTrue(all(item["status"] == "not_observed" for item in out["maps"].values()))
        self.assertEqual(r.calls, 3)

    def test_hud_node_cap_reports_partial_not_absence(self):
        r, m = self.reader()
        root = 0x40001000
        previous = root
        for index in range(p.MAX_NODES + 1):
            node = 0x60000000 + index * 0x1000
            m.allocate(node, 576)
            m.pack(node, "Q", m.base + p.ROLES["hudVtable"][1])
            m.pack(node + 136, "Q", previous)
            m.pack(previous + 200, "Q", node)
            previous = node
        obs = p.Observation(r)
        out = obs.observe("hud", obs.hud)
        self.assertEqual(out["traversal"]["visited"], p.MAX_NODES)
        self.assertEqual(out["traversal"]["stopReason"], "node_limit")
        self.assertFalse(out["traversal"]["exhaustive"])
        self.assertEqual(out["maps"]["minimap"]["status"], "not_observed")

    def test_windows_constructor_identity_guard_uses_only_read_mask(self):
        calls = []

        class Function:
            def __init__(self, action):
                self.action = action

            def __call__(self, *args):
                return self.action(*args)

        class Kernel:
            OpenProcess = Function(lambda mask, inherit, pid: calls.append(("open", mask, inherit, pid)) or 12345)
            CloseHandle = Function(lambda handle: calls.append(("close", handle)) or True)
            ReadProcessMemory = Function(lambda *args: self.fail("VM_READ called before identity validation"))

        def times(handle, created, *unused):
            created._obj.dwHighDateTime = START_FT >> 32
            created._obj.dwLowDateTime = START_FT & p.SENTINEL
            return True

        def exit_code(handle, result):
            result._obj.value = 259
            return True

        kernel = Kernel()
        kernel.GetProcessTimes = Function(times)
        kernel.GetExitCodeProcess = Function(exit_code)
        with patch.object(p.sys, "platform", "win32"), patch.object(p.c, "WinDLL", return_value=kernel, create=True) as dll:
            with self.assertRaisesRegex(ValueError, "Process identity mismatch"):
                p.WindowsMemory(123, START_FT + 1)
        self.assertEqual(calls, [("open", 0x410, False, 123), ("close", 12345)])
        self.assertEqual(dll.call_count, 1)
        self.assertEqual(dll.call_args.args[0], "kernel32")

    def test_unreadable_parent_keeps_partial_chain(self):
        r, m = self.reader()
        m.denied.append((0x40002000, 0x40002000 + 344))
        obs = p.Observation(r)
        out = obs.observe("minimap", lambda target: obs.parent_chain(0x40003000, "minimap", target))
        self.assertEqual(len(out["nodes"]), 2)
        self.assertEqual(out["status"], "not_observed")
        self.assertIn("0x40002000", out["error"]["message"])

    def test_changed_root_fails_final_stability(self):
        r, m = self.reader()
        obs = p.Observation(r)
        obs.observe("world", lambda target: obs.world(target, {}))
        m.pack(m.base + p.ROLES["world"][1], "I", 0x1234)
        out = obs.final_checks()
        self.assertFalse(out["stableWorldRootsAfterRead"])
        self.assertFalse(out["allObservedCheckpointsStable"])
        self.assertEqual(out["differences"][0]["field"], "world.worldOffsetRoot")
        self.assertEqual(out["differences"][0]["actualRawHex"], "34120000")

    def test_existing_output_blocks_before_factory(self):
        path = Path(__file__).resolve()
        before = hashlib.sha256(path.read_bytes()).hexdigest()
        args = argparse.Namespace(pid=123, expected_start_utc=START, out=str(path), label="running")
        calls = []
        with self.assertRaises(FileExistsError):
            p.capture(args, memory_factory=lambda *unused: calls.append(True))
        self.assertEqual(calls, [])
        self.assertEqual(hashlib.sha256(path.read_bytes()).hexdigest(), before)

    def capture_in_memory(self, memory, label="running"):
        output = Path(__file__).resolve().parent / "__memory_only_output__.json"
        sink = OutputSink()
        original_open = Path.open

        def intercepted(path, *args, **kwargs):
            if path == output:
                self.assertEqual(args[0], "x")
                return sink
            return original_open(path, *args, **kwargs)

        args = argparse.Namespace(pid=123, expected_start_utc=START, out=str(output), label=label)
        with patch.object(Path, "open", intercepted), patch.object(p.c, "WinDLL", side_effect=AssertionError("Windows API constructed during isolated check"), create=True):
            result = p.capture(args, memory_factory=lambda *unused: memory, clock=memory.clock, sleeper=memory.clock.sleep)
        self.assertEqual(json.loads(sink.saved), result)
        return result

    def test_same_fake_handle_and_0_1_3_schedule(self):
        memory = fixture()
        report = self.capture_in_memory(memory, "results")
        self.assertEqual(report["status"], "observed")
        self.assertEqual([sample["scheduledOffsetSeconds"] for sample in report["samples"]], [0, 1, 3])
        self.assertEqual([sample["elapsedFromInvocationSeconds"] for sample in report["samples"]], [0, 1, 3])
        self.assertEqual(report["phaseDecision"], "not_evaluated")
        self.assertFalse(report["productAcceptance"])
        self.assertFalse(report["operatorLabelIsObservedPhase"])
        self.assertTrue(report["handleClosed"])
        self.assertEqual(memory.close_count, 1)
        self.assertLess(report["counters"]["readAttempts"], p.MAX_READS)
        self.assertLess(report["counters"]["requestedBytes"], p.MAX_BYTES)

    def test_identity_mismatch_closes_without_vm_read(self):
        memory = fixture()
        memory.start += 1
        report = self.capture_in_memory(memory)
        self.assertEqual(report["status"], "failed")
        self.assertEqual(memory.attempts, [])
        self.assertTrue(memory.closed)
        self.assertEqual(memory.close_count, 1)

    def test_process_exit_stops_future_samples_and_closes(self):
        memory = fixture()
        memory.exit_at = 1
        report = self.capture_in_memory(memory)
        self.assertEqual(report["status"], "failed")
        self.assertEqual(len(report["samples"]), 1)
        self.assertEqual(memory.close_count, 1)
        self.assertTrue(report["handleClosed"])

    def test_attestation_failure_closes_without_vm_read(self):
        memory = fixture()
        with patch.object(memory, "attest", side_effect=ValueError("fake module SHA mismatch")):
            report = self.capture_in_memory(memory)
        self.assertEqual(report["status"], "failed")
        self.assertEqual(memory.attempts, [])
        self.assertEqual(memory.close_count, 1)
        self.assertTrue(report["handleClosed"])


if __name__ == "__main__":
    unittest.main(verbosity=2)
