"""One-shot bounded read-only registry/projectile diagnostic, no game inputs/calls.
Usage: python projectile_probe.py --pid 11192 --tag paused-01
Warmup note: unlike reader shellSeen cache, this diagnostic inspects every live path once.
"""
import argparse,collections,datetime,json,math,pathlib,struct,sys,time
sys.dont_write_bytecode=True
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent.parent))
from live_probe import ProcessReader
p=argparse.ArgumentParser();p.add_argument('--pid',type=int,default=11192);p.add_argument('--tag',default='paused-01');a=p.parse_args()
if not a.tag.replace('-','').replace('_','').isalnum():raise ValueError('Invalid tag')
r=ProcessReader(a.pid);b=r.base;started=time.monotonic();errors=[];records=[];cache={};coverage=collections.Counter()
def tick():
    if time.monotonic()-started>20:raise TimeoutError('20 second diagnostic budget exceeded')
def u(raw,o):return struct.unpack_from('<I',raw,o)[0]
def q(raw,o):return struct.unpack_from('<Q',raw,o)[0]
def f(raw,o):return struct.unpack_from('<f',raw,o)[0]
def block(addr,n):return b''.join(r.read(addr+i,min(65536,n-i)) for i in range(0,n,65536))
def rtti(obj):
    vt=r.q(obj)
    if vt not in cache:
        if not b<=vt<b+0x7000000:raise ValueError('Non-image vtable')
        col=r.q(vt-8);td=r.u(col+12);cache[vt]={'vtableRva':hex(vt-b),'name':r.read(b+td+16,256).split(b'\0')[0].decode('ascii'),'colRva':hex(col-b),'typeDescriptorRva':hex(td)}
    return cache[vt]
def component(e,raw):
    kind=struct.unpack_from('<H',raw,76)[0];slot=r.read(b+0x628a7f2+kind*128,1)[0]
    info={'lookupRva':'0x628a7f2','kind':kind,'slot':slot}
    if slot==255:return None,info
    d=r.read(pool+u(raw,64)+slot*24,24);mode=u(d,0);value=q(d,8);info.update(mode=mode,state=d[16],value=hex(value),componentId=d[18])
    if not d[16]&1 or value==0:return None,info
    if mode>8:raise ValueError('Unknown component mode')
    obj=value if mode==8 else r.q(b+0x6187e40+mode*16)+(value&0xffffffff)
    info.update(address=hex(obj),rtti=rtti(obj));return obj,info
