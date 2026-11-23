"""Resolve typeinfo/fieldinf VA slots to strings (and look for name+offset pairs).

For every typeinfo record we resolve each 64-bit slot that points into the image
and, for the pointed-to object, try to resolve *its* qwords to printable strings
in .rdata.  The goal: find a slot that reliably yields a TYPE NAME.

For fieldinf we walk every candidate record stride and, for each record, for each
slot, do the same, and additionally look for a plausible member OFFSET (u32/u16)
in the same object.
"""
import json
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


def rva_of(va):
    return va - IMAGE_BASE


def off_of_rva(rva):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    size = max(s["rsize"], s["vsize"])
    if not (s["va"] <= rva < s["va"] + size):
        return None
    return s["roff"] + (rva - s["va"])


_str_cache = {}


def str_at_rva(rva, limit=80):
    if rva in _str_cache:
        return _str_cache[rva]
    o = off_of_rva(rva)
    res = None
    if o is not None:
        end = DATA.find(b"\0", o, o + limit)
        if end > o:
            b = DATA[o:end]
            if all(32 <= c < 127 for c in b):
                res = b.decode("ascii")
    _str_cache[rva] = res
    return res


def is_va(v):
    return IMAGE_BASE <= v < IMAGE_BASE + 0x10000000


def read_qwords(va, n=8):
    o = off_of_rva(va - IMAGE_BASE)
    if o is None:
        return None
    return [int.from_bytes(DATA[o + 8 * i:o + 8 * i + 8], "little") for i in range(n)]


out = {}

# ------------------------------------------------------------------ typeinfo
ti = img.blob("typeinfo")
ti_u64 = np.frombuffer(ti, dtype="<u8")
ti_u32 = np.frombuffer(ti, dtype="<u4")
m_A = []
i = ti.find(b"\x89\x00\x08\x00")
while i != -1:
    m_A.append(i)
    i = ti.find(b"\x89\x00\x08\x00", i + 1)
rec_starts = np.array([m - 4 for m in m_A], dtype=np.int64)
q0 = ti_u64[rec_starts // 8]
N = len(rec_starts)

slot_names = ["q0", "q1", "q2", "q3", "q4", "q5", "q6", "q7", "q8", "q9"]
per_slot = {}
for s in range(10):
    col = ti_u64[rec_starts // 8 + s]
    va_mask = (col >= IMAGE_BASE) & (col < IMAGE_BASE + 0x10000000)
    n_va = int(va_mask.sum())
    # resolve the VA targets: what does the pointed-to object look like?
    name_hits = Counter()
    off_hits = Counter()
    samples = []
    idxs = np.flatnonzero(va_mask)
    for r in idxs[:4000]:
        va = int(col[r])
        qs = read_qwords(va, 8)
        if qs is None:
            continue
        for k, q in enumerate(qs):
            if is_va(q):
                s2 = str_at_rva(rva_of(q))
                if s2 and len(s2) >= 2 and s2[0].isalpha():
                    name_hits[k] += 1
                    if len(samples) < 6 and s in (3, 6, 7, 8):
                        samples.append({
                            "rec": int(r), "va": hex(va), "qidx": k,
                            "name": s2, "name_rva": hex(rva_of(q)),
                        })
    per_slot[slot_names[s]] = {
        "records_with_va": n_va,
        "name_hits_per_qindex": {str(k): v for k, v in name_hits.most_common(8)},
        "samples": samples,
    }
    print(slot_names[s], "va:", n_va, "namehits:", dict(name_hits.most_common(4)))
out["typeinfo_slot_resolution"] = per_slot

# sample records with names
detail = []
for r in range(min(N, 3000)):
    row = {"rec": int(r), "rva": hex(img.sec["typeinfo"]["va"] + rec_starts[r]),
           "u32": [int(x) for x in ti_u32[rec_starts[r] // 4: rec_starts[r] // 4 + 20]]}
    for s in (3, 6, 7, 8):
        va = int(ti_u64[rec_starts[r] // 8 + s])
        if is_va(va):
            qs = read_qwords(va, 8)
            row[f"q{s}"] = {"va": hex(va), "strings": [
                (str_at_rva(rva_of(q)) or ("?" if is_va(q) else hex(q)))
                for q in qs]}
    detail.append(row)
out["typeinfo_record_samples"] = detail[:60]

# ------------------------------------------------------------------ fieldinf
fi = img.blob("fieldinf")
fi_u64 = np.frombuffer(fi, dtype="<u8")
fi_u32 = np.frombuffer(fi, dtype="<u4")
fi_res = {}
for stride in (0x18, 0x20, 0x28, 0x30, 0x38):
    nrec = len(fi) // stride
    base = np.arange(nrec, dtype=np.int64) * (stride // 8)
    nslots = stride // 8
    slot_info = []
    for s in range(nslots):
        col = fi_u64[base + s]
        va_mask = (col >= IMAGE_BASE) & (col < IMAGE_BASE + 0x10000000)
        name_at = Counter()
        off_at = Counter()
        idxs = np.flatnonzero(va_mask)
        for r in idxs[:1500]:
            qs = read_qwords(int(col[r]), 8)
            if qs is None:
                continue
            for k, q in enumerate(qs):
                if is_va(q):
                    st = str_at_rva(rva_of(q))
                    if st and len(st) >= 2 and st[0].isalpha():
                        name_at[k] += 1
                elif 0 < q < 0x4000 and (q % 4 == 0):
                    off_at[k] += 1
        slot_info.append({
            "slot": s, "va_recs": int(va_mask.sum()),
            "string_qidx": {str(k): v for k, v in name_at.most_common(6)},
            "smallint_qidx": {str(k): v for k, v in off_at.most_common(6)},
        })
    fi_res[f"0x{stride:x}"] = slot_info
    print("fieldinf stride", hex(stride), [ (x["slot"], x["va_recs"], x["string_qidx"]) for x in slot_info ])
out["fieldinf_slot_resolution"] = fi_res

# fieldinf sample records at stride 0x30 with resolution
nrec = len(fi) // 0x30
base = np.arange(min(nrec, 4000), dtype=np.int64) * 6
samp = []
for r in range(min(nrec, 40)):
    row = {"r": int(r), "u32": [int(x) for x in fi_u32[base[r] * 1:base[r] * 1 + 12]]}
    for s in range(6):
        va = int(fi_u64[base[r] + s])
        if is_va(va):
            qs = read_qwords(va, 8)
            row[f"q{s}"] = {"va": hex(va), "strings": [
                (str_at_rva(rva_of(q)) or ("?" if is_va(q) else hex(q))) for q in qs]}
    samp.append(row)
out["fieldinf_samples_stride30"] = samp

path = HERE / "p2_resolve.json"
path.write_text(json.dumps(out, indent=1), encoding="utf-8")
print("wrote", path)
