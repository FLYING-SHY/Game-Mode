# G-AIR-FLIGHT-27：真实 P47 飞行补救路径

结论为 `PASS_actual_reader_flight_fallback_window_only`。本例接受正常授权实际 C# reader 的一次飞行补救输出、完整采样窗口及之后归零；当前版本总体验收继续为 candidate。自动复核见 [validation](<run27-flight-window-validation.json>)，可重跑逻辑见 [validator](<validate_run27_flight.py>) 的 `validate(root)`。

## 身份与输入

- 游戏 PID31572，startUTC `2026-11-20T18:44:29.8492330Z`，EXE SHA256 `8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC`。
- 正常授权维护 reader PID25740，EXE SHA256 `CD125BFFA33E525CD4B94A753D65A6E4CBB199395E3860E57E85003DB7665F79`；真实 lease active，readerOpenAttempts=1。
- [driver identity](<run27-driver-identity.json>)、[driver result](<run27-driver-result.json>)、[driver](<run27_native_driver.ps1>) 记录输入、子进程和恢复暂停。OS 子进程退出码确实留存为5，与 reader 报告相同；外层 driver exit0只表示驱动完成。
- 原生暂停12:04下，从US空中支援中心确认一次反向P47。初始air扫描后以真实foreground-checked Space恢复；显式prewarm4000ms用于错过phase0，保留原生产门控。[最终原生画面](<run27-native-paused.png>)为12:15暂停。原生推进10.981567秒，reader窗口持续20.014秒，后段处于暂停状态。

## 真实 reader 序列

[实际报告](<../maintenance-run-27-p47-compact-flight/maintenance-live.json>)中的event_window为observed，stopReason=deadline，184个连续ordinal全部completed，额外14ms有明确记录。snapshotAttempts185包含窗口前一次。

| 项目 | 实际值 |
|---|---|
| air scope | all_current_snapshot_rows_diagnostic_not_production_enemy_UI |
| 完整air查询 | 184 |
| 正观测/保留decoded | 44 / 44 |
| 保留frames | 46：最早空帧、44正帧、最新空帧 |
| 替换空帧 / 丢decoded | 138 / 0 |
| 唯一key | `flight:2188088899584:1000008712` |
| 首条正例 | ordinal7，645ms，UTC02:26:27.9987038 |
| 末条正例 | ordinal50，5331ms，UTC02:26:32.6845724 |
| 之后首个零 | ordinal51，5439ms，UTC02:26:32.7921632 |

44次观测属于同一架飞机的同一条缓存条带。owner2188149555200、slot0；XYZ `(34.41206,20.2007885,1.8647356)`，DX/DZ `(-0.158670262,0.9873316)`，半宽5、长45、offset0、radius0、Rocket=false、Point=false，全部保留输出完全一致。首正例645ms与现有600ms等待相容，报告未序列化内部Born时间，因此不把645ms写成精确飞机出生年龄。

## 同窗 raw 与后续暂停状态

三份raw均完整扫描、根/descriptor/entries稳定、无读取错误，目标飞机都是entity `0x1fd7442f800`、generation1000008712、index2963。离线复核只使用actor前184B，并核对vtable、entity回指、代际、mode/state及方向。

| 原始证据 | UTC | 沿原始目标/方向的距离 | 关键观察 |
|---|---|---:|---|
| [raw11](<live-raw-11-p47-compact-middle.json>) | 02:26:29.386898 | -98.820831m | actor20目标与条带一致；+96/+100方向约(-0.1586368,0.9873371) |
| [raw12](<live-raw-12-p47-compact-late.json>) | 02:26:32.500020 | +42.731618m | actor目标已经切为离场点；C#随后仍输出原攻击条带，再归零 |
| [raw13](<live-raw-13-p47-compact-after-clear.json>) | 02:40:39.162764 | +170.080802m | 后续暂停状态中同一代飞机仍存在，位置已超过55m结束阈值 |

raw11/12处于采样窗口内；raw13在窗口结束之后的暂停现场，记录为后续状态而非同刻读。raw11/12的原P47 TimedAbility已为phase2/instanceFFFFFFFF；cast原始目标与方向仍和C#条带相符。`flight:` key由实际fallback路径产生，本例取得此前缺失的原生飞行补救正例。raw12离场目标变化后输出仍固定在原攻击目标，也验证本例没有用离场点覆盖已恢复条带。原始探针按旧诊断格式捕获284B，复核与新consumer取前184B；对象184B分配证据来自先前静态factory，不能由284B读取成功推定对象大小。

## 修正与验证边界

MoverDynamics候选形状使用184B和+96/+100，RecoverStrafe与ReadLoiter49共用明确选择。候选选择限定于维护编译且maintenanceDiagnostics=true；普通reader保持原选择。两树已字节镜像。隔离fixture118+170=288PASS，保留全部旧186断言；双树编译/guard144与依赖preflight204通过；当前runner87项self-test通过。静态与回放材料见 [decoder README](<../../../tests/decoder-layout/README.md>)。

本例没有自然camera异常，same_reader_recovery如实为not_triggered，未以prewarm的成功检查替代恢复。all-sides诊断中的本方P47也不接受enemy-only生产GUI、Loiter、V1、SLD、声音、保护版或完整新baseline；这些仍列在[当前矩阵](<../acceptance-current.md>)。
