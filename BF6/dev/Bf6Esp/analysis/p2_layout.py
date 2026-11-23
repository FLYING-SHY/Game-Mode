#!/usr/bin/env python3
"""p2_layout: rigorous record-layout recovery for the `typeinfo` and `fieldinf`
sections of bf6.exe.

Strategy
  1. locate all "VA-like" qwords in a section (value inside the image) and use
     the histogram of their deltas to recover the record stride;
  2. for every candidate stride score each u32 slot (distinct values, zero
     fraction, pointer-likeness) and keep the winning layout;
  3. dump annotated example records and resolve every pointer to a string.
"""
import json
import struct
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE, OUTDIR  # noqa: E402

IMAGE_TOP = IMAGE_BASE + 0x0C000000
KIND = 0x00080089


def va_like_mask(arr):
    return (arr >= IMAGE_BASE) & (arr < IMAGE_TOP)


def delta_histogram(arr, mask, top=25):
    pos = np.nonzero(mask)[0]
    if len(pos) < 2:
        return []
    d = np.diff(pos)
    c = Counter(int(x) for x in d)
    return c.most_common(top)


def slot_profile(arr, stride, nsample=200000):
    """arr: uint32 view of the section. Returns per-slot stats."""
    n = (len(arr) // stride)
    prof = []
    for j in range(stride // 4):
        col = arr[j:n * stride // 4:stride // 4]
        if len(col) == 0:
            prof.append(None)
            continue
        if len(col) > nsample:
            col = col[:nsample]
        vals, cnt = np.unique(col, return_counts=True)
        nz = int((col != 0).sum())
        # pointer-likeness: value could be an RVA (small) or VA
        va_like = int(((col >= IMAGE_BASE) & (col < IMAGE_TOP)).sum())
        small = int(((col > 0) & (col < 0x10000)).sum())
        prof.append({
            "slot": j,
            "distinct": int(len(vals)),
            "zero_frac": round(1 - nz / len(col), 4),
            "va_like_frac": round(va_like / len(col), 4),
            "small_frac": round(small / len(col), 4),
            "top": [[hex(int(v)), int(c)] for v, c in
                    sorted(zip(vals, cnt), key=lambda t: -t[1])[:6]],
        })
    return prof


def main():
    img = Img()
    data = img.data

    out = {}
    for secname in ("typeinfo", "fieldinf"):
        s = img.sec[secname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        a32 = np.frombuffer(blob, dtype="<u4")
        a64 = np.frombuffer(blob, dtype="<u8")
        print(f"\n{'=' * 70}\n### {secname}: rva={s['va']:#x} file={s['roff']:#x} "
              f"size={s['rsize']:#x} ({len(blob)} bytes)")
        m64 = va_like_mask(a64)
        print("  VA-like qwords:", int(m64.sum()))
        print("  delta histogram of VA-like qword positions:",
              delta_histogram(a64, m64))

        # marker runs
        if secname == "typeinfo":
            mk = np.nonzero(a32 == KIND)[0]
            print("  kind-marker u32 count:", len(mk))
            runs = []
            cur = [int(mk[0])]
            for i in range(1, len(mk)):
                if int(mk[i]) - int(mk[i - 1]) == 0x50:
                    cur.append(int(mk[i]))
                else:
                    runs.append(cur)
                    cur = [int(mk[i])]
            runs.append(cur)
            runs.sort(key=len, reverse=True)
            print("  top runs (len, first_marker_off):",
                  [(len(r), hex(r[0])) for r in runs[:10]])
            out[secname] = {"rva": s["va"], "file_off": s["roff"], "size": s["rsize"],
                            "marker_count": len(mk),
                            "runs": [[len(r), r[0]] for r in runs[:40]]}

        # stride candidates
        cands = [0x10, 0x18, 0x20, 0x24, 0x28, 0x30, 0x38, 0x40, 0x48]
        scores = {}
        for st in cands:
            prof = slot_profile(a32, st)
            if prof is None:
                continue
            # heuristic score: low-distinct slots are structural, high-distinct
            # slots carry hashes; a good stride has BOTH kinds
            low = sum(1 for p in prof if p and p["distinct"] <= 64)
            high = sum(1 for p in prof if p and p["distinct"] > 1000)
            ptr = sum(1 for p in prof if p and p["va_like_frac"] > 0.3)
            scores[hex(st)] = {"low_distinct_slots": low, "high_distinct_slots": high,
                               "va_slots": ptr,
                               "slots": prof}
        for k, v in sorted(scores.items(), key=lambda kv: -kv[1]["low_distinct_slots"]):
            print(f"  stride {k}: low={v['low_distinct_slots']} "
                  f"high={v['high_distinct_slots']} va={v['va_slots']}")
        out[secname]["stride_scores"] = scores

    json.dump(out, open(OUTDIR / "p2_layout_probe.json", "w"), indent=1)
    print("\nwritten p2_layout_probe.json")


if __name__ == "__main__":
    main()
