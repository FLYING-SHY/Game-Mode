"""Check whether the typeinfo<->fieldinf 'link' is a zero-matching artifact."""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
fi = SEC["fieldinf"]
ti = SEC["typeinfo"]

tblob = DATA[ti["roff"]:ti["roff"] + ti["rsize"]]
arr = np.frombuffer(tblob, dtype="<u4")
bases = np.nonzero(arr == 0x00080089)[0] - 1
d0 = arr[bases]
d11 = arr[bases + 11]
print(f"records={len(bases)}")
print(f"  u32_0 : zeros={int((d0 == 0).sum())} distinct={len(np.unique(d0))}")
print(f"  u32_10: zeros={int((arr[bases + 10] == 0).sum())} distinct={len(np.unique(arr[bases + 10]))}")
print(f"  u32_11: zeros={int((d11 == 0).sum())} distinct_nonzero={len(np.unique(d11[d11 != 0]))}")

d11nz = np.unique(d11[d11 != 0])
farr = np.frombuffer(DATA[fi["roff"]:fi["roff"] + fi["rsize"]], dtype="<u4")
print(f"\nfieldinf u32={len(farr)}  zeros={int((farr == 0).sum())}")
hit_nz = int(np.isin(farr, d11nz).sum())
print(f"  fieldinf values in NONZERO u32_11 domain : {hit_nz}")
print(f"  fieldinf zeros in u32_11(=0) domain     : {int((farr == 0).sum())}")
exp_rand = len(farr) * len(d11nz) / 2 ** 32
print(f"  random expectation for {len(d11nz)} keys   : {exp_rand:.1f}")
print(f"  ratio hit/expect = {hit_nz / max(exp_rand, 1e-9):.1f}x")

# what are the nonzero u32_11 values?
print("\n  sample nonzero u32_11:", [hex(int(x)) for x in d11nz[:10]])
# do the matching fieldinf values cluster?
if hit_nz:
    vals = farr[np.isin(farr, d11nz)]
    u, c = np.unique(vals, return_counts=True)
    o = np.argsort(-c)[:10]
    print("  top matching values:", [(hex(int(u[i])), int(c[i])) for i in o])
