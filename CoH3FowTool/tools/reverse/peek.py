"""Read strings at RVAs, and find pointer references to a given RVA.

Usage:
  python peek.py str <rva> [len]
  python peek.py ptr <rva>          # find 8-byte LE pointers to RVA in data sections
  python peek.py lea <rva>          # find 'lea reg,[rip+X]' referencing RVA
"""
import pefile, struct, sys, os, re

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()
secs = []
for s in pe.sections:
    secs.append(dict(name=s.Name.rstrip(b"\x00").decode(), va=s.VirtualAddress,
                     ro=s.PointerToRawData, rs=s.SizeOfRawData, vs=s.Misc_VirtualSize))


def va2off(va):
    for s in secs:
        if s["va"] <= va < s["va"] + max(s["rs"], s["vs"]):
            return s["ro"] + (va - s["va"])
    return None


def cstr(va):
    o = va2off(va)
    if o is None:
        return None
    e = raw.find(b"\x00", o)
    if e < 0:
        return None
    return raw[o:e]


def mode_str(a):
    va = int(a, 16)
    n = int(sys.argv[3]) if len(sys.argv) > 3 else 64
    o = va2off(va)
    if o is None:
        print("unmapped rva")
        return
    print("RVA 0x%X -> file 0x%X  section %s" % (
        va, o, [s["name"] for s in secs if s["va"] <= va < s["va"] + max(s["rs"], s["vs"])]))
    blob = raw[o:o + n]
    print("hex :", blob.hex(" "))
    print("ascii:", "".join(chr(b) if 32 <= b < 127 else "." for b in blob))


def mode_ptr(a):
    va = int(a, 16)
    needle = struct.pack("<Q", va)
    print("pointers to 0x%X:" % va)
    for s in secs:
        if s["name"] == ".text":
            continue
        blob = raw[s["ro"]:s["ro"] + s["rs"]]
        st = 0
        while True:
            i = blob.find(needle, st)
            if i < 0:
                break
            print("  %s file 0x%X  rva 0x%X" % (s["name"], s["ro"] + i, s["va"] + i))
            st = i + 1
    # also 32-bit RVA form
    needle4 = struct.pack("<I", va)
    for s in secs:
        if s["name"] == ".text":
            continue
        blob = raw[s["ro"]:s["ro"] + s["rs"]]
        st = 0
        cnt = 0
        while cnt < 10:
            i = blob.find(needle4, st)
            if i < 0:
                break
            print("  (32-bit) %s file 0x%X rva 0x%X" % (s["name"], s["ro"] + i, s["va"] + i))
            st = i + 1
            cnt += 1


def mode_lea(a):
    """Find 'lea rax/rcx/..., [rip+disp]' whose target == rva."""
    va = int(a, 16)
    text = [s for s in secs if s["name"] == ".text"][0]
    code = raw[text["ro"]:text["ro"] + text["rs"]]
    base = text["va"]
    pat = re.compile(rb"[\x48\x4c]\x8d[\x05\x0d\x15\x1d\x25\x2d\x35\x3d]", re.S)
    n = 0
    for m in pat.finditer(code):
        i = m.start()
        if i + 7 > len(code):
            continue
        disp = struct.unpack_from("<i", code, i + 3)[0]
        target = base + i + 7 + disp
        if target == va:
            print("  lea at rva 0x%X (file 0x%X)" % (base + i, text["ro"] + i))
            n += 1
    print("total lea refs:", n)


if __name__ == "__main__":
    m = sys.argv[1]
    if m == "str":
        mode_str(sys.argv[2])
    elif m == "ptr":
        mode_ptr(sys.argv[2])
    elif m == "lea":
        mode_lea(sys.argv[2])
    else:
        print(__doc__)
