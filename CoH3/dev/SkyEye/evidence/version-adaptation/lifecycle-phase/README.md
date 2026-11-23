# 已知 HUD 链的有界阶段对照探针

[phase_probe.py](phase_probe.py) 为 root 的后续实际采集准备；[phase_probe_selftest.py](phase_probe_selftest.py) 只使用稀疏 fake memory、fake clock、内存输出及模拟 Win32 函数。本次开发只执行离线自检与 `--help`，未执行 live capture、GUI、进程查询或构建。

脚本输出原始观察，`phaseDecision` 固定为 `not_evaluated`，`productAcceptance` 固定为 `false`。`--label` 是操作者提供的阶段标签，`operatorLabelIsObservedPhase=false`；标签不构成阶段实证。`not_observed`、父链失联、战术图关闭、字段变化及预算停止都保留各自原因。

## 调用

在仓库根目录使用 64 位 Windows Python。PID 与启动 UTC 必填，UTC 接受 `Z` 或 `+00:00`，最多保留 7 位小数，按 FILETIME 的 100 ns 精度比较。

以下示例使用[第二个新进程留存身份](../fresh-process-02-identity.json)中的 PID/start；由 root 对照其当前实际进程身份执行。新进程需使用对应的新值。输出父目录须已存在，输出路径须为新文件。

```powershell
python -B -X utf8 CoH3/dev/SkyEye/evidence/version-adaptation/lifecycle-phase/phase_probe.py --pid 33656 --expected-start-utc 2026-11-21T03:42:27.6605831Z --label running --out CoH3/dev/SkyEye/evidence/version-adaptation/lifecycle-phase/fresh02-running.json
```

分别在 `running`、`paused`、`results`、`mainmenu`、`newmatch` 使用新的输出名执行。每次调用只打开一个显式 PID 的句柄，在身份与 SHA 核对后，以第一组为 t=0，调度 t=0/1/3 秒三组采样。`actualOffsetSeconds`、`scheduleDelaySeconds`、各段 UTC 和单调时钟计数保留真实节奏；阶段切换与截图由 root 单独记录，脚本不操作游戏。

```powershell
python -B -X utf8 CoH3/dev/SkyEye/evidence/version-adaptation/lifecycle-phase/phase_probe.py --help
python -B -X utf8 CoH3/dev/SkyEye/evidence/version-adaptation/lifecycle-phase/phase_probe_selftest.py
```

退出码 0 表示三组原始采样执行完毕且末尾进程身份仍符合预期；各字段仍可能 `not_observed`。退出码 3 对应整体失败、进程身份变化或全局预算停止，已采到的观察仍保留。输出冲突在任何进程句柄之前触发 `FileExistsError`，原文件保持原样。

## 来源与访问

