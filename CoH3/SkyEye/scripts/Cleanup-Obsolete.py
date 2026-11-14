"""Remove reviewed obsolete SkyEye artifacts; retain current code, credentials and data.
Default: write a dry-run plan. --apply requires dependencies to have been migrated.

HISTORICAL ONE-SHOT (applied 2026-11-13): superseded by the LicensingSystem/SkyEye
layout refactor (evidence/refactor-manifest.json). Paths below and the
SkyEye-HardenedV2 name guard refer to the pre-refactor layout; do not run again.
"""
from pathlib import Path
import argparse, datetime, hashlib, json, os, shutil

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parents[1]
COH3 = ROOT.parent
PRIVATE = REPO / 'Licensing/.local/hardening-v2'
EXPECTED = Path(r'F:\WorkSpace\Game')
if REPO != EXPECTED or ROOT.name != 'SkyEye-HardenedV2':
    raise SystemExit('Unexpected workspace; no cleanup performed.')

def load(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):
    h = hashlib.sha256()
    with p.open('rb') as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b''): h.update(chunk)
    return h.hexdigest()
def save(p, obj): p.write_text(json.dumps(obj, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
def files(path):
    if path.is_symlink() or path.is_junction(): return []
    if path.is_file(): return [path]
    result = []
    for directory, children, names in os.walk(path, followlinks=False):
        children[:] = [x for x in children if not (Path(directory)/x).is_junction() and not (Path(directory)/x).is_symlink()]
        result.extend(Path(directory)/x for x in names)
    return result

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--apply', action='store_true')
args = parser.parse_args()
manifest_path = ROOT/'evidence/cleanup-manifest.json'
if manifest_path.exists(): raise SystemExit('Cleanup manifest already exists; inspect it instead of repeating cleanup.')

targets = []
def add(path, reason):
    path = Path(path).absolute()
    if not path.exists() and not path.is_junction(): return
    if path == REPO or REPO not in path.parents: raise AssertionError('Target outside workspace')
    if path.is_junction() or path.is_symlink(): raise AssertionError('Unexpected link target: ' + str(path))
    resolved = path.resolve(strict=True)
    if resolved != path: raise AssertionError('Target resolves through a link: ' + str(path))
    if any(path == old or old in path.parents for old, _ in targets): return
    targets.append((path, reason))

for name in ['SkyEye-v0.60.8-Recovered', 'SkyEye-Integration', 'SkyEye-Hardened']:
    add(COH3/name, 'Superseded source and development artifacts')
    for suffix in ['.zip', '.zip.sha256', '.archive-verification.json']:
        add(COH3/(name+suffix), 'Superseded archive or verification sidecar')
for stem in ['SkyEye-Integration-Delivery', 'SkyEye-Hardened-Client', 'SkyEye-Hardened-Maintenance']:
    for suffix in ['.zip', '.zip.sha256', '.archive-verification.json']:
        add(COH3/(stem+suffix), 'Superseded archive or verification sidecar')
add(REPO/'artifacts', 'Old CoH3FowTool desktop test images and results')
for name in ['client', 'server', 'tests', 'docs', 'Directory.Build.props', 'framework-evidence.md', '.dockerignore']:
    add(REPO/'Licensing'/name, 'Retired CE/unsigned-lease source; signed V2 source is authoritative')
for name in ['compose.yaml', 'Dockerfile', 'Initialize-Local.ps1', 'Start-Local.ps1']:
    add(REPO/'Licensing/deployment'/name, 'Retired backend launch/deployment entry point')
cache = REPO/'Licensing/.cache'
for p in cache.iterdir():
    if p.name not in {'dotnet-10', 'dotnet-10-installation.json'}:
        add(p, 'Obsolete backend test output, unused package cache or SDK download')
# Private business records, keys and configurations remain intact. Only known test products are removed.
for name in ['deployment-validation', 'final-build', 'integration-cards', 'test-runs']:
    add(REPO/'Licensing/.local'/name, 'Private obsolete test fixtures and build evidence')
for name in ['admin-acceptance.png', 'browser-permanent-batch-private.png', 'latest-integration.log', 'prepare-native-leases-20261113-100400.py', 'protected-smoke-codes.json']:
    add(REPO/'Licensing/.local'/name, 'Obsolete local test artifact')
# Old backups include obsolete source. Retain only historical credential/database files before deleting the backup tree.
add(REPO/'Licensing/.local/backups', 'Old source/configuration backup; unique private state retained separately')
for relative in ['tools/reactor', 'verification/bin/ReactorTrial', 'artifacts/raw-signed-test', 'client/bin', 'client/obj', 'verification/bin/Release']:
    add(ROOT/relative, 'Trial, duplicate unprotected build or regenerable intermediate output')
for p in (ROOT/'client/CoH3MemoryTool').glob('*Checks*.cs'):
    add(p, 'Historical in-client tests explicitly excluded by the current project')
for relative in ['evidence/release-static-analysis/ilspy-export']:
    add(ROOT/relative, 'Generated decompiler export; measurements and logs retained')
for p in (ROOT/'verification/sandbox').rglob('*.wav'):
    add(p, 'Generated decoded audio; comparison hashes and results retained')

immutable = []
for base in [COH3/'天穹之眼-v0.60.8', ROOT/'delivery/client', ROOT/'delivery/server', ROOT/'client', ROOT/'server']:
    for p in files(base):
        if any(part in {'bin','obj'} for part in p.relative_to(base).parts): continue
        if any(p == t or t in p.parents for t, _ in targets): continue
        immutable.append({'path':str(p), 'bytes':p.stat().st_size, 'sha256':sha(p)})
for name in ['SkyEye-HardenedV2-Client.zip']:
    p = COH3/name
    immutable.append({'path':str(p), 'bytes':p.stat().st_size, 'sha256':sha(p)})
for p in [PRIVATE/'server.json', *(PRIVATE/'data/keys').glob('*')]:
    if p.is_file(): immutable.append({'path':str(p), 'bytes':p.stat().st_size, 'sha256':sha(p)})

# Prove historical baseline was intact at the time of deliberate retirement.
base = load(ROOT/'evidence/baseline-archive-hashes.json')
archives_ok = all(Path(x.get('Path',x.get('path',''))).is_file() and sha(Path(x.get('Path',x.get('path','')))).lower() == x.get('Hash',x.get('Sha256',x.get('sha256',''))).lower() for x in base)
sources = load(ROOT/'evidence/pre-v2-source-baselines.json')
old_roots = {'client':COH3/'SkyEye-Hardened/client', 'server':REPO/'Licensing/server'}
sources_ok = len(sources) == 218 and all((old_roots[x['Tree']]/x['RelativePath']).is_file() and sha(old_roots[x['Tree']]/x['RelativePath']).lower() == x['Sha256'].lower() for x in sources)
if not archives_ok or not sources_ok: raise AssertionError('Historical baseline changed; inspect before deletion.')

rows = []
for path, reason in targets:
    members = files(path)
    rows.append({'path':str(path), 'reason':reason, 'files':len(members), 'bytes':sum(x.stat().st_size for x in members)})
plan = {'createdAt':datetime.datetime.now(datetime.timezone.utc).isoformat(), 'workspace':str(REPO), 'applyRequested':args.apply,
        'preDeletion':{'priorArchivesValid':archives_ok,'priorSourceBaselinesValid':sources_ok,'priorSourceFiles':len(sources)},
        'targets':rows, 'candidateFiles':sum(x['files'] for x in rows), 'candidateBytes':sum(x['bytes'] for x in rows),
        'preserved':immutable, 'privateStatePolicy':'Retain all non-test credential, key, database and configuration files. Current signed state is never a deletion target.',
        'unrelatedProjectRetained':str(COH3/'CoH3Minimap')}
save(ROOT/'evidence/cleanup-plan.json', plan)
if not args.apply:
    print(json.dumps({'dryRun':True,'targetCount':len(rows),'files':plan['candidateFiles'],'bytes':plan['candidateBytes'],'plan':str(ROOT/'evidence/cleanup-plan.json')}, ensure_ascii=True))
    raise SystemExit(0)

# New private backup contains data only, never retired source. Keep it outside public deliverables.
backups = REPO/'Licensing/.local/backups'
private_backup = REPO/'Licensing/.local/retained-private-state'
retained_backup_count = 0
if backups.exists():
    for p in files(backups):
        if p.suffix.lower() in {'.json','.db','.sqlite','.sqlite3','.pem','.key','.env','.pfx','.p12'} and not any(x in {'bin','obj'} for x in p.relative_to(backups).parts):
            destination = private_backup/p.relative_to(backups)
            destination.parent.mkdir(parents=True,exist_ok=True)
            if destination.exists(): raise FileExistsError(str(destination))
            shutil.copy2(p,destination)
            assert sha(p)==sha(destination)
            retained_backup_count += 1

assert (ROOT/'tools/ilspy/ilspycmd.exe').is_file(), 'ILSpy must be migrated first'
assert 'SkyEye-v0.60.8-Recovered' not in (ROOT/'Measure-Protection.py').read_text(encoding='utf-8-sig')
removed=[]
for row in rows:
    path=Path(row['path'])
    # Revalidate each exact path immediately before deletion.
    assert path.resolve(strict=True)==path and REPO in path.parents
    assert not path.is_junction() and not path.is_symlink()
    if path.is_dir(): shutil.rmtree(path)
    else: path.unlink()
    assert not path.exists()
    removed.append(row)
for row in immutable:
    p=Path(row['path'])
    assert p.is_file() and sha(p)==row['sha256'], 'Preserved file changed: '+str(p)
plan.update({'completedAt':datetime.datetime.now(datetime.timezone.utc).isoformat(), 'passed':True, 'removed':removed,
             'removedFiles':sum(x['files'] for x in removed), 'removedBytes':sum(x['bytes'] for x in removed),
             'retainedPrivateBackupFiles':retained_backup_count})
save(manifest_path,plan)
print(json.dumps({'passed':True,'targetsRemoved':len(removed),'filesRemoved':plan['removedFiles'],'bytesRemoved':plan['removedBytes'],'immutableFilesVerified':len(immutable),'privateBackupFilesRetained':retained_backup_count,'manifest':str(manifest_path)}, ensure_ascii=True))
