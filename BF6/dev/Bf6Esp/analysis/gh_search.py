import json, urllib.request, urllib.parse, sys

proxy = "http://127.0.0.1:18081"
opener = urllib.request.build_opener(urllib.request.ProxyHandler({"http": proxy, "https": proxy}))

def get(url):
    req = urllib.request.Request(url, headers={"User-Agent": "curl/8", "Accept": "application/vnd.github+json"})
    with opener.open(req, timeout=45) as r:
        return json.loads(r.read().decode("utf-8"))

QUERIES = [
    "bf2042 sdk dumper",
    "frostbite sdk dumper",
    "battlefield 6 external esp",
    "bf6 memory reader",
    "frostbite typeinfo",
    "battlefield 2042 esp",
    "frostbite entity manager",
    "bf2042 typeinf",
]

seen = set()
for q in QUERIES:
    url = "https://api.github.com/search/repositories?q=%s&sort=stars&per_page=15" % urllib.parse.quote(q)
    try:
        j = get(url)
    except Exception as e:
        print("QUERY %r FAILED %r" % (q, e))
        continue
    print("=== %s (total %d) ===" % (q, j.get("total_count")))
    for it in j.get("items", []):
        n = it["full_name"]
        if n in seen:
            continue
        seen.add(n)
        print("  %-45s stars=%-5s size=%-8s %s" % (n, it.get("stargazers_count"), it.get("size"), (it.get("description") or "")[:90]))
