# Game-Mode

CoH3FowTool —— Company of Heroes 3 本地显示工具（GUI + 授权）。.NET 9 / `net9.0-windows` / WinForms / x64。
远程仓库：`git@github.com:FLYING-SHY/Game-Mode.git`

## 项目结构

- `src/` — 主程序源码。`CoH3FowTool.csproj` 显式编译 `src\*.cs` 与 `src\Gui\*.cs`。
  - `Program.cs` 命令行入口、`DesktopApplication.cs` 控制台宿主、`RuntimeState.cs` 运行时状态
  - `Gui/MainForm.cs`、`Gui/GuiContracts.cs` — WinForms 界面
  - `PatchCatalog.cs` 六处显示补丁目录、`ProcessMemory.cs` 进程内存读写、`PeImage.cs` PE 解析
  - `GameInput.cs` 输入、`FowDisplay.cs` 显示状态、`SteamLaunchOptions.cs` 启动参数
- `licensing/` — 授权子系统：`client` / `server` / `tests` + `deployment` 本地编排。密钥、DB、token 在 `licensing/.local`，**不入库**。
- `tests/` — `Desktop`、`ProtectedClient` 两个测试项目。
- `tools/` — 逆向与打包工具：`reverse/`（Python 静态/动态分析脚本与报告）、`bundlereader`、`input-test`、`sigprobe`。
- `build/` — 混淆与产物校验：`ProtectAssemblies.ps1`、`ProtectedBundle.targets`（由 csproj 导入）、`Verify-Bundle.ps1`、`verify_bundle.py`。
- `docs/` — 设计文档：`licensing-gui-spec.md`、`licensing-models-research.md`。

## 构建

- 开发构建：`pwsh -File .\build.ps1` → `bin\Release\net9.0-windows\CoH3FowTool.exe`
- 发布构建（Obfuscar 2.2.50 混淆 + 单文件）：`pwsh -File .\build.ps1 -Publish -PublicConfiguration <config.json>` → `publish-gui\CoH3FowTool.exe`，构建证据写入 `artifacts\gui-build\<stamp>\build-report.json`
- `licensing/` 有独立的 `Directory.Build.props` 与自有 `.gitignore`

## 注意事项

- `bin/ obj/ dist/ publish/ publish-gui/ artifacts/ licensing/.local/ licensing/.cache/ tools/**/out-*/ tools/reverse/*.png` 不入库（见 `.gitignore`）
- `README.md` / `README-2.0.md` 为 UTF-8 无 BOM；GBK 代码页控制台直接读取会乱码

## Agent skills

### Issue tracker

Issues are tracked in GitHub Issues for `FLYING-SHY/Game-Mode` (all operations via the `gh` CLI). See `docs/agents/issue-tracker.md`.

### Triage labels

The five canonical triage roles use the default label strings `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout: one `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
