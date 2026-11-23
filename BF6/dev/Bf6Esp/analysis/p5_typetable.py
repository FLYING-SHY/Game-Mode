#!/usr/bin/env python3
"""p5_typetable.py - inspect what the typeinfo/fieldinf .rdata pointers really point at."""
import json
import struct
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402


def qword_slots(img, secname):
    s = img.sec[secname]
    raw = img.data[s["roff"]: s["roff"] + s["rsize"]]
    a = np.frombuffer(raw, dtype="<u8")
    vals = a
    out = []
    rdata_lo = img.sec[".rdata"]["va"]
    rdata_hi = rdata_lo + img.sec[".rdata"]["rsize"]
    for i in range(len(vals)):
        v = int(vals[i])
        if rdata_lo <= v < rdata_hi:
            out.append((s["va"] + i * 8, v))
    return out


def main():
    img = Img()
    rdata_lo = img.sec[".rdata"]["va"]
    res = {}
    for secname in ("typeinfo", "fieldinf"):
        slots = qword_slots(img, secname)
        print("=== %s: %d qword slots -> .rdata ===" % (secname, len(slots)))
        if not slots:
            continue
        allzero = 0
        targets = []
        for slot, v in slots[:20]:
            off = img.rva2off(v)
            chunk = img.data[off:off + 48]
            txt = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
            if not any(chunk):
                allzero += 1
            print("  slot rva 0x%08x -> rva 0x%08x | %s | %r" % (slot, v, chunk.hex(" "), txt))
            targets.append(v)
        # count zero-targets over all slots (sampled)
        zero = 0
        n = len(targets)
        for slot, v in slots:
            off = img.rva2off(v)
            if not any(img.data[off:off + 16]):
                zero += 1
        print("  all-zero 16B targets: %d / %d" % (zero, len(slots)))
        tvals = sorted(set(v for _, v in slots))
        print("  distinct targets: %d  min=0x%08x max=0x%08x" % (len(tvals), tvals[0], tvals[-1]))
        # are targets inside a narrow region?
        if tvals:
            lo, hi = tvals[0], tvals[-1]
            print("  target span: 0x%x..0x%x (%.1f KB)" % (lo, hi, (hi - lo) / 1024.0))
            off = img.rva2off(lo)
            print("  bytes at min target:", img.data[off:off + 96].hex(" "))
            print("  ascii:", "".join(chr(b) if 32 <= b < 127 else "." for b in img.data[off:off + 96]))
        res[secname] = {"slots": len(slots), "distinct": len(tvals),
                         "zero_targets": zero,
                         "min": hex(tvals[0]) if tvals else None,
                         "max": hex(tvals[-1]) if tvals else None}

    (OUTDIR / "p5_typetable.json").write_text(json.dumps(res, indent=1), encoding="utf-8")
    print("\nwrote", OUTDIR / "p5_typetable.json")


if __name__ == "__main__":
    main()
