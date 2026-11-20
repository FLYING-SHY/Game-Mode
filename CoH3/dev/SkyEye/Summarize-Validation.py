"""Derive comparisons from immutable execution results; never substitute design for evidence."""
from pathlib import Path
import datetime, hashlib, json
from PIL import Image, ImageChops
R = Path(__file__).resolve().parent
REPO = R.parents[2]
def load(path): return json.loads(path.read_text(encoding='utf-8-sig'))
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
raw = load(R/'evidence/raw-signed-20261116-154841/summary.json')
protected = load(R/'evidence/protected-verification-20261116-154658-342.json')
trial = load(R/'evidence/reactor-client-trial/seven-case-summary.json')
server = load(REPO/'LicensingSystem/server-tests/bin/Release/net10.0/test-runs/20261116-070520-73be182a/results.json')
assert len(raw['results']) == len(protected) == len(trial['Cases']) == 7
assert all(v['exitCode'] == 0 for v in raw['results'])
assert all(v['ExitCode'] == 0 for v in protected)
assert trial['AllPassed'] and trial['PassedCount'] == 7
assert server['passed'] == len(server['checks']) == 120
raw_cases = {c['case']: Path(c['sandbox']) for c in raw['results']}
protected_cases = {c['Name']: Path(c['Result']).parent for c in protected}
trial_root = R/'evidence/reactor-client-trial'
for folder in list(raw_cases.values())+list(protected_cases.values()):
    assert load(folder/'result.json')['passed'] is True
assert load(trial_root/'ui-result.json')['passed'] is True
assert load(trial_root/'audio-audio-result.json')['passed'] is True
pictures = []
for variant, ui_folder in [('release', protected_cases['ui']), ('reactorTrial', trial_root)]:
    for name in ['main','expanded','compact']:
        a, b = raw_cases['ui']/(name+'.png'), ui_folder/(name+'.png')
        with Image.open(a) as im_a, Image.open(b) as im_b:
            assert im_a.size == im_b.size
            diff = ImageChops.difference(im_a.convert('RGB'), im_b.convert('RGB'))
            changed = sum(1 for pixel in diff.get_flattened_data() if pixel != (0,0,0))
            pictures.append({'variant': variant, 'name': name, 'size': list(im_a.size), 'changedPixels': changed, 'originalSha256': sha(a), 'comparedSha256': sha(b)})
            assert changed == 0
audio = load(raw_cases['audio']/'audio-result.json')
audio_comparison = []
for variant, source in [('release', protected_cases['audio']/'audio-result.json'), ('reactorTrial', trial_root/'audio-audio-result.json')]:
    other = load(source)
    identical = audio['decoded'] == other['decoded'] and audio['files'] == other['files'] == 8
    assert identical and other['passed']
    audio_comparison.append({'variant': variant, 'decodedFiles': 8, 'payloadsIdentical': identical, 'audiblePlaybackTested': False})
latest = load(R/'evidence/latest-build.json')
assert sha(Path(latest['Client'])).upper() == latest['OutputSha256']
cleanup = load(R/'evidence/session-cleanup-cloud-20261116.json'); assert cleanup['passed'] and len(cleanup['checks']) == 12 and cleanup['globalActiveSessions'] == 0
entry = load(R/'evidence/protected-real-entry-lifecycle-20261116.json'); assert entry['WindowCaptured'] and entry['Closed']
static = load(R/'evidence/release-static-analysis-20261116b/summary.json')
summary = {'generatedAt': datetime.datetime.now(datetime.timezone.utc).isoformat(), 'passed': True,
 'serverChecksPassed': 120, 'rawPassingModes': 7, 'releasePassingModes': 7, 'reactorTrialPassingModes': 7,
 'rawSha256': raw['clientSHA256'], 'releaseSha256': latest['OutputSha256'], 'reactorTrialSha256': trial['TrialSHA256'],
 'pictures': pictures, 'audio': audio_comparison, 'sessionCleanupChecks': 12, 'globalActiveSessions': cleanup['globalActiveSessions'], 'realEntry': entry,
 'releaseReaCounts': static['records'][1]['counts'],
 'releaseIlspyExportExitCode': static['records'][2]['exitCode'], 'releaseIlspyCSharpFiles': static['records'][2]['csharpFiles'],
 'protectedTiming': {'firstHeartbeatHttpMs': 134, 'activationHttpMs': 156, 'heartbeatIntervalMs': 28229, 'networkGateClosedObservedMs': 59339},
 'timingSource': 'protected-coordinator/network execution logs; elapsed values are wall time since harness startup, except interval subtraction',
 'deployment': {'baseUrl': 'https://124.220.147.56/', 'transport': 'TLS with a publicly trusted Let\'s Encrypt IP certificate', 'endpoint': 'nginx 1.29.8 -> http://127.0.0.1:47835 (ASP.NET Core 10) on 124.220.147.56'},
 'limitations': ['Reactor output is a 14-day demo experiment and excluded from every delivery archive.', 'ILSpy still exports release C#; obfuscation raises analysis cost.', 'Preview pixels and decoded audio do not prove live-game correctness or audible playback.', 'Public deployment, another machine, long sessions and protection patch resistance are not tested.']}
output=R/'evidence/validation-summary.json'
if output.exists(): raise SystemExit('Refusing to overwrite final comparison summary.')
output.write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'passed':True,'server':120,'modesPerVariant':7,'zeroPixelComparisons':len(pictures),'audioVariants':2,'sessionChecks':12},ensure_ascii=True))
