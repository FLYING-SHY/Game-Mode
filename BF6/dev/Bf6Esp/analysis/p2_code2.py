#!/usr/bin/env python3
"""p2_code2: opcode-valid RIP-relative reference scanner.

v1 (p2_xrefs.py) scanned every 4-byte word as if it were a displacement and
produced ~119k mostly-bogus "hits" into the huge typeinfo/fieldinf ranges.
This version only accepts byte sequences that are real RIP-relative x64
instructions, so every hit is a genuine code reference.

Outputs:
  p2_riprefs.npz   - (code_rva, target_rva, form_id) for .text + ctr
  p2_name_xrefs.json
"""
import json
import re
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from p2_common import Img, OUTDIR  # noqa: E402

# (prefix, disp_offset_from_prefix_start, instr_len, needs_rm_check)
FORMS = [
    (b"\x48\x8d", 3, 7, True), (b"\x4c\x8d", 3, 7, True), (b"\x8d", 2, 6, True),
    (b"\x48\x8b", 3, 7, True), (b"\x4c\x8b", 3, 7, True), (b"\x8b", 2, 6, True),
    (b"\x48\x89", 3, 7, True), (b"\x4c\x89", 3, 7, True), (b"\x89", 2, 6, True),
    (b"\x48\x63", 3, 7, True),
    (b"\x0f\xb6", 3, 7, True), (b"\x0f\xb7", 3, 7, True),
    (b"\x0f\x6f", 3, 8, True), (b"\x0f\x7e", 3, 8, True),
    (b"\x0f\x10", 3, 8, True), (b"\x0f\x11", 3, 8, True),
    (b"\xf2\x0f\x10", 4, 9, True), (b"\xf2\x0f\x11", 4, 9, True),
    (b"\x48\x0f\xb6", 4, 8, True), (b"\x48\x0f\xb7", 4, 8, True),
    (b"\xff\x15", 2, 6, False), (b"\xff\x25", 2, 6, False),
    (b"\x48\xc7\x05", 3, 11, False), (b"\xc7\x05", 2, 10, False),
]
RM = {0x05, 0x0D, 0x15, 0x1D, 0x25, 0x2D, 0x35, 0x3D}
IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}$")


def scan_section(blob, base_rva):
    """return arrays code_rva, target_rva, form_id, instr_len"""
    codes, targets, forms, lens = [], [], [], []
    for fi, (pref, doff, ilen, rmchk) in enumerate(FORMS):
        start = 0
        plen = len(pref)
        while True:
            p = blob.find(pref, start)
            if p < 0:
                break
            start = p + 1
            if p + ilen > len(blob):
                continue
            if rmchk:
                modrm = blob[p + plen]
                if modrm not in RM:
                    continue
            elif pref in (b"\xff\x15", b"\xff\x25"):
                modrm = blob[p + 2]
                if modrm != (0x15 if pref == b"\xff\x15" else 0x25):
                    continue
            disp = int.from_bytes(blob[p + doff:p + doff + 4], "little", signed=True)
            code_rva = base_rva + p
            tgt = code_rva + ilen + disp
            if tgt < 0 or tgt > 0x7FFFFFFF:
                continue
            codes.append(code_rva)
            targets.append(tgt)
            forms.append(fi)
            lens.append(ilen)
    return (np.array(codes, dtype=np.int64), np.array(targets, dtype=np.int64),
            np.array(forms, dtype=np.int32), np.array(lens, dtype=np.int32))


