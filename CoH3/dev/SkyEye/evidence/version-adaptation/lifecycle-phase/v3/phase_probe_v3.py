#!/usr/bin/env python3
"""Source-bound v3 entrypoint for the unmodified, frozen v2 observation engine."""
import argparse
import hashlib
import json
from pathlib import Path
import time
from types import FunctionType, ModuleType

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
V1_PATH = ROOT / "evidence/version-adaptation/lifecycle-phase/phase_probe.py"
V2_PATH = ROOT / "evidence/version-adaptation/lifecycle-phase/v2/phase_probe_v2.py"
V1_SHA256 = "21af7198c86ea5f53b06e9df737251d81a21dd345ec243b8974beca75ec1d723"
V2_SHA256 = "e2c56be1b7b49f242b4549545e03e4a50d765e5de9c9cf7b8473a9966525e7c0"
CURRENT_SOURCE_PINS = {
    "client/CoH3MemoryTool/GameLayout52426.cs": "90af6a0f98c5c3c48dd9cad8595377ee90990938753e68b142899b707c596d47",
    "client/CoH3MemoryTool/PlayerInfoReader.cs": "39dde7a0f2bd80e2412fed3439870cf7a49a794a9aab0f812a9afee6f60c65d2",
    "evidence/version-adaptation/air/air_raw_snapshot.py": "0438ebbe4b29bcae3b6d428e46ce2db0fcdf58c87d93f6058f0432dc9ff765d4",
}
SCHEMA = "known-hud-phase-observation-v3"


def sha256(raw):
    return hashlib.sha256(raw).hexdigest()


def checked_bytes(path, expected):
    raw = Path(path).read_bytes()
    actual = sha256(raw)
    if actual != expected:
        raise ValueError("Frozen script pin changed: %s expected=%s actual=%s" % (path, expected, actual))
    return raw


def load_engine():
    """Execute checked original bytes; change only the source-loader globals and JSON sink."""
    raw = checked_bytes(V2_PATH, V2_SHA256)
    engine = ModuleType("frozen_phase_probe_v2_for_v3")
    engine.__file__ = str(V2_PATH)
    exec(compile(raw, engine.__file__, "exec"), engine.__dict__)
    # Frozen v2 load_base already checked and executed v1 bytes. Check its declared binding too.
    if engine.BASE_SHA256 != V1_SHA256 or Path(engine.p.__file__).resolve() != V1_PATH:
        raise ValueError("Frozen v2-to-v1 binding differs from the v3 declaration")
    p = engine.p
    original_loader = p.load_sources
    loader_globals = dict(original_loader.__globals__, SOURCE_PINS=dict(CURRENT_SOURCE_PINS))
    p.load_sources = FunctionType(original_loader.__code__, loader_globals, original_loader.__name__,
                                 original_loader.__defaults__, original_loader.__closure__)
    wrapper_path = Path(__file__).resolve()
    attestation = {
        "schema": "phase-probe-execution-attestation-v3",
        "wrapper": {"path": str(wrapper_path), "sha256": sha256(wrapper_path.read_bytes()),
                    "digestMeaning": "wrapper disk bytes at load; self-reported, not a signature"},
        "measurementEngine": {"path": str(V2_PATH), "sha256": V2_SHA256,
                              "compileFilename": engine.capture.__code__.co_filename,
                              "sourceBytesExecutedUnmodified": True, "entrypoint": "capture"},
        "baseProbe": {"path": str(V1_PATH), "sha256": V1_SHA256,
                      "compileFilename": original_loader.__code__.co_filename,
                      "sourceBytesExecutedUnmodified": True, "checkedBy": "frozen v2 load_base"},
        "sourceBinding": {
            "originalFrozenV1SourcePins": dict(p.SOURCE_PINS),
            "effectiveSourceLoaderPins": dict(CURRENT_SOURCE_PINS),
            "sourceRoot": str(ROOT), "sourceFilesExecuted": False,
            "loaderCodeUnchanged": p.load_sources.__code__ is original_loader.__code__,
            "override": "v1 load_sources same code object with a copied globals dict; only SOURCE_PINS replaced",
            "scope": "load_sources only; original v1 module SOURCE_PINS retained for unchanged v2 reference validation",
            "roleValidation": "unchanged v1 load_sources: exact source hashes, fixed game SHA, individual role RVAs",
            "policy": "fixed allowlist; no CLI pin override, automatic refresh, or arbitrary new source acceptance",
        },
        "reportAdapter": {
            "override": "v2 module-local json proxy: dump adds v3 provenance; other JSON operations delegate unchanged",
            "topLevelScriptSha256Meaning": "v3 wrapper disk SHA; original v2 report schema/SHA retained under frozenEngineReport",
            "measurementFunctionsChanged": False,
        },
        "boundaries": {
            "productSnapshotCalled": False, "productCSharpExecuted": False,
            "productBuildCorrespondenceEstablished": False, "phasePredicateImplemented": False,
            "operatorLabelIsObservedPhase": False, "productAcceptance": False,
            "referencePolicy": "original v2 parser accepts only genuine pinned v1 running/paused reports from the same process/module; v3 reports are not converted to references",
            "limitations": "source provenance plus bounded Python field observations; not product Snapshot or HUD coordinate/phase acceptance",
        },
    }
    engine.json = ReportJson(attestation)
    engine.v3_attestation = attestation
    return engine


