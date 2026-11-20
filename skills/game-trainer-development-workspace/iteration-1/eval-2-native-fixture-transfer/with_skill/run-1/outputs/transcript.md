# case2 with_skill 执行简记

读取会话技能目录，加载 karpathy-guidelines；读取根 AGENTS.md、game-trainer-development/SKILL.md、fixture-lab.md、engine-routes.md 与 fixture_lab.py。只使用 eval_metadata.json 的 prompt；未读取其他运行输出。

以下命令与退出码来自实际 subprocess 执行；完整 stdout/stderr 单独保存。self-test 的临时目录通过 TMP/TEMP/TMPDIR 限定在 outputs/temp。

```text
"C:\Program Files\Python311\python.exe" F:\WorkSpace\Game\skills\game-trainer-development\scripts\fixture_lab.py create --output F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-2-native-fixture-transfer\with_skill\run-1\outputs\fixture
```
开始 UTC：2026-11-16T01:12:52.869777+00:00；实测耗时：0.069110 秒；退出码：0。

```text
"C:\Program Files\Python311\python.exe" F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-2-native-fixture-transfer\with_skill\run-1\outputs\resolver.py --directory F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-2-native-fixture-transfer\with_skill\run-1\outputs\fixture --output F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-2-native-fixture-transfer\with_skill\run-1\outputs
```
开始 UTC：2026-11-16T01:12:52.939198+00:00；实测耗时：0.073554 秒；退出码：0。

```text
"C:\Program Files\Python311\python.exe" F:\WorkSpace\Game\skills\game-trainer-development\scripts\fixture_lab.py verify --directory F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-2-native-fixture-transfer\with_skill\run-1\outputs\fixture --result F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-2-native-fixture-transfer\with_skill\run-1\outputs\candidates.json
```
开始 UTC：2026-11-16T01:12:53.013640+00:00；实测耗时：0.062727 秒；退出码：0。

```text
"C:\Program Files\Python311\python.exe" F:\WorkSpace\Game\skills\game-trainer-development\scripts\fixture_lab.py self-test
```
开始 UTC：2026-11-16T01:12:53.077962+00:00；实测耗时：0.091239 秒；退出码：0。

resolver 只向发现阶段提供 manifest 中的容器元信息（session/build、base、文件名、SHA256），扫描 AOB 并解码两个 displacement；逐字节扫描三帧 int32 候选，扫描 manager 后所有可读 uint64 指针槽，依靠变化序列和结构交集消歧。先保存候选，之后独立函数才读 ground truth 核对。

输入材料公开了生成器和答案布局，因此属于已知协议/材料的模型方法验证，并非真正盲测。未执行真实进程或游戏测试。token 数未暴露，未估造。
