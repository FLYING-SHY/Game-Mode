"""Read-only bounded runtime object/vtable role map; no input or game calls."""
import collections,datetime,json,pathlib,struct,sys
sys.dont_write_bytecode=True
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent.parent))
from live_probe import ProcessReader,probe
folder=pathlib.Path(__file__).resolve().parent;r=ProcessReader(11192);b=r.base;errors=[];cache={};observed=[];coverage={}
def run(label,fn):
    try:return fn()
    except Exception as e:errors.append({'scope':label,'error':str(e)});return None
def rtti(obj):
    vt=r.q(obj)
    if vt in cache:return cache[vt]
    if not b<=vt<b+0x7000000:raise ValueError(f'Non-image vtable {vt:#x}')
    col=r.q(vt-8);td=r.u(col+12);name=r.read(b+td+16,256).split(b'\0')[0].decode('ascii')
    if not name.startswith('.?A'):raise ValueError('No RTTI name')
    info={'name':name,'vtableRva':hex(vt-b),'colRva':hex(col-b),'typeDescriptorRva':hex(td)};cache[vt]=info;return info
def component(e,cid):
    kind=struct.unpack('<H',r.read(e+76,2))[0];slot=r.read(b+0x628a7e0+cid+kind*128,1)[0]
    if slot==255:return None
    raw=r.read(pool+r.u(e+64)+slot*24,24);mode=struct.unpack_from('<I',raw)[0];value=struct.unpack_from('<Q',raw,8)[0]
    if not raw[16]&1 or not value:return None
    if mode>8:raise ValueError('Unknown component mode')
    obj=value if mode==8 else r.q(b+0x6187e40+mode*16)+(value&0xffffffff)
    return obj
def object_record(role,obj,source,blueprint_offset=None):
    info={'role':role,'address':hex(obj),'source':source,'rtti':rtti(obj)}
    if blueprint_offset is not None:info['blueprint']=r.string(r.q(obj+blueprint_offset)+40)
    observed.append(info);return info

def ability_list(head,source):
    count=r.u(head+44);off=r.u(head+40)
    if count>256:raise ValueError(f'Ability count {count}>256')
    if not count:return 0
    raw=r.read(arena+off,count*4);got=0
    for i in range(count):
        rel=struct.unpack_from('<I',raw,i*4)[0]
        if rel==0xffffffff:continue
        obj=arena+rel
        rec=run('ability_object',lambda:object_record('ability_instance',obj,dict(source,index=i),56))
        if rec:got+=1;rec['state12']=r.u(obj+12);rec['targetOffset8']=hex(r.u(obj+8))
    return got

def production(e,vehicle=False):
    compentity=arena+r.u(e+124) if vehicle else e;comp=component(compentity,2 if vehicle else 17)
    if comp is None:return 'no_component'
    off=r.u(comp+(16 if vehicle else 52))
    if off==0xffffffff:return 'no_queue'
    queue=arena+off;count=r.u(queue+52);dataoff=r.u(queue+48)
    if count>128:raise ValueError(f'Production count {count}>128')
    if not count:return 'empty'
    entry=arena+dataoff;raw=r.read(entry,192);ctrloff=struct.unpack_from('<I',raw,184)[0]
    if ctrloff==0xffffffff:raise ValueError('Invalid production controller offset')
    obj=arena+ctrloff
    rec=object_record('vehicle_queue_controller' if vehicle else 'entity_queue_controller',obj,{'entity':hex(e),'component':hex(comp),'queue':hex(queue),'queueCount':count,'entry':hex(entry)},16)
    rec.update(remaining=struct.unpack_from('<f',raw,48)[0],total=struct.unpack_from('<f',raw,52)[0]);return 'observed'

