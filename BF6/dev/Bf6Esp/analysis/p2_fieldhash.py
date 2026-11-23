"""Test whether the fieldinf 'H' values are djb2/fnv hashes of .rdata strings."""
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
DATA = img.data
SEC = img.sec
IB = 0x140000000
FI = SEC["fieldinf"]
RD = SEC[".rdata"]
DA = SEC[".data"]
TX = SEC[".text"]

# ---- 1. universe: every NUL-terminated ASCII string in .rdata --------------
rd = DATA[RD["roff"]:RD["roff"] + RD["rsize"]]
strs = []
for m in re.finditer(rb"[ -~]{2,120}\x00", rd):
    s = m.group()[:-1]
    strs.append((RD["va"] + m.start(), s))
print(f".rdata ASCII strings: {len(strs)}")

# also the whole-file pool of identifiers
allstr = []
for m in re.finditer(rb"[A-Za-z_][A-Za-z0-9_:<>,.]{2,120}\x00", DATA):
    allstr.append(m.group()[:-1])
print(f"whole-file identifier-ish strings: {len(allstr)}")

# ---- 2. fieldinf H values ------------------------------------------------
fi = DATA[FI["roff"]:FI["roff"] + FI["rsize"]]
u32 = np.frombuffer(fi, dtype="<u4").astype(np.uint64)
N = len(u32)


def rva_of(v):
    return (v + 0xC0000000) & 0xFFFFFFFF


keep = np.ones(N, dtype=bool)
for i, v in enumerate(u32.tolist()):
    if v == 0 or v < 0x1000:
        keep[i] = False
        continue
    r = rva_of(v)
    if (DA["va"] <= r < DA["va"] + DA["vsize"]) or (RD["va"] <= r < RD["va"] + RD["rsize"]) \
            or (TX["va"] <= r < TX["va"] + TX["rsize"]):
        keep[i] = False
H = u32[keep]
Hset = set(H.tolist())
print(f"fieldinf non-pointer u32 values: {len(H)} ({len(Hset)} distinct)")

# ---- 3. hash families ----------------------------------------------------


def djb2_xor(bs):
    h = 5381
    for c in bs:
        h = ((h * 33) ^ c) & 0xFFFFFFFF
    return h


def djb2_add(bs):
    h = 5381
    for c in bs:
        h = ((h * 33) + c) & 0xFFFFFFFF
    return h


def fnv1a32(bs):
    h = 0x811C9DC5
    for c in bs:
        h = ((h ^ c) * 0x01000193) & 0xFFFFFFFF
    return h


def sdbm(bs):
    h = 0
    for c in bs:
        h = (c + (h << 6) + (h << 16) - h) & 0xFFFFFFFF
    return h


FAMS = {"djb2_xor": djb2_xor, "djb2_add": djb2_add, "fnv1a32": fnv1a32, "sdbm": sdbm}

for label, pool in (("rdata_strings", [s for _, s in strs]),
                    ("file_identifiers", allstr)):
    for fn, f in FAMS.items():
        hits = []
        for s in pool:
            h = f(s)
            if h in Hset:
                hits.append((s.decode("latin1"), h))
        floor = len(pool) * len(Hset) / 2 ** 32
        print(f"{label:16s} {fn:10s}: hits={len(hits):5d} floor={floor:.2f}  "
              f"{[x[0] for x in hits[:10]]}")
        if len(hits) > max(3 * floor, 5):
            (HERE / f"p2_fieldhash_{label}_{fn}.json").write_text(
                json.dumps([{"name": n, "hash": h} for n, h in hits], indent=1), encoding="utf-8")
