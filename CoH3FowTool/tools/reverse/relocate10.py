"""Tenth stage: extract (handler, command-name) pairs from the console command
registration function, and check whether registration is conditional."""
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


def cstr(rva, maxn=64):
    o = off_of(rva)
    if o is None:
        return None
    e = raw.find(b"\x00", o, o + maxn)
    if e < 0:
        return None
    try:
        return raw[o:e].decode("ascii")
    except UnicodeDecodeError:
        return None


md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)

REG_START = 0x00B63410
REG_END = 0x00B638D9
off = off_of(REG_START)
data = raw[off:off + (REG_END - REG_START)]

pending_handler = None
pending_name = None
print("--- console command registrations ---")
for ins in md.disasm(data, 0x140000000 + REG_START):
    txt = "%s %s" % (ins.mnemonic, ins.op_str)
    if ins.mnemonic == "lea" and ins.op_str.startswith("rax, [rip +"):
        d = int(ins.op_str.split("+")[1].rstrip("]"), 16)
        pending_handler = ins.address + ins.size + d
    elif ins.mnemonic == "lea" and ins.op_str.startswith("r8, [rip +"):
        d = int(ins.op_str.split("+")[1].rstrip("]"), 16)
        pending_name = ins.address + ins.size + d
    elif ins.mnemonic == "call" and ins.op_str == "0x140b65790":
        h = (pending_handler - 0x140000000) if pending_handler else None
        n = (pending_name - 0x140000000) if pending_name else None
        print("  name=%-26s handler_rva=0x%08X  handler_func=%s" % (
            repr(cstr(n)) if n else "?", h or 0,
            tuple(hex(x) for x in func_of(h)) if h and func_of(h) else None))
        pending_handler = None
        pending_name = None
