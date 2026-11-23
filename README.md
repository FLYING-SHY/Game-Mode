# Game-Mode

仓库按「一个授权系统 + 一个游戏一个目录」组织。2026-11-15 起每个游戏目录内固定三区：`original/`（别人的原版，冻结）、`dev/`（自己开发的方向，一个方向一个子目录）、`output/`（与 dev 一一对应的最终交付物）。更早历史：2026-11-13 完成旧代码清理与首轮布局重构，`Licensing/` 更名 `LicensingSystem/`，`SkyEye-HardenedV2` 更名 `CoH3/SkyEye`。

| 目录 | 用途 |
| --- | --- |
| [原版 v0.60.8](<CoH3/original/天穹之眼-v0.60.8/>) | 原始发布文件，冻结区（MANIFEST 校验 + 只读） |
| [dev/SkyEye（天穹之眼）](<CoH3/dev/SkyEye/README.md>) | 唯一维护源码：客户端、授权接入（编译内嵌）、验证工程、构建/保护/封装脚本与证据 |
| [dev/COH3工具](<CoH3/dev/COH3SimpleTool/README.md>) | SkyEye 引擎的极简黑壳版：登录页 + 三开关控制条，无热键无声音，卡密与 SkyEye 通用 |
| [output/SkyEye](<CoH3/output/SkyEye/>) | SkyEye 最终交付：Client / Server / Maintenance ZIP + 包验证记录 |
| [output/COH3SimpleTool](<CoH3/output/COH3SimpleTool/>) | COH3工具受保护 EXE + .exe.config + overlay-settings.txt |
| [BF6/original/Battlefield6-Steam](<BF6/original/Battlefield6-Steam/>) | 战地 6 构建身份元数据，冻结区（游戏本体不入库） |
| [dev/Bf6Esp（战地 6）](<BF6/dev/Bf6Esp/docs/USER-GUIDE.md>) | 战地 6 外部信息工具：授权接入（ProductId `Bf6Esp`）、只读逆向探针、构建脚本与配置档 |
| [output/Bf6Esp](<BF6/output/Bf6Esp/>) | Bf6Esp 交付：EXE + .exe.config + BUILD-INFO.json + overlay-settings.txt |
| [LicensingSystem](<LicensingSystem/README.md>) | 通用授权系统：服务端、测试、部署辅助、SDK 与本机私有状态 |
| [授权服务端发布](<LicensingSystem/output/>) | ASP.NET Core 10 签名服务发布目录（Publish-Server.ps1 产出，Server.zip 打包根） |
| [工作流配置](<docs/agents/>) | Issue、triage 与 domain 工作流 |

## 最新交付包（CoH3/output/SkyEye/）

- [客户包](<CoH3/output/SkyEye/SkyEye-HardenedV2-Client.zip>)：客户程序与运行资源。
- [服务端包](<CoH3/output/SkyEye/SkyEye-HardenedV2-Server.zip>)：签名服务程序及部署辅助（打包根 `LicensingSystem/output`）。
- [维护包](<CoH3/output/SkyEye/SkyEye-HardenedV2-Maintenance.zip>)：最新版源码（含授权系统）、开发映射、构建脚本与验证记录，仅供维护者保存。
- [包验证记录](<CoH3/output/SkyEye/SkyEye-HardenedV2.archive-verification.json>)：当前 ZIP 的哈希和逐项验证结果。

三个包均排除本机管理员令牌、真实卡密、私钥和数据库。维护包不包含 Reactor 试用程序。

## 构建与启动

从仓库根目录执行（顺序固定；Verify-Protected 依赖上一步刷新的 `evidence/latest-build.json`）：

```powershell
# 编译授权服务端与测试，运行全部 120 项隔离测试
& .\LicensingSystem\Build.ps1 -VerifyServer

# 编译游戏客户端 + 验证工程
& .\CoH3\dev\SkyEye\Build.ps1

# 重新生成受保护客户端并自检
& .\CoH3\dev\SkyEye\Build-Protected.ps1 -SelfCheck

# 三组离线回归
& .\CoH3\dev\SkyEye\Verify-Protected.ps1 -LocalOnly

# 复现服务端发布目录（LicensingSystem/output）
& .\LicensingSystem\Publish-Server.ps1 -NoBuild

# 构建 COH3工具受保护版并冒烟（直写 CoH3/output/COH3SimpleTool）
& .\CoH3\dev\COH3SimpleTool\Build-Protected.ps1 -Smoke

# 使用现有签名密钥和数据库启动本机授权服务
& .\LicensingSystem\Start-Server.ps1 -ConfigPath .\LicensingSystem\.local\hardening-v2\server.json
```

线上管理入口为 `https://124.220.147.56/admin`（生产实例已部署，2026-11-16 起）；本机管理入口为 `http://127.0.0.1:47835/admin`。旧的 47831 测试服务已停止。SDK 位于 `LicensingSystem/.cache/dotnet-10`；客户端编译还需 .NET Framework 4.8 引用程序集。

部署新机器、配对公钥和备份私有状态，见[SkyEye 说明](<CoH3/dev/SkyEye/README.md>)、[LicensingSystem 说明](<LicensingSystem/README.md>)与[签名协议](<CoH3/dev/SkyEye/signature-contract.md>)。现有客户 EXE 与现有签名私钥配对；更换私钥后须重新固定公钥并构建、分发客户端。

## original/ 冻结区保护

`**/original/` 下是别人的原版发布文件，永不写入、构建或清理：

- 每游戏一份全量清单 `CoH3/original/MANIFEST.sha256`、`BF6/original/MANIFEST.sha256`；仓库根 [Verify-Original.ps1](<Verify-Original.ps1>) 自动遍历所有 `<Game>/original` 冻结区校验漂移。
- git pre-commit 钩子拒绝 staged 的 `**/original/**` 一切变更（白名单 `original/MANIFEST.sha256` 自身与新增清单）。
- `original/**` 全文件只读属性；解锁一条命令：`Get-ChildItem CoH3\original,BF6\original -Recurse -File | ForEach-Object { $_.IsReadOnly = $false }`。

## 清理与重构记录

[删除清单](<CoH3/dev/SkyEye/evidence/cleanup-manifest.json>)记录了清理目标、删除前旧基线校验及原版/最新版保留哈希；[2026-11-13 重构清单](<CoH3/dev/SkyEye/evidence/refactor-manifest.json>)与[2026-11-15 重构清单](<CoH3/dev/SkyEye/evidence/refactor-manifest-2026-11-15.json>)记录历次目录移动的前缀映射与校验。原有授权记录、配置和密钥保留在 `LicensingSystem/.local/`；历史备份中独有的私有状态已单独保留，旧源码备份已移除。Git 历史保留（CoH3Minimap 独立小地图项目已于 2026-11-15 整体删除，需要时从 Git 历史找回）。

远程仓库：`git@github.com:FLYING-SHY/Game-Mode.git`。历次清理与重构成果在 2026-11-23 一次性提交并推送。

新增一个游戏 = `<Game>/original/`（别人的原版）+ `<Game>/dev/<方向>/`（每条可独立发布的工具线一个子目录）+ `<Game>/output/<方向>/`（一一对应交付物）+ 在授权系统注册对应 ProductId。
