import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
z = np.load(HERE / "p2_riprefs.npz", allow_pickle=True)
code, tgt, form, ilen = z["code"], z["tgt"], z["form"], z["ilen"]

for c in (0x7ba95c0, 0x7ba95c1, 0x7ba95e0, 0x7ba95e1):
    idx = np.nonzero(code == c)[0]
    for i in idx:
        t = int(tgt[i])
        sec = img.sec_of(t) if t < 0x100000000 else "?"
        s = ""
        if sec in (".rdata", ".data"):
            try:
                s = img.cstr_at_rva(t)[:40]
            except Exception:
                s = "?"
        print(f"code=0x{c:x} form={form[i]} ilen={ilen[i]} tgt=0x{t:x} sec={sec} '{s}'")
    if idx.size == 0:
        print(f"code=0x{c:x}: NO ENTRY")

# how many entries have tgt in .rdata but code pointing at a 'lea' whose manual target differs?
print("\nspot check: manual target for code 0x7ba95c0")
o = img.rva2off(0x7ba95c0)
b = img.data[o:o + 8]
disp = int.from_bytes(b[3:7], "little")
print("  bytes", b.hex(), "disp", hex(disp), "manual target", hex(0x7ba95c0 + 7 + disp))
