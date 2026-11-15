# Game-Mode

## 当前布局（2026-11-15 重构后：授权系统独立 + 每游戏 original/dev/output 三区）

- 顶层：`AGENTS.md` / `README.md` / `.gitignore` / `docs/agents/`（Agent 工作流配置）。
- `LicensingSystem/`：通用授权系统本体（所有游戏共用），按 ProductId 隔离多产品。
  - `server/`：net10.0 / ASP.NET Core / SQLite 的在线授权服务与管理后台源码。
  - `server-tests/`：引用同树 server 的隔离后端测试（`Build.ps1 -VerifyServer` 共 120 项）。
  - `deployment/`：Get-DotNet10.ps1、New-DeploymentConfig.ps1、Caddyfile。
  - `.cache/dotnet-10/`：必要的 .NET SDK 10 安装（含安装证明），不能按普通下载缓存删除；`.cache/packages/` 为 server/server-tests 的 NuGet 还原目标（`Directory.Build.props` 指定），保留离线构建所需内容。
  - `.local/hardening-v2/`：当前私有配置、数据库、签名私钥和 Data Protection keyring。`.local/` 内其他非测试授权数据/密钥与 retained-private-state 为历史私有数据，保留原有用途，不进入交付包。
  - `Build.ps1` / `Start-Server.ps1` / `Initialize-Signing.ps1` / `Publish-Server.ps1`：授权系统入口。
  - `output/`：服务端发布目录（`Publish-Server.ps1` 产出，Server.zip 打包根）。
- `CoH3/`：英雄连3。每个游戏目录固定三区，新游戏照抄此模板：
  - `original/`：别人的原版，冻结区。铁律：`**/original/**` 下永不写入、构建、清理。保护栈四层：① 全量清单 `CoH3/original/MANIFEST.sha256`；② 仓库根 `Verify-Original.ps1` 漂移校验；③ pre-commit 钩子拒绝 `**/original/**` 的一切 staged 变更（白名单 `original/MANIFEST.sha256`）；④ 全文件只读属性。
    - `original/天穹之眼-v0.60.8/`：原始发布文件，保持原样。
  - `dev/`：自己开发的一切，一个方向（一条可独立发布的工具线）一个子目录：
    - `dev/SkyEye/`：天穹之眼当前唯一维护源码与发布工程（授权接入编译内嵌于客户端）。`client/`（net48/x64/WinForms，签名租约验签、公钥 pin `PinnedLeaseKey60.cs`）、`verification/`（受保护验证器在 `bin/Protected/net48`）、`delivery/client/`（Finalize.py 的解包打包 staging，dev 内唯一保留的 delivery）、`tools/obfuscar|ilspy`、`evidence/`、`reports/`、`artifacts/`、`Build.ps1` / `Build-Protected.ps1` / `Verify-Protected.ps1` / `Finalize.py` / `Summarize-Validation.py` 等入口。
    - `dev/COH3SimpleTool/`：SkyEye 引擎的极简黑壳版（登录页 + 三开关控制条；无热键/声音/设置面板；ProductId 仍为 `CoH3FowTool`，凭据文件与 SkyEye 共享）。引擎文件复制自 SkyEye 客户端而非引用；入口 `Build-Protected.ps1 -Smoke`，交付直写 `output/COH3SimpleTool/`。
    - `dev/CoH3Minimap/`：自有小地图项目，休眠中。
  - `output/`：与 dev/ 一一对应的最终交付物（平铺、直接可用；文件名带 -Client/-Maintenance 区分客户/维护包）：
    - `output/SkyEye/`：`SkyEye-HardenedV2-{Client,Server,Maintenance}.zip` + `SkyEye-HardenedV2.archive-verification.json`。
    - `output/COH3SimpleTool/`：受保护 EXE + `.exe.config` + `overlay-settings.txt`。
    - `output/CoH3Minimap/`：首次出物前留 `.gitkeep`。
