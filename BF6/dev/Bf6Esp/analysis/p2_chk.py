import sys, re
sys.path.insert(0, r'F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis')
from p2_common import Img
img = Img()
data = img.data
off = img.rva2off(0x8DCEEF8)
print("before:", data[off-40:off])
print("after :", data[off:off+20])
IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}\Z")
print("match:", IDENT.match(b"SquadId"))
rd = img.sec['.rdata']
rdb = data[rd['roff']:rd['roff']+rd['rsize']]
i = 0
found = 0
cnt = 0
while True:
    j = rdb.find(b"\x00", i)
    if j < 0:
        break
    s = rdb[i:j]
    if s == b"SquadId":
        found += 1
        print("SquadId at blob off", hex(i), "va", hex(rd['va']+i))
    if 1 <= len(s) <= 64 and IDENT.match(s):
        cnt += 1
    i = j+1
print("ident count", cnt, "SquadId occurrences", found)
