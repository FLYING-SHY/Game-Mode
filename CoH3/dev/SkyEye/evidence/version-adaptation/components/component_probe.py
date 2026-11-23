"""Bounded 0x410 read-only component descriptor/RTTI probe. No input/injection."""
import argparse,datetime,json,pathlib,struct,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent.parent))
from live_probe import ProcessReader,probe
p=argparse.ArgumentParser();p.add_argument('--out',required=True);p.add_argument('--pid',type=int,default=11192)
a=p.parse_args();r=ProcessReader(a.pid);b=r.base;errors=[];typecache={}
def attempt(fn):
    try:return fn()
    except Exception as e:
        if len(errors)<200:errors.append(str(e))
        return None
def rtti(obj):
    vt=r.q(obj)
    if not b<=vt<b+0x7000000:return None
    if vt in typecache:return typecache[vt]
    col=r.q(vt-8)
    if not b<=col<b+0x7000000:return None
    td=r.u(col+12)
    if not 0<td<0x7000000:return None
    name=r.read(b+td+16,192).split(b'\0')[0].decode('ascii')
    if not name.startswith('.?A'):return None
    info={'vtableRva':hex(vt-b),'name':name,'colRva':hex(col-b),'typeDescriptorRva':hex(td)}
    typecache[vt]=info;return info

def slots(entity,payload,modebase,limit=64):
    off=r.u(entity+64);raw=r.read(payload+off,limit*24);out=[]
    for i in range(limit):
        d=raw[i*24:(i+1)*24];mode=struct.unpack_from('<I',d)[0];value=struct.unpack_from('<Q',d,8)[0];state=d[16]
        if not state&1 or mode>8 or not value:continue
        target=value if mode==8 else r.q(b+modebase+mode*16)+(value&0xffffffff)
        info=attempt(lambda:rtti(target))
        if info:out.append({'slot':i,'mode':mode,'value':hex(value),'state':state,'address':hex(target),'rtti':info,'descriptor':d.hex()})
    return out
try:
    snap=probe(r,{'resource':112926416,'arena':0x6187e40,'world':0x5e0decc})
    entities=[dict(e,playerSlot=row['slot']) for row in snap['players'] for e in row['ownedEntities']]
    reps={}
    for e in entities:reps.setdefault(e['componentKind'],e)
    # Candidate global addresses are only hypotheses, validated independently by live RTTI.
    candidates=sorted(set([102260448]+list(range(102268480-256,102268480+257,8))))
    scores=[]
    for glob in candidates:
        payload=attempt(lambda:r.q(b+glob))
        if not payload or not 0x10000<=payload<0x7fffffffffff:continue
        for modebase in [102260400,102268480]:
            sample=[]
            for e in list(reps.values())[:4]:
                found=attempt(lambda:slots(int(e['address'],16),payload,modebase,32))
                if found:sample.append({'entity':e,'slots':found})
            if sample:scores.append({'globalRva':hex(glob),'payload':hex(payload),'modeBaseRva':hex(modebase),'rttiCount':sum(len(x['slots']) for x in sample),'sample':sample})
    scores.sort(key=lambda x:x['rttiCount'],reverse=True)
    result={'time':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'moduleBase':hex(b),'access':'0x410 read only','budgetBytes':8*1024*1024,'descriptorLayout':'mode u32+0; value u64+8; state byte+16; stride24','status':'candidate_only_not_baseline','entityCount':len(entities),'typeCount':len(reps),'candidateScores':scores}
    if scores:
        best=scores[0];payload=int(best['payload'],16);modebase=int(best['modeBaseRva'],16);rows=[]
        old=[103319618,103319626,103319627,103319632,103319633,103319634,103319637,103319697]
        for kind,e in reps.items():
            row={'entity':e,'slots':attempt(lambda:slots(int(e['address'],16),payload,modebase,96)),'lookupHypotheses':[]}
            for delta in [0,8192]:
                row['lookupHypotheses'].append({'delta':delta,'entries':[{'oldLookupRva':hex(v),'candidateRva':hex(v+delta),'slotByte':attempt(lambda v=v,delta=delta:r.read(b+v+delta+kind*128,1)[0])} for v in old]})
            rows.append(row)
        result['typeRepresentatives']=rows
    result['errors']=errors;result['bytesRead']=r.bytes_read
    result['stableArenaWorld']=snap['stableRootsAfterRead'] and r.q(b+0x6187e40)==int(snap['arena'],16) and r.u(b+0x5e0decc)==int(snap['worldOffset'],16)
    pathlib.Path(a.out).write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf8')
    print(json.dumps({'out':a.out,'bytesRead':r.bytes_read,'entityCount':len(entities),'typeCount':len(reps),'top':[{'globalRva':x['globalRva'],'modeBaseRva':x['modeBaseRva'],'rttiCount':x['rttiCount']} for x in scores[:6]],'errors':len(errors)}))
finally:r.close()
