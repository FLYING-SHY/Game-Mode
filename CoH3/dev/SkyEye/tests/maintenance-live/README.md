# Maintenance live reader

独立维护入口，目标框架 net48/x64。链接当前客户端完整源码、资源、NAudio 嵌入依赖和原 app.config 到同一维护程序集，定义 `MAINTENANCE_DIAGNOSTICS`。不引用已构建正式 EXE，不修改正式启动入口或项目。bin/obj 只位于本测试目录。

## 构建

从仓库根运行：

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-live\Build.ps1
```

这只是普通维护编译，不运行程序，不附加游戏，不调用保护构建或交付脚本。

## 实时采样（由维护者手动运行）

先关闭同产品正式客户端，避免同账号占用或两客户端争用会话。游戏须已由使用者正常启动；等游戏账号日志就绪，并进入受控对局。该入口本身不启动游戏。

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe `
  --out .\CoH3\dev\SkyEye\tests\maintenance-live\results\run-01 `
  --wait-seconds 120
$LASTEXITCODE
```

参数只传给这个维护 EXE，绝不传给正式 EXE。`--out` 必填，指定报告目录；同目录报告会被覆盖，因此每次使用独立子目录。`--wait-seconds` 为授权等待时间，1–600 秒，默认 120；该时间不包含采样过程。

### 正常授权与输入

- 使用默认 `LicenseCoordinator60`、真实 `LicenseApi60`、原 `ActivationForm60`；继承原配置、产品隔离、签名验签、DPAPI 凭据存储和自动登录行为。
- 已有正常保存的凭据且 AutoLogin 开启时，协调器自行加载并尝试正常登录；本入口不手动打开、导出或记录凭据文件，不从命令行接收卡密/token。
- 未保存凭据、自动登录关闭或服务要求重新登录时，需要用户在原激活窗口输入有效卡密并正常提交。`--out` 不意味着静默跳过登录。
- 等到 `LicenseGate60.Allowed` 后才开始采样。每次构造继续经过原 `LicenseGate60.Matches(pid, started)`；没有调用 Grant、注入假身份、伪造授权或解除 Matches。
- 用自有 ApplicationContext 承载原协调器与激活窗，不启动 `LicenseApplication60` 的正式 overlay，避免额外后台采样。关闭时等待协调器 CloseAsync 正常登出。

### 实际采样与结果解释

默认模式（未指定 `--event-seconds`）仅创建一个 `OpenForMaintenanceDiagnostics()` reader、读取一次 Snapshot；后续资源/单位/建筑/生产/升级/颜色共享该快照，事件与相机在后续各自查询，不是跨功能原子快照。报告记录 readerOpenAttempts / snapshotAttempts、每项开始 UTC 与耗时、本次 BuildStatus。相比旧版减少 14 次顶层 reader 构造（另有 VerifyLive58 内 1 个独立 worker）为 1 次、13 次 Snapshot（12 次显式调用 + 聚合迭代器内 1 次）为 1 次。单项异常不终止后续项；快照失败时依赖项 blocked，相机仍可独立尝试。

JSON 与文本分别写入输出目录下 `maintenance-live.json` / `maintenance-live.txt`。Evidence 字段包含资源原数值、Slot/Relation/IsLocalPlayer、本地游戏身份地址、单位路径与计数、建筑状态、生产任务 raw ticks/seconds（沿用引擎 ceil(ticks/8)）、升级路径、颜色 ARGB、相机 View/Projection 各 16 项、地图完整 Frame/仿射矩阵，以及标记/支援/弹道/空袭的本次条目与坐标。报告不记录凭据/token；游戏单位名称与本地内存地址仍按维护证据保存。

状态严格区分：
- `observed`：有读取证据，绝不是新 build verified 或画面对照通过结论；功能项不使用 pass。
- `sample_absent`：本次零条单位/生产/升级/事件，或战术地图明确未打开；不证明没有该对象，也不建立功能覆盖。
- `discovery_timeout`：有界发现未完成；保留 MinimapStatus/MinimapDiagnostic/TacticalStatus51，不直接判定布局错误。
- `fail`：异常、关键 Known=false、无有效本地玩家、授权失效/epoch 改变；`blocked`：reader 或单次快照依赖失败。已有部分 Evidence 保留。
- `not_run`：聚合 VerifyLive58 默认禁用（它重复 Snapshot 且自带 12 秒 mini 循环），避免破坏本入口的发现预算。由上面的细分查询提供证据；不得把该项当通过。
- `pass` 仅用于正常授权状态确认，不用于功能覆盖。

