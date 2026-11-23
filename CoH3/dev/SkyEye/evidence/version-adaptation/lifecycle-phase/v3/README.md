# v3：冻结 v2 测量引擎的显式本轮来源绑定

本目录复用原 v2 的测量代码，适用于本轮 CoH3 `5.1.52426.0`。v1/v2 脚本、测试、README 和历史报告均保持原样。本子任务只完成静态审阅、来源核对和 fake-memory fixture；`realSamples=0`，没有连接游戏、窗口或实际 native API。root 后续真实采样独占 `v3/live-round3/`，不属于这些 offline 文件。

## 冻结清单

所有 SHA256 为文件实际字节哈希；来源路径相对 `F:\WorkSpace\Game\CoH3\dev\SkyEye`。

|角色|路径|SHA256|
|---|---|---|
|v3 入口|`evidence/version-adaptation/lifecycle-phase/v3/phase_probe_v3.py`|`81ee593a762933f0cf119cbc6fdcafc37cfa687ddd171aad6aa6f99dbf7da66b`|
|v3 离线测试|`evidence/version-adaptation/lifecycle-phase/v3/phase_probe_v3_selftest.py`|`7d0417a9b6a126a80e95695810b8583847ca41ff50f58715094c6119b1b6eeac`|
|实际 v2 测量引擎|`evidence/version-adaptation/lifecycle-phase/v2/phase_probe_v2.py`|`e2c56be1b7b49f242b4549545e03e4a50d765e5de9c9cf7b8473a9966525e7c0`|
|v2 固定加载的 v1 基础|`evidence/version-adaptation/lifecycle-phase/phase_probe.py`|`21af7198c86ea5f53b06e9df737251d81a21dd345ec243b8974beca75ec1d723`|
|原样复跑的 v2 测试|`evidence/version-adaptation/lifecycle-phase/v2/phase_probe_v2_selftest.py`|`944ea751f2917c95476b1c60de447059f530e8e45405e29bc797db2b18003055`|
|本轮布局表|`client/CoH3MemoryTool/GameLayout52426.cs`|`90af6a0f98c5c3c48dd9cad8595377ee90990938753e68b142899b707c596d47`|
|本轮 Reader|`client/CoH3MemoryTool/PlayerInfoReader.cs`|`39dde7a0f2bd80e2412fed3439870cf7a49a794a9aab0f812a9afee6f60c65d2`|
|保留的来源说明脚本|`evidence/version-adaptation/air/air_raw_snapshot.py`|`0438ebbe4b29bcae3b6d428e46ce2db0fcdf58c87d93f6058f0432dc9ff765d4`|

Reader 经 `unknown-build-fix` 冻结后独立复核上述哈希。当前 `UsesCandidateLayout()` 仅对 Build52426 返回 true、Build2406 返回 false；未知 build 抛出布局未登记异常。该修复未改变本探针借用的单独角色、字段和路径。布局表的固定游戏 SHA 为 `8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec`，映像大小为 `120369152`。

## 最小接缝与执行关系

1. v3 检查固定 v2 文件 SHA，从同一批字节 `compile`/`exec` 原始 v2；`__file__` 与 code `co_filename` 都保留实际 v2 路径。
2. 原 v2 的 `load_base()` 原样检查固定 v1 SHA，并从其原始字节加载 v1。不存在源码替换、AST 改写或 SHA 冒充。
3. v3 使用 `FunctionType` 复用 v1 `load_sources.__code__`，为该函数复制 globals，仅把该副本的 `SOURCE_PINS` 替换为上述固定本轮来源。原来的哈希校验、固定游戏 SHA 校验及每个角色的 RVA 对照原样执行。
4. v1 模块本身的旧 `SOURCE_PINS` 保持原值。因此 v2 的 `parse_reference()` 仍要求真正的旧 v1 来源清单；其 byte SHA、schema、PID、精确 start、module base/hash/image、0/1/3 samples 等检查均保留。v3 报告没有转换成 v1 引用的路径。本轮没有同 PID 的真实旧 v1 引用时，直接省略 `--reference`；当前 root 的双向路径观测照常进行。
5. 仅给 v2 模块局部的 `json` 名字挂接 dump adapter。它在采样、finally 清理结束后加入 v3 来源字段；`loads` 等其他 JSON 操作委托给原库。测量函数和类未修改。
6. v3 限制输出到其目录树，原 v2 的 `open('x')` 保留；源 pin 失败、参考失败和预算停止仍由原 v2 捕获并输出部分证据。v1/v2 脚本 pin 失败在加载执行前终止。

