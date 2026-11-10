"""Find code references to an RVA (or to a file offset) inside .text.

Usage:
  python xref.py <rva_hex> [<rva_hex> ...]
  python xref.py -f <file_offset_hex> [...]     # treat args as file offsets
"""
import pefile, struct, sys, os

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


if __name__ == "__main__":
    as_file = False
    args = sys.argv[1:]
    if args and args[0] == "-f":
        as_file = True
        args = args[1:]
    for a in args:
        v = int(a, 16)
        if as_file:
            v = pe.get_rva_from_offset(v)
        r = refs(v)
        print("== refs to 0x%X : %d" % (v, len(r)))
        for x in r[:40]:
            print("   rva 0x%X   file 0x%X" % (x, x - tva + tro))
