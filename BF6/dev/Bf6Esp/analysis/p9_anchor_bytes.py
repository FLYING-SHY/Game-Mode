#!/usr/bin/env python3
"""p9_anchor_bytes.py —— 从 bf6.exe 取锚点指令的真实字节，供档案里写 AOB 模式串。

要点：p7/xref_scan 给出的 code_rva 是 RIP 相对指令里 disp32 字段的位置，
不是指令起点。这里向前回溯最多 4 字节，找到真正的指令起点，
并用"目标地址处的字符串"验证解出的引用确实是预期的反射名。

输出：analysis/p9_anchor_bytes.json
"""
import json
import os
import struct

GAME = r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe"
OUTDIR = os.path.dirname(os.path.abspath(__file__))

# disp32 字段所在 RVA（来自 xref_scan 的 code_rva）
ANCHORS = {
    "Text.HealthStringRef": (0x02F94690, "Health"),
    "Text.TeamIdStringRef": (0x02AE8BE0, "TeamId"),
    "Text.LocalPlayerStringRef": (0x03C1C500, "LocalPlayer"),
}

# 需要在 .rdata 里精确定位的反射名
NAME_STRINGS = [
    "ClientSoldierEntity",
    "ClientSoldierHealthComponent",
    "CurrentHealth",
    "MaxHealth",
    "CurrentTeamId",
    "ClientPlayerManager",
    "ClientPlayerIteratorEntity",
    "EcsRimeTransform",
    "EcsWorldTransform",
]

CTR_CLUSTER_RVA = 0x07964095
CTR_CLUSTER_COUNT = 10

RIP_MODRM = {0x05, 0x0D, 0x15, 0x1D, 0x25, 0x2D, 0x35, 0x3D}


def parse_sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[pe:pe + 4] == b"PE\0\0", "not a PE file"
    coff = pe + 4
    number_of_sections = struct.unpack_from("<H", data, coff + 2)[0]
    size_of_optional = struct.unpack_from("<H", data, coff + 16)[0]
    opt = coff + 20
    magic = struct.unpack_from("<H", data, opt)[0]
    image_base = struct.unpack_from("<Q", data, opt + 24)[0] if magic == 0x20B else struct.unpack_from("<I", data, opt + 28)[0]
    sec_table = opt + size_of_optional
    sections = []
    for i in range(number_of_sections):
        off = sec_table + i * 40
        name = data[off:off + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, off + 8)
        sections.append({"name": name, "vaddr": vaddr, "vsize": vsize, "rawsize": rsize, "rawoff": roff})
    return {"imageBase": image_base, "sections": sections}


def rva2off(sections, rva):
    for s in sections:
        if s["vaddr"] <= rva < s["vaddr"] + max(s["vsize"], s["rawsize"]):
            return s["rawoff"] + (rva - s["vaddr"])
    return None


def sec_of(sections, rva):
    for s in sections:
        if s["vaddr"] <= rva < s["vaddr"] + max(s["vsize"], s["rawsize"]):
            return s["name"]
    return None


def cstring_at(data, sections, rva, limit=96):
    off = rva2off(sections, rva)
    if off is None:
        return None
    end = data.find(b"\0", off, off + limit)
    if end == -1:
        return None
    return data[off:end].decode("ascii", "replace")


def pattern_from_bytes(raw, wildcard_disp_offset=None, wildcard_disp_len=4):
    out = []
    for i, byte in enumerate(raw):
        if wildcard_disp_offset is not None and wildcard_disp_offset <= i < wildcard_disp_offset + wildcard_disp_len:
            out.append("??")
        else:
            out.append("%02X" % byte)
    return " ".join(out)


def count_file_matches(data, pattern):
    parts = []
    for token in pattern.split():
        parts.append(None if token == "??" else bytes([int(token, 16)]))
    if not parts or parts[0] is None:
        return 0
    first = parts[0]
    count = 0
    idx = data.find(first)
    while idx != -1:
        ok = True
        for k, token in enumerate(parts):
            if token is None:
                continue
            pos = idx + k
            if pos >= len(data) or data[pos:pos + 1] != token:
                ok = False
                break
        if ok:
            count += 1
        idx = data.find(first, idx + 1)
    return count


def find_exact_strings(data, sections, text):
    needle = text.encode("ascii") + b"\0"
    hits = []
    idx = data.find(needle)
    while idx != -1:
        rva = None
        for s in sections:
            if s["rawoff"] <= idx < s["rawoff"] + s["rawsize"]:
                rva = s["vaddr"] + (idx - s["rawoff"])
                break
        hits.append({"fileOffset": idx, "rva": rva, "section": sec_of(sections, rva) if rva is not None else None})
        idx = data.find(needle, idx + 1)
    return hits


