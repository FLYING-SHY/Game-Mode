"""One-command verification of the CoH3 5.1.52426.0 per-build adaptation.

Compares current files and recorded evidence with
evidence/version-adaptation/delivery-baseline.json. PASS establishes recorded
material consistency only, not full game/cross-session/delivery acceptance.
Writes its own report unless --no-write is supplied, and exits non-zero when a
check fails.

    python CoH3/dev/SkyEye/scripts/verify_perbuild_adaptation.py [--root F:\\WorkSpace\\Game]

Checks
  1.  game executable fingerprint matches the 5.1.52426.0 build
  2.  delivered binaries match the recorded build outputs (hash + size)
  3.  delivery config still points at the production licensing endpoint
  4.  adaptation sources match the recorded hashes (both mirror trees)
  5.  the two mirror trees are byte-identical for the adapted files
  6.  layout table: 37 roles, required literals, and the three key legacy->new pairs
  7.  fixture rerun evidence (build gate / layout preflight / decoder layout)
  8.  role provenance audit: 37/37 backed, 0 unbacked
  9.  archived live-process samples match their recorded hashes
 10.  shipped client runtime logs contain no version-gate rejection
 11.  evidence document present and complete
"""

from __future__ import annotations

import argparse
import hashlib
import json
import pathlib
import re
import sys
from datetime import datetime, timezone

REPO = pathlib.Path(r"F:\WorkSpace\Game")
BASELINE_REL = "CoH3/dev/SkyEye/evidence/version-adaptation/delivery-baseline.json"

ENTRY_RE = re.compile(r"\{\s*(\d+)\s*,\s*0x([0-9A-Fa-f]+)\s*\}")


