"""Decisive decode of the fieldinf element layout (0x30 stride)."""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec

fi = SEC["fieldinf"]
ti = SEC["typeinfo"]

tblob = DATA[ti["roff"]:ti["roff"] + ti["rsize"]]
arr = np.frombuffer(tblob, dtype="<u4")
hits = np.nonzero(arr == 0x00080089)[0]
bases = hits - 1
d0 = set(int(x) for x in arr[bases])
d11 = set(int(x) for x in arr[bases + 11])
d10 = set(int(x) for x in arr[bases + 10])
print(f"typeinfo records={len(bases)} |distinct u32_0|={len(d0)} |u32_10|={len(d10)} |u32_11|={len(d11)}")

farr = np.frombuffer(DATA[fi["roff"]:fi["roff"] + fi["rsize"]], dtype="<u4")
print(f"fieldinf u32 count={len(farr)}  -> 0x30-elements={len(farr)//12}")

S = 6
for s in range(S):
    slot = farr[s::S]
    u = np.unique(slot)
    in11 = int(np.isin(slot, list(d11)).sum())
    in0 = int(np.isin(slot, list(d0)).sum())
    small = int((slot < 0x1000).sum())
    print(f"\nslot {s} (+0x{s*4:02x}): distinct={len(u)} in_u32_11={in11} in_u32_0={in0} <0x1000={small}")
    print(f"   min={slot.min()} max={slot.max()}")
    top = np.unique(slot, return_counts=True)
    order = np.argsort(-top[1])[:8]
    print("   top:", [(int(top[0][i]), int(top[1][i])) for i in order])

# candidate offset slot: which slots are dominated by small values?
print("\n--- value histogram for slot 2 (+0x08) small values ---")
slot2 = farr[2::S]
sm = slot2[slot2 < 0x1000]
u, c = np.unique(sm, return_counts=True)
o = np.argsort(-c)[:20]
print("  ", [(int(u[i]), int(c[i])) for i in o])
print("   total <0x1000:", len(sm), "of", len(slot2))

print("\n--- slot 4 (+0x10) as u64 ptr check ---")
lo = farr[4::S].astype(np.uint64)
hi = farr[5::S].astype(np.uint64)
ptr = lo | (hi << 32)
inbin = ((ptr >= 0x140000000) & (ptr < 0x150000000)).sum()
print("   ptr-like VAs in image:", int(inbin), "of", len(ptr))
sec_of = {}
for v in np.unique(ptr[inbin])[:20000]:
    r = int(v) - 0x140000000
    s = img.sec_of(r)
    sec_of[s] = sec_of.get(s, 0) + 1
print("   sections of distinct ptr targets:", sec_of)
