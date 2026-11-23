"""p2_final2: post-process the four deliverable JSONs with the newest findings
(member-name absence, typeinfo-section content mix, runtime overlay, structural
hash proofs, 64-bit hash families, keystore candidates) and write a consolidated
report p2_report.md.
"""
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
data = img.data
sec = img.sec

MEMBERS = ["HealthComponent", "ClientPlayerManager", "ClPlayer", "ClientPlayerExtent",
           "ClientSoldierEntity", "ClientSoldierWeapon", "TeamId", "SquadId", "PersonaId",
           "Health", "LocalPlayer", "ClientPlayer", "ClientConnection", "GameContext",
           "TimeSync", "OnlineManager", "SlotIndex", "LobbyTeam", "LobbySquad",
           "LobbyPlayerId", "ReturnValue"]

# ---------- (a) occurrences of each member name in the whole file ----------
occ = {}
for m in MEMBERS:
    pat = m.encode()
    cnt = 0
    pos = 0
    first = []
    while True:
        p = data.find(pat, pos)
        if p < 0:
            break
        cnt += 1
        if len(first) < 6:
            first.append({"file_off": hex(p), "rva": hex(img.off2rva(p))})
        pos = p + 1
    occ[m] = {"occurrences": cnt, "first_hits": first}
print("member occurrences done")

# ---------- (b) code rip-refs landing exactly on a member name ----------
z = np.load(HERE / "p2_riprefs.npz")
CODE, TGT = z["code"], z["tgt"]
name_va = {}
for m in MEMBERS:
    pat = m.encode()
    pos = 0
    while True:
        p = data.find(pat, pos)
        if p < 0:
            break
        if p == 0 or data[p - 1] == 0:
            name_va.setdefault(m, []).append(img.off2rva(p))
        pos = p + 1

pd = sec[".pdata"]
b = data[pd["roff"]:pd["roff"] + pd["rsize"]]
n = len(b) // 12
arr = np.frombuffer(b[:n * 12], dtype="<u4").reshape(n, 3)
beg = arr[:, 0].astype(np.int64)
end = arr[:, 1].astype(np.int64)
keep = (beg > 0) & (end > beg)
beg, end = beg[keep], end[keep]


def func_of(rva):
    i = int(np.searchsorted(beg, rva, side="right")) - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


tgt_sorted = np.sort(TGT)
idx = np.searchsorted(tgt_sorted, np.array(sorted(name_va.values())[0]) if name_va else np.array([0]))
refs = []
for m, vas in name_va.items():
    for va in vas:
        lo = int(np.searchsorted(tgt_sorted, va, side="left"))
        hi = int(np.searchsorted(tgt_sorted, va, side="right"))
        for k in range(lo, hi):
            cr = int(CODE[k])
            f = func_of(cr)
            refs.append({"member": m, "name_rva": hex(va), "code_rva": hex(cr),
                         "func": hex(f[0]) if f else None})
print(f"code refs landing exactly on a member-name start: {len(refs)}")
by_member = Counter(r["member"] for r in refs)
by_func = Counter(r["func"] for r in refs if r["func"])

# are those functions type-registration stubs? (store into typeinfo AND LEA .rdata)
ks = json.loads((HERE / "p2_key_stores.json").read_text(encoding="utf-8"))
stub_funcs = {c["func_rva"] for c in ks["candidates"]}
reg = json.loads((HERE / "p2_reg.json").read_text(encoding="utf-8"))
reg_funcs = {s["func"] for s in reg["type_stubs"]}
member_funcs = set(by_func)
in_reg = member_funcs & reg_funcs
print(f"  distinct functions: {len(member_funcs)}  of which known registration stubs: {len(in_reg)}")

# ---------- read the existing deliverables ----------
def load(f):
    return json.loads((HERE / f).read_text(encoding="utf-8"))


ti = load("p2_typeinfo_layout.json")
fi = load("p2_fieldinf_layout.json")
ha = load("p2_hash_attack.json")
nx = load("p2_name_xrefs.json")
c6 = load("p2_crack6.json")
vd = load("p2_vlizer_decode.json")
mc = load("p2_memcheck.json")

