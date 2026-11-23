"""Hex-dump the .rdata reflection name pool around known member names."""
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
data = img.data
IB = 0x140000000

names = [b"HealthComponent", b"ClientPlayerManager", b"TeamId", b"SquadId",
         b"PersonaId", b"SlotIndex", b"ReturnValue", b"LocalPlayer", b"Health"]
for n in names:
    idx = data.find(n)
    print(f"\n=== {n.decode()} first occurrence at file 0x{idx:x} (rva 0x{img.off2rva(idx):x}) ===")
    lo = idx - 0x40
    for k in range(0, 0xA0, 16):
        o = lo + k
        row = data[o:o + 16]
        asc = "".join(chr(c) if 32 <= c < 127 else "." for c in row)
        print(f"  {img.off2rva(o):08x}  {row.hex(' ')}  |{asc}|")
