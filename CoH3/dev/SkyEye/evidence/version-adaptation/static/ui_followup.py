"""Read-only UI property registration and component-pool code evidence."""
from pathlib import Path
import json,struct,re,bisect
import pefile
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
OUT=Path(__file__).resolve().parent
p=json.loads((OUT/'pe-candidates.json').read_text());b=Path(p['target']).read_bytes();pe=pefile.PE(data=b,fast_load=True);base=pe.OPTIONAL_HEADER.ImageBase
md=Cs(CS_ARCH_X86,CS_MODE_64)
def dis(rv,n):
 o=pe.get_offset_from_rva(rv)
 return [{'rva':hex(i.address-base),'bytes':i.bytes.hex(' '),'asm':i.mnemonic+' '+i.op_str} for i in md.disasm(b[o:o+n],base+rv)]
targets={0x3ed40b0:'ScenarioWidth',0x3ed40c0:'ScenarioHeight',0x3e07df8:'PlayableWidth',0x3e07d20:'PlayableHeight',0x3e032a0:'DesiredTacticalMapWidth',0x3e03280:'DesiredTacticalMapHeight',0x3ddc1a8:'Width',0x3ddc1b0:'Height',0x4801dc8:'MapControl',0x47d4038:'worldTerrainWidth',0x47d4050:'worldTerrainHeight',0x3eb5ea0:'Matrix',0x6b9e078:'UI singleton',0x61881b0:'Colour registry',0x6187e40:'Arena',0x6187e70:'TLS arena table'}
refs=[]
for sec in pe.sections:
 if not sec.Characteristics&0x20000000:continue
 data=b[sec.PointerToRawData:sec.PointerToRawData+sec.SizeOfRawData]
 for m in re.finditer(rb'[\x40-\x4f]?[\x8b\x89\x8d\x3b\x39\x63][\x05\x0d\x15\x1d\x25\x2d\x35\x3d]',data):
  pos=m.start();rv=sec.VirtualAddress+pos;sz=len(m.group())+4
  if pos+sz>len(data):continue
  target=rv+sz+struct.unpack_from('<i',data,pos+sz-4)[0]
  if target not in targets:continue
  i=next(md.disasm(data[pos:pos+15],base+rv,count=1),None)
  if i is None or i.size!=sz:continue
  refs.append({'target':hex(target),'label':targets[target],'rva':hex(rv),'instructions':dis(rv,200 if target in [0x47d4038,0x47d4050,0x3eb5ea0,0x3ddc1a8,0x3ddc1b0,0x4801dc8,0x3ed40b0,0x3ed40c0,0x3e07df8,0x3e07d20,0x3e032a0,0x3e03280] else 80)})
names=['GameUIAppWindow@WPFGUI','EEUIAppWindow@WPFGUI','WindowManager@AVUI','Window@AVUI','Application@AVUI']
rtti=[t for t in p['rtti'] if t['name'] in ['.?AV'+x+'@@' for x in names]]
result={'sha256':p['sha256'],'references':refs,'ui_rtti':rtti,'focused_windows':{hex(x):dis(x,n) for x,n in [(0x405576,240),(0x14c9708,60),(0x6a613d,80),(0x9c5e30,64),(0x1411bad,56),(0xdf4566,64),(0xdf4647,64),(0x2eb5d4,110),(0x1829e1,80),(0x820980,180)]},'limit':'Static references only. Property mappings require runtime property id/type/value validation; no uniform RVA shift.'}
checks=[]
for rv,target in [(0xDF457B,0x6ACF308),(0xDF465C,0x6ACF330),(0x1411BC2,0x6AB3FF8),(0x405576,0x61881B0),(0x6A613D,0x6B9E078),(0x8209AD,0x6187E40)]:
 o=pe.get_offset_from_rva(rv); actual=rv+7+struct.unpack_from('<i',b,o+3)[0]
 assert actual==target,(hex(rv),hex(actual),hex(target))
 checks.append({'rva':hex(rv),'target':hex(target),'bytes':b[o:o+7].hex(' '),'pass':True})
assert b[pe.get_offset_from_rva(0x9C5E56):pe.get_offset_from_rva(0x9C5E56)+4]==bytes.fromhex('48894720')
assert b[pe.get_offset_from_rva(0x2EB609):pe.get_offset_from_rva(0x2EB609)+8]==bytes.fromhex('498b8cce707e1806')
result['verification']={'status':'PASS','rip_checks':checks,'wrapper_store_plus32':True,'tls_component_pool_E70_instruction':True}
(OUT/'ui-followup.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
for x in refs:
 if x['target'] in ['0x47d4038','0x47d4050','0x3eb5ea0']:
  print(x['label'],x['rva']);print('\n'.join(v['rva']+' '+v['asm'] for v in x['instructions']))
