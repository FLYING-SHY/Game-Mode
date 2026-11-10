"""Bounded disk-only crosscheck; writes only online_diag_entity_crosscheck* artifacts."""
from pathlib import Path
import hashlib, json, re, struct
import capstone, pefile

ROOT = Path(__file__).resolve().parent
EXE = Path(r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe")
raw = EXE.read_bytes()
pe = pefile.PE(data=raw, fast_load=True)
BASE = pe.OPTIONAL_HEADER.ImageBase
secs = [(s.Name.rstrip(b"\0").decode(), s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData) for s in pe.sections]
def off(rva):
    for _, va, fo, sz in secs:
        if va <= rva < va + sz: return fo + rva - va
    raise ValueError(hex(rva))
def rva(fo):
    for _, va, po, sz in secs:
        if po <= fo < po + sz: return va + fo - po
    return None
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
lines = [f"source={EXE}", f"size={len(raw)}", f"sha256={hashlib.sha256(raw).hexdigest()}", f"image_base={BASE:#x}", "Disk-only: no process, patch, input, or network operations."]
for start, span, label in [(0xCEEE30,0x223,"AC periodic function"),(0x1E96E0,0x107,"display cache path"),(0x5DAAD0,0xAD,"shared handle resolver"),(0x1EA840,0x60,"mask setter"),(0x466DD0,0x40,"signed-word setter"),(0x467100,0x280,"aggregate-state notification")]:
    lines.append(f"\n{label}: RVA={start:#x} raw={off(start):#x} span={span:#x}")
    for ins in md.disasm(raw[off(start):off(start)+span], BASE+start):
        lines.append(f"RVA {ins.address-BASE:08X} raw {off(ins.address-BASE):08X} {ins.bytes.hex(' '):<32} {ins.mnemonic} {ins.op_str}")
# Bounded type-name inventory: names are candidates, never proof of lookup return type.
candidates=[]
for m in re.finditer(rb'\.\?A[UV][^\x00\r\n]{1,180}\x00',raw):
    name=m.group()[:-1].decode('ascii','replace')
    if 'Entity' in name and not any(t in name for t in ('?$','Delegate','Command','Event','Task','Component','Ability','Modifier','Iterator')):
        candidates.append({'name':name,'string_rva':hex(rva(m.start()))})
lines.append("\nRTTI candidate inventory (name presence only, no asserted binding to returned object):")
lines += [json.dumps(x,ensure_ascii=False) for x in candidates[:100]]
(ROOT/'online_diag_entity_crosscheck_asm.txt').write_text('\n'.join(lines)+'\n',encoding='utf-8')
manifest={'source':str(EXE),'size':len(raw),'sha256':hashlib.sha256(raw).hexdigest(),'base':hex(BASE),'sections':secs,'rtti_candidates':candidates,'sites':{'ac_lookup_call':'0xCEEF41','display_lookup_call':'0x1E97B0','display_pass_return_to_setter':'0x1E97C0','display_setter_call':'0x1E97C3','mask_write':'0x1EA874','ac_signed_word_compare':'0xCEEF4F','ac_mask_compare':'0xCEEF56'},'process_access':False,'original_binary_modified':False}
(ROOT/'online_diag_entity_crosscheck_manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('\n'.join(lines[:5]))
print('RTTI candidate count:',len(candidates))
print('\n'.join(json.dumps(x,ensure_ascii=False) for x in candidates[:70]))
print('Saved:',ROOT/'online_diag_entity_crosscheck_asm.txt')
