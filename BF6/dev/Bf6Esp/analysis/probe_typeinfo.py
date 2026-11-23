#!/usr/bin/env python3
"""Structural decode of the Frostbite `typeinfo` / `fieldinf` sections.

Goal: learn the record layout so we can map class names -> fields -> offsets.
Tries: pointer tables into .rdata, index tables, stride detection, and a
hash-function search using the *record-head* universe (low false-positive rate).
"""
import json
import re
import struct
import zlib
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUT = Path(__file__).with_name("typeinfo_decode.json")


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


def crc32_ref(data, init=0xFFFFFFFF, xo=0xFFFFFFFF):
    crc = init
    for c in data:
        crc ^= c
        for _ in range(8):
            crc = (crc >> 1) ^ (0xEDB88320 & -(crc & 1))
    return (crc ^ xo) & 0xFFFFFFFF


def crc32_nonref(data, init=0xFFFFFFFF, xo=0xFFFFFFFF):
    crc = init
    for c in data:
        crc ^= c << 24
        for _ in range(8):
            hi = crc & 0x80000000
            crc = (crc << 1) & 0xFFFFFFFF
            if hi:
                crc ^= 0x04C11DB7
    return (crc ^ xo) & 0xFFFFFFFF


def crc64_ref(data, poly, init=0xFFFFFFFFFFFFFFFF, xo=0xFFFFFFFFFFFFFFFF):
    crc = init
    for c in data:
        crc ^= c
        for _ in range(8):
            crc = (crc >> 1) ^ (poly & -(crc & 1))
    return (crc ^ xo) & 0xFFFFFFFFFFFFFFFF


def crc64_nonref(data, poly, init=0xFFFFFFFFFFFFFFFF, xo=0xFFFFFFFFFFFFFFFF):
    crc = init
    for c in data:
        crc ^= c << 56
        for _ in range(8):
            hi = crc & 0x8000000000000000
            crc = (crc << 1) & 0xFFFFFFFFFFFFFFFF
            if hi:
                crc ^= poly
    return (crc ^ xo) & 0xFFFFFFFFFFFFFFFF


def fnv1a64(d, basis=0xCBF29CE484222325):
    h = basis
    for c in d:
        h ^= c
        h = (h * 0x100000001B3) & 0xFFFFFFFFFFFFFFFF
    return h


