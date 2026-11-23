#!/usr/bin/env python3
"""p2_xrefs: RIP-relative xref scanner for bf6.exe (M3).

Targets
  * the nine reflection names listed in the task (both the *given* file offsets
    and the real NUL-delimited occurrences in .rdata),
  * the `typeinfo` and `fieldinf` section bases (who reads them?).

For every hit the instruction bytes are captured and the RIP-relative opcode
form is decoded; for name references the following 64 bytes are scanned for
32-bit immediates that look like member offsets.
"""
import json
import re
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, IMAGE_BASE, OUTDIR  # noqa: E402

TASK_NAMES = [
    ("HealthComponent", 0x8DDA61F),
    ("ClientPlayerManager", 0x8DDA7B2),
    ("TeamId", 0x8F028F3),
    ("Health", 0x8FF9110),
    ("Health", 0x8FF91C0),
    ("LocalPlayer", 0x9198D10),
    ("ClientConnection", 0x90A3908),
    ("GameContext", 0x8E1A8E8),
    ("ClientSoldierEntity", 0x9212030),
]

# RIP-relative opcode forms: (prefix bytes, description)
FORMS = [
    (b"\x48\x8d\x0d", "lea rcx,[rip+d]"),
    (b"\x48\x8d\x05", "lea rax,[rip+d]"),
    (b"\x48\x8d\x15", "lea rdx,[rip+d]"),
    (b"\x48\x8d\x1d", "lea rbx,[rip+d]"),
    (b"\x48\x8d\x25", "lea rsp,[rip+d]"),
    (b"\x48\x8d\x2d", "lea rbp,[rip+d]"),
    (b"\x48\x8d\x35", "lea rsi,[rip+d]"),
    (b"\x48\x8d\x3d", "lea rdi,[rip+d]"),
    (b"\x4c\x8d\x05", "lea r8,[rip+d]"),
    (b"\x4c\x8d\x0d", "lea r9,[rip+d]"),
    (b"\x4c\x8d\x15", "lea r10,[rip+d]"),
    (b"\x4c\x8d\x1d", "lea r11,[rip+d]"),
    (b"\x4c\x8d\x25", "lea r12,[rip+d]"),
    (b"\x4c\x8d\x2d", "lea r13,[rip+d]"),
    (b"\x4c\x8d\x35", "lea r14,[rip+d]"),
    (b"\x4c\x8d\x3d", "lea r15,[rip+d]"),
    (b"\x48\x8b\x05", "mov rax,[rip+d]"),
    (b"\x48\x8b\x0d", "mov rcx,[rip+d]"),
    (b"\x48\x8b\x15", "mov rdx,[rip+d]"),
    (b"\x48\x8b\x35", "mov rsi,[rip+d]"),
    (b"\x48\x8b\x3d", "mov rdi,[rip+d]"),
    (b"\x4c\x8b\x05", "mov r8,[rip+d]"),
    (b"\x4c\x8b\x0d", "mov r9,[rip+d]"),
    (b"\x48\x89\x05", "mov [rip+d],rax"),
    (b"\x48\x89\x0d", "mov [rip+d],rcx"),
    (b"\x48\x89\x15", "mov [rip+d],rdx"),
    (b"\x4c\x89\x05", "mov [rip+d],r8"),
    (b"\x48\xc7\x05", "mov qword [rip+d],imm32"),
    (b"\x89\x05", "mov dword [rip+d],eax"),
    (b"\x8b\x05", "mov eax,[rip+d]"),
    (b"\x0f\x10", "movsd xmm0,[rip+d]"),
    (b"\x0f\x11", "movsd [rip+d],xmm0"),
]


def decode_form(code_bytes, disp_pos):
    """disp_pos = index of the disp32 inside code_bytes. Returns form or None."""
    for pre, desc in FORMS:
        L = len(pre)
        if disp_pos >= L and code_bytes[disp_pos - L:disp_pos] == pre:
            return desc, disp_pos - L
    return None, None


