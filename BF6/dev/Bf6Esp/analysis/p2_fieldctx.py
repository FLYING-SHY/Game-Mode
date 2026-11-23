"""Classify every u32 in the fieldinf stream and dump context around cell pointers."""
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
FI = SEC["fieldinf"]
TI = SEC["typeinfo"]
DA = SEC[".data"]
RD = SEC[".rdata"]
TX = SEC[".text"]

fi = DATA[FI["roff"]:FI["roff"] + FI["rsize"]]
u32 = np.frombuffer(fi, dtype="<u4")
vals = u32.tolist()
N = len(vals)


def rva_of(v):
    return (v + 0xC0000000) & 0xFFFFFFFF


def classify(v):
    if v == 0:
        return "zero"
    if v < 0x1000:
        return "small"
    r = rva_of(v)
    if r == v and v >= 0x140000000:
        r = v - IB
    if DA["va"] <= r < DA["va"] + DA["vsize"]:
        return "data_va"
    if RD["va"] <= r < RD["va"] + RD["rsize"]:
        return "rdata_va"
    if TX["va"] <= r < TX["va"] + TX["rsize"]:
        return "text_va"
    if TI["va"] <= r < TI["va"] + TI["rsize"]:
        return "ti_va"
    if 0x80000000 <= v <= 0xFFFFFFFF:
        return "hi32"
    return "other"


cats = Counter()
cat_of = []
for v in vals:
    c = classify(v)
    cats[c] += 1
    cat_of.append(c)
print("fieldinf u32 classification:", cats.most_common())

# rdata pointer targets -> strings
rdata_ptr_idx = [i for i, c in enumerate(cat_of) if c == "rdata_va"]
print(f"\nrdata_va pointers: {len(rdata_ptr_idx)}")
strs = []
for i in rdata_ptr_idx:
    r = rva_of(vals[i])
    s = img.cstr_at_rva(r)
    strs.append((i, s))
strs_nonnull = [x for x in strs if x[1]]
print(f"  resolvable strings: {len(strs_nonnull)}")
print("  sample:", [x[1] for x in strs_nonnull[:40]])
cnt = Counter(x[1] for x in strs_nonnull)
print("  top strings:", cnt.most_common(25))

# context around cell pointers
raw = json.loads((HERE / "p2_fieldmap_raw.json").read_text(encoding="utf-8"))
pos = []
for c in raw["cells"]:
    pos.extend(c["positions"])
pos = sorted(set(pos))
print(f"\ncell-pointer positions: {len(pos)}")
for p in pos[:6]:
    ctx = [(p + k, vals[p + k], cat_of[p + k]) for k in range(-4, 18) if 0 <= p + k < N]
    print(f"\n-- idx {p}")
    for i, v, c in ctx:
        mark = " <<<" if i == p else ""
        print(f"   {i:7d} 0x{v:08x} {v:12d} {c}{mark}")
