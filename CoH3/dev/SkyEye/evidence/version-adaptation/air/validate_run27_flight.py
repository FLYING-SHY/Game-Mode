"""Validate retained run27 reader output against its native driver and raw captures.

Offline only. A passing reader window is narrower than full feature/UI acceptance.
The third raw capture is a later paused state, outside the sampled wall-clock window.
"""
import hashlib
import json
import math
from pathlib import Path
import struct
from datetime import datetime


def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def number(value):
    return int(value, 0) if isinstance(value, str) else int(value)


def timestamp(value):
    return datetime.fromisoformat(value.replace("Z", "+00:00"))


def validate(root):
    report_path = root / "maintenance-run-27-p47-compact-flight" / "maintenance-live.json"
    driver_path = root / "air" / "run27-driver-result.json"
    report, driver = load(report_path), load(driver_path)
    results = {item["Feature"]: item for item in report["results"]}
    event = results["event_window"]
    projectile, air = event["Evidence"]["projectile"], event["Evidence"]["air"]
    polls, frames = projectile["polls"], air["frames"]
    zones = [zone for frame in frames for zone in frame["decoded"]]
    positive = [poll for poll in polls if poll["airDecodedCount"] > 0]
    assert report["licenseState"] == "active" and report["readerOpenAttempts"] == 1
    assert report["snapshotAttempts"] == projectile["eventSnapshotAttempts"] + 1
    assert driver["childExitCodeRecorded"] and driver["childExitCode"] == report["exitCode"] == 5
    assert driver["error"] is None and not driver["childKilledAtTimeout"] and driver["pauseExitCode"] == 0
    assert event["Status"] == air["status"] == "observed" and not event.get("Error")
    assert projectile["stopReason"] == "deadline" and projectile["budgetMilliseconds"] == 20000
    assert projectile["elapsedMilliseconds"] >= projectile["budgetMilliseconds"]
    assert projectile["overrunMilliseconds"] == projectile["elapsedMilliseconds"] - projectile["budgetMilliseconds"]
    assert len(polls) == projectile["rounds"] == projectile["projectileQueryAttempts"] == air["queryAttempts"] == 184
    assert [poll["ordinal"] for poll in polls] == list(range(1, len(polls) + 1))
    assert all(poll["status"] == "completed" for poll in polls)
    assert all(poll["finishedMilliseconds"] >= poll["startedMilliseconds"] for poll in polls)
    assert all(left["finishedMilliseconds"] <= right["startedMilliseconds"] for left, right in zip(polls, polls[1:]))
    assert len(frames) == air["retainedFrames"] == 46 and len(frames) + air["droppedFrames"] == len(polls)
    assert len(zones) == air["retainedDecoded"] == air["decodedObservationCount"] == air["nonzeroPolls"] == len(positive) == 44
    assert sum(poll["airDecodedCount"] for poll in polls) == len(zones) and air["droppedDecoded"] == 0
    assert sum(frame["decodedCount"] for frame in frames) == len(zones)
    by_utc = {poll["utc"]: poll for poll in polls}
    for frame in frames:
        poll = by_utc[frame["utc"]]
        assert frame["pollStartedMilliseconds"] == poll["startedMilliseconds"]
        assert frame["readFinishedMilliseconds"] == poll["finishedMilliseconds"]
        assert frame["match"] == projectile["initialMatch"] and frame["epoch"] == projectile["initialEpoch"]
        assert len(frame["decoded"]) == frame["decodedCount"] == poll["airDecodedCount"]
        assert frame["droppedDecodedInThisPoll"] == 0
    assert not frames[0]["decoded"] and not frames[-1]["decoded"]
    zone = zones[0]
    assert zone["Key"].startswith("flight:") and all(item == zone for item in zones)
    for key in ("X", "Y", "Z", "DX", "DZ", "Offset", "HalfWidth", "Length", "Radius"):
        assert math.isfinite(zone[key]), key
    assert abs(math.hypot(zone["DX"], zone["DZ"]) - 1) < 1e-5
    assert (zone["HalfWidth"], zone["Length"], zone["Offset"], zone["Radius"]) == (5, 45, 0, 0)
    assert not zone["Rocket"] and not zone["Point"]
    first_later_zero = next(poll for poll in polls if poll["ordinal"] > positive[-1]["ordinal"] and poll["airDecodedCount"] == 0)
    assert results["same_reader_recovery"]["Status"] == "not_triggered"
    raw_summaries = []
    for leaf in ("live-raw-11-p47-compact-middle.json", "live-raw-12-p47-compact-late.json", "live-raw-13-p47-compact-after-clear.json"):
        path = root / "air" / leaf
        raw = load(path)
        snap = raw["snapshot"]
        assert raw["status"] == "complete_raw_candidate_snapshot" and raw["pid"] == 31572
        assert snap["stableEntries"] and snap["stableRootsAndDescriptors"] and all(snap["scanComplete"].values()) and not snap["errors"]
        assert number(snap["roots"]["world"]) == int(projectile["initialMatch"].split(":")[2])
        plane = next(item for item in snap["airEntities"] if f"flight:{number(item['entity'])}:{item['generation']}" == zone["Key"])
        body = bytes.fromhex(plane["raw392Hex"])
        actor = bytes.fromhex(plane["actor"]["rawHex"])[:184]
        assert len(body) == 392 and len(actor) == 184
        assert struct.unpack_from("<I", body, 104)[0] == plane["generation"]
        assert struct.unpack_from("<I", body, 108)[0] == plane["registryIndex"]
        assert struct.unpack_from("<Q", actor)[0] - number(raw["moduleBase"]) == 0x3D926E0
        assert struct.unpack_from("<I", actor, 36)[0] + number(snap["roots"]["arena"]) == number(plane["entity"])
        assert actor[16] == 2 and struct.unpack_from("<I", actor, 64)[0] == 1
        cast = next(item for item in snap["timedAbilities"] if item.get("blueprint") == r"abilities\races\american\aircraft\strafing_run_p47_us" and number(item["player"]) == zone["Owner"])
        target = struct.unpack_from("<3f", bytes.fromhex(cast["target"]["raw20Hex"]), 4)
        direction = cast["fields"]["direction40"]
        norm = math.hypot(direction[0], direction[2])
        assert cast["fields"]["phase76"] == 2 and cast["fields"]["instance68"] == 0xFFFFFFFF
        assert cast["slot"] == zone["Slot"] == 0
        assert all(abs(zone[key] - value) < 1e-5 for key, value in zip(("X", "Y", "Z"), target))
        assert abs(zone["DX"] - direction[0] / norm) < 1e-6 and abs(zone["DZ"] - direction[2] / norm) < 1e-6
        actor_direction = struct.unpack_from("<2f", actor, 96)
        assert abs(math.hypot(*actor_direction) - 1) < 1e-5
        assert max(abs(actor_direction[0] - zone["DX"]), abs(actor_direction[1] - zone["DZ"])) < 1e-4
        xyz = plane["position44"]
        along = (xyz[0] - zone["X"]) * zone["DX"] + (xyz[2] - zone["Z"]) * zone["DZ"]
        raw_summaries.append({"path": str(path), "sha256": digest(path), "timestampUtc": raw["timestampUtc"], "entity": plane["entity"], "generation": plane["generation"], "index": plane["registryIndex"], "position": xyz, "alongOriginalTargetMetres": along, "actorTarget": struct.unpack_from("<3f", actor, 20), "actorDirection96_100": actor_direction, "actorBytesUsed": len(actor), "castPhase": 2, "castInstance": 0xFFFFFFFF, "failedLegacyRecoveryGates": plane["failedRecoveryRawGates"]})
    assert raw_summaries[0]["alongOriginalTargetMetres"] < 0 < raw_summaries[1]["alongOriginalTargetMetres"] < 55
    assert raw_summaries[2]["alongOriginalTargetMetres"] > 55
    assert all(abs(raw_summaries[0]["actorTarget"][i] - zone[key]) < 1e-5 for i, key in enumerate(("X", "Y", "Z")))
    assert timestamp(positive[0]["utc"]) < timestamp(raw_summaries[0]["timestampUtc"]) < timestamp(raw_summaries[1]["timestampUtc"]) < timestamp(first_later_zero["utc"]) < timestamp(driver["pausedUtc"])
    assert timestamp(raw_summaries[2]["timestampUtc"]) > timestamp(driver["finishedUtc"])
    return {"status": "PASS_actual_reader_flight_fallback_window_only", "readerWindowAccepted": True, "gameFeatureAccepted": False, "readerReport": str(report_path), "readerReportSha256": digest(report_path), "driverReport": str(driver_path), "driverReportSha256": digest(driver_path), "readerAndOsExitCode": 5, "scope": air["scope"], "match": projectile["initialMatch"], "epoch": projectile["initialEpoch"], "completedQueries": len(polls), "elapsedMilliseconds": projectile["elapsedMilliseconds"], "overrunMilliseconds": projectile["overrunMilliseconds"], "retainedFrames": len(frames), "retainedDecoded": len(zones), "droppedFrames": air["droppedFrames"], "droppedDecoded": 0, "distinctFlightKeys": 1, "zone": zone, "firstPositive": positive[0], "lastPositive": positive[-1], "firstLaterZero": first_later_zero, "rawAircraft": raw_summaries, "naturalRecoveryStatus": "not_triggered", "nativeAdvanceSeconds": (timestamp(driver["pausedUtc"]) - timestamp(driver["resumedUtc"])).total_seconds(), "limitations": ["One local-owner flight, not 44 distinct airstrikes; all-current-row diagnostic scope does not prove enemy-only production GUI.", "The native game advanced for about 11 seconds and was then paused; the reader window lasted about 20 wall-clock seconds.", "Raw11/12 are within the sampled window. Raw13 is a later paused state with the same world/entity/generation, not a simultaneous read.", "This does not accept camera recovery, other air types, the complete game version, or protected deliverables."]}


if __name__ == "__main__":
    evidence_root = Path(__file__).resolve().parents[1]
    result = validate(evidence_root)
    output = Path(__file__).with_name("run27-flight-window-validation.json")
    with output.open("x", encoding="utf-8") as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
    print(json.dumps({"output": str(output), "status": result["status"], "readerWindowAccepted": result["readerWindowAccepted"], "gameFeatureAccepted": result["gameFeatureAccepted"]}, ensure_ascii=True))
