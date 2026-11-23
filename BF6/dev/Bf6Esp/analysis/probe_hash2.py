#!/usr/bin/env python3
"""Empirical recovery of the Frostbite property/type name hash.

Strategy (no guessing, no external docs):
  1. Harvest every identifier-looking string from .rdata (the reflection blob).
  2. Build candidate hash universes from the custom `typeinfo` / `fieldinf`
     sections (u32 at record starts and at every 4-byte slot).
  3. For each digest family (crc32/crc32c/crc64/fnv/murmur/xxhash/md5/sha1/jenkins/...)
     hash every harvested name and count how many land inside a universe.
  4. The family with a non-zero, self-consistent hit count is the answer.

Output: analysis/hash_probe2.json  (family -> hits, sample name->digest pairs)
"""
import hashlib
import json
import re
import struct
import zlib
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUT = Path(__file__).with_name("hash_probe2.json")

CRC32_POLY_REFLECT = 0xEDB88320
CRC32C_POLY_REFLECT = 0x82F63B78
CRC32_POLY_NORMAL = 0x04C11DB7
CRC32C_POLY_NORMAL = 0x1EDC6F41


def parse_sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    nsec = struct.unpack_from("<H", data, pe + 6)[0]
    opt = struct.unpack_from("<H", data, pe + 20)[0]
    base = pe + 24 + opt
    out = []
    for i in range(nsec):
        o = base + i * 40
        name = data[o:o + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, o + 8)
        out.append(dict(name=name, va=vaddr, vsize=vsize, roff=roff, rsize=rsize))
    return out


def crc_generic(data, poly, init, xorout, reflect):
    if reflect:
        crc = init
        for c in data:
            crc ^= c
            for _ in range(8):
                crc = (crc >> 1) ^ (poly & -(crc & 1))
        return (crc ^ xorout) & 0xFFFFFFFF
    crc = init
    for c in data:
        crc ^= c << 24
        for _ in range(8):
            crc = ((crc << 1) ^ (poly & -(crc & 0x80000000))) & 0xFFFFFFFF
    return (crc ^ xorout) & 0xFFFFFFFF


def crc64_generic(data, poly, init, xorout, reflect):
    mask = 0xFFFFFFFFFFFFFFFF
    crc = init
    if reflect:
        for c in data:
            crc ^= c
            for _ in range(8):
                crc = (crc >> 1) ^ (poly & -(crc & 1))
        return (crc ^ xorout) & mask
    for c in data:
        crc ^= c << 56
        for _ in range(8):
            crc = ((crc << 1) ^ (poly & -(crc & 0x8000000000000000))) & mask
    return (crc ^ xorout) & mask


def murmur3_32(key, seed=0):
    c1, c2 = 0xCC9E2D51, 0x1B873593
    h = seed
    n = len(key)
    for i in range(0, n - n % 4, 4):
        k = struct.unpack_from("<I", key, i)[0]
        k = (k * c1) & 0xFFFFFFFF
        k = ((k << 15) | (k >> 17)) & 0xFFFFFFFF
        k = (k * c2) & 0xFFFFFFFF
        h ^= k
        h = ((h << 13) | (h >> 19)) & 0xFFFFFFFF
        h = (h * 5 + 0xE6546B64) & 0xFFFFFFFF
    k = 0
    tail = key[n - n % 4:]
    if len(tail) == 3:
        k ^= tail[2] << 16
    if len(tail) >= 2:
        k ^= tail[1] << 8
    if len(tail) >= 1:
        k ^= tail[0]
        k = (k * c1) & 0xFFFFFFFF
        k = ((k << 15) | (k >> 17)) & 0xFFFFFFFF
        k = (k * c2) & 0xFFFFFFFF
        h ^= k
    h ^= n
    h ^= h >> 16
    h = (h * 0x85EBCA6B) & 0xFFFFFFFF
    h ^= h >> 13
    h = (h * 0xC2B2AE35) & 0xFFFFFFFF
    h ^= h >> 16
    return h


