"""Strict offline audit of retained run28; never attaches, builds, or sends input.

validate(root) accepts the version-adaptation directory and writes nothing.
The CLI creates its JSON exclusively; --check recomputes and compares, read only.
Pinned hashes bind this one run and the reviewed source, not a general acceptance.
"""
import argparse
from datetime import datetime, timedelta
import hashlib
import json
import math
from pathlib import Path, PureWindowsPath
import re
import struct


RUNNER_SHA = "CD125BFFA33E525CD4B94A753D65A6E4CBB199395E3860E57E85003DB7665F79"
GAME_SHA = "8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC"
MATCH = "31572:639308258698492330:2188107579392:2188149555200:1000"
PINNED = {
    "evidence/version-adaptation/maintenance-run-28-camera-recovery/maintenance-live.json": "F9E5E343BCBF1E22F2B7B36A87D8C2D1DFDFD752915661D5BE81DBA825101AA6",
    "evidence/version-adaptation/air/run28-driver-identity.json": "C3D56ECB0AF24FBD0088C304C4D7800BCE1B57A5AA8AE116AB9CDD7C9E403B5D",
    "evidence/version-adaptation/air/run28-driver-result.json": "A46079559E5AB81405139D8FEF2E672B8F906E574826F8835ED9CB184CC1058B",
    "evidence/version-adaptation/air/run28_camera_driver.ps1": "7E1D25B60BB82709C2CBE254C50A9CF89328692D050984899CFBB6EB9B002E85",
    "evidence/version-adaptation/air/run28-stdout.txt": "B8AE585EF577B859A8E8FCC4E4E117EA55EF509D403AF1460FEC8A3BC3C19987",
    "evidence/version-adaptation/air/run28-stderr.txt": "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855",
    "evidence/version-adaptation/game_input.py": "0C5C9D65535C0E9ED4122E5B4A433C38ABE6EB05D063DA5B0D5E29C7656A384F",
    "tests/maintenance-live/RecoveryProbe.cs": "2C7C5C18F20E39D8E8220858784C501FA6F66FCD5EEF45D8996E0E4CB6B36B12",
    "tests/maintenance-live/RecoverySampling.cs": "48C4C51451E3FCB90DA7DB4EE9E2E67F1605932475A2CBBC5365541C145F7289",
    "tests/maintenance-live/EventSampling.cs": "00D4E288A40CAFE9E525595BC38FB4793F38AA4E8B639B158E838008B5F84792",
    "tests/maintenance-live/LiveSampling.cs": "02221D71D6B00D9FC0D421FB11A4EFF60635E3A05D774CA2D6EFA7025ED42B4D",
    "tests/maintenance-live/EventPrewarm.cs": "0A4B7467836001CF1B38FAA30D39D9B86DCD73C85AF2733463EB4D58AE19A3A4",
    "tests/maintenance-live/EventPolling.cs": "59E996EAAD6BFEDE1E464DDD25613C83EEF13A85B8D61C54849C98D267DDDD71",
    "tests/maintenance-live/BoundedPoll.cs": "49344C6F36F769EBB8DE13499DC41B956755A7FBD591FA32D7B58FD586AB3EAE",
    "tests/maintenance-live/Program.cs": "933C837887771D5AD73594DAB8DA01A9ECA44ECEE8CD5DD841789E884B4BDB25",
    "client/CoH3MemoryTool/PlayerInfoReader.cs": "BBB359CD59409065D8B4D00ACE784243AE5E99816611BC11617E7C9ED8ADBA9D",
    "tests/maintenance-live/bin/Release/net48/SkyEye.Maintenance.Live.exe": RUNNER_SHA,
    "evidence/version-adaptation/air/run28-native-resume.png": "CD7F3945D27FC5AAC8ACBAEB879B1291AC0894C35ECA1F4BC409C19832510B26",
    "evidence/version-adaptation/air/run28-native-pan-01.png": "2CA6D6E5B5DCB3DB015DDD08A137329B855896ED451112571310BA785AAD00A0",
    "evidence/version-adaptation/air/run28-native-pan-02.png": "354CE3E75D5D4C2DAF2C25297D2051CEF400996A56B13F2B29CFF85D1E4DA402",
    "evidence/version-adaptation/air/run28-native-pan-03.png": "00BF54C03DBD4265F53D3C005B2D8347E6334F8EA4FAEAA2DC697BFD28F6D5A5",
    "evidence/version-adaptation/air/run28-native-pan-04.png": "DA1590BAE530BE44DC1EAD7C3403195FD54A3F6AD386A4746004C350B22DF099",
    "evidence/version-adaptation/air/run28-native-pan-05.png": "CE747B4A861BF571B388260398F96F51D3B87BF0BDB75BBD18C36221C051530E",
    "evidence/version-adaptation/air/run28-native-pan-06.png": "E1DF813C678924BC6FB116108ACB526788ADFDC25533CCEE88B55FDA09AD0C61",
    "evidence/version-adaptation/air/run28-native-pan-07.png": "6D4C13A602FEAED0A7952F93948C3FA0A541030AE51F1EEB212D6FB3AC5451A9",
    "evidence/version-adaptation/air/run28-native-pan-08.png": "D56391FDB67F381879AF8A9578C85DF8B8BC31B7452D7B56524C80FC41B42096",
    "evidence/version-adaptation/air/run28-native-paused.png": "E7097551F86E8A26D28B7CFEE55A207A1414B8DD7E32F65D883C6766AF76F3AB",
}


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def utc_ticks(value):
    """Preserve .NET's seven fractional digits; all recorded wall times are UTC."""
    match = re.fullmatch(r"(\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d)\.(\d{7})Z", value)
    if not match:
        raise ValueError("Expected UTC timestamp with seven fractional digits: " + str(value))
    base = datetime.fromisoformat(match[1]) - datetime(1, 1, 1)
    return (base.days * 86400 + base.seconds) * 10000000 + int(match[2])


