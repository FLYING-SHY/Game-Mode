"""Dump the functions that reference member-name strings (the 54 rip-ref hits)."""
import json
import sys
from collections import defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402
from p2_find import build_pdata, func_of  # noqa: E402

img = Img()
data = img.data
SEC = img.sec
IB = 0x140000000
beg, end = build_pdata(img)

z = np.load(HERE / "p2_riprefs.npz")
code, tgt = z["code"], z["tgt"]

NAMES = ["HealthComponent", "ClientPlayerManager", "TeamId", "Health", "LocalPlayer",
         "ClientConnection", "ClientSoldierEntity", "PersonaId", "ClientPlayer", "TimeSync",
         "ClPlayer", "SquadId", "LobbyTeam", "LobbySquad", "LobbyPlayerId", "SlotIndex",
         "ReturnValue", "GameContext"]

# find every occurrence of each name in the image
sites = []
for nm in NAMES:
    b = nm.encode()
    start = 0
    while True:
        i = data.find(b, start)
        if i < 0:
            break
        start = i + 1
        rva = img.off2rva(i)
        sites.append((nm, rva))
print(f"name string occurrences: {len(sites)}")

# rip refs that land exactly on those string starts
sv = {r: nm for nm, r in sites}
hits = defaultdict(list)
for i in range(len(code)):
    t = int(tgt[i])
    if t in sv:
        hits[sv[t]].append((int(code[i]), t))

print("\ncode refs landing exactly on a member-name string:")
for nm in NAMES:
    lst = hits.get(nm, [])
    if lst:
        print(f"  {nm:22s} {len(lst)} refs  code={[hex(c) for c, _ in lst][:8]}")

# for each hit, dump the enclosing function and all its rip refs
out = []
for nm in NAMES:
    for c, t in hits.get(nm, []):
        f = func_of(beg, end, c)
        if not f:
            out.append({"name": nm, "code": hex(c), "func": None})
            continue
        fs, fe = f
        m = (code >= fs) & (code < fe)
        refs = []
        for i in np.nonzero(m)[0]:
            tt = int(tgt[i])
            desc = ""
            if tt in sv:
                desc = f'str "{sv[tt]}"'
            else:
                for sname, s in SEC.items():
                    if s["va"] <= tt < s["va"] + max(s["rsize"], s["vsize"]):
                        desc = f"[{sname}]"
                        if sname == ".rdata":
                            st = img.cstr_at_rva(tt)
                            if st and len(st) < 50 and st.isprintable():
                                desc += f' "{st}"'
                        break
            refs.append({"code": hex(int(code[i])), "tgt": hex(tt), "desc": desc})
        out.append({"name": nm, "code": hex(c), "func": [hex(fs), hex(fe)],
                    "len": fe - fs, "refs": refs})
        print(f"\n### {nm} ref at {hex(c)} func {hex(fs)}..{hex(fe)}")
        for r in refs:
            print(f"    {r['code']} -> {r['tgt']}  {r['desc']}")

(HERE / "p2_memstub.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
print("\nwrote p2_memstub.json")
