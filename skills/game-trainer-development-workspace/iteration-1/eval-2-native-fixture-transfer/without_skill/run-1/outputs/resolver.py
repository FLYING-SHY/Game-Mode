"""Resolve offline images by bytes and controlled value changes, never oracle offsets."""
import argparse
import json
from pathlib import Path
import struct

PATTERN = [None if x == '??' else int(x, 16) for x in
           '48 8B 81 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9'.split()]
EXPECTED = [100, 73, 91]  # Controlled experiment observations, not manifest fields.


def scan(data):
    return [i for i in range(len(data) - len(PATTERN) + 1)
            if all(b is None or data[i+j] == b for j, b in enumerate(PATTERN))]


def local_file(directory, name):
    if Path(name).name != name or '/' in name or '\\' in name:
        raise ValueError('nonlocal image filename')
    return (directory / name).read_bytes()


def read(data, base, address, fmt):
    offset = address - base
    size = struct.calcsize(fmt)
    if offset < 0 or offset + size > len(data):
        raise ValueError('read outside single image')
    return struct.unpack_from(fmt, data, offset)[0]


def resolve(directory):
    # Explicit allowlist: oracle root/layout/address/expected/hash fields never accessed.
    raw = json.loads((directory / 'manifest.json').read_text(encoding='utf-8'))
    sessions = [dict(id=s['id'], build=s['build'], module_base=s['module_base'],
                     heap_base=s['heap_base'],
                     modules={mode: spec['file'] for mode, spec in s['aob'].items()},
                     frames=[f['file'] for f in s['frames']]) for s in raw['sessions']]
    del raw
    evidence, grouped = [], {}
    for s in sessions:
        mb, hb = s['module_base'], s['heap_base']
        modes = []
        for mode, filename in s['modules'].items():
            image = local_file(directory, filename)
            hits = scan(image)
            decoded = [dict(hit_rva=h,
                            field_displacement=struct.unpack_from('<i', image, h+3)[0],
                            rip_next=mb+h+14,
                            rip_displacement=struct.unpack_from('<i', image, h+10)[0],
                            root_address=mb+h+14+struct.unpack_from('<i', image, h+10)[0])
                       for h in hits]
            modes.append(dict(mode=mode, hits=hits, decoded=decoded,
                              status='zero' if not hits else 'unique' if len(hits)==1 else 'multiple',
                              accepted=len(hits)==1))
        selected = next(x for x in modes if x['mode']=='unique')
        if not selected['accepted']:
            raise ValueError('unique image has ambiguous or missing instruction')
        d = selected['decoded'][0]
        module = local_file(directory, s['modules']['unique'])
        heaps = [local_file(directory, f) for f in s['frames']]
        if len(heaps) != len(EXPECTED) or len({len(x) for x in heaps}) != 1:
            raise ValueError('inconsistent controlled frames')
        manager = read(module, mb, d['root_address'], '<Q')
        # First-frame equality intentionally retains both decoys; later frames filter them.
        initial = []
        dynamic = []
        for off in range(0, len(heaps[0])-3, 4):
            values = [struct.unpack_from('<i', h, off)[0] for h in heaps]
            if values[0] == EXPECTED[0]:
                initial.append(dict(address=hb+off, values=values, retained=values==EXPECTED))
            if values == EXPECTED:
                dynamic.append(hb+off)
        chains = []
        # Search all aligned pointer slots from manager through the mapped heap.
        # No manager->actor offset is supplied; validate pointer, decoded field, and all frames.
        for slot in range(0, len(heaps[0])-7, 8):
            slot_address = hb+slot
            if slot_address < manager:
                continue
            pointers = [struct.unpack_from('<Q', h, slot)[0] for h in heaps]
            if len(set(pointers)) != 1:
                continue
            actor = pointers[0]
            address = actor + d['field_displacement']
            if hb <= actor < hb+len(heaps[0]) and address in dynamic:
                values = [read(h, hb, address, '<i') for h in heaps]
                if values == EXPECTED:
                    chains.append(dict(pointer_slot=slot_address, ptr_offset=slot_address-manager,
                                       actor=actor, address=address, values=values))
        if len(chains) != 1:
            raise ValueError('expected one structure and dynamic-value-supported chain')
        c = chains[0]
        signature = (d['root_address']-mb, c['ptr_offset'], d['field_displacement'])
        group = grouped.setdefault(s['build'], dict(signature=signature, sessions=[], resolved_addresses={}))
        if group['signature'] != signature:
            raise ValueError('layout did not transfer across sessions')
        group['sessions'].append(s['id'])
        group['resolved_addresses'][s['id']] = c['address']
        evidence.append(dict(session=s['id'], module_base=mb, heap_base=hb, aob=modes,
                             manager=manager, initial_value_candidates=initial,
                             dynamic_candidates=dynamic, chains=chains))
    candidates = []
    for build, g in sorted(grouped.items()):
        if len(set(g['sessions'])) < 2:
            raise ValueError('need multiple sessions per build')
        root, link, field = g['signature']
        candidates.append(dict(build=build, root_rva=root, ptr_offsets=[link], health_offset=field,
                               sessions=g['sessions'], resolved_addresses=g['resolved_addresses']))
    return dict(schema=1, candidates=candidates), dict(schema=1, sessions=evidence)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--directory', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    candidate, evidence = resolve(args.directory)
    args.output.mkdir(parents=True, exist_ok=True)
    (args.output/'candidates.json').write_text(json.dumps(candidate, indent=2)+'\n', encoding='utf-8')
    (args.output/'resolver-evidence.json').write_text(json.dumps(evidence, indent=2)+'\n', encoding='utf-8')
    print(json.dumps(dict(passed=True, candidates=candidate['candidates'], evidence_sessions=len(evidence['sessions'])), indent=2))
