#!/usr/bin/env python3
"""p4_ctr.py - inspect the `ctr` section and the known name-xref code sites."""
import math
import re
import struct
import sys
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402

SITES = [
    ("ctr", 0x7964098, "LocalPlayer* cluster"),
    ("ctr", 0x79640e0, "LocalPlayer* cluster end"),
    ("ctr", 0x7404014, "Health"),
    ("ctr", 0x740401c, "Health"),
    ("ctr", 0x74f0abc, "GameContext"),
    (".text", 0x2f94690, "Health"),
    (".text", 0x2f94e64, "Health"),
    (".text", 0x2ae8be0, "TeamId"),
    (".text", 0x3c1c500, "LocalPlayer"),
    (".text", 0x3c1c524, "LocalPlayer"),
    (".text", 0x3bde188, "ClientConnection"),
    (".text", 0x67daf28, "ClientGame"),
]


def entropy(b):
    if not b:
        return 0.0
    c = Counter(b)
    n = len(b)
    return -sum((v / n) * math.log2(v / n) for v in c.values())


def rip_targets(img, rva, length=0x400):
    """scan for RIP-relative LEA/MOV in a window, return list of (code_rva, target_rva, kind)"""
    off = img.rva2off(rva)
    buf = img.data[off:off + length]
    res = []
    i = 0
    n = len(buf)
    while i < n - 6:
        b = buf[i]
        # REX.W optional
        j = i
        rex = 0
        if 0x40 <= b <= 0x4F:
            rex = b
            j = i + 1
            if j >= n:
                break
            b = buf[j]
        if b in (0x8D, 0x8B, 0x89, 0x3B, 0x39) and j + 1 < n:
            modrm = buf[j + 1]
            if (modrm & 0xC7) == 0x05:  # mod=00, rm=101 -> RIP-relative
                disp = struct.unpack_from("<i", buf, j + 2)[0]
                instr_len = (j + 6) - i
                next_rva = rva + i + instr_len
                tgt = next_rva + disp
                kind = {0x8D: "lea", 0x8B: "mov_load", 0x89: "mov_store",
                        0x3B: "cmp", 0x39: "cmp"}[b]
                res.append((rva + i, tgt, kind, rex))
                i = j + 6
                continue
        i += 1
    return res


def describe(img, rva):
    t = img.cstr_at_rva(rva)
    return t if t else ""


def main():
    img = Img()
    for s in img.secs:
        blob = img.blob(s["name"]) if s["rsize"] else b""
        print("%-10s rva=0x%08x size=%9d roff=0x%08x entropy=%.3f" % (
            s["name"], s["va"], s["rsize"], s["roff"], entropy(blob[:1 << 20])))

    print("\n=== name xref code sites ===")
    for sec, rva, label in SITES:
        off = img.rva2off(rva)
        if off is None:
            print("%s 0x%x NOT MAPPED" % (sec, rva))
            continue
        print("\n--- %s @ rva 0x%x (file 0x%x) : %s" % (sec, rva, off, label))
        print("    bytes:", img.data[off:off + 16].hex(" "))
        for crva, tgt, kind, rex in rip_targets(img, rva - 0x40, 0x120):
            s = describe(img, tgt)
            if s:
                print("    0x%08x %-9s -> 0x%08x  %r" % (crva, kind, tgt, s[:80]))

    print("\n=== ctr LocalPlayer cluster raw window ===")
    rva = 0x7964050
    off = img.rva2off(rva)
    for k in range(0, 0x100, 16):
        chunk = img.data[off + k: off + k + 16]
        h = " ".join("%02x" % b for b in chunk)
        a = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
        print("  %012x  %-47s  %s" % (rva + k, h, a))


if __name__ == "__main__":
    main()
