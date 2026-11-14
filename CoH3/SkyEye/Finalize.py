"""Seal verified V2 outputs; exclude local secrets and every Reactor trial binary."""
from pathlib import Path
import argparse, base64, datetime, hashlib, json, os, re, zipfile
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--refresh', action='store_true', help='Revalidate preserved releases and atomically refresh current Server/Maintenance archives after cleanup.')
args = parser.parse_args()
R = Path(__file__).resolve().parent
REPO = R.parents[1]
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
archives={kind:PARENT/('SkyEye-HardenedV2-'+kind+'.zip') for kind in ['Client','Server','Maintenance']}
proof=PARENT/'SkyEye-HardenedV2.archive-verification.json'
seal=R/'evidence/final-validation.json'
for p in [*archives.values(),proof,seal]:
    if p.exists() and not args.refresh: raise FileExistsError('Final output already exists; use --refresh for the authorized cleanup refresh: '+str(p))
if args.refresh and not (R/'evidence/cleanup-manifest.json').is_file():
    raise FileNotFoundError('A verified cleanup manifest is required for --refresh.')
summary=load(R/'evidence/validation-summary.json')
check('derived comparison passed',summary['passed'])
check('120 server regression checks',summary['serverChecksPassed']==120)
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
server=REPO/'LicensingSystem/delivery/server/Licensing.Server.dll'
check('published server is tested build',sha(server)==sha(REPO/'LicensingSystem/server/bin/Release/net10.0/Licensing.Server.dll'))
check('published startup wrapper exists',(REPO/'LicensingSystem/delivery/server/Start-Server.ps1').is_file())
check('published deployment helpers exist',all((REPO/'LicensingSystem/delivery/server/deployment'/n).is_file() for n in ['Get-DotNet10.ps1','New-DeploymentConfig.ps1','Caddyfile']))
refactor=load(R/'evidence/refactor-manifest.json')
check('layout refactor applied and mapped',all(Path(m['to']).exists() for m in refactor['moves']) and all(not Path(m['from']).exists() for m in refactor['moves']))
auth_prefix=[{'from':m['from']+os.sep,'to':m['to']+os.sep} for m in refactor['prefix_map']]
def remap(p):
    p=str(Path(p))
    for m in auth_prefix:
        if p.startswith(m['from']): return Path(m['to']+p[len(m['from']):])
    return Path(p)
