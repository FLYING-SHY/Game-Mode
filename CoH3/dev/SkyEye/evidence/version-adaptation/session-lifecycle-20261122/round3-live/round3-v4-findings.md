**Round3 v4：历史原始观测索引与复核结论**

主记录为 results、mainmenu、紧同步 second-running002、second-paused001；second-running001 只作先行补充。索引完整固定每个 raw、真实 v3 paused 引用、冻结 v1–v4、截图 PNG/sidecar 与本生成器的 SHA256。哈希用于内容绑定，不是签名。

PID `30504`，启动时间 `2026-11-22T01:23:35.3993114Z` / FILETIME `134397842153993114`；模块 SHA256 `8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec`。历史 Reader SHA256 `39dde7a0f2bd80e2412fed3439870cf7a49a794a9aab0f812a9afee6f60c65d2`。当前生产源码的变化不改写这些采样时 sourceManifest；生成器不读取当前 C#、不执行或导入冻结 probe。

| 记录 | 类型 | reads | 请求/返回 bytes | 秒 | 截图前后秒 | raw SHA256 |
|---|---|---:|---:|---:|---:|---|
| [results](<../../lifecycle-phase/v4/live-round3/pid30504-results-ref-paused-001.json>) | primary | 13386 | 950814/947670 | 3.266 | 43.910122 | `76c8968954b3ff94e42cb60354a2e88f2ce6f810cecc968b76c90315e50e7889` |
| [mainmenu](<../../lifecycle-phase/v4/live-round3/pid30504-mainmenu-ref-paused-001.json>) | primary | 13887 | 959550/955998 | 3.282 | 38.906679 | `6d0a41c32016e10628fbf706e48fa0cdde54799fc2f92bbba999edd022a34765` |
| [second-running002](<../../lifecycle-phase/v4/live-round3/pid30504-second-running-ref-paused-002.json>) | primary | 15393 | 1057566/1053342 | 3.281 | 3.787214 | `9fc92f78a114175a215552fb99965ab9b681803a410cb8bcf02c124aec4e726b` |
| [second-paused001](<../../lifecycle-phase/v4/live-round3/pid30504-second-paused-ref-paused-001.json>) | primary | 15393 | 1063182/1058886 | 3.313 | 3.768359 | `950b7254e1508bf4e5301bf18a6346897735f88daed40b8089702d674e56ff1e` |
| [second-running001](<../../lifecycle-phase/v4/live-round3/pid30504-second-running-ref-paused-001.json>) | preliminary_only | 15393 | 1057566/1053342 | 3.282 | 98.678382 | `83f6707611dde8d8d3e97d232d4e1b0a13e99271f0333ef9c79578257d7ccfa5` |

所有 raw 保留 `phaseDecision=not_evaluated`、`productAcceptance=false`、`currentTreeCoverage=unknown`，句柄已关闭、身份/模块与历史 source binding 一致；各次均未超过 20000 reads / 2 MiB requested / 10 秒，总 deadline overrun 为 0。journal 连号、逐项读长/返回、累计与每样本账目、initial/recheck 分区读 span 均重算。大块 body/vector 日志通常不含原始字节，因此 parsed 节点值和 elements hash 不是从 journal 独立重解码所得。

| 记录 | before（sidecar 原时区） | raw UTC 起止 | after（sidecar 原时区） | 根代理既有读图结论 |
|---|---|---|---|---|
| results | [state-23-results-v4-before.png](<state-23-results-v4-before.png>) 2026-11-22T10:42:22.1417401+08:00 | 2026-11-22T02:43:02.303819Z → 2026-11-22T02:43:05.571225Z | [state-24-results-v4-after.png](<state-24-results-v4-after.png>) 2026-11-22T10:43:06.0518626+08:00 | 费蒙维尔结果页，02:58；前后均由 lead 亲读图确认。 |
| mainmenu | [state-26-stable-after-results-close.png](<state-26-stable-after-results-close.png>) 2026-11-22T10:44:34.8074435+08:00 | 2026-11-22T02:45:09.958666Z → 2026-11-22T02:45:13.243407Z | [state-27-mainmenu-v4-after.png](<state-27-mainmenu-v4-after.png>) 2026-11-22T10:45:13.7141225+08:00 | 主菜单；state26 是本次 probe 的 before，state27 为 after。 |
| second-running002 | [state-37-second-match-tight-before.png](<state-37-second-match-tight-before.png>) 2026-11-22T10:57:25.4605199+08:00 | 2026-11-22T02:57:25.569259Z → 2026-11-22T02:57:28.852609Z | [state-38-second-match-tight-after.png](<state-38-second-match-tight-after.png>) 2026-11-22T10:57:29.2477337+08:00 | 第二局运行；04:13/资源1531 → 04:17/资源1548，lead 亲读图。 |
| second-paused001 | [state-40-second-paused-tight-before.png](<state-40-second-paused-tight-before.png>) 2026-11-22T11:02:04.1001553+08:00 | 2026-11-22T03:02:04.213752Z → 2026-11-22T03:02:07.520889Z | [state-41-second-paused-tight-after.png](<state-41-second-paused-tight-after.png>) 2026-11-22T11:02:07.8685142+08:00 | 第二局 Esc 菜单；前后均 07:39/资源2458，lead 亲读图。 |
| second-running001 | [state-35-second-match-running-before.png](<state-35-second-match-running-before.png>) 2026-11-22T10:54:10.8326493+08:00 | 2026-11-22T02:55:45.662075Z → 2026-11-22T02:55:48.945738Z | [state-36-second-match-running-after.png](<state-36-second-match-running-after.png>) 2026-11-22T10:55:49.5110310+08:00 | 第二局运行的先行观测；截图间隔较大，002 作为主要运行证据。 |

