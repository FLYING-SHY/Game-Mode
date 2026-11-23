# Battlefield 6 — Steam 本地安装（冻结参考）

本目录不存放游戏本体（162 GB 过大），只存放**构建身份**：安装路径、文件清单、关键二进制哈希。
`dev/` 下所有偏移、签名、结构结论都必须绑定本文件的 `BUILD-ID`。

## BUILD-ID

`BF6S-1.0.0.0-1F385AD9`

## 采集环境

| 项 | 值 |
|---|---|
| 采集时间 | 2026-11-21T22:38+08:00 |
| 安装根 | `G:\SteamLibrary\steamapps\common\Battlefield 6` |
| 平台 | Steam (win32-x64, Windows) |
| 文件总数 | 615 |
| 总字节 | 173,979,269,896 (162.03 GB) |

## 主模块

| 项 | 值 |
|---|---|
| 路径 | `G:\SteamLibrary\steamapps\common\Battlefield 6\bf6.exe` |
| 大小 | 199,230,312 |
| SHA-256 | `1F385AD9AF30B8BA7F0C87CEEE92182710BB58F98708A2DE7F8D119042BAEFF4` |
| FileVersion | 1, 0, 0, 0 |
| ProductVersion | 1, 0, 0, 0 |
| Company | EA Digital Illusions CE AB |
| 描述 | Battlefield™ 6 |
| 架构 | PE32+ / AMD64 (machine 0x8664) |
| 节数 | 13 |

## 引擎与运行面

- 引擎：Frostbite（`Data\Win32\*.toc` 超级捆绑、`initfs_Win32`、`layout.toc`、`oo2core_9_win64.dll` Oodle）。
- 渲染：DX12，附带 `Engine.Render.Core2.PlatformPcDx12.retail.dll`、DLSS/XeSS/FSR 三套升频器与 `dstorage`（DirectStorage）。
- 反作弊：`EAAntiCheat` 目录 + `EAAntiCheat.GameServiceLauncher.exe` + `EAJavelinInstaller_installscript.vdf` → EA Javelin（内核级）。

## 子目录

| 目录 | 内容 |
|---|---|
| `Data\Win32\` | 超级捆绑索引 `weapons.toc` `globals.toc` `characters.toc` `vehicles.toc` `ui.toc` 与 `game/` `installation/` `SP/` `loc/` |
| `x64\` | `D3D12Core.dll` |
| `SP\` | 单人战役独立 `bf6.exe` 与同一套渲染 DLL |
| `EAAntiCheat\` | 反作弊安装包与运行时 |
| `Scripts\` | 安装/运行脚本 |
| `__Installer\` | 分发元数据 |
| `Update\` | 更新暂存 |

## 校验

```powershell
# 关键二进制漂移检查（411 项）
Get-Content .\MANIFEST.sha256 | ForEach-Object {
  $h,$rel = $_ -split '  ',2
  $actual = (Get-FileHash ("G:\SteamLibrary\steamapps\common\Battlefield 6\" + $rel) -Algorithm SHA256).Hash.ToLower()
  if ($actual -ne $h) { "DRIFT $rel" }
}
```

`MANIFEST.sha256` 覆盖全部 `.exe` `.dll` `.toc` `.cfg` `.digest` `.vdf` `.png`、`initfs*`、`layout.toc`、`chunkmanifest`；
`FILES.tsv` 覆盖全部 615 个文件的相对路径 / 字节数 / UTC 修改时间。
