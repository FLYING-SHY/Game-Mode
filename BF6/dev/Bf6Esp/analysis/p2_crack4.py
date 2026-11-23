"""More exotic hash families on the 2499 verified (name -> hash) anchors.

Covers: UTF-16LE input, murmur3_32 with seed sweep, xxh32 with seed sweep,
murmur2_32a, siphash-2-4 zero key, cityhash-ish, reversed name, salted bytes.
A single exact hit = scheme found.
"""
import json
import struct
import zlib
from pathlib import Path

HERE = Path(__file__).resolve().parent
pairs = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
seen = {}
for p in pairs:
    if p["type"] not in seen:
        seen[p["type"]] = p
anch = list(seen.values())
NAMES = [a["type"] for a in anch]
MODS = [a["module"] or "" for a in anch]
H = [a["hash"] for a in anch]
print("anchors:", len(anch))

M32 = 0xFFFFFFFF


def murmur3_32(data, seed=0):
    c1, c2 = 0xCC9E2D51, 0x1B873593
    length = len(data)
    h1 = seed
    rounded = length & ~3
    for i in range(0, rounded, 4):
        k1 = (data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24))
        k1 = (k1 * c1) & M32
        k1 = ((k1 << 15) | (k1 >> 17)) & M32
        k1 = (k1 * c2) & M32
        h1 ^= k1
        h1 = ((h1 << 13) | (h1 >> 19)) & M32
        h1 = (h1 * 5 + 0xE6546B64) & M32
    k1 = 0
    tail = length & 3
    if tail >= 3:
        k1 ^= data[rounded + 2] << 16
    if tail >= 2:
        k1 ^= data[rounded + 1] << 8
    if tail >= 1:
        k1 ^= data[rounded]
        k1 = (k1 * c1) & M32
        k1 = ((k1 << 15) | (k1 >> 17)) & M32
        k1 = (k1 * c2) & M32
        h1 ^= k1
    h1 ^= length
    h1 ^= h1 >> 16
    h1 = (h1 * 0x85EBCA6B) & M32
    h1 ^= h1 >> 13
    h1 = (h1 * 0xC2B2AE35) & M32
    h1 ^= h1 >> 16
    return h1


P1 = 0x9E3779B1
P2 = 0x85EBCA77
P3 = 0xC2B2AE3D
P4 = 0x27D4EB2F
P5 = 0x165667B1


def xxh32(data, seed=0):
    n = len(data)
    if n >= 16:
        v1 = (seed + P1 + P2) & M32
        v2 = (seed + P2) & M32
        v3 = seed & M32
        v4 = (seed - P1) & M32
        i = 0
        while i <= n - 16:
            for vi, off in ((0, 0), (1, 4), (2, 8), (3, 12)):
                k = int.from_bytes(data[i + off:i + off + 4], "little")
                v = [v1, v2, v3, v4][vi]
                v = (v + k * P2) & M32
                v = ((v << 13) | (v >> 19)) & M32
                v = (v * P1) & M32
                if vi == 0:
                    v1 = v
                elif vi == 1:
                    v2 = v
                elif vi == 2:
                    v3 = v
                else:
                    v4 = v
            i += 16
        h = (((v1 << 1) | (v1 >> 31)) + ((v2 << 7) | (v2 >> 25)) +
             ((v3 << 12) | (v3 >> 20)) + ((v4 << 18) | (v4 >> 14))) & M32
    else:
        h = (seed + P5) & M32
        i = 0
    h = (h + n) & M32
    while i + 4 <= n:
        k = int.from_bytes(data[i:i + 4], "little")
        h = (h + k * P3) & M32
        h = (((h << 17) | (h >> 15)) * P4) & M32
        i += 4
    while i < n:
        h = (h + data[i] * P5) & M32
        h = (((h << 11) | (h >> 21)) * P1) & M32
        i += 1
    h ^= h >> 15
    h = (h * P2) & M32
    h ^= h >> 13
    h = (h * P3) & M32
    h ^= h >> 16
    return h