没有自动接受新源、自动刷新 pin 或命令行 pin override。下一次任何被 pin 的文件发生变化，先核实差异，再新增显式版本或审计变更，保留现有冻结证据。

## 报告 schema 与 attestation

|字段|准确含义|
|---|---|
|`schema = known-hud-phase-observation-v3`|由 v3 入口输出、使用冻结 v2 测量引擎的报告|
|顶层 `scriptSha256`|v3 wrapper 在加载时读到的磁盘字节 SHA；自报告，不是签名或独立代码证明|
|`frozenEngineReport.schema/scriptSha256`|保留原 v2 报告的 schema 和 v2 自读磁盘 SHA，不将 v2 假称为 v3|
|`frozenEngineReport.reportedDiskShaMatchesExecutedSource`|v2 自读磁盘 SHA 是否仍匹配已验证且实际执行的冻结 v2 字节；来源失败尚未走到此处时为 null|
|`executionAttestation.wrapper`|wrapper 实际路径、SHA 和自报告边界|
|`executionAttestation.measurementEngine`|被检查并原样执行的 v2 路径、SHA、编译文件名和 `capture` 入口|
|`baseProbe` / `executionAttestation.baseProbe`|v2 固定加载的 v1 基础脚本及其 SHA|
|`sourceManifest`|原 `load_sources` 对当前三份来源逐项检查后形成的路径、SHA、字节数|
|`sourceBindingVerified`|上述当前来源清单是否全部等于 loader 的固定有效 pins；失败时 false|
|`executionAttestation.sourceBinding`|原 v1 pins、当前有效 pins、固定 root、重绑定范围、未执行来源文件的声明|
|`executionAttestation.reportAdapter`|序列化接缝与 `scriptSha256` 的新含义|
|`executionAttestation.boundaries`|未调用产品 `PlayerInfoReader.Snapshot()`、未执行 C# 或来源脚本、未建立产品二进制对应、没有实现阶段判据|

原 v2 的 identity、module、roles、caps、读日志、错误/短读计数、0/1/3 时间表、独占输出和关句柄证据都保留。全局上限仍为 `10s / 2MiB / 20000` 次读，单读 `64KiB`，保留结束预算；camera/world/hud/stability 分区及双向 HUD 路径的两遍验证均原样执行。进程访问掩码仍为 `0x410`。没有全进程扫描，也没有把跨样本缓存或 flags 等同于路径成员关系。

`phaseDecision` 始终是 `not_evaluated`，`productAcceptance=false`，`operatorLabelIsObservedPhase=false`。world、相机、HUD 路径、flags 或闭合子图在多阶段相似，并不构成阶段判据。Python 只读取借用的字段，从未调用产品 `Snapshot()`；产品 Snapshot 成功、坐标正确与阶段验证需各自真实证据。

## 离线证据与复跑

`offline-validation.json`：47/47 通过，包含原字节执行的 v2 32 项 + wrapper 15 项。实际 `ctypes.WinDLL` 被测试屏障阻断，`realSamples=0`，17 个历史 v1/v2 文件的前后 SHA 全部一致。新增反例覆盖 v1/v2 字节漂移、每份来源字节漂移、固定 pin 错误、即使配套测试 hash 也仍检查角色 RVA、输出越界/撞名、身份变化、v3 冒作 v1 引用和混用 source-only/live 参数。正例核对真实 code object 与冻结源代码编译结果一致、当前有效 pin 与旧参考 pin 作用域隔离、完整 provenance、预算和读日志保留。

