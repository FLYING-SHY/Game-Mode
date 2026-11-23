"""Explore the fieldinf stream around .data type-cell pointers."""
import json
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
FI = SEC["fieldinf"]
DATA_S = SEC[".data"]
TI = SEC["typeinfo"]
IB = 0x140000000

fi = DATA[FI["roff"]:FI["roff"] + FI["rsize"]]
u32 = np.frombuffer(fi, dtype="<u4")

recs = json.loads((HERE / "p2_allrec.json").read_text(encoding="utf-8"))
reg = json.loads((HERE / "p2_reg.json").read_text(encoding="utf-8"))
stub_strings = {int(s["func"], 16): s["strings"] for s in reg["type_stubs"]}


def is_cell(rva):
    """rva -> .data pointer to a 0x20 type cell whose q0 is in the typeinfo section"""
    if not (DATA_S["va"] <= rva < DATA_S["va"] + DATA_S["vsize"]):
        return None
    o = img.rva2off(rva)
    if o is None:
        return None
    q0 = int.from_bytes(DATA[o:o + 8], "little")
    q0r = norm(q0)
    if not (TI["va"] <= q0r < TI["va"] + TI["rsize"]):
        return None
    flags = int.from_bytes(DATA[o + 8:o + 10], "little")
    size = int.from_bytes(DATA[o + 10:o + 12], "little")
    return {"cell_rva": rva, "q0": q0, "q0_rva": q0r, "flags": flags, "size": size}


def norm(v):
    return v - IB if v >= IB else v


# build cell -> (type name, module, node base) from the decoded stubs
cell2type = {}
for r in recs:
    fn = r["fn"]
    base = r["base"]
    vals = {int(k): v for k, v in r["vals"].items()}
    datavals = [(off, norm(v)) for off, v in sorted(vals.items())
                if DATA_S["va"] <= norm(v) < DATA_S["va"] + DATA_S["vsize"]]
    mod = None
    cands = []
    for off, v in datavals:
        c = is_cell(v)
        if c:
            cands.append((off, c))
        o = img.rva2off(v)
        if o is not None:
            q0 = int.from_bytes(DATA[o:o + 8], "little")
            if SEC[".rdata"]["va"] <= q0 < SEC[".rdata"]["va"] + SEC[".rdata"]["rsize"]:
                s = img.cstr_at_rva(q0 - IB)
                if s:
                    mod = s
    strings = stub_strings.get(fn, [])
    types = [x for x in strings if x != mod] or strings
    for off, c in cands:
        cell2type.setdefault(c["cell_rva"], []).append(
            {"type": types[0] if types else None, "module": mod, "node_base": base,
             "slot": off, "q0": c["q0"], "flags": c["flags"], "size": c["size"], "fn": fn})

print(f"cells referenced by stubs: {len(cell2type)}")
print("sample:", json.dumps(list(cell2type.values())[0], indent=1))

# which cells appear in the fieldinf payload stream?
cell_lo = {va & 0xFFFFFFFF for va in cell2type}
hits = np.nonzero(np.isin(u32, list(cell_lo)))[0]
print(f"\nfieldinf u32 matching a stub-referenced cell low32: {len(hits)} at "
      f"{len(set(u32[hits].tolist()))} distinct values")

# dump the neighbourhood of the first few hits
print("\n--- fieldinf neighbourhood ---")
for h in hits[:12]:
    lo = max(0, int(h) - 4)
    vals = [hex(int(x)) for x in u32[lo:lo + 24]]
    print(f"  idx {int(h):7d} (off 0x{int(h)*4:x}): " + " ".join(vals))

# how many u32 in the whole section equal any cell low?
allcell = Counter()
for va in cell2type:
    allcell[va & 0xFFFFFFFF] += 1
print(f"\ndistinct cell lows known from stubs: {len(allcell)}")
in_stream = int(np.isin(u32, list(allcell)).sum())
print(f"fieldinf u32 matching those lows: {in_stream}")