小地图与战术地图共同使用最多 5000ms 的轮询预算、最多 101 轮，每轮间隔最多 50ms。首次 false 会重试，不即刻 fail。两项放在最后，结束即 Dispose reader：引擎 StopMinimapDiscovery 设置 disposed/递增 epoch，后台循环在下一检查点退出；引擎单扫描另有 80ms / 32MiB 限额。此处是协作式取消；一次同步原生调用或调度延迟仍可能使实测耗时略超 5000ms，并非可强制抢占的硬实时保证。默认路径没有长期循环、持续重连或第二 reader，亦不运行 VerifyLive58 的独立 worker；可选事件窗口同样只复用这一个 reader，且另有明确预算。授权 UI 与心跳继续运行。

退出码：0 = 所有结果均 observed/pass（只表示读证据齐备）；2 = 参数错误；3 = 授权超时/用户取消/任一 fail 或 blocked；4 = 初始化、报告写入或关闭异常；5 = 无失败但包含 sample_absent/discovery_timeout/not_run/cancelled，覆盖不完整；可选事件窗口的 Ctrl+C 合作取消使用 cancelled/5，与原登录窗采样前关闭的 fail/3 区分。默认保留 VerifyLive58=not_run，因此即使其余项有样本也返回 5，绝不宣称全功能 Pass。任何维护读取均不更新 verified hash，G/R/D 仍需对局事件与画面对照。

## 离线 runner 自检（不接触游戏）

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe --self-test
```

该分支在 AudioDependency/UI/LicenseCoordinator 创建前返回；不读取凭据、不发现进程、不调用维护工厂。使用虚拟时钟测试首次 false 后成功、5 秒截止、冻结时钟轮数上限、已过期预算、最后 1ms 睡眠夹紧、零事件 sample_absent、报告序列化与退出码优先级。自检通过只证明 runner 控制逻辑，不是游戏 G/R/D。

## 可选实际 C# 弹道事件窗口

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe `
  --out .\CoH3\dev\SkyEye\tests\maintenance-live\results\event-01 `
  --wait-seconds 120 --event-seconds 30
```

`--event-seconds N` 显式取1..60；省略默认为0且不进入事件循环。显式0/负数/超过60/非数字返回usage/2，发生在音频、UI、coordinator初始化之前。正常授权流程不变，只登录一次、只构造一个维护reader；单次各功能采样之后、最后的地图查询之前，在同一reader运行该窗口，不重复登录、不另起worker。

每轮调用真实 C# `Snapshot()`，确认恰好一个自己，再绑定行颜色到marker source，调用 `ReadProjectileImpacts(initialMatch, enemyPlayers)`；只将Relation=敌方的players作为owner范围，完整保留实际reader的武器筛选、decoder、owner、source cache与sequence逻辑。没有Python谓词替代、无原始进程内存重读，也不运行离线decoder回放来充当G。初始MatchKey和LicenseGate.Epoch贯穿整个窗口；每个Snapshot/弹道调用前后都检查，变化即结束并报fail/blocked，不跨局复用。支持仍沿用默认单次查询；空袭默认单次查询，只有显式 `--air-events` 才在同一循环追加实际空袭读取（见下）。

目标是100ms起始间隔，扣除同步query耗时；慢调用不会为了追赶cadence新增reader或并发读。窗口最多N*10+1轮，有真实Stopwatch截止与独立轮数上限。预算从事件窗口起算，不包含授权等待、前面的单次采样和最后地图最多5秒；不是整个进程的硬超时。同步OS读取/Snapshot协作取消，不强行终止；报告保留overrunMilliseconds，两个读之间预算已尽则不再调用弹道读。

在有控制台时Ctrl+C合作取消：不杀进程，窗口退出、跳过可选地图发现、Dispose reader、协调器正常登出后写报告；结果cancelled，若无其他fail/blocked则exit5。取消发生在授权等待期间也不开始事件读。原登录窗采样前关闭行为不变。授权心跳仍在UI线程继续运行，未放宽Allowed/Matches/epoch约束。

报告新增 `event_window`：仅该项有真实非零返回才observed，绝不代表整体Pass；成功调用且零输出为sample_absent，不证明没有弹道或布局有效；连一次弹道调用都未开始为not_run。异常保留fail/blocked，先前有效帧作为历史数据保留，失败轮不录入decoded帧。初始reader打开失败同样blocked，无重新打开/重连。

`Evidence.polls`记录每轮UTC、起止毫秒、状态、decodedCount；`frames`记录非零返回时序和原始**decoded public method输出**：Entity/Id/Index/Slot/SourceKey/Number/Name/Weapon/Label/ARGB/X/Y/Z/Radius。这不是raw memory bytes。重复entity可跨轮多次出现，decodedObservationCount不是唯一炮弹数。每个非零poll最多留16项、总128帧/256项；DroppedFrames/DroppedDecoded精确记录裁剪。默认报告上限1MiB，启用窗口时4MiB；保留列表硬限额不随N扩大。输出仍为既有JSON/TXT，默认命名不变。

