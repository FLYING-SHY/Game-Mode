"""Locate every occurrence of a string in the exe and every code xref to it.

Usage:
  python telemetry.py used_cheats debug_used GE_PlayerCheat
"""
import pefile, struct, sys

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()
secs = {}
for s in pe.sections:
    secs[s.Name.rstrip(b"\x00").decode()] = (s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData)

tva, tro, trs = secs[".text"]
code = raw[tro:tro + trs]
MODRM_RIP = {0x05, 0x0d, 0x15, 0x1d, 0x25, 0x2d, 0x35, 0x3d}


def refs(target):
    out = []
    n = len(code)
    for i in range(n - 6):
        b0 = code[i]
        if b0 in (0x48, 0x4c, 0x4d, 0x49) and code[i + 1] in (0x8d, 0x8b) and code[i + 2] in MODRM_RIP:
            d = struct.unpack_from("<i", code, i + 3)[0]
            if tva + i + 7 + d == target:
                out.append(tva + i)
        elif b0 in (0xe8, 0xe9):
            d = struct.unpack_from("<i", code, i + 1)[0]
            if tva + i + 5 + d == target:
                out.append(tva + i)
        elif b0 == 0xff and code[i + 1] == 0x15:
            d = struct.unpack_from("<i", code, i + 2)[0]
            if tva + i + 6 + d == target:
                out.append(tva + i)
    return out


def all_occurrences(needle):
    b = needle.encode()
    out = []
    i = raw.find(b)
    while i != -1:
        out.append(i)
        i = raw.find(b, i + 1)
    return out


for needle in sys.argv[1:]:
    print("=" * 70)
    print("STRING %r" % needle)
    for off in all_occurrences(needle):
        rva = pe.get_rva_from_offset(off)
        print("  file 0x%08X   rva 0x%08X   xrefs:" % (off, rva))
        rs = refs(rva)
        if not rs:
            print("      (none)")
        for x in rs:
            print("      rva 0x%08X   file 0x%08X" % (x, x - tva + tro))
