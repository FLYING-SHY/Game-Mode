import pefile, capstone, struct, sys, bisect

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

pdata = [s for s in secs if s["name"] == ".pdata"][0]
pd = raw[pdata["ro"]:pdata["ro"] + pdata["rs"]]
funcs = []
for i in range(0, len(pd) - 11, 12):
    b, e, u = struct.unpack_from("<III", pd, i)
    if b and e and e > b:
        funcs.append((b, e, u))
funcs.sort()
starts = [f[0] for f in funcs]

md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
md.detail = True

def dump(va, label=""):
    i = bisect.bisect_right(starts, va) - 1
    if i < 0:
        print("no func"); return
    b, e, _ = funcs[i]
    off = va2off(b)
    code = raw[off: off + (e - b)]
    print(f"\n===== func 0x{b:X}-0x{e:X} (size 0x{e-b:X}) {label} =====")
    for insn in md.disasm(code, b):
        if insn.id == 0:
            continue
        mark = ">>" if insn.address == va else "  "
        print(f"{mark} 0x{insn.address:X}  {insn.bytes.hex():<22} {insn.mnemonic} {insn.op_str}")

if __name__ == "__main__":
    for a in sys.argv[1:]:
        dump(int(a, 16))
