"""Decode the fieldinf section: per-slot link test + raw element dump."""
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

fi = SEC["fieldinf"]
ti = SEC["typeinfo"]
print(f"fieldinf va=0x{fi['va']:x} roff=0x{fi['roff']:x} rsize=0x{fi['rsize']:x}")

# ---- raw dump of the first bytes ----
o = fi["roff"]
print("\n--- first 0x100 bytes of fieldinf ---")
for k in range(0, 0x100, 16):
    print(f"  {fi['va']+k:08x}  " + " ".join(f"{x:02x}" for x in DATA[o + k:o + k + 16]))

# ---- typeinfo records (stride 0x50, marker 0x00080089 at +0x04) ----
MARK = 0x00080089
tblob = DATA[ti["roff"]:ti["roff"] + ti["rsize"]]
arr = np.frombuffer(tblob, dtype="<u4")
hits = np.nonzero(arr == MARK)[0]
# record base = marker index - 1 (u32 index)
bases = hits - 1
rec = arr.reshape(-1, 16)
key0 = arr[bases * 1]        # u32_0 of each record
key11 = arr[bases * 16 + 11]  # u32_11
d0 = set(int(x) for x in key0)
d11 = set(int(x) for x in key11)
print(f"\ntypeinfo records={len(bases)} distinct u32_0={len(d0)} distinct u32_11={len(d11)}")

# ---- fieldinf u32 array ----
farr = np.frombuffer(DATA[fi["roff"]:fi["roff"] + fi["rsize"]], dtype="<u4")
print(f"fieldinf u32 count = {len(farr)}")

STRIDE = 0x30 // 4  # u32 per 0x30 group
for s in range(0, 12):
    slot = farr[s::STRIDE]
    in11 = int(np.isin(slot, list(d11)).sum())
    in0 = int(np.isin(slot, list(d0)).sum())
    small = int((slot < 0x1000).sum())
    datalow = int(((slot & 0xFFFF0000) == 0x00490000).sum())
    print(f" slot {s:2d}: distinct={len(np.unique(slot)):7d} in_u32_11_domain={in11:7d} "
          f"in_u32_0_domain={in0:5d} values<0x1000={small:7d} 0x0049xxxx={datalow:7d}")

# ---- element dump at the best stride ----
print("\n--- first 6 elements at 0x18 stride ---")
for i in range(6):
    base = fi["va"] + i * 0x18
    oo = img.rva2off(base)
    b = DATA[oo:oo + 0x18]
    u = np.frombuffer(b, dtype="<u4")
    print(f"  rva 0x{base:x}: " + " ".join(f"{x:08x}" for x in u))
