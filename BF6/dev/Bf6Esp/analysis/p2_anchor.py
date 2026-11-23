#!/usr/bin/env python3
"""p2_anchor: for each typeinfo record, follow the embedded pointers
(.data ptr at q3, .data ptr at q6, typeinfo ptr at q7, .text fn at q8) and
check whether the .data targets are tables of .rdata identifier pointers
(i.e. the class member-NAME table).  This is the anchor hunt.
"""
import json
import re
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR, IMAGE_BASE  # noqa: E402

IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}\Z")


def main():
    img = Img()
    data = img.data
    # --- name map (VA -> bytes) for .rdata
    rd = img.sec[".rdata"]
    rdb = data[rd["roff"]:rd["roff"] + rd["rsize"]]
    base = IMAGE_BASE + rd["va"]
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
    print("identifiers:", len(namemap))

    ts = img.sec["typeinfo"]
    tb = data[ts["roff"]:ts["roff"] + ts["rsize"]]
    t32 = np.frombuffer(tb, dtype="<u4")
    mk = np.nonzero(t32 == 0x00080089)[0]
    # u32 index of marker; hash at marker-1
    recs = []
    for m in mk:
        if m < 1 or m + 18 >= len(t32):
            continue
        h = int(t32[m - 1])
        cnt = int(t32[m + 9])          # u32[10] relative to record start (m-1) -> +10
        # record start index = m-1 ; fields:
        f = [int(t32[m - 1 + k]) for k in range(20)]
        recs.append({"hash": h, "fields": f, "off": ts["roff"] + (m - 1) * 4})
    print("records:", len(recs))

    # field offsets of the qwords we care about (record start = m-1 -> qword k = fields[2k],fields[2k+1])
    def qw(r, k):
        f = r["fields"]
        return (f[2 * k + 1] << 32) | f[2 * k]

    def sec_of_va(va):
        rva = va - IMAGE_BASE
        for n, s in img.sec.items():
            if s["va"] <= rva < s["va"] + s["vsize"]:
                return n
        return None

    # how many records have q3 / q6 pointing into .data, q7 into typeinfo, q8 into .text?
    stat = {"q3_data": 0, "q6_data": 0, "q7_typeinfo": 0, "q8_text": 0}
    samples = []
    nq3_name_tables = 0
    for r in recs:
        q3, q6, q7, q8 = qw(r, 3), qw(r, 6), qw(r, 7), qw(r, 8)
        s3, s6 = sec_of_va(q3), sec_of_va(q6)
        s7, s8 = sec_of_va(q7), sec_of_va(q8)
        if s3 == ".data":
            stat["q3_data"] += 1
        if s6 == ".data":
            stat["q6_data"] += 1
        if s7 == "typeinfo":
            stat["q7_typeinfo"] += 1
        if s8 == ".text":
            stat["q8_text"] += 1
        # is q3 a table of name pointers?
        if s3 == ".data":
            fo = img.rva2off(q3 - IMAGE_BASE)
            cnt = 0
            names = []
            for k in range(64):
                p = int.from_bytes(data[fo + k * 8:fo + k * 8 + 8], "little")
                if p in namemap:
                    cnt += 1
                    names.append(namemap[p].decode("latin1"))
                else:
                    break
            if cnt >= 2:
                nq3_name_tables += 1
                if len(samples) < 12:
                    samples.append({"hash": hex(r["hash"]), "count": r["fields"][10],
                                    "q3": hex(q3), "q3_names": names[:20],
                                    "q6": hex(q6), "q7": hex(q7), "q8": hex(q8),
                                    "rec_off": hex(r["off"])})
    print("section stats:", stat)
    print("records whose q3 .data target is a name-pointer table:", nq3_name_tables)
    for s in samples:
        print("  ", s)
    json.dump({"stat": stat, "nq3_name_tables": nq3_name_tables, "samples": samples},
              open(OUTDIR / "p2_anchor.json", "w"), indent=1)
    print("written p2_anchor.json")


if __name__ == "__main__":
    main()
