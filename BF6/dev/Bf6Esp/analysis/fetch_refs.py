import os, sys, urllib.request, hashlib

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
]

def fetch(url):
    req = urllib.request.Request(url, headers={"User-Agent": "curl/8"})
    with urllib.request.urlopen(req, timeout=30) as r:
        return r.read()

ok, fail = [], []
for f in FILES:
    dest = os.path.join(OUT, f.replace("/", "__"))
    try:
        data = fetch(BASE + f)
        with open(dest, "wb") as h:
            h.write(data)
        ok.append((f, len(data)))
    except Exception as e:
        fail.append((f, repr(e)))

print("OK:")
for f, n in ok:
    print("  %-45s %d bytes" % (f, n))
print("FAIL:")
for f, e in fail:
    print("  %-45s %s" % (f, e))
