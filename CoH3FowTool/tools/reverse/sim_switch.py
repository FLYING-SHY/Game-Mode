"""Resolve the sim-side cheat command dispatch switch (PCMD 0x80..0x9B) and dump each case.

Usage: python sim_switch.py
"""
import struct

from capstone import Cs, CS_ARCH_X86, CS_MODE_64

EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
IMAGE_BASE = 0x140000000
TEXT_DELTA = 0xA00  # RVA - file offset for .text


def load_sections(data):
    e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
    coff = e_lfanew + 4
    nsec = struct.unpack_from("<H", data, coff + 2)[0]
    opt_size = struct.unpack_from("<H", data, coff + 16)[0]
    opt = coff + 20
    sec = opt + opt_size
    out = []
    for i in range(nsec):
        off = sec + i * 40
        name = data[off:off + 8].rstrip(b"\x00").decode("ascii", "replace")
        vsize, vaddr, rawsize, rawptr = struct.unpack_from("<IIII", data, off + 8)
        out.append((name, vaddr, vsize, rawptr, rawsize))
    return out


def rva_to_off(sections, rva):
    for name, vaddr, vsize, rawptr, rawsize in sections:
        if vaddr <= rva < vaddr + max(vsize, rawsize):
            return rawptr + (rva - vaddr), name
    raise ValueError(hex(rva))


def main():
    with open(EXE, "rb") as f:
        data = f.read()
    sections = load_sections(data)
    md = Cs(CS_ARCH_X86, CS_MODE_64)

    # jump table referenced at 0x215CF25: mov edx, [r13 + edx*4 + 0x215DBFC]
    table_rva = 0x215DBFC
    off, _ = rva_to_off(sections, table_rva)
    print("jump table @ rva 0x%X file 0x%X" % (table_rva, off))
    entries = []
    for i in range(0x1C):
        tgt = struct.unpack_from("<I", data, off + i * 4)[0]
        entries.append(tgt)
        print("  PCMD 0x%02X -> 0x%08X" % (0x80 + i, tgt))

    print()
    for idx in (3,):  # 0x83 = PCMD_CheatRevealAll
        tgt = entries[idx]
        foff, _ = rva_to_off(sections, tgt)
        print("===== case PCMD 0x%02X  rva 0x%08X  file 0x%08X =====" % (0x80 + idx, tgt, foff))
        blob = data[foff:foff + 0x180]
        for ins in md.disasm(blob, IMAGE_BASE + tgt):
            print("  0x%08X  %-38s ; %s" % (ins.address, ins.mnemonic + " " + ins.op_str, ins.bytes.hex(" ")))
            if ins.mnemonic == "ret":
                break
        print()


if __name__ == "__main__":
    main()
