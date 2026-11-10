"""Exact byte dump with absolute file offsets (no guessing).

usage: dump_at.py <file_off_hex> [span_hex]
prints one line per 16 bytes with the EXACT file offset of the first byte.
"""
import sys

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

off = int(sys.argv[1], 16)
span = int(sys.argv[2], 16) if len(sys.argv) > 2 else 0x40

with open(P, "rb") as f:
    f.seek(off)
    data = f.read(span)

for i in range(0, len(data), 16):
    chunk = data[i:i + 16]
    hexs = " ".join("%02X" % b for b in chunk)
    print("0x%08X  %s" % (off + i, hexs))
