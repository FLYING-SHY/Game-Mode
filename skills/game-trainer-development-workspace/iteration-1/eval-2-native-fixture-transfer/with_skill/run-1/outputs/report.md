# case2 with_skill 实测报告

结果：create、独立 resolver、verify、self-test 四条命令退出码均为 0；verify passed=true，self-test 11/11 checks=true。两个 build 各两个 session 均完成三帧定位，最终 ground truth 核对四项全匹配。

## 输入、环境与身份

目标是离线 native64 风格模块/heap 镜像定位，不涉及真实游戏。使用 Python 3.11.9、Windows 10.0.26300、64-bit AMD64 Python。会话提供文件读写与 PowerShell 工具；已实际调用 Python CLI。未调用或宣称接通 CE、调试器、进程内存 MCP。模块头是 SIM\0，不是 PE/CLR，也没有实际游戏版本。

已加载 karpathy-guidelines，读取根 AGENTS.md、game-trainer-development/SKILL.md 及 fixture-lab/engine-routes references。元数据只用于取得 prompt；未读取其他运行输出。完整输入路径、大小与 SHA256、Python 路径、命令参数、UTC 起始时刻、真实计时与退出码见 [execution.json](execution.json)。生成器 SHA256 为 `1a3bd6f1ee7c29c65469f7cc09505170f023b49de233705325ae9295a3aff643`。25 个固定素材文件位于 [fixture manifest](fixture/manifest.json)，载入每个镜像时核对其 SHA256。

## 定位方法与可观察契约

[resolver.py](resolver.py) 为实际执行的独立算法，没有导入生成器的 scan/resolve/oracle_candidate，也没有硬编码两个 build 的 root/链/field 数值。

1. 对 manifest 做字段白名单投影，只将 id/build、module/heap base、镜像文件名和 hash 送入发现阶段；不传入 root/layout/health/decoy 地址或 expected_values。
2. 使用公开协议 AOB `48 8B 81 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9`，扫描整个 1024-byte module，逐个记录命中与解码值。通配符包括字段 displacement 和 RIP displacement。
3. 从实际指令字节读取 signed int32，计算 `root = module_base + hit + 14 + rip_disp`；字段偏移取首条指令的 signed displacement。0/多命中不采用，只有 unique 图像进入指针链分析。
4. 对每份 2048-byte heap 的所有可读 int32 起点逐字节扫描，按公开的受控变化记录 `[100,73,91]` 过滤。每 session 初始 100 有三个候选，三帧排除常量 `[100,100,100]` 和顺序错误 `[100,91,73]` 两条 decoy。
5. 从 module 根读取 uint64 manager，在 manager 到 heap 最后可读 uint64 起点间逐字节扫描指针槽。不预设结构大小或链偏移。要求三帧指向同一有效 actor，actor+解码字段偏移与唯一变化候选相交；每 session 只余一条链。
6. 按 build 合并，验证两个 session 的 root RVA、链偏移、字段偏移相同，各 session 独立解引用并记录绝对地址。先写 [candidates.json](candidates.json) 与 [resolver-evidence.json](resolver-evidence.json)，随后才由独立末尾函数读取 root/layout ground truth，写 [ground-truth-check.json](ground-truth-check.json)。最终 verify 再单独核对。

扫描预算由输入镜像区域大小限定：每 session 一份 unique module 的结构定位、三份 heap 的全域数据扫描；AOB 检查另覆盖三个模式各 1024 bytes。所有 typed read 限制在单个镜像区域内。没有跨会话缓存绝对地址。

## 推导结果

| build | root RVA | manager→actor 偏移 | health 偏移 | sessions |
|---|---:|---:|---:|---|
| A | 0x180 | 0x20 | 0x38 | A0、A1 |
| B | 0x240 | 0x30 | 0x58 | B0、B1 |

| session | root 绝对地址 | 解析字段地址 | 三帧实际值 |
|---|---:|---:|---|
| A0 | 0x140000180 | 0x200000438 | 100,73,91 |
| A1 | 0x140000180 | 0x2002004B8 | 100,73,91 |
| B0 | 0x180000240 | 0x201000458 | 100,73,91 |
| B1 | 0x180000240 | 0x2012004D8 | 100,73,91 |

unique 命中 RVA 为 0x40；A 的 RIP 位移是 306（0x132），B 是 498（0x1F2）。multi 的第二命中 RVA 为 0xA0，对应位移 A=210、B=402；两命中即使解到同一 root 也不采用。每个 session 的 unique/zero/multi 命中数均为 1/0/2，共 12 项。详细扫描、RIP 计算、manager/actor/slot 地址和消歧轨迹见 [resolver-evidence.json](resolver-evidence.json)。

## 验证与证据等级

| 项目 | 预期 | 实际 | 等级 / 证据 |
|---|---|---|---|
| create | 4 sessions、25 固定文件 | 退出 0，created=true | F / [完整输出](create.stdout.log) |
| resolver | 两 build、每 build 两 session 独立求解 | 退出 0，三帧与结构唯一交集；最终四项匹配 | S/F / [完整输出](resolver.stdout.log) |
| verify | 四 session、12 AOB、patch model 全通过 | 退出 0，passed=true | F / [完整输出](verify.stdout.log) |
| self-test | 11 项 checks 全通过 | 退出 0，11/11 true | F / [完整输出](self-test.stdout.log) |
| 实际进程 / 游戏操作 | 此任务无需执行 | not_run | G 未执行 |
| 新进程、换局、重连、读档 | 模型之外需要实测 | not_run；此处仅离线 session 模拟 | R 未执行 |
| 实机交付启动 | 独立产品依赖与启动测试 | not_run | D 未执行 |

self-test 覆盖 two_builds_two_sessions_decoys、aob_0_1_multiple_rip、normal_enable_disable、restore_conflict、last_byte_boundary、short_read、outside_image、negative_size、stale_session、decoy_rejected、deterministic。self-test 内使用 oracle 验证验证器分支，它不构成独立定位成绩。

verify 的 patch model 在独立 bytearray 中模拟 `0f8511000000 → 909090909090`；正常关闭恢复原字节，冲突 `cc9090909090` 保持现场，四项布尔均为 true。resolver 本身只读镜像，没有真实补丁安装、页面权限、线程同步或 trampoline 测试。

## 复现、范围与交付

运行 `python run_eval.py` 即可按 create → resolver → verify → self-test 顺序复现。完整绝对命令、cwd、退出码与实测耗时见 [transcript.md](transcript.md) 和 [execution.json](execution.json)。各步 stdout 与 stderr 均独立保存；所有 stderr 为空。self-test 的 TMP/TEMP/TMPDIR 指向 outputs/temp，生成器的临时实验也限定在指定 outputs 中。

所有写入限定在本 outputs；没有修改输入脚本、技能、original 或其他结果。输入公开了生成器、AOB、受控变化和答案布局，因此这是已知材料的算法实现与 F 级方法迁移验证，并非真正盲测。发现阶段隔离了 manifest ground truth，但阅读过公开材料的事实仍影响盲测性质。结果只支持这个离线模型；下一项真实验证需另有实际测试进程及受控动作数据，本次任务未进行。

没有估造 token 或会话总耗时。token 使用量未暴露；执行时刻及每条命令耗时均由运行器实际采集。主要交付为 [resolver.py](resolver.py)、[candidates.json](candidates.json)、[resolver-evidence.json](resolver-evidence.json)、[verify 输出](verify.stdout.log)、[self-test 输出](self-test.stdout.log)、[transcript.md](transcript.md)。
