"""Match runtime registration records against the static typeinfo array."""
import json
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402
from p2_rec import decode_stub, str_at_rva, func_of  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
TI = SEC["typeinfo"]
TI_VA, TI_OFF = TI["va"], TI["roff"]
STRIDE = 0x50
KIND = 0x00080089

# static records: marker positions
ti = DATA[TI_OFF:TI_OFF + TI["rsize"]]
mk = []
start = 0
while True:
    p = ti.find(b"\x89\x00\x08\x00", start)
    if p < 0:
        break
    mk.append(p)
    start = p + 1
mk = np.array(mk)
rec_starts = mk - 4          # record base = marker - 4
print("static records:", len(rec_starts), "base alignment mod 0x50:",
      Counter := __import__("collections").Counter((rec_starts % STRIDE).tolist()).most_common(5))


def static_rec(rva):
    """Return dict of the static record containing rva, or None."""
    off = rva - TI_VA
    if off < 0 or off >= len(ti):
        return None
    # find nearest record start <= off
    i = int(np.searchsorted(rec_starts, off, "right")) - 1
    if i < 0:
        return None
    base = int(rec_starts[i])
    if base + STRIDE > len(ti):
        return None
    b = ti[base:base + STRIDE]
    u32 = np.frombuffer(b[:0x40], dtype="<u4")
    u64 = np.frombuffer(b, dtype="<u8")
    return {"base_rva": TI_VA + base, "delta": off - base,
            "u32": [int(x) for x in u32], "u64": [hex(int(x)) for x in u64]}


def main():
    for fn in [0x68140, 0x67b20, 0x683f0]:
        b, e, slots = decode_stub(fn)
        base = min(slots)
        print(f"\n########## stub {b:08x}  runtime base {base:08x} (mod0x50={(base-TI_VA)%STRIDE})")
        for s in sorted(slots):
            v = slots[s]
            if isinstance(v, int):
                print(f"   +0x{s-base:x} {s:08x} = {v:016x} {img.sec_of(v)} {str_at_rva(v) or ''}")
        # static record that contains the runtime base
        sr = static_rec(base)
        print("   static record containing runtime base:", json.dumps(sr, indent=None)[:600] if sr else None)
        # on-disk dump of 0x50 bytes at base-0x20 .. base+0x30
        o = img.rva2off(base - 0x20)
        print(f"   on-disk @ base-0x20 ({base-0x20:08x}):")
        for k in range(0, 0x90, 16):
            chunk = DATA[o + k:o + k + 16]
            print(f"      {base-0x20+k:08x}  " + " ".join(f"{x:02x}" for x in chunk))


main()
