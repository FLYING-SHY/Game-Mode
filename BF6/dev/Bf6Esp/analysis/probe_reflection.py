#!/usr/bin/env python3
"""BF6 / Frostbite reflection probe.

Parses the custom `typeinfo` / `fieldinf` sections of bf6.exe and the .rdata
reflection string blob, to answer:
  * how type/property names are hashed (so field offsets can be resolved at runtime)
  * which entity/player/health/team/transform names exist in this build
  * where the string blob lives (file offset + VA) for later AOB work
"""
import json
import re
import struct
import sys
import zlib
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000


def parse_sections(data: bytes):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[pe:pe + 4] == b"PE\0\0"
    nsec = struct.unpack_from("<H", data, pe + 6)[0]
    opt_size = struct.unpack_from("<H", data, pe + 20)[0]
    base = pe + 24 + opt_size
    out = []
    for i in range(nsec):
        off = base + i * 40
        name = data[off:off + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, off + 8)
        out.append(dict(name=name, va=vaddr, vsize=vsize, roff=roff, rsize=rsize))
    return out


def file_to_va(sections, off):
    for s in sections:
        if s["roff"] <= off < s["roff"] + s["rsize"]:
            return IMAGE_BASE + s["va"] + (off - s["roff"])
    return None


def va_to_file(sections, va):
    rva = va - IMAGE_BASE
    for s in sections:
        if s["va"] <= rva < s["va"] + s["rsize"]:
            return s["roff"] + (rva - s["va"])
    return None


def printable_runs(buf: bytes, minlen=4):
    return [(m.start(), m.group().decode("ascii")) for m in re.finditer(rb"[ -~]{%d,}" % minlen, buf)]


def find_hash_candidates(name: str, hashes: dict):
    """Try common Frostbite-style hashes for `name` and see which one is present."""
    b = name.encode()
    cands = {
        "crc32": zlib.crc32(b) & 0xFFFFFFFF,
        "crc32-lower": zlib.crc32(b.lower()) & 0xFFFFFFFF,
        "crc32-upper": zlib.crc32(b.upper()) & 0xFFFFFFFF,
    }
    # FNV-1a 32
    h = 0x811C9DC5
    for c in b:
        h = ((h ^ c) * 0x01000193) & 0xFFFFFFFF
    cands["fnv1a32"] = h
    h = 0x811C9DC5
    for c in b.lower():
        h = ((h ^ c) * 0x01000193) & 0xFFFFFFFF
    cands["fnv1a32-lower"] = h
    # FNV-1 32
    h = 0x811C9DC5
    for c in b:
        h = ((h * 0x01000193) & 0xFFFFFFFF) ^ c
    cands["fnv1_32"] = h
    # djb2
    h = 5381
    for c in b:
        h = ((h * 33) + c) & 0xFFFFFFFF
    cands["djb2"] = h
    # sdbm
    h = 0
    for c in b:
        h = (c + (h << 6) + (h << 16) - h) & 0xFFFFFFFF
    cands["sdbm"] = h
    # murmur3-ish / xxhash32 not attempted
    hits = {}
    for k, v in cands.items():
        if v in hashes:
            hits[k] = v
    return cands, hits


