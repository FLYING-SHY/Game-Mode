#!/usr/bin/env python3
"""M1: byte-level decode of the Frostbite `typeinfo` record layout.

Evidence chain:
  * section `typeinfo` (RVA 0xAECC000, raw 0xA84DE00, 0x5BEC00 bytes)
  * marker b'\x89\x00\x00\x08' occurs 23701 times, consecutive delta 0x50 in
    23550 cases  -> record stride 0x50, marker sits at record offset +4.

This script:
  1. rebuilds the record grid,
  2. builds a per-field (u32 index and u64 index) histogram + classification,
  3. locates which slot holds the .rdata pointer(s) and dumps 10 of the
     pointer targets raw (are they NUL strings or tables of u32/u64?),
  4. writes p2_typeinfo_layout.json + p2_anchor_pairs.json (name<->record
     pairs used by the M2 hash attack).
"""
import json
import struct
import sys
from collections import Counter
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE, OUTDIR, hexdump  # noqa: E402

STRIDE = 0x50
# NOTE: the earlier probe wrote the marker as "89 00 00 08" but that was a
# mis-read of the hex dump "89000800".  The real byte sequence in the file is
# 89 00 08 00  (u32 = 0x00080089).
MARKER = b"\x89\x00\x08\x00"


def find_marks(blob):
    marks = []
    pos = 0
    while True:
        i = blob.find(MARKER, pos)
        if i < 0:
            break
        marks.append(i)
        pos = i + 1
    return marks


