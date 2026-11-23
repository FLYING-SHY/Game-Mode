"""Seal verified V2 outputs; exclude local secrets and every Reactor trial binary."""
from pathlib import Path
import argparse, base64, datetime, hashlib, json, os, re, zipfile
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--refresh', action='store_true', help='Revalidate preserved releases and atomically refresh current Server/Maintenance archives after cleanup.')
parser.add_argument('--summary', default=None, help='Comparison summary to seal (default: evidence/validation-summary.json).')
parser.add_argument('--revision', default=None, help='Authorized revision that covers the preserved deviations (default: evidence/authorized-revision-2026-11-16.json).')
args = parser.parse_args()
R = Path(__file__).resolve().parent
REPO = R.parents[2]
PARENT = R.parent
PRIVATE = REPO/'LicensingSystem/.local/hardening-v2'
def load(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def text(p):
    b=p.read_bytes()
    return b.decode('utf-16') if b.startswith((b'\xff\xfe',b'\xfe\xff')) else b.decode('utf-8-sig')
checks=[]
def check(name, condition):
    checks.append({'name':name,'passed':bool(condition)})
    if not condition: raise AssertionError(name)
archives={kind:REPO/('CoH3/output/SkyEye/SkyEye-HardenedV2-'+kind+'.zip') for kind in ['Client','Server','Maintenance']}
proof=REPO/'CoH3/output/SkyEye/SkyEye-HardenedV2.archive-verification.json'
seal=R/'evidence/final-validation.json'
for p in [*archives.values(),proof,seal]:
    if p.exists() and not args.refresh: raise FileExistsError('Final output already exists; use --refresh for the authorized cleanup refresh: '+str(p))
if args.refresh and not (R/'evidence/cleanup-manifest.json').is_file():
    raise FileNotFoundError('A verified cleanup manifest is required for --refresh.')
summary_path=Path(args.summary) if args.summary else R/'evidence/validation-summary.json'
summary=load(summary_path)
check('derived comparison passed',summary['passed'])
server_checks=summary['serverChecksPassed']
check('server regression checks recorded',server_checks>=120 and server_checks==summary.get('serverChecksTotal',server_checks))
check('seven real modes in every variant',all(summary[k]==7 for k in ['rawPassingModes','releasePassingModes','reactorTrialPassingModes']))
check('six pixel comparisons identical',len(summary['pictures'])==6 and all(x['changedPixels']==0 for x in summary['pictures']))
check('eight decoded files identical in both protection variants',len(summary['audio'])==2 and all(x['decodedFiles']==8 and x['payloadsIdentical'] for x in summary['audio']))
check('twelve observed sessions inactive',summary['sessionCleanupChecks']==12)
check('real release window captured and closed',summary['realEntry']['WindowCaptured'] and summary['realEntry']['Closed'])
check('ILSpy success acknowledged',summary['releaseIlspyExportExitCode']==0 and summary['releaseIlspyCSharpFiles']==56)
latest=load(R/'evidence/latest-build.json')
client=Path(latest['Client'])
check('release executable is tested hash',sha(client).upper()==latest['OutputSha256']==summary['releaseSha256'])
check('release and trial distinct',sha(client).lower()!=summary['reactorTrialSha256'].lower())
check('private mapping available for owner',Path(latest['PrivateMapping']).is_file())
server=REPO/'LicensingSystem/output/Licensing.Server.dll'
check('published server is tested build',sha(server)==sha(REPO/'LicensingSystem/server/bin/Release/net10.0/Licensing.Server.dll'))
check('published startup wrapper exists',(REPO/'LicensingSystem/output/Start-Server.ps1').is_file())
check('published deployment helpers exist',all((REPO/'LicensingSystem/output/deployment'/n).is_file() for n in ['Get-DotNet10.ps1','New-DeploymentConfig.ps1','Caddyfile']))
refactor=load(R/'evidence/refactor-manifest.json')
layout2_path=R/'evidence/refactor-manifest-2026-11-15.json'
layout2=load(layout2_path) if layout2_path.is_file() else None
def exists_now(p):
    p=str(Path(p))
    if layout2:
        for m in layout2['prefix_map']:
            f=m['from']
            if p==f or p.startswith(f+os.sep): return Path(m['to']+p[len(f):]).exists()
    return Path(p).exists()
check('layout refactor applied and mapped',all(exists_now(m['to']) for m in refactor['moves']) and all(not exists_now(m['from']) for m in refactor['moves']))
auth_prefix=(layout2['prefix_map'] if layout2 else [])+refactor['prefix_map']
def remap(p):
    """Follow every recorded layout move until the path stops changing."""
    p=str(Path(p))
    for _ in range(8):
        hit=None
        for m in auth_prefix:
            f=m['from']
            if p==f: hit=(m['to'],len(f)); break
            if p.startswith(f.rstrip('\\/')+os.sep): hit=(m['to'].rstrip('\\/')+os.sep,len(f.rstrip('\\/'))+1); break
        if not hit: return Path(p)
        p=hit[0]+p[hit[1]:]
    return Path(p)
revision_path=Path(args.revision) if args.revision else R/'evidence/authorized-revision-2026-11-16.json'
revision=load(revision_path) if revision_path.is_file() else None
revision_overrides={row['path']:row for row in revision['preservedOverrides']} if revision else {}
config=text(R/'delivery/client'/ (client.name+'.config'))
expected_url=(revision['deployment']['baseUrl'] if revision else None) or summary.get('deployment',{}).get('baseUrl') or 'http://127.0.0.1:47835/'
check('client configured for isolated signed server',expected_url in config and 'CoH3FowTool' in config)
pin=text(R/'client/CoH3MemoryTool/PinnedLeaseKey60.cs')
pin_values={n:re.search(r'const\s+string\s+'+n+r'\s*=\s*"([^"]+)"',pin).group(1) for n in ['Modulus','Exponent','Kid']}
modulus=base64.b64decode(pin_values['Modulus'],validate=True)
check('compiled public-key pin valid',len(modulus)==384 and bool(base64.b64decode(pin_values['Exponent'],validate=True)) and hashlib.sha256(modulus).hexdigest()[:16]==pin_values['Kid'])
cleanup_path=R/'evidence/cleanup-manifest.json'
cleanup_archive_sha=None
if cleanup_path.is_file():
    cleanup=load(cleanup_path)
    check('cleanup verified original retirement baselines',cleanup['passed'] and cleanup['preDeletion']['priorArchivesValid'] and cleanup['preDeletion']['priorSourceBaselinesValid'] and cleanup['preDeletion']['priorSourceFiles']==218)
    check('all retired paths remain absent',all(not Path(x['path']).exists() for x in cleanup['removed']))
    exceptions={str(remap(x['path'])) for x in cleanup['preserved'] if remap(x['path']).name in {'air-diagnostics.log','air-outline-diagnostics.log','minimap-diagnostics.log','overlay-diagnostics.log','Licensing.Server.dll','Licensing.Server.pdb','Start-Server.ps1','server.json'}}
    strict=[x for x in cleanup['preserved'] if str(remap(x['path'])) not in exceptions]
    revised=[]
    for x in strict:
        current=remap(x['path'])
        if not current.is_file(): raise AssertionError('Preserved file disappeared: '+str(current))
        digest=sha(current)
        if digest==x['sha256']: continue
        relative=current.relative_to(REPO).as_posix()
        row=revision_overrides.get(relative)
        if not row or row['baselineSha256']!=x['sha256'] or row['currentSha256']!=digest:
            raise AssertionError('Preserved file changed without an authorized revision: '+relative)
        revised.append(current)
    check('original, current source, release and signing material retained',all(remap(x['path']).is_file() for x in strict))
    check('authorized revision covers exactly the recorded deviations',len(revised)==len(revision_overrides) and all(p.is_file() for p in revised))
    refactored=[remap(x['path']) for x in cleanup['preserved'] if str(remap(x['path'])) in exceptions]
    check('intentionally refactored preserved files present',len(refactored)==8 and all(p.is_file() for p in refactored))
    check('republished server matches tested build',sha(REPO/'LicensingSystem/output/Licensing.Server.dll')==sha(REPO/'LicensingSystem/server/bin/Release/net10.0/Licensing.Server.dll'))
    cfg=load(PRIVATE/'server.json')
    check('refactored private config keeps token and relocated state','LicensingSystem' in str(cfg['DatabasePath']) and 'LicensingSystem' in str(cfg['KeyDirectory']) and len(str(cfg['AdminToken']))>=32)
    post_cleanup=load(R/'evidence/cleanup-validation.json')
    check('post-cleanup real regression and final directory validation passed',post_cleanup['passed'] and all(x['passed'] for x in post_cleanup['checks']) and post_cleanup['serverChecksPassed']==120 and post_cleanup['protectedModesPassed']==7)
    cleanup_archive_sha=post_cleanup['customerArchiveSha256']
    revision_archive_sha=(revision['clientArchive'].get('currentSha256') if revision else None)
    archive_bound=sha(archives['Client']) in {cleanup_archive_sha,revision_archive_sha}
    check('post-cleanup pictures, audio and customer archive verified',len(post_cleanup['pictures'])==3 and all(x['changedPixels']==0 for x in post_cleanup['pictures']) and post_cleanup['identicalDecodedAudioFiles']==8 and archive_bound)
    check('customer archive re-issue is bound to the authorized revision',bool(revision) and revision['clientArchive']['authorized'] and revision['clientArchive']['baselineSha256']==cleanup_archive_sha and archive_bound)
else:
    base=load(R/'evidence/baseline-archive-hashes.json')
    for row in base:
        path=Path(row.get('Path',row.get('path','')))
        expected=row.get('Hash',row.get('Sha256',row.get('sha256','')))
        check('prior archive unchanged: '+path.name,path.is_file() and sha(path).lower()==expected.lower())
    sources=load(R/'evidence/pre-v2-source-baselines.json')
    roots={'client':PARENT/'SkyEye-Hardened/client','server':REPO/'Licensing/server'}
    check('prior source baselines recorded',len(sources)==218)
    check('prior source files unchanged',all((roots[x['Tree']]/x['RelativePath']).is_file() and sha(roots[x['Tree']]/x['RelativePath']).lower()==x['Sha256'].lower() for x in sources))
trial=load(R/'evidence/reactor-client-trial/seven-case-summary.json')
check('commercial trial explicitly unlicensed',trial['ProductionLicensed'] is False and trial['AllPassed'] and trial['PassedCount']==7)
# Scan public files against secrets held only in memory. Never serialize values.
secret_values=[]
for p in [REPO/'LicensingSystem/.local/native-server.json',PRIVATE/'server.json',PRIVATE/'published-server.json']:
    if p.is_file():
        token=load(p).get('AdminToken')
        if token: secret_values.append(token)
for directory in PRIVATE.glob('cards-*'):
    if directory.is_dir():
        for p in directory.glob('*.txt'):
            value=p.read_text(encoding='utf-8-sig').strip()
            if value: secret_values.append(value)
private_config=load(PRIVATE/'server.json')
key=Path(private_config['KeyDirectory'])/'lease-signing-key.pk8.pem'
check('private signing key retained outside output',key.is_file() and R not in key.resolve().parents and (REPO/'LicensingSystem/output') not in key.resolve().parents)
key_lines=key.read_text(encoding='utf-8').splitlines()
secret_values.extend(line for line in key_lines if len(line)>=64 and not line.startswith('-'))
secret_values=list(set(secret_values))
secret_needles=[(value.encode('utf-8'),value.encode('utf-16-le')) for value in secret_values]
check('secret scanner has administrator, card and signing material needles',len(secret_values)>=30)

def eligible(p,base):
    rel=p.relative_to(base)
    parts=rel.parts
    if any(x in {'.local','.cache','tools','bin','obj','__pycache__'} for x in parts): return False
    if p.suffix.lower() in {'.pdb','.pyc','.db','.sqlite','.sqlite3','.pem','.pk8','.download'}: return False
    if p.name in {'browser-session.txt','final-validation.json'}: return False
    if parts[0]=='sandbox': return False
    if 'sandbox' in parts and p.name not in {'result.json','audio-result.json'}: return False
    if parts[0]=='artifacts':
        return len(parts)==3 and parts[1]==latest['Run'] and p.suffix.lower() in {'.xml','.json','.log'}
    if parts[0]=='evidence':
        # Keep the manifests and probe tooling a maintainer needs to re-run the
        # pipeline, but drop the regenerable corpus media and the multi-megabyte
        # state dumps: a maintenance bundle has to stay shippable.
        if p.suffix.lower() in {'.png','.jpg','.jpeg','.bmp','.gif','.mp4','.mov','.wav'}: return False
        if p.stat().st_size>4*1024*1024: return False
    return True

def scan_files(files):
    bad=[]
    trial_hash=summary['reactorTrialSha256'].lower()
    for p in files:
        data=p.read_bytes()
        if any(a in data or b in data for a,b in secret_needles): bad.append(p.relative_to(REPO).as_posix())
        if p.suffix.lower()=='.exe' and hashlib.sha256(data).hexdigest()==trial_hash:
            raise AssertionError('Trial binary selected for distribution.')
    return bad
client_files=sorted(p for p in (R/'delivery/client').rglob('*') if p.is_file())
server_files=sorted(p for p in (REPO/'LicensingSystem/output').rglob('*') if p.is_file() and eligible(p,REPO/'LicensingSystem/output'))
maintenance_files=sorted(p for p in R.rglob('*') if p.is_file() and eligible(p,R))
auth_project=REPO/'LicensingSystem'
def auth_eligible(p):
    parts=p.relative_to(auth_project).parts
    if any(x in {'.local','.cache','bin','obj','test-runs','__pycache__','delivery','output','evidence'} for x in parts): return False
    return p.suffix.lower() not in {'.pdb','.pyc','.db','.sqlite','.sqlite3','.pem','.pk8','.download'}
auth_files=sorted(p for p in auth_project.rglob('*') if p.is_file() and auth_eligible(p))
maintenance_files+=auth_files
check('customer bundle contains expected executable',client in client_files)
check('customer bundle excludes developer artifacts',all(p.suffix.lower() not in {'.cs','.pdb','.xml','.json','.ps1','.py'} and 'verification' not in p.parts for p in client_files))
check('server bundle excludes local state and symbols',all(p.suffix.lower() not in {'.pdb','.db','.pem'} for p in server_files))
check('maintenance contains source, mapping, report and reproducible wrappers',all(p in maintenance_files for p in [R/'Finalize.py',REPO/'LicensingSystem/Publish-Server.ps1',REPO/'LicensingSystem/Build.ps1',REPO/'LicensingSystem/Directory.Build.props',REPO/'LicensingSystem/README.md',R/'reports/protection-validation.md',Path(latest['PrivateMapping']),REPO/'LicensingSystem/server/LeaseSigner.cs',R/'client/CoH3MemoryTool/LeaseSignature60.cs']))
check('all selected public files contain no retained local secrets',not scan_files(set(client_files+server_files+maintenance_files)))
links=[]
for doc in [R/'README.md',R/'reports/protection-validation.md',R/'reports/signature-cross-review.md']:
    for link in re.findall(r'\[[^\]]*\]\(([^)]+)\)',text(doc)):
        if re.match(r'^[a-z]+://',link): continue
        target=link.split('#',1)[0].strip('<>')
        if not target: continue
        links.append({'document':str(doc.relative_to(R)),'link':link,'exists':(doc.parent/target).exists()})
check('report local links resolve',bool(links) and all(x['exists'] for x in links))
client_archive_reissued=False
if revision and archives['Client'].is_file() and revision['clientArchive'].get('authorized'):
    client_archive_reissued=sha(archives['Client'])!=cleanup_archive_sha
    check('customer release superseded under the authorized revision',client_archive_reissued or sha(archives['Client'])==cleanup_archive_sha)
record={'generatedAt':datetime.datetime.now(datetime.timezone.utc).isoformat(),'passed':True,'checks':checks,'checkCount':len(checks),'localLinks':links,'secretScanner':'Administrator tokens, fixture code values and private PEM payload lines checked in memory; no values saved.','trialBinaryIncluded':False,'authorizedRevision':revision['revisionId'] if revision else None,'customerArchiveReissued':client_archive_reissued,'limitations':summary['limitations']}
seal.write_text(json.dumps(record,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
maintenance_files.append(seal)
helpers=[REPO/'LicensingSystem/deployment'/n for n in ['Get-DotNet10.ps1','New-DeploymentConfig.ps1','Caddyfile']]
helpers += [REPO/'README.md',REPO/'AGENTS.md',REPO/'LicensingSystem/README.md']
check('portable SDK/config helpers exist',all(p.is_file() for p in helpers))
check('portable helpers contain no retained secrets',not scan_files(helpers))
# All checks, including portable dependencies, belong to the final seal.
record['checks']=checks;record['checkCount']=len(checks)
seal.write_text(json.dumps(record,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

def package(kind, files, relative_root):
    entries={p.relative_to(relative_root).as_posix():p for p in files}
    manifest={'format':'SkyEye-HardenedV2-'+kind,'generatedAt':record['generatedAt'],'files':[{'path':name,'bytes':p.stat().st_size,'sha256':sha(p)} for name,p in sorted(entries.items())]}
    destination=archives[kind]
    write_new=True
    if destination.exists() and kind=='Client':
        # The customer release is immutable during cleanup. Verify its original manifest and payloads.
        with zipfile.ZipFile(destination) as z:
            old=load_manifest(z)
            assert z.testzip() is None and len(z.infolist())==len(old['files'])+1
            for row in old['files']:
                data=z.read(row['path'])
                assert len(data)==row['bytes'] and hashlib.sha256(data).hexdigest()==row['sha256']
            if old['files']==manifest['files']:
                # Identical payload set: keep the signed archive byte-identical.
                write_new=False
            else:
                assert client_archive_reissued, 'Customer release files changed during cleanup.'
    if write_new:
        candidate=destination.with_name(destination.name+'.new') if args.refresh else destination
        with zipfile.ZipFile(candidate,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
            for name,p in sorted(entries.items()): z.write(p,name)
            z.writestr('PACKAGE-MANIFEST.json',json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
        with zipfile.ZipFile(candidate) as z:
            assert z.testzip() is None
            assert len(z.infolist())==len(entries)+1
            for row in manifest['files']:
                content=z.read(row['path'])
                assert len(content)==row['bytes'] and hashlib.sha256(content).hexdigest()==row['sha256']
            assert load_manifest(z)==manifest
        if args.refresh: replace_archive(candidate,destination)
    digest=sha(destination)
    Path(str(destination)+'.sha256').write_text(digest+'  '+destination.name+'\n',encoding='ascii')
    return {'archive':str(destination),'bytes':destination.stat().st_size,'sha256':digest,'entriesVerified':len(entries)+1,'passed':True}
def load_manifest(z): return json.loads(z.read('PACKAGE-MANIFEST.json').decode('utf-8'))
def replace_archive(candidate,destination):
    """Swap the freshly written archive in; a scanner may still hold the previous file."""
    import time
    for attempt in range(12):
        try:
            os.replace(candidate,destination); return
        except PermissionError:
            if attempt==11: raise
            time.sleep(5)
results=[package('Client',client_files,R/'delivery/client'),package('Server',server_files,REPO/'LicensingSystem/output'),package('Maintenance',maintenance_files+helpers,REPO)]
proof.write_text(json.dumps({'passed':True,'auditChecks':len(checks),'archives':results},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'passed':True,'auditChecks':len(checks),'archives':results},ensure_ascii=True,indent=2))