class ReportJson:
    """Local serialization seam; all sampling, read journals and cleanup stay in frozen v2."""
    def __init__(self, attestation):
        self.attestation = attestation

    def __getattr__(self, name):
        return getattr(json, name)

    def dump(self, report, destination, **kwargs):
        report["frozenEngineReport"] = {
            "schema": report["schema"], "scriptSha256": report.get("scriptSha256"),
            "reportedDiskShaMatchesExecutedSource": (report.get("scriptSha256") ==
                                                       self.attestation["measurementEngine"]["sha256"])
            if "scriptSha256" in report else None,
        }
        report["schema"] = SCHEMA
        report["scriptSha256"] = self.attestation["wrapper"]["sha256"]
        report["executionAttestation"] = self.attestation
        manifest = report.get("sourceManifest")
        report["sourceBindingVerified"] = (manifest is not None and
            {key: value["sha256"] for key, value in manifest.items()} ==
            self.attestation["sourceBinding"]["effectiveSourceLoaderPins"])
        return json.dump(report, destination, **kwargs)


def capture(args, memory_factory=None, clock=time.monotonic, sleeper=time.sleep):
    # v2 retains its exclusive 'x' creation check. Limit the wrapper's outputs to its assigned tree.
    output = Path(args.out).resolve()
    if not output.is_relative_to(HERE):
        raise ValueError("v3 output must remain below %s" % HERE)
    engine = load_engine()
    factory = engine.p.WindowsMemory if memory_factory is None else memory_factory
    return engine.capture(args, memory_factory=factory, clock=clock, sleeper=sleeper)


def offline_attestation():
    engine = load_engine()
    manifest = engine.p.load_sources()
    return {"schema": "phase-probe-source-check-v3", "runMode": "offline_source_verification_only",
            "nativeOrGameCallsMade": False, "realSamples": 0, "phaseDecision": "not_evaluated",
            "sourceManifest": manifest, "executionAttestation": engine.v3_attestation}


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--offline-attest", action="store_true", help="Verify fixed sources only; no process access")
    parser.add_argument("--pid", type=int)
    parser.add_argument("--expected-start-utc", help="Exact UTC, up to seven fractional digits")
    parser.add_argument("--label", choices=("running", "paused", "results", "mainmenu", "newmatch"), help="Operator label")
    parser.add_argument("--out", help="New JSON below v3; exclusive output creation")
    parser.add_argument("--reference", help="Optional genuine frozen v1 running/paused JSON from the same process")
    parser.add_argument("--reference-sha256", help="Exact lower-case reference byte SHA256")
    args = parser.parse_args(argv)
    if args.offline_attest:
        if any(value is not None for key, value in vars(args).items() if key != "offline_attest"):
            parser.error("--offline-attest is a standalone source-only check")
        print(json.dumps(offline_attestation(), ensure_ascii=False, indent=2))
        return 0
    if any(getattr(args, key) is None for key in ("pid", "expected_start_utc", "label", "out")):
        parser.error("live invocation requires --pid, --expected-start-utc, --label and --out")
    report = capture(args)
    print(json.dumps({"output": str(Path(args.out).resolve()), "status": report["status"],
                      "samples": len(report["samples"]), "phaseDecision": report["phaseDecision"],
                      "counters": report.get("counters"), "handleClosed": report["handleClosed"],
                      "schema": report["schema"], "sourceBindingVerified": report["sourceBindingVerified"]},
                     ensure_ascii=False))
    return 0 if report["status"] == "observed" else 3


if __name__ == "__main__":
    raise SystemExit(main())
