"""Recheck artifact claims against disk only; no process APIs or dynamic game access."""
from pathlib import Path
import json,hashlib,struct
import pefile
OUT=Path(__file__).resolve().parent
p=json.loads((OUT/'pe-candidates.json').read_text(encoding='utf-8'))
f=json.loads((OUT/'focused-evidence.json').read_text(encoding='utf-8'))
b=Path(p['target']).read_bytes();pe=pefile.PE(data=b,fast_load=True);base=pe.OPTIONAL_HEADER.ImageBase
assert hashlib.sha256(b).hexdigest()=='8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec'
assert len(b)==108391096 and len(p['old_dependencies'])==36
for rv,target,size in [(0x930c32,0x6bb1ed0,7),(0x2727fe,0x6bb2238,7),(0x272810,0x6bb2240,7),(0x17932a,0x5e0decc,6),(0x189e43,0x6187f58,7)]:
 o=pe.get_offset_from_rva(rv); assert rv+size+struct.unpack_from('<i',b,o+size-4)[0]==target
for t in f['selected_rtti']:
 col=int(t['col'],16);o=pe.get_offset_from_rva(col);vals=struct.unpack_from('<6I',b,o)
 assert vals[0]==1 and vals[5]==col and vals[3]==int(t['type_descriptor'],16) and vals[1]==t['object_offset']
 for v in t['vtables']:
  vo=pe.get_offset_from_rva(int(v['rva'],16));assert struct.unpack_from('<Q',b,vo-8)[0]==base+col
for s in f['signatures']:
 assert s['rva'] in s['executable_raw_hits']
 if s['rva']=='0x930c32':assert len(s['executable_raw_hits'])==44
 else:assert len(s['executable_raw_hits'])==1
assert all(d['new_file_same_rva_bytes'] is None for d in p['old_dependencies'] if int(d['rva'],16)>=0x6a00000)
result={'status':'PASS','sha256':p['sha256'],'old_dependency_count':36,'rtti_selected_record_count':len(f['selected_rtti']),'rip_target_checks':5,'signature_counts':{s['rva']:len(s['executable_raw_hits']) for s in f['signatures']},'scope':'Static-file consistency only; no dynamic correctness claim.'}
(OUT/'verification.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
