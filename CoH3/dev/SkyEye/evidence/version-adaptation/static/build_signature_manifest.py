"""Consolidate existing evidence; scan only newly introduced focused byte patterns.
No RTTI rescan, process API, patch, build, or engine write. Output is this directory.
"""
from pathlib import Path
import json,struct,re,hashlib
import pefile
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
from capstone.x86_const import X86_OP_MEM,X86_REG_RIP,X86_OP_IMM
OUT=Path(__file__).resolve().parent
focused=json.loads((OUT/'focused-evidence.json').read_text(encoding='utf-8'))
roles=json.loads((OUT/'role-guards.json').read_text(encoding='utf-8'))
compact=json.loads((OUT/'compactguard-evidence.json').read_text(encoding='utf-8'))
ui=json.loads((OUT/'ui-followup.json').read_text(encoding='utf-8'))
target=focused['source'];b=Path(target).read_bytes();sha=hashlib.sha256(b).hexdigest().upper();assert sha==str(focused['sha256']).upper()
pe=pefile.PE(data=b,fast_load=True);base=pe.OPTIONAL_HEADER.ImageBase;cs=Cs(CS_ARCH_X86,CS_MODE_64);cs.detail=True
secs=[s for s in pe.sections if s.Characteristics&0x20000000]
def off(rv):return pe.get_offset_from_rva(rv)
def scan(sig):
 pattern=b''.join(b'.' if x=='??' else re.escape(bytes.fromhex(x)) for x in sig.split())
 return [hex(s.VirtualAddress+m.start()) for s in secs for m in re.finditer(b'(?='+pattern+b')',b[s.PointerToRawData:s.PointerToRawData+s.SizeOfRawData],re.S)]
def instructions(rv,n):
 return [{'rva':hex(i.address-base),'bytes':i.bytes.hex(' '),'asm':i.mnemonic+' '+i.op_str} for i in cs.disasm(b[off(rv):off(rv)+n],base+rv)]
def decode_refs(rv,n):
 refs=[]
 for i in cs.disasm(b[off(rv):off(rv)+n],base+rv):
  delta=i.address-base-rv
  for o in i.operands:
   if o.type==X86_OP_MEM and o.mem.base==X86_REG_RIP:refs.append({'instruction_offset':delta,'instruction_size':i.size,'kind':'rip_relative','expected_target_rva':hex(i.address-base+i.size+o.mem.disp)})
   elif o.type==X86_OP_IMM and (i.mnemonic=='call' or i.mnemonic.startswith('j')):refs.append({'instruction_offset':delta,'instruction_size':i.size,'kind':'direct_branch','expected_target_rva':hex(o.imm-base)})
 return refs
def new_feature(name,rv,n,category,note):
 raw=b[off(rv):off(rv)+n];mask=bytearray(b'\xff'*len(raw));size=0
 for i in cs.disasm(raw,base+rv):
  pos=i.address-base-rv;size=pos+i.size
  if any(o.type==X86_OP_MEM and o.mem.base==X86_REG_RIP for o in i.operands):mask[pos+i.disp_offset:pos+i.disp_offset+i.disp_size]=b'\0'*i.disp_size
  if i.mnemonic=='call' or i.mnemonic.startswith('j'):
   if i.imm_size:mask[pos+i.imm_offset:pos+i.imm_offset+i.imm_size]=b'\0'*i.imm_size
  if i.mnemonic=='ret':break
 sig=' '.join(f'{x:02X}' if m else '??' for x,m in zip(raw[:size],mask[:size]));hits=scan(sig)
 return {'id':name,'category':category,'expected_rva':hex(rv),'length':size,'sig':sig,'mask':''.join('x' if m else '?' for m in mask[:size]),'all_raw_hits':hits,'raw_hit_count':len(hits),'decode_sites':decode_refs(rv,size),'key_instructions':instructions(rv,size),'evidence_source':'new focused key-instruction pattern, no RTTI rescan','note':note}