def chunkread(addr,length):return b''.join(r.read(addr+i,min(65536,length-i)) for i in range(0,length,65536))
try:
    snap=probe(r,{'resource':112926416,'arena':0x6187e40,'world':0x5e0decc});arena=int(snap['arena'],16);pool=r.q(b+0x6187e70);world=arena+int(snap['worldOffset'],16)
    ents=[dict(e,playerSlot=p['slot']) for p in snap['players'] for e in p['ownedEntities']]
    ability_counts=[];production_counts=collections.Counter()
    for player in snap['players']:
        addr=int(player['address'],16);ability_counts.append({'player':hex(addr),'kind':'player','count':run('player_abilities',lambda:ability_list(addr+1016,{'player':hex(addr)}))})
    for ent in ents:
        e=int(ent['address'],16);ability=run('ability_component',lambda:component(e,21))
        if ability:ability_counts.append({'entity':hex(e),'kind':'component','count':run('component_abilities',lambda:ability_list(arena+r.u(ability+76),{'entity':hex(e),'component':hex(ability)}))})
        health=run('health_component',lambda:component(e,10))
        if health and r.f(health+108)>0:
            status=run('production',lambda:production(e,False));production_counts['entity_'+str(status)]+=1
            if '\\vehicles\\' in ent['blueprint']:
                status=run('vehicle_production',lambda:production(e,True));production_counts['vehicle_'+str(status)]+=1
    # Registry covers non-owned transient projectiles and loiter entities too.
    reg=arena+r.u(world+260);count=r.u(reg+24);coverage['registryCapacity']=count;coverage['maxLiveObjects']=4096
    if not 0<count<=65536:raise ValueError('Registry capacity out of bound')
    offsets=chunkread(arena+r.u(reg+8),count*8);generations=chunkread(arena+r.u(reg+20),count*4);live=0;paths=collections.Counter();projectile_names=[];loiter_names=[]
    for i in range(count):
        off=struct.unpack_from('<I',offsets,i*8)[0];generation=struct.unpack_from('<I',generations,i*4)[0]
        if off==0xffffffff or not generation:continue
        if live>=4096:coverage['truncated']=True;break
        live+=1;e=arena+off
        def inspect():
            header=r.read(e,128)
            if struct.unpack_from('<I',header,104)[0]!=generation or struct.unpack_from('<I',header,108)[0]!=i:raise ValueError('Registry entity generation/index mismatch')
            name=r.string(struct.unpack_from('<Q',header,56)[0]+40);paths[name]+=1
            if '\\projectiles\\' in name:
                projectile_names.append(name);obj=component(e,18)
                if obj:object_record('projectile_component',obj,{'entity':hex(e),'blueprint':name,'registryIndex':i})
            # Candidate body at entity+388 is only sampled on airborne/loiter/mine paths, not arbitrary entities.
            if any(s in name.lower() for s in ['loiter','aircraft','airplane','mine']):
                loiter_names.append(name);offbody=r.u(e+388)
                if offbody!=0xffffffff:
                    obj=arena+offbody;rec=object_record('entity_body388_candidate',obj,{'entity':hex(e),'blueprint':name,'registryIndex':i})
                    rec.update(ownerBackReference=hex(arena+r.u(obj+36)),state64=r.u(obj+64),matchesOwner=arena+r.u(obj+36)==e)
        run('registry_entity',inspect)
    coverage.update(registryLiveInspected=live,ownedEntities=len(ents),abilityLists=ability_counts,production=dict(production_counts),projectilePaths=projectile_names,loiterOrMinePaths=loiter_names,registryBlueprintCounts=dict(paths))
    summary={}
    for x in observed:
        key=(x['role'],x['rtti']['name'],x['rtti']['vtableRva']);summary[key]=summary.get(key,0)+1
    report={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':11192,'moduleBase':hex(b),'access':'0x410 read only','status':'candidate_not_baseline','observed':observed,'summary':[{'role':k[0],'name':k[1],'vtableRva':k[2],'count':v} for k,v in summary.items()],'coverage':coverage,'errors':errors,'stableArenaWorld':snap['stableRootsAfterRead'] and r.q(b+0x6187e40)==arena and r.u(b+0x5e0decc)==world-arena}
    report['bytesRead']=r.bytes_read
    out=folder/'role-probe-01.json';out.write_text(json.dumps(report,indent=2),encoding='utf8')
    print(json.dumps({'out':str(out),'bytesRead':r.bytes_read,'summary':report['summary'],'errors':errors[:10],'errorCount':len(errors),'registryCapacity':count,'liveInspected':live,'production':dict(production_counts),'projectilePaths':projectile_names,'loiterOrMinePaths':loiter_names}))
finally:r.close()
