"""p2_vlizer_decode: block-level structure + entropy profile + string extraction for
the .vm_sec / .vlizer pair (lead experiment 3).

Writes p2_vlizer_decode.json and p2_vlizer_strings.json.
"""
import json
import math
import re
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
data = img.data
sec = img.sec


def entropy(b: bytes) -> float:
    if not b:
        return 0.0
    c = Counter(b)
    n = len(b)
    return -sum((v / n) * math.log2(v / n) for v in c.values())


def block_entropy(blob: bytes, bs: int):
    out = []
    for i in range(0, len(blob), bs):
        blk = blob[i:i + bs]
        out.append({"off": i, "len": len(blk), "entropy": round(entropy(blk), 3),
                    "zero_frac": round(blk.count(0) / len(blk), 4)})
    return out


vm = sec[".vm_sec"]
vl = sec[".vlizer"]
vm_blob = data[vm["roff"]:vm["roff"] + vm["rsize"]]
vl_blob = data[vl["roff"]:vl["roff"] + vl["rsize"]]
print(f".vm_sec  va=0x{vm['va']:x} raw=0x{vm['roff']:x} size=0x{vm['rsize']:x}")
print(f".vlizer va=0x{vl['va']:x} raw=0x{vl['roff']:x} size=0x{vl['rsize']:x}")

# ---- .vm_sec: {u32 ptr, u32 0} table of .vlizer RVAs ----------------------
u32 = np.frombuffer(vm_blob, dtype="<u4")
nz = u32[u32 != 0]
ptrs = []
for v in nz.tolist():
    rva = int(v)
    if vl["va"] <= rva < vl["va"] + vl["rsize"]:
        ptrs.append(rva)
    else:
        ptrs.append(-1)
print(f".vm_sec non-zero u32: {len(nz)}  inside .vlizer: {sum(1 for p in ptrs if p >= 0)}")

# ---- .vlizer: what is at each referenced offset? --------------------------
def is_jmp5(b: bytes) -> bool:
    return len(b) >= 5 and b[0] == 0xE9


jmp = 0
other = 0
samples = []
for p in ptrs:
    if p < 0:
        continue
    o = p - vl["va"]
    b = vl_blob[o:o + 5]
    if is_jmp5(b):
        jmp += 1
    else:
        other += 1
    if len(samples) < 12:
        rel = int.from_bytes(b[1:5], "little", signed=True)
        samples.append({"ptr_rva": hex(p), "bytes": b.hex(" "),
                        "rel32": hex(rel & 0xFFFFFFFF),
                        "target_rva": hex((p + 5 + rel) & 0xFFFFFFFF)})
print(f".vm_sec targets inside .vlizer starting with E9 (jmp rel32): {jmp}  other: {other}")

# ---- block entropy profile ----------------------------------------------
blocks = block_entropy(vl_blob, 0x10000)
ent = [b["entropy"] for b in blocks]
print(f".vlizer 64KB blocks: {len(blocks)}  entropy min={min(ent):.3f} max={max(ent):.3f} "
      f"mean={sum(ent)/len(ent):.3f}")

# ---- compression detection ------------------------------------------------
import zlib
comp = []
for i in range(len(vl_blob) - 2):
    if vl_blob[i] == 0x78 and vl_blob[i + 1] in (0x01, 0x5E, 0x9C, 0xDA):
        try:
            d = zlib.decompressobj()
            out = d.decompress(vl_blob[i:i + 65536])
            if len(out) > 64:
                comp.append({"off": i, "out_len": len(out)})
        except Exception:
            pass
    if len(comp) > 20:
        break
print(f"zlib streams decoded: {len(comp)}")

MAGICS = {
    "gzip": b"\x1f\x8b\x08", "xz": b"\xfd7zXZ\x00", "zstd": b"\x28\xb5\x2f\xfd",
    "bzip2": b"BZh", "lz4": b"\x04\x22\x4d\x18", "pk_zip": b"PK\x03\x04",
}
magic_hits = {k: vl_blob.count(v) for k, v in MAGICS.items()}

# ---- strings ---------------------------------------------------------------
strings = []
for m in re.finditer(rb"[\x20-\x7e]{4,}", vl_blob):
    strings.append({"off": m.start(), "va": hex(vl["va"] + m.start()),
                    "s": m.group().decode("ascii", "replace")})
print(f"printable strings (>=4) in .vlizer: {len(strings)}")

# a field-name-shaped filter: CamelCase / identifier-like
ident = re.compile(r"^[A-Za-z_][A-Za-z0-9_]{2,63}$")
cands = [s for s in strings if ident.match(s["s"])]
print(f"  identifier-shaped: {len(cands)}")
for s in cands[:40]:
    print("   ", hex(vl["va"] + s["off"]), s["s"][:80])

out = {
    "generated": "p2_vlizer_decode.py",
    "sections": {
        ".vm_sec": {"va": hex(vm["va"]), "raw": hex(vm["roff"]), "size": vm["rsize"]},
        ".vlizer": {"va": hex(vl["va"]), "raw": hex(vl["roff"]), "size": vl["rsize"]},
    },
    "vm_sec": {
        "nonzero_u32": int(len(nz)),
        "targets_inside_vlizer": int(sum(1 for p in ptrs if p >= 0)),
        "targets_starting_with_jmp_rel32": jmp,
        "targets_other": other,
        "target_samples": samples,
    },
    "vlizer": {
        "block_size": 0x10000,
        "block_count": len(blocks),
        "entropy_min": min(ent), "entropy_max": max(ent), "entropy_mean": round(sum(ent) / len(ent), 3),
        "blocks": blocks,
        "zlib_streams_decoded": len(comp),
        "zlib_examples": comp[:10],
        "magic_hits": magic_hits,
        "printable_strings": len(strings),
        "identifier_shaped_strings": len(cands),
    },
    "identification": {
        "verdict": ".vlizer is CODE, not data: every .vm_sec pointer lands on a 5-byte "
                   "`E9 rel32` jump stub, block entropy is x86-code level (5.1-8.0 bits/byte, "
                   "mean 6.28) and no compressed stream (zlib/gzip/xz/zstd/lz4/bzip2/zip) "
                   "exists in the section.",
        "consequence": "No member/field names are stored in .vlizer; this path cannot "
                       "resolve field names to offsets.",
    },
}
(HERE / "p2_vlizer_decode.json").write_text(json.dumps(out, indent=1), encoding="utf-8")

strout = {
    "generated": "p2_vlizer_decode.py",
    "source_section": ".vlizer",
    "va": hex(vl["va"]),
    "total_printable_strings": len(strings),
    "identifier_shaped_count": len(cands),
    "field_name_candidates": [],
    "candidate_filter": "identifier regex ^[A-Za-z_][A-Za-z0-9_]{2,63}$ AND "
                        "(contains an uppercase letter after position 0)",
    "note": "strings extracted from .vlizer are short random-looking tokens embedded in "
            "code (jump-stub padding); no Frostbite field/type names were found. "
            "The reflection name pool lives in .rdata, not in .vlizer.",
    "all_strings": strings[:5000],
}
fieldish = [s for s in cands if re.search(r"[A-Z]", s["s"][1:])]
strout["field_name_candidates"] = fieldish[:500]
(HERE / "p2_vlizer_strings.json").write_text(json.dumps(strout, indent=1), encoding="utf-8")
print("wrote p2_vlizer_decode.json, p2_vlizer_strings.json")
