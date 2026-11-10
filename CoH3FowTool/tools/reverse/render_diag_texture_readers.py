"""Static inventory of the render block's fog field readers; no game process access."""
from pathlib import Path
import bisect
import hashlib
import json
import struct
import capstone
from capstone.x86_const import X86_OP_MEM, X86_REG_RIP
import pefile

ROOT = Path(__file__).resolve().parent
SOURCE = Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
EXPECTED = '21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca'
raw = SOURCE.read_bytes()
assert hashlib.sha256(raw).hexdigest() == EXPECTED
pe = pefile.PE(data=raw, fast_load=True)
BASE = pe.OPTIONAL_HEADER.ImageBase
text = next(s for s in pe.sections if s.Name.rstrip(b'\0') == b'.text')
start, end = text.PointerToRawData, text.PointerToRawData + text.SizeOfRawData
ex = pe.OPTIONAL_HEADER.DATA_DIRECTORY[3]
rows = sorted(t for t in struct.iter_unpack('<III', pe.get_data(ex.VirtualAddress, ex.Size)) if t[0] < t[1])
starts = [t[0] for t in rows]
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
md.detail = True

def function_at(rva):
    index = bisect.bisect_right(starts, rva) - 1
    return rows[index] if index >= 0 and rows[index][0] <= rva < rows[index][1] else None

def describe(ins):
    return {'rva': hex(ins.address - BASE), 'bytes': ins.bytes.hex(' '), 'asm': ins.mnemonic + ' ' + ins.op_str}

candidates = set()
for disp in (0x5E8, 0x5E9):
    token = struct.pack('<I', disp)
    off = raw.find(token, start, end)
    while off != -1:
        for before in range(2, 9):
            pos = off - before
            if pos < start:
                continue
            ins = next(md.disasm(raw[pos:pos + 15], BASE + pe.get_rva_from_offset(pos), 1), None)
            if ins is not None and pos + ins.size >= off + 4 and any(
                o.type == X86_OP_MEM and o.mem.disp == disp and o.mem.base != X86_REG_RIP for o in ins.operands):
                candidates.add(ins.address - BASE)
        off = raw.find(token, off + 1, end)

results, seen = [], set()
for address in sorted(candidates):
    fn = function_at(address)
    if fn is None:
        continue
    a, z, unwind = fn
    decoded = list(md.disasm(pe.get_data(a, z - a), BASE + a))
    for index, ins in enumerate(decoded):
        if ins.address - BASE not in candidates or ins.address in seen:
            continue
        seen.add(ins.address)
        matching = [o for o in ins.operands if o.type == X86_OP_MEM and o.mem.disp in (0x5E8, 0x5E9)]
        if not matching:
            continue
        item = describe(ins)
        item['function'] = [hex(a), hex(z), hex(unwind)]
        item['context'] = [describe(v) for v in decoded[max(0, index - 8):index + 9]]
        results.append(item)

out = {'source': str(SOURCE), 'sha256': EXPECTED, 'image_base': hex(BASE), 'field_candidates': results,
       'limitations': ['Static displacement matches; object type and actual execution need separate data-flow validation.']}
(ROOT / 'render_diag_texture_readers.json').write_text(json.dumps(out, indent=2), encoding='utf-8')
lines = []
for item in results:
    lines.append(f"Function {item['function']} field instruction {item['rva']}")
    lines.extend(f"  {v['rva']:<12} {v['bytes']:<42} {v['asm']}" for v in item['context'])
    lines.append('')
(ROOT / 'render_diag_texture_readers.txt').write_text('\n'.join(lines), encoding='utf-8')
print('Verified source SHA256:', EXPECTED)
print('Instruction candidates:', len(results))
for item in results:
    print(item['rva'], 'function=', item['function'][:2], item['asm'])