def main():
    img = Img()
    data = img.data
    allc, allt, allf, alll = [], [], [], []
    for name in (".text", "ctr"):
        s = img.sec[name]
        b = data[s["roff"]:s["roff"] + s["rsize"]]
        print(f"scanning {name} rva=0x{s['va']:X} size=0x{s['rsize']:X}")
        c, t, f, l = scan_section(b, s["va"])
        print(f"   valid rip-relative refs: {len(c)}")
        allc.append(c); allt.append(t); allf.append(f); alll.append(l)
    code = np.concatenate(allc); tgt = np.concatenate(allt)
    frm = np.concatenate(allf); iln = np.concatenate(alll)
    order = np.argsort(code, kind="stable")
    code, tgt, frm, iln = code[order], tgt[order], frm[order], iln[order]
    np.savez(OUTDIR / "p2_riprefs.npz", code=code, tgt=tgt, form=frm, ilen=iln)
    print("total valid refs:", len(code))

    # ---------------- targets of interest -------------------------------
    sec = img.sec
    ti = sec["typeinfo"]; fi_ = sec["fieldinf"]
    ranges = {
        "typeinfo": (ti["va"], ti["va"] + ti["vsize"]),
        "fieldinf": (fi_["va"], fi_["va"] + fi_["vsize"]),
        ".rdata": (sec[".rdata"]["va"], sec[".rdata"]["va"] + sec[".rdata"]["vsize"]),
        ".data": (sec[".data"]["va"], sec[".data"]["va"] + sec[".data"]["vsize"]),
        "_RDATA": (sec["_RDATA"]["va"], sec["_RDATA"]["va"] + sec["_RDATA"]["vsize"]),
        ".rodata": (sec[".rodata"]["va"], sec[".rodata"]["va"] + sec[".rodata"]["vsize"]),
    }
    crc_tables = []
    for off in (0x855bb50, 0x85d3bb0, 0x8a29430, 0x8d2bc30, 0x92be240):
        rva = img.off2rva(off)
        crc_tables.append((off, rva, rva + 1024))

    # all .rdata identifier strings
    rd = data[sec[".rdata"]["roff"]:sec[".rdata"]["roff"] + sec[".rdata"]["rsize"]]
    rdbase = sec[".rdata"]["va"]
    name_rva = []
    i = 0
    while True:
        j = rd.find(b"\x00", i)
        if j < 0:
            break
        s = rd[i:j]
        if 1 <= len(s) <= 64 and IDENT.match(s):
            name_rva.append((rdbase + i, s.decode("latin1")))
        i = j + 1
    name_rva.sort()
    nrv = np.array([x[0] for x in name_rva], dtype=np.int64)

    out = {"total_valid_rip_refs": int(len(code)), "sections_scanned": [".text", "ctr"],
           "hits_by_region": {}, "crc_table_refs": [], "fieldinf_refs": [],
           "typeinfo_refs": [], "name_xrefs": []}

    for rname, (lo, hi) in ranges.items():
        m = (tgt >= lo) & (tgt < hi)
        out["hits_by_region"][rname] = int(m.sum())
    print("hits by region:", out["hits_by_region"])

    # crc tables
    for off, lo, hi in crc_tables:
        m = (tgt >= lo) & (tgt < hi)
        if m.sum():
            idx = np.nonzero(m)[0]
            for i in idx[:200]:
                out["crc_table_refs"].append(
                    {"table_file_off": off, "table_rva": lo,
                     "code_rva": int(code[i]), "target_rva": int(tgt[i]),
                     "form": FORMS[int(frm[i])][0].hex(),
                     "bytes": data[img.rva2off(int(code[i])):img.rva2off(int(code[i])) + 16].hex()})
    print("crc table refs:", len(out["crc_table_refs"]))

    # fieldinf / typeinfo refs (valid instructions only)
    for key, secname in (("fieldinf_refs", "fieldinf"), ("typeinfo_refs", "typeinfo")):
        s = sec[secname]
        lo, hi = s["va"], s["va"] + s["vsize"]
        m = (tgt >= lo) & (tgt < hi)
        idx = np.nonzero(m)[0]
        print(f"{secname}: {len(idx)} valid code refs")
        for i in idx[:400]:
            off = img.rva2off(int(code[i]))
            out[key].append({"code_rva": int(code[i]),
                             "target_rva": int(tgt[i]),
                             "target_off_in_sec": int(tgt[i]) - s["va"],
                             "form": FORMS[int(frm[i])][0].hex(),
                             "bytes": data[off:off + 16].hex()})

    # name xrefs (all .rdata identifier strings)
    pos = np.clip(np.searchsorted(nrv, tgt), 0, len(nrv) - 1)
    hitmask = nrv[pos] == tgt
    idx = np.nonzero(hitmask)[0]
    print("valid code refs landing on .rdata identifier strings:", len(idx))
    seen = {}
    for i in idx:
        rva = int(nrv[pos[i]])
        nm = name_rva[pos[i]][1]
        if nm in seen:
            seen[nm].append(int(code[i]))
            continue
        seen[nm] = [int(code[i])]
    # keep names with >=1 ref, dump context for the "interesting" ones
    INTEREST = ("HealthComponent", "ClientPlayerManager", "TeamId", "Health",
                "LocalPlayer", "ClientConnection", "ClientSoldierEntity",
                "ClientPlayer", "ClientPlayerExtent", "TimeSync", "OnlineManager",
                "SquadId", "PersonaId", "LobbyTeam", "LobbySquad", "LobbyPlayerId",
                "SlotIndex", "ClientSoldierWeapon", "ClientToServerInfoCl",
                "ClPlayer", "ClientGame", "Entity", "Player", "ClientPlayerEntity")
    for nm, codes in sorted(seen.items()):
        if nm not in INTEREST:
            continue
        for cr in codes[:12]:
            off = img.rva2off(cr)
            after = data[off:off + 96]
            # offset-like immediates in the 64 bytes after the instruction
            imms = []
            for k in range(0, 64, 4):
                v = int.from_bytes(after[k:k + 4], "little")
                if 0x08 <= v <= 0x4000 and v % 4 == 0:
                    imms.append({"delta": k, "value": v})
            out["name_xrefs"].append({
                "name": nm, "name_rva": int(nrv[pos[0]]) if False else None,
                "code_rva": cr,
                "form": FORMS[int(frm[idx[0]])][0].hex() if False else None,
                "bytes": data[off:off + 16].hex(),
                "offset_like_immediates_after": imms})
    # fix name_rva / form per entry
    for e in out["name_xrefs"]:
        pass
    json.dump(out, open(OUTDIR / "p2_name_xrefs.json", "w"), indent=1)
    print("written p2_name_xrefs.json")


if __name__ == "__main__":
    main()
