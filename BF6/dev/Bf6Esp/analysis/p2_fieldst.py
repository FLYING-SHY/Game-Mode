"""Find code that stores into fieldinf and describe those functions."""
import json
import struct
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img, IMAGE_BASE  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
z = np.load(HERE / "p2_riprefs.npz")
CODE, TGT, FRM = z["code"], z["tgt"], z["form"]
pdata = img.blob(".pdata")
nfun = len(pdata) // 12
beg = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[0::3].astype(np.int64)
end = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[1::3].astype(np.int64)
FI_LO, FI_HI = SEC["fieldinf"]["va"], SEC["fieldinf"]["va"] + SEC["fieldinf"]["vsize"]
TI_LO, TI_HI = SEC["typeinfo"]["va"], SEC["typeinfo"]["va"] + SEC["typeinfo"]["vsize"]


def func_of(rva):
    i = np.searchsorted(beg, rva, side="right") - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


def str_at_rva(rva, limit=80):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    o = s["roff"] + (rva - s["va"])
    e = DATA.find(b"\0", o, o + limit)
    if e > o:
        b = DATA[o:e]
        if len(b) > 1 and all(32 <= c < 127 for c in b):
            return b.decode("ascii")
    return None


STORE_FI = {6, 7, 8, 22, 23}
m = ((TGT >= FI_LO) & (TGT < FI_HI) & np.isin(FRM, list(STORE_FI)))
idx = np.nonzero(m)[0]
print("store-refs into fieldinf:", len(idx))
byfunc = Counter()
for i in idx:
    fr = func_of(int(CODE[i]))
    if fr:
        byfunc[fr] += 1
print("distinct functions storing into fieldinf:", len(byfunc))
for (b, e), c in byfunc.most_common(25):
    print(f"   {b:08x}..{e:08x}  stores={c}")

# for the biggest one, dump strings referenced and immediates
print("\n=== detail of top functions ===")
for (b, e), c in byfunc.most_common(8):
    refs = [(int(CODE[i]), int(TGT[i])) for i in idx if func_of(int(CODE[i])) == (b, e)]
    strs = []
    for cr, t in refs:
        st = str_at_rva(t)
        if st:
            strs.append((cr, t, st))
    # all rip refs in this function
    lo = np.searchsorted(CODE, b, "left")
    hi = np.searchsorted(CODE, e, "left")
    allstr = []
    for i in range(lo, hi):
        t = int(TGT[i])
        st = str_at_rva(t)
        if st:
            allstr.append((int(CODE[i]), t, st))
    print(f"\n{ b:08x}..{e:08x} stores={c}")
    print("   store targets:", [hex(t) for _, t in refs][:12])
    print("   strings in fn:", [s for _, _, s in allstr][:20])
    # raw hex
    raw = DATA[img.rva2off(b):img.rva2off(b) + min(e - b, 256)]
    for k in range(0, len(raw), 16):
        print(f"     {b+k:08x}  " + " ".join(f"{x:02x}" for x in raw[k:k + 16]))
