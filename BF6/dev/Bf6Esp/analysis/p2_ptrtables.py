#!/usr/bin/env python3
"""p2_ptrtables: resolve the pointer tables that typeinfo/fieldinf point at.

For every pointer target we walk the target region, resolve embedded pointers
to strings, and look for the reflection names.  This is where (name, hash)
anchor pairs should come from.
"""
import json
import struct
import sys
from collections import Counter
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE, OUTDIR  # noqa: E402

KNOWN = ["Health", "TeamId", "SquadId", "PersonaId", "ClientPlayerManager",
         "ClientSoldierEntity", "HealthComponent", "LocalPlayer", "ClientPlayer",
         "Player", "Entity", "ClientConnection", "GameContext", "TimeSync",
         "LobbyTeam", "LobbySquad", "LobbyPlayerId", "SlotIndex"]


def cstr(img, data, rva, limit=120):
    off = img.rva2off(rva)
    if off is None:
        return None
    end = data.find(b"\x00", off, off + limit)
    if end <= off:
        return None
    s = data[off:end]
    if not s or not all(32 <= b < 127 for b in s):
        return None
    return s.decode("ascii")


def resolve_table(img, data, rva, nslots=24):
    """read nslots u64 from rva, resolve each to a string if possible"""
    off = img.rva2off(rva)
    if off is None:
        return None, []
    out = []
    for k in range(nslots):
        if off + k * 8 + 8 > len(data):
            break
        v = struct.unpack_from("<Q", data, off + k * 8)[0]
        s = None
        if IMAGE_BASE <= v < IMAGE_BASE + 0x0C000000:
            s = cstr(img, data, v - IMAGE_BASE)
        out.append((v, s))
    return off, out


def main():
    img = Img()
    data = img.data

    # ---------------- typeinfo .rdata pointer tables ---------------------
    ts = img.sec["typeinfo"]
    tblob = data[ts["roff"]:ts["roff"] + ts["rsize"]]
    arr = np.frombuffer(tblob, dtype="<u8").astype(np.int64)
    rlo, rhi = img.sec_range(".rdata")
    inr = (arr >= IMAGE_BASE + rlo) & (arr < IMAGE_BASE + rhi)
    pos = np.nonzero(inr)[0]
    print("typeinfo qwords -> .rdata:", len(pos))

    names_found = Counter()
    examples = []
    for i, p in enumerate(pos):
        t = int(arr[p]) - IMAGE_BASE
        _, slots = resolve_table(img, data, t, 12)
        strs = [s for _, s in slots if s]
        for s in strs:
            names_found[s] += 1
        if len(examples) < 25 and strs:
            examples.append({"slot_rva": ts["va"] + int(p) * 8,
                             "target_rva": t,
                             "strings": strs,
                             "raw": [hex(v) for v, _ in slots]})
    print("distinct strings resolved from typeinfo ptr tables:", len(names_found))
    print("top 40:", names_found.most_common(40))
    hits = {k: v for k, v in names_found.items() if k in KNOWN}
    print("KNOWN reflection names among them:", hits)

    # search all resolved strings for known names (case sensitive)
    for i, p in enumerate(pos):
        t = int(arr[p]) - IMAGE_BASE
        _, slots = resolve_table(img, data, t, 12)
        for v, s in slots:
            if s and any(k in s for k in KNOWN):
                print(f"  *** typeinfo slot_rva={ts['va'] + int(p) * 8:#x} "
                      f"target={t:#x} -> {s!r}")
                break

    json.dump({"distinct": len(names_found),
               "top": names_found.most_common(200),
               "examples": examples},
              open(OUTDIR / "p2_ptrtables_typeinfo.json", "w"), indent=1)

    # ---------------- fieldinf .data pointer targets ---------------------
    fs = img.sec["fieldinf"]
    fblob = data[fs["roff"]:fs["roff"] + fs["rsize"]]
    f64 = np.frombuffer(fblob, dtype="<u8").astype(np.int64)
    dvlo, dvhi = img.sec_range(".data")
    ind = (f64 >= IMAGE_BASE + dvlo) & (f64 < IMAGE_BASE + dvhi)
    fpos = np.nonzero(ind)[0]
    print("\nfieldinf qwords -> .data:", len(fpos))
    fnames = Counter()
    fex = []
    for i, p in enumerate(fpos[:4000]):
        t = int(f64[p]) - IMAGE_BASE
        _, slots = resolve_table(img, data, t, 8)
        strs = [s for _, s in slots if s]
        for s in strs:
            fnames[s] += 1
        if len(fex) < 20 and strs:
            fex.append({"slot_rva": fs["va"] + int(p) * 8, "target_rva": t,
                        "strings": strs,
                        "raw": [hex(v) for v, _ in slots]})
    print("fieldinf ptr target strings top:", fnames.most_common(30))

    # raw dump of a couple of .data targets
    for rva in (0x9fb5ad8, 0xa1190a8, 0x9fb5be8):
        off = img.rva2off(rva)
        print(f"\n=== .data target {rva:#x} file {off:#x} ===")
        for k in range(0, 0x80, 16):
            chunk = data[off + k:off + k + 16]
            h = " ".join(f"{b:02x}" for b in chunk)
            a = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
            print(f"   {rva + k:012x} {h:<47} {a}")

    json.dump({"top": fnames.most_common(100), "examples": fex},
              open(OUTDIR / "p2_ptrtables_fieldinf.json", "w"), indent=1)


if __name__ == "__main__":
    main()
