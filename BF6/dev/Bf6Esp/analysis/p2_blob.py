"""Inspect the .rdata reflection-name neighbourhood: are names stored with offsets?"""
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
rd = SEC[".rdata"]


def dump(rva, n=512, label=""):
    o = img.rva2off(rva)
    b = DATA[o:o + n]
    print(f"\n===== {label} rva 0x{rva:x} (file 0x{o:x}) =====")
    # print as strings + hex
    for m in re.finditer(rb"[ -~]{3,}", b):
        print(f"   +0x{m.start():04x}  {m.group().decode('ascii', 'replace')[:110]}")
    print("   hex:")
    for k in range(0, min(n, 256), 16):
        print(f"     {rva+k:08x}  " + " ".join(f"{x:02x}" for x in b[k:k + 16]))


# brief says the reflection NAME blob starts at file 0x8DDA7B2 (VA 0x148DDC1B2)
for rva, lbl in [(0x8DDA600, "around HealthComponent(0x8ddc01f) / ClientPlayerManager"),
                 (0x90A3A00, "around ClientPlayerManager/ClientConnection"),
                 (0x9213900, "around ClientSoldierEntity"),
                 (0x8DD5E00, "around Health(0x8dd5e1f)")]:
    dump(rva, 384, lbl)
