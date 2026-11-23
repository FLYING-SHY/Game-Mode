"""Derive comparisons from immutable execution results; never substitute design for evidence.

Every input is a recorded execution artifact. The tool refuses to write a summary whose
inputs disagree, and it never overwrites an existing summary. Defaults reproduce the
2026-11-16 release summary; the 2026-11-21 refresh points the same checks at the new
raw/protected runs, the new static analysis, the new real entry and the new cleanup proof.
"""
from pathlib import Path
import argparse, datetime, hashlib, json, re
from PIL import Image, ImageChops

R = Path(__file__).resolve().parent
REPO = R.parents[2]

ap = argparse.ArgumentParser(description=__doc__)
ap.add_argument('--raw', default='evidence/raw-signed-20261116-154841/summary.json')
ap.add_argument('--protected', default='evidence/protected-verification-20261116-154658-342.json')
ap.add_argument('--trial', default='evidence/reactor-client-trial/seven-case-summary.json')
ap.add_argument('--trial-root', default='evidence/reactor-client-trial')
ap.add_argument('--trial-raw-ui', default=None,
                help='Raw ui folder used as the pixel baseline of the trial variant (default: the raw ui folder of --raw).')
ap.add_argument('--raw-ui', default=None,
                help='Raw ui folder used as the pixel baseline of the release variant (default: the ui folder of --raw).')
ap.add_argument('--release-ui', default=None,
                help='Protected ui folder compared against --raw-ui (default: the ui folder of --protected).')
ap.add_argument('--server-results', default='LicensingSystem/server-tests/bin/Release/net10.0/test-runs/20261116-070520-73be182a/results.json')
ap.add_argument('--cleanup', default='evidence/session-cleanup-cloud-20261116.json')
ap.add_argument('--real-entry', default='evidence/protected-real-entry-lifecycle-20261116.json')
ap.add_argument('--static', default='evidence/release-static-analysis-20261116b/summary.json')
ap.add_argument('--out', default='evidence/validation-summary.json')
ap.add_argument('--allow-global-sessions', type=int, default=0,
                help='Active cloud sessions owned by out-of-scope owners that are tolerated in addition to the fixture checks.')
ap.add_argument('--limitation', action='append', default=[], help='Additional limitation recorded in the summary.')
args = ap.parse_args()


def resolve(value):
    path = Path(value)
    if path.is_absolute():
        return path
    if value.replace('\\', '/').startswith('LicensingSystem/'):
        return REPO / path
    return R / path


