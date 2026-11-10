#!/usr/bin/env python3
"""Scan .text for every RIP-relative reference to a target RVA.

For each 4-byte displacement at offset i, assume the instruction ends at i+4:
    target_rva = text_va + i + 4 + disp
and report hits for the requested target.

Usage:
    python rip_to.py <target_rva_hex> [<target_rva_hex> ...]
"""
import struct
import sys

EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
TEXT_VA = 0x00001000
TEXT_RO = 0x00000600
TEXT_RS = 0x3D52C00


def main():
    targets = {int(a, 16) for a in sys.argv[1:]}
    if not targets:
        print("usage: rip_to.py <rva_hex> [...]")
        return
    data = open(EXE, "rb").read()
    text = data[TEXT_RO:TEXT_RO + TEXT_RS]
    hits = 0
    for i in range(len(text) - 4):
        disp = struct.unpack_from("<i", text, i)[0]
        # instruction ends at i+4 -> next instruction address
        nxt = TEXT_VA + i + 4
        t = nxt + disp
        if t in targets:
            rva = TEXT_VA + i
            lo = max(0, i - 8)
            print("HIT rva=0x%08X  target=0x%08X  bytes[-8..+8]=%s" % (
                rva, t, text[lo:i + 12].hex(" ")))
            hits += 1
    print("total hits:", hits)


if __name__ == "__main__":
    main()
