"""Full annotated dump of a registration stub: hex + calls + rip refs."""
import json
import struct
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img, IMAGE_BASE  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
z = np.load(HERE / "p2_riprefs.npz")
code, tgt, form = z["code"], z["tgt"], z["form"]
pdata = img.blob(".pdata")
nfun = len(pdata) // 12
beg = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[0::3].astype(np.int64)
end = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[1::3].astype(np.int64)


def func_of(rva):
    i = np.searchsorted(beg, rva, side="right") - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


def str_at_rva(rva, limit=100):
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


fr = int(sys.argv[1], 16)
b, e = func_of(fr)
print(f"function {hex(b)}..{hex(e)}")
raw = DATA[img.rva2off(b):img.rva2off(b) + (e - b)]
for k in range(0, len(raw), 16):
    print(f"  {b+k:08x}  " + " ".join(f"{x:02x}" for x in raw[k:k + 16]))

print("\n-- calls (E8 rel32) --")
i = 0
while True:
    i = raw.find(b"\xe8", i)
    if i < 0 or i >= len(raw) - 5:
        break
    rel = struct.unpack_from("<i", raw, i + 1)[0]
    t = b + i + 5 + rel
    print(f"   {b+i:08x}  call {hex(t)}  ({img.sec_of(t)})")
    i += 1

print("\n-- rip-relative refs --")
for c, f, t in zip(code, tgt, form):
    if b <= c < e:
        st = str_at_rva(int(t))
        print(f"   {c:08x} form={int(f):2d} -> {hex(int(t))} {img.sec_of(int(t))} {st if st else ''}")
