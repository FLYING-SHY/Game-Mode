"""Brute-force RIP-relative xrefs: any 32-bit displacement in .text that
resolves to a target RVA. Usage: python xref_rva.py <rva_hex> [...]"""
import bisect
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
text = raw[tro:tro + trs]

pd_va, pd_po, pd_sz = secs[".pdata"]
pstarts, pends = [], []
for off in range(pd_po, pd_po + pd_sz - 8, 12):
    st, en, _ = struct.unpack_from("<III", raw, off)
    if st == 0 and en == 0:
        continue
    pstarts.append(st)
    pends.append(en)


def func_of(rva):
    i = bisect.bisect_right(pstarts, rva) - 1
    if i < 0:
        return None
    return (pstarts[i], pends[i]) if rva < pends[i] else None


targets = set(int(a, 16) for a in sys.argv[1:])
# map: for a position i (file offset of displacement), the instruction end is
# at file offset i+4, and rip = rva(text end). We test end offsets 6..8 bytes
# after the disp for typical lea/mov encodings.
for t in sorted(targets):
    print("=== target rva 0x%08X ===" % t)
    hits = 0
    for i in range(len(text) - 4):
        disp = struct.unpack_from("<i", text, i)[0]
        # assume instruction ends at i+4, next instruction starts there
        end_rva = tva + i + 4
        if end_rva + disp == t:
            rva = tva + i
            print("   disp at rva 0x%08X (ins bytes %s)  func=%s" % (
                rva, text[max(0, i - 3):i + 4].hex(),
                tuple(hex(x) for x in func_of(rva)) if func_of(rva) else None))
            hits += 1
            if hits > 20:
                print("   ... more")
                break
    if not hits:
        print("   (none)")
