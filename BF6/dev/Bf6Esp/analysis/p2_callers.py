"""Find callers of the ctr thunks that return the fieldinf / typeinfo section base."""
import json
import struct
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402
from p2_find import build_pdata, func_of  # noqa: E402

img = Img()
data = img.data
SEC = img.sec
IB = 0x140000000

beg, end = build_pdata(img)

# the rip-ref code sites that point at the fieldinf / typeinfo section head
z = np.load(HERE / "p2_riprefs.npz")
code, tgt = z["code"], z["tgt"]
heads = {}
for name, lo in (("fieldinf", 0xB48B000), ("typeinfo", 0xAECC000)):
    m = (tgt >= lo) & (tgt < lo + 0x40)
    sites = sorted(set(int(code[i]) for i in np.nonzero(m)[0]))
    heads[name] = sites
    print(f"{name} head refs (code sites): {[hex(s) for s in sites]}")

# gather candidate thunk addresses: any 9-byte aligned site in ctr whose bytes are
# 48 8d 05 <disp32> c3  (lea rax,[rip+X]; ret)
thunks = {}
for name, sites in heads.items():
    th = []
    for s in sites:
        for cand in range(s - 16, s + 16):
            o = img.rva2off(cand)
            if o is None:
                continue
            b = data[o:o + 7]
            if len(b) == 7 and b[0] == 0x48 and b[1] == 0x8D and b[2] == 0x05 and b[6] == 0xC3:
                disp = struct.unpack_from("<i", b, 3)[0]
                th.append((cand, cand + 7, (cand + 7 + disp) & 0xFFFFFFFF))
    thunks[name] = th
    print(f"{name} thunks: {[(hex(a), hex(b), hex(t)) for a, b, t in th]}")

# scan .text + ctr for E8 rel32 calls into those thunk ranges
textsec = [".text", "ctr"]
calls = []
for secname in textsec:
    s = SEC[secname]
    b = data[s["roff"]:s["roff"] + s["rsize"]]
    idx = np.nonzero(b == 0xE8)[0]
    for i in idx:
        rel = struct.unpack_from("<i", b, int(i) + 1)[0]
        src = s["va"] + int(i)
        dst = (src + 5 + rel) & 0xFFFFFFFF
        for name, th in thunks.items():
            for a, e, t in th:
                if a <= dst < e:
                    calls.append((name, src, dst, t))
                    break
print(f"\ncalls into section-head thunks: {len(calls)}")
cnt = Counter()
for name, src, dst, t in calls:
    f = func_of(beg, end, src)
    cnt[(name, f)] += 1
for k, v in cnt.most_common(40):
    print(f"   {k[0]:10s} func={k[1]} x{v}")

# dump the biggest caller functions
out = []
for (name, f), v in cnt.most_common(12):
    if not f:
        continue
    fs, fe = f
    o = img.rva2off(fs)
    blob = data[o:o + (fe - fs)]
    out.append({"name": name, "func": [fs, fe], "calls": v, "hex": blob.hex()})
    print(f"\n--- {name} func 0x{fs:x}..0x{fe:x} ({len(blob)} bytes) calls={v}")
    for i in range(0, min(len(blob), 0x200), 16):
        print(f"   {fs + i:012X}  {blob[i:i + 16].hex(' ')}")
(HERE / "p2_callers.json").write_text(json.dumps({"calls": calls, "dumps": out}, indent=1),
                                      encoding="utf-8")
print("\nwrote p2_callers.json")
