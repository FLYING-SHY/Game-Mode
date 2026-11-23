# SkyEye Maintenance UI — UNVERIFIED / 仅维护

此独立测试项目产出 `bin/Release/net48/SkyEye.Maintenance.UI.Unverified.exe`，不替换正式客户端、不写 output、不新增 verified hash。候选布局仅供维护观察；本项目的构建或 fixture 通过都不是实际游戏功能/交付通过。

## 构建（不启动）

从仓库根运行：

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-ui\Build.ps1
```

只调用仓库已有 .NET10 SDK、Windows PowerShell5.1 和独立项目；bin/obj/日志/报告全部留在本目录。没有 ProjectReference，不重建正式工程或保护工程。

构建先运行5个生成 fixture，然后生成3个调用方文件，编译，再运行 reflection-only 元数据检查。最终 `build-report.json` 与 `obj/maintenance-ui-generated/source-manifest.json` 记录维护标记、入口、产物 SHA256、原文件/生成文件 SHA256、3次精确替换、资源逻辑名、配置一致性；日志为 `build.log`、`generation-self-test.log`、`offline-verification.log`。

## 仅3处维护调用方替换

所有源文件链接自当前 client。仅下列3个文件排除原始 Compile 项，并在每次编译前生成到本项目 obj：

| 文件 | 唯一替换 |
|---|---|
| OverlaySettingsForm.cs | `new PlayerInfoReader()` → `PlayerInfoReader.OpenForMaintenanceDiagnostics()` |
| SquadMarkerWindow.cs | `new PlayerInfoReader(markerScene.Context)` → `PlayerInfoReader.OpenForMaintenanceDiagnostics(markerScene.Context)` |
| AirOverlayWindow.cs | 同上 |

每个文件要求恰好1个预期原始表达式且仅1个 reader 构造；缺失、多处、新增其他构造或已存在维护工厂均使构建失败。无全树替换，PlayerInfoReader及内部 worker/普通构造/候选hash限制/Matches授权判断保持链接原文。默认正式工程未改；环境变量没有维护开关。

原始 `ControlsProgram` 是 EXE入口，仍执行 DPI、音频和原始单实例互斥、LicenseApplication60/LicenseCoordinator60及原始激活页。没有新的 UI封装、伪造许可、命令行凭据或读写凭据的新增代码。公共 pin、ProductId、服务端地址、租约验签/心跳/撤销/关窗逻辑均沿用当前 client。App.config 最终显式复制原始文件，避免 SDK对XML格式化或增补造成字节差异；离线检查逐字节hash相等，不打印配置或凭据内容。

四个资源保持精确逻辑名：`CoH3.InterfaceIcon`、`names-zh-hans.tsv`、`CoH3.BarrageCatalogue.tsv`、`CoH3.Audio.NAudio.dll`。sounds复制到本项目产物，NAudio仍为原始嵌入装载方案。

## 维护者后续启动（本次未执行）

关闭正在运行的正式/其他维护客户端后，由 lead 自行启动上述 EXE。原始互斥和共享凭据/会话语义保留，勿并行启动多个产品客户端；实际激活、窗口绘制、热键、关闭/撤销与游戏读链应单独留证。候选必须匹配引擎限定的候选hash，普通 reader创建仍默认门控。

本次只构建并检查元数据，未执行 EXE、Main、登录、进程发现、游戏附加或UI渲染。实际窗口、生命周期、动画/移动与跨局恢复均 `not_run`。`MAINTENANCE_DIAGNOSTICS` 可用只说明此独立编译显式包含维护代码，并不意味着未知任意build被正式支持。
