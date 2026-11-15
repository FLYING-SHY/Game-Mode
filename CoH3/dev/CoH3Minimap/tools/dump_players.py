#!/usr/bin/env python3
"""只读探查 player 对象 team 字段与本地玩家来源：dump 8 个 player 对象头 + World/session 头。"""
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

print(f"unique players={len(players)}")
for plid, pl in players.items():
    data = r.read(pl, 0x60)
    u32s = struct.unpack('<24I', data)
    print(f"player {plid} @{pl:#x}")
    print('  u32[0x00..0x5c]:', ' '.join(f'{v:8x}' for v in u32s))

print('--- World header 0x40 ---')
wh = r.read(World, 0x40)
print(' '.join(f'{v:8x}' for v in struct.unpack('<16I', wh)))
print('--- session head 0x100 ---')
sh = r.read(segBase + sessOff, 0x100)
print(' '.join(f'{v:8x}' for v in struct.unpack('<64I', sh)))
