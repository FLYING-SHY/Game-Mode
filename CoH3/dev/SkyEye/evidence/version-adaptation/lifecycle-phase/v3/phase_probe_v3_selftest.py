"""Offline-only fixture tests: frozen v2 checks plus v3 source/attestation rejection cases."""
import argparse
import ctypes
import io
import json
from pathlib import Path
import sys
from types import ModuleType
import unittest
from unittest.mock import patch
import phase_probe_v3 as w

TEST_PATH = w.V2_PATH.with_name("phase_probe_v2_selftest.py")
TEST_SHA256 = "944ea751f2917c95476b1c60de447059f530e8e45405e29bc797db2b18003055"


def load_frozen_checks():
    engine = w.load_engine()
    module = ModuleType("frozen_phase_probe_v2_checks")
    module.__file__ = str(TEST_PATH)
    with patch.dict(sys.modules, phase_probe_v2=engine):
        exec(compile(w.checked_bytes(TEST_PATH, TEST_SHA256), str(TEST_PATH), "exec"), module.__dict__)
    return module


f = load_frozen_checks()


class WrapperChecks(unittest.TestCase):
    def capture(self, memory=None, source_change=None, pins_change=None):
        memory = memory or f.fixture()
        output = str(w.HERE / "MEMORY_ONLY_NOT_WRITTEN.json")
        args = argparse.Namespace(pid=memory.pid, expected_start_utc=f.START, label="results", out=output,
                                  reference=None, reference_sha256=None)
        sink, calls = f.Sink(), []
        original_open, original_read = Path.open, Path.read_bytes

        def open_path(path, mode="r", *a, **kw):
            if str(path) == output:
                self.assertEqual(mode, "x")
                return sink
            return original_open(path, mode, *a, **kw)

        def read_bytes(path):
            raw = original_read(path)
            return source_change(path, raw) if source_change else raw

        def factory(pid, start):
            calls.append((pid, start))
            return memory

        with patch.object(Path, "open", open_path), patch.object(Path, "read_bytes", read_bytes):
            if pins_change:
                with patch.dict(w.CURRENT_SOURCE_PINS, pins_change):
                    report = w.capture(args, factory, memory.clock, memory.clock.sleep)
            else:
                report = w.capture(args, factory, memory.clock, memory.clock.sleep)
        self.assertEqual(json.loads(sink.saved), report)
        return report, memory, calls

    def test_offline_source_attestation_has_no_sampling(self):
        report = w.offline_attestation()
        self.assertEqual(report["runMode"], "offline_source_verification_only")
        self.assertFalse(report["nativeOrGameCallsMade"])
        self.assertEqual(report["realSamples"], 0)
        self.assertEqual(report["phaseDecision"], "not_evaluated")
        self.assertEqual({key: value["sha256"] for key, value in report["sourceManifest"].items()},
                         w.CURRENT_SOURCE_PINS)

    def test_effective_pin_scope_preserves_original_reference_pins(self):
        engine = w.load_engine()
        self.assertNotEqual(engine.p.SOURCE_PINS, w.CURRENT_SOURCE_PINS)
        self.assertEqual(engine.p.load_sources.__globals__["SOURCE_PINS"], w.CURRENT_SOURCE_PINS)
        self.assertIsNot(engine.p.load_sources.__globals__, engine.p.__dict__)
        self.assertEqual(engine.parse_reference.__globals__["p"].SOURCE_PINS,
                         engine.v3_attestation["sourceBinding"]["originalFrozenV1SourcePins"])
        raw = json.dumps(f.reference()).encode()
        self.assertIn("hud", engine.parse_reference(raw, w.sha256(raw), 123, f.START_FT, {}))

    def test_capture_and_core_code_objects_match_unmodified_frozen_sources(self):
        original = ModuleType("independent_frozen_v2")
        original.__file__ = str(w.V2_PATH)
        exec(compile(w.checked_bytes(w.V2_PATH, w.V2_SHA256), str(w.V2_PATH), "exec"), original.__dict__)
        engine = w.load_engine()
        for name in ("capture", "parse_reference", "same_identity"):
            self.assertEqual(getattr(engine, name).__code__, getattr(original, name).__code__)
        for left, right in ((engine.Reader, original.Reader), (engine.Observation, original.Observation),
                            (engine.KnownTree, original.KnownTree),
                            (engine.p.BoundedReader, original.p.BoundedReader),
                            (engine.p.WindowsMemory, original.p.WindowsMemory)):
            for name, method in vars(left).items():
                if hasattr(method, "__code__"):
                    self.assertEqual(method.__code__, getattr(right, name).__code__, (left.__name__, name))
        self.assertEqual(engine.p.load_sources.__code__, original.p.load_sources.__code__)
        differences = {key for key in engine.p.load_sources.__globals__
                       if engine.p.load_sources.__globals__[key] is not engine.p.__dict__[key]}
        self.assertEqual(differences, {"SOURCE_PINS", "load_sources"})

    def test_positive_fixture_schema_provenance_and_no_phase_claim(self):
        report, memory, calls = self.capture()
        self.assertEqual(report["schema"], w.SCHEMA)
        self.assertEqual(report["status"], "observed")
        self.assertEqual(report["frozenEngineReport"]["schema"], "known-hud-phase-observation-v2")
        self.assertEqual(report["frozenEngineReport"]["scriptSha256"], w.V2_SHA256)
        self.assertTrue(report["frozenEngineReport"]["reportedDiskShaMatchesExecutedSource"])
        self.assertEqual(report["scriptSha256"], w.sha256(Path(w.__file__).read_bytes()))
        a = report["executionAttestation"]
        self.assertEqual(a["measurementEngine"]["sha256"], w.V2_SHA256)
        self.assertEqual(a["baseProbe"]["sha256"], w.V1_SHA256)
        self.assertTrue(a["sourceBinding"]["loaderCodeUnchanged"])
        self.assertFalse(a["sourceBinding"]["sourceFilesExecuted"])
        self.assertFalse(a["boundaries"]["productSnapshotCalled"])
        self.assertFalse(a["boundaries"]["phasePredicateImplemented"])
        self.assertTrue(report["sourceBindingVerified"])
        self.assertFalse(report["operatorLabelIsObservedPhase"])
        self.assertFalse(report["productAcceptance"])
        self.assertEqual(report["phaseDecision"], "not_evaluated")
        self.assertEqual([s["scheduledOffsetSeconds"] for s in report["samples"]], [0, 1, 3])
        self.assertEqual(report["counters"]["readAttempts"], len(report["readJournal"]))
        self.assertEqual(report["counters"]["requestedBytes"], sum(e["length"] for e in report["readJournal"]))
        self.assertEqual(calls, [(123, f.START_FT)])
        self.assertEqual(memory.close_count, 1)
        self.assertTrue(report["handleClosed"])
        for sample in report["samples"]:
            self.assertEqual(sample["hud"]["phaseDecision"], "not_evaluated")
            self.assertTrue(any(item["currentRootReachability"] == "bidirectional_path_observed"
                                for item in sample["hud"]["proofs"]))

    def test_every_source_drift_fails_before_factory_and_keeps_v3_failure(self):
        for relative in w.CURRENT_SOURCE_PINS:
            target = w.ROOT / relative
            with self.subTest(relative=relative):
                report, memory, calls = self.capture(source_change=lambda path, raw: raw + b"\n"
                                                     if path == target else raw)
                self.assertEqual(report["status"], "failed")
                self.assertIn("Source pin changed:", report["error"]["message"])
                self.assertEqual(report["schema"], w.SCHEMA)
                self.assertFalse(report["sourceBindingVerified"])
                self.assertEqual((calls, memory.attempts, memory.close_count), ([], [], 0))
                self.assertIsNone(report["handleClosed"])
                self.assertIsNone(report["frozenEngineReport"]["scriptSha256"])

    def test_wrong_fixed_pin_is_not_automatically_refreshed(self):
        key = "client/CoH3MemoryTool/PlayerInfoReader.cs"
        report, memory, calls = self.capture(pins_change={key: "0" * 64})
        self.assertEqual(report["status"], "failed")
        self.assertIn("expected=" + "0" * 64, report["error"]["message"])
        self.assertEqual((calls, memory.attempts), ([], []))

    def test_role_guard_survives_source_loader_rebinding(self):
        key = "client/CoH3MemoryTool/GameLayout52426.cs"
        target = w.ROOT / key
        raw = target.read_bytes()
        changed = raw.replace(b"0x6187E40", b"0x6187E41")
        self.assertNotEqual(raw, changed)
        report, memory, calls = self.capture(source_change=lambda path, data: changed if path == target else data,
                                             pins_change={key: w.sha256(changed)})
        self.assertEqual(report["status"], "failed")
        self.assertIn("Known candidate SHA/individual RVA roles differ", report["error"]["message"])
        self.assertEqual((calls, memory.attempts), ([], []))

    def test_frozen_v2_drift_rejected_before_execution(self):
        original = Path.read_bytes
        with patch.object(Path, "read_bytes", lambda path: original(path) + b"\n" if path == w.V2_PATH else original(path)):
            with self.assertRaisesRegex(ValueError, "Frozen script pin changed"):
                w.load_engine()

    def test_frozen_v1_drift_rejected_by_original_v2_loader(self):
        original = Path.read_bytes
        with patch.object(Path, "read_bytes", lambda path: original(path) + b"\n" if path == w.V1_PATH else original(path)):
            with self.assertRaisesRegex(ValueError, "Frozen v1 probe SHA256 mismatch"):
                w.load_engine()

    def test_existing_v3_output_preserved_before_factory(self):
        target = Path(w.__file__).resolve()
        before = target.read_bytes()
        args = argparse.Namespace(pid=123, expected_start_utc=f.START, label="results", out=str(target),
                                  reference=None, reference_sha256=None)
        with self.assertRaises(FileExistsError):
            w.capture(args, lambda *args: self.fail("fixture factory called on collision"))
        self.assertEqual(target.read_bytes(), before)

    def test_outside_v3_output_rejected_before_engine_loading(self):
        args = argparse.Namespace(out=str(w.V1_PATH))
        with patch.object(w, "load_engine", side_effect=AssertionError("engine loading should not start")):
            with self.assertRaisesRegex(ValueError, "v3 output must remain below"):
                w.capture(args)

    def test_identity_mismatch_still_stops_vmread_and_closes(self):
        memory = f.fixture()
        memory.start += 1
        report, memory, _ = self.capture(memory)
        self.assertEqual(report["status"], "failed")
        self.assertEqual(memory.attempts, [])
        self.assertEqual(memory.close_count, 1)
        self.assertTrue(report["sourceBindingVerified"])

    def test_v3_report_is_not_silently_accepted_as_v1_reference(self):
        report, _, _ = self.capture()
        raw = json.dumps(report).encode()
        with self.assertRaisesRegex(ValueError, "Reference requires an observed running/paused v1"):
            w.load_engine().parse_reference(raw, w.sha256(raw), 123, f.START_FT, {})

    def test_json_adapter_delegates_reference_json_unchanged(self):
        engine = w.load_engine()
        self.assertIs(engine.json.loads, json.loads)
        self.assertIs(engine.json.dumps, json.dumps)

    def test_offline_cli_rejects_combined_live_parameters(self):
        with patch.object(sys, "stderr", io.StringIO()), self.assertRaises(SystemExit) as error:
            w.main(["--offline-attest", "--pid", "30504"])
        self.assertEqual(error.exception.code, 2)