`source-binding-offline.json`：固定三源的只读校验快照，附 wrapper/engine/base/override attestation；实际 native 加载同样被屏障阻断。该文件的 `realSamples=0`，不是游戏观测。

`offline-validation-initial.json` 保留首轮 fixture 失败：反例生成器把大写 `0x6187E40` 按小写查找，导致测试没有真正制造差异。只修正测试的大小写后得到 47/47；wrapper 两次运行的 SHA 完全相同。

从仓库根复跑，输出报告必须选新文件名：

```powershell
$python = 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe'
$v3 = 'F:\WorkSpace\Game\CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v3'
& $python -B -X utf8 "$v3\phase_probe_v3.py" --offline-attest
& $python -B -X utf8 "$v3\phase_probe_v3_selftest.py" --report "$v3\offline-validation-recheck-001.json"
```

## 仅 root 执行的 live 交接

本子任务没有执行以下命令。root 已独占 `v3/live-round3/`。开始前人工按当前画面选择标签；这些标签只记录操作者陈述，不会成为自动阶段结论。

root 在执行前重新核对 PID/name/start，随后探针仍会通过原 v1 的 FILETIME identity/module/hash 校验再次检查：

```powershell
$python = 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe'
$v3 = 'F:\WorkSpace\Game\CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v3'
$entry = "$v3\phase_probe_v3.py"
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $entry).Hash.ToLowerInvariant() -ne '81ee593a762933f0cf119cbc6fdcafc37cfa687ddd171aad6aa6f99dbf7da66b') {
    throw 'Frozen v3 entrypoint drifted; inspect before continuing.'
}
& $python -B -X utf8 $entry --offline-attest
if ($LASTEXITCODE -ne 0) { throw 'Source-only preflight failed.' }
$expectedStart = [datetime]::Parse('2026-11-22T01:23:35.3993114Z', [cultureinfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind)
$target = Get-Process -Id 30504 -ErrorAction Stop
if ($target.ProcessName -ne 'RelicCoH3' -or $target.StartTime.ToUniversalTime().Ticks -ne $expectedStart.ToUniversalTime().Ticks) {
    throw 'PID/name/start changed; revise the explicit identity contract before sampling.'
}
# Current main-menu observation only after root rechecks the displayed state.
New-Item -ItemType Directory -Force -Path "$v3\live-round3" | Out-Null
& $python -B -X utf8 $entry --pid 30504 --expected-start-utc '2026-11-22T01:23:35.3993114Z' --label mainmenu --out "$v3\live-round3\pid30504-mainmenu-001.json"
```

后续每次重新核对上述 identity，并使用新输出名：

```powershell
& $python -B -X utf8 $entry --pid 30504 --expected-start-utc '2026-11-22T01:23:35.3993114Z' --label newmatch --out "$v3\live-round3\pid30504-newmatch-001.json"
& $python -B -X utf8 $entry --pid 30504 --expected-start-utc '2026-11-22T01:23:35.3993114Z' --label running --out "$v3\live-round3\pid30504-running-001.json"
& $python -B -X utf8 $entry --pid 30504 --expected-start-utc '2026-11-22T01:23:35.3993114Z' --label paused --out "$v3\live-round3\pid30504-paused-001.json"
& $python -B -X utf8 $entry --pid 30504 --expected-start-utc '2026-11-22T01:23:35.3993114Z' --label results --out "$v3\live-round3\pid30504-results-001.json"
```

这些命令按真实状态逐个执行，不是连续运行的阶段制造器。进程重启后停止使用旧 PID/start，不把旧地址或引用带入新进程。每份报告均检查 `status`、`handleClosed`、identity、预算/partial evidence、`sourceBindingVerified`、`phaseDecision` 与 `productAcceptance`；`observed` 仅表示有界原始观测完成。

本目录没有新增 `.ps1` 文件；若后续把带中文的命令另存为 Windows PowerShell 5.1 脚本，使用 UTF-8 BOM。