# ================= typeinfo layout additions =================
ti["section_content"] = {
    "finding": "the 'typeinfo' section is a MIXED blob, not a pure reflection table",
    "evidence": {
        "static_records_kind_0x00080089": 23701,
        "static_records_other_kinds": "0x4810b / 0x280063 / 0x300063 / 0x100045 / 0x380063 / "
                                     "0x80045 / 0x400063 / 0x88045 / 0x48045 / 0x48063 "
                                     "(referenced by .data type cells)",
        "plain_globals": "1682 functions write 16-byte UUIDs straight into the section "
                         "(movaps/movups of a 4x u32 constant built on the stack); 0 of those "
                         "targets coincide with a static record base",
        "runtime_nodes": "10172 registration stubs fill all-zero regions of the section "
                         "at runtime",
    },
    "consequence": "a reader must locate records by their 0x00080089 marker, not by "
                   "assuming a flat array over the section",
}
ti["runtime_overlay"] = {
    "fact": "runtime node base = static record base + 8 (verified on 2499/2499 anchors)",
    "runtime_written_bytes": "static+0x08..0x17 = {u64 name pointer, u16 flags, u16 size, u32 ?} "
                              "copied from the return value of helper 0x50B0A20",
    "on_disk_value": "ZERO - the type NAME pointer only exists in the live process",
    "mec_compatibility": "info{name ptr, u16 flags, u16 size, module ptr, align, u16 fieldCount} "
                         "matches the Mirror's Edge Catalyst layout quoted in refs/docs__research",
    "implication_for_an_esp": "resolve type/field names by walking the LIVE typeinfo + fieldinf "
                              "structures; the on-disk file only carries the baked key ids",
}
ti["semantics"]["u32_0"] = (
    "per-type key id: 23701 distinct, NOT a digest of the type name. Proven negative: "
    "70+ digest schemes x 11 name shapes = 0 hits on 2499 verified anchors; the structural "
    "test rejects EVERY polynomial family (h=(h*M) OP c) for any multiplier/init/order: "
    "ADD-family gcd over same-last-byte pairs = 1, XOR-family low-8 test rejected "
    "(29021/29143 pairs violate), init solve = 1/2499; xxh64/xxh32/siphash24/murmur2_64/"
    "murmur3_x64_128 (lo+hi32) all 0 hits. The value is BAKED IN BY THE BUILD TOOL.")

# ================= fieldinf additions =================
fi["member_names"] = {
    "string_pointers_in_section": 1,
    "rdata_va_candidates_in_section": 495,
    "rdata_va_candidates_resolving_to_a_string": 48,
    "resolved_strings_are": "false positives (' types' x11, 'AndSizeCombo' x4, "
                             "'GameReportFilter' x2, ...)",
    "verdict": "the fieldinf section carries NO member names; only 32-bit keys",
}
fi["type_linkage"] = {
    "cell_references_in_section": 64886,
    "distinct_data_type_cells": 7432,
    "cells_pointing_at_a_kind_0x00080089_static_record": 2661,
    "intersection_with_the_2499_anchor_records": 0,
    "why": "the anchor set only covers the records reached by the 5000 decoded registration "
           "stubs; the other ~24000 records are not name-linked on disk",
    "chain": "fieldinf payload -> .data 0x20 cell (VA low 32 bits) -> cell.q0 -> static "
             "typeinfo record -> (name via code, not via the key)",
    "rva_conversion": "rva = (va_low32 + 0xC0000000) & 0xFFFFFFFF  (image base 0x140000000)",
}
fi["offset_availability"] = {
    "member_offsets_present": True,
    "where": "payload slot of the 8-byte {u32 payload, u32 tag} elements",
    "example_histogram": {"0": 26327, "32": 2069, "40": 1335, "1": 1244, "48": 1119,
                          "24": 1064, "2": 1058, "56": 769, "64": 753, "8": 676,
                          "80": 533, "72": 530, "16": 523, "4": 513, "96": 447},
    "note": "offsets are recoverable statically per type; the NAMES are not",
}

