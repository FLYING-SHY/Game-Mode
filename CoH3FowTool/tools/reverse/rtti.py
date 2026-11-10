"""Find a C++ class's RTTI vtable from its mangled type-name string.

Usage:
  python rtti.py ".?AVAntiCheatWorldService@@"          # locate vtable
  python rtti.py ".?AVGameEvent_PlayerCheat@@" --dump   # also dump vtable entries

How it works (MSVC x64):
  TypeDescriptor  = { void* vftable; void* spare; char name[]; }   name at +0x10
  CompleteObjectLocator (x64, 24 bytes):
      u32 signature(1); u32 offset; u32 cdOffset;
      u32 pTypeDescriptor; u32 pClassDescriptor; u32 pSelf;
  vftable[-1] = pointer to the COL  =>  vftable = COL + 24
"""
import pefile, struct, sys

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()
BASE = pe.OPTIONAL_HEADER.ImageBase
secs = []
for s in pe.sections:
    secs.append((s.Name.rstrip(b"\x00").decode(), s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData))


def rva_to_off(rva):
    for n, va, ro, rs in secs:
        if va <= rva < va + max(rs, pe.sections[0].Misc_VirtualSize if False else rs):
            return ro + (rva - va)
    return None


def off_to_rva(off):
    for n, va, ro, rs in secs:
        if ro <= off < ro + rs:
            return va + (off - ro)
    return None


def find_str(s):
    b = s.encode()
    out = []
    i = raw.find(b)
    while i >= 0:
        out.append(i)
        i = raw.find(b, i + 1)
    return out


def vtable_for(name):
    hits = find_str(name)
    if not hits:
        print("!! name not found:", name)
        return []
    res = []
    for nameoff in hits:
        td_off = nameoff - 0x10
        td_rva = off_to_rva(td_off)
        if td_rva is None:
            continue
        # search whole file for 4-byte TD rva
        needle = struct.pack("<I", td_rva)
        j = raw.find(needle)
        while j >= 0:
            hit_rva = off_to_rva(j)
            # the 4-byte TD pointer lives at COL+12
            col_rva = hit_rva - 12 if hit_rva is not None else None
            if col_rva is not None and col_rva >= 12:
                col_off = rva_to_off(col_rva)
                if col_off is not None and col_off >= 0x18:
                    sig, offv, cd, ptd, pcd, pself = struct.unpack_from("<IIIIII", raw, col_off)
                    if sig == 1 and ptd == td_rva and pself == col_rva:
                        # vftable[-1] holds the absolute address of the COL
                        abs_col = struct.pack("<Q", BASE + col_rva)
                        k = raw.find(abs_col)
                        while k >= 0:
                            vt_rva = off_to_rva(k + 8)
                            if vt_rva is not None:
                                res.append((nameoff, td_rva, col_rva, vt_rva))
                            k = raw.find(abs_col, k + 1)
            j = raw.find(needle, j + 1)
    return res


if __name__ == "__main__":
    args = [a for a in sys.argv[1:]]
    dump = "--dump" in args
    args = [a for a in args if not a.startswith("--")]
    for nm in args:
        print("=== %s ===" % nm)
        for nameoff, td, col, vt in vtable_for(nm):
            print("  name file 0x%X | TypeDescriptor rva 0x%X | COL rva 0x%X | vftable rva 0x%X" %
                  (nameoff, td, col, vt))
            if dump:
                vtoff = rva_to_off(vt)
                for k in range(16):
                    fo = vtoff + k * 8
                    if fo + 8 > len(raw):
                        break
                    v = struct.unpack_from("<Q", raw, fo)[0]
                    if v == 0:
                        print("    [%2d] 0x%016X  (end)" % (k, v))
                        break
                    print("    [%2d] rva 0x%X" % (k, v - BASE))
