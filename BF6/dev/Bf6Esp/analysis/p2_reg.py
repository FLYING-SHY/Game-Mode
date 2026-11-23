"""Find type/field registration stubs and build name -> stub maps."""
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
TI = (SEC["typeinfo"]["va"], SEC["typeinfo"]["va"] + SEC["typeinfo"]["vsize"])
FI = (SEC["fieldinf"]["va"], SEC["fieldinf"]["va"] + SEC["fieldinf"]["vsize"])

z = np.load(HERE / "p2_riprefs.npz")
code, tgt, form = z["code"], z["tgt"], z["form"]
pdata = img.blob(".pdata")
nfun = len(pdata) // 12
beg = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[0::3].astype(np.int64)
end = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[1::3].astype(np.int64)


def func_of(rva):
    i = np.searchsorted(beg, rva, side="right") - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


def str_at_rva(rva, limit=100):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    o = s["roff"] + (rva - s["va"])
    e = DATA.find(b"\0", o, o + limit)
    if e > o:
        b = DATA[o:e]
        if len(b) > 1 and all(32 <= c < 127 for c in b):
            return b.decode("ascii")
    return None


STORE_FORMS = {6, 7, 8, 22, 23}
# group refs by function
fref = defaultdict(list)
for c, t, f in zip(code, tgt, form):
    fr = func_of(int(c))
    if fr:
        fref[fr].append((int(c), int(f), int(t)))

reg_types = []       # stubs writing typeinfo
reg_fields = []      # stubs writing fieldinf
for (b, e), refs in fref.items():
    stores_ti = [r for r in refs if r[2] >= TI[0] and r[2] < TI[1] and r[1] in STORE_FORMS]
    stores_fi = [r for r in refs if r[2] >= FI[0] and r[2] < FI[1] and r[1] in STORE_FORMS]
    if not stores_ti and not stores_fi:
        continue
    strs = [(c, t, str_at_rva(t)) for c, f, t in refs
            if SEC[".rdata"]["va"] <= t < SEC[".rdata"]["va"] + SEC[".rdata"]["vsize"]
            and str_at_rva(t)]
    if stores_ti and strs:
        reg_types.append({"func": hex(b), "end": hex(e), "n_store_ti": len(stores_ti),
                          "n_store_fi": len(stores_fi),
                          "strings": [s[2] for s in strs],
                          "ti_targets": sorted({hex(r[2]) for r in stores_ti})[:12]})
    if stores_fi and strs:
        reg_fields.append({"func": hex(b), "end": hex(e), "n_store_fi": len(stores_fi),
                           "n_store_ti": len(stores_ti),
                           "strings": [s[2] for s in strs],
                           "fi_targets": sorted({hex(r[2]) for r in stores_fi})[:12]})

print("type-registration stubs with .rdata strings:", len(reg_types))
for r in reg_types[:20]:
    print(" ", r["func"], r["strings"][:6], r["ti_targets"][:4])
print()
print("field-registration stubs with .rdata strings:", len(reg_fields))
for r in reg_fields[:20]:
    print(" ", r["func"], r["strings"][:8], r["fi_targets"][:4])

# name -> stub map for type stubs
names = Counter()
name_to_func = {}
for r in reg_types:
    for s in r["strings"]:
        names[s] += 1
        name_to_func.setdefault(s, []).append(r["func"])
print()
print("distinct strings referenced by type stubs:", len(names))
print("most common:", names.most_common(15))

(HERE / "p2_reg.json").write_text(json.dumps(
    {"type_stubs": reg_types[:5000], "field_stubs": reg_fields[:5000],
     "name_to_func": {k: v[:4] for k, v in name_to_func.items()}}, indent=1), encoding="utf-8")
print("wrote p2_reg.json")

# on-disk content at the stub write area
for rva in (0xB48A400, 0xB48A470, 0xB48A488):
    o = img.rva2off(rva)
    print(hex(rva), DATA[o:o + 0x60].hex())
