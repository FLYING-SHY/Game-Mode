#!/usr/bin/env python3
"""Follow the .rdata pointers stored inside the `fieldinf` section.

Probe p3_sections.py established that `fieldinf` records embed qword
pointers into .rdata (high-frequency targets 0x149fb5ad8 / 0x149fb5b30 /
0x149fb5b88 / 0x149fb5be8 / 0x149fb5c28).  Those pointers are the most
likely home of the *name -> member offset* description blocks.

This script:
  1. collects every 8-byte slot in fieldinf that points into .rdata,
  2. groups slots by target,
  3. hexdumps + ASCII-renders each hot target region,
  4. looks for a "pointer to string  +  u32 offset" record shape.
"""
import json
import struct
from collections import Counter, defaultdict
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
OUT = Path(__file__).with_name("p3_fieldinf_ptrs.json")
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


def cstr(data, off, limit=256):
    end = data.find(b"\x00", off, off + limit)
    if end < 0:
        return ""
    try:
        return data[off:end].decode("ascii")
    except UnicodeDecodeError:
        return ""


def main():
    data = GAME.read_bytes()

    fi_va, fi_roff, fi_rsize = SECS["fieldinf"]
    fi = data[fi_roff:fi_roff + fi_rsize]

    rd_va, rd_roff, rd_rsize = SECS[".rdata"]

    slots = []
    for i in range(0, len(fi) - 8, 8):
        v = struct.unpack_from("<Q", fi, i)[0]
        rva = v - IMAGE_BASE
        if rd_va <= rva < rd_va + rd_rsize:
            slots.append((fi_va + i, rva))

    print("rdata-pointer slots in fieldinf:", len(slots))

    by_target = defaultdict(list)
    for slot_va, tgt_rva in slots:
        by_target[tgt_rva].append(slot_va)

    hot = sorted(by_target.items(), key=lambda kv: -len(kv[1]))[:12]

    report = {
        "total_slots": len(slots),
        "distinct_targets": len(by_target),
        "hot": [],
    }

    for tgt_rva, slot_list in hot:
        off, _ = rva_to_off(tgt_rva)
        region = data[off:off + 256]
        # interpret region as qword pairs and (ptr,u32) records
        pairs = []
        for k in range(0, min(len(region), 128), 16):
            p = struct.unpack_from("<Q", region, k)[0]
            n = struct.unpack_from("<I", region, k + 8)[0]
            ps = ""
            if IMAGE_BASE <= p < IMAGE_BASE + 0x10000000:
                po, sec = rva_to_off(p - IMAGE_BASE)
                if po is not None:
                    ps = cstr(data, po, 80)
            pairs.append({
                "rva": hex(tgt_rva + k),
                "qword0": hex(p),
                "u32_1": hex(n),
                "str0": ps,
            })
        report["hot"].append({
            "target_rva": hex(tgt_rva),
            "target_off": hex(off),
            "slot_count": len(slot_list),
            "first_slots": [hex(s) for s in slot_list[:8]],
            "hexdump": region[:128].hex(" "),
            "ascii": "".join(chr(b) if 32 <= b < 127 else "." for b in region[:128]),
            "pairs": pairs,
        })

    # --- does the .rdata region at those targets look like {char* name, u32 off}? ---
    # Scan the whole .rdata for records that pair a valid string pointer with a
    # small u32 (candidate member offset) - i.e. an interleaved table.
    rd = data[rd_roff:rd_roff + rd_rsize]
    cand = []
    for i in range(0, len(rd) - 16, 8):
        p = struct.unpack_from("<Q", rd, i)[0]
        if not (IMAGE_BASE <= p < IMAGE_BASE + 0x10000000):
            continue
        prva = p - IMAGE_BASE
        po, sec = rva_to_off(prva)
        if po is None or sec != ".rdata":
            continue
        s = cstr(data, po, 64)
        if not s or not all(32 <= ord(c) < 127 for c in s) or len(s) < 3:
            continue
        if not (s[0].isalpha() or s[0] == "_"):
            continue
        n = struct.unpack_from("<I", rd, i + 8)[0]
        if n < 0x2000:
            cand.append({"slot_rva": hex(rd_va + i), "name": s, "off": hex(n)})

    report["rd_name_off_pairs"] = {
        "count": len(cand),
        "sample": cand[:120],
    }

    OUT.write_text(json.dumps(report, indent=1), encoding="utf-8")
    print("written", OUT)
    print("name+off pairs:", len(cand))
    for c in cand[:40]:
        print(c["slot_rva"], c["off"], c["name"])


if __name__ == "__main__":
    main()
