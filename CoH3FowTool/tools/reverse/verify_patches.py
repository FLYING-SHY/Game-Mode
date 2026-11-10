#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
verify_patches.py —— 直接对真实 exe 反汇编, 核对 2.0 的 6 个显示补丁点的
"原始字节 -> 补丁后字节" 语义是否正确, 并报告磁盘上的当前状态。

用法:
    python verify_patches.py [RelicCoH3.exe 路径]
默认指向 G:\\SteamLibrary\\steamapps\\common\\Company of Heroes 3\\RelicCoH3.exe
"""
import sys, os, struct
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

DEFAULT_EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

# name, anchor hex, anchor->target 偏移, target 长度, 原始字节, 补丁字节
SITES = [
    ("TerrainFrame",
     "20 5B C3 B0 01 EB F6 CC CC CC CC CC CC CC CC CC 40 53 48 83 EC 20 48 8B D9 E8 B2 05 00 00 84 C0",
     32, 2, "74 09", "90 90"),
    ("EntityDisplay", "8B DA 48 8B F1 E8 A1 ED F9 FF 84 C0", 12, 2, "74 0D", "90 90"),
    ("VisibilityA", "84 BA 00 00 00 E8 BA 0F 00 00 84 C0", 12, 2, "74 09", "90 90"),
    ("VisibilityB", "8B F2 48 8B E9 E8 60 05 00 00 84 C0", 12, 2, "74 0D", "90 90"),
    ("VisibilityC", "41 C6 04 24 00 E8 BF 01 00 00 84 C0", 12, 2, "74 0D", "90 90"),
    ("VisibilityD", "84 88 00 00 00 E8 7C E6 FF FF 84 C0", 12, 2, "74 09", "90 90"),
]


def hx(s):
    return bytes(int(x, 16) for x in s.split())


def sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    n = struct.unpack_from("<H", data, pe + 6)[0]
    opt = pe + 24
    base = struct.unpack_from("<Q", data, opt + 24)[0]
    out = []
    off = pe + 24 + struct.unpack_from("<H", data, pe + 20)[0]
    for i in range(n):
        name = data[off:off + 8].rstrip(b"\0").decode("latin1")
        vs, va, rs, ro = struct.unpack_from("<IIII", data, off + 8)
        out.append((name, va, vs, ro, rs))
        off += 40
    return base, out


def rva_of(base, secs, off):
    for name, va, vs, ro, rs in secs:
        if ro <= off < ro + rs:
            return base + va + (off - ro)
    return None


def find_all(data, pat):
    res, i = [], data.find(pat)
    while i >= 0:
        res.append(i)
        i = data.find(pat, i + 1)
    return res


def disasm(md, code, va):
    return ["%s  %s" % (i.mnemonic, i.op_str) for i in md.disasm(code, va)]


def main():
    exe = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_EXE
    if not os.path.exists(exe):
        print("找不到 exe:", exe)
        return 1
    data = open(exe, "rb").read()
    base, secs = sections(data)
    md = Cs(CS_ARCH_X86, CS_MODE_64)

    print("exe   : %s" % exe)
    print("size  : %d bytes" % len(data))
    print()
    problems = 0
    for name, anchor, off, ln, orig, patched in SITES:
        hits = find_all(data, hx(anchor))
        if len(hits) != 1:
            print("[%s] 锚点命中 %d 次 (期望 1) -- %s" % (name, len(hits), "跳过"))
            problems += 1
            continue
        toff = hits[0] + off
        cur = data[toff:toff + ln]
        va = rva_of(base, secs, toff)
        print("[%s] file 0x%08X  rva 0x%X" % (name, toff, (va - base) if va else -1))
        print("   原始 : %s" % cur.hex(" "))
        print("   期望 : %s" % hx(orig).hex(" "))
        print("   补丁后: %s" % hx(patched).hex(" "))
        print("   原始指令  : %s" % " ; ".join(disasm(md, cur, va)))
        print("   补丁后指令: %s" % " ; ".join(disasm(md, hx(patched), va)))
        if cur == hx(orig):
            print("   磁盘状态: [原始]")
        elif cur == hx(patched):
            print("   磁盘状态: [已补]")
        else:
            print("   磁盘状态: [!! 不认识的字节]")
            problems += 1
        print()

    print("ALL OK" if problems == 0 else "PROBLEMS: %d" % problems)
    return 0 if problems == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
