# Bf6Esp 架构说明

> Battlefield 6（`bf6.exe`）外部只读 ESP 覆盖层。
> 工具线源码：`BF6/dev/Bf6Esp/`，交付物：`BF6/output/Bf6Esp/`。

---

## 1. 目标与边界

Bf6Esp 是一个**外部、只读**的覆盖层工具：在游戏画面之上绘制玩家方框、血条、距离、名字等 ESP 信息。

它**只做三件事**：

1. 以只读方式打开游戏进程并读取内存；
2. 在游戏窗口之上绘制一个透明图层窗口；
3. 按授权状态决定是否允许运行。

它**明确不做**（见第 2 节）：注入 DLL、写游戏内存、给客户端打补丁、加载驱动、改写游戏文件。

---

## 2. 为什么坚持「纯外部只读」

Battlefield 6 随附 **EA Javelin 内核级反作弊**（`EAAntiCheat` + `EAJavelinInstaller_installscript.vdf`，见 `BF6/original/Battlefield6-Steam/BUILD-INFO.md`）。

内核级反作弊改变的是「被抓到之后的后果」，而不是「能不能抓」：

| 路线 | 需要的权限 | 被检测到时 | 结论 |
| --- | --- | --- | --- |
| 注入 DLL / 远程线程 | `PROCESS_CREATE_THREAD` + `PROCESS_VM_OPERATION` + `PROCESS_VM_WRITE` | 客户端内出现非游戏模块，Javelin 可直接判定篡改 | 不采用 |
| 写内存改数值 | `PROCESS_VM_WRITE` | 内存改动可被校验链路发现 | 不采用 |
| 客户端补丁 / 绕过校验 | 需要改文件或改内存 | 文件校验 + 内存校验双重暴露 | 不采用 |
| 内核驱动读写 | 需要驱动签名/加载 | 内核组件本身就是高危特征 | 不采用 |
| **外部只读 + 独立覆盖层** | **仅 `PROCESS_VM_READ` + `PROCESS_QUERY_LIMITED_INFORMATION`** | 客户端内存零改动，无第三方模块，无补丁 | **本项目采用** |

只读路线把「可被观测到的行为」压缩到最小：进程里没有任何我们的模块、没有任何被改写的字节、没有任何跨进程写入。覆盖层是**独立进程的独立窗口**，游戏侧看不到它。

代码层面强制这一约束：

- `Memory/ProcessHandle.cs`：句柄包装明确只申请 `PROCESS_VM_READ | PROCESS_QUERY_LIMITED_INFORMATION`，并在注释里写明「永不申请写/注入权限」的原因；
- `Memory/MemoryReader.cs`：**只有读 API**，类中不存在任何写入方法（连 `WriteProcessMemory` 的 P/Invoke 都没有引入）；
- `Memory/NativeMemory.cs`：P/Invoke 表面只包含 `OpenProcess / CloseHandle / ReadProcessMemory / CreateToolhelp32Snapshot / Module32First / Module32Next`。

---

## 3. 组件与数据流

