import os, urllib.request

OUT = r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis\refs"
os.makedirs(OUT, exist_ok=True)
BASE = "https://raw.githubusercontent.com/SLoSkady/MECMM/main/"
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

proxy = "http://127.0.0.1:18080"
opener = urllib.request.build_opener(urllib.request.ProxyHandler({"http": proxy, "https": proxy}))

ok, fail = [], []
for f in FILES:
    dest = os.path.join(OUT, f.replace("/", "__"))
    try:
        req = urllib.request.Request(BASE + f, headers={"User-Agent": "curl/8"})
        with opener.open(req, timeout=45) as r:
            data = r.read()
        with open(dest, "wb") as h:
            h.write(data)
        ok.append((f, len(data)))
    except Exception as e:
        fail.append((f, repr(e)[:160]))

print("OK:")
for f, n in ok:
    print("  %-45s %d bytes" % (f, n))
print("FAIL:")
for f, e in fail:
    print("  %-45s %s" % (f, e))
