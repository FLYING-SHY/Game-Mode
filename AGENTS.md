# Game-Mode

单仓库，两条产品线：

- `CoH3FowTool/` — Company of Heroes 3 本地显示工具（GUI + 授权接入）。.NET 9 / `net9.0-windows` / WinForms / x64。
- `Licensing/` — **通用授权系统**（所有游戏修改器共用）。.NET 9 / ASP.NET Core Minimal API / SQLite + Keygen CE。

远程仓库：`git@github.com:FLYING-SHY/Game-Mode.git`

## 项目结构

```
CoH3FowTool/            # 产品：CoH3 战争先锋修改器
  src/                  # 主程序源码。CoH3FowTool.csproj 显式编译 src\*.cs 与 src\Gui\*.cs
    Program.cs 命令行入口、DesktopApplication.cs 控制台宿主、RuntimeState.cs 运行时状态
    Gui/MainForm.cs、Gui/GuiContracts.cs — WinForms 界面
    PatchCatalog.cs 六处显示补丁目录、ProcessMemory.cs 进程内存读写、PeImage.cs PE 解析
    GameInput.cs 输入、FowDisplay.cs 显示状态、SteamLaunchOptions.cs 启动参数
  tests/                # Desktop、ProtectedClient 两个测试项目
  tools/                # 逆向与打包工具：reverse/（Python 静态/动态分析脚本与报告）、bundlereader、input-test、sigprobe
  build/                # 混淆与产物校验：ProtectAssemblies.ps1、ProtectedBundle.targets（由 csproj 导入）、Verify-Bundle.ps1、verify_bundle.py
  docs/                 # 产品级设计文档：licensing-gui-spec.md
Licensing/              # 通用授权系统（被所有修改器复用）
  client/               # 客户端 SDK：namespace Licensing；程序集 Licensing.Client
  server/               # 授权服务端 + 管理后台（admin.html/js/css）：程序集 Licensing.Server
  tests/                # 真实 Keygen CE 集成测试：程序集 Licensing.Tests
  deployment/           # 本地编排：compose.yaml、Initialize-Local.ps1、Start-Local.ps1
  docs/                 # 授权系统设计文档：licensing-models-research.md
docs/agents/            # Agent 工作流配置（issue tracker / triage / domain）
```

## 授权系统的多产品模型

- 每个游戏修改器在授权系统里有一个 **产品 id**（`ProductId`），默认 `CoH3FowTool`（`Licensing/client/ProductIds.cs`）。
- 产品 id 决定三件事：**授权码哈希空间**（`{productId}|code-v1|`）、**设备指纹前缀**（`{productId}|device-v1|`）、**Keygen 策略**（每产品 permanent/timed 两条 policy）。
- 服务端配置 `Licensing/.local/server.json` 用 `Products[]` 声明产品目录，`DefaultProductId` 指定默认产品；旧版单产品配置（顶层 `PermanentPolicyId`/`TimedPolicyId`）仍然兼容读取。
- 客户端把 `ProductId` 放进激活/校验请求体，并写进签名证书的 `product` metadata（旧的 `coh3App`/`coh3Kind` 键继续兼容）。
- 新增一个修改器 = 在 `server.json` 增加一条 `Products` 项 + 在 Keygen 建对应 product/policy，客户端侧只需设置 `LicenseClientOptions.ProductId`。

## 构建

- 修改器开发构建：`powershell -NoProfile -ExecutionPolicy Bypass -File .\CoH3FowTool\build.ps1` → `CoH3FowTool\bin\Release\net9.0-windows\CoH3FowTool.exe`（本机未安装 `pwsh`，Windows PowerShell 5.1 可直接跑）
- 修改器发布构建（Obfuscar 2.2.50 混淆 + 单文件）：`powershell -NoProfile -ExecutionPolicy Bypass -File .\CoH3FowTool\build.ps1 -Publish -PublicConfiguration <config.json>` → `CoH3FowTool\publish-gui\CoH3FowTool.exe`，构建证据写入 `CoH3FowTool\artifacts\gui-build\<stamp>\build-report.json`
- 授权系统：`dotnet build .\Licensing\tests\Licensing.Tests.csproj -c Release`；本地编排用 `.\Licensing\deployment\Initialize-Local.ps1` + `.\Licensing\deployment\Start-Local.ps1`
- `Licensing/` 有独立的 `Directory.Build.props` 与自有 `.gitignore`

## 注意事项

- `bin/ obj/ dist/ publish/ publish-gui/ artifacts/ Licensing/.local/ Licensing/.cache/ tools/**/out-*/ tools/reverse/*.png` 不入库（见 `.gitignore`）
- `README.md` / `README-2.0.md` 为 UTF-8 无 BOM；GBK 代码页控制台直接读取会乱码
- 授权系统独立演进：修改 `Licensing/` 时不得引入具体游戏的产品耦合，产品维度只经由 `ProductId` 表达

## Agent skills

### Issue tracker

Issues are tracked in GitHub Issues for `FLYING-SHY/Game-Mode` (all operations via the `gh` CLI). See `docs/agents/issue-tracker.md`.

### Triage labels

The five canonical triage roles use the default label strings `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout: one `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