```
                    ┌──────────────── 授权层 (License/*) ────────────────┐
                    │ LicenseCoordinator60 → LicenseApi60 → LicenseGate60  │
                    │ 状态：未授权/已授权/过期/设备不匹配/使用中/需更新     │
                    └───────────────┬────────────────────────────────────┘
                                    │ Allowed（未授权时读取与覆盖层都不放行）
  ┌─────────────────────────────────┴──────────────────────────────────────────┐
  │ ① 进程与内存层 (Memory/*)                                                  │
  │    GameProcess.Attach()  → 进程名 bf6.exe / bf6Trial.exe                   │
  │    Toolhelp 模块快照     → ModuleBase / ModuleSize / ExePath                │
  │    ProcessHandle          → 只读句柄                                       │
  │    MemoryReader           → Read<T> / ReadBytes / TryRead（失败计数）      │
  │    PeImage                → DOS/NT 头 + 节表（内存优先，磁盘回退）        │
  │    SignatureScanner       → AOB 扫描（?? / ? 通配），按节或整模块            │
  └─────────────────────────────────┬──────────────────────────────────────────┘
                                    │ 读取
  ┌─────────────────────────────────┴──────────────────────────────────────────┐
  │ ② 档案层 (Profiles/*)                                                      │
  │    OffsetProfile：offsets / structs / signatures / enums / resolution       │
  │    ProfileStore：按 build SHA-256 选档案；不匹配则退回签名解析；再不行报   │
  │                  「版本不匹配」                                             │
  │    CalibrationReport60：校准模式的产物（候选地址、命中 RVA、启发式发现）   │
   └─────────────────────────────────┬──────────────────────────────────────────┘
                                     │ 档案里的静态锚点（段表 / 唯一 AOB / 名字 RVA）
   ┌─────────────────────────────────┴──────────────────────────────────────────┐
   │ ②' 运行时定位层 (Esp/RuntimeLocator60.cs, Esp/RuntimeReflection60.cs,        │
   │                    Esp/EntityFieldProfiler60.cs)                            │
   │    RuntimeLocator60：四步流水线                                             │
   │      ① 反射扫描：MEC 式 TypeInfo/FieldInfo 链 → 字段名 → 偏移              │
   │      ② 实体容器：.data 里 {指针,数量,容量} 三元组                            │
   │      ③ 投影矩阵：float[16] 形状扫描 + 视口宽高比/落点率打分                 │
   │      ④ 字段推断：不靠名字，靠多实体数值分布反推血量/队伍/位置/姓名           │
   │         （支持 Frostbite ECS：实体里只存组件指针）                            │
   │    RuntimeLayout60：字段偏移 + 组件指针槽（FieldPointerOffsets）             │
   └─────────────────────────────────┬──────────────────────────────────────────┘
                                     │ 运行时布局（内联偏移 or 组件指针 + 内层偏移）
   ┌─────────────────────────────────┴──────────────────────────────────────────┐
   │ ③ ESP 数据层 (Esp/*)                                                       │
   │    IEntitySource（抽象）                                                   │
   │      ├─ ProfileEntitySource     用档案里的 offsets/structs 取值            │
   │      ├─ CalibrationEntitySource 用校准报告里的 resolved/structs 取值      │
   │      └─ RuntimeEntitySource     用 RuntimeLayout60 取值（内联/组件统一解析）│
   │    PlayerReader      → EntitySnapshot 列表（未知字段优雅跳过并记录日志）   │
   │    WorldToScreen     → 4x4 视图投影矩阵 → 屏幕像素（相机背后/出屏返回 false）│
   └─────────────────────────────────┬──────────────────────────────────────────┘
                                     │ 快照 + 投影结果
   ┌─────────────────────────────────┴──────────────────────────────────────────┐
   │ ④ 渲染层 (Esp/OverlayWindow.cs, Esp/EspRenderer.cs)                        │
   │    OverlayWindow：透明分层窗口（WS_EX_LAYERED|TRANSPARENT|TOOLWINDOW|      │
   │                  NOACTIVATE），UpdateLayeredWindow 逐像素 alpha，           │
   │                  跟随前台游戏窗口矩形，取不到则退回主屏                    │
   │    EspRenderer：GDI+ 抗锯齿绘制方框/血条/距离/名字/下拉线/FOV 圈/离屏箭头   │
   │    EspEngine60：16ms 定时器驱动整条读取→投影→绘制链路                    │
   │    EspSettings + SettingsForm：设置持久化到 overlay-settings.txt          │
   │    HotkeyWindow60：RegisterHotKey + 隐藏消息窗口，全局热键切换覆盖层      │
   └────────────────────────────────────────────────────────────────────────────┘
```

### 目录

| 路径 | 说明 |
| --- | --- |
| `client/Memory/` | 进程、只读句柄、读内存、PE 解析、AOB 扫描 |
| `client/Profiles/` | 偏移档案、档案存储/匹配、校准报告、JSON 读写 |
| `client/Esp/` | 运行时定位与字段推断、实体快照、实体来源抽象、投影、渲染、覆盖层窗口、设置、热键 |
| `client/License/` | 从 CoH3 天穹之眼复制并适配的授权客户端栈 |
| `client/App/` | 入口与各 CLI / GUI 模式（probe / calibrate / selftest / 激活窗口） |
| `profiles/` | 随包档案（当前 build 的 `bf6-1.0.0.0.json`） |
| `docs/` | 本文件与状态表 |

