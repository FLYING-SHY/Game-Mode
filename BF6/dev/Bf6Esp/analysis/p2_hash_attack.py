#!/usr/bin/env python3
"""p2_hash_attack v2 - crack the Frostbite name hash (padding-correct).

KEY FIX vs v1: names are grouped by exact byte length so that hash functions
never see trailing NUL padding (CRC/FNV/murmur are length sensitive).

Hash universe : u32 values at *specific* record slots
                typeinfo u32[0] (23701 recs), fieldinf u32[0..10] @0x30.
Name universe : every identifier-like NUL-terminated ASCII string in .rdata.
Noise floor   : |names| * |hashes| / 2**32 expected random coincidences.
"""
import json
import re
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402

IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}$")

# ---------------------------------------------------------------- families
def crc32_table(poly, reflected=True):
    t = np.zeros(256, dtype=np.uint32)
    if reflected:
        for i in range(256):
            c = i
            for _ in range(8):
                c = (c >> 1) ^ (poly if c & 1 else 0)
            t[i] = c
    else:
        for i in range(256):
            c = i << 24
            for _ in range(8):
                c = ((c << 1) ^ poly) & 0xFFFFFFFF if c & 0x80000000 else (c << 1) & 0xFFFFFFFF
            t[i] = c
    return t


def crc32_refl(mat, table, init=0xFFFFFFFF, xorout=0xFFFFFFFF):
    n, L = mat.shape
    c = np.full(n, init, dtype=np.uint32)
    for j in range(L):
        c = table[(c ^ mat[:, j]) & 0xFF] ^ (c >> 8)
    return c ^ np.uint32(xorout)


def crc32_nonrefl(mat, table, init=0xFFFFFFFF, xorout=0xFFFFFFFF):
    n, L = mat.shape
    c = np.full(n, init, dtype=np.uint32)
    for j in range(L):
        c = ((c << np.uint32(8)) & np.uint32(0xFFFFFFFF)) ^ table[((c >> np.uint32(24)) ^ mat[:, j]) & 0xFF]
    return c ^ np.uint32(xorout)


def fnv1a32(mat):
    h = np.full(mat.shape[0], 0x811C9DC5, dtype=np.uint32)
    for j in range(mat.shape[1]):
        h = (h ^ mat[:, j]) * np.uint32(16777619)
    return h


def fnv1_32(mat):
    h = np.full(mat.shape[0], 0x811C9DC5, dtype=np.uint32)
    for j in range(mat.shape[1]):
        h = (h * np.uint32(16777619)) ^ mat[:, j]
    return h


def fnv1a64(mat):
    h = np.full(mat.shape[0], 0xCBF29CE484222325, dtype=np.uint64)
    for j in range(mat.shape[1]):
        h = (h ^ mat[:, j].astype(np.uint64)) * np.uint64(1099511628211)
    return h


def fnv1_64(mat):
    h = np.full(mat.shape[0], 0xCBF29CE484222325, dtype=np.uint64)
    for j in range(mat.shape[1]):
        h = (h * np.uint64(1099511628211)) ^ mat[:, j].astype(np.uint64)
    return h


def polymul(mat, mul, init, add=0):
    h = np.full(mat.shape[0], np.uint32(init), dtype=np.uint32)
    for j in range(mat.shape[1]):
        h = (h * np.uint32(mul)) + mat[:, j].astype(np.uint32) + np.uint32(add)
    return h


def jenkins_oat(mat):
    h = np.zeros(mat.shape[0], dtype=np.uint32)
    for j in range(mat.shape[1]):
        h = (h + mat[:, j].astype(np.uint32)) & np.uint32(0xFFFFFFFF)
        h = (h + (h << np.uint32(10))) & np.uint32(0xFFFFFFFF)
        h = (h ^ (h >> np.uint32(6))) & np.uint32(0xFFFFFFFF)
    h = (h + (h << np.uint32(3))) & np.uint32(0xFFFFFFFF)
    h = (h ^ (h >> np.uint32(11))) & np.uint32(0xFFFFFFFF)
    h = (h + (h << np.uint32(15))) & np.uint32(0xFFFFFFFF)
    return h


