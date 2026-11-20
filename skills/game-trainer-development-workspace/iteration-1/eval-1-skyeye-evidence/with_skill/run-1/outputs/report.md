# SkyEye v0.60.8 静态机制报告（case1 with_skill / run-1）

本轮证据支持：SkyEye 在已核查路径中是外部只读进程读取器加独立 Windows 覆盖层。数据定位混合使用模块相对常量、arena 内 32 位相对偏移、对象身份与结构校验，以及有预算的小地图对象发现。没有在已核查调用链发现游戏内存写入、代码补丁或远程线程安装；窗口 `RestoreLayer66` 恢复的是置顶层级。此结论不是对所有依赖和全部运行时路径的绝对证明。

## 1. 输入身份、范围与工具

工作根为 `F:\WorkSpace\Game`。原版、源码和报告只读；全部派生文件只写本报告所在 outputs。未运行样本或游戏，未构建项目，未访问授权私有数据，未读取 assertions 或其他评估输出。

- 原 EXE：`CoH3/original/天穹之眼-v0.60.8/天穹之眼-v0.60.8.exe`，1,231,360 字节，SHA256 `F89B09C2BFF2751C5E7904D3E302B6966ABB7C8CC16D56A98BE09582AAD4E2D0`。本轮 PE 解析得到 Machine `0x8664`、PE32+ `0x020B`、CLR directory RVA 8192 / size 72、文件版本 `0.60.8.0`。[原版配置](/F:/WorkSpace/Game/CoH3/original/天穹之眼-v0.60.8/天穹之眼-v0.60.8.exe.config:3) 声明 .NET Framework 4.8。
- 工具：宿主 read/glob/grep 与 PowerShell 均实际成功；仓库 ILSpy CLI 实际报告 `9.1.0.7988`，五次逐类型导出均 exit 0。仅工具被执行，原 EXE 被作为静态输入读取。当前工具列表没有 REA 动态捕获接口；未调用动态附加或截图工具。
- 原版五类 `Native`、`PlayerInfoReader`、`SquadMarkerWindow`、`GameVersion45`、`AlphaSurface` 已重新静态导出。排除 ILSpy 追加在 stdout 的升级通知、统一 CRLF/LF 并 TrimEnd 后，五类分别与当前源码逐字符相等。证据为 [清理后比较](comparison-clean.json)、[原始比较](original-source-comparison.json) 及 `original-*.cs`。此结果只映射这五类，不扩大为整个工程等价。
- 源码与存量报告的逐文件大小和 SHA256 见 [输入身份](input-identity.json)，PE 字段见 [PE 身份](pe-identity.json)。支持游戏文件 hash 是 `2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde`，属于另一对象；本轮没有游戏 EXE 身份或游戏版本实测。

目标契约：静态说明“进程 → 数据根 → 校验 → 快照 → 投影 → 窗口”的机制；把源码行为、历史验收与动态待验证分层。开启预期是读取有效本局信息并显示；关闭/失效预期是隐藏或清空覆盖层；排除假阳性靠完整读取长度、身份/generation、前后根一致性及新鲜度，不能把画面标记等同游戏被修改。

## 2. 数据定位与可复查调用链

### 2.1 连接、权限与版本

[Native.cs:25](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/Native.cs:25) 声明 `OpenProcess`，31–41 行封装 `ReadProcessMemory`；每次读取先检查 `LicenseGate60.Allowed`，未允许时返回 false 且 read=0；44–45 行为 `VirtualQueryEx`。此文件没有 WriteProcessMemory 声明。

[PlayerInfoReader.cs:2328](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2328) 要求名为 RelicCoH3 的进程恰好一个，取进程启动时间和 MainModule.BaseAddress，再调用版本评估。两个构造入口分别在 2311、2350 行使用 `OpenProcess(1040u, false, pid)`；1040=`0x410`=`PROCESS_QUERY_INFORMATION|PROCESS_VM_READ`，没有 VM_WRITE/VM_OPERATION。上下文构造入口 2296–2319 还核对启动时间、模块基址、Gate 身份与 Match。

