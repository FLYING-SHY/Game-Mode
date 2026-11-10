import pefile, capstone, sys

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
pe = pefile.PE(P, fast_load=True)
text = [s for s in pe.sections if s.Name.rstrip(b"\x00") == b".text"][0]
raw = open(P, "rb").read()
code = raw[text.PointerToRawData: text.PointerToRawData + text.SizeOfRawData]
tbase = text.VirtualAddress

md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
md.detail = True

def va2off(va):
    return va - tbase

def dump(va, before=0x120, after=0x180, label=""):
    start = va - before
    off = va2off(start)
    chunk = code[off: off + before + after]
    print(f"\n########## {label} @0x{va:X} ##########")
    for insn in md.disasm(chunk, start):
        mark = ">>" if insn.address == va else "  "
        print(f"{mark} 0x{insn.address:X}  {insn.bytes.hex():<24} {insn.mnemonic} {insn.op_str}")

if __name__ == "__main__":
    for a in sys.argv[1:]:
        dump(int(a, 16), label="")
