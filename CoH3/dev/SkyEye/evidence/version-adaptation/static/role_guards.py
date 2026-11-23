"""Read-only PE role evidence. No process access; all output stays beside this script."""
from pathlib import Path
import json,struct,re,bisect,hashlib
import pefile
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
from capstone.x86_const import X86_OP_MEM,X86_REG_RIP
OUT=Path(__file__).resolve().parent
p=json.loads((OUT/'pe-candidates.json').read_text(encoding='utf-8'))
b=Path(p['target']).read_bytes();pe=pefile.PE(data=b,fast_load=True);base=pe.OPTIONAL_HEADER.ImageBase
assert hashlib.sha256(b).hexdigest().upper()=='8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC'
cs=Cs(CS_ARCH_X86,CS_MODE_64);cs.detail=True
def off(rv):return pe.get_offset_from_rva(rv)
def u32(rv):return struct.unpack_from('<I',b,off(rv))[0]
def q64(rv):return struct.unpack_from('<Q',b,off(rv))[0]
def text(rv):
 o=off(rv);return b[o:b.index(0,o)].decode('ascii','replace')
pd=next(s for s in pe.sections if s.Name.startswith(b'.pdata'))
funcs=sorted(struct.unpack_from('<III',b,o) for o in range(pd.PointerToRawData,pd.PointerToRawData+pd.SizeOfRawData-11,12))
starts=[x[0] for x in funcs]
def bounds(rv):
 i=bisect.bisect_right(starts,rv)-1
 return funcs[i] if i>=0 and funcs[i][0]<=rv<funcs[i][1] else None
execs=[s for s in pe.sections if s.Characteristics&0x20000000]
def executable(rv):return any(s.VirtualAddress<=rv<s.VirtualAddress+s.SizeOfRawData for s in execs)
def dis(rv,n=160):
 f=bounds(rv);n=min(n,f[1]-rv) if f else n
 return [{'rva':hex(i.address-base),'bytes':i.bytes.hex(' '),'asm':i.mnemonic+' '+i.op_str} for i in cs.disasm(b[off(rv):off(rv)+n],base+rv)]
def sig(rv,n=48):
 raw=b[off(rv):off(rv)+n];mask=bytearray(b'\xff'*len(raw));size=0
 for i in cs.disasm(raw,base+rv):
  pos=i.address-base-rv;size=pos+i.size
  if any(o.type==X86_OP_MEM and o.mem.base==X86_REG_RIP for o in i.operands):mask[pos+i.disp_offset:pos+i.disp_offset+i.disp_size]=b'\0'*i.disp_size
  if i.mnemonic.startswith('j') or i.mnemonic=='call':
   if i.imm_size:mask[pos+i.imm_offset:pos+i.imm_offset+i.imm_size]=b'\0'*i.imm_size
  if i.mnemonic=='ret':break
 raw=raw[:size];mask=mask[:size];pattern=b''.join(re.escape(bytes([v])) if m else b'.' for v,m in zip(raw,mask));hits=[]
 for s in execs:
  hits.extend(hex(s.VirtualAddress+m.start()) for m in re.finditer(pattern,b[s.PointerToRawData:s.PointerToRawData+s.SizeOfRawData],re.S))
 return {'rva':hex(rv),'bytes':raw.hex(' '),'sig':' '.join(f'{v:02X}' if m else '??' for v,m in zip(raw,mask)),'mask':''.join('x' if m else '?' for m in mask),'hits':hits,'hit_count':len(hits),'decode':'Masked RIP disp32: target=instruction RVA+instruction size+signed disp32; direct branch rel32/rel8 similarly. Retained field offsets are invariants to recheck, not version-independent guarantees.'}
names=['TimedAbility','AlwaysOnAbility','Ability','SpawnProductionItem','ProductionItem','EntityUpgradeProductionItem','SquadUpgradeProductionItem','PlayerUpgradeProductionItem','ProjectileExt','EntityDynamics','MoverDynamics','NavigationDynamics','ProjectileDynamics','BuildingDynamics','AttachmentDynamics']
classes=[dict(t) for t in p['rtti'] if t['name'] in ['.?AV'+n+'@@' for n in names] and t['object_offset']==0]
vtset={int(v['rva'],16) for t in classes for v in t['vtables']}
xrefs={v:[] for v in vtset}
# LEA/MOV RIP-relative static vtable references; exact decoder verification after cheap byte prefilter.
for s in execs:
 data=b[s.PointerToRawData:s.PointerToRawData+s.SizeOfRawData]
 for m in re.finditer(rb'[\x48-\x4f][\x8d\x8b\x89][\x05\x0d\x15\x1d\x25\x2d\x35\x3d]....',data,re.S):
  rv=s.VirtualAddress+m.start();target=rv+7+struct.unpack_from('<i',m.group(),3)[0]
  if target not in vtset:continue
  i=next(cs.disasm(m.group(),base+rv),None)
  if i and i.size==7 and any(o.type==X86_OP_MEM and o.mem.base==X86_REG_RIP for o in i.operands):xrefs[target].append(rv)
