#!/usr/bin/env python3
"""p11_xref_strings.py —— 全量解析 bf6.exe 的 RIP 相对引用，解析出目标字符串。

背景：xref_scan.py 早先把 code_rva 报成 disp32 字段位置（不是指令起点），
并且 target 用的是"子串首次出现"的偏移，而不是 C 字符串起点，
所以 p9/p10 回溯解码全部失败。

本脚本做法（更稳）：
1) 向量化扫 .text / ctr 的每个 4 字节对齐 dword，当作 RIP 相对指令的 disp32 字段；
   目标地址 = disp32 字段所在 RVA + 4 + disp（即"下一条指令地址 + 位移"，标准 x64 语义）；
2) 只保留落进 .rdata / .data 的目标，去重后逐个读取 C 字符串；
3) 反向：对每个命中，把 disp32 字段地址往前回溯，按 x64 指令模板表确定真正的指令起点与长度，
   产出可写入档案的 AOB 模式串（disp32 用 ?? 通配）并统计全文件唯一性；
4) 特别挑出与 ESP 相关的名字（Health/Team/Position/Transform/Player/…）做重点输出。

输出：analysis/p11_xref_strings.json
"""
import json
import os
import struct
from collections import defaultdict

import numpy as np

GAME = r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe"
OUTDIR = os.path.dirname(os.path.abspath(__file__))

CODE_SECTIONS = (".text", "ctr")
DATA_SECTIONS = (".rdata", ".data")

# x64 指令模板：(前缀字节序列) -> (disp32 在指令内的偏移, 指令总长度)
RIP_TEMPLATES = [
    ((0x48,), 3, 7),   # 48 8D/8B/89/63 modrm(rip) disp32
    ((0x4C,), 3, 7),
    ((0x49,), 3, 7),
    ((0x41,), 3, 7),
    ((0x8B,), 2, 6),   # 8B modrm(rip) disp32
    ((0x89,), 2, 6),
    ((0x0F,), 3, 6),   # 0F 10/11 modrm(rip) disp32
    ((0xE8,), 1, 5),   # call rel32
    ((0xE9,), 1, 5),   # jmp rel32
    ((0xC7,), 1, 6),   # C7 05 disp32 imm32 (mov dword [rip+d], imm32)
]


def parse_sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    nsec = struct.unpack_from("<H", data, pe + 6)[0]
    opt = struct.unpack_from("<H", data, pe + 20)[0]
    base = pe + 24 + opt
    out = []
    for i in range(nsec):
        o = base + i * 40
        name = data[o:o + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, o + 8)
        out.append({"name": name, "vaddr": vaddr, "vsize": vsize, "rawsize": rsize, "rawoff": roff})
    return out


def rva2off(sections, rva):
    for s in sections:
        span = max(s["vsize"], s["rawsize"])
        if s["vaddr"] <= rva < s["vaddr"] + span:
            return s["rawoff"] + (rva - s["vaddr"])
    return None


def sec_of(sections, rva):
    for s in sections:
        span = max(s["vsize"], s["rawsize"])
        if s["vaddr"] <= rva < s["vaddr"] + span:
            return s["name"]
    return None


def cstring_at(data, sections, rva, limit=72):
    off = rva2off(sections, rva)
    if off is None:
        return None
    end = data.find(b"\0", off, off + limit)
    if end == -1:
        return None
    chunk = data[off:end]
    if not chunk:
        return ""
    try:
        text = chunk.decode("ascii")
    except UnicodeDecodeError:
        return None
    if not all(32 <= ord(c) < 127 for c in text):
        return None
    return text


def build_pattern(raw, disp_offset):
    out = []
    for i, byte in enumerate(raw):
        out.append("??" if disp_offset <= i < disp_offset + 4 else "%02X" % byte)
    return " ".join(out)


def count_matches(data, pattern):
    tokens = [None if t == "??" else bytes([int(t, 16)]) for t in pattern.split()]
    if not tokens or tokens[0] is None:
        return 0
    first = tokens[0]
    count = 0
    idx = data.find(first)
    while idx != -1:
        ok = True
        for k, token in enumerate(tokens):
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


