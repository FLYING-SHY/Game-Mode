#!/usr/bin/env python3
"""Decode the Frostbite reflection tables that live in .data (bug-fixed).

The previous probe had an RVA/VA mix-up, so it produced no neighbourhoods.
This version:
  * finds every 8-byte slot in .data / .rdata / typeinfo that points at an
    ASCII string in .rdata,
  * groups them into contiguous runs (i.e. real tables),
  * for every run, prints name -> neighbouring u32 values, which is where the
    property offset / type id lives,
  * dumps the distinct string vocabulary of those tables.

Output: analysis/data_tables.json
"""
import json
import re
import struct
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUT = Path(__file__).with_name("data_tables.json")

IDENT = re.compile(r"^[A-Za-z_][A-Za-z0-9_:<>, .\-]{1,80}$")


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


def cstr(data, off, limit=140):
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
    return t if IDENT.match(t) else None


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

    # ---- collect pointer slots -------------------------------------------
    slots = {}   # secname -> {rva_of_slot: string}
    for sname in (".data", ".rdata", "typeinfo", "fieldinf"):
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
            t = cstr(data, off)
            if t:
                slots.setdefault(sname, {})[s["va"] + i] = t

    for k, v in slots.items():
        print(f"{k}: {len(v)} string-pointer slots")

    # ---- contiguous runs ---------------------------------------------------
    runs = []
    for sname, m in slots.items():
        addrs = sorted(m)
        cur = []
        for a in addrs:
            if cur and a - cur[-1] != 8:
                if len(cur) >= 3:
                    runs.append((sname, cur))
                cur = []
            cur.append(a)
        if len(cur) >= 3:
            runs.append((sname, cur))
    print("runs(>=3):", len(runs))
    runs.sort(key=lambda r: -len(r[1]))
    for sname, run in runs[:15]:
        print(f"  {sname} run len={len(run)} at {hex(run[0])}..{hex(run[-1])}")

    # ---- vocabulary --------------------------------------------------------
    vocab = {}
    for sname, m in slots.items():
        for a, t in m.items():
            vocab.setdefault(t, []).append((sname, hex(a)))
    print("distinct strings:", len(vocab))

    # ---- look for the property names we care about -------------------------
    want = ["m_health", "m_maxHealth", "m_teamId", "m_transform", "m_position",
            "m_entityId", "m_name", "m_transformComponent", "ClientSoldierEntity",
            "HealthComponent", "SoldierHealthComponent", "ClientPlayerManager",
            "m_pEntityManager", "m_entities", "m_viewMatrix", "m_projectionMatrix",
            "m_cameraTransform", "m_teamId", "TeamId", "Health"]
    report = {"want": {}, "vocab_sample": {}, "runs": []}

    for w in want:
        hits = []
        for sname, m in slots.items():
            for a, t in m.items():
                if t == w:
                    hits.append((sname, hex(a)))
        if hits:
            report["want"][w] = hits[:12]

    # dump neighbourhoods of the wanted ones
    neigh = []
    for w, hits in report["want"].items():
        for sname, ahex in hits[:4]:
            a = int(ahex, 16)
            off = rva_to_off(a)
            if off is None:
                continue
            win = data[off - 0x20:off + 0x40]
            u32 = [hex(x) for x in struct.unpack_from("<24I", win, 0)]
            q = [hex(x) for x in struct.unpack_from("<12Q", win, 0)]
            neigh.append({"name": w, "sec": sname, "slot": ahex, "u32": u32, "q": q})
    report["neighbourhoods"] = neigh

    # biggest runs: full name lists (these are the type->property tables)
    for sname, run in runs[:40]:
        report["runs"].append({
            "sec": sname, "start": hex(run[0]), "len": len(run),
            "names": [slots[sname][a] for a in run[:120]],
        })

    # most common strings
    common = sorted(vocab.items(), key=lambda kv: -len(kv[1]))[:80]
    report["vocab_sample"] = {k: len(v) for k, v in common}

    OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print("written:", OUT)


if __name__ == "__main__":
    main()
