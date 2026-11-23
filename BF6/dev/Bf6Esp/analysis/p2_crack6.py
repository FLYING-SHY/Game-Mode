"""p2_crack6: 64-bit / non-polynomial hash families against the 2499 verified
(type name -> key) anchors.  A single hit would prove the scheme.
"""
import json
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))

M64 = (1 << 64) - 1
M32 = (1 << 32) - 1


def rotl(x, r, w=64):
    m = (1 << w) - 1
    return ((x << r) | (x >> (w - r))) & m


# ---------------- xxhash ----------------
def xxh64(data: bytes, seed: int = 0) -> int:
    P1, P2, P3, P4, P5 = (0x9E3779B185EBCA87, 0xC2B2AE3D27D4EB4F,
                          0x165667B19E3779F9, 0x85EBCA77C2B2AE63, 0x27D4EB2F165667C5)

    def rnd(acc, inp):
        acc = (acc + inp * P2) & M64
        acc = rotl(acc, 31)
        return (acc * P1) & M64

    n = len(data)
    if n >= 32:
        v1 = (seed + P1 + P2) & M64
        v2 = (seed + P2) & M64
        v3 = seed & M64
        v4 = (seed - P1) & M64
        i = 0
        while i + 32 <= n:
            v1 = rnd(v1, int.from_bytes(data[i:i + 8], "little"))
            v2 = rnd(v2, int.from_bytes(data[i + 8:i + 16], "little"))
            v3 = rnd(v3, int.from_bytes(data[i + 16:i + 24], "little"))
            v4 = rnd(v4, int.from_bytes(data[i + 24:i + 32], "little"))
            i += 32
        h = (rotl(v1, 1) + rotl(v2, 7) + rotl(v3, 12) + rotl(v4, 18)) & M64
        for v in (v1, v2, v3, v4):
            h ^= rnd(0, v)
            h = (h * P1 + P4) & M64
    else:
        h = (seed + P5) & M64
        i = 0
    h = (h + n) & M64
    while i + 8 <= n:
        k = int.from_bytes(data[i:i + 8], "little")
        k = (k * P2) & M64
        k = rotl(k, 31)
        k = (k * P1) & M64
        h ^= k
        h = (rotl(h, 27) * P1 + P4) & M64
        i += 8
    if i + 4 <= n:
        h ^= (int.from_bytes(data[i:i + 4], "little") * P1) & M64
        h = (rotl(h, 23) * P2 + P3) & M64
        i += 4
    while i < n:
        h ^= (data[i] * P5) & M64
        h = (rotl(h, 11) * P1) & M64
        i += 1
    h ^= h >> 33
    h = (h * P2) & M64
    h ^= h >> 29
    h = (h * P3) & M64
    h ^= h >> 32
    return h


def xxh32(data: bytes, seed: int = 0) -> int:
    P1, P2, P3, P4, P5 = (0x9E3779B1, 0x85EBCA77, 0xC2B2AE3D, 0x27D4EB2F, 0x165667B1)

    def rnd(acc, inp):
        acc = (acc + inp * P3) & M32
        acc = rotl(acc, 17, 32)
        return (acc * P4) & M32

    n = len(data)
    if n >= 16:
        v1 = (seed + P1 + P2) & M32
        v2 = (seed + P2) & M32
        v3 = seed & M32
        v4 = (seed - P1) & M32
        i = 0
        while i + 16 <= n:
            v1 = rnd(v1, int.from_bytes(data[i:i + 4], "little"))
            v2 = rnd(v2, int.from_bytes(data[i + 4:i + 8], "little"))
            v3 = rnd(v3, int.from_bytes(data[i + 8:i + 12], "little"))
            v4 = rnd(v4, int.from_bytes(data[i + 12:i + 16], "little"))
            i += 16
        h = (rotl(v1, 1, 32) + rotl(v2, 7, 32) + rotl(v3, 12, 32) + rotl(v4, 18, 32)) & M32
    else:
        h = (seed + P5) & M32
        i = 0
    h = (h + n) & M32
    while i + 4 <= n:
        h = (h + int.from_bytes(data[i:i + 4], "little") * P3) & M32
        h = (rotl(h, 17, 32) * P4) & M32
        i += 4
    while i < n:
        h = (h + data[i] * P5) & M32
        h = (rotl(h, 11, 32) * P1) & M32
        i += 1
    h ^= h >> 15
    h = (h * P2) & M32
    h ^= h >> 13
    h = (h * P3) & M32
    h ^= h >> 16
    return h


# ---------------- siphash-2-4 ----------------
def siphash24(data: bytes, key: bytes = b"\x00" * 16) -> int:
    k0 = int.from_bytes(key[0:8], "little")
    k1 = int.from_bytes(key[8:16], "little")
    v0 = 0x736F6D6570736575 ^ k0
    v1 = 0x646F72616E646F6D ^ k1
    v2 = 0x6C7967656E657261 ^ k0
    v3 = 0x7465646279746573 ^ k1

    def rnd():
        nonlocal v0, v1, v2, v3
        v0 = (v0 + v1) & M64
        v1 = rotl(v1, 13)
        v1 ^= v0
        v0 = rotl(v0, 32)
        v2 = (v2 + v3) & M64
        v3 = rotl(v3, 16)
        v3 ^= v2
        v0 = (v0 + v3) & M64
        v3 = rotl(v3, 21)
        v3 ^= v0
        v2 = (v2 + v1) & M64
        v1 = rotl(v1, 17)
        v1 ^= v2
        v2 = rotl(v2, 32)

    n = len(data)
    i = 0
    while i + 8 <= n:
        m = int.from_bytes(data[i:i + 8], "little")
        v3 ^= m
        rnd()
        rnd()
        v0 ^= m
        i += 8
    tail = data[i:]
    last = ((n & 0xFF) << 56) | int.from_bytes(tail + b"\x00" * (7 - len(tail)), "little")
    v3 ^= last
    rnd()
    rnd()
    v0 ^= last
    v2 ^= 0xFF
    for _ in range(4):
        rnd()
    return (v0 ^ v1 ^ v2 ^ v3) & M64


