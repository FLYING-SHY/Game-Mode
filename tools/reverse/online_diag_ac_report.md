# CoH3 2.0：在线检测相关静态逆向结果

日期：2026-11-08。样本：Steam build `26443338`，游戏版本 `3.0.0.52071`。

**当前 2.0 尚未实现公开匹配／天梯的免检测或免封。** 本次 REA/Ghidra 分析发现，游戏自带的 `AntiCheatWorldService` 会读取显示工具所影响的同类对象状态，并有“显示结果与另一项实体允许条件不一致时追加内部记录”的分支。因此，“只修改本地显示，所以检测逻辑看不到变化”这一推断不成立。

这证明存在一条相关的静态观察路径；尚未证明它在某次真实公开匹配中的启用状态、具体运行对象别名、数据上传、服务端判定或处罚结果。本轮只读取磁盘样本、生成分析副本和文档，没有修改游戏进程、源码或已发布的 2.0 程序，也没有进入公开匹配。

## 1. 工具来源与样本

- 实际使用已安装的 REA `6.8.0`，包目录为 `F:\BaseEnv\nvm\v24.9.0\node_modules\rea-agents`。命令入口为 [rea.cmd](<F:/BaseEnv/nodejs/rea.cmd>)；该 Node 目录链接到 `F:\BaseEnv\nvm\v24.9.0`。
- 用户指定的源码目录 `F:\WorkSpace\github\rea` 是同一项目的源码 checkout，但缺少编译产物。源码与安装包的入口、Ghidra bridge 哈希不同，本次没有将二者称为同一构建，也没有安装或升级依赖。
- 实际 Node `24.9.0` 不满足该包报告的支持范围 `^22.23.4 || ^24.22.1 || >=26.12.1`。下列 REA 命令实际成功，但不据此将运行环境称为受支持组合。
- Provider：Ghidra `12.1.4`，JDK `25`。详细来源与显示链首轮复核见 [REA 来源报告](online_diag_rea_review.md)。

```text
原始游戏 SHA256
21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca

分析副本一 SHA256（13 组 / 27 段 / 4022 字节选定代码）
d46761f4c5dd442487197bb4063e951a0d8ddfa99058c9455059a10da2afff8d

分析副本二 SHA256（18 组 / 32 段 / 7052 字节选定代码）
d9dec704f8dc48aee6253acc53443af2071aa7c244345672fd713fda6a2966cd
```

两份副本保持原始 RVA、数据页写权限、所选 `.pdata` 与链式 unwind 的非连续冷分支。它们是局部分析样本，不是完整游戏数据库。原始游戏摘要、两份副本摘要、全部选定函数片段与原文件逐字节一致性，已由 [校验脚本](online_diag_verify_evidence.py) 验证；结果见 [机器可读校验记录](online_diag_verification.json)。

## 2. 自带服务不是只有名字的空线索

MSVC RTTI 将虚表 `RVA 0x03D7D298` 绑定到 `AntiCheatWorldService`，COL 为 `0x05029FA8`，TypeDescriptor 为 `0x0604A890`。16 个虚表槽对应 6 个不同实现入口；其中周期方法为 `RVA 0x00CEED80`。

[周期方法伪码](online_diag_ac_methods_1/functions/0x140ceed80.c) 显示，它按计数和配置条件执行实体统计与玩家采样。其冷分支属于该函数的异常链，已一并纳入分析；仅看第一个 `.pdata` 区间会漏掉这些行为。

[实体统计伪码](online_diag_ac_methods_1/functions/0x140ceee30.c) 遍历实体，调用 `RVA 0x005DAAD0` 查询显示对象，读取返回对象的 `word[+0x6A]`、`byte[+0x69]` 和渲染位图相关字段，记录计数。记录对象的 RTTI 是 `VisibilityStatusData`，证据见 [记录类型与调用清单](online_diag_ac_records.json)。

这些事实支持“服务会观察显示状态”的结论。它们不意味着每次进入游戏、每种模式或每个玩家都必然执行该分支；在线启用配置仍未知。

## 3. 显示路径与服务路径确有对象数据流关联

| 路径 | 原始指令关系 |
|---|---|
| 显示更新 | `1E97B0: call 5DAAD0` → `1E97C0: mov rcx,rax` → `1E97C3: call 1EA840`，返回指针没有调整 |
| 字段更新 | `1EA874` 更新该对象 `byte[+0x69]` 的 mask；本次显示路径传入 mask `2` |
| 服务统计 | `CEEF41: call 5DAAD0`，随后读取返回对象 `word[+0x6A]` 与 `byte[+0x69]` |

两条路径的 registry 指针链和 `entity+0x88` 句柄来源相同。静态证据确定了同一种 resolver 返回布局；是否在具体时刻命中同一实例，还要求 registry、handle 和映射状态相同，本次未做运行地址别名验证。

`+0x69` 是组合 mask；`+0x6A` 为带符号的层级／计数候选。它们共同影响显示抑制条件，并向子对象传播。**现有证据没有把这两个字段命名为“作弊标志”或“检测次数”。** 完整字节与数据流分析见 [对象交叉报告](online_diag_entity_crosscheck_report.md)。

## 4. 不一致会进入内部记录

[本地玩家采样伪码](online_diag_ac_predicates_1/functions/0x14374bab8.c) 的关键分支可概括为：

