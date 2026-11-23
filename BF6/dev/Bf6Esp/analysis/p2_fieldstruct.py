"""Map the internal structure of the fieldinf section (u32 stream)."""
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
        return "Z"
    if v < 0x1000:
        return "S"
    r = rva_of(v)
    if DA["va"] <= r < DA["va"] + DA["vsize"]:
        return "D"
    if RD["va"] <= r < RD["va"] + RD["rsize"]:
        return "R"
    if TX["va"] <= r < TX["va"] + TX["rsize"]:
        return "T"
    return "H"


cat = [classify(v) for v in vals]

print("first 220 u32 of fieldinf (idx: value cat):")
line = []
for i in range(220):
    line.append(f"{i}:{vals[i]:08x}{cat[i]}")
    if len(line) == 8:
        print("  " + "  ".join(line))
        line = []
if line:
    print("  " + "  ".join(line))

# runs of the 48-byte pattern: key,0,0,0, cell,1,0,0,0,0,0,0
pat = []
i = 0
while i + 12 <= N:
    if (cat[i] in "HS" and vals[i + 1] == 0 and vals[i + 2] == 0 and vals[i + 3] == 0
            and cat[i + 4] == "D" and vals[i + 5] == 1 and all(vals[i + k] == 0 for k in range(6, 12))):
        pat.append(i)
        i += 12
    else:
        i += 1
print(f"\n48-byte {{key,0,0,0,cellptr,1,0*6}} records: {len(pat)}")
if pat:
    runs = []
    s = pat[0]
    prev = pat[0]
    for x in pat[1:]:
        if x == prev + 12:
            prev = x
        else:
            runs.append((s, prev))
            s = x
            prev = x
    runs.append((s, prev))
    print(f"  contiguous runs: {len(runs)}")
    for a, b in runs[:12]:
        print(f"    idx {a}..{b}  ({b - a + 1} records)")

# density map of 'S' (small=offset candidate) per 1024-u32 window
win = 1024
dens = []
for a in range(0, N, win):
    seg = cat[a:a + win]
    dens.append((a, seg.count("S"), seg.count("D"), seg.count("H"), seg.count("Z"), seg.count("R")))
print("\nwindow map (idx, S, D, H, Z, R) for the first 40 windows of 1024 u32:")
for d in dens[:40]:
    print("  ", d)
print("  ...")
print("total windows:", len(dens))
tot = sum(d[1] for d in dens)
print("small values total:", tot)
# windows with many small values
rich = sorted(dens, key=lambda x: -x[1])[:15]
print("\nrichest windows by 'S' count:", rich)

(HERE / "p2_fieldstruct.json").write_text(json.dumps({
    "n_u32": N,
    "cat_counts": dict(Counter(cat)),
    "pattern48_records": len(pat),
    "pattern48_runs": [[a, b] for a, b in runs[:50]],
    "window_density": dens,
}, indent=1), encoding="utf-8")
print("\nwrote p2_fieldstruct.json")
