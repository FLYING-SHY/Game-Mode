# SkyEye 只读机制报告（case1 / without_skill / run-1）

## 结论与证据强度

现有核心代码实现的是**外部进程只读采样 + 游戏对象结构解析 + 独立透明窗口绘制**。所查进程句柄权限为 `1040 = 0x410 = PROCESS_QUERY_INFORMATION | PROCESS_VM_READ`；读取路径调用 `ReadProcessMemory`。已审查的核心路径没有对游戏内存写入、远程分配、远程线程或游戏文件补丁安装步骤。工具会写自身目录的定位缓存、设置及诊断日志，因此“只读游戏”不等于“程序从不写文件”。

这次不只依赖维护源码推断原版：使用仓库已有 ILSpy 9.1.0.7988，把原版 EXE 的四个指定类型静态导出到本次输出目录。统一 CRLF/LF、去除文件末尾空白后，`Native`、`PlayerInfoReader`、`SquadMarkerWindow`、`GameVersion45` 四类均与维护源码逐字符一致，机器结果见 [static-comparison.json](<F:/WorkSpace/Game/skills/game-trainer-development-workspace/iteration-1/eval-1-skyeye-evidence/without_skill/run-1/outputs/static-comparison.json>)。该结果限定于四类，并非整程序相同或全部副作用审计。

