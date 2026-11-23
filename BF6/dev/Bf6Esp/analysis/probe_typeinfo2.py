#!/usr/bin/env python3
"""Decode the real record layout of the `typeinfo` / `fieldinf` sections.

Observation from probe_typeinfo: every non-empty record starts with the 4 bytes
`89 00 00 08` at offset +4 and there ARE pointers into .rdata (9715 of them).
So: find the record stride via the recurring marker, then dump full records with
their .rdata pointers resolved to strings. That yields (name -> field/offset)
tables usable by the runtime locator.
"""
import json
import re
import struct
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUT = Path(__file__).with_name("typeinfo_records.json")

MARK = bytes.fromhex("89000800")


def parse_sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    nsec = struct.unpack_from("<H", data, pe + 6)[0]
    opt = struct.unpack_from("<H", data, pe + 20)[0]
    base = pe + 24 + opt
    out = []
    for i in range(nsec):
        o = base + i * 40
        name = data[o:o + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, o + 8)
        out.append(dict(name=name, va=vaddr, vsize=vsize, roff=roff, rsize=rsize))
    return out


def cstr(data, off, limit=160):
    end = data.find(b"\0", off, off + limit)
    if end < 0:
        return None
    s = data[off:end]
    try:
        t = s.decode("utf-8")
    except UnicodeDecodeError:
        return None
    if all(32 <= ord(c) < 127 for c in t):
        return t
    return None


def main():
    data = GAME.read_bytes()
    secs = parse_sections(data)
    sec = {s["name"]: s for s in secs}
    rdata = sec[".rdata"]
    rlo, rhi = rdata["va"], rdata["va"] + rdata["rsize"]

    def read_at_rva(rva, n):
        for s in secs:
            if s["va"] <= rva < s["va"] + s["rsize"]:
                o = s["roff"] + (rva - s["va"])
                return data[o:o + n]
        return None

    report = {}
    for sname in ("typeinfo", "fieldinf"):
        s = sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        marks = [m.start() for m in re.finditer(re.escape(MARK), blob)]
        # stride histogram
        deltas = {}
        for i in range(1, len(marks)):
            d = marks[i] - marks[i - 1]
            deltas[d] = deltas.get(d, 0) + 1
        top = sorted(deltas.items(), key=lambda kv: -kv[1])[:8]

        # records start at (mark - 4)
        rec_starts = sorted(set(m - 4 for m in marks))
        stride = top[0][0] if top else 0

        samples = []
        for rs in rec_starts[:40]:
            rec = blob[rs:rs + (stride or 0x30)]
            if len(rec) < 16:
                continue
            qwords = []
            for i in range(0, len(rec) - 7, 8):
                v = struct.unpack_from("<Q", rec, i)[0]
                rva = v - IMAGE_BASE if IMAGE_BASE <= v < IMAGE_BASE + 0x10000000 else None
                disp = None
                if rva is not None and rlo <= rva < rhi:
                    txt = cstr(data, read_at_rva(rva, 160) and (rdata["roff"] + (rva - rlo)) or 0)
                    disp = {"rva": hex(rva), "str": txt}
                qwords.append(hex(v) if disp is None else disp)
            samples.append({"off": hex(s["roff"] + rs), "rec": rec.hex(), "qwords": qwords})

        # also: all .rdata pointers found in the section with resolved strings
        ptrs = []
        for i in range(0, len(blob) - 8, 8):
            v = struct.unpack_from("<Q", blob, i)[0]
            rva = v - IMAGE_BASE
            if rlo <= rva < rhi:
                txt = cstr(data, rdata["roff"] + (rva - rlo))
                ptrs.append({"off": hex(s["roff"] + i), "rva": hex(rva), "str": txt})
        report[sname] = {
            "mark_count": len(marks),
            "top_deltas": [(hex(d), c) for d, c in top],
            "stride": hex(stride),
            "samples": samples[:25],
            "ptr_count": len(ptrs),
            "ptr_with_string": len([p for p in ptrs if p["str"]]),
            "ptr_sample": ptrs[:60],
        }
        print(sname, "marks:", len(marks), "top deltas:", [(hex(d), c) for d, c in top])
        print("  ptrs:", len(ptrs), "with strings:", report[sname]["ptr_with_string"])
        for p in ptrs[:25]:
            print("   ", p["off"], p["rva"], repr(p["str"])[:70])

    OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print("written:", OUT)


if __name__ == "__main__":
    main()
