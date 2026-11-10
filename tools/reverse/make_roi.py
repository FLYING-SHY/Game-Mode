#!/usr/bin/env python3
"""Cut small ROI raw binaries out of RelicCoH3.exe so REA/Ghidra can decompile
them in seconds instead of re-importing the 108 MB PE every time.

New build 26443338 layout:
    .text  file[0x600 .. 0x3D53200)  rva[0x1000 .. 0x3D53C00)   delta = +0xA00
    .rdata file[0x3D53200 .. 0x5BF8C00) rva[0x3D54000 .. 0x5BFA000) delta = +0xE00

Usage:
    python make_roi.py
"""
import os

EXE = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "roi")
IMAGE_BASE = 0x140000000

# name, start RVA, end RVA (exclusive, from .pdata unwind info)
TEXT_ROIS = [
    ("CheatGate",        0x03908B6C, 0x03908BC4),
    ("RevealAllHandler", 0x039091FC, 0x03909291),
    ("DoCommand",        0x004820C0, 0x00482133),
    ("TelemetryFn",      0x001D8BA0, 0x001D93D5),
]

# .rdata blob holding the "true" / "false" literals used by the telemetry block
RDATA_ROIS = [
    ("TelemetryStrings", 0x03D98B00, 0x03D98B80),
]


def main() -> None:
    os.makedirs(OUT, exist_ok=True)
    data = open(EXE, "rb").read()
    print("exe size", len(data))

    def emit(name, rva0, rva1):
        if rva1 <= 0x03D54000:
            off0, off1 = rva0 - 0xA00, rva1 - 0xA00
        else:
            off0, off1 = rva0 - 0xE00, rva1 - 0xE00
        blob = data[off0:off1]
        path = os.path.join(OUT, name + ".bin")
        with open(path, "wb") as fh:
            fh.write(blob)
        va = IMAGE_BASE + rva0
        print("%-20s rva=0x%08X..0x%08X file=0x%08X..0x%08X len=%d" % (
            name, rva0, rva1, off0, off1, len(blob)))
        print("    file : %s" % path)
        print("    cmd  : rea decompile --provider ghidra --target-format raw-binary "
              "--architecture x86_64 --entry-point 0x%X --file-offset 0x0 \"%s\"" % (va, path))
        return path

    for t in TEXT_ROIS:
        emit(*t)
    for t in RDATA_ROIS:
        emit(*t)


if __name__ == "__main__":
    main()
