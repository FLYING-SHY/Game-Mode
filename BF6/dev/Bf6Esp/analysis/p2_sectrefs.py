"""Find the code that touches the reflection blobs: RIFXRIFF module table,
fieldinf section, typeinfo section.  Map refs to functions via .pdata."""
import json
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
print("pdata functions:", len(beg))

z = np.load(HERE / "p2_riprefs.npz")
code, tgt = z["code"], z["tgt"]
print("rip refs:", len(code))

regions = {
    "rifx_table": (0x9AF48C8, 0x9AF5C00),
    "fieldinf_head": (0xB48B000, 0xB48B040),
    "typeinfo_head": (0xAECC000, 0xAECC040),
    "fieldinf_all": (0xB48B000, 0xB48B000 + 0x247C00),
    "typeinfo_all": (0xAECC000, 0xAECC000 + 0x5BEC00),
}
out = {}
for nm, (lo, hi) in regions.items():
    m = (tgt >= lo) & (tgt < hi)
    idx = np.nonzero(m)[0]
    funcs = Counter()
    for i in idx:
        cr = int(code[i])
        f = func_of(beg, end, cr)
        if f:
            funcs[f] += 1
        else:
            funcs[("nofunc", cr)] += 1
    print(f"\n{nm}: {len(idx)} refs -> {len(funcs)} distinct sites")
    for f, c in funcs.most_common(12):
        print(f"    {f} x{c}")
    out[nm] = [{"tgt": int(tgt[i]), "code": int(code[i]),
                "func": list(func_of(beg, end, int(code[i])) or [])} for i in idx[:4000]]

(HERE / "p2_sectrefs.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("\nwrote p2_sectrefs.json")
