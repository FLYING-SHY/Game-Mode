#!/usr/bin/env python3
"""Shared helpers for the p2_* BF6 reflection-metadata probes.

Re-uses the section parsing approach from probe_typeinfo.py / probe_dataptrs2.py
but centralises it so the new probes stay consistent.
"""
import re
import struct
from pathlib import Path

import numpy as np

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUTDIR = Path(__file__).resolve().parent

IDENT = re.compile(r"^[A-Za-z_][A-Za-z0-9_:<>, .\-]{0,120}$")


def parse_sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    nsec = struct.unpack_from("<H", data, pe + 6)[0]
    opt = struct.unpack_from("<H", data, pe + 20)[0]
    base = pe + 24 + opt
    out = []
    for i in range(nsec):
        o = base + i * 40
        name = data[o:o + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, o + 8)
        out.append(dict(name=name, va=vaddr, vsize=vsize, roff=roff, rsize=rsize))
    return out


class Img:
    def __init__(self, path=GAME):
        self.data = path.read_bytes()
        self.secs = parse_sections(self.data)
        self.sec = {s["name"]: s for s in self.secs}
        self.data = self.data

    # -- addressing ------------------------------------------------------
    def rva2off(self, rva):
        for s in self.secs:
            size = max(s["rsize"], s["vsize"])
            if s["va"] <= rva < s["va"] + size:
                return s["roff"] + (rva - s["va"])
        return None

    def off2rva(self, off):
        for s in self.secs:
            if s["roff"] <= off < s["roff"] + s["rsize"]:
                return s["va"] + (off - s["roff"])
        return None

    def sec_of(self, rva):
        for s in self.secs:
            size = max(s["rsize"], s["vsize"])
            if s["va"] <= rva < s["va"] + size:
                return s["name"]
        return "?"

    def blob(self, secname):
        s = self.sec[secname]
        return self.data[s["roff"]:s["roff"] + s["rsize"]]

    # -- strings ---------------------------------------------------------
    def cstr(self, off, limit=200):
        if off is None or off < 0 or off >= len(self.data):
            return None
        end = self.data.find(b"\0", off, off + limit)
        if end <= off:
            return None
        try:
            t = self.data[off:end].decode("utf-8")
        except UnicodeDecodeError:
            return None
        if not t or not all(32 <= ord(c) < 127 for c in t):
            return None
        return t

    def cstr_at_rva(self, rva, limit=200):
        return self.cstr(self.rva2off(rva), limit)

    def sec_range(self, secname):
        s = self.sec[secname]
        return s["va"], s["va"] + s["rsize"]


def hexdump(data, base_rva, start, length):
    out = []
    for k in range(0, length, 16):
        chunk = data[start + k:start + k + 16]
        if len(chunk) < 16:
            break
        h = " ".join(f"{b:02x}" for b in chunk)
        a = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
        out.append(f"{base_rva + k:012x}  {h}  {a}")
    return out
