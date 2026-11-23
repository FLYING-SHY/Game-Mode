# 当前 UI 根到 HUD 的双向引用采样

v1 在暂停、运行和结算页记录了相同的 `matchKeyCandidate`，结算页仍有通过探针检查的镜头和本方身份。运行和暂停采样找到了 HUD 与战术图，结算页只找到根节点。旧遍历受 `slice_limit` 截断，因此结算页的 HUD 可达性仍为未知。 [暂停样本](../fresh02-paused-before-results.json) · [运行样本](../fresh02-running-kwk38-progress.json) · [结算页样本](../fresh02-results.json)

v2 保留 0、1、3 秒采样，复用 v1 的进程和模块核验、已知字段读取及全局预算。它增加了对当前 UI 根和 HUD 路径的两次独立读取，供结算页、主菜单和新局对照。

## 采集

从仓库根目录使用 64 位 Windows Python。`--out` 的父目录须已存在，输出文件须为新路径。`--label` 是操作者记录的画面标签，支持 `running`、`paused`、`results`、`mainmenu`、`newmatch`。PID 和 UTC 创建时间需对应当前进程，时间保留至 FILETIME 的第七位小数。

同一进程上的结算页可使用暂停样本中的旧 HUD 和战术图地址作为候选。本例对应 PID `33656` 及其已保留的暂停样本。

```powershell
$phase = '.\CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase'
python -B -X utf8 "$phase\v2\phase_probe_v2.py" `
  --pid 33656 --expected-start-utc '2026-11-21T03:42:27.6605831Z' --label results `
  --reference "$phase\fresh02-paused-before-results.json" `
  --reference-sha256 8481416d9cc9a7748dd9528467bc0b6481385c9ef6a44ee1e422f3397164f939 `
  --out "$phase\v2\fresh02-results-v2.json"
```

省略 `--reference` 和 `--reference-sha256` 即可从当前根单独遍历。新进程需使用自身 PID 和启动时间。若使用参考报告，准备同一进程的 v1 运行或暂停报告及其小写字节 SHA-256。探针核对参考文件、v1 脚本及源码摘要、报告及三组样本的 PID 和创建时间。打开目标后、读取内存前再核对当前模块基址、映像大小和模块 SHA-256。参考报告中的脚本摘要属于报告字段，`reportedScriptShaIsAttestation=false`。

## 输出判读

- `hud.proofs` 的 `origins` 标出候选来自旧地址或当前根遍历。首遍读取记录候选 vtable、模板指针、各节点 `parent+136` 和 `flags+192`。每条边同时检查父节点的内联候选槽 `+144…+568`，或 `+504` 下容器的 `+112` 数据及 `+124` 数量。完整见证记录字段偏移或容器索引及所读指针字节。
- `bidirectional_path_observed` 表示两次读取均观察到该候选通过已知字段和当前根相连，并且节点、父指针与子引用见证一致。HUD 路径从 HUD 开始。地图路径从模板开始，须经过对应控件及 HUD。根由当前单例 `+136` 确定，第二遍前后分别再读。两遍使用各自的缓存，每组重新读取。
- `unknown` 保留失败、缺边、换根、预算截断和其他未建立路径的结果。`not_listed_in_read_fields` 具体指已读的候选槽和容器范围。节点 vtable 或父指针仍有效时，也可能找不到相应的子引用。
- `hud.traversal` 记录队列余量、未完成节点、上限与读错。队列耗尽且无读取异常时 `currentTreeCoverage=known_graph_complete`，范围为本轮已知遍历规则。`wholeTreeStabilityChecked=false`。当某条双向路径已经完成复读，整体遍历截断仍可同时记录这条路径与 `currentTreeCoverage=unknown`。
- `flags192Stable`、`bit20Set` 和 `allChainBit20Set` 是独立观测。产品战术图读取以 `0x20` 汇总链的可见性，关闭战术图即可返回 `false`。小地图在等待发现、重测失败或 epoch 改变时也可返回 `false`。结算页若复用相同的控件或根，需要依靠对照样本评估这些字段的区分能力。 [小地图缓存和失败处理](../../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L1466) · [战术图可见性](../../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3491) · [战术图缓存与返回值](../../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3704)
- `camera`、`world`、`stability` 保留 v1 的原字段和稳定性复读。`matchKeyCandidate` 是由探针原字段获得的候选值。`readJournal` 逐笔记录请求与结果，`counters` 给出总耗用。`status=observed` 表示采样按计划完成，各字段状态仍需分别查看。`phaseDecision` 固定为 `not_evaluated`，`productAcceptance` 固定为 `false`。

## 时限与读取量

单个进程句柄使用 `PROCESS_QUERY_INFORMATION | PROCESS_VM_READ (0x410)`，结束时关闭。全局上限为 10 秒、2 MiB 请求字节与 20,000 次读取请求，失败和短读计入上限。每组 HUD 搜索最多 768 节点、64 层父链和每容器 512 项。读取范围是固定根、已知字段以及通过父节点和 vtable 约束的候选。分区配额为后续组及复读保留读取量。

| 每组分区 | 读取尝试 | 请求量 | 协作时限 |
|---|---:|---:|---:|
| 镜头 | 768 | 16 KiB | 0.4 秒 |
| 世界与本方身份 | 96 | 8 KiB | 0.2 秒 |
| HUD 遍历 | 5,000 | 560 KiB | 1.35 秒 |
| 稳定性与路径复查 | 560 | 88 KiB | 0.6 秒 |

10 秒为协作时限，系统调用进行中可能越过限时。关闭句柄后、写 JSON 前记录 `deadlineOverrunSeconds`。0、1、3 秒相对模块核验完成后起算，实际采样时间和调度偏差随组记录。全局预算耗尽时保留已有观测并停止后续采样。进程身份、参考校验或全局预算失败时退出码为 `3`。

## 离线验证

```powershell
python -B -X utf8 .\CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v2\phase_probe_v2_selftest.py
```

32 项检查通过，使用稀疏假内存、假时钟和内存输出。覆盖参考与模块绑定、旧父指针但子引用移除、战术图关闭、换根、边变化、读取故障、父链循环和容器上限、逐组缓存隔离、全局预算、独占输出与句柄关闭。保留的暂停 JSON 也通过了离线参考解析。实机阶段对照由后续采集给出。
