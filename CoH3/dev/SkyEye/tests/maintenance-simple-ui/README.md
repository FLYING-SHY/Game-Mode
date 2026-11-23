# COH3SimpleTool maintenance-only UI fixture

独立 net48/x64 WinExe，名称 `COH3SimpleTool.Maintenance.UI.Unverified`，仅定义维护编译符号 `MAINTENANCE_DIAGNOSTICS`。不引用已构建的正式EXE、不修改正式项目，也不运行保护或交付构建；bin/obj、生成源与报告全在本目录。候选布局仍未获得正式build验证。

## 构建和离线验证

从仓库根执行：

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-simple-ui\Build.ps1
```

脚本先用Windows PowerShell 5.1运行生成器5项fixture（精确替换成功；缺失、多次、额外构造器、已有维护工厂全部失败），再调用仓库现有.NET 10 SDK编译，最后用ReflectionOnlyLoadFrom检查元数据，绝不执行Main。日志：`generation-self-test.log`、`build.log`、`offline-verification.log`；报告：`build-report.json`，生成来源清单：`obj/maintenance-simple-ui-generated/source-manifest.json`。

核验原入口、两个维护工厂、3个原资源逻辑名称、原App.config逐字节相同、全部原C#源码及原项目/config/icon/manifest/资源哈希构建前后不变；每个生成文件必须严格等于原文件做唯一一次已知表达式替换后的全文。没有新增环境变量开关、授权分支、假身份或Grant。

## 原始调用链与替换

正式Simple入口是 `CoH3/dev/COH3SimpleTool/CoH3MemoryTool/SimpleProgram.cs:27`：`SimpleProgram -> SimpleLicenseApp -> 原LicenseCoordinator60 + BlackLoginForm`。这不是SkyEye的`LicenseApplication60/ActivationForm60`；保留Simple自己的正常黑壳登录、自动登录、真实API、验签、心跳、Matches、revoke/close和原mutex。未修改ProductId、pin或凭据处理，不读取私有配置目录或密钥。

仅将以下3个正式源文件的**副本**生成到本目录obj：

| 原文件（均在CoH3/dev/COH3SimpleTool/CoH3MemoryTool） | 正式reader调用点 | 生成副本的唯一替换 |
|---|---|---|
| ControlBarForm.cs:356 | `new PlayerInfoReader()` | `PlayerInfoReader.OpenForMaintenanceDiagnostics()` |
| SquadMarkerWindow.cs:394 | `new PlayerInfoReader(markerScene.Context)` | `PlayerInfoReader.OpenForMaintenanceDiagnostics(markerScene.Context)` |
| AirOverlayWindow.cs:363 | `new PlayerInfoReader(markerScene.Context)` | `PlayerInfoReader.OpenForMaintenanceDiagnostics(markerScene.Context)` |

`Adaptation58.cs:14`的备用调用、`PlayerInfoReader.cs:283`的内部worker和`:2304/:2309`的工厂内部构造原样链接。reader内部完全未替换；每个窗口上下文参数原样传递。其余Simple源代码直接链接原文件，排除原bin/obj及`*Checks*.cs`。

链接Simple原app.ico、app.manifest与3个原资源：`CoH3.InterfaceIcon`、`names-zh-hans.tsv`、`CoH3.BarrageCatalogue.tsv`。Simple原工程没有NAudio引用或音频资源，本工程也不人为添加SkyEye音频依赖。MSBuild可能重排config XML，因此AfterBuild再次原字节复制原app.config，随后比较SHA256。

## 本次离线结果

- 生成器fixture：5/5 PASS。
- Release构建：0错误、15项原Simple源码CS0649警告（未初始化字段），未新增抑制项。
- Reflection-only核验全部通过：原SimpleProgram入口、2个维护工厂、3个资源、153个原输入哈希未变、原config逐字节相同、3个生成副本各仅替换1次。
- 应用、授权、游戏附加均not_run；此次产物SHA256见build-report.json。

## 后续手动启动（本任务未启动）

先关闭同产品正式/其他维护客户端以免共享账号会话占用；此入口仍需正常登录，游戏由操作者自行启动。

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-simple-ui\bin\Release\net48\COH3SimpleTool.Maintenance.UI.Unverified.exe
```

这是原黑壳UI，不是新的报告窗口；不接受授权旁路参数。离线构建只建立S/F证据。实际激活、窗口所有权/渲染、游戏附加、切局、关闭/撤销等G/R/D均为not_run，由lead后续独立验证。正式源码、原版区与output区均无写入。
