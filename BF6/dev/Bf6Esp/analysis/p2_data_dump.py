#!/usr/bin/env python3
import re, sys
sys.path.insert(0, r'F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis')
from p2_common import Img, IMAGE_BASE
img = Img(); data = img.data
IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}\Z")
rd = img.sec['.rdata']
rdb = data[rd['roff']:rd['roff']+rd['rsize']]
base = IMAGE_BASE + rd['va']
namemap = {}
i = 0
while True:
    j = rdb.find(b"\x00", i)
    if j < 0: break
    s = rdb[i:j]
    if 1 <= len(s) <= 64 and IDENT.match(s):
        namemap[base+i] = s
    i = j+1

def dump(rva, nbytes=0x100, label=""):
    off = img.rva2off(rva)
    print(f"\n--- {label} rva {hex(rva)} file {hex(off)}")
    for k in range(0, nbytes, 16):
        row = data[off+k:off+k+16]
        print(f"  {rva+k:08x}  " + " ".join(f"{x:02x}" for x in row))
    # qword view
    print("  qwords:")
    for k in range(0, min(nbytes, 0x100), 8):
        q = int.from_bytes(data[off+k:off+k+8], "little")
        nm = namemap.get(q)
        extra = ""
        if nm: extra = "  -> " + nm.decode("latin1")
        elif 0x140000000 <= q < 0x140000000+0x2000000:
            extra = "  (image ptr)"
        print(f"    +{k:04x} {hex(q)}{extra}")

dump(0x9AF48C8, 0x100, "src array")
dump(0x9AF61E8, 0x100, "typeinfo q3 target")
dump(0x9AD0EC0, 0x80, "table B head")