features=[]
labels={0x930c32:('resource_root_getter','resources'),0x2727fe:('resource_vector_iteration','resources'),0x272880:('resource_vector_count','resources'),0x17932a:('world_offset','world')}
for s in focused['signatures']:
 rv=int(s['rva'],16);sig=s['signature'];name,cat=labels[rv];hits=s['executable_raw_hits']
 features.append({'id':name,'category':cat,'expected_rva':hex(rv),'length':s['length'],'sig':sig,'mask':''.join('?' if x=='??' else 'x' for x in sig.split()),'all_raw_hits':hits,'raw_hit_count':len(hits),'decode_sites':decode_refs(rv,s['length']),'key_instructions':instructions(rv,s['length']),'evidence_source':'focused-evidence.json/signatures','note':s['note']})
for s in roles['signatures']:
 rv=int(s['rva'],16);features.append({'id':'role_'+hex(rv),'category':'guards','expected_rva':hex(rv),'length':len(s['mask']),'sig':s['sig'],'mask':s['mask'],'all_raw_hits':s['hits'],'raw_hit_count':s['hit_count'],'decode_sites':decode_refs(rv,len(s['mask'])),'key_instructions':instructions(rv,len(s['mask'])),'evidence_source':'role-guards.json/signatures','note':'Raw byte pattern only; see role identity record, not a live-object guarantee.'})
extra=[('camera_registry',0x189e43,16,'camera','Q(global)+184 vector; main/secondary GameUICore RTTI must be checked.'),('ui_singleton_read',0x6a613d,32,'ui','Window-related singleton and screen dimensions; class is GameUIAppWindow.'),('ui_singleton_store',0x14c9708,14,'ui','Constructor/setter store to UI singleton.'),('colour_property_registry',0x405576,48,'property_registry','Registry mask+0x298, buckets+0x280, sentinel+0x270; key 0x320000 requires further chain checks.'),('matrix_wrapper',0x1411bc2,48,'ui_properties','Wrapper+32 is the pointer slot, not wrapper start.'),('playable_width_wrapper',0xdf457b,48,'ui_properties','Selected wrapper registration group; same-name registrations elsewhere exist.'),('playable_height_wrapper',0xdf465c,48,'ui_properties','Selected wrapper registration group; same-name registrations elsewhere exist.'),('wrapper_pointer_store',0x9c5e56,4,'ui_properties','Store [rdi+32]; xref constructor-to-wrapper context is essential.'),('health_lookup',0x2eb5d4,24,'components','lookup disp32 is image-relative indexed memory, not RIP-relative.'),('tls_component_pool',0x2eb5f7,26,'components','Pool anchor displacement 0x6187E70 is indexed by TLS; not plain singleton.'),('component_descriptor_resolver',0x820980,56,'components','Mode8 direct pointer; other modes TLS-indexed arena plus uint payload.'),('arena_helper_anchor',0x8209ad,32,'roots','Arena anchor0x6187E40 is TLS/mode indexed.'),('projectile_dynamics_install',0x10e25a2,24,'guards','ProjectileDynamics initialization; +64 default2 does not validate later mode0/3 states.'),('projectile_vtable_install',0x10ccaa0,14,'guards','ProjectileExt class identity from TD/COL, not nearest vtable.'),('mover_vtable_install',0x707a9d,16,'guards','MoverDynamics constructor writes class vtable and mode+64.')]
features.extend(new_feature(*x) for x in extra)
# Non-RIP SIB/disp operands use image-base register, not instruction-relative address.
for name,at,expected in [('health_lookup',0x2eb5e0,0x628a7ea),('tls_component_pool',0x2eb609,0x6187e70)]:
 f=next(x for x in features if x['id']==name);i=next(cs.disasm(b[off(at):off(at)+15],base+at));mem=next(o.mem for o in i.operands if o.type==X86_OP_MEM);assert mem.disp==expected
 f['decode_sites'].append({'instruction_offset':at-int(f['expected_rva'],16),'instruction_size':i.size,'kind':'image_indexed_displacement','expected_target_rva':hex(expected),'rule':'disp32 is image-relative anchor only if the register identified as imageBase is established by surrounding instructions; index/TLS semantics remain separate.'})
