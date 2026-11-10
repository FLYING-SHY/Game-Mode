#!/usr/bin/env python3
"""Build a minimal PE64 that mirrors the ORIGINAL image layout of RelicCoH3.exe,
so that every RIP-relative reference inside a cut-out function still resolves.

REA/Ghidra on Windows only admits native PE (raw-binary is 6502-only there), and
re-importing the 108 MB PE costs ~1 hour per query.  So: carve the wanted
function into a tiny PE that keeps the original VirtualAddress values.

Sections produced:
  .text   the cut-out function bytes, placed at its ORIGINAL RVA
  .rdata  the .rdata pages that the function references by RIP-relative lea,
          also at their ORIGINAL RVA (so the displacements stay correct)

Usage:
    python make_mini_pe.py
"""
import os
import struct

EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "mini_pe")
IMAGE_BASE = 0x140000000
FILE_ALIGN = 0x200
SECT_ALIGN = 0x1000

RDATA_VA = 0x03D54000
RDATA_SIZE = 0x1EA6000
RDATA_FILE_DELTA = 0xE00          # file_off = rva - 0xE00 inside .rdata
TEXT_FILE_DELTA = 0xA00           # file_off = rva - 0xA00 inside .text

# name, first rva (entry point), last rva (exclusive)
TARGETS = [
    ("CheatGate",        0x03908B6C, 0x03908BC4),
    ("RevealAllHandler", 0x039091FC, 0x03909291),
    ("DoCommand",        0x004820C0, 0x00482180),   # +0x4D keeps the execute path
    ("TelemetryBlock",   0x001D8F60, 0x001D8FD9),   # the used_cheats/debug_used block
    ("TelemetryFn",      0x001D8BA0, 0x001D93D5),   # whole stat-report function
]


def align_up(value, alignment):
    return (value + alignment - 1) // alignment * alignment


def rip_targets(blob, rva0):
    """Every RIP-relative lea target that the blob references."""
    out = []
    for i in range(len(blob) - 7):
        # 48 8D xx disp32 / 4C 8D xx disp32  (lea r64, [rip+disp])
        if blob[i] in (0x48, 0x4C) and blob[i + 1] == 0x8D and (blob[i + 2] & 0xC7) == 0x05:
            disp = struct.unpack_from("<i", blob, i + 3)[0]
            out.append(rva0 + i + 7 + disp)
    return out


def build_pe(sections, entry_rva):
    """sections: list of (name, virtual_address, data(bytes), characteristics)"""
    n = len(sections)
    dos = bytearray(0x40)
    dos[0:2] = b"MZ"
    struct.pack_into("<I", dos, 0x3C, 0x40)

    coff = struct.pack("<HHIIIHH", 0x8664, n, 0, 0, 0, 0xF0, 0x0022)

    headers_len = 0x40 + 4 + 20 + 0xF0 + 40 * n
    size_of_headers = align_up(headers_len, FILE_ALIGN)
    size_of_image = align_up(
        max(va + len(d) for _, va, d, _ in sections), SECT_ALIGN)

    size_of_code = sum(align_up(len(d), FILE_ALIGN) for nm, va, d, c in sections if c & 0x20)
    size_of_init = sum(align_up(len(d), FILE_ALIGN) for nm, va, d, c in sections if c & 0x40)

    opt = bytearray(0xF0)
    struct.pack_into("<H", opt, 0x00, 0x020B)
    opt[2] = 14
    struct.pack_into("<I", opt, 0x04, size_of_code)
    struct.pack_into("<I", opt, 0x08, size_of_init)
    struct.pack_into("<I", opt, 0x10, entry_rva)
    struct.pack_into("<I", opt, 0x14, sections[0][1])
    struct.pack_into("<Q", opt, 0x18, IMAGE_BASE)
    struct.pack_into("<I", opt, 0x20, SECT_ALIGN)
    struct.pack_into("<I", opt, 0x24, FILE_ALIGN)
    struct.pack_into("<HH", opt, 0x28, 6, 0)
    struct.pack_into("<HH", opt, 0x30, 6, 0)
    struct.pack_into("<I", opt, 0x38, size_of_image)
    struct.pack_into("<I", opt, 0x3C, size_of_headers)
    struct.pack_into("<H", opt, 0x44, 3)          # Subsystem = CONSOLE
    struct.pack_into("<H", opt, 0x46, 0x8160)
    struct.pack_into("<Q", opt, 0x48, 0x100000)
    struct.pack_into("<Q", opt, 0x50, 0x1000)
    struct.pack_into("<Q", opt, 0x58, 0x100000)
    struct.pack_into("<Q", opt, 0x60, 0x1000)
    struct.pack_into("<I", opt, 0x68, 16)
    # DataDirectory stays all-zero (no imports, no exception table, no relocs)

    sect_table = bytearray()
    raw_ptr = size_of_headers
    for name, va, data, chars in sections:
        raw_size = align_up(len(data), FILE_ALIGN)
        sect_table += struct.pack(
            "<8sIIIIIIHHI",
            name.encode().ljust(8, b"\0"), len(data), va, raw_size, raw_ptr,
            0, 0, 0, 0, chars)
        raw_ptr += raw_size

    header = bytes(dos) + b"PE\0\0" + coff + bytes(opt) + bytes(sect_table)
    header += b"\0" * (size_of_headers - len(header))

    body = bytearray()
    for name, va, data, chars in sections:
        raw_size = align_up(len(data), FILE_ALIGN)
        body += data + b"\0" * (raw_size - len(data))

    return header + bytes(body)


def main():
    os.makedirs(OUT, exist_ok=True)
    exe = open(EXE, "rb").read()

    for name, rva0, rva1 in TARGETS:
        off0 = rva0 - TEXT_FILE_DELTA
        off1 = rva1 - TEXT_FILE_DELTA
        code = exe[off0:off1]

        # keep the code at its original RVA
        text_va = rva0 & ~0xFFF
        text = bytearray(rva0 - text_va) + code

        # collect referenced .rdata pages, one section per contiguous run
        pages = set()
        for tgt in rip_targets(code, rva0):
            if RDATA_VA <= tgt < RDATA_VA + RDATA_SIZE:
                pages.add((tgt // 0x1000) * 0x1000)
        runs = []
        for page in sorted(pages):
            if runs and page == runs[-1][1]:
                runs[-1][1] = page + 0x1000
            else:
                runs.append([page, page + 0x1000])

        sections = [(".text", text_va, bytes(text), 0x60000020)]
        for idx, (lo, hi) in enumerate(runs):
            blob = bytearray(hi - lo)
            for page in range(lo, hi, 0x1000):
                src = page - RDATA_FILE_DELTA
                blob[page - lo:page - lo + 0x1000] = exe[src:src + 0x1000]
            sections.append((".rdata" if idx == 0 else ".rdata%d" % idx,
                              lo, bytes(blob), 0x40000040))

        blob = build_pe(sections, rva0)
        path = os.path.join(OUT, name + ".exe")
        with open(path, "wb") as fh:
            fh.write(blob)
        print("%-20s entry=0x%08X  text_va=0x%08X len=%d  rdata=%s  file=%d  -> %s" % (
            name, rva0, text_va, len(text),
            ",".join("0x%X-0x%X" % (lo, hi) for lo, hi in runs) or "-",
            len(blob), path))


if __name__ == "__main__":
    main()
