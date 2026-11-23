"""ANCHOR EXPERIMENT v2: link runtime type NAMES to static typeinfo records.

Registration stub -> runtime record (in typeinfo section) -> type-info .data object
-> its q0 points at the static record base.  Then u32[0] of that static record is
the name hash candidate.  Produces (name, hash) anchor pairs for hash cracking.
"""
import json
import sys
from collections import Counter
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
TI = SEC["typeinfo"]
TI_VA, TI_OFF, TI_SIZE = TI["va"], TI["roff"], TI["rsize"]
ti = DATA[TI_OFF:TI_OFF + TI_SIZE]

# static record table: marker 89 00 08 00 at base+4
marks = []
s = 0
while True:
    p = ti.find(b"\x89\x00\x08\x00", s)
    if p < 0:
        break
    marks.append(p - 4)
    s = p + 1
bases = np.array(sorted(set(marks)), dtype=np.int64)
print("static records (kind 0x00080089):", len(bases))


def qstr(va):
    try:
        o = img.rva2off(va - IB)
    except Exception:
        return None
    if o is None:
        return None
    e = DATA.find(b"\x00", o, o + 128)
    if e < 0:
        return None
    return DATA[o:e].decode("latin1")


reg = json.loads((HERE / "p2_reg.json").read_text(encoding="utf-8"))
stubs = reg["type_stubs"]
print("stubs in file:", len(stubs))

pairs = []
delta_hist = Counter()
mod_ok = mod_bad = 0
for st in stubs:
    fn = int(st["func"], 16)
    r = decode_stub(fn)
    if not r:
        continue
    b, e, slots = r
    base = min(slots)
    mod_ptr = slots.get(base + 0x10)
    inf_ptr = slots.get(base + 0x18)
    if not isinstance(mod_ptr, int) or not isinstance(inf_ptr, int):
        continue
    try:
        mo = img.rva2off(mod_ptr)
        io = img.rva2off(inf_ptr)
    except Exception:
        continue
    if mo is None or io is None:
        continue
    mod_name_va = int.from_bytes(DATA[mo:mo + 8], "little")
    mn = qstr(mod_name_va)
    uniq = list(dict.fromkeys(st["strings"]))
    if mn and mn in uniq:
        mod_ok += 1
        uniq = [x for x in uniq if x != mn]
    else:
        mod_bad += 1
    if not uniq:
        continue
    type_name = uniq[0]
    q0 = int.from_bytes(DATA[io:io + 8], "little")
    q0_rva = q0 - IB
    if not (TI_VA <= q0_rva < TI_VA + TI_SIZE):
        continue
    off = q0_rva - TI_VA
    i = int(np.searchsorted(bases, off, "right")) - 1
    if i < 0:
        continue
    delta_hist[off - int(bases[i])] += 1
    nb = int(bases[i])
    u = np.frombuffer(ti[nb:nb + 0x50], dtype="<u4")
    pairs.append({"type": type_name, "module": mn, "fn": b,
                  "rec_off": nb, "delta": off - nb,
                  "hash": int(u[0]), "kind": int(u[1]),
                  "u32": [int(x) for x in u]})

print(f"module name resolved ok={mod_ok} bad={mod_bad}")
print("delta (info_target - static_record_base):", delta_hist.most_common(10))
print("anchor pairs:", len(pairs))
seen = set()
for p in pairs:
    if p["type"] in seen:
        continue
    seen.add(p["type"])
    print(f"   {p['type']:<45} mod={str(p['module'])[:18]:<18} "
          f"hash=0x{p['hash']:08x} kind=0x{p['kind']:08x} d={p['delta']} "
          f"u32[2..5]={p['u32'][2:6]}")
    if len(seen) > 25:
        break
(HERE / "p2_anchors.json").write_text(json.dumps(pairs, indent=1), encoding="utf-8")
print("wrote p2_anchors.json")
