"""Classify every function that holds a code reference landing on a member-name string:
is it a type-registration stub (stores into the typeinfo section AND LEAs a .rdata string)?
"""
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
data = img.data
sec = img.sec
MEMBERS = ["HealthComponent", "ClientPlayerManager", "ClPlayer", "ClientPlayerExtent",
           "ClientSoldierEntity", "ClientSoldierWeapon", "TeamId", "SquadId", "PersonaId",
           "Health", "LocalPlayer", "ClientPlayer", "ClientConnection", "GameContext",
           "TimeSync", "OnlineManager", "SlotIndex", "LobbyTeam", "LobbySquad",
           "LobbyPlayerId", "ReturnValue"]

# --- function table
pd = sec[".pdata"]
b = data[pd["roff"]:pd["roff"] + pd["rsize"]]
n = len(b) // 12
arr = np.frombuffer(b[:n * 12], dtype="<u4").reshape(n, 3)
beg = arr[:, 0].astype(np.int64)
end = arr[:, 1].astype(np.int64)
keep = (beg > 0) & (end > beg)
beg, end = beg[keep], end[keep]


def func_of(rva):
    i = int(np.searchsorted(beg, rva, side="right")) - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


z = np.load(HERE / "p2_riprefs.npz")
CODE, TGT, FORM = z["code"], z["tgt"], z["form"]
ti = sec["typeinfo"]
rd = sec[".rdata"]
STORE_FORMS = {6, 7, 8, 15, 22, 23}

# --- per function: stores into typeinfo, LEAs into .rdata
sto = defaultdict(int)
lea = defaultdict(int)
ti_lo, ti_hi = ti["va"], ti["va"] + ti["rsize"]
rd_lo, rd_hi = rd["va"], rd["va"] + rd["rsize"]
for i in range(len(CODE)):
    t = int(TGT[i])
    f = int(CODE[i])
    if ti_lo <= t < ti_hi:
        fn = func_of(f)
        if fn:
            sto[fn[0]] += 1
    elif rd_lo <= t < rd_hi:
        fn = func_of(f)
        if fn:
            lea[fn[0]] += 1

# --- member-name refs
name_rvas = defaultdict(list)
for m in MEMBERS:
    pat = m.encode()
    pos = 0
    while True:
        p = data.find(pat, pos)
        if p < 0:
            break
        if p == 0 or data[p - 1] == 0:
            name_rvas[m].append(img.off2rva(p))
        pos = p + 1

tgt_sorted = np.sort(TGT)
refs = []
for m, vas in name_rvas.items():
    for va in vas:
        lo = int(np.searchsorted(tgt_sorted, va, side="left"))
        hi = int(np.searchsorted(tgt_sorted, va, side="right"))
        for k in range(lo, hi):
            cr = int(CODE[k])
            f = func_of(cr)
            refs.append({"member": m, "name_rva": hex(va), "code_rva": hex(cr),
                         "func_rva": hex(f[0]) if f else None,
                         "func_is_type_reg_stub": bool(f and sto.get(f[0], 0) and lea.get(f[0], 0)),
                         "stores_into_typeinfo": int(sto.get(f[0], 0)) if f else 0,
                         "rdata_leas": int(lea.get(f[0], 0)) if f else 0})

print(f"refs: {len(refs)}")
print("per member:", dict(Counter(r['member'] for r in refs)))
stubs = sum(1 for r in refs if r["func_is_type_reg_stub"])
print(f"refs inside a type-registration stub (stores into typeinfo AND LEAs .rdata): "
      f"{stubs}/{len(refs)}")
fns = sorted({r["func_rva"] for r in refs if r["func_rva"]})
print(f"distinct functions: {len(fns)}")
for fr in fns:
    sub = [r for r in refs if r["func_rva"] == fr]
    print(f"  {fr}: members={sorted({r['member'] for r in sub})} "
          f"stores_ti={sub[0]['stores_into_typeinfo']} rdata_leas={sub[0]['rdata_leas']} "
          f"stub={sub[0]['func_is_type_reg_stub']}")

out = {"generated": "p2_memcheck.py",
       "refs": len(refs),
       "refs_in_type_registration_stubs": stubs,
       "distinct_functions": len(fns),
       "per_member": dict(Counter(r["member"] for r in refs)),
       "functions": [{"func_rva": fr,
                      "members": sorted({r["member"] for r in refs if r["func_rva"] == fr}),
                      "stores_into_typeinfo": [r for r in refs if r["func_rva"] == fr][0]["stores_into_typeinfo"],
                      "rdata_leas": [r for r in refs if r["func_rva"] == fr][0]["rdata_leas"],
                      "is_type_registration_stub": [r for r in refs if r["func_rva"] == fr][0]["func_is_type_reg_stub"]}
                     for fr in fns],
       "ref_list": refs}
(HERE / "p2_memcheck.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("wrote p2_memcheck.json")