for t in classes:
 ch=int(t['class_descriptor'],16);num=u32(ch+8);arr=u32(ch+12);t['bases']=[]
 for k in range(min(num,32)):
  bc=u32(arr+k*4);td=u32(bc)
  t['bases'].append({'name':text(td+16),'bcd':hex(bc),'contained':u32(bc+4),'mdisp':struct.unpack_from('<i',b,off(bc+8))[0],'pdisp':struct.unpack_from('<i',b,off(bc+12))[0],'vdisp':struct.unpack_from('<i',b,off(bc+16))[0]})
 for v in t['vtables']:
  rv=int(v['rva'],16);v['slot344']=hex(q64(rv+344)-base) if executable(q64(rv+344)-base) else None
  v['xref_count']=len(xrefs[rv]);v['xrefs']=[hex(x) for x in xrefs[rv]]
  v['xref_windows']=[{'rva':hex(x),'bounds':[hex(y) for y in bounds(x)] if bounds(x) else None,'instructions':dis(x,100)} for x in xrefs[rv][:4]]
windows=[{'rva':hex(rv),'bounds':[hex(y) for y in bounds(rv)] if bounds(rv) else None,'instructions':dis(rv,n)} for rv,n in [(0xcd0f10,0xc0),(0xcd0f60,0xa0),(0x765680,0x90),(0x903180,0x60),(0x550170,0x90),(0xd23080,0x90),(0x11370e0,0x90),(0x5501b0,0x90)]]
# Selected straight-line windows: constructors and role-specific field resolvers.
extra=[(0x54c760,0x7a),(0x7e1e80,0x35),(0x6e7120,0x3e),(0x6e66e2,0x40),(0x707a9d,0x65),(0x1330ed7,0x60),(0x9063f2,0x60),(0x904300,0x50),(0xcd0f1d,0x38),(0x328a7f8,0x60),(0x903c60,0x60),(0x10e25a2,0x60),(0xa3897b,0x60),(0x342e010,0x60)]
windows.extend({'rva':hex(rv),'bounds':[hex(y) for y in bounds(rv)] if bounds(rv) else None,'instructions':dis(rv,n)} for rv,n in extra)
field_access=[]
for t in classes:
 if t['name'] not in ['.?AVMoverDynamics@@','.?AVTimedAbility@@','.?AVProjectileDynamics@@']:continue
 for v in t['vtables']:
  vr=int(v['rva'],16)
  for k in range(80):
   fr=q64(vr+k*8)-base
   if not executable(fr):break
   f=bounds(fr)
   if not f or f[1]-fr>0x5000:continue
   hits=[]
   for i in cs.disasm(b[off(fr):off(fr)+f[1]-fr],base+fr):
    if any(o.type==X86_OP_MEM and o.mem.disp in ([0x10,0x14,0x18,0x1c,0x24,0x40,0xac,0xb0,0x110,0x118] if 'Dynamics' in t['name'] else [8,12,0x38,0x48,0x4c,0x50,0x90,0x98,0xa0]) for o in i.operands):
     hits.append({'rva':hex(i.address-base),'bytes':i.bytes.hex(' '),'asm':i.mnemonic+' '+i.op_str})
   if hits:field_access.append({'class':t['name'],'slot':k*8,'function':hex(fr),'hits':hits[:12]})
