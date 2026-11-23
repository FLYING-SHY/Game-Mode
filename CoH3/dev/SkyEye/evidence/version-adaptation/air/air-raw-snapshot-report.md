# 有界空袭原始快照探针（task-1）

## 交付与验证

- 入口：[air_raw_snapshot.py](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/air_raw_snapshot.py>)。
- 最新 fake-only 证明：[self-test-03.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/self-test-03.json#L55-L170>)：`self_test_pass`，26 项通过，`liveProcessOpened=false`；基准 fake 快照 64 次读取请求、3236 字节请求/返回。
- 使用 Python 内置 `compile(source, path, 'exec')` 完成语法编译，结果 `COMPILE_PASS no_bytecode_written`。未生成 `.pyc`，未构建客户端或测试二进制。
- 本任务只运行编译和 `--self-test`；未连接 pid31572、未执行游戏、未输入游戏操作。fake 中的 P47 通过只证明解码路径与夹具一致，不是实际游戏样本成功。
- 初次自检的源码提取器误匹配 `list.Add`，记录了 `Unexpected AirCatalogue.Add source format`；已将提取模式限定为静态构造器行首 `Add("...")`。早期 [self-test-01.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/air/self-test-01.json>) 保留失败记录；验收以最新证明为准。

## 父代理运行方式（此处仅给命令，本代理未执行）

从工作区根目录运行；输出必须是新文件，已有文件触发 `FileExistsError`，不会覆盖：

```powershell
& 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe' -B `
  CoH3/dev/SkyEye/evidence/version-adaptation/air/air_raw_snapshot.py `
  --pid 31572 `
  --out CoH3/dev/SkyEye/evidence/version-adaptation/air/live-raw-01.json
```

离线自检，不带 `--pid`；两种模式互斥：

```powershell
& 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe' -B `
  CoH3/dev/SkyEye/evidence/version-adaptation/air/air_raw_snapshot.py `
  --self-test `
  --out CoH3/dev/SkyEye/evidence/version-adaptation/air/self-test-new.json
```

退出码：完整且稳定的候选快照 / 自检通过为 `0`；预算、读错误、变化中、源码解析错误等为 `2`。参数错误由 `argparse` 返回 `2`。输出 JSON 同时打印简短状态摘要。

## 源码依据与映射

每次启动从当前 [GameLayout52426.cs](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/GameLayout52426.cs#L22-L60>) 解析各角色独立映射，不采用统一 delta、legacy 回退或进程内扫描。当前五个角色：

| 角色 | RVA |
| --- | --- |
| resource 静态对象 | `0x6BB1ED0` |
| arena 表首指针 | `0x6187E40` |
| world 相对偏移 | `0x5E0DECC` |
| TimedAbility vtable | `0x3D94A40` |
| MoverDynamics vtable | `0x3D926E0` |

依据：

- [PlayerInfoReader.ReadAirZones](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L327-L611>)：player+1016 技能 vector，count+44≤256 / data+40；技能148字节；target20字节；world+260 全局 registry，count+24∈1..65536 / offsets+8（stride8）/ ids+20（stride4）；飞机实体392字节和 motion284字节。
- [AirLogic.Cast / RecoverStrafe](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/AirLogic.cs#L18-L93>)：vtable、身份、phase/type/flags、方向与位置、actor 回指/状态/高度等条件。
- [AirCatalogue](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/AirCatalogue.cs#L25-L68>)：运行时解析当前行首 `Add(...)` 和 `.Point=true`，从而分辨命中目录的原生 P47 与目录外 recon；目录外对象仍保留证据。
- [live_probe](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/live_probe.py#L48-L81>)：沿用已建立的 resource / arena / world 与 world+252 玩家向量路径，玩家数上限16。
- JSON 的 `sources` 记录上述来源及 Loiter、位置判定等文件的绝对路径和 SHA-256，`currentRvas` 记录本次实际解析值。

## 采集字段

| JSON 路径 | 内容与用途 |
| --- | --- |
| `snapshot.roots`、`registry` | arena/world/resource 静态地址、entity registry 的 count/data/ids 偏移 |
| `snapshot.resources`、`timers` | 有界资源/计时器名称和索引，包含 P47/L2 相关 timer 名称时原样保存 |
| `snapshot.players` | 玩家地址、完整64位身份、slot、技能 vector count/data；采全部玩家，避免将本地 P47 的缺失误当目录缺失 |
| `snapshot.timedAbilities[].raw148Hex` | 技能原始148字节，保留非预期 vtable 的条目供对照 |
| `snapshot.timedAbilities[].vtable`、`blueprint` | 实际vtable、模块内RVA、预期RVA、目录匹配及技能路径 |
| `snapshot.timedAbilities[].fields` | target offset+8/count+12、type+20、identity+24、direction+40/+44/+48、blueprint+56、instance+68、phase+76、flags+144 |
| `snapshot.timedAbilities[].target` | 仅 count==1 且偏移有效时读取 target20字节，含 kind+0、position+4/+8/+12 |
| `snapshot.timedAbilities[].gates`、`failedGates` | Cast 前置条件、目录条件和两次原始读取一致性；`rawCastCandidate` 只是原始字段候选判定 |
| `snapshot.airEntities[]` | 保留 aircraft 路径以及 ebps 中 P47/recon 命名路径（另含V1），即使未在当前目录；标记 `cataloguedAircraft`、`p47Name`、`reconName` |
| `snapshot.airEntities[].raw392Hex` | entity 原始392字节；owner+88、id+104、index+108、flags+160、position+44/+48/+52、actor offset+388 |
| `snapshot.airEntities[].actor` | motion284字节（V1仅68字节），实际/预期vtable、mode+16、position+20/+24/+28、backref+36、state+64、heading+272/+276/+280 |
| `snapshot.airEntities[].recoveryRawGates` | P47/strafe恢复原始条件、owner是否位于已读玩家、actor目标与entity身份的复读稳定性；`rawRecoveryCandidate` 不是最终绘制判定 |
| `snapshot.scanComplete`、`coverage`、`errors` | 全部计数、最多100条详细读错误、是否完整遍历；预算中止仍保存部分证据 |
| `snapshot.stabilityChecks`、`stableEntries` | roots、world向量偏移、player vector与身份、每个技能count/data、资源/timer vector、registry count/data/ids复读；技能148/target20和飞机身份/actor目标稳定性 |
| `stats` | 请求次数、请求字节、返回字节和耗时；失败请求在OS调用前计入预算 |

## 预算与读边界

- 总上限：10秒、2097152请求字节、20000次 `ReadProcessMemory` 尝试；单次≤65536字节。
- 普通读取止于9.5秒、总字节减8192、总次数减64，预留最后0.5秒/8192字节/64次给稳定性复读；最终复读仍受总上限约束。
- 时间从 `OpenProcess` 前开始，读前/读后与循环逐次检查；同步Win32调用本身没有强制中断接口，因此某次OS调用阻塞时，以返回后检测、停止后续读取并报告超时处理。
- 只打开 `0x410 = PROCESS_QUERY_INFORMATION | PROCESS_VM_READ`；获取主模块名称和大小，验证主模块名 `RelicCoH3.exe` 及所用RVA位于其映像内。
- 所有读取验证用户态地址范围及完整span；arena偏移拒绝 `0xFFFFFFFF`；在解引用前核对源代码中的vector数量与stride；蓝图/表名字符串≤1024字节。
- 没有进程写入、远程线程、注入、扫描、调用游戏函数、暂停/恢复、游戏输入或授权系统逻辑。

## 解释顺序与限制

1. 先检查 `status`、`scanComplete`、`stableRootsAndDescriptors`、`stableEntries` 与 `errors`。预算停止、读失败、任一变化中条目均使 `absenceEvidenceCompleteWithinThisSnapshot=false`；不要以空数组直接断言P47缺失。
2. 对技能看 `blueprint`：命中原生P47目录后，再看vtable、target/type/identity/phase/flags/方向和稳定性失败项。recon技能即使字段有效，目录失败也会留下原始字段，而非静默消失。
3. 对飞机看 P47/recon 路径、owner、id/index、motion vtable和状态，区分“实际未采到P47飞机”“只有目录外侦察机”“有P47但原始gate失败”。
4. 本探针是一次非原子快照；未复刻 `airSeen`、`suppressRetainedAir`、忽略集合、`NewFlight`/`Born<2500ms`、600ms恢复等待、CastTracker、Loiter状态机或敌我玩家筛选。`temporalAndEnemyGatesEvaluated=false` 明示该限制。
5. 未运行C#生产解码器或UI绘制，也未验证候选可执行文件指纹。Python的方向范数运算采用双精度；阈值周边的精确C#单精度行为仍以原始字节和实际解码器为准。数据与目录通过不等于屏幕显示成功。
6. `absenceEvidenceCompleteWithinThisSnapshot=true` 只表示本次稳定遍历的瞬时证据完整；不同读取之间仍存在时间差，也不代表事件期间、其他PID、其他对局或后续版本的缺失结论。
