"""Disassemble a window of the current exe at a given FILE offset (exact addresses).

Usage:  python dis_at.py <file_off_hex> [span_hex] [count]
"""
import sys

import capstone
import pefile

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()

secs = {}
for s in pe.sections:
    secs[s.Name.rstrip(b"\x00").decode()] = (
        s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData)


def rva_of(off):
    for name, (va, po, sz) in secs.items():
        if po <= off < po + sz:
            return va + (off - po)
    return None


def dis(off, span=0x80, count=0):
    rva = rva_of(off)
    data = raw[off:off + span]
    md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
    md.detail = False
    n = 0
    for ins in md.disasm(data, 0x140000000 + rva):
        print("  0x%09X (file 0x%08X)  %-28s %s %s" % (
            ins.address, off + (ins.address - 0x140000000 - rva),
            ins.bytes.hex(" "), ins.mnemonic, ins.op_str))
        n += 1
        if count and n >= count:
            break
        if ins.mnemonic == "ret" and n > 3:
            break


if __name__ == "__main__":
    off = int(sys.argv[1], 16)
    span = int(sys.argv[2], 16) if len(sys.argv) > 2 else 0x80
    cnt = int(sys.argv[3]) if len(sys.argv) > 3 else 0
    print("=== file 0x%08X  rva 0x%08X ===" % (off, rva_of(off) or 0))
    dis(off, span, cnt)
