"""扫描 BF6 数据目录，查找 Frostbite 成员名（字段名）是否存在于游戏数据文件中。

用途：bf6.exe 里没有成员名（只有类型名 + 成员偏移）。
如果 Data\*.toc 或 layout.toc 里存在 CurrentHealth / MaxHealth / CurrentTeamId，
就说明成员名与字段偏移表在运行时由数据包构建 —— 那就可以离线建立 name->offset 表。

用法：python p12_data_names.py [根目录] [最大扫描字节]
"""
import os
import sys

NAMES = [
    b"CurrentHealth",
    b"MaxHealth",
    b"CurrentTeamId",
    b"ClientSoldierHealthComponent",
    b"ClientSoldierEntity",
    b"healthComponent",
    b"transform",
    b"eulerAngles",
    b"linearPosition",
]


def main() -> int:
    root = sys.argv[1] if len(sys.argv) > 1 else r"G:\SteamLibrary\steamapps\common\Battlefield 6\Data"
    budget = int(sys.argv[2]) if len(sys.argv) > 2 else (6 * 1024 * 1024 * 1024)
    print("root =", root)
    if not os.path.isdir(root):
        print("目录不存在")
        return 1
    total = 0
    hits = []
    scanned_files = 0
    skipped = []
    for dirpath, _dirs, files in os.walk(root):
        for name in files:
            path = os.path.join(dirpath, name)
            try:
                size = os.path.getsize(path)
            except OSError:
                continue
            if size > 512 * 1024 * 1024:
                skipped.append((path, size))
                continue
            if total + size > budget:
                skipped.append((path, size))
                continue
            try:
                with open(path, "rb") as handle:
                    data = handle.read()
            except OSError as exc:
                print("读取失败", path, exc)
                continue
            total += len(data)
            scanned_files += 1
            for needle in NAMES:
                count = data.count(needle)
                if count:
                    idx = data.find(needle)
                    hits.append((needle.decode(), path, count, idx))
            if scanned_files % 25 == 0:
                print("已扫描 %d 文件 / %.1f MB" % (scanned_files, total / 1048576.0))
    print("=" * 70)
    print("扫描文件数 =", scanned_files, " 读取字节 =", total)
    for needle, path, count, idx in hits:
        print("HIT  %-34s x%-4d offset=0x%X  %s" % (needle, count, idx, path))
    if not hits:
        print("没有命中任何成员名。")
    print("=" * 70)
    print("跳过（过大或超预算）文件 %d 个：" % len(skipped))
    for path, size in skipped[:40]:
        print("  %10d  %s" % (size, path))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
