"""Pair (name string, stored u32) inside the typeinfo/fieldinf registration stubs.

For every function that stores into typeinfo/fieldinf we collect
  * the u32 immediates it stores (c7 05 / 48 c7 05 forms)
  * every RIP-relative reference it makes into .rdata (resolved to strings)
If a registration stub references exactly one name string and stores one u32
constant, that is an (name, hash) anchor.
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
    size = max(s["rsize"], s["vsize"])
    if not (s["va"] <= rva < s["va"] + size):
        return None
    o = s["roff"] + (rva - s["va"])
    e = DATA.find(b"\0", o, o + limit)
    if e > o:
        b = DATA[o:e]
        if len(b) > 1 and all(32 <= c < 127 for c in b):
            return b.decode("ascii")
    return None


# store forms: 6=48 89, 7=4c 89, 8=89, 22=48 c7 05, 23=c7 05
is_store = np.isin(form, [6, 7, 8, 22, 23])
in_sections = ((tgt >= TI[0]) & (tgt < TI[1])) | ((tgt >= FI[0]) & (tgt < FI[1]))
sel = np.flatnonzero(is_store & in_sections)

funcs = defaultdict(lambda: {"imm": [], "str": [], "stores": []})
for i in sel:
    c0 = int(code[i]); t0 = int(tgt[i]); f0 = int(form[i])
    fr = func_of(c0)
    if not fr:
        continue
    d = funcs[fr]
    d["stores"].append((c0, f0, t0))
    if f0 in (22, 23):
        doff = 3 if f0 == 22 else 2
        imm = struct.unpack_from("<I", DATA, img.rva2off(c0) + doff + 4)[0]
        d["imm"].append((c0, imm))
    elif f0 in (6, 7, 8):
        d["imm"].append((c0, None))

# string refs per function
is_ref = ~is_store
sel2 = np.flatnonzero(is_ref)
for i in sel2:
    c0 = int(code[i]); t0 = int(tgt[i])
    if not (SEC[".rdata"]["va"] <= t0 < SEC[".rdata"]["va"] + SEC[".rdata"]["vsize"]):
        continue
    fr = func_of(c0)
    if not fr:
        continue
    s = str_at_rva(t0)
    if s:
        funcs[fr]["str"].append((c0, t0, s))

out = []
for (b, e), d in sorted(funcs.items()):
    imms = [x[1] for x in d["imm"] if x[1] is not None]
    strs = [x[2] for x in d["str"]]
    out.append({"func_rva": hex(b), "func_end": hex(e),
                "n_stores": len(d["stores"]), "imms": imms[:20],
                "strings": strs[:20]})

# focus: functions with exactly one string and some immediates
one = [r for r in out if len(r["strings"]) == 1 and r["imms"]]
print("functions with >=1 store:", len(out))
print("functions with exactly 1 string ref:", len(one))
for r in one[:25]:
    print(r["func_rva"], "imms", [hex(i) for i in r["imms"][:6]], "str", r["strings"])
print()
multi = [r for r in out if len(r["strings"]) >= 2]
print("functions with >=2 string refs:", len(multi))
for r in multi[:15]:
    print(r["func_rva"], "imms", [hex(i) for i in r["imms"][:6]], "strs", r["strings"][:5])

(HERE / "p2_pairs.json").write_text(json.dumps(
    {"functions": out[:4000]}, indent=1), encoding="utf-8")
print("wrote p2_pairs.json")