[GameVersion45.cs:10](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/GameVersion45.cs:10) 保存支持 hash；25–49 行以路径、长度、mtime ticks 缓存 SHA256 结果，未知 hash 或异常置 `Unverified`。它没有阻止继续读取；[OverlaySettingsForm.cs:2186](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/OverlaySettingsForm.cs:2186) 的提示明确允许继续测试。不能称为严格版本拒绝策略，也不能称自动跨版本适配。

### 2.2 基址、相对偏移、结构与快照

[PlayerInfoReader.cs:2355](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2355) 从 `image + 112918560 + 840` 的 +32/+40 取资源名称表边界，检查跨度 <=6144 且是 48 的倍数；逐条读取字符串和索引，索引 <10，并要求 manpower/fuel/munition/command 存在。2371 行使用 `image + 112918560 + 560` 的相似表读取技能计时名称，跨度 <=12288。这是硬编码 RVA 和结构布局加语义检验，不是运行时 AOB 自动推导。

[PlayerInfoReader.cs:2406](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2406) 的 `Read(long a,int n)` 排除低于65536的地址和超过65536的长度，且要求 ReadProcessMemory 成功、返回字节数等于请求数。`Q` 读8字节指针，`U` 读4字节无符号值，`F` 读4字节浮点并排除 NaN/Infinity。2440 行 `Resolve(long arena,uint offset)` 拒绝 `uint.MaxValue`，用 `checked(arena + offset)`；这体现 x64 绝对指针与 arena 内32位相对偏移混用，移植时必须分别建模。

完整玩家链在 [PlayerInfoReader.cs:2697](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2697)：

1. 验进程未退出且启动时间未变；本地 identity 非零、localIndex<16。
2. `arena = Q(image + 102260400)`，`worldOffset = U(image + 98614988)`，`world = Resolve(arena,worldOffset)`。
3. `playersHeader = Resolve(arena,U(world+252))`，count 在1..16；从 header+12 的相对偏移定位数组，按4字节元素取玩家相对偏移。
4. 从 `Resolve(arena,U(world+284))` 获取队伍表，验证 slot<16，读取 player+1704 的40字节资源块、人口和升级列表，派生关系与战斗群，调用 ReadEntities。
5. 2786–2799 行重新读 arena、worldOffset、玩家列表计数/指针和本地 identity；变化就抛弃整份结果。MatchKey 包含 PID、启动 ticks、world、本地玩家地址和身份；再捕获 marker 上下文。这降低跨局及中途列表变化造成的混合快照，但不是游戏侧原子快照保证。

[PlayerInfoReader.cs:3281](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:3281) 在班组读取前后检查 Match、arena/worldOffset；3310–3324 行检查 entity generation(+96)、squadOffset(+124)、owner(+88)、存活 component 与坐标，再对 squad identity 前后复验。它排除对象池复用、旧对象和无效坐标，不能以一个相等数字作为定位证据。

### 2.3 相机与渐进小地图发现

[PlayerInfoReader.cs:1118](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:1118) 从 `Q(Q(image+102260680)+184)` 的容器寻找相机，候选须满足 vtable `image+64684536`；从 core+416 指向的数据读816字节，View位于+88、Projection位于+752。检查有限数、矩阵关键项、平移一致性，最后复查 core+416；缓存命中同样核验类型。

[PlayerInfoReader.cs:1539](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:1539) 先 MiniTree53，再 hint/缓存候选，再 VirtualQueryEx 区域扫描。单批主动预算80ms、请求累计32MiB、64KiB块、8字节对齐；区域要求 committed/private，排除guard/no-access且保护位匹配，小区域pass限制4MiB。扫描匹配 `image+65511760` 只是候选，仍通过 ReadMinimapLayout 和中心位置条件验证；不能把 vtable 匹配当最终发现。