for f in features:
 decoded=[]
 for hit in f['all_raw_hits']:
  hr=int(hit,16);values=[]
  for site in f['decode_sites']:
   at=hr+site['instruction_offset'];i=next(cs.disasm(b[off(at):off(at)+15],base+at),None)
   if i is None:values.append(None);continue
   if site['kind']=='rip_relative':
    operand=next((o for o in i.operands if o.type==X86_OP_MEM and o.mem.base==X86_REG_RIP),None);value=at+i.size+operand.mem.disp if operand else None
   elif site['kind']=='direct_branch':
    operand=next((o for o in i.operands if o.type==X86_OP_IMM),None);value=operand.imm-base if operand else None
   else:
    operand=next((o for o in i.operands if o.type==X86_OP_MEM),None);value=operand.mem.disp if operand else None
   values.append(hex(value) if value is not None else None)
  decoded.append([hit,values])
 f['decoded_raw_hits']=decoded
 f['decoded_raw_hits_rule']='Each pair is [raw_hit_RVA, target_RVAs_in_decode_sites_order]; target equality alone does not establish object semantics.'
 f['selected_rva']=None
manifest={'schema':'coh3-static-signature-manifest/v1','policy':{'search_scope':'executable section raw bytes only; overlapping matches retained','zero_hits':'record zero_hit; selected_rva=null','multiple_hits':'record multi_hit; selected_rva=null, never select first','one_hit':'unique_raw_candidate only, not live/runtime validation; selected_rva=null','decode_rule':'RIP: instruction RVA + size + signed displacement; direct branch: instruction RVA + size + signed immediate; SIB disp is image anchor only with established image-base register.','never':'No process attach, memory write, patch, engine modification, RVA fallback, or inferred uniform delta.'},'builds':{sha:{'target':target,'version':'5.1.52426.0','file_length':len(b),'image_base':hex(base),'features':features,'role_candidates':compact['roles'],'rtti':compact['rtti'],'rtti_check':'COL signature1, pSelf=COL RVA, COL.pTypeDescriptor=TD RVA, vtable[-1]=imageBase+COL; names at TD+16.','key_ui_instructions':ui['verification'],'historical_limits':['Old supported hash 2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde has no supplied old executable: no old-to-new RTTI equivalence proof.','All seven guards remain maintenance candidates; teammate runtime observations are separate evidence.','No new-session dynamic validation performed by this static task.']}}}
index={'sha256':sha,'version':'5.1.52426.0','manifest':'signatures-by-hash.json','feature_count':len(features),'features':[{'id':f['id'],'category':f['category'],'expected_rva':f['expected_rva'],'raw_hit_count':f['raw_hit_count'],'expected_decode_targets':[s['expected_target_rva'] for s in f['decode_sites']],'selected_rva':None} for f in features],'role_candidates':compact['roles'],'other_preserved_evidence':['focused-evidence.json','ui-followup.json','role-guards.json','compactguard-evidence.json','broad-pattern-exploration.json']}
(OUT/'signature-index.json').write_text(json.dumps(index,ensure_ascii=False,indent=2),encoding='utf-8')
# Compact JSON keeps the manifest small while all raw hits and instructions remain present.
(OUT/'signatures-by-hash.json').write_text(json.dumps(manifest,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
print(json.dumps({'status':'PASS_manifest_generated','features':len(features),'reused_patterns':len(focused['signatures'])+len(roles['signatures']),'new_focused_patterns':len(extra),'all_raw_hits':sum(x['raw_hit_count'] for x in features),'manifest_bytes':(OUT/'signatures-by-hash.json').stat().st_size}))
