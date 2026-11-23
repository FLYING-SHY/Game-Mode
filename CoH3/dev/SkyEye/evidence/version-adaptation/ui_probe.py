"""Bounded read-only UI graph hypotheses; candidates only, no global scan/write."""
import argparse, collections, datetime, json, pathlib, struct
from live_probe import ProcessReader
p=argparse.ArgumentParser();p.add_argument('--pid',type=int,required=True);p.add_argument('--out',required=True);a=p.parse_args();r=ProcessReader(a.pid)
def rtti(obj):
    vt=r.q(obj)
    if not r.base <= vt < r.base+120369152: raise ValueError('vtable outside game module')
    col=r.q(vt-8)
    if r.u(col)!=1: raise ValueError('invalid RTTI signature')
    name=r.read(r.base+r.u(col+12)+16,200).split(b'\0')[0].decode('ascii')
    return {'type':name,'vtableRva':hex(vt-r.base),'objectOffset':r.u(col+4)}
def children(node):
    found=set();body=r.read(node,576)
    for off in range(144,576,8):
        ptr=struct.unpack_from('<Q',body,off)[0]
        try:
            if ptr>65536 and ptr!=node and r.q(ptr+136)==node:
                rtti(ptr);found.add(ptr)
        except (OSError,ValueError,UnicodeError):pass
    try:
        vec=r.q(node+504);n=r.u(vec+124);data=r.q(vec+112)
        if n<=512 and data>65536:
            buf=r.read(data,n*8)
            for (ptr,) in struct.iter_unpack('<Q',buf):
                try:
                    if ptr>65536 and r.q(ptr+136)==node:rtti(ptr);found.add(ptr)
                except (OSError,ValueError,UnicodeError):pass
    except (OSError,ValueError):pass
    return found
result={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'verification':'candidate_only','hypotheses':[]}
try:
    for root in [112837064,112844920,112845144]:
        out={'globalRva':hex(root)}
        try:
            singleton=r.q(r.base+root);obj=r.q(singleton+136);out.update(singleton=hex(singleton),root=hex(obj),**rtti(obj));queue=collections.deque([obj]);seen=set();counts=collections.Counter();selected=[]
            while queue and len(seen)<6000:
                node=queue.popleft()
                if node in seen:continue
                seen.add(node)
                try:
                    info=rtti(node);counts[info['type']]+=1
                    if any(s in info['type'] for s in ['MapControl','AnvilRTSHUD','Root','Window','MapPanel']):
                        selected.append({'address':hex(node),'parent':hex(r.q(node+136)),**info,'bodyHex':r.read(node,576).hex()})
                    queue.extend(children(node)-seen)
                except (OSError,ValueError,UnicodeError) as e:
                    if 'budget' in str(e):raise
            out.update(visited=len(seen),queueRemaining=len(queue),types=dict(counts),selected=selected)
        except Exception as e:out['error']=str(e)
        result['hypotheses'].append(out)
    result['propertyHypotheses']=[]
    for old in [111878264,111989640,111989680]:
        for delta in [0,7856,8064,8080]:
            item={'oldSlotRva':hex(old),'slotRva':hex(old+delta)}
            try:
                prop=r.q(r.base+old+delta);item.update(address=hex(prop),id=struct.unpack('<H',r.read(prop+184,2))[0],**rtti(prop))
            except Exception as e:item['error']=str(e)
            result['propertyHypotheses'].append(item)
    for hypothesis in result['hypotheses']:
        for node in hypothesis.get('selected',[]):
            if 'MapControl' not in node['type']:continue
            try:
                body=bytes.fromhex(node['bodyHex']);count=struct.unpack_from('<I',body,124)[0]&0x3ff;data=r.q(int(node['address'],16)+112);entries=[]
                if count>512:raise ValueError('property count out of bound')
                for off in range(0,count*16,16):
                    flags,key,_,value=struct.unpack('<HHIQ',r.read(data+off,16));entry={'flags':hex(flags),'key':key,'value':hex(value)}
                    try:
                        resolved=value
                        if flags&0x70:resolved=r.q(value+(48 if flags&0x40 else 56 if flags&0x20 else 40))
                        if resolved>65536:entry.update(resolved=hex(resolved),float40=r.f(resolved+40),**rtti(resolved))
                    except Exception as e:entry['error']=str(e)
                    entries.append(entry)
                node['properties']=entries
            except Exception as e:node['propertiesError']=str(e)
    result['bytesRead']=r.bytes_read;pathlib.Path(a.out).write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf8');print(json.dumps({k:v for k,v in result.items() if k!='hypotheses'}));print(json.dumps([{k:v for k,v in x.items() if k not in ('types','selected')}|{'selected':[{'address':y['address'],'type':y['type'],'vtableRva':y['vtableRva']} for y in x.get('selected',[])]} for x in result['hypotheses']],indent=2))
finally:r.close()
