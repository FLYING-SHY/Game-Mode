"""p13: 抽取 BF6 反射成员名/类型名的精确 RVA，并验证候选 AOB 的文件级唯一性。

输出：analysis/p13_names.json
  {
    "names": { "<明文名>": {"rvas": [..], "section": "..", "fileMatches": n} },
    "patterns": { "<签名名>": {"pattern": "48 8D 05 ?? ..", "fileMatches": n, "rva": 0x...} }
  }
"""
import json
import os
import struct
import sys

GAME = r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe"
OUT = r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis\p13_names.json"

IMAGE_BASE = 0x140000000

# 候选签名：字节串（None = 通配）
CANDIDATE_PATTERNS = {
    # 模块注册表 magic "RIFXRIFF"
    "Data.ModuleRegistryMagic": b"RIFXRIFF",
    # ctr thunk 连续 10 个
    "Ctr.ThunkCluster10": b"\x48\x8d\x05\x00\x60\x23\xc3" * 1,  # 占位，下面单独算
}

# 精确名（NUL 结尾）候选
WANTED = [
    # 血量
    "CurrentHealth", "MaxHealth", "HealthPercentage", "HealthStateType_Alive",
    "ClientSoldierHealthComponent", "ClientVehicleHealthComponent",
    "ClientControllableHealthComponent", "GameHealthComponent", "ClientGameHealthComponent",
    "healthComponent", "health",
    # 队伍
    "CurrentTeamId", "TeamId", "OwningTeam", "ShooterTeamId", "LocalTeam",
    "Property_TeamId", "Team1", "Team2",
    # 位置/朝向
    "positionX", "positionY", "positionZ", "Position", "worldPosition",
    "transformPosition", "eulerAngles", "linearPosition", "Transform",
    "EcsRimeTransform", "EcsWorldTransform", "EcsTransformSpaceComponent",
    # 实体/玩家
    "ClientSoldierEntity", "ClientPlayerManager", "ClientPlayerIteratorEntity",
    "ClientConnection", "ClientSoldierCameraComponent", "ClientSoldierEntryComponent",
    "ClientSoldierWeapon", "BFClientPlayerExtent", "BFPlayerExtent",
    "ClientPlayerExtent", "AimEntity", "LocalPlayer", "LocalPlayerId",
    # 相机/矩阵
    "CameraTransform", "EvaluatedCameraTransform", "g_worldViewProjMatrix",
    "g_invProjMatrix", "invViewProjMatrix", "ViewMatrix", "ProjectionMatrix",
    # 其它常见 Frostbite 属性名
    "m_position", "m_transform", "m_health", "m_teamId", "teamId",
    "playerName", "PlayerName", "Name", "SoldierName",
    "isAlive", "Alive", "Dead", "IsLocalPlayer", "LocalPlayerEntity",
]


