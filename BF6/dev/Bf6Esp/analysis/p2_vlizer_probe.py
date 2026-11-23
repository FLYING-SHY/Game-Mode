"""Probe the .vlizer blob: header, 5-byte item chain, zlib validation."""
import json
import re
import sys
import zlib
from collections import Counter
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
VM = SEC[".vm_sec"]
VL = SEC[".vlizer"]
vm = DATA[VM["roff"]:VM["roff"] + VM["rsize"]]
vl = DATA[VL["roff"]:VL["roff"] + VL["rsize"]]
VLVA = VL["va"]

u32 = list(int.from_bytes(vm[i:i + 4], "little") for i in range(0, len(vm) - 3, 4))
ptrs = [v for v in u32 if v]
print(f"non-zero u32 in .vm_sec: {len(ptrs)}")
print("first 24 ptrs:", [hex(p) for p in ptrs[:24]])
offs = [p - VLVA for p in ptrs]
print("offsets in .vlizer:", [hex(o) for o in offs[:24]])
print("all offsets >=0 and < len(vlizer):", all(0 <= o < len(vl) for o in offs))

# ---- header region ----
print("\n--- .vlizer[0:0x260] ---")
for k in range(0, 0x260, 16):
    print(f"  {k:06x}  " + " ".join(f"{x:02x}" for x in vl[k:k + 16]))

# ---- items pointed at by .vm_sec ----
print("\n--- bytes at the first 12 pointed offsets ---")
for o in offs[:12]:
    print(f"  +0x{o:06x}: " + vl[o:o + 16].hex(" ") + "  |" +
          "".join(chr(c) if 32 <= c < 127 else "." for c in vl[o:o + 16]) + "|")

# ---- 5-byte item hypothesis: decode every 5-byte record as {u32 le, u8} ----
n5 = len(vl) // 5
rec5 = [vl[i * 5:i * 5 + 5] for i in range(n5)]
vals = [int.from_bytes(r[:4], "little") for r in rec5]
tags = [r[4] for r in rec5]
print(f"\n5-byte record view: n={n5}")
print("  u32 part: zeros=", sum(1 for v in vals if v == 0), "distinct=", len(set(vals)),
      "max=", hex(max(vals)), "min=", hex(min(vals)))
small = [v for v in vals if 0 < v < 0x1000]
print(f"  u32 part in (0,0x1000): {len(small)}  top:", Counter(small).most_common(12))
print("  tag byte histogram (top 12):", Counter(tags).most_common(12))

# what do the pointed-at 5-byte records look like?
print("\n--- pointed records as {u32,u8} ---")
seen = []
for o in offs[:40]:
    i = o // 5
    if i * 5 == o and i < n5:
        r = vl[i * 5:i * 5 + 5]
        seen.append((hex(o), hex(int.from_bytes(r[:4], "little")), r[4]))
for s in seen[:40]:
    print("   ", s)

# ---- validate zlib candidates ----
print("\n--- zlib validation ---")
cands = []
for m in (b"\x78\x01", b"\x78\x5e", b"\x78\x9c", b"\x78\xda"):
    s = 0
    while True:
        p = vl.find(m, s)
        if p < 0:
            break
        cands.append(p)
        s = p + 1
ok = 0
samples = []
for p in sorted(set(cands)):
    try:
        d = zlib.decompressobj().decompress(vl[p:p + 65536])
        if len(d) > 64:
            ok += 1
            if len(samples) < 12:
                samples.append({"offset": hex(p), "out_len": len(d),
                                "head": d[:64].decode("latin1")})
    except Exception:
        pass
print(f"  candidate 78xx positions: {len(set(cands))} ; real zlib streams: {ok}")
for s in samples:
    print("   ", s)

out = {"vm_sec_ptr_count": len(ptrs), "first_offsets": [hex(o) for o in offs[:24]],
       "header_hex": vl[:0x260].hex(" "),
       "five_byte": {"n": n5, "u32_zeros": sum(1 for v in vals if v == 0),
                     "u32_distinct": len(set(vals)), "u32_max": hex(max(vals)),
                     "u32_min": hex(min(vals)),
                     "u32_lt_0x1000": len(small),
                     "u32_lt_0x1000_top": Counter(small).most_common(20),
                     "tag_histogram": Counter(tags).most_common(20),
                     "pointed_records": seen[:60]},
       "zlib": {"candidates": len(set(cands)), "real_streams": ok, "samples": samples}}
(HERE / "p2_vlizer_probe.json").write_text(json.dumps(out, indent=1, default=str), encoding="utf-8")
print("\nwrote p2_vlizer_probe.json")
