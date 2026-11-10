"""Summarize a REA inspect-pe-imports JSON report: list imported DLLs and flag anti-cheat / telemetry libs.

Usage: python rea_imports_summary.py <rea_imports.json>
"""
import json
import re
import sys

SUSPICIOUS = [
    "easyanticheat", "anticheat", "battleye", "beservice", "bedaisy",
    "gameguard", "nprotect", "xigncode", "themida", "vmprotect", "punkbuster",
    "easyhook", "telemetry", "snowplow", "analytics", "appsflyer",
    "amplitude", "flurry", "bugsnag", "crashlytics", "sentry", "playfab",
    "eaclauncher", "gameoverlayrenderer", "steam_api", "steamservice",
]


def walk_strings(obj):
    if isinstance(obj, str):
        yield obj
    elif isinstance(obj, dict):
        for v in obj.values():
            yield from walk_strings(v)
    elif isinstance(obj, list):
        for v in obj:
            yield from walk_strings(v)


def main():
    path = sys.argv[1]
    with open(path, "r", encoding="utf-8-sig", errors="replace") as f:
        raw = f.read()
    data = json.loads(raw)
    print("top-level keys:", list(data.keys())[:40])

    # find dll-ish strings
    dlls = set()
    names = set()
    for s in walk_strings(data):
        low = s.lower()
        if low.endswith(".dll"):
            dlls.add(s)
        elif re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]{2,}", s):
            names.add(s)

    print("\n=== imported DLLs (%d) ===" % len(dlls))
    for d in sorted(dlls, key=str.lower):
        print("  " + d)

    hits = sorted({d for d in dlls if any(k in d.lower() for k in SUSPICIOUS)})
    print("\n=== anti-cheat / telemetry / workshop dll hits ===")
    print("  " + (", ".join(hits) if hits else "(none)"))

    # imported function names of interest
    fnhits = sorted({n for n in names if any(k in n.lower() for k in SUSPICIOUS)})
    print("\n=== imported symbol hits (anti-cheat/telemetry/workshop) ===")
    for n in fnhits[:80]:
        print("  " + n)
    if not fnhits:
        print("  (none)")


if __name__ == "__main__":
    main()