def parse_sections(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[pe:pe + 4] == b"PE\0\0"
    coff = pe + 4
    nsections = struct.unpack_from("<H", data, coff + 2)[0]
    opt_size = struct.unpack_from("<H", data, coff + 16)[0]
    opt = coff + 20
    magic = struct.unpack_from("<H", data, opt)[0]
    assert magic == 0x20B, hex(magic)
    sec_off = opt + opt_size
    secs = []
    for i in range(nsections):
        off = sec_off + i * 40
        name = data[off:off + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rawsize, rawoff = struct.unpack_from("<IIII", data, off + 8)
        secs.append({
            "name": name, "vaddr": vaddr, "vsize": vsize,
            "rawsize": rawsize, "rawoff": rawoff,
            "vaddr_end": vaddr + max(vsize, rawsize),
        })
    return secs


def sec_of(secs, rva):
    for s in secs:
        if s["vaddr"] <= rva < s["vaddr_end"]:
            return s["name"]
    return None


def count_matches(data, pattern):
    """pattern: list of (bytes|None) 逐字节匹配。"""
    n = len(pattern)
    if n == 0:
        return 0
    first = pattern[0]
    if first is None:
        # 全部通配，跳过
        return 0
    count = 0
    start = 0
    while True:
        idx = data.find(first, start)
        if idx < 0:
            break
        ok = True
        for k in range(1, n):
            p = pattern[k]
            if p is None:
                continue
            if idx + k >= len(data) or data[idx + k] != p:
                ok = False
                break
        if ok:
            count += 1
            start = idx + 1
        else:
            start = idx + 1
    return count


def main():
    if not os.path.isfile(GAME):
        print("找不到", GAME)
        return 1
    with open(GAME, "rb") as fh:
        data = fh.read()
    print("文件大小", len(data))
    secs = parse_sections(data)

    out = {"imageBase": IMAGE_BASE, "sections": secs, "names": {}, "patterns": {}}

    # --- 名字 ---
    for name in WANTED:
        needle = name.encode("ascii") + b"\0"
        hits = []
        start = 0
        while True:
            idx = data.find(needle, start)
            if idx < 0:
                break
            hits.append(idx)
            start = idx + 1
            if len(hits) > 64:
                break
        entries = []
        for off in hits:
            # 找到所属节
            for s in secs:
                if s["rawoff"] <= off < s["rawoff"] + s["rawsize"]:
                    rva = s["vaddr"] + (off - s["rawoff"])
                    entries.append({"fileOffset": off, "rva": rva, "section": s["name"]})
                    break
        if entries:
            out["names"][name] = {"fileMatches": len(hits), "rvas": [e["rva"] for e in entries],
                                   "hits": entries}
            print("%-38s fileMatches=%-4d rva=%s" % (
                name, len(hits), ", ".join("0x%X" % e["rva"] for e in entries[:6])))

    # --- 候选签名唯一性 ---
    # 1) 模块注册表 magic
    magic = b"RIFXRIFF"
    cnt = count_matches(data, [magic])
    idx = data.find(magic)
    rva = None
    sec = None
    if idx >= 0:
        for s in secs:
            if s["rawoff"] <= idx < s["rawoff"] + s["rawsize"]:
                rva = s["vaddr"] + (idx - s["rawoff"])
                sec = s["name"]
                break
    out["patterns"]["Data.ModuleRegistryMagic"] = {
        "pattern": "52 49 46 58 52 49 46 58", "fileMatches": cnt, "rva": rva, "section": sec}
    print("magic RIFXRIFF fileMatches=%d rva=%s" % (cnt, hex(rva) if rva else None))

    # 2) typeinfo 标记 89 00 00 08（typeinfo 节内计数）
    ti = next((s for s in secs if s["name"] == "typeinfo"), None)
    if ti:
        blob = data[ti["rawoff"]:ti["rawoff"] + ti["rawsize"]]
        mark = struct.pack("<I", 0x00080089)
        n = blob.count(mark)
        out["patterns"]["TypeInfo.RecordMarker"] = {
            "pattern": "89 00 00 08", "fileMatches": n, "section": "typeinfo"}
        print("typeinfo 标记 0x00080089 出现", n, "次")

    # 3) ctr thunk 连续 10 个（80 字节）：从 p9 已知簇起点 RVA 0x07964095
    ctr = next((s for s in secs if s["name"] == "ctr"), None)
    if ctr:
        r = 0x07964095
        off = ctr["rawoff"] + (r - ctr["vaddr"])
        blob = data[off:off + 80]
        pat = []
        for i in range(80):
            if i % 8 == 3 or i % 8 == 4 or i % 8 == 5 or i % 8 == 6:
                pat.append(None)
            else:
                pat.append(blob[i:i + 1])
        cnt = count_matches(data, pat)
        out["patterns"]["Ctr.ThunkCluster10"] = {
            "pattern": "48 8D 05 ?? ?? ?? ?? C3 (x10)", "fileMatches": cnt, "rva": r}
        print("ctr thunk 簇(10 个) fileMatches=%d" % cnt)

    with open(OUT, "w", encoding="utf-8") as fh:
        json.dump(out, fh, ensure_ascii=False, indent=1)
    print("已写出", OUT)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
