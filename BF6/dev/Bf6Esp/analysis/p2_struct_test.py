"""Structural test on the 2499 (type-name -> key) anchors.

If key = h(name) with h a polynomial hash  h = (h*M) OP c  then:
  * ADD variant, forward: names sharing the LAST byte give keyA-keyB divisible by M
  * XOR variant, forward: names sharing the LAST byte give (keyA^keyB) & 0xFF == 0
  * same two tests with the FIRST byte for a reversed byte order.
GCD over every pair inside a group yields the candidate multiplier set, and a
candidate M lets us solve for the init value and verify against all anchors.
"""
import json
import math
import sys
from collections import defaultdict
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))

anchors = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
items = [(a["type"], a["hash"]) for a in anchors]
print(f"anchors: {len(items)}")
# sanity: keys unique?
keys = [k for _, k in items]
print("distinct keys:", len(set(keys)), "of", len(keys))

by_len = defaultdict(list)
for n, k in items:
    by_len[len(n)].append((n, k))
print("name lengths:", sorted((L, len(v)) for L, v in by_len.items()))


def groups(by_last: bool):
    g = defaultdict(list)
    for n, k in items:
        b = n[-1] if by_last else n[0]
        g[(len(n), b)].append((n, k))
    return g


def test_xor_low(gg, label):
    """(keyA^keyB)&0xFF == 0 for every pair in a group"""
    bad = 0
    tot = 0
    for key, lst in gg.items():
        for i in range(len(lst)):
            for j in range(i + 1, len(lst)):
                tot += 1
                if (lst[i][1] ^ lst[j][1]) & 0xFF:
                    bad += 1
    print(f"  XOR-low8 test [{label}]: {bad}/{tot} pairs violate  -> "
          f"{'REJECTED' if bad > 0.02 * tot else 'consistent'}")
    return bad, tot


def gcd_candidates(gg, label):
    """any multiplier M must divide every keyA-keyB inside a group"""
    G = 0
    npairs = 0
    for key, lst in gg.items():
        if len(lst) < 2:
            continue
        for i in range(len(lst)):
            for j in range(i + 1, len(lst)):
                d = abs(lst[i][1] - lst[j][1])
                if d == 0:
                    continue
                G = math.gcd(G, d)
                npairs += 1
    print(f"  ADD-gcd test [{label}]: pairs={npairs} gcd={G} (0x{G:x})")
    return G


g_last = groups(True)
g_first = groups(False)
print("\n--- forward byte order ---")
g1 = gcd_candidates(g_last, "last byte")
test_xor_low(g_last, "last byte")
print("\n--- reversed byte order ---")
g2 = gcd_candidates(g_first, "first byte")
test_xor_low(g_first, "first byte")

# ---- if a candidate multiplier exists, solve for init and verify ----------
for G in {g1, g2}:
    if G == 0:
        continue
    # candidate multipliers: odd divisors of G (a hash needs an odd multiplier)
    cands = [m for m in range(3, 1 << 20, 2) if G % m == 0]
    cands = sorted(set(cands) | {33, 31, 131, 65599, 16777619})
    print(f"\ncandidate multipliers from gcd 0x{G:x}: {cands[:20]} ({len(cands)} total)")

# ---- direct check: solve init for each (M, order) using one anchor -------
def poly_hash(name: bytes, M: int, init: int, op: str, rev: bool) -> int:
    h = init & 0xFFFFFFFF
    data = name[::-1] if rev else name
    for c in data:
        if op == "xor":
            h = ((h * M) ^ c) & 0xFFFFFFFF
        else:
            h = ((h * M) + c) & 0xFFFFFFFF
    return h


def solve_init(name: bytes, key: int, M: int, op: str, rev: bool) -> int:
    """for the ADD family the hash is affine in init: key = init*M^n + K"""
    n = len(name)
    # K = value with init 0
    K = poly_hash(name, M, 0, op, rev)
    # invert M^n mod 2^32 if M odd
    mn = pow(M, n, 1 << 32)
    if mn % 2 == 0:
        return None
    inv = pow(mn, -1, 1 << 32)
    return ((key - K) * inv) & 0xFFFFFFFF


print("\n--- solving init for candidate (M, order, op) with the ADD family ---")
report = []
for M in [31, 33, 37, 39, 41, 131, 65599, 16777619, 0x01000193, 5381, 5, 257, 65537, 0x1000193]:
    for rev in (False, True):
        n0, k0 = items[0]
        init = solve_init(n0.encode(), k0, M, "add", rev)
        if init is None:
            continue
        ok = sum(1 for n, k in items if poly_hash(n.encode(), M, init, "add", rev) == k)
        if ok:
            print(f"  M={M} rev={rev} init=0x{init:08x}: {ok}/{len(items)} anchors match")
            report.append({"M": M, "rev": rev, "init": init, "matches": ok})
print("report:", report)
