# Game-Mode

仓库按「一个授权系统 + 一个游戏一个目录」组织：授权系统代码独立在根目录 `LicensingSystem/`，每个游戏一个目录放各自的客户端、接入与交付产物。2026-11-13 完成旧代码清理与布局重构：恢复阶段、初次授权接入和第一轮加固的旧代码与发布包已删除；授权目录从 `Licensing/` 更名为 `LicensingSystem/`，游戏工程从 `SkyEye-HardenedV2` 更名为 `CoH3/SkyEye`。

| 目录 | 用途 |
| --- | --- |
| [原版 v0.60.8](<CoH3/天穹之眼-v0.60.8/>) | 原始发布文件，保持不变 |
| [SkyEye（天穹之眼最新版）](<CoH3/SkyEye/README.md>) | 客户端源码、授权接入（编译内嵌）、验证工程、构建与交付脚本 |
| [SkyEye 客户端交付](<CoH3/SkyEye/delivery/client/>) | 签名授权 + Obfuscar 的可运行客户目录 |
| [LicensingSystem](<LicensingSystem/README.md>) | 通用授权系统：服务端、测试、部署辅助、SDK 与本机私有状态 |
| [授权服务端交付](<LicensingSystem/delivery/server/>) | ASP.NET Core 10 签名服务发布目录 |
| [CoH3Minimap](<CoH3/CoH3Minimap/README.md>) | 独立小地图项目，保留其现有内容 |
| [工作流配置](<docs/agents/>) | Issue、triage 与 domain 工作流 |

## 最新交付包（CoH3 根）

- [客户包](<CoH3/SkyEye-HardenedV2-Client.zip>)：客户程序与运行资源。
- [服务端包](<CoH3/SkyEye-HardenedV2-Server.zip>)：签名服务程序及部署辅助（打包根 `LicensingSystem/delivery/server`）。
- [维护包](<CoH3/SkyEye-HardenedV2-Maintenance.zip>)：最新版源码（含授权系统）、开发映射、构建脚本与验证记录，仅供维护者保存。
- [包验证记录](<CoH3/SkyEye-HardenedV2.archive-verification.json>)：当前 ZIP 的哈希和逐项验证结果。

三个包均排除本机管理员令牌、真实卡密、私钥和数据库。维护包不包含 Reactor 试用程序。

## 构建与启动

从仓库根目录执行：

```powershell
# 编译授权服务端与测试，运行全部 120 项隔离测试
& .\LicensingSystem\Build.ps1 -VerifyServer

# 编译游戏客户端 + 验证工程
& .\CoH3\SkyEye\Build.ps1

# 重新生成受保护客户端并自检
& .\CoH3\SkyEye\Build-Protected.ps1 -SelfCheck

# 使用现有签名密钥和数据库启动本机授权服务
& .\LicensingSystem\Start-Server.ps1 -ConfigPath .\LicensingSystem\.local\hardening-v2\server.json
```

本机管理入口为 `http://127.0.0.1:47835/admin`。旧的 47831 测试服务已停止。SDK 位于 `LicensingSystem/.cache/dotnet-10`；客户端编译还需 .NET Framework 4.8 引用程序集。

部署新机器、配对公钥和备份私有状态，见[SkyEye 说明](<CoH3/SkyEye/README.md>)、[LicensingSystem 说明](<LicensingSystem/README.md>)与[签名协议](<CoH3/SkyEye/signature-contract.md>)。现有客户 EXE 与现有签名私钥配对；更换私钥后须重新固定公钥并构建、分发客户端。新增一个游戏 = 新建一个游戏目录（客户端 + 授权接入内嵌编译）+ 在授权系统注册对应 ProductId。

## 清理与重构记录

[删除清单](<CoH3/SkyEye/evidence/cleanup-manifest.json>)记录了清理目标、删除前旧基线校验及原版/最新版保留哈希；[重构清单](<CoH3/SkyEye/evidence/refactor-manifest.json>)记录了本次目录移动的前缀映射与校验。原有授权记录、配置和密钥保留在 `LicensingSystem/.local/`；历史备份中独有的私有状态已单独保留，旧源码备份已移除。Git 历史与独立小地图项目保留。

远程仓库：`git@github.com:FLYING-SHY/Game-Mode.git`。清理与重构没有提交或推送。
