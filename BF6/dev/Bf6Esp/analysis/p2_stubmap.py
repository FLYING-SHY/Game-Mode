"""Extract per-type record contents from registration stubs."""
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402
from p2_code2 import FORMS, RM  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
pdata = img.blob(".pdata")
nfun = len(pdata) // 12
beg = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[0::3].astype(np.int64)
end = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[1::3].astype(np.int64)


def func_of(rva):
    i = np.searchsorted(beg, rva, side="right") - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


def str_at_rva(rva, limit=60):
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


# fix ilen for 0f-forms (7 bytes not 8)
FIX = {b"\x0f\xb6": 7, b"\x0f\xb7": 7, b"\x0f\x6f": 7, b"\x0f\x7e": 7,
       b"\x0f\x10": 7, b"\x0f\x11": 7, b"\xf2\x0f\x10": 8, b"\xf2\x0f\x11": 8,
       b"\x48\x0f\xb6": 8, b"\x48\x0f\xb7": 8}


def decode(fn_rva):
    b, e = func_of(fn_rva)
    if b is None:
        return None
    raw = DATA[img.rva2off(b):img.rva2off(b) + (e - b)]
    regs = {}
    out = []
    p = 0
    while p < len(raw):
        hit = None
        for fi, (pref, doff, ilen, rmchk) in enumerate(FORMS):
            if raw[p:p + len(pref)] != pref:
                continue
            il = FIX.get(pref, ilen)
            if p + il > len(raw):
                continue
            if rmchk and raw[p + len(pref)] not in RM:
                continue
            disp = int.from_bytes(raw[p + doff:p + doff + 4], "little", signed=True)
            hit = (fi, il, b + p + il + disp, raw[p:p + il].hex())
            break
        if hit:
            fi, il, t, hx = hit
            out.append((b + p, il, fi, t, hx))
            p += il
        else:
            p += 1
    return b, e, out


def stub_records(fn_rva):
    """Return dict: typeinfo_slot -> value source."""
    r = decode(fn_rva)
    if not r:
        return None
    b, e, ins = r
    rec = {}
    for addr, il, fi, t, hx in ins:
        if fi in (6, 7, 8):          # mov [rip+disp], reg
            rec[t] = ("reg", hx)
        elif fi == 22:              # mov qword [rip+disp], imm32
            rec[t] = ("imm", int.from_bytes(bytes.fromhex(hx)[-4:], "little", signed=True))
        elif fi in (16, 17):        # movups [rip+disp], xmm
            rec[t] = ("xmm", hx)
    return b, e, rec, ins


def main():
    stubs = [int(x, 16) for x in sys.argv[1:]] or [0x68140, 0x68020, 0x683f0, 0x68760, 0x68a20]
    for fn in stubs:
        r = stub_records(fn)
        if not r:
            print(f"{fn:08x}: no pdata function")
            continue
        b, e, rec, ins = r
        print(f"\n=== stub {b:08x}..{e:08x} ===")
        names = {}
        for addr, il, fi, t, hx in ins:
            if fi in (0, 1, 2):     # lea
                st = str_at_rva(t)
                if st:
                    names[t] = st
        for t in sorted(rec):
            kind, val = rec[t]
            print(f"   slot {t:08x} ({img.sec_of(t)}) <- {kind}")
        print("   name-ish LEAs:", names)


main()
