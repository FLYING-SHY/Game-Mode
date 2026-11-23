"""Offline sparse-memory checks; output uses an in-memory sink, never a game process."""
import argparse
import io
import json
from pathlib import Path
import struct
import unittest
from unittest.mock import patch
import phase_probe_v2 as v

p = v.p
START = "2026-11-21T03:42:27.6605831Z"
START_FT = p.start_filetime(START)
ROOT, HUD, MINI, TACTICAL, MT, TT = [0x40001000 + i * 0x1000 for i in range(6)]
SINGLETON = 0x40000000


class Clock:
    def __init__(self):
        self.now = 0.0

    def __call__(self):
        return self.now

    def sleep(self, delay):
        self.now += delay


class Memory:
    base, image_size, pid = 0x140000000, p.IMAGE_SIZE, 123

    def __init__(self):
        self.clock, self.data, self.attempts, self.denied = Clock(), {}, [], []
        self.closed, self.close_count, self.start, self.exit_at = False, 0, START_FT, float("inf")
        self.after_read = None

    def put(self, address, raw):
        self.data.update((address + i, value) for i, value in enumerate(raw))

    def pack(self, address, fmt, *values):
        self.put(address, struct.pack("<" + fmt, *values))

    def allocate(self, address, size):
        self.put(address, bytes(size))

    def read(self, address, size):
        self.attempts.append((address, size))
        if any(address < end and address + size > begin for begin, end in self.denied):
            raise OSError("fake access fault at %#x len=%d" % (address, size))
        try:
            raw = bytes(self.data[address + i] for i in range(size))
        except KeyError:
            raise OSError("fake unmapped at %#x len=%d" % (address, size)) from None
        if self.after_read:
            self.after_read(address, size)
        return raw

    def identity(self):
        return {"pid": self.pid, "startFiletime": self.start, "startUtc": p.filetime_utc(self.start),
                "stillActive": self.clock() < self.exit_at, "utc": p.utc_now()}

    def attest(self, tick):
        tick()
        return {"path": "FAKE_MEMORY_ONLY", "sha256": p.GAME_SHA256, "imageSize": self.image_size, "base": hex(self.base)}

    def close(self):
        self.closed, self.close_count = True, self.close_count + 1


