#!/usr/bin/env python3
"""Read-only CoH3 console diagnostics. Never accesses a running process.
Derived mini-PEs retain original RVAs and are written only as console_diag_*.
"""
import bisect
import hashlib
import re
import struct
import sys
from pathlib import Path
import capstone
import numpy as np
import pefile

EXE = Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
OUT = Path(__file__).resolve().parent
raw = EXE.read_bytes()
pe = pefile.PE(data=raw, fast_load=True)
base = pe.OPTIONAL_HEADER.ImageBase
secs = [(s.Name.rstrip(b'\0').decode(), s.VirtualAddress, s.PointerToRawData,
         s.SizeOfRawData, s.Misc_VirtualSize) for s in pe.sections]
exc = pe.OPTIONAL_HEADER.DATA_DIRECTORY[pefile.DIRECTORY_ENTRY['IMAGE_DIRECTORY_ENTRY_EXCEPTION']]
exc_off = next(ro + exc.VirtualAddress-va for nm,va,ro,rs,vs in secs if va <= exc.VirtualAddress < va+rs)
funcs = sorted(x for x in struct.iter_unpack('<III',raw[exc_off:exc_off+exc.Size]) if x[0] and x[1]>x[0])
starts = [x[0] for x in funcs]
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
md.detail = True

def off(rva):
    for nm, va, ro, rs, vs in secs:
        if va <= rva < va + rs:
            return ro + rva - va
    return None

def rva(fo):
    for nm, va, ro, rs, vs in secs:
        if ro <= fo < ro + rs:
            return va + fo - ro
    return None

def func(a):
    i = bisect.bisect_right(starts, a)-1
    return funcs[i] if i >= 0 and funcs[i][0] <= a < funcs[i][1] else None

def hx(a): return '0x%08X' % a

def dis(a, span=None):
    f = func(a)
    if span is None:
        if not f:
            span = 0x100
        else:
            a, end, uw = f
            span = end-a
    fo = off(a)
    print('DIS RVA=%s file=%s span=%s pdata=%s' % (hx(a), hx(fo), hx(span), str(f)))
    for ins in md.disasm(raw[fo:fo+span], base+a):
        note = ''
        for op in ins.operands:
            if op.type == capstone.x86.X86_OP_MEM and op.mem.base == capstone.x86.X86_REG_RIP:
                t = ins.address + ins.size + op.mem.disp - base
                note += ' => RVA='+hx(t)
                o = off(t)
                if o is not None:
                    bb = raw[o:o+100].split(b'\0',1)[0]
                    if bb and all(32 <= c < 127 for c in bb): note += ' '+repr(bb)
        print('%s file=%s %-32s %-8s %s%s' % (hx(ins.address-base), hx(off(ins.address-base)), ins.bytes.hex(' '), ins.mnemonic, ins.op_str, note))

def strings(pattern):
    rx = re.compile(pattern, re.I)
    for m in re.finditer(rb'[\x20-\x7e]{4,}', raw):
        s = m.group().decode('ascii')
        if rx.search(s):
            print('STR file=%s rva=%s %s' % (hx(m.start()), hx(rva(m.start()) or 0), s[:600]))

def refs(targets, tails=(0,1,4)):
    wanted = set(targets)
    text = next(s for s in secs if s[0] == '.text')
    _, va, ro, rs, vs = text
    blob = memoryview(raw)[ro:ro+rs]
    seen = set()
    for lo in range(0,rs-4,0x100000):
        hi = min(rs-4,lo+0x100000)
        disp = np.ndarray((hi-lo,),dtype='<i4',buffer=blob,offset=lo,strides=(1,))
        total = np.arange(va+lo+4,va+hi+4,dtype=np.int64)
        total += disp
        for tail in tails:
            inds = np.flatnonzero(np.isin(total+tail, list(wanted)))
            for ind in inds:
                k = lo+int(ind)
                target = int(total[ind])+tail
                for st in range(max(0,k-11),k+1):
                    ins = next(md.disasm(bytes(blob[st:st+15]),base+va+st,count=1),None)
                    if not ins: continue
                    valid = False
                    for op in ins.operands:
                        if op.type == capstone.x86.X86_OP_MEM and op.mem.base == capstone.x86.X86_REG_RIP:
                            valid |= ins.address+ins.size+op.mem.disp == base+target
                        if op.type == capstone.x86.X86_OP_IMM and (ins.group(capstone.CS_GRP_CALL) or ins.group(capstone.CS_GRP_JUMP)):
                            valid |= op.imm == base+target
                    key=(ins.address,target)
                    if valid and key not in seen:
                        seen.add(key)
                        a=ins.address-base
                        print('REF target=%s ins=%s file=%s fn=%s %s %s %s' % (hx(target),hx(a),hx(off(a)),str(func(a)),ins.bytes.hex(' '),ins.mnemonic,ins.op_str))
    print('TOTAL verified refs',len(seen))

