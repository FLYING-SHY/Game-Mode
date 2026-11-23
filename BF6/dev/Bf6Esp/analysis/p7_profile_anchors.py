# -*- coding: utf-8 -*-
"""p7_profile_anchors.py — 从 bf6.exe 抽取真实档案数据：
  1) 完整段表（VA/RVA/VirtualSize/RawSize/RawOffset）
  2) 关键反射名的全部出现位置（file offset -> RVA）
  3) 可用作 AOB 的锚点字节（带通配的唯一性验证）
输出：analysis/p7_profile_anchors.json
"""
import json
import os
import struct

GAME = r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe"
OUTDIR = r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis"


def parse_sections(data):
    e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[e_lfanew:e_lfanew + 4] == b"PE\0\0"
    coff = e_lfanew + 4
    nsec = struct.unpack_from("<H", data, coff + 2)[0]
    opt_size = struct.unpack_from("<H", data, coff + 16)[0]
    opt = coff + 20
    magic = struct.unpack_from("<H", data, opt)[0]
    image_base = struct.unpack_from("<Q", data, opt + 24)[0]
    sec_off = opt + opt_size
    secs = []
    for i in range(nsec):
        o = sec_off + i * 40
        name = data[o:o + 8].split(b"\0")[0].decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, o + 8)
        secs.append({
            "name": name, "rva": vaddr, "virtual_size": vsize,
            "raw_size": rsize, "raw_offset": roff,
            "characteristics": struct.unpack_from("<I", data, o + 36)[0],
        })
    return {"image_base": image_base, "sections": secs}