def murmur2_32a(data, seed=0):
    m = 0x5BD1E995
    r = 24
    length = len(data)
    h = (seed ^ length) & M32
    i = 0
    while length >= 4:
        k = int.from_bytes(data[i:i + 4], "little")
        k = (k * m) & M32
        k ^= k >> r
        k = (k * m) & M32
        h = (h * m) & M32
        h ^= k
        i += 4
        length -= 4
    if length == 3:
        h ^= data[i + 2] << 16
    if length >= 2:
        h ^= data[i + 1] << 8
    if length >= 1:
        h ^= data[i]
        h = (h * m) & M32
    h ^= h >> 13
    h = (h * m) & M32
    h ^= h >> 15
    return h


def djb2_xor(data, init=5381):
    h = init
    for c in data:
        h = ((h * 33) ^ c) & M32
    return h


def djb2_add(data, init=5381):
    h = init
    for c in data:
        h = ((h * 33) + c) & M32
    return h


def fnv1a(data):
    h = 0x811C9DC5
    for c in data:
        h = ((h ^ c) * 0x01000193) & M32
    return h


def crc32s(data):
    return zlib.crc32(data) & M32


def try_family(label, fn, names, mods, tag=""):
    hits = 0
    for i, n in enumerate(names):
        if fn(n) == H[i]:
            hits += 1
    if hits:
        print(f"  HIT {hits}/{len(names)}  {label} {tag}")
    return hits


enc_variants = {
    "ascii": lambda s: s.encode("latin1", "ignore"),
    "utf16le": lambda s: s.encode("utf-16-le", "ignore"),
    "utf16le_nul": lambda s: (s + "\x00").encode("utf-16-le", "ignore"),
    "rev": lambda s: s[::-1].encode("latin1", "ignore"),
}

print("\n--- simple families x encodings ---")
for encname, enc in enc_variants.items():
    for fname, f in (("djb2_xor", djb2_xor), ("djb2_add", djb2_add),
                     ("fnv1a32", fnv1a), ("crc32", crc32s),
                     ("murmur3_32", lambda d: murmur3_32(d, 0)),
                     ("murmur2_32a", lambda d: murmur2_32a(d, 0)),
                     ("xxh32", lambda d: xxh32(d, 0))):
        try_family(fname, lambda s, f=f, enc=enc: f(enc(s)), NAMES, MODS, encname)

print("\n--- murmur3 seed sweep 0..255 (ascii) ---")
best = (0, None)
for seed in range(256):
    hits = sum(1 for i, n in enumerate(NAMES) if murmur3_32(n.encode(), seed) == H[i])
    if hits > best[0]:
        best = (hits, seed)
        if hits:
            print(f"   seed {seed}: {hits}")
print("  best:", best)

print("\n--- xxh32 seed sweep 0..255 ---")
best = (0, None)
for seed in range(256):
    hits = sum(1 for i, n in enumerate(NAMES) if xxh32(n.encode(), seed) == H[i])
    if hits > best[0]:
        best = (hits, seed)
print("  best:", best)

print("\n--- murmur2 seed sweep 0..255 ---")
best = (0, None)
for seed in range(256):
    hits = sum(1 for i, n in enumerate(NAMES) if murmur2_32a(n.encode(), seed) == H[i])
    if hits > best[0]:
        best = (hits, seed)
print("  best:", best)

print("\n--- djb2/fnv init sweep (ascii, early exit on first 3 anchors) ---")
probe = [(n.encode(), H[i]) for i, n in enumerate(NAMES[:3])]
for fname, f in (("djb2_xor", djb2_xor), ("djb2_add", djb2_add)):
    found = 0
    for init in range(0, 1 << 20):
        if all(f(d, init) == e for d, e in probe):
            hits = sum(1 for i, n in enumerate(NAMES) if f(n.encode(), init) == H[i])
            print(f"   {fname} init={init}: {hits}/{len(NAMES)}")
            found = 1
            break
    if not found:
        print(f"   {fname}: no init in 0..2^20")
print("done")
