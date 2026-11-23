# Lifecycle phase v4：严格 v3 引用提示入口

状态：离线 S/F 已验证，40 项测试通过；`realSamples=0`、`nativeWinDLLAttempts=0`。本目录只为 ROOT 提供审阅和后续采样入口。当前没有新增游戏测量或产品验收证据，`phaseDecision=not_evaluated`、`productAcceptance=false`、`operatorLabelIsObservedPhase=false`。

## 固定交付

| 文件 | SHA256 |
|---|---|
| [phase_probe_v4.py](phase_probe_v4.py) | `b8cc05b3ae3ed8c162bd6119b2f7896ec23f389ab273c1337c03eac3f1dcb23f` |
| [phase_probe_v4_selftest.py](phase_probe_v4_selftest.py) | `bf1e256e49e639bbf26124ab5a4656a45d006f0c39be877647adb90ec2582d01` |
| [offline-validation.json](offline-validation.json) | `22475fbb588b1afc70c98823f7382772f72b92d4158b4e0d8ec7d8139c6c560c` |

[freeze.json](freeze.json) 记录交付文件哈希、固定依赖、引用字节 SHA 和完整启动命令；不通过自动更新 pin 掩盖漂移。旧 v1/v2/v3、产品源码、旧 JSON 均保持原样，未读私有许可数据，未 commit/push。

## 实现 seam

1. 核对冻结 v3 文件 SHA，原字节 `compile/exec` 后调用其 `load_engine()`。由它逐层核对并加载冻结 v2/v1，保留当前 source-loader globals 的绑定及其真实文件名。
2. 显式替换返回的 v2 对象的 `parse_reference`，接受真实 v3 schema，不转换或伪装为 v1 引用。
3. 显式替换该对象的 JSON 报告 sink，输出 v4 schema 和来源说明；原 v3 的完整 attestation 保留在 `executionAttestation.frozenV3LoaderAttestation`。测量函数、控制流、Reader/ReadJournal、分区/全局预算、模块比较、进程身份检查和 handle close 的代码对象未变。
4. 外层只增加输出路径约束：解析后的输出路径须位于本目录；冻结 capture 继续以 `x` 模式独占创建文件，再开始来源/引用校验和进程工作。native 默认工厂只留给 ROOT 后续明确选择的 CLI，离线套件对 `ctypes.WinDLL` 从导入前起实施阻断。

冻结依赖：

- v3 `81ee593a762933f0cf119cbc6fdcafc37cfa687ddd171aad6aa6f99dbf7da66b`
- v2 `e2c56be1b7b49f242b4549545e03e4a50d765e5de9c9cf7b8473a9966525e7c0`
- v1 `21af7198c86ea5f53b06e9df737251d81a21dd345ec243b8974beca75ec1d723`

## 引用契约与证据边界

- 外部传入精确、小写的引用**原始字节** SHA；文件严格小于 8 MiB。JSON 拒绝重复键、非有限数。固定 v3 schema/script SHA，完整比较三层脚本 attestation、sourceBinding 的原始/有效 pin 及全部边界字段；sourceManifest 的全部路径/SHA/大小必须一致。
- 要求 `status=observed`、`handleClosed=true`、`sourceBindingVerified=true`、只读访问标记和全部 nonclaim 标记。模块固定 game SHA/imageSize、合法用户指针 base，保留 `reference.module.base/sha256/imageSize` 供冻结 capture 在当前模块 attest 后逐项比较。
- 要求相同 PID 与 UTC 100ns start，顶层和三组 `identityBefore/After` 均为同一存活进程，原始偏移严格为 `[0,1,3]`，`sameProcessIdentity=true`。运行/暂停只是历史 operator 标签。
- 每组至少一个唯一 HUD hint。minimap/tactical 为可选项：只要出现，就必须在全部三组拥有相同地址、完整双遍 proof；所有 proof 都检查，存在重复 kind、共享地址、缺失 recheck、歧义或不完整路径即报具体字段原因。选择此策略是因为 HUD 是最小必需对象，而地图控件的有无不应成为 phase 标签；真实两个正样本仍各提取三类。
- 对 initial/recheck 独立检查实际 nodes、kind/control/HUD/root vtable、父链、终端 root、向下 edge、inline/vector witness 的指针字节、偏移/计数/索引/摘要格式及 raw journal 读记录，重新计算结构一致性；`firstPassPath`、`bidirectional_path_observed`、`structureStable` 这些标签本身不够。flags192 的值和派生布尔亦互相核对，允许真实两遍 flags 不同。
- 引用 journal 逐条核对 ordinal、合法 span、成功/失败、实际字节数以及总计费，按每组累计 readAttempts 分段，分别确认 `ui.*` 和 `verify.ui.*` 对路径 body576/vector 的成功读记录。冻结日志仅对长度 <=8 的读保留 rawHex，576B 节点和较大数组没有完整原字节；这里验证保留下来的结构与 witness 自洽，来源以外部固定归档 SHA 绑定。报告中的脚本 SHA 也明确是来源陈述而非签名。
- 地址仅作提示。冻结 Observation 在**当前**内存对旧控件地址重新 trace/recheck 两遍，再读取/复核当前 root。历史 root 只作元数据，允许当前 root 不同。当前模块 base/SHA/imageSize 不同则停在失败；旧地址失效、缺少向下 membership、root 在两遍间变化等均保留 `unknown`。
- 原 `currentTreeCoverage=unknown` 如实记录。即使 `known_graph_complete` 也只是冻结已知图字段的覆盖结果，不构成 phase 或产品成功；当前 results 历史报告未取得 proof 的状态仍为覆盖未知。

