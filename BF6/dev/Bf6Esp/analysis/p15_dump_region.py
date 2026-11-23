#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""p15_dump_region.py —— 定点查看 .rdata 里某个 RVA 区域的结构。

用法：python p15_dump_region.py <startRva> <length>
输出：hex + ASCII 视图，以及该区域里所有可打印串（含偏移）。
"""
import io
import json
import struct
import sys

EXE = r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe"
P13 = r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis\p13_names.json"


def load_sections():
    with io.open(P13, encoding="utf-8") as fh:
        d = json.load(fh)
    return d["sections"]


def main():
    start = int(sys.argv[1], 0)
    length = int(sys.argv[2], 0)
    sections = load_sections()
    sec = None
    for s in sections:
        if s["vaddr"] <= start < s["vaddr"] + s["rawsize"]:
            sec = s
            break
    if sec is None:
        raise SystemExit("RVA 不在任何节内")
    off = sec["rawoff"] + (start - sec["vaddr"])
    with io.open(EXE, "rb") as fh:
        fh.seek(off)
        blob = fh.read(length)

    lines = []
    for i in range(0, len(blob), 16):
        chunk = blob[i:i + 16]
        hexpart = " ".join("%02X" % b for b in chunk)
        asciipart = "".join(chr(b) if 0x20 <= b < 0x7F else "." for b in chunk)
        lines.append("0x%08X  %-47s  %s" % (start + i, hexpart, asciipart))
    out = io.open(r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis\logs\region_%08X.txt" % start, "w", encoding="utf-8")
    out.write("\n".join(lines))
    out.close()

    # 串清单
    print("=== strings ===")
    i = 0
    while i < len(blob):
        if 0x20 <= blob[i] < 0x7F:
            j = i
            while j < len(blob) and blob[j] != 0:
                j += 1
            s = blob[i:j].decode("ascii", "replace")
            if j < len(blob) and blob[j] == 0 and len(s) >= 3:
                print("0x%08X  len=%-3d  %r" % (start + i, len(s), s))
            i = j + 1
        else:
            i += 1


if __name__ == "__main__":
    main()
