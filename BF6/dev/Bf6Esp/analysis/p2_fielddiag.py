"""Diagnose where the fieldinf-referenced type cells point inside the typeinfo section."""
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
IB = 0x140000000
TI = SEC["typeinfo"]
DA = SEC[".data"]

raw = json.loads((HERE / "p2_fieldmap_raw.json").read_text(encoding="utf-8"))
cells = raw["cells"]
print(f"cells: {len(cells)}")

mod50 = Counter()
kinds = Counter()
flagstat = Counter()
sizes = Counter()
rec_off_set = set()
for c in cells:
    rec_off_set.add(c["rec_off"])
    mod50[c["rec_off"] % 0x50] += 1
    rva = TI["va"] + c["rec_off"]
    o = img.rva2off(rva)
    w = DATA[o:o + 16]
    kinds[hex(int.from_bytes(w[4:8], "little"))] += 1
    flagstat[c["flags"]] += 1
    sizes[c["size"]] += 1

print("\nrec_off % 0x50 histogram (top 12):", mod50.most_common(12))
print("\nu32 at q0+4 (i.e. 'kind' word) histogram (top 12):", kinds.most_common(12))
print("\ncell flags histogram:", flagstat.most_common(8))
print("cell size histogram:", sizes.most_common(12))

# do these rec_off collide with the anchor rec_offs?
anchors = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
aoff = {a["rec_off"] for a in anchors}
print(f"\nanchor rec_off count {len(aoff)}, overlap with fieldinf-cell rec_offs: {len(aoff & rec_off_set)}")

# is q0 == marker-4 style (kind marker at +4)?
mark = 0x00080089
hit4 = 0
for c in cells:
    rva = TI["va"] + c["rec_off"]
    o = img.rva2off(rva)
    if int.from_bytes(DATA[o + 4:o + 8], "little") == mark:
        hit4 += 1
print(f"cells whose q0+4 == 0x00080089 marker: {hit4}/{len(cells)}")

# look at raw bytes of the first few cells' targets
for c in cells[:5]:
    rva = TI["va"] + c["rec_off"]
    o = img.rva2off(rva)
    print(f"  rec_off {c['rec_off']} rva 0x{rva:x}: " + DATA[o:o + 32].hex())
