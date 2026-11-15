"""Verify the new package, preserve the prior evidence, then remove exactly the two old dist files."""
from pathlib import Path, PurePosixPath
import hashlib
import json
import posixpath
import re
import zipfile

ROOT = Path(__file__).resolve().parents[1]
DIST = (ROOT / "dist").resolve(strict=True)
PUBLISH = ROOT / "artifacts" / "preview-20261112-041209"
NEW_REPORT = DIST / "validation-0.5.1-native-preview-20261112-041209.json"
NEW_ZIP = DIST / "CoH3Minimap-0.5.1-native-preview-20261112-041209.zip"
OLD_REPORT = DIST / "validation-0.5-native-preview-20261112-004209.json"
OLD_ZIP = DIST / "CoH3Minimap-0.5-native-preview-20261112-004209.zip"
SOURCE = ROOT.parent / "CoH3FowTool" / "tools" / "reverse" / "native_fow_20261111" / "native_match_state_20261112"
SCREENSHOT = ROOT / "artifacts" / "native-window-3532-20261112-041509-773236.png"
EXE_HASH = "9B89AF4D3D2DE3F0803A1E314E2F5221E7AAA932891C19C0D3899F630EC20DFB"
ZIP_HASH = "6F59A2E3845BDCDC37E03172460B9BE8F69B539A497B2DA6E3821D3AE1ED6836"
OLD_ZIP_HASH = "E9AB2A69E1606564D56D2424F9778734F3AB84C228EFFB09B296B92BE3BA8425"


def sha(data):
    return hashlib.sha256(data).hexdigest().upper()


report = json.loads(NEW_REPORT.read_text(encoding="utf-8-sig"))
assert report["FileVersion"] == "0.5.1.0"
assert Path(report["Exe"]).resolve(strict=True) == (PUBLISH / "CoH3Minimap.exe").resolve(strict=True)
assert Path(report["Zip"]).resolve(strict=True) == NEW_ZIP.resolve(strict=True)
assert report["NativePostMatchStateHintImplemented"] is True
pending = ("FogActiveUiCompositionVerified", "TemporalBlendExact", "ContinuousMovementSyncMeasured",
           "SameSceneVisualComparisonPassed", "LevelBAccepted")
assert all(report[key] is False for key in pending)
assert sha((PUBLISH / "CoH3Minimap.exe").read_bytes()) == report["ExeSha256"] == EXE_HASH
assert sha(NEW_ZIP.read_bytes()) == report["ZipSha256"] == ZIP_HASH
expected_entries = {"CoH3Minimap.exe", "README.md", "THIRD-PARTY-NOTICES.md"} | {
    item.relative_to(PUBLISH).as_posix() for item in (PUBLISH / "licenses").rglob("*") if item.is_file()}
with zipfile.ZipFile(NEW_ZIP) as archive:
    assert archive.testzip() is None
    entries = [info.filename for info in archive.infolist() if not info.is_dir()]
    assert set(entries) == expected_entries and len(entries) == len(expected_entries)
    for name in entries:
        logical = PurePosixPath(name)
        assert not logical.is_absolute() and ".." not in logical.parts and "\\" not in name
        assert archive.read(name) == (PUBLISH / name).read_bytes(), name
    for name in ("README.md", "THIRD-PARTY-NOTICES.md"):
        markdown = archive.read(name).decode("utf-8-sig")
        for target in re.findall(r"\[[^\]]+\]\(([^)]+)\)", markdown):
            target = target.strip("<>").split("#", 1)[0]
            if not target or re.match(r"^[a-zA-Z][a-zA-Z0-9+.-]*:", target):
                continue
            relative = posixpath.normpath(posixpath.join(str(PurePosixPath(name).parent), target))
            assert relative in entries or any(item.startswith(relative.rstrip("/") + "/") for item in entries), relative
assert (PUBLISH / "selftest.stderr.txt").stat().st_size == 0
passes = [line for line in (PUBLISH / "selftest.stdout.txt").read_text(encoding="utf-8-sig").splitlines()
          if line.startswith("PASS")]
