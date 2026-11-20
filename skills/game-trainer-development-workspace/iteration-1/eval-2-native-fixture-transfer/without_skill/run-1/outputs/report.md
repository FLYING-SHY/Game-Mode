# case2 without_skill：离线 native fixture 结果

实际运行结果：create / resolver / verify / self-test 退出码均为 0；verify `passed=true`；self-test `passed=true`，11/11 checks 通过。

## 输入与执行边界

本轮加载现有 karpathy-guidelines，读取仓库根 AGENTS.md、指定 eval metadata 的 prompt、共同输入 fixture_lab.py 与 fixture-lab.md。未读取新建 game-trainer-development/SKILL.md、其他 reference 或其他评估分支输出。metadata 读取工具返回整个短文件，包含空 assertions 数组；未使用任何断言或评分内容。

共同输入本身公开包含布局答案，因此本轮是公开格式下的字节定位实现与复现，不能解释成隐藏答案的盲测。定位程序没有把公开 root/field/link 数值写成定位常量，也没有调用 oracle_candidate。manifest 读取后通过 allowlist 只提取 session/build 标识、module/heap base、镜像文件名和帧顺序；pointer_root、field_layout、health_address、decoy_addresses、expected_values 均未参与定位。官方 verify / self-test 内部使用答案表属于验证器行为。

所有本轮写入均在 outputs 内，self-test 的 TMP/TEMP 也指向 outputs/temp。没有附加真实进程，没有真实游戏操作。

## 定位算法与证据

实现见 [resolver.py](resolver.py)，逐 session 原始证据见 [resolver-evidence.json](resolver-evidence.json)。

1. 对模块完整字节范围执行带 wildcard 的 AOB 扫描，不预置命中位置。实际 unique 命中 RVA 0x40，zero 无命中，multi 命中 0x40 / 0xA0；仅唯一命中可进入链解析。四 session × 三模式共 12 个扫描记录。
2. 从首条指令 hit+3 解码有符号 int32 字段 displacement。从第二条指令 hit+10 解码有符号 RIP displacement，计算 `root = module_base + hit + 14 + disp32`。这里 3、10、14 是所识别指令编码的位置及长度，不是目标结构偏移。
3. 从 root 读取 manager。按 4 字节对齐扫描三帧 heap 的 int32 值，使用共同输入给出的受控实验序列 `[100,73,91]`。每 session 第一帧均有三个 100，后两帧排除 `[100,100,100]` 和 `[100,91,73]`。
4. 按 8 字节对齐枚举 manager 及其之后映射 heap 内的所有指针槽，不提供链偏移常量。要求指针在 heap 区域内、各帧稳定，且 `pointer + decoded_field_displacement` 等于变化扫描唯一目标；由 `slot_address - manager` 反推出链偏移。
5. 同 build 的两个 session 必须获得完全一致的 root RVA / chain offset / field displacement；每个 session 绝对字段地址分别计算并提交。

| Build | 扫描所得 root RVA | 扫描所得 ptr_offsets | 解码所得 health_offset | Sessions |
|---|---:|---:|---:|---|
| A | 0x180 | [0x20] | 0x38 | A0, A1 |
| B | 0x240 | [0x30] | 0x58 | B0, B1 |

A unique：next RIP = 0x14000004E，disp32 = 0x132，root = 0x140000180。B unique：next RIP = 0x18000004E，disp32 = 0x1F2，root = 0x180000240。multi 的第二命中使用不同 displacement，仍解出同一 root；即使两次解码 root 相同，多命中也标记 accepted=false。

| Session | manager | actor | 字段地址 | 三帧读取值 | 初始候选 → 最终 |
|---|---|---|---|---|---|
| A0 | 0x200000100 | 0x200000400 | 0x200000438 | [100,73,91] | 3 → 1 |
| A1 | 0x200200100 | 0x200200480 | 0x2002004B8 | [100,73,91] | 3 → 1 |
| B0 | 0x201000100 | 0x201000400 | 0x201000458 | [100,73,91] | 3 → 1 |
| B1 | 0x201200100 | 0x201200480 | 0x2012004D8 | [100,73,91] | 3 → 1 |

完整提交候选见 [candidates.json](candidates.json)，官方验证结果见 [verify.json](verify.json)。四个 session 字段地址和变化序列全部通过，12/12 AOB 测试通过。bytearray patch model 的 enabled、disabled_restored、restore_conflict、conflict_preserved 四项均 true。

## 实际命令与测量

执行入口 [run_eval.py](run_eval.py) 用当前 Python 启动以下四条命令；完整参数、解释器版本、UTC 开始时刻、实际 perf_counter 耗时和退出码保存在 [execution.json](execution.json)。

```powershell
python F:\WorkSpace\Game\skills\game-trainer-development\scripts\fixture_lab.py create --output .\fixture
python .\resolver.py --directory .\fixture --output .
python F:\WorkSpace\Game\skills\game-trainer-development\scripts\fixture_lab.py verify --directory .\fixture --result .\candidates.json
python F:\WorkSpace\Game\skills\game-trainer-development\scripts\fixture_lab.py self-test
```

上面的相对路径以本 outputs 为 cwd；入口实际使用绝对路径。

| 步骤 | UTC 开始时间 | 实测秒数 | Exit |
|---|---|---:|---:|
| create | 2026-11-16T01:12:12.172180+00:00 | 0.06790539997746237 | 0 |
| resolver | 2026-11-16T01:12:12.241721+00:00 | 0.05906449997564778 | 0 |
| verify | 2026-11-16T01:12:12.302123+00:00 | 0.06381970000802539 | 0 |
| self-test | 2026-11-16T01:12:12.367622+00:00 | 0.09115910000400618 | 0 |

这些仅为子进程实测运行耗时，不代表完整会话耗时。没有可用的独立 token 计数，本报告不填造 token 数。

完整 stdout / stderr：

- [create.stdout.log](create.stdout.log)、[create.stderr.log](create.stderr.log)
- [resolver.stdout.log](resolver.stdout.log)、[resolver.stderr.log](resolver.stderr.log)
- [verify.stdout.log](verify.stdout.log)、[verify.stderr.log](verify.stderr.log)
- [self-test.stdout.log](self-test.stdout.log)、[self-test.stderr.log](self-test.stderr.log)

四份 stderr 均为空。[self-test.json](self-test.json) 的 11 项检查为：two_builds_two_sessions_decoys、aob_0_1_multiple_rip、normal_enable_disable、restore_conflict、last_byte_boundary、short_read、outside_image、negative_size、stale_session、decoy_rejected、deterministic，全部 true。

## 证据等级与适用限制

本结果属于 F（fixture）级离线模型验证：SIM 模块 / heap 文件、已知受控值变化、单级指针链、指定 AOB 编码和 patch bytearray 模型。resolver 假设 int32 四字节对齐、uint64 八字节对齐、manager 后非负链偏移和一层 manager→actor 链；这与共同输入匹配，不构成通用 native 分析器。

self-test 是工具自带的 oracle 驱动验证器回归，独立字节定位证据由本轮 resolver 和 evidence 文件提供。真实 PE/ELF 装载、真实 ASLR、进程附加、系统内存 API、并发读写、页保护、实际补丁安装、线程一致性、trampoline 和真实游戏功能均尚未执行。