# ---------------- murmur2 64 ----------------
def murmur2_64(data: bytes, seed: int = 0) -> int:
    m = 0xC6A4A7935BD1E995
    r = 47
    h = (seed ^ (len(data) * m)) & M64
    n = len(data) // 8
    for i in range(n):
        k = int.from_bytes(data[i * 8:i * 8 + 8], "little")
        k = (k * m) & M64
        k ^= k >> r
        k = (k * m) & M64
        h ^= k
        h = (h * m) & M64
    tail = data[n * 8:]
    if tail:
        h ^= int.from_bytes(tail, "little")
        h = (h * m) & M64
    h ^= h >> r
    h = (h * m) & M64
    h ^= h >> r
    return h


# ---------------- murmur3 x64 128 ----------------
def murmur3_x64_128(data: bytes, seed: int = 0):
    c1 = 0x87C37B91114253D5
    c2 = 0x4CF5AD432745937F
    h1 = seed & M64
    h2 = seed & M64

    def fmix(k):
        k ^= k >> 33
        k = (k * 0xFF51AFD7ED558CCD) & M64
        k ^= k >> 33
        k = (k * 0xC4CEB9FE1A85EC53) & M64
        k ^= k >> 33
        return k

    nb = len(data) // 16
    for i in range(nb):
        k1 = int.from_bytes(data[i * 16:i * 16 + 8], "little")
        k2 = int.from_bytes(data[i * 16 + 8:i * 16 + 16], "little")
        k1 = (k1 * c1) & M64
        k1 = rotl(k1, 31)
        k1 = (k1 * c2) & M64
        h1 ^= k1
        h1 = rotl(h1, 27)
        h1 = (h1 + h2) & M64
        h1 = (h1 * 5 + 0x52DCE729) & M64
        k2 = (k2 * c2) & M64
        k2 = rotl(k2, 33)
        k2 = (k2 * c1) & M64
        h2 ^= k2
        h2 = rotl(h2, 31)
        h2 = (h2 + h1) & M64
        h2 = (h2 * 5 + 0x38495AB5) & M64
    tail = data[nb * 16:]
    k1 = k2 = 0
    for i in range(len(tail) - 1, -1, -1):
        if i >= 8:
            k2 ^= tail[i] << (8 * (i - 8))
        else:
            k1 ^= tail[i] << (8 * i)
    if len(tail) > 8:
        k2 = (k2 * c2) & M64
        k2 = rotl(k2, 33)
        k2 = (k2 * c1) & M64
        h2 ^= k2
    if len(tail) > 0:
        k1 = (k1 * c1) & M64
        k1 = rotl(k1, 31)
        k1 = (k1 * c2) & M64
        h1 ^= k1
    h1 ^= len(data)
    h2 ^= len(data)
    h1 = (h1 + h2) & M64
    h2 = (h2 + h1) & M64
    h1 = fmix(h1)
    h2 = fmix(h2)
    h1 = (h1 + h2) & M64
    h2 = (h2 + h1) & M64
    return h1, h2


def main():
    anchors = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
    items = [(a["type"], a["hash"]) for a in anchors]
    keys = {k for _, k in items}
    print(f"anchors {len(items)} distinct keys {len(keys)}")

    fams = {}
    fams["xxh64_lo32"] = lambda b: xxh64(b) & M32
    fams["xxh64_hi32"] = lambda b: (xxh64(b) >> 32) & M32
    fams["xxh32"] = lambda b: xxh32(b)
    fams["siphash24_lo32"] = lambda b: siphash24(b) & M32
    fams["siphash24_hi32"] = lambda b: (siphash24(b) >> 32) & M32
    fams["murmur2_64_lo32"] = lambda b: murmur2_64(b) & M32
    fams["murmur2_64_hi32"] = lambda b: (murmur2_64(b) >> 32) & M32
    fams["murmur3_128_h1_lo32"] = lambda b: murmur3_x64_128(b)[0] & M32
    fams["murmur3_128_h1_hi32"] = lambda b: (murmur3_x64_128(b)[0] >> 32) & M32
    fams["murmur3_128_h2_lo32"] = lambda b: murmur3_x64_128(b)[1] & M32
    fams["murmur3_128_h2_hi32"] = lambda b: (murmur3_x64_128(b)[1] >> 32) & M32

    res = {}
    for name, fn in fams.items():
        hit = 0
        for n, k in items:
            if fn(n.encode()) == k:
                hit += 1
        res[name] = hit
        print(f"  {name:22s} hits {hit}/{len(items)}")

    # also test the reverse-direction (bytes reversed) variants of xxh64
    hit = sum(1 for n, k in items if (xxh64(n.encode()[::-1]) & M32) == k)
    res["xxh64_lo32_reversed"] = hit
    print(f"  {'xxh64_lo32_reversed':22s} hits {hit}/{len(items)}")

    out = {"generated": "p2_crack6.py",
           "anchor_count": len(items),
           "families": res,
           "verdict": "no family produced a single hit on the 2499 verified "
                      "(type name -> key) anchors" if sum(res.values()) == 0 else "HIT FOUND",
           "coverage_note": "each family is evaluated on all anchors; a single correct hit "
                            "would have identified the scheme"}
    (HERE / "p2_crack6.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
    print("wrote p2_crack6.json")


if __name__ == "__main__":
    main()
