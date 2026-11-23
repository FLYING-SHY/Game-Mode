"""Search the whole file for qualified/mangled forms of known type names."""
import re
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec

tests = [
    b"AICommander::ServerAICommanderOrderEntity",
    b"ServerAICommanderOrderEntity",
    b"AICommander::",
    b"games::AICommander",
    b"fb::games",
    b"fb::",
    b"::AftermathProcess",
    b"AftermathRender::",
    b"AftermathProcess",
    b"ServerActivityEntryComponentInterface",
    b"ActivityRuntime::ServerActivityEntryComponentInterface",
]
for t in tests:
    idx = []
    s = 0
    while True:
        p = DATA.find(t, s)
        if p < 0:
            break
        idx.append(p)
        s = p + 1
        if len(idx) > 4000:
            break
    print(f"{t!r}: {len(idx)} hits")
    for p in idx[:6]:
        r = img.off2rva(p)
        ctx = DATA[max(0, p - 24):p + len(t) + 24]
        print(f"    off=0x{p:x} rva=0x{r:x} ctx={ctx!r}")

# all occurrences of one type name with 40 bytes of leading context
print("\n--- context of every 'AftermathProcess' occurrence ---")
t = b"AftermathProcess"
s = 0
while True:
    p = DATA.find(t, s)
    if p < 0:
        break
    s = p + 1
    r = img.off2rva(p)
    lead = DATA[max(0, p - 40):p]
    trail = DATA[p + len(t):p + len(t) + 24]
    print(f"  off=0x{p:x} rva=0x{r:x} sec={img.sec_of(r)}")
    print(f"     lead ={lead!r}")
    print(f"     trail={trail!r}")
