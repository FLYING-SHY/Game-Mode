#!/usr/bin/env python3
"""p2_hash_sweep: parameterised search for the BF6 name-hash.

Families (vectorised numpy, exact byte length via mask):
  A_xor : h = (h * M) ^ c          (Frostbite/MEC style: h = h*33 ^ c)
  A_add : h = (h * M) + c
  B_xor : h = (h ^ c) * M          (FNV-1a style)
  B_add : h = (h + c) * M
plus crc32 variants, fnv1a variants.
Multipliers x init values x op x name-form grid.
Hit criterion: hits > 20x noise floor (floor = |names| * |universe| / 2**32).
"""
import json
import re
import sys
import time
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402

IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}\Z")

MULTS = [31, 33, 37, 39, 41, 131, 17, 5, 257, 65537, 65599, 131071, 16777619,
         0x01000193, 0x85EBCA6B, 0xC2B2AE35, 0x9E3779B1, 0x45D9F3B, 3, 7, 11,
         101, 127, 1021, 0x5BD1E995, 0xCC9E2D51, 0x1B873593, 1000003, 5381,
         0x1000193, 16777619 - 2]
INITS = [0, 1, 5381, 0x811C9DC5, 0xCBF29CE4, 0x1505, 0xDEADBEEF, 0xFFFFFFFF,
         0x1000193, 7, 17, 0xFFFFFFFF, 0x811C9DC5 ^ 0xFFFFFFFF, 0x01000193]


def crc_table(poly):
    t = np.zeros(256, dtype=np.uint32)
    for i in range(256):
        c = i
        for _ in range(8):
            c = (c >> 1) ^ (poly if c & 1 else 0)
        t[i] = c
    return t


def snake(s):
    out = []
    for i, ch in enumerate(s):
        if i and ch.isupper() and not s[i - 1].isupper():
            out.append("_")
        out.append(ch.lower())
    return "".join(out)


PREFIXES = (b"Client", b"Ecs", b"Fb", b"FBUI", b"Server", b"Local", b"Bf")


def strip_prefix(s):
    for p in PREFIXES:
        if s.startswith(p) and len(s) > len(p) + 2:
            return s[len(p):]
    return s


def build_forms(names):
    forms = {}
    forms["raw"] = names
    forms["lower"] = [s.lower() for s in names]
    forms["upper"] = [s.upper() for s in names]
    forms["raw+nul"] = [s + b"\x00" for s in names]
    forms["strip"] = [strip_prefix(s) for s in names]
    forms["snake"] = [snake(s.decode("latin1")).encode("latin1") for s in names]
    forms["fb::"] = [b"fb::" + s for s in names]
    forms["ecs::"] = [b"Ecs::" + s for s in names]
    return forms


def matify(lst):
    L = max(len(s) for s in lst) + 1
    m = np.zeros((len(lst), L), dtype=np.uint8)
    mask = np.zeros((len(lst), L), dtype=bool)
    for i, s in enumerate(lst):
        m[i, :len(s)] = np.frombuffer(s, dtype=np.uint8)
        mask[i, :len(s)] = True
    return m, mask


def hash_family(m, mask, M, init, kind):
    n = m.shape[0]
    h = np.full(n, init & 0xFFFFFFFF, dtype=np.uint32)
    Mv = np.uint32(M)
    for j in range(m.shape[1]):
        c = m[:, j].astype(np.uint32)
        if kind == "A_xor":
            upd = (h * Mv) ^ c
        elif kind == "A_add":
            upd = (h * Mv) + c
        elif kind == "B_xor":
            upd = (h ^ c) * Mv
        else:  # B_add
            upd = (h + c) * Mv
        h = np.where(mask[:, j], upd, h).astype(np.uint32)
    return h


def crc32_hashes(m, mask, table, init, xorout):
    n = m.shape[0]
    h = np.full(n, init & 0xFFFFFFFF, dtype=np.uint32)
    for j in range(m.shape[1]):
        upd = table[(h ^ m[:, j]) & 0xFF] ^ (h >> 8)
        h = np.where(mask[:, j], upd, h).astype(np.uint32)
    return (h ^ xorout).astype(np.uint32)


