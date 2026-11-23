"""Second pass on the typeinfo record <-> .data object relationship."""
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
TI_VA = SEC["typeinfo"]["va"]
TI_VSZ = SEC["typeinfo"]["vsize"]
FI_VA = SEC["fieldinf"]["va"]
FI_VSZ = SEC["fieldinf"]["vsize"]


def off_of_rva(rva):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    size = max(s["rsize"], s["vsize"])
    if not (s["va"] <= rva < s["va"] + size):
        return None
    return s["roff"] + (rva - s["va"])


out = {}
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
rec_rva = TI_VA + rs

# 1) delta between the .data object's back-pointer and the record start
deltas = Counter()
same_as_q7 = 0
ptr_in_ti = 0
checked = 0
for k in range(N):
    va = int(q[k, 6])
    if va == 0:
        continue
    o = off_of_rva(va - IMAGE_BASE)
    if o is None:
        continue
    checked += 1
    p = int.from_bytes(DATA[o:o + 8], "little")
    prva = p - IMAGE_BASE
    if TI_VA <= prva < TI_VA + TI_VSZ:
        ptr_in_ti += 1
        deltas[prva - int(rec_rva[k])] += 1
    if p == int(q[k, 7]):
        same_as_q7 += 1
out["backptr_delta_hist"] = [{"delta": d, "n": c} for d, c in deltas.most_common(20)]
out["backptr_into_typeinfo"] = [checked, ptr_in_ti]
out["backptr_equals_record_q7"] = same_as_q7
print("checked", checked, "into typeinfo", ptr_in_ti, "== q7", same_as_q7)
print("deltas:", deltas.most_common(10))

# 2) the 5473 records without q6: what do they look like?
noq6 = np.flatnonzero(q[:, 6] == 0)
withq6 = np.flatnonzero(q[:, 6] != 0)
out["records_without_q6"] = int(len(noq6))
out["records_with_q6"] = int(len(withq6))
if len(noq6):
    out["no_q6_sample_u32"] = [[int(x) for x in u32[k, :20]] for k in noq6[:5]]
    out["no_q6_first_index"] = int(noq6[0])
    out["no_q6_last_index"] = int(noq6[-1])
    # are the no-q6 records contiguous?
    print("no-q6 index range:", noq6[0], noq6[-1], "count", len(noq6))

# 3) run structure: for each run, how many records have q6?
runs = []
mm = np.array(m)
brk = np.flatnonzero(np.diff(mm) != 0x50) + 1
st = np.concatenate(([0], brk))
en = np.concatenate((brk, [len(mm)]))
run_info = []
for s, e in zip(st, en):
    idxs = np.arange(s, e)
    has6 = int((q[idxs, 6] != 0).sum())
    run_info.append({"marker_off": int(mm[s]), "count": int(e - s), "with_q6": has6,
                     "rva": hex(TI_VA + int(mm[s]) - 4)})
run_info.sort(key=lambda r: -r["count"])
out["runs_with_q6_counts"] = run_info[:30]
print("top runs:", [(r["count"], r["with_q6"]) for r in run_info[:10]])

# 4) fieldinf: u32 RVAs into typeinfo / fieldinf?
fi = img.blob("fieldinf")
fi_u32 = np.frombuffer(fi, dtype="<u4")
in_ti = int(((fi_u32 >= TI_VA) & (fi_u32 < TI_VA + TI_VSZ)).sum())
in_fi = int(((fi_u32 >= FI_VA) & (fi_u32 < FI_VA + FI_VSZ)).sum())
out["fieldinf_u32_rva_into_typeinfo"] = in_ti
out["fieldinf_u32_rva_into_fieldinf"] = in_fi
print("fieldinf u32 rvas into typeinfo:", in_ti, "into fieldinf:", in_fi)

# 5) do typeinfo u32[11] (hash2) values appear in fieldinf?
h11 = set(u32[:, 11].tolist())
h00 = set(u32[:, 0].tolist())
fiu = fi_u32
out["fieldinf_matches_typeinfo_u32_11"] = int(np.isin(fiu, list(h11)).sum())
out["fieldinf_matches_typeinfo_u32_00"] = int(np.isin(fiu, list(h00)).sum())
out["fieldinf_u32_total"] = int(len(fiu))
print("fieldinf matches u32_11:", out["fieldinf_matches_typeinfo_u32_11"],
      "u32_00:", out["fieldinf_matches_typeinfo_u32_00"])

# 6) .data objects: full qword dump statistics for the first 200 distinct targets
objs = np.unique(q[withq6, 6])[:200]
dumps = []
slot_str = Counter()
for t in objs:
    o = off_of_rva(int(t) - IMAGE_BASE)
    if o is None:
        continue
    words = [int.from_bytes(DATA[o + 8 * i:o + 8 * i + 8], "little") for i in range(12)]
    desc = []
    for k, w in enumerate(words):
        if IMAGE_BASE <= w < IMAGE_BASE + 0x10000000:
            pr = w - IMAGE_BASE
            s = img.sec_of(pr)
            desc.append(f"q{k}:VA/{s}")
            if s in (".rdata", ".data"):
                e = DATA.find(b"\0", off_of_rva(pr), off_of_rva(pr) + 60)
                if e > off_of_rva(pr):
                    b = DATA[off_of_rva(pr):e]
                    if all(32 <= c < 127 for c in b) and len(b) > 2:
                        desc[-1] += f"='{b.decode()}'"
                        slot_str[k] += 1
        else:
            desc.append(f"q{k}:0x{w:x}")
    dumps.append({"obj": hex(int(t)), "w": desc})
out["data_object_dumps"] = dumps[:30]
out["data_object_string_slots"] = dict(slot_str)
print("data object string slot histogram:", dict(slot_str.most_common()))
for d in dumps[:6]:
    print(d["obj"], " ".join(d["w"]))

path = HERE / "p2_link2.json"
path.write_text(json.dumps(out, indent=1), encoding="utf-8")
print("wrote", path)
