import os, base64, json, urllib.request

OUT = r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis\refs"
os.makedirs(OUT, exist_ok=True)
REPO = "SLoSkady/MECMM"
FILES = [
    "tools/sdk/fbhash.ps1",
    "tools/sdk/TypeInfo.cs",
    "tools/sdk/Inspect.cs",
    "tools/sdk/Instances.cs",
    "docs/research/05-type-info-sdk.md",
    "docs/research/02-player-position.md",
    "docs/research/03-camera.md",
    "docs/research/01-loading-and-overlay.md",
    "docs/research/04-depth-buffer.md",
    "docs/ru/explained.md",
    "README.md",
]

proxy = "http://127.0.0.1:18081"
opener = urllib.request.build_opener(urllib.request.ProxyHandler({"http": proxy, "https": proxy}))

ok, fail = [], []
for f in FILES:
    dest = os.path.join(OUT, f.replace("/", "__"))
    url = "https://api.github.com/repos/%s/contents/%s" % (REPO, f)
    try:
        req = urllib.request.Request(url, headers={"User-Agent": "curl/8", "Accept": "application/vnd.github+json"})
        with opener.open(req, timeout=45) as r:
            raw = r.read()
        j = json.loads(raw.decode("utf-8"))
        data = base64.b64decode(j["content"])
        with open(dest, "wb") as h:
            h.write(data)
        ok.append((f, len(data)))
    except Exception as e:
        fail.append((f, repr(e)[:200]))

print("OK:")
for f, n in ok:
    print("  %-45s %d bytes" % (f, n))
print("FAIL:")
for f, e in fail:
    print("  %-45s %s" % (f, e))
