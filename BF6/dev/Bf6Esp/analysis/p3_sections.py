#!/usr/bin/env python3
"""Decode the `typeinfo` / `fieldinf` custom PE sections of bf6.exe.

Both sections are RW (perm 6) and were clearly emitted by the engine's
reflection serialiser.  Prior probes established:

  * typeinfo: records on a 0x50 stride, marker word 0x08000089 at +0x04.
  * fieldinf: no marker, ~2.39 MB.

This script dumps the raw layout of both sections so that the record
structure (and therefore the name -> member-offset mapping) can be
reconstructed statically.
"""
import json
import struct
from collections import Counter
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
OUT = Path(__file__).with_name("p3_sections.json")
IMAGE_BASE = 0x140000000

SECS = {
    ".text": (0x1000, 0x600, 0x72C0600),
    "ctr": (0x72C2000, 0x72C0C00, 0x111CA00),
    ".rdata": (0x83DF000, 0x83DD600, 0x16EE600),
    ".data": (0x9ACE000, 0x9ACBC00, 0x64A200),
    "typeinfo": (0xAECC000, 0xA84DE00, 0x5BEC00),
    "fieldinf": (0xB48B000, 0xAE0CA00, 0x247C00),
}


def rva_to_off(rva):
    for name, (va, roff, rsize) in SECS.items():
        if va <= rva < va + rsize:
            return roff + (rva - va), name
    return None, None


def hexdump(blob, start, length, base=0):
    rows = []
    for off in range(start, min(start + length, len(blob)), 16):
        chunk = blob[off:off + 16]
        if not chunk:
            break
        rows.append({
            "off": hex(base + off),
            "hex": chunk.hex(" "),
            "asc": "".join(chr(b) if 32 <= b < 127 else "." for b in chunk),
        })
    return rows


def main():
    data = GAME.read_bytes()

    report = {}

    # ---------------- typeinfo: stride 0x50 records ------------------------
    ti_va, ti_roff, ti_rsize = SECS["typeinfo"]
    ti = data[ti_roff:ti_roff + ti_rsize]
    recs = []
    for i in range(0, min(len(ti), 0x50 * 40), 0x50):
        r = ti[i:i + 0x50]
        if len(r) < 0x50:
            break
        recs.append({
            "rva": hex(ti_va + i),
            "raw": r.hex(),
            "u32": [hex(struct.unpack_from("<I", r, k * 4)[0]) for k in range(20)],
            "u64": [hex(struct.unpack_from("<Q", r, k * 8)[0]) for k in range(10)],
        })
    report["typeinfo"] = {
        "va": hex(ti_va),
        "roff": hex(ti_roff),
        "rsize": ti_rsize,
        "first_records": recs[:24],
        "head_hexdump": hexdump(ti, 0, 256, ti_va),
    }

    # ---------------- fieldinf: raw structure ------------------------------
    fi_va, fi_roff, fi_rsize = SECS["fieldinf"]
    fi = data[fi_roff:fi_roff + fi_rsize]
    report["fieldinf"] = {
        "va": hex(fi_va),
        "roff": hex(fi_roff),
        "rsize": fi_rsize,
        "head_hexdump": hexdump(fi, 0, 512, fi_va),
        "tail_hexdump": hexdump(fi, len(fi) - 256, 256, fi_va + len(fi) - 256),
    }

    # u32 histogram of the whole fieldinf section: a table of offsets would
    # show a small set of very common values (offsets < 0x2000).
    vals = struct.unpack_from("<%dI" % (len(fi) // 4), fi, 0)
    hist = Counter(vals)
    report["fieldinf"]["u32_common"] = [
        [hex(v), c] for v, c in hist.most_common(40)
    ]
    small = [v for v in vals if v < 0x4000]
    report["fieldinf"]["u32_small_hist"] = [
        [hex(v), c] for v, c in Counter(small).most_common(40)
    ]

    # scan fieldinf for 0x50 / 0x40 / 0x30 / 0x20 / 0x18 stride patterns by
    # looking at how often each alignment offset yields a plausible record.
    for stride in (0x50, 0x48, 0x40, 0x38, 0x30, 0x28, 0x20, 0x18, 0x10):
        # count how many records start with a plausible u32 (offset < 0x4000)
        ok = 0
        for i in range(0, len(fi) - stride, stride):
            a = struct.unpack_from("<I", fi, i)[0]
            if a < 0x4000:
                ok += 1
        report["fieldinf"].setdefault("stride_probe", []).append([hex(stride), ok])

    # typeinfo -> fieldinf pointer at 0xaecea18 (slot rva) - dump neighbourhood
    slot_off = rva_to_off(0xAECEA18)
    if slot_off[0]:
        o = slot_off[0]
        report["typeinfo"]["fieldinf_ptr_slot"] = {
            "slot_rva": hex(0xAECEA18),
            "off": hex(o),
            "hexdump": hexdump(data, o - 64, 192, 0xAECEA18 - 64),
        }

    OUT.write_text(json.dumps(report, indent=1), encoding="utf-8")
    print("written", OUT)
    print(json.dumps(report["fieldinf"]["stride_probe"]))
    print(json.dumps(report["fieldinf"]["u32_common"][:16]))


if __name__ == "__main__":
    main()
