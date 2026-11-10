#!/usr/bin/env python3
"""Disk-only x64 memory displacement finder, built on the independent static helper."""
import struct
import sys
import capstone
import console_diag_static as s

text = next(sec for sec in s.secs if sec[0] == '.text')
_, va, ro, rs, vs = text
blob = s.raw[ro:ro+rs]
verified = {}

for word in sys.argv[1:]:
    value = int(word, 16)
    needle = struct.pack('<I', value)
    pos = 0
    seen = set()
    while True:
        at = blob.find(needle, pos)
        if at < 0:
            break
        pos = at+1
        for start in range(max(0, at-11), at+1):
            addr = va+start
            fn = s.func(addr)
            if fn:
                if fn[0] not in verified:
                    fo = s.off(fn[0])
                    verified[fn[0]] = {ins.address-s.base: ins for ins in s.md.disasm(s.raw[fo:fo+fn[1]-fn[0]], s.base+fn[0])}
                ins = verified[fn[0]].get(addr)
                if ins is None:
                    continue
                confidence = 'boundary'
            else:
                ins = next(s.md.disasm(blob[start:start+15], s.base+addr, count=1), None)
                confidence = 'candidate'
            if ins is None or addr in seen:
                continue
            if not any(op.type == capstone.x86.X86_OP_MEM and op.mem.disp == value for op in ins.operands):
                continue
            seen.add(addr)
            print('DISP', hex(value), confidence, s.hx(addr), 'file='+s.hx(s.off(addr)), 'fn='+str(tuple(s.hx(x) for x in fn) if fn else None), ins.bytes.hex(' '), ins.mnemonic, ins.op_str)
    print('TOTAL', hex(value), len(seen))
