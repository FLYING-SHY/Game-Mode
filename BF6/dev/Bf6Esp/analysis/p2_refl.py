"""Dump the .data reflection region (0x9AF48C8 .. 0x9AF5C00) and the fieldinf
pointer targets (VA 0x149FB5B88 & friends)."""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000


def cstr_va(va, maxn=120):
    o = img.rva2off(va - IB)
    if o is None:
        return None
    e = DATA.find(b"\x00", o, o + maxn)
    if e < 0:
        return None
    s = DATA[o:e]
    try:
        t = s.decode("ascii")
    except Exception:
        return None
    return t if t.isprintable() else None


def sec_of(rva):
    for nm, s in SEC.items():
        if s["va"] <= rva < s["va"] + max(s["vsize"], s["rsize"]):
            return nm
    return None


print("=== .data region 0x9AF48C8 .. 0x9AF5C00 (pointer array) ===")
o = img.rva2off(0x9AF48C8)
for i in range(0, 0x1400, 8):
    q = int.from_bytes(DATA[o + i:o + i + 8], "little")
    if q == 0:
        continue
    if q > 0x140000000:
        s = cstr_va(q)
        nm = sec_of(q - IB)
        print(f"  +0x{i:04x} (rva 0x{0x9AF48C8 + i:x}) -> {q:016x} {nm}"
              f"{(' str=' + s[:60]) if s else ''}")
    else:
        print(f"  +0x{i:04x} (rva 0x{0x9AF48C8 + i:x}) = {q:016x} (raw)")

print("\n=== cell at rva 0x9FB5B88 (the most common fieldinf value) ===")
o2 = img.rva2off(0x9FB5B88)
print("  hex:", DATA[o2:o2 + 0x100].hex())
q = np.frombuffer(DATA[o2:o2 + 0x100], dtype="<u8")
u = np.frombuffer(DATA[o2:o2 + 0x100], dtype="<u4")
print("  u64:", [f"{x:016x}" for x in q[:8]])
print("  u32:", [f"{x:08x}" for x in u[:16]])
for j, x in enumerate(q[:8]):
    if 0x140000000 < x < 0x150000000:
        s = cstr_va(int(x))
        print(f"    q{j} -> rva 0x{int(x) - IB:x} {sec_of(int(x) - IB)}"
              f"{(' str=' + s[:60]) if s else ''}")

print("\n=== .data 0x9AF49C0 region raw (RIFXRIFX area) ===")
o3 = img.rva2off(0x9AF49C0)
print(DATA[o3:o3 + 0x80].hex())
print(DATA[o3:o3 + 0x80])
