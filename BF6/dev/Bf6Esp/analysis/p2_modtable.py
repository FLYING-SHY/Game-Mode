"""Decode the RIFXRIFX module registration table in .data."""
import json
import struct
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000

hits = []
start = 0
while True:
    i = DATA.find(b"RIFXRIFX", start)
    if i < 0:
        break
    hits.append(i)
    start = i + 1
print("RIFXRIFX occurrences:", [hex(h) for h in hits])

for h in hits[:3]:
    rva = img.off2rva(h)
    print(f"\n=== file offset 0x{h:x} (rva 0x{rva:x}) ===")
    base = h - 0x40
    for k in range(0, 0x100, 8):
        o = base + k
        q = int.from_bytes(DATA[o:o + 8], "little")
        desc = ""
        if q:
            qr = q - IB if q >= IB else q
            for name, s in SEC.items():
                if s["va"] <= qr < s["va"] + s["rsize"]:
                    desc = f"[{name}]"
                    if name == ".rdata":
                        st = img.cstr_at_rva(qr)
                        if st:
                            desc += f' "{st}"'
                    break
        print(f"  +0x{k:03x} rva 0x{img.off2rva(o):x}: 0x{q:016x} {desc}")
