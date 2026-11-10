"""Sixth stage: dump the full function that builds PCMD_CheatRevealAll (edx=0x83)."""
import bisect
import struct

import capstone
import pefile

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()

secs = {}
for s in pe.sections:
    secs[s.Name.rstrip(b"\x00").decode()] = (
        s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData)
tva, tro, trs = secs[".text"]

pd_va, pd_po, pd_sz = secs[".pdata"]
starts, ends = [], []
for off in range(pd_po, pd_po + pd_sz - 8, 12):
    st, en, _ = struct.unpack_from("<III", raw, off)
    if st == 0 and en == 0:
        continue
    starts.append(st)
    ends.append(en)


def func_of(rva):
    i = bisect.bisect_right(starts, rva) - 1
    if i < 0:
        return None
    return (starts[i], ends[i]) if rva < ends[i] else None


def off_of(rva):
    for name, (va, po, sz) in secs.items():
        if va <= rva < va + sz:
            return po + (rva - va)
    return None


md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)


def dis(rva, end=None):
    off = off_of(rva)
    if end is None:
        end = rva + 0x200
    data = raw[off:off + (end - rva)]
    for ins in md.disasm(data, 0x140000000 + rva):
        print("  0x%09X  %-26s %s %s" % (
            ins.address, ins.bytes.hex(" "), ins.mnemonic, ins.op_str))


TARGET = 0x03909264
fb = func_of(TARGET)
print("function containing rva 0x%08X -> %s" % (
    TARGET, tuple(hex(x) for x in fb) if fb else None))
if fb:
    dis(fb[0], fb[1])

print("\n=== blacklist table at rva 0x004F1DF58 ===")
o = off_of(0x004F1DF58)
print("  file 0x%08X" % o)
vals = struct.unpack_from("<12i", raw, o)
print("  ints:", vals)