### 可选 all-sides 空袭诊断

在上面的命令追加 `--air-events`（默认 false；需同时指定 `--event-seconds 1..60`，否则初始化前退出2）。不带该参数的既有事件输出结构不变。此开关只扩展维护读取，不改变引擎、授权或生产 UI。

每轮沿用同一个实际 Snapshot/BindMarkerColours 结果；先保留原敌方弹道读取，再于预算和取消检查后调用实际 C# `ReadAirZones(initialMatch, Players(all current rows))`，包括自己、友方与敌方。目标是让使用者受控 LOCAL US 空袭留下真实结构证据，而不是声称生产 enemy-only 空袭 UI 已验证。前后逐次检查最初固定 MatchKey/LicenseEpoch/Allowed；任何变化终止该窗口，早前帧只作为历史记录。两个同步查询间取消/到期会跳过空袭查询；同步查询仍为合作式预算。

启用时 `event_window.Evidence` 分成 `projectile`（原证据对象、原计数）及独立 `air`；每轮 `projectile.polls` 额外记录可空 `airDecodedCount`（未调用或调用失效为 null）。`air` 单列 scope/status/queryAttempts/nonzeroPolls/decodedObservationCount、保留与丢弃计数和 frames。成功空袭查询按有界策略保留：固定预留2个空帧位置，只留首个空帧与最新空帧（只有一次空读时只留1帧），最新空帧替换时计入 droppedFrames；其余最多126帧留给最早的正帧，避免长前导空读挤掉后续真实结构。帧按读取时间顺序排列。每帧最多16项，总预算仍为128帧/256项；正帧超容量时整帧计入 droppedFrames、全部条目计入 droppedDecoded；正帧内裁剪同样精确记入 droppedDecodedInThisPoll 与 droppedDecoded。decoded预算耗尽后的已保留正帧仍保留 decodedCount/时间、decoded为空。成功读取次数 = retainedFrames + droppedFrames，decodedObservationCount = retainedDecoded + droppedDecoded；空帧替换只增加帧丢弃数，不增加decoded丢弃数。上限不随时长增加，原JSON字段保持兼容。保留 Key/Owner/Offset/Slot/Name/X/Y/Z/DX/DZ/HalfWidth/Length/Radius/Rocket/Point，与 UTC/起止毫秒/match/epoch；非 raw memory。新增 Key、Owner、Offset 仅在 --air-events 的 air.frames[].decoded 中原样记录 ReadAirZones 返回值：Key 用于后续实测区分 flight: fallback 与 cast key，Owner 保留读取器输出的归属值，Offset 保留原始几何偏移；runner 不重建或推断这些值。重复区域跨轮重复计数，不是唯一空袭数。零空袭只为 sample_absent，未开始为 not_run，异常窗口为 invalidated；绝不把零事件标成 PASS。聚合 SampleCount/observed 可来自任一流，空袭结论须单看 air，不能从弹道 observed 推导空袭覆盖。

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe --air-event-self-test
```

此独立分支在音频/UI/coordinator之前返回，18项纯合成测试覆盖 enemy-only 与 all-current-rows 路由、551轮中的400前导空帧→83正帧→68后导空帧、首/最新空帧替换和时序、126正帧与晚到空帧、每帧16/总256项裁剪、混合淘汰、原结构序列化，以及600轮每次Add后的计数恒等式与上限。551轮fixture应保留85帧/83条，droppedFrames=466、droppedDecoded=0；该fixture并非真实对局回放。合成数据仅验证runner，不证明真实空袭读取；此修复只构建并运行离线测试，未启动游戏、授权UI或读进程。

### 新窗口离线测试（不重复原20项）

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe --event-self-test
```

此独立分支同样在音频/UI/协调器创建前返回。16项虚拟时钟/数据自检覆盖1..60解析、100ms cadence、冻结时钟轮数限额、取消前后、慢读超预算、剩余1ms、between-read stop、异常同对象传播、零帧、两种保留上限、decoded与时序序列化、cancelled退出5。未重跑原20个self-tests；未实际登录、附加或运行游戏。这些测试仅F，真正G事件由维护者后续正常授权运行留证。

### 可选同实例恢复证据

