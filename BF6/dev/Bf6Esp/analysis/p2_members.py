"""Find .data tables that point at MEMBER-name strings and dump their cells."""
import json
import re
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000

# 1) locate the member-name strings named in the brief
TARGETS = ["HealthComponent", "ClientPlayerManager", "TeamId", "Health", "LocalPlayer",
           "ClientConnection", "GameContext", "ClientSoldierEntity", "SquadId", "PersonaId",
           "LobbyTeam", "LobbySquad", "LobbyPlayerId", "SlotIndex", "ReturnValue"]
rdata = DATA[SEC[".rdata"]["roff"]:SEC[".rdata"]["roff"] + SEC[".rdata"]["rsize"]]
rdbase = SEC[".rdata"]["va"]
found = {}
for t in TARGETS:
    b = t.encode() + b"\x00"
    p = 0
    hits = []
    while True:
        i = rdata.find(b, p)
        if i < 0:
            break
        hits.append(rdbase + i)
        p = i + 1
    found[t] = hits
    print(f"{t:<24} {len(hits)} occurrences  rvas={[hex(x) for x in hits[:6]]}")

# 2) search the whole file for absolute VAs (or low-half RVAs) of those strings
print("\n--- absolute pointer occurrences in .data / .rdata ---")
for t, hits in found.items():
    for rva in hits[:4]:
        va = IB + rva
        pat = va.to_bytes(8, "little")
        p = 0
        while True:
            i = DATA.find(pat, p)
            if i < 0:
                break
            p = i + 1
            o = i
            # only report .data / .rdata hits
            r = img.off2rva(o)
            sec = img.sec_of(r)
            if sec not in (".data", ".rdata"):
                continue
            print(f"  {t:<22} str_rva=0x{rva:x}  ptr at file 0x{o:x} (rva 0x{r:x}, {sec})")
            # dump the surrounding cell
            base = o - (o % 8)
            for k in range(-0x20, 0x41, 8):
                if base + k < 0 or base + k + 8 > len(DATA):
                    continue
                q = int.from_bytes(DATA[base + k:base + k + 8], "little")
                mark = "  <<<" if base + k == o else ""
                extra = ""
                if 0x140000000 <= q < 0x150000000:
                    oo = img.rva2off(q - IB)
                    if oo is not None:
                        e = DATA.find(b"\x00", oo, oo + 64)
                        if e > oo:
                            try:
                                s = DATA[oo:e].decode("ascii")
                                if s.isprintable():
                                    extra = f' -> "{s[:40]}"'
                            except Exception:
                                pass
                print(f"      rva 0x{img.off2rva(base + k):x}: {q:016x}{extra}{mark}")
            print()
