#!/usr/bin/env python3
"""p2_code: find code that references the typeinfo/fieldinf sections and the
CRC32 tables, then dump the referencing code bytes for manual decoding.
"""
import json
import struct
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE, OUTDIR  # noqa: E402

CRC_TABLE_FILE_OFFS = [0x855bb50, 0x85d3bb0, 0x8a29430, 0x8d2bc30, 0x92be240]


def standard_crc32_table():
    t = []
    for i in range(256):
        c = i
        for _ in range(8):
            c = (c >> 1) ^ (0xEDB88320 if c & 1 else 0)
        t.append(c)
    return t


def dump_bytes(data, rva, n=0x60, base_label=""):
    off = img.rva2off(rva)
    if off is None:
        print(f"  (rva {rva:#x} unmapped)")
        return []
    lines = []
    for k in range(0, n, 16):
        chunk = data[off + k:off + k + 16]
        if not chunk:
            break
        h = " ".join(f"{b:02x}" for b in chunk)
        a = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
        lines.append(f"   {rva + k:012x} {h:<47} {a}")
    return lines


def main():
    global img
    img = Img()
    data = img.data

    # ---- CRC tables: are they standard? -------------------------------
    std = standard_crc32_table()
    print("=== CRC32 table check ===")
    crc_tables = []
    for fo in CRC_TABLE_FILE_OFFS:
        rva = img.off2rva(fo)
        vals = list(struct.unpack_from("<256I", data, fo))
        same = vals == std
        # also try to recover the poly by brute force over common polys
        poly = None
        if not same:
            for p in range(1, 1 << 32, 1):
                pass
        crc_tables.append({"file_off": hex(fo), "rva": hex(rva),
                           "identical_to_standard": same,
                           "first8": [hex(v) for v in vals[:8]],
                           "last4": [hex(v) for v in vals[-4:]]})
        print(f"  table @file {fo:#x} rva {rva:#x}: standard={same}")
        if not same:
            # find where it diverges
            for i, (a, b) in enumerate(zip(vals, std)):
                if a != b:
                    print(f"     first divergence at index {i}: got {a:#x} want {b:#x}")
                    break

    # ---- RIP-relative scan: targets inside typeinfo/fieldinf -----------
    ts = img.sec["typeinfo"]
    fs = img.sec["fieldinf"]
    ranges = [("typeinfo", ts["va"], ts["va"] + ts["vsize"]),
              ("fieldinf", fs["va"], fs["va"] + fs["vsize"])]
    # plus crc table addresses
    exact = {}
    for t in crc_tables:
        exact[int(t["rva"], 16)] = "crc_table_" + t["file_off"]
    # plus typeinfo/fieldinf base
    exact[ts["va"]] = "typeinfo_base"
    exact[fs["va"]] = "fieldinf_base"
    exact_vals = np.array(sorted(exact.keys()), dtype=np.int64)

    hits = []
    CHUNK = 1 << 22
    for sname in (".text", "ctr"):
        s = img.sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        n = len(blob) // 4
        for off in range(0, n, CHUNK):
            m = min(CHUNK, n - off)
            arr = np.frombuffer(blob, dtype="<u4", count=m, offset=off * 4).astype(np.int64)
            idx = np.arange(off, off + m, dtype=np.int64)
            disp = np.where(arr >= (1 << 31), arr - (1 << 32), arr)
            tgt = s["va"] + idx * 4 + 4 + disp
            for nm, lo, hi in ranges:
                mask = (tgt >= lo) & (tgt < hi)
                for pos in np.nonzero(mask)[0]:
                    hits.append({"sec": sname, "code_rva": s["va"] + (off + int(pos)) * 4,
                                 "target_rva": int(tgt[pos]), "kind": nm})
            mask = np.isin(tgt, exact_vals)
            for pos in np.nonzero(mask)[0]:
                t = int(tgt[pos])
                hits.append({"sec": sname, "code_rva": s["va"] + (off + int(pos)) * 4,
                             "target_rva": t, "kind": exact[t]})

    print(f"\n=== {len(hits)} RIP-relative hits into typeinfo/fieldinf/crc ===")
    from collections import Counter
    print(Counter(h["kind"] for h in hits).most_common())
    for h in hits[:80]:
        print(f"  {h['sec']} code@{h['code_rva']:#x} -> {h['target_rva']:#x} ({h['kind']})")
        for ln in dump_bytes(data, h["code_rva"] - 0x20, 0x40):
            print(ln)

    json.dump({"crc_tables": crc_tables, "hits": hits},
              open(OUTDIR / "p2_code_refs.json", "w"), indent=1)


if __name__ == "__main__":
    main()
