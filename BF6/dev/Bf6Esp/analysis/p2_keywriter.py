"""Find the code that WRITES the per-type u32 key at typeinfo+0x00 (the record base slot).
Strategy: typeinfo record bases are known (23701, stride 0x50, marker 0x00080089 at +4).
Search the opcode-valid RIP-relative store refs (p2_riprefs.npz) whose target == a record base.
"""
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

ti = DATA[TI["roff"]:TI["roff"] + TI["rsize"]]
arr = np.frombuffer(ti, dtype="<u4")
marks = np.nonzero(arr == 0x00080089)[0] - 1
bases = np.array(sorted(set(marks.tolist())), dtype=np.int64)
base_set = set(bases.tolist())
print("typeinfo record bases:", len(bases))

z = np.load(HERE / "p2_riprefs.npz")
CODE, TGT, FORM = z["code"], z["tgt"], z["form"]
print("rip refs:", len(CODE), "forms:", Counter(FORM.tolist()).most_common())

# store forms (ids from p2_code2.FORMS): 6=48 89, 7=4c 89, 8=89, 15=0f 11 (movups),
# 22=48 c7 05 (mov qword imm32), 23=c7 05 (mov dword imm32)
STORE_FORMS = {6, 7, 8, 15, 22, 23}
mask = np.isin(FORM, list(STORE_FORMS))
idx = np.nonzero(mask)[0]
hits = []
for i in idx:
    t = int(TGT[i])
    if t in base_set:
        hits.append((int(CODE[i]), t))
print("store-refs landing exactly on a typeinfo record base (u32_0 slot):", len(hits))
by_code = Counter(c for c, _ in hits)
print("distinct code sites:", len(by_code))
for c, n in by_code.most_common(20):
    print(f"  code RVA 0x{c:08x}  -> {n} record-base targets")

# how many stores target the typeinfo section at all, and which offsets of the 0x50 record?
ti_lo, ti_hi = TI["va"], TI["va"] + TI["rsize"]
in_sec = idx[(TGT[idx] >= ti_lo) & (TGT[idx] < ti_hi)]
off_hist = Counter((int(TGT[i]) - ti_lo) % 0x50 for i in in_sec)
print(f"\nstore refs into the typeinfo section: {len(in_sec)} at {len(set(int(CODE[i]) for i in in_sec))} code sites")
print("  target offset mod 0x50 histogram (top 12):",
      [(hex(k), v) for k, v in off_hist.most_common(12)])
site_hist = Counter(int(CODE[i]) for i in in_sec)
print("  top code sites:", [(hex(c), n) for c, n in site_hist.most_common(12)])

# do ANY stores land INSIDE a static 0x50 record (not only on the u32_0 slot)?
mask_rec = np.zeros(len(ti), dtype=bool)
for b in bases:
    mask_rec[b:b + 0x50] = True
off_in = np.array([(int(TGT[i]) - ti_lo) for i in in_sec], dtype=np.int64)
inside = off_in[(off_in >= 0) & (off_in < len(ti))]
inside = inside[mask_rec[inside]]
# resolve each inside-target to (record base, offset within record)
pos = np.searchsorted(bases, inside, side="right") - 1
real_off = inside - bases[pos]
ih = Counter(int(o) for o in real_off)
print(f"\nstores landing INSIDE a static 0x50 record: {len(inside)}")
print("  offsets within the record:", [(hex(k), v) for k, v in ih.most_common(16)])
print("  stores writing the u32_0 key slot (offset 0):", int(ih.get(0, 0)))
print("  stores writing the kind slot  (offset 4):", int(ih.get(4, 0)))

# sanity: for stores at record-offset 0/4, is the target really a static record
# (i.e. does the marker 0x00080089 sit at target+4)?
def marker_at(off):
    if off + 4 >= len(ti):
        return False
    return int.from_bytes(ti[off + 4:off + 8], "little") == 0x00080089


cand0 = []
# recompute pairs directly
pairs = []
for i in in_sec:
    o = int(TGT[i]) - ti_lo
    if 0 <= o < len(ti) and mask_rec[o]:
        p = int(np.searchsorted(bases, o, side="right") - 1)
        pairs.append((int(CODE[i]), o, int(o - bases[p])))
c0 = [p for p in pairs if p[2] == 0]
c4 = [p for p in pairs if p[2] == 4]
print(f"\n  [check] stores at record-offset 0: {len(c0)}, of which target+4 holds the 0x00080089 marker: "
      f"{sum(1 for _, o, _ in c0 if marker_at(o))}")
print(f"  [check] stores at record-offset 4: {len(c4)}, of which target   holds the 0x00080089 marker: "
      f"{sum(1 for _, o, _ in c4 if int.from_bytes(ti[o:o+4],'little') == 0x00080089)}")
for code, o, rel in c0[:6]:
    print(f"    code 0x{code:08x} -> section+0x{o:08x} (rel {rel}) : "
          f"{ti[o:o+16].hex(' ')}")

out = {"typeinfo_bases": len(bases), "store_refs_to_record_base": len(hits),
       "sites": [{"code_rva": c, "count": n} for c, n in by_code.most_common(200)],
       "examples": [{"code_rva": c, "target_rva": t} for c, t in hits[:50]],
       "store_refs_into_typeinfo_section": len(in_sec),
       "target_offset_mod_0x50_histogram": {hex(k): v for k, v in off_hist.most_common(64)},
       "top_code_sites": [{"code_rva": c, "count": n} for c, n in site_hist.most_common(40)]}
(HERE / "p2_keywriter.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("wrote p2_keywriter.json")
