"""Read-only administrator verification against the deployed cloud licensing service.

Mirrors scripts/verify_session_cleanup.py but targets the published HTTPS endpoint instead of
the retired loopback service. Persist no code, token, device, or identity values.
"""
import argparse, datetime, html, http.cookiejar, json, re, urllib.parse, urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--base-url', default='https://124.220.147.56/')
parser.add_argument('--private-config', default=r'F:\WorkSpace\Game\LicensingSystem\.local\hardening-v2\server.json')
parser.add_argument('--out', default=str(ROOT / 'evidence' / 'session-cleanup-cloud-20261116.json'))
args = parser.parse_args()

config = json.loads(Path(args.private_config).read_text(encoding='utf-8-sig'))
base = args.base_url.rstrip('/') + '/'
if urllib.parse.urlsplit(base).scheme != 'https':
    raise SystemExit('Expected an https deployment endpoint.')
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

def get(path, data=None):
    request = urllib.request.Request(urllib.parse.urljoin(base, path), data=data)
    with opener.open(request, timeout=20) as response:
        return response.read().decode('utf-8-sig')

page = get('admin/login')
csrf = html.unescape(re.search(r'name=__RequestVerificationToken\s+value="([^"]+)"', page).group(1))
form = urllib.parse.urlencode({'token': config['AdminToken'], '__RequestVerificationToken': csrf}).encode()
page = get('admin/login', form)
if 'name="csrf-token"' not in page:
    raise SystemExit('Administrator authentication failed.')

checks = []
for fixture_name in ['raw-live-fixtures.json', 'protected-live-fixtures.json', 'reactor-trial-live-fixtures.json']:
    fixtures = json.loads((ROOT / 'evidence' / fixture_name).read_text(encoding='utf-8-sig'))
    for fixture in fixtures['fixtures'][:4]:
        query = urllib.parse.urlencode({'codeId': fixture['id'], 'activeOnly': 'true'})
        response = json.loads(get('api/admin/sessions?' + query))
        sessions = response.get('items')
        if not isinstance(sessions, list):
            raise SystemExit('Unexpected session response shape.')
        checks.append({'fixtureSet': fixture_name, 'case': fixture['name'], 'activeSessions': len(sessions), 'passed': len(sessions) == 0})

global_response = json.loads(get('api/admin/sessions?' + urllib.parse.urlencode({'activeOnly': 'true'})))
global_items = global_response.get('items')
if not isinstance(global_items, list):
    raise SystemExit('Unexpected global session response shape.')

summary = {
    'checkedAt': datetime.datetime.now(datetime.timezone.utc).isoformat(),
    'transport': 'real HTTPS authenticated administrator read-only session query',
    'endpoint': urllib.parse.urlsplit(base).hostname,
    'passed': all(item['passed'] for item in checks) and len(global_items) == 0,
    'checks': checks,
    'globalActiveSessions': len(global_items),
    'limitation': 'No active session at observation time; protocol finally/logout is additionally evidenced by awaited client execution.'
}
output = Path(args.out)
if output.exists():
    raise SystemExit('Refusing to replace prior cleanup evidence.')
output.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'checks': len(checks), 'globalActiveSessions': len(global_items), 'passed': summary['passed']}))
if not summary['passed']:
    raise SystemExit(1)
