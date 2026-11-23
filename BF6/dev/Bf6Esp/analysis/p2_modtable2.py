"""Dump the .data module registration region 0x9AF48C8..0x9AF5C00."""
import json
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000

lo, hi = 0x9AF48C8, 0x9AF5C00
off = img.rva2off(lo)
print("file offset:", hex(off))


def describe(q):
    if not q:
        return "0"
    qr = q - IB if q >= IB else q
    for name, s in SEC.items():
        if s["va"] <= qr < s["va"] + s["rsize"]:
            extra = ""
            if name == ".rdata":
                st = img.cstr_at_rva(qr)
                if st and len(st) < 60:
                    extra = f' "{st}"'
            elif name == ".data":
                o = img.rva2off(qr)
                if o is not None:
                    nxt = int.from_bytes(DATA[o:o + 8], "little")
                    extra = f" ->0x{nxt:x}"
            return f"[{name}]{extra}"
    return ""


for k in range(0, hi - lo, 8):
    o = off + k
    q = int.from_bytes(DATA[o:o + 8], "little")
    rva = lo + k
    d = describe(q)
    extra = ""
    if not d:
        b = DATA[o:o + 8]
        if all(32 <= c < 127 for c in b):
            extra = f'  ascii "{b.decode()}"'
        else:
            extra = "  " + b.hex()
    print(f"0x{rva:08x}: 0x{q:016x} {d}{extra}")
