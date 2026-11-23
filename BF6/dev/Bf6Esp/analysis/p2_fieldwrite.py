#!/usr/bin/env python3
"""p2_fieldwrite: which functions write into the fieldinf / typeinfo sections?
(group RIP-relative refs by enclosing .pdata function)"""
import struct
import sys
from collections import Counter
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402

img = Img()
data = img.data

# ---- .pdata runtime functions
pd = img.sec[".pdata"]
n = pd["rsize"] // 12
raw = np.frombuffer(data[pd["roff"]:pd["roff"] + n * 12], dtype="<u4").reshape(n, 3)
beg = raw[:, 0].astype(np.int64)
end = raw[:, 1].astype(np.int64)
order = np.argsort(beg)
beg, end = beg[order], end[order]
print("pdata functions:", n)


def func_of(rva):
    i = int(np.searchsorted(beg, rva)) - 1
    while i >= 0 and i < len(beg):
        if beg[i] <= rva < end[i]:
            return int(beg[i]), int(end[i])
        i -= 1
    return None


z = np.load(OUTDIR / "p2_riprefs.npz")
code = z["code"]
tgt = z["tgt"]
form = z["form"]

for sec, lo, hi in (("fieldinf", 0xB48B000, 0xB6D2C00), ("typeinfo", 0xAECC000, 0xB48B000)):
    m = (tgt >= lo) & (tgt < hi)
    idx = np.nonzero(m)[0]
    cnt = Counter()
    for i in idx:
        f = func_of(int(code[i]))
        cnt[f] += 1
    print(f"\n=== {sec}: {len(idx)} refs, top functions")
    for f, c in cnt.most_common(18):
        if f is None:
            print(f"   func <none>  refs={c}")
        else:
            print(f"   func {f[0]:#x}-{f[1]:#x}  refs={c}")
    # dump raw bytes of the top 3
    for f, c in cnt.most_common(3):
        if f is None:
            continue
        b0, b1 = f
        off = img.rva2off(b0)
        print(f"\n--- bytes of {b0:#x}-{b1:#x} ({c} refs)")
        for k in range(0, min(b1 - b0, 0x400), 16):
            row = data[off + k:off + k + 16]
            print(f"  {b0+k:08x}  " + " ".join(f"{x:02x}" for x in row))
