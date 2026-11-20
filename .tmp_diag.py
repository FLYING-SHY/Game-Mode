import html, json, re, socket, urllib.parse, urllib.request, http.cookiejar, datetime

HOST = '124.220.147.56'
base = 'https://124.220.147.56/'
cfg = json.loads(open(r'F:\WorkSpace\Game\LicensingSystem\.local\hardening-v2\server.json', encoding='utf-8-sig').read())

jar = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))

def req(path, data=None, headers=None):
    r = urllib.request.Request(urllib.parse.urljoin(base, path), data=data, headers=headers or {})
    try:
        with opener.open(r, timeout=20) as resp:
            return resp.status, resp.read().decode('utf-8-sig')
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode('utf-8-sig', 'replace')

st, login = req('admin/login')
m = re.search(r'name=__RequestVerificationToken\s+value="([^"]+)"', login)
form = urllib.parse.urlencode({'token': cfg['AdminToken'], '__RequestVerificationToken': html.unescape(m.group(1))}).encode()
st, page = req('admin/login', form, {'Content-Type': 'application/x-www-form-urlencoded'})
csrf = html.unescape(re.search(r'<meta\s+name="csrf-token"\s+content="([^"]+)"', page).group(1))
H = {'X-CSRF-TOKEN': csrf}

out = {}
for name, path in [('audit', 'api/admin/audit?page=1&pageSize=40'),
                   ('sessions', 'api/admin/sessions?activeOnly=false'),
                   ('codes', 'api/admin/codes?page=1&pageSize=20')]:
    st, body = req(path, headers=H)
    try:
        out[name] = json.loads(body)
    except Exception:
        out[name] = {'http': st, 'raw': body[:300]}
    print('==', name, 'http', st)

Path = r'F:\WorkSpace\Game\.tmp_admin_dump.json'
json.dump(out, open(Path, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print('dumped ->', Path)

try:
    s = socket.create_connection((HOST, 22), timeout=6); s.settimeout(6)
    print('SSH banner:', s.recv(64).decode('utf-8', 'replace').strip()[:50]); s.close()
except Exception as e:
    print('SSH:', type(e).__name__, e)
