"""Bounded static consumer scan; original EXE read-only, output prefix fixed."""
from pathlib import Path
import bisect, hashlib, json, re, struct, sys
import capstone as cs
from capstone.x86 import X86_OP_MEM, X86_OP_IMM
import pefile, numpy as np
src=Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
root=Path(__file__).resolve().parent
raw=src.read_bytes(); pe=pefile.PE(data=raw,fast_load=True); base=pe.OPTIONAL_HEADER.ImageBase
sections={s.Name.rstrip(b'\0').decode():(s.VirtualAddress,s.PointerToRawData,s.SizeOfRawData) for s in pe.sections}
tva,tro,tsz=sections['.text']; text=raw[tro:tro+tsz]
pva,pro,psz=sections['.pdata']; ranges=[]
for off in range(pro,pro+psz-11,12):
 s,e,u=struct.unpack_from('<III',raw,off)
 if s and e:ranges.append((s,e,u))
ranges.sort(); starts=[x[0] for x in ranges]
def bounds(r):
 i=bisect.bisect_right(starts,r)-1
 return ranges[i][:2] if i>=0 and r<ranges[i][1] else None
md=cs.Cs(cs.CS_ARCH_X86,cs.CS_MODE_64); md.detail=True
head=[f'source={src}',f'sha256={hashlib.sha256(raw).hexdigest()}',f'base={base:X}',f'text_RVA={tva:X} raw={tro:X} size={tsz:X}']
def fmt(i):return f'{i.address-base:08X} raw={pe.get_offset_from_rva(i.address-base):08X} {i.bytes.hex(" "):<34} {i.mnemonic} {i.op_str}'
def dump(r,n):
 o=pe.get_offset_from_rva(r)
 return list(md.disasm(raw[o:o+n],base+r))
def line_dump(r,n):return '\n'.join(fmt(i) for i in dump(r,n))
mode=sys.argv[1] if len(sys.argv)>1 else 'scan'; out=head+[f'mode={mode}']
if mode=='scan':
 # Byte prefilter supports cmp/mov/movsx/movzx, REX/66, disp8/disp32, SIB.
 pat=re.compile(rb'\x66?[\x40-\x4f]?(?:\x0f[\xb6\xb7\xbe\xbf]|[\x38-\x3b]|\x8a|\x8b|\x80|\x81|\x83)[\x40-\xbf](?:[\x00-\xff])?[\x69\x6a]')
 hits={}
 for m in pat.finditer(text):
  for off in range(max(0,m.start()-2),m.start()+5):
   i=next(md.disasm(text[off:off+15],base+tva+off,count=1),None)
   if not i or i.mnemonic not in ('cmp','mov','movsx','movzx'):continue
   for op in i.operands:
    if op.type==X86_OP_MEM and op.mem.disp in (0x69,0x6a) and op.mem.base and not op.mem.index and ((op.mem.disp==0x69 and op.size==1) or(op.mem.disp==0x6a and op.size==2)):
     if i.mnemonic=='mov' and i.operands[0].type==X86_OP_MEM:continue
     hits[(i.address-base,op.mem.disp,md.reg_name(op.mem.base))]=i
 # Per-byte prefix duplicates are retained temporarily, then pdata alignment validates.
 hs=sorted(hits); pairs=[]; seen=set()
 for r,d,b in hs:
  if d!=0x6a:continue
  for s,e,c in hs:
   if e!=0x69 or b!=c or abs(r-s)>48:continue
   f=bounds(r); g=bounds(s)
   if f!=g:continue
   aligned=False
   if f:
    ins=dump(f[0],f[1]-f[0]); addrs={i.address-base for i in ins}
    aligned=r in addrs and s in addrs
   else: aligned=None
   key=(f,r,s,b)
   if key in seen:continue
   seen.add(key); pairs.append({'word':r,'byte':s,'base_reg':b,'pdata':f,'aligned':aligned})
 out.append(f'raw_qualified_reads={len(hits)} pairs={len(pairs)}')
 for p in pairs:
  f=p['pdata']; r=p['word'];s=p['byte'];b=p['base_reg']
  out.append(f'\nPAIR word={r:X} byte={s:X} reg={b} pdata={None if f is None else [hex(x) for x in f]} aligned={p["aligned"]}')
  out.extend(fmt(hits[(a,d,b)]) for a,d in [(r,0x6a),(s,0x69)])
  if p['aligned'] is True:
   ins=dump(f[0],f[1]-f[0]); window=[i for i in ins if min(r,s)-40<=i.address-base<=max(r,s)+64]
   out.extend(fmt(i) for i in window)
 (root/'render_diag_cull_candidates.json').write_text(json.dumps({'source':str(src),'sha256':hashlib.sha256(raw).hexdigest(),'pairs':pairs},indent=2),encoding='utf-8')
elif mode=='dump':
 for a in sys.argv[2:]:
  parts=a.split(':');r=int(parts[0],16);f=bounds(r); n=int(parts[1],16) if len(parts)>1 else(f[1]-r if f else 0x100)
  out.extend([f'\nDUMP {r:08X} span={n:X} pdata={None if f is None else [hex(x) for x in f]}',line_dump(r,n)])
elif mode=='xref':
 targets={int(a,16) for a in sys.argv[2:]}; idx=np.flatnonzero(np.isin(np.frombuffer(text,dtype=np.uint8),[0xe8,0xe9]))
 for x in idx:
  x=int(x)
  if x+5>len(text):continue
  target=tva+x+5+struct.unpack_from('<i',text,x+1)[0]
  if target in targets:out.append(f'target={target:X} {"call" if text[x]==0xe8 else "jmp"} {tva+x:X} pdata={None if bounds(tva+x) is None else [hex(y) for y in bounds(tva+x)]} bytes={text[x:x+5].hex()}')
elif mode=='ptr':
 for a in sys.argv[2:]:
  target=int(a,16); needle=struct.pack('<Q',base+target); at=raw.find(needle);out.append(f'\nTARGET {target:X}')
  while at>=0:
   out.append(f'absolute_pointer={pe.get_rva_from_offset(at):X}')
   for back in range(0,0x400,8):
    o=at-back-8
    try:
     col=struct.unpack_from('<Q',raw,o)[0]-base; co=pe.get_offset_from_rva(col)
     sig,delta,cd,td,chd,selfr=struct.unpack_from('<IIIIII',raw,co)
     if sig==1 and selfr==col:
      no=pe.get_offset_from_rva(td)+16;end=raw.index(b'\0',no)
      out.append(f'vftable={pe.get_rva_from_offset(o+8):X} slot={back:X} COL={col:X} TD={td:X} offset={delta:X} name={raw[no:end].decode(errors="replace")}')
      out.append(f'raw_slot={raw[at:at+8].hex()} raw_COL={raw[co:co+24].hex()} raw_TD_name={raw[no-16:end+1].hex()}');break
    except Exception:pass
   at=raw.find(needle,at+1)
suffix=('_'+sys.argv[2].replace(':','_')) if len(sys.argv)>2 else ''
path=root/f'render_diag_cull_{mode}{suffix}.txt';path.write_text('\n'.join(out)+'\n',encoding='utf-8');print('\n'.join(out));print(f'OUTPUT={path}')