```text
显示对象存在
且 signed16(obj+0x6A) <= 0
且 byte(obj+0x69) == 0
    → 认为该对象处于该显示分支所计数的状态
    → 调用另一项实体允许谓词
    → 谓词返回 false 时，追加一条带内部值 0x800 的 12 字节记录
```

这是对已查机器分支的概括，不是完整检测器或处罚规则。`0x800` 的官方枚举名称尚未恢复；追加到内存容器不等于已证明上传或封禁。

该实体允许谓词还检查实体状态、特殊规则、祖先对象，并对实体的空间范围查询两类表。它不是简单读一个字段。两类表尚未可靠绑定为“当前可见”“已探索”等业务名称。原始汇编补足了 REA 在外部调用参数恢复上的缺失，见 [谓词补充报告](online_diag_entity_crosscheck_predicate_report.md)。

[样本比对方法](online_diag_ac_predicates_1/functions/0x14374b088.c) 还比较不同来源的计数和样本，包含追加其他内部记录的分支。本次没有把其虚接口继续绑定到网络传输、服务器裁决或账号处罚。

**对 2.0 的实际含义：直接没有写模拟状态或遥测字段，不等于显示变化不会被其他统计消费者观察。** 此前只凭“本地显示修改”排除检测的说法应撤回。

## 5. 实际 REA 导出与可重现证据

所有三次父任务导出均成功，导出失败数为 `0`。第一轮有意限制 128 个函数，其余 59 个未导出；后两轮限定地址补齐相关采样与谓词，不声称全程序覆盖。

| 导出 | 写出 / 剩余 | Evidence ID |
|---|---:|---|
| [周期与统计方法](online_diag_ac_methods_1.json) | 128 / 59 | `ev_760b74116f9512c8543362892ab131877fff10e0192562be91c9c0089d6282b4` |
| [两个玩家采样方法](online_diag_ac_samples_1.json) | 2 / 0 | `ev_5e6e1ef4c56338abbf438e696dba8844010eab5ef095666f640ac086a1a83f19` |
| [样本比对与实体条件](online_diag_ac_predicates_1.json) | 8 / 0 | `ev_6fd183d66ea693f22d7027ad556d3c22bc63096d66feea81370c73db2932a77f` |
| [显示对象交叉核对](online_diag_entity_crosscheck_rea.json) | 单函数证据 | `ev_7e33dbb21459112f452c5b84ad12dc24c715f583d6f6c1f391f611b0e74364f3` |

父任务实际调用形式：

```powershell
& 'F:\BaseEnv\nodejs\rea.cmd' decompile-all '.\tools\reverse\online_diag_ac_rw.exe' --provider ghidra --limit 128 --output '.\tools\reverse\online_diag_ac_methods_1' --format json
& 'F:\BaseEnv\nodejs\rea.cmd' decompile-all '.\tools\reverse\online_diag_ac_rw.exe' --provider ghidra --address-start 0x14374B890 --address-end 0x14374BF56 --limit 16 --output '.\tools\reverse\online_diag_ac_samples_1' --format json
& 'F:\BaseEnv\nodejs\rea.cmd' decompile-all '.\tools\reverse\online_diag_ac_rw2.exe' --provider ghidra --address-start 0x14374B088 --address-end 0x14374C328 --limit 64 --output '.\tools\reverse\online_diag_ac_predicates_1' --format json
```

复现时须使用新的输出目录；REA 会拒绝覆盖已有导出目录。环境使用 `GHIDRA_INSTALL_DIR`、`JAVA_HOME`、启动期限 `120000ms`、堆上限 `2G`，禁用 `Decompiler Switch Analysis,Aggressive Instruction Finder,Decompiler Parameter ID`。生成样本的 [脚本](online_diag_ac_carve.py) 当前为第二版；第一版／第二版的实际片段清单分别保存在 [清单一](online_diag_ac_carve.json) 和 [清单二](online_diag_ac_carve2.json)，现有样本没有被覆盖。

外部函数 ABI、部分返回类型和虚调用落点仍有不完整恢复。某些 C 输出保留 unresolved/no-return 提示；原始汇编优先于推测类型。校验通过证明所选字节与产物来源一致，不证明所有反编译类型或业务解释正确。

## 6. 已验证与未知

| 问题 | 本次结果 |
|---|---|
| 2.0 在本地遭遇战显示地形、敌方基地及实体，开关／还原有效 | 已有 [本地实测记录](validation_2_0.md) |
| 客户端有会观察相关显示状态的自带服务 | 本次静态方法与数据流证据支持 |
| 存在显示结果与实体允许条件不一致时的内部记录 | 本次样本分支证据支持 |
| 当前公开匹配／天梯一定启用上述路径 | 未验证 |
| 实际运行时两个路径命中同一显示实例 | 未做运行地址核对 |
| 本次工具实际触发这些记录 | 未做运行触发实测 |
| 记录如何上传、服务端怎样判定、是否会封号 | 尚未追明 |
| 2.0 已经是免检测／免封版本 | 尚未实现，也没有该验证证据 |

[Relic 官方协议](https://content.relic.com/legal/eula_terms.html) 第 9、15 节明确保留作弊相关处罚和暂停访问权；协议没有披露本次路径的启用、上传或处罚条件。第 7 节的通用产品举例也不证明 CoH3 当前部署了某个特定第三方产品。

本轮交付是检测相关的逆向证据和结论纠正，没有发布新的隐蔽版程序。已发布 2.0 的本地效果验证依然有效，不能由此延伸为公开匹配的免检测或免封保证。