- 历史清理：旧 CoH3FowTool、SkyEye-v0.60.8-Recovered、SkyEye-Integration、SkyEye-Hardened 的代码及旧交付包已清理；`Licensing/` 已更名 `LicensingSystem/`；`SkyEye-HardenedV2` → `CoH3/SkyEye`（2026-11-13）→ `CoH3/dev/SkyEye`（2026-11-15）。不应从 Git 恢复旧名来补最新版依赖；确需旧记录时先查清理清单、重构清单（`CoH3/dev/SkyEye/evidence/`）及 Git 历史。

## 新增一个游戏

新建 `<Game>/` 目录：`original/`（别人的原版，落位后立刻生成 MANIFEST 并设只读）、`dev/<方向>/`（每条可独立发布的工具线一个子目录，客户端 + 授权接入内嵌编译）、`output/<方向>/`（与 dev 一一对应的交付物），并在授权系统注册对应 ProductId（授权码哈希空间 `{productId}|code-v1|`、设备指纹前缀 `{productId}|device-v1|`）。授权系统不引入具体游戏耦合。

## 构建与验证

从仓库根目录运行（顺序固定；Build-Protected 必须先于 Verify-Protected，后者读 `evidence/latest-build.json` 内的绝对路径）：

```powershell
& .\LicensingSystem\Build.ps1 -VerifyServer              # 服务端 + 120 测试
& .\CoH3\dev\SkyEye\Build.ps1                            # 客户端 + 验证工程
& .\CoH3\dev\SkyEye\Build-Protected.ps1 -SelfCheck       # 混淆保护 + 自检
& .\CoH3\dev\SkyEye\Verify-Protected.ps1 -LocalOnly      # 三组离线回归
& .\LicensingSystem\Publish-Server.ps1 -NoBuild          # 复现服务端发布目录（LicensingSystem/output）
& .\CoH3\dev\COH3SimpleTool\Build-Protected.ps1 -Smoke   # 简易工具保护版直写 output
```

联机七组回归：`Verify-Protected.ps1 -PrivateCodeDirectory <私有码目录>`（先经 `scripts/prepare_live_cards.py` 签发隔离卡）。脚本使用 `LicensingSystem/.cache/dotnet-10/dotnet.exe`；Windows 客户端构建还需本机 .NET Framework 4.8 引用程序集。

默认签名服务端口为 `47835`。现有客户端与当前签名私钥配对；更换私钥必须重新 pin（`LicensingSystem/Initialize-Signing.ps1 -PinFile`，默认指向 `CoH3/dev/SkyEye/client/CoH3MemoryTool/PinnedLeaseKey60.cs`）、构建、保护并分发客户端。不要更改业务 ProductId 或恢复旧的无签名响应降级。

## 文件与发布约束

- 不提交或推送，除非用户明确要求。
- 保持 `.local` 中令牌、卡密、数据库、签名私钥和 Data Protection keyring 私密；发布包不得包含这些内容。
- 客户包不放源码、开发映射、验证器或 PDB；维护包含开发映射（含 LicensingSystem 授权源码），仅供维护者。
- `CoH3/dev/SkyEye/Finalize.py --refresh` 仅在存在已验证的清理清单与重构清单时刷新当前包；客户 ZIP 须保持逐项一致。
- 文本采用 UTF-8；Windows PowerShell 5.1 可执行项目脚本。
- `**/original/**` 冻结区永不写入、构建、清理（见上保护栈）；`dev/CoH3Minimap` 休眠项目不作为任何构建/清理目标。
- REA 完整环境位于 `F:\WorkSpace\REA`；Windows capture-process 有已记录的 capability_unavailable，静态工具成功不等于动态测量成功。

## 工作流

Issues 使用 GitHub `FLYING-SHY/Game-Mode`，经 gh CLI 操作，见 `docs/agents/issue-tracker.md`。五种 triage 标签为 needs-triage、needs-info、ready-for-agent、ready-for-human、wontfix，见 `docs/agents/triage-labels.md`。领域模型使用根 GLOSSARY.md 与 docs/adr/，见 `docs/agents/domain.md`。