---

## 4. 档案（profile）绑定：为什么偏移不进代码

游戏每次更新都会重排内存。所有**易变数据**（地址、成员偏移、特征码）一律不进源码，而是放进与 build 绑定的 JSON 档案：

- 档案里记录 `buildSha256`（`bf6.exe` 的 SHA-256）、`moduleSize`、以及 `offsets / structs / signatures / enums / resolution / sections`；
- 运行时用 `ProfileStore.ComputeSha256(游戏 exe 路径)` 计算当前 build 的哈希，再 `MatchByHash` 选档案；
- **命中** → 用档案里的偏移；
- **未命中但模式串可用** → 走签名解析（`ProfileStore.TryResolveBySignature`，支持 `match` / `match+off` / `rel32` / `value` 四种解析模式）；
- **两者都不行** → 明确报「版本不匹配」，并提示需要重新校准，绝不拿旧偏移硬跑。

这样游戏更新后，受影响的只是一个 JSON 文件，而不是一次重新编译；而且旧 build 的档案仍然保留，可随时切回。

**本 build 的特殊之处**：BF6 的 `bf6.exe` 里**有类型名、有成员偏移，但没有成员名**（第 5 节有完整证据链）。因此档案里的 `offsets` 只能是"已知锚点 + 兜底"，真正的字段偏移由 `RuntimeLocator60` 在运行时推断（反射扫描 → 数值分布反推）。档案命中与否决定的是"能不能少走弯路"，不是"能不能跑"。

---

## 5. 校准（calibrate）工作流：偏移是怎么被填进去的

### 5.1 为什么不能靠"静态找偏移"

对当前 build（`bf6.exe` SHA-256 `1F385AD9…BAEFF4`）的静态分析结论已经收敛（详见 `docs/STATUS.md` §2.2）：

- **类型名在**：`.rdata` 里有 `ClientSoldierEntity`、`ClientSoldierHealthComponent` 等明文类型名，`typeinfo` 段有 23,701 条类型记录；
- **成员偏移在**：`fieldinf` 段里存在大量小整数（成员偏移）与 `.data` 类型元胞指针；
- **成员名不在**：整个 exe（含 `.cas` 数据包、`layout.toc`）里都找不到「字段名 → 偏移」的对应关系。typeinfo 记录里的 32 位 key 是**构建期烘焙**值（70+ 种摘要族全部 0 命中，并用 gcd / 低 8 位一致性做了结构性排除），`.vlizer` 段是代码而非数据，`layout.toc` 是高熵加密块。

⇒ 结论：**按名字取偏移在 BF6 上不成立**，只能在活进程内存里做（运行时反射），或者不依赖名字、改用数值分布反推。

### 5.2 运行时定位（默认路径，无需手工填偏移）

`Bf6Esp.exe` 启动覆盖层时会后台跑 `RuntimeLocator60`，四步：

1. **反射扫描**（`RuntimeReflection60`）：按 MEC 风格布局解析进程内存里的 `TypeInfo` / `FieldInfo` 链，取出「字段名 → 偏移」；成员名此时已在内存中（这正是静态拿不到的那部分）。
2. **实体容器**：扫描 `.data`，找 `{指针, 数量, 容量}` 三元组（数量 1–8192、容量 ≥ 数量、指针 8 字节对齐且落在已提交区间、前 64 元素有效数 ≥ 4）。
3. **投影矩阵**：扫 `.data` / `.rdata` / `ctr` / `.rodata` 与堆，找 `float[16]`（`m[15] ≈ 0`、`m[0]/m[5] ∈ (0.02,50)`、`m[10]/m[11]/m[14]` 之一 `≈ ±1`），再用视口宽高比误差 + 实体投影落点率打分。
4. **字段统计推断**（`EntityFieldProfiler60`）：**不靠名字**，靠多实体数值分布反推字段：
   - 血量 = 所有实体取值 ∈ [0.5, 1000] 且不全相等；最大血量 = 与之同量级的**恒定**槽；
   - 队伍 = int ∈ [0,64]、取值种类 ≤ 8、非全零；
   - 位置 = 连续三 float 有限且 |v| < 1e5、跨实体至少两轴变化；**或** 4x4 变换矩阵的平移分量（行主序 `[12,13,14]` / 列主序 `[3,7,11]`，先过 `LooksLikeTransform` 形状校验）；
   - 姓名 = 8 字节对齐处是已提交区间内的指针且指向可打印 ASCII；
   - **ECS 组件布局**：实体里可能只存组件指针，因此额外做一遍「组件寻址」——把实体头部每个 8 字节槽当组件指针探一遍，命中字段记为「内层偏移 + 指针槽」（`RuntimeLayout60.FieldPointerOffsets`）。