## 离线验证

从 `F:\WorkSpace\Game` 运行下面命令；结果文件名须是本目录下的新文件：

```powershell
python -B -X utf8 CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v4\phase_probe_v4_selftest.py `
  --report CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v4\offline-validation-rerun-001.json
```

[offline-validation.json](offline-validation.json) 为最终本轮 40 项结果，0 fail/error/skip。表驱动反例在内存修改真实 JSON，不改旧 fixture。覆盖 SHA、PID/start100ns/每层 identity、module/source/三层 wrapper/schema、三样本集、缺失 recheck、歧义 hint、节点/edge/witness/template/vtable/journal、重复 JSON 键/非有限数，以及实际 `v4.capture` 的 fake-memory 双遍读取、不同当前 root、模块逐项比较、source/script 漂移、独占文件、路径约束、失败/短读计费、分区/字节/时间/全局预算、root 变化和 close。

Fake-memory 输出 sink 在内存中，每次实际 capture 返回值均与 sink 序列化内容比较；唯一新建的测试结果文件位于 v4。审计 hook 对所有 write-open 实施 v4 路径约束，`writesObserved` 包含对已有文件的独占创建碰撞尝试；碰撞测试亦核对原文件字节不变。结果保存历史根/v2/v3 全文件 before/after SHA，并证实一致。未重新执行整套 legacy 测试，报告明确 `legacyTestSuiteRerun=false`。

[offline-validation-initial.json](offline-validation-initial.json) 保留首轮 38 项中 2 个测试构造失败：真实首组 flags192Stable 已为 false，原反例赋了同值；预算测试在引用解析之前降低上限，先被历史 journal 上限校验挡住。只修正测试，并新增 2 项计费/分区测试后重跑；入口检查未放宽。

两个只读正样本：

| 引用 | 字节数 | 精确 SHA256 |
|---|---:|---|
| [running](../v3/live-round3/pid30504-running-001.json) | 5039164 | `760a92a813aac94c94444dc2ee1b2d3d01a941c648305a68488f0194e008ac59` |
| [paused](../v3/live-round3/pid30504-paused-001.json) | 5031347 | `ba29370662feeab744da3fa20cbf3812eb69ae5d9b2fd79fcf7f63f4591696fd` |

两份引用分别提取 `hud=0x1d3cc5ec570`、`minimap=0x1d3cca73b30`、`tactical=0x1d3ccc60fb0`；三组覆盖都记录为 unknown。

## 供 ROOT 审阅的启动命令

以下 CLI **未在本任务执行**。它以 paused 归档字节为地址提示，重新读取当前内存；输出只在 v4 新建文件。PID/start 必须仍符合目标进程，标签是 operator 记录，保持全部 nonclaim 标记。

```powershell
python -B -X utf8 CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v4\phase_probe_v4.py `
  --pid 30504 `
  --expected-start-utc 2026-11-22T01:23:35.3993114Z `
  --label results `
  --reference CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v3\live-round3\pid30504-paused-001.json `
  --reference-sha256 ba29370662feeab744da3fa20cbf3812eb69ae5d9b2fd79fcf7f63f4591696fd `
  --out CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v4\pid30504-results-reference-paused-001.json
```

作为其他记录场景时，只调整 `--label` 和新的 `--out` 文件名。若要选择 running 归档，则文件路径与 SHA 必须一起换成表中 running 值。任一源码 pin 漂移或身份不符都会失败并保留具体错误，不刷新 pin。

只核对来源、完全不访问进程的 CLI：

```powershell
python -B -X utf8 CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v4\phase_probe_v4.py --offline-attest
```