def main():
    data = GAME.read_bytes()
    sections = parse_sections(data)
    by_name = {s["name"]: s for s in sections}

    report = {"file": str(GAME), "size": len(data), "sections": sections}

    # ---- 1. locate the reflection string blob -------------------------------
    marker = b"ClientPlayerManager"
    blob_off = data.find(marker)
    report["reflection_blob_file_off"] = hex(blob_off)
    report["reflection_blob_va"] = hex(file_to_va(sections, blob_off) or 0)

    # ---- 2. typeinfo / fieldinf section layout -----------------------------
    for nm in ("typeinfo", "fieldinf"):
        s = by_name[nm]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        report[nm] = {
            "va": hex(IMAGE_BASE + s["va"]),
            "roff": hex(s["roff"]),
            "rsize": s["rsize"],
            "vsize": s["vsize"],
            "head_hex": blob[:96].hex(),
        }
        # try to detect the record stride by looking for repeated periodicity
        for stride in (0x20, 0x28, 0x30, 0x40, 0x48):
            if s["rsize"] % stride == 0:
                report[nm]["stride_candidates"] = report[nm].get("stride_candidates", [])
                report[nm]["stride_candidates"].append(hex(stride))
        # 4-byte value histogram of the first 4 KiB (hash candidates live here)
        vals = {}
        for i in range(0, min(len(blob), 0x4000) - 4, 4):
            v = struct.unpack_from("<I", blob, i)[0]
            vals[v] = vals.get(v, 0) + 1
        report[nm]["first4k_u32_sample"] = sorted(vals.items(), key=lambda kv: -kv[1])[:12]

    # ---- 3. do typeinfo records point at names? ---------------------------
    ti = by_name["typeinfo"]
    blob = data[ti["roff"]:ti["roff"] + ti["rsize"]]
    ptr_hits = 0
    name_ptr_examples = []
    for i in range(0, min(len(blob), 0x2000), 4):
        v = struct.unpack_from("<I", blob, i)[0]
        if IMAGE_BASE + 0x0D000000 < v < IMAGE_BASE + 0x0F000000:
            ptr_hits += 1
            if len(name_ptr_examples) < 8:
                name_ptr_examples.append((hex(i), hex(v)))
    report["typeinfo"]["in_rdata_ptr_count_first8k"] = ptr_hits
    report["typeinfo"]["ptr_examples"] = name_ptr_examples

    # ---- 4. which reflection names exist ----------------------------------
    interesting = [
        "ClientEntityManager", "EntityManager", "ClientPlayerManager", "ClientPlayer",
        "ClientSoldierEntity", "ClientCharacterEntity", "SoldierEntity", "ClPlayer",
        "HealthComponent", "SoldierHealthComponent", "ClientPlayerHealthComponent",
        "m_health", "m_teamId", "m_transformation", "m_name", "m_entityId",
        "Player_%d", "ClientPlayerIteratorEntity", "NativePtr<ClientPlayerManager>",
        "ClClientToServerInfo", "ClientConnection", "TimeSync", "ClientGameContext",
        "GameContext", "viewMatrix", "ViewMatrix", "ProjectionMatrix",
        "ClientCameraEntity", "CameraEntity", "ClientCamera",
        "SoldierAABB", "ClientAreaAABB", "AABB", "BoundingBox",
        "ClientPhysicsComponent", "ClientSoldierWeapon", "SoldierWeapon",
        "SoldierHealthComponent", "VehicleHealthComponent",
        "ClientDamage", "ClientSoldierAim", "Aim", "SoldierAimingComponent",
    ]
    found = {}
    for nm in interesting:
        offs = [m.start() for m in re.finditer(re.escape(nm.encode()), data)]
        found[nm] = [hex(o) for o in offs[:6]]
    report["names"] = found

    # ---- 5. hash algorithm detection --------------------------------------
    # candidate 32-bit values that appear in typeinfo/fieldinf
    ti_blob = data[by_name["typeinfo"]["roff"]:by_name["typeinfo"]["roff"] + by_name["typeinfo"]["rsize"]]
    fi_blob = data[by_name["fieldinf"]["roff"]:by_name["fieldinf"]["rsize"]] if "fieldinf" in by_name else b""
    universe = set()
    for blob in (ti_blob, fi_blob):
        for i in range(0, len(blob) - 4, 4):
            universe.add(struct.unpack_from("<I", blob, i)[0])
    report["hash_universe_size"] = len(universe)
    probe = {}
    for nm in ["ClientPlayerManager", "ClientSoldierEntity", "HealthComponent", "m_health", "m_teamId"]:
        cands, hits = find_hash_candidates(nm, universe)
        probe[nm] = {"candidates": {k: hex(v) for k, v in cands.items()}, "found_in_sections": hits}
    report["hash_probe"] = probe

    out = Path(__file__).with_name("reflection_probe.json")
    out.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2)[:6000])
    print("written:", out)


if __name__ == "__main__":
    main()