def sha256(path: pathlib.Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


class Checks:
    def __init__(self) -> None:
        self.rows: list[dict] = []

    def add(self, ident: str, ok: bool, detail: str) -> bool:
        self.rows.append({"id": ident, "status": "PASS" if ok else "FAIL", "detail": detail})
        return ok

    @property
    def failed(self) -> list[dict]:
        return [row for row in self.rows if row["status"] == "FAIL"]


def check_game(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    game = baseline["game"]
    path = pathlib.Path(game["path"])
    if not path.exists():
        checks.add("game_fingerprint", False, f"game executable not present: {path}")
        return
    actual = sha256(path)
    checks.add(
        "game_fingerprint",
        actual == game["sha256"],
        f"{path.name} sha256={actual} expected={game['sha256']} version={game['version']} build={game['expectedBuild']}",
    )


def check_delivered(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    for item in baseline["deliveredBinaries"]:
        path = root / item["path"]
        if not path.exists():
            checks.add(f"delivered:{path.name}", False, f"missing: {item['path']}")
            continue
        actual = sha256(path)
        size = path.stat().st_size
        ok = actual == item["sha256"] and size == item["bytes"]
        checks.add(
            f"delivered:{path.name}",
            ok,
            f"{item['path']} bytes={size}/{item['bytes']} sha256={actual[:16]}… expected={item['sha256'][:16]}…",
        )


def check_config(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    spec = baseline["deliveryConfig"]
    path = root / spec["path"]
    if not path.exists():
        checks.add("delivery_config", False, f"missing: {spec['path']}")
        return
    text = path.read_text(encoding="utf-8", errors="replace")
    missing = [needle for needle in spec["mustContain"] if needle not in text]
    leaked = [needle for needle in spec["mustNotContain"] if needle in text]
    checks.add(
        "delivery_config",
        not missing and not leaked,
        f"{spec['path']} missing={missing or 'none'} localEndpointLeak={leaked or 'none'}",
    )


def check_sources(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    mismatches = []
    for item in baseline["sourceFiles"]:
        path = root / item["rel"]
        if not path.exists():
            mismatches.append(f"missing {item['rel']}")
            continue
        actual = sha256(path)
        if actual != item["sha256"]:
            mismatches.append(f"{item['rel']} {actual[:12]}… != {item['sha256'][:12]}…")
    checks.add(
        "source_files",
        not mismatches,
        f"{len(baseline['sourceFiles'])} files checked, mismatches: {mismatches or 'none'}",
    )

    pairs = [
        ("GameVersion45.cs", "CoH3/dev/SkyEye/client/CoH3MemoryTool", "CoH3/dev/COH3SimpleTool/CoH3MemoryTool"),
        ("GameLayout52426.cs", "CoH3/dev/SkyEye/client/CoH3MemoryTool", "CoH3/dev/COH3SimpleTool/CoH3MemoryTool"),
        ("PlayerInfoReader.cs", "CoH3/dev/SkyEye/client/CoH3MemoryTool", "CoH3/dev/COH3SimpleTool/CoH3MemoryTool"),
        ("AirLogic.cs", "CoH3/dev/SkyEye/client/CoH3MemoryTool", "CoH3/dev/COH3SimpleTool/CoH3MemoryTool"),
    ]
    drift = []
    for name, left_dir, right_dir in pairs:
        left = root / left_dir / name
        right = root / right_dir / name
        if not left.exists() or not right.exists():
            drift.append(f"{name}: missing")
            continue
        if left.read_bytes() != right.read_bytes():
            drift.append(f"{name}: trees differ")
    checks.add(
        "mirror_trees_identical",
        not drift,
        f"{len(pairs)} adapted files compared across trees, drift: {drift or 'none'}",
    )


def check_layout(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    spec = baseline["layoutTable"]
    path = root / spec["rel"]
    if not path.exists():
        checks.add("layout_table", False, f"missing: {spec['rel']}")
        return
    text = path.read_text(encoding="utf-8")
    entries = {int(legacy, 10): int(new, 16) for legacy, new in ENTRY_RE.findall(text)}
    missing_literals = [lit for lit in spec["mustContainLiterals"] if lit.lower() not in text.lower()]
    key_pairs = {
        "resource": (baseline["legacyLayoutRvas"]["resource"], baseline["newLayoutRvas"]["resource"]),
        "arena": (baseline["legacyLayoutRvas"]["arena"], baseline["newLayoutRvas"]["arena"]),
        "world": (baseline["legacyLayoutRvas"]["world"], baseline["newLayoutRvas"]["world"]),
    }
    pair_problems = []
    for label, (legacy, new) in key_pairs.items():
        actual = entries.get(legacy)
        if actual != new:
            pair_problems.append(f"{label}: {legacy} -> {actual and hex(actual)} expected {hex(new)}")
    ok = (
        len(entries) == spec["expectedRoleCount"]
        and not missing_literals
        and not pair_problems
    )
    checks.add(
        "layout_table",
        ok,
        f"roles={len(entries)}/{spec['expectedRoleCount']} missingLiterals={missing_literals or 'none'} "
        f"keyPairProblems={pair_problems or 'none'}",
    )


def check_fixtures(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    spec = baseline["fixtures"]
    path = root / spec["rel"]
    if not path.exists():
        checks.add("fixtures", False, f"missing: {spec['rel']}")
        return
    data = json.loads(path.read_text(encoding="utf-8"))
    suites = data.get("suites", {})
    problems = []
    for dotted, expected in spec["expect"].items():
        node = data
        for part in dotted.split("."):
            node = node.get(part) if isinstance(node, dict) else None
            if node is None:
                break
        if node != expected:
            problems.append(f"{dotted}={node} expected {expected}")
    def check_exit_codes(node: object, location: str) -> None:
        if isinstance(node, dict):
            for name, value in node.items():
                child = f"{location}.{name}"
                if name == "exitCode" and value != 0:
                    problems.append(f"{child}={value}")
                else:
                    check_exit_codes(value, child)
        elif isinstance(node, list):
            for index, value in enumerate(node):
                check_exit_codes(value, f"{location}[{index}]")

    check_exit_codes(suites, "suites")
    checks.add(
        "fixtures",
        not problems,
        f"suites={list(suites)} problems={problems or 'none'}",
    )


def check_provenance(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    spec = baseline["roleProvenance"]
    path = root / spec["rel"]
    if not path.exists():
        checks.add("role_provenance", False, f"missing: {spec['rel']}")
        return
    data = json.loads(path.read_text(encoding="utf-8"))
    unbacked = data.get("unbacked", [])
    ok = data.get("roleCount") == spec["expectedRoleCount"] and len(unbacked) == spec["expectedUnbacked"]
    checks.add(
        "role_provenance",
        ok,
        f"roleCount={data.get('roleCount')} manifestBacked={data.get('manifestBacked')} "
        f"reportBacked={data.get('reportBacked')} unbacked={len(unbacked)}",
    )


def check_live_samples(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    spec = baseline["liveSamples"]
    index_path = root / spec["index"]
    if not index_path.exists():
        checks.add("live_samples", False, f"missing: {spec['index']}")
        return
    index = json.loads(index_path.read_text(encoding="utf-8"))
    folder = index_path.parent
    problems = []
    for sample in index.get("samples", []):
        path = folder / sample["file"]
        if not path.exists():
            problems.append(f"missing {sample['file']}")
            continue
        actual = sha256(path)
        if actual != sample["sha256"]:
            problems.append(f"{sample['file']} hash drift")
    if len(index.get("samples", [])) < spec["minSamples"]:
        problems.append(f"only {len(index.get('samples', []))} samples")

    newest = folder / "live-new-roots-1922-match2.json"
    if newest.exists():
        payload = json.loads(newest.read_text(encoding="utf-8"))
        players = payload.get("snapshot", {}).get("players", [])
        if len(players) != 8 or not payload.get("snapshot", {}).get("stableRootsAfterRead"):
            problems.append(f"newest live sample players={len(players)} stable={payload.get('snapshot', {}).get('stableRootsAfterRead')}")
        else:
            checks.add(
                "live_sample_newest",
                True,
                f"live-new-roots-1922-match2.json players={len(players)} "
                f"bytesRead={payload.get('bytesRead')} stableRootsAfterRead=True",
            )
    else:
        problems.append("missing live-new-roots-1922-match2.json")
    checks.add(
        "live_samples",
        not problems,
        f"{len(index.get('samples', []))} samples verified, problems={problems or 'none'}",
    )


def decode_log(path: pathlib.Path) -> str:
    """Client diagnostics are UTF-8 (no BOM); fall back to GBK for older runs."""
    raw = path.read_bytes()
    for encoding in ("utf-8-sig", "utf-8", "gbk"):
        try:
            return raw.decode(encoding)
        except UnicodeDecodeError:
            continue
    return raw.decode("utf-8", errors="replace")


def check_runtime_logs(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    spec = baseline["clientRuntimeLogs"]
    folder = root / spec["dir"]
    problems = []
    total_lines = 0
    required_counts = dict.fromkeys(spec["requiredSubstrings"], 0)
    if not required_counts:
        problems.append("no requiredSubstrings configured")
    for name in spec["files"]:
        path = folder / name
        if not path.exists():
            problems.append(f"missing {name}")
            continue
        text = decode_log(path)
        lines = [line for line in text.splitlines() if line.strip()]
        total_lines += len(lines)
        for needle in spec["forbiddenSubstrings"]:
            if needle in text:
                problems.append(f"{name} contains {needle!r}")
        for needle in required_counts:
            required_counts[needle] += text.count(needle)
    for needle, count in required_counts.items():
        if count == 0:
            problems.append(f"missing required substring {needle!r} in runtime logs")
    draws = required_counts.get(spec["requiredSubstrings"][0], 0) if required_counts else 0
    checks.add(
        "client_runtime_logs",
        not problems and draws > 0,
        f"{len(spec['files'])} logs, {total_lines} lines, minimap draws={draws}, problems={problems or 'none'}",
    )


def check_document(checks: Checks, root: pathlib.Path, baseline: dict) -> None:
    spec = baseline["evidenceDocument"]
    path = root / spec["rel"]
    if not path.exists():
        checks.add("evidence_document", False, f"missing: {spec['rel']}")
        return
    data = path.read_bytes()
    text = data.decode("utf-8", errors="replace")
    ok = len(data) >= spec["minBytes"] and "游戏版本未验证" in text and "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec" in text
    checks.add(
        "evidence_document",
        ok,
        f"{path.name} bytes={len(data)} (min {spec['minBytes']}) lines={text.count(chr(10)) + 1}",
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Verify recorded 5.1.52426.0 material consistency, not full G/R/D acceptance")
    parser.add_argument("--root", default=str(REPO), help="repository root")
    parser.add_argument("--no-write", action="store_true", help="do not write the report file")
    args = parser.parse_args()
    root = pathlib.Path(args.root)

    baseline_path = root / BASELINE_REL
    if not baseline_path.exists():
        print(f"baseline not found: {baseline_path}", file=sys.stderr)
        return 2
    baseline = json.loads(baseline_path.read_text(encoding="utf-8"))

    checks = Checks()
    check_game(checks, root, baseline)
    check_delivered(checks, root, baseline)
    check_config(checks, root, baseline)
    check_sources(checks, root, baseline)
    check_layout(checks, root, baseline)
    check_fixtures(checks, root, baseline)
    check_provenance(checks, root, baseline)
    check_live_samples(checks, root, baseline)
    check_runtime_logs(checks, root, baseline)
    check_document(checks, root, baseline)

    stamp = datetime.now(timezone.utc).strftime("%Y%m%d-%H%M%S")
    report = {
        "verifiedUtc": stamp,
        "root": str(root),
        "baseline": BASELINE_REL,
        "verificationScope": "recorded_material_consistency_only",
        "acceptanceClaim": "PASS establishes material consistency only; full game/cross-session/delivery acceptance is not assessed.",
        "checks": checks.rows,
        "passed": not checks.failed,
        "summary": {
            "total": len(checks.rows),
            "passed": len(checks.rows) - len(checks.failed),
            "failed": len(checks.failed),
        },
    }
    report_path = root / f"CoH3/dev/SkyEye/evidence/version-adaptation/verify-perbuild-{stamp}.json"
    if not args.no_write:
        report_path.write_text(json.dumps(report, ensure_ascii=False, indent=1), encoding="utf-8")

    width = max(len(row["id"]) for row in checks.rows)
    for row in checks.rows:
        print(f"[{row['status']}] {row['id']:<{width}}  {row['detail']}")
    print(f"-- {report['summary']['passed']}/{report['summary']['total']} recorded-material checks passed; full G/R/D acceptance not assessed")
    print(f"-- report: {'not written (--no-write)' if args.no_write else report_path.name}")
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
