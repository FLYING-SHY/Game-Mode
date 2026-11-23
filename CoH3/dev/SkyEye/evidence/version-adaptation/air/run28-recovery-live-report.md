# run28：自然镜头异常后的同 reader 恢复实证

**结论：维护 reader 的一次真实自然镜头异常后，同一对象完成了直接 ReadMineCamera 与 Snapshot 恢复。恢复探针记录 1 轮、camera 1 次、Snapshot 1 次、6ms，原事件窗口继续保留 fail、air invalidated 和 runner/OS exit3。** 这是维护 reader 恢复的有限正例；生产 GUI 的实际重试与重连仍另待验证。

独立核验仅读取既有证据、相关源码和已冻结 runner，新增本报告、[核验脚本](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/validate_run28_recovery.py>)及[核验 JSON](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28-recovery-validation.json>)三个限定文件。核验过程中未启动游戏、GUI、授权会话、构建或 live 采样，未修改 core、runner 或 acceptance-current。本文时间均为 UTC；原生屏幕为同一轮离线遭遇战，非注入式 fixture。

## 输入与完整性

- [原始 reader 报告](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-28-camera-recovery/maintenance-live.json>)：48,669 bytes，SHA256 `F9E5E343BCBF1E22F2B7B36A87D8C2D1DFDFD752915661D5BE81DBA825101AA6`。
- [driver identity](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28-driver-identity.json>)：SHA256 `C3D56ECB0AF24FBD0088C304C4D7800BCE1B57A5AA8AE116AB9CDD7C9E403B5D`。
- [driver result](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28-driver-result.json>)：SHA256 `A46079559E5AB81405139D8FEF2E672B8F906E574826F8835ED9CB184CC1058B`。
- [driver 源码](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28_camera_driver.ps1>)：SHA256 `7E1D25B60BB82709C2CBE254C50A9CF89328692D050984899CFBB6EB9B002E85`。
- [runner](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/bin/Release/net48/SkyEye.Maintenance.Live.exe>)：driver 启动前、结束后及本次离线复核均为 `CD125BFFA33E525CD4B94A753D65A6E4CBB199395E3860E57E85003DB7665F79`。
- driver 记录游戏 PID `31572`，启动时间 `2026-11-20T18:44:29.8492330Z`；记录游戏 SHA256 `8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC`。本次核验沿用该记录，未重新访问运行中游戏的可执行文件。
- 核验 JSON 逐项列出 27 个输入的 SHA256、字节数与绝对路径：上述报告/driver、stdout/stderr、输入助手、恢复及调度源码、核心 reader、runner 和 10 张原生截图。stdout 为 GB18030；原始字节先做 SHA 绑定，再按该编码核对控制台结果，stderr 为 0 bytes。

本次 run 的选项为：

```text
--event-seconds 60 --air-events --recovery-probe --event-prewarm-delay-ms 1000
```

报告仍为 `maintenanceOnly=true / buildVerifiedByThisRun=false / visualVerification=not_run`，各有值的 BuildStatus 均为 `FingerprintMismatch`。这些字段及整体版本、保护交付状态保持独立。

## 原事件窗口及计数

[原始报告](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-28-camera-recovery/maintenance-live.json>)记录真实 lease active、`readerOpenAttempts=1`。1000ms prewarm 的起止 live match 检查各一次，随后开始请求 60 秒的事件窗口。

| 计数位置 | Snapshot 尝试数 | 本轮结果 |
|---|---:|---|
| 初始资源 Snapshot | 1 | observed，2 行 |
| event 第 1–97 轮 | 97 | Snapshot 后的 projectile 与 air 查询均完成 |
| event 第 98 轮 | 1 | Snapshot 内部自然抛出“镜头已切换。”；该轮未产生 projectile/air 查询结果 |
| 后续独立恢复探针 | 1 | 同 reader Snapshot observed，2 行 |
| 总计 | **100** | **1 + 98 + 1** |

事件 ledger 有连续 ordinal `1..98`；前 97 条 `completed`，最后一条 `incomplete`、两个 decodedCount 均为 null。`eventSnapshotAttempts=98`，`projectileQueryAttempts=air.queryAttempts=97`。每条 UTC 与单调计时均按序，整份 ledger 的 UTC 相对单调时钟最大偏差为 `1.198ms`。

实际窗口计时为 `10586ms`、overrun `0`、stopReason `exception`，早于请求的 `60000ms`。23 条 projectile 解码观察保留为历史记录，非 23 个独立投射物。air 的 97 次已完成查询均为零，保留 first/latest 两个空帧、替换 95 个空帧，最终 air 状态是 `invalidated`。独立恢复结果成功后，`event_window.Status=fail` 和 reader/OS `exitCode=3` 均未被擦除。

