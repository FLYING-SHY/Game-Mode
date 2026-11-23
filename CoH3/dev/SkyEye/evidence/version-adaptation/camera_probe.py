"""Read-only bounded camera/global hypotheses; records RTTI and layout, not a baseline."""
import argparse, datetime, json, math, pathlib, struct
from live_probe import ProcessReader
p=argparse.ArgumentParser();p.add_argument('--pid',type=int,required=True);p.add_argument('--out',required=True);a=p.parse_args()
r=ProcessReader(a.pid)
def cstring(addr): return r.read(addr,256).split(b'\0')[0].decode('ascii',errors='replace')
def rtti(obj):
    vt=r.q(obj); col=r.q(vt-8); type_rva=r.u(col+12)
    return {'vtableRva':hex(vt-r.base),'type':cstring(r.base+type_rva+16),'locatorOffset':r.u(col+4)}
def camera(root):
    out={'rootRva':hex(root)}
    try:
        obj=r.q(r.base+root); vec=r.q(obj+184);begin,end=r.q(vec),r.q(vec+8)
        if not 0 <= end-begin <= 8192 or (end-begin)%8: raise ValueError('Invalid camera vector')
        out.update(owner=hex(obj),begin=hex(begin),end=hex(end),candidates=[])
        for slot in range(begin,end,8):
            ptr=r.q(slot)
            if ptr<65576: continue
            core=ptr-40; item={'core':hex(core)}
            try:
                item.update(rtti(core))
                matrix=r.q(core+416); data=r.read(matrix,816)
                view=struct.unpack_from('<16f',data,88); projection=struct.unpack_from('<16f',data,752)
                item.update(view=view,projection=projection,
                    matrixInvariant=all(math.isfinite(x) for x in view+projection) and abs(view[15]-1)<.01 and projection[0]>0 and projection[5]>0 and abs(projection[11]-1)<.01)
                controller=r.q(core+680);item['controller']={'address':hex(controller),**rtti(controller),'identity':hex(r.q(controller+8)),'mode':r.read(controller+24,1)[0]}
            except Exception as e:item['error']=str(e)
            out['candidates'].append(item)
    except Exception as e:out['error']=str(e)
    return out
try:
    result={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'base':hex(r.base),'verification':'hypothesis_not_verified',
            'hypotheses':[camera(x) for x in [102260680,102268760,102268872]]}
    pathlib.Path(a.out).write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False),encoding='utf8');print(json.dumps(result,ensure_ascii=True,indent=2,allow_nan=False))
finally:r.close()
