"""Offline validation of historical run26 samples; later camera failure stays a failure."""
import base64
import hashlib
import json
import math
from pathlib import Path
import struct


def validate():
    root = Path(__file__).resolve().parents[1]
    report_path = root / "maintenance-run-26-p47-retained" / "maintenance-live.json"
    report = json.loads(report_path.read_text(encoding="utf8"))
    event = next(x for x in report["results"] if x["Feature"] == "event_window")
    projectile, air = event["Evidence"]["projectile"], event["Evidence"]["air"]
    polls, frames = projectile["polls"], air["frames"]
    completed = [x for x in polls if x["status"] == "completed"]
    positive = [x for x in polls if (x.get("airDecodedCount") or 0) > 0]
    zones = [zone for frame in frames for zone in frame["decoded"]]
    assert report["licenseState"] == "active" and report["readerOpenAttempts"] == 1
    assert report["exitCode"] == 3 and event["Status"] == "fail" and air["status"] == "invalidated"
    assert projectile["stopReason"] == "exception" and "镜头已切换" in event["Error"]
    assert len(polls) == projectile["rounds"] == 429
    assert len(completed) == air["queryAttempts"] == 428
    assert len(frames) == air["retainedFrames"] == 68
    assert len(zones) == air["retainedDecoded"] == air["decodedObservationCount"] == 66
    assert air["droppedFrames"] == air["queryAttempts"] - len(frames) == 360
    assert air["droppedDecoded"] == 0
    assert [x["ordinal"] for x in polls] == list(range(1, len(polls) + 1))
    assert all(f["match"] == projectile["initialMatch"] and f["epoch"] == projectile["initialEpoch"] for f in frames)
    starts = [f["pollStartedMilliseconds"] for f in frames]
    assert starts == sorted(starts)
    assert all(f["decodedCount"] == len(f["decoded"]) + f["droppedDecodedInThisPoll"] for f in frames)
    assert all(f["readFinishedMilliseconds"] >= f["pollStartedMilliseconds"] for f in frames)
    assert sum(x.get("airDecodedCount") or 0 for x in polls) == len(positive) == 66
    assert len({z["Key"] for z in zones}) == 1 and not zones[0]["Key"].startswith("flight:")
    assert all(all(math.isfinite(z[k]) for k in ("X", "Y", "Z", "DX", "DZ", "Offset", "HalfWidth", "Length", "Radius")) for z in zones)
    assert all(abs(math.hypot(z["DX"], z["DZ"]) - 1) < 1e-5 for z in zones)

    cast_path = root / "air" / "live-raw-07-p47-early.json"
    raw = json.loads(cast_path.read_text(encoding="utf8"))
    snapshot = raw["snapshot"]
    assert snapshot["stableEntries"] and snapshot["stableRootsAndDescriptors"] and all(snapshot["scanComplete"].values()) and not snapshot["errors"]
    cast = next(x for x in snapshot["timedAbilities"] if x.get("rawCastCandidate") and x.get("blueprint") == r"abilities\races\american\aircraft\strafing_run_p47_us")
    target = bytes.fromhex(cast["target"]["raw20Hex"])
    key = ":".join((str(int(cast["player"], 16)), str(int(cast["address"], 16)), str(cast["fields"]["instance68"]), cast["blueprint"], base64.b64encode(target[4:16]).decode("ascii")))
    assert key == zones[0]["Key"]
    position = struct.unpack_from("<3f", target, 4)
    direction = cast["fields"]["direction40"]
    norm = math.hypot(direction[0], direction[2])
    direction_xz = [direction[0] / norm, direction[2] / norm]
    assert all(all(abs(z[k] - v) < 1e-5 for k, v in zip(("X", "Y", "Z"), position)) and abs(z["DX"] - direction_xz[0]) < 1e-6 and abs(z["DZ"] - direction_xz[1]) < 1e-6 and z["Owner"] == int(cast["player"], 16) and z["Slot"] == cast["slot"] and z["HalfWidth"] == 5 and z["Length"] == 45 and z["Offset"] == 0 for z in zones)
    later_empty = [x for x in completed if x["ordinal"] > positive[-1]["ordinal"] and x["airDecodedCount"] == 0]
    assert later_empty
    aircraft = []
    for number in (8, 9):
        path = next((root / "air").glob(f"live-raw-{number:02d}-*.json"))
        stage = json.loads(path.read_text(encoding="utf8"))
        snap = stage["snapshot"]
        assert snap["stableEntries"] and snap["stableRootsAndDescriptors"] and all(snap["scanComplete"].values()) and not snap["errors"]
        for plane in snap["airEntities"]:
            if not plane.get("p47Name"):
                continue
            # Destroyed registry entries may retain body metadata without any actor.
            actor_data = plane.get("actor") or {}
            actor = bytes.fromhex(actor_data.get("rawHex", ""))
            body = bytes.fromhex(plane["raw392Hex"])
            xyz = plane["position44"]
            along = (xyz[0] - position[0]) * direction_xz[0] + (xyz[2] - position[2]) * direction_xz[1]
            aircraft.append({"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "timestampUtc": stage["timestampUtc"], "entity": plane["entity"], "generation": plane["generation"], "index": plane["registryIndex"], "position": xyz, "alongOriginalCastMetres": along, "actorPresent": bool(actor), "actor96_100": struct.unpack_from("<2f", actor, 96) if len(actor) >= 104 else None, "body32_36_40": struct.unpack_from("<3f", body, 32), "failedLegacyRecoveryGates": plane["failedRecoveryRawGates"]})
    return {"status": "PASS_historical_air_prefix_only_later_camera_failure", "windowAccepted": False, "gameFeatureAccepted": False, "readerReport": str(report_path), "readerReportSha256": hashlib.sha256(report_path.read_bytes()).hexdigest(), "readerReportedExitCode": 3, "osChildExitCode": "not_recorded_by_driver", "error": event["Error"], "scope": air["scope"], "match": projectile["initialMatch"], "epoch": projectile["initialEpoch"], "attemptedPolls": len(polls), "completedAirQueries": len(completed), "retainedDecoded": len(zones), "retainedFrames": len(frames), "droppedFrames": air["droppedFrames"], "droppedDecoded": 0, "firstPositive": positive[0], "lastPositive": positive[-1], "firstLaterEmpty": later_empty[0], "lastCompletedPoll": completed[-1], "failedPoll": polls[-1], "zone": zones[0], "zoneDistinctKeys": 1, "allRetainedZonesSameGeometry": all(z == zones[0] for z in zones), "rawCast": {"path": str(cast_path), "sha256": hashlib.sha256(cast_path.read_bytes()).hexdigest(), "instance": cast["fields"]["instance68"], "keyExactlyMatchesActualCSharp": True, "targetXYZ": position, "directionXZ": direction_xz}, "rawAircraft": aircraft, "limitation": "Prefix samples passed per-read guards before the later Snapshot camera failure. This validates historical retained outputs only, not the complete event window, recovery, enemy-only GUI or protected build."}


if __name__ == "__main__":
    result = validate()
    output = Path(__file__).with_name("run26-reader-prefix-validation.json")
    with output.open("x", encoding="utf8") as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2)
    print(json.dumps({"out": str(output), "status": result["status"], "retainedDecoded": result["retainedDecoded"], "zone": result["zone"], "rawAircraft": result["rawAircraft"]}, ensure_ascii=True, indent=2))
