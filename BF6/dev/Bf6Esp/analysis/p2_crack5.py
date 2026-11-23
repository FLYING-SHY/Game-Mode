"""Final hash attempt on the anchors: module-seeded and module-combined schemes."""
import json
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
M32 = 0xFFFFFFFF


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


def fnv1a(data, init=0x811C9DC5):
    h = init
    for c in data:
        h = ((h ^ c) * 0x01000193) & M32
    return h


def fnv1(data, init=0x811C9DC5):
    h = init
    for c in data:
        h = ((h * 0x01000193) ^ c) & M32
    return h


def crc32s(data, init=None):
    return zlib.crc32(data) & M32


BASE = {"djb2_xor": djb2_xor, "djb2_add": djb2_add, "fnv1a": fnv1a,
        "fnv1": fnv1, "crc32": crc32s}

print("anchors:", len(anch))
report = []

# ---- A: module-seeded -------------------------------------------------------
for fname, f in BASE.items():
    for shape in ("name", "mod::name"):
        hits = 0
        for i, (n, m) in enumerate(zip(NAMES, MODS)):
            init = f(m.encode())
            s = n.encode() if shape == "name" else f"{m}::{n}".encode()
            if f(s, init) == H[i]:
                hits += 1
        report.append((hits, f"{fname}/seed=hash(module)/{shape}"))
        if hits:
            print(f"  HIT {hits}  {fname}/seed=hash(module)/{shape}")

# ---- B: module-combined -----------------------------------------------------
for fname, f in BASE.items():
    hits = 0
    for i, (n, m) in enumerate(zip(NAMES, MODS)):
        a, b = f(n.encode()), f(m.encode())
        if (a ^ b) == H[i] or ((a + b) & M32) == H[i]:
            hits += 1
    report.append((hits, f"{fname}/combine(module,name)"))
    if hits:
        print(f"  HIT {hits}  {fname}/combine(module,name)")

# ---- C: separator shapes ----------------------------------------------------
SEPS = ["::", ".", "_", "\x00", "/", ":", " ", "", "#"]
for fname, f in BASE.items():
    for sep in SEPS:
        for order in ("mn", "nm"):
            hits = 0
            for i, (n, m) in enumerate(zip(NAMES, MODS)):
                s = (m + sep + n) if order == "mn" else (n + sep + m)
                if f(s.encode()) == H[i]:
                    hits += 1
            report.append((hits, f"{fname}/{order}sep={sep!r}"))
            if hits:
                print(f"  HIT {hits}  {fname}/{order}sep={sep!r}")

# ---- D: prefixed shapes -----------------------------------------------------
for fname, f in BASE.items():
    for pre in ("fb::", "fb.", "Ecs::", "Ecs.", "fb::games::", "fb::games."):
        for what in ("name", "mod::name"):
            hits = 0
            for i, (n, m) in enumerate(zip(NAMES, MODS)):
                base = n if what == "name" else f"{m}::{n}"
                if f((pre + base).encode()) == H[i]:
                    hits += 1
            report.append((hits, f"{fname}/pre={pre}/{what}"))
            if hits:
                print(f"  HIT {hits}  {fname}/pre={pre}/{what}")

# ---- E: name with trailing NUL / length prefix -----------------------------
for fname, f in BASE.items():
    for suffix in ("\x00", "\x00\x00"):
        hits = 0
        for i, n in enumerate(NAMES):
            if f((n + suffix).encode()) == H[i]:
                hits += 1
        report.append((hits, f"{fname}/suffix={suffix!r}"))
        if hits:
            print(f"  HIT {hits}  {fname}/suffix={suffix!r}")
    # length-prefixed (LE u32 / u8)
    for lp in ("u32le", "u8"):
        hits = 0
        for i, n in enumerate(NAMES):
            b = n.encode()
            pre = len(b).to_bytes(4, "little") if lp == "u32le" else bytes([len(b)])
            if f(pre + b) == H[i]:
                hits += 1
        report.append((hits, f"{fname}/lenprefix={lp}"))
        if hits:
            print(f"  HIT {hits}  {fname}/lenprefix={lp}")

best = sorted(report, reverse=True)[:12]
print("\nBEST:", best)

# ---- F: sanity: is u32[0] even name-dependent?  check hash-vs-name statistics -
print("\n--- per-module hash pattern ---")
from collections import defaultdict
bym = defaultdict(list)
for n, m, h in zip(NAMES, MODS, H):
    bym[m].append(h)
for m, hs in list(bym.items())[:6]:
    print(f"   {m:<26} n={len(hs):4d} distinct={len(set(hs)):4d} "
          f"first={hex(hs[0])}")
print("done")
