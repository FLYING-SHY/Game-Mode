#!/usr/bin/env python3
"""p2_tables_dump: for each detected name-pointer table, dump the raw cell
contents around the name pointers to look for an adjacent member OFFSET."""
import json
import re
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR, IMAGE_BASE  # noqa: E402

IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}\Z")


def main():
    img = Img()
    data = img.data
    rd = img.sec[".rdata"]
    rdb = data[rd["roff"]:rd["roff"] + rd["rsize"]]
    base = IMAGE_BASE + rd["va"]
    namemap = {}
    i = 0
    while True:
        j = rdb.find(b"\x00", i)
        if j < 0:
            break
        s = rdb[i:j]
        if 1 <= len(s) <= 64 and IDENT.match(s):
            namemap[base + i] = s
        i = j + 1
    namemap_bytes = {k: v for k, v in namemap.items()}
    colour = {b"AliceBlue", b"AntiqueWhite", b"Aqua"}

    tables = json.load(open(OUTDIR / "p2_name_tables.json"))
    tables = [t for t in tables if t["count"] >= 5]
    tables.sort(key=lambda t: -t["count"])
    out = []
    for t in tables[:12]:
        s = img.sec[t["section"]]
        off = s["roff"] + (int(t["rva"], 16) - s["va"])
        n = t["count"]
        stride = t["stride"]
        cells = []
        for k in range(min(n, 14)):
            c = off + k * stride
            q = int.from_bytes(data[c:c + 8], "little")
            rest = [int.from_bytes(data[c + 8 + 4 * z:c + 12 + 4 * z], "little")
                    for z in range(max(0, (stride - 8) // 4))]
            nm = namemap_bytes.get(q)
            cells.append({"slot": k, "off": hex(c), "ptr": hex(q),                          "name": nm.decode("latin1") if nm else None,
                          "rest_u32": rest[:7]})
        print(f"\n=== {t['section']} rva={t['rva']} n={n} stride={stride}")
        for cc in cells:
            print("   ", cc["slot"], cc["off"], cc["name"], cc["rest_u32"])
        out.append({"table": t, "cells": cells})

    json.dump(out, open(OUTDIR / "p2_table_cells.json", "w"), indent=1)
    print("\nwritten p2_table_cells.json")


if __name__ == "__main__":
    main()
