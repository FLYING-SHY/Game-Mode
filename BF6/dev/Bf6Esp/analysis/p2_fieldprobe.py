#!/usr/bin/env python3
"""p2_fieldprobe: resolve fieldinf records -> (hash, flags, offset, pointer, strings)."""
import json
import struct
import sys
from collections import Counter
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE, OUTDIR  # noqa: E402


def cstr_at(img, data, rva, limit=80):
    off = img.rva2off(rva)
    if off is None:
        return None
    end = data.find(b"\x00", off, off + limit)
    if end <= off:
        return None
    s = data[off:end]
    if not s or not all(32 <= b < 127 for b in s):
        return None
    return s.decode("ascii")


def main():
    img = Img()
    data = img.data
    fs = img.sec["fieldinf"]
    blob = data[fs["roff"]:fs["roff"] + fs["rsize"]]
    a32 = np.frombuffer(blob, dtype="<u4")
    a64 = np.frombuffer(blob, dtype="<u8")
    print("fieldinf size", hex(len(blob)))

    # --- VA-like qword positions -> delta histogram (bytes) -------------
    m = (a64 >= IMAGE_BASE) & (a64 < IMAGE_BASE + 0x0C000000)
    pos = np.nonzero(m)[0]
    d = Counter((np.diff(pos) * 8).tolist())
    print("VA-qword delta (bytes) top:", d.most_common(12))

    # --- try stride 0x30 over the whole section -------------------------
    for STR in (0x30, 0x20, 0x28, 0x40):
        n = len(blob) // STR
        print(f"\n=== stride {STR:#x}: {n} records ===")
        cols = []
        for j in range(STR // 4):
            cols.append(a32[j:n * (STR // 4):STR // 4])
        for j, col in enumerate(cols):
            vals, cnt = np.unique(col[:300000], return_counts=True)
            top = sorted(zip(vals.tolist(), cnt.tolist()), key=lambda t: -t[1])[:4]
            print(f"  slot{j:2d}: distinct={len(vals):7d} "
                  f"zero={int((col[:300000] == 0).sum()) / len(col[:300000]):.3f} "
                  + " ".join(f"{v:#x}:{c}" for v, c in top))

    # --- resolve pointers of stride-0x30 records -------------------------
    STR = 0x30
    n = len(blob) // STR
    recs = []
    for i in range(0, n, 1):
        base = i * STR
        h = int(a32[i * (STR // 4) + 0])
        flags = int(a32[i * (STR // 4) + 1])
        off = int(a32[i * (STR // 4) + 2])
        u3 = int(a32[i * (STR // 4) + 3])
        p = int(a64[i * (STR // 8) + 2])
        if p < IMAGE_BASE or p >= IMAGE_BASE + 0x0C000000:
            p = 0
        recs.append((i, h, flags, off, u3, p))

    # which slots are pointers?
    ptrs = [r for r in recs if r[5]]
    print(f"\nrecords with a VA at u64[2]: {len(ptrs)}/{len(recs)}")
    print("sample resolved (index, hash, flags, offset, ptr, strings at ptr):")
    seen = Counter()
    for (i, h, fl, off, u3, p) in ptrs[:400]:
        rva = p - IMAGE_BASE
        so = img.rva2off(rva)
        strs = []
        if so is not None:
            for k in range(0, 64, 8):
                v = struct.unpack_from("<Q", data, so + k)[0]
                if IMAGE_BASE <= v < IMAGE_BASE + 0x0C000000:
                    s = cstr_at(img, data, v - IMAGE_BASE)
                    if s:
                        strs.append((hex(v - IMAGE_BASE), s))
        seen[str(tuple(s[1] for s in strs))] += 1
        if i < 40:
            print(f"  i={i:6d} hash={h:#010x} flags={fl:#x} off={off:#x} u3={u3:#x} "
                  f"ptr={rva:#x} -> {strs}")
    print("\nmost common string-tuples seen at the pointer target:")
    for k, v in seen.most_common(12):
        print("  ", v, k)

    json.dump({"records_sample": recs[:5000]},
              open(OUTDIR / "p2_fieldprobe.json", "w"), indent=1)


if __name__ == "__main__":
    main()
