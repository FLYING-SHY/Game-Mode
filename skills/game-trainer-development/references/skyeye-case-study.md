# SkyEye：从只读内存到外部叠加层的证据案例

## 阅读用途与证据分层

本案例用来借鉴 Windows 外部读取器的结构、生命周期和验证方法。分析对象是本仓库 CoH3 的 SkyEye；这些地址与类型假设属于该版本，移植其他游戏时重新发现。文中的仓库链接从本文件位置回到仓库根，文字中的路径采用仓库相对形式。

区分三种证据：

| 层级 | 已知内容 | 可以支持的结论 |
|---|---|---|
| 前轮调查结果（2026-11-16，沿用，本文未重新执行） | 原 EXE 为 x64 / .NET Framework 4.8；SHA256 为 `F89B09C2BFF2751C5E7904D3E302B6966ABB7C8CC16D56A98BE09582AAD4E2D0`；用 ILSpy `9.1.0.7988` 逐类型反编译 Native、PlayerInfoReader、AlphaSurface、SquadMarkerWindow、GameVersion45，统一换行后与当前五类源码 exact equal | 仅这五类的原版映射；不是整个源码树或所有发布包的等价证明 |
| 本轮静态复核 | 亲自读取以下五类的相关代码和 protection-validation 报告 | 源码控制流、常量、检查点、绘制方式；不等于本轮启动游戏或重新测量 |
| 存量验收报告 | 保护、授权、UI、协议、打包的既有执行记录 | 报告列明环境和测试边界内的结果；不外推到完整对局 |

原版映射之外的类（例如租约、布局、预测、时钟或绘图辅助类）即使由这五类引用，也仍是“当前源码可见、此次未建立原 EXE 映射”。引述它们的调用只能证明调用点，不能推出原版来源或实现相同。

## 读取边界与地址模型

