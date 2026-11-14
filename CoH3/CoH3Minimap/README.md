# CoH3Minimap 0.5.6-native-preview

独立置顶窗口，从游戏的原生 HUD 模型读取小地图数据。0.5 已改用原生 Marks、图标句柄、SelectionColor、地图角度与相机框；显示范围由游戏当前的原生 Marks 集合决定。

程序仅申请进程查询与读取权限，使用 ReadProcessMemory。默认流程按已验证的根指针读取有界集合，读取地图归档，不写入游戏内存、不注入、不修改游戏配置。

## 运行

解压发行包，双击 CoH3Minimap.exe。运行游戏并进入具有小地图的场景后，窗口会自动连接。首次读取新地图时，后台从游戏的 ScenariosMP.sga 解码对应底图；界面继续响应。

- 左键拖动窗口，右键选择大小或关闭。
- 窗口置顶，采集和重画约 10 Hz。
- 游戏退出、地图切换或对象变化时，丢弃失效样本并重连。
- 状态栏显示已绘标记、缺图、未支持项和原生迷雾开关；原生属性为真时标明“赛后查看地图”。
- 当前预览没有授权码要求。
- 0.5.2 新增右键菜单“实验 CPU 迷雾（mod=1，无时间混合）”，默认关闭。也可运行 `CoH3Minimap.exe --experimental-cpu-fog` 开启；窗口标题和状态栏会标明实验模式。原生开关未知、原生关闭或缺少有效位平面时，跳过该层。
- **0.5.3 新增右键菜单“显示全部单位（迷雾上敌军）”，默认关闭**；也可运行 `CoH3Minimap.exe --show-all-units` 开启。
- **0.5.4 修正多人局着色：实时判定本地玩家（session+0x08），本方蓝、其余玩家全部红**；旧版 1000/1001 硬编码在 4v4 多人局会把敌我混色。
- **0.5.5 区分队友与敌军：接入原生队伍表（Player_GetTeam handler rva 0xBE7A00 同源，`team = U32[segBase+U32[sessOff+0x11C]+slot*4]`），本队（含队友）=蓝、敌队=红**；实测 4v4 队伍表 `[0,1,0,1,0,1,0,1]` 按 slot 分两队。队伍读不到时退回本地玩家 ID 着色。
- **0.5.6 单位图标全员化 + 鼠标穿透**：图标候选链扩为 SBP 原名 → `原名_symbol` → 阵营前缀变体（`unit_german_`/`unit_aef_`/`unit_british_`/`unit_soviet_`/`unit_west_german_` + 去阵营后缀名 + `_symbol`，及对应 `vehicle_` 前缀）→ 去后缀名 → 类别兜底；步兵不再是圆点，按阵营使用通用步兵符号（ger/dak→grenadier_ger、us→riflemen_us、uk→unit_british_tommy_with_tommy_symbol、sov→unit_soviet_conscript_01_symbol、ak→unit_west_german_volksgrenadiers_symbol，无阵营尾缀退 attachment_infantry_uk_symbol）。图标 16px 绘于 22px 队伍底色圆上（露出约 3px 蓝/红敌我环）；无法识别的单位保持 8px 圆点。右键菜单新增「鼠标穿透（小地图上右键解除）」：开启后窗口对所有鼠标点击透明（WS_EX_LAYERED|WS_EX_TRANSPARENT），游戏可正常操作；在小地图区域内按下右键会自动解除穿透并弹回菜单（全局低级鼠标钩子拦截该次右键，不会传给游戏）。状态栏新增「单位 已画/总数 · 本队 N」计数。

## 显示全部单位（迷雾上敌军）

0.5.3 通过只读枚举本地模拟侧全部小队（Squad），把包括迷雾下敌军在内的所有单位全局显示在外部小地图上，不需要修改游戏进程或内存：

