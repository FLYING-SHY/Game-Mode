"""Dump .data objects that appear as top values in fieldinf slots."""
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec


def str_at_rva(rva, limit=60):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    o = s["roff"] + (rva - s["va"])
    e = DATA.find(b"\0", o, o + limit)
    if e > o:
        b = DATA[o:e]
        if len(b) > 1 and all(32 <= c < 127 for c in b):
            return b.decode("ascii")
    return None


def dump(rva, n=0x60):
    o = img.rva2off(rva)
    print(f"--- {rva:08x} ({img.sec_of(rva)}) ---")
    for k in range(0, n, 16):
        chunk = DATA[o + k:o + k + 16]
        txt = "".join(chr(c) if 32 <= c < 127 else "." for c in chunk)
        print(f"  {rva+k:08x}  " + " ".join(f"{x:02x}" for x in chunk) + "  |" + txt + "|")
    for k in range(0, n, 8):
        v = int.from_bytes(DATA[o + k:o + k + 8], "little")
        s = img.sec_of(v) if 0 < v < 0x7FFFFFFF else None
        st = str_at_rva(v) if s == ".rdata" else None
        if st:
            print(f"      +{k:02x}: {v:016x} -> {s!r}")


for rva in (0x9FB5B88, 0x9FB5BE8, 0x9FB5C28, 0x9FB5B30, 0x9FB57F8, 0x9FB5BC8):
    dump(rva)
    print()

# also the .data source array 0x9AF48C8 region
dump(0x9AF48C8, 0x100)
