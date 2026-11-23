#!/usr/bin/env python3
"""Harvest the runtime reflection tables that live in .data.

Evidence: .data holds qwords pointing straight at printable strings in .rdata
(e.g. "TeamId", "ClientGame", "Health"). Those are Frostbite property/type name
tables. Neighbouring qwords usually carry the property offset / type id, which
is exactly what a runtime locator needs.

Output: analysis/data_reflection_tables.json
"""
import json
import re
import struct
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUT = Path(__file__).with_name("data_reflection_tables.json")


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


def cstr(data, off, limit=200):
    if off < 0 or off >= len(data):
        return None
    end = data.find(b"\0", off, off + limit)
    if end <= off:
        return None
    s = data[off:end]
    try:
        t = s.decode("utf-8")
    except UnicodeDecodeError:
        return None
    if len(t) < 2 or not all(32 <= ord(c) < 127 for c in t):
        return None
    if not re.fullmatch(r"[A-Za-z_][A-Za-z0-9_:<>, .\-]*", t):
        return None
    return t


def main():
    data = GAME.read_bytes()
    secs = parse_sections(data)
    sec = {s["name"]: s for s in secs}
    rdata = sec[".rdata"]
    rlo, rhi = rdata["va"], rdata["va"] + rdata["rsize"]

    def rva_to_off(rva):
        for s in secs:
            if s["va"] <= rva < s["va"] + s["rsize"]:
                return s["roff"] + (rva - s["va"])
        return None

    findings = []
    for sname in (".data", "typeinfo"):
        s = sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        for i in range(0, len(blob) - 8, 8):
            v = struct.unpack_from("<Q", blob, i)[0]
            if not (IMAGE_BASE <= v < IMAGE_BASE + 0x10000000):
                continue
            rva = v - IMAGE_BASE
            if not (rlo <= rva < rhi):
                continue
            off = rva_to_off(rva)
            txt = cstr(data, off)
            if not txt:
                continue
            findings.append((sname, s["va"] + i, rva, txt))
    print("string-pointer findings:", len(findings))

    # group by the pointer's own address alignment to infer record stride
    by_str = {}
    for sname, addr, rva, txt in findings:
        by_str.setdefault(txt, []).append((sname, hex(addr), hex(rva)))

    # dump neighbourhoods for the interesting property names
    watch = ["Health", "TeamId", "m_health", "m_teamId", "m_maxHealth", "HealthComponent",
             "ClientGame", "GameContext", "LocalPlayer", "ClientSoldierEntity",
             "ClientPlayerManager", "m_transform", "m_position", "m_entityId", "m_name"]
    dumps = []
    seen = set()
    for sname, addr, rva, txt in findings:
        if txt not in watch:
            continue
        key = (sname, addr & ~0xFF)
        if key in seen:
            continue
        seen.add(key)
        off = rva_to_off(addr - IMAGE_BASE)
        if off is None:
            continue
        win = data[off:off + 0x60]
        cells = []
        for k in range(0, len(win), 8):
            q = struct.unpack_from("<Q", win, k)[0]
            cells.append(hex(q))
        u32s = [hex(x) for x in struct.unpack_from("<12I", win, 0)]
        dumps.append({"sec": sname, "addr": hex(addr), "str": txt, "qwords": cells, "u32": u32s})

    report = {
        "total": len(findings),
        "distinct_strings": len(by_str),
        "watch": {k: v[:8] for k, v in by_str.items() if k in watch},
        "neighbourhoods": dumps[:120],
    }
    OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
    for d in dumps[:40]:
        print(d["sec"], d["addr"], repr(d["str"]))
        print("   u32:", d["u32"][:12])
    print("written:", OUT)


if __name__ == "__main__":
    main()
