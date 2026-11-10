"""Fifth stage: identify the new SimWorld::DoCommand + its callers, and hunt the
cheat gate (reads of the [ctx+0x670] / [ctx+0x671] flags)."""
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


def rva_of(off):
    for name, (va, po, sz) in secs.items():
        if po <= off < po + sz:
            return va + (off - po)
    return None


def off_of(rva):
    for name, (va, po, sz) in secs.items():
        if va <= rva < va + sz:
            return po + (rva - va)
    return None


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


def dis(rva, span=0x60):
    off = off_of(rva)
    data = raw[off:off + span]
    for ins in md.disasm(data, 0x140000000 + rva):
        print("  0x%09X  %-26s %s %s" % (
            ins.address, ins.bytes.hex(" "), ins.mnemonic, ins.op_str))


# ---- 1. DoCommand function --------------------------------------------------
KEY = 0x00482112  # the blacklist cmp inside DoCommand
fb = func_of(KEY)
print("blacklist cmp rva 0x%08X -> func %s" % (
    KEY, tuple(hex(x) for x in fb) if fb else None))
if fb:
    print("--- DoCommand head ---")
    dis(fb[0], 0x60)

# ---- 2. callers of DoCommand ------------------------------------------------
if fb:
    cs = calls_to(fb[0])
    print("\ncallers of DoCommand (rva 0x%08X): %d" % (fb[0], len(cs)))
    for c in cs:
        # look back 0x40 bytes for 'mov edx, 0x83'
        back = raw[off_of(c) - 0x50:off_of(c)]
        if bytes.fromhex("BA 83 00 00 00") in back:
            print("  *** PCMD 0x83 caller rva 0x%08X file 0x%08X" % (c, off_of(c)))
            dis(max(fb[0], c - 0x60), 0x80)
        else:
            print("      caller rva 0x%08X" % c)

# ---- 3. cheat gate flag reads ----------------------------------------------
print("\n=== reads of [ctx+0x670] / [ctx+0x671] ===")
for pat in (b"\x70\x06\x00\x00", b"\x71\x06\x00\x00"):
    i = raw.find(pat, tro)
    n = 0
    while i != -1 and i < tro + trs and n < 40:
        n += 1
        print("  file 0x%08X rva 0x%08X  ctx=%s" % (
            i, rva_of(i), " ".join("%02X" % b for b in raw[i - 4:i + 6])))
        i = raw.find(pat, i + 1)
    print("  -- pattern %s total %d --" % (pat.hex(), n))
