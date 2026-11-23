"""CRACK THE HASH using the anchor pairs (type name -> static typeinfo u32[0]).

2499 verified (name, hash) pairs from registration stubs.  A correct scheme must
match on ALL anchors (or on a large fraction if the hash is salted per-type).
"""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
pairs = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
print("anchors:", len(pairs))
pairs = [p for p in pairs if p["type"] and p["hash"]]
print("usable:", len(pairs))

# de-dup by name (keep first)
seen = {}
for p in pairs:
    if p["type"] not in seen:
        seen[p["type"]] = p
anchors = list(seen.values())
print("distinct names:", len(anchors))
names = [a["type"] for a in anchors]
mods = [a["module"] or "" for a in anchors]
hashes = np.array([a["hash"] for a in anchors], dtype=np.uint64)


def bytes_of(s):
    return s.encode("latin1", "ignore")


def h_djb2_add(s, init=5381):
    h = init
    for c in bytes_of(s):
        h = (h * 33 + c) & 0xFFFFFFFF
    return h


def h_djb2_xor(s, init=5381):
    h = init
    for c in bytes_of(s):
        h = ((h * 33) ^ c) & 0xFFFFFFFF
    return h


def h_djb2_xor64(s, init=5381):
    h = init
    for c in bytes_of(s):
        h = ((h * 33) ^ c) & 0xFFFFFFFFFFFFFFFF
    return h


def h_fnv1a32(s):
    h = 0x811C9DC5
    for c in bytes_of(s):
        h = ((h ^ c) * 0x01000193) & 0xFFFFFFFF
    return h


def h_fnv1a64(s):
    h = 0xCBF29CE484222325
    for c in bytes_of(s):
        h = ((h ^ c) * 0x100000001B3) & 0xFFFFFFFFFFFFFFFF
    return h


def h_sdbm(s):
    h = 0
    for c in bytes_of(s):
        h = (c + (h << 6) + (h << 16) - h) & 0xFFFFFFFF
    return h


def h_jenkins(s):
    h = 0
    for c in bytes_of(s):
        h = (h + c) & 0xFFFFFFFF
        h = (h + (h << 10)) & 0xFFFFFFFF
        h ^= (h >> 6)
    h = (h + (h << 3)) & 0xFFFFFFFF
    h ^= (h >> 11)
    h = (h + (h << 15)) & 0xFFFFFFFF
    return h


def crc32_std(s):
    import zlib
    return zlib.crc32(bytes_of(s)) & 0xFFFFFFFF


def crc32_init0(s):
    import zlib
    return zlib.crc32(bytes_of(s), 0) & 0xFFFFFFFF


def h_poly(s, M, init=0, op="xor"):
    h = init
    for c in bytes_of(s):
        if op == "xor":
            h = ((h * M) ^ c) & 0xFFFFFFFF
        elif op == "add":
            h = ((h * M) + c) & 0xFFFFFFFF
        elif op == "xorfirst":
            h = (((h ^ c) * M)) & 0xFFFFFFFF
        elif op == "addfirst":
            h = (((h + c) * M)) & 0xFFFFFFFF
    return h


FAMS = {
    "djb2_add": h_djb2_add,
    "djb2_xor": h_djb2_xor,
    "djb2_xor64": h_djb2_xor64,
    "fnv1a32": h_fnv1a32,
    "fnv1a64": h_fnv1a64,
    "sdbm": h_sdbm,
    "jenkins_oat": h_jenkins,
    "crc32": crc32_std,
    "crc32_init0": crc32_init0,
}

FORMS = {
    "name": lambda n, m: n,
    "name_nul": lambda n, m: n + "\x00",
    "mod_name": lambda n, m: f"{m}::{n}",
    "mod_name_dot": lambda n, m: f"{m}.{n}",
    "mod_name_us": lambda n, m: f"{m}_{n}",
    "modname_name": lambda n, m: f"{m}{n}",
    "lower": lambda n, m: n.lower(),
    "upper": lambda n, m: n.upper(),
}

MULTS = [31, 33, 37, 39, 41, 131, 16777619, 65599, 131071, 17, 5, 257, 65537,
         0x01000193, 0x1000193, 0x85EBCA6B, 0xC2B2AE35, 0x9E3779B1, 0x45D9F3B,
         0x100000001B3 & 0xFFFFFFFF, 0x9E3779B1, 0xCC9E2D51, 0x1B873593,
         0x5BD1E995, 0xC6A4A793, 0xB492B66F, 5381, 131, 1103515245,
         0x01000193, 0x811C9DC5, 0xDEECE66D & 0xFFFFFFFF]
INITS = [0, 1, 5381, 0x811C9DC5, 0xCBF29CE4, 0x1505, 0xDEADBEEF, 0xFFFFFFFF,
         0x1000193, 0x9E3779B9, 0x01000193, 0x811C9DC5, 0xDEECE66D, 0x27D4EB2F]

results = []
sample = list(range(0, len(anchors), max(1, len(anchors) // 60)))[:60]
for fname, f in FAMS.items():
    for formname, form in FORMS.items():
        hits = 0
        for i in sample:
            s = form(names[i], mods[i])
            if f(s) == int(hashes[i]):
                hits += 1
        if hits:
            results.append((hits, len(sample), f"{fname}/{formname}"))
            print(f"HIT {hits}/{len(sample)}  {fname}/{formname}")

for M in set(MULTS):
    for init in set(INITS):
        for op in ("xor", "add", "xorfirst", "addfirst"):
            for formname, form in FORMS.items():
                hits = 0
                for i in sample:
                    s = form(names[i], mods[i])
                    if h_poly(s, M, init, op) == int(hashes[i]):
                        hits += 1
                if hits:
                    results.append((hits, len(sample), f"poly/{op}/M={M}/init={init}/{formname}"))
                    print(f"HIT {hits}/{len(sample)}  poly/{op}/M={M}/init={init}/{formname}")

print("\nTOP:", sorted(results, reverse=True)[:20] if results else "NO HITS")

# 64-bit variants of the poly family (hash truncated to 32 in either half)
print("\n--- 64-bit poly family (truncation test) ---")
for M in (1099511628211, 31, 33, 131, 0x100000001B3, 0x9E3779B97F4A7C15):
    for init in (0, 5381, 0xCBF29CE484222325, 0x811C9DC5):
        for op in ("xor", "add", "xorfirst", "addfirst"):
            hl = hu = 0
            for i in sample:
                h = init
                for c in bytes_of(names[i]):
                    if op == "xor":
                        h = ((h * M) ^ c) & 0xFFFFFFFFFFFFFFFF
                    elif op == "add":
                        h = ((h * M) + c) & 0xFFFFFFFFFFFFFFFF
                    elif op == "xorfirst":
                        h = (((h ^ c) * M)) & 0xFFFFFFFFFFFFFFFF
                    else:
                        h = (((h + c) * M)) & 0xFFFFFFFFFFFFFFFF
                exp = int(hashes[i])
                if (h & 0xFFFFFFFF) == exp:
                    hl += 1
                if ((h >> 32) & 0xFFFFFFFF) == exp:
                    hu += 1
            if hl or hu:
                print(f"HIT 64bit lo={hl} hi={hu} M={M} init={init} op={op}")
print("done")
