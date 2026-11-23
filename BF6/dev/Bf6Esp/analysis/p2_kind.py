#!/usr/bin/env python3
"""p2_kind: for every function that contains the typeinfo 'kind' word 0x00080089,
list the .rdata strings it references (validated RIP-relative forms) and the
typeinfo/fieldinf addresses it writes.  This should expose name <-> record pairs.
"""
import json
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402
from p2_find import build_pdata, func_of  # noqa: E402


def main():
    img = Img()
    data = img.data
    beg, end = build_pdata(img)
    z = np.load(OUTDIR / "p2_riprefs.npz")
    code, tgt = z["code"], z["tgt"]
    rd = img.sec[".rdata"]
    rd_lo, rd_hi = rd["va"], rd["va"] + rd["vsize"]
    ti = img.sec["typeinfo"]
    fi = img.sec["fieldinf"]

    # kind-word sites
    pat = b"\x89\x00\x08\x00"
    sites = []
    for secname in (".text", "ctr"):
        s = img.sec[secname]
        b = data[s["roff"]:s["roff"] + s["rsize"]]
        st = 0
        while True:
            p = b.find(pat, st)
            if p < 0:
                break
            st = p + 1
            sites.append(s["va"] + p)

    out = []
    for site in sites:
        f = func_of(beg, end, site)
        rec = {"site_rva": hex(site), "func": [hex(f[0]), hex(f[1])] if f else None,
               "strings": [], "section_writes": []}
        if f:
            fs, fe = f
            m = (code >= fs) & (code < fe)
            for i in np.nonzero(m)[0]:
                t = int(tgt[i])
                if rd_lo <= t < rd_hi:
                    off = img.rva2off(t)
                    raw = data[off:off + 96]
                    s = raw.split(b"\x00")[0]
                    if s and all(32 <= c < 127 for c in s):
                        rec["strings"].append({"code_rva": hex(int(code[i])),
                                               "str_rva": hex(t),
                                               "str": s.decode("latin1")})
                elif ti["va"] <= t < ti["va"] + ti["vsize"]:
                    rec["section_writes"].append({"code_rva": hex(int(code[i])),
                                                  "sec": "typeinfo",
                                                  "off": hex(t - ti["va"])})
                elif fi["va"] <= t < fi["va"] + fi["vsize"]:
                    rec["section_writes"].append({"code_rva": hex(int(code[i])),
                                                  "sec": "fieldinf",
                                                  "off": hex(t - fi["va"])})
        out.append(rec)
        print(f"\nsite {rec['site_rva']} func {rec['func']}")
        for s in rec["strings"][:40]:
            print(f"    str  {s['code_rva']} -> 0x{s['str_rva'][2:].upper()} {s['str']!r}")
        for w in rec["section_writes"][:20]:
            print(f"    wr   {w['code_rva']} -> {w['sec']}+{w['off']}")

    json.dump(out, open(OUTDIR / "p2_kind.json", "w"), indent=1)
    print("\nwritten p2_kind.json")


if __name__ == "__main__":
    main()
