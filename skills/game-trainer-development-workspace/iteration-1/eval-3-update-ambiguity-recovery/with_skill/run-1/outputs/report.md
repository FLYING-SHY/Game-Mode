# 更新歧义与恢复修复协议

本次交付是纯 Python 标准库、内存 bytearray 构造的 F（fixture）级验证。真实执行 11 个 unittest 测试，0 failures、0 errors，退出码 0。实际游戏 G 与真实重复会话 R 均为 `not_run`。没有附加进程，也没有执行任何游戏内操作。

## 输入、能力与身份

- 已读根 AGENTS.md；只在本次 outputs 目录产生交付物，冻结原版与既有工程未参与。
- 加载 karpathy-guidelines；读取 game-trainer-development/SKILL.md，以及 engineering.md、fixture-lab.md。采用按 build 校验、语义筛选、会话绑定、补丁所有权恢复原则。
- 只使用 eval_metadata.json 的 prompt 执行任务，没有采用 assertions 或其他运行输出作依据。
- 实测工作目录 F:\WorkSpace\Game；Python 路径 C:\Program Files\Python311\python.exe，版本 3.11.9，64 bit AMD64；使用 read/glob/pwsh/write 工具。任务所需离线能力已实测。真实游戏、动态附加、调试器未运行。
- 自制镜像大小 0x500，开头 SIM\0，并非 PE。模块基址默认 0x140000000；模拟代码范围 [0x100,0x280)，数据范围 [0x300,0x500)。第二会话另用基址 0x140200000、PID 22、start 200、epoch 3。
- 原 build SHA256：c0f7dc8b8d8e06175b692f0cd20cbe58a5e498665f07263ede4746f78523c276。
- 双候选新 build SHA256：c2231bd4300365ba5ceb14952f0c61e334092356afb815053448fbe1b20069f9。
- 0 命中新 build SHA256：39ffc3e7bcc47b91d4eb723d5025dd461cf87dd28457bb5c56569bee5ae99bdc。
- 两个语义候选均成立的 build SHA256：4bee85e1eb5641ccc1811c08fd945a3c1cb75198f54aad95ae0daadbf17dbf27。

这是显式构造测试，不是盲测定位成绩。fixture() 构造素材；resolve() 只扫字节、解码并验证语义，无答案表输入。locate() 自动登记 fixture hash 仅用于让预期失败素材到达待测试分支；部署时应传入人工审核的 approved build 清单。专门的 build gate 测试确认仅登记旧 build 时新 build 被阻止，且读取内容 hash 不符时停止。

## 可执行修复顺序

1. 冻结旧 build 的 hash、架构、模块名、模式、布局、原字节与测试记录。对每个新 build 建独立资料；保留 build gate。未知 hash 保持 disabled/resolving 状态，直到适配审核完成。
2. 记录完整扫描段和字节预算。本模型固定完整代码范围，预算不足报告 scan_budget_incomplete，不能将未扫描完解释成 0 命中。实际工程应再添加时间上限及取消令牌。
3. 使用模式 `48 8B 0D ?? ?? ?? ?? 48 85 C9 0F 85 ?? ?? ?? ??`。可变 RIP disp32 与分支位移为通配，稳定 opcode/context 保留。记录全部命中及每个排除理由，禁止取首个来消除歧义。
4. 0 命中报告 signature_zero，停止写入，核对 build/段/范围及代码变动。1 命中仍继续指令与结构检验。多命中逐一验证；恰好一个语义候选可成为该模型内的 ready；0 个或多个语义候选均停止。模型中的 ready 尚需真实行为实验后才能成为产品可用配置。
5. 本模式首条是 7 字节 `mov rcx,[rip+disp32]`。以 little-endian 有符号 int32 读取 hit+3，计算 `rootVA = moduleBase + hit + 7 + disp32`，再由 root 处读取 uint64 对象指针。先检查 64 位范围、数据区、对齐和读取长度，再访问目标。不是把 disp32 当绝对地址，也不是把 root 存储槽当对象本身。
6. 对象布局为 `<4sIiiI`：ACTR 类型标记、owner uint32、hp int32、max_hp int32、count uint32。独立不变量为 owner==7、0<=hp<=max_hp<=10000 且 max_hp>0、1<=count<=16。真实游戏须通过受控动作证明该对象属于目标角色，排除显示缓存、共享对象和派生值。
7. 将解析结果和补丁所有权绑定完整 Session(PID,startTime,moduleBase,buildHash,epoch)。使用缓存、开启、关闭前重新核对；任一字段改变进入 stale，停止写入。旧会话的补丁记录仅供诊断，不向新会话地址回写。新进程/切局/重连要取消旧扫描并重新解析。
8. enable 顺序：核对会话→重验结构→比较完整原字节→保存原字节→执行→读回→登记 owned/enabled。本例在 hit+10 将完整 6 字节近 JNE `0f8511000000` 替换为 `909090909090`。重复开启只有 owned 且当前仍等于自己的补丁才幂等。保存的是安装前精确字节，不是依赖另一 build 的旧常量。
9. 安装短写：模型注入一次 5/6 字节写入；仅当现场等于本次已知部分写入结果时回滚，读回复核后仍进入 faulted，要求重新解析。生产适配器需精确区分已写长度、未知写入结果和并发冲突，保留证据。
10. disable/正常工具退出共用关闭路径：同会话、owned、当前完整字节等于安装字节时，才恢复原字节并读回；重复关闭幂等。若当前是 `cc9090909090`，记录 restore_conflict、保持 owned 记录和冲突现场、进入 conflict，停止后续操作，供人工比较。恢复短写进入 faulted，保留记录。
11. 真实指令补丁集成时另验证完整指令边界、线程同步、页权限、指令缓存刷新、x64 ABI、分支范围及 RIP 重定位；模型只替换固定已知的一条指令，不构建 trampoline、不验证并发原子性。

