#!/usr/bin/env python3
"""Second pass over `fieldinf`: 4-byte-aligned pointer scan.

p3_fieldinf_ptrs.py scanned only on 8-byte alignment and found 1 slot,
yet the u32 histogram of `fieldinf` is dominated by values like
0x49fb5b88 / 0x49fb5b30 - i.e. the HIGH half of a little-endian VA
0x0000000149fb5b88.  Those pointers are therefore stored at 4-byte
aligned (not 8-byte aligned) positions.

This pass:
  * scans fieldinf on 4-byte stride for full 64-bit VAs into .rdata/.data,
  * groups them by target and reports the hot targets,
  * for each hot target, renders the .rdata/.data region both as raw hex
    and as a decoded stream of {ptr,u32} / {ptr,u64} records so that a
    name -> member-offset table becomes visible.
"""
import json
import struct
from collections import defaultdict
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
OUT = Path(__file__).with_name("p3_fieldinf_ptrs2.json")
IMAGE_BASE = 0x140000000

SECS = {
    ".text": (0x1000, 0x600, 0x72C0600),
    "ctr": (0x72C2000, 0x72C0C00, 0x111CA00),
    ".rdata": (0x83DF000, 0x83DD600, 0x16EE600),
    ".data": (0x9ACE000, 0x9ACBC00, 0x64A200),
    "typeinfo": (0xAECC000, 0xA84DE00, 0x5BEC00),
    "fieldinf": (0xB48B000, 0xAE0CA00, 0x247C00),
}
RANGES = [(n, v[0], v[0] + v[2]) for n, v in SECS.items()]


def rva_to_off(rva):
    for name, (va, roff, rsize) in SECS.items():
        if va <= rva < va + rsize:
            return roff + (rva - va), name
    return None, None


def cstr(data, off, limit=96):
    end = data.find(b"\x00", off, off + limit)
    if end < 0:
        return ""
    try:
        return data[off:end].decode("ascii")
    except UnicodeDecodeError:
        return ""


def is_name(s):
    if not s or len(s) < 3 or len(s) > 64:
        return False
    if not (s[0].isalpha() or s[0] == "_"):
        return False
    return all(c.isalnum() or c in "_." for c in s)


def main():
    data = GAME.read_bytes()

    fi_va, fi_roff, fi_rsize = SECS["fieldinf"]
    fi = data[fi_roff:fi_roff + fi_rsize]

    targets = defaultdict(list)
    for i in range(0, len(fi) - 8, 4):
        v = struct.unpack_from("<Q", fi, i)[0]
        if IMAGE_BASE <= v < IMAGE_BASE + 0x10000000:
            rva = v - IMAGE_BASE
            for name, lo, hi in RANGES:
                if lo <= rva < hi:
                    targets[(name, rva)].append(fi_va + i)
                    break

    print("distinct ptr targets (4B stride):", len(targets))
    print("total slots:", sum(len(v) for v in targets.values()))

    hot = sorted(targets.items(), key=lambda kv: -len(kv[1]))[:16]

    report = {"total_slots": sum(len(v) for v in targets.values()),
              "distinct_targets": len(targets), "hot": []}

    for (sec, tgt_rva), slot_list in hot:
        off, _ = rva_to_off(tgt_rva)
        region = data[off:off + 512]
        recs = []
        # try to decode the region as a sequence of 16-byte records
        for k in range(0, min(len(region), 256), 8):
            p = struct.unpack_from("<Q", region, k)[0]
            n = struct.unpack_from("<I", region, k + 8)[0] if k + 12 <= len(region) else 0
            ps = ""
            if IMAGE_BASE <= p < IMAGE_BASE + 0x10000000:
                po, psec = rva_to_off(p - IMAGE_BASE)
                if po is not None:
                    s = cstr(data, po)
                    if is_name(s):
                        ps = s
            recs.append({"rva": hex(tgt_rva + k), "ptr": hex(p),
                         "u32": hex(n), "str": ps})
        report["hot"].append({
            "section": sec,
            "target_rva": hex(tgt_rva),
            "target_off": hex(off),
            "slot_count": len(slot_list),
            "first_slots": [hex(s) for s in slot_list[:10]],
            "hexdump": region[:192].hex(" "),
            "ascii": "".join(chr(b) if 32 <= b < 127 else "." for b in region[:192]),
            "recs": recs[:24],
        })

    # ---- global search: {char* name, u32 offset} records anywhere in .rdata
    # with the string living in .rdata, scanning on 8-byte stride.
    report["scan_name_off_tables"] = scan_tables(data)

    OUT.write_text(json.dumps(report, indent=1), encoding="utf-8")
    print("written", OUT)
    for h in report["hot"]:
        print(h["section"], h["target_rva"], "slots=", h["slot_count"])
        print("   ", h["ascii"][:160])


def scan_tables(data):
    """Look for interleaved {name-ptr, u32 offset} tables in .rdata."""
    out = []
    rd_va, rd_roff, rd_rsize = SECS[".rdata"]
    rd = data[rd_roff:rd_roff + rd_rsize]
    for i in range(0, len(rd) - 16, 8):
        p = struct.unpack_from("<Q", rd, i)[0]
        if not (IMAGE_BASE <= p < IMAGE_BASE + 0x10000000):
            continue
        po, psec = rva_to_off(p - IMAGE_BASE)
        if po is None or psec != ".rdata":
            continue
        s = cstr(data, po, 64)
        if not is_name(s):
            continue
        n = struct.unpack_from("<I", rd, i + 8)[0]
        if n > 0x1000:
            continue
        out.append({"slot_rva": hex(rd_va + i), "name": s, "off": n})
    return out


if __name__ == "__main__":
    main()
