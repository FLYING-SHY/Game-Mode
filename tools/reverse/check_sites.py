"""Validate the new-build patch sites: anchor uniqueness, expected bytes,
and a disassembly of original vs patched."""
import capstone
import pefile

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()
secs = {s.Name.rstrip(b"\x00").decode(): (s.VirtualAddress, s.PointerToRawData,
                                          s.SizeOfRawData) for s in pe.sections}
tva, tro, trs = secs[".text"]


def off_of(rva):
    for name, (va, po, sz) in secs.items():
        if va <= rva < va + sz:
            return po + (rva - va)
    return None


md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)

# name, anchor hex, anchor->target, target len, expected hex, patch hex
SITES = [
    ("CheatGate",
     "49 03 C8 E8 75 3D 88 FC 4C 8B D0", 0x0B, 17,
     "33 C0 4D 85 D2 74 0A 41 38 82 80 06 00 00 0F 94 C0",
     "B8 01 00 00 00 90 90 90 90 90 90 90 90 90 90 90 90"),
    ("RevealAllGate",
     "40 53 48 83 EC 30 4C 8B D9 E8 62 F9 FF FF 84 C0", 0x10, 2,
     "74 7D", "90 90"),
    ("DoCommandTrust",
     "4D 8B 84 C1 00 5E 18 06 4C 03 C1 41 80 78 60 00", 0x10, 2,
     "74 0E", "90 90"),
    ("TelemetryUsedCheats",
     "C6 44 24 20 00 4C 8D 0D CB FB BB 03 48 8D 15 C0 98 D1 03 "
     "49 8B CE E8 A0 FB 14 00", -4, 4,
     "4D 0F 45 C4", "90 90 90 90"),
    ("TelemetryDebugUsed",
     "C6 44 24 20 00 4C 8D 0D A2 FB BB 03 48 8D 15 B7 98 D1 03 "
     "49 8B CE E8 77 FB 14 00", -4, 4,
     "4D 0F 45 C4", "90 90 90 90"),
]


def hx(s):
    return bytes(int(x, 16) for x in s.split())


ok = True
for name, ahex, off2t, tlen, exphex, patchhex in SITES:
    a = hx(ahex)
    hits = []
    pos = 0
    while True:
        j = raw.find(a, pos)
        if j < 0:
            break
        hits.append(j)
        pos = j + 1
    print("=== %s ===" % name)
    print("   anchor occurrences: %d %s" % (
        len(hits), [hex(h) for h in hits[:5]]))
    if len(hits) != 1:
        ok = False
        print("   !! anchor not unique")
        continue
    ao = hits[0]
    to = ao + off2t
    if ao <= to + tlen - 1 and to <= ao + len(a) - 1:
        ok = False
        print("   !! anchor overlaps target region")
    cur = raw[to:to + tlen]
    exp = hx(exphex)
    pat = hx(patchhex)
    print("   target file 0x%08X  rva 0x%08X" % (to, tva + (to - tro)))
    print("   current : %s" % cur.hex(" "))
    print("   expected: %s   %s" % (exp.hex(" "), "OK" if cur == exp else "MISMATCH"))
    if cur != exp:
        ok = False
    rva = tva + (to - tro)
    for label, blob in (("original", cur), ("patched ", pat)):
        print("   %s:" % label)
        for ins in md.disasm(blob, 0x140000000 + rva):
            print("      0x%08X  %-22s %s %s" % (
                ins.address, ins.bytes.hex(), ins.mnemonic, ins.op_str))
print()
print("ALL OK" if ok else "PROBLEMS FOUND")

