"""M2: typeinfo/fieldinf record layout analysis (lead's spec).

Outputs p2_layout2.json with:
  * marker byte-order verification (raw hex of record heads)
  * per-field u32/u64 value distributions over ALL typeinfo records
  * run/table segmentation
  * MEC-style field-role hypotheses (size/align/fieldCount/super)
  * fieldinf stride scoring (0x10..0x40) incl. u16-offset hypothesis
  * typeHash cross-link check (typeinfo u32[0] values vs fieldinf u32s)
"""
import json
import re
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
out = {}

# ---------------------------------------------------------------- markers
ti = img.blob("typeinfo")
fi = img.blob("fieldinf")
rd = img.blob(".rdata")


def all_offsets(blob, pat):
    res = []
    i = blob.find(pat)
    while i != -1:
        res.append(i)
        i = blob.find(pat, i + 1)
    return np.array(res, dtype=np.int64)


m_A = all_offsets(ti, b"\x89\x00\x08\x00")   # u32 0x00080089
m_B = all_offsets(ti, b"\x89\x00\x00\x08")   # u32 0x08000089
out["markers"] = {
    "typeinfo_count_89_00_08_00": int(len(m_A)),
    "typeinfo_count_89_00_00_08": int(len(m_B)),
    "fieldinf_count_89_00_08_00": int(len(all_offsets(fi, b"\x89\x00\x08\x00"))),
    "fieldinf_count_89_00_00_08": int(len(all_offsets(fi, b"\x89\x00\x00\x08"))),
}

REC = 0x50
rec_starts = m_A - 4                      # record starts 4 bytes before marker
out["markers"]["first3_record_heads_hex"] = [
    ti[s:s + 0x50].hex() for s in rec_starts[:3]]
out["markers"]["section_head_0x60_hex"] = ti[:0x60].hex()
out["markers"]["record_start_mod_0x50_hist"] = {
    str(k): int(v) for k, v in Counter((rec_starts % REC).tolist()).most_common(12)}

# ---------------------------------------------------------------- runs
runs = []
if len(m_A):
    breaks = np.flatnonzero(np.diff(m_A) != REC) + 1
    starts = np.concatenate(([0], breaks))
    ends = np.concatenate((breaks, [len(m_A)]))
    for s, e in zip(starts, ends):
        runs.append({"first_marker_off": int(m_A[s]), "count": int(e - s),
                     "marker_off_mod_0x50": int(m_A[s] % REC),
                     "record_start_rva": int(img.sec["typeinfo"]["va"] + m_A[s] - 4)})
runs.sort(key=lambda r: -r["count"])
out["typeinfo_runs_top20"] = runs[:20]
out["typeinfo_runs_total"] = len(runs)
out["typeinfo_records_total"] = int(len(m_A))

