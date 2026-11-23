# P47 实际施放：run25 计数及 run26 历史 payload

本增量对应游戏 5.1.52426.0 / SHA256 `8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC`，游戏 PID31572、startUTC `2026-11-20T18:44:29.8492330Z`，同一离线 AI 对局。状态仍为 candidate；本报告不登记 baseline。

## 1. 输入与采样范围

通过原生 US 空中支援中心 P47 按钮和拖拽目标施放。第二次确认画面 [原生确认图](<../round05-p47-02-confirm.png>) 显示接受的红色攻击区域和排队图标，本方弹药544→494。两次原生攻击方向近似正交，原始 timed-ability 方向分别约 `(0.020236,-0.999795)`、`(0.999970,0.007747)`。

采样器经真实授权 coordinator，active lease 后使用同一个维护 `PlayerInfoReader` 调用实际 C# `Snapshot()` / `ReadAirZones()`。`--air-events` 明确传入所有当前 snapshot 玩家，因而能够采集本方控制的 P47；生产 GUI 的 enemy-only 玩家筛选保持原样。这些本方诊断结果不替代敌方 GUI 显示或条带像素对齐证据。

## 2. run25：计数正例，payload 被旧保留策略挤出

[原始报告](<../maintenance-run-25-p47-prewarmed/maintenance-live.json>) SHA256 `2e1ada1429eeb61d6df1f1ebd7b8c06fddb1b1b70709919190412193fc2f0dad`，[计数复核](<run25-reader-counter-validation.json>)。

- 60秒、551次完整 air 查询，83次正观测；正例 ordinal221–303，UTC `01:24:58.5949709Z` 至 `01:25:07.5450765Z`，ordinal304为后续空读。
- 最早128个空帧耗尽旧容量：retainedFrames128、retainedDecoded0、droppedFrames423、droppedDecoded83。实际正查询存在，精确 key、owner、geometry 和 cast/fallback 路由的 payload 已缺失，计数不替代这些字段。
- [raw03](<live-raw-03-p47-early.json>) 有 active cast、无 P47；[raw04](<live-raw-04-p47-middle.json>) 和 [raw05](<live-raw-05-p47-late.json>) 同时记录该 cast 与同一 P47 代次。raw 是独立时刻的只读快照，不当成缺失 C# payload。

随后仅改维护 sampler 的有界保留策略：保留最早/最新空帧、最多126个正帧，原128帧/256 decoded/每帧16上限不变；空帧替换和正帧截断逐项计入丢弃数。另将既有 `AirZone.Key/Owner/Offset` 加入输出，不改产品路由。18个 air fixture、16个 event fixture、20个基础 fixture 共54项通过。run26 使用最终 sampler SHA256 `BB3D856A2E5683F4C14492E8A9FD385A47204010C93D61CE1BB56798D61EA564`。

## 3. run26：保留66条同一 cast 的历史正例，完整窗口失败

[原始报告](<../maintenance-run-26-p47-retained/maintenance-live.json>) SHA256 `85829f13f953a434a28139b4d6883364bc63596155d597f55c55768177b8d22d`，[独立离线校验](<run26-reader-prefix-validation.json>) 与 [校验脚本](<validate_run26_prefix.py>)。driver [运行身份](<run26-driver-identity.json>)。

| 项目 | 结果 |
|---|---|
| reader / lease | readerOpenAttempts1，lease active，固定 match/epoch1 |
| 窗口 | 请求60秒；429次 Snapshot poll；428次完整 air 查询；46.780秒因异常提前结束 |
| 保留账本 | 68帧、66 decoded、360个被替换空帧、0 decoded 丢失 |
| 首/末正例 | ordinal12 `01:50:10.4223985Z` / ordinal77 `01:50:17.5193676Z` |
| 首个后续空读 | ordinal78 `01:50:17.6294220Z`，后续空读持续至 ordinal428 |
| 唯一施放 | 66条只有1个 key，owner `0x1fd77e08000`、slot0、instance3481，均为 P47扫射 |
| 目标 | XYZ `(34.4120636,20.2007885,1.86473608)` |
| 方向 / 范围 | DX/DZ `(0.999969959,0.00774675328)`；halfWidth5、length45、offset0、radius0；非 Rocket / Point |

