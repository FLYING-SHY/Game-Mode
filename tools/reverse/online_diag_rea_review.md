# REA 逆向记录：显示更新路径与在线证据范围

查核日期：2026-11-08。当前记录针对 Steam build 26443338。

## 本次实际工具来源

用户指定的 [REA 源码仓库](<F:/WorkSpace/github/rea/README.md>) 中，包名为 `rea-agents`，版本为 `6.8.0`，Git HEAD 为 `c5301e6145a9c8e06719f27e83da55af8e5480fc`。该目录没有 `dist` 和 `node_modules`；直接执行源码入口得到 `REA's compiled runtime is missing.`。

本次成功分析使用另外安装的 [REA CLI](<F:/BaseEnv/nodejs/rea.cmd>)，其实际包位于 `F:\BaseEnv\nvm\v24.9.0\node_modules\rea-agents`。CLI 报告版本 `6.8.0`，后端为 Ghidra `12.1.4`。源码入口和已安装入口、Ghidra bridge 的 SHA256 不同，因此这里没有将两套文件视为同一构建。

版本命令还报告 Node `24.9.0` 不满足该包声明的 `^22.23.4 || ^24.22.1 || >=26.12.1`。本次命令实际成功，但没有据此声称环境完全通过版本支持检查。本次未升级或重新安装 Node、Java、Ghidra、REA。

| 项目 | SHA256 |
|---|---|
| 源码入口 | `11120FDC741C2EECA9CE903DE5FD18CD1C7C3FFFA15AE2BECCCA750F161D9EE3` |
| 已安装入口 | `67CA7682986FF2B90678E1797E625CACA6026A1732D9CAD1A1088253C7BB45E5` |
| 源码 Ghidra bridge | `9535F217805C5EEE3EE73F9487F82799320D9B5644F65A3426424422705D4652` |
| 已安装 Ghidra bridge | `60CFDB22B436107C2139B5DBF5C48C6A6FA89B8B704AC88B0A7370A9FFC91FC1` |

## 分析对象与命令

重新读取原始游戏摘要：`21EDB5C05C33E89FB5F9D88A3FC6738817ABD80E0366F648B18ADE999CC975CA`。本次命令分析的是此前从该版本提取、保持原 RVA、保留可写全局页及冷分支的 [显示更新 PE 切片](console_diag_tick_rw.exe)，其 SHA256 为 `c6eef3ef46021bae53191c8ffb03919566306cc47b4b80ad3d18e42b1acba78d`。这是局部分析对象，外部函数未全部包含。

在工程根目录执行：

```powershell
$env:GHIDRA_INSTALL_DIR = 'F:\BaseEnv\ghidra\ghidra_12.1.4_PUBLIC'
$env:JAVA_HOME = 'F:\BaseEnv\java\jdk25'
$env:REA_GHIDRA_STARTUP_TIMEOUT_MS = '120000'
$env:GHIDRA_HEADLESS_MAXMEM = '2G'
$env:REA_GHIDRA_DISABLED_ANALYZERS = 'Decompiler Switch Analysis,Aggressive Instruction Finder,Decompiler Parameter ID'
& 'F:\BaseEnv\nodejs\rea.cmd' decompile-all '.\tools\reverse\console_diag_tick_rw.exe' --provider ghidra --address-start 0x140C964A0 --address-end 0x140C966FF --limit 4 --output '.\tools\reverse\online_diag_display_review_1' --format json
```

`--output` 应指定尚未存在的目录。若重现分析，请选用新的输出目录名。

命令退出码为 `0`。REA 的 [证据信封](online_diag_display_review_1.utf8.json) 记录：

- Evidence ID：`ev_130b78f155fcb21283ece8e9c464806befccceeccd0668c3699e5d95d84ac8fe`。
- 所选函数 `4`，写出 `4`，失败 `0`，剩余 `0`。
- Provider：Ghidra `12.1.4`。
- [逐函数索引](online_diag_display_review_1/decompile-index.json) 保留地址、函数范围、文件摘要和 provider limitations。

## 本次确认的行为

| RVA | 反编译观察 | 证据 |
|---|---|---|
| `0x00C964A0` | 每帧读取本地 UI 查询结果，再调用 `0x00C96650`；不同分支排入 `FOWTextureUpdate` 或 `FOWTextureUpdateRevealExplored` | [每帧更新函数](online_diag_display_review_1/functions/0x140c964a0.c) |
| `0x00C96650` | 取得显示对象，提交显示参数，更新 manager 的 `+0x41` 镜像字段 | [参数提交函数](online_diag_display_review_1/functions/0x140c96650.c) |
| `0x00C966A0` | 根据线程模式直接写显示对象 `+0x5E9`，或排入 `0x01045700` 任务完成写入 | [显示任务分派函数](online_diag_display_review_1/functions/0x140c966a0.c) |

这解释了此前仅改一次显示缓存会在正常 tick 中复原的现象，也与 2.0 本地画面对照验证相符。这份局部调用链没有证明完整性检测不存在，也没有覆盖全部网络或统计消费者。

## 在线相关结论的边界

此前的 [本地实测记录](validation_2_0.md) 证实了显示、持续运行、关闭和还原；本次额外生成了真实 REA/Ghidra 反编译证据。公开匹配、天梯同步以及服务器端判定尚未完成验证。

当前实现通过可写进程句柄、`VirtualProtectEx`、`WriteProcessMemory` 和 `FlushInstructionCache` 修改六处代码及四个显示状态字段。可观察的代码修改仍存在；是否由特定客户端或服务端逻辑检查，需要独立证据。

[Relic 官方协议](https://content.relic.com/legal/eula_terms.html) 第 9、15 节说明了作弊相关处罚和暂停访问权。第 7 节列举某些游戏可能采用的 TPMs；该通用表述不证明 CoH3 当前安装了其中任何特定产品。

后续已完成 `AntiCheatWorldService` 的 RTTI、虚表、周期方法、玩家采样与显示字段交叉核对，并实际生成三组新的 REA/Ghidra 导出。服务会读取显示路径所影响的同类对象状态，并存在显示结果与另一实体允许条件不一致时追加内部记录的分支。当前公开匹配启用、实际运行别名、上传与处罚仍未知；2.0 尚未实现在线免检测／免封。详见 [检测相关逆向报告](online_diag_ac_report.md) 与 [字节／产物校验](online_diag_verification.json)。
