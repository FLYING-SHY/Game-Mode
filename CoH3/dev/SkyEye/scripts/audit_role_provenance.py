#!/usr/bin/env python3
"""Audit the provenance of every offset in the 5.1.52426.0 layout table.

Read-only. Parses the shipped table (GameLayout52426.cs), the recorded static
signature manifest (static/signatures-by-hash.json) and every relocation report
under evidence/version-adaptation/, then reports for each table entry whether it
is backed by

  * the static manifest  - a decode-site expected_target_rva or a role_candidates
                           entry pointing at the same new RVA, or
  * an earlier report    - a relocation report that lists the same new RVA.

The adaptation write-up itself (perbuild-adaptation-52426.md) is excluded from
the report evidence, otherwise the audit would grade its own homework.

Usage:
    python CoH3/dev/SkyEye/scripts/audit_role_provenance.py [--repo-root PATH]

Outputs (written next to the reports):
    evidence/version-adaptation/role-provenance.json
    evidence/version-adaptation/role-provenance.md
"""
from __future__ import annotations

import argparse
import json
import pathlib
import re
import sys

ENTRY = re.compile(
    r"\{\s*(?P<legacy>[^,]+?)\s*,\s*(?P<new>0x[0-9a-fA-F]+)\s*\}\s*,?\s*//\s*(?P<note>.*)"
)
RVA_TEXT = re.compile(r"0x([0-9a-fA-F]{5,8})")
SELF_DOC = "perbuild-adaptation-52426.md"


def find_repo_root(start: pathlib.Path) -> pathlib.Path:
    for candidate in [start, *start.parents]:
        if (candidate / "CoH3" / "dev" / "SkyEye").is_dir():
            return candidate
    raise SystemExit("repository root not found (expected CoH3/dev/SkyEye below it)")


def parse_table(table_path: pathlib.Path) -> list[dict]:
    rows: list[dict] = []
    for line in table_path.read_text(encoding="utf-8-sig").splitlines():
        match = ENTRY.search(line)
        if not match:
            continue
        legacy_raw = match.group("legacy").strip()
        if legacy_raw.startswith("{{"):
            legacy_dec = None
            legacy_hex = None
        else:
            legacy_dec = int(legacy_raw, 0)
            legacy_hex = legacy_dec
        rows.append(
            {
                "legacyRaw": legacy_raw,
                "legacyDec": legacy_dec,
                "legacyHex": None if legacy_hex is None else f"0x{legacy_hex:X}",
                "newRva": int(match.group("new"), 16),
                "newRvaHex": match.group("new").lower(),
                "note": match.group("note").strip(),
            }
        )
    return rows


def manifest_index(manifest_path: pathlib.Path) -> dict[int, list[str]]:
    data = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    builds = data.get("builds", {})
    if not builds:
        raise SystemExit(f"no builds in {manifest_path}")
    build = next(iter(builds.values()))
    by_new: dict[int, list[str]] = {}
    for candidate in build.get("role_candidates", []):
        new_hex = candidate.get("new_candidate_rva")
        if new_hex:
            by_new.setdefault(int(new_hex, 16), []).append(
                f"role_candidates:{candidate.get('name', '?')}"
                f"[{candidate.get('evidence_level', '?')}]"
            )
    for feature in build.get("features", []):
        for site in feature.get("decode_sites", []):
            target = site.get("expected_target_rva")
            if target:
                by_new.setdefault(int(target, 16), []).append(
                    f"feature:{feature.get('id', '?')}->{site.get('kind', '?')}"
                )
    return by_new


def report_index(reports_root: pathlib.Path) -> dict[int, list[str]]:
    hits: dict[int, list[str]] = {}
    for path in sorted(reports_root.rglob("*.md")):
        relative = path.relative_to(reports_root).as_posix()
        if relative == SELF_DOC:
            continue
        text = path.read_text(encoding="utf-8", errors="replace")
        for number in set(RVA_TEXT.findall(text)):
            hits.setdefault(int(number, 16), []).append(relative)
    return hits


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", default=None)
    args = parser.parse_args()

    here = pathlib.Path(__file__).resolve()
    root = pathlib.Path(args.repo_root).resolve() if args.repo_root else find_repo_root(here.parents[3])
    sky = root / "CoH3" / "dev" / "SkyEye"
    table_path = sky / "client" / "CoH3MemoryTool" / "GameLayout52426.cs"
    manifest_path = sky / "evidence" / "version-adaptation" / "static" / "signatures-by-hash.json"
    reports_root = sky / "evidence" / "version-adaptation"

    rows = parse_table(table_path)
    manifest = manifest_index(manifest_path)
    reports = report_index(reports_root)

    for row in rows:
        row["manifestSources"] = manifest.get(row["newRva"], [])
        files = reports.get(row["newRva"], [])
        row["reportMentions"] = files
        row["backedByManifest"] = bool(row["manifestSources"])
        row["backedByReport"] = bool(files)
        row["backed"] = row["backedByManifest"] or row["backedByReport"]

    payload = {
        "table": table_path.as_posix(),
        "manifest": manifest_path.as_posix(),
        "roleCount": len(rows),
        "manifestBacked": sum(1 for r in rows if r["backedByManifest"]),
        "reportBacked": sum(1 for r in rows if r["backedByReport"] and not r["backedByManifest"]),
        "unbacked": [r["newRvaHex"] for r in rows if not r["backed"]],
        "rows": rows,
    }

    json_path = reports_root / "role-provenance.json"
    md_path = reports_root / "role-provenance.md"
    json_path.write_text(json.dumps(payload, ensure_ascii=False, indent=1), encoding="utf-8")

    lines = [
        "# 5.1.52426.0 布局表偏移溯源审计",
        "",
        "由 `CoH3/dev/SkyEye/scripts/audit_role_provenance.py` 自动生成（只读）。",
        "",
        f"- 表内角色总数：{payload['roleCount']}",
        f"- 静态清单直接支撑：{payload['manifestBacked']}",
        f"- 既有重定位报告支撑：{payload['reportBacked']}",
        f"- 无来源条目：{len(payload['unbacked'])} {payload['unbacked']}",
        "",
        "| 旧偏移 | 新 RVA | 来源 | 说明 |",
        "| --- | --- | --- | --- |",
    ]
    for row in rows:
        if row["backedByManifest"]:
            source = "静态清单：" + "；".join(row["manifestSources"])
        elif row["backedByReport"]:
            source = "报告：" + "、".join(row["reportMentions"][:3])
        else:
            source = "**无**"
        lines.append(
            f"| {row['legacyHex'] or row['legacyRaw']} | `{row['newRvaHex']}` | {source} | {row['note']} |"
        )
    md_path.write_text("\n".join(lines) + "\n", encoding="utf-8")

    print(f"roles={payload['roleCount']}")
    print(f"manifest-backed={payload['manifestBacked']}")
    print(f"report-backed={payload['reportBacked']}")
    print(f"unbacked={payload['unbacked']}")
    print(f"wrote {json_path}")
    print(f"wrote {md_path}")
    return 0 if not payload["unbacked"] else 1


if __name__ == "__main__":
    sys.exit(main())
