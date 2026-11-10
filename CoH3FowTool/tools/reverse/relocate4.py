"""Fourth stage: find the cheat gate candidates and the reveal-all sim handler."""
import struct

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
    for i in range(len(code) - 5):
        if code[i] != 0xE8:
            continue
        d = struct.unpack_from("<i", code, i + 1)[0]
        if tva + i + 5 + d == target_rva:
            out.append(tva + i)
    return out


def rip_xrefs(target_rva):
    out = []
    for i in range(len(code) - 6):
        b0 = code[i]
        if b0 in (0x48, 0x4c, 0x4d, 0x49) and code[i + 1] in (0x8d, 0x8b) and \
                code[i + 2] in (0x05, 0x0d, 0x15, 0x1d, 0x25, 0x2d, 0x35, 0x3d):
            d = struct.unpack_from("<i", code, i + 3)[0]
            if tva + i + 7 + d == target_rva:
                out.append(tva + i)
    return out


def show(tag, pat, span=0x40):
    i = raw.find(pat)
    print("\n%s  pattern=%s  file 0x%08X" % (
        tag, " ".join("%02X" % b for b in pat), i))
    lo = max(0, i - span)
    for off in range(lo, min(len(raw), i + len(pat) + span), 16):
        chunk = raw[off:off + 16]
        print("   0x%08X  %s" % (off, " ".join("%02X" % b for b in chunk)))


# --- gate candidates ---------------------------------------------------------
for off in (0x014E77D0, 0x014E8FD0, 0x0151E850):
    rva = rva_of(off)
    cs = calls_to(rva)
    print("gate candidate file 0x%08X rva 0x%08X -> %d direct calls" % (
        off, rva, len(cs)))
    show("  ctx", raw[off:off + 12], 0x18)

# --- 'revealed fow' xref -----------------------------------------------------
s = raw.find(b"revealed fow")
r = rva_of(s)
print("\n'revealed fow' file 0x%08X rva 0x%08X" % (s, r))
for x in rip_xrefs(r):
    print("   xref rva 0x%08X file 0x%08X" % (x, off_of(x)))

# --- blacklist loop: 'cmp rax, 0x30' -----------------------------------------
pat = bytes.fromhex("48 83 F8 30")
i = raw.find(pat)
while i != -1:
    print("\nblacklist-loop candidate file 0x%08X rva 0x%08X" % (i, rva_of(i)))
    for off in range(i - 0x30, i + 0x18, 16):
        print("   0x%08X  %s" % (off, " ".join("%02X" % b for b in raw[off:off + 16])))
    i = raw.find(pat, i + 1)