## 触发与恢复时序

异常栈包含 `ReadMineCamera → LocalIdentity58 → Snapshot → SampleEvents → EventPolling`，恢复 TriggerError 是 event Error 追加外层 Probe 栈之前的同一异常文本。第 98 个 poll 的开始时间为 `03:08:59.3349766Z`。

| 事件 | UTC | 恢复探针单调计时 |
|---|---|---:|
| 自然异常捕获 / TriggeredUtc | `2026-11-21T03:08:59.3355359Z` | 尚未启动恢复计时 |
| 独立恢复 feature 开始 | `2026-11-21T03:08:59.3413529Z` | 约 0ms |
| 直接 ReadMineCamera 开始 | `2026-11-21T03:08:59.3438609Z` | 2ms |
| 直接 ReadMineCamera 返回 | `2026-11-21T03:08:59.3438609Z` | 2ms |
| Snapshot 开始 | `2026-11-21T03:08:59.3438609Z` | 2ms |
| Snapshot 返回 | `2026-11-21T03:08:59.3478715Z` | 6ms |
| 后续地图探针开始 | `2026-11-21T03:08:59.3493951Z` | 独立阶段 |

`ElapsedMilliseconds=6` 是恢复探针自身记录；异常捕获至首次直接 camera 开始为 `8.325ms`，至 Snapshot 返回为 `12.3356ms`。Snapshot 的 UTC 差为 `4.0106ms`。camera 两端时间相同，表示留存时钟分辨率下的 0ms，执行时间精度以该记录为限。

最大配置为 3 轮、合作式 2000ms 预算；本次第一轮成功即停止，`StopReason=camera_and_snapshot_observed`、`OverrunMilliseconds=0`。`DirectCameraAttempts=1` 仅统计探针显式调用：初始 camera、prewarm live checks 及 Snapshot 内部的 camera 读取不在该计数中。

## 同对象、match、lease 与身份守卫

触发标签为 `020266e5`。恢复起止、camera 前后、Snapshot 前后共六组上下文完全一致：

```text
ReaderIdentity = 020266e5
SameReader = true
ReaderOpenAttempts = 1
Match = 31572:639308258698492330:2188107579392:2188149555200:1000
Epoch = 1
Allowed = true
Valid = true
```

**identity hash 只是标签。** [RecoveryProbe.cs:114](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/RecoveryProbe.cs:114>)实际使用 `ReferenceEquals(current.Reader, trigger.Reader)`；Valid 同时要求触发与当前 open count 均为 1、原 Match 非空且一致、Allowed 和原 epoch 一致。六个序列化的 SameReader/Valid 记录按这段逻辑解释，未用 hash 相等替代对象同一性。[RecoverySampling.cs:62](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/RecoverySampling.cs:62>)将同一个 reader 传入上下文及两个读取 delegate。

本次恢复 Snapshot 的本方行：

```text
rowCount = 2
Slot = 0
Relation = 自己
IsLocalPlayer = true
playerAddress = 1fd77e08000
identity = 3e8
```

该行与初始资源 Snapshot 的本方 slot、relation、IsLocalPlayer、identity 一致。地址 `0x1fd77e08000` 等于 MatchKey 第四段十进制 `2188149555200`；身份低 32 位 `0x3e8=1000` 对应最后一段，高 32 位 `0` 对应 slot0。MatchKey 的 process/startTicks 与 driver 的 PID、启动 UTC 加 08:00 后本地 DateTime ticks 一致。

[PlayerInfoReader.cs:2768](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2768>)的 Snapshot 还校验游戏进程/启动时间、实时本地身份、world 和玩家列表，并在结束前复核后提交 MatchKey。缓存 MatchKey guard 与这次成功 Snapshot 的实时检查分别记录；单凭缓存字符串相等的含义较窄。

## 返回矩阵与源码路径

camera 返回原始 reader 顺序的两个有限 float 数组，各 16 项；下列每四项换行仅为阅读，未转换行列约定。

```text
View = [
  -6.39171844e-08, 0.681999564, 0.731352568, 0,
  1.88543368e-08, 0.731352568, -0.681999564, 0,
  -1, -1.88543368e-08, -6.39171844e-08, 0,
  -84.53733, 82.75858, 141.987061, 1
]
Projection = [
  0.974278569, 0, 0, 0,
  0, 1.7320509, 0, 0,
  0, 0, 1.00000107, 1,
  0, 0, -0.1000001, 0
]
```

