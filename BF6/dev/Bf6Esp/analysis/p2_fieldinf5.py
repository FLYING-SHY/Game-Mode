"""Follow the fieldinf payload pointers into .data and the typeinfo section."""
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

fi = SEC["fieldinf"]
ti = SEC["typeinfo"]

farr = np.frombuffer(DATA[fi["roff"]:fi["roff"] + fi["rsize"]], dtype="<u4")
print("first 0x60 u32 of fieldinf (rva 0x%x):" % fi["va"])
for i in range(0x60):
    v = int(farr[i])
    s = ""
    if 0x140000000 <= v < 0x150000000:
        r = v - IB
        sec = img.sec_of(r)
        try:
            st = img.cstr_at_rva(r)
            if st and st.isprintable():
                s = f"  -> {sec} '{st[:40]}'"
        except Exception:
            s = f"  -> {sec}"
    print(f"  +0x{i*4:03x} (rva 0x{fi['va']+i*4:x}): {v:10d} 0x{v:08x}{s}")


def dump_rva(rva, n=0x40, label=""):
    o = img.rva2off(rva)
    print(f"\n[{label}] rva 0x{rva:x}")
    for k in range(0, n, 16):
        b = DATA[o + k:o + k + 16]
        u = np.frombuffer(b, dtype="<u4")
        q = np.frombuffer(b, dtype="<u8")
        print(f"   +0x{k:02x}  " + " ".join(f"{x:08x}" for x in u) + f"   q0=0x{q[0]:016x} q1=0x{q[1]:016x}")


# .data cell behind the most frequent payload
dump_rva(0x9FB5AD8, 0x30, "data cell 0x9FB5AD8")
q0 = int.from_bytes(DATA[img.rva2off(0x9FB5AD8):img.rva2off(0x9FB5AD8) + 8], "little")
print("cell.q0 = 0x%x -> section %s" % (q0, img.sec_of(q0 - IB) if q0 > IB else "?"))
if q0 > IB:
    dump_rva(q0 - IB, 0x50, "typeinfo node from cell.q0")
    dump_rva(q0 - IB + 0x30, 0x20, "node+0x30 (info cell ptr)")

# static typeinfo record for key 0xdbb23984
tblob = DATA[ti["roff"]:ti["roff"] + ti["rsize"]]
arr = np.frombuffer(tblob, dtype="<u4")
hits = np.nonzero(arr == 0x00080089)[0]
bases = hits - 1
for key in (0xdbb23984, 0x6021b344, 0xd0434e4e, 0x49FB5AD8):
    idx = np.nonzero(arr[bases] == key)[0]
    if idx.size:
        b = int(bases[idx[0]])
        print(f"\nkey 0x{key:x} -> typeinfo record u32-index {b} (rva 0x{ti['va'] + b*4:x})")
        dump_rva(ti["va"] + b * 4, 0x50, f"typeinfo record 0x{key:x}")
    else:
        print(f"\nkey 0x{key:x} -> not a typeinfo u32_0")
