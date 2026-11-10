"""Find all RIP-relative references to an RVA inside .text.

Scans every instruction-like position for the 32-bit displacement form
  48 8d 05/0d/15/1d/25/2d/35/3d  <disp32>     (lea reg, [rip+disp])
  48 8b 05/0d/15/1d/25/2d/35/3d  <disp32>     (mov reg, [rip+disp])
  e8/e9 <disp32>                             (call/jmp rel32)
  ff 15 <disp32>                             (call [rip+disp])
plus the 32-bit forms without REX.W.

Usage:  python ripref.py <rva_hex> [more...]
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


def refs_to(target):
    out = []
    n = len(code)
    for i in range(n - 6):
        b0 = code[i]
        # lea/mov reg, [rip+disp32]
        if b0 in (0x48, 0x4c, 0x4d, 0x49) and code[i + 1] in (0x8d, 0x8b) and code[i + 2] in MODRM_RIP:
            disp = struct.unpack_from("<i", code, i + 3)[0]
            if tva + i + 7 + disp == target:
                out.append((tva + i, f"{code[i:i+7].hex(' ')}  {b0:02x} {code[i+1]:02x} {code[i+2]:02x}"))
        # call/jmp rel32
        elif b0 in (0xe8, 0xe9):
            disp = struct.unpack_from("<i", code, i + 1)[0]
            if tva + i + 5 + disp == target:
                out.append((tva + i, f"{code[i:i+5].hex(' ')}  call/jmp rel32"))
        # call [rip+disp32]
        elif b0 == 0xff and code[i + 1] == 0x15:
            disp = struct.unpack_from("<i", code, i + 2)[0]
            if tva + i + 6 + disp == target:
                out.append((tva + i, f"{code[i:i+6].hex(' ')}  call [rip+disp]"))
    return out


if __name__ == "__main__":
    for a in sys.argv[1:]:
        t = int(a, 16)
        r = refs_to(t)
        print(f"\n== refs to 0x{t:X}: {len(r)}")
        for a_, txt in r[:60]:
            print(f"   rva 0x{a_:X}   file 0x{a_ - tva + tro:X}   {txt}")