- [Native.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/Native.cs#L25-L45) 声明 OpenProcess、ReadProcessMemory、VirtualQueryEx；读取包装在每次调用前检查 `LicenseGate60.Allowed`。该文件没有 WriteProcessMemory 声明。
- [PlayerInfoReader.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2296-L2355) 两个构造入口均以 `1040u` 打开进程，即 `0x410 = PROCESS_QUERY_INFORMATION (0x400) | PROCESS_VM_READ (0x10)`。上下文入口同时检查 PID 对应进程启动时间和模块基址，避免 PID 复用或旧上下文混入。
- [PlayerInfoReader.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2406-L2437) `Read(long a, int n)` 检查地址下界、长度 `0..65536` 和完整读取长度；`Q` 为 8 字节指针，`U` 为 4 字节无符号值，`F` 排除 NaN / Infinity。完整读检查和领域校验分别解决传输失败与数据语义错误。
- [PlayerInfoReader.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2355-L2397) 使用 `image + 112918560 + 840` 与 `image + 112918560 + 560` 定位表，并检查跨度、48 字节步长、索引和必要资源名。`image` 来自当前模块基址；这里的常数是硬编码 RVA/结构偏移，不是跨版本自动定位结果。
- [GameVersion45.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/GameVersion45.cs#L10-L49) 支持的游戏文件 SHA256 为 `2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde`，与上表的原修改器 EXE 哈希是不同对象。Assess 以路径、长度、最后修改 ticks 为缓存键；未知哈希或异常置 `Unverified`。本类只标记状态，没有抛异常阻断；调用方后续如何使用该状态需另外追踪。

上述读取器与叠加层代码没有建立“修改游戏内存”或“游戏代码补丁”的证据。名称、界面开关、绘制结果都不足以证明写入。本文结论限于已读代码路径，不把五类中未见写 API 外推成整个工程绝无写入。

## 有预算的发现与失效处理

[PlayerInfoReader.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1539-L1815) 的小地图发现是渐进任务：先尝试树查找、缓存 hint 和候选，再遍历内存区。关键预算为：

| 参数 | 源码值与含义 |
|---|---|
| 时间 | `stopwatch.ElapsedMilliseconds < 80`，单工作批次主动检查的预算；不是硬实时保证 |
| 数据量 | `num2 < 33554432`，32 MiB；按请求长度累计，不是成功读取量 |
| 块 | `new byte[65536]`，64 KiB；扫描 8 字节对齐候选 |
| 区域 | 已提交、私有、非 guard/no-access 且满足保护位过滤；小区域 pass 限制为不大于 4 MiB |
| 候选 | 最多 64 个，优先复用 hint，缓存候选按 60 秒时效筛选，每次最多轮转 4 个 |
| 重试 | 一般继续搜索约 100 ms；遍历轮回后可延至 2000 ms |

不要把 vtable 候选 `image + 65511760` 的命中当成最终结构证据；后续仍通过 ReadMinimapLayout 和布局位置条件验证。

[PlayerInfoReader.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1203-L1226) 在对局变化时递增 `miniEpoch`、清控制指针、重置进度与候选；[RecoverMinimap](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1518-L1534) 同样递增 epoch。后台任务抓取 epoch 与独立 scan token：每阶段与提交前检查 epoch，过期结果停止发布；finally 仅在 token 仍对应本任务时清 `miniScanning`。epoch 防旧对局结果污染，token 防旧任务收尾误清新任务状态，两个职责分别保留。

## 外部 overlay 与采样节奏

[AlphaSurface.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/AlphaSurface.cs#L84-L169) 创建 32-bit top-down DIB，Bitmap 使用 `Format32bppPArgb`，经 UpdateLayeredWindow 提交预乘 alpha；尺寸变化先清旧资源。清理顺序包含 Graphics、Bitmap、恢复旧 GDI 对象、删除 DIB 与 DC，适合借鉴资源归属管理。SquadMarkerWindow 的主要绘制入口是 OnPaint/GDI；AlphaSurface 是另一条 layered-window 绘制路径，两者分别追踪，不能把工程中存在 AlphaSurface 推成所有窗口都调用 UpdateLayeredWindow。

[SquadMarkerWindow.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/SquadMarkerWindow.cs#L111-L232) 使用独立 WinForms 窗口、ShowWithoutActivation、扩展样式、命中测试穿透及鼠标激活处理。扩展样式十进制 `134742176` 对应 `0x080800A0`；逐位核验后再移植，不把整个数当作跨平台常量。

[SquadMarkerWindow.cs](../../../CoH3/dev/SkyEye/client/CoH3MemoryTool/SquadMarkerWindow.cs#L350-L477) 检查 Gate、scene 存在和 3 秒新鲜度、前台 PID、最小化和客户区；按 PositionRate 采样单位，绘制帧独立设 FrameRate，每帧获取相机，再投影世界坐标。弹道与空域约 33.33 ms 的读取调度是源码意图，不代表实际稳定 30 Hz；实际节奏需观测。DropConnection 会释放 reader、清 match、候选绘制集合与计时状态，避免退出或切局残留画面。

## 如何引用存量验收

[protection-validation.md](../../../CoH3/dev/SkyEye/reports/protection-validation.md#L30-L54) 记录服务端 120/120、客户端三版各七组、像素比较 changedPixels 为 0、音频解码一致、真实入口窗口。它还明确入口窗口不代表进入实际游戏。

[验证边界](../../../CoH3/dev/SkyEye/reports/protection-validation.md#L80-L90) 明列尚未覆盖实际完整对局、长期压力、第二台机器及混淆后全部游戏路径。引用时写“既有报告记录”，不要写“本轮全部测试通过”。本轮没有执行构建、游戏连接、原版反编译、性能回归或发布验证。

## 迁移到其他游戏时的交付要求

把常量集中进带游戏文件哈希的版本描述；为每个 RVA 记录来源、指令/结构证据、失败条件。保留 PID + 启动时间 + 模块 + 对局 generation 的上下文，后台结果提交前重验。将发现、采样、投影、绘制分开计时；分别统计扫描预算、读取失败、数据年龄、帧间隔。新增写入功能时另建补丁事务和恢复证据，不能沿用本案例的只读结论充当写入验证。
