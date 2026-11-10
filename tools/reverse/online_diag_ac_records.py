"""Read-only RTTI/type/call notes for AC record-producing methods."""
from pathlib import Path
import hashlib
import json
import struct
import capstone
from capstone.x86_const import X86_OP_IMM, X86_OP_MEM, X86_REG_RIP
import pefile

ROOT=Path(__file__).resolve().parent
source=Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
raw=source.read_bytes()
assert hashlib.sha256(raw).hexdigest()=='21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca'
pe=pefile.PE(data=raw,fast_load=True)
base=pe.OPTIONAL_HEADER.ImageBase
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);md.detail=True

def data(rva,n): return pe.get_data(rva,n)
def text(rva): return data(rva,240).split(b'\0',1)[0].decode('ascii',errors='replace')
def type_at(vt):
    try:
        col=struct.unpack('<Q',data(vt-8,8))[0]-base
        sig,adj,cd,td,chd,selfr=struct.unpack('<6I',data(col,24))
        if sig!=1 or selfr!=col:return None
        return {'vtable_rva':hex(vt),'col_rva':hex(col),'type_descriptor_rva':hex(td),
                'name':text(td+16),'this_adjustment':adj}
    except (ValueError,struct.error):return None

result={'record_type':type_at(0x3DF1BA0),'methods':[]}
for a,z in ((0xCEEE30,0xCEF053),(0x374B890,0x374BAB7),(0x374BAB8,0x374BF55)):
    calls=[];refs=[];types=[];strings=[]
    for ins in md.disasm(data(a,z-a),base+a):
        if ins.mnemonic=='call' and ins.operands[0].type==X86_OP_IMM:
            calls.append({'call_rva':hex(ins.address-base),'target_rva':hex(ins.operands[0].imm-base)})
        for op in ins.operands:
            if op.type==X86_OP_MEM and op.mem.base==X86_REG_RIP:
                target=ins.address+ins.size+op.mem.disp-base
                refs.append({'instruction_rva':hex(ins.address-base),'target_rva':hex(target)})
                t=type_at(target)
                if t and t not in types:types.append(t)
                s=text(target)
                if len(s)>4 and all(32<=ord(c)<127 for c in s):strings.append(s)
    result['methods'].append({'entry_rva':hex(a),'end_rva':hex(z),'calls':calls,'rip_targets':refs,
                              'referenced_types':types,'strings':sorted(set(strings))})
(ROOT/'online_diag_ac_records.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
