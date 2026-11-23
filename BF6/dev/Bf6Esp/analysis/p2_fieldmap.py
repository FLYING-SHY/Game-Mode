"""Build the fieldinf -> type-name -> member-offset map.

Chain: fieldinf u32 (low32 of a .data type-cell VA)
       -> .data cell (0x20 B) -> q0 = VA of the static typeinfo record
       -> typeinfo record -> (u32_0 key, module desc, layout id)
       -> name via the verified anchor table (stub -> node -> cell -> q0 -> record).
"""
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000
FI = SEC["fieldinf"]
TI = SEC["typeinfo"]
DA = SEC[".data"]

fi = DATA[FI["roff"]:FI["roff"] + FI["rsize"]]
u32 = np.frombuffer(fi, dtype="<u4")
anchors = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
rec2name = {}
for a in anchors:
    rec2name[a["rec_off"]] = (a["type"], a["module"], a["hash"])
print(f"anchor records: {len(rec2name)}")


def cell_at(rva):
    """return dict if rva points at a .data type cell, else None"""
    if not (DA["va"] <= rva < DA["va"] + DA["vsize"]):
        return None
    o = img.rva2off(rva)
    if o is None:
        return None
    q0 = int.from_bytes(DATA[o:o + 8], "little")
    q0r = (q0 - IB) if q0 >= IB else q0
    if not (TI["va"] <= q0r < TI["va"] + TI["rsize"]):
        return None
    flags = int.from_bytes(DATA[o + 8:o + 10], "little")
    size = int.from_bytes(DATA[o + 10:o + 12], "little")
    return {"rva": rva, "q0": q0, "q0_rva": q0r, "rec_off": q0r - TI["va"],
            "flags": flags, "size": size}


# scan the fieldinf stream for cell pointers
hits = []
pos = 0
N = len(u32)
vals = u32.tolist()
i = 0
while i < N:
    v = vals[i]
    c = None
    for cand in ((v + 0xC0000000) & 0xFFFFFFFF, v):
        c = cell_at(cand)
        if c:
            break
    if c:
        hits.append((i, c))
    i += 1
print(f"fieldinf positions holding a .data type-cell pointer: {len(hits)}")

named = [(i, c) for i, c in hits if c["rec_off"] in rec2name]
print(f"  of which the cell resolves to an ANCHORED type record: {len(named)}")
if named:
    ex = named[:8]
    for i, c in ex:
        nm = rec2name[c["rec_off"]]
        print(f"    idx {i:7d} cell rva 0x{c['rva']:x} -> rec_off {c['rec_off']} "
              f"= {nm[0]} ({nm[1]}) hash 0x{nm[2]:08x} size={c['size']}")

# distinct cells
cells = {}
for i, c in hits:
    cells.setdefault(c["rva"], {"cell": c, "positions": [], "name": None})
    cells[c["rva"]]["positions"].append(i)
    if c["rec_off"] in rec2name:
        cells[c["rva"]]["name"] = rec2name[c["rec_off"]][0]
print(f"\ndistinct cells referenced from fieldinf: {len(cells)}")
print(f"  cells that resolve to a named type     : {sum(1 for v in cells.values() if v['name'])}")
namedcells = {k: v for k, v in cells.items() if v["name"]}
print("  sample names:", [v["name"] for v in list(namedcells.values())[:15]])

# multi-hit cells (same cell referenced several times?)
mult = Counter(len(v["positions"]) for v in cells.values())
print("  hits-per-cell histogram:", mult.most_common(8))

# ---- block structure: between two consecutive cell pointers, what is there? ----
print("\n--- block neighbourhoods (named cells) ---")
pos_of_named = sorted(i for i, c in named)
for k in range(min(4, len(pos_of_named))):
    p = pos_of_named[k]
    seg = [hex(v) for v in vals[max(0, p - 2):p + 20]]
    print(f"  idx {p}: " + " ".join(seg))

out = {"fieldinf_cell_pointers": len(hits),
       "anchored_cell_pointers": len(named),
       "distinct_cells": len(cells),
       "named_cells": len(namedcells),
       "hits_per_cell_histogram": {str(k): v for k, v in mult.items()},
       "cells": [{"cell_rva": hex(k), "rec_off": v["cell"]["rec_off"], "name": v["name"],
                  "size": v["cell"]["size"], "flags": v["cell"]["flags"],
                  "positions": v["positions"][:20], "n_positions": len(v["positions"])}
                 for k, v in sorted(cells.items())]}
(HERE / "p2_fieldmap_raw.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("\nwrote p2_fieldmap_raw.json")