def load(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def read_log(path):
    """Verification logs are written by Windows PowerShell and may be UTF-16LE."""
    data = Path(path).read_bytes()
    if data.startswith((b'\xff\xfe', b'\xfe\xff')):
        return data.decode('utf-16')
    return data.decode('utf-8-sig', errors='replace')


def log_timings(entries):
    """Extract the heartbeat/activation timings from the recorded protected execution logs."""
    logs = {row['Name']: Path(row['Log']) for row in entries}
    timings = {}
    coordinator = logs.get('coordinator')
    if coordinator and coordinator.is_file():
        text = read_log(coordinator)
        def stamp(prefix, endpoint, index=0):
            found = re.findall(prefix + r' \[(\d+)ms\] ' + endpoint, text)
            return int(found[index]) if len(found) > index else None
        activate_http = stamp('HTTP', r'api/v1/activate')
        activate_reply = stamp('REPLY', r'api/v1/activate')
        heartbeat_http = stamp('HTTP', r'api/v1/heartbeat')
        heartbeat_reply = stamp('REPLY', r'api/v1/heartbeat')
        if activate_http is not None and activate_reply is not None:
            timings['activationHttpMs'] = activate_reply - activate_http
        if heartbeat_http is not None and heartbeat_reply is not None:
            timings['firstHeartbeatHttpMs'] = heartbeat_reply - heartbeat_http
        if activate_reply is not None and heartbeat_http is not None:
            timings['heartbeatIntervalMs'] = heartbeat_http - activate_reply
    network = logs.get('network')
    if network and network.is_file():
        text = read_log(network)
        samples = re.findall(r'STATE \[(\d+)ms\] network_wait', text)
        if samples:
            timings['networkGateClosedObservedMs'] = int(samples[-1])
        else:
            found = re.findall(r'PASS \[(\d+)ms\] real 28-second attempt and real 60-second lease expiry', text)
            if found:
                timings['networkGateClosedObservedMs'] = int(found[0])
    return timings


raw = load(resolve(args.raw))
protected = load(resolve(args.protected))
trial = load(resolve(args.trial))
server = load(resolve(args.server_results))
server_checks = len(server['checks'])
assert server['passed'] == server_checks and server_checks >= 120
assert len(raw['results']) == len(protected) == len(trial['Cases']) == 7
assert all(v['exitCode'] == 0 for v in raw['results'])
assert all(v['ExitCode'] == 0 for v in protected)
assert trial['AllPassed'] and trial['PassedCount'] == 7
raw_cases = {c['case']: Path(c['sandbox']) for c in raw['results']}
protected_cases = {c['Name']: Path(c['Result']).parent for c in protected}
trial_root = resolve(args.trial_root)
for folder in list(raw_cases.values()) + list(protected_cases.values()):
    assert load(folder / 'result.json')['passed'] is True
assert load(trial_root / 'ui-result.json')['passed'] is True
assert load(trial_root / 'audio-audio-result.json')['passed'] is True
trial_raw_ui = Path(args.trial_raw_ui) if args.trial_raw_ui else raw_cases['ui']
release_raw_ui = Path(args.raw_ui) if args.raw_ui else raw_cases['ui']
release_ui = Path(args.release_ui) if args.release_ui else protected_cases['ui']
pictures = []
for variant, ui_folder in [('release', release_ui), ('reactorTrial', trial_root)]:
    baseline = release_raw_ui if variant == 'release' else trial_raw_ui
    for name in ['main', 'expanded', 'compact']:
        a, b = baseline / (name + '.png'), ui_folder / (name + '.png')
        with Image.open(a) as im_a, Image.open(b) as im_b:
            assert im_a.size == im_b.size
            diff = ImageChops.difference(im_a.convert('RGB'), im_b.convert('RGB'))
            changed = sum(1 for pixel in diff.get_flattened_data() if pixel != (0, 0, 0))
            pictures.append({'variant': variant, 'name': name, 'size': list(im_a.size), 'changedPixels': changed,
                             'originalSha256': sha(a), 'comparedSha256': sha(b)})
            assert changed == 0
audio = load(raw_cases['audio'] / 'audio-result.json')
audio_comparison = []
for variant, source in [('release', protected_cases['audio'] / 'audio-result.json'), ('reactorTrial', trial_root / 'audio-audio-result.json')]:
    other = load(source)
    identical = audio['decoded'] == other['decoded'] and audio['files'] == other['files'] == 8
    assert identical and other['passed']
    audio_comparison.append({'variant': variant, 'decodedFiles': 8, 'payloadsIdentical': identical, 'audiblePlaybackTested': False})
latest = load(R / 'evidence/latest-build.json')
assert sha(Path(latest['Client'])).upper() == latest['OutputSha256']
cleanup = load(resolve(args.cleanup))
assert cleanup['passed'] and len(cleanup['checks']) == 12 and cleanup['globalActiveSessions'] <= args.allow_global_sessions
entry = load(resolve(args.real_entry))
assert entry['WindowCaptured'] and entry['Closed']
static = load(resolve(args.static))
timings = log_timings(protected)
summary = {
    'generatedAt': datetime.datetime.now(datetime.timezone.utc).isoformat(),
    'passed': True,
    'serverChecksPassed': server_checks,
    'serverChecksTotal': server_checks,
    'serverTestRun': resolve(args.server_results).parent.name,
    'rawPassingModes': 7,
    'releasePassingModes': 7,
    'reactorTrialPassingModes': 7,
    'rawSha256': raw['clientSHA256'],
    'releaseSha256': latest['OutputSha256'],
    'reactorTrialSha256': trial['TrialSHA256'],
    'sources': {
        'raw': str(resolve(args.raw).relative_to(R)) if str(resolve(args.raw)).startswith(str(R)) else str(resolve(args.raw)),
        'protected': str(resolve(args.protected).relative_to(R)) if str(resolve(args.protected)).startswith(str(R)) else str(resolve(args.protected)),
        'trial': str(resolve(args.trial).relative_to(R)) if str(resolve(args.trial)).startswith(str(R)) else str(resolve(args.trial)),
        'serverResults': str(resolve(args.server_results).relative_to(REPO)),
        'sessionCleanup': str(resolve(args.cleanup).relative_to(R)) if str(resolve(args.cleanup)).startswith(str(R)) else str(resolve(args.cleanup)),
        'realEntry': str(resolve(args.real_entry).relative_to(R)) if str(resolve(args.real_entry)).startswith(str(R)) else str(resolve(args.real_entry)),
        'staticAnalysis': str(resolve(args.static).relative_to(R)) if str(resolve(args.static)).startswith(str(R)) else str(resolve(args.static)),
        'trialPixelBaseline': str(trial_raw_ui),
        'releasePixelBaseline': str(release_raw_ui),
        'releasePixelFolder': str(release_ui),
    },
    'pictures': pictures,
    'audio': audio_comparison,
    'sessionCleanupChecks': 12,
    'globalActiveSessions': cleanup['globalActiveSessions'],
    'allowedForeignSessions': cleanup.get('allowedForeignSessions', 0),
    'foreignSessionNote': cleanup.get('foreignSessionNote', ''),
    'realEntry': entry,
    'releaseReaCounts': static['records'][1]['counts'],
    'releaseIlspyExportExitCode': static['records'][2]['exitCode'],
    'releaseIlspyCSharpFiles': static['records'][2]['csharpFiles'],
    'protectedTiming': timings,
    'timingSource': 'protected-coordinator/network execution logs of the recorded run; elapsed values are wall time since harness startup, except interval subtraction',
    'deployment': {'baseUrl': 'https://124.220.147.56/', 'transport': 'TLS with a publicly trusted Let\'s Encrypt IP certificate', 'endpoint': 'nginx 1.29.8 -> http://127.0.0.1:47835 (ASP.NET Core 10) on 124.220.147.56'},
    'limitations': [
        'Reactor output is a 14-day demo experiment and excluded from every delivery archive.',
        'ILSpy still exports release C#; obfuscation raises analysis cost.',
        'Preview pixels and decoded audio do not prove live-game correctness or audible playback.',
        'Public deployment, another machine, long sessions and protection patch resistance are not tested.',
    ] + list(args.limitation),
}
output = resolve(args.out)
if output.exists():
    raise SystemExit('Refusing to overwrite final comparison summary.')
output.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'passed': True, 'server': server_checks, 'modesPerVariant': 7, 'zeroPixelComparisons': len(pictures),
                  'audioVariants': 2, 'sessionChecks': 12, 'out': str(output)}, ensure_ascii=True))
