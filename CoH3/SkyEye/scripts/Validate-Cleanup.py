"""Verify the completed cleanup and real regression, then discard regenerated test outputs.

HISTORICAL ONE-SHOT (applied 2026-11-13): superseded by the LicensingSystem/SkyEye
layout refactor (evidence/refactor-manifest.json). The SkyEye-HardenedV2 path guard
below is intentional and permanently prevents a second run.
"""
from pathlib import Path
import ast, datetime, hashlib, json, shutil, socket, urllib.request
from PIL import Image, ImageChops

R = Path(__file__).resolve().parents[1]
REPO = R.parents[1]
assert R == Path(r'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2')
OUT = R/'evidence/cleanup-validation.json'
if OUT.exists(): raise SystemExit('Cleanup validation already recorded; inspect the evidence before repeating.')
def load(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def save(p, data): p.write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
checks = []
def check(name, value):
    checks.append({'name':name, 'passed':bool(value)})
    if not value: raise AssertionError(name)

cleanup = load(R/'evidence/cleanup-manifest.json')
latest = load(R/'evidence/latest-build.json')
check('reviewed cleanup completed', cleanup['passed'] and len(cleanup['removed']) == 107)
check('all preserved files match their pre-deletion hashes', all(Path(x['path']).is_file() and sha(Path(x['path'])) == x['sha256'] for x in cleanup['preserved']))
server_result = R/'server-tests/bin/Release/net10.0/test-runs/20261113-083359-defbcd41/results.json'
server = load(server_result)
check('post-cleanup backend real HTTP and SQLite regression', server['passed'] == len(server['checks']) == 120)
current_path = R/'evidence/protected-verification-20261113-163401-399.json'
current = load(current_path)
prior = load(R/'evidence/protected-verification-20261113-144238-873.json')
check('post-cleanup seven protected modes returned exit zero', len(current) == 7 and all(x['ExitCode'] == 0 for x in current))
current_cases = {x['Name']:Path(x['Result']).parent for x in current}
prior_cases = {x['Name']:Path(x['Result']).parent for x in prior}
check('every post-cleanup result explicitly passed', all(load(Path(x['Result']))['passed'] is True for x in current))
check('unprotected rebuild matches the original protection input', sha(R/'client/bin/Release/net48'/Path(latest['Client']).name).upper() == latest['InputSha256'])
check('distributed protected client remains identical', sha(Path(latest['Client'])).upper() == latest['OutputSha256'])
check('published server matches rebuilt server', sha(R/'delivery/server/Licensing.Server.dll') == sha(R/'server/bin/Release/net10.0/Licensing.Server.dll'))
pictures = []
for name in ['main','expanded','compact']:
    with Image.open(prior_cases['ui']/(name+'.png')) as a, Image.open(current_cases['ui']/(name+'.png')) as b:
        check('post-cleanup '+name+' image size', a.size == b.size)
        diff = ImageChops.difference(a.convert('RGB'), b.convert('RGB'))
        changed = sum(1 for pixel in diff.getdata() if pixel != (0,0,0))
        check('post-cleanup '+name+' pixels identical', changed == 0)
        pictures.append({'name':name,'changedPixels':changed,'size':list(a.size)})
old_audio, new_audio = (load(group['audio']/'audio-result.json') for group in [prior_cases,current_cases])
check('post-cleanup eight decoded audio payloads identical', new_audio['passed'] and new_audio['files'] == old_audio['files'] == 8 and new_audio['decoded'] == old_audio['decoded'])
tools = load(R/'evidence/cleanup-tools-validation.json')
check('relocated ILSpy and PowerShell scripts validated', tools['Passed'] and tools['IlspyExitCode'] == 0 and tools['PowerShellScriptsParsed'] == 17)
for p in [*R.glob('*.py'), *R.joinpath('scripts').glob('*.py')]: ast.parse(p.read_text(encoding='utf-8-sig'), filename=str(p))
check('current Python scripts parse', True)
with urllib.request.urlopen('http://127.0.0.1:47835/health', timeout=10) as response:
    health = json.load(response)
check('current signing service remains healthy', health.get('status') == 'ready')
with socket.socket() as connection:
    connection.settimeout(2)
    legacy_closed = connection.connect_ex(('127.0.0.1',47831)) != 0
check('retired service port is closed', legacy_closed)
check('shared SDK and independent project retained', (REPO/'Licensing/.cache/dotnet-10/dotnet.exe').is_file() and (REPO/'CoH3/CoH3Minimap/README.md').is_file())

# The regression deliberately rebuilt these disposable copies and decoded new WAV files.
# Resolve every exact target and validate the complete list before any deletion.
targets = [R/x for x in ['client/bin','client/obj','verification/bin/Release','server-tests/bin/Release/net10.0/test-runs']]
targets += sorted((R/'verification/sandbox').rglob('*.wav'))
removals = []
for p in targets:
    if not p.exists(): continue
    assert R in p.parents and p.resolve(strict=True) == p and not p.is_symlink() and not p.is_junction()
    members = [p] if p.is_file() else [f for f in p.rglob('*') if f.is_file()]
    assert all(not f.is_symlink() and not f.is_junction() and f.resolve(strict=True) == f for f in members)
    removals.append({'path':str(p),'files':len(members),'bytes':sum(f.stat().st_size for f in members)})
save(R/'evidence/cleanup-server-tests-results.json', server)
for row in removals:
    p = Path(row['path'])
    assert p.resolve(strict=True) == p and R in p.parents
    if p.is_dir(): shutil.rmtree(p)
    else: p.unlink()
check('all retired paths remain absent after removing regression outputs', all(not Path(x['path']).exists() for x in cleanup['removed']))
check('protected verifier remains available', Path(latest['ProtectedVerifier']).is_file())
check('preserved files still match after regression-output cleanup', all(Path(x['path']).is_file() and sha(Path(x['path'])) == x['sha256'] for x in cleanup['preserved']))
record = {'generatedAt':datetime.datetime.now(datetime.timezone.utc).isoformat(),'passed':True,'checks':checks,
          'serverChecksPassed':120,'protectedModesPassed':7,'verificationReport':str(current_path),
          'serverResultCopy':'evidence/cleanup-server-tests-results.json','pictures':pictures,'identicalDecodedAudioFiles':8,
          'retirement':{'targets':len(cleanup['removed']),'files':cleanup['removedFiles'],'bytes':cleanup['removedBytes']},
          'regeneratedOutputsRemoved':removals,'currentServiceHealth':health,'legacyPortClosed':legacy_closed,
          'customerArchiveSha256':sha(R.parent/'SkyEye-HardenedV2-Client.zip')}
save(OUT, record)
print(json.dumps({'passed':True,'checks':len(checks),'serverChecks':120,'protectedModes':7,'zeroPixelComparisons':3,'identicalAudioFiles':8,'regeneratedFilesRemoved':sum(x['files'] for x in removals),'regeneratedBytesRemoved':sum(x['bytes'] for x in removals)}, ensure_ascii=True))
