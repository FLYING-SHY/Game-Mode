#!/usr/bin/env python3
"""Decode the metadata record layout that sits next to Frostbite property names.

For every 8-byte slot in .data / .rdata that points at a known reflection name,
dump +-0x60 bytes as hex + resolved qwords (any qword landing on a printable
.rdata string is annotated with that string). This shows whether an offset /
type-id / hash sits next to the name, i.e. whether we can resolve member
offsets by name at runtime.

Output: analysis/meta_neighbourhoods.json
"""
import json
import re
import struct
from pathlib import Path

GAME = Path(r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe")
IMAGE_BASE = 0x140000000
OUT = Path(__file__).with_name("meta_neighbourhoods.json")
IDENT = re.compile(r"^[ -~]{2,100}$")


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


def main():
    data = GAME.read_bytes()
    secs = parse_sections(data)
    sec = {s["name"]: s for s in secs}
    rdata = sec[".rdata"]
    rlo, rhi = rdata["va"], rdata["va"] + rdata["rsize"]

    def rva_to_off(rva):
        for s in secs:
            if s["va"] <= rva < s["va"] + s["rsize"]:
                return s["roff"] + (rva - s["va"])
        return None

    def off_to_rva(off):
        for s in secs:
            if s["roff"] <= off < s["roff"] + s["rsize"]:
                return s["va"] + (off - s["roff"])
        return None

    def str_at_rva(rva):
        o = rva_to_off(rva)
        if o is None:
            return None
        end = data.find(b"\0", o, o + 120)
        if end <= o:
            return None
        t = data[o:end]
        try:
            s = t.decode("ascii")
        except UnicodeDecodeError:
            return None
        return s if IDENT.match(s) else None

    # locate every occurrence of the reflection names in the file, then find
    # 8-byte slots anywhere in the image that point at them.
    targets = [
        "TeamId", "Health", "LocalPlayer", "ClientGame", "GameContext",
        "ClientPlayerManager", "ClientSoldierEntity", "HealthComponent",
        "ClientPlayer", "ClientConnection", "Player_%d", "OnlineManager",
    ]
    slots = {}   # target -> list of (section, slot_rva)
    for t in targets:
        tb = t.encode()
        pos = 0
        while True:
            i = data.find(tb, pos)
            if i < 0:
                break
            pos = i + 1
            # only treat as the name itself if it is NUL/space delimited
            if i > 0 and data[i - 1] not in (0, 0x20, 0x3A, 0x2C, 0x3B, 0x5B, 0x28):
                continue
            j = i + len(tb)
            if j < len(data) and data[j] not in (0, 0x20, 0x3A, 0x2C, 0x3B, 0x29, 0x3D):
                continue
            rva = off_to_rva(i)
            if rva is None:
                continue
            slots.setdefault(t, []).append(rva)

    # now scan whole file for qword pointers to those rvas
    ptr_hits = []
    import numpy as np
    arr = np.frombuffer(data, dtype=np.uint8)
    for t, rvas in slots.items():
        for rva in rvas:
            needle = struct.pack("<Q", IMAGE_BASE + rva)
            start = 0
            while True:
                i = data.find(needle, start)
                if i < 0:
                    break
                start = i + 1
                srva = off_to_rva(i)
                if srva is None:
                    continue
                owner = next((s["name"] for s in secs
                              if s["va"] <= srva < s["va"] + s["rsize"]), "?")
                ptr_hits.append({"target": t, "name_rva": hex(rva),
                                 "slot_rva": hex(srva), "sec": owner})

    print("pointer hits:", len(ptr_hits))
    from collections import Counter
    print(Counter(h["sec"] for h in ptr_hits))

    # neighbourhoods for the .data hits
    out = []
    for h in ptr_hits:
        if h["sec"] != ".data":
            continue
        rva = int(h["slot_rva"], 16)
        off = rva_to_off(rva)
        win = data[off - 0x40:off + 0x80]
        lines = []
        for k in range(0, len(win), 16):
            chunk = win[k:k + 16]
            if len(chunk) < 16:
                break
            hexs = " ".join(f"{b:02x}" for b in chunk)
            ann = []
            for q in range(0, 16, 8):
                v = struct.unpack_from("<Q", chunk, q)[0]
                if IMAGE_BASE <= v < IMAGE_BASE + 0x10000000:
                    s = str_at_rva(v - IMAGE_BASE)
                    if s:
                        ann.append(f"+{k+q}:{s!r}")
            lines.append(f"{rva - 0x40 + k:012x}  {hexs}  {' '.join(ann)}")
        out.append({"target": h["target"], "slot_rva": h["slot_rva"], "dump": lines})

    OUT.write_text(json.dumps({"ptr_hits": ptr_hits[:4000], "n": len(ptr_hits),
                               "neighbourhoods": out[:60]}, indent=2), encoding="utf-8")
    print("written", OUT)
    for o in out[:6]:
        print("=" * 70)
        print(o["target"], o["slot_rva"])
        for l in o["dump"]:
            print("  ", l)


if __name__ == "__main__":
    main()
