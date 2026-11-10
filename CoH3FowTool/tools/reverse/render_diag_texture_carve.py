"""Carve read-only rendering methods at original RVAs, including chained unwind fragments.
Preserves source section permissions and selected exception metadata. The resulting
PE is an analysis artifact, not an executable patch or a whole-program audit.
"""
from pathlib import Path
import bisect
import hashlib
import json
import struct
from collections import deque
import capstone
from capstone.x86_const import X86_OP_IMM, X86_OP_MEM, X86_REG_RIP
import pefile
from make_mini_pe import build_pe

ROOT = Path(__file__).resolve().parent
SOURCE = Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
EXPECTED = '21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca'
raw = SOURCE.read_bytes()
assert hashlib.sha256(raw).hexdigest() == EXPECTED
pe = pefile.PE(data=raw, fast_load=True)
BASE = pe.OPTIONAL_HEADER.ImageBase
PAGE = 0x1000
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
md.detail = True
exception = pe.OPTIONAL_HEADER.DATA_DIRECTORY[3]
pdata = pe.get_data(exception.VirtualAddress, exception.Size)
ranges = sorted(v for v in struct.iter_unpack('<III', pdata[:len(pdata)//12*12])
                if v[0] and v[1] > v[0])
starts = [v[0] for v in ranges]
notes = []

def section(rva):
    return next((s for s in pe.sections if s.VirtualAddress <= rva <
                 s.VirtualAddress + max(s.Misc_VirtualSize, s.SizeOfRawData)), None)

def source_bytes(rva, size):
    s = section(rva)
    assert s is not None, hex(rva)
    rel = rva - s.VirtualAddress
    available = max(0, min(size, s.SizeOfRawData-rel))
    return raw[s.PointerToRawData+rel:s.PointerToRawData+rel+available] + bytes(size-available)

def info(entry):
    u = entry[2]
    if u & 3:
        raise ValueError('Unaligned unwind RVA: '+hex(u))
    head = source_bytes(u, 4)
    ncode, flags = head[2], head[0] >> 3
    size = (4+ncode*2+3) & ~3
    if flags & 4:
        parent = struct.unpack('<III', source_bytes(u+size, 12))
        return size+12, parent
    return size+(4 if flags & 3 else 0), None

root_cache = {}
def unwind_root(entry, seen=None):
    if entry in root_cache:
        return root_cache[entry]
    if seen is None:
        seen = set()
    if entry in seen:
        raise ValueError('Cyclic unwind chain')
    seen.add(entry)
    _, parent = info(entry)
    result = unwind_root(parent, seen) if parent else entry
    root_cache[entry] = result
    return result

# Exception records, rather than a first contiguous range, identify hot/cold parts.
groups = {}
for record in ranges:
    key = unwind_root(record)
    groups.setdefault(key, []).append(record)

def containing(rva):
    i = bisect.bisect_right(starts, rva)-1
    if i >= 0 and ranges[i][0] <= rva < ranges[i][1]:
        return ranges[i]
    return None

def leaf(rva):
    for ins in md.disasm(source_bytes(rva, 256), BASE+rva):
        if ins.mnemonic in ('ret', 'retf', 'jmp'):
            return (rva, ins.address-BASE+ins.size, 0)
    raise ValueError('No bounded leaf ending for '+hex(rva))

SEEDS = {
    'fog_block_constructor': 0x4B1D50,
    'reveal_task_enqueue': 0xC96650,
    'reveal_task_execute': 0x1045700,
    'fog_field_packet_reader': 0x60F900,
    'field_pack_snapshot': 0x15713E8,
}
selected = {}
queue = deque(SEEDS.values())
while queue:
    rva = queue.popleft()
    record = containing(rva)
    key = unwind_root(record) if record else leaf(rva)
    if key in selected:
        continue
    fragments = groups[key] if record else [key]
    total = sum(z-a for a,z,_ in fragments)
    assert total <= 0x20000, (hex(rva), total)
    assert len(selected) < 64, 'Branch closure too large'
    selected[key] = fragments
    for a,z,_ in fragments:
        for ins in md.disasm(source_bytes(a,z-a), BASE+a):
            # Follow out-of-line branches, not every transitive function call.
            if ins.group(capstone.CS_GRP_JUMP) and ins.operands[0].type == X86_OP_IMM:
                target = ins.operands[0].imm-BASE
                target_section = section(target)
                if target_section and target_section.Characteristics & 0x20000000:
                    if not any(x <= target < y for x,y,_ in fragments):
                        queue.append(target)

code_records = sorted(set(v for rows in selected.values() for v in rows))
permissions = {}
def add_page(rva):
    s = section(rva)
    if s is None:
        return False
    page = rva & -PAGE
    permissions[page] = s.Characteristics
    return True

def add_span(rva, size):
    for page in range(rva & -PAGE, (rva+size+PAGE-1) & -PAGE, PAGE):
        add_page(page)

unwind_nodes = set()
def add_unwind(entry):
    if entry[2] == 0 or entry in unwind_nodes:
        return
    unwind_nodes.add(entry)
    size, parent = info(entry)
    add_span(entry[2], size)
    if parent:
        add_unwind(parent)

calls, references, strings = set(), set(), set()
for a,z,u in code_records:
    add_span(a,z-a)
    add_unwind((a,z,u))
    for ins in md.disasm(source_bytes(a,z-a), BASE+a):
        if ins.mnemonic == 'call' and ins.operands[0].type == X86_OP_IMM:
            calls.add(ins.operands[0].imm-BASE)
        for operand in ins.operands:
            if operand.type == X86_OP_MEM and operand.mem.base == X86_REG_RIP:
                target = ins.address+ins.size+operand.mem.disp-BASE
                target_section = section(target)
                references.add(target)
                if target_section and not target_section.Characteristics & 0x20000000:
                    add_span(target,16)
                    val = source_bytes(target, min(240, target_section.VirtualAddress+
                                                   max(target_section.Misc_VirtualSize,target_section.SizeOfRawData)-target))
                    val = val.split(b'\0',1)[0]
                    if len(val) >= 4 and all(32 <= x < 127 for x in val):
                        strings.add(val.decode('ascii'))

# Original section permissions are retained for each included data page.

runs = []
for page, flags in sorted(permissions.items()):
    if runs and runs[-1][1] == page and runs[-1][2] == flags:
        runs[-1][1] += PAGE
    else:
        runs.append([page,page+PAGE,flags])
sections = []
for i,(a,z,flags) in enumerate(runs):
    prefix = 'c' if flags & 0x20000000 else ('d' if flags & 0x80000000 else 'r')
    sections.append((prefix+str(i),a,b''.join(source_bytes(p,PAGE) for p in range(a,z,PAGE)),flags))
# A minimal selected exception directory lets Ghidra join chained code fragments.
pdata_rva = (max(z for a,z,_ in runs)+PAGE-1) & -PAGE
pdata_bytes = b''.join(struct.pack('<III',*v) for v in code_records if v[2])
sections.append(('.pdata',pdata_rva,pdata_bytes,0x40000040))
blob = bytearray(build_pe(sections, SEEDS['fog_block_constructor']))
opt = struct.unpack_from('<I',blob,0x3C)[0]+24
struct.pack_into('<II',blob,opt+0x70+3*8,pdata_rva,len(pdata_bytes))
output = ROOT/'render_diag_texture_rw.exe'
assert not output.exists(), 'Choose a new output name to preserve existing evidence'
output.write_bytes(blob)
manifest = {
    'source': str(SOURCE), 'source_sha256':EXPECTED, 'image_base':hex(BASE),
    'analysis_copy':str(output), 'analysis_copy_sha256':hashlib.sha256(blob).hexdigest(),
    'seeds':{name:hex(rva) for name,rva in SEEDS.items()},
    'function_groups':[{'root':[hex(v) for v in key],
                        'fragments':[[hex(v) for v in row] for row in rows]}
                       for key,rows in selected.items()],
    'sections':[{'name':name,'rva':hex(a),'size':len(data),'flags':hex(flags)}
                for name,a,data,flags in sections],
    'external_direct_calls':[hex(x) for x in sorted(calls)
                             if not any(a <= x < z for a,z,_ in code_records)],
    'rip_targets':[hex(x) for x in sorted(references)], 'ascii_strings':sorted(strings),
    'limitations':['Static carved PE; no process or network operations.',
                   'External direct and indirect callees are not a complete program.',
                   'Original section page permissions and chained unwind fragments retained.',
                   'No claim about online activation, reporting, detection or account actions.'],
}
(ROOT/'render_diag_texture_carve.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('Analysis PE:', output, 'bytes=', len(blob), 'sections=',len(sections))
print('SHA256:',manifest['analysis_copy_sha256'])
print('Function groups=',len(selected),'fragments=',len(code_records))
for name,rva in SEEDS.items():
    record=containing(rva)
    key=unwind_root(record) if record else leaf(rva)
    print(name,hex(rva),'fragments=',len(selected[key]),'bytes=',sum(z-a for a,z,u in selected[key]))
print('ASCII strings:',sorted(strings))
