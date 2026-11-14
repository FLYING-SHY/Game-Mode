# Game-Mode

## 当前布局（2026-11-13 重构后：授权系统独立 + 一个游戏一个目录）

- `LicensingSystem/`：通用授权系统本体（所有游戏修改器共用），按 ProductId 隔离多产品。
  - `server/`：net10.0 / ASP.NET Core / SQLite 的在线授权服务与管理后台源码。
  - `server-tests/`：引用同树 server 的隔离后端测试（`Build.ps1 -VerifyServer` 共 120 项）。
  - `deployment/`：Get-DotNet10.ps1、New-DeploymentConfig.ps1、Caddyfile。
  - `.cache/dotnet-10/`：必要的 .NET SDK 10 安装（含安装证明），不能按普通下载缓存删除；`.cache/packages/` 为 server/server-tests 的 NuGet 还原目标（`Directory.Build.props` 指定），保留离线构建所需内容。
  - `.local/hardening-v2/`：当前私有配置、数据库、签名私钥和 Data Protection keyring。
  - `.local/` 内其他非测试授权数据/密钥与 retained-private-state：历史私有数据，保留原有用途，不进入交付包。
  - `Build.ps1` / `Start-Server.ps1` / `Initialize-Signing.ps1` / `Publish-Server.ps1`：授权系统入口。
  - `delivery/server/`：服务端发布目录（Server.zip 打包根）。
- `CoH3/SkyEye/`：天穹之眼当前唯一维护源码与发布工程（授权接入编译内嵌于客户端）。
  - `client/`：net48 / x64 / WinForms 客户端，包含签名租约验签、公钥 pin（`PinnedLeaseKey60.cs`）与运行资源。
  - `verification/`：引用同树 client 的客户端验证工程；受保护验证器在 `bin/Protected/net48`。
  - `delivery/client/`：当前可分发客户目录。
  - `tools/obfuscar/`、`tools/ilspy/`：当前保护及测量工具。Reactor 试用工具和 EXE 已清理，仅保留实验记录与可配置脚本。
  - `evidence/`、`reports/`、`artifacts/`：当前版本的测试记录、报告和私有开发映射。
  - `Build.ps1`（客户端+验证工程）/ `Build-Protected.ps1` / `Verify-Protected.ps1` / `Finalize.py` / `Summarize-Validation.py` 等为游戏侧入口。
- `CoH3/天穹之眼-v0.60.8/`：原始发布文件。保持原样，不作为开发输出目录。
- `CoH3/CoH3Minimap/`：独立小地图项目，未修改。
- `docs/agents/`：Agent 工作流配置。

旧 CoH3FowTool、SkyEye-v0.60.8-Recovered、SkyEye-Integration、SkyEye-Hardened 的代码及旧交付包已清理；`Licensing/` 已更名 `LicensingSystem/`，`SkyEye-HardenedV2` 已更名 `CoH3/SkyEye`。不应从 Git 恢复旧名来补最新版依赖；确需旧记录时先查清理清单、重构清单及 Git 历史。

## 新增一个游戏

新建 `CoH3/<游戏名>/`（客户端 + 授权接入内嵌编译 + 该游戏交付产物），并在授权系统注册对应 ProductId（授权码哈希空间 `{productId}|code-v1|`、设备指纹前缀 `{productId}|device-v1|`）。授权系统不引入具体游戏耦合。

## 构建与验证

从仓库根目录运行：

```powershell
& .\LicensingSystem\Build.ps1 -VerifyServer          # 服务端 + 120 测试
& .\CoH3\SkyEye\Build.ps1                            # 客户端 + 验证工程
& .\CoH3\SkyEye\Build-Protected.ps1 -SelfCheck       # 混淆保护 + 自检
& .\CoH3\SkyEye\Verify-Protected.ps1 -LocalOnly      # 三组离线回归
& .\LicensingSystem\Publish-Server.ps1 -NoBuild      # 复现服务端发布目录
```

联机七组回归：`Verify-Protected.ps1 -PrivateCodeDirectory <私有码目录>`（先经 `scripts/prepare_live_cards.py` 签发隔离卡）。脚本使用 `LicensingSystem/.cache/dotnet-10/dotnet.exe`；Windows 客户端构建还需本机 .NET Framework 4.8 引用程序集。

默认签名服务端口为 `47835`。现有客户端与当前签名私钥配对；更换私钥必须重新 pin（`LicensingSystem/Initialize-Signing.ps1 -PinFile`）、构建、保护并分发客户端。不要更改业务 ProductId 或恢复旧的无签名响应降级。

## 文件与发布约束

- 不提交或推送，除非用户明确要求。
- 保持 `.local` 中令牌、卡密、数据库、签名私钥和 Data Protection keyring 私密；发布包不得包含这些内容。
- 客户包不放源码、开发映射、验证器或 PDB；维护包含开发映射（含 LicensingSystem 授权源码），仅供维护者。
- `CoH3/SkyEye/Finalize.py --refresh` 仅在存在已验证的清理清单与重构清单时刷新当前包；客户 ZIP 须保持逐项一致。
- 文本采用 UTF-8；Windows PowerShell 5.1 可执行项目脚本。
- 原版与独立小地图项目不作为天穹之眼构建/清理的输出目标。
- REA 完整环境位于 `F:\WorkSpace\REA`；Windows capture-process 有已记录的 capability_unavailable，静态工具成功不等于动态测量成功。

## 工作流

Issues 使用 GitHub `FLYING-SHY/Game-Mode`，经 gh CLI 操作，见 `docs/agents/issue-tracker.md`。五种 triage 标签为 needs-triage、needs-info、ready-for-agent、ready-for-human、wontfix，见 `docs/agents/triage-labels.md`。领域模型使用根 GLOSSARY.md 与 docs/adr/，见 `docs/agents/domain.md`。
