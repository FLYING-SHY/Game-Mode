#!/usr/bin/env python3
"""p2_writers: which code writes the .data name-pointer tables?  (hash computation hunt)"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402

z = np.load(OUTDIR / "p2_riprefs.npz")
code = z["code"]
tgt = z["tgt"]
form = z["form"]
ilen = z["ilen"]
print("refs:", len(code))

TARGETS = {
    "ui_props_A": (0x9AD5110, 0x9AD5110 + 0x20 * 389),
    "ui_props_B": (0x9AD0EC0, 0x9AD0EC0 + 0x20 * 354),
    "console_vars": (0xA05C2B0, 0xA05C2B0 + 0x20 * 505),
    "lobby_names": (0x9ACEF40, 0x9ACEF40 + 0x20 * 30),
}
for nm, (lo, hi) in TARGETS.items():
    m = (tgt >= lo) & (tgt < hi)
    idx = np.nonzero(m)[0]
    print(f"\n{nm} range {hex(lo)}-{hex(hi)}: {len(idx)} refs")
    seen = []
    for i in idx[:40]:
        print("   code rva", hex(int(code[i])), "form", str(form[i]), "tgt", hex(int(tgt[i])),
              "delta", hex(int(tgt[i]) - lo))
