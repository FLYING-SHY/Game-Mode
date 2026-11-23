#!/usr/bin/env python3
"""Locate the Frostbite name-hash primitive inside bf6.exe.

Community Frostbite tooling resolves property names by hash. To do that without
guessing, we (a) locate a CRC table / polynomial constant in the binary and
(b) test candidate digest variants against a name we can also see in the binary.

Outputs: crc_table_va, crc_table_file_off, polynomial constant hits, and the
name->digest pairs for every variant tested.
"""
import json
import re
import struct
import zlib
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUT = Path(__file__).with_name("hash_probe.json")


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


def file_to_va(secs, off):
    for s in secs:
        if s["roff"] <= off < s["roff"] + s["rsize"]:
            return IMAGE_BASE + s["va"] + (off - s["roff"])
    return None


def crc_variants(name: str):
    b = name.encode()
    out = {}
    out["crc32_std"] = zlib.crc32(b) & 0xFFFFFFFF
    out["crc32_nul"] = zlib.crc32(b + b"\0") & 0xFFFFFFFF
    out["crc32_lower"] = zlib.crc32(b.lower()) & 0xFFFFFFFF
    out["crc32_upper"] = zlib.crc32(b.upper()) & 0xFFFFFFFF
    # crc32 with length prefix (LE u32)
    out["crc32_len_prefix"] = zlib.crc32(struct.pack("<I", len(b)) + b) & 0xFFFFFFFF
    # init 0 / no final xor
    out["crc32_raw"] = 0
    crc = 0xFFFFFFFF
    for c in b:
        crc ^= c
        for _ in range(8):
            crc = (crc >> 1) ^ (0xEDB88320 & -(crc & 1))
    out["crc32_raw"] = crc
    # init 0, no xor (i.e. crc of raw)
    out["crc32_init0"] = zlib.crc32(b, 0) & 0xFFFFFFFF
    # adler
    out["adler32"] = zlib.adler32(b) & 0xFFFFFFFF
    # jenkins one-at-a-time
    h = 0
    for c in b:
        h = (h + c) & 0xFFFFFFFF
        h = (h + (h << 10)) & 0xFFFFFFFF
        h ^= h >> 6
    h = (h + (h << 3)) & 0xFFFFFFFF
    h ^= h >> 11
    h = (h + (h << 15)) & 0xFFFFFFFF
    out["joaat"] = h
    return out


def main():
    data = GAME.read_bytes()
    secs = parse_sections(data)
    res = {"file": str(GAME)}

    # --- 1. standard CRC32 reflected table ---------------------------------
    table = []
    for i in range(256):
        c = i
        for _ in range(8):
            c = (c >> 1) ^ (0xEDB88320 if c & 1 else 0)
        table.append(c)
    tbytes = b"".join(struct.pack("<I", v) for v in table)
    hits = []
    start = 0
    while True:
        i = data.find(tbytes, start)
        if i < 0:
            break
        hits.append(hex(i))
        start = i + 4
    res["crc32_table_file_offsets"] = hits[:8]
    res["crc32_table_count"] = len(hits)
    if hits:
        res["crc32_table_va"] = hex(file_to_va(secs, int(hits[0], 16)) or 0)

    # --- 2. polynomial constant hits ---------------------------------------
    poly = struct.pack("<I", 0xEDB88320)
    phits = [hex(m.start()) for m in re.finditer(re.escape(poly), data)]
    res["poly_EDB88320_hits"] = phits[:16]
    res["poly_EDB88320_count"] = len(phits)

    # --- 3. other candidate 64-bit hash constants --------------------------
    for cname, cval in (("fnv64_offset", 0xCBF29CE484222325), ("fnv64_prime", 0x100000001B3)):
        b = struct.pack("<Q", cval)
        res[f"{cname}_hits"] = [hex(m.start()) for m in re.finditer(re.escape(b), data)][:8]

    # --- 4. digest variants for candidate names ----------------------------
    universe = set()
    for nm in ("typeinfo", "fieldinf"):
        s = next((x for x in secs if x["name"] == nm), None)
        if not s:
            continue
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        for i in range(0, len(blob) - 4, 4):
            universe.add(struct.unpack_from("<I", blob, i)[0])
        # also 64-bit halves
    res["universe_u32"] = len(universe)

    names = ["ClientPlayerManager", "ClientSoldierEntity", "HealthComponent",
             "ClientEntityManager", "m_health", "m_teamId", "m_transformation"]
    probe = {}
    for nm in names:
        v = crc_variants(nm)
        probe[nm] = {"digests": {k: hex(x) for k, x in v.items()},
                     "in_universe": [k for k, x in v.items() if x in universe]}
    res["name_probe"] = probe

    # --- 5. 64-bit universe probe ------------------------------------------
    u64 = set()
    for nm in ("typeinfo", "fieldinf"):
        s = next((x for x in secs if x["name"] == nm), None)
        if not s:
            continue
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        for i in range(0, len(blob) - 8, 8):
            u64.add(struct.unpack_from("<Q", blob, i)[0])
    res["universe_u64"] = len(u64)
    probe64 = {}
    for nm in names:
        h = 0xCBF29CE484222325
        for c in nm.encode():
            h ^= c
            h = (h * 0x100000001B3) & 0xFFFFFFFFFFFFFFFF
        probe64[nm] = {"fnv1a64": hex(h), "in_u64": h in u64}
    res["name_probe64"] = probe64

    OUT.write_text(json.dumps(res, indent=2), encoding="utf-8")
    print(json.dumps(res, indent=2))


if __name__ == "__main__":
    main()
