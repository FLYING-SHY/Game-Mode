# Maintenance 同 reader 恢复诊断交接（2026-11-21）

本次实现与离线验证已完成，源码及维护 EXE 冻结。改动全部位于 tests/maintenance-live；未改产品 engine / GUI / output，未启动游戏、GUI、授权或真实采样。实际同 reader camera/Snapshot 恢复仍需由后续真实记录证明，离线 fixture 只验证诊断控制流及证据隔离。

## 入口

维护 EXE：`F:\WorkSpace\Game\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe`

SHA256：`CD125BFFA33E525CD4B94A753D65A6E4CBB199395E3860E57E85003DB7665F79`

在既有正常维护命令上附加：

```text
--event-seconds 60 --air-events --recovery-probe --event-prewarm-delay-ms 4000
```

`--recovery-probe` 默认关闭，仅配合 1..60 秒事件窗口；`--event-prewarm-delay-ms` 默认 0，接受 0..5000，正值要求事件窗口。没有改正常授权入口。独立 `--recovery-self-test` 在所有应用初始化之前返回。

## 自然异常后的证据

- [EventSampling.cs:90](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/EventSampling.cs:90) 捕获自然事件异常；仅源 camera 六条诊断消息触发可选恢复，保存同 reader 引用及原事件 match/epoch，再原样 throw。未注入异常。
- [LiveSampling.cs:115](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/LiveSampling.cs:115) 在初始 air 查询之前固定事件 match/epoch，恢复探针位于事件之后、maps 之前，reader 构造/Dispose 没有变化。
- [RecoveryProbe.cs:10](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/RecoveryProbe.cs:10) 纯 delegate 核心：最多 3 轮、合作式 2000ms 预算，每轮显式 ReadMineCamera 成功后才调用 Snapshot，第一对成功即止；失败间隔最多 100ms。同步读可超时，记录 overrun，不额外开启下一轮。
- [RecoveryProbe.cs:85](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/RecoveryProbe.cs:85) 每次调用前后记录守卫，即使抛错也记录；[RecoveryProbe.cs:114](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/RecoveryProbe.cs:114) 要求 ReferenceEquals 同对象、原始/当前 readerOpenAttempts 都为 1、非空原 match 一致、原 lease epoch 一致且 Allowed。identity hash 仅为标记。
- [RecoverySampling.cs:58](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/RecoverySampling.cs:58) 独立 same_reader_recovery 结果记录触发错误/UTC、每次 camera 矩阵与 Snapshot 本地身份、计数、耗时、停止理由、前后上下文。Snapshot 自己的内部 camera 调用另不计入 directCameraAttempts。
- 成功恢复不会修改原 event_window fail、历史 air invalidated 或 exit3；[BoundedPoll.cs:21](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/BoundedPoll.cs:21) 聚合规则没有改变。maps 独立，永不充作 camera/Snapshot 恢复证据；生产 GUI 重试行为没有在本实现中执行。
- 没有自然 camera 异常时，same_reader_recovery 明确为 not_triggered，零恢复调用。

## 可选 prewarm

[RecoverySampling.cs:10](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/RecoverySampling.cs:10) 在初始 air 之后、事件窗口之前等待；起止各用 IsCurrentMatch55 做一次真实 world/local identity 守卫，其内部 camera 调用明确不是恢复证据；中间使用最多 50ms 等待切片和原 match/epoch/Allowed 守卫。取消、前后真实守卫或中间上下文失败时不启动事件查询。[EventPrewarm.cs:10](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/EventPrewarm.cs:10) 还限制等待循环最多 101 轮，冻结时钟会显式失败。

初始真实守卫成功后重启延时计时器，再输出并 Flush：

```text
event_prewarm_delay started milliseconds=4000 utc=...
event_prewarm_delay finished outcome=elapsed elapsedMilliseconds=... utc=...
```

记录 requested/start/end/elapsed/liveMatchChecks/outcome，起止都沿用初始 air 之前的事件 match/epoch。

## 离线验证

完整维护 runner Build.ps1：0 warning / 0 error。四个早退分支全部通过：

| 分支 | 检查数 | 日志 |
|---|---:|---|
| --self-test | 20 | [self-test.log](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/self-test.log) |
| --event-self-test | 16 | [event-self-test.log](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/event-self-test.log) |
| --air-event-self-test | 18 | [air-event-self-test.log](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/air-event-self-test.log) |
| --recovery-self-test | 33 | [recovery-self-test.log](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/recovery-self-test.log) |

共 87 项。另用 [RecoveryProbeFixture.csproj](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/recovery-fixture/RecoveryProbeFixture.csproj) 独立构建，无 engine/UI/授权项目引用，同 33 项再次通过；该次日志为 [fixture-self-test.log](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/recovery-fixture/fixture-self-test.log)。fixture EXE SHA256 `C1D139B46DC99FA6A27F13DAE456B43EF3DF1C521E9BD0CD5C9CB84440B8653C`。

fixture 证明：自然错误分类；零触发零读；假 camera 失败后假 map 成功没有 camera 证据；同一假对象后续 camera/Snapshot 成功；上下文/reader/epoch/openCount/Allowed 改变立即终止；异常也做后置守卫；最多 3 次；取消/预算/超时/间隔边界；原 fail/air invalidated/exit3 保持；矩阵/本地身份/UTC 序列化；prewarm 参数、切片、上下文、取消、冻结时钟上限。未声称执行真实 engine cache 清空或 GUI 恢复。

8 组 CLI 参数检查均在应用初始化前 exit2，覆盖缺 event、-1/5001/abc、0/5000 边界及 recovery 配合事件；边界合法用缺 --out 确保不进入应用。见 [recovery-cli-test.log](F:/WorkSpace/Game/CoH3/dev/SkyEye/tests/maintenance-live/recovery-cli-test.log)。该日志包含 PowerShell 对预期 stderr 的 NativeCommandError 包装，8 项断言均 PASS。