# ---------------------------------------------------------------- field stats
N = len(rec_starts)
ti_u8 = np.frombuffer(ti, dtype=np.uint8)
u32 = np.frombuffer(ti, dtype="<u4")
# field i of record r lives at u32 index (rec_starts[r])/4 + i
idx0 = (rec_starts // 4).astype(np.int64)
u64 = np.frombuffer(ti, dtype="<u8")
idx0_8 = (rec_starts // 8).astype(np.int64)

SECS = {s["name"]: (s["va"], s["va"] + s["vsize"]) for s in img.secs}


def sec_of_rva(v):
    for k, (a, b) in SECS.items():
        if a <= v < b:
            return k
    return None


def field_stats_u32(fidx):
    col = u32[idx0 + fidx]
    cnt = Counter(col.tolist())
    top = cnt.most_common(6)
    zero = int((col == 0).sum())
    small = int(((col > 0) & (col < 0x1000)).sum())
    vas = Counter()
    for v in cnt:
        s = sec_of_rva(int(v))
        if s:
            vas[s] += 1
    return {
        "distinct": int(len(cnt)),
        "zero_frac": round(zero / N, 4),
        "small_lt_0x1000_frac": round(small / N, 4),
        "min": int(col.min()), "max": int(col.max()),
        "top_values": [{"v": int(v), "n": int(c)} for v, c in top],
        "va_slots_by_section": dict(vas),
        "distinct_va_values": int(sum(vas.values())),
    }


out["typeinfo_u32_fields"] = {f"f{i:02d}@+{4*i}": field_stats_u32(i) for i in range(20)}


def field_stats_u64(fidx):
    col = u64[idx0_8 + fidx]
    cnt = Counter(col.tolist())
    zero = int((col == 0).sum())
    vas = Counter()
    for v in cnt:
        s = sec_of_rva(int(v))
        if s:
            vas[s] += 1
    return {
        "distinct": int(len(cnt)),
        "zero_frac": round(zero / N, 4),
        "top_values": [{"v": int(v), "n": int(c)}
                       for v, c in cnt.most_common(6)],
        "va_slots_by_section": dict(vas),
        "distinct_va_values": int(sum(vas.values())),
    }


out["typeinfo_u64_fields"] = {f"q{i}@+{8*i}": field_stats_u64(i) for i in range(10)}

# MEC role hypothesis: look for a 'size' (u16) and 'fieldCount' (u16) and 'align' (u8)
mec = {}
for fidx in range(20):
    col = u32[idx0 + fidx]
    lo = (col & 0xFFFF)
    hi = (col >> 16) & 0xFFFF
    mec[f"f{fidx:02d}"] = {
        "u16_lo_in_0..0x4000_frac": round(float(((lo > 0) & (lo < 0x4000)).mean()), 4),
        "u16_lo_distinct": int(len(np.unique(lo))),
        "u16_hi_in_0..0x4000_frac": round(float(((hi > 0) & (hi < 0x4000)).mean()), 4),
        "u16_hi_distinct": int(len(np.unique(hi))),
        "u16_lo_max": int(lo.max()), "u16_hi_max": int(hi.max()),
    }
out["typeinfo_mec_role_hypothesis"] = mec

# u8 view of every byte position in the record
byte_hist = {}
for b in range(0x50):
    col = ti_u8[rec_starts + b]
    cnt = Counter(col.tolist())
    byte_hist[f"b{b:02x}"] = {
        "distinct": int(len(cnt)),
        "top": [{"v": int(v), "n": int(c)} for v, c in cnt.most_common(4)],
    }
out["typeinfo_byte_positions"] = byte_hist

# ---------------------------------------------------------------- fieldinf
FREC_CANDS = [0x10, 0x18, 0x20, 0x28, 0x30, 0x38, 0x40, 0x48]
NF = len(fi)
fi_u32 = np.frombuffer(fi, dtype="<u4")
fi_u64 = np.frombuffer(fi, dtype="<u8")
typehash_domain = set(u32[idx0].tolist())          # typeinfo u32[0]
all_type_u32 = set(np.unique(u32[idx0[:, None] + np.arange(20)]).tolist())

scores = {}
for stride in FREC_CANDS:
    nslots = stride // 4
    nrec = NF // stride
    base = np.arange(nrec, dtype=np.int64) * (stride // 4)
    slot_stats = []
    for s in range(nslots):
        col = fi_u32[base + s]
        cnt = Counter(col.tolist())
        distinct = len(cnt)
        zero = int((col == 0).sum())
        in_type_hash = int(np.isin(col, list(typehash_domain)).sum())
        in_all_type = int(np.isin(col, list(all_type_u32)).sum())
        vas = Counter()
        for v in cnt:
            sec = sec_of_rva(int(v))
            if sec:
                vas[sec] += 1
        slot_stats.append({
            "slot": s, "distinct": distinct,
            "zero_frac": round(zero / nrec, 4),
            "frac_in_typeinfo_hash_domain": round(in_type_hash / nrec, 4),
            "frac_in_any_typeinfo_u32": round(in_all_type / nrec, 4),
            "va_values": dict(vas),
            "top": [{"v": int(v), "n": int(c)} for v, c in cnt.most_common(5)],
        })
    scores[f"0x{stride:x}"] = {"nrec": nrec, "slots": slot_stats}

out["fieldinf_stride_scores"] = scores

# u16 offset hypothesis for fieldinf (any stride): fraction of u16 slots in 0..0x4000
u16vals = np.frombuffer(fi, dtype="<u2")
u16vals = u16vals[: (NF // 2)]
inrange = int(((u16vals > 0) & (u16vals < 0x4000)).sum())
out["fieldinf_u16_global"] = {
    "total_u16": int(len(u16vals)),
    "in_0..0x4000_frac": round(inrange / len(u16vals), 4),
    "distinct": int(len(np.unique(u16vals))),
    "top": [{"v": int(v), "n": int(c)}
            for v, c in Counter(u16vals.tolist()).most_common(12)],
}

# typeHash cross-link: does any fieldinf u32 equal a typeinfo u32[0]?
link = int(np.isin(fi_u32, list(typehash_domain)).sum())
link_all = int(np.isin(fi_u32, list(all_type_u32)).sum())
out["fieldinf_typehash_crosslink"] = {
    "fieldinf_u32_total": int(len(fi_u32)),
    "matches_typeinfo_f00": link,
    "matches_any_typeinfo_u32": link_all,
    "expected_random": round(len(fi_u32) * len(typehash_domain) / 2 ** 32, 1),
}

# ---------------------------------------------------------------- dump
path = HERE / "p2_layout2.json"
path.write_text(json.dumps(out, indent=1), encoding="utf-8")
print("wrote", path)
print("marker counts:", out["markers"]["typeinfo_count_89_00_08_00"],
      out["markers"]["typeinfo_count_89_00_00_08"])
for k, v in out["typeinfo_u32_fields"].items():
    print(f"{k:12s} distinct={v['distinct']:6d} zero={v['zero_frac']:.3f} "
          f"small={v['small_lt_0x1000_frac']:.3f} va={v['distinct_va_values']:6d} "
          f"top={[(x['v'], x['n']) for x in v['top_values'][:3]]}")
print("fieldinf crosslink:", out["fieldinf_typehash_crosslink"])
for k, v in scores.items():
    s = v["slots"]
    print(k, "nrec", v["nrec"],
          "| distinct per slot:", [x["distinct"] for x in s],
          "| inhash:", [x["frac_in_typeinfo_hash_domain"] for x in s])
