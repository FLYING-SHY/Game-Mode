"""fieldinf layout: per-slot classification for each stride candidate."""
import json
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
TI = SEC["typeinfo"]
fi = DATA[SEC["fieldinf"]["roff"]:SEC["fieldinf"]["roff"] + SEC["fieldinf"]["rsize"]]
ti = DATA[TI["roff"]:TI["roff"] + TI["rsize"]]

# typeinfo static records (0x50 stride, marker at base+4)
marks = []
s = 0
while True:
    p = ti.find(b"\x89\x00\x08\x00", s)
    if p < 0:
        break
    marks.append(p - 4)
    s = p + 1
bases = np.array(sorted(set(marks)), dtype=np.int64)
u32_by_rec = np.array([np.frombuffer(ti[b:b + 0x50], dtype="<u4") for b in bases])
hash2_set = set(u32_by_rec[:, 11].tolist())
hash0_set = set(u32_by_rec[:, 0].tolist())
print("typeinfo records:", len(bases), "hash2 distinct:", len(hash2_set))

STRIDES = [0x10, 0x18, 0x20, 0x28, 0x30, 0x38, 0x40, 0x48]
out = {}
for st in STRIDES:
    n = len(fi) // st
    arr = np.frombuffer(fi[: n * st], dtype="<u4").reshape(n, st // 4)
    slots = []
    for j in range(st // 4):
        col = arr[:, j]
        nz = col[col != 0]
        s2 = set(col.tolist())
        inter2 = len(s2 & hash2_set)
        inter0 = len(s2 & hash0_set)
        u16v = col & 0xFFFF
        u16hi = (col >> 16) & 0xFFFF
        slots.append({
            "slot": j,
            "distinct": int(len(s2)),
            "zero_frac": round(float((col == 0).mean()), 4),
            "in_hash2_domain": inter2,
            "in_hash0_domain": inter0,
            "u16lo_max": int(u16v.max()) if len(u16v) else 0,
            "u16hi_max": int(u16hi.max()) if len(u16hi) else 0,
            "u16lo_small_frac": round(float((u16v <= 0x1000).mean()), 4),
            "u16hi_small_frac": round(float((u16hi <= 0x1000).mean()), 4),
            "top": [[int(v), int(c)] for v, c in Counter(col.tolist()).most_common(5)],
        })
    out[hex(st)] = {"records": n, "slots": slots}
    print(f"\n=== stride {hex(st)} ({n} records) ===")
    for s2 in slots:
        print(f"  slot{s2['slot']}: distinct={s2['distinct']:6d} zero%={s2['zero_frac']:.3f} "
              f"hash2dom={s2['in_hash2_domain']:6d} hash0dom={s2['in_hash0_domain']:6d} "
              f"u16lo<0x1000={s2['u16lo_small_frac']:.3f} u16hi<0x1000={s2['u16hi_small_frac']:.3f} "
              f"top={[[hex(v),c] for v,c in s2['top']]}")

(HERE / "p2_fieldinf_slots.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("\nwrote p2_fieldinf_slots.json")