## 真实运行与证据

运行命令（在本 outputs 目录）：

```powershell
$testOutput = python .\offline_repair_test.py 2>&1
$testExit = $LASTEXITCODE
$testOutput | Set-Content -Encoding utf8 .\run.log
"exit_code=$testExit" | Add-Content -Encoding utf8 .\run.log
$testOutput
exit $testExit
```

直接运行 `python offline_repair_test.py` 同样执行全部测试，失败时返回非零退出码；运行不生成游戏文件。日志由命令真实捕获。开始时间由执行中 datetime.now 取得：2026-11-16T01:15:33.998103+00:00。unittest 实测 `Ran 11 tests in 0.003s`，它是测试阶段耗时，不是任务全程耗时。未记录 token 用量，未推算时间或 token。

| 测试 | 预期与实际 | 等级 |
|---|---|---|
| old_unique_and_new_multi_choose_semantic_second | 旧版唯一 0x120；新版原始 0x120、0x1a0，首个 object_owner 不符，选择第二个；root=0x308,obj=0x3c0 | F，pass |
| zero_and_unresolved_multi_stop | 0 命中 signature_zero；双语义候选 semantic_ambiguity；两者 writes==0 | F，pass |
| unique_still_requires_semantics | 单命中但 hp=101,max=100，health_semantics 排除 | F，pass |
| signed_rip_and_expected_opcode | 正位移 root 0x300；负位移从 0x200 解到 0x80；错误 49 opcode 停止 | F，pass |
| rip_outside_region_is_rejected | RIP 解到 0x700，root_region 排除 | F，pass |
| build_gate_and_bounded_scan | 未登记新 hash、预算不完整、内容 hash 被改变各自停止 | F，pass |
| stale_session_all_identity_dimensions_no_write | PID/start/base/build/epoch 分别变化，启停均 stale_session，内存和写次数保持不变 | F，pass |
| normal_repeated_enable_disable_and_exit_cleanup | 两个独立构造会话，重复开启幂等，重复关闭幂等，两轮恢复全镜像一致 | F，pass；真实 R not_run |
| original_mismatch_cached_semantics_and_partial_rollback | 原字节位移变化停止；缓存 owner 变化停止；5/6 写入失败恢复原镜像并 faulted | F，pass |
| restore_conflict_preserves_other_component | cc9090909090 保持原样，写次数不增加，状态 conflict，之后 enable 停止 | F，pass |
| read_bounds | 负 RVA、负长度、越界读取停止；末字节可精确读取 | F，pass |

所有判定使用 unittest 断言、异常检查、完整 bytearray 比较及写入次数比较，并非仅打印 passed。日志 observations 保存实际扫描与排除结果。

## 待真实游戏验证清单

全部为 not_run：

- 对实际旧/新模块记录磁盘 hash、加载模块身份、架构、代码段和运行时原字节；验证真实页面权限和精确读写结果。磁盘 build 身份与会话对象应分离，别对已装补丁的模块重复套用 pristine-image hash。
- 实际反汇编确认 mov/test/JNE 的指令边界、RIP 目标与对象布局。确认 0 命中版本需要新模式还是布局迁移，不通过关闭 build 校验解决。
- 受控动作前后至少三帧观察目标对象及 decoy，验证 owner/数值变化和功能目标，不只根据初始结构值判断。
- 同 build 至少两次新进程、不同 ASLR、加载存档/切局/重连，核对取消旧扫描、epoch 失效、重新解析和缓存清理。
- 单次与反复启停，功能开/关的行为差异、恢复后的游戏行为、正常关闭、进程已退出的停止路径；另验证读写失败和断连的诊断状态。
- 并发其他组件修改字节时保留冲突证据；真实线程协调、写入原子性、页保护恢复和指令缓存；若增加 trampoline，证明无线程执行后再释放资源。
- 从最终交付目录在目标机器运行、核对依赖和默认关闭状态；真实 build 审核通过后才更新对应 profile。

## 交付身份

- [offline_repair_test.py](offline_repair_test.py)：可运行实现与 11 个测试。SHA256 BC88E4F498C51CA2AD27B3B702D4436B4DFF179D09D79F840D4F8B17FAAD8B28。
- [run.log](run.log)：原始真实运行日志。SHA256 5244C4B4D8F23ACD7420273BB3841108F238B61D9C6B39856CD9DCA4D0AEE94E。
- [transcript.md](transcript.md)：工具与命令结果简记。

没有提交或推送。没有将 fixture 成功表述为真实游戏有效。
