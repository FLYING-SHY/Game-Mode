#!/usr/bin/env python3
"""dump player+0x10 指向的子对象，找 team 分组。"""
import ctypes, struct, sys
from collections import OrderedDict
sys.path.insert(0, r'E:\Cache\AI\Deepseek\default-workspace\CoH3Minimap\tools')
from probe_squads import find_game, module_base, Reader

pid = int(sys.argv[1]) if len(sys.argv) > 1 else find_game()
B = module_base(pid)
r = Reader(pid)
segBase = r.u64(B + 0x6185EB0)
sessOff = r.u32(B + 0x5E0BECC)
World = segBase + r.u32(segBase + sessOff + 0x100)
count = r.u32(World + 0xC)
handleOff = r.u32(World + 0x08)
validOff = r.u32(World + 0x14)

players = OrderedDict()
for i in range(min(count, 4096)):
    if r.u32(segBase + validOff + 4 * i) == 0:
        continue
    sq = segBase + r.u32(segBase + handleOff + 8 * i)
    pl = segBase + r.u32(sq + 0x58)
    plid = r.u32(pl + 8)
    if plid not in players:
        players[plid] = pl

print("player -> sub-object(player+0x10) dump:")
subs = {}
for plid, pl in players.items():
    sub = r.u64(pl + 0x10)
    subs[plid] = sub
    d = r.read(sub, 0x60)
    u = struct.unpack('<24I', d)
    print(f"P{plid} sub={sub:#x}: " + ' '.join(f'{v:x}' for v in u[:24]))

# 找 u32 呈两值分组的偏移（假设 4v4 两组）
print("\n-- offset candidates with two-value split --")
rows = {plid: struct.unpack('<64I', r.read(sub, 0x100)) for plid, sub in subs.items()}
for off in range(0x100 // 4):
    col = {plid: rows[plid][off] for plid in rows}
    vals = set(col.values())
    if 2 <= len(vals) <= 3 and all(0 <= v < 16 for v in vals):
        print(f"sub+{off*4:03x}: {col}")