def fnv1a64_hashes(m, mask):
    n = m.shape[0]
    h = np.full(n, 0xCBF29CE484222325, dtype=np.uint64)
    for j in range(m.shape[1]):
        upd = (h ^ m[:, j].astype(np.uint64)) * np.uint64(1099511628211)
        h = np.where(mask[:, j], upd, h).astype(np.uint64)
    return h


def score(h, nn, u, ubig):
    pos = np.clip(np.searchsorted(u, h), 0, len(u) - 1)
    hitm = u[pos] == h
    hits = int(hitm.sum())
    bigm = hitm & (u[pos] >= 0x10000)
    hits_big = int(bigm.sum())
    floor = nn * len(u) / 2 ** 32
    floor_big = nn * ubig / 2 ** 32
    return hits, hits_big, floor, floor_big, pos, hitm


def main():
    t0 = time.time()
    img = Img()
    data = img.data

    # ---------- universes ----------
    ts = img.sec["typeinfo"]
    tb = data[ts["roff"]:ts["roff"] + ts["rsize"]]
    t32 = np.frombuffer(tb, dtype="<u4")
    mk = np.nonzero(t32 == 0x00080089)[0]
    T0 = np.unique(t32[mk - 1])           # type hash slot
    T1 = np.unique(t32[mk + 10])          # second hash slot
    TALL = np.unique(t32)
    fs = img.sec["fieldinf"]
    fb = data[fs["roff"]:fs["roff"] + fs["rsize"]]
    f32 = np.frombuffer(fb, dtype="<u4")
    STR = 0x30
    nrec = len(fb) // STR
    F0 = np.unique(f32[0:nrec * (STR // 4):STR // 4])
    FALL = np.unique(f32)
    universes = {"T0_typehash": T0, "T1_typehash2": T1, "F0_fieldhash": F0,
                 "TALL": TALL, "FALL": FALL}
    ubig_count = {k: int((v >= 0x10000).sum()) for k, v in universes.items()}
    for k, v in universes.items():
        print(f"universe {k}: {len(v)} distinct ({ubig_count[k]} >= 0x10000)")

    # ---------- names ----------
    cand = OUTDIR / "name_candidates.json"
    names = []
    if cand.exists():
        try:
            raw = json.load(open(cand))
            if isinstance(raw, dict):
                raw = raw.get("names") or raw.get("candidates") or []
            if isinstance(raw, list):
                names = [x.encode("latin1") if isinstance(x, str) else bytes(x) for x in raw]
                names = [s for s in names if IDENT.match(s)]
        except Exception:
            names = []
    if not names:
        # whole-file scan: maximal [A-Za-z_][A-Za-z0-9_]{2,63} followed by NUL
        pat = re.compile(rb"[A-Za-z_][A-Za-z0-9_]{2,63}\x00")
        seen = set()
        for mo in pat.finditer(data):
            s = mo.group(0)[:-1]
            if s not in seen:
                seen.add(s)
                names.append(s)
        names = sorted(seen)
    print("name universe:", len(names))

    # subset for the grid (reflection-like names, <=32 chars)
    refl = [s for s in names if len(s) <= 32 and
            re.match(rb"^[A-Z][A-Za-z0-9_]{2,31}\Z", s)]
    grid_names = refl[:40000]
    print("grid name subset:", len(grid_names))

    forms = build_forms(grid_names)
    mats = {k: matify(v) for k, v in forms.items()}

    std = crc_table(0xEDB88320)
    cast = crc_table(0x82F63B78)

    results = []
    best = []
    kinds = ["A_xor", "A_add", "B_xor", "B_add"]
    for fname, (m, mask) in mats.items():
        nn = m.shape[0]
        # --- polynomial families
        for kind in kinds:
            for M in MULTS:
                for init in INITS:
                    h = hash_family(m, mask, M, init, kind)
                    for uname, u in universes.items():
                        hits, hb, floor, floor_big, pos, hitm = score(
                            h, nn, u, ubig_count[uname])
                        rec = {"form": fname, "family": kind, "M": M, "init": init,
                               "universe": uname, "hits": hits, "hits_big": hb,
                               "floor": round(floor, 3), "floor_big": round(floor_big, 3),
                               "ratio": round(hits / floor, 2) if floor else None,
                               "ratio_big": round(hb / floor_big, 2) if floor_big else None}
                        results.append(rec)
                        if hb >= 3 and hb > 20 * max(floor_big, 1e-9):
                            idx = np.nonzero(hitm & (u[pos] >= 0x10000))[0]
                            rec["pairs"] = [{"name": forms[fname][int(i)].decode("latin1"),
                                             "hash": int(h[int(i)])} for i in idx[:20]]
                            best.append(rec)
        # --- crc32 / fnv families
        for label, fn in (
            ("crc32_std", lambda: crc32_hashes(m, mask, std, 0xFFFFFFFF, 0xFFFFFFFF)),
            ("crc32_std_init0", lambda: crc32_hashes(m, mask, std, 0, 0)),
            ("crc32c_std", lambda: crc32_hashes(m, mask, cast, 0xFFFFFFFF, 0xFFFFFFFF)),
            ("fnv1a64", lambda: fnv1a64_hashes(m, mask)),
        ):
            h = fn()
            if h.dtype == np.uint64:
                for shift, tag in ((0, "lo32"), (32, "hi32")):
                    hh = ((h >> np.uint64(shift)) & np.uint64(0xFFFFFFFF)).astype(np.uint32)
                    for uname, u in universes.items():
                        hits, hb, floor, floor_big, pos, hitm = score(
                            hh, nn, u, ubig_count[uname])
                        rec = {"form": fname, "family": label + "_" + tag, "M": None,
                               "init": None, "universe": uname, "hits": hits,
                               "hits_big": hb, "floor": round(floor, 3),
                               "floor_big": round(floor_big, 3),
                               "ratio": round(hits / floor, 2) if floor else None,
                               "ratio_big": round(hb / floor_big, 2) if floor_big else None}
                        results.append(rec)
                        if hb >= 3 and hb > 20 * max(floor_big, 1e-9):
                            idx = np.nonzero(hitm & (u[pos] >= 0x10000))[0]
                            rec["pairs"] = [{"name": forms[fname][int(i)].decode("latin1"),
                                             "hash": int(hh[int(i)])} for i in idx[:20]]
                            best.append(rec)
            else:
                for uname, u in universes.items():
                    hits, hb, floor, floor_big, pos, hitm = score(
                        h, nn, u, ubig_count[uname])
                    rec = {"form": fname, "family": label, "M": None, "init": None,
                           "universe": uname, "hits": hits, "hits_big": hb,
                           "floor": round(floor, 3), "floor_big": round(floor_big, 3),
                           "ratio": round(hits / floor, 2) if floor else None,
                           "ratio_big": round(hb / floor_big, 2) if floor_big else None}
                    results.append(rec)
                    if hb >= 3 and hb > 20 * max(floor_big, 1e-9):
                        idx = np.nonzero(hitm & (u[pos] >= 0x10000))[0]
                        rec["pairs"] = [{"name": forms[fname][int(i)].decode("latin1"),
                                         "hash": int(h[int(i)])} for i in idx[:20]]
                        best.append(rec)
        print(f"  form {fname} done  ({time.time()-t0:.1f}s)")

    results.sort(key=lambda r: (-r["hits"], r["floor"]))
    print("\n=== top 30 by hits ===")
    for r in results[:30]:
        print(f"  {r['family']:14s} M={r['M']} init={r['init']} form={r['form']:8s} "
              f"U={r['universe']:14s} hits={r['hits']:6d} floor={r['floor']:.3f} "
              f"ratio={r['ratio']}")
    print(f"\n=== significant (hits>=3 and >20x floor): {len(best)} ===")
    for r in best[:40]:
        print("  ", {k: r[k] for k in ("family", "M", "init", "form", "universe",
                                       "hits", "floor", "ratio")})
        for p in r.get("pairs", [])[:8]:
            print("       ", p)

    json.dump({"n_names_total": len(names), "n_grid": len(grid_names),
               "universes": {k: len(v) for k, v in universes.items()},
               "n_configs": len(results), "significant": best, "results": results},
              open(OUTDIR / "p2_hash_sweep.json", "w"), indent=1)
    print(f"\nwritten p2_hash_sweep.json  ({time.time()-t0:.1f}s)")


if __name__ == "__main__":
    main()
