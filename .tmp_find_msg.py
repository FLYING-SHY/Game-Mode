import os, sys

ROOT = r'F:\WorkSpace\Game'
NEEDLES = ['完整性校验失败', '完整性', '授权记录']
SKIP_DIRS = {'.git', '.cache', 'tools', 'node_modules'}

hits = []
for dirpath, dirnames, filenames in os.walk(ROOT):
    dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS]
    for fn in filenames:
        p = os.path.join(dirpath, fn)
        try:
            size = os.path.getsize(p)
            if size > 60 * 1024 * 1024:
                continue
            data = open(p, 'rb').read()
        except Exception:
            continue
        texts = []
        for enc in ('utf-8', 'utf-16-le', 'gbk'):
            try:
                texts.append((enc, data.decode(enc, errors='ignore')))
            except Exception:
                pass
        for enc, text in texts:
            for n in NEEDLES:
                if n in text:
                    idx = text.find(n)
                    snippet = text[max(0, idx - 60): idx + 60].replace('\n', ' ').replace('\r', ' ')
                    hits.append((os.path.relpath(p, ROOT), enc, n, snippet))
                    break

for h in hits:
    print('|'.join(h))
print('total hits:', len(hits))
