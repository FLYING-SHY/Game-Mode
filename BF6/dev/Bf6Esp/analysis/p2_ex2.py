"""Dump one stub in full detail (bytes + decoded record map)."""
import json
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402
from p2_rec import decode_stub, str_at_rva, func_of  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
recs = json.loads((HERE / "p2_allrec.json").read_text(encoding="utf-8"))
want = sys.argv[1] if len(sys.argv) > 1 else None

# pick a record whose +0x30 holds a .rdata string
pick = None
for r in recs:
    v = r["vals"].get("48") or r["vals"].get("30")
    if v and SEC[".rdata"]["va"] <= v < SEC[".rdata"]["va"] + SEC[".rdata"]["vsize"]:
        pick = r
        break
print("picked:", pick["fn"] if pick else None)
if want:
    for r in recs:
        if hex(r["fn"]) == want.lower():
            pick = r
            break

b, e, slots = decode_stub(pick["fn"])
print(f"=== {b:08x}..{e:08x} ===")
raw = DATA[img.rva2off(b):img.rva2off(b) + (e - b)]
for k in range(0, len(raw), 16):
    print(f"  {b+k:08x}  " + " ".join(f"{x:02x}" for x in raw[k:k + 16]))
print()
base = min(slots)
for s in sorted(slots):
    v = slots[s]
    if isinstance(v, int):
        sec = img.sec_of(v)
        st = str_at_rva(v) if sec == ".rdata" else None
        print(f"  +0x{s-base:x}  {s:08x}  = {v:016x}  {sec}  {st or ''}")
    else:
        print(f"  +0x{s-base:x}  {s:08x}  = {v}")

# dump the .data objects
print()
for s in sorted(slots):
    v = slots[s]
    if isinstance(v, int) and SEC[".data"]["va"] <= v < SEC[".data"]["va"] + SEC[".data"]["vsize"]:
        o = img.rva2off(v)
        print(f"--- .data object @ {v:08x} (slot +0x{s-base:x}) ---")
        for k in range(0, 0x40, 16):
            chunk = DATA[o + k:o + k + 16]
            txt = "".join(chr(c) if 32 <= c < 127 else "." for c in chunk)
            print(f"   {v+k:08x}  " + " ".join(f"{x:02x}" for x in chunk) + "  |" + txt + "|")
        for k in range(0, 0x40, 8):
            q = int.from_bytes(DATA[o + k:o + k + 8], "little")
            sec2 = img.sec_of(q) if 0 < q < 0x7FFFFFFF else None
            st2 = str_at_rva(q) if sec2 == ".rdata" else None
            if sec2:
                print(f"      +{k:02x}: {q:016x} {sec2} {st2 or ''}")
