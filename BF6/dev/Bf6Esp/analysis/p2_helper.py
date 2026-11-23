"""Raw analysis of helper 0x50B0A20 (builds the runtime type node header)."""
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec

FN = 0x50B0A20
o = img.rva2off(FN)
raw = DATA[o:o + 0x180]
for k in range(0, len(raw), 16):
    print(f"  {FN + k:08x}  " + " ".join(f"{x:02x}" for x in raw[k:k + 16]))

print("\n-- stores of rdx (type name) / r8 (module name) inside the helper --")
for i in range(len(raw) - 3):
    w = raw[i:i + 4]
    if w[:3] == b"\x48\x89\xd0" or w[:3] == b"\x48\x89\xd1":
        pass
    # mov [reg+disp], rdx  -> 48 89 42 disp | 48 89 52 disp
    if raw[i] == 0x48 and raw[i + 1] == 0x89:
        modrm = raw[i + 2]
        reg = (modrm >> 3) & 7
        if reg == 2:  # rdx
            print(f"   {FN + i:08x}: 48 89 {modrm:02x} -> mov [...], rdx")
    if raw[i] == 0x4C and raw[i + 1] == 0x89:
        modrm = raw[i + 2]
        reg = (modrm >> 3) & 7
        if reg == 0:  # r8
            print(f"   {FN + i:08x}: 4c 89 {modrm:02x} -> mov [...], r8")
    if raw[i] == 0x48 and raw[i + 1] == 0x89 and ((raw[i + 2] >> 3) & 7) == 0:
        print(f"   {FN + i:08x}: 48 89 {raw[i+2]:02x} -> mov [...], rax")
