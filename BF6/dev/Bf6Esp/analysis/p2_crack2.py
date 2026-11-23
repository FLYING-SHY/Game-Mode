"""Vectorised brute force of the name->hash scheme using the 2499 anchor pairs.

A single exact match proves the scheme (no noise floor involved).
Grid:  h = (h OP c) * M  /  h = (h * M) OP c   with init, over many string shapes,
plus 64-bit states with 32-bit truncation (low/high) and a final avalanche mix.
"""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
pairs = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
seen = {}
for p in pairs:
    if p["type"] and p["type"] not in seen:
        seen[p["type"]] = p
anch = list(seen.values())
NAMES = [a["type"] for a in anch]
MODS = [a["module"] or "" for a in anch]
HASH = np.array([a["hash"] for a in anch], dtype=np.uint64)
print("anchors:", len(anch))

MULTS = [31, 33, 37, 39, 41, 131, 16777619, 65599, 131071, 17, 5, 257, 65537,
         0x01000193, 0x85EBCA6B, 0xC2B2AE35, 0x9E3779B1, 0x45D9F3B, 0xCC9E2D51,
         0x1B873593, 0x5BD1E995, 0xC6A4A793, 0xB492B66F, 5381, 1103515245,
         0xDEECE66D, 0x9E3779B9, 0x27D4EB2F, 0x01000193, 0x1000193, 7, 3, 129,
         0x100000001B3, 6364136223846793005 % (1 << 32)]
INITS = [0, 1, 5381, 0x811C9DC5, 0xCBF29CE4, 0x1505, 0xDEADBEEF, 0xFFFFFFFF,
         0x9E3779B9, 0x01000193, 0x811C9DC5, 0xDEECE66D, 0x27D4EB2F, 0x1000193,
         0x12345678, 0xABCDEF01, 0xFFFFFFFF - 1, 0]


def build(shape):
    """returns list of byte-lists per anchor for the given shape"""
    out = []
    for n, m in zip(NAMES, MODS):
        if shape == "name":
            s = n
        elif shape == "name_nul":
            s = n + "\x00"
        elif shape == "lower":
            s = n.lower()
        elif shape == "upper":
            s = n.upper()
        elif shape == "mod::name":
            s = f"{m}::{n}"
        elif shape == "mod.name":
            s = f"{m}.{n}"
        elif shape == "mod_name":
            s = f"{m}_{n}"
        elif shape == "modname_name":
            s = f"{m}{n}"
        elif shape == "name_mod":
            s = f"{n}::{m}"
        elif shape == "fb::name":
            s = f"fb::{n}"
        elif shape == "name_snake":
            s = "".join("_" + ch.lower() if ch.isupper() else ch for ch in n).lstrip("_")
        else:
            raise ValueError(shape)
        out.append(s.encode("latin1", "ignore"))
    return out


def vec_hash(bufs, M, init, op, bits=32):
    L = max(len(b) for b in bufs)
    mask = (1 << bits) - 1
    arr = np.zeros((len(bufs), L), dtype=np.uint64)
    for i, b in enumerate(bufs):
        if b:
            arr[i, :len(b)] = np.frombuffer(b, dtype=np.uint8)
    h = np.full(len(bufs), init, dtype=np.uint64)
    for j in range(L):
        c = arr[:, j]
        if op == "mulxor":
            h = ((h * M) ^ c) & mask
        elif op == "muladd":
            h = ((h * M) + c) & mask
        elif op == "xormul":
            h = ((h ^ c) * M) & mask
        elif op == "addmul":
            h = ((h + c) * M) & mask
        elif op == "xoradd":
            h = ((h ^ c) + c) & mask
        elif op == "addxor":
            h = ((h + c) ^ c) & mask
    return h


SHAPES = ["name", "name_nul", "lower", "upper", "mod::name", "mod.name",
          "mod_name", "modname_name", "name_mod", "fb::name", "name_snake"]
OPS = ["mulxor", "muladd", "xormul", "addmul"]

print("\n=== 32-bit sweep ===")
best = []
for shape in SHAPES:
    bufs = build(shape)
    for M in MULTS:
        for init in INITS:
            for op in OPS:
                h = vec_hash(bufs, M, init, op)
                n = int((h == HASH).sum())
                if n:
                    best.append((n, shape, M, hex(init), op))
                    print(f"  HIT {n}/{len(anch)} shape={shape} M={M} init={hex(init)} op={op}")
print("best:", sorted(best, reverse=True)[:10] if best else "NONE")

print("\n=== 64-bit sweep (low/high truncation) ===")
best64 = []
for shape in SHAPES:
    bufs = build(shape)
    for M in MULTS:
        for init in INITS:
            for op in OPS:
                h = vec_hash(bufs, M, init, op, bits=64)
                lo = (h & 0xFFFFFFFF) == HASH
                hi = ((h >> 32) & 0xFFFFFFFF) == HASH
                n = int(lo.sum()) + int(hi.sum())
                if n:
                    best64.append((n, shape, M, hex(init), op))
                    print(f"  HIT64 {n}/{len(anch)} shape={shape} M={M} init={hex(init)} op={op} lo={int(lo.sum())} hi={int(hi.sum())}")
print("best64:", sorted(best64, reverse=True)[:10] if best64 else "NONE")
