"""Ninth stage: call sites of the registration gate + uniqueness checks for candidate anchors."""
import struct

import pefile

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()

secs = {}
for s in pe.sections:
    secs[s.Name.rstrip(b"\x00").decode()] = (
        s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData)
tva, tro, trs = secs[".text"]
code = raw[tro:tro + trs]


def off_of(rva):
    for name, (va, po, sz) in secs.items():
        if va <= rva < va + sz:
            return po + (rva - va)
    return None


def calls_to(target_rva):
    out = []
    for i in range(len(code) - 5):
        if code[i] != 0xE8:
            continue
        d = struct.unpack_from("<i", code, i + 1)[0]
        if tva + i + 5 + d == target_rva:
            out.append(tva + i)
    return out


for name, tgt in (("registration gate", 0x0028B2C0),
                  ("runtime gate", 0x03908B6C)):
    cs = calls_to(tgt)
    print("%-18s rva 0x%08X -> %d call sites" % (name, tgt, len(cs)))
    for c in cs[:10]:
        print("     rva 0x%08X file 0x%08X" % (c, off_of(c)))
    if len(cs) > 10:
        print("     ... (%d more)" % (len(cs) - 10))