def validate(root):
    root = Path(root).resolve()
    project = root.parents[1]
    checks, manifest, data = [], [], {}

    def need(condition, label):
        if not condition:
            raise ValueError("run28 validation failed: " + label)
        checks.append(label)

    for name, expected in PINNED.items():
        path = project / name
        blob = path.read_bytes()
        actual = hashlib.sha256(blob).hexdigest().upper()
        need(actual == expected, "sha256:" + name)
        data[name] = blob
        manifest.append({"path": str(path), "bytes": len(blob), "sha256": actual})

    def load(name):
        return json.loads(data["evidence/version-adaptation/" + name].decode("utf-8-sig"))

    def source(name, *fragments):
        text = " ".join(data[name].decode("utf-8-sig").split())
        need(all(" ".join(part.split()) in text for part in fragments), "source:" + name)
        return text

    reader_source = source("client/CoH3MemoryTool/PlayerInfoReader.cs",
        'if (Q(cameraCore + 416) != num6) { throw new Exception("镜头已切换。"); }',
        'catch { cameraCore = 0L; throw; }',
        'if (cameraCore == 0 || Q(cameraCore) != image + BuildRva45(64684536))',
        'if (process.HasExited || process.StartTime != started)', 'long num = LocalIdentity58();',
        'LocalIdentity58() != num || Q(num11 + 8) != num',
        'string text = process.Id + ":" + started.Ticks + ":" + num5 + ":" + num11 + ":" + num2;',
        'MatchKey = text;', 'BindLocalAccount59(list, num);', 'started = this.process.StartTime;')
    source("tests/maintenance-live/RecoveryProbe.cs",
        'internal const int MaximumAttempts = 3;', 'internal const int BudgetMilliseconds = 2000;',
        'attempt.Camera = Read(readCamera, true);', 'if (attempt.Camera.Status == "observed")',
        'attempt.Snapshot = Read(readSnapshot, false);', 'evidence.CameraAndSnapshotRecovered = true; break;',
        'RuntimeHelpers.GetHashCode(reader).ToString("x8")',
        'Valid = trigger.Reader != null && ReferenceEquals(current.Reader, trigger.Reader) && trigger.ReaderOpenAttempts == 1 && current.ReaderOpenAttempts == 1 && !string.IsNullOrEmpty(trigger.Match) && current.Match == trigger.Match && current.Allowed && current.Epoch == trigger.Epoch',
        'Before = Guard(context(), trigger)', 'read.After = Guard(context(), trigger);',
        'evidence.EndContext = Guard(context(), trigger);')
    source("tests/maintenance-live/RecoverySampling.cs", 'var camera = reader.ReadMineCamera();',
        'snapshotAttempts++; var rows = reader.Snapshot();', 'rows.Single(row => row.Relation == "自己")',
        'Reader = reader, ReaderOpenAttempts = readerOpenAttempts, Match = reader?.MatchKey,',
        'Epoch = LicenseGate60.Epoch, Allowed = LicenseGate60.Allowed',
        'if (!reader.IsCurrentMatch55())', 'Console.Out.Flush();')
    source("tests/maintenance-live/EventSampling.cs",
        'snapshotAttempts++; eventSnapshots++; var players = reader.Snapshot();',
        'if (recoveryProbe && RecoveryProbe.IsCameraFailure(exception)) recoveryTrigger = new RecoveryProbeTrigger',
        'Reader = reader, ReaderOpenAttempts = readerOpenAttempts, Match = match, Epoch = epoch,',
        'Error = exception.ToString() }; throw;',
        'status = stopReason == "exception" ? "invalidated"')
    live_source = source("tests/maintenance-live/LiveSampling.cs",
        'readerOpenAttempts++; try { reader = PlayerInfoReader.OpenForMaintenanceDiagnostics(); }',
        'snapshotAttempts++; rows = reader.Snapshot();',
        'SampleRecovery(reader, recoveryTrigger);', 'finally { reader?.Dispose(); }')
    need(live_source.count("OpenForMaintenanceDiagnostics()") == 1 and
         live_source.index("SampleRecovery(reader, recoveryTrigger)") < live_source.index("else ProbeMaps(reader,"),
         "source:one reader allocation and recovery before maps")
    source("tests/maintenance-live/BoundedPoll.cs", 'if (result.Status == "fail" || result.Status == "blocked") return 3;')
    source("tests/maintenance-live/Program.cs", 'ExitCode = BoundedPoll.ExitCodeFor(results);',
        'maintenanceOnly = true, buildVerifiedByThisRun = false, visualVerification = "not_run"')
    source("evidence/version-adaptation/air/run28_camera_driver.ps1",
        "if (Test-Path -LiteralPath $path) { throw", "$retainedProcessHandle = $child.Handle",
        "'event_prewarm_delay started milliseconds=1000'", "--pid 31572 --left-click",
        "$record.childExitCode = $child.ExitCode", "$record.runnerSha256After = (Get-FileHash")
    source("evidence/version-adaptation/game_input.py", "if u.GetForegroundWindow()!=h:raise RuntimeError",
        "u.ClientToScreen(h,c.byref(point))", "u.mouse_event(down,0,0,0,0)")

    report = load("maintenance-run-28-camera-recovery/maintenance-live.json")
    identity, driver = load("air/run28-driver-identity.json"), load("air/run28-driver-result.json")
    features = {item["Feature"]: item for item in report["results"]}
    need(len(features) == len(report["results"]), "unique feature names")
    event, recovery = features["event_window"], features["same_reader_recovery"]
    projectile, air, recovered = event["Evidence"]["projectile"], event["Evidence"]["air"], recovery["Evidence"]
    polls = projectile["polls"]
    need(report["maintenanceOnly"] is True and report["buildVerifiedByThisRun"] is False and
         report["visualVerification"] == "not_run", "maintenance evidence only")
    need(report["licenseState"] == "active" and features["authorization"]["Status"] == "pass" and
         report["readerOpenAttempts"] == recovered["ReaderOpenAttemptsAtTrigger"] == 1, "active lease and one reader open")
    need({item["BuildStatus"] for item in report["results"] if item["BuildStatus"] is not None} ==
         {"FingerprintMismatch"}, "candidate fingerprint status retained")
    need(identity["gamePid"] == 31572 and identity["gameStartUtc"] == "2026-11-20T18:44:29.8492330Z" and
         identity["gameSha256"] == GAME_SHA and driver["runnerPid"] == 14352,
         "recorded process identities and game hash")
    need(identity["runnerSha256"] == driver["runnerSha256After"] == RUNNER_SHA,
         "driver before/after/current runner hashes agree")
    args = identity["arguments"]
    need(args[0] == "--out" and PureWindowsPath(args[1]).name == "maintenance-run-28-camera-recovery" and
         args[2:] == ["--wait-seconds", "120", "--event-seconds", "60", "--air-events", "--recovery-probe",
                     "--event-prewarm-delay-ms", "1000"] and report["eventWindowSeconds"] == 60 and
         report["recoveryProbeRequested"] is True and report["eventPrewarmDelayMilliseconds"] == 1000,
         "explicit recovery and event flags")
    need(driver["status"] == "completed_driver_only_consult_reader_report" and driver["error"] is None and
         driver["childExitCodeRecorded"] is True and driver["childKilledAtTimeout"] is False and
         driver["pauseExitCode"] == 0 and driver["childExitCode"] == report["exitCode"] == 3,
         "OS exit3 retained without forced timeout")
    need(event["Status"] == "fail" and projectile["stopReason"] == "exception" and air["status"] == "invalidated" and
         "镜头已切换。" in event["Error"] and event["Error"].startswith(recovered["TriggerError"]) and
         all(name in recovered["TriggerError"] for name in ("ReadMineCamera()", "LocalIdentity58()", "Snapshot()")),
         "natural camera exception stack and original failure preserved")
    need(recovery["Status"] == recovered["Status"] == "observed" and recovery["Error"] is None and
         recovered["CameraAndSnapshotRecovered"] is True and recovered["StopReason"] == "camera_and_snapshot_observed" and
         recovery["SampleCount"] == 1, "independent successful recovery result")
    need(projectile["initialMatch"] == recovered["InitialMatch"] == MATCH and
         projectile["initialEpoch"] == recovered["InitialEpoch"] == 1, "original event match and epoch retained")
    need(projectile["requestedSeconds"] == 60 and projectile["budgetMilliseconds"] == 60000 and
         projectile["targetCadenceMilliseconds"] == 100 and projectile["maximumRounds"] == 601 and
         projectile["elapsedMilliseconds"] == 10586 and projectile["overrunMilliseconds"] == 0,
         "event stopped on exception before requested deadline")
    need(len(polls) == projectile["rounds"] == projectile["eventSnapshotAttempts"] == 98 and
         projectile["projectileQueryAttempts"] == air["queryAttempts"] == 97 and
         [poll["ordinal"] for poll in polls] == list(range(1, 99)), "98 attempts and 97 completed query rounds")
    need(all(poll["status"] == "completed" and type(poll["decodedCount"]) is int and
             poll["decodedCount"] >= 0 and poll["airDecodedCount"] == 0 for poll in polls[:-1]) and
         polls[-1]["status"] == "incomplete" and polls[-1]["decodedCount"] is None and
         polls[-1]["airDecodedCount"] is None, "terminal incomplete snapshot has no query result")
    need(all(0 <= poll["startedMilliseconds"] <= poll["finishedMilliseconds"] <= projectile["elapsedMilliseconds"]
             for poll in polls) and all(left["finishedMilliseconds"] <= right["startedMilliseconds"] and
             utc_ticks(left["utc"]) <= utc_ticks(right["utc"]) for left, right in zip(polls, polls[1:])),
         "every poll has ordered UTC and monotonic bounds")
    drift = max(abs((utc_ticks(poll["utc"]) - utc_ticks(polls[0]["utc"])) / 10000 -
                    (poll["startedMilliseconds"] - polls[0]["startedMilliseconds"])) for poll in polls)
    need(drift < 5, "UTC/stopwatch ledger relative drift under5ms")
    need(sum(poll["decodedCount"] for poll in polls[:-1]) == projectile["decodedObservationCount"] == 23 and
         sum(poll["decodedCount"] > 0 for poll in polls[:-1]) == projectile["nonzeroPolls"] == 23 and
         event["SampleCount"] == 23, "projectile counts are retained historical observations")
    frames = projectile["frames"]
    need(len(frames) == projectile["retainedFrames"] == 23 and projectile["retainedDecoded"] == 23 and
         projectile["droppedFrames"] == projectile["droppedDecoded"] == 0 and
         sum(len(frame["decoded"]) for frame in frames) == 23, "projectile frame and drop ledger")
    need(air["scope"] == "all_current_snapshot_rows_diagnostic_not_production_enemy_UI" and
         air["nonzeroPolls"] == air["decodedObservationCount"] == air["retainedDecoded"] == air["droppedDecoded"] == 0 and
         len(air["frames"]) == air["retainedFrames"] == 2 and air["droppedFrames"] == 95 and
         air["retainedFrames"] + air["droppedFrames"] == air["queryAttempts"], "97 historical empty air reads remain invalidated")
    for stream, stream_frames in (("projectile", frames), ("air", air["frames"])):
        need(all(frame["match"] == MATCH and frame["epoch"] == 1 and
                 frame["decodedCount"] == len(frame["decoded"]) and frame["droppedDecodedInThisPoll"] == 0 and
                 any(poll["utc"] == frame["utc"] and poll["startedMilliseconds"] == frame["pollStartedMilliseconds"] and
                     poll["finishedMilliseconds"] >= frame["readFinishedMilliseconds"] >= poll["startedMilliseconds"]
                     and poll["decodedCount" if stream == "projectile" else "airDecodedCount"] == frame["decodedCount"]
                     for poll in polls[:-1]) for frame in stream_frames), stream + " frames bind completed ledger rounds")
    need(air["frames"][0]["utc"] == polls[0]["utc"] and air["frames"][-1]["utc"] == polls[-2]["utc"],
         "air retains first/latest successful empty reads")

    need(recovered["MaximumAttempts"] == 3 and recovered["BudgetMilliseconds"] == 2000 and
         recovered["DirectCameraAttempts"] == recovered["SnapshotAttempts"] == len(recovered["Attempts"]) == 1 and
         recovered["ElapsedMilliseconds"] == recovery["ElapsedMilliseconds"] == 6 and recovered["OverrunMilliseconds"] == 0,
         "one successful pair within cooperative caps")
    attempt = recovered["Attempts"][0]
    camera, snapshot = attempt["Camera"], attempt["Snapshot"]
    need(attempt["Ordinal"] == 1 and all(read["Called"] is True and read["Status"] == "observed" and
         read["Error"] is None for read in (camera, snapshot)), "direct camera and snapshot both actually called")
    guards = {"start": recovered["StartContext"], "cameraBefore": camera["Before"], "cameraAfter": camera["After"],
              "snapshotBefore": snapshot["Before"], "snapshotAfter": snapshot["After"], "end": recovered["EndContext"]}
    expected_guard = {"ReaderIdentity": "020266e5", "SameReader": True, "ReaderOpenAttempts": 1,
                      "Match": MATCH, "Epoch": 1, "Allowed": True, "Valid": True}
    need(recovered["TriggerReaderIdentity"] == "020266e5" and
         all(guard == expected_guard for guard in guards.values()), "six original-reference match epoch lease guards")
    need(0 <= camera["StartedMilliseconds"] <= camera["FinishedMilliseconds"] <= snapshot["StartedMilliseconds"] <=
         snapshot["FinishedMilliseconds"] <= recovered["ElapsedMilliseconds"], "recovery monotonic sequence")
    need(utc_ticks(recovered["TriggeredUtc"]) <= utc_ticks(recovery["StartedUtc"]) <= utc_ticks(camera["StartedUtc"]) <=
         utc_ticks(camera["FinishedUtc"]) <= utc_ticks(snapshot["StartedUtc"]) <= utc_ticks(snapshot["FinishedUtc"]),
         "exception precedes direct camera then snapshot UTC")
    matrices = camera["Evidence"]
    need(all(len(matrices[name]) == 16 and all(type(value) in (float, int) and math.isfinite(value)
         for value in matrices[name]) for name in ("View", "Projection")) and
         matrices["convention"] == "16 floats per matrix, raw reader ordering", "two finite16-float camera matrices")
    view, projection = matrices["View"], matrices["Projection"]
    need(abs(view[15] - 1) <= 0.01 and projection[0] > 0 and projection[5] > 0 and
         abs(projection[11] - 1) <= 0.01, "camera engine format invariants")
    local = snapshot["Evidence"]["local"]
    initial_local = [row for row in features["snapshot/resources"]["Evidence"] if row["Relation"] == "自己"]
    parts = [int(value) for value in MATCH.split(":")]
    need(len(initial_local) == 1 and snapshot["Evidence"]["rowCount"] == features["snapshot/resources"]["SampleCount"] == 2 and
         all(local[key] == initial_local[0][key] for key in ("Slot", "Relation", "IsLocalPlayer", "identity")) and
         local["IsLocalPlayer"] is True and int(local["playerAddress"], 16) == parts[3] and
         int(local["identity"], 16) & 0xffffffff == parts[4] and int(local["identity"], 16) >> 32 == local["Slot"] == 0,
         "snapshot local identity address and original match correspond")
    need(parts[0] == identity["gamePid"] and parts[1] == utc_ticks(identity["gameStartUtc"]) + 8 * 3600 * 10000000,
         "match process start ticks agree with UTCplus8 recorded start")
    need(report["snapshotAttempts"] == 1 + projectile["eventSnapshotAttempts"] + recovered["SnapshotAttempts"] == 100,
         "snapshot accounting:initial1 plus event98 plus recovery1 equals100")

    prewarm = features["event_prewarm_delay"]["Evidence"]
    need(features["event_prewarm_delay"]["Status"] == "observed" and prewarm["requestedMilliseconds"] ==
         prewarm["elapsedMilliseconds"] == 1000 and prewarm["outcome"] == "elapsed" and
         prewarm["initialMatch"] == MATCH and prewarm["initialEpoch"] == 1 and prewarm["liveMatchChecks"] == 2,
         "prewarm completed with original context and two live checks")
    timeline = [identity["startedUtc"], features["snapshot/resources"]["StartedUtc"], report["snapshotCapturedUtc"],
                prewarm["startedUtc"], driver["resumedUtc"], prewarm["finishedUtc"], event["StartedUtc"], polls[0]["utc"],
                polls[-1]["utc"], recovered["TriggeredUtc"], recovery["StartedUtc"], camera["StartedUtc"],
                camera["FinishedUtc"], snapshot["StartedUtc"], snapshot["FinishedUtc"], report["utc"],
                driver["pausedUtc"], driver["finishedUtc"]]
    need(all(utc_ticks(left) <= utc_ticks(right) for left, right in zip(timeline, timeline[1:])), "complete recorded UTC sequence")
    maps = {name: {key: features[name][key] for key in ("Status", "StartedUtc", "ElapsedMilliseconds")}
            for name in ("minimap", "tactical")}
    need(all(utc_ticks(item["StartedUtc"]) >= utc_ticks(snapshot["FinishedUtc"]) for item in maps.values()),
         "maps start after direct recovery and cannot supply its evidence")
    pans = driver["nativePans"]
    points = [(140, 900), (260, 930), (220, 800), (90, 820)] * 2
    need(len(pans) == 8 and [pan["ordinal"] for pan in pans] == list(range(1, 9)) and
         [(pan["clientX"], pan["clientY"]) for pan in pans] == points and
         all(PureWindowsPath(pan["capture"]).name == "run28-native-pan-%02d.png" % pan["ordinal"] for pan in pans),
         "eight native pans match driver coordinates and capture names")
    need(utc_ticks(driver["resumedUtc"]) < utc_ticks(pans[0]["finishedUtc"]) and
         all(utc_ticks(left["finishedUtc"]) < utc_ticks(right["finishedUtc"]) for left, right in zip(pans, pans[1:])) and
         utc_ticks(pans[-1]["finishedUtc"]) < utc_ticks(report["utc"]) < utc_ticks(driver["pausedUtc"]),
         "pan times precede final report and normal pause")
    need(utc_ticks(pans[5]["finishedUtc"]) < utc_ticks(recovered["TriggeredUtc"]) < utc_ticks(pans[6]["finishedUtc"]),
         "natural exception occurred between sixth/seventh capture completion")
    for name, blob in data.items():
        if name.endswith(".png"):
            need(blob[:8] == b"\x89PNG\r\n\x1a\n" and struct.unpack(">II", blob[16:24]) == (1936, 1119), "png dimensions:" + name)
    stdout = data["evidence/version-adaptation/air/run28-stdout.txt"].decode("gb18030")
    need(all(marker in stdout for marker in ("license_state=active", "event_window stop=exception polls=98 decodedObservations=23",
         "event_window: fail", "镜头已切换。", "same_reader_recovery: observed cameraAttempts=1 snapshotAttempts=1 stop=camera_and_snapshot_observed")) and
         not data["evidence/version-adaptation/air/run28-stderr.txt"], "retained console agrees and stderr empty")
    need(any(item["Status"] in ("fail", "blocked") for item in report["results"]), "exit3 remains required after recovery")

    return {
        "status": "pass", "offlineOnly": True, "checkCount": len(checks), "checks": checks,
        "validatorSha256": digest(Path(__file__)), "inputs": manifest,
        "readerRecoveryAccepted": True, "originalEventWindowAccepted": False,
        "productionGuiRecoveryAccepted": False, "gameFeatureAccepted": False, "protectedDeliveryAccepted": False,
        "gamePid": identity["gamePid"], "gameStartUtc": identity["gameStartUtc"], "recordedGameSha256": GAME_SHA,
        "runnerPid": driver["runnerPid"], "runnerSha256": RUNNER_SHA, "match": MATCH, "epoch": 1,
        "readerIdentityLabel": recovered["TriggerReaderIdentity"], "referenceEvidence": "ReferenceEquals source guard plus six recorded Valid/SameReader contexts; hash label alone is not identity proof",
        "readerOpenAttempts": 1, "snapshotAccounting": {"initial": 1, "eventAttemptsIncludingFailure": 98, "recovery": 1, "total": 100},
        "originalWindow": {"status": event["Status"], "error": event["Error"], "airStatus": air["status"], "readerAndOsExitCode": 3,
            "requestedSeconds": 60, "elapsedMilliseconds": projectile["elapsedMilliseconds"], "polls": len(polls),
            "completedQueryRounds": 97, "finalPoll": polls[-1], "decodedHistoricalProjectiles": 23,
            "historicalEmptyAirQueries": 97, "maximumLedgerUtcStopwatchDriftMilliseconds": round(drift, 7)},
        "recovery": {"probeStartedUtc": recovery["StartedUtc"], **recovered,
            "triggerToFirstCameraMilliseconds": (utc_ticks(camera["StartedUtc"]) - utc_ticks(recovered["TriggeredUtc"])) / 10000,
            "triggerToSnapshotFinishedMilliseconds": (utc_ticks(snapshot["FinishedUtc"]) - utc_ticks(recovered["TriggeredUtc"])) / 10000,
            "recordedCameraWallMilliseconds": (utc_ticks(camera["FinishedUtc"]) - utc_ticks(camera["StartedUtc"])) / 10000,
            "recordedSnapshotWallMilliseconds": (utc_ticks(snapshot["FinishedUtc"]) - utc_ticks(snapshot["StartedUtc"])) / 10000},
        "prewarm": prewarm, "mapsAfterRecovery": maps, "nativePans": pans,
        "nativeAdvanceSeconds": (utc_ticks(driver["pausedUtc"]) - utc_ticks(driver["resumedUtc"])) / 10000000,
        "limitations": [
            "Only one naturally occurring exception and one recovered direct camera/Snapshot pair in a maintenance runner; no production GUI retry was exercised.",
            "The event window failed at about10.586s of the requested60s; air invalidated and reader/OS exit3 remain unchanged.",
            "MatchKey guards are cached equality checks. Successful Snapshot adds live process/world/local-identity checks; no raw camera-pointer before/after dump was retained.",
            "RuntimeHelpers identity hash is only a label. ReferenceEquals in the reviewed source implements the recorded SameReader/Valid guards.",
            "Snapshot and prewarm perform additional internal camera reads; DirectCameraAttempts1 is not the total camera-call count. Maps are separate later probes.",
            "Eight retained pan captures support the native driver sequence; their completion timestamps do not identify which exact click caused the pointer change.",
            "Driver-captured game binary hash is retained, not re-read from the running game. Runner/source/capture hashes are rechecked offline; this is not a new build or binary-to-source attestation.",
            "A zero recorded direct-camera duration reflects timer granularity, not proof of zero execution time. No build/fingerprint, other-feature or protected-delivery acceptance is granted."
        ]
    }


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--check", action="store_true", help="Read-only rerun and compare the existing validation JSON")
    options = parser.parse_args()
    output = Path(__file__).with_name("run28-recovery-validation.json")
    if not options.check and output.exists():
        raise FileExistsError("Exclusive evidence path already exists: " + str(output))
    result = validate(options.root)
    if options.check:
        if json.loads(output.read_text(encoding="utf-8")) != result:
            raise ValueError("Existing validation JSON differs from the recomputed result")
    else:
        with output.open("x", encoding="utf-8", newline="\n") as stream:
            json.dump(result, stream, ensure_ascii=False, indent=2, allow_nan=False)
            stream.write("\n")
    print(json.dumps({"status": result["status"], "checks": result["checkCount"], "mode": "check" if options.check else "create",
        "output": str(output), "readerRecoveryAccepted": result["readerRecoveryAccepted"], "originalEventWindowAccepted": False,
        "productionGuiRecoveryAccepted": False}, ensure_ascii=True))
