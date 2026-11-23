#!/usr/bin/env python3
"""p4_fbhash.py - verify Frostbite djb2-xor name hash against BF6 typeinfo/fieldinf."""
import json
import re
import struct
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402

IDENT = re.compile(rb"[A-Za-z_][A-Za-z0-9_:<>, .\-]{2,120}")
TARGETS = [
    "CurrentHealth", "MaxHealth", "HealthPercentage", "Health", "CurrentTeamId", "TeamId",
    "LocalPlayer", "ClientSoldierEntity", "ClientPlayerManager", "EcsWorldTransform",
    "EcsRimeTransform", "ClientSoldierHealthComponent", "Transform", "Position",
    "GameContext", "ClientGame", "Player", "Soldier", "Name", "PlayerName",
]


def fbhash(b: bytes) -> int:
    h = 5381
    for c in b:
        h = ((h * 33) & 0xFFFFFFFF) ^ c
    return h & 0xFFFFFFFF


def universe(img: Img, secname: str):
    s = img.sec[secname]
    raw = img.data[s["roff"]: s["roff"] + s["rsize"]]
    a = np.frombuffer(raw, dtype="<u4")
    return np.unique(a)


def main():
    img = Img()
    ti = universe(img, "typeinfo")
    fi = universe(img, "fieldinf")
    print("typeinfo distinct u32:", ti.size, " fieldinf distinct u32:", fi.size)

    rdata = img.blob(".rdata")
    names = set()
    for m in IDENT.finditer(rdata):
        names.add(m.group(0))
    names = sorted(names)
    print("rdata identifiers:", len(names))

    variants = {
        "raw": lambda b: b,
        "lower": lambda b: b.lower(),
        "upper": lambda b: b.upper(),
        "raw+nul": lambda b: b + b"\0",
        "lower+nul": lambda b: b.lower() + b"\0",
    }

    tiset = set(int(x) for x in ti.tolist())
    fiset = set(int(x) for x in fi.tolist())
    # field-name universe should exclude obviously non-name values but keep it simple
    results = {}
    for vname, vf in variants.items():
        for universe_name, uset in (("typeinfo", tiset), ("fieldinf", fiset)):
            hits = []
            for n in names:
                h = fbhash(vf(n))
                if h in uset:
                    hits.append((n, h))
            results[(vname, universe_name)] = hits
            print("variant=%-10s universe=%-9s hits=%d  (floor~%.1f)" % (
                vname, universe_name, len(hits), len(names) * len(uset) / 2 ** 32))

    out = {}
    for (vname, uname), hits in results.items():
        out["%s|%s" % (vname, uname)] = [{"name": n.decode("ascii", "replace"), "hash": "0x%08x" % h} for n, h in hits]

    print("\n=== target hashes (raw) ===")
    tgt = {}
    for t in TARGETS:
        h = fbhash(t.encode())
        tgt[t] = {"hash": "0x%08x" % h,
                  "in_typeinfo": h in tiset,
                  "in_fieldinf": h in fiset}
        print("  %-32s 0x%08x  typeinfo=%s fieldinf=%s" % (t, h, h in tiset, h in fiset))

    (OUTDIR / "p4_fbhash.json").write_text(json.dumps(
        {"targets": tgt,
         "hits": {("%s|%s" % k): [{"name": n.decode("ascii", "replace"), "hash": "0x%08x" % h} for n, h in v]
                  for k, v in results.items()}},
        indent=1), encoding="utf-8")
    print("\nwrote", OUTDIR / "p4_fbhash.json")


if __name__ == "__main__":
    main()
