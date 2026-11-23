"""Which .rdata identifier strings are referenced from code, and how often."""
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
z = np.load(HERE / "p2_riprefs.npz")
CODE, TGT = z["code"], z["tgt"]
IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}$")

rd = DATA[SEC[".rdata"]["roff"]:SEC[".rdata"]["roff"] + SEC[".rdata"]["rsize"]]
rdbase = SEC[".rdata"]["va"]
names = {}
i = 0
while True:
    j = rd.find(b"\x00", i)
    if j < 0:
        break
    s = rd[i:j]
    if 1 <= len(s) <= 64 and IDENT.match(s):
        names[rdbase + i] = s.decode("latin1")
    i = j + 1
nrv = np.array(sorted(names), dtype=np.int64)
print("rdata identifiers:", len(nrv))

pos = np.clip(np.searchsorted(nrv, TGT), 0, len(nrv) - 1)
hit = nrv[pos] == TGT
idx = np.nonzero(hit)[0]
print("code refs landing exactly on an identifier start:", len(idx))

cnt = Counter()
for i in idx:
    cnt[int(TGT[i])] += 1
print("distinct identifiers referenced from code:", len(cnt))
print("\ntop 60 by ref count:")
for rva, c in cnt.most_common(60):
    print(f"   {rva:08x} {names[rva]:<40} x{c}")

# lowercase / member-like names
low = [(rva, names[rva], c) for rva, c in cnt.items()
       if names[rva][0].islower() and len(names[rva]) > 2]
print("\nlowercase-starting identifiers referenced from code:", len(low))
low.sort(key=lambda x: -x[2])
for rva, nm, c in low[:60]:
    print(f"   {rva:08x} {nm:<40} x{c}")

(HERE / "p2_namecode.json").write_text(json.dumps(
    [{"rva": int(r), "name": names[r], "refs": c} for r, c in cnt.most_common()]),
    encoding="utf-8")
print("wrote p2_namecode.json")