def protected_hashes():
    paths = [path for directory in (w.V1_PATH.parent, w.V2_PATH.parent)
             for path in directory.iterdir() if path.is_file()]
    return {str(path.relative_to(w.ROOT)): w.sha256(path.read_bytes()) for path in sorted(paths)}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--report", help="New exclusive validation JSON below v3")
    args = parser.parse_args()
    before = protected_hashes()
    output = None
    if args.report:
        output = Path(args.report).resolve()
        if not output.is_relative_to(w.HERE):
            parser.error("validation report must remain below v3")
        output = output.open("x", encoding="utf-8", newline="\n")
    suite = unittest.TestSuite([unittest.defaultTestLoader.loadTestsFromTestCase(f.Checks),
                                unittest.defaultTestLoader.loadTestsFromTestCase(WrapperChecks)])
    # Any accidental attempt to resolve the native game API turns the fixture run into a failure.
    with patch.object(ctypes, "WinDLL", side_effect=AssertionError("Actual native calls blocked in v3 offline suite"), create=True):
        result = unittest.TextTestRunner(verbosity=2).run(suite)
    after = protected_hashes()
    unchanged = before == after
    report = {"schema": "phase-probe-offline-validation-v3", "runMode": "offline_fixture_only",
              "python": sys.version, "testsRun": result.testsRun, "failures": len(result.failures),
              "errors": len(result.errors), "skipped": len(result.skipped),
              "successful": result.wasSuccessful() and unchanged,
              "nativeWinDLLBlocked": True, "realSamples": 0, "phaseDecision": "not_evaluated",
              "frozenV2TestSource": {"path": str(TEST_PATH), "sha256": TEST_SHA256,
                                     "sourceBytesExecutedUnmodified": True},
              "wrapperSha256": w.sha256(Path(w.__file__).read_bytes()),
              "selftestSha256": w.sha256(Path(__file__).read_bytes()),
              "protectedHistoricalFilesUnchanged": unchanged, "protectedHashesBefore": before,
              "protectedHashesAfter": after,
              "failureDetails": [{"test": str(test), "traceback": text} for test, text in result.failures + result.errors]}
    if output:
        with output:
            json.dump(report, output, ensure_ascii=False, indent=2)
            output.write("\n")
    print(json.dumps({key: report[key] for key in ("schema", "testsRun", "failures", "errors", "successful",
                                                  "nativeWinDLLBlocked", "realSamples", "protectedHistoricalFilesUnchanged")},
                     ensure_ascii=False))
    return 0 if report["successful"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