所有 RVA 都是[当前维护布局](../../../client/CoH3MemoryTool/GameLayout52426.cs#L22)中逐项建立的角色，没有 delta 估算或其他 RVA 候选。游戏主模块必须为 `RelicCoH3.exe`，image size 为 `120369152`，磁盘 SHA256 为：

```text
8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec
```

源文件 pin 在脚本中固定；[维护布局](../../../client/CoH3MemoryTool/GameLayout52426.cs)、[reader](../../../client/CoH3MemoryTool/PlayerInfoReader.cs)及[既有有界 raw probe](../air/air_raw_snapshot.py)的字节变化会在打开进程前停止。报告列出源文件 SHA、大小、绝对路径、脚本 SHA 和角色映射。这是来源追踪，未声称源码与某个客户端二进制构建完全对应。

访问掩码仅 `0x410 = PROCESS_QUERY_INFORMATION | PROCESS_VM_READ`，沿用[既有 raw reader](../live_probe.py#L5)的只读连接方式及用户地址范围，收紧为当前任务的预算。模块列表只来自该显式 PID 的固定 2048 项缓冲；不枚举全机进程。进程写入、线程创建、调用游戏函数、窗口或输入 API 均未实现。实际连接及模块枚举部分尚待 root live 验证。

游戏磁盘文件在散列前后核对 size/mtime；散列本身上限 256 MiB。该 SHA 绑定磁盘主模块，不属于完整内存代码完整性证明。

## 有限字段

| 观察 | 已有来源与输出 |
| --- | --- |
| 进程 | FILETIME 创建时间、UTC、PID、still-active、每组前后身份与最终关闭句柄结果。 |
| Camera | [ReadMineCamera](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L1125)的已知 owner → `+184` vector，最多 8192 字节 / 1024 个元素，匹配已知 GameUICore vtable；读取 core、`+416` 矩阵对象、816 字节既有 body 中的 pose、View、Projection，保留矩阵 raw hex、有限性、格式与 pose residual。 |
| 本地身份 | [LocalIdentity58](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L216)的 core `+680`、controller vtable、identity `+8`、byte `+24`；只报告该 byte 的值，未赋予比赛阶段语义。 |
| World/玩家 | [Snapshot](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2768)的 arena/world roots、resolved world、`+252` 玩家列表、列表 `+12/+16`、最多 16 个相对玩家指针与 `+8` identity/slot；不读取 entity registry、不扫描单位或资源。 |
| 候选 MatchKey | 仅在已观察 camera 格式、controller 本地身份检查通过且恰好一个玩家 identity 匹配时按现有格式拼接；明确标注 `matchKeyCandidateIsProductSnapshot=false`，并未执行 C# Snapshot。 |
| HUD 发现 | [MiniTree53](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L1925)的已知 UI singleton → `+136` 根，先匹配 GameUIAppWindow vtable，再沿[MiniChildren53](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L1858) / [TacticalChildren51](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3496)已建立的指针字段与 `+504` 子数组遍历；候选都复核 `child+136 == parent` 与已有 vtable 范围。 |
| 地图父链 | 每种控件只取有界遍历中的第一个已知 vtable；minimap template `+472`、tactical template `+528`，从 template 最多 64 层 `parent+136`，保留 address/vtable、flags `+192`、bit `0x20`、translation `+168`、size `+296`、HUD/root/control 是否出现及终止原因。 |
| 稳定性 | 每组结束前重读已登记的 roots、身份、指针及地图父链 vtable/parent/flags，逐项保存变化前后 raw hex 和具体读取错误。`stableWorldRootsAfterRead` 单独表示 arena/world 两项；`allObservedCheckpointsStable` 只覆盖登记项，不代表所有数据或比赛仍在运行。 |

HUD 搜索最多每组 384 节点，子数组最多 512 项，父链最多 64 层。遍历可因队列用尽、找到各一类控件、节点上限、局部 slice 或全局预算而停止。`traversal.exhaustive` 恒为 false：这些原因都不表示控件在整个进程内不存在。没有已知根或未找到控件时输出 `not_observed`。

## 预算及留存

- 全次调用：10 秒合作式截止时间、2 MiB 请求字节、20000 次 ReadProcessMemory 尝试，单次 1..65536 字节；失败和短读也计入尝试数与请求字节。
- 各组 HUD 遍历局部 slice：4500 次读取、512 KiB、0.6 秒；用于给其余观察和后续采样留下额度。
- 全局预留 0.5 秒 / 65536 字节 / 768 次读取给结束稳定性复核；复核也受绝对总上限约束。
- 读取前后检查截止时间。阻塞中的系统调用、磁盘散列和最终 JSON 写入不作强制抢占；关闭句柄后、JSON 序列化前记录 `elapsedSeconds` / `deadlineOverrunSeconds`，最终写文件耗时未计。该限制需要与原始 UTC、实际调度延迟一同审阅。
- `readJournal` 逐次保存 address、length、field、ordinal、UTC、elapsed、成功或具体错误；最多 8 字节的成功读取附 raw hex。`requestedBytes` 是强约束计数，`returnedBytes` 统计后端实际交回的 buffer；Windows 部分读取的 `actual` 值保留在错误文本中。
- 输出先以 `open("x")` 独占预留，不覆写。句柄在 `finally` 关闭；失败会留存失败 JSON，进程身份/SHA 失败前不做 VM_READ。

## 解释边界与下一步对照

地图属性、transform 和完整坐标布局没有在 Python 重新实现；`completeLayoutValidationPerformed=false`、`csharpTryReadExecuted=false`。父链检查采用现有字段，单独的 bit `0x20` 只沿用[战术图可见性读取](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3491)的位值，不作比赛阶段判断。没有读取或调用产品的 TryReadMinimap/TryReadTactical、GUI scene、许可证状态或 host 的 LastError；探针也未为它们伪造值。

发现只用当前已知 UI 树入口，未采用产品 discovery 的 VirtualQueryEx 区域扫描分支，也未加载磁盘位置 cache。若 HUD 保留在未到达的分支或已脱离当前根，本次结果只说明该有界路径没有观察到它。

root 后续应在实际运行、战术暂停、原生结果页、真正主菜单及新局上分别采样，并与同 UTC 的原生画面、GUI 显隐记录对齐。暂停与暂时找不到地图是必要的反例；稳定 world、静止矩阵、未变化的资源或地图 flags 单项，都还没有已验证的结果页判定语义。本目录只交付采样能力。

## 离线验证记录

2026-11-21 开发自检：27 项通过，使用 bundled Python `-B -X utf8`，未产生 pycache。覆盖：

- UTC 第七位、源文件 pin/角色映射、地址与 sentinel、读取次数/字节/时间上限及预留；
- 失败和短读计费、局部 HUD slice、384 节点/512 子数组/64 父链限制、父链环；
- 已知完整图、未发现根、读取异常保留部分链、NaN 的 raw 留存与 JSON 编码；
- roots 变化、无效本地身份抑制候选 MatchKey、实际采样延迟；
- 同一 fake 句柄的 0/1/3 调度、身份不符/退出/SHA 失败、输出冲突与 finally 关闭；
- 模拟 Win32 构造时只请求 `0x410`，身份不符在模块枚举和 VM_READ 前关闭句柄。

自检中的新输出由内存 sink 接管，输出冲突使用已存在的自检脚本验证其 SHA 前后不变。测试没有创建实际游戏采样，也不构成 live 兼容性结论。