`--recovery-probe` 须与 `--event-seconds 1..60` 同用；默认关闭。仅当真实 event_window 查询抛出 ReadMineCamera 源码的六种明确镜头诊断消息时，记录原 reader 对象、原 match/lease epoch、UTC 和异常，再由原 Probe 继续把 event_window 记为 fail。随后独立的 `same_reader_recovery` 最多执行3轮：每轮直接 ReadMineCamera，成功后才 Snapshot；第一组均成功即停止。总合作预算2000ms，失败后的间隔最多100ms；同步读取可超时，实耗和 OverrunMilliseconds 单列。没有自然镜头诊断时明确 `not_triggered`，不读镜头或 Snapshot，不合成错误、不反射或修改 reader/内存。

`same_reader_recovery.Evidence` 记录 DirectCameraAttempts、SnapshotAttempts、逐次 StartedUtc/FinishedUtc/起止毫秒、异常、镜头 View/Projection 各16项及成功 Snapshot 的本地 Slot/Relation/IsLocalPlayer/identity。ReferenceEquals 比较原对象，hash 只是显示标签，readerOpenAttempts 必须保持1；每次调用（包含抛异常的调用）前后都对照原 match/epoch/Allowed。MatchKey 本身是缓存守卫，完整成功 Snapshot 另执行引擎的实时世界/本地身份一致性检查。Snapshot 内部经 LocalIdentity58 再读镜头，不计入 DirectCameraAttempts。顶层 snapshotAttempts 会包含这些新增显式 Snapshot 调用。

原 event_window fail、air invalidated、历史保留帧和 ExitCodeFor 均保持原值：恢复成功仍 exit3。地图结果继续单列，不计为镜头恢复；这个维护探针也不覆盖生产 GUI 的恢复分支。恢复并非完整版本或画面验收。

```powershell
# 仅由维护者按正常入口运行；自然错误未出现时应为 not_triggered。
& .\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe `
  --out .\CoH3\dev\SkyEye\tests\maintenance-live\results\run-recovery `
  --event-seconds 60 --air-events --recovery-probe

# 直接在初始化音频/UI/coordinator之前运行纯合成自检。
& .\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe --recovery-self-test
```

另有不链接任何 engine/coordinator/UI 的 net48 隔离工程，只链接 RecoveryProbe、EventPrewarm、原 BoundedPoll/LiveProbeResult 及纯 fixture：

```powershell
& .\LicensingSystem\.cache\dotnet-10\dotnet.exe build .\CoH3\dev\SkyEye\tests\maintenance-live\recovery-fixture\RecoveryProbeFixture.csproj -c Release -v:minimal
& .\CoH3\dev\SkyEye\tests\maintenance-live\recovery-fixture\bin\Release\net48\RecoveryProbeFixture.exe
```

33项虚拟时钟/合成 delegate 测试验证：失败后 fake map 不产生镜头证据、同 fake reader 后续 camera/Snapshot 成功、三轮硬上限、异常日志、原 fail/air invalidated/exit3 保留、调用前后 match/epoch/Allowed/对象变化、取消、合作预算和超时记录，以及下述预热的解析/取消/时钟冻结/剩余1ms。fixture 不验证真实引擎缓存重发现或真实 GUI，也没有向真实 reader 注入异常。BuildStatus 与 decoded layout 等实际引擎验证仍由原自检及维护者采证负责。

### 可选 event 预热延迟

`--event-prewarm-delay-ms 0..5000` 默认0；非零值须与 `--event-seconds 1..60` 同用。执行顺序为初始 air query → 可选预热 → event_window → 可选同实例恢复 → maps。原 event match/epoch 在初始 air query 前固定，后续等待、event 和 recovery 共用；预热失败或取消会记录 blocked/fail/cancelled 并跳过全部 event 查询。初始 air 结果仍独立记录，runner 不推断它一定为空或一定存在事件。

预热以最多50ms的 sleep 协作取消，最多101轮；开始/结束各用现有 IsCurrentMatch55 做实时世界与本地身份校验，等候循环仅检查缓存 MatchKey 和 Allowed/epoch。IsCurrentMatch55 自身读取镜头，结果记录 liveMatchChecks，取消时不追加结束镜头读；这些预热守卫不是恢复探针证据。开始前成功校验后打印并 Flush `event_prewarm_delay started milliseconds=4000 utc=...`，结束打印 outcome/实耗/UTC，独立 Evidence 保留 requestedMilliseconds/startedUtc/finishedUtc/elapsedMilliseconds/initialMatch/initialEpoch/liveMatchChecks。该入口不启动游戏、不提供游戏输入或事件生成器。

```powershell
& .\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe `
  --out .\CoH3\dev\SkyEye\tests\maintenance-live\results\run-prewarm `
  --event-seconds 60 --air-events --recovery-probe --event-prewarm-delay-ms 4000
```
