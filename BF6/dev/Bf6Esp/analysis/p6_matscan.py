#!/usr/bin/env python3
"""p6_matscan.py - hunt for view / projection matrix blobs in bf6.exe static data.

Strategy: perspective (projection) matrices have a rigid zero/signed-one
structure.  Scan every 4-byte aligned 64-byte window in the data-ish
sections and keep windows whose layout matches a perspective projection
within tolerance.  Also keep rigid "view matrix" candidates (orthonormal
3x3 + translation in the last row) for comparison.

Output: analysis/p6_matscan.json
"""
import json
import math
import struct
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402


def zeros_like(a, tol):
    return np.abs(a) <= tol


def perspective_score(M):
    """M is 4x4 row-major numpy. Return (ok, reason)."""
    # structural zeros of an affine projection
    checks = [
        ("m01", M[0, 1]), ("m03", M[0, 3]),
        ("m10", M[1, 0]), ("m13", M[1, 3]),
        ("m20", M[2, 0]), ("m21", M[2, 1]),
        ("m30", M[3, 0]), ("m31", M[3, 1]),
        ("m33", M[3, 3]),
    ]
    for name, v in checks:
        if abs(float(v)) > 1e-4:
            return False, "nonzero %s=%g" % (name, v)
    if abs(abs(float(M[2, 3])) - 1.0) > 1e-3:
        return False, "m23=%g not +-1" % float(M[2, 3])
    if abs(float(M[3, 2])) < 1e-6:
        return False, "m32 zero"
    return True, "ok"


def view_score(M):
    """Affine view matrix: upper 3x3 orthonormal, translation in last row."""
    R = M[:3, :3]
    if abs(float(M[3, 3]) - 1.0) > 1e-4:
        return False, "m33!=1"
    if abs(float(M[0, 3])) > 1e-4 or abs(float(M[1, 3])) > 1e-4 or abs(float(M[2, 3])) > 1e-4:
        return False, "translation in wrong place"
    # columns orthonormal
    for i in range(3):
        n = float(np.linalg.norm(R[:, i]))
        if abs(n - 1.0) > 0.02:
            return False, "col %d norm %g" % (i, n)
    dot = float(R[:, 0] @ R[:, 1])
    if abs(dot) > 0.02:
        return False, "col0.col1=%g" % dot
    t = M[3, :3]
    if float(np.linalg.norm(t)) < 1e-3:
        return False, "no translation"
    return True, "ok"


def main():
    img = Img()
    out = {"perspective": [], "view": []}
    for secname in (".data", ".rdata", "ctr", ".rodata"):
        if secname not in img.sec:
            continue
        s = img.sec[secname]
        blob = img.data[s["roff"]:s["roff"] + s["rsize"]]
        n = len(blob)
        # sliding 4-byte aligned window over the whole section
        idx = np.arange(0, n - 63, 4)
        arr = np.frombuffer(blob[: (n // 4) * 4], dtype=np.float32)
        # build window view: shape (num_windows, 16)
        w = np.lib.stride_tricks.sliding_window_view(arr, 16)[::1]
        # sliding_window_view over axis 0 -> shape (len-15, 16); take every 4th start
        w = w[::1]
        # Only every 4th element start corresponds to 16-byte aligned float index
        # (float index i -> byte offset 4i; we want byte offset multiple of 4 => all i ok)
        # but we want 16-byte aligned blocks for cleanliness -> i % 4 == 0
        w = w[::4]
        M = w.reshape(-1, 4, 4)
        # quick structural prefilter using vectorised ops
        m01 = np.abs(M[:, 0, 1])
        m03 = np.abs(M[:, 0, 3])
        m10 = np.abs(M[:, 1, 0])
        m13 = np.abs(M[:, 1, 3])
        m20 = np.abs(M[:, 2, 0])
        m21 = np.abs(M[:, 2, 1])
        m30 = np.abs(M[:, 3, 0])
        m31 = np.abs(M[:, 3, 1])
        m33 = np.abs(M[:, 3, 3])
        m23 = np.abs(M[:, 2, 3])
        m32 = np.abs(M[:, 3, 2])
        struct_mask = (
            (m01 < 1e-4) & (m03 < 1e-4) & (m10 < 1e-4) & (m13 < 1e-4)
            & (m20 < 1e-4) & (m21 < 1e-4) & (m30 < 1e-4) & (m31 < 1e-4)
            & (m33 < 1e-4) & (np.abs(m23 - 1.0) < 1e-3) & (m32 > 1e-6)
        )
        for i in np.nonzero(struct_mask)[0]:
            mm = M[i]
            ok, why = perspective_score(mm)
            if ok:
                rva = s["va"] + int(i) * 16
                out["perspective"].append({
                    "section": secname, "rva": rva, "va": 0x140000000 + rva,
                    "values": [float(x) for x in mm.reshape(-1)],
                })
        # view candidates
        vmask = (np.abs(M[:, 3, 3] - 1.0) < 1e-4) & (np.abs(M[:, 0, 3]) < 1e-4) \
            & (np.abs(M[:, 1, 3]) < 1e-4) & (np.abs(M[:, 2, 3]) < 1e-4)
        for i in np.nonzero(vmask)[0]:
            mm = M[i]
            ok, why = view_score(mm)
            if ok:
                rva = s["va"] + int(i) * 16
                out["view"].append({
                    "section": secname, "rva": rva, "va": 0x140000000 + rva,
                    "values": [float(x) for x in mm.reshape(-1)],
                })
        print("%-8s windows=%d  persp=%d view=%d" % (secname, M.shape[0], len(out["perspective"]), len(out["view"])))

    (OUTDIR / "p6_matscan.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
    print("perspective hits:", len(out["perspective"]), " view hits:", len(out["view"]))
    for h in out["perspective"][:40]:
        print("  PERSP %s rva=0x%x %s" % (h["section"], h["rva"], [round(v, 5) for v in h["values"]]))
    for h in out["view"][:40]:
        print("  VIEW  %s rva=0x%x %s" % (h["section"], h["rva"], [round(v, 5) for v in h["values"]]))


if __name__ == "__main__":
    main()
