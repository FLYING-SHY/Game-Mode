"""Read-only PE-file investigation; outputs only next to this script. No process APIs."""
from pathlib import Path
import struct, re, json, hashlib, sys
import pefile
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
TARGET=Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
OUT=Path(__file__).resolve().parent
b=TARGET.read_bytes(); pe=pefile.PE(data=b,fast_load=True); base=pe.OPTIONAL_HEADER.ImageBase
md=Cs(CS_ARCH_X86,CS_MODE_64)
def rva(off): return pe.get_rva_from_offset(off)
def offset(rv):
    # pefile may return a numerical offset for virtual-only .data; never treat it as file-backed bytes.
    for s in pe.sections:
        if s.VirtualAddress <= rv < s.VirtualAddress+s.SizeOfRawData:
            return s.PointerToRawData+rv-s.VirtualAddress
    if 0 <= rv < pe.OPTIONAL_HEADER.SizeOfHeaders: return rv
    return None
def hx(x): return hex(x)
def bytes_at(rv,n=32):
    o=offset(rv)
    return b[o:o+n].hex(' ') if o is not None else None
def allfind(needle):
    start=0
    while True:
        p=b.find(needle,start)
        if p<0: return
        yield p; start=p+1
sections=[{'name':s.Name.rstrip(b'\0').decode(),'rva':hx(s.VirtualAddress),'virtual_size':hx(s.Misc_VirtualSize),'raw':hx(s.PointerToRawData),'raw_size':hx(s.SizeOfRawData),'flags':hx(s.Characteristics)} for s in pe.sections]
# MSVC x64 RTTI: TypeDescriptor name at +16; COL is six uint32, signature=1, pSelf=current RVA.
import numpy as np
# Index aligned COLs and pointer references once (avoids one full-file search per RTTI type).
cols_by_td={}; pointer_refs={}
for sec in pe.sections:
    raw=b[sec.PointerToRawData:sec.PointerToRawData+sec.SizeOfRawData]
    if len(raw)<24: continue
    words=np.frombuffer(raw[:len(raw)//4*4],dtype='<u4')
    ii=np.arange(len(words)-5,dtype=np.uint64)
    candidates=np.where((words[:-5]==1) & (words[5:]==sec.VirtualAddress+ii*4))[0]
    for idx in candidates:
        of=sec.PointerToRawData+int(idx)*4
        td=struct.unpack_from('<I',b,of+12)[0]
        cols_by_td.setdefault(td,[]).append(of+12)
    if sec.Characteristics & 0x20000000: continue
    qs=np.frombuffer(raw[:len(raw)//8*8],dtype='<u8')
    idxs=np.where((qs>=base)&(qs<base+pe.OPTIONAL_HEADER.SizeOfImage))[0]
    for idx in idxs:
        pointer_refs.setdefault(int(qs[idx]),[]).append(sec.PointerToRawData+int(idx)*8)
types=[]; tables=[]
for m in re.finditer(rb'\.\?A[UV][^\x00]{1,400}\x00',b):
    td=rva(m.start())-16; name=m.group()[:-1].decode('ascii','replace')
    for hit in cols_by_td.get(td,[]):
        coloff=hit-12
        if coloff<0: continue
        vals=struct.unpack_from('<6I',b,coloff)
        try: col=rva(coloff)
        except: continue
        if vals[0]!=1 or vals[5]!=col: continue
        row={'name':name,'type_descriptor':hx(td),'col':hx(col),'object_offset':vals[1],'cd_offset':vals[2],'class_descriptor':hx(vals[4]),'vtables':[]}
        for ptr in pointer_refs.get(base+col,[]):
            vt=rva(ptr)+8
            funcs=struct.unpack_from('<8Q',b,ptr+8)
            fr=[v-base for v in funcs]
            if not all(0<=v<pe.OPTIONAL_HEADER.SizeOfImage for v in fr[:2]): continue
            code_prefix=[]
            for v in fr:
                if not any(s.Characteristics & 0x20000000 and s.VirtualAddress<=v<s.VirtualAddress+s.Misc_VirtualSize for s in pe.sections): break
                code_prefix.append(v)
            entry={'rva':hx(vt),'functions':[hx(v) for v in code_prefix],'bytes':bytes_at(vt,64),'function_sample_note':'At most first eight contiguous executable-section pointers; bytes may extend beyond vtable; not a complete table-length claim.'}
            row['vtables'].append(entry); tables.append(dict(name=name,object_offset=vals[1],**entry))
        types.append(row)
# Inventory every hard-coded image-relative dependency and large switch case in this source.
src=Path(r'F:\WorkSpace\Game\CoH3\dev\SkyEye\client\CoH3MemoryTool\PlayerInfoReader.cs').read_text(encoding='utf-8-sig')
deps={}
for i,line in enumerate(src.splitlines(),1):
    for pat in [r'image \+ (\d+)',r'case (\d{7,})L:',r'MiniPropertyId\((\d+)L\)',r', (103\d{6})L\)',r'\? (645\d+) :',r': (645\d+)\)',r'num\d* == (64\d{6}|65\d{6})']:
        for m in re.finditer(pat,line): deps.setdefault(int(m.group(1)),[]).append({'line':i,'source':line.strip()})
old=[{'rva':hx(v),'decimal':v,'uses':uses,'new_file_same_rva_bytes':bytes_at(v)} for v,uses in sorted(deps.items()) if v != 268435456]
# Candidate RIP-relative references, byte-prefiltered then individually decoded at exact opcode.
roots=[112926416,102268480,98623180]
windows=[(v-0x800,v+0x1000) for v in roots]
# Follow-up UI singleton/registry/property-id windows: proximity only selects references, not mappings.
windows += [(v-0x4000,v+0x6000) for v in [112837064,102261280,111878264,111989640,111989680]]
strings=[]
for needle in [b'manpower\0',b'munition\0',b'fuel\0',b'command\0',b'shared_ability_1\0',b'shared_barrage_ability\0']:
    for pos in allfind(needle): strings.append({'text':needle[:-1].decode(),'rva':hx(rva(pos)),'file_offset':hx(pos)})
stringrvas={int(s['rva'],16) for s in strings}
refs=[]
for s in pe.sections:
    if not s.Characteristics & 0x20000000: continue
    data=b[s.PointerToRawData:s.PointerToRawData+s.SizeOfRawData]
    for m in re.finditer(rb'[\x40-\x4f]?[\x8b\x89\x8d\x3b\x39\x63][\x05\x0d\x15\x1d\x25\x2d\x35\x3d]',data):
        p=m.start(); rv=s.VirtualAddress+p; size=len(m.group())+4
        if p+size>len(data): continue
        target=rv+size+struct.unpack_from('<i',data,p+size-4)[0]
        if target not in stringrvas and not any(lo<=target<hi for lo,hi in windows): continue
        ins=next(md.disasm(data[p:p+15],base+rv,count=1),None)
        if ins is None or ins.size!=size: continue
        sig=data[p:p+size-4].hex(' ')+' ?? ?? ?? ??'
        refs.append({'rva':hx(rv),'target':hx(target),'instruction':ins.mnemonic+' '+ins.op_str,'bytes':data[p:p+size].hex(' '),'signature_prefix':sig,'following_bytes':data[p:p+40].hex(' '),'disassembly':[f'{i.address-base:#x}: {i.mnemonic} {i.op_str}' for i in md.disasm(data[p:p+40],base+rv)]})
report={'target':str(TARGET),'sha256':hashlib.sha256(b).hexdigest(),'length':len(b),'machine':hx(pe.FILE_HEADER.Machine),'image_base':hx(base),'size_of_image':hx(pe.OPTIONAL_HEADER.SizeOfImage),'sections':sections,'old_dependencies':old,'resource_strings':strings,'rtti':types,'rip_references':refs,'limitations':['Static only; no process opened or game launched.','Reference prefilter covers REX + common RIP-relative 7-byte MOV/LEA/CMP/MOVSXD only, not complete disassembly.','RTTI vtable function RVAs are file identities, not proof of runtime object layout.','No old supported executable available in scoped workspace discovery.']}
(OUT/'pe-candidates.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
interesting=[t for t in tables if re.search('Player|Camera|MiniMap|Minimap|Tactical|Resource|World|Arena|Transform|Ability|Screen',t['name'],re.I)]
(OUT/'rtti-priority.json').write_text(json.dumps(interesting,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'sha256':report['sha256'],'sections':sections,'rtti_records':len(types),'vtables':len(tables),'priority_tables':len(interesting),'references':len(refs),'old_dependencies':len(old),'root_exact_reference_counts':{hex(root):sum(int(r['target'],16)==root for r in refs) for root in roots}},indent=2))
