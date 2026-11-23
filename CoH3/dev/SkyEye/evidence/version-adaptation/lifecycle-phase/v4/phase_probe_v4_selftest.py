"""Offline genuine-v3 parser and actual v4 fake-memory capture regression suite."""
import argparse
from copy import deepcopy
import ctypes
import io
import json
import os
from pathlib import Path
import sys
from types import ModuleType
import unittest
from unittest.mock import patch

# Covers module loading as well as every test. Even caught native attempts fail the run.
NATIVE_GUARD = patch.object(ctypes, "WinDLL", side_effect=AssertionError("Actual native calls blocked in v4 offline suite"), create=True)
NATIVE_CALLS = NATIVE_GUARD.start()
import phase_probe_v4 as w

V2_TEST_PATH = w.HERE.parent / "v2/phase_probe_v2_selftest.py"
V2_TEST_SHA256 = "944ea751f2917c95476b1c60de447059f530e8e45405e29bc797db2b18003055"
REFERENCES = {"running": "760a92a813aac94c94444dc2ee1b2d3d01a941c648305a68488f0194e008ac59",
              "paused": "ba29370662feeab744da3fa20cbf3812eb69ae5d9b2fd79fcf7f63f4591696fd"}
START = "2026-11-22T01:23:35.3993114Z"
PID = 30504
FIXTURE_ENGINE = w.load_engine()
START_FT = FIXTURE_ENGINE.p.start_filetime(START)
f = ModuleType("frozen_v2_fixture_helpers_for_v4")
f.__file__ = str(V2_TEST_PATH)
raw_test = V2_TEST_PATH.read_bytes()
if w.sha256(raw_test) != V2_TEST_SHA256:
    raise ValueError("Frozen v2 fixture helper SHA256 mismatch")
with patch.dict(sys.modules, phase_probe_v2=FIXTURE_ENGINE):
    exec(compile(raw_test, str(V2_TEST_PATH), "exec"), f.__dict__)


def reference_path(label="running"):
    return w.HERE.parent / ("v3/live-round3/pid30504-%s-001.json" % label)


def reference_bytes(label="running"):
    raw = reference_path(label).read_bytes()
    if w.sha256(raw) != REFERENCES[label]:
        raise ValueError("Retained genuine v3 %s fixture byte SHA256 changed" % label)
    return raw


RAW = reference_bytes()
DOC = json.loads(RAW)


def set_field(doc, path, value):
    target = doc
    for key in path[:-1]:
        target = target[key]
    target[path[-1]] = value


def memory_fixture():
    """Model-only topology from reference proofs, with a deliberately different CURRENT root."""
    base = int(DOC["module"]["base"], 16)
    with patch.object(f.Memory, "base", base):
        memory = f.fixture()
    memory.base, memory.pid, memory.start = base, PID, START_FT
    hud = DOC["samples"][-1]["hud"]
    old_root = int(hud["root"], 16)
    current_root = f.ROOT
    remap = lambda value: current_root if value == old_root else value
    nodes = {}
    for proof in hud["proofs"]:
        for node in proof["initial"]["nodes"]:
            nodes[int(node["address"], 16)] = node
    for address, node in nodes.items():
        current = remap(address)
        memory.allocate(current, 576)
        memory.pack(current, "Q", int(node["vtable"], 16))
        memory.pack(current + 136, "Q", remap(int(node["parent136"], 16)))
        memory.pack(current + 192, "I", int(node["flags192"], 16))
    vectors = set()
    for proof in hud["proofs"]:
        trace = proof["initial"]
        if "templateField" in trace:
            template = trace["templateField"]
            memory.pack(int(proof["address"], 16) + template["offset"], "Q", remap(int(template["pointer"], 16)))
        for edge in trace["edges"]:
            parent, child = remap(int(edge["parent"], 16)), remap(int(edge["child"], 16))
            witness = edge["witness"]
            if "inlineOffset" in witness:
                memory.pack(parent + witness["inlineOffset"], "Q", child)
            else:
                vector = witness["vector"]
                address, data, count = int(vector["address"], 16), int(vector["data112"], 16), vector["count124"]
                if address not in vectors:
                    memory.allocate(address, 128)
                    memory.allocate(data, count * 8)
                    vectors.add(address)
                memory.pack(parent + 504, "Q", address)
                memory.pack(address + 112, "Q", data)
                memory.pack(address + 124, "I", count)
                memory.pack(data + witness["index"] * 8, "Q", child)
    return memory


