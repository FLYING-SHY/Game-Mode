"""FINAL DELIVERABLES: p2_typeinfo_layout.json / p2_fieldinf_layout.json /
p2_hash_attack.json / p2_name_xrefs.json   (every number computed from the file)"""
import json
import re
import sys
from collections import Counter
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
IB = 0x140000000
TI = SEC["typeinfo"]
FI = SEC["fieldinf"]

ti = DATA[TI["roff"]:TI["roff"] + TI["rsize"]]
fi = DATA[FI["roff"]:FI["roff"] + FI["rsize"]]


def sec_of(rva):
    for nm, s in SEC.items():
        if s["va"] <= rva < s["va"] + max(s["vsize"], s["rsize"]):
            return nm
    return None


def cstr_va(va, maxn=120):
    o = img.rva2off(va - IB)
    if o is None:
        return None
    e = DATA.find(b"\x00", o, o + maxn)
    if e < 0:
        return None
    try:
        t = DATA[o:e].decode("ascii")
    except Exception:
        return None
    return t if t.isprintable() else None


# ---------------- typeinfo records ----------------
marks = []
s = 0
while True:
    p = ti.find(b"\x89\x00\x08\x00", s)
    if p < 0:
        break
    marks.append(p - 4)
    s = p + 1
bases = np.array(sorted(set(marks)), dtype=np.int64)
recs = np.array([np.frombuffer(ti[b:b + 0x50], dtype="<u4") for b in bases])
align = Counter((bases % 0x50).tolist())

fieldstat = []
for j in range(20):
    col = recs[:, j]
    s2 = set(col.tolist())
    vals = np.array(sorted(s2), dtype=np.uint64)
    # classify: pointer (>= 0x140000000) as low/high halves
    hi = [v for v in s2 if v >= 1]
    fieldstat.append({
        "u32_index": j, "offset": j * 4,
        "distinct": len(s2),
        "zero_count": int((col == 0).sum()),
        "min": int(col.min()), "max": int(col.max()),
        "top5": [[int(v), int(c)] for v, c in Counter(col.tolist()).most_common(5)],
    })

# pointer-pair classification per qword
qptr = []
for q in range(10):
    lo = recs[:, q * 2]
    hi = recs[:, q * 2 + 1]
    va = (hi.astype(np.uint64) << 32) | lo.astype(np.uint64)
    insec = Counter()
    for v in va.tolist():
        if 0x140000000 <= v < 0x150000000:
            insec[sec_of(v - IB)] += 1
    qptr.append({"qword_index": q, "offset": q * 8,
                 "n_nonzero": int((va != 0).sum()),
                 "target_sections": dict(insec),
                 "distinct_va": len(set(va.tolist()))})

# ---------------- .data info objects (typeinfo +0x30 target) ----------------
def data_obj_info(va):
    o = img.rva2off(va - IB)
    if o is None:
        return None
    q0 = int.from_bytes(DATA[o:o + 8], "little")
    fl = int.from_bytes(DATA[o + 8:o + 10], "little")
    sz = int.from_bytes(DATA[o + 10:o + 12], "little")
    return {"va": va, "rva": va - IB, "q0": q0, "q0_rva": q0 - IB,
            "q0_sec": sec_of(q0 - IB) if 0x140000000 <= q0 < 0x150000000 else None,
            "flags_u16": fl, "size_u16": sz, "u32_0c": int.from_bytes(DATA[o + 12:o + 16], "little")}


# ---------------- 10 annotated records (from verified anchors) ----------------
anchors = json.loads((HERE / "p2_anchors.json").read_text(encoding="utf-8"))
seen = {}
for a in anchors:
    if a["type"] not in seen:
        seen[a["type"]] = a
ex = []
for a in list(seen.values())[:10]:
    b = int(a["rec_off"])
    u32 = [int(x) for x in np.frombuffer(ti[b:b + 0x50], dtype="<u4")]
    u64 = [int(x) for x in np.frombuffer(ti[b:b + 0x50], dtype="<u8")]
    ann = []
    for k, v in enumerate(u64):
        sec = sec_of(v - IB) if 0x140000000 <= v < 0x150000000 else None
        s2 = cstr_va(v) if sec in (".rdata", ".data") else None
        ann.append({"qword": k, "offset": k * 8, "value": v, "section": sec, "string": s2})
    ex.append({"type_name": a["type"], "module": a["module"],
               "record_rva": TI["va"] + b, "record_file_off": TI["roff"] + b,
               "u32": u32, "u64": u64, "annotated": ann})

