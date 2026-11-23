#!/usr/bin/env python3
"""RIP-relative xref scanner for bf6.exe (vectorised, whole .text in one pass).

Finds every instruction in .text (and the `ctr` code section) whose RIP-relative
32-bit displacement targets one of the interesting reflection strings / the
typeinfo section, plus every absolute 64-bit pointer to those addresses in the
read-only data.

Output: analysis/xrefs.json
"""
import json
import struct
import numpy as np
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUT = Path(__file__).with_name("xrefs.json")

TARGET_STRINGS = [
    "ClientPlayerManager", "ClPlayer", "ClientPlayerExtent", "ClientPlayerIteratorEntity",
    "Player_%d", "ClientSoldierEntity", "HealthComponent", "LocalPlayer", "TimeSync",
    "ClientConnection", "GameContext", "ClientGame", "OnlineManager", "ClientSoldierWeapon",
    "ClientSoldierWeaponSpawnInfo", "ClPlayerManager.IdleState", "ClientToServerInfoCl",
    "NativePtr<ClientPlayerManager>", "GameWorld", "ClientGameContext", "EntityManager",
    "ClientEntityManager", "m_health", "m_teamId", "Health", "TeamId",
]


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


def main():
    data = GAME.read_bytes()
    secs = parse_sections(data)
    sec = {s["name"]: s for s in secs}

    # ---- build target RVA set ---------------------------------------------
    targets = {}          # rva -> label
    for nm in TARGET_STRINGS:
        pat = nm.encode()
        start = 0
        while True:
            i = data.find(pat, start)
            if i < 0:
                break
            # locate the containing section and convert to RVA
            rva = None
            for s in secs:
                if s["roff"] <= i < s["roff"] + s["rsize"]:
                    rva = s["va"] + (i - s["roff"])
                    break
            if rva is not None and rva not in targets:
                targets[rva] = f"{nm}@{hex(i)}"
            start = i + 1

    # also the typeinfo / fieldinf section ranges (add their base addresses)
    for nm in ("typeinfo", "fieldinf"):
        s = sec[nm]
        targets.setdefault(s["va"], f"section:{nm}+0")

    trva = np.array(sorted(targets.keys()), dtype=np.int64)
    tset = set(int(x) for x in trva)
    print("targets:", len(tset))

    # ---- RIP-relative scan over code sections -----------------------------
    results = {}
    CHUNK = 1 << 22
    for sname in (".text", "ctr"):
        s = sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        n = len(blob) // 4
        hits = []
        for off in range(0, n, CHUNK):
            m = min(CHUNK, n - off)
            arr = np.frombuffer(blob, dtype="<u4", count=m, offset=off * 4).astype(np.int64)
            idx = np.arange(off, off + m, dtype=np.int64)
            disp = arr - (1 << 32) if False else arr
            # sign-extend disp32
            disp = np.where(disp >= (1 << 31), disp - (1 << 32), disp)
            tgt = (s["va"] + idx * 4 + 4 + disp)
            tgt = np.where(tgt < 0, tgt + (1 << 32), tgt)
            mask = np.isin(tgt, trva)
            if mask.any():
                for pos in np.nonzero(mask)[0]:
                    rva = int(tgt[pos])
                    hits.append((s["va"] + (off + int(pos)) * 4, rva, targets[rva]))
        results[sname] = hits
        print(sname, "rip-relative hits:", len(hits))

    # ---- absolute 64-bit pointer scan in data sections ---------------------
    ptr_hits = []
    for sname in (".rdata", ".data", "_RDATA", "typeinfo", "fieldinf"):
        s = sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        arr = np.frombuffer(blob, dtype="<u8")
        arr = arr.astype(np.int64)
        # absolute VA = rva + IMAGE_BASE
        abs_va = arr - IMAGE_BASE
        mask = np.isin(abs_va, trva)
        if mask.any():
            for pos in np.nonzero(mask)[0]:
                rva = int(abs_va[pos])
                ptr_hits.append((sname, s["va"] + int(pos) * 8, rva, targets[rva]))
    results["abs_ptrs"] = ptr_hits
    print("absolute pointer hits:", len(ptr_hits))

    # ---- report ------------------------------------------------------------
    out = {"targets": {hex(k): v for k, v in targets.items()},
           "rip": {k: [(hex(a), hex(b), c) for a, b, c in v] for k, v in results.items() if k != "abs_ptrs"},
           "abs_ptrs": [(a, hex(b), hex(c), d) for a, b, c, d in ptr_hits]}
    OUT.write_text(json.dumps(out, indent=2), encoding="utf-8")
    for k in (".text", "ctr"):
        print("=== ", k)
        for a, b, c in results[k][:400]:
            print("  code@", hex(a), "->", hex(b), c)
    print("=== abs ptrs")
    for a, b, c, d in ptr_hits[:80]:
        print("  ", a, hex(b), "->", hex(c), d)
    print("written:", OUT)


if __name__ == "__main__":
    main()
