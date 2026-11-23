"""Bounded read-only PID11192 marker-path reconstruction; no game control/login."""
import argparse, collections, datetime, hashlib, json, math, pathlib, re, struct, sys, time
sys.dont_write_bytecode=True
HERE=pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent))
from live_probe import ProcessReader

def valid_position(p): return all(math.isfinite(x) for x in p) and abs(p[0])<20000 and abs(p[1])<2000 and abs(p[2])<20000

def supported(path): return path.startswith('sbps\\races\\') and not any(x in path for x in ['\\buildings\\','\\base_defense_','\\howitzer_base_'])

def main():
    p=argparse.ArgumentParser();p.add_argument('--self-test',action='store_true');a=p.parse_args()
    if a.self_test:
        assert valid_position([100,5,-100]) and not valid_position([20000,0,0]) and not valid_position([0,2000,0]) and not valid_position([0,0,float('nan')])
        assert supported('sbps\\races\\german\\infantry\\jaeger_ger') and not supported('ebps\\races\\german\\infantry\\jaeger_ger')
        assert not (False and valid_position([1,2,3])) and (True and valid_position([1,2,3]))
        print('PASS position finite/bounds; PASS supported squad filter; PASS false colour gate suppresses valid position; no process opened');return 0
    source=HERE.parents[2]/'client'/'CoH3MemoryTool'/'GameLayout52426.cs';text=source.read_text(encoding='utf8')
    mapping={int(k):int(v,16) for k,v in re.findall(r'\{\s*(\d+),\s*0x([0-9A-Fa-f]+)\s*\}',text)}
    runner_path=HERE.parents[2]/'tests'/'maintenance-live'/'LiveSampling.cs';runner_text=runner_path.read_text(encoding='utf8')
    runner_binding='source.ColourKnown = row.GameColourKnown;' in runner_text and 'source.Colour = row.GameColour;' in runner_text
    r=ProcessReader(11192);b=r.base;start=time.perf_counter();calls=0;requested=0;errors=[];original=r.read;stage='roots'
    def read(addr,n):
        nonlocal calls,requested
        calls+=1;requested+=n
        if calls>20000 or requested>2*1024*1024 or time.perf_counter()-start>10:raise ValueError('Diagnostic bounded budget exceeded')
        try:return original(addr,n)
        except Exception as ex:errors.append({'stage':stage,'address':hex(addr),'length':n,'error':str(ex)});raise
    r.read=read
    def resolve(arena,off):
        if off==0xffffffff:raise ValueError('Unavailable arena offset FFFFFFFF')
        return arena+off
    def health(entity):
        kind=struct.unpack('<H',read(entity+76,2))[0];index=read(b+mapping[103319626]+kind*128,1)[0]
        info={'kind':kind,'index':index}
        if index==255:return None,info
        desc=r.q(b+mapping[102260448])+r.u(entity+64)+index*24;raw=read(desc,24);mode=struct.unpack_from('<I',raw)[0];value=struct.unpack_from('<Q',raw,8)[0];state=raw[16]
        info.update(descriptor=hex(desc),mode=mode,value=hex(value),state=state,raw=raw.hex())
        if not state&1 or not value:return None,info
        if mode>8:raise ValueError('Component mode out of bounds')
        target=value if mode==8 else r.q(b+mapping[102260400]+mode*16)+(value&0xffffffff)
        hp=r.f(target+108)
        if not math.isfinite(hp):raise ValueError('Nonfinite health')
        info.update(address=hex(target),health=hp);return hp,info
    report={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':11192,'moduleBase':hex(b),'access':'0x410 PROCESS_QUERY_INFORMATION|PROCESS_VM_READ','status':'pending','sourceSha256':hashlib.sha256(text.encode('utf8')).hexdigest(),'runnerSourceSha256':hashlib.sha256(runner_text.encode('utf8')).hexdigest(),'runnerColourBindingPresentAtCapture':runner_binding,'sourceModel':'Reconstruction of ReadOwnedSquad/TrackSquadMember/ReadSquadMarkers, not a call into authorized runner Snapshot. No credentials/authentication involved.','players':[],'sources':[],'limitations':['Single paused PID; no UI/visual or lifecycle acceptance.','Source ColourKnown=false is inferred from C# default and missing runner assignment, not read from runner heap.','Infrastructure49 marker sources are excluded from this squad-only reconstruction.']}
    try:
        arena=r.q(b+mapping[102260400]);wo=r.u(b+mapping[98614988]);world=resolve(arena,wo);vec=resolve(arena,r.u(world+252));pc=r.u(vec+16);pd=resolve(arena,r.u(vec+12))
        if not 0<pc<=16:raise ValueError('Player count out of bounds')
        sources={};registry_checks=[]
        for pi in range(pc):
            player=resolve(arena,r.u(pd+pi*4));slot=r.u(player+12);registry=resolve(arena,r.u(player+1792));count=r.u(registry+40);dataoff=r.u(registry+36);data=resolve(arena,dataoff)
            if count>4096:raise ValueError('Entity count out of bounds')
            row={'slot':slot,'registryCount':count,'rejections':collections.Counter()};report['players'].append(row)
            registry_checks.append((registry,count,dataoff))
            for i in range(count):
                stage='snapshot entity validation';off=r.u(data+i*4)
                if off==0xffffffff:row['rejections']['sentinel']+=1;continue
                entity=resolve(arena,off);raw=read(entity,128);owner=resolve(arena,struct.unpack_from('<I',raw,88)[0])
                if owner!=player:row['rejections']['other_owner']+=1;continue
                squad_off=struct.unpack_from('<I',raw,124)[0]
                if squad_off==0xffffffff:row['rejections']['no_squad']+=1;continue
                hp,hi=health(entity)
                if hp is None or hp<=0:row['rejections']['no_live_health']+=1;continue
                squad=resolve(arena,squad_off);identity=r.q(squad+104);path=r.string(r.q(squad+56)+40)
                if not supported(path):row['rejections']['unsupported_squad']+=1;continue
                key=(slot,squad)
                if key not in sources:sources[key]={'slot':slot,'squad':hex(squad),'squadAddress':squad,'identity':hex(identity),'identityValue':identity,'path':path,'members':[],'positions':[],'sourceColourKnownDefault':False,'runnerColourAssignmentPresentAtCapture':runner_binding}
                src=sources[key];member={'entity':hex(entity),'entityAddress':entity,'owner':hex(owner),'ownerAddress':owner,'generation':struct.unpack_from('<I',raw,96)[0],'squadOffset':squad_off,'snapshotRaw128':raw.hex(),'snapshotHealth':hi};src['members'].append(member)
        for src in sources.values():
            stage='source squad identity';squad=src['squadAddress'];src['identityBeforeMatches']=r.q(squad+104)==src['identityValue']
            for member in src['members']:
                stage='marker member checks'
                try:
                    raw=read(member['entityAddress'],128);gen=struct.unpack_from('<I',raw,96)[0];sqoff=struct.unpack_from('<I',raw,124)[0];owner=resolve(arena,struct.unpack_from('<I',raw,88)[0]);pos=list(struct.unpack_from('<3f',raw,44))
                    checks={'generation':gen==member['generation'],'squadOffset':sqoff==member['squadOffset'],'owner':owner==member['ownerAddress']}
                    hp,hi=health(member['entityAddress']);checks.update(liveHealth=hp is not None and hp>0,validPosition=valid_position(pos))
                    member.update(recheckRaw128=raw.hex(),checks=checks,health=hi,position=pos,status='position_observed' if all(checks.values()) else 'filtered')
                    if all(checks.values()):src['positions'].append(pos)
                except Exception as ex:member.update(status='read_error',error=repr(ex))
                member.pop('entityAddress');member.pop('ownerAddress')
            src['identityAfterMatches']=r.q(squad+104)==src['identityValue'];src['memberCount']=len(src['members']);src['validPositionCount']=len(src['positions'])
            src['centreIfRealColourBound']=[sum(p[axis] for p in src['positions'])/len(src['positions']) for axis in range(3)] if src['positions'] and src['identityBeforeMatches'] and src['identityAfterMatches'] else None
            src['preFixRunnerGateOutcome']='Skipped before member checks because source.ColourKnown defaults false'
            src['capturedRunnerBindingStatus']='Binding now present in source; actual runner execution is separate evidence' if runner_binding else 'No direct colour assignment found in captured runner source'
            src.pop('squadAddress');src.pop('identityValue');report['sources'].append(src)
        report['stableRegistryRechecks']=all(r.u(reg+40)==n and r.u(reg+36)==off for reg,n,off in registry_checks)
        report['stableRootsAfterRead']=r.q(b+mapping[102260400])==arena and r.u(b+mapping[98614988])==wo
        report['status']='read_error' if errors or any(m['status']=='read_error' for s in report['sources'] for m in s['members']) else 'observed_sources' if report['sources'] else 'unknown_no_sources'
        report.update(sourceCount=len(report['sources']),memberCount=sum(s['memberCount'] for s in report['sources']),validMemberCount=sum(s['validPositionCount'] for s in report['sources']),centresIfColourBound=sum(s['centreIfRealColourBound'] is not None for s in report['sources']))
    except Exception as ex:report.update(status='probe_error',stage=stage,error=repr(ex))
    finally:
        report.update(readCalls=calls,requestedBytes=requested,successfulBytes=r.bytes_read,readFailures=errors,elapsedMilliseconds=round((time.perf_counter()-start)*1000,3));r.close()
        out=HERE/'markers-probe-01.json';out.write_text(json.dumps(report,ensure_ascii=False,indent=2,allow_nan=False),encoding='utf8')
    print(json.dumps({k:report.get(k) for k in ['status','sourceCount','memberCount','validMemberCount','centresIfColourBound','readCalls','requestedBytes','elapsedMilliseconds','stableRegistryRechecks','stableRootsAfterRead']}))
    return 3 if report['status'] in ('read_error','probe_error') or not report.get('stableRootsAfterRead') else 0
if __name__=='__main__':sys.exit(main())
