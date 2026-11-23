"""Inspect the non-standard sections .vm_sec and .vlizer for a reflection DB."""
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec

for name in (".vm_sec", ".vlizer"):
    s = SEC.get(name)
    if not s:
        print(f"{name}: MISSING from section table; known={list(SEC)}")
        continue
    print(f"\n########## {name}: va=0x{s['va']:x} vsize=0x{s.get('vsize',0):x} "
          f"roff=0x{s['roff']:x} rsize=0x{s['rsize']:x}")
    o = s["roff"]
    n = min(s["rsize"], 0x400)
    b = DATA[o:o + n]
    for k in range(0, n, 16):
        print(f"  {s['va']+k:08x}  " + " ".join(f"{x:02x}" for x in b[k:k + 16]))
    # string census over the whole section
    blob = DATA[o:o + s["rsize"]]
    strs = [m.group().decode("ascii", "replace") for m in re.finditer(rb"[ -~]{4,}", blob)]
    print(f"  -- printable runs >=4: {len(strs)}; first 40:")
    for t in strs[:40]:
        print("     ", t[:100])
    if strs:
        for probe in ("Health", "TeamId", "ClientSoldierEntity", "RIFX", "Field", "Offset"):
            c = sum(1 for t in strs if probe in t)
            if c:
                print(f"     probe {probe!r}: {c} hits")
