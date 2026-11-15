"""Verify frozen Fog-window evidence and a published preview without game access."""
from pathlib import Path
from datetime import datetime
import argparse
import hashlib
import json
import re
import xml.etree.ElementTree as ET
import zipfile
import pefile


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def load(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release", type=Path)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    evidence = root / "artifacts/fog-window-integration-20261112"
    if args.output.exists():
        raise FileExistsError(args.output)
    release = load(args.release)
    exe, archive = Path(release["Exe"]), Path(release["Zip"])
    checks = {}

    def check(name, value):
        checks[name] = bool(value)
        if not value:
            raise AssertionError(name)

    check("release_file_version", release["FileVersion"] == "0.5.2.0")
    check("published_exe_digest", sha(exe) == release["ExeSha256"].lower())
    check("published_zip_digest", sha(archive) == release["ZipSha256"].lower())
    image = pefile.PE(str(exe), fast_load=True)
    check("exe_is_x64", image.FILE_HEADER.Machine == 0x8664)
    image.close()
    check("published_selftest_flag", release["PublishedExeSyntheticSelftestPassed"] is True and release["GameAccessedBySelftest"] is False)
    lines = (exe.parent / "selftest.stdout.txt").read_text(encoding="utf-8-sig").splitlines()
    check("fourteen_published_pass_lines", len(lines) == 14 and all(line.startswith("PASS") for line in lines))
    check("published_stderr_empty", not (exe.parent / "selftest.stderr.txt").read_text(encoding="utf-8-sig").strip())
    check("optional_feature_flags", release["ExperimentalCpuFogWindowImplemented"] is True and release["ExperimentalCpuFogDefaultEnabled"] is False and release["ExperimentalCpuFogModulationAssumption"] == "1" and release["ExperimentalCpuFogTemporalHistoryImplemented"] is False and release["FogRawFixtureExportImplemented"] is True)
    pending = ["FogActiveUiCompositionVerified", "TemporalBlendExact", "ContinuousMovementSyncMeasured", "SameSceneVisualComparisonPassed", "LevelBAccepted"]
    check("five_acceptance_flags_stay_false", all(release[k] is False for k in pending))
    with zipfile.ZipFile(archive) as package:
        names = package.namelist()
        check("archive_has_unique_entries", len(names) == len(set(names)))
        check("zip_exe_equals_published", hashlib.sha256(package.read("CoH3Minimap.exe")).hexdigest() == sha(exe) == release["ZipExeSha256"].lower())
        check("zip_readme_equals_current", package.read("README.md") == (root / "README.md").read_bytes())
        check("zip_notices_equal_current", package.read("THIRD-PARTY-NOTICES.md") == (root / "THIRD-PARTY-NOTICES.md").read_bytes())
        licenses = sorted((root / "licenses").glob("*"))
        check("all_license_files_packaged", bool(licenses) and all(package.read("licenses/" + p.name) == p.read_bytes() for p in licenses if p.is_file()))
    project = ET.parse(root / "CoH3Minimap.csproj").getroot()
    check("project_version_and_fixed_rid", project.findtext(".//Version") == "0.5.2-native-preview" and project.findtext(".//RuntimeIdentifier") == "win-x64")
    lock = load(root / "packages.lock.json")
    before = load(evidence / "lock-recovery/packages-lock-before.json")
    framework = "net9.0-windows7.0"
    check("lock_rid_and_packages_preserved", framework + "/win-x64" in lock["dependencies"] and lock["dependencies"][framework] == before["dependencies"][framework])
    lock_proof = load(evidence / "lock-recovery/restore-verification.json")
    check("lock_recovery_current_digest", lock_proof["Passed"] is True and lock_proof["ExistingDependencyGroupUnchanged"] is True and sha(root / "packages.lock.json") == lock_proof["AfterSha256"].lower())
    live_root = evidence / "live-review"
    live = load(live_root / "live-verification.json")
    check("live_32_checks_pass", live["Passed"] is True and live["CheckCount"] == 32 and len(live["Checks"]) == 32 and all(v is True for v in live["Checks"].values()))
    check("live_artifact_digests", all(sha(live_root / name) == digest for name, digest in live["Artifacts"].items()) and sha(live_root / "verify_live.py") == live["VerifierSha256"])
    check("three_window_images_frozen", len(live["WindowImages"]) == 3 and all(sha(Path(v["Path"])) == v["Sha256"] for v in live["WindowImages"].values()))
    check("live_boundaries_remain_false", all(live[k] is False for k in ["ExactSameFrame", "ContinuousMovementSyncMeasured", "ActiveGpuReplicaVerified", "TemporalBlendExact", "SameSceneVisualComparisonPassed", "LevelBAccepted"]))
    offline_root = evidence / "offline-review"
    comparison_path = offline_root / "comparison.json"
    ui_path = offline_root / "ui-check-final.json"
    check("independent_result_files_frozen", sha(comparison_path) == "f64240edb19737aa85fac9c2876db8acea742932b2c6ba214f8780214a7ae568" and sha(ui_path) == "1cbd591515ab3980059d7c1b5bddcc5a103a7e55686ece65cfdddb743b47dc6e")
    comparison, ui = load(comparison_path), load(ui_path)
    check("three_actual_renderer_regressions", comparison["Passed"] is True and comparison["FixtureCount"] == 3 and comparison["IndependentAssemblyProcesses"] is True and len(comparison["Samples"]) == 3 and all(s["OldDefaultEqualsNewOffByteForByte"] is True and s["NewOnFogDrawn"] is True and s["MarkAndViewportResultsPreserved"] is True for s in comparison["Samples"]))
    check("independent_inputs_and_dlls_frozen", all(sha(path) == comparison[key] for path, key in [(root / "artifacts/fog-raw-export-dev-20261112/CoH3Minimap.dll", "OldAssemblySha256"), (root / "artifacts/fog-window-dev-20261112/CoH3Minimap.dll", "NewAssemblySha256"), (root / "artifacts/fog-cpu-live-20261112-154016/fog-raw.json", "RawFixtureSha256"), (root / "artifacts/fog-cpu-live-20261112-154016/terrain-source.png", "TerrainFileSha256")]))
    ui_check = ui["Check"]
    check("default_off_menu_roundtrip", ui_check["Passed"] is True and ui_check["InitialChecked"] is False and ui_check["OnInitiallyChecked"] is True and ui_check["BothWorkersStopped"] is True and ui_check["BothDisposed"] is True and ui_check["ShowCalled"] is False and ui_check["RealWindowInputSent"] is False and ui_check["DefaultTitle"] == ui_check["RestoredTitle"])
    check("optional_api_default_false", ui["Api"]["Parameters"][-1]["IsOptional"] is True and ui["Api"]["Parameters"][-1]["Default"] is False)
    report = evidence / "FOG_WINDOW_INTEGRATION.md"
    text = report.read_text(encoding="utf-8")
    links = re.findall(r"\[[^\]]+\]\(<?([^)>]+)>?\)", text)
    check("report_links_resolve", bool(links) and all((report.parent / link.split("#", 1)[0]).resolve().is_file() for link in links))
    readme = (root / "README.md").read_text(encoding="utf-8")
    check("readme_has_current_flags_and_report", "--experimental-cpu-fog" in readme and "--fog-bytes-output" in readme and "fog-window-integration-20261112/FOG_WINDOW_INTEGRATION.md" in readme)
    sources = ["NativeFrameRendering.cs", "NativeFrameRenderingTests.cs", "NativeMinimapForm.cs", "Program.cs", "NativeHudProbe.cs"]
    result = dict(VerifiedAt=datetime.now().astimezone().isoformat(), Passed=True, CheckCount=len(checks), Checks=checks, ReleaseManifest=str(args.release.resolve()), ReleaseManifestSha256=sha(args.release), ExeSha256=sha(exe), ZipSha256=sha(archive), ReportSha256=sha(report), ReadmeSha256=sha(root / "README.md"), SourceSha256={name: sha(root / "src" / name) for name in sources}, ReportLinks=len(links), GameAccessedDuringVerification=False, PriorTestsRerun=False, FullGoalAccepted=False, VerifierSha256=sha(Path(__file__)))
    with args.output.open("x", encoding="utf-8") as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
    print(json.dumps(dict(Passed=True, CheckCount=len(checks), ReportLinks=len(links), ExeSha256=result["ExeSha256"], ZipSha256=result["ZipSha256"], FullGoalAccepted=False), indent=2))


if __name__ == "__main__":
    main()