def xxhash32(data, seed=0):
    P1, P2, P3, P4, P5 = 2654435761, 2246822519, 3266489917, 668265263, 374761393
    M = 0xFFFFFFFF

    def rotl(x, r):
        return ((x << r) | (x >> (32 - r))) & M

    n = len(data)
    if n >= 16:
        v1 = (seed + P1 + P2) & M
        v2 = (seed + P2) & M
        v3 = seed & M
        v4 = (seed - P1) & M
        i = 0
        while i <= n - 16:
            for vi, off in ((1, 0), (2, 4), (3, 8), (4, 12)):
                k = struct.unpack_from("<I", data, i + off)[0]
                if vi == 1:
                    v1 = (rotl((v1 + k * P2) & M, 13) * P1) & M
                elif vi == 2:
                    v2 = (rotl((v2 + k * P2) & M, 13) * P1) & M
                elif vi == 3:
                    v3 = (rotl((v3 + k * P2) & M, 13) * P1) & M
                else:
                    v4 = (rotl((v4 + k * P2) & M, 13) * P1) & M
            i += 16
        h = (rotl(v1, 1) + rotl(v2, 7) + rotl(v3, 12) + rotl(v4, 18)) & M
    else:
        h = (seed + P5) & M
        i = 0
    h = (h + n) & M
    while i + 4 <= n:
        k = struct.unpack_from("<I", data, i)[0]
        h = (h + (k * P3) & M) & M
        h = (rotl(h, 17) * P4) & M
        i += 4
    while i < n:
        h = (h + data[i] * P5) & M
        h = (rotl(h, 11) * P1) & M
        i += 1
    h ^= h >> 15
    h = (h * P2) & M
    h ^= h >> 13
    h = (h * P3) & M
    h ^= h >> 16
    return h


def fnv32(data, seed, a_prime):
    h = seed
    for c in data:
        h = ((h ^ c) * a_prime) & 0xFFFFFFFF
    return h


def fnv32_alt(data, seed, a_prime):
    h = seed
    for c in data:
        h = ((h * a_prime) & 0xFFFFFFFF) ^ c
    return h


def jenkins_oaat(data):
    h = 0
    for c in data:
        h = (h + c) & 0xFFFFFFFF
        h = (h + (h << 10)) & 0xFFFFFFFF
        h ^= h >> 6
    h = (h + (h << 3)) & 0xFFFFFFFF
    h ^= h >> 11
    h = (h + (h << 15)) & 0xFFFFFFFF
    return h


def elf_cksum(data):
    c = 0
    for b in data:
        c = (c + b) & 0xFFFFFFFF
        c = (c + (c << 8)) & 0xFFFFFFFF
    return c


def build_families():
    """name -> callable(data)->int  (32-bit families)"""
    fams = {}
    for tag, poly, refl in (("crc32", CRC32_POLY_REFLECT, True),
                            ("crc32n", CRC32_POLY_NORMAL, False),
                            ("crc32c", CRC32C_POLY_REFLECT, True),
                            ("crc32cn", CRC32C_POLY_NORMAL, False)):
        for init in (0x00000000, 0xFFFFFFFF, 0x12345678, 0xDEADBEEF):
            for xo in (0x00000000, 0xFFFFFFFF):
                fams[f"{tag}:init={init:08x}:xo={xo:08x}"] = (
                    lambda d, p=poly, r=refl, i=init, x=xo: crc_generic(d, p, i, x, r))
    for seed in (0, 0x811C9DC5, 2166136261, 0x1505):
        for tag, fn in (("fnv1a", fnv32), ("fnv1", fnv32_alt)):
            fams[f"{tag}:seed={seed:08x}"] = (lambda d, s=seed, f=fn: f(d, s, 0x01000193))
    fams["murmur3_32:seed0"] = lambda d: murmur3_32(d, 0)
    fams["murmur3_32:seed1"] = lambda d: murmur3_32(d, 1)
    fams["xxhash32:seed0"] = lambda d: xxhash32(d, 0)
    fams["jenkins_oaat"] = jenkins_oaat
    fams["elf_cksum"] = elf_cksum
    fams["adler32"] = lambda d: zlib.adler32(d) & 0xFFFFFFFF
    fams["crc32_std_zlib"] = lambda d: zlib.crc32(d) & 0xFFFFFFFF
    # md5/sha1/sha256 prefix32
    fams["md5_le32"] = lambda d: struct.unpack("<I", hashlib.md5(d).digest()[:4])[0]
    fams["md5_be32"] = lambda d: struct.unpack(">I", hashlib.md5(d).digest()[:4])[0]
    fams["sha1_le32"] = lambda d: struct.unpack("<I", hashlib.sha1(d).digest()[:4])[0]
    fams["sha256_le32"] = lambda d: struct.unpack("<I", hashlib.sha256(d).digest()[:4])[0]
    fams["blake2b_le32"] = lambda d: struct.unpack("<I", hashlib.blake2b(d, digest_size=8).digest()[:4])[0]
    return fams


