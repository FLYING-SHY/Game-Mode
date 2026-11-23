"""Definitive layout: static typeinfo record bytes vs runtime stub slot map."""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402
from p2_rec import decode_stub  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000
TI = SEC["typeinfo"]
ti = DATA[TI["roff"]:TI["roff"] + TI["rsize"]]

pairs = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
seen = {}
for p in pairs:
    if p["type"] not in seen:
        seen[p["type"]] = p
anch = list(seen.values())


def sec_of_va(va):
    r = va - IB
    for nm, s in SEC.items():
        if s["va"] <= r < s["va"] + max(s["vsize"], s["rsize"]):
            return nm, r
    return None, r


def cstr_va(va):
    r = va - IB
    o = img.rva2off(r)
    if o is None:
        return None
    e = DATA.find(b"\x00", o, o + 200)
    if e < 0:
        return None
    s = DATA[o:e]
    try:
        t = s.decode("ascii")
    except Exception:
        return None
    return t if t.isprintable() else None


def dump_rec(off):
    b = ti[off:off + 0x50]
    u32 = np.frombuffer(b, dtype="<u4")
    u64 = np.frombuffer(b, dtype="<u8")
    print(f"  raw: {b.hex()}")
    print("  u32:", [f"{x:08x}" for x in u32])
    for i in range(10):
        v = int(u64[i])
        nm, r = sec_of_va(v)
        extra = ""
        if nm in (".rdata", ".data"):
            s = cstr_va(v)
            if s:
                extra = f' -> "{s[:50]}"'
        print(f"   q{i} @+0x{i*8:02x} = {v:016x}  sec={nm} rva={r:x}{extra}")


# group anchors by type so we can also find the stub
reg = json.loads((HERE / "p2_reg.json").read_text(encoding="utf-8"))
stubs = reg["type_stubs"]
by_type = {}
for st in stubs:
    uniq = list(dict.fromkeys(st["strings"]))
    if len(uniq) >= 2:
        by_type.setdefault(uniq[1], []).append(st)

for want in ["AftermathProcess", "ServerActivityEntryComponentInterface",
             "ClientAICommanderOrderComponent", "AftermathContext"]:
    a = seen.get(want)
    if not a:
        print(f"\n=== {want}: NOT IN ANCHORS ===")
        continue
    print(f"\n=== {want} (module {a['module']}) static record @typeinfo+0x{a['rec_off']:x} "
          f"(RVA 0x{TI['va'] + a['rec_off']:x}) ===")
    dump_rec(a["rec_off"])
    for st in by_type.get(want, [])[:2]:
        r = decode_stub(int(st["func"], 16))
        if not r:
            continue
        b, e, slots = r
        print(f"  -- stub {st['func']} runtime record base 0x{b:x} slots:")
        for k in sorted(slots):
            v = slots[k]
            if isinstance(v, tuple):
                continue
            nm, rr = sec_of_va(v) if v > 0xFFFF else ("imm", v)
            extra = ""
            if nm in (".rdata", ".data"):
                s = cstr_va(v)
                if s:
                    extra = f' -> "{s[:40]}"'
            print(f"     +0x{k - b:02x} = {v:016x} sec={nm}{extra}")
