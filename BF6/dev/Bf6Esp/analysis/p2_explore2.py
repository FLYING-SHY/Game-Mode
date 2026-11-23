#!/usr/bin/env python3
"""p2_explore2: annotated record dumps for typeinfo (0x50) and fieldinf (0x20)."""
import sys
from collections import Counter
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE  # noqa: E402

MARKER = b"\x89\x00\x08\x00"


def main():
    img = Img()
    rlo, rhi = img.sec_range(".rdata")
    tlo, thi = img.sec_range(".text")
    dlo, dhi = img.sec_range(".data")
    plo, phi = img.sec_range(".pdata")

    def ann(v):
        if v == 0:
            return "0"
        for nm, lo, hi in ((".rdata", rlo, rhi), (".text", tlo, thi),
                           (".data", dlo, dhi), (".pdata", plo, phi)):
            if lo <= v < hi:
                return f"RVA->{nm}"
        if v < 0x1000:
            return f"small({v})"
        return ""

    # ---------------- typeinfo -------------------------------------------
    ti = img.sec["typeinfo"]
    blob = img.data[ti["roff"]:ti["roff"] + ti["rsize"]]
    marks = []
    p = 0
    while True:
        i = blob.find(MARKER, p)
        if i < 0:
            break
        marks.append(i)
        p = i + 1
    runs = []
    i = 0
    while i < len(marks):
        j = i + 1
        while j < len(marks) and marks[j] - marks[j - 1] == 0x50:
            j += 1
        runs.append((marks[i], j - i))
        i = j
    runs.sort(key=lambda r: -r[1])
    print("### typeinfo: 30 records of the biggest run, u32 view")
    start = runs[0][0] - 4
    for r in range(30):
        off = ti["roff"] + start + r * 0x50
        raw = img.data[off:off + 0x50]
        u32 = [int.from_bytes(raw[k:k + 4], "little") for k in range(0, 0x50, 4)]
        nz = {k: (hex(v), ann(v)) for k, v in enumerate(u32) if v}
        print(f"  rec{r:3d} off={hex(off)} nz={nz}")
    print()
    print("### typeinfo: 30 records of the 3rd biggest run")
    start = runs[2][0] - 4
    for r in range(30):
        off = ti["roff"] + start + r * 0x50
        raw = img.data[off:off + 0x50]
        u32 = [int.from_bytes(raw[k:k + 4], "little") for k in range(0, 0x50, 4)]
        nz = {k: (hex(v), ann(v)) for k, v in enumerate(u32) if v}
        print(f"  rec{r:3d} off={hex(off)} nz={nz}")

    # ---------------- fieldinf -------------------------------------------
    fi = img.sec["fieldinf"]
    fblob = img.data[fi["roff"]:fi["roff"] + fi["rsize"]]
    print()
    print("### fieldinf: 40 records at stride 0x20 from section start")
    for r in range(40):
        off = fi["roff"] + r * 0x20
        raw = img.data[off:off + 0x20]
        u32 = [int.from_bytes(raw[k:k + 4], "little") for k in range(0, 0x20, 4)]
        print(f"  rec{r:3d} off={hex(off)} u32={[hex(v) for v in u32]}")

    # global per-slot stats at stride 0x20
    n = len(fblob) // 0x20
    mat = np.frombuffer(fblob[:n * 0x20], dtype="<u4").reshape(n, 8)
    print()
    print("### fieldinf stride 0x20 global per-slot stats")
    for j in range(8):
        col = mat[:, j]
        vals, cnts = np.unique(col, return_counts=True)
        order = np.argsort(-cnts)[:6]
        print(f"  slot{j}: zero={int((col==0).sum())}/{n} distinct={len(vals)} "
              f"top={[(hex(int(vals[k])), int(cnts[k])) for k in order]}")

    # is the +0x10 slot sequential?
    col = mat[:, 4].astype(np.int64)
    d = np.diff(col)
    print("  slot4 diffs:", Counter(d.tolist()).most_common(6))
    print("  slot4 range:", hex(int(col.min())), hex(int(col.max())))

    # check slot6 vs next slot0
    a = mat[:-1, 6].astype(np.int64)
    b = mat[1:, 0].astype(np.int64)
    print("  slot6[i]==slot0[i+1] frac:", float((a == b).mean()), "of", len(a))


if __name__ == "__main__":
    main()