def build_families64():
    fams = {}
    for tag, poly, refl in (("crc64ecma_ref", 0xC96C5795D7870F42, True),
                             ("crc64ecma", 0x42F0E1EBA9EA3693, False),
                             ("crc64iso_ref", 0x95AC9329AC4BC9B5, True),
                             ("crc64iso", 0x000000000000001B, False)):
        for init in (0, 0xFFFFFFFFFFFFFFFF):
            for xo in (0, 0xFFFFFFFFFFFFFFFF):
                fams[f"{tag}:init={init:016x}:xo={xo:016x}"] = (
                    lambda d, p=poly, r=refl, i=init, x=xo: crc64_generic(d, p, i, x, r))
    fams["fnv1a64"] = lambda d: (lambda h: h)(_fnv1a64(d))
    fams["fnv1a64_lower"] = lambda d: _fnv1a64(d.lower())
    fams["md5_le64"] = lambda d: struct.unpack("<Q", hashlib.md5(d).digest()[:8])[0]
    fams["sha1_le64"] = lambda d: struct.unpack("<Q", hashlib.sha1(d).digest()[:8])[0]
    return fams


def _fnv1a64(data):
    h = 0xCBF29CE484222325
    for c in data:
        h ^= c
        h = (h * 0x100000001B3) & 0xFFFFFFFFFFFFFFFF
    return h


def main():
    data = GAME.read_bytes()
    secs = parse_sections(data)
    sec = {s["name"]: s for s in secs}

    # ---- 1. harvest identifier names from .rdata --------------------------
    rd = sec[".rdata"]
    rblob = data[rd["roff"]:rd["roff"] + rd["rsize"]]
    name_re = re.compile(rb"[A-Za-z_][A-Za-z0-9_]{3,63}")
    names = set()
    for m in name_re.finditer(rblob):
        names.add(m.group().decode("ascii"))
    names = sorted(names)
    print("candidate names in .rdata:", len(names))

    # ---- 2. universes ------------------------------------------------------
    def u32_universe(sname):
        s = sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        return {struct.unpack_from("<I", blob, i)[0] for i in range(0, len(blob) - 4, 4)}

    def u64_universe(sname):
        s = sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        return {struct.unpack_from("<Q", blob, i)[0] for i in range(0, len(blob) - 8, 8)}

    ti32 = u32_universe("typeinfo")
    fi32 = u32_universe("fieldinf")
    ti64 = u64_universe("typeinfo")
    fi64 = u64_universe("fieldinf")
    universe32 = ti32 | fi32
    universe64 = ti64 | fi64
    print("universe32:", len(universe32), "universe64:", len(universe64))

    # also: strings in .rdata that are *inside* typeinfo-like tables? not needed.

    results = {"name_count": len(names),
               "universe32": len(universe32),
               "universe64": len(universe64),
               "fams32": {}, "fams64": {}}

    # ---- 3. 32-bit families ----------------------------------------------
    fams = build_families()
    for fam, fn in fams.items():
        hits = 0
        samples = []
        for nm in names:
            v = fn(nm.encode())
            if v in universe32:
                hits += 1
                if len(samples) < 5:
                    samples.append((nm, hex(v)))
        if hits:
            results["fams32"][fam] = {"hits": hits, "samples": samples}
        print(f"32 {fam}: {hits}")

    # ---- 4. 64-bit families ----------------------------------------------
    fams64 = build_families64()
    for fam, fn in fams64.items():
        hits = 0
        samples = []
        for nm in names:
            v = fn(nm.encode())
            if v in universe64:
                hits += 1
                if len(samples) < 5:
                    samples.append((nm, hex(v)))
        if hits:
            results["fams64"][fam] = {"hits": hits, "samples": samples}
        print(f"64 {fam}: {hits}")

    # ---- 5. lowercase/uppercase variants of the best families -------------
    best32 = sorted(results["fams32"].items(), key=lambda kv: -kv[1]["hits"])[:5]
    best64 = sorted(results["fams64"].items(), key=lambda kv: -kv[1]["hits"])[:5]
    results["best32"] = best32
    results["best64"] = best64

    OUT.write_text(json.dumps(results, indent=2), encoding="utf-8")
    print("written:", OUT)


if __name__ == "__main__":
    main()
