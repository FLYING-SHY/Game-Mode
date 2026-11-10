"""Locate the sim-side PCMD_CheatRevealAll handler in the new exe and check
whether it contains its own trust / cheat-flag gate."""
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


def off_of(rva):
    for name, (va, po, sz) in secs.items():
        if va <= rva < va + sz:
            return po + (rva - va)
    return None


md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)

needle = b"revealed fow"
o = raw.find(needle)
print("'revealed fow' file 0x%08X" % o)
s_rva = None
for name, (va, po, sz) in secs.items():
    if po <= o < po + sz:
        s_rva = va + (o - po)
        print("  section %s rva 0x%08X" % (name, s_rva))
        break

# find RIP-relative references to that rva inside .text
print("\n=== xrefs to 'revealed fow' ===")
found = []
for i in range(len(text) - 6):
    # lea reg, [rip+disp32]  -> 48 8D /r  disp32  (mod=00 r/m=101)
    if text[i] in (0x48, 0x4C) and text[i + 1] == 0x8D and (text[i + 2] & 0xC7) == 0x05:
        disp = struct.unpack_from("<i", text, i + 3)[0]
        rva = tva + i
        tgt = rva + 7 + disp
        if tgt == s_rva:
            found.append(rva)
for r in found:
    print("  lea at rva 0x%08X   func=%s" % (
        r, tuple(hex(x) for x in func_of(r)) if func_of(r) else None))

if found:
    # dump the containing function
    f = func_of(found[0])
    if f:
        print("\n=== containing function %s ===" % (tuple(hex(x) for x in f),))
        data = raw[off_of(f[0]):off_of(f[0]) + (f[1] - f[0])]
        for ins in md.disasm(data, 0x140000000 + f[0]):
            print("  0x%08X  %-22s %s %s" % (
                ins.address, ins.bytes.hex(), ins.mnemonic, ins.op_str))
