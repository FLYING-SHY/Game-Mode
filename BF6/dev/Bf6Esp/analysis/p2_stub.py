"""Decode a registration stub into a store map, and follow the stored pointers."""
import json
import struct
import sys
from collections import defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img, IMAGE_BASE  # noqa: E402
from p2_code2 import FORMS, RM  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
z = np.load(HERE / "p2_riprefs.npz")
CODE, TGT, FRM = z["code"], z["tgt"], z["form"]
pdata = img.blob(".pdata")
nfun = len(pdata) // 12
beg = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[0::3].astype(np.int64)
end = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[1::3].astype(np.int64)


def func_of(rva):
    i = np.searchsorted(beg, rva, side="right") - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


def str_at_rva(rva, limit=80):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    o = s["roff"] + (rva - s["va"])
    e = DATA.find(b"\0", o, o + limit)
    if e > o:
        b = DATA[o:e]
        if len(b) > 1 and all(32 <= c < 127 for c in b):
            return b.decode("ascii")
    return None


def decode(fn_rva):
    b, e = func_of(fn_rva)
    raw = DATA[img.rva2off(b):img.rva2off(b) + (e - b)]
    out = []
    p = 0
    while p < len(raw):
        hit = None
        for fi, (pref, doff, ilen, rmchk) in enumerate(FORMS):
            if raw[p:p + len(pref)] != pref:
                continue
            if p + ilen > len(raw):
                continue
            if rmchk and raw[p + len(pref)] not in RM:
                continue
            if not rmchk and pref in (b"\xff\x15", b"\xff\x25"):
                if raw[p + 2] != (0x15 if pref == b"\xff\x15" else 0x25):
                    continue
            disp = int.from_bytes(raw[p + doff:p + doff + 4], "little", signed=True)
            hit = (fi, ilen, b + p + ilen + disp, raw[p:p + ilen].hex())
            break
        if hit:
            fi, ilen, t, hx = hit
            out.append((b + p, ilen, fi, t, hx))
            p += ilen
        else:
            p += 1
    return b, e, out


STORE_FI = {6, 7, 8, 22, 23}
LEA_FI = {0, 1, 2}


def main():
    fn = int(sys.argv[1], 16)
    b, e, ins = decode(fn)
    print(f"== stub {hex(b)}..{hex(e)} ==")
    stores = []
    for addr, ilen, fi, t, hx in ins:
        sec = img.sec_of(t)
        st = str_at_rva(t) if sec == ".rdata" else None
        kind = {0: "lea", 1: "lea", 2: "lea", 6: "mov-load", 7: "mov-load", 8: "mov-store",
                22: "movq-store-imm", 23: "movq-store"}.get(fi, f"f{fi}")
        tag = ""
        if fi in STORE_FI:
            stores.append((t, kind, hx))
            tag = "  <== STORE"
        extra = ""
        if fi == 22:
            imm = int.from_bytes(bytes.fromhex(hx)[-4:], "little", signed=True)
            extra = f" imm={imm}"
        if fi in LEA_FI:
            extra = f" -> {st or ''}"
        print(f"  {addr:08x} {hx:<20} {kind:<16} tgt={t:08x} {sec}{extra}{tag}")

    print("\n-- store targets (sorted) --")
    for t, kind, hx in sorted(stores):
        sec = img.sec_of(t)
        st = str_at_rva(t) if sec == ".rdata" else None
        print(f"   {t:08x} {sec:<10} {st or ''}")

    # follow stored .data/.rdata pointers
    print("\n-- follow stored pointers --")
    for t, kind, hx in sorted(set(stores)):
        sec = img.sec_of(t)
        if sec not in (".data", ".rdata"):
            continue
        o = img.rva2off(t)
        print(f"   ptr @ {t:08x} ({sec}) contents: {DATA[o:o+64].hex()}")
        for k in range(0, 64, 8):
            v = int.from_bytes(DATA[o + k:o + k + 8], "little")
            s2 = img.sec_of(v) if 0 < v < 0x7FFFFFFF else None
            st2 = str_at_rva(v) if s2 == ".rdata" else None
            print(f"      +{k:02x}: {v:016x} {s2 or ''} {st2 or ''}")


main()
