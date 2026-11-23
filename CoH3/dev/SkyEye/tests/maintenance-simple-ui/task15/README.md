# Task15：当前源码的极简维护 UI

本目录提供独立的 net48/x64 WinExe，入口保留 `CoH3MemoryTool.SimpleProgram`，程序集名为 `COH3SimpleTool.Maintenance.UI.Unverified`。本次仅完成构建和离线检查：`maintenanceOnly=true`、`buildVerified=false`，GUI、授权和游戏附加均为本任务 `not_run`。

[项目](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/MaintenanceSimpleUi.csproj>)直接链接[极简生产工程](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/COH3SimpleTool.csproj>)所在目录的源文件。`ClientRoot=../../../../COH3SimpleTool` 已解析核准；本次 bin、obj、生成源、SDK/NuGet 临时目录及日志均在 task15 下。

## 与历史构建的关系

上层已有[历史构建报告](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/build-report.json>)和[历史来源清单](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/obj/maintenance-simple-ui-generated/source-manifest.json>)，其构建时间为 `2026-11-20T18:17:11.1218377Z`。[历史 EXE](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/bin/Release/net48/COH3SimpleTool.Maintenance.UI.Unverified.exe>)的 SHA256 仍为 `55283309B3F8DEEE9C89B3DFA0D4C060E611665766C72B32B18B8A2ED0D7FDE4`；历史报告 SHA256 仍为 `69C10C223C8DA8ABB6845A941F1763CFAEBCEAA0D313948AC6DCD3AE3AE49C0B`。

旧清单与当前源有三项已知差异：[AirLogic.cs](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/AirLogic.cs>)、[PlayerInfoReader.cs](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/PlayerInfoReader.cs>)及[MineOverlayWindow.cs](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/MineOverlayWindow.cs>)。本次另建 task15 目录，旧清单保持历史用途。构建前后的153项生产输入、15项上层极简历史工件、12项现有 SkyEye 维护 UI 静态工件均保持原哈希；精确路径及哈希见[构建前基线](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/baseline.json>)和[最终复核记录](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/task15-review.json>)。

## 最小复制与调用方生成

[模板差异](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/template.diff>)列出四份复制文件的全部变化：项目、生成器、离线检查脚本各增加一级源目录回退；构建脚本增加一级仓库目录回退。每份文件仅有一处相对路径字面量变化。

[生成器](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/GenerateSources.ps1>)只生成三个调用方副本，分别做一次精确替换：

| 原调用方 | 原表达式 | 生成副本中的表达式 |
|---|---|---|
| [ControlBarForm.cs:356](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs:356>) | `new PlayerInfoReader()` | `PlayerInfoReader.OpenForMaintenanceDiagnostics()` |
| [SquadMarkerWindow.cs:394](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/SquadMarkerWindow.cs:394>) | `new PlayerInfoReader(markerScene.Context)` | `PlayerInfoReader.OpenForMaintenanceDiagnostics(markerScene.Context)` |
| [AirOverlayWindow.cs:363](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/AirOverlayWindow.cs:363>) | `new PlayerInfoReader(markerScene.Context)` | `PlayerInfoReader.OpenForMaintenanceDiagnostics(markerScene.Context)` |

其余生产源原文链接，reader 内部 worker、候选指纹限制、`Matches`、租约、凭据、ProductId 和公钥 pin 原样保留。[原入口](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/SimpleProgram.cs:14>)的互斥和[SimpleLicenseApp](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/SimpleLicenseApp.cs:27>)登录链不变；控制条仍为顶部玩家信息、主画面标记和小地图标记三个开关。

项目沿用原 icon、manifest 和配置，嵌入资源恰为 `CoH3.InterfaceIcon`、`names-zh-hans.tsv`、`CoH3.BarrageCatalogue.tsv`。没有新增声音、热键、托盘或 NAudio 依赖。构建后再次逐字节复制原配置，并比较输出字节。

## 本次结果与冻结哈希

- 生成器 fixture：5/5 PASS，覆盖唯一精确替换，以及缺失、多次、额外构造器、已有维护工厂四个失败输入。
- Release 构建：0错误、15项原 Simple 源码 CS0649 警告。
- [原离线检查](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/Verify-Offline.ps1>)：153项输入哈希、三个完整生成文件、两个维护工厂、入口、资源和配置全部通过。
- [编译后元数据](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/compiled-metadata.json>)：ReflectionOnlyLoadFrom，AMD64、PE32Plus、原 SimpleProgram 入口、恰好三个资源、两项工厂签名，程序集引用中没有 NAudio。检查未调用 Main。
- [最终复核记录](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/task15-review.json>)：234项离线检查通过，包含构建前后输入与历史文件一致性、四份模板的精确差异及本次产物绑定。此数量与生成器5项 fixture 分别计。

| 本次工件 | SHA256 |
|---|---|
| [EXE](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/bin/Release/net48/COH3SimpleTool.Maintenance.UI.Unverified.exe>) | `A169466A2CCF6B37354CE04562CE31C89CA7FA4AC9941F299E545B8BC65F6563` |
| [config](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/bin/Release/net48/COH3SimpleTool.Maintenance.UI.Unverified.exe.config>) | `603BA077967B612C1E1A9B5F809AE718C5F6ADDF0DFF753BC06CC63B585CD441` |
| [构建报告](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/build-report.json>) | `0757C53414A6FFD9FF47EE502E8C464D27AEBCC346ED4FB2E92F331D07C21C03` |
| [当前来源清单](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/obj/maintenance-simple-ui-generated/source-manifest.json>) | `B8079C0E2FB068461CD2829D44253B1CFC2F9F8685176234C3BAD593F19DFB41` |

来源清单生成时间为 `2026-11-21T05:44:46.7064759Z`，构建报告时间为 `2026-11-21T05:44:48.3376201Z`。本次来源明确绑定当前飞机结构及地雷错误清理源码；旧报告没有被改写为当前源码的证明。

## 构建及后续调用

在仓库根重新构建时使用本目录的[Build.ps1](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/Build.ps1>)。以下环境设置将本次 SDK/NuGet 临时文件限定到此目录，并关闭 SDK 初次启动的欢迎、证书和工具路径动作：

```powershell
$fixture = Join-Path (Get-Location).Path 'CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15'
$env:DOTNET_CLI_HOME = Join-Path $fixture '.dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:NUGET_PACKAGES = Join-Path $fixture '.nuget/packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $fixture '.nuget/http-cache'
$env:NUGET_PLUGINS_CACHE_PATH = Join-Path $fixture '.nuget/plugin-cache'
$env:MSBUILDDISABLENODEREUSE = '1'
& (Join-Path $fixture 'Build.ps1')
```

重新构建会更新本目录的 bin、obj、报告和日志；上表仅归本次冻结构建。第一次尝试在 SDK 初始化时被 PowerShell 5.1 的空 stderr 错误处理截断，项目编译尚未开始；完成隔离 SDK 初始化后，未修改的复制脚本成功构建。最终[构建日志](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/build.log>)、[fixture日志](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/generation-self-test.log>)和[离线检查日志](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/offline-verification.log>)保留在本目录。

后续由 lead 独占客户端、授权和游戏操作。下列调用使用原 Simple 登录链及会话互斥；本任务未执行此命令：

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-simple-ui\task15\bin\Release\net48\COH3SimpleTool.Maintenance.UI.Unverified.exe
```

本次只建立构建和静态证据。激活、窗口渲染及所有权、游戏读取、切局、关闭和撤销需分别记录实际运行证据；候选布局的正式验证状态保持未验证。
