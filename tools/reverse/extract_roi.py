"""Extract function-sized ROIs from RelicCoH3.exe as raw binaries for REA/Ghidra.

Each ROI starts exactly at a function's first byte (per .pdata) so that
--target-format raw-binary --entry-point <VA> maps file offset 0 to the
function entry and Ghidra analyses it as the program entry.
"""
import os
import struct
import sys

import pefile

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "roi")

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()
IMAGE_BASE = pe.OPTIONAL_HEADER.ImageBase

secs = []
for s in pe.sections:
    secs.append(dict(
        name=s.Name.rstrip(b"\x00").decode(),
        va=s.VirtualAddress,
        ro=s.PointerToRawData,
        rs=s.SizeOfRawData,
        vs=s.Misc_VirtualSize,
    ))


def rva2off(rva):
    for s in secs:
        if s["va"] <= rva < s["va"] + max(s["rs"], s["vs"]):
            return s["ro"] + (rva - s["va"])
    return None


def off2rva(off):
    for s in secs:
        if s["ro"] <= off < s["ro"] + s["rs"]:
            return s["va"] + (off - s["ro"])
    return None


# --- .pdata -> function boundaries ---
pdata = next(s for s in secs if s["name"] == ".pdata")
pd = raw[pdata["ro"]:pdata["ro"] + pdata["rs"]]
funcs = []
for i in range(0, len(pd) - 11, 12):
    begin, end, unwind = struct.unpack_from("<III", pd, i)
    if begin and end and end > begin:
        funcs.append((begin, end))
funcs.sort()
starts = [f[0] for f in funcs]


def fn_bounds(rva):
    import bisect
    i = bisect.bisect_right(starts, rva) - 1
    if i < 0:
        return None
    b, e = funcs[i]
    if b <= rva < e:
        return b, e
    return None


# ROI definitions: name -> (target rva, pad bytes before/after for context)
ROIS = [
    ("cheatgate", 0x0216B3D0, 0x40, 0x200),
    ("docommand", 0x021F7F00, 0x40, 0x800),
    ("revealall", 0x0350C6F0, 0x40, 0x600),
    ("telemetry", 0x037D9350, 0x40, 0x1000),
    ("statlog", 0x037986E0, 0x40, 0x600),
    ("statlog2", 0x037A6C00, 0x40, 0x600),
]


def main():
    os.makedirs(OUT, exist_ok=True)
    manifest = []
    for name, rva, pre, post in ROIS:
        b = fn_bounds(rva)
        if b is None:
            # not a known .pdata function: use the rva as-is
            start_rva = rva & ~0xF
            end_rva = start_rva + 0x400
        else:
            start_rva, end_rva = b
        size = (end_rva - start_rva) + pre + post
        off = rva2off(start_rva - pre)
        if off is None:
            print("skip", name)
            continue
        data = raw[off:off + size]
        path = os.path.join(OUT, name + ".bin")
        with open(path, "wb") as f:
            f.write(data)
        # file offset 0 of the ROI maps to the VA of (start_rva - pre)
        base_va = IMAGE_BASE + start_rva - pre
        entry_va = IMAGE_BASE + start_rva
        manifest.append((name, path, len(data), base_va, entry_va, start_rva, end_rva))
        print("%-10s file_off=0x%08X  rva=0x%08X..0x%08X  len=%d  entry=0x%016X" %
              (name, off, start_rva, end_rva, len(data), entry_va))
    with open(os.path.join(OUT, "roi_manifest.txt"), "w", encoding="utf-8") as f:
        for name, path, size, base_va, entry_va, sr, er in manifest:
            f.write("%s\t%s\t%d\t0x%016X\t0x%016X\t0x%08X\t0x%08X\n" %
                    (name, path, size, base_va, entry_va, sr, er))
    return 0


if __name__ == "__main__":
    sys.exit(main())
