"""Bounded original-file evidence for 374AF68 and direct key callees; no target execution."""
from pathlib import Path
import hashlib
import pefile, capstone
root=Path(__file__).resolve().parent
source=Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
data=source.read_bytes(); pe=pefile.PE(data=data,fast_load=True)
base=pe.OPTIONAL_HEADER.ImageBase
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
regions=[(0x374AF68,0x11e),(0x374AD7C,0x182),(0x374AC24,0x14c),(0x2496120,0x68),(0x2496188,0x68),(0x62E150,0xe0),(0x62E230,0xe0),(0x4FCB00,0x60),(0x183490,0x2b),(0x374C0B0,0xcd)]
lines=[f'source={source}',f'sha256={hashlib.sha256(data).hexdigest()}',f'base={base:#x}','Static original-file reads only; no game process/network/input/patch operations.']
for start,span in regions:
    raw=pe.get_offset_from_rva(start)
    lines.append(f'\nRVA {start:08X} raw {raw:08X} span {span:X}')
    for ins in md.disasm(data[raw:raw+span],base+start):
        irva=ins.address-base
        lines.append(f'RVA {irva:08X} raw {pe.get_offset_from_rva(irva):08X} {ins.bytes.hex(" "):<32} {ins.mnemonic} {ins.op_str}')
out=root/'online_diag_entity_crosscheck_predicate_asm.txt'
out.write_text('\n'.join(lines)+'\n',encoding='utf-8')
print(out)