def find(needles):
    targets=[]
    for s in needles:
        nd=s.encode()+b'\0'
        st=0
        while True:
            fo=raw.find(nd,st)
            if fo<0: break
            st=fo+1
            a=rva(fo)
            print('FIND',repr(s),'file='+hx(fo),'rva='+hx(a or 0))
            if a is not None: targets.append(a)
    if targets: refs(targets)

def imms(targets):
    wanted=set(targets)
    text=next(s for s in secs if s[0]=='.text')
    _,va,ro,rs,vs=text
    blob=raw[ro:ro+rs]
    seen=set()
    for target in targets:
        pat=struct.pack('<I',target & 0xffffffff)
        pos=0
        while True:
            at=blob.find(pat,pos)
            if at<0: break
            pos=at+1
            for st in range(max(0,at-11),at+1):
                ins=next(md.disasm(blob[st:st+15],base+va+st,count=1),None)
                if not ins: continue
                if any(op.type==capstone.x86.X86_OP_IMM and op.imm==target for op in ins.operands):
                    key=(ins.address,target)
                    if key in seen: continue
                    seen.add(key)
                    a=ins.address-base
                    print('IMM target=%s ins=%s file=%s fn=%s %s %s %s'%(hx(target),hx(a),hx(off(a)),str(func(a)),ins.bytes.hex(' '),ins.mnemonic,ins.op_str))
    print('TOTAL immediate candidates',len(seen))

def ptrs(targets):
    for t in targets:
        pat=struct.pack('<Q',base+t)
        st=0
        while True:
            fo=raw.find(pat,st)
            if fo<0:break
            st=fo+1
            print('PTR target=%s file=%s rva=%s' % (hx(t),hx(fo),hx(rva(fo) or 0)))

def mini(name, intervals):
    from make_mini_pe import build_pe
    pages={}
    def source_section(a):
        return next((s for s in pe.sections if s.VirtualAddress <= a < s.VirtualAddress + max(s.Misc_VirtualSize, s.SizeOfRawData)), None)
    def add(a,b,chars):
        for p in range(a & ~0xfff,(b+0xfff)&~0xfff,0x1000):
            if source_section(p) is not None:
                pages[p]=pages.get(p,0)|chars
    for a,b in intervals:
        add(a,b,0x60000020)
        fo=off(a)
        for ins in md.disasm(raw[fo:fo+b-a],base+a):
            for op in ins.operands:
                if op.type == capstone.x86.X86_OP_MEM and op.mem.base == capstone.x86.X86_REG_RIP:
                    t=ins.address+ins.size+op.mem.disp-base
                    section=source_section(t)
                    if section is not None: add(t,t+1,int(section.Characteristics))
    runs=[]
    for p,c in sorted(pages.items()):
        if runs and runs[-1][1] == p and runs[-1][2] == c:runs[-1][1]=p+0x1000
        else:runs.append([p,p+0x1000,c])
    sections=[]
    for i,(a,b,c) in enumerate(runs):
        bb=bytearray()
        for p in range(a,b,0x1000):
            fo=off(p)
            bb+=(raw[fo:fo+0x1000].ljust(0x1000,b'\0') if fo is not None else bytes(0x1000))
        sections.append(('.c%d'%i if c & 0x20 else '.d%d'%i,a,bytes(bb),c))
    path=OUT/('console_diag_'+name+'.exe')
    if path.exists(): raise RuntimeError('Refusing to overwrite existing derived artifact: '+str(path))
    path.write_bytes(build_pe(sections,intervals[0][0]))
    print('MINI',path,'sections',runs,'sha256',hashlib.sha256(path.read_bytes()).hexdigest())

if __name__=='__main__':
    cmd,*args=sys.argv[1:]
    if cmd=='identity':
        print('size',len(raw),'sha256',hashlib.sha256(raw).hexdigest(),'imagebase',hex(base),'functions',len(funcs))
        for s in secs:print(s)
    elif cmd=='strings':strings(args[0])
    elif cmd=='find':find(args)
    elif cmd=='refs':refs([int(a,16) for a in args])
    elif cmd=='calls':refs([int(a,16) for a in args],tails=(0,))
    elif cmd=='dis':dis(int(args[0],16), int(args[1],16) if len(args)>1 else None)
    elif cmd=='func':
        for a in args:print(a,tuple(hx(v) for v in func(int(a,16))) if func(int(a,16)) else None)
    elif cmd=='data':
        a=int(args[0],16); n=int(args[1],16) if len(args)>1 else 0x80
        for k in range(0,n,8):
            bb=raw[off(a)+k:off(a)+k+8]
            print(hx(a+k),bb.hex(' '),'Q='+hex(int.from_bytes(bb,'little')),repr(bb))
    elif cmd=='imms':imms([int(a,16) for a in args])
    elif cmd=='ptrs':ptrs([int(a,16) for a in args])
    elif cmd=='mini':mini(args[0],[tuple(int(v,16) for v in a.split(':')) for a in args[1:]])
    else:raise SystemExit('unknown command '+cmd)
