#!/usr/bin/env python3
"""Emit verified disk-only diagnostic addresses; never opens a process."""
import hashlib
import json
from pathlib import Path
import console_diag_static as s

expected = '21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca'
actual = hashlib.sha256(s.raw).hexdigest()
if actual != expected:
    raise SystemExit('Disk image hash mismatch: ' + actual)

def bytes_at(rva, length):
    at = s.off(rva)
    if at is None:
        raise ValueError('No raw mapping: ' + hex(rva))
    return s.raw[at:at+length].hex(' ')

gates = [
    (0x1874E8, 'UI visibility query', '74 09'),
    (0x187EF0, 'UI reveal-all getter consumed each frame by FOWTextureUpdate', '74 09'),
    (0x187F42, 'UI visibility query', '74 0d'),
    (0x1882E3, 'UI visibility query', '74 0d'),
    (0x189E26, 'UI visibility query', '74 09'),
    (0x1E9701, 'Local entity visibility cache to display mask', '74 0d'),
]
items = []
for rva, role, expected_bytes in gates:
    original = bytes_at(rva, 2)
    if original != expected_bytes:
        raise SystemExit(f'Unexpected instruction at {rva:X}: {original}')
    owner = s.func(rva)
    items.append({
        'rva': hex(rva), 'preferred_va': hex(s.base+rva),
        'raw_offset': hex(s.off(rva)), 'original': original,
        'tested_parent_replacement': '90 90',
        'role': role, 'function_begin_rva': hex(owner[0]),
        'window_rva': hex(rva-7), 'window_original': bytes_at(rva-7, 28),
    })

proofs = {}
for name in ('tick_rw', 'entity_rw', 'context_rw', 'simcase', 'foweffect'):
    file = s.OUT / ('console_diag_' + name + '_rea.json')
    if file.exists():
        obj = json.loads(file.read_text(encoding='utf-8-sig'))
        proofs[name] = {
            'json': str(file), 'evidence_id': obj.get('evidence_id'),
            'is_error': 'error' in obj,
        }

manifest = {
    'source': str(s.EXE), 'size': len(s.raw), 'sha256': actual,
    'preferred_image_base': hex(s.base), 'process_access_performed_by_this_script': False,
    'six_display_gates': items,
    'console': {
        'manager_slot_rva': '0x6ba4430', 'vector_begin_offset': '0x0',
        'vector_end_offset': '0x8', 'element_stride': 8,
        'console_vtable_rva': '0x3e3a5c8',
        'opened_byte_offset': '0x10', 'drawing_byte_offset': '0x11',
        'animation_float_offset': '0x18',
        'opened_getter_rva': '0x14f6680', 'opened_getter_original': bytes_at(0x14F6680, 5),
        'type_getter_rva': '0x14f9170', 'type_getter_original': bytes_at(0x14F9170, 6),
        'open_functor_rva': '0x14e0550', 'open_functor_original': bytes_at(0x14E0550, 17),
        'global_mode_tag': '0x474c5541', 'scar_mode_tag': '0x53434152',
        'scar_gate_rva': '0xb40b10', 'scar_gate_original': bytes_at(0xB40B10, 13),
    },
    'local_display_state': {
        'ui_object_rva': '0x6a887a8', 'ui_all_byte_rva': '0x6a888a0',
        'ui_entities_byte_rva': '0x6a888a1',
        'manager_slot_rva': '0x6b02cc0', 'manager_alloc_size': '0x48',
        'manager_enabled_byte_offset': '0x40', 'manager_all_byte_offset': '0x41',
        'manager_constructor_rva': '0x128f860',
        'manager_constructor_original': bytes_at(0x128F860, 0x45),
        'manager_init_qword_offsets': {'0x30': 32, '0x38': 32},
        'manager_has_proven_vtable': False,
        'render_root_slot_rva': '0x6185f18', 'render_data_pointer_offset': '0x50',
        'render_data_enabled_byte_offset': '0x5e8', 'render_data_all_byte_offset': '0x5e9',
        'render_data_alloc_size': '0x648', 'render_data_allocation_site_rva': '0x4b32b3',
        'render_data_allocation_original': bytes_at(0x4B32B3, 5),
        'render_data_publish_site_rva': '0x4b32e5',
        'render_data_publish_original': bytes_at(0x4B32E5, 4),
        'render_data_constructor_rva': '0x4b1d50',
        'render_data_prefix_zero_original': bytes_at(0x4B1D6C, 9),
        'render_data_bool_init_rva': '0x4b1f73',
        'render_data_bool_init_original': bytes_at(0x4B1F73, 9),
        'render_data_has_proven_vtable': False,
        'render_task_rva': '0x1045700', 'render_task_original': bytes_at(0x1045700, 14),
        'tick_function_rva': '0xc964a0', 'ui_getter_rva': '0x187ee0',
        'queued_setter_rva': '0xc96650',
        'manager_bool_write_rva': '0xc96685',
        'manager_bool_write_original': bytes_at(0xC96685, 4),
    },
    'entity_formula': 'revealWanted = context1884A0() && UI_A1; visible = (flags & 4) != 0 && (((flags & 0x10) != 0) || revealWanted); setterArg3 = ((flags & 2) != 0 || visible) ? 0 : 1; call1EA840(displayEntity, 2, setterArg3)',
    'rea_proofs': proofs,
    'limits': [
        'No game process input, reads, writes or injections performed by this child/script.',
        'Patch replacements are parent-tested descriptors only; this script does not apply them.',
        'D and manager are data structures with no vtable assignment in the reviewed constructors; pointer shape checks are not RTTI identity proof.',
        'UI visibility queries may also be consumed by other local UI functions; not an exhaustive caller-side-effect audit.',
        'Parent runtime results are separate evidence; no independent child gameplay validation.',
    ],
}
output = s.OUT / 'console_diag_manifest.json'
output.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(str(output))
print('REA_EVIDENCE', json.dumps(proofs, ensure_ascii=False))