def decode_anchor(data, sections, disp_rva, expected_name):
    """在 disp_rva 附近回溯，找到一条 RIP 相对指令，其目标字符串 == expected_name。"""
    disp_off = rva2off(sections, disp_rva)
    if disp_off is None:
        return None
    for back in range(0, 5):
        start_rva = disp_rva - back
        start_off = disp_off - back
        if start_off < 0 or start_off + 8 > len(data):
            continue
        window = data[start_off:start_off + 8]
        candidates = []
        # 48/4C 8D/8B modrm(rip) disp32  -> 7 字节
        if window[0] in (0x48, 0x4C) and window[1] in (0x8D, 0x8B) and (window[2] & 0xC7) == 0x05:
            candidates.append((3, 7, "lea/mov r64,[rip+disp32]"))
        # E8 rel32 -> 5 字节
        if window[0] == 0xE8:
            candidates.append((1, 5, "call rel32"))
        # 0F 10/11 modrm(rip) -> 6 字节
        if window[0] == 0x0F and window[1] in (0x10, 0x11) and (window[2] & 0xC7) == 0x05:
            candidates.append((3, 6, "movss/movsd xmm,[rip+disp32]"))
        # 8B /r modrm(rip) -> 6 字节
        if window[0] == 0x8B and (window[1] & 0xC7) == 0x05:
            candidates.append((2, 6, "mov r32,[rip+disp32]"))
        for disp_pos, insn_len, label in candidates:
            disp = struct.unpack_from("<i", window, disp_pos)[0]
            target_rva = start_rva + insn_len + disp
            if sec_of(sections, target_rva) != ".rdata":
                continue
            text = cstring_at(data, sections, target_rva)
            if text != expected_name:
                continue
            raw = data[start_off:start_off + insn_len]
            pattern = pattern_from_bytes(raw, disp_pos, 4)
            return {
                "instructionRva": start_rva,
                "dispRva": disp_rva,
                "instructionBytes": raw.hex(),
                "pattern": pattern,
                "targetRva": target_rva,
                "targetText": text,
                "label": label,
                "fileMatches": count_file_matches(data, pattern),
            }
    return None


def main():
    with open(GAME, "rb") as handle:
        data = handle.read()
    info = parse_sections(data)
    sections = info["sections"]

    out = {"imageBase": info["imageBase"], "sections": sections, "anchors": {}, "names": {}}

    for name, (disp_rva, expected) in ANCHORS.items():
        out["anchors"][name] = decode_anchor(data, sections, disp_rva, expected)

    for name in NAME_STRINGS:
        hits = find_exact_strings(data, sections, name)
        hex_pattern = " ".join("%02X" % b for b in name.encode("ascii"))
        out["names"][name] = {
            "exactHits": hits,
            "pattern": hex_pattern,
            "fileMatches": count_file_matches(data, hex_pattern),
        }

    off = rva2off(sections, CTR_CLUSTER_RVA)
    cluster = []
    for i in range(CTR_CLUSTER_COUNT):
        chunk = data[off + i * 8: off + i * 8 + 8]
        cluster.append({"rva": CTR_CLUSTER_RVA + i * 8, "bytes": chunk.hex()})
    out["ctrCluster"] = {
        "rva": CTR_CLUSTER_RVA,
        "count": CTR_CLUSTER_COUNT,
        "entries": cluster,
        "pattern": "48 8D 05 ?? ?? ?? ?? C3",
        "fileMatches": count_file_matches(data, "48 8D 05 ?? ?? ?? ?? C3"),
    }

    path = os.path.join(OUTDIR, "p9_anchor_bytes.json")
    with open(path, "w", encoding="utf-8") as handle:
        json.dump(out, handle, ensure_ascii=False, indent=2)
    print("wrote", path)
    for name, entry in out["anchors"].items():
        if entry is None:
            print(name, "-> NOT DECODED")
            continue
        print("%-26s instr=0x%X pattern=%-28s matches=%d target=0x%X %r" % (
            name, entry["instructionRva"], entry["pattern"], entry["fileMatches"],
            entry["targetRva"], entry["targetText"]))
    for name, entry in out["names"].items():
        rvas = [h["rva"] for h in entry["exactHits"] if h["rva"] is not None]
        print("%-32s matches=%d rvas=%s" % (name, entry["fileMatches"], [hex(r) for r in rvas]))
    print("ctr cluster pattern matches:", out["ctrCluster"]["fileMatches"])


if __name__ == "__main__":
    main()
