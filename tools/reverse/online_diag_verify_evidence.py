"""Verify on-disk provenance of the bounded REA anti-cheat investigation.
Reads PE files and existing REA artifacts only; never opens a game process.
"""
from pathlib import Path
import hashlib
import json
import pefile

ROOT = Path(__file__).resolve().parent
EXPECTED_SOURCE = '21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca'

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def load(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


manifests = [load(ROOT / 'online_diag_ac_carve.json'),
             load(ROOT / 'online_diag_ac_carve2.json')]
source = Path(manifests[0]['source'])
source_sha = sha(source)
assert source_sha == EXPECTED_SOURCE, 'Source executable changed'
original = pefile.PE(str(source), fast_load=True)
assert original.OPTIONAL_HEADER.ImageBase == 0x140000000
copies = []
for manifest in manifests:
    assert manifest['source_sha256'] == source_sha
    copy_path = Path(manifest['analysis_copy'])
    copy_sha = sha(copy_path)
    assert copy_sha == manifest['analysis_copy_sha256'], f'Analysis copy changed: {copy_path}'
    image = pefile.PE(str(copy_path), fast_load=True)
    assert image.OPTIONAL_HEADER.ImageBase == original.OPTIONAL_HEADER.ImageBase
    fragments = []
    for group in manifest['function_groups']:
        for begin, end, _unwind in group['fragments']:
            begin, end = int(begin, 16), int(end, 16)
            before = original.get_data(begin, end - begin)
            after = image.get_data(begin, end - begin)
            assert len(before) == end - begin and before == after, f'Code mismatch at {begin:#x}'
            fragments.append({'start_rva': hex(begin), 'end_rva_exclusive': hex(end),
                              'length': len(before)})
    for rva in (0x5E0BECC, 0x6185E00):
        section = image.get_section_by_rva(rva)
        assert section is not None and section.Characteristics & 0x80000000, 'Expected writable data'
    copies.append({'path': str(copy_path), 'sha256': copy_sha,
                   'function_groups': len(manifest['function_groups']),
                   'verified_fragments': len(fragments),
                   'verified_code_bytes': sum(x['length'] for x in fragments),
                   'fragments': fragments})

exports = []
for name, image_sha in [('online_diag_ac_methods_1', copies[0]['sha256']),
                        ('online_diag_ac_samples_1', copies[0]['sha256']),
                        ('online_diag_ac_predicates_1', copies[1]['sha256'])]:
    evidence = load(ROOT / f'{name}.json')
    normalized = evidence['normalized_result']
    counts = normalized['counts']
    assert normalized['target_sha256'] == image_sha, f'Evidence target mismatch: {name}'
    assert normalized['provider']['id'] == 'ghidra'
    assert normalized['provider']['version'] == '12.1.4'
    assert counts['failed'] == 0 and counts['written'] == counts['requested'], name
    exports.append({'name': name, 'evidence_id': evidence['evidence_id'],
                    'counts': counts, 'target_sha256': image_sha,
                    'profile': normalized['analysis_profile_digest']})

critical = [('online_diag_ac_methods_1', '0x140ceed80.c'),
            ('online_diag_ac_methods_1', '0x140ceee30.c'),
            ('online_diag_ac_samples_1', '0x14374b890.c'),
            ('online_diag_ac_samples_1', '0x14374bab8.c'),
            ('online_diag_ac_predicates_1', '0x14374c0b0.c'),
            ('online_diag_ac_predicates_1', '0x14374c180.c'),
            ('online_diag_ac_predicates_1', '0x14374b088.c')]
functions = []
for directory, filename in critical:
    path = ROOT / directory / 'functions' / filename
    text = path.read_text(encoding='utf-8-sig')
    assert text.strip() and 'halt_baddata' not in text, f'Invalid critical decompile: {path}'
    functions.append({'path': str(path), 'sha256': sha(path),
                      'warnings': [line.strip() for line in text.splitlines()
                                   if 'WARNING' in line or 'unresolved' in line or 'basis=unknown' in line]})

self_text = (ROOT / 'online_diag_ac_predicates_1/functions/0x14374bab8.c').read_text(encoding='utf-8-sig')
assert '0x6a' in self_text and '0x69' in self_text and '0x800' in self_text
records = load(ROOT / 'online_diag_ac_records.json')
assert records['record_type']['name'] == '.?AVVisibilityStatusData@@'

result = {'result': 'PASS', 'source': str(source), 'source_sha256': source_sha,
          'analysis_copies': copies, 'rea_exports': exports,
          'critical_function_artifacts': functions,
          'limits': ['Static byte and artifact provenance only; not a runtime alias check.',
                     'First export is intentionally limited: remaining functions are recorded.',
                     'Imported/external ABI, virtual targets and some inferred return types remain unresolved.',
                     'Does not establish online activation, upload, enforcement or absence of detection.']}
out = ROOT / 'online_diag_verification.json'
out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print('PASS: original SHA256, both analysis copies, all selected code fragments, writable globals,')
print('      three REA envelopes and seven critical C artifacts verified.')
for item in copies:
    print(Path(item['path']).name, 'fragments=', item['verified_fragments'],
          'code_bytes=', item['verified_code_bytes'])
for item in exports:
    print(item['name'], 'written=', item['counts']['written'],
          'remaining=', item['counts']['remaining'], 'evidence=', item['evidence_id'])
print('Result:', out)