核验检查全部 32 个值为有限数、数组长度为 16、`View[15]≈1`、`Projection[0/5]>0`、`Projection[11]≈1`。

[PlayerInfoReader.cs:1194](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:1194>)在读取矩阵后发现 `Q(cameraCore + 416) != num6` 时抛出此次诊断消息，catch 将 `cameraCore=0L` 后继续抛出；下一次调用可从 [PlayerInfoReader.cs:1129](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:1129>)重新枚举 camera。上述缓存路径是源码解释，留存报告没有 camera 指针前后原始值。

[EventSampling.cs:90](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/EventSampling.cs:90>)保存自然异常的原 reader/match/epoch 后继续 throw；[LiveSampling.cs:168](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/LiveSampling.cs:168>)将原 feature 标成 fail。恢复由 [RecoverySampling.cs:58](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/RecoverySampling.cs:58>)记录为独立 feature；[BoundedPoll.cs:28](<F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/BoundedPoll.cs:28>)仍因保留的 fail 返回 exit3。reader 只打开一次，统一 Dispose 位于恢复与地图之后。

## 原生输入与地图的独立位置

[driver](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28_camera_driver.ps1:42>)等待真实 prewarm 标记后，以 [前台检查输入助手](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/game_input.py>)发送空格恢复及原生小地图左键；记录八次坐标为 `(140,900)、(260,930)、(220,800)、(90,820)` 重复两遍。截图均为 `1936×1119`，随输入一并保留并由哈希绑定。

直接检查 [pan01](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28-native-pan-01.png>)、[pan06](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28-native-pan-06.png>)、[pan08](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28-native-pan-08.png>)可见不同主场景和小地图视锥，HUD 分别为 `12:17 / 12:25 / 12:28`；[最终暂停图](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28-native-paused.png>)为 `12:31`，显示“战术暂停已启用”。driver 记录恢复 `03:08:48.6772939Z`、暂停 `03:09:05.2781387Z`，时间差 `16.6008448s`，与截图推进相符。

异常处于第六张截图完成 `03:08:58.3790125Z` 与第七张完成 `03:08:59.9887676Z` 之间；截图完成时间没有逐次鼠标 down/up 的精确日志，因此具体触发点击保留为待定。

地图查询直到恢复 Snapshot 已返回才开始，耗时共享 `5015ms`；minimap observed、tactical sample_absent。该阶段既未替代直接 camera 的矩阵，也未替代恢复 Snapshot 的本方身份。最终报告时间 `03:09:04.5065267Z` 晚于这些查询，正常暂停再晚于报告；driver 记录 `childKilledAtTimeout=false`、实际子进程 exit3，未靠超时终止取得结果。

## 可重复核验与交付状态

[核验脚本](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/validate_run28_recovery.py>)初次执行以 exclusive `x` 模式创建 [核验 JSON](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/run28-recovery-validation.json>)，已有输出会提前报错。此次完成 **87 项 run28 离线审计检查**；这是该脚本的检查数，与此前维护 runner 的 87 项自测分别计。

只读复跑命令（已执行通过）：

```powershell
& 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe' -B -X utf8 `
  'F:\WorkSpace\Game\CoH3\dev\SkyEye\evidence\version-adaptation\air\validate_run28_recovery.py' --check
```

`--check` 从输入重算，再与既有 JSON 全对象比较，完全一致；也可通过 `runpy.run_path` 取得 `validate(root)`，其中 root 是 version-adaptation 目录，函数本身只读且无写入。命令采用 `-B`，不生成 pycache。

另执行了 13 组纯内存负例：Snapshot 总数错、末 poll 状态错、query 数错、同 hash 但 SameReader=false、Allowed=false、矩阵 NaN、本方地址错、原 event fail 被覆盖、camera Called=false、地图时序提前、camera UTC 逆序、尝试上限错，以及输入字节 SHA 漂移，全部被严格校验检出。该实验仅在当前 Python 内存中替换解析结果或读入 bytes，未改原始文件。再次调用默认创建入口时，exclusive 输出保护生效，输出 SHA 保持不变；最后 `validate(root)` 仍与既有 JSON 完全一致。

本证据接受的范围是 `readerRecoveryAccepted=true`。`originalEventWindowAccepted / productionGuiRecoveryAccepted / gameFeatureAccepted / protectedDeliveryAccepted` 全部为 false。当前源码哈希、runner 哈希与留存输入已绑定；本次未重建或做二进制到源码的重新证明。GUI 重试、其他事件类型、第二次全新游戏进程、同进程跨局及最终保护交付继续由各自实际证据决定。
