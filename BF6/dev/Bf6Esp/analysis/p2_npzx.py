import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
z = np.load(HERE / "p2_riprefs.npz", allow_pickle=True)
code, tgt, form, ilen = z["code"], z["tgt"], z["form"], z["ilen"]

for target in (0x8dd5e1f, 0x8dc6dec, 0x90a3ac8, 0x8dceef8):
    idx = np.nonzero(tgt == target)[0]
    print(f"tgt 0x{target:x}: {idx.size} entries")
    for i in idx:
        print(f"    code=0x{code[i]:x} form={form[i]} ilen={ilen[i]}")
