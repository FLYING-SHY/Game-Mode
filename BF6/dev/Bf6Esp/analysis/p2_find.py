#!/usr/bin/env python3
"""p2_find: locate the runtime reflection registration / hash code.

1. Parse .pdata (RUNTIME_FUNCTION) -> RVA -> (func_start, func_end) map.
2. Search .text + ctr for magic immediates:
     89 00 08 00        (the typeinfo 'kind' word 0x00080089)
     20 83 B8 ED        (CRC32 poly 0xEDB88320)
     C5 9D 1C 81        (FNV-1a 32 offset basis)
     93 01 00 01        (FNV-1a 32 prime)
     C5 29 F4 84 42 ... (FNV-1a 64 basis)
3. Take the CRC-table code references found by p2_code2.py, map them to their
   enclosing function and dump those functions.
"""
import json
import struct
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402


def build_pdata(img):
    s = img.sec[".pdata"]
    b = img.data[s["roff"]:s["roff"] + s["rsize"]]
    n = len(b) // 12
    arr = np.frombuffer(b[:n * 12], dtype="<u4").reshape(n, 3)
    beg = arr[:, 0].astype(np.int64)
    end = arr[:, 1].astype(np.int64)
    keep = (beg > 0) & (end > beg)
    return beg[keep], end[keep]


def func_of(beg, end, rva):
    i = int(np.searchsorted(beg, rva, side="right")) - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


def main():
    img = Img()
    data = img.data
    beg, end = build_pdata(img)
    print(f"pdata functions: {len(beg)}")

    z = np.load(OUTDIR / "p2_riprefs.npz")
    code, tgt = z["code"], z["tgt"]
    crc_lo = [0x14855D550, 0x1485D55B0, 0x148A2AE30, 0x148D2D630, 0x1492BFC40]
    print("\n=== CRC table references (validated opcode forms) ===")
    crc_hits = []
    for lo in crc_lo:
        m = (tgt >= lo) & (tgt < lo + 1024)
        for i in np.nonzero(m)[0]:
            cr = int(code[i])
            f = func_of(beg, end, cr)
            off = img.rva2off(cr)
            crc_hits.append({"table_rva": hex(lo), "code_rva": hex(cr),
                             "func": [hex(f[0]), hex(f[1])] if f else None,
                             "bytes": data[off:off + 24].hex()})
            print(f"  table=0x{lo:X} code=0x{cr:X} func={f and (hex(f[0]),hex(f[1]))} "
                  f"bytes={data[off:off+16].hex()}")

    # ---- magic immediate searches -------------------------------------
    print("\n=== magic immediate search in .text + ctr ===")
    pats = {
        "kind_00080089": b"\x89\x00\x08\x00",
        "crc_poly_edb88320": b"\x20\x83\xb8\xed",
        "fnv32_basis": b"\xc5\x9d\x1c\x81",
        "fnv32_prime": b"\x93\x01\x00\x01",
        "fnv64_basis": b"\xc5\x29\xf4\x84\x42\x24\x9e\xfb",
        "fnv64_prime": b"\xb3\x71\x64\xe9\x6b\xd5\x02\x01",
        "crc32c_poly": b"\x78\xb8\x3f\x82",
        "crc32_koop_poly": b"\xb7\x1d\xc1\x04",
    }
    magic = {}
    for nm, pat in pats.items():
        hits = []
        for secname in (".text", "ctr"):
            s = img.sec[secname]
            b = data[s["roff"]:s["roff"] + s["rsize"]]
            start = 0
            while True:
                p = b.find(pat, start)
                if p < 0:
                    break
                start = p + 1
                rva = s["va"] + p
                f = func_of(beg, end, rva)
                hits.append({"rva": hex(rva), "sec": secname,
                             "func": [hex(f[0]), hex(f[1])] if f else None})
        magic[nm] = hits
        print(f"  {nm}: {len(hits)} hits")
        for h in hits[:12]:
            print("      ", h)

    # ---- dump functions that touch CRC tables -------------------------
    print("\n=== functions containing CRC-table references ===")
    funcs = {}
    for h in crc_hits:
        if h["func"]:
            funcs.setdefault(tuple(h["func"]), []).append(h)
    dumps = []
    for (fs, fe), hs in funcs.items():
        off = img.rva2off(int(fs, 16))
        blob = data[off:off + (int(fe, 16) - int(fs, 16))]
        print(f"\n--- func 0x{fs} .. 0x{fe} (len {len(blob)}) refs={[(x['table_rva'], x['code_rva']) for x in hs]}")
        # print hex in 16-byte rows, first 0x300 bytes
        for i in range(0, min(len(blob), 0x300), 16):
            row = blob[i:i + 16]
            print(f"   {int(fs,16)+i:012X}  {row.hex(' ')}")
        dumps.append({"start": fs, "end": fe, "len": len(blob),
                      "refs": hs, "hex": blob[:0x600].hex()})

    json.dump({"crc_refs": crc_hits, "magic_immediates": magic,
               "crc_funcs": dumps}, open(OUTDIR / "p2_find.json", "w"), indent=1)
    print("\nwritten p2_find.json")


if __name__ == "__main__":
    main()
