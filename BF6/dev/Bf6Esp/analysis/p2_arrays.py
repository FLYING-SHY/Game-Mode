#!/usr/bin/env python3
"""p2_arrays: find contiguous tables of pointers to .rdata identifier strings
(name tables) in every writable/read-only section, then look for a parallel
array of small integers (member offsets) next to them.
"""
import json
import re
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR, IMAGE_BASE  # noqa: E402

IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}\Z")
REFL = re.compile(rb"^[A-Z][A-Za-z0-9_]{2,40}\Z")


def main():
    img = Img()
    data = img.data
    rd = img.sec[".rdata"]
    rdb = data[rd["roff"]:rd["roff"] + rd["rsize"]]
    base = IMAGE_BASE + rd["va"]

    # map: VA -> (name bytes, is_camel)
    namemap = {}
    i = 0
    while True:
        j = rdb.find(b"\x00", i)
        if j < 0:
            break
        s = rdb[i:j]
        if 1 <= len(s) <= 64 and IDENT.match(s):
            namemap[base + i] = s
        i = j + 1
    print("identifier strings in .rdata:", len(namemap))
    vas = np.array(sorted(namemap), dtype=np.int64)

    sections = [s for s in img.sec if s not in (".text", "ctr", ".pdata", ".rsrc", ".reloc")]
    tables = []
    for sname in sections:
        s = img.sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        n = len(blob) // 8
        q = np.frombuffer(blob[:n * 8], dtype="<u8").astype(np.int64)
        pos = np.clip(np.searchsorted(vas, q), 0, len(vas) - 1)
        isname = vas[pos] == q
        for stride in (1, 2, 3, 4, 5, 6, 8, 12, 16):
            for phase in range(stride):
                st = stride
                hit = isname[phase::st]
                idx = 0
                while idx < len(hit):
                    if hit[idx]:
                        j = idx
                        while j + 1 < len(hit) and hit[j + 1]:
                            j += 1
                        if j - idx + 1 >= 3:
                            slots = [phase + (idx + k) * st for k in range(j - idx + 1)]
                            rva0 = s["va"] + slots[0] * 8
                            names = [namemap[int(vas[pos[k]])].decode("latin1")
                                     for k in slots]
                            after = blob[(slots[-1] + 1) * 8:(slots[-1] + 1) * 8 + 0x80]
                            smalls = []
                            for k in range(0, len(after) - 3, 4):
                                v = int.from_bytes(after[k:k + 4], "little")
                                if 0 < v <= 0x2000 and v % 4 == 0:
                                    smalls.append((k, v))
                            tables.append({"section": sname, "rva": hex(rva0),
                                           "stride": stride * 8, "count": len(slots),
                                           "names": names[:80], "small_after": smalls[:40]})
                        idx = j + 1
                    else:
                        idx += 1
    print("name tables found:", len(tables))
    tables.sort(key=lambda t: -t["count"])
    for t in tables[:30]:
        print(f"\n  {t['section']} rva={t['rva']} n={t['count']}")
        print("    ", t["names"][:25])
        if t["small_after"]:
            print("     small ints after:", t["small_after"][:16])
    json.dump(tables, open(OUTDIR / "p2_name_tables.json", "w"), indent=1)
    print("\nwritten p2_name_tables.json")


if __name__ == "__main__":
    main()
