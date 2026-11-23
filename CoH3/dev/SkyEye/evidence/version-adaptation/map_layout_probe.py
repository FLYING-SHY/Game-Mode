"""Read-only map hierarchy/property/transform observation using RTTI roles."""
import argparse,datetime,json,pathlib,struct
from live_probe import ProcessReader
p=argparse.ArgumentParser();p.add_argument('--pid',type=int,required=True);p.add_argument('--graph',required=True);p.add_argument('--out',required=True);a=p.parse_args();r=ProcessReader(a.pid)
def rtti(obj):
    vt=r.q(obj)
    if not r.base<=vt<r.base+120369152:raise ValueError('vtable out of module')
    col=r.q(vt-8)
    if r.u(col)!=1:raise ValueError('invalid COL')
    return r.read(r.base+r.u(col+12)+16,200).split(b'\0')[0].decode('ascii')
def properties(node):
    count=r.u(node+124)&0x3ff
    if count>512:raise ValueError('property bound')
    data=r.read(r.q(node+112),count*16) if count else b'';out={}
    for off in range(0,len(data),16):
        flags,key,_,value=struct.unpack_from('<HHIQ',data,off)
        if flags&0x70:value=r.q(value+(48 if flags&0x40 else 56 if flags&0x20 else 40))
        elif flags&0x100:continue
        out[key]=value
    return out
result={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'verification':'candidate_only','maps':[]}
try:
    graph=json.loads(pathlib.Path(a.graph).read_text(encoding='utf8'))
    for hypothesis in graph['hypotheses']:
        for selected in hypothesis.get('selected',[]):
            if 'MapControl' not in selected['type']:continue
            item={'type':selected['type'],'address':selected['address']};result['maps'].append(item)
            try:
                node=int(selected['address'],16);props=properties(node);item['worldSizeCandidates']={str(k):r.f(props[k]+40) for k in [874,875] if k in props};template=r.q(node+(528 if 'TacticalMap' in item['type'] else 472));item['template']=hex(template);item['displaySize']=list(struct.unpack('<2f',r.read(template+296,8)));chain=[];seen=set();node=template
                while node and node not in seen and len(seen)<64:
                    seen.add(node);body=r.read(node,344);props=properties(node);entry={'address':hex(node),'type':rtti(node),'translation':list(struct.unpack_from('<2f',body,168)),'size':list(struct.unpack_from('<2f',body,296)),'visibilityFlags':hex(struct.unpack_from('<I',body,160)[0])};chain.append(entry)
                    transform=props.get(40)
                    if transform:
                        entry['transform']={'address':hex(transform),'type':rtti(transform),'vtableRva':hex(r.q(transform)-r.base),'floats208':list(struct.unpack('<12f',r.read(transform+208,48)))}
                        tprops=properties(transform)
                        entry['transform']['propertyKeys']=list(tprops)
                        for k,v in tprops.items():
                            if v>65536:
                                try:
                                    typ=rtti(v)
                                    if 'Matrix' in typ:entry['transform']['boxedMatrix']={'key':k,'type':typ,'data40':r.read(v+40,32).hex()}
                                except (OSError,ValueError,UnicodeError):pass
                    node=struct.unpack_from('<Q',body,136)[0]
                item['chain']=chain;item['terminatedAtRoot']=node==0
            except Exception as e:item['error']=str(e)
    result['bytesRead']=r.bytes_read;pathlib.Path(a.out).write_text(json.dumps(result,indent=2,allow_nan=False),encoding='utf8');print(json.dumps(result,indent=2,allow_nan=False))
finally:r.close()
