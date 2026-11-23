#!/usr/bin/env python3
import sys
sys.path.insert(0, r'F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis')
from p2_common import Img
img = Img()
data = img.data
for rva in (0x8396d80, 0x839e740, 0x82fcae0, 0x8396e00):
    off = img.rva2off(rva)
    print(f"--- rva {hex(rva)} file {hex(off)}")
    b = data[off:off+0x100]
    for i in range(0, 0x100, 16):
        print(f"  {rva+i:08x}  " + " ".join(f"{x:02x}" for x in b[i:i+16]))
