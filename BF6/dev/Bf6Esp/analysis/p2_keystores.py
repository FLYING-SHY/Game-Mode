"""lead experiment 2 (light): every store into the typeinfo section, grouped by
enclosing function, with the 10172 type-registration stubs excluded.

Scans .text with a CORRECTED RIP-relative table (p2_code2.FORMS overstates the
length of the 0f-forms by one byte, which shifts their targets by +1).

Output: p2_key_stores.json
"""
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

REGS = ["rax", "rcx", "rdx", "rbx", "rsp", "rbp", "rsi", "rdi",
        "r8", "r9", "r10", "r11", "r12", "r13", "r14", "r15"]
RM = {0x05, 0x0D, 0x15, 0x1D, 0x25, 0x2D, 0x35, 0x3D}

# (prefix, disp_off, ilen, kind)   ilen = full instruction length
STORE_FORMS = [
    (b"\x48\x89", 3, 7, "mov"),   # mov [rip+disp32], r64
    (b"\x4c\x89", 3, 7, "mov"),   # mov [rip+disp32], r64 (r8..r15)
    (b"\x89", 2, 6, "mov"),       # mov [rip+disp32], r32
    (b"\x0f\x11", 3, 7, "movups"),  # movups [rip+disp32], xmm
    (b"\x48\xc7\x05", 3, 11, "movq"),  # mov qword [rip+disp32], imm32
    (b"\xc7\x05", 2, 10, "movd"),     # mov dword [rip+disp32], imm32
]
LEA_FORMS = [
    (b"\x48\x8d", 3, 7),
    (b"\x4c\x8d", 3, 7),
    (b"\x8d", 2, 6),
]


def scan(blob, base_rva, forms):
    out = []
    for fi, form in enumerate(forms):
        pref, doff, ilen = form[0], form[1], form[2]
        kind = form[3] if len(form) > 3 else "lea"
        start = 0
        while True:
            p = blob.find(pref, start)
            if p < 0:
                break
            start = p + 1
            if p + ilen > len(blob):
                continue
            modrm = blob[p + len(pref)]
            if modrm not in RM:
                continue
            disp = int.from_bytes(blob[p + doff:p + doff + 4], "little", signed=True)
            code_rva = base_rva + p
            tgt = code_rva + ilen + disp
            out.append((code_rva, tgt, fi, kind, modrm))
    return out


