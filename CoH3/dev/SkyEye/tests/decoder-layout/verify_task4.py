"""Verify frozen task-4 source, original fixtures and complete REA CIL evidence offline."""
from pathlib import Path
import datetime
import difflib
import hashlib
import json
import re
import struct

HERE = Path(__file__).resolve().parent
ENGINE = HERE.parents[1] / "client" / "CoH3MemoryTool"
checks = []

def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def check(condition, name):
    if not condition:
        raise AssertionError(name)
    checks.append(name)

manifest = load(HERE / "source-manifest.json")
for group in ("linkedSourceHashes", "fixtureSourceHashes", "capturedAirInputs"):
    records = manifest[group]
    check(all(sha(Path(r["path"])) == r["sha256"].lower() for r in records), group + " still match successful fixture run")
check(sha(Path(manifest["capturedEvent"])) == manifest["capturedEventSha256"].lower(), "captured projectile input unchanged")

expected_core = {
    "AirLogic.cs": "8e3eced8cd1e9f120e76da38ba2d5146c0e56fcfe670ea66b4486fb2011dac50",
    "PlayerInfoReader.cs": "bbb359cd59409065d8b4d00ace784243ae5e99816611bc11617e7c9ed8adba9d",
}
for name, digest in expected_core.items():
    check(sha(ENGINE / name) == digest, name + " remains frozen after parent notification")

# Invert just this task's reviewed edits to prove every other guard and timer stayed intact.
reader = (ENGINE / "PlayerInfoReader.cs").read_text(encoding="utf-8-sig")
selector_start = reader.index("\tprivate AirMoverShape AirMoverShape45()")
selector_end = reader.index("\tprivate long BuildRva45(long legacy)", selector_start)
selector = reader[selector_start:selector_end]
check(selector == "\tprivate AirMoverShape AirMoverShape45()\n\t{\n#if MAINTENANCE_DIAGNOSTICS\n\t\tif (maintenanceDiagnostics) return AirMoverShape.Candidate52426_184;\n#endif\n\t\treturn AirMoverShape.Legacy284;\n\t}\n\n", "reader explicit shape selection requires maintenance compilation AND maintenanceDiagnostics=true")
restored = reader[:selector_start] + reader[selector_end:]
restored, count = re.subn(r"(?m)^(\t+)AirMoverShape moverShape = AirMoverShape45\(\);\n\1byte\[\] (array6|array) = Read\((Resolve\(arena, (?:num21|num)\)), AirLogic.MoverReadSize\(moverShape\)\);", r"\1byte[] \2 = Read(\3, 284);", restored)
check(count == 2, "both reader consumers use the explicit shape for bounded reads")
restored = restored.replace(", BuildRva45, moverShape);", ", BuildRva45);")
restored = restored.replace("\t\tAirLogic.MoverDirection(array, moverShape, out float headingX, out _, out float headingZ);\n", "")
restored = restored.replace("px, pz, headingX, headingZ, length, width", "px, pz, BitConverter.ToSingle(array, 272), BitConverter.ToSingle(array, 280), length, width")
check(restored == (HERE / "task4-baseline" / "PlayerInfoReader.cs.txt").read_text(encoding="utf-8-sig"), "inverting only shape/extent/direction edits restores reader baseline exactly; constructors, fingerprint, identity and timing guards unchanged")

logic = (ENGINE / "AirLogic.cs").read_text(encoding="utf-8-sig")
start = logic.index("internal enum AirMoverShape")
end = logic.index("\tpublic static bool Passed(", start)
restored = logic[:start] + "internal static class AirLogic\n{\n" + logic[end:]
restored = restored.replace(", AirMoverShape moverShape = AirMoverShape.Legacy284)", ")")
restored = restored.replace("actor.Length < MoverReadSize(moverShape)", "actor.Length < 284")
restored = restored.replace("\t\tMoverDirection(actor, moverShape, out float num4, out float num5, out float num6);", "\t\tfloat num4 = BitConverter.ToSingle(actor, 272);\n\t\tfloat num5 = BitConverter.ToSingle(actor, 276);\n\t\tfloat num6 = BitConverter.ToSingle(actor, 280);")
check(restored == (HERE / "task4-baseline" / "AirLogic.cs.txt").read_text(encoding="utf-8-sig"), "inverting shape helper/signature edits restores AirLogic baseline exactly; all other decoder guards unchanged")

