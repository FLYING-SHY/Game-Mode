# -*- coding: utf-8 -*-
"""p8_field_names.py — 从 .rdata 提取"像字段名"的标识符清单（按关键字分组）。
用途：为档案 Structs 与运行时反射解析器准备候选字段名。
输出：analysis/p8_field_names.json
"""
import json
import os
import re
import struct

GAME = r"G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe"
OUTDIR = r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis"

GROUPS = {
    "health": r"health|hp$|damage|alive|dead|revive|downed|manDown|kill",
    "team": r"team|ally|enemy|neutral|squad|attacker|defender|objective",
    "position": r"position|worldPos|translation|transform|location|coords",
    "name": r"^name$|playerName|soldierName|clanTag|displayName|username",
    "local": r"local|self|own|controlled",
    "entity": r"entity|instance|actor|player|soldier|character",
    "matrix": r"matrix|viewProj|projection|viewMatrix|viewmatrix|worldToScreen|camera",
    "id": r"(^|[a-z])[Ii]d$|index|handle|pointer|ptr$",
    "list": r"list|array|collection|manager|registry|pool",
}


def main():
    with open(GAME, "rb") as fh:
        data = fh.read()
    e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
    coff = e_lfanew + 4
    nsec = struct.unpack_from("<H", data, coff + 2)[0]
    opt_size = struct.unpack_from("<H", data, coff + 16)[0]
    opt = coff + 20
    image_base = struct.unpack_from("<Q", data, opt + 24)[0]
    sec_off = opt + opt_size
    sections = []
    for i in range(nsec):
        o = sec_off + i * 40
        name = data[o:o + 8].split(b"\0")[0].decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, o + 8)
        sections.append((name, vaddr, vsize, rsize, roff))

    rdata = None
    for s in sections:
        if s[0] == ".rdata":
            rdata = data[s[4]:s[4] + s[3]]
            break
    ident = re.compile(rb"[A-Za-z_$][A-Za-z0-9_$]{2,63}")
    names = set()
    for m in ident.finditer(rdata):
        try:
            names.add(m.group(0).decode("ascii"))
        except Exception:
            pass

    result = {"image_base": image_base, "total": len(names), "groups": {}}
    for group, pattern in GROUPS.items():
        rx = re.compile(pattern, re.IGNORECASE)
        hits = sorted(n for n in names if rx.search(n))
        result["groups"][group] = hits[:400]
    # 只含字母数字的短名（更像字段）
    result["field_like"] = sorted(n for n in names if 3 <= len(n) <= 28 and n[0].islower() and re.match(r"^[a-z][A-Za-z0-9]{2,27}$", n))[:4000]

    with open(os.path.join(OUTDIR, "p8_field_names.json"), "w", encoding="utf-8") as fh:
        json.dump(result, fh, ensure_ascii=False, indent=1)

    for group in GROUPS:
        hits = result["groups"][group]
        print("== %s (%d) ==" % (group, len(hits)))
        print(", ".join(hits[:60]))


if __name__ == "__main__":
    main()
