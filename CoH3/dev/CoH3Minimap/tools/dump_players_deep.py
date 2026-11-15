#!/usr/bin/env python3
"""深度 dump player(0x60-0x140) 与 squad 头(0x00-0x60)，找 4v4 分组模式。"""
import ctypes, struct, sys
from collections import OrderedDict
sys.path.insert(0, r'E:\Cache\AI\Deepseek\default-workspace\CoH3Minimap\tools')
from probe_squads import find_game, module_base, Reader

pid = int(sys.argv[1]) if len(sys.argv) > 1 else find_game()
B = module_base(pid)
r = Reader(pid)
segBase = r.u64(B + 0x6185EB0)
sessOff = r.u32(B + 0x5E0BECC)
local_id = r.u32(segBase + sessOff + 0x08)
local_slot = r.u32(segBase + sessOff + 0x0C)
print(f"LOCAL player id={local_id} slot={local_slot}")
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
        players[plid] = (pl, sq)

for plid, (pl, sq) in players.items():
    d = r.read(pl, 0x140)
    print(f"player {plid} @{pl:#x} deep u32[0x60..0x13c]:")
    for off in range(0x60, 0x140, 0x20):
        vals = struct.unpack_from('<8I', d, off)
        print(f"  +{off:03x}: " + ' '.join(f'{v:8x}' for v in vals))
print()
# squad 头：看 player 指针以外的 team 候选
sq0 = next(iter(players.values()))[1]
d = r.read(sq0, 0x60)
print(f"squad sample @{sq0:#x} u32[0x00..0x5c]:")
for off in range(0, 0x60, 0x20):
    vals = struct.unpack_from('<8I', d, off)
    print(f"  +{off:03x}: " + ' '.join(f'{v:8x}' for v in vals))
