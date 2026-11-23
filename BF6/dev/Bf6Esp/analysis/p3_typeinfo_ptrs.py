#!/usr/bin/env python3
"""4-byte-stride pointer scan of the `typeinfo` section.

The earlier 8-byte-aligned pass over `typeinfo` found 9715 .rdata
pointers that all resolved to *empty* strings, which is what you get when
you only look at even-aligned slots of a table whose entries are 4-byte
packed.  This pass scans on a 4-byte stride, resolves every pointer into
.rdata / .data, and reports the ones that land on a *non-empty* ASCII
identifier - those are the type / property name references.

Output: analysis/p3_typeinfo_ptrs.json
"""
import json
import struct
from collections import defaultdict
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
OUT = Path(__file__).with_name("p3_typeinfo_ptrs.json")
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


def cstr(data, off, limit=128):
    end = data.find(b"\x00", off, off + limit)
    if end < 0:
        return ""
    try:
        return data[off:end].decode("ascii")
    except UnicodeDecodeError:
        return ""


def is_name(s):
    if not s or len(s) < 2 or len(s) > 96:
        return False
    if not (s[0].isalpha() or s[0] == "_"):
        return False
    return all(32 <= ord(c) < 127 for c in s)


def main():
    data = GAME.read_bytes()
    ti_va, ti_roff, ti_rsize = SECS["typeinfo"]
    ti = data[ti_roff:ti_roff + ti_rsize]

    hits = []          # (slot_rva, target_rva, sec, name)
    empty_ptrs = 0
    for i in range(0, len(ti) - 8, 4):
        v = struct.unpack_from("<Q", ti, i)[0]
        if not (IMAGE_BASE <= v < IMAGE_BASE + 0x10000000):
            continue
        rva = v - IMAGE_BASE
        off, sec = rva_to_off(rva)
        if off is None:
            continue
        s = cstr(data, off)
        if not s:
            empty_ptrs += 1
            continue
        if not is_name(s):
            continue
        hits.append({"slot_rva": ti_va + i, "tgt_rva": rva, "sec": sec, "name": s})

    print("non-empty name pointers:", len(hits), " empty:", empty_ptrs)

    by_target = defaultdict(list)
    for h in hits:
        by_target[(h["sec"], h["tgt_rva"])].append(h["slot_rva"])

    hot = sorted(by_target.items(), key=lambda kv: -len(kv[1]))[:20]
    report = {
        "name_ptr_count": len(hits),
        "empty_ptr_count": empty_ptrs,
        "distinct_name_ptrs": len(by_target),
        "hot": [],
        "all_names": sorted({h["name"] for h in hits}),
    }

    for (sec, tgt_rva), slots in hot:
        off, _ = rva_to_off(tgt_rva)
        region = data[off:off + 256]
        report["hot"].append({
            "sec": sec,
            "target_rva": hex(tgt_rva),
            "count": len(slots),
            "slots": [hex(s) for s in slots[:12]],
            "ascii": "".join(chr(b) if 32 <= b < 127 else "." for b in region[:192]),
            "hexdump": region[:96].hex(" "),
        })

    # neighbourhood of every slot that points at a name: 32 bytes before and
    # after, so we can see whether a member offset is stored alongside.
    ctx = []
    for h in hits[:400]:
        slot_off = h["slot_rva"] - ti_va + ti_roff
        lo = max(0, slot_off - 32)
        blob = data[lo:lo + 96]
        ctx.append({
            "slot_rva": hex(h["slot_rva"]),
            "name": h["name"],
            "hexdump": blob.hex(" "),
            "ascii": "".join(chr(b) if 32 <= b < 127 else "." for b in blob),
        })
    report["slot_context"] = ctx

    OUT.write_text(json.dumps(report, indent=1), encoding="utf-8")
    print("written", OUT)
    for h in report["hot"]:
        print(h["sec"], h["target_rva"], h["count"], h["ascii"][:100])
    print("sample names:", report["all_names"][:40])


if __name__ == "__main__":
    main()