report={'target':p['target'],'sha256':p['sha256'],'classes':classes,'windows':windows,'field_access_samples':field_access,'signatures':[sig(0xcd0f10),sig(0xcd0f1d,56),sig(0xcd0f60,66),sig(0x7e1e97,14),sig(0x54c7c4,14),sig(0x54c783,14),sig(0x76568f,15),sig(0x550170),sig(0xd23080),sig(0x11370e0),sig(0x328a7f8,64)],'limits':['Static class identity and field access only; no process access or sample-based old-to-new proof.','vtable slot344 meaningful only for dynamics-family class, not arbitrary class with 344 bytes following it.','PE bases are RTTI declarations, not proof that live object takes a particular branch.']}
(OUT/'role-guards.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
role_specs=[(64568064,'TimedAbility',0x3d94a40,'static_identity_plus_agent1_runtime_role','Base Ability is not a substitute.'),(64584880,'SpawnProductionItem',0x3d98bf0,'static_identity_plus_agent1_runtime_role','Constructor stores blueprint-like argument at +16; runtime production role observed by agent1.'),(64557040,'SquadUpgradeProductionItem',0x3d92058,'static_role_candidate_not_observed','Vehicle path follows SquadSimInventoryItem queue; no upgrade object observed, old executable absent.'),(64557304,'EntityUpgradeProductionItem',0x3d92160,'static_role_candidate_not_observed','Normal path follows ProductionQueueExt; no upgrade object observed; PlayerUpgradeProductionItem alternative is 0x3E57008.'),(64559016,'MoverDynamics',0x3d926e0,'static_role_candidate_not_observed','Matches state64 discriminator and virtual-slot movement semantics; aircraft, +272/+280 loiter fields not proven.'),(65307976,'ProjectileExt',0x3e49618,'static_identity_plus_agent1_runtime_role','Actual projectile extension observed by agent1, not necessarily owned projectile.'),(65313128,'ProjectileDynamics',0x3e4aa38,'static_role_candidate','Inherited EntityDynamics; constructor initializes mode+64 to2, not proof of live mode0/3. Pure ProjectileLogic/AirLogic actor guard, not ProjectileExt.'),(13437424,'MoverDynamics virtual +344',0xcd0f10,'static_callable_candidate','This is a function RVA, not a vtable. vtable0x3D926E0+344 contains image+0xCD0F10.')]
checks=[]
for site,target in [(0x7646d7,0x3d94a40),(0x76568f,0x3d94a40),(0x7e1e97,0x3d98bf0),(0x54c783+4,0x3d92058),(0x54c7c8,0x3d92160),(0x707a9d,0x3d926e0)]:
 i=next(cs.disasm(b[off(site):off(site)+7],base+site));actual=site+i.size+i.disp;assert actual==target
 checks.append({'instruction':hex(site),'bytes':i.bytes.hex(' '),'target':hex(actual),'rule':'instruction_rva + 7 + signed disp32'})
assert q64(0x3d926e0+344)==base+0xcd0f10
compact_classes=[]
for t in classes:
 if t['name'] not in ['.?AV'+n+'@@' for n in ['TimedAbility','SpawnProductionItem','EntityUpgradeProductionItem','SquadUpgradeProductionItem','PlayerUpgradeProductionItem','MoverDynamics','ProjectileExt','ProjectileDynamics']]:continue
 assert u32(int(t['col'],16)+20)==int(t['col'],16)
 assert u32(int(t['col'],16)+12)==int(t['type_descriptor'],16)
 for v in t['vtables']:assert q64(int(v['rva'],16)-8)==base+int(t['col'],16)
 compact_classes.append({k:t[k] for k in ['name','type_descriptor','col','class_descriptor']}|{'bases':[x['name'] for x in t['bases']],'vtable':t['vtables'][0]['rva']})
compact={'target':p['target'],'sha256':p['sha256'],'status':'PASS_static_internal_consistency_not_dynamic_validation','roles':[{'old_decimal':old,'old_rva':hex(old),'name':name,'new_candidate_rva':hex(new),'evidence_level':level,'limitation':note} for old,name,new,level,note in role_specs],'rtti':compact_classes,'rip_decode_checks':checks,'slot344_check':{'vtable':'0x3D926E0','byte_offset':344,'function_rva':'0xCD0F10'},'field_evidence':[{'class':f['class'],'slot':f['slot'],'function':f['function'],'hits':f['hits']} for f in field_access if (f['class']=='.?AVTimedAbility@@' and f['slot'] in [8,40,80]) or (f['class']=='.?AVMoverDynamics@@' and f['slot'] in [72,144]) or (f['class']=='.?AVProjectileDynamics@@' and f['slot'] in [456,520])],'selected_instructions':[{**w,'instructions':w['instructions'][:20]} for w in windows if w['rva'] in ['0x7e1e80','0x54c760','0x707a9d','0xcd0f10','0xcd0f1d','0xcd0f60','0x328a7f8','0x10e25a2','0xa3897b','0x342e010']],'signatures':report['signatures'],'limits':report['limits']+['Signatures are complete masked bytes for the declared length; raw executable-section matches only, not decoded function-boundary uniqueness.','Prior live evidence is teammate-reported, not collected by this script. No old executable available for direct old-to-new vtable identity proof.','No verified default or uniform RVA delta is inferred.']}
for w in compact['selected_instructions']:
 w['instructions']=[i['rva']+' | '+i['bytes']+' | '+i['asm'] for i in w['instructions']]
for f in compact['field_evidence']:
 f['hits']=[i['rva']+' | '+i['bytes']+' | '+i['asm'] for i in f['hits']]
compact['signatures']=[{k:v for k,v in s.items() if k not in ['bytes','hits']}|{'first_raw_hits':s['hits'][:8],'full_hits_in':'role-guards.json'} for s in report['signatures']]
(OUT/'compactguard-evidence.json').write_text(json.dumps(compact,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'classes':len(classes),'xref_count':sum(len(x) for x in xrefs.values()),'output':'role-guards.json','signatures':[(s['rva'],s['hit_count']) for s in report['signatures']]}))
