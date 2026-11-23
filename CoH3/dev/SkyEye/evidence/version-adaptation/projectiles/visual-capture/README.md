# Wespe 炮弹层画面采证

此目录仅新增独立取证 CLI、fake 自检与说明。真实维护窗口的准备、真实捕获和全部 UI 输入由父代理执行。本子任务没有启动、打开、操作游戏或 GUI，也没有运行任何真实捕获。

## 文件与运行环境

- [bounded_visual_capture.py](./bounded_visual_capture.py)：Python 3.10+、Pillow、Windows 10+；仅在实际调用 `run_capture` 时初始化 Win32。导入和 `--help` 不枚举窗口，不解析其他脚本参数。
- [selftest_visual_capture.py](./selftest_visual_capture.py)：fake-clock / fake-window 自检，不构造 live backend；临时文件只在本目录内创建与清理。
- [selftest-report.json](./selftest-report.json)：29 项离线自检的逐项名称与通过状态。离线通过证明控制逻辑；真实 GPU / layered-window 的 PrintWindow 结果由父代理核验。

实现只读参考了现有 [capture_overlay_state.py](../../capture_overlay_state.py) 与 [window_probe.py](../../../adaptation-52426-20261121-004302/window_probe.py)，没有导入或执行它们，避免附带 argparse、焦点或输入逻辑。面板筛选依据维护源码中设置窗有标题、绘图窗无标题/无边框的现状。

## 父代理运行命令

从仓库根目录执行；`--out` 的父目录应已存在，目标目录必须全新。下例的两个 PID 来自父代理在本轮提供的维护运行状态，后续运行应显式更新为当时选定的 PID。

```powershell
$python = 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe'
& $python -B .\CoH3\dev\SkyEye\evidence\version-adaptation\projectiles\visual-capture\bounded_visual_capture.py `
  --game-pid 31572 --overlay-pid 23716 `
  --out .\CoH3\dev\SkyEye\evidence\version-adaptation\projectiles\visual-capture\wespe-capture-01 `
  --seconds 20 --cadence-ms 500 --desktop-game-region