def murmur3_32(mat, seed=0):
    h = np.full(mat.shape[0], np.uint32(seed), dtype=np.uint32)
    c1 = np.uint32(0xcc9e2d51)
    c2 = np.uint32(0x1b873593)
    for j in range(mat.shape[1]):
        k = mat[:, j].astype(np.uint32)
        k = (k * c1) & np.uint32(0xFFFFFFFF)
        k = ((k << np.uint32(15)) | (k >> np.uint32(17))) & np.uint32(0xFFFFFFFF)
        k = (k * c2) & np.uint32(0xFFFFFFFF)
        h = (h ^ k) & np.uint32(0xFFFFFFFF)
        h = ((h << np.uint32(13)) | (h >> np.uint32(19))) & np.uint32(0xFFFFFFFF)
        h = (h * np.uint32(5) + np.uint32(0xe6546b64)) & np.uint32(0xFFFFFFFF)
    h = (h ^ np.uint32(mat.shape[1])) & np.uint32(0xFFFFFFFF)
    h = (h ^ (h >> np.uint32(16))) & np.uint32(0xFFFFFFFF)
    h = (h * np.uint32(0x85ebca6b)) & np.uint32(0xFFFFFFFF)
    h = (h ^ (h >> np.uint32(13))) & np.uint32(0xFFFFFFFF)
    h = (h * np.uint32(0xc2b2ae35)) & np.uint32(0xFFFFFFFF)
    h = (h ^ (h >> np.uint32(16))) & np.uint32(0xFFFFFFFF)
    return h


def murmur2_32_py(names):
    """pure-python MurmurHash2A 32-bit"""
    out = []
    M = 0x5bd1e995
    for s in names:
        l = len(s)
        h = (l * M) & 0xFFFFFFFF
        i = 0
        while l - i >= 4:
            k = int.from_bytes(s[i:i + 4], "little")
            k = (k * M) & 0xFFFFFFFF
            k ^= k >> 24
            k = (k * M) & 0xFFFFFFFF
            h = (h * M) & 0xFFFFFFFF
            h ^= k
            i += 4
        rem = s[i:]
        if rem:
            k = int.from_bytes(rem + b"\x00" * (4 - len(rem)), "little")
            h = (h * M) & 0xFFFFFFFF
            h ^= k
        h ^= h >> 13
        h = (h * M) & 0xFFFFFFFF
        h ^= h >> 15
        out.append(h)
    return np.array(out, dtype=np.uint32)


def xxh32_py(names, seed=0):
    P1, P2, P3, P4, P5 = 2654435761, 2246822519, 3266489917, 668265263, 374761393
    M = 0xFFFFFFFF
    out = []
    for s in names:
        l = len(s)
        if l >= 16:
            v1 = (seed + P1 + P2) & M
            v2 = (seed + P2) & M
            v3 = seed & M
            v4 = (seed - P1) & M
            i = 0
            while l - i >= 16:
                for vi in range(4):
                    k = int.from_bytes(s[i:i + 4], "little")
                    v = [v1, v2, v3, v4][vi]
                    v = (v + k * P2) & M
                    v = ((v << 13) | (v >> 19)) & M
                    v = (v * P1) & M
                    if vi == 0: v1 = v
                    elif vi == 1: v2 = v
                    elif vi == 2: v3 = v
                    else: v4 = v
                i += 16
            h = (((v1 << 1) | (v1 >> 31)) + ((v2 << 7) | (v2 >> 25)) +
                 ((v3 << 12) | (v3 >> 20)) + ((v4 << 18) | (v4 >> 14))) & M
        else:
            h = (seed + P5) & M
            i = 0
        h = (h + l) & M
        while l - i >= 4:
            k = int.from_bytes(s[i:i + 4], "little")
            h = (h + k * P3) & M
            h = (((h << 17) | (h >> 15)) * P4) & M
            i += 4
        while i < l:
            h = (h + s[i] * P5) & M
            h = (((h << 11) | (h >> 21)) * P1) & M
            i += 1
        h ^= h >> 15
        h = (h * P2) & M
        h ^= h >> 13
        h = (h * P3) & M
        h ^= h >> 16
        out.append(h)
    return np.array(out, dtype=np.uint32)


def sip24_py(names, k0=0, k1=0):
    out = []
    for s in names:
        v0 = k0 ^ (0x736f6d6570736575 ^ len(s))
        v1 = k1 ^ 0x646f72616e646f6d
        v2 = k0 ^ 0x6c7967656e657261
        v3 = k1 ^ 0x7465646279746573
        b = s + b"\x00" * ((8 - len(s) % 8) % 8)
        for i in range(0, len(b), 8):
            m = int.from_bytes(b[i:i + 8], "little")
            v3 ^= m
            v0 = (v0 + v1) & 0xFFFFFFFFFFFFFFFF
            v1 = ((v1 << 13) | (v1 >> 51)) & 0xFFFFFFFFFFFFFFFF
            v1 ^= v0
            v0 = ((v0 << 32) | (v0 >> 32)) & 0xFFFFFFFFFFFFFFFF
            v2 = (v2 + v3) & 0xFFFFFFFFFFFFFFFF
            v3 = ((v3 << 16) | (v3 >> 48)) & 0xFFFFFFFFFFFFFFFF
            v3 ^= v2
            v0 = (v0 + v3) & 0xFFFFFFFFFFFFFFFF
            v2 = ((v2 << 21) | (v2 >> 43)) & 0xFFFFFFFFFFFFFFFF
            v2 ^= v0
            v1 = (v1 + v2) & 0xFFFFFFFFFFFFFFFF
            v2 = ((v2 << 17) | (v2 >> 47)) & 0xFFFFFFFFFFFFFFFF
            v2 ^= v1
            v3 = (v3 + v2) & 0xFFFFFFFFFFFFFFFF
            v2 = ((v2 << 32) | (v2 >> 32)) & 0xFFFFFFFFFFFFFFFF
        v0 ^= 0xff
        out.append((v0 ^ v1 ^ v2 ^ v3) & 0xFFFFFFFF)
    return np.array(out, dtype=np.uint32)


