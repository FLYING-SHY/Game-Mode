"""Locate code that references MEMBER-name strings and show the surrounding instructions."""
import json
import struct
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000

z = np.load(HERE / "p2_riprefs.npz", allow_pickle=True)
code, tgt, form = z["code"], z["tgt"], z["form"]

NAMES = ["HealthComponent", "ClientPlayerManager", "TeamId", "Health", "LocalPlayer",
         "ClientConnection", "ClientSoldierEntity", "SquadId", "PersonaId",
         "LobbyTeam", "LobbySquad", "LobbyPlayerId", "SlotIndex", "ReturnValue",
         "ClientPlayer", "ClPlayer", "TimeSync"]

# string RVAs (from the earlier scan) -> resolve here
rd = SEC[".rdata"]
blob = DATA[rd["roff"]:rd["roff"] + rd["rsize"]]
rvas = {}
for n in NAMES:
    b = n.encode() + b"\x00"
    p = 0
    out = []
    while True:
        i = blob.find(b, p)
        if i < 0:
            break
        out.append(rd["va"] + i)
        p = i + 1
    rvas[n] = out

allv = {r: n for n, lst in rvas.items() for r in lst}
hits = {}
for v, n in allv.items():
    idx = np.nonzero(tgt == v)[0]
    if idx.size:
        for i in idx:
            hits.setdefault(n, []).append((int(code[i]), int(form[i]), v))

tot = sum(len(v) for v in hits.values())
print(f"rip-refs landing exactly on {len(allv)} member-name strings: {tot}")
for n in NAMES:
    lst = hits.get(n, [])
    print(f"  {n:<22} strings={len(rvas[n]):<3} refs={len(lst)}")

out = {}
for n, lst in hits.items():
    recs = []
    for c, f, v in lst:
        o = img.rva2off(c)
        w = DATA[o - 24:o + 48]
        recs.append({"code_rva": c, "form": f, "target_str_rva": v - IB,
                     "win_before": w[:24].hex(), "win_after": DATA[o:o + 48].hex()})
    out[n] = recs

(HERE / "p2_memxref.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("\nwrote p2_memxref.json")

# print the first few windows of the most interesting names (single-occurrence type names)
for n in ["ClientPlayerManager", "ClientConnection", "ClientSoldierEntity", "Health", "TeamId"]:
    for r in out.get(n, [])[:4]:
        print(f"\n--- {n} @ code rva 0x{r['code_rva']:x} form={r['form']} str rva 0x{r['target_str_rva']:x}")
        b = bytes.fromhex(r["win_before"]) + bytes.fromhex(r["win_after"])
        print("   ", " ".join(f"{x:02x}" for x in b))
