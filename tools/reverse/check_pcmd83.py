#!/usr/bin/env python3
"""Confirm the new-build sim-side PCMD_CheatRevealAll handler details."""
import struct

EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
SECT = [
    (".text",  0x00001000, 0x3D53C00, 0x00000600, 0x3D52C00),
    (".rdata", 0x03D54000, 0x1EA6000, 0x03D53200, 0x1EA6000),
    (".data",  0x05BFA000, 0x0534E00, 0x05BF9200, 0x0534E00),
]


def rva_of(off):
    for name, va, vs, ro, rs in SECT:
        if ro <= off < ro + rs:
            return va + (off - ro)
    return None


def off_of(rva):
    for name, va, vs, ro, rs in SECT:
        if va <= rva < va + vs:
            return ro + (rva - va)
    return None


data = open(EXE, "rb").read()

# 1. what string does `lea rax, [rip + 0x1371c5a]` at rva 0x143437C57 point to?
target = 0x03437C5E + 0x1371C5A
print("GE_PlayerCheat lea target rva = 0x%08X" % target)
off = off_of(target)
print("  file = 0x%08X" % off if off else "  file = -")
if off:
    print("  bytes = %r" % data[off:off + 40])

# 2. the "MOD -- (CHEAT)..." string target sanity
t2 = 0x03437C3B + 0x1371985
o2 = off_of(t2)
print("\nlog fmt target rva = 0x%08X file=0x%08X" % (t2, o2))
print("  bytes = %r" % data[o2:o2 + 60])

# 3. callers of 0x143437BF8  (E8 rel32) and of 0x1433a1c68 (CRC)
def find_calls(va_target):
    out = []
    text_va, text_ro, text_rs = 0x00001000, 0x00000600, 0x3D52C00
    text = data[text_ro:text_ro + text_rs]
    for i in range(len(text) - 5):
        if text[i] != 0xE8:
            continue
        disp = struct.unpack_from("<i", text, i + 1)[0]
        nxt = 0x140000000 + text_va + i + 5
        if nxt + disp == va_target:
            out.append(0x140000000 + text_va + i)
    return out


for name, va in (("pcmd83 handler 0x143437BF8", 0x143437BF8),
                 ("crc fn 0x1433a1c68", 0x1433a1c68),
                 ("event dispatch 0x1434342f4", 0x1434342f4)):
    calls = find_calls(va)
    print("\n%s -> %d call site(s):" % (name, len(calls)))
    for c in calls[:20]:
        print("   call rva = 0x%08X" % (c - 0x140000000))

# 4. is 0x1433a1c68 a CRC32 update? dump its first 0x60 bytes
import sys
sys.path.insert(0, r"C:\Users\21642\Documents\deepseek-harness\default-workspace\CoH3FowTool\tools\reverse")
try:
    from capstone import Cs, CS_ARCH_X86, CS_MODE_64
    md = Cs(CS_ARCH_X86, CS_MODE_64)
    o = off_of(0x1433a1c68 - 0x140000000)
    print("\n0x1433a1c68 disasm:")
    for ins in md.disasm(data[o:o + 0x60], 0x1433a1c68):
        print("  0x%x  %s %s" % (ins.address, ins.mnemonic, ins.op_str))
except Exception as e:
    print("capstone err:", e)
