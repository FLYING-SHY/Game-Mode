"""Bounded pointer-slot search for observed property IDs, with RTTI validation."""
import argparse,datetime,json,pathlib,struct
from live_probe import ProcessReader
p=argparse.ArgumentParser();p.add_argument('--pid',type=int,required=True);p.add_argument('--out',required=True);a=p.parse_args();r=ProcessReader(a.pid)
result={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'verification':'candidate_only','rangeRva':['0x6ac9000','0x6ad5000'],'hits':[]}
try:
    start,end=0x6AC9000,0x6AD5000;buf=r.read(r.base+start,end-start)
    for off in range(0,len(buf),8):
        value=struct.unpack_from('<Q',buf,off)[0]
        if not 65536<=value<0x7fffffffffff:continue
        try:
            key=int.from_bytes(r.read(value+184,2),'little')
            if key not in [874,875]:continue
            vt=r.q(value)
            if not r.base<=vt<r.base+120369152:continue
            col=r.q(vt-8)
            if r.u(col)!=1:continue
            typ=r.read(r.base+r.u(col+12)+16,160).split(b'\0')[0].decode('ascii')
            if typ!='.?AVDependencyProperty@AVUI@@':continue
            result['hits'].append({'slotRva':hex(start+off),'key':key,'address':hex(value),'type':typ,'body':r.read(value,200).hex()})
        except (OSError,ValueError,UnicodeError):pass
    matrix=r.q(r.base+0x6AB4018);result['matrix']={'slotRva':'0x6AB4018','address':hex(matrix),'key':int.from_bytes(r.read(matrix+184,2),'little')}
    result['bytesRead']=r.bytes_read;pathlib.Path(a.out).write_text(json.dumps(result,indent=2),encoding='utf8');print(json.dumps(result,indent=2))
finally:r.close()