def main():
    img = Img()
    data = img.data
    rlo, rhi = img.sec_range(".rdata")

    # ---------------- target set -----------------------------------------
    targets = {}          # rva -> label
    target_kind = {}      # rva -> 'name' | 'section'
    for nm, foff in TASK_NAMES:
        rva = img.off2rva(foff)
        targets[rva] = f"{nm}@given_{foff:#x}"
        target_kind[rva] = "name"
    # all standalone (NUL delimited) occurrences of the same names
    for nm, _ in TASK_NAMES:
        pat = nm.encode()
        p = 0
        while True:
            i = data.find(pat, p)
            if i < 0:
                break
            p = i + 1
            before_ok = (i == 0) or data[i - 1] == 0
            after = i + len(pat)
            after_ok = after >= len(data) or data[after] == 0
            if not (before_ok and after_ok):
                continue
            rva = img.off2rva(i)
            if rva is None or rva in targets:
                continue
            targets[rva] = f"{nm}@standalone_{i:#x}"
            target_kind[rva] = "name"

    # section bases
    for nm in ("typeinfo", "fieldinf"):
        s = img.sec[nm]
        for off in (0, 0x10, 0x100):
            rva = s["va"] + off
            targets[rva] = f"section:{nm}+{off:#x}"
            target_kind[rva] = "section"

    trva = np.array(sorted(targets.keys()), dtype=np.int64)
    tset = set(int(x) for x in trva)
    print("targets:", len(tset))

    # ---------------- RIP-relative scan ----------------------------------
    CHUNK = 1 << 22
    allhits = []
    for sname in (".text", "ctr"):
        s = img.sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        n = len(blob) // 4
        for off in range(0, n, CHUNK):
            m = min(CHUNK, n - off)
            arr = np.frombuffer(blob, dtype="<u4", count=m, offset=off * 4).astype(np.int64)
            idx = np.arange(off, off + m, dtype=np.int64)
            disp = np.where(arr >= (1 << 31), arr - (1 << 32), arr)
            tgt = s["va"] + idx * 4 + 4 + disp
            mask = np.isin(tgt, trva)
            for pos in np.nonzero(mask)[0]:
                abs_pos = int(pos)          # index of the disp32 field (4-byte aligned)
                code_rva = s["va"] + (off + abs_pos) * 4
                tgt_rva = int(tgt[pos])
                # instruction bytes: 8 before the disp field + 8 after
                start = s["roff"] + (off + abs_pos) * 4 - 8
                cb = data[start:start + 24]
                form, ins_off = decode_form(cb, 8)
                allhits.append({
                    "sec": sname,
                    "code_rva": code_rva,
                    "disp_field_rva": code_rva,
                    "target_rva": tgt_rva,
                    "label": targets[tgt_rva],
                    "kind": target_kind[tgt_rva],
                    "instr_rva": (code_rva - 8 + ins_off) if ins_off is not None else None,
                    "form": form,
                    "bytes24": cb.hex(),
                })
        print(sname, "hits so far:", len(allhits))

    # ---------------- absolute 64-bit pointers in data sections ----------
    abs_ptrs = []
    for sname in (".rdata", ".data", "typeinfo", "fieldinf", "_RDATA", ".vlizer"):
        if sname not in img.sec:
            continue
        s = img.sec[sname]
        blob = data[s["roff"]:s["roff"] + s["rsize"]]
        arr = np.frombuffer(blob, dtype="<u8").astype(np.int64)
        rva_abs = arr - IMAGE_BASE
        mask = np.isin(rva_abs, trva)
        for pos in np.nonzero(mask)[0]:
            t = int(rva_abs[pos])
            abs_ptrs.append({"sec": sname, "slot_rva": s["va"] + int(pos) * 8,
                              "target_rva": t, "label": targets[t]})
    print("abs ptrs:", len(abs_ptrs))

    # ---------------- offset-immediate mining after name xrefs -----------
    def plausible_offset(v):
        return (0x08 <= v <= 0x2000) and (v % 4 == 0)

    for h in allhits:
        if h["kind"] != "name":
            continue
        # bytes after the disp field
        s = img.sec[h["sec"]]
        disp_off = s["roff"] + (h["disp_field_rva"] - s["va"])
        after = data[disp_off + 4: disp_off + 4 + 64]
        cands = []
        for k in range(0, len(after) - 3):
            v = int.from_bytes(after[k:k + 4], "little")
            if plausible_offset(v):
                cands.append({"after_off": hex(k + 4), "value": hex(v)})
        h["offset_like_immediates_after"] = cands

    out = {
        "targets": {hex(k): v for k, v in sorted(targets.items())},
        "rip": allhits,
        "abs_ptrs": abs_ptrs,
    }
    (OUTDIR / "p2_name_xrefs.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
    print("written p2_name_xrefs.json")

    # ---------------- console summary -----------------------------------
    for h in allhits:
        if h["kind"] != "name":
            continue
        print(f"{h['sec']} code@{h['code_rva']:#x} -> {h['label']}  form={h['form']} "
              f"imm={h.get('offset_like_immediates_after')}")
    print("--- section refs ---")
    for h in allhits:
        if h["kind"] == "section":
            print(f"{h['sec']} code@{h['code_rva']:#x} -> {h['label']} form={h['form']} "
                  f"bytes={h['bytes24']}")
    print("--- abs ptrs (first 40) ---")
    for a in abs_ptrs[:40]:
        print("  ", a)


if __name__ == "__main__":
    main()