def main():
    img = Img()
    data = img.data
    ti = img.sec["typeinfo"]
    blob = data[ti["roff"]:ti["roff"] + ti["rsize"]]
    rlo, rhi = img.sec_range(".rdata")
    tlo, thi = img.sec_range(".text")
    dlo, dhi = img.sec_range(".data")

    marks = find_marks(blob)
    print("markers:", len(marks))

    # ---- longest run of consecutive markers spaced exactly 0x50 ----------
    best = (0, 0, 0)  # start_index, count, stride
    i = 0
    runs = []
    while i < len(marks):
        j = i + 1
        while j < len(marks) and marks[j] - marks[j - 1] == STRIDE:
            j += 1
        runs.append((marks[i], j - i, marks[j - 1] if j > i else marks[i]))
        if (j - i) > best[1]:
            best = (marks[i], j - i, STRIDE)
        i = j
    runs.sort(key=lambda r: -r[1])
    print("top marker runs (off,count,last):", [(hex(r[0]), r[1], hex(r[2])) for r in runs[:6]])

    rec0 = runs[0][0] - 4          # marker is at +4 inside the record
    nrec = runs[0][1]
    print(f"grid: rec0=0x{rec0:x} nrec={nrec} end=0x{rec0 + nrec * STRIDE:x} "
          f"(section size {len(blob):#x})")

    body = blob[rec0:rec0 + nrec * STRIDE]
    u32 = np.frombuffer(body, dtype="<u4").reshape(nrec, STRIDE // 4)
    u64 = np.frombuffer(body, dtype="<u8").reshape(nrec, STRIDE // 8)

    report = {
        "section": {"name": "typeinfo", "rva": hex(ti["va"]), "raw_off": hex(ti["roff"]),
                    "raw_size": ti["rsize"], "vsize": ti["vsize"]},
        "marker": {"bytes": MARKER.hex(), "count": len(marks),
                   "stride": STRIDE, "marker_offset_in_record": 4,
                   "grid_first_record_file_off": hex(ti["roff"] + rec0),
                   "grid_records": nrec,
                   "top_runs": [{"file_off": hex(ti["roff"] + r[0]), "count": r[1]} for r in runs[:8]]},
    }

    # ---- per-field histogram / classification (u32 view) -----------------
    fields32 = []
    for j in range(u32.shape[1]):
        col = u32[:, j]
        vals, cnts = np.unique(col, return_counts=True)
        order = np.argsort(-cnts)[:8]
        zero = int((col == 0).sum())
        small = int(((col > 0) & (col < 0x1000)).sum())
        rdata_hit = int(((col >= rlo) & (col < rhi)).sum())
        text_hit = int(((col >= tlo) & (col < thi)).sum())
        data_hit = int(((col >= dlo) & (col < dhi)).sum())
        off8 = int(((col > 0) & (col < 0x400) & (col % 8 == 0)).sum())
        fields32.append({
            "index": j, "byte_off": hex(j * 4),
            "zero": zero, "small_lt_0x1000": small,
            "rdata_rva": rdata_hit, "text_rva": text_hit, "data_rva": data_hit,
            "aligned_offset_lt_0x400": off8,
            "distinct": int(len(vals)),
            "top": [[hex(int(vals[k])), int(cnts[k])] for k in order],
        })

    # ---- per-field classification (u64 view) -----------------------------
    fields64 = []
    for j in range(u64.shape[1]):
        col = u64[:, j].astype(np.int64)
        vals, cnts = np.unique(col, return_counts=True)
        order = np.argsort(-cnts)[:8]
        zero = int((col == 0).sum())
        va = int(((col >= IMAGE_BASE) & (col < IMAGE_BASE + 0x100000000)).sum())
        rdata_hit = int(((col - IMAGE_BASE >= rlo) & (col - IMAGE_BASE < rhi)).sum())
        text_hit = int(((col - IMAGE_BASE >= tlo) & (col - IMAGE_BASE < thi)).sum())
        fields64.append({
            "index": j, "byte_off": hex(j * 8),
            "zero": zero, "va_like": va,
            "rdata_ptr": rdata_hit, "text_ptr": text_hit,
            "distinct": int(len(vals)),
            "top": [[hex(int(vals[k]) & 0xFFFFFFFFFFFFFFFF), int(cnts[k])] for k in order],
        })

    report["fields_u32"] = fields32
    report["fields_u64"] = fields64

    # ---- where do the .rdata pointers live? -----------------------------
    ptr_mask = np.zeros_like(u64, dtype=bool)
    for j in range(u64.shape[1]):
        col = u64[:, j].astype(np.int64) - IMAGE_BASE
        ptr_mask[:, j] = (col >= rlo) & (col < rhi)
    slot_counts = {j: int(ptr_mask[:, j].sum()) for j in range(u64.shape[1])}
    print("rdata-pointer slots:", slot_counts)

    rows, cols = np.nonzero(ptr_mask)
    ptr_rec_idx = rows
    ptr_slot = cols
    ptr_tgt_rva = (u64[rows, cols] - IMAGE_BASE).astype(np.int64)

    # ---- dump the pointer targets raw ------------------------------------
    def classify_target(rva):
        off = img.rva2off(int(rva))
        if off is None:
            return {"rva": hex(int(rva)), "status": "unmapped"}
        raw = data[off:off + 64]
        u32s = list(struct.unpack_from("<16I", raw, 0))
        u64s = list(struct.unpack_from("<8Q", raw, 0))
        s = img.cstr(off, 120)
        # does the 64 bytes look like a table of u32 RVAs into .rdata?
        rva_like = sum(1 for v in u32s if rlo <= v < rhi)
        off_like = sum(1 for v in u32s if 0 < v < 0x1000)
        return {
            "rva": hex(int(rva)), "file_off": hex(off),
            "raw64": raw.hex(),
            "u32": [hex(v) for v in u32s],
            "u64": [hex(v) for v in u64s],
            "cstr": s,
            "u32_in_rdata_range": rva_like,
            "u32_small_int": off_like,
            "verdict": ("NUL-string" if s else
                        ("table-of-u32-rvas" if rva_like >= 8 else
                         ("table-of-small-offsets" if off_like >= 8 else "other"))),
        }

    picks = []
    seen_t = set()
    for i in range(min(len(ptr_tgt_rva), 4000)):
        t = int(ptr_tgt_rva[i])
        if t in seen_t:
            continue
        seen_t.add(t)
        picks.append(classify_target(t))
        if len(picks) >= 12:
            break
    report["pointer_target_dumps"] = picks

    # ---- anchor pairs: (name string, record u32 fields) -----------------
    anchors = []
    for i in range(len(ptr_tgt_rva)):
        t = int(ptr_tgt_rva[i])
        off = img.rva2off(t)
        if off is None:
            continue
        s = img.cstr(off, 120)
        if not s or len(s) < 2:
            continue
        r = int(ptr_rec_idx[i])
        anchors.append({
            "rec": r,
            "rec_file_off": hex(ti["roff"] + rec0 + r * STRIDE),
            "name": s,
            "name_rva": hex(t),
            "slot": int(ptr_slot[i]),
            "u32": [hex(int(x)) for x in u32[r]],
        })
    report["anchor_count"] = len(anchors)
    report["anchor_sample"] = anchors[:40]

    # ---- 10 annotated example records -----------------------------------
    examples = []
    order = np.argsort(-(u32 != 0).sum(axis=1))
    for r in list(order[:10]):
        r = int(r)
        ann = []
        for j in range(u32.shape[1]):
            v = int(u32[r, j])
            if v == 0:
                continue
            note = ""
            if rlo <= v < rhi:
                note = "->rdata rva " + hex(v) + " " + repr(img.cstr_at_rva(v) or "")
            ann.append({"u32_index": j, "value": hex(v), "note": note})
        ex = {
            "rec_index": r,
            "file_off": hex(ti["roff"] + rec0 + r * STRIDE),
            "raw80": blob[rec0 + r * STRIDE:rec0 + (r + 1) * STRIDE].hex(),
            "u32": [hex(int(x)) for x in u32[r]],
            "u64": [hex(int(x)) for x in u64[r]],
            "annotated_nonzero": ann,
        }
        examples.append(ex)
    report["example_records"] = examples

    # ---- first 8 raw records (grid head) --------------------------------
    report["grid_head"] = [
        {"rec": r,
         "file_off": hex(ti["roff"] + rec0 + r * STRIDE),
         "raw80": blob[rec0 + r * STRIDE:rec0 + (r + 1) * STRIDE].hex(),
         "u32": [hex(int(x)) for x in u32[r]]}
        for r in range(8)
    ]

    # ---- consistency of the "record" claim: does slot1 == 0x08000089? ---
    report["marker_slot_check"] = {
        "u32_index1_equals_0x08000089": int((u32[:, 1] == 0x08000089).sum()),
        "u32_index1_equals_0x08000089_frac": float((u32[:, 1] == 0x08000089).mean()),
        "any_other_slot_constant": {
            str(j): [hex(int(x)) for x in np.unique(u32[:, j])[:4].tolist()]
            for j in range(u32.shape[1])
            if len(np.unique(u32[:, j])) <= 4
        },
    }

    (OUTDIR / "p2_typeinfo_layout.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    (OUTDIR / "p2_anchor_pairs.json").write_text(json.dumps({
        "stride": STRIDE, "rec0_file_off": hex(ti["roff"] + rec0),
        "anchors": anchors,
    }, indent=1), encoding="utf-8")
    print("written p2_typeinfo_layout.json / p2_anchor_pairs.json")
    print("anchors:", len(anchors))
    for p in picks[:12]:
        print("  tgt", p["rva"], p["verdict"], repr(p.get("cstr")))


if __name__ == "__main__":
    main()
