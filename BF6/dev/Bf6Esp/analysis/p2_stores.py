"""Find code that STORES into the typeinfo / fieldinf sections and group by function."""
import json
import struct
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
SEC = img.sec
TI = (SEC["typeinfo"]["va"], SEC["typeinfo"]["va"] + SEC["typeinfo"]["vsize"])
FI = (SEC["fieldinf"]["va"], SEC["fieldinf"]["va"] + SEC["fieldinf"]["vsize"])

z = np.load(HERE / "p2_riprefs.npz")
code, tgt, form, ilen = z["code"], z["tgt"], z["form"], z["ilen"]
print("refs:", len(code))

# store forms: 48 89 05|0d|15|1d|25|2d|35|3d (mov qword [rip+d], r64)
#             89 05|0d|15|1d|25|2d|35|3d          (mov dword [rip+d], r32)
#             c7 05 (mov dword [rip+d], imm32), 48 c7 05
# store forms (index into p2_code2.FORMS): 6=48 89, 7=4c 89, 8=89, 22=48 c7 05, 23=c7 05
STORES = (6, 7, 8, 22, 23)
FORMNAME = {6: "48_89", 7: "4c_89", 8: "89", 22: "48_c7_05", 23: "c7_05"}
sel = np.isin(form, list(STORES)) & (((tgt >= TI[0]) & (tgt < TI[1])) |
                                      ((tgt >= FI[0]) & (tgt < FI[1])))
print("store refs into typeinfo/fieldinf:", int(sel.sum()))
c = Counter()
for f, t in zip(form[sel], tgt[sel]):
    c["typeinfo" if TI[0] <= t < TI[1] else "fieldinf"] += 1
print(c)

# .pdata function ranges
pdata = img.blob(".pdata")
nfun = len(pdata) // 12
beg = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[0::3].astype(np.int64)
end = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[1::3].astype(np.int64)
print("pdata funcs:", nfun)


def func_of(rva):
    i = np.searchsorted(beg, rva, side="right") - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


by_func = defaultdict(list)
for cc, ff, tt in zip(code[sel], form[sel], tgt[sel]):
    fr = func_of(int(cc))
    if fr:
        by_func[fr].append((int(cc), FORMNAME.get(int(ff), str(int(ff))), int(tt)))
print("distinct functions writing typeinfo/fieldinf:", len(by_func))

rows = []
for (b, e), lst in sorted(by_func.items(), key=lambda kv: -len(kv[1])):
    rows.append({"func_rva": hex(b), "func_end": hex(e), "n_stores": len(lst),
                 "sites": [{"code_rva": hex(c0), "form": f0, "target_rva": hex(t0),
                            "section": "typeinfo" if TI[0] <= t0 < TI[1] else "fieldinf"}
                           for c0, f0, t0 in lst[:12]]})
for r in rows[:25]:
    print(r["func_rva"], r["n_stores"], [(s["code_rva"], s["form"], s["target_rva"]) for s in r["sites"][:5]])

(HERE / "p2_stores.json").write_text(json.dumps(
    {"total_store_refs": int(sel.sum()), "functions": rows[:200]}, indent=1), encoding="utf-8")
print("wrote p2_stores.json")