config=text(R/'delivery/client'/ (client.name+'.config'))
check('client configured for isolated signed server','47835' in config and 'CoH3FowTool' in config)
pin=text(R/'client/CoH3MemoryTool/PinnedLeaseKey60.cs')
pin_values={n:re.search(r'const\s+string\s+'+n+r'\s*=\s*"([^"]+)"',pin).group(1) for n in ['Modulus','Exponent','Kid']}
modulus=base64.b64decode(pin_values['Modulus'],validate=True)
check('compiled public-key pin valid',len(modulus)==384 and bool(base64.b64decode(pin_values['Exponent'],validate=True)) and hashlib.sha256(modulus).hexdigest()[:16]==pin_values['Kid'])
cleanup_path=R/'evidence/cleanup-manifest.json'
if cleanup_path.is_file():
    cleanup=load(cleanup_path)
    check('cleanup verified original retirement baselines',cleanup['passed'] and cleanup['preDeletion']['priorArchivesValid'] and cleanup['preDeletion']['priorSourceBaselinesValid'] and cleanup['preDeletion']['priorSourceFiles']==218)
    check('all retired paths remain absent',all(not Path(x['path']).exists() for x in cleanup['removed']))
    exceptions={str(remap(x['path'])) for x in cleanup['preserved'] if remap(x['path']).name in {'air-diagnostics.log','air-outline-diagnostics.log','minimap-diagnostics.log','overlay-diagnostics.log','Licensing.Server.dll','Licensing.Server.pdb','Start-Server.ps1','server.json'}}
    strict=[x for x in cleanup['preserved'] if str(remap(x['path'])) not in exceptions]
    check('original, current source, release and signing material retained',all(remap(x['path']).is_file() and sha(remap(x['path']))==x['sha256'] for x in strict))
    refactored=[remap(x['path']) for x in cleanup['preserved'] if str(remap(x['path'])) in exceptions]
    check('intentionally refactored preserved files present',len(refactored)==8 and all(p.is_file() for p in refactored))
    check('republished server matches tested build',sha(REPO/'LicensingSystem/delivery/server/Licensing.Server.dll')==sha(REPO/'LicensingSystem/server/bin/Release/net10.0/Licensing.Server.dll'))
    cfg=load(PRIVATE/'server.json')
    check('refactored private config keeps token and relocated state','LicensingSystem' in str(cfg['DatabasePath']) and 'LicensingSystem' in str(cfg['KeyDirectory']) and len(str(cfg['AdminToken']))>=32)
    post_cleanup=load(R/'evidence/cleanup-validation.json')
    check('post-cleanup real regression and final directory validation passed',post_cleanup['passed'] and all(x['passed'] for x in post_cleanup['checks']) and post_cleanup['serverChecksPassed']==120 and post_cleanup['protectedModesPassed']==7)
    check('post-cleanup pictures, audio and immutable customer archive verified',len(post_cleanup['pictures'])==3 and all(x['changedPixels']==0 for x in post_cleanup['pictures']) and post_cleanup['identicalDecodedAudioFiles']==8 and sha(archives['Client'])==post_cleanup['customerArchiveSha256'])
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
check('private signing key retained outside delivery',key.is_file() and R not in key.resolve().parents and (REPO/'LicensingSystem/delivery') not in key.resolve().parents)
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
server_files=sorted(p for p in (REPO/'LicensingSystem/delivery/server').rglob('*') if p.is_file() and eligible(p,REPO/'LicensingSystem/delivery/server'))
maintenance_files=sorted(p for p in R.rglob('*') if p.is_file() and eligible(p,R))
auth_project=REPO/'LicensingSystem'
def auth_eligible(p):
    parts=p.relative_to(auth_project).parts
    if any(x in {'.local','.cache','bin','obj','test-runs','__pycache__','delivery','evidence'} for x in parts): return False
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
record={'generatedAt':datetime.datetime.now(datetime.timezone.utc).isoformat(),'passed':True,'checks':checks,'checkCount':len(checks),'localLinks':links,'secretScanner':'Administrator tokens, fixture code values and private PEM payload lines checked in memory; no values saved.','trialBinaryIncluded':False,'limitations':summary['limitations']}
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
    if destination.exists() and kind=='Client':
        # The customer release is immutable during cleanup. Verify its original manifest and payloads.
        with zipfile.ZipFile(destination) as z:
            old=load_manifest(z)
            assert old['files']==manifest['files'], 'Customer release files changed during cleanup.'
            assert z.testzip() is None and len(z.infolist())==len(entries)+1
            for row in old['files']:
                data=z.read(row['path'])
                assert len(data)==row['bytes'] and hashlib.sha256(data).hexdigest()==row['sha256']
    else:
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
        if args.refresh: os.replace(candidate,destination)
    digest=sha(destination)
    Path(str(destination)+'.sha256').write_text(digest+'  '+destination.name+'\n',encoding='ascii')
    return {'archive':str(destination),'bytes':destination.stat().st_size,'sha256':digest,'entriesVerified':len(entries)+1,'passed':True}
def load_manifest(z): return json.loads(z.read('PACKAGE-MANIFEST.json').decode('utf-8'))
results=[package('Client',client_files,R/'delivery/client'),package('Server',server_files,REPO/'LicensingSystem/delivery/server'),package('Maintenance',maintenance_files+helpers,REPO)]
proof.write_text(json.dumps({'passed':True,'auditChecks':len(checks),'archives':results},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'passed':True,'auditChecks':len(checks),'archives':results},ensure_ascii=True,indent=2))