assert len(passes) == 14
verification_path = SOURCE / "verification.json"
verification = json.loads(verification_path.read_text(encoding="utf-8-sig"))
assert verification["passed"] is True and len(verification["checks"]) == 17
assert all(value is True for value in verification["checks"].values())
assert (SOURCE / "MATCH_STATE_REPORT.md").is_file()
assert sha(SCREENSHOT.read_bytes()) == "FF641DD428C35F839CD9AD8D2D3036AB3021EDD69E153E8E361D6C6FA671EDF1"
old_data = OLD_REPORT.read_bytes()
assert sha(OLD_ZIP.read_bytes()) == OLD_ZIP_HASH
assert {item.name for item in DIST.iterdir() if item.is_file()} == {
    NEW_REPORT.name, NEW_ZIP.name, OLD_REPORT.name, OLD_ZIP.name}
history = ROOT / "artifacts" / "validation-history" / OLD_REPORT.name
history.parent.mkdir(parents=True, exist_ok=True)
if history.exists():
    assert history.read_bytes() == old_data
else:
    with history.open("xb") as stream:
        stream.write(old_data)
assert history.read_bytes() == old_data
report["ArchiveFileCountVerified"] = len(entries)
report["ZipCrcVerified"] = True
report["AllArchiveFilesMatchSources"] = True
report["PackagedLocalMarkdownLinksVerified"] = True
report["PublishedExeSyntheticPassLines"] = len(passes)
report["PublishedExeWindowProcessId"] = 3532
report["PublishedExeWindowVisuallyReviewed"] = True
report["PublishedExeWindowImage"] = str(SCREENSHOT)
report["PublishedExeWindowImageSha256"] = sha(SCREENSHOT.read_bytes())
report["NativePostMatchStateReview"] = {
    "Report": str(SOURCE / "MATCH_STATE_REPORT.md"), "Verification": str(verification_path),
    "VerificationSha256": sha(verification_path.read_bytes()), "ChecksPassed": len(verification["checks"]),
    "OriginalInstructionAssertionsPassed": 6, "ActualReaExports": 2, "SameRvaCodeRanges": 4,
    "DevelopmentBuildCachedUiSamples": 6, "RuntimeEvidence": verification["runtime_evidence"],
    "FinalPublishedWindowHintVisuallyVerified": True, "CurrentScene": "GameOverSpectatingMap (cached native UI)",
    "SimulationRunningInferred": False, "AtomicSnapshotClaimed": False,
    "MovementAcceptanceFromThisScene": False,
}
report["HistoricalPreviewEvidence"] = {
    "Report": str(history), "Sha256": sha(old_data), "Version": "0.5.0-native-preview",
    "Meaning": "Prior static-scene and research evidence; not dynamic acceptance of the new executable.",
}
NEW_REPORT.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
confirmed = json.loads(NEW_REPORT.read_text(encoding="utf-8"))
assert all(confirmed[key] is False for key in pending)
assert confirmed["NativePostMatchStateReview"]["ChecksPassed"] == 17
assert sha(Path(confirmed["HistoricalPreviewEvidence"]["Report"]).read_bytes()) == confirmed["HistoricalPreviewEvidence"]["Sha256"]
# Resolve and verify the two exact old targets before either deletion. No wildcard or recursive deletion.
old_targets = [(OLD_ZIP, OLD_ZIP_HASH), (OLD_REPORT, sha(old_data))]
for path, expected_hash in old_targets:
    resolved = path.resolve(strict=True)
    assert resolved.parent == DIST and resolved.name == path.name
    assert resolved not in (NEW_REPORT.resolve(strict=True), NEW_ZIP.resolve(strict=True))
    assert sha(resolved.read_bytes()) == expected_hash
for path, _ in old_targets:
    path.unlink()
assert {item.name for item in DIST.iterdir() if item.is_file()} == {NEW_REPORT.name, NEW_ZIP.name}
print(json.dumps({"passed": True, "version": report["FileVersion"], "selftest_pass_lines": len(passes),
                  "archive_files": len(entries), "match_state_checks": 17,
                  "pending_acceptance_flags_preserved": True,
                  "dist_files": sorted(item.name for item in DIST.iterdir() if item.is_file()),
                  "historical_report_preserved": str(history), "report": str(NEW_REPORT)}, indent=2))