def main():
    img = Img()
    data = img.data
    sec = img.sec

    # ---- .pdata function table
    pd = sec[".pdata"]
    b = data[pd["roff"]:pd["roff"] + pd["rsize"]]
    n = len(b) // 12
    arr = np.frombuffer(b[:n * 12], dtype="<u4").reshape(n, 3)
    beg = arr[:, 0].astype(np.int64)
    end = arr[:, 1].astype(np.int64)
    keep = (beg > 0) & (end > beg)
    beg, end = beg[keep], end[keep]
    print(f"pdata functions: {len(beg)}")

    ti = sec["typeinfo"]
    ti_lo, ti_hi = ti["va"], ti["va"] + ti["rsize"]
    rd = sec[".rdata"]
    rd_lo, rd_hi = rd["va"], rd["va"] + rd["rsize"]

    tx = sec[".text"]
    blob = data[tx["roff"]:tx["roff"] + tx["rsize"]]
    print(f"scanning .text ({len(blob)} bytes) ...")
    stores = scan(blob, tx["va"], STORE_FORMS)
    leas = scan(blob, tx["va"], LEA_FORMS)
    print(f"  store candidates: {len(stores)}  lea candidates: {len(leas)}")

    def func_of(rva):
        i = int(np.searchsorted(beg, rva, side="right")) - 1
        if i >= 0 and beg[i] <= rva < end[i]:
            return int(beg[i]), int(end[i])
        return None

    # ---- functions that LEA a .rdata string (registration stubs)
    lea_funcs = defaultdict(int)
    for cr, tgt, fi, kind, modrm in leas:
        if rd_lo <= tgt < rd_hi:
            f = func_of(cr)
            if f:
                lea_funcs[f[0]] += 1
    print(f"functions LEA-ing a .rdata string: {len(lea_funcs)}")

    # ---- stores into the typeinfo section
    hits = [s for s in stores if ti_lo <= s[1] < ti_hi]
    print(f"store refs into the typeinfo section: {len(hits)}")

    by_func = defaultdict(list)
    for cr, tgt, fi, kind, modrm in hits:
        f = func_of(cr)
        by_func[f[0] if f else -1].append((cr, tgt, fi, kind, modrm, f))

    stub_funcs = {fs for fs in by_func if fs in lea_funcs}
    cand_funcs = {fs for fs in by_func if fs not in lea_funcs}
    print(f"  enclosing functions: {len(by_func)}")
    print(f"  registration stubs (store into typeinfo AND LEA .rdata): {len(stub_funcs)}")
    print(f"  candidate non-registration functions: {len(cand_funcs)}")

    def disas(cr, kind, modrm, ilen_map):
        off = img.rva2off(cr)
        raw = data[off:off + ilen_map]
        if kind == "movq":
            return f"mov qword ptr [rip+0x{disp_txt(raw,3):x}], 0x{int.from_bytes(raw[7:11],'little'):x}"
        if kind == "movd":
            return f"mov dword ptr [rip+0x{disp_txt(raw,2):x}], 0x{int.from_bytes(raw[6:10],'little'):x}"
        reg = (modrm >> 3) & 7
        if raw[0] in (0x48, 0x4C):
            reg += 8
        if kind == "movups":
            return f"movups xmmword ptr [rip+0x{disp_txt(raw,3):x}], xmm{reg}"
        if kind == "mov":
            r64 = raw[0] in (0x48, 0x4C)
            return f"mov qword ptr [rip+0x{disp_txt(raw,3):x}], {REGS[reg]}" if r64 \
                else f"mov dword ptr [rip+0x{disp_txt(raw,2):x}], {REGS[reg]}"

    def disp_txt(raw, off):
        return int.from_bytes(raw[off:off + 4], "little", signed=True)

    ilen_map = {0: 7, 1: 7, 2: 6, 3: 7, 4: 11, 5: 10}

    candidates = []
    for fs in sorted(cand_funcs):
        items = sorted(by_func[fs])
        f = items[0][5]
        recs = []
        for cr, tgt, fi, kind, modrm, ff in items:
            off = img.rva2off(cr)
            recs.append({
                "code_rva": hex(cr),
                "target_rva": hex(tgt),
                "instr": disas(cr, kind, modrm, ilen_map[fi]),
                "target_bytes": data[img.rva2off(tgt):img.rva2off(tgt) + 16].hex(" "),
                "code_window": data[off - 16:off + 16].hex(" "),
            })
        candidates.append({
            "func_rva": hex(fs),
            "func_end": hex(f[1]) if f else None,
            "n_stores_into_typeinfo": len(items),
            "rdata_lea_count": lea_funcs.get(fs, 0),
            "stores": recs[:64],
        })

    # also record, for completeness, the stub functions summary
    stub_top = []
    for fs in sorted(stub_funcs, key=lambda k: -len(by_func[k]))[:40]:
        stub_top.append({"func_rva": hex(fs), "n_stores": len(by_func[fs]),
                         "rdata_lea_count": lea_funcs.get(fs, 0)})

    out = {
        "generated": "p2_keystores.py",
        "note": "opcode-valid RIP-relative stores whose target lies inside the typeinfo "
                "section; registration stubs (functions that also LEA a .rdata string) excluded.",
        "typeinfo_section_rva": [hex(ti_lo), hex(ti_hi)],
        "text_scan_bytes": len(blob),
        "store_candidates_in_text": len(stores),
        "lea_candidates_in_text": len(leas),
        "functions_lea_rdata": len(lea_funcs),
        "store_refs_into_typeinfo": len(hits),
        "enclosing_functions": len(by_func),
        "registration_stub_functions": len(stub_funcs),
        "candidate_functions": len(cand_funcs),
        "candidates": candidates,
        "registration_stub_top": stub_top,
    }
    (HERE / "p2_key_stores.json").write_text(json.dumps(out, indent=1), encoding="utf-8")
    print("wrote p2_key_stores.json")
    for c in candidates[:15]:
        print(f"  cand func {c['func_rva']} stores={c['n_stores_into_typeinfo']} "
              f"rdata_lea={c['rdata_lea_count']} first={c['stores'][0]['instr'] if c['stores'] else ''}")


if __name__ == "__main__":
    main()