最多64个候选、60秒时效、每轮至多4个缓存候选；一般100ms后继续，完整绕回可延至2000ms。这些是代码调度意图，不是本轮测得性能。32MiB按请求长度计，不是成功读取量；扫描中止也不证明不存在对象。

[PlayerInfoReader.cs:1203](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:1203) 切局递增 miniEpoch 并清状态；RecoverMinimap同样递增。任务提交前验证epoch；1803–1814行finally只在 scan token 仍属于自己时清 miniScanning。epoch防旧局结果，token防旧任务收尾覆盖新任务，两者不可合并省略。

## 3. 写入和“补丁”的判定

本轮对客户端目录全部 `*.cs` 搜索 `WriteProcessMemory|VirtualAllocEx|VirtualProtectEx|CreateRemoteThread|NtWriteVirtualMemory|DllImport|GameVersion45\.`；返回的PInvoke为读取、查询、时钟、热键、窗口和GDI相关，前五项写入/注入API名称零命中。结合0x410权限与以上控制流，已核查功能属于外部读取和本地显示。

局部缓存会写工具自己的文本文件，例如 grep 定位到 PlayerInfoReader.cs:1107 与1900的 hint/root保存；这不等于写游戏内存。`SquadMarkerWindow.RestoreLayer66`（146–163）调用 `SetWindowPos(HWND_TOPMOST,...,19)` 恢复窗口Z序，没有原指令保存、远程写入、跳板或代码恢复事务。

搜索名称缺失不是对动态导入、依赖、所有二进制行为的形式化排除。五类原版映射提升了本报告对指定原EXE相同五类的可信度；其余类只支持当前源码调用点结论。若新增写入路线，需要独立的版本/原字节检查、所有权、冲突恢复及关闭/退出测试，现有只读证据不能替代。

## 4. 覆盖层实现与生命周期

[SquadMarkerWindow.cs:111](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/SquadMarkerWindow.cs:111) 是无边框、TopMost、不入任务栏、ShowWithoutActivation 的独立WinForms窗口。ExStyle `134742176 = 0x080800A0` 包含 NOACTIVATE、LAYERED、TOOLWINDOW、TRANSPARENT；WndProc对 WM_NCHITTEST 返回-1、WM_MOUSEACTIVATE返回3，形成鼠标穿透且不抢焦点。背景色作为TransparencyKey，使用DoubleBuffered。

[SquadMarkerWindow.cs:350](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/SquadMarkerWindow.cs:350) 的Frame先检查Gate、scene、3秒新鲜度、前台PID、最小化、战术地图与有效客户区，失败就Pause隐藏。按PositionRate读班组；FrameRate控制绘制节奏；弹道/空域约33.33ms调度；每帧FreshCamera后调用 camera.Project，把世界坐标变成客户区像素，再叠加ClientToScreen的窗口位置。实际投影公式属于MineCamera，本轮未逐行审核该类，不能据调用点宣称矩阵约定已动态验证。

[SquadMarkerWindow.cs:547](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/SquadMarkerWindow.cs:547) 更新窗口bounds、按dirty region刷新；600–645行的OnPaint用GDI Graphics绘制图标与线条，统计绘制次数、P95、采样年龄，明确排除游戏渲染/显示器延迟。这个主班组窗口并未直接调用AlphaSurface。

另一条半透明路线在 [AlphaSurface.cs:84](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/AlphaSurface.cs:84)：32位top-down DIB、Format32bppPArgb、UpdateLayeredWindow和预乘alpha；清理Graphics/Bitmap、恢复旧GDI对象、删除DIB/DC。实际消费者为 [AirOverlayWindow.cs:420](/F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/AirOverlayWindow.cs:420)，清透明画布后绘制空域/小地图，441行Present。该消费者本轮未建立原EXE映射，只能归当前源码证据。