def fixture():
    m = Memory()
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
    m.pack(matrix + 88, "16f", 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
    m.pack(matrix + 752, "16f", 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, -.1, 0)
    m.allocate(manager, 32)
    m.pack(manager, "QQ", b + p.ROLES["controllerVtable"][1], (1 << 32) | 1001)
    m.pack(b + p.ROLES["uiSingleton"][1], "Q", SINGLETON)
    m.pack(SINGLETON + 136, "Q", ROOT)
    for address, kind, parent in ((ROOT, "root", 0), (HUD, "hud", ROOT), (MINI, "minimap", HUD),
                                  (TACTICAL, "tactical", HUD), (MT, "root", MINI), (TT, "root", TACTICAL)):
        m.allocate(address, 576)
        m.pack(address, "Q", b + p.ROLES[kind + "Vtable"][1])
        m.pack(address + 136, "Q", parent)
        m.pack(address + 192, "I", 0x20)
    m.pack(ROOT + 200, "Q", HUD)
    m.pack(HUD + 200, "Q", MINI)
    m.pack(HUD + 472, "Q", TACTICAL)
    m.pack(MINI + 472, "Q", MT)
    m.pack(TACTICAL + 528, "Q", TT)
    return m


def reference():
    m = fixture()
    reader = p.BoundedReader(m, m.clock)
    samples = [p.Observation(reader).sample(offset) for offset in (0, 1, 3)]
    return {"schema": "known-hud-phase-observation-v1", "scriptSha256": v.BASE_SHA256,
            "status": "observed", "operatorLabel": "paused", "operatorLabelIsObservedPhase": False,
            "phaseDecision": "not_evaluated", "pid": m.pid, "expectedStartFiletime": START_FT,
            "expectedStartUtc": START, "handleClosed": True, "identityBefore": m.identity(), "identityAfter": m.identity(),
            "sourceManifest": {key: {"sha256": value} for key, value in p.SOURCE_PINS.items()},
            "module": m.attest(reader.tick), "samples": samples}


class Sink(io.StringIO):
    def close(self):
        self.saved = self.getvalue()
        super().close()


class Checks(unittest.TestCase):
    def observe(self, memory=None, hints=None):
        m = memory or fixture()
        r = v.Reader(m, m.clock, m.clock())
        return v.Observation(r, hints or {}).sample(0), r, m

    def proofs(self, sample):
        return {item["kind"]: item for item in sample["hud"]["proofs"]}

    def parse(self, doc=None):
        raw = json.dumps(reference() if doc is None else doc).encode()
        metadata = {}
        hints = v.parse_reference(raw, v.sha256(raw), 123, START_FT, metadata)
        return hints, metadata

    def capture(self, m=None, reference_path=None, reference_sha=None, out="MEMORY_ONLY.json", reference_doc=None):
        m = m or fixture()
        reference_raw = None
        if reference_doc is not None:
            reference_raw = json.dumps(reference_doc).encode()
            reference_path = str(Path(__file__).resolve())
            reference_sha = v.sha256(reference_raw) if reference_sha is None else reference_sha
        args = argparse.Namespace(pid=m.pid, expected_start_utc=START, label="results", out=out,
                                  reference=reference_path, reference_sha256=reference_sha)
        sink, calls = Sink(), []
        original = Path.open

        def open_path(path, mode="r", *a, **kw):
            if str(path) == out:
                self.assertEqual(mode, "x")
                return sink
            if reference_raw is not None and str(path) == reference_path and mode == "rb":
                return io.BytesIO(reference_raw)
            return original(path, mode, *a, **kw)

        def factory(pid, start):
            calls.append((pid, start))
            return m

        with patch.object(Path, "open", open_path):
            result = v.capture(args, factory, m.clock, m.clock.sleep)
        self.assertEqual(json.loads(sink.saved), result)
        return result, m, calls

    def test_pinned_sources_and_base(self):
        self.assertEqual(v.sha256(v.BASE_PATH.read_bytes()), v.BASE_SHA256)
        self.assertEqual(set(p.load_sources()), set(p.SOURCE_PINS))
        with patch.object(v, "BASE_SHA256", "0" * 64), self.assertRaises(ValueError):
            v.load_base()

    def test_reference_pins_and_unique_hints(self):
        hints, metadata = self.parse()
        self.assertEqual(hints, {"hud": HUD, "minimap": MINI, "tactical": TACTICAL})
        self.assertFalse(metadata["reportedScriptShaIsAttestation"])
        self.assertEqual(metadata["sampleOffsetSeconds"], 3)

    def test_reference_rejects_changed_fields(self):
        for path, value in ((["schema"], "other"), (["scriptSha256"], "0" * 64), (["pid"], 124),
                            (["expectedStartFiletime"], START_FT + 1), (["identityAfter", "stillActive"], False),
                            (["module", "sha256"], "0" * 64), (["module", "imageSize"], 1),
                            (["handleClosed"], False), (["operatorLabel"], "results")):
            doc = reference()
            target = doc
            for key in path[:-1]:
                target = target[key]
            target[path[-1]] = value
            with self.subTest(path=path), self.assertRaises(ValueError):
                self.parse(doc)

    def test_reference_byte_sha_required(self):
        raw = json.dumps(reference()).encode()
        for digest in (None, "0" * 64, v.sha256(raw).upper()):
            with self.assertRaises(ValueError):
                v.parse_reference(raw, digest, 123, START_FT, {})

    def test_reference_sample_start_or_source_pin_mismatch(self):
        doc = reference()
        doc["samples"][1]["identityAfter"]["startFiletime"] += 1
        with self.assertRaises(ValueError):
            self.parse(doc)
        doc = reference()
        doc["sourceManifest"][next(iter(p.SOURCE_PINS))]["sha256"] = "0" * 64
        with self.assertRaises(ValueError):
            self.parse(doc)

    def test_real_retained_reference_is_accepted_offline(self):
        path = v.BASE_PATH.parent / "fresh02-paused-before-results.json"
        raw = path.read_bytes()
        self.assertEqual(v.sha256(raw), "8481416d9cc9a7748dd9528467bc0b6481385c9ef6a44ee1e422f3397164f939")
        metadata = {}
        hints = v.parse_reference(raw, v.sha256(raw), 33656, START_FT, metadata)
        self.assertEqual(hints, {"hud": 0x298e3363b70, "tactical": 0x2987e2adef0})

    def test_bidirectional_paths_rechecked(self):
        sample, r, m = self.observe(hints={"hud": HUD, "tactical": TACTICAL})
        self.assertTrue(sample["stability"]["stableWorldRootsAfterRead"])
        self.assertEqual(sample["hud"]["verification"]["status"], "checked")
        for proof in self.proofs(sample).values():
            self.assertEqual(proof["currentRootReachability"], "bidirectional_path_observed")
            self.assertTrue(proof["structureStable"])
        self.assertTrue(any(e["field"] == "verify.ui.body576" for e in r.journal))
        self.assertTrue(sample["hud"]["traversal"]["queueExhausted"])
        self.assertTrue(sample["hud"]["traversal"]["knownGraphComplete"])

    def test_parent_pointer_without_downward_membership_stays_unknown(self):
        m = fixture()
        m.pack(ROOT + 200, "Q", 0)
        sample, _, _ = self.observe(m, {"hud": HUD, "tactical": TACTICAL})
        for proof in self.proofs(sample).values():
            self.assertEqual(proof["currentRootReachability"], "unknown")
            self.assertEqual(proof["initial"]["termination"], "current_root_reached")
            self.assertTrue(any(edge["downwardMembership"] == "not_listed_in_read_fields" for edge in proof["initial"]["edges"]))
        self.assertEqual(sample["hud"]["phaseDecision"], "not_evaluated")

    def test_closed_tactical_flags_do_not_erase_connected_path(self):
        m = fixture()
        m.pack(TACTICAL + 192, "I", 0x18)
        sample, _, _ = self.observe(m)
        proof = self.proofs(sample)["tactical"]
        self.assertEqual(proof["currentRootReachability"], "bidirectional_path_observed")
        self.assertFalse(proof["initial"]["allChainBit20Set"])

    def test_flags_change_is_separate_from_structure(self):
        m = fixture()
        r = v.Reader(m, m.clock, 0)
        o = v.Observation(r, {"hud": HUD})
        out = o.observe("hud", o.hud)
        m.pack(HUD + 192, "I", 0x18)
        o.final_checks()
        proof = next(x for x in out["proofs"] if x["kind"] == "hud")
        self.assertEqual(proof["currentRootReachability"], "bidirectional_path_observed")
        self.assertFalse(proof["flags192Stable"])

    def test_root_change_invalidates_all_paths(self):
        m = fixture()
        o = v.Observation(v.Reader(m, m.clock, 0), {"hud": HUD})
        out = o.observe("hud", o.hud)
        new_root = 0x60000000
        m.allocate(new_root, 576)
        m.pack(new_root, "Q", m.base + p.ROLES["rootVtable"][1])
        m.pack(SINGLETON + 136, "Q", new_root)
        o.final_checks()
        self.assertTrue(all(x["currentRootReachability"] == "unknown" for x in out["proofs"]))
        self.assertFalse(out["verification"]["rootStable"])

    def test_downward_edge_changes_between_passes(self):
        m = fixture()
        o = v.Observation(v.Reader(m, m.clock, 0), {"hud": HUD})
        out = o.observe("hud", o.hud)
        m.pack(ROOT + 200, "Q", 0)
        o.final_checks()
        self.assertTrue(all(x["currentRootReachability"] == "unknown" for x in out["proofs"]))

    def test_reference_vtable_changed(self):
        m = fixture()
        m.pack(HUD, "Q", m.base + p.ROLES["rootVtable"][1])
        sample, _, _ = self.observe(m, {"hud": HUD})
        proof = self.proofs(sample)["hud"]
        self.assertEqual(proof["initial"]["termination"], "control_vtable_changed")
        self.assertEqual(proof["currentRootReachability"], "unknown")

    def test_parent_cycle_and_limit(self):
        for count, loop in ((3, True), (65, False)):
            m = fixture()
            nodes = [0x50000000 + i * 0x1000 for i in range(count)]
            m.pack(HUD + 136, "Q", nodes[0])
            for i, address in enumerate(nodes):
                m.allocate(address, 576)
                m.pack(address, "Q", m.base + p.ROLES["rootVtable"][1])
                parent = nodes[(i + 1) % count] if loop or i + 1 < count else 0
                m.pack(address + 136, "Q", parent)
                m.pack(address + 200, "Q", HUD if i == 0 else nodes[i - 1])
            tree, trace = v.KnownTree(v.Reader(m, m.clock, 0), "test."), {}
            tree.trace("hud", HUD, ROOT, trace)
            self.assertEqual(trace["termination"], "parent_cycle" if loop else "parent_limit")
            self.assertFalse(trace["firstPassPath"])

    def test_vector_witness_and_changed_elements(self):
        m = fixture()
        vector, data = 0x60000000, 0x60001000
        m.pack(ROOT + 200, "Q", 0)
        m.pack(ROOT + 504, "Q", vector)
        m.allocate(vector, 128)
        m.pack(vector + 112, "Q", data)
        m.pack(vector + 124, "I", 1)
        m.pack(data, "Q", HUD)
        o = v.Observation(v.Reader(m, m.clock, 0), {"hud": HUD})
        out = o.observe("hud", o.hud)
        self.assertIn("vector", out["proofs"][0]["initial"]["edges"][0]["witness"])
        m.pack(data, "Q", 0)
        o.final_checks()
        self.assertEqual(out["proofs"][0]["currentRootReachability"], "unknown")

    def test_oversized_vector_not_followed_or_treated_as_absent(self):
        m = fixture()
        vector = 0x60000000
        m.pack(ROOT + 200, "Q", 0)
        m.pack(ROOT + 504, "Q", vector)
        m.allocate(vector, 128)
        m.pack(vector + 112, "Q", 0x60001000)
        m.pack(vector + 124, "I", 513)
        sample, r, _ = self.observe(m, {"hud": HUD})
        proof = self.proofs(sample)["hud"]
        self.assertEqual(proof["initial"]["edges"][0]["downwardMembership"], "unknown")
        self.assertTrue(any(x.get("reason") == "child_count_limit" for x in sample["hud"]["anomalies"]))
        self.assertFalse(any(e["field"] == "ui.vectorElements" for e in r.journal))

    def test_read_fault_preserved_and_cached_per_pass(self):
        m = fixture()
        m.pack(ROOT + 144, "Q", 0x70000000)
        sample, r, _ = self.observe(m)
        self.assertTrue(sample["hud"]["anomalies"])
        self.assertFalse(sample["hud"]["traversal"]["knownGraphComplete"])
        self.assertTrue(any(e["status"] == "failed" for e in r.journal))
        tree = v.KnownTree(r, "test.")
        self.assertIsNone(tree.parent(0x70000000))
        count = r.calls
        self.assertIsNone(tree.parent(0x70000000))
        self.assertEqual(r.calls, count)
        self.assertEqual(len(tree.anomalies), 1)

    def test_cache_not_carried_between_samples(self):
        m = fixture()
        r = v.Reader(m, m.clock, 0)
        first = v.Observation(r, {"hud": HUD}).sample(0)
        m.pack(ROOT + 200, "Q", 0)
        second = v.Observation(r, {"hud": HUD}).sample(1)
        self.assertEqual(self.proofs(first)["hud"]["currentRootReachability"], "bidirectional_path_observed")
        self.assertEqual(self.proofs(second)["hud"]["currentRootReachability"], "unknown")

    def test_duplicate_candidates_use_cache(self):
        m = fixture()
        child = 0x70000000
        m.allocate(child, 144)
        m.pack(child, "Q", m.base + p.ROLES["rootVtable"][1])
        m.pack(child + 136, "Q", 0)
        for node in (ROOT, HUD, MINI, TACTICAL, MT, TT):
            m.pack(node + 144, "Q", child)
        sample, r, _ = self.observe(m)
        entries = [e for e in r.journal if e["field"] == "ui.childParent136" and e["address"] == hex(child + 136)]
        self.assertEqual(len(entries), 1)
        self.assertGreater(sample["hud"]["cacheHits"]["childParent136"], 0)

    def test_partition_caps_reserve_later_reads(self):
        m, target = fixture(), 0x80000000
        m.allocate(target, 8)
        r = v.Reader(m, m.clock, 0)
        with r.partition("hud"):
            for _ in range(5000):
                r.read(target, 8, "test.hud")
            with self.assertRaises(v.SampleLimit):
                r.read(target, 8, "blocked")
        with r.partition("stability"):
            r.read(target, 8, "test.final")
        self.assertEqual(r.calls, 5001)
        self.assertEqual(len(r.journal), 5001)

    def test_failed_and_short_reads_charged(self):
        m = fixture()
        r = v.Reader(m, m.clock, 0)
        with self.assertRaises(OSError):
            r.read(0x70000000, 8, "failed")
        with patch.object(m, "read", return_value=b"abc"), self.assertRaises(OSError):
            r.read(0x70000000, 8, "short")
        self.assertEqual((r.calls, r.requested, r.returned), (2, 16, 3))
        self.assertEqual([e["status"] for e in r.journal], ["failed", "failed"])

    def test_bytes_and_deadline_before_backend(self):
        m = fixture()
        r = v.Reader(m, m.clock, 0)
        r.requested = p.MAX_BYTES - p.RESERVE_BYTES
        with self.assertRaises(p.BudgetExceeded):
            r.read(ROOT, 1, "bytes")
        r.requested = 0
        m.clock.now = p.MAX_SECONDS
        with self.assertRaises(p.BudgetExceeded):
            r.read(ROOT, 1, "time")
        self.assertEqual(m.attempts, [])

    def test_budget_stop_retains_partial_unknown(self):
        with patch.dict(v.PARTITIONS, hud=(4, 560 * 1024, 1.35)):
            sample, _, _ = self.observe(hints={"hud": HUD})
        self.assertEqual(sample["hud"]["traversal"]["stopReason"], "partition_limit")
        self.assertEqual(sample["hud"]["currentTreeCoverage"], "unknown")
        self.assertTrue(all(x["currentRootReachability"] == "unknown" for x in sample["hud"]["proofs"]))
        self.assertEqual(sample["hud"]["proofs"][0]["initial"]["termination"], "read_limit")

    def test_finished_positive_path_does_not_complete_unfinished_walk(self):
        with patch.dict(v.PARTITIONS, hud=(5, 560 * 1024, 1.35)):
            sample, _, _ = self.observe(hints={"hud": HUD})
        self.assertEqual(self.proofs(sample)["hud"]["currentRootReachability"], "bidirectional_path_observed")
        self.assertEqual(sample["hud"]["currentTreeCoverage"], "unknown")
        self.assertFalse(sample["hud"]["coverageIsPhaseEvidence"])
        self.assertEqual(sample["hud"]["phaseDecision"], "not_evaluated")

    def test_node_cap_retains_unfinished_queue(self):
        m = fixture()
        tree, out = v.KnownTree(v.Reader(m, m.clock, 0), "test."), {}
        with patch.object(v, "MAX_NODES", 2):
            tree.walk(ROOT, lambda *args: None, out)
        self.assertEqual(out["stopReason"], "node_limit")
        self.assertGreater(out["queueRemaining"], 0)
        self.assertFalse(out["knownGraphComplete"])

    def test_capture_one_handle_schedule_and_ledger(self):
        result, m, calls = self.capture()
        self.assertEqual(result["status"], "observed")
        self.assertEqual(calls, [(123, START_FT)])
        self.assertEqual([s["actualOffsetSeconds"] for s in result["samples"]], [0, 1, 3])
        self.assertEqual(result["counters"]["readAttempts"], len(result["readJournal"]))
        self.assertEqual(result["counters"]["requestedBytes"], sum(e["length"] for e in result["readJournal"]))
        self.assertEqual(m.close_count, 1)
        self.assertFalse(result["productAcceptance"])
        self.assertEqual(result["phaseDecision"], "not_evaluated")

    def test_capture_reference_and_current_module_must_match(self):
        doc = reference()
        result, m, calls = self.capture(reference_doc=doc)
        self.assertEqual(result["status"], "observed")
        self.assertTrue(result["reference"]["currentModuleMatch"])
        m = fixture()
        original = m.attest
        def changed_module(tick):
            return dict(original(tick), base=hex(m.base + 4096))
        m.attest = changed_module
        result, m, _ = self.capture(m, reference_doc=doc)
        self.assertEqual(result["status"], "failed")
        self.assertFalse(result["reference"]["currentModuleMatch"])
        self.assertEqual(m.attempts, [])
        self.assertEqual(m.close_count, 1)

    def test_blocking_read_overrun_is_recorded_and_stops(self):
        m = fixture()
        m.after_read = lambda *args: setattr(m.clock, "now", 11.0)
        result, m, _ = self.capture(m)
        self.assertEqual(result["status"], "budget_exhausted")
        self.assertEqual(len(m.attempts), 1)
        self.assertEqual(result["deadlineOverrunSeconds"], 1.0)
        self.assertEqual(m.close_count, 1)

    def test_global_read_budget_stops_capture_after_partial_sample(self):
        with patch.object(p, "MAX_READS", 800):
            result, m, calls = self.capture()
        self.assertEqual(result["status"], "budget_exhausted")
        self.assertEqual(len(result["samples"]), 1)
        self.assertLessEqual(result["counters"]["readAttempts"], 800)
        self.assertEqual(result["counters"]["readAttempts"], len(m.attempts))
        self.assertTrue(result["handleClosed"])
        self.assertEqual(len(calls), 1)

    def test_capture_identity_failure_closes_without_vmread(self):
        m = fixture()
        m.start += 1
        result, m, _ = self.capture(m)
        self.assertEqual(result["status"], "failed")
        self.assertEqual(m.attempts, [])
        self.assertEqual(m.close_count, 1)

    def test_existing_output_precedes_open_and_preserves_bytes(self):
        target = Path(__file__).resolve()
        before = v.sha256(target.read_bytes())
        args = argparse.Namespace(pid=123, expected_start_utc=START, label="results", out=str(target),
                                  reference=None, reference_sha256=None)
        with self.assertRaises(FileExistsError):
            v.capture(args, lambda *args: self.fail("process opened on output collision"))
        self.assertEqual(v.sha256(target.read_bytes()), before)

    def test_bad_reference_stops_before_process(self):
        path = str(v.BASE_PATH.parent / "fresh02-paused-before-results.json")
        result, m, calls = self.capture(reference_path=path, reference_sha="0" * 64)
        self.assertEqual(result["status"], "failed")
        self.assertEqual(calls, [])
        self.assertEqual(m.attempts, [])
        self.assertIn("byte SHA256", result["error"]["message"])


if __name__ == "__main__":
    unittest.main(verbosity=2)