def main():
    with open(GAME, "rb") as fh:
        data = fh.read()
    img = parse_sections(data)
    secs = img["sections"]

    def off2rva(off):
        for s in secs:
            if s["raw_offset"] <= off < s["raw_offset"] + s["raw_size"]:
                return s["rva"] + (off - s["raw_offset"])
        return None

    def rva2off(rva):
        for s in secs:
            if s["rva"] <= rva < s["rva"] + max(s["virtual_size"], s["raw_size"]):
                return s["raw_offset"] + (rva - s["rva"])
        return None

    def sec_of_rva(rva):
        for s in secs:
            if s["rva"] <= rva < s["rva"] + max(s["virtual_size"], s["raw_size"]):
                return s["name"]
        return None

    # ---- 2) 反射名出现位置 ----------------------------------------------
    names = [
        "ClientSoldierEntity", "ClientSoldierHealthComponent", "CurrentHealth",
        "MaxHealth", "CurrentTeamId", "ClientPlayerManager", "ClientSoldierEntity",
        "ClientPlayerIteratorEntity", "HealthComponent", "LocalPlayer",
        "ClientGame", "GameContext", "ClientConnection", "ClientSoldierWeapon",
        "ClientSoldierCameraComponent", "EcsRimeTransform", "EcsWorldTransform",
        "CurrentHealthPercentage", "HealthPercentage", "OwningTeam",
        "ShooterTeamId", "LocalTeam", "ClientSoldierEntryComponent",
        "BFClientPlayerExtent", "BFPlayerExtent",
    ]
    name_hits = {}
    for nm in names:
        pat = nm.encode("ascii") + b"\0"
        positions = []
        start = 0
        while True:
            k = data.find(pat, start)
            if k < 0:
                break
            positions.append(k)
            start = k + 1
        name_hits[nm] = [{
            "file_offset": k,
            "rva": off2rva(k),
            "section": sec_of_rva(off2rva(k)) if off2rva(k) is not None else None,
        } for k in positions[:64]]

    # ---- 3) AOB 候选 ----------------------------------------------------
    def hexpat(buf):
        return " ".join("%02X" % b for b in buf)

    def count_matches(pattern_bytes, wildcards):
        """wildcards: set of byte indexes that are wildcarded (mask 0x0F-ish -> 完全通配)"""
        n = len(pattern_bytes)
        hits = []
        first = pattern_bytes[0]
        start = 0
        while True:
            k = data.find(bytes([first]), start)
            if k < 0 or k + n > len(data):
                break
            ok = True
            for i in range(n):
                if i in wildcards:
                    continue
                if data[k + i] != pattern_bytes[i]:
                    ok = False
                    break
            if ok:
                hits.append(k)
            start = k + 1
        return hits

    anchors = []

    # (a) ctr thunk 簇：RVA 0x7964095 起 10 个 lea rax,[rip+str]; ret（每条 8 字节）
    thunk_rva = 0x7964095
    off = rva2off(thunk_rva)
    if off is not None:
        raw = data[off:off + 10 * 8]
        pat = bytearray(raw)
        wild = set()
        for i in range(10):
            base = i * 8
            wild.update({base + 3, base + 4, base + 5, base + 6})
            assert raw[base:base + 3] == b"\x48\x8d\x05", raw[base:base + 3].hex()
            assert raw[base + 7] == 0xC3, raw[base + 7:base + 8].hex()
        hits = count_matches(bytes(pat), wild)
        anchors.append({
            "name": "Ctr.LocalPlayerIdThunkCluster",
            "rva": thunk_rva,
            "section": sec_of_rva(thunk_rva),
            "pattern": hexpat(pat),
            "wildcard_offsets": sorted(wild),
            "file_matches": len(hits),
            "note": "10 连 lea rax,[rip+str]; ret，指向 LocalPlayerId_* 枚举名；用作 build 指纹锚点",
        })

    # (b) .text 里对 Health 字符串的 RIP 引用点
    for tag, rva in (("Text.HealthStringRef", 0x2f94690),
                     ("Text.TeamIdStringRef", 0x2ae8be0),
                     ("Text.LocalPlayerStringRef", 0x3c1c500)):
        o = rva2off(rva)
        if o is None:
            continue
        raw = data[o:o + 16]
        wild = set()
        # 识别 lea 指令（48 8d xx / 4c 8d xx）与后续 disp32
        if raw[0] in (0x48, 0x4C) and raw[1] == 0x8D:
            wild.update({3, 4, 5, 6})
        hits = count_matches(raw, wild)
        anchors.append({
            "name": tag,
            "rva": rva,
            "section": sec_of_rva(rva),
            "pattern": hexpat(raw),
            "wildcard_offsets": sorted(wild),
            "file_matches": len(hits),
            "note": "对明文名字的 RIP 相对引用点（静态锚点）",
        })

    # (c) 反射名 ASCII 作为模式串（唯一性验证）
    for nm in ("ClientSoldierEntity", "ClientSoldierHealthComponent", "CurrentHealth",
               "MaxHealth", "CurrentTeamId", "ClientPlayerManager"):
        hits = count_matches(nm.encode("ascii"), set())
        first = hits[0] if hits else None
        rva = off2rva(first) if first is not None else None
        anchors.append({
            "name": "Reflect." + nm,
            "rva": rva,
            "section": sec_of_rva(rva) if rva is not None else None,
            "pattern": hexpat(nm.encode("ascii")),
            "wildcard_offsets": [],
            "file_matches": len(hits),
            "note": "明文反射名，运行时反射解析器的名字白名单锚点",
        })

    out = {
        "game": GAME,
        "image_base": img["image_base"],
        "sections": secs,
        "name_hits": name_hits,
        "anchors": anchors,
    }
    with open(os.path.join(OUTDIR, "p7_profile_anchors.json"), "w", encoding="utf-8") as fh:
        json.dump(out, fh, ensure_ascii=False, indent=1)

    # 控制台摘要
    print("image_base=0x%X  sections=%d" % (img["image_base"], len(secs)))
    for s in secs:
        print("  %-10s rva=0x%08X vsize=0x%08X rsize=0x%08X roff=0x%08X" % (
            s["name"], s["rva"], s["virtual_size"], s["raw_size"], s["raw_offset"]))
    print("anchors:")
    for a in anchors:
        print("  %-34s rva=0x%08X sec=%-8s matches=%d" % (
            a["name"], a["rva"] or 0, a["section"], a["file_matches"]))
    print("name hits:")
    for nm, lst in name_hits.items():
        uniq = sorted({h["section"] for h in lst})
        print("  %-32s n=%-3d sections=%s" % (nm, len(lst), ",".join(uniq)))


if __name__ == "__main__":
    main()