推断结果写入 `RuntimeLayout60`，`RuntimeEntitySource` 按「内联偏移 or 组件指针 + 内层偏移」统一解析取值。

### 5.3 校准命令与报告

```powershell
# 游戏运行时，导出原始发现（全程只读，不写游戏内存）
Bf6Esp.exe --calibrate                      # 写入 profiles\calibration-<UTC>.json
Bf6Esp.exe --calibrate --out D:\find.json   # 或指定路径
```

报告（`CalibrationReport60`）内容：

1. 模块基址/大小、PE 节表、磁盘 exe 的 SHA-256；
2. 档案里每条签名的命中数与命中 RVA（未定位的模式串会被标为「模式串非法」，不会产生误命中）；
3. **通用启发式**：像投影矩阵的 4x4 float 数组、目标落在 `typeinfo`/`fieldinf` 节内的指针、指针的连续数组；
4. **反射扫描结果**：`reflectionTypes`（类型名/大小/super）、`reflectionFields`（字段名/偏移/归属类型/置信度 1–3）、`reflectionSummary`；
5. **运行时布局结果**：`resolved`（实体列表地址、实体数量、投影矩阵地址、本地玩家地址、本地队伍）与 `structs`（位置/血量/队伍/姓名的成员偏移）。

### 5.4 离线自检（不需要游戏）

| 命令 | 校验内容 |
| --- | --- |
| `--selftest` | 只读通道：PE 头解析 + 256 字节 RPM 往返逐字节比对 |
| `--selftest-reflect` | 合成夹具上的反射解析：7/7 字段偏移 + 2/2 类型还原 |
| `--selftest-locate` | 对自身进程跑完整定位流水线（限堆预算），验证不崩、不死循环 |
| `--selftest-entity` | 合成夹具上的字段统计推断：三个布局场景（连续 float / 列主序矩阵 / ECS 组件指针）各 12/12 |

`--probe` 是同一套能力的轻量版：只回答「游戏在不在 / 档案匹不匹配 / 签名命中几个 / 读取失败几次」。

---

## 6. 游戏更新后的适配步骤

1. 记下新的 `bf6.exe` SHA-256（`Get-FileHash` 或 `--calibrate` 报告里的 `buildSha256`）；
2. 新建档案文件 `profiles/bf6-<新版本>.json`，复制旧档案的 `signatures`（模式串本身通常不变）与 `resolution` 规则，把 `offsets` 里的运行时目标清零（0 = 未知，不是偏移 0）；
3. **刷新静态锚点**：重新跑 `analysis/p14_profile_data.py`，拿到新 build 的节表（RVA/VirtualSize/RawSize）、`RIFXRIFF` 唯一锚点的新 RVA、以及各明文名字的新 RVA，回填 `sections`、`ModuleRegistryMagicRva`、`TypeInfoRecordMarkerRva`、`ReflectionName.*` 与 `signatures` 里的 `Name.*` 命中 RVA；
4. **静态唯一性复核**：每个模式串在全文件必须**恰好命中一次**（`RIFXRIFF` 当前命中 1 次）；出现多命中说明该串已失去定位价值，必须换成更长的上下文或改用运行时推断；
5. 游戏运行时执行 `Bf6Esp.exe --calibrate`，拿到 `resolved`（实体列表/投影矩阵/本地玩家地址）与 `reflectionFields`（字段名 → 偏移）；
6. 把确认过的偏移回填 `offsets` / `structs`（十六进制成员偏移）；组件布局则把「组件指针槽」写进 `structs` 的对应项；
7. 跑 `--probe` 确认「档案命中」；进游戏用覆盖层逐项验证（方框/血条/队伍配色/距离）；
8. 旧档案不删除，按版本并存，便于回退对照。