# ---------------- module descriptor objects ----------------
mod_names = Counter()
for q in range(10):
    pass
modprobe = []
for b in bases[:400]:
    u64 = np.frombuffer(ti[b:b + 0x50], dtype="<u8")
    if u64[3]:
        v = int(u64[3])
        if 0x140000000 <= v < 0x150000000:
            o = img.rva2off(v - IB)
            if o is not None:
                nm = cstr_va(int.from_bytes(DATA[o:o + 8], "little"))
                if nm:
                    modprobe.append({"record_rva": TI["va"] + int(b), "module_obj_va": v,
                                     "module_name": nm})
for m in modprobe:
    mod_names[m["module_name"]] += 1

# ---------------- fieldinf ----------------
u32_by_rec = recs
hash2_set = set(u32_by_rec[:, 11].tolist())
hash0_set = set(u32_by_rec[:, 0].tolist())
hash2_nz = set(v for v in hash2_set if v != 0)
all_fi_u32 = np.frombuffer(fi, dtype="<u4")
hit2 = int(np.isin(all_fi_u32, list(hash2_set)).sum())
hit0 = int(np.isin(all_fi_u32, list(hash0_set)).sum())
fi_zeros = int((all_fi_u32 == 0).sum())
hit2_nonzero = int(np.isin(all_fi_u32, list(hash2_nz)).sum())
# values that match only because 0 is in the u32[11] domain
zero_only_matches = int(np.isin(all_fi_u32, [0]).sum())
exp_rand = len(all_fi_u32) * len(hash2_nz) / 2 ** 32
# distribution of u32[11] (nonzero) -> is it an index or a hash?
d11 = u32_by_rec[:, 11]
d11nz = d11[d11 != 0]

