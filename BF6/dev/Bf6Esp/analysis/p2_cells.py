#!/usr/bin/env python3
"""p2_cells: dump .data cells that hold reflection-name pointers + section heads."""
import json
import sys
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE, OUTDIR  # noqa: E402


def dump(img, data, rva, length=0x60, label=""):
    off = img.rva2off(rva)
    if off is None:
        print(f"  (rva {rva:#x} not mapped)")
        return
    end = off + length
    print(f"  --- {label} rva={rva:#x} file={off:#x} ---")
    for k in range(0, length, 16):
        chunk = data[off + k:off + k + 16]
        if not chunk:
            break
        h = " ".join(f"{b:02x}" for b in chunk)
        a = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
        print(f"   {rva + k:012x} {h:<47} {a}")


def main():
    img = Img()
    data = img.data
    hits = json.load(open(OUTDIR / "p2_name_ptrs.json"))
    print("pointer hits:", len(hits))
    print(Counter(h["target"] for h in hits).most_common())

    # group by table: contiguous slot_rva gaps < 0x400
    hits.sort(key=lambda h: h["slot_rva"])
    groups = []
    cur = []
    for h in hits:
        if cur and h["slot_rva"] - cur[-1]["slot_rva"] > 0x400:
            groups.append(cur)
            cur = []
        cur.append(h)
    if cur:
        groups.append(cur)
    print("tables:", len(groups))
    for g in groups:
        print(f"  table {g[0]['sec']} slot {g[0]['slot_rva']:#x}..{g[-1]['slot_rva']:#x} "
              f"n={len(g)} names={[x['target'].split('@')[0] for x in g][:12]}")

    # dump the biggest table fully
    g = max(groups, key=len)
    print("\n=== biggest table dump ===")
    dump(img, data, g[0]["slot_rva"] - 0x40, 0x200, "table start")

    # typeinfo head
    ts = img.sec["typeinfo"]
    print("\n=== typeinfo head ===")
    dump(img, data, ts["va"], 0x200, "typeinfo")
    fs = img.sec["fieldinf"]
    print("\n=== fieldinf head ===")
    dump(img, data, fs["va"], 0x200, "fieldinf")

    # the 9715 .rdata pointer targets inside typeinfo (raw)
    tblob = data[ts["roff"]:ts["roff"] + ts["rsize"]]
    import numpy as np
    arr = np.frombuffer(tblob, dtype="<u8").astype(np.int64)
    rlo, rhi = img.sec_range(".rdata")
    inr = (arr >= IMAGE_BASE + rlo) & (arr < IMAGE_BASE + rhi)
    pos = np.nonzero(inr)[0]
    print("\n=== typeinfo .rdata pointer targets (raw 64B each) ===")
    for i, p in enumerate(pos[:12]):
        t = int(arr[p]) - IMAGE_BASE
        off = img.rva2off(t)
        print(f"\n[target {i}] slot_rva={ts['va'] + int(p) * 8:#x} target_rva={t:#x} "
              f"file={off:#x}")
        for k in range(0, 64, 16):
            chunk = data[off + k:off + k + 16]
            h = " ".join(f"{b:02x}" for b in chunk)
            a = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
            print(f"    {t + k:012x} {h:<47} {a}")


if __name__ == "__main__":
    main()
