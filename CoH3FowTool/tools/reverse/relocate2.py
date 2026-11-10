"""Second stage of re-location on the current exe.

- find all call sites of a given rva
- loose search for the second cheat gate
- xref of the SimWorld 'untrusted source' string
- .pdata function boundaries for a set of rvas
"""
import bisect
import struct
import sys

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


def calls_to(target_rva):
    out = []
    n = len(code)
    for i in range(n - 5):
        if code[i] != 0xE8:
            continue
        d = struct.unpack_from("<i", code, i + 1)[0]
        if tva + i + 5 + d == target_rva:
            out.append(tva + i)
    return out


def find_all(pat):
    out = []
    i = raw.find(pat)
    while i != -1:
        out.append(i)
        i = raw.find(pat, i + 1)
    return out


# ---- .pdata ----------------------------------------------------------------
pdata = []
with open(P, "rb") as f:
    pass

# parse .pdata manually from the section table
pd_va = pd_po = pd_sz = None
for name, (va, po, sz) in secs.items():
    if name == ".pdata":
        pd_va, pd_po, pd_sz = va, po, sz
starts = []
ends = []
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
    if rva < ends[i]:
        return (starts[i], ends[i])
    return None


# ---- 1. call sites of CheatGateA candidate ---------------------------------
gate = 0x014E81D0
cs = calls_to(gate)
print("call sites of rva 0x%08X : %d" % (gate, len(cs)))
for c in cs[:120]:
    print("   rva 0x%08X  file 0x%08X  func=%s" % (
        c, off_of(c), tuple(hex(x) for x in func_of(c)) if func_of(c) else None))

# ---- 2. loose search for a second gate -------------------------------------
for pat in ("0F B6 81 71", "0F B6 81 70", "8B 81 71 06 00 00",
            "0F B6 41 71", "0F B6 42 71", "0F B6 43 71", "0F B6 44 24"):
    hits = find_all(bytes.fromhex(pat))
    print("\nloose %-16s hits=%d  %s" % (
        pat, len(hits), " ".join("0x%08X" % h for h in hits[:12])))

# ---- 3. 'untrusted source' string xrefs ------------------------------------
needle = b"SimWorld::DoCommand - (sender %u) trying to run a cheat command"
for off in find_all(needle):
    rva = rva_of(off)
    print("\n'untrusted source' string file 0x%08X rva 0x%08X" % (off, rva))
    for i in range(len(code) - 6):
        b0 = code[i]
        if b0 in (0x48, 0x4c, 0x4d, 0x49) and code[i + 1] in (0x8d, 0x8b) and \
                code[i + 2] in (0x05, 0x0d, 0x15, 0x1d, 0x25, 0x2d, 0x35, 0x3d):
            d = struct.unpack_from("<i", code, i + 3)[0]
            if tva + i + 7 + d == rva:
                print("   xref rva 0x%08X file 0x%08X func=%s" % (
                    tva + i, off_of(tva + i),
                    tuple(hex(x) for x in func_of(tva + i)) if func_of(tva + i) else None))

# ---- 4. pdata bounds for the interesting rvas ------------------------------
for r in (0x001D8F91, 0x001D8FA6, 0x014E81D0, 0x006BA86A, 0x00B637EF):
    print("pdata for 0x%08X -> %s" % (r, tuple(hex(x) for x in func_of(r)) if func_of(r) else None))