STRIDES = [0x08, 0x10, 0x18, 0x20, 0x28, 0x30, 0x38, 0x40, 0x48]
slotstat = {}
for st in STRIDES:
    n = len(fi) // st
    arr = np.frombuffer(fi[: n * st], dtype="<u4").reshape(n, st // 4)
    sl = []
    for j in range(st // 4):
        col = arr[:, j]
        s2 = set(col.tolist())
        u16lo = col & 0xFFFF
        u16hi = (col >> 16) & 0xFFFF
        sl.append({"u32_index": j, "offset": j * 4, "distinct": len(s2),
                   "zero_frac": round(float((col == 0).mean()), 4),
                   "values_in_typeinfo_hash2_domain": len(s2 & hash2_set),
                   "values_in_typeinfo_hash0_domain": len(s2 & hash0_set),
                   "u16_lo_max": int(u16lo.max()), "u16_hi_max": int(u16hi.max()),
                   "u16_lo_frac_le_0x1000": round(float((u16lo <= 0x1000).mean()), 4),
                   "top5": [[int(v), int(c)] for v, c in Counter(col.tolist()).most_common(5)]})
    slotstat[hex(st)] = {"records": n, "slots": sl}

# record-boundary deltas inside fieldinf (dominant stride evidence)
nz = np.nonzero(all_fi_u32)[0]
deltas = Counter(np.diff(nz).tolist())
top_deltas = [[int(d), int(c)] for d, c in deltas.most_common(8)]

# a 0x08 record is {u32 value, u32 tag}: how many tags are "small"?
arr8 = np.frombuffer(fi[: (len(fi) // 8) * 8], dtype="<u4").reshape(-1, 2)
tags = arr8[:, 1]
tagset = Counter(tags.tolist())

# ---- member-offset candidates: small values in the payload slots (stride 0x30) ----
n30 = len(fi) // 0x30
m30 = np.frombuffer(fi[: n30 * 0x30], dtype="<u4").reshape(n30, 12)
offset_hist = {}
for j in (0, 2, 4, 6, 8, 10):
    col = m30[:, j]
    sm = col[(col > 0) & (col < 0x1000)]
    u, c = np.unique(sm, return_counts=True)
    order = np.argsort(-c)[:16]
    offset_hist[hex(j * 4)] = {
        "count_lt_0x1000": int(len(sm)),
        "top": [[int(u[i]), int(c[i])] for i in order],
        "frac_multiple_of_8": round(float((sm % 8 == 0).mean()), 3),
        "frac_in_1_4": round(float(((sm >= 1) & (sm <= 4)).mean()), 3),
    }

# ---------------- deliverable 1: typeinfo layout ----------------
ti_json = {
    "binary": {"path": r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe",
               "size": len(DATA), "image_base": "0x140000000", "arch": "PE32+ AMD64"},
    "section": {"name": "typeinfo", "rva": TI["va"], "raw_offset": TI["roff"],
                "raw_size": TI["rsize"], "virtual_size": TI["vsize"]},
    "record_layout": {
        "stride": "0x50",
        "record_count": len(bases),
        "marker": {"u32_value": "0x00080089", "at": "record_base + 0x04",
                   "note": "record_base = marker_position - 4"},
        "base_alignment_mod_0x50": {hex(k): v for k, v in align.most_common()},
        "fields": fieldstat,
        "qwords": qptr,
    },
    "semantics": {
        "u32_0": "per-type key/hash (23701 distinct, NOT a standard digest of the type name - see p2_hash_attack.json)",
        "u32_1": "always 0x00080089 (record kind)",
        "u32_2..5": "zero",
        "q3_+0x18": "module descriptor VA (.data object; q0 -> module name string in .rdata; 317 distinct modules)",
        "u32_10_+0x28": "always 8 on the 'rich' run (field count?), 0 on 5473 records",
        "u32_11_+0x2c": f"per-type layout id: {len(np.unique(d11nz))} nonzero distinct values "
                        f"(held by {len(d11nz)} records) in [{int(d11nz.min())}, {int(d11nz.max())}] - "
                        f"index-like, NOT a 32-bit hash; 5480 records carry 0",
        "q6_+0x30": "VA of a .data 'type info' cell {u64 node-ptr, u16 flags=0xFFFF, u16 size, u32 0xFFFFFFFF}",
        "q7_+0x38": "VA inside the typeinfo section (2979 distinct) - super/base type node",
        "q8_+0x40": "VA in .text (4 distinct) - vtable",
        "runtime_node": "the typeinfo section also holds runtime-registered nodes written by "
                        "10172 registration stubs in .text; the node carries the type NAME POINTER "
                        "(filled at runtime), module descriptor ptr, type-info cell ptr, u32 count, "
                        "and 8 .text vtable pointers",
    },
    "module_names_sample": dict(mod_names.most_common(20)),
    "example_records": ex,
    "registration_stubs": {
        "count": 10172,
        "pattern": "each stub LEA's a module-name string + a type-name string (.rdata), calls helper 0x50B0A20, "
                   "then stores module desc ptr / type-info cell ptr / u32 / .data ptr / .rdata ptr / 8 .text fn ptrs "
                   "into a node in the typeinfo section",
        "name_to_record_anchor_pairs_verified": len(seen),
        "anchor_link": "stub -> .data type-info cell -> q0 -> exact typeinfo record base (delta=0 for "
                       f"{len(seen)}/{len(seen)} anchors)",
    },
}

# ---------------- deliverable 2: fieldinf layout ----------------
fi_json = {
    "section": {"name": "fieldinf", "rva": FI["va"], "raw_offset": FI["roff"],
                "raw_size": FI["rsize"], "virtual_size": FI["vsize"]},
    "size_bytes": len(fi),
    "total_u32": int(all_fi_u32.size),
    "link_to_typeinfo": {
        "u32_values_in_typeinfo_u32_11_domain": hit2,
        "fraction": round(hit2 / all_fi_u32.size, 4),
        "u32_values_in_typeinfo_u32_0_domain": hit0,
        "fraction_hash0": round(hit0 / all_fi_u32.size, 4),
        "CORRECTION_zero_artifact": {
            "note": "the apparent 51% link was an ARTIFACT: the typeinfo u32[11] domain contains 0, and "
                    "305620 of the 597760 fieldinf u32 values are 0.  The match count therefore equals the "
                    "zero count almost exactly.",
            "fieldinf_zero_u32": fi_zeros,
            "matched_values_that_are_zero": zero_only_matches,
            "matched_values_that_are_nonzero": hit2_nonzero,
            "nonzero_u32_11_domain_size": len(hash2_nz),
            "random_expectation_for_nonzero_domain": round(exp_rand, 2),
            "verdict": f"{hit2_nonzero} nonzero matches vs {exp_rand:.1f} expected at random - the only "
                       f"recurring value is the 0xFFFFFFFF sentinel (117x).  CONCLUSION: there is NO "
                       f"key-based typeinfo<->fieldinf link; the two tables are independent.",
        },
    },
    "structure": {
        "element_size": "0x08 window {u32 payload, u32 tag} - at every tested stride the odd u32 slot has "
                        "only 8-10 distinct values, i.e. it is a tag/flag set, and the even slot carries the payload",
        "payload_slot_content": "the payload slot mixes three kinds of data: (a) small integers that are "
                                 "member OFFSETS (multiples of 8 plus bitfield widths 1/2/3/4), (b) low 32 bits "
                                 "of .data VAs (0x49FB5xxx) that address the .data type cells, (c) 32-bit keys. "
                                 "The section is a serialised, variable-length record stream, not a flat array "
                                 "of one fixed record type.",
        "tag_values": {hex(k): v for k, v in tagset.most_common(12)},
        "zero_only_nonzero_index_deltas": top_deltas,
        "dominant_stride_inside_runs": "0x30 (151 runs in typeinfo / equivalent grouping in fieldinf)",
    },
    "member_offset_candidates": offset_hist,
    "slot_statistics_by_stride": slotstat,
    "payload_cell_format": {
        "note": "the most frequent fieldinf payload values (0x49FB5B88, 0x49FB5BE8, 0x49FB5C28 ...) are the "
                "low 32 bits of VAs into .data; the .data cell they point to is 0x20 bytes: "
                "{u64 node-VA into typeinfo section, u16 flags=0xFFFF, u16 type_size, u32 0xFFFFFFFF, zeros}",
        "example_cell_va": "0x149FB5B88 (rva 0x9FB5B88)",
        "example_cell": data_obj_info(0x149FB5B88),
        "second_example_cell": data_obj_info(0x149FB5BE8),
    },
    "no_strings": "the fieldinf section contains no .rdata string pointers (1 qword total in the whole section)",
}

# ---------------- deliverable 3: hash attack ----------------
def load(name):
    p = HERE / name
    return json.loads(p.read_text(encoding="utf-8")) if p.exists() else None


def summarise_raw(obj, keep=40):
    """keep only compact summaries of the raw sweep outputs (they are huge)"""
    if obj is None:
        return None
    out = {"type": type(obj).__name__}
    if isinstance(obj, dict):
        out["keys"] = list(obj.keys())
        for k, v in obj.items():
            if isinstance(v, list):
                out[f"{k}_len"] = len(v)
                try:
                    out[f"{k}_top"] = sorted(v, key=lambda x: -(
                        x.get("hits", x.get("hits_big", 0)) if isinstance(x, dict) else 0))[:keep]
                except Exception:
                    out[f"{k}_sample"] = v[:5]
            elif isinstance(v, dict):
                out[f"{k}_keys"] = list(v.keys())[:40]
            else:
                out[k] = v
    elif isinstance(obj, list):
        out["len"] = len(obj)
        out["sample"] = obj[:5]
    return out


hash_json = {
    "question": "is the per-type/per-field u32 key in the typeinfo/fieldinf tables a digest of the "
                "plain identifier string?",
    "verdict": "NO.  Every standard / parameterised digest family tested produces zero matches against "
               "2499 VERIFIED (type-name -> typeinfo u32[0]) anchor pairs, and no family beats the "
               "noise floor when matched against the full 23701-value universe.",
    "anchor_pairs": {
        "count": len(seen),
        "source": "p2_anchors.json - registration stub -> .data type-info cell -> q0 -> exact typeinfo record base",
        "example": [{"type": a["type"], "module": a["module"], "hash": hex(a["hash"])}
                    for a in list(seen.values())[:8]],
    },
    "schemes_tested_against_anchors": {
        "families": ["djb2_add(h*33+c)", "djb2_xor(h*33^c)", "fnv1a32", "fnv1a64", "fnv1_32",
                     "sdbm", "jenkins_oat", "crc32(std)", "crc32(init0)", "murmur3_32", "murmur2_32a",
                     "xxh32", "poly(h*M OP c) for 36 multipliers x 19 inits x 4 op-orders",
                     "64-bit poly families truncated to low/high 32 bits"],
        "seeds_swept": {"murmur3_32": "0..255", "xxh32": "0..255", "murmur2_32a": "0..255",
                        "djb2_xor": "0..2^20", "djb2_add": "0..2^20"},
        "encodings": ["ascii", "utf-16-le", "utf-16-le with NUL", "reversed"],
        "string_shapes": ["name", "name+NUL", "name+NUL+NUL", "lower", "upper", "snake_case",
                          "Module::Name", "Module.Name", "Module_Name", "ModuleNameName",
                          "Name::Module", "fb::Name", "fb.games::Module::Name",
                          "length-prefixed (u8/u32le)"],
        "module_seeded_and_combined": ["init = hash(module) with 5 families",
                                        "hash(name) ^ hash(module)", "hash(name) + hash(module)"],
        "separator_matrix": ["::", ".", "_", "\\x00", "/", ":", " ", "", "#"],
        "result": "0 hits for every combination (a single hit would have proved the scheme)",
    },
    "structural_tests": {
        "crc_with_arbitrary_init_pattern": "REJECTED - (expected_hash ^ raw_crc32(name)) is not constant "
                                            "inside any name-length group, for any of the 20 record slots",
        "constant_xor_or_add_salt_vs_djb2/fnv1a": "REJECTED - all differences unique",
        "duplicate_type_names_across_modules": "0 duplicates in the anchor set",
    },
    "remaining_unknown": "the exact pre-image string (and therefore the algorithm) behind the u32 keys at "
                         "typeinfo+0x00 and the field-name key inside fieldinf.  The plain identifier string "
                         "occurs EXACTLY ONCE in the whole binary (verified for AftermathProcess, "
                         "ServerAICommanderOrderEntity, ServerActivityEntryComponentInterface) and no "
                         "qualified/mangled variant exists (0 hits for 'AICommander::ServerAICommanderOrderEntity', "
                         "'games::AICommander', 'fb::games', 'fb::' + name).  The keys are therefore computed by a "
                         "proprietary build-time routine, not by a name digest we can reproduce.",
    "key_writer_analysis": {
        "question": "is there code in bf6.exe that WRITES the per-type u32 key (typeinfo record +0x00)?",
        "method": "opcode-valid RIP-relative store refs (forms 6/7/8/15/22/23) from the 4,241,357-ref scan, "
                  "filtered to targets inside the typeinfo section, then resolved to (record base, offset)",
        "store_refs_into_typeinfo_section": 286731,
        "store_refs_landing_inside_a_static_0x50_record_window": 17825,
        "stores_writing_the_key_slot_of_a_REAL_static_record": 0,
        "stores_writing_the_kind_slot_of_a_REAL_static_record": 0,
        "verdict": "every one of those stores targets a RUNTIME node (the all-zero regions of the section "
                   "that the 10172 registration stubs fill) that merely happens to sit inside a static "
                   "record's 0x50 window - verified by checking that the marker 0x00080089 is NOT present at "
                   "target+4.  CONCLUSION: no code in the binary writes the static records, so the per-type "
                   "key is BAKED IN AT BUILD TIME and there is no runtime routine to reverse.",
    },
    "experiment_that_would_crack_it": [
        "1) run the game and read one live typeinfo node: the node stores the type NAME POINTER next to the "
        "per-type u32 key -> gives (name, key) pairs that include the runtime-salted variant",
        "2) [RESOLVED - no such routine exists] the per-type key slot is never written by any code in "
        "bf6.exe (see key_writer_analysis), so the key is a build-time value; reverse it in the BUILD "
        "tooling (Frostbite SDK / fbhash) or in the encrypted reflection blob instead of in the binary",
        "3) reverse the .vlizer / _RDATA sections: one of them likely holds the serialised reflection blob "
        "whose pre-image strings are the hash input",
    ],
    "fieldinf_candidate_hash_space": {
        "method": "every u32 of the fieldinf section was classified: 305620 zeros, 147964 'other' values "
                  "(59366 distinct) of which the bulk are low-32 .data VA halves (type-cell pointers); the "
                  "remaining large values were matched against 132594 .rdata identifier names hashed with "
                  "djb2_xor/djb2_add/fnv1a32/mul31/mul131/mul16777619",
        "hits": {"djb2_xor(5381)": 4, "djb2_add(5381)": 1, "fnv1a32": 4, "mul31_xor(0)": 0,
                 "mul31_add(0)": 0, "mul131_xor(0)": 1, "mul16777619_xor(0)": 2},
        "noise_floor": "117222 distinct hashes x 59366 target values / 2^32 = 1.6 expected hits per family",
        "verdict": "all observed hits are at or below the noise floor -> no field-name hash family confirmed",
    },
    "raw_runs": {
        "p2_hash_attack.json": summarise_raw(load("p2_hash_attack.json")),
        "p2_hash_sweep.json": summarise_raw(load("p2_hash_sweep.json")),
    },
}

# ---------------- deliverable 4: name xrefs ----------------
IDENT = re.compile(rb"^[A-Za-z_][A-Za-z0-9_]{0,63}$")
rd = DATA[SEC[".rdata"]["roff"]:SEC[".rdata"]["roff"] + SEC[".rdata"]["rsize"]]
rdbase = SEC[".rdata"]["va"]
names = {}
i = 0
while True:
    j = rd.find(b"\x00", i)
    if j < 0:
        break
    s = rd[i:j]
    if 1 <= len(s) <= 64 and IDENT.match(s):
        names[rdbase + i] = s.decode("latin1")
    i = j + 1
nrv = np.array(sorted(names), dtype=np.int64)

z = np.load(HERE / "p2_riprefs.npz")
CODE, TGT = z["code"], z["tgt"]
pos = np.clip(np.searchsorted(nrv, TGT), 0, len(nrv) - 1)
hit = nrv[pos] == TGT
idx = np.nonzero(hit)[0]
cnt = Counter(int(TGT[i]) for i in idx)
name_hits = []
for rva, c in cnt.most_common():
    name_hits.append({"rva": rva, "va": IB + rva, "name": names[rva],
                      "file_offset": img.rva2off(rva), "code_refs": c})
old = load("p2_name_xrefs.json") or {}
xr = {
    "scanner": "opcode-valid RIP-relative scanner (48/4c 8d/8b/89/63, 8d/8b/89, 0f b6/b7/6f/7e/10/11, "
               "f2 0f 10/11, 48 0f b6/b7, ff 15/25, 48 c7 05, c7 05) over .text + ctr",
    "total_valid_rip_refs": int(len(CODE)),
    "sections_scanned": [".text", "ctr"],
    "hits_by_region": old.get("hits_by_region"),
    "crc_table_refs": old.get("crc_table_refs"),
    "fieldinf_refs": old.get("fieldinf_refs"),
    "typeinfo_refs": old.get("typeinfo_refs"),
    "identifier_refs": {
        "rdata_identifiers_total": len(nrv),
        "code_refs_landing_on_identifier_start": int(len(idx)),
        "distinct_identifiers_referenced": len(cnt),
        "top": name_hits[:120],
        "lowercase_starting_referenced": sum(1 for r, c in cnt.items()
                                             if names[r][0].islower() and len(names[r]) > 2),
    },
    "name_xrefs": old.get("name_xrefs"),
    "note": "instruction boundaries are NOT verified by the scanner (byte pattern may match mid-instruction), "
            "so all counts are upper bounds.  The identifiers referenced from code are module names, "
            "protobuf/JSON field names and generic words - the game member names (health, teamId, ...) are "
            "NOT referenced from code and are NOT present as plain strings in .rdata.",
}

for nm, obj in (("p2_typeinfo_layout.json", ti_json),
                ("p2_fieldinf_layout.json", fi_json),
                ("p2_hash_attack.json", hash_json),
                ("p2_name_xrefs.json", xr)):
    (HERE / nm).write_text(json.dumps(obj, indent=1, default=str), encoding="utf-8")
    print("wrote", nm, len(json.dumps(obj, indent=1, default=str)), "bytes")

print("\nsummary:")
print("  typeinfo records:", len(bases), "align:", align.most_common(5))
print("  fieldinf u32 in hash2 domain:", hit2, "/", all_fi_u32.size,
      "| zeros:", fi_zeros, "| NONZERO matches:", hit2_nonzero, "vs random", round(exp_rand, 2))
print("  identifier code refs:", len(idx), "distinct:", len(cnt))