- 枚举链：sim World 槽位表 → 有效槽 → 小队对象 → 坐标 `f32[squad+0x2C/0x34]`、玩家 `U32[player+8]`（静态 SCAR 证据链 + 只读实测双重验证）。
- 每支小队再读蓝图（SBP）名与类型表（124 种类型，每次启动重建），按类型显示游戏原版符号图标：候选链 = SBP 原名 → `原名_symbol` → 阵营前缀变体（unit_/vehicle_×阵营×去后缀×_symbol）→ 去后缀名 → 类别兜底（armor→tank_icon、vehicle→vehicle_light_symbol、artillery、team_weapon_minimap_icon、sniper_ger、emplacement_symbol），步兵按阵营通用步兵符号，覆盖 423 张解包原生符号；无法识别时用实心圆点兜底。
- 队伍底色（0.5.5 起）：每支小队按其玩家的 slot 查原生队伍表（`U32[segBase+U32[session+0x11C]+slot*4]`，与游戏 SCAR `Player_GetTeam` 同源），与本地玩家队伍相同=蓝（含全部队友）、不同=红；队伍不可用时退回本地玩家 ID 判定。图标绘制在迷雾层之上。
- 实测：真实对局中敌军 10 支小队与窗口红点一一对应（像素簇统计）；4v4 对局 60 支小队按队伍表正确分红蓝（队伍表 `[0,1,0,1,0,1,0,1]`）；防御工事正确显示符号图标（[验证报告](artifacts/entity-integration-20261112/ENTITY_INTEGRATION_REVIEW.md)）。
- 边界：坐标映射为结构+相机推理与数量级像素验证，未做单点像素回归；小队数据非原子快照；蓝图名匹配不到符号时使用类别兜底。

实验层使用原生三个位平面及已核对的生成/模糊数学，位于领土之上、标记和视口之下，0.9透明度只应用一次。它采用 mod=1 假设，没有上一帧时间混合。当前活跃 GPU 材质参数、动态过渡仍在核对；完整 Level B 验收尚未通过。

窗口中的“迷雾：原生关闭”来自游戏自己的 UI 资源选择开关；程序仅读取该属性。

## 已验证内容与边界

| 项目 | 结果 |
| --- | --- |
| 原生 HUD 根、Marks 集合、Position、Scale、图标和颜色缓存 | 静态 REA/原字节核对、合成测试及真实进程采样通过 |
| 地图中心坐标、Y 翻转、实际 Map.Angle、任意旋转完整显示 | 原生布局公式与合成像素测试通过；真实窗口静止场景相符 |
| 当前地图底图 | 按原生路径精确读取 SGAv10/rrtex；Twin Beach 与 Semois 两图解码像素哈希核对通过 |
| 领土、可交互区遮罩 | 原始 BGRA 与 alpha 通道已核对；真实场景红蓝、边界和遮罩相符 |
| 相机视口 | 按原生 PerspectiveBox 的 0→1→3→2 顺序绘制；静止画面相符 |
| 迷雾原始数据与生成数学 | 三个位平面、LSB 位序、灰度优先级、边框、单轮 3×3 模糊已核对并测试 |
| 实验 CPU Fog 窗口层 | 默认关闭；真实三样本旧版/default与新版/off逐字节一致；on模式暗层、图层顺序、菜单切换及19标记保留已核对 |
| 活跃迷雾最终 UI 材质、上一帧混合 | CPU 候选采用 mod=1、无时间混合；实际 GPU 参数与逐帧复刻尚未完成 |
| 原生“赛后查看地图”状态 | 原 XAML 绑定、属性注册、REA getter 与 6 帧只读缓存核对通过 |
| 连续移动、切换视角时的逐帧同步误差 | 尚未测量；赛后查看地图的静止截图不作为动态验收 |

标记颜色取原生 SelectionColor 缓存，图案取原生句柄解析结果；不从模型组件或出现数量推断队伍。已实际观察到从 Twin Beach 换到 Semois 后，原生路径、角度、标记与视口一起改变。

以下研究记录保留0.5.1时期的结论；0.5.2窗口接入与验证见本节末。

后续静态研究已核对客户端 `ClientPixelShader` 的109项名称表及构造、缓存、查名路径（[研究报告](../CoH3FowTool/tools/reverse/native_fow_20261111/program_identity_20261112/CLIENT_SHADER_IDENTITY_REPORT.md)）。`Image=2`、`OpacityMaskImage=6` 仅表示这张客户端表的索引；与当前迷雾实际绘制程序、后端纹理槽和 SRV 的关系仍待核对。此研究尚未改变预览版绘制器。

当前战术暂停场景新增了[双窗口对照与第二轮核对记录](<artifacts/research-round2-20261112/ROUND2_REVIEW.md>)：外部显示19/19标记；30次成功采样形成29对相邻尝试，551次匹配坐标比较均无变化。原版迷雾开启，外部仍缺少相应明暗层；这组静止证据未用于动态或完整 Level B 验收。当前资源工厂槽的只读快照已确认默认分支，实际迷雾程序与 SRV 绑定仍在追踪。现有发行包未改。