# ================= hash attack additions =================
ha["schemes_tested_against_anchors"]["64bit_families_p2_crack6"] = c6["families"]
ha["schemes_tested_against_anchors"]["64bit_families_verdict"] = c6["verdict"]
ha["structural_tests"]["add_family_any_multiplier"] = (
    "REJECTED - for names sharing the last byte every key difference must be divisible by the "
    "multiplier; the gcd over 29143 such pairs is 1, so no multiplier > 1 can fit. "
    "Same test on the reversed byte order (20808 pairs): gcd 1.")
ha["structural_tests"]["xor_family_any_multiplier"] = (
    "REJECTED - for names sharing the last byte the low 8 bits of the key must match; "
    "29021/29143 pairs violate it (reversed order: 20722/20808).")
ha["structural_tests"]["init_solve_for_candidate_multipliers"] = (
    "for M in {31,33,37,39,41,131,65599,16777619,0x01000193,5381,5,257,65537} and both byte "
    "orders, solving the init from one anchor reproduces exactly 1/2499 anchors")
ha["structural_tests"]["scope"] = (
    "these tests are independent of the multiplier and of the initial value, so they exclude "
    "the whole class of 32-bit polynomial hashes (and their 64-bit truncations) for the "
    "type-name -> key mapping")
ha["key_writer_analysis"]["non_registration_store_candidates"] = {
    "store_refs_into_typeinfo_section_rescan": ks["store_refs_into_typeinfo"],
    "enclosing_functions": ks["enclosing_functions"],
    "registration_stub_functions_excluded": ks["registration_stub_functions"],
    "candidate_non_registration_functions": ks["candidate_functions"],
    "details": "p2_key_stores.json (function RVA + decoded store instruction + target bytes)",
}
ha["remaining_unknown"] = (
    "The per-type 32-bit key cannot be reproduced from the type name: it is a build-time id "
    "(no code in bf6.exe writes the static records - 0/3590 key-slot stores and 0/3174 "
    "kind-slot stores hit a real static record).  What is STILL unknown: the algorithm the "
    "Frostbite build tool used, and whether the field keys in the fieldinf section are the "
    "same scheme.  What is NOT needed for name resolution: the type names live in .rdata and "
    "are referenced by 10172 registration stubs, and the offsets live in the fieldinf payload "
    "slots - the names for FIELDS however do not exist anywhere in bf6.exe, they are only "
    "materialised in the live process.")

# ================= name xrefs additions =================
nx["member_name_absence"] = {
    "question": "do the wanted member names (Health, TeamId, SquadId, ...) exist in bf6.exe "
                "together with their offsets?",
    "occurrences_in_file": occ,
    "code_refs_landing_on_a_member_name_start": len(refs),
    "refs_per_member": dict(by_member),
    "distinct_functions": len(member_funcs),
    "classification": {
        "refs_inside_a_type_registration_stub": mc["refs_in_type_registration_stubs"],
        "refs_in_functions_that_never_touch_the_typeinfo_section":
            len(refs) - mc["refs_in_type_registration_stubs"],
        "what_those_other_functions_are":
            "JSON/protobuf serializers (0x10c7420 LEA '\"', 'type', '\":', '}'), string getters "
            "(`lea rax,[rip+str]; ret`) and type-name users such as 0x16b0110 / 0x118e600 which "
            "build a 'LocalPlayer' / 'Health' string",
        "functions_that_pair_a_name_with_an_offset": 0,
    },
    "verdict": "the names that exist are TYPE names (TeamId, Health, ClientPlayer, "
               "LocalPlayer, ClientPlayerManager, HealthComponent, ...); every code "
               "reference to them is a type-registration stub or a type-name user, and no "
               "field/member registration code exists.  There is NO name+offset table "
               "anywhere in the file.",
    "name_cell_table": {
        "location": ".data RVA 0x9ACEF40",
        "cell": "{u64 name ptr, 24 zero bytes} (some cells carry u32 1 at +0x10)",
        "contents": "UI property names (SquadId, TeamId, LobbySquad, LobbyTeam, PersonaId, "
                    "ReturnValue, SquadLimitsOut, SlotIndex, ...)",
        "offsets_present": False,
    },
    "ui_property_region": {
        "rva": "0x84C000-0x84F000 in .rdata",
        "note": "the first occurrence of Health/TeamId/SquadId/PersonaId/SlotIndex/LocalPlayer "
                "sits in the BFUI property area - UI bindings, not the gameplay reflection",
    },
    "name_tables_scanned": 13672,
    "name_plus_offset_tables_found": 0,
    "per_function_detail": "p2_memcheck.json",
}
nx["type_name_pool"] = {
    "where": ".rdata, first pool at RVA 0x8DDA7B2 (file 0x8DDA7B2 / VA 0x148DDC1B2)",
    "content": "type names, template instantiations and nested names "
               "(ClientPlayerManager, ClientPlayerManager.IdleState, "
               "NativePtr<ClientPlayerExtent>, ClientCharacterEntityGhost, ...)",
    "member_names_present": False,
    "name_xrefs": "every code reference is a registration stub that also writes a typeinfo node",
}
nx["module_table_RIFXRIFF"] = {
    "magic": "ASCII 'RIFXRIFF' (u64 0x4646495258464952) at .data RVA 0x9AF49A0",
    "table_start": ".data RVA 0x9AF48C8",
    "record": "{u64 module name ptr, u64[4]=0, u64 count, u64 header{u32 size,u32 count}, "
              "per type: {u64 type name, u64 super type name, u64 .text fn x3}}",
    "modules": ["ReinforcementInventory", "Reflection", "Data", "RenderBase", "RenderCommon",
                "Render", "Renderbones", "RenderLayersRender"],
    "contains_member_names": False,
}
nx["related_outputs"] = {
    "p2_key_stores.json": "non-registration-stub stores into the typeinfo section "
                           "(function RVA + instruction + target bytes)",
    "p2_vlizer_decode.json": "block structure / entropy / identification of .vm_sec + .vlizer",
    "p2_vlizer_strings.json": "strings extracted from .vlizer (no field names)",
    "p2_crack6.json": "64-bit hash families against the anchors",
    "p2_anchors.json": "2499 verified (type name -> static record) anchors",
    "p2_riprefs.npz": "4,241,357 opcode-valid RIP-relative references (code, target, form)",
}

