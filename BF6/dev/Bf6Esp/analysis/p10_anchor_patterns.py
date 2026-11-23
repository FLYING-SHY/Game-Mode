#!/usr/bin/env python3
"""p10_anchor_patterns.py —— 为档案生成真实可用的 AOB 模式串。

做法：
1) 在 .rdata 里精确定位反射名（带 NUL 的完整匹配），得到候选 RVA；
2) 在 .text / ctr 里扫所有 4 字节 disp32 窗口，命中条件：target = window_end + disp
   等于某个候选 RVA（window_end = 窗口起点 + 4，即 RIP 相对寻址的下一条指令地址）；
3) 对每个命中向前回溯，解出真实指令起点与 opcode，输出可写入档案的 AOB；
4) 用全文件匹配数验证唯一性。

输出：analysis/p10_anchor_patterns.json
"""
import json
import os
import struct

import numpy as np

GAME = r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe"
OUTDIR = os.path.dirname(os.path.abspath(__file__))

NAMES = [
    "ClientSoldierEntity",
    "ClientSoldierHealthComponent",
    "CurrentHealth",
    "MaxHealth",
    "CurrentTeamId",
    "ClientPlayerManager",
    "ClientPlayerIteratorEntity",
    "ClientConnection",
    "EcsRimeTransform",
    "EcsWorldTransform",
    "ClientSoldierCameraComponent",
    "ClientSoldierEntryComponent",
    "OwningTeam",
    "ShooterTeamId",
]

# 反汇编里常见、可用回溯确认的 RIP 相对寻址前缀
RIP_PREFIX = {
    # (opcode 前缀字节序列) -> (disp 在指令内的偏移, 指令长度)
    (0x48, 0x8D): (3, 7),
    (0x48, 0x8B): (3, 7),
    (0x4C, 0x8D): (3, 7),
    (0x4C, 0x8B): (3, 7),
    (0x48, 0x89): (3, 7),
    (0x48, 0xC7): (3, 7),  # mov qword [rip+disp32], imm32 -> 实际 11 字节，仅作参考
}


def parse_sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[pe:pe + 4] == b"PE\0\0"
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


def cstring_at(data, sections, rva, limit=96):
    off = rva2off(sections, rva)
    if off is None:
        return None
    end = data.find(b"\0", off, off + limit)
    if end == -1:
        return None
    try:
        return data[off:end].decode("ascii")
    except UnicodeDecodeError:
        return None


def find_exact(data, sections, text):
    needle = text.encode("ascii") + b"\0"
    out = []
    idx = data.find(needle)
    while idx != -1:
        rva = None
        for s in sections:
            if s["rawoff"] <= idx < s["rawoff"] + s["rawsize"]:
                rva = s["vaddr"] + (idx - s["rawoff"])
                break
        if rva is not None:
            out.append(rva)
        idx = data.find(needle, idx + 1)
    return out


def pattern_of(raw, disp_offset):
    out = []
    for i, byte in enumerate(raw):
        out.append("??")
    for i, byte in enumerate(raw):
        if not (disp_offset <= i < disp_offset + 4):
            out[i] = "%02X" % byte
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


def main():
    with open(GAME, "rb") as handle:
        data = handle.read()
    info = parse_sections(data)
    sections = info["sections"]
    sec_by_name = {s["name"]: s for s in sections}

    result = {"imageBase": info["imageBase"], "sections": sections, "names": {}}

    code_sections = [s for s in sections if s["name"] in (".text", "ctr")]

    for name in NAMES:
        targets = find_exact(data, sections, name)
        entry = {"name": name, "stringRvas": [hex(t) for t in targets], "refs": []}
        for target_rva in targets:
            # 在代码节里找指向该 RVA 的 RIP 相对引用
            for cs in code_sections:
                start = cs["rawoff"]
                size = cs["rawsize"]
                chunk = data[start:start + size]
                arr = np.frombuffer(chunk, dtype=np.uint8)
                # 构造 4 字节窗口的 disp 值
                win = (arr[:-3].astype(np.int64) << 24) | (arr[1:-2].astype(np.int64) << 16) | \
                      (arr[2:-1].astype(np.int64) << 8) | arr[3:].astype(np.int64)
                win = np.where(win >= (1 << 31), win - (1 << 32), win)
                # 目标 = 窗口末尾 RVA + disp
                win_end_rva = (cs["vaddr"] + np.arange(len(win), dtype=np.int64) + 4)
                hit = np.nonzero(win_end_rva + win == target_rva)[0]
                for h in hit:
                    disp_pos = int(h) + start  # 文件偏移
                    disp_rva = cs["vaddr"] + int(h)
                    # 回溯确定指令起点
                    decoded = None
                    for back in range(0, 5):
                        so = disp_pos - back
                        if so < 0 or so + 8 > len(data):
                            continue
                        b = data[so:so + 2]
                        key = (b[0], b[1])
                        if key not in RIP_PREFIX:
                            continue
                        disp_offset = back + RIP_PREFIX[key][1] - 4  # disp 相对指令起点
                        insn_len = RIP_PREFIX[key][1]
                        if back + 4 > len(data):
                            continue
                        disp = struct.unpack_from("<i", data, disp_pos)[0]
                        insn_start_rva = disp_rva - back
                        tgt = insn_start_rva + insn_len + disp
                        if tgt != target_rva:
                            continue
                        raw = data[so:so + insn_len]
                        decoded = {
                            "instructionRva": insn_start_rva,
                            "bytes": raw.hex(),
                            "pattern": pattern_of(raw, RIP_PREFIX[key][1] - 4),
                            "targetRva": target_rva,
                            "opcode": "%02X %02X" % key,
                        }
                        break
                    if decoded is None:
                        continue
                    decoded["section"] = cs["name"]
                    decoded["fileMatches"] = count_matches(data, decoded["pattern"])
                    decoded["stringText"] = cstring_at(data, sections, target_rva)
                    entry["refs"].append(decoded)
        result["names"][name] = entry
        uniq = sorted({r["pattern"] for r in entry["refs"]})
        print("%-32s stringRvas=%s refs=%d patterns=%s" % (
            name, entry["stringRvas"], len(entry["refs"]), uniq[:3]))

    path = os.path.join(OUTDIR, "p10_anchor_patterns.json")
    with open(path, "w", encoding="utf-8") as handle:
        json.dump(result, handle, ensure_ascii=False, indent=2)
    print("wrote", path)


if __name__ == "__main__":
    main()
