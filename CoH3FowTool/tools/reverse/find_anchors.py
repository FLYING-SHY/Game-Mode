"""Locate the five patch anchors in the current RelicCoH3.exe and report their offsets."""
import hashlib
import os
import sys

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

data = open(P, "rb").read()
print("file size = %d bytes" % len(data))
print("sha256    = %s" % hashlib.sha256(data).hexdigest())
print("mtime     = %s" % __import__("time").strftime(
    "%Y-%m-%d %H:%M:%S", __import__("time").localtime(os.path.getmtime(P))))

import pefile
pe = pefile.PE(P, fast_load=True)
print("ImageBase = 0x%X  SizeOfImage = 0x%X" % (
    pe.OPTIONAL_HEADER.ImageBase, pe.OPTIONAL_HEADER.SizeOfImage))


def off2rva(off):
    for s in pe.sections:
        if s.PointerToRawData <= off < s.PointerToRawData + s.SizeOfRawData:
            return s.VirtualAddress + (off - s.PointerToRawData)
    return None


SITES = [
    ("CheatGate", "49 03 C8 E8 75 3D 88 FC 4C 8B D0", +0x0B, 17),
    ("RevealAllGate", "40 53 48 83 EC 30 4C 8B D9 E8 62 F9 FF FF 84 C0", +0x10, 2),
    ("DoCommandTrust", "4D 8B 84 C1 00 5E 18 06 4C 03 C1 41 80 78 60 00", +0x10, 2),
    ("TelemetryUsedCheats",
     "C6 44 24 20 00 4C 8D 0D CB FB BB 03 48 8D 15 C0 98 D1 03 49 8B CE E8 A0 FB 14 00", -0x04, 4),
    ("TelemetryDebugUsed",
     "C6 44 24 20 00 4C 8D 0D A2 FB BB 03 48 8D 15 B7 98 D1 03 49 8B CE E8 77 FB 14 00", -0x04, 4),
]

for name, hexs, delta, tlen in SITES:
    pat = bytes.fromhex(hexs)
    hits = []
    start = 0
    while True:
        i = data.find(pat, start)
        if i < 0:
            break
        hits.append(i)
        start = i + 1
        if len(hits) > 8:
            break
    print("\n%s  anchor=%s" % (name, hexs))
    if not hits:
        print("   !! NOT FOUND")
        continue
    for h in hits:
        t = h + delta
        rva = off2rva(t)
        tgt = data[t:t + tlen]
        print("   anchor file=0x%08X rva=0x%08X -> target file=0x%08X rva=0x%08X bytes=%s" % (
            h, off2rva(h) or 0, t, rva or 0, " ".join("%02X" % b for b in tgt)))
