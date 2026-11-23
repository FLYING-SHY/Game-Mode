"""Scan every discovered name-pointer table for cells that also carry a member offset."""
import json
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000

tables = json.loads((HERE / "p2_name_tables.json").read_text(encoding="utf-8"))
print("tables:", type(tables), len(tables))
if isinstance(tables, dict):
    print("keys:", list(tables.keys())[:10])
    items = tables.get("tables", [])
else:
    items = tables
print("items:", len(items))
print("sample:", json.dumps(items[0], indent=1)[:1200])

best = []
for t in items:
    sec = t.get("section") or t.get("sec")
    rva = t.get("rva")
    stride = t.get("stride")
    n = t.get("count") or t.get("n")
    if isinstance(rva, str):
        rva = int(rva, 16)
    if not sec or rva is None or not stride or not n:
        continue
    if sec not in (".rdata", ".data"):
        continue
    if n < 2:
        continue
    o = img.rva2off(rva)
    if o is None:
        continue
    offs = []
    for i in range(min(n, 64)):
        cell = DATA[o + i * stride:o + (i + 1) * stride]
        for k in range(0, len(cell) - 3):
            v = int.from_bytes(cell[k:k + 4], "little")
            if 0 < v < 0x1000 and v % 4 == 0:
                offs.append((i, k, v))
    if offs:
        best.append((len(offs), sec, hex(rva), stride, n, offs[:24]))

best.sort(reverse=True)
print(f"\ntables with offset-like cells: {len(best)}")
for b in best[:25]:
    print(f"  n_off={b[0]:4d} {b[1]:8s} rva={b[2]} stride={b[3]} n={b[4]} {b[5][:12]}")
(HERE / "p2_taboff.json").write_text(json.dumps(
    [{"n_off": b[0], "sec": b[1], "rva": b[2], "stride": b[3], "n": b[4], "cells": b[5]} for b in best[:500]],
    indent=1), encoding="utf-8")
print("\nwrote p2_taboff.json")
