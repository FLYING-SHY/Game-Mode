"""Detailed dump of one or more typeinfo registration stubs (manual disassembly aid)."""
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
    if o is None:
        return None
    e = DATA.find(b"\0", o, o + limit)
    if e > o:
        b = DATA[o:e]
        if len(b) > 1 and all(32 <= c < 127 for c in b):
            return b.decode("ascii")
    return None


targets = [int(x, 16) for x in sys.argv[1:]] or [0x68140, 0x683f0, 0x68760]
out = []
for fr in targets:
    b, e = fr, func_of(fr)[1]
    print(f"===== function {hex(b)} .. {hex(e)}  size {e-b}")
    raw = DATA[img.rva2off(b):img.rva2off(b) + (e - b)]
    for k in range(0, len(raw), 16):
        chunk = raw[k:k + 16]
        print(f"  {b+k:08x}  " + " ".join(f"{x:02x}" for x in chunk))
    refs = []
    for c, t, f in zip(code, tgt, form):
        if b <= c < e:
            st = str_at_rva(int(t))
            refs.append({"code": hex(int(c)), "form": int(f), "target": hex(int(t)),
                         "section": img.sec_of(int(t)), "string": st})
    print("  refs:")
    for r in refs:
        print("   ", r)
    imms = []
    for r in refs:
        if r["form"] in (22, 23):
            doff = 3 if r["form"] == 22 else 2
            off = img.rva2off(int(r["code"], 16))
            imm = struct.unpack_from("<I", DATA, off + doff + 4)[0]
            imms.append({"code": r["code"], "target": r["target"], "imm32": hex(imm)})
    print("  stored immediates:", imms)
    out.append({"func": hex(b), "end": hex(e), "refs": refs, "imms": imms,
                "hex": raw.hex()})
(HERE / "p2_fn_dump.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("wrote p2_fn_dump.json")