截图为 selected-window，非桌面合成图；内容结论来源是 lead 已完成的 read_image 检查，本生成器只校验文件身份、PNG/sidecar 尺寸和 before ≤ probe ≤ after 顺序。它们不构成产品覆盖层合成、产品真实 GUI 或产品 G 验收。

跨局对比真实 [v3 paused 引用](<../../lifecycle-phase/v3/live-round3/pid30504-paused-001.json>)：第一局 paused、results、第二局 running/paused 的 world/local/player entries 与 matchKey 完全复用，arena `0x1d01d600000`、world `0x1d023a00000`、vector `0x1d01d61c600`、array `0x1d01ea15140`、local `0x1d026209000`，key `30504:639309074153993114:1993462513664:1993504493568:1001`。UI root 始终 `0x1d2a08a61c0`；这些字段相等不足以定义新局 epoch。主菜单则是 1 玩家/local `0x1d026207000`/lowIdentity1000。

第二局当前 HUD `0x1d098515200`、minimap `0x1d3b7dad490`、tactical `0x1d2ae303150`：0/1/3 秒、initial/recheck 均有当前根双向路径，按 HUD/minimap/tactical 顺序分别 12/26/22 nodes、11/25/21 edges。每份第二局 raw 审核 18 positive traces / 360 node entries / 342 edges（228 inline + 114 vector）。HUD 换址，11 个祖先 address/vtable/parent136 完全复用；共同父 `0x1d2a061eb10` 的 `+200` 槽 `0x1d2a061ebd8` 从 `70c55eccd3010000` 变为 `00525198d0010000`。同一 HUD 地址跨局重用尚未实测。

每个 v4 raw 都对第一局旧 HUD/minimap/tactical 做了 18 次完整 576-byte body 读取（3 controls × 2 passes × 3 samples），均在 `control_vtable_changed` 早停：空 nodes/edges、firstPassPath=false、reachability=unknown。其 structureStable/flags192Stable=true 只是空列表比较；未解码旧 parent/flags，不推断销毁、脱链、隐藏或对象不存在。results/mainmenu 没有当前正路径，仍保持 unknown；queue_exhausted 不等于完整覆盖。

| 第二局记录 | HUD flags192Stable(0/1/3) | minimap | tactical | 0x20 变化 |
|---|---|---|---|---|
| second-running002 | T/F/T | T/F/T | T/F/T | 无 |
| second-paused001 | T/F/F | F/F/F | T/F/F | 无 |
| second-running001 | T/T/T | F/T/T | T/T/T | 无 |

HUD/minimap 两遍链上的 mask `0x20` 均为 true，tactical 为 false；raw flags 在 running 与 paused 都会变化。paused 3s 的 minimap control 自身也从 `0x2a01c030` → `0x2a018030`，XOR `0x4000`；其他已测变化为 `0x4000` 或 `0x40004000`，均未改变 `0x20`。flags192Stable 不是独立的暂停判据或上下文保留必要条件；完整差异节点、读 ordinal、各样本截断/异常与 checkpoints 见 [JSON 索引](<round3-v4-observation-index.json>)。

重算命令（仓库根）：

```powershell
python -B -X utf8 CoH3/dev/SkyEye/tests/live-lifecycle/index_round3_v4.py --check
```

[生成器](<../../../../tests/live-lifecycle/index_round3_v4.py>)默认仅以 exclusive create 新建两份固定输出；`--check` 在内存重建并逐字节比较两份既存输出，发现 drift 即失败，全程无写。product Snapshot、build correspondence、产品真实 GUI、覆盖层合成与产品 G 验收仍为 not_run/false；本索引没有实现 phase predicate 或产品 epoch/cache 修改。