新增的 [Fog 绘制路径核对](<artifacts/research-fog-path-20261112/FOG_PATH_REVIEW.md>) 已追到命令配对消费、两个 Fog 名称的专用 Technique ID、批次参数和提交数组算法。数组函数的11段原范围、690字节、159条指令均由实际 REA 完整覆盖，105项独立检查通过；当前 GPU 绑定、最终迷雾合成与动态同步仍未验收，0.5.1发行保持原样。

[实际加载的 D3D12 导出对照](<../CoH3FowTool/tools/reverse/native_fow_20261111/loaded_d3d12_exports_20261112/LOADED_D3D12_EXPORTS_REPORT.md>) 已将原 ordinal 101/102 与命名导出和游戏 IAT 对应，45项离线核验通过。此观察闭合了导出身份；具体设备对象的来源、创建返回值与活跃绘制绑定仍是后续验收项。

补充的 [创建回调与字段快照](<../CoH3FowTool/tools/reverse/native_fow_20261111/device_creation_upstream_20261112/CREATION_UPSTREAM_REPORT.md>) 记录2026-11-12 13:28+08:00观测时回调槽指向 A2CBB0，46项离线检查通过。完整发布窗口已证明调用参数等于发布存储使用的对象数值；真实 REA 声明缺口仍保留，历史创建过程尚未证实。[13:42的新连续采样](<artifacts/research-live-20261112-134243/CURRENT_OBSERVATION.md>) 共20次尝试、18次成功，16对连续有效尝试中304次标记坐标比较均无变化，迷雾选择器均为开启。本组没有同步画面对照，移动、迷雾合成与 Level B 验收继续保持未通过。

[两组 arena 槽路由](<../CoH3FowTool/tools/reverse/native_fow_20261111/draw_arena_routing_20261112/DRAW_ARENA_ROUTING_REPORT.md>) 的完整单体已核对1308字节、302条指令，116项检查通过，真实 REA 声明缺口为0。[调用入口 RAX 的补充核对](<../CoH3FowTool/tools/reverse/native_fow_20261111/draw_arena_entry_rax_20261112/ARENA_ENTRY_RAX_REVIEW.md>) 另验证了调用窗口与 callee 的14字节前缀：旧槽载入值到达58B500入口，但58B508首次写 EAX 即终止这条显式寄存器数据流。同一生产对象/批次实例、实际消费和最终绘制绑定仍未证实。

新增 [入口来源联合核验](<../CoH3FowTool/tools/reverse/native_fow_20261111/draw_arena_owner_join_20261112/ARENA_OWNER_JOIN_REPORT.md>) 已把 F97460 那一次全局槽载入，经 R12 保存到3AD170入口 P 的数值链闭合，95项检查通过；该等式限定普通 Win64 call-return 路径。[两类回调分流](<../CoH3FowTool/tools/reverse/native_fow_20261111/draw_callback_routes_20261112/CALLBACK_ROUTES_REPORT.md>) 另有54项原字节检查，分开整对象槽与记录槽；后者的默认叶只设 AL=1 后返回。同一生产批次、当前回调值及 GPU 绑定仍待核验，现有应用与发行保持原样。

[分配器原体核对](<../CoH3FowTool/tools/reverse/native_fow_20261111/draw_batch_allocator_20261112/DRAW_BATCH_ALLOCATOR_REPORT.md>) 已通过130项独立检查：普通可达219字节全部进入真实 REA 声明，仅漏不可达的1字节 CC。返回值为当次 arena 基址加 offset，本体写入集中在容量字段，记录登记与消费仍待核对。[2026-11-12 15:00的窗口观察](<artifacts/research-window-state-20261112-150035/CURRENT_WINDOW_STATE.md>) 清楚显示“战术暂停已启用”和08:55游戏钟；该图没有同步HUD采样，未与13:42样本合并为动态验收。