class ReferenceChecks(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.engine = w.load_engine()

    def parse(self, doc=None, raw=None, digest=None, pid=PID, start=START_FT):
        raw = raw if raw is not None else RAW if doc is None else json.dumps(doc, allow_nan=False).encode()
        metadata = {}
        hints = self.engine.parse_reference(raw, w.sha256(raw) if digest is None else digest, pid, start, metadata)
        return hints, metadata

    def reject(self, path, value, reason):
        doc = deepcopy(DOC)
        set_field(doc, path, value)
        with self.subTest(path=path, value=str(value)[:80]), self.assertRaisesRegex(ValueError, reason):
            self.parse(doc)

    def test_genuine_running_and_paused_readonly_fixtures(self):
        for label in REFERENCES:
            raw = reference_bytes(label)
            hints, metadata = self.parse(raw=raw)
            self.assertEqual(hints, {"hud": 0x1d3cc5ec570, "tactical": 0x1d3ccc60fb0, "minimap": 0x1d3cca73b30})
            self.assertEqual(metadata["operatorLabel"], label)
            self.assertEqual(metadata["module"], {key: DOC["module"][key] for key in ("base", "sha256", "imageSize")})
            self.assertTrue(metadata["hintsOnly"])
            self.assertFalse(metadata["reportedScriptShaIsAttestation"])
            self.assertFalse(metadata["referenceRootIsCurrentRootRequirement"])
            self.assertEqual([s["currentTreeCoverage"] for s in metadata["samples"]], ["unknown"] * 3)

    def test_optional_maps_policy_accepts_single_unique_hud(self):
        doc = deepcopy(DOC)
        for sample in doc["samples"]:
            sample["hud"]["proofs"] = [p for p in sample["hud"]["proofs"] if p["kind"] == "hud"]
        hints, metadata = self.parse(doc)
        self.assertEqual(hints, {"hud": 0x1d3cc5ec570})
        self.assertEqual(metadata["candidateCounts"], {"hud": 1, "minimap": 0, "tactical": 0})

    def test_exact_external_byte_sha_and_case_required(self):
        for digest in ("", "0" * 64, REFERENCES["running"].upper()):
            with self.subTest(digest=digest), self.assertRaisesRegex(ValueError, "byte SHA256"):
                self.parse(digest=digest)
        with self.assertRaisesRegex(ValueError, "byte SHA256"):
            self.engine.parse_reference(RAW, None, PID, START_FT, {})
        with self.assertRaisesRegex(ValueError, "byte SHA256"):
            self.engine.parse_reference(RAW + b"\n", REFERENCES["running"], PID, START_FT, {})

    def test_reference_must_be_strictly_smaller_than_eight_mib(self):
        for size in (8 * 1024 * 1024, 8 * 1024 * 1024 + 1):
            raw = b" " * size
            with self.subTest(size=size), self.assertRaisesRegex(ValueError, "smaller than 8 MiB"):
                self.parse(raw=raw)

    def test_wrong_schema_script_status_and_nonclaim_fields(self):
        for path, value in ((["schema"], "known-hud-phase-observation-v1"), (["schema"], w.SCHEMA),
                            (["scriptSha256"], "0" * 64), (["status"], "failed"), (["handleClosed"], False),
                            (["sourceBindingVerified"], 1), (["phaseDecision"], "running"),
                            (["operatorLabelIsObservedPhase"], True), (["productAcceptance"], True),
                            (["operatorLabel"], "results"), (["accessMask"], "0x1f0fff")):
            self.reject(path, value, path[0])

    def test_every_attestation_layer_source_binding_and_boundary_is_fixed(self):
        attestation = ["executionAttestation"]
        mutations = [(attestation + ["schema"], "other")]
        for layer in ("wrapper", "measurementEngine", "baseProbe"):
            mutations.extend(((attestation + [layer, "sha256"], "0" * 64),
                              (attestation + [layer, "path"], "forged.py")))
        mutations.extend(((attestation + ["measurementEngine", "sourceBytesExecutedUnmodified"], False),
                          (attestation + ["baseProbe", "compileFilename"], "other.py"),
                          (attestation + ["sourceBinding", "loaderCodeUnchanged"], 1),
                          (attestation + ["sourceBinding", "sourceFilesExecuted"], True),
                          (attestation + ["sourceBinding", "sourceRoot"], "elsewhere"),
                          (attestation + ["reportAdapter", "measurementFunctionsChanged"], True),
                          (attestation + ["boundaries", "productAcceptance"], True)))
        for key in self.engine.p.SOURCE_PINS:
            mutations.extend(((attestation + ["sourceBinding", "originalFrozenV1SourcePins", key], "0" * 64),
                              (attestation + ["sourceBinding", "effectiveSourceLoaderPins", key], "0" * 64)))
        for path, value in mutations:
            self.reject(path, value, "executionAttestation")

    def test_source_manifest_requires_fixed_all_entries_paths_sha_and_bytes(self):
        for key in w.SOURCE_BYTES:
            for field, value in (("sha256", "0" * 64), ("path", "another-source"), ("bytes", 1)):
                self.reject(["sourceManifest", key, field], value, "sourceManifest")
        self.reject(["sourceManifest"], {}, "sourceManifest")
        self.reject(["frozenEngineReport", "scriptSha256"], "0" * 64, "frozenEngineReport")
        self.reject(["frozenEngineReport", "reportedDiskShaMatchesExecutedSource"], 1, "frozenEngineReport")
        self.reject(["baseProbe", "sha256"], "0" * 64, "baseProbe")
        self.reject(["roles", "hudVtable", "candidateRva"], "0x1", "roles")

    def test_top_and_all_sample_identity_layers_and_utc_100ns(self):
        for path, value in ((["pid"], PID + 1), (["pid"], True), (["expectedStartFiletime"], START_FT + 1),
                            (["expectedStartUtc"], "2026-11-22T01:23:35.3993115Z"),
                            (["expectedStartUtc"], "2026-11-22T01:23:35.3993114+08:00")):
            self.reject(path, value, "pid|expectedStart|explicit UTC")
        for parent in ([], ["samples", 0], ["samples", 1], ["samples", 2]):
            for phase in ("identityBefore", "identityAfter"):
                for key, value in (("pid", PID + 1), ("startFiletime", START_FT + 1),
                                   ("startUtc", "2026-11-22T01:23:35.3993115Z"), ("stillActive", False), ("exitCode", 0)):
                    self.reject(parent + [phase, key], value, "identity|100 ns")
        for index in range(3):
            self.reject(["samples", index, "sameProcessIdentity"], 1, "sameProcessIdentity")

    def test_module_sha_image_size_base_and_hash_stability(self):
        for key, value in (("sha256", "0" * 64), ("imageSize", 1), ("base", "0x10001"),
                           ("base", "0x7ffffff00000"), ("base", "0x0"), ("base", "0X7ff6985b0000"),
                           ("fileStableDuringHash", False), ("fileBytes", 0)):
            self.reject(["module", key], value, "module")

    def test_original_three_sample_set_and_types(self):
        self.reject(["samples"], DOC["samples"][:2], "sample set")
        self.reject(["samples"], [DOC["samples"][0], DOC["samples"][0], DOC["samples"][2]], "sample set")
        self.reject(["samples", 1, "scheduledOffsetSeconds"], True, "sample set")
        self.reject(["samples", 2, "scheduledOffsetSeconds"], 2, "sample set")

    def test_incomplete_unknown_and_missing_recheck_rejected(self):
        path = ["samples", 0, "hud", "proofs", 0]
        for suffix, value, reason in ((["initial", "firstPassPath"], False, "firstPassPath"),
                                      (["recheck", "firstPassPath"], False, "firstPassPath"),
                                      (["currentRootReachability"], "unknown", "currentRootReachability"),
                                      (["structureStable"], False, "structureStable"),
                                      (["recheck"], None, "recheck"), (["kind"], "root", "kind")):
            self.reject(path + suffix, value, reason)
        self.reject(["samples", 1, "hud", "proofs"], [], "missing proofs")
        self.reject(["samples", 0, "hud", "verification", "status"], "unknown", "verification.status")
        self.reject(["samples", 0, "hud", "verification", "finalRoot"], "0x10000", "finalRoot")

    def test_duplicate_and_ambiguous_hint_kinds_and_addresses(self):
        doc = deepcopy(DOC)
        hud = doc["samples"][0]["hud"]
        hud["proofs"] = [deepcopy(hud["proofs"][0]), deepcopy(hud["proofs"][0])]
        with self.assertRaisesRegex(ValueError, "duplicate/ambiguous hint kind"):
            self.parse(doc)
        hud["proofs"][1]["address"] = "0x10000"
        with self.assertRaisesRegex(ValueError, "duplicate/ambiguous hint kind"):
            self.parse(doc)
        self.reject(["samples", 0, "hud", "proofs", 1, "address"], "0x1d3cc5ec570", "ambiguous control address")
        doc = deepcopy(DOC)
        for sample in doc["samples"]:
            sample["hud"]["proofs"] = sample["hud"]["proofs"][1:]
        with self.assertRaisesRegex(ValueError, "unique HUD hint required"):
            self.parse(doc)

    def test_path_nodes_edges_and_vtables_not_just_success_labels(self):
        prefix = ["samples", 0, "hud", "proofs", 0, "initial"]
        for suffix, value, reason in ((["nodes"], [], "path length"), (["edges"], [], "edges.length"),
                                      (["nodes", 0, "parent136"], "0x10000", "parent136 chain"),
                                      (["nodes", 0, "vtable"], "0x10000", "ancestor range"),
                                      (["nodes", 0, "bit20Set"], False, "bit20Set"),
                                      (["nodes", 0, "flags192"], "0x100000000", "uint32"),
                                      (["controlVtable"], "0x10000", "controlVtable"),
                                      (["edges", 0, "child"], "0x10000", "child"),
                                      (["edges", 0, "parent"], "0x10000", "parent"),
                                      (["edges", 0, "childParent136"], "0x10000", "childParent136"),
                                      (["edges", 0, "downwardMembership"], "unknown", "downwardMembership")):
            self.reject(prefix + suffix, value, reason)

    def test_inline_witness_raw_bytes_offsets_and_shape(self):
        prefix = ["samples", 0, "hud", "proofs", 0, "initial", "edges", 0, "witness"]
        for suffix, value, reason in (([], None, "witness object"), (["pointerRawHex"], "00" * 8, "pointerRawHex"),
                                      (["inlineOffset"], 136, "inlineOffset"), (["inlineOffset"], 201, "child slot"),
                                      (["vector"], {}, "keys")):
            self.reject(prefix + suffix, value, reason)

    def test_vector_witness_shape_bounds_digest_and_raw_pointer(self):
        prefix = ["samples", 0, "hud", "proofs", 0, "initial", "edges", 1, "witness"]
        for suffix, value in ((["index"], 2), (["index"], True), (["pointerRawHex"], "00" * 8),
                              (["vector", "count124"], 513), (["vector", "count124"], 0),
                              (["vector", "address"], "0x10001"), (["vector", "data112"], "0x0"),
                              (["vector", "elementsSha256"], "0"), (["vector", "elementsSha256"], "A" * 64)):
            self.reject(prefix + suffix, value, "witness")

    def test_template_field_kind_offsets_and_minimap_parent(self):
        self.reject(["samples", 0, "hud", "proofs", 1, "initial", "templateField", "offset"], 472, "templateField")
        self.reject(["samples", 0, "hud", "proofs", 2, "recheck", "templateParentMatchesControl"], False, "templateParentMatchesControl")
        self.reject(["samples", 0, "hud", "proofs", 0, "initial", "allChainBit20Set"], False, "allChainBit20Set")
        original = DOC["samples"][0]["hud"]["proofs"][0]["flags192Stable"]
        self.reject(["samples", 0, "hud", "proofs", 0, "flags192Stable"], not original, "flags192Stable")

    def test_recheck_structure_recomputed_instead_of_trusting_structurestable(self):
        self.reject(["samples", 0, "hud", "proofs", 0, "recheck", "edges", 0, "witness", "inlineOffset"],
                    208, "actual two-pass structure")

    def test_journal_accounting_and_required_independent_raw_reads(self):
        self.reject(["readJournal", 0, "ordinal"], 2, "ordinal")
        self.reject(["counters", "requestedBytes"], 1, "counters.requestedBytes")
        self.reject(["samples", 1, "counters", "readAttempts"], 0, "readAttempts")
        doc = deepcopy(DOC)
        address = DOC["samples"][0]["hud"]["proofs"][0]["address"]
        boundary = doc["samples"][0]["counters"]["readAttempts"]
        for entry in doc["readJournal"][:boundary]:
            if entry["field"] == "verify.ui.body576" and entry["address"] == address:
                entry["field"] = "forged-label"
        with self.assertRaisesRegex(ValueError, "successful body576 journal read missing"):
            self.parse(doc)

    def test_cross_sample_address_change_even_with_internally_consistent_paths(self):
        doc = deepcopy(DOC)
        old, new = 0x1d3cc5ec570, 0x1d3cc5ec580
        def remap(value):
            return json.loads(json.dumps(value).replace(hex(old), hex(new)).replace(
                old.to_bytes(8, "little").hex(), new.to_bytes(8, "little").hex()))
        doc["samples"][1] = remap(doc["samples"][1])
        first = doc["samples"][0]["counters"]["readAttempts"]
        last = doc["samples"][1]["counters"]["readAttempts"]
        doc["readJournal"][first:last] = remap(doc["readJournal"][first:last])
        with self.assertRaisesRegex(ValueError, r"samples\[1\].hints"):
            self.parse(doc)

    def test_strict_json_duplicate_keys_and_nonfinite_constants(self):
        for raw, reason in ((b'{"schema":"a","schema":"b"}', "duplicate JSON key"),
                            (b'{"x":NaN}', "non-finite JSON"), (b'[]', "root object"), (b'{', "Expecting")):
            with self.subTest(raw=raw), self.assertRaisesRegex(ValueError, reason):
                self.parse(raw=raw)


class CaptureChecks(unittest.TestCase):
    def capture(self, memory=None, raw=RAW, digest=None, engine=None):
        memory = memory or memory_fixture()
        output = str(w.HERE / "MEMORY_ONLY_NOT_WRITTEN.json")
        args = argparse.Namespace(pid=PID, expected_start_utc=START, label="results", out=output,
                                  reference=str(reference_path()) if raw is not None else None,
                                  reference_sha256=(w.sha256(raw) if digest is None else digest) if raw is not None else digest)
        sink, calls = f.Sink(), []
        original_open = Path.open
        def open_path(path, mode="r", *args, **kwargs):
            if str(path) == output:
                self.assertEqual(mode, "x")
                return sink
            if path == reference_path() and mode == "rb" and raw is not None:
                return io.BytesIO(raw)
            return original_open(path, mode, *args, **kwargs)
        def factory(pid, start):
            calls.append((pid, start))
            return memory
        engine = engine or w.load_engine()
        with patch.object(Path, "open", open_path), patch.object(w, "load_engine", return_value=engine):
            report = w.capture(args, factory, memory.clock, memory.clock.sleep)
        self.assertEqual(json.loads(sink.saved), report)
        return report, memory, calls

    def test_actual_v4_capture_reloads_reference_addresses_twice_and_current_root_can_differ(self):
        report, memory, calls = self.capture()
        self.assertEqual(report["status"], "observed")
        self.assertEqual(report["schema"], w.SCHEMA)
        self.assertEqual(report["frozenEngineReport"]["schema"], "known-hud-phase-observation-v2")
        self.assertEqual(report["scriptSha256"], w.sha256(Path(w.__file__).read_bytes()))
        self.assertTrue(report["sourceBindingVerified"])
        self.assertTrue(report["reference"]["currentModuleMatch"])
        self.assertEqual(calls, [(PID, START_FT)])
        self.assertEqual(memory.close_count, 1)
        self.assertTrue(report["handleClosed"])
        self.assertEqual([s["actualOffsetSeconds"] for s in report["samples"]], [0, 1, 3])
        for sample in report["samples"]:
            self.assertNotEqual(sample["hud"]["root"], report["reference"]["root"])
            self.assertEqual(sample["hud"]["root"], hex(f.ROOT))
            self.assertEqual(sample["hud"]["verification"]["status"], "checked")
            for proof in sample["hud"]["proofs"]:
                self.assertIn("reference_address", proof["origins"])
                self.assertEqual(proof["currentRootReachability"], "bidirectional_path_observed")
                for prefix in ("ui.", "verify.ui."):
                    self.assertTrue(any(e["field"] == prefix + "body576" and e["address"] == proof["address"]
                                        and e["status"] == "read" for e in report["readJournal"]))
        self.assertEqual(report["counters"]["readAttempts"], len(report["readJournal"]))
        self.assertEqual(report["counters"]["requestedBytes"], sum(e["length"] for e in report["readJournal"]))
        self.assertLessEqual(report["counters"]["readAttempts"], 20000)
        self.assertLessEqual(report["counters"]["requestedBytes"], 2 * 1024 * 1024)
        self.assertEqual(report["phaseDecision"], "not_evaluated")
        self.assertFalse(report["operatorLabelIsObservedPhase"])
        self.assertFalse(report["productAcceptance"])

    def test_bad_reference_sha_stops_before_memory_factory(self):
        report, memory, calls = self.capture(digest="0" * 64)
        self.assertEqual(report["status"], "failed")
        self.assertIn("byte SHA256", report["error"]["message"])
        self.assertEqual((calls, memory.attempts, memory.close_count), ([], [], 0))
        self.assertIsNone(report["handleClosed"])

    def test_oversized_and_incomplete_references_fail_before_factory(self):
        doc = deepcopy(DOC)
        del doc["samples"][1]["hud"]["proofs"][0]["recheck"]
        for raw in (b" " * (8 * 1024 * 1024 + 1), json.dumps(doc).encode()):
            report, memory, calls = self.capture(raw=raw)
            self.assertEqual(report["status"], "failed")
            self.assertEqual((calls, memory.attempts), ([], []))

    def test_unchanged_module_comparison_checks_base_sha_and_image_size(self):
        for field, value in (("base", hex(int(DOC["module"]["base"], 16) + 4096)),
                             ("sha256", "0" * 64), ("imageSize", 1)):
            memory = memory_fixture()
            original = memory.attest
            memory.attest = lambda tick, key=field, new=value: dict(original(tick), **{key: new})
            report, memory, _ = self.capture(memory)
            self.assertEqual(report["status"], "failed")
            self.assertFalse(report["reference"]["currentModuleMatch"])
            self.assertEqual(memory.attempts, [])
            self.assertEqual(memory.close_count, 1)

    def test_current_hint_read_fault_is_retained_as_unknown_with_journal(self):
        memory = memory_fixture()
        control = int(DOC["samples"][0]["hud"]["proofs"][0]["address"], 16)
        memory.denied.append((control, control + 576))
        report, memory, _ = self.capture(memory)
        self.assertEqual(report["status"], "observed")
        for sample in report["samples"]:
            proof = next(p for p in sample["hud"]["proofs"] if p["kind"] == "hud")
            self.assertEqual(proof["initial"]["termination"], "control_read_failed")
            self.assertEqual(proof["currentRootReachability"], "unknown")
            self.assertEqual(sample["hud"]["currentTreeCoverage"], "unknown")
        self.assertTrue(any(e["status"] == "failed" and e["address"] == hex(control) for e in report["readJournal"]))
        self.assertEqual(memory.close_count, 1)

    def test_parent_chain_without_downward_membership_is_unknown(self):
        memory = memory_fixture()
        for offset in range(144, 576, 8):
            memory.pack(f.ROOT + offset, "Q", 0)
        report, _, _ = self.capture(memory)
        for sample in report["samples"]:
            self.assertTrue(all(p["currentRootReachability"] == "unknown" for p in sample["hud"]["proofs"]))
            self.assertTrue(any(e["downwardMembership"] == "not_listed_in_read_fields"
                                for p in sample["hud"]["proofs"] for e in p["initial"]["edges"]))
            self.assertEqual(sample["hud"]["currentTreeCoverage"], "known_graph_complete")
            self.assertFalse(sample["hud"]["coverageIsPhaseEvidence"])

    def test_root_changed_between_current_passes_invalidates_proofs(self):
        memory = memory_fixture()
        new_root = 0x60000000
        memory.allocate(new_root, 576)
        memory.pack(new_root, "Q", memory.base + FIXTURE_ENGINE.p.ROLES["rootVtable"][1])
        calls = [0]
        def after_read(address, size):
            if address == memory.base + FIXTURE_ENGINE.p.ROLES["uiSingleton"][1]:
                calls[0] += 1
                if calls[0] == 2:
                    memory.pack(f.SINGLETON + 136, "Q", new_root)
        memory.after_read = after_read
        report, _, _ = self.capture(memory)
        hud = report["samples"][0]["hud"]
        self.assertEqual(hud["verification"]["reason"], "root_changed_between_passes")
        self.assertTrue(all(p["currentRootReachability"] == "unknown" for p in hud["proofs"]))

    def test_flags_change_remains_separate_from_membership(self):
        memory = memory_fixture()
        control = int(DOC["samples"][0]["hud"]["proofs"][0]["address"], 16)
        changed = [False]
        def after_read(address, size):
            if address == control and size == 576 and not changed[0]:
                changed[0] = True
                memory.pack(control + 192, "I", 0x18)
        memory.after_read = after_read
        report, _, _ = self.capture(memory)
        proof = next(p for p in report["samples"][0]["hud"]["proofs"] if p["kind"] == "hud")
        self.assertTrue(proof["structureStable"])
        self.assertFalse(proof["flags192Stable"])
        self.assertEqual(proof["currentRootReachability"], "bidirectional_path_observed")
        self.assertEqual(report["phaseDecision"], "not_evaluated")

    def test_hud_partition_stop_keeps_coverage_unknown_and_recheck(self):
        engine = w.load_engine()
        with patch.dict(engine.PARTITIONS, hud=(5, 560 * 1024, 1.35)):
            report, memory, _ = self.capture(engine=engine)
        self.assertEqual(report["samples"][0]["hud"]["traversal"]["stopReason"], "partition_limit")
        self.assertEqual(report["samples"][0]["hud"]["currentTreeCoverage"], "unknown")
        self.assertEqual(memory.close_count, 1)
        self.assertTrue(report["handleClosed"])

    def test_global_read_budget_and_blocking_overrun_close_and_keep_partial_evidence(self):
        engine, memory = w.load_engine(), memory_fixture()
        original_attest = memory.attest
        def lower_budget_after_reference_validation(tick):
            engine.p.MAX_READS = 800
            return original_attest(tick)
        memory.attest = lower_budget_after_reference_validation
        # The retained report was made under 20000; lower only this current fake capture's budget.
        with patch.object(engine.p, "MAX_READS", engine.p.MAX_READS):
            report, memory, _ = self.capture(memory, engine=engine)
        self.assertEqual(report["status"], "budget_exhausted")
        self.assertLessEqual(report["counters"]["readAttempts"], 800)
        self.assertEqual(len(report["readJournal"]), len(memory.attempts))
        self.assertEqual(memory.close_count, 1)
        memory = memory_fixture()
        memory.after_read = lambda *args: setattr(memory.clock, "now", 11.0)
        report, memory, _ = self.capture(memory)
        self.assertEqual(report["status"], "budget_exhausted")
        self.assertEqual(len(memory.attempts), 1)
        self.assertEqual(report["deadlineOverrunSeconds"], 1.0)
        self.assertEqual(memory.close_count, 1)

    def test_failed_short_reads_charged_and_byte_time_limits_precede_backend(self):
        engine, memory = w.load_engine(), memory_fixture()
        reader = engine.Reader(memory, memory.clock, memory.clock())
        with self.assertRaises(OSError):
            reader.read(0x70000000, 8, "fixture.failed")
        with patch.object(memory, "read", return_value=b"abc"), self.assertRaises(OSError):
            reader.read(0x70000000, 8, "fixture.short")
        self.assertEqual((reader.calls, reader.requested, reader.returned), (2, 16, 3))
        self.assertEqual([entry["status"] for entry in reader.journal], ["failed", "failed"])
        attempts = list(memory.attempts)
        reader.requested = engine.p.MAX_BYTES - engine.p.RESERVE_BYTES
        with self.assertRaises(engine.p.BudgetExceeded):
            reader.read(f.ROOT, 1, "fixture.bytes.blocked")
        reader.requested = 0
        memory.clock.now = engine.p.MAX_SECONDS
        with self.assertRaises(engine.p.BudgetExceeded):
            reader.read(f.ROOT, 1, "fixture.deadline.blocked")
        self.assertEqual(memory.attempts, attempts)

    def test_partition_attempt_bound_reserves_stability_and_preserves_caps(self):
        engine, memory = w.load_engine(), memory_fixture()
        reader = engine.Reader(memory, memory.clock, memory.clock())
        with reader.partition("hud"):
            for _ in range(5000):
                reader.read(f.ROOT, 8, "fixture.hud")
            with self.assertRaises(engine.SampleLimit):
                reader.read(f.ROOT, 8, "fixture.hud.blocked")
        with reader.partition("stability"):
            reader.read(f.ROOT, 8, "fixture.final")
        self.assertEqual(reader.calls, 5001)
        self.assertEqual(len(reader.journal), 5001)
        self.assertEqual((engine.p.MAX_SECONDS, engine.p.MAX_BYTES, engine.p.MAX_READS, engine.p.MAX_SINGLE),
                         (10.0, 2 * 1024 * 1024, 20000, 65536))
        self.assertEqual(engine.PARTITIONS, {"camera": (768, 16 * 1024, 0.4), "world": (96, 8 * 1024, 0.2),
                                           "hud": (5000, 560 * 1024, 1.35), "stability": (560, 88 * 1024, 0.6)})

    def test_identity_change_before_reads_closes_handle(self):
        memory = memory_fixture()
        memory.start += 1
        report, memory, _ = self.capture(memory)
        self.assertEqual(report["status"], "failed")
        self.assertEqual(memory.attempts, [])
        self.assertEqual(memory.close_count, 1)

    def test_no_reference_still_uses_frozen_current_tree_path(self):
        report, memory, _ = self.capture(raw=None)
        self.assertEqual(report["status"], "observed")
        self.assertIsNone(report["reference"])
        self.assertTrue(all(p["origins"] == ["current_root_walk"] for p in report["samples"][0]["hud"]["proofs"]))
        self.assertEqual(memory.close_count, 1)

    def test_collision_preserves_existing_bytes_before_factory(self):
        target = Path(w.__file__).resolve()
        before = target.read_bytes()
        args = argparse.Namespace(pid=PID, expected_start_utc=START, label="results", out=str(target),
                                  reference=str(reference_path()), reference_sha256=REFERENCES["running"])
        with self.assertRaises(FileExistsError):
            w.capture(args, lambda *args: self.fail("factory called on output collision"))
        self.assertEqual(target.read_bytes(), before)

    def test_outside_v4_and_resolved_parent_escape_rejected_before_loader(self):
        for output in (w.V3_PATH, w.HERE / "../v3/NOT_WRITTEN.json"):
            with patch.object(w, "load_engine", side_effect=AssertionError("loader should not start")):
                with self.assertRaisesRegex(ValueError, "v4 output must remain below"):
                    w.capture(argparse.Namespace(out=str(output)))

    def test_missing_source_or_drift_stops_before_memory_factory_without_pin_refresh(self):
        original = Path.read_bytes
        for key in w.SOURCE_BYTES:
            path = Path(FIXTURE_ENGINE.v3_attestation["sourceBinding"]["sourceRoot"]) / key
            with self.subTest(source=key), patch.object(Path, "read_bytes", lambda p: original(p) + b"\n" if p == path else original(p)):
                report, memory, calls = self.capture()
            self.assertEqual(report["status"], "failed")
            self.assertIn("Source pin changed:", report["error"]["message"])
            self.assertEqual((calls, memory.attempts, memory.close_count), ([], [], 0))
            self.assertFalse(report["sourceBindingVerified"])

    def test_frozen_v1_v2_v3_script_drift_stops_before_execution(self):
        paths = ((w.V3_PATH, "Frozen v3 wrapper SHA256 mismatch"),
                 (Path(FIXTURE_ENGINE.__file__), "Frozen script pin changed"),
                 (Path(FIXTURE_ENGINE.p.__file__), "Frozen v1 probe SHA256 mismatch"))
        original = Path.read_bytes
        for target, reason in paths:
            with self.subTest(path=target), patch.object(Path, "read_bytes", lambda p: original(p) + b"\n" if p == target else original(p)):
                with self.assertRaisesRegex(ValueError, reason):
                    w.load_engine()

    def test_only_parser_and_sink_seams_differ_from_frozen_v3_engine(self):
        wrapper = ModuleType("independent_frozen_v3")
        wrapper.__file__ = str(w.V3_PATH)
        exec(compile(w.V3_PATH.read_bytes(), str(w.V3_PATH), "exec"), wrapper.__dict__)
        original, engine = wrapper.load_engine(), w.load_engine()
        self.assertEqual(engine.capture.__code__, original.capture.__code__)
        self.assertEqual(engine.same_identity.__code__, original.same_identity.__code__)
        for left, right in ((engine.Reader, original.Reader), (engine.Observation, original.Observation),
                            (engine.KnownTree, original.KnownTree), (engine.p.BoundedReader, original.p.BoundedReader),
                            (engine.p.WindowsMemory, original.p.WindowsMemory), (engine.p.Observation, original.p.Observation)):
            for name, method in vars(left).items():
                if hasattr(method, "__code__"):
                    self.assertEqual(method.__code__, getattr(right, name).__code__, (left.__name__, name))
        self.assertEqual(engine.p.load_sources.__code__, original.p.load_sources.__code__)
        self.assertEqual(engine.p.SOURCE_PINS, original.p.SOURCE_PINS)
        self.assertEqual(engine.p.load_sources.__globals__["SOURCE_PINS"], original.p.load_sources.__globals__["SOURCE_PINS"])
        self.assertEqual(engine.PARTITIONS, original.PARTITIONS)
        self.assertIs(engine.json.loads, json.loads)
        self.assertIsInstance(engine.parse_reference, w.V3ReferenceParser)

    def test_offline_attest_and_cli_mixed_mode_check(self):
        report = w.offline_attestation()
        self.assertFalse(report["nativeOrGameCallsMade"])
        self.assertEqual(report["realSamples"], 0)
        self.assertEqual(report["phaseDecision"], "not_evaluated")
        self.assertFalse(report["productAcceptance"])
        with patch.object(sys, "stderr", io.StringIO()), self.assertRaises(SystemExit) as error:
            w.main(["--offline-attest", "--pid", str(PID)])
        self.assertEqual(error.exception.code, 2)


def protected_hashes():
    root = w.HERE.parent
    paths = [p for p in root.iterdir() if p.is_file()]
    for version in ("v2", "v3"):
        paths.extend(p for p in (root / version).rglob("*") if p.is_file())
    return {str(path): w.sha256(path.read_bytes()) for path in sorted(paths)}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--report", required=True, help="New exclusive validation JSON below v4")
    args = parser.parse_args()
    output = Path(args.report).resolve()
    if not output.is_relative_to(w.HERE):
        parser.error("validation report must remain below v4")
    writes = []
    def write_audit(event, args):
        if event == "open":
            path, mode, flags = args
            if flags & (os.O_WRONLY | os.O_RDWR | os.O_CREAT | os.O_TRUNC | os.O_APPEND) and isinstance(path, (str, bytes)):
                target = Path(os.fsdecode(path)).resolve()
                if not target.is_relative_to(w.HERE):
                    raise AssertionError("Offline write outside assigned v4 tree: " + str(target))
                writes.append({"path": str(target), "mode": mode})
    sys.addaudithook(write_audit)
    before = protected_hashes()
    # Reserve the only on-disk output before tests; fixture capture sinks stay in memory.
    with output.open("x", encoding="utf-8", newline="\n") as destination:
        suite = unittest.TestSuite([unittest.defaultTestLoader.loadTestsFromTestCase(ReferenceChecks),
                                   unittest.defaultTestLoader.loadTestsFromTestCase(CaptureChecks)])
        result = unittest.TextTestRunner(verbosity=2).run(suite)
        after = protected_hashes()
        unchanged = before == after
        report = {"schema": "phase-probe-offline-validation-v4", "runMode": "offline_fixture_only", "python": sys.version,
                  "testsRun": result.testsRun, "failures": len(result.failures), "errors": len(result.errors),
                  "skipped": len(result.skipped), "successful": result.wasSuccessful() and unchanged and NATIVE_CALLS.call_count == 0,
                  "nativeWinDLLBlocked": True, "nativeWinDLLAttempts": NATIVE_CALLS.call_count, "realSamples": 0,
                  "phaseDecision": "not_evaluated", "productAcceptance": False, "operatorLabelIsObservedPhase": False,
                  "wrapperSha256": w.sha256(Path(w.__file__).read_bytes()), "selftestSha256": w.sha256(Path(__file__).read_bytes()),
                  "frozenV2FixtureHelper": {"path": str(V2_TEST_PATH), "sha256": V2_TEST_SHA256,
                                            "sourceBytesExecutedUnmodified": True, "legacyTestSuiteRerun": False},
                  "positiveReferences": {label: {"path": str(reference_path(label)), "sha256": digest,
                                                "bytes": len(reference_bytes(label))} for label, digest in REFERENCES.items()},
                  "protectedHistoricalFilesUnchanged": unchanged, "protectedHashesBefore": before, "protectedHashesAfter": after,
                  "writesObserved": writes,
                  "failureDetails": [{"test": str(test), "traceback": text} for test, text in result.failures + result.errors]}
        json.dump(report, destination, ensure_ascii=False, indent=2, allow_nan=False)
        destination.write("\n")
    print(json.dumps({key: report[key] for key in ("schema", "testsRun", "failures", "errors", "successful",
                      "nativeWinDLLBlocked", "nativeWinDLLAttempts", "realSamples", "protectedHistoricalFilesUnchanged")}, ensure_ascii=False))
    return 0 if report["successful"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
