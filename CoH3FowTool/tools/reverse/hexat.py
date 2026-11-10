#!/usr/bin/env python
"""Dump hex bytes around a file offset and test candidate anchor uniqueness.

usage:
  python hexat.py <file_off_hex> [span]          # dump window
  python hexat.py --count "48 8D 15 A3 65 34 03" # count occurrences of a byte pattern
"""
import sys, os

EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"


def data():
    with open(EXE, "rb") as f:
        return f.read()


def main():
    d = data()
    if len(sys.argv) > 1 and sys.argv[1] == "--count":
        pat = bytes(int(x, 16) for x in sys.argv[2].split())
        hits = []
        i = d.find(pat)
        while i != -1:
            hits.append(i)
            i = d.find(pat, i + 1)
        print(f"pattern len={len(pat)} occurrences={len(hits)}")
        for h in hits[:20]:
            print(f"  file 0x{h:08X}")
        return

    off = int(sys.argv[1], 16)
    span = int(sys.argv[2], 16) if len(sys.argv) > 2 else 0x40
    lo = max(0, off - span // 4)
    hi = min(len(d), off + span)
    for base in range(lo, hi, 16):
        chunk = d[base:base + 16]
        hexs = " ".join(f"{b:02X}" for b in chunk)
        ascii_ = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
        print(f"0x{base:08X}  {hexs:<47}  {ascii_}")


if __name__ == "__main__":
    main()