reviewed_modes = []
for mode, original_count, current_count in (("Release", 89, 118), ("Maintenance", 97, 170)):
    fixture = load(HERE / "bin" / mode / "net10.0" / "fixture-report.json")
    baseline = load(HERE / "task4-baseline" / f"{mode.lower()}-fixture-report.json")
    check(fixture["legacyAndMapperPassed"] == baseline["passed"] == original_count and fixture["checks"][:original_count] == baseline["checks"], mode + " original assertion names, order and count preserved")
    check(fixture["passed"] == len(fixture["checks"]) == current_count, mode + " all recorded fixture checks passed")
    artifact = load(HERE / f"rea-{mode.lower()}-artifact.json")
    evidence = load(HERE / f"rea-{mode.lower()}-members.json")
    members = evidence["normalized_result"]
    dll = HERE / "bin" / mode / "net10.0" / "DecoderLayout.Fixture.dll"
    digest = sha(dll)
    check(artifact["subject"]["digest"]["sha256"] == evidence["subject"]["digest"]["sha256"] == members["artifact"]["sha256"] == digest, mode + " REA evidence belongs to the currently tested DLL")
    check(artifact["normalized_result"]["coverage"] == {"state": "complete", "issues": []} and members["coverage"] == {"state": "complete", "issues": []} and not members["selection"]["unmatched_type_names"], mode + " REA metadata coverage complete with all three selected types")
    mover_fields = [f["name"] for f in members["fields"] if f["declaring_type"] == "CoH3MemoryTool.AirMoverShape"]
    expected_fields = ["value__", "Legacy284"] + (["Candidate52426_184"] if mode == "Maintenance" else [])
    check(mover_fields == expected_fields, mode + " candidate shape member compile isolation verified in actual metadata")
    layout_fields = [f["name"] for f in members["fields"] if f["declaring_type"] == "CoH3MemoryTool.GameLayout52426"]
    layout_methods = [m["name"] for m in members["methods"] if m["declaring_type"] == "CoH3MemoryTool.GameLayout52426"]
    check((not layout_fields and not layout_methods) if mode == "Release" else (layout_fields == ["candidates", "requiredDependencies"] and "RequireCandidate" in layout_methods), mode + " candidate role table/fingerprint member compile isolation verified")
    constants = {}
    for name, expected in (("MoverReadSize", [284] if mode == "Release" else [184, 284]), ("MoverDirection", [272, 276, 280] if mode == "Release" else [96, 100, 272, 276, 280])):
        method = next(m for m in members["methods"] if m["declaring_type"] == "CoH3MemoryTool.AirLogic" and m["name"] == name)
        body = method["body"]
        check(body["status"] == "present" and body["truncated_instructions"] == 0 and body["decoded_instruction_count"] == body["instruction_count"] == len(body["anchors"]), mode + " " + name + " CIL anchors cover all instructions")
        constants[name] = [int(a["operand"]) for a in body["anchors"] if a["operand_kind"] == "constant" and a["opcode"].startswith("ldc.i4")]
        check(constants[name] == expected, mode + " " + name + " exact compiled integer constants verified")
        raw = dll.read_bytes(); offset = body["file_offset"]
        header = 1 if body["header_format"] == "tiny" else ((struct.unpack_from("<H", raw, offset)[0] >> 12) * 4)
        il = raw[offset + header:offset + header + body["il_size"]]
        check(hashlib.sha256(il).hexdigest() == body["il_sha256"], mode + " " + name + " returned CIL digest matches artifact bytes")
    reviewed_modes.append({"mode": mode, "originalPassed": original_count, "currentPassed": current_count, "dll": str(dll), "sha256": digest, "moverFields": mover_fields, "layoutFields": layout_fields, "layoutMethods": layout_methods, "constants": constants, "memberEvidenceId": evidence["evidence_id"], "moduleMvid": members["module"]["mvid"]})

patch = []
for name in ("Program.cs", "Run.ps1", "README.md", "DecoderLayout.Fixture.csproj", "AirMoverReplay.cs", "verify_task4.py"):
    before = HERE / "task4-baseline" / f"{name}.txt"
    old = before.read_text(encoding="utf-8-sig").splitlines(True) if before.exists() else []
    new = (HERE / name).read_text(encoding="utf-8-sig").splitlines(True)
    patch.extend(difflib.unified_diff(old, new, fromfile=f"task4-baseline/{name}" if before.exists() else "/dev/null", tofile=f"CoH3/dev/SkyEye/tests/decoder-layout/{name}"))
(HERE / "task4-fixture.diff").write_text("".join(patch), encoding="utf-8", newline="")
report = {"status": "PASS_F_only_task4_review", "utc": datetime.datetime.now(datetime.timezone.utc).isoformat(), "checksPassed": len(checks), "checks": checks, "coreSourceFrozen": [{"path": str(ENGINE / name), "sha256": digest} for name, digest in expected_core.items()], "original186Preserved": True, "fixtureChecksPassed": 288, "configurations": reviewed_modes, "gameOrReaderExecution": "not_run", "productionReleaseOrMaintenanceRunnerBuild": "not_run", "reaProvider": "rea-dotnet-static", "reaTargetExecution": False, "limitations": ["Fixture DLLs only; not a formal release artifact audit.", "Reader selector/unchanged guards verified by source inversion, not a linked or live reader run.", "No native loiter event, enemy UI, camera recovery or full air lifetime validation."]}
(HERE / "task4-review.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"status": report["status"], "reviewChecksPassed": len(checks), "fixtureChecksPassed": 288, "original186Preserved": True, "report": str(HERE / "task4-review.json")}, indent=2))
