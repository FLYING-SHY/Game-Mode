"""Disk-only bounded getter/caller discovery. All outputs use render_diag_reader_."""
from pathlib import Path
import bisect, hashlib, json, re, struct, sys
import capstone, pefile, numpy as np
source=Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
root=Path(__file__).resolve().parent
raw=source.read_bytes(); pe=pefile.PE(data=raw,fast_load=True); base=pe.OPTIONAL_HEADER.ImageBase
secs={s.Name.rstrip(b'\0').decode():(s.VirtualAddress,s.PointerToRawData,s.SizeOfRawData) for s in pe.sections}
tva,tro,tsz=secs['.text']; text=raw[tro:tro+tsz]
pva,pro,psz=secs['.pdata']; ranges=[]
for off in range(pro,pro+psz-11,12):
    s,e,u=struct.unpack_from('<III',raw,off)
    if s and e: ranges.append((s,e,u))
ranges.sort(); starts=[r[0] for r in ranges]
def fn(r):
    i=bisect.bisect_right(starts,r)-1
    return ranges[i][:2] if i>=0 and r<ranges[i][1] else None
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def dump(r,span):
    off=pe.get_offset_from_rva(r)
    return '\n'.join(f'{i.address-base:08X} raw={pe.get_offset_from_rva(i.address-base):08X} {i.bytes.hex(" "):<30} {i.mnemonic} {i.op_str}' for i in md.disasm(raw[off:off+span],base+r))
mode=sys.argv[1] if len(sys.argv)>1 else 'scan'
lines=[f'source={source}',f'sha256={hashlib.sha256(raw).hexdigest()}',f'base={base:#x}',f'mode={mode}']
if mode=='scan':
    a=list(re.finditer(rb'\x66[\x40-\x4f]?\x83[\x78-\x7b\x7d-\x7f]\x6a\x00',text))
    b=[m.start() for m in re.finditer(rb'[\x40-\x4f]?\x80[\x78-\x7b\x7d-\x7f]\x69\x00',text)]
    lines.append(f'cmpword6a={len(a)} cmpbyte69={len(b)}')
    for m in a:
        close=[x for x in b if abs(x-m.start())<96]
        if not close: continue
        r=tva+m.start(); bounds=fn(r)
        lines.append(f'\nCANDIDATE {r:08X} pdata={bounds} byte69={[hex(tva+x) for x in close]}')
        if bounds and bounds[1]-bounds[0]<512: lines.append(dump(bounds[0],bounds[1]-bounds[0]))
        else: lines.append(dump(r,100))
elif mode=='xref':
    rel=np.frombuffer(text[:len(text)-3],dtype=np.uint8)
    offsets=np.flatnonzero(rel==0xe8)
    jmps=np.flatnonzero(rel==0xe9)
    for arg in sys.argv[2:]:
        target=int(arg,16); lines.append(f'\nTARGET {target:08X}')
        for op,idxs in [('call',offsets),('jmp',jmps)]:
            for idx in idxs:
                idx=int(idx)
                if idx+5>len(text): continue
                d=struct.unpack_from('<i',text,idx+1)[0]
                if tva+idx+5+d==target:
                    r=tva+idx; lines.append(f'{op} {r:08X} pdata={fn(r)} bytes={text[idx:idx+5].hex()}')
elif mode=='type':
    for arg in sys.argv[2:]:
        vt=int(arg,16); vo=pe.get_offset_from_rva(vt); col=struct.unpack_from('<Q',raw,vo-8)[0]-base
        co=pe.get_offset_from_rva(col); sig,delta,cd,td,chd,selfr=struct.unpack_from('<IIIIII',raw,co)
        no=pe.get_offset_from_rva(td)+16; end=raw.index(b'\0',no)
        lines.append(f'vftable={vt:08X} COL={col:08X} TD={td:08X} sig={sig} self={selfr:X} offset={delta:X} name={raw[no:end].decode(errors="replace")}')
elif mode=='ptr':
    for arg in sys.argv[2:]:
        target=int(arg,16); needle=struct.pack('<Q',base+target); at=raw.find(needle)
        lines.append(f'\nPOINTER TARGET {target:08X}')
        while at>=0:
            r=pe.get_rva_from_offset(at); lines.append(f'pointer RVA {r:08X}')
            for back in range(0,0x300,8):
                off=at-back-8
                if off<0: break
                col=struct.unpack_from('<Q',raw,off)[0]-base
                try:
                    co=pe.get_offset_from_rva(col)
                    sig,delta,cd,td,chd,selfr=struct.unpack_from('<IIIIII',raw,co)
                    if sig==1 and selfr==col:
                        no=pe.get_offset_from_rva(td)+16; end=raw.index(b'\0',no)
                        lines.append(f'vftable={pe.get_rva_from_offset(off+8):08X} slot={back:X} COL={col:08X} TD={td:08X} offset={delta:X} name={raw[no:end].decode(errors="replace")}')
                        lines.append(f'raw_slot={raw[at:at+8].hex()} raw_COL={raw[co:co+24].hex()} raw_TD_name={raw[no-16:end+1].hex()}')
                        break
                except Exception: pass
            at=raw.find(needle,at+1)
elif mode=='dump':
    for arg in sys.argv[2:]:
        parts=arg.split(':'); r=int(parts[0],16); bounds=fn(r)
        span=int(parts[1],16) if len(parts)>1 else (bounds[1]-r if bounds else 0x100)
        lines.append(f'\nDUMP {r:08X} span={span:X} pdata={bounds}\n'+dump(r,span))
suffix=('_'+sys.argv[2].replace(':','_')) if len(sys.argv)>2 else ''
out=root/f'render_diag_reader_{mode}{suffix}.txt'; out.write_text('\n'.join(lines)+'\n',encoding='utf-8'); print('\n'.join(lines)); print(f'OUTPUT={out}')
