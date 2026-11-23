"""Dump the interesting non-registration-stub functions that store into the typeinfo section."""
import json
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
data = img.data
ks = json.loads((HERE / "p2_key_stores.json").read_text(encoding="utf-8"))
cands = ks["candidates"]

# ---- the 16-byte (movups) writers -----------------------------------------
movups = [c for c in cands if any(s["instr"].startswith("movups") for s in c["stores"])]
print(f"candidate functions containing a 16-byte (movups) store into typeinfo: {len(movups)}")
# cluster their func_rva
fr = sorted(int(c["func_rva"], 16) for c in movups if c["func_rva"] != "-0x1")
print(f"  with pdata entry: {len(fr)}  range {hex(min(fr))}..{hex(max(fr))}")

# how many distinct target addresses do they write to, and what is stored on disk?
tg = []
for c in movups:
    for s in c["stores"]:
        tg.append((int(s["code_rva"], 16), int(s["target_rva"], 16)))
tg.sort()
print(f"  total movups stores: {len(tg)}")

# dump the first few functions fully
def dump_func(rva, n=160):
    off = img.rva2off(rva)
    return data[off:off + n].hex(" ")


print("\n=== first 4 movups writers ===")
for c in movups[:4]:
    print(f"\nfunc {c['func_rva']} .. {c['func_end']}  n_stores={c['n_stores_into_typeinfo']}")
    for s in c["stores"][:4]:
        print(f"   {s['instr']}  -> {s['target_rva']}")
        print(f"     target bytes on disk: {s['target_bytes']}")
    frv = int(c["func_rva"], 16)
    print("   code:", dump_func(frv, 120))

# the 474-store blob without a pdata entry
big = max(cands, key=lambda c: c["n_stores_into_typeinfo"])
print(f"\n=== largest candidate (no pdata) {big['func_rva']} stores={big['n_stores_into_typeinfo']} ===")
codes = [int(s["code_rva"], 16) for s in big["stores"]]
print(f"  code rva range: {hex(min(codes))} .. {hex(max(codes))}")
tgt = sorted({int(s["target_rva"], 16) for s in big["stores"]})
print(f"  distinct targets: {len(tgt)}  first={hex(tgt[0])} last={hex(tgt[-1])}")
off = img.rva2off(min(codes))
print("  code head:", data[off:off + 96].hex(" "))
print("  target sample bytes:", data[img.rva2off(tgt[0]):img.rva2off(tgt[0]) + 32].hex(" "))

# what kind of values live at the targets of the movups writers?
print("\n=== on-disk content at movups writer targets ===")
cnt = Counter()
for c in movups:
    for s in c["stores"]:
        b = bytes.fromhex(s["target_bytes"].replace(" ", ""))
        cnt[b[:8].hex()] += 1
for k, v in cnt.most_common(10):
    print(f"  {k}  x{v}")

# do any of those targets sit on a static 0x50 record base, and is the +4 marker there?
ti = img.sec["typeinfo"]
blob = data[ti["roff"]:ti["roff"] + ti["rsize"]]
arr = np.frombuffer(blob, dtype="<u4")
marks = set((np.nonzero(arr == 0x00080089)[0] - 1).tolist())
hit = 0
for c in movups:
    for s in c["stores"]:
        o = int(s["target_rva"], 16) - ti["va"]
        if o in marks:
            hit += 1
print(f"  movups stores landing exactly on a static record base: {hit}")
