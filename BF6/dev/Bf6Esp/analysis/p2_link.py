"""Link typeinfo records <-> .data type objects <-> fieldinf records.

Hypotheses tested
  H1: the .data object referenced by typeinfo record slot q6 has +0x00 pointing
      back at its own typeinfo record (self reference).
  H2: fieldinf records contain a VA into the typeinfo section = the owning type
      record; the number of fieldinf records pointing at a type record should
      equal that record's field-count field (u32[10] == 8 in the sampled runs).
  H3: the .data object has MEC-style {+0x08 u16 flags, +0x0A u16 size}.
"""
import json
import struct
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img, IMAGE_BASE  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
TI_VA = SEC["typeinfo"]["va"]
TI_VSZ = SEC["typeinfo"]["vsize"]
FI_VA = SEC["fieldinf"]["va"]


def off_of_rva(rva):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    size = max(s["rsize"], s["vsize"])
    if not (s["va"] <= rva < s["va"] + size):
        return None
    return s["roff"] + (rva - s["va"])


out = {}

# ---------------------------------------------------------------- typeinfo
ti = img.blob("typeinfo")
ti_u64 = np.frombuffer(ti, dtype="<u8")
ti_u32 = np.frombuffer(ti, dtype="<u4")
m = []
i = ti.find(b"\x89\x00\x08\x00")
while i != -1:
    m.append(i)
    i = ti.find(b"\x89\x00\x08\x00", i + 1)
rs = np.array([x - 4 for x in m], dtype=np.int64)
q = ti_u64[(rs[:, None] // 8) + np.arange(10)]
u32 = ti_u32[(rs[:, None] // 4) + np.arange(20)]
N = len(rs)
rec_rva = TI_VA + rs                      # rva of each record start
rec_va = rec_rva + IMAGE_BASE
rva2idx = {int(r): k for k, r in enumerate(rec_rva)}
print("typeinfo records:", N)

# H1: self reference through the q6 .data object
self_ref = 0
back = []
sizes = Counter()
flags = Counter()
for k in range(N):
    va = int(q[k, 6])
    if va == 0:
        continue
    o = off_of_rva(va - IMAGE_BASE)
    if o is None:
        continue
    p = int.from_bytes(DATA[o:o + 8], "little")
    if p in rva2idx:
        self_ref += 1
        if len(back) < 5:
            back.append({"rec": k, "obj_va": hex(va), "points_at_rec": rva2idx[p],
                         "same_record": rva2idx[p] == k})
    f = struct.unpack_from("<H", DATA, o + 8)[0]
    s = struct.unpack_from("<H", DATA, o + 10)[0]
    flags[f] += 1
    sizes[s] += 1
out["H1_self_ref"] = {"objects_checked": int((q[:, 6] != 0).sum()),
                      "self_referencing": self_ref, "examples": back}
out["H3_flags_at_+0x08"] = flags.most_common(12)
out["H3_size_at_+0x0A"] = sizes.most_common(20)
print("H1: objects with back-pointer into typeinfo:", self_ref, "of",
      int((q[:, 6] != 0).sum()))
print("H3 flags:", flags.most_common(6), "sizes:", sizes.most_common(8))

# does the back pointer equal the record itself?
same = 0
tot = 0
for k in range(N):
    va = int(q[k, 6])
    if va == 0:
        continue
    o = off_of_rva(va - IMAGE_BASE)
    if o is None:
        continue
    p = int.from_bytes(DATA[o:o + 8], "little")
    if p in rva2idx:
        tot += 1
        same += (rva2idx[p] == k)
out["H1_backpointer_is_own_record"] = {"checked": tot, "identical": same}

# ---------------------------------------------------------------- fieldinf
fi = img.blob("fieldinf")
fi_u64 = np.frombuffer(fi, dtype="<u8")
fi_u32 = np.frombuffer(fi, dtype="<u4")
link_stats = {}
for stride in (0x10, 0x18, 0x20, 0x28, 0x30):
    nrec = len(fi) // stride
    nslots = stride // 8
    base = np.arange(nrec, dtype=np.int64) * nslots
    per_slot = []
    for s in range(nslots):
        col = fi_u64[base + s]
        in_ti = np.flatnonzero((col >= TI_VA + IMAGE_BASE) &
                               (col < TI_VA + TI_VSZ + IMAGE_BASE))
        cnt = Counter()
        for r in in_ti:
            ridx = rva2idx.get(int(col[r]) - IMAGE_BASE)
            if ridx is not None:
                cnt[ridx] += 1
        per_slot.append({"slot": s, "recs_pointing_into_typeinfo": int(len(in_ti)),
                         "distinct_target_records": len(cnt),
                         "max_fields_per_record": cnt.most_common(1)[0][1] if cnt else 0})
    link_stats[f"0x{stride:x}"] = per_slot
    print("fieldinf stride", hex(stride), [(x["slot"], x["recs_pointing_into_typeinfo"],
                                            x["distinct_target_records"],
                                            x["max_fields_per_record"]) for x in per_slot])
out["fieldinf_typeinfo_links"] = link_stats

# H2: compare observed field counts with typeinfo u32[10]
best = None
for stride in (0x18, 0x20, 0x28, 0x30):
    nrec = len(fi) // stride
    nslots = stride // 8
    base = np.arange(nrec, dtype=np.int64) * nslots
    for s in range(nslots):
        col = fi_u64[base + s]
        mask = (col >= TI_VA + IMAGE_BASE) & (col < TI_VA + TI_VSZ + IMAGE_BASE)
        if mask.sum() < 100:
            continue
        cnt = Counter()
        for r in np.flatnonzero(mask):
            ridx = rva2idx.get(int(col[r]) - IMAGE_BASE)
            if ridx is not None:
                cnt[ridx] += 1
        if not cnt:
            continue
        agree = 0
        checked = 0
        for ridx, c in cnt.items():
            fc = int(u32[ridx, 10])
            checked += 1
            agree += (fc == c)
        rec = {"stride": hex(stride), "slot": s, "records_linked": len(cnt),
               "agree_with_u32_10": agree, "checked": checked,
               "examples": [{"rec": int(k), "fields": v, "u32_10": int(u32[k, 10]),
                             "u32_0": int(u32[k, 0]), "u32_11": int(u32[k, 11])}
                            for k, v in cnt.most_common(8)]}
        print("H2", hex(stride), "slot", s, "linked records", len(cnt),
              "agree", agree, "/", checked)
        if best is None or agree > best["agree_with_u32_10"]:
            best = rec
out["H2_fieldcount_agreement"] = best

path = HERE / "p2_link.json"
path.write_text(json.dumps(out, indent=1), encoding="utf-8")
print("wrote", path)
