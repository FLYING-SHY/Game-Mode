"""Capture real REA metadata for the release candidate and an ILSpy export attempt."""
import hashlib, json, subprocess
from pathlib import Path
ROOT = Path(__file__).resolve().parent
OUT = ROOT / 'evidence' / 'release-static-analysis'
OUT.mkdir(parents=True, exist_ok=True)
NODE = Path(r'F:\WorkSpace\REA\.runtime\node-v24.22.3-win-x64\node.exe')
REA = Path(r'F:\WorkSpace\REA\scripts\rea.mjs')
client = next((ROOT / 'delivery' / 'client').glob('*.exe'))
records = []
for command, name in [('inspect-managed-artifact', 'artifact'), ('inspect-managed-members', 'members')]:
    output = OUT / (name + '.json')
    error = OUT / (name + '.stderr.txt')
    if output.exists():
        raise SystemExit('Refusing to overwrite existing static evidence: ' + str(output))
    argv = [str(NODE), str(REA), command, str(client), '--json']
    with output.open('xb') as stdout, error.open('xb') as stderr:
        result = subprocess.run(argv, stdout=stdout, stderr=stderr, check=False)
    if result.returncode:
        raise SystemExit(f'REA {command} failed: {result.returncode}')
    envelope = json.loads(output.read_text(encoding='utf-8-sig'))
    data = envelope['normalized_result']
    records.append({'operation': command, 'argv': argv, 'exitCode': result.returncode,
                    'output': str(output), 'sha256': hashlib.sha256(output.read_bytes()).hexdigest(),
                    'evidenceId': envelope.get('evidence_id'), 'coverage': data.get('coverage'),
                    'counts': {key: len(data[key]) for key in ['types', 'methods', 'fields', 'member_refs'] if isinstance(data.get(key), list)}})
ilspy = ROOT / 'tools' / 'ilspy' / 'ilspycmd.exe'
export = OUT / 'ilspy-export'
argv = [str(ilspy), str(client), '--project', '--outputdir', str(export)]
with (OUT / 'ilspy.stdout.txt').open('xb') as stdout, (OUT / 'ilspy.stderr.txt').open('xb') as stderr:
    result = subprocess.run(argv, stdout=stdout, stderr=stderr, check=False)
records.append({'operation': 'ILSpy project export attempt', 'argv': argv, 'exitCode': result.returncode,
                'csharpFiles': len(list(export.rglob('*.cs'))),
                'limitation': 'Successful decompilation is recorded as such; renaming/string hiding is not prevention of decompilation.'})
summary = {'clientSha256': hashlib.sha256(client.read_bytes()).hexdigest(), 'records': records}
(OUT / 'summary.json').write_text(json.dumps(summary, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps(summary, ensure_ascii=True, indent=2))