新增 [真实原字节 CPU Fog 实验](<artifacts/fog-cpu-live-20261112-154016/CPU_FOG_EXPERIMENT.md>)：开发源码已支持显式原字节侧车，Ancona场景3/3样本的字节、SHA、置位数与普通诊断一致；独立复算与C#生成/mean9逐字节相同，38项检查通过，并生成标注对照图。该图采用mod=1、无temporal假设，后续截图与采样分属两个时刻；实时窗口Fog层、当前GPU合成和动态同步仍待验收，0.5.1发行包保持原样。

0.5.2已完成 [实验CPU Fog窗口接入](<artifacts/fog-window-integration-20261112/FOG_WINDOW_INTEGRATION.md>)：14组自测通过，同一批3份真实数据的旧版/default与新版/off原始BGRA逐字节一致，独立菜单切换检查通过。成组静态截图中两个外部窗口均19/19标记；32项现场核验通过，实验层仅使地图变暗。[三栏对照图](<artifacts/fog-window-integration-20261112/live-review/live-window-comparison.png>) 标明游戏当时战术暂停08:55，采样与截图为相邻的独立区间；未按同GPU帧、持续移动或完整Level B验收。

## 支持版本

当前仅支持 build 26550104，游戏主程序 SHA256：

```
2406B3EE580BFC983B0AEA93A31C26F9A6C98F10F706C5CEC6A13AB5E3A22CDE
```

连接前校验磁盘主程序哈希。更新后哈希变化时停止读取固定偏移，需重新验证版本。

## 自测与诊断

```powershell
CoH3Minimap.exe --selftest
CoH3Minimap.exe --native-probe --samples 10 --output native-hud.json
```

自测使用合成内存与真实 WinForms 消息循环，不连接游戏。诊断命令只读当前进程，并输出原生根、标记、图层摘要、迷雾资源选择状态与 MatchState。MatchState 记录 IsGameOverSpectatingMap 的原生缓存、CacheEpoch、GlobalEpoch 和两者是否相等；它不判定模拟是否正在运行。读取失败、缓存失效或未核对的 HUD 类型返回未知；前后二读相同也不是原子快照。成功采样不等于完整画面或账号检测验证。程序为 WinExe，从脚本运行时请等待退出并检查进程 ExitCode。

0.5.2 探针包含 `ProbeSchemaVersion=2`：每次尝试记录零起始序号、`Stopwatch` 单调起止时间、成功或丢弃原因，并分别统计已尝试、已捕获与未执行次数。时间区间包含诊断哈希计算，代表采集尝试，不代表游戏帧。

0.5.2 也可显式导出三 plane 原字节与原 CPU 图层，最多5个样本，两输出须为独立新文件；既有文件保持原样。无该选项时只输出摘要。以下命令等待发行程序退出，输出目录应每次另取新名称，详细边界见 [CPU Fog 实验](<artifacts/fog-cpu-live-20261112-154016/CPU_FOG_EXPERIMENT.md>)。

```powershell
$capture = Start-Process .\CoH3Minimap.exe -ArgumentList @(
  '--native-probe', '--samples', '3',
  '--output', 'artifacts\fog-raw-fresh\probe.json',
  '--fog-bytes-output', 'artifacts\fog-raw-fresh\fog-raw.json'
) -Wait -PassThru
$capture.ExitCode
```

[离线分析器](<E:/Cache/AI/Deepseek/default-workspace/CoH3Minimap/tools/analyze_native_continuity.py>)按场景身份与标记模型、载荷、类型匹配，仅比较相邻且均成功的采集尝试；跨丢弃样本、切换场景和缺失图层恢复不计作连续变化。旧报告缺少尝试日志时，尝试数、丢弃数和未执行数保留未知。它只读取 JSON；位置或哈希发生变化也不等于画面对照、延迟或 Level B 验收通过。

```powershell
python tools\analyze_native_continuity.py native-hud.json --output native-summary.json
```

## 构建与发行

```powershell
dotnet build -c Release
pwsh -File build-preview.ps1
```

构建脚本发布 Windows x64 自包含单文件，等待发行 EXE 的自测结束，再生成 ZIP、SHA256 和验证报告。图标随 EXE 内嵌；地图底图从本机游戏归档读取。首次地图解码在后台线程进行。

第三方组件和原始许可见 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) 与 licenses 目录。游戏资源的权利属于相应权利人。

## 账号验证状态

只读描述的是访问方式，并非免检测或免封的证明。本版没有公开匹配、天梯免封的验证证据；此前账号限制的具体处置原因也没有后台证据。本预览不作免封承诺。