TANK={'weapon\\german\\ballistic_weapon\\tank_gun\\380mm_rocket_launcher_sturmtiger_ger','weapon\\german\\ballistic_weapon\\tank_gun\\75mm_semovente_artillery_barrage_ger','weapon\\afrika_korps\\ballistic_weapon\\tank_gun\\75mm_stug_iii_short_ak','weapon\\afrika_korps\\ballistic_weapon\\tank_gun\\75mm_barrage_stug_iii_short_ak'}
def pos(v):return all(math.isfinite(x) and abs(x)<100000 for x in v) # diagnostic broad bound, not engine ValidPosition
try:
    arena=r.q(b+0x6187e40);wo=r.u(b+0x5e0decc);world=arena+wo;pool=r.q(b+0x6187e70)
    pv=arena+r.u(world+252);pc=r.u(pv+16)
    if not 0<pc<=16:raise ValueError('Player count invalid')
    pd=arena+r.u(pv+12);players={}
    for j in range(pc):
        x=arena+r.u(pd+j*4);players[x]={'address':hex(x),'slot':r.u(x+12),'identity':hex(r.q(x+8))}
    reg=arena+r.u(world+260);count=r.u(reg+24);offs=r.u(reg+8);gens=r.u(reg+20)
    if not 0<count<=65536:raise ValueError('Registry capacity invalid')
    offsets=block(arena+offs,count*8);generations=block(arena+gens,count*4)
    for i in range(count):
        tick();off=u(offsets,i*8);gen=u(generations,i*4)
        if off==0xffffffff or not gen:continue
        coverage['liveInspected']+=1;e=arena+off
        try:
            h=r.read(e,128)
            if u(h,104)!=gen or u(h,108)!=i:coverage['generationMismatch']+=1;continue
            name=r.string(q(h,56)+40)
            if not(name.startswith('ebps\\') and '\\projectiles\\' in name):continue
            coverage['projectilePaths']+=1;rec={'entity':hex(e),'registryIndex':i,'generation':gen,'blueprint':name,'gates':{}};records.append(rec);g=rec['gates']
            raw=r.read(e,392);rec['rawBodyHex']=raw.hex();bodyoff=u(raw,388);g['bodyPresent']=bodyoff!=0xffffffff
            rec['body']={'offset388':hex(bodyoff),'flags160':hex(u(raw,160)),'position44':[f(raw,x) for x in (44,48,52)]}
            comp,ci=component(e,raw);rec['component']=ci;g['componentPresent']=comp is not None
            if bodyoff!=0xffffffff:
                actor=arena+bodyoff;motion=r.read(actor,80);rec['rawActorHex']=motion.hex();rec['actor']={'address':hex(actor),'rtti':rtti(actor),'backref36':hex(arena+u(motion,36)),'state64':u(motion,64),'mode16':motion[16],'position20':[f(motion,x) for x in (20,24,28)]}
                g['actorVtable']=q(motion,0)==b+0x3e4aa38;g['actorBackref']=arena+u(motion,36)==e;g['actorMode16']=motion[16] in (1,2)
            if comp:
                ext=r.read(comp,208);rec['rawExtHex']=ext.hex();owner=u(ext,56);weapon=r.string(q(ext,64)+40);mortar=weapon.startswith('weapon\\') and ('\\mortar\\' in weapon or 'mortar' in weapon.rsplit('\\',1)[-1]);arc=weapon in TANK or weapon=='weapon\\american\\explosive_weapon\\heavy_artillery\\500lb_bomb_p47_us'
                weaponok=weapon in TANK or (weapon.startswith('weapon\\') and any(x in weapon for x in ('\\heavy_artillery\\','\\light_artillery\\','\\mortar\\')))
                rec['ext']={'rtti':rtti(comp),'backref8':hex(arena+u(ext,8)),'ownerOffset56':hex(owner),'ownerPlayer':players.get(arena+owner),'weapon':weapon,'sourceType16':ext[16],'source20':u(ext,20),'source24':u(ext,24),'radius104':f(ext,104),'mortar':mortar,'arc':arc}
                g.update(extVtable=q(ext,0)==b+0x3e49618,ownerValid=owner!=0xffffffff,ownerInPlayers=arena+owner in players,weapon=weaponok,extBackref=arena+u(ext,8)==e,sourceType=ext[16]==3,source20=u(ext,20)!=0,radius=math.isfinite(f(ext,104)) and 0<=f(ext,104)<=100,notDestroyed=(u(raw,160)&0x4000)==0)
                if bodyoff!=0xffffffff:
                    g['actorState64']=u(motion,64)==0 or ((mortar or arc) and u(motion,64)==3)
                    xyz=[f(motion,x) for x in (20,24,28)];target=[f(raw,x) for x in (44,48,52)];g['notArrived']=sum((x-y)**2 for x,y in zip(xyz,target))>0.0001
                    rec['positionsFiniteBroadBound']=pos(xyz) and pos(target)
            rec['generationStable']=r.u(e+104)==gen and r.u(e+108)==i
            rec['failedGates']=[k for k,v in g.items() if not v]
            rec['note']='ownerInPlayers is not enemy attribution; exact enemy/local selection and engine ValidPosition, cache warmup, source-name/timing not reimplemented'
        except Exception as ex:
            coverage['errors']+=1
            if len(errors)<100:errors.append({'index':i,'entity':hex(e),'error':str(ex)})
    stable=r.q(b+0x6187e40)==arena and r.u(b+0x5e0decc)==wo and r.u(reg+24)==count and r.u(reg+8)==offs and r.u(reg+20)==gens
    report={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'base':hex(b),'access':'0x410 query/read only','arena':hex(arena),'worldOffset':hex(wo),'registry':hex(reg),'registryCapacity':count,'coverage':dict(coverage),'records':records,'errors':errors,'stableRootsAndRegistry':stable,'bytesRead':r.bytes_read,'elapsedSeconds':time.monotonic()-started,'status':'observed_candidates_not_decode_verified' if records else ('sample_absent' if not errors else 'inconclusive_read_errors'),'limits':{'singleReadBytes':65536,'totalBytes':8388608,'seconds':20,'maxRegistry':65536},'notes':['one snapshot; no gameplay input/write','all-live inspection bypasses shellSeen warmup; not actual C# Decode execution','absence only for complete successful registry scan; no cross-session assertion']}
    out=pathlib.Path(__file__).resolve().parent/(a.tag+'.json');out.write_text(json.dumps(report,indent=2,allow_nan=False),encoding='utf8');print(json.dumps({'out':str(out),'status':report['status'],'coverage':dict(coverage),'records':len(records),'bytesRead':r.bytes_read,'stable':stable,'errors':errors[:3]}))
finally:r.close()