```

`--desktop-game-region` 为显式选择的合成采样；省略它仅保存单窗口 PrintWindow 结果。默认游戏图为完整像素尺寸 JPEG，quality=95、subsampling=0；`--game-format png` 可选择游戏 PNG。overlay 始终保存完整像素尺寸 PNG。所有图像均不缩放、不裁减窗口细节。桌面图仅取与整个游戏窗口完全一致的 bbox。

前台必须已经是指定游戏 PID 或指定 overlay PID。通过会夺走前台的终端启动时，脚本会立即停止；它不会切焦点或等待用户重新切回。父代理应通过其既有后台执行通道启动，并在采集前自行让指定窗口位于前台。

## 捕获与边界

- 必填 `--game-pid`、`--overlay-pid`、`--out`。两个 PID 必须是不同的正整数。
- `--seconds` 为 1..60，默认 20；`--cadence-ms` 至少 200，默认 500。硬上限为 120 次组尝试与 80 MiB 总文件字节；没有放大上限的 CLI 开关。
- 枚举入口为 EnumWindows；只对匹配两个 PID 的窗口读取标题、类名、几何和可见性。唯一额外的 PID 查询来自 GetForegroundWindow，供前台保护使用。没有进程句柄、进程内存读写、注入、输入、窗口移动或焦点修改 API。
- 首次选取游戏 PID 下可绘制的最大面积顶层窗口，同面积按 HWND 升序。整段会话固定该 HWND / PID / TID / 类名。完整游戏窗口必须包含在某一个实际 monitor rect 内，不能借用多显示器虚拟包围框中的空隙。最小化、DWM cloaked、不可见窗口不视为可绘制窗口。
- overlay 捕获该 PID 下所有可见、非最小化、非 cloaked、正尺寸、无标题且不带 WS_CAPTION 的顶层窗口。带标题或 caption 的窗口作为面板排除，并在组 JSON 中逐项列出；不可绘制数量也单独记录。零个可见层是合法状态，仍要求该 PID 存在至少一个可校验的顶层窗口。
- 每组顺序为游戏 PrintWindow → 各 overlay PrintWindow → 可选桌面游戏区域。每一张图前后校验时间、前台、固定 HWND/PID/TID/类名、几何、显示区域；循环前后也校验已选窗口。组末再次核对可见 overlay 集合，改变时明确丢弃该组并停止。
- PrintWindow 输出缓冲先填黑，标记 `PW_RENDERFULLCONTENT`，获取 32-bit bitmap 后保存 RGB。黑底/色键的 overlay 图只证明该层绘制内容。GPU 游戏有时返回黑图、旧图，即使 PrintWindow 返回成功；父代理应目视核查游戏图，并结合可选桌面图确认真实画面。
- 可选桌面图使用 `ImageGrab.grab(bbox=game.rect, include_layered_windows=True, all_screens=True)`，记录 `ImageGrab.game_bbox.composited`。这是当时桌面区域的实际合成像素，包含 overlay 及区域内的任何遮挡；它不隔离其他遮挡窗口，也不是单窗口纯游戏图。前台检查与系统截图不是原子操作：每次调用前后及等待中的 50 ms 轮询尽量缩短竞态；发现其他前台即停止后续捕获，并丢弃尚未提交的整组。
- 各图是顺序采样，具备各自 UTC 与 monotonic 起止，不宣称同时曝光。PNG/JPEG 的 hash 为实际文件编码字节 SHA-256；窗口 rect 与 bitmap 尺寸同时保留。
- 同步 PrintWindow / ImageGrab / 编码在本工具的 daemon 线程执行，主线程每 50 ms 复查保护条件。超时/前台变化后不等待被挂起的原生调用，不开始下一张图；CLI 写完报告后退出本工具进程。`nativeCallAbandoned` 表明是否放弃了仍在执行的本工具线程。此脚本应独立运行；不要导入到需要长期存活的宿主后调用 live backend。
- `seconds` 约束捕获及等待阶段。停止后的元数据收尾、文件系统写入时间另计；它们不启动新截图。每张文件写入前也会再次检查保护条件。

## 文件与计数

每组产生游戏图、零个或多个 overlay 图、可选桌面图及一个组 JSON。所有图先留在内存，只有整组截图、后校验、组元数据及预算检查通过后才创建文件。没有大图临时文件或事后降采样。

80 MiB 包括所有图、组 JSON 与最终 `session.json`；预留 64 KiB 给最终报告，故图和组 JSON 的可用额度略低于 80 MiB。若候选图或含 JSON 的整组超过剩余额度，停止原因是 `byte_budget`，不会提交该组，并明确记录候选字节、剩余额度、丢组与丢图数。磁盘实际文件字节之和等于最终报告的 `totalBytesWritten`。

`session.json` 记录：

- `groupsAttempted/groupsSaved/groupsDropped`：开始的组、完整保存的组、停止时未完成的组。
- `imagesCaptured/imagesSaved/imagesDropped`：完成编码的图、完整提交组内的图、未完整提交组内完成编码的图；原生调用失败尚未产生图的次数不会伪计为完成图。
- `missedCadenceSlots/missedCadenceRanges`：捕获/编码/写入超过节拍后跳过的确切 cadence slot，避免补帧突发；停止后的未来 slot 不计作丢帧。
- `stopReason/stopDetail`：例如 `deadline`、`group_limit`、`byte_budget`、`foreground_changed`、`window_invalid`、`game_outside_display`、`overlay_set_changed`、`error`。
- `initialValidation/finalValidation`：前后身份校验；末校验通过时保留校验数与身份列表 hash，具体窗口元数据可查最后完整组。到期或帧满时若末校验失败，停止原因为 `window_invalid_at_end`。
- `writeFailures`：若 OS 写入失败或提交中保护条件变化，已写入的部分文件保留并逐项列出，`partialGroup=true`；该组整体仍计入 dropped，实际文件字节继续计入总预算。不要把这些部分文件当作完整组。

正常 `deadline` / `group_limit` 且至少保存一组时退出码为 0；提前保护停止、预算停止、捕获错误或零完整组为 3；参数、路径准备错误为 2。现存 `--out` 会在构造任何 backend 前报错，原目录不被复用。

## 离线自检

```powershell
& $python -B .\CoH3\dev\SkyEye\evidence\version-adaptation\projectiles\visual-capture\selftest_visual_capture.py
```

自检覆盖：参数范围、目录冲突、导入惰性、截止（含图像中途到期）、120 组、图像与 JSON 字节预算、完整尺寸/编码 hash、面板排除、零可见 overlay、初始/途中/等待中的前台变化、指定 overlay 前台、HWND/PID 失效、实际显示器边界及间隙、overlay 集合改变、尺寸错误、原生捕获异常、节拍精确跳槽、桌面显式选择和纯 fake 的线程看守。若需要新的 JSON 报告，传入本目录下一个尚不存在的 `--report` 文件，现有报告不会被覆盖。
