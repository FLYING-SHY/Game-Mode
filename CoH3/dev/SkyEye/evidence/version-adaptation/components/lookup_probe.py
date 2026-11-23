"""Read-only, bounded cross-type lookup-column search, with explicit absences."""
import datetime,json,pathlib,struct,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent.parent))
from live_probe import ProcessReader
folder=pathlib.Path(__file__).resolve().parent
prior=json.loads((folder/'component-probe-01.json').read_text(encoding='utf8'))
r=ProcessReader(11192);b=r.base;errors=[]
try:
    # Each expected name uses only first slot before any adjacent object's duplicate.
    rows=prior['typeRepresentatives']; expected={}
    for row in rows:
        kind=row['entity']['componentKind']; names={}
        for s in row['slots'] or []:
            if s['slot']>=32:continue
            names.setdefault(s['rtti']['name'],s['slot'])
        expected[kind]=names
    # Search declared RVA window only; raw memory reads never exceed 64KiB.
    start=103319616-0x100000;end=103319616+0x100000+max(expected)*128+128
    buf=bytearray();ranges=[]
    for addr in range(start,end,65536):
        n=min(65536,end-addr)
        try:chunk=r.read(b+addr,n)
        except Exception as e:errors.append(str(e));chunk=bytes(n)
        buf.extend(chunk);ranges.append([hex(addr),n])
    queries=[('HealthExt',103319626),('CombatExt',103319627),('MovingExt',None),('AbilityExt',103319637),('WeaponExt',None),('WeaponAimExt',103319697),('WeaponListExt',103319632),('ProjectileExt',103319634),('SquadWeaponExt',103319633)]
    found=[]
    for typename,old in queries:
        name='.?AV'+typename+'@@'; positive={k:v[name] for k,v in expected.items() if name in v}
        if len(positive)<2:continue
        seedkind,seedslot=next(iter(positive.items())); candidates=[];pos=0;needle=bytes([seedslot])
        while True:
            pos=buf.find(needle,pos)
            if pos<0:break
            candidate=start+pos-seedkind*128;pos+=1
            if not start<=candidate<end-max(expected)*128:continue
            if all(buf[candidate-start+k*128]==slot for k,slot in positive.items()):
                mismatch=[];mapping=[]
                for k,names in expected.items():
                    actual=buf[candidate-start+k*128];want=names.get(name,255)
                    mapping.append({'type':k,'slot':actual,'expected':want})
                    if actual!=want:mismatch.append(k)
                candidates.append({'lookupRva':hex(candidate),'deltaFromOld':candidate-old if old else None,'positiveCount':len(positive),'mismatches':mismatch,'mapping':mapping})
        candidates.sort(key=lambda x:len(x['mismatches']))
        found.append({'name':name,'oldLookupRva':hex(old) if old else None,'positiveCount':len(positive),'candidates':candidates[:20],'candidateCount':len(candidates)})
    # Check candidate descriptor pools against E70 across every observed type, not just four.
    globals=sorted(set(int(x['globalRva'],16) for x in prior['candidateScores']));ref=r.q(b+0x6187e70);poolcompare=[]
    for g in globals:
        payload=r.q(b+g);matches=[];failed=[]
        for row in rows:
            e=int(row['entity']['address'],16);off=r.u(e+64);kind=row['entity']['componentKind']
            try: equal=r.read(ref+off,32*24)==r.read(payload+off,32*24)
            except Exception as exc:errors.append(str(exc));equal=False
            (matches if equal else failed).append(kind)
        poolcompare.append({'globalRva':hex(g),'payload':hex(payload),'equalToE70Types':matches,'differentOrFailedTypes':failed})
    report={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':11192,'moduleBase':hex(b),'access':'0x410 read only','status':'candidate_not_baseline','searchStartRva':hex(start),'searchEndRva':hex(end),'searchRanges':ranges,'queries':found,'poolComparisons':poolcompare,'bytesRead':r.bytes_read,'errors':errors,'limitations':['RTTI slot assumptions derive from prior same PID snapshot; max slot31 prevents common neighboring allocation spill but is not an independently proven slot count','Absence expected FF inferred from no RTTI slot in first32; high slot may be present','Table matches do not establish runtime caller linkage or process restart stability']}
    out=folder/'lookup-probe-01.json';out.write_text(json.dumps(report,indent=2),encoding='utf8')
    print(json.dumps({'out':str(out),'bytesRead':r.bytes_read,'errors':len(errors),'queries':[{'name':x['name'],'positiveCount':x['positiveCount'],'candidateCount':x['candidateCount'],'best':[{k:v for k,v in c.items() if k!='mapping'} for c in x['candidates'][:3]]} for x in found],'poolComparisons':[{k:v for k,v in q.items() if k!='equalToE70Types'} for q in poolcompare]}))
finally:r.close()
