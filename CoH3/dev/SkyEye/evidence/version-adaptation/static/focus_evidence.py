"""Focused static evidence, no process APIs; output restricted to this directory."""
from pathlib import Path
import json, struct, bisect, re
import pefile
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
from capstone.x86_const import X86_OP_MEM,X86_REG_RIP
OUT=Path(__file__).resolve().parent
p=json.loads((OUT/'pe-candidates.json').read_text(encoding='utf-8'))
b=Path(p['target']).read_bytes(); pe=pefile.PE(data=b,fast_load=True); base=pe.OPTIONAL_HEADER.ImageBase
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True
pd=next(s for s in pe.sections if s.Name.startswith(b'.pdata'))
funcs=[struct.unpack_from('<III',b,o) for o in range(pd.PointerToRawData,pd.PointerToRawData+pd.SizeOfRawData-11,12)]
funcs=sorted(f for f in funcs if f[0] and f[1]>f[0]); starts=[f[0] for f in funcs]
def off(rv):return pe.get_offset_from_rva(rv)
def enclosing(rv):
 i=bisect.bisect_right(starts,rv)-1
 return funcs[i] if i>=0 and funcs[i][0]<=rv<funcs[i][1] else None
def dis(rv,n):
 return [{'rva':hex(i.address-base),'bytes':i.bytes.hex(' '),'asm':i.mnemonic+' '+i.op_str} for i in md.disasm(b[off(rv):off(rv)+n],base+rv)]
def signature(rv,n=48):
 raw=b[off(rv):off(rv)+n];mask=bytearray(b'\xff'*len(raw)); size=0
 for i in md.disasm(raw,base+rv):
  pos=i.address-base-rv;size=pos+i.size
  if i.mnemonic=='ret':break
  if any(o.type==X86_OP_MEM and o.mem.base==X86_REG_RIP for o in i.operands):mask[pos+i.disp_offset:pos+i.disp_offset+i.disp_size]=b'\0'*i.disp_size
  if i.mnemonic.startswith('j') or i.mnemonic=='call':
   if i.imm_size:mask[pos+i.imm_offset:pos+i.imm_offset+i.imm_size]=b'\0'*i.imm_size
 raw=raw[:size];mask=mask[:size]
 pattern=b''.join(re.escape(bytes([v])) if m else b'.' for v,m in zip(raw,mask));hits=[]
 for s in pe.sections:
  if s.Characteristics&0x20000000:
   for m in re.finditer(pattern,b[s.PointerToRawData:s.PointerToRawData+s.SizeOfRawData],re.S):hits.append(hex(s.VirtualAddress+m.start()))
 return {'rva':hex(rv),'length':size,'signature':' '.join(f'{v:02X}' if m else '??' for v,m in zip(raw,mask)),'executable_raw_hits':hits,'note':'Counts raw byte matches, not runtime identity proof; wildcard only RIP displacement and direct branch immediates.'}
focus=[0x930c32,0x930cb0,0x2727fe,0x272880,0x17932a,0x113139,0x189e43]
windows=[]
for rv in focus:
 f=enclosing(rv);start=f[0] if f and f[1]-f[0]<0x600 else rv
 windows.append({'requested_rva':hex(rv),'pdata_bounds':[hex(v) for v in f] if f else None,'start_rva':hex(start),'instructions':dis(start,min(f[1]-start,0x600) if f else 160)})
# Exact NUL-delimited resource literals; explicitly separate suffix matches.
strings=[]
for s in p['resource_strings']:
 o=int(s['file_offset'],16); st=o
 while st>0 and b[st-1]!=0 and o-st<200:st-=1
 strings.append(dict(s,full_string_start=hex(pe.get_rva_from_offset(st)),full_string=b[st:b.index(0,o)].decode('ascii','replace'),nul_boundary=(st==o)))
# Class-name candidate list, not old-to-new correspondence proof.
names=['Ability','AlwaysOnAbility','Entity','Squad','Player','BufferedPlayer','CameraEntity@Camera','CameraSwitchBoard@Camera','WorldCameraInterface@WPFGUI','Identity','TransformComponent','TacticalMapControl@WPFGUI','StrategicMapControl@WPFGUI','MatrixTransform@AVUI','TransformGroup@AVUI','TranslateTransform@AVUI','RotateTransform@AVUI','ScaleTransform@AVUI','Grid@AVUI','Canvas@AVUI','MapControl@WPFGUI','AnvilRTSHUDPage@WPFGUI','GameUICore@UI@Relic','TeamColourManager@UI@Relic','EntityWeaponExt','WeaponExt','Weapon','Projectile','ProjectileExt','SimEntity','Identity@Camera','GameWorld','SimWorld']
vt=[t for t in p['rtti'] if t['name'] in ['.?AV'+n+'@@' for n in names]]
for t in vt:
 for v in t['vtables']:
  rv=int(v['rva'],16);v['first_function_pdata']=enclosing(int(v['functions'][0],16));v['first_function_bytes']=dis(int(v['functions'][0],16),32)
report={'source':p['target'],'sha256':p['sha256'],'windows':windows,'signatures':[signature(v) for v in [0x930c32,0x2727fe,0x272880,0x17932a]],'strings':strings,'selected_rtti':vt,'old_dependencies':p['old_dependencies']}
(OUT/'focused-evidence.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
for w in windows:
 print('WINDOW',w['requested_rva'],w['pdata_bounds']);[print(i['rva'],i['asm']) for i in w['instructions'][:30]]
print('SIGNATURES',json.dumps(report['signatures']));print('RTTI',[(t['name'],t['object_offset'],[v['rva'] for v in t['vtables']]) for t in vt])
