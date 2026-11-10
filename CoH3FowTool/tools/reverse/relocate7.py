"""Seventh stage: the new cheat gate function + its call sites + the revealall handler."""
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
code = raw[tro:tro + trs]

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


def calls_to(target_rva):
    out = []
    for i in range(len(code) - 5):
        if code[i] != 0xE8:
            continue
        d = struct.unpack_from("<i", code, i + 1)[0]
        if tva + i + 5 + d == target_rva:
            out.append(tva + i)
    return out


md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)


def dis(rva, end=None, label=""):
    print("--- %s dis @0x%08X ---" % (label, rva))
    off = off_of(rva)
    if end is None:
        end = rva + 0x120
    data = raw[off:off + (end - rva)]
    for ins in md.disasm(data, 0x140000000 + rva):
        print("  0x%09X  %-26s %s %s" % (
            ins.address, ins.bytes.hex(" "), ins.mnemonic, ins.op_str))


GATE = 0x03908B6C
fb = func_of(GATE)
print("gate rva 0x%08X -> func %s" % (GATE, tuple(hex(x) for x in fb) if fb else None))
if fb:
    dis(fb[0], fb[1], "gate")

cs = calls_to(GATE)
print("\ncall sites of gate: %d" % len(cs))
for c in cs:
    print("   rva 0x%08X file 0x%08X func=%s" % (
        c, off_of(c), tuple(hex(x) for x in func_of(c)) if func_of(c) else None))

print("\n=== call-site context (first 6) ===")
for c in cs[:6]:
    dis(max(0, c - 0x20), c + 0x20, "csite 0x%08X" % c)
