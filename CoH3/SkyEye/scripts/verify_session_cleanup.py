"""Read-only administrator verification; persist no code, token, device, or identity values."""
import datetime, html, http.cookiejar, json, re, urllib.parse, urllib.request
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
config = json.loads(Path(r'F:\WorkSpace\Game\LicensingSystem\.local\hardening-v2\server.json').read_text(encoding='utf-8-sig'))
base = config['ListenUrl'].rstrip('/') + '/'
if urllib.parse.urlsplit(base).hostname != '127.0.0.1':
    raise SystemExit('Expected isolated loopback service.')
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
def get(path, data=None):
    req = urllib.request.Request(urllib.parse.urljoin(base, path), data=data)
    with opener.open(req, timeout=15) as response:
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
        sessions = response.get('items') if isinstance(response, dict) else None
        if not isinstance(sessions, list):
            raise SystemExit('Unexpected session response shape.')
        checks.append({'fixtureSet': fixture_name, 'case': fixture['name'], 'activeSessions': len(sessions), 'passed': len(sessions) == 0})
summary = {'checkedAt': datetime.datetime.now(datetime.timezone.utc).isoformat(),
           'transport': 'real HTTP authenticated administrator read-only session query',
           'passed': all(item['passed'] for item in checks), 'checks': checks,
           'limitation': 'No active session at observation time; protocol finally/logout is additionally evidenced by awaited client execution.'}
output = ROOT / 'evidence' / 'session-cleanup.json'
if output.exists():
    raise SystemExit('Refusing to replace prior cleanup evidence.')
output.write_text(json.dumps(summary, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'checks': len(checks), 'passed': summary['passed']}))
if not summary['passed']:
    raise SystemExit(1)
