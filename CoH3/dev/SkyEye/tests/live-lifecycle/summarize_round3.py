"""Index retained observations; does not classify a game or execute product code."""
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
VISUAL = ROOT / "evidence/version-adaptation/session-lifecycle-20261122/round3-live"
RAW = ROOT / "evidence/version-adaptation/lifecycle-phase/v3/live-round3"
PID, START = 30504, 134397842153993114
GAME_SHA = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec"
STAGES = (
    ("mainmenu", "state-02-mainmenu-before", "state-03-mainmenu-after", "main menu with continue/start/armory/store/replay/mods", None),
    ("running", "state-12-running-before", "state-13-running-after", "game clock progresses 02:12 -> 02:15; resources 983 -> 999", "760a92a813aac94c94444dc2ee1b2d3d01a941c648305a68488f0194e008ac59"),
    ("paused", "state-15-paused-before", "state-16-paused-after", "Escape menu; game clock 02:58 and resources 1194 unchanged", "ba29370662feeab744da3fa20cbf3812eb69ae5d9b2fd79fcf7f63f4591696fd"),
    ("results", "state-20-results-before", "state-21-results-after", "match data - exit; Femontville 02:58, units tab", "fcf57146d7e032598dbe9967a7013163f8f1fb10373da0eca589b76f9a7100f8"),
)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def ref(path):
    return {"path": str(path), "bytes": path.stat().st_size, "sha256": sha(path)}


def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def utc(text):
    return datetime.fromisoformat(text.replace("Z", "+00:00"))


def check(condition, reason):
    if not condition:
        raise ValueError(reason)


def same_identity(value):
    return value["pid"] == PID and value["startFiletime"] == START and value["stillActive"] is True


def main():
    output = VISUAL / "round3-observation-index.json"
    check(not output.exists(), "Index already exists; preserve prior evidence")
    stages = []
    bindings = []
    for label, before, after, observation, expected_sha in STAGES:
        path = RAW / ("pid30504-" + label + "-001.json")
        doc = load(path)
        if expected_sha:
            check(sha(path) == expected_sha, label + ": retained report SHA differs")
        check(doc["schema"] == "known-hud-phase-observation-v3" and doc["status"] == "observed", label + ": report type/status")
        check(doc["pid"] == PID and doc["expectedStartFiletime"] == START, label + ": process binding")
        check(all(same_identity(doc[key]) for key in ("identityBefore", "identityAfter")), label + ": outer identity")
        check(doc["module"]["sha256"].lower() == GAME_SHA and doc["handleClosed"] is True, label + ": module/close")
        check(doc["sourceBindingVerified"] is True and doc["phaseDecision"] == "not_evaluated" and doc["productAcceptance"] is False, label + ": scope")
        check([item["scheduledOffsetSeconds"] for item in doc["samples"]] == [0, 1, 3], label + ": sample set")
        for sample in doc["samples"]:
            check(sample["sameProcessIdentity"] and all(same_identity(sample[key]) for key in ("identityBefore", "identityAfter")), label + ": sample identity")
        before_path, after_path = VISUAL / (before + ".json"), VISUAL / (after + ".json")
        b, a = load(before_path), load(after_path)
        for capture in (b, a):
            check(capture["ProcessId"] == PID and capture["ScreenshotSucceeded"] is True, label + ": visual process/capture")
            check((capture["Width"], capture["Height"]) == (2560, 1440), label + ": visual size")
        check(utc(b["CapturedAt"]) <= utc(doc["startedUtc"]) <= utc(doc["finishedUtc"]) <= utc(a["CapturedAt"]), label + ": capture time order")
        bindings.append((doc["module"], doc["sourceManifest"], doc["executionAttestation"]))
        observations = []
        for sample in doc["samples"]:
            hud = sample["hud"]
            positive = [proof for proof in hud["proofs"] if proof.get("currentRootReachability") == "bidirectional_path_observed" and proof.get("structureStable") is True and proof["initial"].get("firstPassPath") is True and proof["recheck"].get("firstPassPath") is True]
            observations.append({"offsetSeconds": sample["scheduledOffsetSeconds"], "reportedPositiveProofs": [{"kind": p["kind"], "address": p["address"], "allChainBit20Set": p["initial"].get("allChainBit20Set")} for p in positive], "knownGraphComplete": hud["traversal"]["knownGraphComplete"], "traversalStopReason": hud["traversal"]["stopReason"], "matchKeyCandidate": sample["world"].get("matchKeyCandidate")})
        stages.append({"observerVisualLabel": label, "observerVisualNote": observation, "visualClassificationMethod": "manual inspection of both retained pictures; no automatic classifier", "beforeMetadata": ref(before_path), "beforePicture": ref(VISUAL / (before + ".png")), "rawReport": ref(path), "afterMetadata": ref(after_path), "afterPicture": ref(VISUAL / (after + ".png")), "captureType": "selected-window PrintWindow; no composited-overlay claim", "reportedObservations": observations, "counters": doc["counters"], "elapsedSeconds": doc["elapsedSeconds"], "absenceClaim": False})
    check(all(binding == bindings[0] for binding in bindings), "Reports differ in module/source/engine binding")
    report = {"schema": "round3-visual-raw-observation-index-v1", "generatedUtc": datetime.now(timezone.utc).isoformat(), "generator": ref(Path(__file__)), "status": "recorded_material_consistency_only", "productAcceptance": False, "productSnapshotExecutedByProbe": False, "phasePredicateImplemented": False, "commonModule": bindings[0][0], "sourceManifest": bindings[0][1], "executionAttestation": bindings[0][2], "stageCount": len(stages), "stages": stages, "limits": ["Positive paths are recorded structural evidence; whole-graph negative coverage remains unknown.", "Same retained world/key at results is a counterexample to treating valid world data as an active match.", "No product GUI/render/audio/hotkey or protected delivery acceptance is asserted."]}
    with output.open("x", encoding="utf-8", newline="\n") as destination:
        json.dump(report, destination, ensure_ascii=False, indent=2, allow_nan=False)
        destination.write("\n")
    print(json.dumps({"output": str(output), "stages": len(stages), "status": report["status"], "productAcceptance": False}, ensure_ascii=False))


if __name__ == "__main__":
    main()
