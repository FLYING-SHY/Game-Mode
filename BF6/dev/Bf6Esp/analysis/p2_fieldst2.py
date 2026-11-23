"""Dump raw store sites into fieldinf (no pdata needed)."""
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

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


def str_at_rva(rva, limit=60):
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
print("total store refs into fieldinf:", len(idx))
codes = CODE[idx]
print("code rva range:", hex(int(codes.min())), hex(int(codes.max())))
print("in .text?", (codes >= SEC[".text"]["va"]).sum(), "in ctr?", (codes >= SEC["ctr"]["va"]).sum())
# nearest function
for i in idx[:30]:
    c = int(CODE[i]); t = int(TGT[i])
    j = np.searchsorted(beg, c, "right") - 1
    fb = int(beg[j]) if j >= 0 else -1
    fe = int(end[j]) if j >= 0 else -1
    o = img.rva2off(c)
    print(f"  code {c:08x} (fn {fb:08x}..{fe:08x} in_pdata={fb<=c<fe}) -> {t:08x} "
          f"off_in_sec=0x{t - FI_LO:x}  bytes {DATA[o:o+8].hex()}")