DropConnection（SquadMarkerWindow.cs:312）释放reader并清Match、标记、绘制集合和计时；失效/过期时隐藏，连续5帧异常或切局时清commands隐藏；Dispose释放时钟和sprites。关闭功能的恢复是移除自己显示内容，不是恢复游戏状态。静态代码并未证明所有瞬时失败都立即清屏；异常分支允许不足5帧期间保留先前画面。

## 5. 证据等级与既有验收边界

| 结论 | 本轮出处/工件 | 等级 | 限制 |
|---|---|---|---|
| 原版PE/CLR与版本身份 | pe-identity.json、input-identity.json、原版配置 | S通过 | 未执行原EXE |
| 原版五类映射 | comparison-clean.json、五份original导出 | S通过 | 统一换行/末尾空白并排除工具通知；仅五类 |
| 外部只读、arena模型、扫描与绘制机制 | 上述源码行号 | S通过 | 没有真实内存或运行时观测 |
| 服务端120/120、三版各七组、截图零像素差、音频载荷一致 | 既有protection-validation.md:30–54 | 历史验收转述 | 本轮只读报告，未复跑底层日志/测试 |
| 完整实际游戏、跨新进程/新局、长期/第二机 | 未执行 | G/R not_run | 缺少本轮行为与生命周期证据 |
| fixture、编译、交付启动 | 未执行 | F/D not_run | 本任务仅静态报告 |

[存量报告:82](/F:/WorkSpace/Game/CoH3/dev/SkyEye/reports/protection-validation.md:82) 明列未覆盖实际游戏完整对局、长期压力、第二台机器、混淆后全部游戏内路径等；54行真实激活窗口也明确不代表进入游戏。服务端签名、Obfuscar保护与游戏读取是独立机制，报告中的120项测试不能证明偏移可用；本轮不继续调查授权系统或私有状态。

## 6. 可迁移经验和开发者执行清单

可迁移的是结构与验证方法：只读最小权限；PID+启动时间+模块+Match generation绑定；完整读与语义不变量分层；缓存复验；容器前后重读；预算扫描与候选再验证；epoch/token分离；发现/采集/投影/绘制解耦；过期隐藏；GDI资源归属与销毁顺序。不能迁移的是本报告所有RVA、vtable、arena偏移、component ID、队伍编码、矩阵布局、进程名或hash；修改器为.NET也不证明目标游戏为Unity/托管引擎。

开发时按顺序执行：

1. 为目标游戏记录真实模块hash、架构、引擎文件证据；把RVA和结构放入按build hash选择的版本描述。保留旧版，未知hash先标为待验证。
2. 在明确测试构建中用受控移动/资源变化建立候选，至少用对象类型、owner/generation、数量边界等两个独立不变量排歧义；保存每级解引用和完整读取长度。
3. 先实现单一只读快照接口和离线短读/无效矩阵/过期session fixture；不要依赖manifest答案抄地址。
4. 在至少两个新进程中重定位，再覆盖新局/重连/进程退出、最小化、切前台、DPI/窗口移动；记录预期与实际截图、读取年龄和P95帧间隔。
5. 若游戏更新，重验根、布局、对象语义和投影；若增加写入，单独实现与验证补丁事务。最后才做交付目录依赖和启动验证。

本轮最具体的下一动作是对受控目标游戏构建做只读G级对照：移动同一单位并改变相机，保存世界坐标、矩阵、投影像素与画面标记的同步记录；随后重启游戏重复。该动作未在本轮执行。

## 7. 结果和资源记录

静态分析、身份采集、五类原版映射与机制报告成功。首次JSON提取因PowerShell默认编码失败，UTF-8重读成功；初次五类比较因ILSpy升级提示全为false，排除通知后的比较全部true。上述失败保留在transcript和原比较文件中。

实际游戏运行、真实效果、跨会话稳定性均为not_run。运行时未提供本子任务的token统计或duration_ms；token/time均记未知，不根据文件数量、工具调用或墙钟估造。未生成虚假的metrics。