def build_family_dict(std, cast, koop):
    f = {}
    f["crc32_refl"] = lambda m: crc32_refl(m, std, 0xFFFFFFFF, 0xFFFFFFFF)
    f["crc32_refl_init0"] = lambda m: crc32_refl(m, std, 0, 0)
    f["crc32_refl_init0_xorFF"] = lambda m: crc32_refl(m, std, 0, 0xFFFFFFFF)
    f["crc32_refl_init0_xorFF_noout"] = lambda m: crc32_refl(m, std, 0, 0xFFFFFFFF)
    f["crc32_nonrefl"] = lambda m: crc32_nonrefl(m, koop, 0xFFFFFFFF, 0xFFFFFFFF)
    f["crc32_nonrefl_init0"] = lambda m: crc32_nonrefl(m, koop, 0, 0)
    f["crc32c_refl"] = lambda m: crc32_refl(m, cast, 0xFFFFFFFF, 0xFFFFFFFF)
    f["crc32c_refl_init0"] = lambda m: crc32_refl(m, cast, 0, 0)
    f["fnv1a32"] = fnv1a32
    f["fnv1_32"] = fnv1_32
    f["fnv1a64_lo32"] = lambda m: (fnv1a64(m) & np.uint64(0xFFFFFFFF)).astype(np.uint32)
    f["fnv1a64_hi32"] = lambda m: (fnv1a64(m) >> np.uint64(32)).astype(np.uint32)
    f["fnv1_64_lo32"] = lambda m: (fnv1_64(m) & np.uint64(0xFFFFFFFF)).astype(np.uint32)
    f["fnv1_64_hi32"] = lambda m: (fnv1_64(m) >> np.uint64(32)).astype(np.uint32)
    f["djb2"] = lambda m: polymul(m, 33, 5381)
    f["sdbm"] = lambda m: polymul(m, 65599, 0)
    f["poly31_7"] = lambda m: polymul(m, 31, 0, 7)
    f["poly31_0"] = lambda m: polymul(m, 31, 0, 0)
    f["poly33_0"] = lambda m: polymul(m, 33, 0, 0)
    f["poly131_0"] = lambda m: polymul(m, 131, 0, 0)
    f["poly16777619_0"] = lambda m: polymul(m, 16777619, 0, 0)
    f["elf"] = lambda m: polymul(m, 16, 0, 0)
    f["poly37_0"] = lambda m: polymul(m, 37, 0, 0)
    f["poly17_0"] = lambda m: polymul(m, 17, 0, 0)
    f["jenkins_oat"] = jenkins_oat
    f["murmur3_32"] = murmur3_32
    return f