原版 EXE SHA256 在静态导出前后均为 `f89b09c2bff2751c5e7904d3e302b6966abb7c8cc16d56a98be09582aad4e2d0`，与原版 [SHA256SUMS.txt:11](<F:/WorkSpace/Game/CoH3/original/天穹之眼-v0.60.8/SHA256SUMS.txt#L11>) 一致。本次没有启动 EXE、游戏或服务，没有构建，没有访问授权私有数据，没有改动原项目。原版其余文件未做前后全量哈希校验。

## 1. 怎样找到进程、版本及数据

### 1.1 进程身份、模块基址和版本提示

[PlayerInfoReader.cs:2328](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2328>) 要求 `Process.GetProcessesByName("RelicCoH3")` 恰好返回一个进程，记录启动时间和 `MainModule.BaseAddress`。基址记为 `image`，后续全局地址使用 `image + 相对偏移`，从而适应模块加载基址变化，但**固定偏移依旧依赖游戏版本**。

[GameVersion45.cs:10](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/GameVersion45.cs#L10>) 的已知游戏文件 SHA256 为 `2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde`。`Assess(string path)` 用文件全路径、长度、最后写入时间组成缓存键，只读计算文件 SHA256；不匹配或异常时置 `Unverified`。它没有下载补丁或改写游戏的逻辑。维护界面 [OverlaySettingsForm.cs:2184](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/OverlaySettingsForm.cs#L2184>) 把它显示为“版本未验证”，提示仍允许继续测试；不能把哈希不匹配误称为强制拒绝运行。这里的游戏哈希与前述工具 EXE 哈希用途不同。

句柄创建前检查 `LicenseGate60.Matches(pid, started)`；[Native.cs:34](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/Native.cs#L34>) 的每次实际读取又检查 `LicenseGate60.Allowed`。这说明进程身份与工具授权约束位于数据入口，本次只读这些源码分支，未打开凭据、数据库或私钥。

### 1.2 不是逐字节盲扫资源，而是已知结构解析

[PlayerInfoReader.cs:2355](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2355>) 从 `image + 112918560 + 840` 取得资源名称表，检查起止地址、总跨度不超过 6144、条目跨度整除 48，再读取名称及索引，只接受索引小于 10。它确认 `manpower/fuel/munition/command` 四个名字均出现；因此资源顺序通过名称映射，避免完全依赖索引顺序。

同一构造函数从 `image + 112918560 + 560` 读取技能计时名称表，检查最大跨度 12288、条目 48 字节，并识别 `shared_ability_1` 与 `shared_barrage_ability`。

[PlayerInfoReader.cs:2406](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2406>) 中 `Read(long a, int n)` 限制地址至少 65536、单次大小 0..65536，要求系统读取返回成功且字节数完整；`U/Q/F` 分别解码 uint32、int64、float，浮点拒绝 NaN/Infinity。`Resolve(long arena, uint offset)` 拒绝 `uint.MaxValue`，用 `checked(arena + offset)` 恢复 arena 内的相对地址。这里并非所有字段都是普通 64 位绝对指针。

### 1.3 世界、玩家、资源与对象生命周期

[PlayerInfoReader.cs:2710](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2710>) 的关键链是：

```text
arena = Q(image + 102260400)
worldOffset = U(image + 98614988)
world = arena + worldOffset
playerContainer = arena + U(world + 252)
playerCount = U(playerContainer + 16)   // 1..16
playerArray = arena + U(playerContainer + 12)
teamArray = arena + U(world + 284)
player = arena + U(playerArray + slot * 4)
```

本地玩家不是简单认定为列表第一个：[PlayerInfoReader.cs:212](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L212>) 由镜头关联对象读取本地 identity，检查类型标记、状态、低位非零及高位索引小于 16，再在当前玩家表匹配 identity。上面这些固定偏移及类型标记是当前游戏专用知识，源码没有记录它们最初如何被逆向发现。

资源在 `player + 1704` 的 40 字节块中依据名称表映射读取，人口为 `F(player + 1256) + F(player + 1304)`；升级条目检查数量最多 1024并以 104 字节跨度解析。[PlayerInfoReader.cs:2743](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2743>) 同时解码队伍、关系、名字、战斗群并调用实体读取。

`MatchKey` 包含 PID、进程启动时间、世界对象、本地玩家地址和 identity 低位。读完后重新比较 arena、world offset、玩家表数量及起点、本地 identity，变化就丢弃本次结果；升级数组也做读取前后校验。[PlayerInfoReader.cs:2786](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2786>) 证明它主动应对换局及并发更新。不过多次读取仍非游戏提供的原子快照，重读降低混合状态概率，不构成绝对一致性证明。

### 1.4 镜头、UI 树与有限内存扫描互补

[PlayerInfoReader.cs:1118](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1118>) 用缓存的 `cameraCore` 配合首字段 `image + 64684536` 类型标记检查；失效时从 `Q(Q(image + 102260680) + 184)` 指向的列表重新找镜头。读取 `Q(cameraCore + 416)` 下 816 字节，提取偏移 88 的 View 和偏移 752 的 Projection，各 16 个浮点数；检查有限值、矩阵特定元素、镜头位置一致性，并在结束时复核对象指针。异常会清空镜头缓存，后续重试。

小地图不是复用固定屏幕像素：[PlayerInfoReader.cs:1918](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1918>) 先尝试 UI 根和树遍历，以 `image + 64823200`、`image + 65511760` 等类型标记确认根/控件，校验布局后才接受。UI 树每轮限制约 25 ms、最多访问 6000 个节点。

[PlayerInfoReader.cs:1540](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1540>) 的后台发现先试树、缓存 hint 和轮换候选；再以 `VirtualQueryEx` 查询区域，过滤已提交、私有、非 guard/noaccess 且具可读保护的区域，64 KiB 分块、8 字节对齐寻找类型标记。循环以 80 ms 或 32 MiB 为停止条件，候选列表上限 64；找到候选后还需 `ReadMinimapLayout` 与屏幕位置条件验证。80 ms 是源码检查的循环预算，不是本次测得的调用总耗时上限。

缓存保存 PID、启动时间、image 和地址，读取也以这几个值解析；[PlayerInfoReader.cs:1084](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1084>)、[PlayerInfoReader.cs:1900](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1900>) 表明缓存服务于当前进程上下文。停止发现会增加 epoch，让旧后台任务结果失效，见同文件 1841 行。

## 2. 是否写游戏内存或安装补丁

| 检查 | 结果和出处 | 能支持的判断 |
|---|---|---|
| OpenProcess 权限 | 两个构造函数均为 `1040u`，见 [PlayerInfoReader.cs:2311](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L2311>) 与 2350 行 | 所查句柄具查询/读取权限，不含 VM_WRITE、VM_OPERATION、CREATE_THREAD |
| Native 导入 | [Native.cs:25](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/Native.cs#L25>)：OpenProcess、CloseHandle、ReadProcessMemory、VirtualQueryEx、热键和前台窗口 API | 核心远程数据通道是读取 |
| 当前客户端 C# 静态检索 | 搜索 `WriteProcessMemory|VirtualProtectEx|CreateRemoteThread|VirtualAllocEx|InstallPatch|patch`，未出现相关命中；同时搜索全部 DllImport 和 OpenProcess | 所检索源码未见常见写内存/注入/补丁入口；文本负命中不是对任意动态解析或全部外部依赖的形式证明 |
| 本地文件写入 | 定位缓存写入见 [PlayerInfoReader.cs:1107](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs#L1107>)；诊断写入见 [OverlayDiagnostics55.cs:16](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/OverlayDiagnostics55.cs#L16>) | 目标是工具的 `AppDomain.CurrentDomain.BaseDirectory`，不属于给游戏安装补丁 |
| 原版四类复核 | 本次静态导出与维护源码四类一致 | 四类中的上述机制有原版 EXE 直接静态证据 |

结论应写成“已核查路径没有游戏写入或补丁安装证据”，避免外推成对整个原版二进制、全部依赖、任意运行环境的绝对无写入承诺。本次没有执行游戏内跟踪，也没有运行安装过程。

## 3. 覆盖层怎样实现

[SquadMarkerWindow.cs:12](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/SquadMarkerWindow.cs#L12>) 继承 `Form`，是独立进程窗口。其窗口机制如下：

- 111..121 行：`ShowWithoutActivation = true`，扩展风格 OR `134742176 = 0x080800A0`，组合 NOACTIVATE、LAYERED、TOOLWINDOW、TRANSPARENT。
- 165..177 行：无边框、不显示任务栏、TopMost、手动定位、颜色键透明和双缓冲。
- 204..233 行：处理 `WM_NCHITTEST(132)` 返回 `HTTRANSPARENT(-1)`，`WM_MOUSEACTIVATE(33)` 返回 `MA_NOACTIVATE(3)`，避免标记层抢交互焦点。
- 146..163 行：`SetWindowPos(HWND_TOPMOST, ..., 19)` 恢复层级并记录 Win32 错误。这是调整自身窗口堆叠，不是修改游戏渲染函数。

[SquadMarkerWindow.cs:350](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/SquadMarkerWindow.cs#L350>) 每帧检查 Gate、场景存在和新鲜度；场景超过 3 秒、游戏不在前台、最小化或战术地图打开时暂停隐藏。通过 `GetClientRect + ClientToScreen` 对齐游戏客户区，将覆盖层 Bounds 移到相同屏幕坐标。按对局上下文变化重建 reader，避免沿用上一局实体。

采样与绘制分频：位置读取由 `PositionRate` 决定，绘制由 `FrameRate` 决定，炮弹和空袭读取约每 33.33 ms 一次，镜头每个有效帧重新采样。`FreshCamera()` 最多尝试三次；连续失败达到五帧或提示换局时清空/隐藏。上述为源码调度目标，本次没有实测达到率。

[MineCamera.cs:12](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/MineCamera.cs#L12>) 把世界坐标乘 View，再乘 Projection 得到裁剪空间，`w < 0.05` 或除法后坐标超出 [-1,1] 就剔除；最后映射到客户区像素。主绘制调用见 [SquadMarkerWindow.cs:469](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/SquadMarkerWindow.cs#L469>)：先生成精灵/线段命令，合并新旧覆盖区域后 `Invalidate(region)` 和 `Update()`；`OnPaint` 在 600..645 行用 GDI+ 绘线、`DrawImageUnscaled` 绘精灵。

帧间隔 P95、相机耗时和采样到绘制耗时只是代码中已有的观测能力；638 行明确不包含游戏渲染及显示器延迟。本次没有运行这些统计。

原版 [先看这里.txt:3](<F:/WorkSpace/Game/CoH3/original/天穹之眼-v0.60.8/先看这里.txt#L3>) 与 [软件使用说明.txt:7](<F:/WorkSpace/Game/CoH3/original/天穹之眼-v0.60.8/软件使用说明.txt#L7>) 都要求窗口化/无边框，并未保证独占全屏可见；这与外部窗口架构相符。不能据此推断存在 DirectX 注入或游戏内绘制 Hook。

## 4. 哪些经验可迁移，以及怎样执行

| 可迁移的工程方法 | 开发者执行步骤 | 当前游戏专用内容与验收条件 |
|---|---|---|
| 进程生命周期绑定 | 用目标 PID、启动时间、模块基址形成读取上下文；重启/换局即清理快照与缓存 | 重建目标进程名、世界/玩家 identity；用进程重启和换局用例验证旧任务不发布 |
| 模块相对定位 + 对象不变量 | 先确认版本，再从稳定根建立有界结构解析；加入数量、跨度、类型标记和可读性校验 | 本报告所有偏移、arena 规则、类型标记不可直接复用；须在目标游戏重新取证 |
| 名称映射与语义层分离 | 把原始字节解析成玩家、实体、关系、资源模型，再由独立显示层消费 | 资源名、蓝图路径、阵营关系和技能类别要重新建模；用已知场景核对 |
| 并发变化容错 | 在读取前后复核容器、generation 和 identity；异常丢弃当前帧；以 epoch 取消旧发现 | 重读不是锁，需明确可接受陈旧度并测试快速增删对象、观战/切场景 |
| 有预算的发现过程 | 先缓存/树结构，再受限区域扫描；限制时间、字节数、候选数并记录失败原因 | 80 ms/32 MiB/64 候选只是当前实现参数；根据目标响应和机器负载调整，重新测量 |
| 独立覆盖层与投影 | 保留不激活/穿透/客户区跟随/前台隐藏的窗口骨架，把相机和投影交给游戏适配器 | 矩阵布局、左右手系、Y 方向、DPI、缩放、超宽屏、多显示器与显示模式都需逐项验证 |
| 采样/绘制分频与可观测性 | 分别记录位置采样、相机采样、绘制耗时、帧间隔 P95和错误状态 | 不把绘制率当游戏 FPS或端到端延迟；基于代表性场景报告实际数值 |

建议按“只读连接与生命周期 → 单个已知字段 → 玩家/实体结构 → 相机投影 → 窗口层 → 换局与失效恢复 → 性能测量”顺序推进，每一步保留输入版本、结构证据和预期/实际结果。本次只交付取证与执行建议，没有实现其他游戏的适配器。

## 5. 历史保护验收能证明什么

已核查 [protection-validation.md](<F:/WorkSpace/Game/CoH3/dev/SkyEye/reports/protection-validation.md>)，没有运行它引用的脚本，也没有读取其私有服务数据。

- 9..12、68..76 行：正式路线是签名租约 + Obfuscar 2.2.50；Reactor 7.7.1.0 是历史选择性虚拟化试验，未给高频绘制/预测/Gate 热路径虚拟化。正式版仍能用 ILSpy 导出 56 个 C# 文件，不能宣称语义不可分析。
- 32..50 行：报告记载 120/120 服务端测试、各版七组回归、28.010 秒心跳间隔、约 59 秒 Gate 关闭、UI 像素及音频一致。这些是既有报告的历史观测，本次未复跑或独立复核其底层 JSON。
- 54 行：当时真实 EXE 打开激活窗口，并未据此宣称进入游戏。
- 80..84 行明确排除实际游戏完整对局、长期压力、第二台机器、全部混淆后游戏路径等。保护测试通过不能替代游戏地址正确性、单位识别、覆盖精度或真实对局性能验收。

原版 [overlay-diagnostics.log:1](<F:/WorkSpace/Game/CoH3/original/天穹之眼-v0.60.8/overlay-diagnostics.log#L1>) 已有“对局数据尚未就绪”和多次“信息地址无效”，本次仅读取前 35 行。它证明历史读取曾遇到失败，单凭日志不能区分未进入对局、版本变化、无效缓存或其他原因，也不能证明后来修复成功。

## 6. 本次实际成功、失败与未验证项

**成功：**读取根规则、只提取评估 prompt；核查全部五个指定文件；检索当前客户端相关系统调用与本地写入；只读静态导出原版四类；比较四类一致；原版工具 EXE 两次哈希一致；所有生成文件位于指定 outputs。

**实际失败：**首次 PowerShell `Get-Content -Raw | ConvertFrom-Json` 未指定 UTF8，中文按本机默认编码误读，返回 `ConvertFrom-Json` 对象格式错误；第二次显式 `-Encoding UTF8` 成功，只输出 prompt。该错误是输入读取编码问题，不是样本失败。

**未验证：**没有运行样本/游戏/服务；没有实时进程、画面、内存写入追踪或端到端延迟测量；没有重新执行历史测试；没有反编译原版全部类型、审计全部依赖或证明源码与当前交付 EXE 整体一致。会话没有提供可调用的 REA 专用 MCP/skill，采用已有 ILSpy CLI 做静态类型导出；不把静态导出成功称为动态测量成功。

本次没有可用的完成通知 token/持续时间统计，未填写推测 token、工具数或耗时。实际操作、命令和结果见 [transcript.md](<F:/WorkSpace/Game/skills/game-trainer-development-workspace/iteration-1/eval-1-skyeye-evidence/without_skill/run-1/outputs/transcript.md>)。
