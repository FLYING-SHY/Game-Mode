# case2 without_skill 执行简记

- 用户目标：独立运行 eval-2-native-fixture-transfer 的 without_skill/run-1；使用共同输入，实际写字节扫描 resolver，运行 create/resolver/verify/self-test，保存候选、日志、报告；所有写入限定本 outputs；不附加真实进程，不填造 token/time。
- 已检查会话可用技能目录，加载 karpathy-guidelines；读取根 AGENTS.md。未加载新建 game-trainer-development/SKILL.md，未读取其他 reference。
- 读取指定 eval_metadata.json 得到 prompt；read 工具同时返回同文件的空 assertions 数组，无任何断言信息用于实现。只读取指定 fixture_lab.py 与 fixture-lab.md 作为共同任务输入。共同输入已公开 oracle 布局，报告明确这不是隐藏答案盲测。
- 编写 [resolver.py](resolver.py)：从完整模块扫描 AOB，解码字段 disp32 和 RIP disp32；扫描三帧 heap 值变化；枚举 heap 指针槽，反推 manager→actor 偏移；同 build 两个 session 交叉核对布局。manifest 仅 allowlist 提取标识/base/文件名元信息。
- 编写 [run_eval.py](run_eval.py)：实际启动四个子进程，输出原始 stdout/stderr、JSON、实际 UTC 时间与 perf_counter 耗时；TMP/TEMP 指向 outputs/temp，避免 self-test 在目录外产生临时素材。
- 实际运行 `python <本目录>/run_eval.py`。解释器 C:\Program Files\Python311\python.exe。四步 create、resolver、verify、self-test 全部退出码 0，无重试，无后台任务。
- 实际执行时间：2026-11-16T01:12:12.172180+00:00 开始 create；四步记录见 [execution.json](execution.json)。未记录独立 token 数，没有编造全会话耗时。
- 读取并检查 [verify.json](verify.json)、[resolver-evidence.json](resolver-evidence.json)、[self-test.json](self-test.json)：verify passed=true，A/B 两 build × 两 session 全通过，12/12 AOB 行通过，11/11 self-test checks 通过，patch model 四项 true。
- 每 session 三个首帧同值候选变为一个 [100,73,91] 目标，排除常值和反序变化 decoy。布局实际推导结果 A: root 0x180 / link 0x20 / field 0x38；B: root 0x240 / link 0x30 / field 0x58。
- 写入 [report.md](report.md) 记录方法、数值、完整日志索引、实际耗时与 F 级证据边界。未执行真实进程/游戏测试。
- 交付目录：F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-2-native-fixture-transfer\without_skill\run-1\outputs。
