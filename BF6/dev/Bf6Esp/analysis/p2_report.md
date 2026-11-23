# BF6 reflection metadata — final report (subagent frostbite-hash)

binary: G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe
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
