"""Decode the per-type object that typeinfo record slot q6 points at.

MEC layout (refs/docs__research__05-type-info-sdk.md):
  info +0x00 name, +0x08 u16 flags, +0x0A u16 size, +0x10 module,
  +0x18 u8 align, +0x1A u16 fieldCount, +0x20 super*, +0x30 FieldInfo[]
"""
import json
import struct
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img, IMAGE_BASE  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec


def off_of_rva(rva):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    size = max(s["rsize"], s["vsize"])
    if not (s["va"] <= rva < s["va"] + size):
        return None
    return s["roff"] + (rva - s["va"])


def str_at_rva(rva, limit=80):
    o = off_of_rva(rva)
    if o is None:
        return None
    end = DATA.find(b"\0", o, o + limit)
    if end > o:
        b = DATA[o:end]
        if all(32 <= c < 127 for c in b):
            return b.decode("ascii")
    return None


def is_va(v):
    return IMAGE_BASE <= v < IMAGE_BASE + 0x10000000


ti = img.blob("typeinfo")
ti_u64 = np.frombuffer(ti, dtype="<u8")
m = []
i = ti.find(b"\x89\x00\x08\x00")
while i != -1:
    m.append(i)
    i = ti.find(b"\x89\x00\x08\x00", i + 1)
rs = np.array([x - 4 for x in m], dtype=np.int64)
q = ti_u64[(rs[:, None] // 8) + np.arange(10)]
N = len(rs)

nonzero_q6 = q[:, 6] != 0
print("records:", N, "with q6:", int(nonzero_q6.sum()))
print("with q3:", int((q[:, 3] != 0).sum()))
print("with both q3&q6:", int(((q[:, 3] != 0) & (q[:, 6] != 0)).sum()))

out = {"records": int(N), "with_q3": int((q[:, 3] != 0).sum()),
       "with_q6": int(nonzero_q6.sum()),
       "with_q3_and_q6": int(((q[:, 3] != 0) & (q[:, 6] != 0)).sum())}

# index of the first record with q6 != 0
nz = np.flatnonzero(nonzero_q6)
if len(nz):
    out["first_record_with_q6"] = int(nz[0])
    out["last_record_with_q6"] = int(nz[-1])

# ---- dump q6 objects --------------------------------------------------
samples = []
targets = q[nz, 6]
uniq = np.unique(targets)
print("distinct q6 targets:", len(uniq))
for t in uniq[:12]:
    o = off_of_rva(int(t) - IMAGE_BASE)
    if o is None:
        continue
    raw = DATA[o:o + 0x60]
    words = struct.unpack_from("<12Q", raw)
    words16 = struct.unpack_from("<24H", raw)
    desc = []
    for k, w in enumerate(words):
        if is_va(w):
            desc.append(f"q{k}=VA->{img.sec_of(w - IMAGE_BASE)}:{str_at_rva(w - IMAGE_BASE)}")
        else:
            desc.append(f"q{k}=0x{w:x}")
    samples.append({"va": hex(int(t)), "rva": hex(int(t) - IMAGE_BASE),
                    "words": desc,
                    "u16": [f"0x{x:04x}" for x in words16]})
    print("---", hex(int(t)))
    print("  ", " | ".join(desc))
    print("   u16:", " ".join(f"{x:04x}" for x in words16))
out["q6_object_samples"] = samples

# ---- aggregate: for every q6 target, find which qword index holds a string
cnt = Counter()
n = 0
for t in uniq[:3000]:
    o = off_of_rva(int(t) - IMAGE_BASE)
    if o is None:
        continue
    n += 1
    for k in range(12):
        w = int.from_bytes(DATA[o + 8 * k:o + 8 * k + 8], "little")
        if is_va(w):
            s = str_at_rva(w - IMAGE_BASE)
            if s and len(s) >= 2 and s[0].isalpha():
                cnt[k] += 1
print("string-at-qindex histogram over", n, "objects:", dict(cnt.most_common()))
out["q6_string_qindex_hist"] = {str(k): v for k, v in cnt.most_common()}

# ---- same for q3 (module) objects
cnt3 = Counter()
mods = Counter()
for t in np.unique(q[q[:, 3] != 0, 3])[:400]:
    o = off_of_rva(int(t) - IMAGE_BASE)
    if o is None:
        continue
    for k in range(12):
        w = int.from_bytes(DATA[o + 8 * k:o + 8 * k + 8], "little")
        if is_va(w):
            s = str_at_rva(w - IMAGE_BASE)
            if s and len(s) >= 2 and s[0].isalpha():
                cnt3[k] += 1
                if k == 0:
                    mods[s] += 1
print("q3 string-at-qindex:", dict(cnt3.most_common()))
print("q3 module names (top):", mods.most_common(10))
out["q3_string_qindex_hist"] = {str(k): v for k, v in cnt3.most_common()}
out["q3_module_names_top"] = mods.most_common(20)

# ---- what is q6's section?
secs = Counter(img.sec_of(int(t) - IMAGE_BASE) for t in uniq[:2000])
out["q6_target_sections"] = dict(secs)
print("q6 target sections:", secs.most_common())

path = HERE / "p2_obj.json"
path.write_text(json.dumps(out, indent=1), encoding="utf-8")
print("wrote", path)
