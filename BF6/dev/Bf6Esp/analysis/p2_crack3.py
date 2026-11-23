"""Is the typeinfo u32[0] a hash of the type NAME?  Structure-aware tests.

Test A (CRC family): for any reflected CRC32 with an arbitrary init I,
     crc_I(s) = raw_crc(s) ^ K(len(s))
so (H ^ raw_crc(s)) must be CONSTANT inside every name-length group.
Test B: difference analysis vs djb2-xor / djb2-add / fnv1a (XOR or subtract).
Test C: same name in two different modules -> same hash?
Test D: which of the 20 record slots behaves like a name hash.
"""
import json
import zlib
from collections import Counter, defaultdict
from pathlib import Path

HERE = Path(__file__).resolve().parent
pairs = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
seen = {}
for p in pairs:
    if p["type"] and p["type"] not in seen:
        seen[p["type"]] = p
anch = list(seen.values())
NAMES = [a["type"] for a in anch]
MODS = [a["module"] or "" for a in anch]
H = [a["hash"] for a in anch]
U32 = [a["u32"] for a in anch]
print("anchors:", len(anch))

# ---- Test C: duplicate names -------------------------------------------------
byname = defaultdict(list)
for a in anch:
    byname[a["type"]].append(a)
dups = {k: v for k, v in byname.items() if len(v) > 1}
print(f"Test C: names appearing in >1 anchor: {len(dups)}")
for k, v in list(dups.items())[:5]:
    hs = {x["hash"] for x in v}
    ms = {x["module"] for x in v}
    print(f"   {k:<50} modules={ms} hashes={[hex(x) for x in hs]}")

# ---- Test B: difference analysis --------------------------------------------
def djb2_xor(s, init=5381):
    h = init
    for c in s.encode():
        h = ((h * 33) ^ c) & 0xFFFFFFFF
    return h


def djb2_add(s, init=5381):
    h = init
    for c in s.encode():
        h = ((h * 33) + c) & 0xFFFFFFFF
    return h


def fnv1a(s):
    h = 0x811C9DC5
    for c in s.encode():
        h = ((h ^ c) * 0x01000193) & 0xFFFFFFFF
    return h


for fn, fname in ((djb2_xor, "djb2_xor"), (djb2_add, "djb2_add"), (fnv1a, "fnv1a32")):
    for shape in ("name", "mod::name"):
        xs = Counter()
        for n, m, h in zip(NAMES, MODS, H):
            s = n if shape == "name" else f"{m}::{n}"
            v = fn(s)
            xs[(h ^ v) & 0xFFFFFFFF] += 1
            xs[("sub", (h - v) & 0xFFFFFFFF)] += 1
        top = xs.most_common(3)
        print(f"Test B {fname}/{shape}: top diffs {[(a if isinstance(a,tuple) else hex(a), b) for a,b in top]}")

# ---- Test A: CRC family, per-length XOR constant -----------------------------
def crcs(name):
    b = name.encode()
    return {
        "raw_std": zlib.crc32(b, 0) & 0xFFFFFFFF,
        "std": zlib.crc32(b) & 0xFFFFFFFF,
    }


for slot in range(20):
    groups = defaultdict(set)
    for n, u in zip(NAMES, U32):
        hv = u[slot]
        groups[len(n)].add((hv ^ crcs(n)["raw_std"]) & 0xFFFFFFFF)
    # a genuine CRC-with-init scheme: one value per length group
    multi = [L for L, s in groups.items() if len(s) > 1]
    if not multi and groups:
        print(f"Test A slot {slot}: *** CRC32-with-arbitrary-init pattern *** "
              f"groups={len(groups)} e.g. L={sorted(groups)[0]} const={hex(list(groups[sorted(groups)[0]])[0])}")
        break
else:
    print("Test A: no slot shows the CRC-with-init pattern")

# ---- Test D: which slot equals djb2_xor(name) --------------------------------
for slot in range(20):
    n_eq = sum(1 for n, u in zip(NAMES, U32) if djb2_xor(n) == u[slot])
    if n_eq:
        print(f"Test D slot {slot}: djb2_xor matches {n_eq}/{len(NAMES)}")
for slot in range(20):
    n_eq = sum(1 for n, u in zip(NAMES, U32) if djb2_xor(n) == u[slot + 20] if slot + 20 < 20)
print("Test D done")

# ---- value stats of slot 0 for anchors ---------------------------------------
vals = [u[0] for u in U32]
print("\nslot0 of anchors: distinct", len(set(vals)), "of", len(vals),
      "min", hex(min(vals)), "max", hex(max(vals)))
par = Counter(v & 1 for v in vals)
print("parity:", par)
mod4 = Counter(v & 3 for v in vals)
print("mod4:", mod4)
top = Counter(vals).most_common(5)
print("most common slot0 values:", [(hex(v), c) for v, c in top])
