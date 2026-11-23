"""Dump a handful of functions that reference member-name strings but are NOT
type-registration stubs, to see what they actually do."""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
data = img.data
sec = img.sec

pd = sec[".pdata"]
b = data[pd["roff"]:pd["roff"] + pd["rsize"]]
n = len(b) // 12
arr = np.frombuffer(b[:n * 12], dtype="<u4").reshape(n, 3)
beg = arr[:, 0].astype(np.int64)
end = arr[:, 1].astype(np.int64)
keep = (beg > 0) & (end > beg)
beg, end = beg[keep], end[keep]

TARGETS = [0xc7e9f0, 0xc7f4a0, 0x10c7420, 0x16b0110, 0x118e600, 0x1109060, 0x4d08b0, 0x92eb4b]


def cstr(rva):
    o = img.rva2off(rva)
    e = data.index(b"\x00", o)
    return data[o:e].decode("ascii", "replace")


z = np.load(HERE / "p2_riprefs.npz")
CODE, TGT, FORM = z["code"], z["tgt"], z["form"]
rd = sec[".rdata"]
rd_lo, rd_hi = rd["va"], rd["va"] + rd["rsize"]
ti = sec["typeinfo"]
ti_lo, ti_hi = ti["va"], ti["va"] + ti["rsize"]

for t in TARGETS:
    i = int(np.searchsorted(beg, t, side="right")) - 1
    fb = int(beg[i]) if i >= 0 else t
    fe = int(end[i]) if i >= 0 else t + 64
    off = img.rva2off(fb)
    raw = data[off:off + min(fe - fb, 220)]
    print(f"\n=== func 0x{fb:x}..0x{fe:x} (len {fe-fb}) ===")
    print("  bytes:", raw[:160].hex(" "))
    # refs inside this function
    lo = int(np.searchsorted(CODE, fb))
    hi = int(np.searchsorted(CODE, fe))
    for k in range(lo, hi):
        tg = int(TGT[k])
        tag = ""
        if rd_lo <= tg < rd_hi:
            try:
                tag = f"-> .rdata '{cstr(tg)[:48]}'"
            except Exception:
                tag = "-> .rdata"
        elif ti_lo <= tg < ti_hi:
            tag = f"-> typeinfo+0x{tg-ti_lo:x}"
        else:
            s = img.sec_of(tg)
            tag = f"-> {s} +0x{tg - img.sec[s]['va']:x}" if s else f"-> 0x{tg:x}"
        print(f"    0x{int(CODE[k]):x} form={int(FORM[k])} {tag}")
