"""Per-slot statistics of the fieldinf u32 stream on a 12- and 24-slot grid."""
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
u32 = np.frombuffer(fi, dtype="<u4").astype(np.int64)
N = len(u32)


def rva_of(v):
    return (v + 0xC0000000) & 0xFFFFFFFF


def cls(v):
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


for stride in (12, 24):
    n = N // stride
    grid = u32[:n * stride].reshape(n, stride)
    print(f"\n===== stride {stride} u32 ({stride * 4} bytes), {n} records =====")
    for s in range(stride):
        col = grid[:, s]
        cc = Counter(cls(int(v)) for v in col)
        nz = col[col != 0]
        print(f"  slot {s:2d}: distinct={len(set(col.tolist())):6d} nonzero={len(nz):6d} "
              f"min={int(nz.min()) if len(nz) else 0:10d} max={int(nz.max()) if len(nz) else 0:10d} "
              f"cats={dict(cc.most_common(4))}")

print("\n===== first 40 records, stride 12 =====")
n = N // 12
grid = u32[:n * 12].reshape(n, 12)
for i in range(40):
    row = grid[i]
    parts = []
    for j, v in enumerate(row):
        v = int(v)
        if v == 0:
            continue
        parts.append(f"[{j}]{v:#010x}{cls(v)}")
    print(f" rec {i:4d} @{i * 12:6d}: " + " ".join(parts))

print("\n===== first 40 records, stride 24 (nonzero only) =====")
n2 = N // 24
grid2 = u32[:n2 * 24].reshape(n2, 24)
for i in range(40):
    parts = []
    for j, v in enumerate(grid2[i]):
        v = int(v)
        if v == 0:
            continue
        parts.append(f"[{j}]{v:#010x}{cls(v)}")
    print(f" rec {i:4d} @{i * 24:6d}: " + " ".join(parts))
