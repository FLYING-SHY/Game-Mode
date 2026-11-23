#!/usr/bin/env python3
"""p2_global: global pointer hunt for reflection names + section metadata."""
import json
import struct
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE, OUTDIR, hexdump  # noqa: E402

NAMES = [
    "ClientPlayerManager", "HealthComponent", "ClientSoldierEntity", "TeamId",
    "Health", "SquadId", "PersonaId", "LocalPlayer", "ClientConnection",
    "GameContext", "ClientPlayer", "ClientPlayerExtent",
    "ClientPlayerIteratorEntity", "TimeSync", "OnlineManager",
    "ClientSoldierWeapon", "LobbyTeam", "LobbySquad", "LobbyPlayerId",
    "SlotIndex", "ClPlayer", "ClPlayerManager.IdleState",
    "NativePtr<ClientPlayerManager>", "ClientToServerInfoCl", "ClientGame",
    "ClientPlayerEntity", "Player", "Entity",
]


def main():
    img = Img()
    data = img.data

    # ---- 1. PE section headers (raw) ------------------------------------
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    nsec = struct.unpack_from("<H", data, pe + 6)[0]
    optsz = struct.unpack_from("<H", data, pe + 20)[0]
    secoff = pe + 24 + optsz
    print(f"pe={pe:#x} nsec={nsec} optsz={optsz} file_size={len(data)}")
    hdrs = []
    for i in range(nsec):
        o = secoff + i * 40
        nm = data[o:o + 8].rstrip(b"\x00").decode("latin1")
        vsize, va, rsize, roff = struct.unpack_from("<IIII", data, o + 8)
        chars = struct.unpack_from("<I", data, o + 36)[0]
        hdrs.append(dict(name=nm, vsize=vsize, va=va, rsize=rsize, roff=roff,
                         chars=chars))
        print(f"  {nm:10s} va={va:#010x} vsize={vsize:#010x} roff={roff:#010x} "
              f"rsize={rsize:#010x} chars={chars:#010x}")
    (OUTDIR / "p2_pe_sections.json").write_text(json.dumps(hdrs, indent=1),
                                                encoding="utf-8")

    # ---- 2. standalone occurrences of the reflection names --------------
    occ = {}
    for nm in NAMES:
        pat = nm.encode()
        p = 0
        lst = []
        while True:
            i = data.find(pat, p)
            if i < 0:
                break
            p = i + 1
            if i and data[i - 1] != 0:
                continue
            j = i + len(pat)
            if j < len(data) and data[j] != 0:
                continue
            rva = img.off2rva(i)
            lst.append((i, rva))
        occ[nm] = lst
        print(f"{nm:30s} standalone={len(lst)} "
              + " ".join(f"{o:#x}/{r:#x}" if r else f"{o:#x}/?" for o, r in lst[:6]))

    # ---- 3. hunt 8-byte pointers to those occurrences -------------------
    targets = {}
    for nm, lst in occ.items():
        for o, r in lst:
            if r is not None:
                targets[IMAGE_BASE + r] = f"{nm}@{o:#x}"
    tvals = np.array(sorted(targets.keys()), dtype=np.int64)
    print("pointer targets:", len(tvals))

    hits = []
    for nm, s in img.sec.items():
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        if len(blob) < 8:
            continue
        arr = np.frombuffer(blob, dtype="<u8").astype(np.int64)
        m = np.isin(arr, tvals)
        pos = np.nonzero(m)[0]
        if len(pos):
            print(f"  {nm}: {len(pos)} pointer hits")
        for p in pos:
            hits.append({"sec": nm, "slot_rva": s["va"] + int(p) * 8,
                         "target": targets[int(arr[p])]})
    json.dump(hits, open(OUTDIR / "p2_name_ptrs.json", "w"), indent=1)

    # ---- 4. absolute pointers to the typeinfo / fieldinf sections -------
    for nm in ("typeinfo", "fieldinf"):
        s = img.sec[nm]
        base_va = IMAGE_BASE + s["va"]
        pat = struct.pack("<Q", base_va)
        p = 0
        found = []
        while True:
            i = data.find(pat, p)
            if i < 0:
                break
            p = i + 1
            found.append((i, img.off2rva(i)))
        print(f"abs ptr to {nm} base ({base_va:#x}): {found[:10]}")
        # also near-base offsets
        for off in (0x10, 0x100, 0x1000):
            pat2 = struct.pack("<Q", base_va + off)
            p = 0
            f2 = []
            while True:
                i = data.find(pat2, p)
                if i < 0:
                    break
                p = i + 1
                f2.append(i)
            if f2:
                print(f"   +{off:#x}: {f2[:10]}")

    # ---- 5. dump the .rdata pointer targets inside typeinfo ------------
    ts = img.sec["typeinfo"]
    tblob = data[ts["roff"]:ts["roff"] + ts["rsize"]]
    arr = np.frombuffer(tblob, dtype="<u8").astype(np.int64)
    rlo, rhi = img.sec_range(".rdata")
    inr = (arr >= IMAGE_BASE + rlo) & (arr < IMAGE_BASE + rhi)
    pos = np.nonzero(inr)[0]
    print(f"typeinfo qwords -> .rdata: {len(pos)}")
    tgt_rvas = [(int(p), int(arr[p]) - IMAGE_BASE) for p in pos]
    print("first 20 (slot_rva, target_rva):")
    for s_off, t in tgt_rvas[:20]:
        print(f"   slot_rva={ts['va'] + s_off * 8:#x} target_rva={t:#x}")
    dumps = []
    for i, (s_off, t) in enumerate(tgt_rvas[:12]):
        foff = img.rva2off(t)
        raw = data[foff:foff + 64]
        s = raw.split(b"\x00")[0]
        print(f"\n--- target[{i}] rva={t:#x} file={foff:#x} as_str={s[:40]!r}")
        hexdump(data, t, foff, 64)
        dumps.append({"slot_rva": ts["va"] + s_off * 8, "target_rva": t,
                      "file_off": foff, "raw": raw.hex(),
                      "nul_string": s.decode("latin1", "replace")})
    json.dump(dumps, open(OUTDIR / "p2_typeinfo_ptrtargets.json", "w"), indent=1)

    # ---- 6. dump fieldinf head ------------------------------------------
    fs = img.sec["fieldinf"]
    print("\n=== fieldinf head ===")
    hexdump(data, fs["va"], fs["roff"], 0x200)

    # ---- 7. dump the tight cluster of standalone class names ------------
    for foff in (0x90a1f00, 0x90a2000, 0x8f78c00, 0x8dc5300, 0x8dcd400):
        rva = img.off2rva(foff)
        print(f"\n=== .rdata around file {foff:#x} (rva {rva:#x}) ===")
        for ln in hexdump(data, rva, foff, 0x200):
            print("  ", ln)


if __name__ == "__main__":
    main()
