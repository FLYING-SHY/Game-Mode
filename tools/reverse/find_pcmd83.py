#!/usr/bin/env python3
"""Locate the sim-side PCMD_CheatRevealAll handler in the new build.

Strategy:
 1. find the "revealed fow" format string -> RVA
 2. look for a 64-bit pointer to it inside .rdata/.data  (string table)
 3. look for RIP-relative references to that pointer from .text
 4. look for the GE_PlayerCheat event id 0x580000 as an immediate in .text

Usage:
    python find_pcmd83.py
"""
import struct

EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

# section table of the new build: name, va, vsize, raw_off, raw_size
SECTIONS = [
    (".text",   0x00001000, 0x3D53C00, 0x00000600, 0x3D52C00),
    (".rdata",  0x03D54000, 0x1EA6000, 0x03D53200, 0x1EA6000),
    (".data",   0x05BFA000, 0x0534E00, 0x05BF9200, 0x0534E00),
]
IMAGE_BASE = 0x140000000


def main():
    data = open(EXE, "rb").read()

    def rva_of(off):
        for name, va, vs, ro, rs in SECTIONS:
            if ro <= off < ro + rs:
                return va + (off - ro)
        return None

    def off_of(rva):
        for name, va, vs, ro, rs in SECTIONS:
            if va <= rva < va + vs:
                return ro + (rva - va)
        return None

    # ---- 1. the format string -------------------------------------------
    needles = [
        b"MOD -- (CHEAT) player %d revealed fow",
        b"revealed fow",
        b"trying to run a cheat command",
    ]
    for nd in needles:
        pos = 0
        while True:
            i = data.find(nd, pos)
            if i < 0:
                break
            print("STRING %-40r file=0x%08X rva=%s" % (
                nd[:38], i, ("0x%08X" % rva_of(i)) if rva_of(i) else "-"))
            pos = i + 1

    # ---- 2. pointers to those strings -----------------------------------
    targets = []
    for nd in needles:
        i = data.find(nd)
        if i < 0:
            continue
        r = rva_of(i)
        if r:
            targets.append((nd, r))

    print()
    for nd, r in targets:
        va = IMAGE_BASE + r
        for pack, label in (("<Q", "VA"), ("<I", "RVA")):
            blob = struct.pack(pack, va if label == "VA" else r)
            pos = 0
            while True:
                j = data.find(blob, pos)
                if j < 0:
                    break
                jr = rva_of(j)
                print("PTR %-6s %-30r -> file=0x%08X rva=%s" % (
                    label, nd[:28], j, ("0x%08X" % jr) if jr else "-"))
                pos = j + 1

    # ---- 3. RIP-relative references from .text to those pointers ---------
    text_va, text_vs, text_ro, text_rs = 0x00001000, 0x3D53C00, 0x00000600, 0x3D52C00
    text = data[text_ro:text_ro + text_rs]

    print()
    for nd, r in targets:
        va = IMAGE_BASE + r
        blob = struct.pack("<Q", va)
        # find pointer slots in .rdata/.data
        slots = []
        pos = 0
        while True:
            j = data.find(blob, pos)
            if j < 0:
                break
            slots.append(j)
            pos = j + 1
        for slot in slots:
            sr = rva_of(slot)
            if sr is None:
                continue
            # scan .text for rip-relative lea/mov targeting sr
            for i in range(len(text) - 7):
                if text[i] in (0x48, 0x4C) and text[i + 1] in (0x8D, 0x8B):
                    modrm = text[i + 2]
                    if (modrm & 0xC7) == 0x05:
                        disp = struct.unpack_from("<i", text, i + 3)[0]
                        inst_rva = text_va + i
                        if inst_rva + 7 + disp == sr:
                            print("XREF %-30r -> slot rva=0x%08X  <- code rva=0x%08X" % (
                                nd[:28], sr, inst_rva))

    # ---- 4. event id 0x580000 as an immediate ----------------------------
    print()
    for pat, label in ((b"\xba\x00\x00\x58\x00", "mov edx,0x580000"),
                       (b"\x41\xba\x00\x00\x58\x00", "mov r10d,0x580000"),
                       (b"\xb9\x00\x00\x58\x00", "mov ecx,0x580000")):
        pos = 0
        while True:
            i = text.find(pat, pos)
            if i < 0:
                break
            print("EVENT %-18s .text+0x%08X  rva=0x%08X" % (
                label, i, text_va + i))
            pos = i + 1


if __name__ == "__main__":
    main()
