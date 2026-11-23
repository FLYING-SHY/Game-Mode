#!/usr/bin/env python3
"""p2_neigh: raw neighbourhood dumps around known reflection names.

Goal: find out whether a name string sits inside a table that also carries a
member offset / type id / hash, either in .rdata (next to the string) or in
.data (pointer table).
"""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, hexdump  # noqa: E402

NAMES = [
    ("HealthComponent", 0x8DDA61F),
    ("ClientPlayerManager", 0x8DDA7B2),
    ("TeamId", 0x8F028F3),
    ("Health", 0x8FF9110),
    ("Health", 0x8FF91C0),
    ("LocalPlayer", 0x9198D10),
    ("ClientConnection", 0x90A3908),
    ("GameContext", 0x8E1A8E8),
    ("ClientSoldierEntity", 0x9212030),
]


def main():
    img = Img()
    for nm, foff in NAMES:
        rva = img.off2rva(foff)
        print("=" * 96)
        print(f"{nm} @ file {foff:#x} rva {rva:#x}")
        for l in hexdump(img.data, rva - 0x40, foff - 0x40, 0xC0):
            mark = "  <<<" if (rva - 0x40) <= foff < (rva - 0x40) + 0xC0 else ""
            print("  ", l, mark)

    # .data table around "TeamId"
    print("=" * 96)
    print("### .data records around TeamId pointers")
    for slot in (0x9ACEF60, 0x9ACEFA0, 0x9ACF000):
        print("-" * 80)
        off = img.rva2off(slot)
        for l in hexdump(img.data, slot - 0x40, off - 0x40, 0x100):
            print("  ", l)


if __name__ == "__main__":
    main()
