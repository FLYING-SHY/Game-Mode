#!/usr/bin/env python3
"""Offline image resolver. Manifest ground truth is used only after candidates are saved."""
import argparse
import hashlib
import json
from pathlib import Path
import struct

PATTERN = '48 8B 81 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9'
OBSERVED = [100, 73, 91]  # Controlled action record supplied by fixture protocol.


def save(path, data):
    path.write_text(json.dumps(data, indent=2) + '\n', encoding='utf-8')


def scan(data):
    pattern = [None if x == '??' else int(x, 16) for x in PATTERN.split()]
    return [i for i in range(len(data) - len(pattern) + 1)
            if all(x is None or data[i+j] == x for j, x in enumerate(pattern))]


def read(base, data, address, fmt):
    size = struct.calcsize(fmt)
    offset = address - base
    if offset < 0 or offset + size > len(data):
        raise ValueError('read outside single image')
    return struct.unpack_from(fmt, data, offset)[0]


def load(directory, spec):
    name = spec['file']
    if Path(name).name != name or '/' in name or '\\' in name:
        raise ValueError('image filename must be local basename')
    data = (directory / name).read_bytes()
    if hashlib.sha256(data).hexdigest() != spec['sha256']:
        raise ValueError('image hash mismatch')
    return data


def containers(directory):
    # Explicit allowlist: no root/layout/address/expected-values fields reach discovery.
    manifest = json.loads((directory / 'manifest.json').read_text(encoding='utf-8'))
    return [{k: s[k] for k in ('id', 'build', 'module_base', 'heap_base', 'frames', 'aob')}
            for s in manifest['sessions']]


def discover(directory):
    records, aob_records, sessions = [], [], []
    for s in containers(directory):
        base, hb = s['module_base'], s['heap_base']
        modules = {}
        for mode, spec in s['aob'].items():
            module = load(directory, spec)
            modules[mode] = module
            hits = scan(module)
            decoded = []
            for hit in hits:
                disp = struct.unpack_from('<i', module, hit + 10)[0]
                root = base + hit + 14 + disp
                field = struct.unpack_from('<i', module, hit + 3)[0]
                decoded.append({'hit_rva': hit, 'next_instruction': base + hit + 14,
                                'signed_rip_displacement': disp, 'root_address': root,
                                'field_displacement': field})
            aob_records.append({'session': s['id'], 'mode': mode, 'count': len(hits),
                                'accepted': len(hits) == 1, 'decoded': decoded})
        module = modules['unique']
        hits = scan(module)
        if len(hits) != 1:
            raise ValueError('unique image has ambiguous or missing signature')
        hit = hits[0]
        root = base + hit + 14 + struct.unpack_from('<i', module, hit + 10)[0]
        field = struct.unpack_from('<i', module, hit + 3)[0]
        manager = read(base, module, root, '<Q')
        frames = [load(directory, f) for f in s['frames']]
        if len(frames) != len(OBSERVED) or len({len(f) for f in frames}) != 1:
            raise ValueError('invalid controlled frame set')
        # Byte-wise int32 scan, including the last readable start. No known field address.
        same_value = []
        for offset in range(len(frames[0]) - 4 + 1):
            values = [struct.unpack_from('<i', frame, offset)[0] for frame in frames]
            if values[0] == OBSERVED[0]:
                same_value.append({'address': hb + offset, 'values': values,
                                   'accepted': values == OBSERVED})
        changing = {c['address'] for c in same_value if c['accepted']}
        # Scan all possible byte positions at/after manager for heap pointers.
        # No manager-to-actor offset, object extent or alignment is supplied.
        links = []
        for slot in range(max(manager, hb), hb + len(frames[0]) - 8 + 1):
            actors = [read(hb, frame, slot, '<Q') for frame in frames]
            if not all(hb <= actor and actor + field in changing for actor in actors):
                continue
            values = [read(hb, frame, actor + field, '<i') for frame, actor in zip(frames, actors)]
            if values == OBSERVED and len(set(actors)) == 1:
                links.append({'ptr_offset': slot - manager, 'pointer_slot': slot,
                              'actor': actors[0], 'field_address': actors[0] + field,
                              'values': values})
        if len(changing) != 1 or len(links) != 1:
            raise ValueError('dynamic/structural candidates remain ambiguous')
        link = links[0]
        record = {'id': s['id'], 'build': s['build'], 'module_base': base,
                  'root_rva': root-base, 'root_address': root, 'manager': manager,
                  'health_offset': field, 'ptr_offsets': [link['ptr_offset']],
                  'resolved_address': link['field_address']}
        records.append(record)
        sessions.append({'session': s['id'], 'module_bytes_scanned': len(module),
                         'heap_bytes_per_frame': len(frames[0]), 'frames': len(frames),
                         'pointer_scan_start': max(manager, hb),
                         'pointer_scan_end_exclusive': hb + len(frames[0]) - 8 + 1,
                         'initial_same_value_candidates': same_value,
                         'structural_candidates': links, 'resolution': record})
    candidates = []
    for build in sorted({r['build'] for r in records}):
        group = [r for r in records if r['build'] == build]
        layouts = {(r['root_rva'], tuple(r['ptr_offsets']), r['health_offset']) for r in group}
        if len(group) < 2 or len(layouts) != 1:
            raise ValueError('cross-session layout inconsistent')
        r = group[0]
        candidates.append({'build': build, 'root_rva': r['root_rva'],
                           'ptr_offsets': r['ptr_offsets'], 'health_offset': r['health_offset'],
                           'sessions': [x['id'] for x in group],
                           'resolved_addresses': {x['id']: x['resolved_address'] for x in group}})
    return {'schema': 1, 'candidates': candidates}, {'aob': aob_records, 'sessions': sessions}


def final_ground_truth_check(directory, result):
    manifest = json.loads((directory / 'manifest.json').read_text(encoding='utf-8'))
    checks = []
    for c in result['candidates']:
        for s in manifest['sessions']:
            if s['id'] in c['sessions']:
                checks.append({'session': s['id'], 'matches':
                               c['root_rva'] == s['pointer_root']['rva'] and
                               c['ptr_offsets'] == s['field_layout']['ptr_offsets'] and
                               c['health_offset'] == s['field_layout']['health_offset'] and
                               c['resolved_addresses'][s['id']] == s['health_address']})
    return {'stage': 'after candidate serialization only', 'checks': checks,
            'passed': all(c['matches'] for c in checks)}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--directory', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    result, evidence = discover(args.directory)
    save(args.output / 'candidates.json', result)
    save(args.output / 'resolver-evidence.json', evidence)
    check = final_ground_truth_check(args.directory, result)
    save(args.output / 'ground-truth-check.json', check)
    print(json.dumps({'discovery': result, 'final_ground_truth': check}, indent=2))
    return 0 if check['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
