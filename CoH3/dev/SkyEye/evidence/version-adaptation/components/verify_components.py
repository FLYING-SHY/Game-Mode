"""Re-read each owned entity using candidate lookup columns; no game writes."""
import collections,datetime,json,pathlib,struct,sys
sys.dont_write_bytecode=True
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent.parent))
from live_probe import ProcessReader,probe
folder=pathlib.Path(__file__).resolve().parent;r=ProcessReader(11192);b=r.base;errors=[];cache={}
def rtti(obj):
    vt=r.q(obj)
    if vt in cache:return cache[vt]
    if not b<=vt<b+0x7000000:raise ValueError('Non-image vtable')
    col=r.q(vt-8);td=r.u(col+12);name=r.read(b+td+16,192).split(b'\0')[0].decode('ascii')
    if not name.startswith('.?A'):raise ValueError('No RTTI name')
    cache[vt]={'name':name,'vtableRva':hex(vt-b)};return cache[vt]
def decode(e,kind,lookup,pool):
    slot=r.read(b+lookup+kind*128,1)[0]
    if slot==255:return {'slot':255,'absent':True}
    address=pool+r.u(e+64)+slot*24;raw=r.read(address,24);mode=struct.unpack_from('<I',raw)[0];value=struct.unpack_from('<Q',raw,8)[0];state=raw[16]
    out={'slot':slot,'descriptorAddress':hex(address),'descriptor':raw.hex(),'mode':mode,'value':hex(value),'state':state,'componentIdByte':raw[18]}
    if not state&1 or not value:out['inactive']=True;return out
    if mode>8:raise ValueError(f'Invalid mode {mode}')
    target=value if mode==8 else r.q(b+0x6187e40+mode*16)+(value&0xffffffff)
    out.update(address=hex(target),rtti=rtti(target));return out
try:
    snap=probe(r,{'resource':112926416,'arena':0x6187e40,'world':0x5e0decc});entities=[dict(e,playerSlot=p['slot']) for p in snap['players'] for e in p['ownedEntities']]
    old=[103319618,103319626,103319627,103319632,103319633,103319634,103319637,103319697]
    pools=[0x6187dd0,0x6187dd8,0x6187e70,0x6187e78,0x6187e80,0x6187e88];payloads={g:r.q(b+g) for g in pools};rows=[];summaries={x:{'oldLookupRva':hex(x),'newLookupRva':hex(x+8096),'componentId':x-103319616,'names':collections.Counter(),'absent':0,'inactive':0,'active':0,'idMismatch':[],'types':set(),'modeCounts':collections.Counter()} for x in old}
    for ent in entities:
        e=int(ent['address'],16);kind=struct.unpack('<H',r.read(e+76,2))[0];row={'entity':ent,'currentKind':kind,'components':[]}
        for x in old:
            try:
                q=decode(e,kind,x+8096,payloads[0x6187e70]);q['oldLookupRva']=hex(x);q['newLookupRva']=hex(x+8096);s=summaries[x];s['types'].add(kind)
                if q.get('absent'):s['absent']+=1
                elif q.get('inactive'):s['inactive']+=1
                else:
                    s['active']+=1;s['names'][q['rtti']['name']]+=1;s['modeCounts'][q['mode']]+=1
                    if q['componentIdByte']!=x-103319616:s['idMismatch'].append(ent['address'])
                row['components'].append(q)
            except Exception as ex:errors.append({'entity':ent['address'],'lookup':hex(x+8096),'error':str(ex)})
        rows.append(row)
    # Compare only valid lookup-selected descriptors; unused slots never serve as evidence.
    comparisons=[];unique={e['componentKind']:e for e in entities}
    for g in pools:
        diffs=[];checked=0
        for kind,ent in unique.items():
            for x in old:
                try:
                    lhs=decode(int(ent['address'],16),kind,x+8096,payloads[0x6187e70]);rhs=decode(int(ent['address'],16),kind,x+8096,payloads[g]);checked+=1
                    a1={k:v for k,v in lhs.items() if k!='descriptorAddress'};a2={k:v for k,v in rhs.items() if k!='descriptorAddress'}
                    if a1!=a2:diffs.append({'entity':ent,'lookupRva':hex(x+8096),'E70':lhs,'other':rhs})
                except Exception as ex:diffs.append({'entity':ent,'lookupRva':hex(x+8096),'error':str(ex)})
        comparisons.append({'globalRva':hex(g),'payload':hex(payloads[g]),'checks':checked,'differenceCount':len(diffs),'differences':diffs})
    for s in summaries.values():s['types']=sorted(s['types']);s['names']=dict(s['names']);s['modeCounts']=dict(s['modeCounts'])
    report={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':11192,'moduleBase':hex(b),'access':'0x410 read only','status':'candidate_not_baseline','poolGlobalRva':'0x6187e70','modeBasesRva':'0x6187e40 + mode*16','lookupRootRva':'0x628a7e0','entityCount':len(entities),'typeCount':len(unique),'summaries':list(summaries.values()),'poolComparisons':comparisons,'entities':rows,'errors':errors,'bytesRead':r.bytes_read,'stableArenaWorld':snap['stableRootsAfterRead'] and r.q(b+0x6187e40)==int(snap['arena'],16) and r.u(b+0x5e0decc)==int(snap['worldOffset'],16)}
    report['bytesRead']=r.bytes_read
    out=folder/'components-verified-01.json';out.write_text(json.dumps(report,indent=2),encoding='utf8')
    print(json.dumps({'out':str(out),'bytesRead':r.bytes_read,'errors':len(errors),'entityCount':len(entities),'summaries':[{k:v for k,v in s.items() if k!='types'} for s in summaries.values()],'poolComparisons':[{k:v for k,v in x.items() if k!='differences'} for x in comparisons]}))
finally:r.close()
