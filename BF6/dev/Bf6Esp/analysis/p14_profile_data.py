#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""p14_profile_data.py —— 为偏移档案准备权威静态数据。

做三件事：
  1) 解析 bf6.exe 的 PE 节表（vaddr / vsize / rawsize / rawoff）。
  2) 对候选明文名做 **NUL 结尾精确匹配**（避免 CurrentHealth 命中 CurrentHealthNormalized）。
  3) 把每个候选名转成 ASCII 字节模式串，统计它在**整个文件**里的匹配次数，
     只有唯一（matches == 1）的模式串才允许写进档案当定位锚点。

产物：analysis/p14_profile_data.json
"""
import json
import io
import os
import struct

EXE = r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe"
OUT = r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis\p14_profile_data.json"

NAMES = [
    # 类型名
    "ClientSoldierEntity",
    "ClientSoldierHealthComponent",
    "ClientSoldierCameraComponent",
    "ClientSoldierEntryComponent",
    "ClientSoldierWeapon",
    "ClientPlayerManager",
    "ClientPlayerIteratorEntity",
    "ClientConnection",
    "ClientPlayerExtent",
    "BFClientPlayerExtent",
    "BFPlayerExtent",
    # 成员 / 属性名（BF6 里这些名字在磁盘上属于 BFUI 属性区，不是成员表）
    "CurrentHealth",
    "MaxHealth",
    "CurrentTeamId",
    "OwningTeam",
    "ShooterTeamId",
    "positionX",
    "positionY",
    "positionZ",
    "HealthPercentage",
    "Alive",
    "Dead",
    "playerName",
    "PlayerName",
    "LocalPlayer",
    "LocalTeam",
    "g_worldViewProjMatrix",
    "g_invProjMatrix",
    "ViewMatrix",
    "ProjectionMatrix",
    "EvaluatedCameraTransform",
    "CameraTransform",
    "EcsRimeTransform",
    "EcsWorldTransform",
    "EcsTransformSpaceComponent",
    "LocalPlayerId",
    "TeamId",
    "teamId",
]

# 额外锚点：模块注册表 magic（ASCII "RIFXRIFF"）
MAGIC = b"RIFXRIFF"


def parse_sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[pe:pe + 4] == b"PE\0\0", "不是 PE 文件"
    coff = pe + 4
    machine, nsec = struct.unpack_from("<HH", data, coff)
    opt_size = struct.unpack_from("<H", data, coff + 16)[0]
    opt = coff + 20
    magic = struct.unpack_from("<H", data, opt)[0]
    image_base = struct.unpack_from("<Q", data, opt + 24)[0]
    sec_off = opt + opt_size
    sections = []
    for i in range(nsec):
        off = sec_off + i * 40
        name = data[off:off + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, off + 8)
        sections.append({
            "name": name, "vaddr": vaddr, "vsize": vsize,
            "rawsize": rsize, "rawoff": roff,
        })
    return {"machine": machine, "imageBase": image_base, "sections": sections}


def exact_occurrences(data, needle):
    """NUL 结尾精确匹配：needle + b"\0"。"""
    hits = []
    start = 0
    pat = needle + b"\0"
    while True:
        idx = data.find(pat, start)
        if idx < 0:
            break
        hits.append(idx)
        start = idx + 1
    return hits


def count_pattern(data, pat):
    hits = []
    start = 0
    while True:
        idx = data.find(pat, start)
        if idx < 0:
            break
        hits.append(idx)
        start = idx + 1
    return hits


def rva_of(sections, offset):
    for s in sections:
        if s["rawoff"] <= offset < s["rawoff"] + s["rawsize"]:
            return s["vaddr"] + (offset - s["rawoff"]), s["name"]
    return None, None


def main():
    if not os.path.exists(EXE):
        raise SystemExit("找不到 " + EXE)
    with io.open(EXE, "rb") as fh:
        data = fh.read()
    pe = parse_sections(data)
    sections = pe["sections"]
    out = {"exe": EXE, "fileSize": len(data), "imageBase": pe["imageBase"],
           "sections": sections, "names": {}, "anchors": []}

    for name in NAMES:
        needle = name.encode("ascii")
        occ = exact_occurrences(data, needle)
        items = []
        for off in occ:
            rva, sec = rva_of(sections, off)
            items.append({"fileOffset": off, "rva": rva, "section": sec})
        out["names"][name] = {"count": len(occ), "occurrences": items}

    # 锚点：ASCII 名字模式串 + 全文件匹配次数
    for name in NAMES:
        pat = name.encode("ascii")
        hits = count_pattern(data, pat)
        rec = {"name": "Name." + name, "pattern": " ".join("%02X" % b for b in pat),
               "section": ".rdata", "fileMatches": len(hits),
               "occurrences": [{"fileOffset": h, "rva": rva_of(sections, h)[0]} for h in hits[:8]]}
        out["anchors"].append(rec)

    magic_hits = count_pattern(data, MAGIC)
    out["anchors"].append({
        "name": "ModuleRegistryMagic",
        "pattern": " ".join("%02X" % b for b in MAGIC),
        "section": ".data", "fileMatches": len(magic_hits),
        "occurrences": [{"fileOffset": h, "rva": rva_of(sections, h)[0]} for h in magic_hits[:8]],
    })

    # typeinfo 记录标记
    marker = struct.pack("<I", 0x00080089)
    ti = [s for s in sections if s["name"] == "typeinfo"][0]
    blob = data[ti["rawoff"]:ti["rawoff"] + ti["rawsize"]]
    out["typeInfoMarker"] = {"value": "89 00 00 08", "count": count_pattern(blob, marker).__len__(),
                             "sectionRva": ti["vaddr"]}

    with io.open(OUT, "w", encoding="utf-8") as fh:
        json.dump(out, fh, ensure_ascii=False, indent=1)
    print("写出", OUT)
    for rec in out["anchors"]:
        if rec["fileMatches"] <= 2:
            print("  唯一/少命中:", rec["name"], rec["fileMatches"], rec["occurrences"][:2])
    print("typeinfo 标记命中:", out["typeInfoMarker"]["count"])


if __name__ == "__main__":
    main()