for f, d in (("p2_typeinfo_layout.json", ti), ("p2_fieldinf_layout.json", fi),
             ("p2_hash_attack.json", ha), ("p2_name_xrefs.json", nx)):
    (HERE / f).write_text(json.dumps(d, indent=1), encoding="utf-8")
    print(f"updated {f} ({(HERE / f).stat().st_size} bytes)")

# ================= consolidated report =================
rep = """# BF6 reflection metadata — final report (subagent frostbite-hash)

binary: G:\\SteamLibrary\\steamapps\\common\\Battlefield 6\\bf6.exe
size   : 199,230,312 bytes, PE32+ AMD64, image base 0x140000000
sha256 : 1f385ad9af30b8ba7f0c87ceee92182710bb58f98708a2de7f8d119042baeff4

## 1. What the file actually contains

| region | RVA | content |
|---|---|---|
| typeinfo | 0xAECC000 | MIXED blob: 23,701 static records of kind 0x00080089 (stride 0x50), runtime node regions, 1,682 UUID globals |
| fieldinf | 0xB48B000 | serialised 8-byte element stream {{u32 payload, u32 tag}} - offsets + .data type-cell pointers + 32-bit keys, **no strings** |
| .rdata  | 0x83DF000 | type-name pool (type names only), enum thunk targets in `ctr` |
| .data   | 0x9ACE000 | module registry ('RIFXRIFF' table), 0x20-byte type-info cells, UI name-cell tables |

## 2. Answers to the three missions

**M1 - record layouts** (p2_typeinfo_layout.json, p2_fieldinf_layout.json)
- typeinfo record: base = marker(0x00080089) - 4, stride 0x50, {{u32 key, u32 kind, 0,0,
  module .data VA, 0, u32 8|0, u32 layout-id, .data cell VA, .text super, .text vtable}}
  - u32_0 = per-type key id (23,701 distinct), u32_11 = index-like layout id (18,196 distinct)
  - the section is NOT a flat array: 23,198/23,701 bases sit at base % 0x50 == 0x30
- fieldinf: 597,760 u32 = 298,880 elements of 8 bytes; tag slot has only 10 distinct values
  {{0,1,0x88,0x1000,0x2000,0x10000,0xFFFFFFFF,0xFFFF0000}}; payload mixes member offsets
  (multiples of 8 + bitfield widths 1/2/3/4), .data cell VA low-32 (0x49FB5xxx) and keys
- **correction**: the earlier "51 % of fieldinf values match typeinfo u32_11" was a zero-value
  artefact (305,620 of the fieldinf u32 are 0 and 0 is in the u32_11 domain).  Excluding zeros
  only 117 values match and all are the 0xFFFFFFFF sentinel vs 2.53 expected by chance =>
  **typeinfo and fieldinf are independent tables, there is no key-type link**

**M2 - the hash** (p2_hash_attack.json, p2_crack6.json)
- 2,499 (type name -> key) anchors were established from code: registration stub -> runtime
  node -> .data type cell -> static record base (delta 0 on 2,499/2,499)
- tested: djb2 add/xor (+ init sweep 0..2^20), fnv1a 32/64, sdbm, jenkins oat, crc32
  (reflected/non-reflected/crc32c), murmur2/murmur3 (+ seed sweep 0..255), xxh32/xxh64,
  siphash24, murmur3_x64_128, plus 36 multipliers x 19 inits x 4 op-orders x 11 name shapes
  (raw/lower/upper/+NUL/prefix-stripped/snake_case/fb::/Ecs::/module-qualified/reversed/
  UTF-16) -> **0 hits everywhere**
- structural proofs (independent of multiplier and init):
  - ADD family h=(h*M)+c: gcd of all same-last-byte key differences = 1 => impossible
  - XOR family h=(h*M)^c: 29,021/29,143 same-last-byte pairs violate the low-8-bit law
  - init solve for 13 candidate multipliers x 2 byte orders: 1/2,499 matches
- no code writes the static records: 0/3,590 key-slot stores and 0/3,174 kind-slot stores
  hit a real static record (all of them land in the all-zero runtime-node regions)
- **verdict: the per-type key is a build-time id, not a digest of the name.  It cannot be
  recomputed from bf6.exe and is not needed for name resolution.**

**M3 - name xrefs** (p2_name_xrefs.json)
- 4,241,357 opcode-valid RIP-relative references were scanned (.text + ctr)
- 182,519 land exactly on a .rdata identifier start, 43,840 distinct identifiers referenced;
  the top ones are module names / protobuf JSON field names / generic words
- the wanted member names appear in 451 code references spread over 83 functions; 126 of them
  sit in type-registration stubs (they LEA a module name + a type name and write a typeinfo
  node), the remaining 325 are JSON/protobuf serializers, string getters and type-name users
  (e.g. 0x16b0110 builds a "LocalPlayer" string) - **none of them pairs a name with an offset**
- the .rdata pool contains type names only; the UI property area 0x84C000-0x84F000 holds the
  first occurrence of Health/TeamId/SquadId/PersonaId/SlotIndex/LocalPlayer (UI bindings)
- 13,672 name-pointer tables were scanned for name+offset pairs: **0 found**
- .vm_sec/.vlizer (lead experiment 3): .vm_sec holds 7,298 non-zero u32 of which 1,641 point
  into .vlizer and 1,626 of those start with `E9 rel32` => .vlizer is 5-byte jump-stub CODE
  (64 KB-block entropy 5.15-7.99, mean 6.28; zlib/gzip/xz/zstd/lz4/bzip2/zip: no stream) and
  holds no field names

## 3. Consequence for the ESP tool

bf6.exe contains type NAMES and member OFFSETS, but never the member NAMES:
- type name -> static record: solvable statically (10,172 registration stubs; 2,499 anchors
  verified end-to-end)
- member offset per type: present in the fieldinf payload slots (statically)
- member name: **absent from the file** - it is materialised at runtime (the runtime node
  carries {u64 name ptr, u16 flags, u16 size} at static_record+0x08, zero on disk)

So an external reader that wants "offset of member X" has to walk the LIVE reflection
(MEC-style: info{{name, flags, size, module, align, fieldCount}}, class{{super, FieldInfo[]}}),
which is exactly the layout the static analysis pinned down, or use a runtime dump.
"""
(HERE / "p2_report.md").write_text(rep, encoding="utf-8")
print("wrote p2_report.md")
