"""Decode the .vm_sec -> .vlizer descriptor structure and profile the .vlizer blob."""
import json
import math
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
IB = 0x140000000

VM = SEC[".vm_sec"]
VL = SEC[".vlizer"]
vm = DATA[VM["roff"]:VM["roff"] + VM["rsize"]]
vl = DATA[VL["roff"]:VL["roff"] + VL["rsize"]]

# ---- section index for RVA lookup ----
SECT = []
for nm, s in SEC.items():
    SECT.append((nm, s["va"], s["va"] + max(s["vsize"], s["rsize"])))
SECT.sort(key=lambda x: x[1])


def sec_of(rva):
    for nm, lo, hi in SECT:
        if lo <= rva < hi:
            return nm
    return None


# ---- .vm_sec: locate the first non-zero u32 ----
u32 = np.frombuffer(vm, dtype="<u4")
nz = np.nonzero(u32)[0]
print(f".vm_sec size={len(vm)} bytes, {len(u32)} u32, first non-zero u32 index={nz[0]} "
      f"(+0x{nz[0]*4:x}), last={nz[-1]} (+0x{nz[-1]*4:x})")
first = int(nz[0])
tail = u32[first:]
print(f"  non-zero region: {len(tail)} u32 = {len(tail)//8} 8-byte records + {len(tail)%8} extra")

# interpret as 8-byte records (a, b)
n8 = len(tail) // 8
a = tail[: n8 * 2: 2].astype(np.int64)
b = tail[1: n8 * 2: 2].astype(np.int64)
d = b - a
print(f"\n8-byte record view: n={n8}")
print("  delta (b-a) histogram:", [(int(k), int(v)) for k, v in Counter(d.tolist()).most_common(12)])
print("  a: min 0x%x max 0x%x ; b: min 0x%x max 0x%x" % (a.min(), a.max(), b.min(), b.max()))
print("  section(a):", dict(Counter(sec_of(int(x)) for x in a)))
print("  section(b):", dict(Counter(sec_of(int(x)) for x in b)))
print("  distinct a:", len(set(a.tolist())), " distinct b:", len(set(b.tolist())))
in_vl_a = sum(1 for x in a if VL["va"] <= x < VL["va"] + VL["rsize"])
in_vl_b = sum(1 for x in b if VL["va"] <= x < VL["va"] + VL["rsize"])
print(f"  a inside .vlizer: {in_vl_a}/{n8} ; b inside .vlizer: {in_vl_b}/{n8}")

# also: flat interpretation - is the array just u32 pointers, 5 bytes apart?
diffs = np.diff(tail.astype(np.int64))
print("\nflat u32 diff histogram (top 12):",
      [(int(k), int(v)) for k, v in Counter(diffs.tolist()).most_common(12)])

# chain structure: sorted unique a values, gaps
sa = np.array(sorted(set(a.tolist())))
gaps = np.diff(sa)
print("  sorted unique a gaps:", [(int(k), int(v)) for k, v in Counter(gaps.tolist()).most_common(12)])

# ---- .vlizer profile ----
print(f"\n.vlizer size={len(vl)} bytes  va=0x{VL['va']:x}  first 64 bytes:")
print("  " + vl[:64].hex(" "))

BLOCK = 0x10000
nb = len(vl) // BLOCK
ent = []
for i in range(nb):
    ch = vl[i * BLOCK:(i + 1) * BLOCK]
    c = Counter(ch)
    n = len(ch)
    h = -sum((v / n) * math.log2(v / n) for v in c.values())
    ent.append(round(h, 4))
print(f"  entropy per {BLOCK:#x} block: min={min(ent)} max={max(ent)} mean={sum(ent)/len(ent):.4f}")
low = [(i, ent[i]) for i in range(nb) if ent[i] < 7.5]
print(f"  blocks with entropy < 7.5: {len(low)} -> {low[:20]}")

MAGIC = {
    "zlib_78_01": b"\x78\x01", "zlib_78_9c": b"\x78\x9c", "zlib_78_da": b"\x78\xda",
    "gzip": b"\x1f\x8b\x08", "lz4": b"\x04\x22\x4d\x18", "xz": b"\xfd7zXZ",
    "zstd": b"\x28\xb5\x2f\xfd", "lzma": b"\x5d\x00\x00", "bzip2": b"BZh",
    "pk": b"PK\x03\x04", "zlib78_5e": b"\x78\x5e", "raw_deflate_hint": b"\x78\x9c",
}
print("  magic scan:")
for nm, m in MAGIC.items():
    hits = []
    s = 0
    while True:
        p = vl.find(m, s)
        if p < 0:
            break
        hits.append(p)
        s = p + 1
        if len(hits) > 200:
            break
    print(f"    {nm:16s} {len(hits)} hits {['0x%x'%h for h in hits[:8]]}")

out = {
    "vm_sec": {"va": VM["va"], "raw_offset": VM["roff"], "size": VM["rsize"],
               "first_nonzero_u32_index": first, "nonzero_u32_count": len(tail),
               "record8_count": n8,
               "delta_histogram": {str(int(k)): int(v) for k, v in Counter(d.tolist()).most_common(32)},
               "section_of_a": dict(Counter(sec_of(int(x)) for x in a)),
               "section_of_b": dict(Counter(sec_of(int(x)) for x in b)),
               "a_inside_vlizer": in_vl_a, "b_inside_vlizer": in_vl_b,
               "distinct_a": len(set(a.tolist())), "distinct_b": len(set(b.tolist())),
               "a_min": hex(int(a.min())), "a_max": hex(int(a.max())),
               "flat_diff_histogram": {str(int(k)): int(v) for k, v in Counter(diffs.tolist()).most_common(32)},
               "sorted_unique_a_gap_histogram": {str(int(k)): int(v) for k, v in Counter(gaps.tolist()).most_common(32)},
               "first_records": [{"a": hex(int(a[i])), "b": hex(int(b[i])), "d": int(d[i])}
                                 for i in range(min(40, n8))]},
    "vlizer": {"va": VL["va"], "raw_offset": VL["roff"], "size": VL["rsize"],
               "first64_hex": vl[:64].hex(" "),
               "entropy_per_64k": {"min": min(ent), "max": max(ent),
                                   "mean": round(sum(ent) / len(ent), 4),
                                   "blocks": ent}},
}
(HERE / "p2_vlizer_layout.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("\nwrote p2_vlizer_layout.json")