def main():
    data = GAME.read_bytes()
    secs = parse_sections(data)
    sec = {s["name"]: s for s in secs}
    rdata = sec[".rdata"]
    report = {}

    for sname in ("typeinfo", "fieldinf"):
        s = sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        r = {"roff": hex(s["roff"]), "rsize": s["rsize"], "vsize": s["vsize"]}

        # 1. any u64 that is a VA into .rdata / .text ?
        rva_lo = rdata["va"]
        rva_hi = rdata["va"] + rdata["rsize"]
        ptr_hits = []
        for i in range(0, len(blob) - 8, 8):
            v = struct.unpack_from("<Q", blob, i)[0]
            if IMAGE_BASE + rva_lo <= v < IMAGE_BASE + rva_hi:
                ptr_hits.append((i, v - IMAGE_BASE))
        r["rdata_ptr_hits"] = len(ptr_hits)
        r["rdata_ptr_examples"] = [(hex(i), hex(v)) for i, v in ptr_hits[:12]]

        # 2. any u32 that is an RVA into .rdata ?
        rva32_hits = []
        for i in range(0, len(blob) - 4, 4):
            v = struct.unpack_from("<I", blob, i)[0]
            if rva_lo <= v < rva_hi:
                rva32_hits.append((i, v))
        r["rdata_rva32_hits"] = len(rva32_hits)
        r["rdata_rva32_examples"] = [(hex(i), hex(v)) for i, v in rva32_hits[:12]]

        # 3. non-zero density profile at several strides
        dens = {}
        for stride in (8, 12, 16, 0x18, 0x20, 0x28, 0x30, 0x40):
            nz = 0
            tot = 0
            for i in range(0, len(blob) - stride + 1, stride):
                chunk = blob[i:i + stride]
                tot += 1
                if any(chunk):
                    nz += 1
            dens[hex(stride)] = f"{nz}/{tot}"
        r["nonzero_density"] = dens

        # 4. first 12 records of the most plausible strides
        for stride in (0x20, 0x28, 0x30, 0x18):
            recs = []
            for i in range(0, min(len(blob), stride * 12), stride):
                recs.append(blob[i:i + stride].hex())
            r[f"records_{hex(stride)}"] = recs

        # 5. non-zero u64 histogram (top values)
        vals = {}
        for i in range(0, len(blob) - 8, 8):
            v = struct.unpack_from("<Q", blob, i)[0]
            if v:
                vals[v] = vals.get(v, 0) + 1
        r["top_u64"] = sorted(((hex(k), v) for k, v in vals.items()), key=lambda kv: -kv[1])[:10]

        report[sname] = r

    # ---- hash search against a *tight* universe ---------------------------
    # Use only the u64 values that sit at 8-byte aligned record heads of typeinfo
    ti = sec["typeinfo"]
    tib = data[ti["roff"]:ti["roff"] + ti["rsize"]]
    tight = set()
    for i in range(0, len(tib) - 8, 8):
        v = struct.unpack_from("<Q", tib, i)[0]
        if v:
            tight.add(v)
    lo = set()
    for v in tight:
        lo.add(v & 0xFFFFFFFF)
        lo.add(v >> 32)
    report["tight_u64_count"] = len(tight)
    report["tight_lo32_count"] = len(lo)

    # names to test
    names = []
    for nm in ["Health", "TeamId", "m_health", "m_teamId", "ClientSoldierEntity",
               "ClientPlayerManager", "HealthComponent", "SoldierHealthComponent",
               "ClientEntityManager", "EntityManager", "GameContext", "ClientGame",
               "m_transformation", "m_position", "m_name", "TransformComponent",
               "Soldier", "ClientSoldierWeapon", "Player", "SoldierEntity"]:
        names.append(nm)

    def variants(nm):
        yield nm
        yield nm.lower()
        yield nm.upper()
        yield "fb::" + nm
        yield "fb::" + nm.lower()
        yield "Client" + nm
        yield "m_" + nm
        yield "Client" + nm + "Component"

    fams = {
        "crc32_std": lambda d: zlib.crc32(d) & 0xFFFFFFFF,
        "crc32_ref_init0": lambda d: crc32_ref(d, 0, 0),
        "crc32_ref_init0_xo": lambda d: crc32_ref(d, 0, 0xFFFFFFFF),
        "crc32_ref_initFF_xo0": lambda d: crc32_ref(d, 0xFFFFFFFF, 0),
        "crc32_nonref_init0": lambda d: crc32_nonref(d, 0, 0),
        "crc32_nonref_initFF": lambda d: crc32_nonref(d, 0xFFFFFFFF, 0xFFFFFFFF),
        "crc64_iso_ref": lambda d: crc64_ref(d, 0x95AC9329AC4BC9B5),
        "crc64_ecma_ref": lambda d: crc64_ref(d, 0xC96C5795D7870F42),
        "crc64_iso_nonref": lambda d: crc64_nonref(d, 0x000000000000001B),
        "crc64_ecma_nonref": lambda d: crc64_nonref(d, 0x42F0E1EBA9EA3693),
        "fnv1a64": lambda d: fnv1a64(d),
    }
    hits = {}
    for fam, fn in fams.items():
        for nm in names:
            for v in variants(nm):
                d = v.encode()
                h = fn(d)
                if h in tight or (h & 0xFFFFFFFF) in lo or (h >> 32) in lo:
                    hits.setdefault(fam, []).append((nm, v, hex(h)))
    report["hash_hits"] = hits
    OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2)[:9000])
    print("written:", OUT)


if __name__ == "__main__":
    main()
