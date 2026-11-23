#!/usr/bin/env python3
"""p2_explore: raw look at the typeinfo / fieldinf sections before modelling."""
import sys
from collections import Counter
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR, hexdump  # noqa: E402

MARKER = b"\x89\x00\x08\x00"


def marks(blob):
    out = []
    p = 0
    while True:
        i = blob.find(MARKER, p)
        if i < 0:
            break
        out.append(i)
        p = i + 1
    return out


def main():
    img = Img()
    for sname in ("typeinfo", "fieldinf"):
        s = img.sec[sname]
        blob = img.data[s["roff"]:s["roff"] + s["rsize"]]
        print("=" * 100)
        print(sname, "rva", hex(s["va"]), "raw", hex(s["roff"]), "size", hex(s["rsize"]))
        print("-- first 0x100 bytes --")
        for l in hexdump(img.data, s["va"], s["roff"], 0x100):
            print("  ", l)
        if sname == "typeinfo":
            m = marks(blob)
            print("-- marker runs --")
            runs = []
            i = 0
            while i < len(m):
                j = i + 1
                while j < len(m) and m[j] - m[j - 1] == 0x50:
                    j += 1
                runs.append((m[i], j - i))
                i = j
            runs.sort(key=lambda r: -r[1])
            print("   n_runs", len(runs), "sum", sum(r[1] for r in runs))
            for r in runs[:10]:
                print(f"    start=0x{r[0]:x} (file 0x{s['roff']+r[0]:x}) len={r[1]} "
                      f"phase={r[0] % 0x50:#x}")
            print("   phases:", Counter(r[0] % 0x50 for r in runs).most_common(8))
            # dump the first records of the biggest run
            big = runs[0]
            print("-- hex at biggest run start --")
            for l in hexdump(img.data, s["va"] + big[0] - 4, s["roff"] + big[0] - 4, 0x120):
                print("  ", l)
            # u32 at marker-4 histogram over all markers
            pre = np.array([int.from_bytes(blob[m - 4:m], "little") for m in m], dtype=np.int64)
            post = np.array([int.from_bytes(blob[m + 4:m + 8], "little") for m in m], dtype=np.int64)
            print("-- u32 at marker-4: distinct", len(np.unique(pre)), "top",
                  Counter(pre.tolist()).most_common(6))
            print("-- u32 at marker+4: distinct", len(np.unique(post)), "top",
                  Counter(post.tolist()).most_common(6))
        else:
            print("-- fieldinf: byte value histogram (top 12) --")
            print("  ", Counter(blob).most_common(12))
            print("-- 0x100 bytes at 0x1000 --")
            for l in hexdump(img.data, s["va"] + 0x1000, s["roff"] + 0x1000, 0x100):
                print("  ", l)


if __name__ == "__main__":
    main()
