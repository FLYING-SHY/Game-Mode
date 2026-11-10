"""Re-locate the six patch points in the CURRENT RelicCoH3.exe.

Strategy: the game code itself is very unlikely to have changed (only the
layout moved), so we search for the *original byte patterns* of each patch
site and then rebuild a unique anchor around it.

Usage:  python relocate.py
"""
import struct
import sys

import pefile

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()

secs = {}
for s in pe.sections:
    secs[s.Name.rstrip(b"\x00").decode()] = (
        s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData)
tva, tro, trs = secs[".text"]

print("size=%d  .text file[0x%X..0x%X) rva[0x%X..0x%X)  delta=0x%X" % (
    len(raw), tro, tro + trs, tva, tva + trs, tva - tro))


def rva_of(off):
    for name, (va, po, sz) in secs.items():
        if po <= off < po + sz:
            return va + (off - po)
    return None


def find_all(pat, start=0):
    out = []
    i = raw.find(pat, start)
    while i != -1:
        out.append(i)
        i = raw.find(pat, i + 1)
    return out


def show(tag, pat, ctx_before=0x20, ctx_after=0x30):
    hits = find_all(pat)
    print("\n### %s   pattern=%s   hits=%d" % (
        tag, " ".join("%02X" % b for b in pat), len(hits)))
    for h in hits:
        lo = max(0, h - ctx_before)
        hi = min(len(raw), h + len(pat) + ctx_after)
        print("  file 0x%08X  rva 0x%08X  in_text=%s" % (
            h, rva_of(h), tro <= h < tro + trs))
        for off in range(lo, hi, 16):
            chunk = raw[off:off + 16]
            mark = "  <<<" if off <= h < off + 16 else ""
            print("    0x%08X  %s%s" % (
                off, " ".join("%02X" % b for b in chunk), mark))
    return hits


# ---- 1. the two cheat gates -------------------------------------------------
show("CheatGateA body", bytes.fromhex("0F B6 81 70 06 00 00"))
show("CheatGateB body", bytes.fromhex("0F B6 81 71 06 00 00"))

# ---- 2. telemetry read points ----------------------------------------------
show("used_cheats read", bytes.fromhex("44 0F B6 40 20"))
show("debug_used  read", bytes.fromhex("44 0F B6 40 21"))
