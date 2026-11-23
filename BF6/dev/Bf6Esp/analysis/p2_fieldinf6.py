"""Classify every u32 in the fieldinf section; isolate the candidate FIELD-NAME-HASH space."""
import json
import re
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec

fi = SEC["fieldinf"]
ti = SEC["typeinfo"]
tblob = DATA[ti["roff"]:ti["roff"] + ti["rsize"]]
arr = np.frombuffer(tblob, dtype="<u4")
bases = np.nonzero(arr == 0x00080089)[0] - 1
d0 = arr[bases]
d11 = arr[bases + 11]
print(f"typeinfo recs={len(bases)} distinct u32_0={len(np.unique(d0))} distinct u32_11={len(np.unique(d11))}")
print("  sample u32_11:", [hex(int(x)) for x in d11[:8]])
print("  is 0xdbb23984 in u32_11 domain:", 0xdbb23984 in set(int(x) for x in d11))
print("  is 0x6021b344 in u32_11 domain:", 0x6021b344 in set(int(x) for x in d11))
print("  is 0xd0434e4e in u32_11 domain:", 0xd0434e4e in set(int(x) for x in d11))

farr = np.frombuffer(DATA[fi["roff"]:fi["roff"] + fi["rsize"]], dtype="<u4")
S11 = np.array(sorted(set(int(x) for x in d11)), dtype=np.uint32)
S0 = np.array(sorted(set(int(x) for x in d0)), dtype=np.uint32)

m11 = np.isin(farr, S11)
m0 = np.isin(farr, S0)
small = farr < 0x400
ptrlow = (farr >= 0x00400000) & (farr < 0x00500000)
other = ~(m11 | m0 | small | ptrlow)
print(f"\nfieldinf u32 = {len(farr)}")
print(f"  in typeinfo u32_11 domain : {int(m11.sum())}")
print(f"  in typeinfo u32_0  domain : {int((m0 & ~m11).sum())}")
print(f"  small (<0x400)            : {int((small & ~m11 & ~m0).sum())}")
print(f"  .data VA low (0x40..0x50)  : {int((ptrlow & ~m11 & ~m0).sum())}")
print(f"  OTHER (candidate hashes)  : {int(other.sum())}  distinct={len(np.unique(farr[other]))}")

oth = farr[other]
print("\n  top OTHER values:", [(hex(int(v)), int(c)) for v, c in
                                zip(*[a[:12] for a in np.unique(oth, return_counts=True)])][0:0] or
      [(hex(int(v)), int(c)) for v, c in sorted(zip(*np.unique(oth, return_counts=True)), key=lambda t: -t[1])[:12]])

# candidate hash universe for FIELD NAMES
rd = SEC[".rdata"]
blob = DATA[rd["roff"]:rd["roff"] + rd["rsize"]]
names = set(m.group().decode("ascii") for m in re.finditer(rb"[A-Za-z_][A-Za-z0-9_]{1,63}", blob))
names = sorted(names)
print(f"\n.rdata identifier universe: {len(names)}")
nb = np.array([n.encode() for n in names], dtype="S64")
u8 = nb.view(np.uint8).reshape(len(names), -1)
lens = np.array([len(n) for n in names])

M32 = 0xFFFFFFFF
osh = np.zeros(len(names), dtype=np.uint32)


def fam_mul_xor(mult, init):
    h = np.full(len(names), np.uint32(init))
    for i in range(u8.shape[1]):
        h = (h * np.uint32(mult)) ^ u8[:, i].astype(np.uint32)
        h &= M32
    return h


def fam_mul_add(mult, init):
    h = np.full(len(names), np.uint32(init))
    for i in range(u8.shape[1]):
        h = (h * np.uint32(mult)) + u8[:, i].astype(np.uint32)
        h &= M32
    return h


osh_target = set(int(x) for x in np.unique(oth))
results = []
for label, fn, mult, init in [
    ("djb2_xor(5381)", fam_mul_xor, 33, 5381),
    ("djb2_add(5381)", fam_mul_add, 33, 5381),
    ("fnv1a32", fam_mul_xor, 0x01000193, 0x811C9DC5),
    ("mul31_xor(0)", fam_mul_xor, 31, 0),
    ("mul31_add(0)", fam_mul_add, 31, 0),
    ("mul131_xor(0)", fam_mul_xor, 131, 0),
    ("mul16777619_xor(0)", fam_mul_xor, 16777619, 0),
]:
    h = fn(mult, init)
    u = np.unique(h)
    inter = np.intersect1d(u, np.array(sorted(osh_target), dtype=np.uint32))
    results.append((label, len(u), len(inter)))
    if len(inter):
        for v in inter[:10]:
            idx = np.nonzero(h == v)[0][:5]
            print(f"    HIT {label}: 0x{int(v):08x} <- {[names[i] for i in idx]}")

print("\n  family / distinct-hashes / hits-into-fieldinf-OTHER-space")
for r in results:
    print(f"    {r[0]:<22} {r[1]:>8} {r[2]:>6}")

out = {
    "fieldinf_u32_total": int(len(farr)),
    "in_typeinfo_u32_11_domain": int(m11.sum()),
    "in_typeinfo_u32_0_domain_only": int((m0 & ~m11).sum()),
    "small_lt_0x400": int((small & ~m11 & ~m0).sum()),
    "data_va_low": int((ptrlow & ~m11 & ~m0).sum()),
    "other_candidate_hashes": int(other.sum()),
    "other_distinct": int(len(np.unique(oth))),
    "family_hits": {r[0]: r[2] for r in results},
    "identifier_universe": len(names),
}
(HERE / "p2_fieldinf6.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("\nwrote p2_fieldinf6.json")
