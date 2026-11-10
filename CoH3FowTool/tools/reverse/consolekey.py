#!/usr/bin/env python3
"""Locate console-key config parsing in RelicCoH3.exe (new build).

1. find string occurrences (consoleKey / consoleKey_SCAR / RelicConsole / Debug Console)
2. map file offset -> RVA
3. find RIP-relative xrefs in .text
4. dump the .pdata function containing each xref

Usage:
    python consolekey.py [needle ...]
"""
import bisect
import struct
import sys

import capstone
import pefile

EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"


def load():
    data = open(EXE, "rb").read()
    pe = pefile.PE(EXE, fast_load=True)
    secs = []
    for s in pe.sections:
        secs.append((s.Name.rstrip(b"\x00").decode(), s.VirtualAddress,
                     s.PointerToRawData, s.SizeOfRawData))
    return data, pe, secs


def off_to_rva(secs, off):
    for name, va, ro, rs in secs:
        if ro <= off < ro + rs:
            return va + (off - ro), name
    return None, None


def rva_to_off(secs, rva):
    for name, va, ro, rs in secs:
        if va <= rva < va + max(rs, 1):
            return ro + (rva - va), name
    return None, None


def pdata_entries(pe):
    try:
        entries = []
        for e in pe.DIRECTORY_ENTRY_EXCEPTION:
            entries.append((e.struct.BeginAddress, e.struct.EndAddress))
        entries.sort()
        return entries
    except Exception:
        return []


def func_of(pd, rva):
    if not pd:
        return None
    starts = [e[0] for e in pd]
    i = bisect.bisect_right(starts, rva) - 1
    if i < 0:
        return None
    b, en = pd[i]
    if b <= rva < en:
        return b, en
    return None


def main():
    needles = sys.argv[1:] or [b"consoleKey", b"consoleKey_SCAR", b"RelicConsole",
                               b"Debug Console", b"open_console", b"close_console",
                               b"echo_console", b"SendConsoleCommand"]
    data, pe, secs = load()
    pd = pdata_entries(pe)
    text = None
    for name, va, ro, rs in secs:
        if name == ".text":
            text = (va, ro, rs)
    tva, tro, trs = text

    for nd in needles:
        print("=" * 70)
        print("needle:", nd)
        start = 0
        while True:
            i = data.find(nd, start)
            if i < 0:
                break
            start = i + 1
            rva, sec = off_to_rva(secs, i)
            print("  string file=0x%08X rva=0x%08X (%s)" % (i, rva or 0, sec))
            if rva is None or sec == ".text":
                continue
            # scan .text for rip-relative refs
            blob = data[tro:tro + trs]
            hits = []
            for k in range(len(blob) - 4):
                disp = struct.unpack_from("<i", blob, k)[0]
                if tva + k + 4 + disp == rva:
                    hits.append(tva + k)
            for h in hits[:8]:
                fn = func_of(pd, h)
                print("    xref rva=0x%08X  func=%s" % (h, fn))
            if not hits:
                print("    (no rip-relative xref)")


if __name__ == "__main__":
    main()