脚本逐条验证完整 poll ordinal、账本恒等式、帧时间顺序、有限且归一化的方向、所有保留帧 match/epoch，并将 [raw07](<live-raw-07-p47-early.json>) 的 owner/ability/instance/path/target bytes 组成的 key 与 C# 输出精确比较；坐标、方向和范围逐条一致。所有66条 payload 完全相同，因此这是一次施放的重复观测，不是66次攻击。该 key 不是 `flight:` key，当前正例明确走 timed-cast 路由。

ordinal429 的实际异常为 `System.Exception: 镜头已切换。`，调用链 `ReadMineCamera → LocalIdentity58 → Snapshot → SampleEvents`。窗口维持 `fail` / `stopReason=exception` / `air.status=invalidated` / 报告 `exitCode=3`。早先428次成功读是历史记录；未通过整个窗口。driver 的 OS child exitCode 未成功留存；外层 exit0不替代 reader 报告状态。

## 4. MoverDynamics 形状缺陷与修正依据

相同 PE 静态 factory RVA `0x707A10` 的分配长度为 `0xB8`（184字节），ctor `0x707A70` 安装 vtable RVA `0x3D926E0`。旧 `RecoverStrafe` 和 `ReadLoiter49` 却读取284字节、取 `+272/+276/+280` 方向；这些偏移超出新对象分配边界。在 raw04/05 中它们是整数样式数据，作为 float 得到约e-43，`unitHeading` 失败。结构仍稳定不等于跨对象尾部是合法字段。

raw04→05 同一 P47 `0x1fd7445fe00` / generation1000008534 / index3108 的实际 XZ 位移归一化约 `(0.020231772,-0.999795317)`；actor `+96/+100` 与此方向相符，且与原始 cast 方向一致。raw08 的另一 P47 `0x1fd74467a00` / generation1000008610 / index3063 在近正交施放下，actor `+96/+100=(0.999970019,0.007746754)`，再次与原生 cast 一致。静态序列化链 `this+0x48` 再 `+0x18` 保存两个4字节字段，提供独立字段形状旁证。

raw05 已进入离开目标阶段，actor 目标/高度条件也可能改变；它不应被当作仍可恢复新扫射的正例。[raw08](<live-raw-08-p47-middle.json>) 还含保留在 registry 的无 actor 负项；[raw09](<live-raw-09-p47-late.json>) 没有有效 P47。分析脚本按可选 actor 处理这些负项。

本报告时点修正正在独立任务中进行；后续必须分别记录源、raw回放以及真实新事件结果，不将本处定位直接写成功能通过。Loiter 共用 mover 结构，但仍缺真实巡航事件语义与生命周期证据。

## 5. 镜头错误后的窄恢复结论

run26 失败后，同一个 reader 的 map probes 完成一次有世界/布局校验的当前小地图读取；战术地图未打开。`readerOpenAttempts=1` 及 runner 单一局部 reader 流程共同支持实例未更换。

地图路径绕开 `LocalIdentity58()` / `ReadMineCamera()`，可与已清零的 cameraCore 同时成功；因此这不证明同实例镜头重发现或 Snapshot 恢复。`01:50:56.0015496Z` 是 map feature 的 StartedUtc，5006ms为两地图共用预算，不是精确地图成功时刻。另需真实同实例的后续 camera/Snapshot 成功和生产 GUI 自动重试分支证据。

仍开放：敌方 air GUI、flight fallback 新事件、Loiter/V1、清空后新事件、方向/宽长/生命周期像素对照、同实例镜头恢复、跨局和真正 fresh 游戏进程、双工具保护交付。新 baseline 继续 not verified。
