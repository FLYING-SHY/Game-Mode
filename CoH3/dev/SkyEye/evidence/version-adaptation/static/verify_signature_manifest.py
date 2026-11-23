"""Offline validator. Never attaches, patches, selects a candidate, or modifies engines.
Usage: python verify_signature_manifest.py [--target EXE] [--manifest JSON]
All outputs stay beside this script; an unknown hash is rejected before matching.
"""
from pathlib import Path
import argparse,json,re,hashlib,struct
import pefile
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
from capstone.x86_const import X86_OP_MEM,X86_REG_RIP,X86_OP_IMM
OUT=Path(__file__).resolve().parent

def match(data,sig):
 pattern=b''.join(b'.' if x=='??' else re.escape(bytes.fromhex(x)) for x in sig.split())
 return [m.start() for m in re.finditer(b'(?='+pattern+b')',data,re.S)]
def classify(hits):
 return {'classification':'zero_hit' if len(hits)==0 else 'unique_raw_candidate' if len(hits)==1 else 'multi_hit','selected_rva':None,'runtime_validated':False}

def main():
 ap=argparse.ArgumentParser();ap.add_argument('--manifest',default=str(OUT/'signatures-by-hash.json'));ap.add_argument('--target');args=ap.parse_args()
 manifest=json.loads(Path(args.manifest).read_text(encoding='utf-8'))
 fixtures=[]
 for name,data,sig,expected in [('zero',bytes.fromhex('90 90 90'),'CC DD',[]),('multi',bytes.fromhex('AA BB 00 00 AA BB'),'AA BB',[0,4]),('unique_is_not_validation',bytes.fromhex('AA BB 00'),'AA BB',[0])]:
  hits=match(data,sig);assert hits==expected
  result=classify(hits);assert result['selected_rva'] is None and result['runtime_validated'] is False
  fixtures.append({'id':name,'fixture_bytes':data.hex(' '),'sig':sig,'all_hits':hits,**result,'pass':True})
 target=args.target or next(iter(manifest['builds'].values()))['target'];b=Path(target).read_bytes();sha=hashlib.sha256(b).hexdigest().upper()
 if sha not in manifest['builds']:
  result={'status':'unknown_hash_no_matching_performed','sha256':sha,'target':target,'selected_rva':None,'fixtures':fixtures}
  (OUT/'signature-validation.json').write_text(json.dumps(result,indent=2),encoding='utf-8');print(json.dumps(result));return 2
 build=manifest['builds'][sha];assert len(b)==build['file_length'];pe=pefile.PE(data=b,fast_load=True);base=pe.OPTIONAL_HEADER.ImageBase
 cs=Cs(CS_ARCH_X86,CS_MODE_64);cs.detail=True
 def off(rv):return pe.get_offset_from_rva(rv)
 secs=[s for s in pe.sections if s.Characteristics&0x20000000]
 results=[];failures=[]
 for f in build['features']:
  assert len(f['sig'].split())==len(f['mask'])==f['length']
  assert f['mask']==''.join('?' if x=='??' else 'x' for x in f['sig'].split())
  hits=[hex(s.VirtualAddress+x) for s in secs for x in match(b[s.PointerToRawData:s.PointerToRawData+s.SizeOfRawData],f['sig'])]
  checks={'all_hits_equal_manifest':hits==f['all_raw_hits'],'count_equal':len(hits)==f['raw_hit_count'],'expected_site_present':f['expected_rva'] in hits}
  decoded=[]
  for hit in hits:
   hr=int(hit,16);values=[]
   for site in f['decode_sites']:
    at=hr+site['instruction_offset'];i=next(cs.disasm(b[off(at):off(at)+15],base+at),None);value=None
    if i is not None:
     if site['kind']=='rip_relative':
      o=next((o for o in i.operands if o.type==X86_OP_MEM and o.mem.base==X86_REG_RIP),None);value=at+i.size+o.mem.disp if o else None
     elif site['kind']=='direct_branch':
      o=next((o for o in i.operands if o.type==X86_OP_IMM),None);value=o.imm-base if o else None
     elif site['kind']=='image_indexed_displacement':
      o=next((o for o in i.operands if o.type==X86_OP_MEM),None);value=o.mem.disp if o else None
    values.append(hex(value) if value is not None else None)
   decoded.append([hit,values])
  checks['all_decoded_targets_equal_manifest']=decoded==f['decoded_raw_hits']
  actual_at_expected=next((v for h,v in decoded if h==f['expected_rva']),None)
  checks['expected_site_target_constraints']=actual_at_expected==[s['expected_target_rva'] for s in f['decode_sites']]
  for check,ok in checks.items():
   if not ok:failures.append({'id':f['id'],'check':check})
  results.append({'id':f['id'],'expected_rva':f['expected_rva'],'raw_hit_count':len(hits),'checks':checks,**classify(hits)})
 rtti=[]
 for t in build['rtti']:
  col=int(t['col'],16);td=int(t['type_descriptor'],16);vt=int(t['vtable'],16)
  check={'col_signature1':struct.unpack_from('<I',b,off(col))[0]==1,'col_self':struct.unpack_from('<I',b,off(col+20))[0]==col,'type_descriptor':struct.unpack_from('<I',b,off(col+12))[0]==td,'vtable_minus8':struct.unpack_from('<Q',b,off(vt-8))[0]==base+col}
  o=off(td+16);check['name']=b[o:b.index(0,o)].decode('ascii')==t['name']
  rtti.append({'name':t['name'],'checks':check})
  if not all(check.values()):failures.append({'id':t['name'],'check':'rtti_chain'})
 slot344=struct.unpack_from('<Q',b,off(0x3d926e0+344))[0]==base+0xcd0f10
 if not slot344:failures.append({'check':'MoverDynamics_slot344'})
 result={'status':'PASS_offline_internal_consistency_only' if not failures else 'FAIL','target':target,'sha256':sha,'feature_count':len(results),'total_raw_hits':sum(x['raw_hit_count'] for x in results),'features':results,'rtti':rtti,'MoverDynamics_slot344':slot344,'fixtures':fixtures,'failures':failures,'limits':['No candidate is selected, including unique patterns.','Raw matches include mid-instruction offsets; decoder target agreement is a static constraint, not runtime object validation.','Image-base/TLS indexed operand displacement requires surrounding register semantics; manifest preserves instructions rather than guessing the external thread.','No old executable or two-new-session dynamic verification is performed.']}
 (OUT/'signature-validation.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
 print(json.dumps({'status':result['status'],'features':len(results),'raw_hits':result['total_raw_hits'],'fixtures':[(x['id'],x['classification']) for x in fixtures],'failures':failures}));return 1 if failures else 0
if __name__=='__main__':raise SystemExit(main())