> **注意**：第 3、5 步的产物在当前 build 上大部分是**自动**的——`RuntimeLocator60` 每次启动都会重新推断，不依赖旧档案里的硬编码偏移；档案里的偏移主要起「加速 + 兜底」作用。这正是为 BF6 这种「有类型名、无成员名」的游戏准备的。

---

## 7. 授权层

授权栈直接从自有产品 **CoH3 天穹之眼**（`CoH3/dev/SkyEye/client/CoH3MemoryTool/`）复制而来，只做了一处语义修改：`ProductId` 改为 `Bf6Esp`。

- 租约公钥 pin（`PinnedLeaseKey60`）与 `skyeye-lease-v2` 签名域**保持原样**（服务端配对关系不能动）；
- 客户端凭据存到 `%LOCALAPPDATA%\Bf6Esp\SelfHostedLicensing`（DPAPI 保护）；
- `LicenseGate60.Allowed` 决定覆盖层是否启动；`MemoryReader` 在未授权时拒绝读取（诊断模式 `--probe/--calibrate/--selftest` 例外，只放开「是否读」，句柄权限不变）；
- GUI 用中文显示状态：未授权 / 已授权 / 已过期 / 设备不匹配 / 使用中 / 需要更新。

---

## 8. 构建与运行

```powershell
# 构建 + 发布到 BF6\output\Bf6Esp\
powershell.exe -NoProfile -ExecutionPolicy Bypass -File F:\WorkSpace\Game\BF6\dev\Bf6Esp\Build.ps1

# 只编译
& 'F:\WorkSpace\Game\LicensingSystem\.cache\dotnet-10\dotnet.exe' build 'F:\WorkSpace\Game\BF6\dev\Bf6Esp\client\Bf6Esp.csproj' -c Release --nologo
```

> 本机的 PowerShell 执行策略会拦截 `F:\WorkSpace\Game` 下的未签名脚本（RemoteSigned + 该卷被判定为非本地来源），
> 因此直接 `& .\Build.ps1` 会报「未对文件进行数字签名」。用上面的 `-ExecutionPolicy Bypass` 方式即可；
> `Build.ps1` 自身带 UTF-8 BOM，Windows PowerShell 5.1 可正确解析其中的中文。

交付目录内容：`Bf6Esp.exe`（+ `Bf6Esp.exe.config`）、`overlay-settings.txt`（默认设置）、`profiles/*.json`、`BUILD-INFO.json`（构建时间、源文件清单 + 每文件 SHA-256、目标游戏 build 哈希）。

---

## 9. 设置项

`overlay-settings.txt`（与可执行文件同目录，key=value）：

| 键 | 含义 |
| --- | --- |
| `overlay_enabled` | 覆盖层总开关（热键切换的就是它） |
| `box_style` | `full` 完整矩形 / `corner` 四角折线 |
| `box_thickness` | 线条粗细 1–8 |
| `max_distance` | 最大显示距离（米） |
| `only_enemies` / `only_alive` | 只显示敌人 / 只显示存活 |
| `show_health_bar` / `show_distance` / `show_name` | 血条 / 距离 / 名字 |
| `show_snap_line` / `show_fov_circle` / `fov_radius` / `show_offscreen_arrow` | 下拉线 / FOV 圈 / FOV 半径 / 离屏箭头 |
| `enemy_color` / `friendly_color` / `neutral_color` / `text_color` / `snap_line_color` | 颜色（`#AARRGGBB`） |
| `health_high_color` / `health_mid_color` / `health_low_color` | 血条三档颜色 |
| `hotkey` | 全局热键：`End` / `F1` / `Insert` / `Home` / `Pause` / `Delete` |
