"""Byte-level grep across files for one or more ASCII needles, printing a text window around hits.

Usage: python bingrep.py <file-or-dir> <needle> [needle2 ...]
"""
import os
import sys

NEEDLES = [n.encode("ascii", "ignore") for n in sys.argv[2:]]
ROOT = sys.argv[1]
WINDOW = 260


def scan(path):
    hits = []
    with open(path, "rb") as f:
        blob = f.read()
    for needle in NEEDLES:
        start = 0
        while True:
            i = blob.find(needle, start)
            if i < 0:
                break
            hits.append((i, needle))
            start = i + 1
            if len(hits) > 200:
                break
    return blob, hits


def main():
    files = []
    if os.path.isfile(ROOT):
        files = [ROOT]
    else:
        for dirpath, _dirs, names in os.walk(ROOT):
            for n in names:
                files.append(os.path.join(dirpath, n))
    for path in files:
        try:
            blob, hits = scan(path)
        except Exception:
            continue
        if not hits:
            continue
        print("=== %s (%d hits) ===" % (path, len(hits)))
        for off, needle in hits[:20]:
            lo = max(0, off - WINDOW)
            hi = min(len(blob), off + len(needle) + WINDOW)
            chunk = blob[lo:hi]
            txt = "".join(chr(c) if 32 <= c < 127 else ("\n" if c == 10 else ".") for c in chunk)
            print("--- offset 0x%X (%s) ---" % (off, needle.decode()))
            print(txt)
            print()


if __name__ == "__main__":
    main()
