"""Decode one BIG registration stub in absolute terms and resolve every value."""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402
from p2_rec import decode_stub  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000


def sec_of(rva):
    for nm, s in SEC.items():
        if s["va"] <= rva < s["va"] + max(s["vsize"], s["rsize"]):
            return nm
    return None


def cstr_va(va):
    o = img.rva2off(va - IB)
    if o is None:
        return None
    e = DATA.find(b"\x00", o, o + 160)
    if e < 0:
        return None
    s = DATA[o:e]
    try:
        t = s.decode("ascii")
    except Exception:
        return None
    return t if t.isprintable() else None


for fn_hex in ("68140", "6c2c0", "67bc0"):
    fn = int(fn_hex, 16)
    r = decode_stub(fn)
    if not r:
        print(f"stub {fn_hex}: decode failed")
        continue
    b, e, slots = r
    print(f"\n=== stub 0x{fn_hex}  (func range {b:x}-{e:x}) ===")
    for k in sorted(slots):
        v = slots[k]
        if not isinstance(v, int):
            continue
        rva = v - IB if v > 0xFFFF else None
        nm = sec_of(rva) if rva is not None else "imm"
        extra = ""
        if nm in (".rdata", ".data"):
            s = cstr_va(v)
            if s:
                extra = f' -> "{s[:60]}"'
        print(f"   store @0x{k:08x} (rva 0x{k - IB:08x}) = {v:016x} {nm}{extra}")

    # dump any .data object stored
    for k in sorted(slots):
        v = slots[k]
        if not isinstance(v, int) or v < 0xFFFF:
            continue
        rva = v - IB
        if sec_of(rva) == ".data":
            o = img.rva2off(rva)
            print(f"   --- .data object @0x{v:x} (rva 0x{rva:x}) 96 bytes ---")
            print("      ", DATA[o:o + 96].hex())
            q = np.frombuffer(DATA[o:o + 96], dtype="<u8")
            u = np.frombuffer(DATA[o:o + 96], dtype="<u4")
            print("      u64:", [f"{x:016x}" for x in q])
            print("      u32:", [f"{x:08x}" for x in u])
            for j, x in enumerate(q):
                if 0x140000000 < x < 0x150000000:
                    s2 = cstr_va(int(x))
                    print(f"        q{j} -> rva 0x{int(x) - IB:x} {sec_of(int(x) - IB)}"
                          f"{(' str=' + s2[:40]) if s2 else ''}")