def main():
    img = Img()
    data = img.data
    rd = img.blob(".rdata")

    names = []
    i = 0
    while True:
        j = rd.find(b"\x00", i)
        if j < 0:
            break
        s = rd[i:j]
        if 1 <= len(s) <= 64 and IDENT.match(s):
            names.append(s)
        i = j + 1
    names = sorted(set(names))
    refl = [s for s in names if re.match(rb"^[A-Z][A-Za-z0-9_]{2,40}$", s)]
    print(f"identifiers={len(names)} camelcase={len(refl)}")

    def group_by_len(lst):
        g = {}
        for s in lst:
            g.setdefault(len(s), []).append(s)
        out = []
        for L in sorted(g):
            grp = g[L]
            m = np.frombuffer(b"".join(grp), dtype=np.uint8).reshape(len(grp), L)
            out.append((L, m, grp))
        return out

    VARIANTS = {
        "raw": names,
        "lower": [s.lower() for s in names],
        "upper": [s.upper() for s in names],
        "raw+nul": [s + b"\x00" for s in names],
        "lower+nul": [s.lower() + b"\x00" for s in names],
    }
    UNIV = {"ident": names, "camel": refl}

    ts = img.sec["typeinfo"]
    tb = data[ts["roff"]:ts["roff"] + ts["rsize"]]
    t32 = np.frombuffer(tb, dtype="<u4")
    mk = np.nonzero(t32 == 0x00080089)[0]
    ti_hash = np.unique(t32[mk - 1])

    fs = img.sec["fieldinf"]
    fb = data[fs["roff"]:fs["roff"] + fs["rsize"]]
    f32 = np.frombuffer(fb, dtype="<u4")
    STR = 0x30
    nrec = len(fb) // STR
    fields = {j: f32[j:nrec * (STR // 4):STR // 4] for j in range(STR // 4)}
    hash_sets = {"typeinfo_u32_0": ti_hash}
    for j in (0, 2, 4, 6, 8, 10):
        hash_sets[f"fieldinf_u32_{j}"] = np.unique(fields[j])
    hash_sets["fieldinf_all_even"] = np.unique(
        np.concatenate([fields[j] for j in (0, 2, 4, 6, 8, 10)]))
    hash_sets["typeinfo_all"] = np.unique(t32)
    for k, v in hash_sets.items():
        print(f"  hashset {k:20s} {len(v)}")

    std = crc32_table(0xEDB88320, True)
    cast = crc32_table(0x82F63B78, True)
    koop = crc32_table(0x04C11DB7, False)
    fams = build_family_dict(std, cast, koop)

    results = []
    significant = []
    for uname, ulist in UNIV.items():
        for vname, vlist in VARIANTS.items():
            groups = group_by_len(vlist)
            for fname, fn in fams.items():
                hv = np.zeros(len(vlist), dtype=np.uint32)
                idx = 0
                for L, m, grp in groups:
                    hv[idx:idx + len(grp)] = fn(m)
                    idx += len(grp)
                for hname, hset in hash_sets.items():
                    pos = np.clip(np.searchsorted(hset, hv), 0, len(hset) - 1)
                    hitmask = hset[pos] == hv
                    nh = int(hitmask.sum())
                    floor = len(vlist) * len(hset) / 2 ** 32
                    rec = {"universe": uname, "variant": vname, "family": fname,
                           "hash_set": hname, "hits": nh, "names": len(vlist),
                           "hashes": len(hset), "noise_floor": round(floor, 4)}
                    if nh > max(3, 4 * floor):
                        ii = np.nonzero(hitmask)[0]
                        rec["pairs"] = [{"name": vlist[i].decode("latin1"),
                                         "hash": int(hv[i])} for i in ii[:60]]
                        significant.append(rec)
                    results.append(rec)
            print(f"  done {uname}/{vname}")

    # slow pure-python families on a reduced universe (camel, raw)
    reduced = refl
    groups = group_by_len(reduced)
    for fname, fn in (("murmur2_32a", murmur2_32_py), ("xxh32", xxh32_py),
                      ("siphash24_zero_key", sip24_py)):
        hv = np.zeros(len(reduced), dtype=np.uint32)
        idx = 0
        for L, m, grp in groups:
            hv[idx:idx + len(grp)] = fn(grp)
            idx += len(grp)
        for hname, hset in hash_sets.items():
            pos = np.clip(np.searchsorted(hset, hv), 0, len(hset) - 1)
            hitmask = hset[pos] == hv
            nh = int(hitmask.sum())
            floor = len(reduced) * len(hset) / 2 ** 32
            rec = {"universe": "camel", "variant": "raw", "family": fname,
                   "hash_set": hname, "hits": nh, "names": len(reduced),
                   "hashes": len(hset), "noise_floor": round(floor, 4)}
            if nh > max(3, 4 * floor):
                ii = np.nonzero(hitmask)[0]
                rec["pairs"] = [{"name": reduced[i].decode("latin1"),
                                 "hash": int(hv[i])} for i in ii[:60]]
                significant.append(rec)
            results.append(rec)
        print(f"  done {fname}")

    results.sort(key=lambda r: -r["hits"])
    print("\n=== top 20 by hits ===")
    for r in results[:20]:
        print(f"  {r['family']:24s} {r['universe']:7s} {r['variant']:10s} "
              f"{r['hash_set']:20s} hits={r['hits']:6d} floor={r['noise_floor']:.3f}")
    print(f"\n=== significant count: {len(significant)} ===")
    for r in significant[:40]:
        print(f"  {r['family']:24s} {r['universe']:7s} {r['variant']:10s} "
              f"{r['hash_set']:20s} hits={r['hits']} floor={r['noise_floor']:.3f}")
        for p in r.get("pairs", [])[:8]:
            print("      ", p)

    json.dump({"n_ident": len(names), "n_camel": len(refl),
               "hash_sets": {k: len(v) for k, v in hash_sets.items()},
               "results": results, "significant": significant},
              open(OUTDIR / "p2_hash_attack.json", "w"), indent=1)
    print("\nwritten p2_hash_attack.json")


if __name__ == "__main__":
    main()
