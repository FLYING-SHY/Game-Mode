"""Eighth stage: find the cheat-command registration helper (hash 0x5ff018d0c7448e70)."""
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


md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)


def dis(rva, end=None, label=""):
    print("--- %s @0x%08X ---" % (label, rva))
    off = off_of(rva)
    if end is None:
        end = rva + 0x90
    data = raw[off:off + (end - rva)]
    for ins in md.disasm(data, 0x140000000 + rva):
        print("  0x%09X  %-26s %s %s" % (
            ins.address, ins.bytes.hex(" "), ins.mnemonic, ins.op_str))


pat = struct.pack("<Q", 0x5ff018d0c7448e70)
hits = []
i = raw.find(pat)
while i != -1:
    hits.append(i)
    i = raw.find(pat, i + 1)
print("hash hits: %d" % len(hits))
for h in hits[:4]:
    r = h - tro + tva
    fb = func_of(r)
    print("\nhit file 0x%08X rva 0x%08X func=%s" % (
        h, r, tuple(hex(x) for x in fb) if fb else None))
    if fb:
        dis(fb[0], fb[1], "reg helper")
