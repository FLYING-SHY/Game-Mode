"""Dump a function (by RVA) from the new exe with file/rva/va annotations."""
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

md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)


def off_of(rva):
    for name, (va, po, sz) in secs.items():
        if va <= rva < va + sz:
            return po + (rva - va)
    return None


rva = int(sys.argv[1], 16)
span = int(sys.argv[2], 16) if len(sys.argv) > 2 else 0x100
off = off_of(rva)
data = raw[off:off + span]
print("rva 0x%08X  file 0x%08X  span 0x%X" % (rva, off, span))
for ins in md.disasm(data, 0x140000000 + rva):
    fo = off_of(ins.address - 0x140000000)
    print("  0x%08X [file 0x%08X]  %-24s %s %s" % (
        ins.address, fo if fo is not None else -1,
        ins.bytes.hex(), ins.mnemonic, ins.op_str))
