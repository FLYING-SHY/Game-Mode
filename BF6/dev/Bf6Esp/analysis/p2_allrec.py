"""Decode ALL registration stubs into runtime type-record maps and summarize."""
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402
from p2_rec import decode_stub, str_at_rva  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
TI_LO = SEC["typeinfo"]["va"]
TI_HI = TI_LO + SEC["typeinfo"]["vsize"]


def main():
    reg = json.loads((HERE / "p2_reg.json").read_text(encoding="utf-8"))
    stubs = [int(s["func"], 16) for s in reg["type_stubs"]]
    print("stubs:", len(stubs))
    shapes = Counter()
    recs = []
    rdata_slot_vals = Counter()
    per_type = []
    for fn in stubs:
        r = decode_stub(fn)
        if not r:
            continue
        b, e, slots = r
        if not slots:
            continue
        base = min(slots)
        items = []
        for s in sorted(slots):
            v = slots[s]
            items.append((s - base, v))
        shape = tuple(d for d, v in items)
        shapes[shape] += 1
        # gather values
        vals = {}
        for d, v in items:
            if isinstance(v, int):
                vals[d] = v
        per_type.append({"fn": b, "base": base, "vals": {str(k): v for k, v in vals.items()}})
        for d, v in vals.items():
            if SEC[".rdata"]["va"] <= v < SEC[".rdata"]["va"] + SEC[".rdata"]["vsize"]:
                rdata_slot_vals[(d, v)] += 1
    print("distinct record shapes:", len(shapes))
    for sh, c in shapes.most_common(6):
        print(f"   n={c}  offsets={sh}")
    print()
    print("most common (.rdata value at slot):")
    for (d, v), c in rdata_slot_vals.most_common(12):
        print(f"   +0x{d:x} -> {v:08x} ({str_at_rva(v)})  x{c}")

    # how many distinct .rdata values across all slots?
    allrd = set()
    for pt in per_type:
        for d, v in pt["vals"].items():
            if SEC[".rdata"]["va"] <= v < SEC[".rdata"]["va"] + SEC[".rdata"]["vsize"]:
                allrd.add(v)
    print("\ndistinct .rdata values stored in type records:", len(allrd))
    # how many distinct .text values
    alltx = set()
    for pt in per_type:
        for d, v in pt["vals"].items():
            if SEC[".text"]["va"] <= v < SEC[".text"]["va"] + SEC[".text"]["vsize"]:
                alltx.add(v)
    print("distinct .text values stored in type records:", len(alltx))
    # .data values
    alldd = set()
    for pt in per_type:
        for d, v in pt["vals"].items():
            if SEC[".data"]["va"] <= v < SEC[".data"]["va"] + SEC[".data"]["vsize"]:
                alldd.add(v)
    print("distinct .data values stored in type records:", len(alldd))

    (HERE / "p2_allrec.json").write_text(json.dumps(per_type[:20000]), encoding="utf-8")
    print("wrote p2_allrec.json")


main()
