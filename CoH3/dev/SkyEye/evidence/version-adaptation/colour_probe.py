"""Read-only registry lookup for the actual UI colour config key."""
import argparse,datetime,json,pathlib
from live_probe import ProcessReader
p=argparse.ArgumentParser();p.add_argument('--pid',type=int,required=True);p.add_argument('--out',required=True);a=p.parse_args();r=ProcessReader(a.pid)
result={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'verification':'candidate_only','pid':a.pid,'hypotheses':[]}
try:
    for slot in [102261280,0x61881B0]:
        out={'globalRva':hex(slot)}
        try:
            reg=r.q(r.base+slot);h=14695981039346656037
            for value in (3276800).to_bytes(4,'little'):h=((h^value)*1099511628211)&0xffffffffffffffff
            mask=r.q(reg+664)
            if mask>1048576:raise ValueError('mask out of bounds')
            bucket=r.q(reg+640)+(h&mask)*16;sentinel=r.q(reg+624);last=r.q(bucket);node=r.q(bucket+8);seen=set();out.update(registry=hex(reg),mask=mask,bucket=hex(bucket))
            while node!=sentinel and node not in seen and len(seen)<128:
                seen.add(node)
                if r.u(node+16)==3276800:
                    begin,end=r.q(node+24),r.q(node+32)
                    if end-begin!=8:raise ValueError('config is not unique')
                    config=r.q(begin);out.update(config=hex(config),configKey=r.u(config+72),configBody=r.read(config,160).hex());break
                if node==last:break
                node=r.q(node+8)
            out['chainLength']=len(seen)
        except Exception as e:out['error']=str(e)
        result['hypotheses'].append(out)
    result['bytesRead']=r.bytes_read;pathlib.Path(a.out).write_text(json.dumps(result,indent=2),encoding='utf8');print(json.dumps(result,indent=2))
finally:r.close()