def decode_instruction(data, sections, disp_rva, target_rva):
    """从 disp32 字段所在 RVA 往前回溯，确定指令起点/长度。"""
    disp_off = rva2off(sections, disp_rva)
    if disp_off is None:
        return None
    for back in range(0, 6):
        start_rva = disp_rva - back
        start_off = disp_off - back
        if start_off < 0 or start_off + 12 > len(data):
            continue
        for prefixes, disp_rel, insn_len in RIP_TEMPLATES:
            if back != disp_rel:
                continue
            for pre in prefixes:
                if data[start_off] != pre:
                    continue
                if insn_len == 6 and pre == 0xC7:
                    continue
                disp = struct.unpack_from("<i", data, disp_off)[0]
                end_rva = start_rva + insn_len
                if end_rva + disp != target_rva:
                    continue
                raw = data[start_off:start_off + insn_len]
                return {
                    "instructionRva": start_rva,
                    "instructionLength": insn_len,
                    "bytes": raw.hex(),
                    "pattern": build_pattern(raw, disp_rel),
                    "opcode": "%02X" % pre,
                }
    return None


INTEREST = (
    "health", "team", "position", "transform", "player", "soldier", "entity",
    "matrix", "view", "projection", "camera", "local", "alive", "name",
)


def main():
    with open(GAME, "rb") as handle:
        data = handle.read()
    sections = parse_sections(data)
    sec_by_name = {s["name"]: s for s in sections}

    data_ranges = []
    for name in DATA_SECTIONS:
        s = sec_by_name[name]
        data_ranges.append((s["vaddr"], s["vaddr"] + max(s["vsize"], s["rawsize"])))

    refs_by_target = defaultdict(list)

    for sname in CODE_SECTIONS:
        s = sec_by_name[sname]
        blob = data[s["rawoff"]:s["rawoff"] + s["rawsize"]]
        n = len(blob) // 4
        arr = np.frombuffer(blob, dtype="<u4").astype(np.int64)
        disp = np.where(arr >= (1 << 31), arr - (1 << 32), arr)
        idx = np.arange(n, dtype=np.int64)
        # disp32 字段的起始 RVA，next-instruction RVA = 该 RVA + 4
        field_rva = s["vaddr"] + idx * 4
        tgt = field_rva + 4 + disp
        lo = min(r[0] for r in data_ranges)
        hi = max(r[1] for r in data_ranges)
        mask = (tgt >= lo) & (tgt < hi)
        hits = np.nonzero(mask)[0]
        print(sname, "refs into data:", len(hits))
        for h in hits:
            refs_by_target[int(tgt[h])].append((sname, int(field_rva[h])))

    print("distinct targets:", len(refs_by_target))

    out = {"imageBase": 0x140000000, "sections": sections, "targets": {}, "interesting": []}
    interesting = []

    for target_rva in sorted(refs_by_target):
        text = cstring_at(data, sections, target_rva)
        if text is None or not text:
            continue
        refs = refs_by_target[target_rva]
        entry = {"targetRva": target_rva, "section": sec_of(sections, target_rva), "text": text, "refs": []}
        for sname, field_rva in refs:
            dec = decode_instruction(data, sections, field_rva, target_rva)
            if dec is None:
                continue
            dec["section"] = sname
            dec["dispFieldRva"] = field_rva
            dec["fileMatches"] = count_matches(data, dec["pattern"])
            entry["refs"].append(dec)
        if not entry["refs"]:
            continue
        out["targets"][hex(target_rva)] = entry
        low = text.lower()
        if any(k in low for k in INTEREST):
            interesting.append(entry)

    out["interesting"] = interesting

    path = os.path.join(OUTDIR, "p11_xref_strings.json")
    with open(path, "w", encoding="utf-8") as handle:
        json.dump(out, handle, ensure_ascii=False, indent=2)
    print("wrote", path, "targets with decoded refs:", len(out["targets"]), "interesting:", len(interesting))

    for entry in interesting[:60]:
        for ref in entry["refs"][:2]:
            print("  %-40s @0x%X  instr@0x%X  %-30s matches=%d" % (
                entry["text"][:40], entry["targetRva"], ref["instructionRva"], ref["pattern"], ref["fileMatches"]))


if __name__ == "__main__":
    main()
