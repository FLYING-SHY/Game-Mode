"""Read-only AntiCheatWorldService inventory for the exact investigated PE.
No process handles, patching, input, network operations or game execution.
"""
from pathlib import Path
import bisect
import hashlib
import json
import struct
import capstone
from capstone.x86_const import X86_OP_IMM, X86_OP_MEM, X86_REG_RIP
import numpy as np
import pefile

SOURCE = Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
OUT = Path(__file__).resolve().parent
raw = SOURCE.read_bytes()
sha = hashlib.sha256(raw).hexdigest()
assert sha == '21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca', sha
pe = pefile.PE(data=raw, fast_load=True)
base = pe.OPTIONAL_HEADER.ImageBase
sections = [(s.Name.rstrip(b'\0').decode(), s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData, s.Misc_VirtualSize) for s in pe.sections]

def off(rva):
    for name, va, po, sz, vs in sections:
        if va <= rva < va + sz:
            return po + rva - va
    raise ValueError(hex(rva))

def rva_at(fileoff):
    for name, va, po, sz, vs in sections:
        if po <= fileoff < po + sz:
            return va + fileoff - po
    raise ValueError(hex(fileoff))

def bytes_at(rva, size):
    p = off(rva)
    return raw[p:p+size]

text = next(s for s in sections if s[0] == '.text')
_, tva, tpo, tsz, _ = text

def is_code(rva):
    return tva <= rva < tva + tsz

pdata = next(s for s in sections if s[0] == '.pdata')
ranges = [v for v in struct.iter_unpack('<III', raw[pdata[2]:pdata[2]+pdata[3]//12*12]) if v[0] and v[1] > v[0]]
ranges.sort()
starts = [v[0] for v in ranges]

def function(rva):
    i = bisect.bisect_right(starts, rva) - 1
    if i >= 0 and ranges[i][0] <= rva < ranges[i][1]:
        return ranges[i]
    # Tiny leaf methods do not need an exception-directory entry.
    p = off(rva)
    md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
    for ins in md.disasm(raw[p:p+128], base+rva):
        if ins.mnemonic in ('ret', 'retf'):
            return rva, ins.address-base+ins.size, None
        if ins.mnemonic == 'jmp':
            return rva, ins.address-base+ins.size, None
    return rva, rva+128, None

name = b'.?AVAntiCheatWorldService@@\0'
name_off = raw.index(name)
td = rva_at(name_off-16)
cols = []
i = raw.find(struct.pack('<I', td))
while i >= 0:
    if i >= 12:
        sig, adjust, cd, ptd, pcd, pself = struct.unpack_from('<6I', raw, i-12)
        try:
            col = rva_at(i-12)
            if sig == 1 and ptd == td and pself == col:
                cols.append((col, adjust))
        except ValueError:
            pass
    i = raw.find(struct.pack('<I', td), i+1)

md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
md.detail = True
vtables = []
methods = {}
for col, adjust in cols:
    needle = struct.pack('<Q', base+col)
    pos = raw.find(needle)
    while pos >= 0:
        vt = rva_at(pos+8)
        entries = []
        for slot in range(128):
            value = struct.unpack_from('<Q', raw, pos+8+slot*8)[0]
            r = value-base
            if not is_code(r):
                break
            entries.append({'slot':slot,'rva':hex(r)})
            if r not in methods:
                a,z,u = function(r)
                insns = list(md.disasm(bytes_at(a,z-a), base+a))
                calls, strings, refs = [], [], []
                for ins in insns:
                    if ins.mnemonic in ('call','jmp') and ins.operands and ins.operands[0].type == X86_OP_IMM:
                        calls.append(hex(ins.operands[0].imm-base))
                    for op in ins.operands:
                        if op.type == X86_OP_MEM and op.mem.base == X86_REG_RIP:
                            target = ins.address + ins.size + op.mem.disp - base
                            refs.append(hex(target))
                            try:
                                q = off(target)
                                s = raw[q:q+200].split(b'\0',1)[0]
                                if len(s)>=4 and all(32 <= c < 127 for c in s):
                                    strings.append(s.decode('ascii'))
                            except ValueError:
                                pass
                methods[r] = {'entry_rva':hex(r),'begin_rva':hex(a),'end_rva':hex(z),'size':z-a,'unwind_rva':None if u is None else hex(u), 'calls':sorted(set(calls)), 'rip_targets':sorted(set(refs)), 'strings':sorted(set(strings)), 'disassembly':['%08x  %-24s %s %s' % (ins.address-base,ins.bytes.hex(),ins.mnemonic,ins.op_str) for ins in insns]}
        # Find RIP-relative references to the vtable in code, then verify the instruction.
        references = []
        code = raw[tpo:tpo+tsz]
        for phase in range(4):
            n = (len(code)-phase)//4
            disp = np.frombuffer(code, dtype='<i4', count=n, offset=phase)
            next_rva = np.arange(n,dtype=np.int64)*4 + tva + phase + 4
            hits = np.flatnonzero(next_rva + disp == vt)
            for h in hits:
                field = int(h)*4 + phase
                for back in (2,3):
                    start = field-back
                    if start < 0:
                        continue
                    ins = next(md.disasm(code[start:start+16],base+tva+start), None)
                    if ins and ins.disp_offset == back and ins.disp_size == 4 and any(op.type==X86_OP_MEM and op.mem.base==X86_REG_RIP and ins.address+ins.size+op.mem.disp==base+vt for op in ins.operands):
                        a,z,u = function(ins.address-base)
                        references.append({'instruction_rva':hex(ins.address-base),'instruction':ins.mnemonic+' '+ins.op_str,'function_begin':hex(a),'function_end':hex(z)})
        vtables.append({'rva':hex(vt),'col_rva':hex(col),'this_adjustment':adjust,'entries':entries,'code_references':references})
        pos = raw.find(needle,pos+1)

result = {'source':str(SOURCE),'source_sha256':sha,'image_base':hex(base),'type_name_file_offset':hex(name_off),'type_descriptor_rva':hex(td),'vtables':vtables,'methods':list(methods.values()),'limitations':['Static bytes and candidate function boundaries only; no runtime or network behavior observed.','Vtable stops at first non-code pointer; shared leaf bodies may have multiple roles.','Direct-call and ASCII-string inventory is not a complete transitive behavior analysis.']}
(OUT/'online_diag_ac_inventory.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
lines = ['AntiCheatWorldService: source sha256='+sha,'TypeDescriptor='+hex(td)]
for vt in vtables:
    lines.append('vtable=%s slots=%d references=%s' % (vt['rva'],len(vt['entries']),vt['code_references']))
for m in methods.values():
    lines.append('\nMETHOD %s [%s,%s) size=%d calls=%s strings=%s' % (m['entry_rva'],m['begin_rva'],m['end_rva'],m['size'],m['calls'],m['strings']))
    lines.extend(m['disassembly'])
(OUT/'online_diag_ac_inventory.txt').write_text('\n'.join(lines),encoding='utf-8')
print('\n'.join(line for line in lines if line.startswith(('AntiCheatWorldService:', 'TypeDescriptor=', 'vtable=', '\nMETHOD'))))
print('Saved online_diag_ac_inventory.json and .txt; unique methods=',len(methods))
