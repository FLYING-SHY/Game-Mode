# 《英雄连3》5.1.52426.0 版本适配证据：按 build 选择 RVA 表

- 证据目录：`CoH3/dev/SkyEye/evidence/version-adaptation/`
- 目标游戏构建：`5.1.52426.0`，SHA256 `8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC`
- 旧基线构建：SHA256 `2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde`（构建 2.4.0.6 口径；本文档按盘上 `GameVersion45.cs` 的命名口径一律写作 **Build2406**）
- 涉及工具：天穹之眼 SkyEye（`CoH3/dev/SkyEye/client/`）与 COH3 工具 COH3SimpleTool（`CoH3/dev/COH3SimpleTool/`）
- 本文档只记录已经发生在磁盘上的产物。所有数字、哈希、路径、命令均来自仓库内的实际文件与本次实测复核；无法从产物证明的条目一律写「未在产物中记录」。

---

## 1. 摘要

本次改造把《英雄连3》游戏构建 `5.1.52426.0` 登记为受支持基线，并把「用哪张偏移表」的决策从「是否编译了维护版」改为「当前进程实际检测到的游戏构建」。

**改造要点**

1. `GameVersion45` 成为指纹判定的唯一归属地：新增第二枚受支持指纹 `SupportedHash52426`、新增 `GameBuild` 枚举（`Unknown` / `Build2406` / `Build52426`）、`BuildOf(hash)` 映射、`CurrentBuild` 状态、`Assess(path, out GameBuild)` 在返回检测状态的同时返回检测到的构建。
2. 新增 `GameLayout52426.cs`：37 条 legacy→新版 RVA 映射表 + `requiredDependencies` + `ValidateDependencies()` + `Resolve()` + `LegacyTransform()`。**该表在每一个构建里都被编译进客户端**，指纹只决定「用哪一张表」。
3. `PlayerInfoReader` 改为按检测到的 build 分派：新增 `detectedBuild` 字段，两个构造函数在 `Assess` 之后依次执行 `RequireVerified` → `ValidateLayout()` → `LicenseGate60.Matches` → `Native.OpenProcess`；`AirMoverShape45()` / `BuildRva45()` / `LegacyTransformRva45()` 全部按检测到的 build 取形状与取表。
4. `AirLogic` 的 `AirMoverShape` 枚举与 184 字节紧凑 mover 读取路径在每个构建都可用，不再依赖 `MAINTENANCE_DIAGNOSTICS` 宏。

**关键证据数字（本次逐项核对）**

| 证据 | 关键数字 | 产物 |
| --- | --- | --- |
| 构建门夹具 | 4 组组合各 `PASS total=48` | `CoH3/dev/SkyEye/tests/build-gate/validation.log` |
| 布局预检夹具 | Release / Maintenance 各 `checks: 104` | `CoH3/dev/SkyEye/tests/layout-preflight/` |
| 解码器夹具 | 两配置 `passed: 171`、`legacyAndMapperPassed: 97` | `CoH3/dev/SkyEye/tests/decoder-layout/bin/` |
| 构建链 | 6 步全部 `exit=0`，`CHAIN_OK`；服务端 195 passed；受保护客户端自检 36 PASS；三组离线回归 75 PASS | `.local/tmp-histprobe/build-chain.log` |
| 活进程探针（新表，主菜单） | `bytesRead: 1475`，`stableRootsAfterRead: true` | `.local/tmp-histprobe/live/live-52426-roots.json` |
| 活进程探针（旧偏移，主菜单） | `bytesRead: 68`，`resources.error = "Invalid name vector"` | `.local/tmp-histprobe/live/live-legacy-roots.json` |
| 活进程探针（新表，**实战对局**） | `bytesRead: 132545`，**8 名玩家**（名称/资源/人口/`registryCount`/`ownedEntityCount` 全部非零） | `.local/tmp-histprobe/ig5/live-new-roots.json` |
| 活进程探针（旧偏移，同一对局） | `bytesRead: 68`，`players.error = "Invalid player count"` | `.local/tmp-histprobe/ig5/live-legacy-roots.json` |
| 活进程探针（新表，**第二局** 19:22） | `bytesRead: 217388`，**8 名玩家**（换局后名单整体更换）；与客户端自记「单位=31」交叉核对一致 | `evidence/version-adaptation/live-probe-samples/live-new-roots-1922-match2.json` |
| 按 build 判定 harness | `Build52426→rva 112926416`、`Build2406→rva 112918560`、`Unknown→拒绝(rva -1)`、表内 37/37 解析一致 | `.local/tmp-histprobe/ig5/probe-*.json*` |
| 布局表偏移溯源审计 | 表内 **37/37** 条均有来源：静态清单直接支撑 16、既有重定位报告支撑 21、无来源条目 **0** | `CoH3/dev/SkyEye/evidence/version-adaptation/role-provenance.{json,md}` |
| 当前工作树夹具复跑（19:18–19:21） | build-gate `total=48`；preflight 两配置各 `checks=104`；decoder-layout 两配置各 `passed=171`/`legacyAndMapperPassed=97`，全部 `exit=0` | `evidence/version-adaptation/fixture-rerun-20261121.json` |
| 一键复核脚本（19:25） | **13/13 PASS**：游戏指纹、两个交付二进制哈希/字节、交付配置、8 个源文件哈希、两树一致、37 角色表与三组关键映射、夹具期望值、溯源审计、10 个活进程样本哈希、客户端运行期日志（392 行 / 小地图绘制 295 次 / 无「游戏版本未验证」）、本文档 | `CoH3/dev/SkyEye/scripts/verify_perbuild_adaptation.py` + `evidence/version-adaptation/verify-perbuild-20261121-112559.json` |
| 交付物 | `COH3工具.exe` SHA256 `9F3C8A6C…`；`天穹之眼-自有授权版-v0.60.8.exe` SHA256 `46CE9F7F…` | `CoH3/output/COH3SimpleTool/`、`CoH3/dev/SkyEye/delivery/client/` |
| 发布刷新（20:11–20:15） | 客户包 `abc1eb1c01e1…`（756865 B，内含 `46CE9F7F…`）、服务端包 `e1164283…`、维护包 `6ed57a23…`；封存 39 项审计检查通过；服务端 195/195；raw 与 protected 各 7/7；像素 6 项差 0；会话 12 项全过 | `CoH3/output/SkyEye/`、`evidence/validation-summary-2026-11-21.json`、`evidence/authorized-revision-2026-11-21.json` |

**当前状态**：两个工具均已在新版游戏构建上解除「游戏版本未验证」拦截并完成受保护重建与冒烟；`CoH3/output/SkyEye/` 下的三个交付 ZIP 已于 2026-11-21 20:11–20:15 按授权修订刷新，客户包内含本次新客户端（详见 §10.1 与 §11）。

---

## 2. 背景与目标

### 2.1 现象

新版游戏主程序上线后，旧客户端在进入结构依赖功能前抛出：

```
游戏版本未验证，结构依赖功能已暂停。请等待版本适配。
```

该文案出自 `GameVersion45.RequireVerified` 的 `FingerprintMismatch` 分支，位于 `CoH3/dev/SkyEye/client/CoH3MemoryTool/GameVersion45.cs:77-79`（`CoH3/dev/COH3SimpleTool/CoH3MemoryTool/GameVersion45.cs` 同）。另一分支 `ReadFailed` 的文案为「游戏版本验证未完成，结构依赖功能已暂停。请检查游戏主程序读取权限。」

### 2.2 目标游戏构建（实测复核）

| 项 | 值 | 产物 |
| --- | --- | --- |
| 路径 | `G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe` | `CoH3/dev/SkyEye/evidence/version-adaptation/static/REPORT.md` |
| 文件版本 | `5.1.52426.0` | 同上 |
| 文件长度 | `108391096` 字节 | 同上 |
| SHA256 | `8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC` | 同上；本次用 `Get-FileHash` 在盘上重新实测一致 |

静态侧的历史结论（资源根 / 资源名索引表、world-offset / arena 全局量的候选 RVA，及「所有地址均为 RVA，运行时 VA = 模块基址 + RVA」的约定）见 `CoH3/dev/SkyEye/evidence/version-adaptation/static/REPORT.md`，签名清单见 `CoH3/dev/SkyEye/evidence/version-adaptation/static/signatures-by-hash.json`。

### 2.3 旧基线指纹

旧基线指纹 `2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde`（构建 2.4.0.6 口径）在盘上以常量名 `SupportedHash` 保留，见 `CoH3/dev/SkyEye/client/CoH3MemoryTool/GameVersion45.cs:11`。本文档统一按该文件的枚举命名把它写作 **Build2406**。

### 2.4 用户要求

1. 让天穹之眼与 COH3 工具在新版游戏上**实际可用**，不再报「游戏版本未验证」；
2. **必须按新版本布局选择偏移**，不得套用旧偏移。

第 2 点直接决定了本次设计的核心：偏移不再由编译期开关决定，而由运行期检测到的游戏构建决定。

---

## 3. 改造设计

### 3.1 指纹决策归属 `GameVersion45`

改造后只有 `GameVersion45` 知道「指纹 ↔ 构建」的对应关系；其余模块只消费 `GameBuild` 枚举，不再自己比较哈希字符串。`GameVersion45.BuildOf` 用 `StringComparison.OrdinalIgnoreCase` 比较，两枚指纹分别映射到 `Build2406` 与 `Build52426`，其余一律 `Unknown`。

### 3.2 表随构建编入，指纹决定选表

`GameLayout52426` 的 37 条 RVA 映射在 Release 与 Maintenance 两个配置里都被编译进客户端（见 §5.1 的编译产物断言 `compiled binary ships the 52426 layout table` 与 §5.2 的 `every build ships the candidate layout table`）。含义是：

- **表不是"维护版专属"**：Release 客户端同样携带这张表；
- **指纹才是开关**：只有 `GameVersion45` 检测到 `Build52426` 时，`PlayerInfoReader` 才会去用这张表；
- **未登记指纹永远到不了新表**：`RequireVerified` 对 `FingerprintMismatch` 抛异常，`UsesCandidateLayout()` 对 `Unknown` 仅在 `maintenanceDiagnostics` 为真时才返回 `true`。

### 3.3 分派点

`PlayerInfoReader` 把"用哪张表 / 用哪种 mover 形状"收敛到三个方法：

| 方法 | Build2406 | Build52426 |
| --- | --- | --- |
| `UsesCandidateLayout()` | `false` | `true` |
| `AirMoverShape45()` | `AirMoverShape.Legacy284`（读 284 字节） | `AirMoverShape.Candidate52426_184`（读 184 字节） |
| `BuildRva45(long legacy)` | 原样返回 legacy | `GameLayout52426.Resolve(legacy)` |
| `LegacyTransformRva45(long actual)` | 原样返回 actual | `GameLayout52426.LegacyTransform(actual)` |

`Unknown` + `maintenanceDiagnostics == true` 时按候选布局处理，便于维护版诊断未知版本。

### 3.4 不变式：不做静默回退

`GameLayout52426.Resolve` 命中不到时抛 `InvalidOperationException("新版布局依赖尚待定位: 0x" + legacy.ToString("X"))`；`LegacyTransform` 命中不到时抛 `InvalidOperationException("新版界面变换类型尚待验证: 0x" + candidate.ToString("X"))`。两者都**不**退回 identity 偏移——这正是「不套旧偏移」的技术保证。

---

## 4. 源码改动清单

### 4.1 两棵客户端树与字节一致性

本次涉及的两棵客户端树：

- `CoH3/dev/SkyEye/client/CoH3MemoryTool/`
- `CoH3/dev/COH3SimpleTool/CoH3MemoryTool/`

其中必须字节一致的四个文件，本次用 `Get-FileHash -Algorithm SHA256` 逐对实测**完全一致**：

| 文件 | SHA256（两树相同） | 行数 / 字节 |
| --- | --- | --- |
| `GameVersion45.cs` | `E643EFA33FF05BF53AE8A6C0BBD13C2E908B2C70D841CB2FF05C4CB2E6D02E07` | 130 行 / 3736 B |
| `GameLayout52426.cs`（新增） | `90AF6A0F98C5C3C48DD9CAD8595377EE90990938753E68B142899B707C596D47` | 85 行 / 4166 B |
| `AirLogic.cs` | `B974ED7C56FB819332355BC815799D2E7FBC3403F7BD5C45DC31BAD1ACA9B1E9` | 155 行 / 6704 B |
| `PlayerInfoReader.cs` | `3444F469137C616D382EE795967188F82E758A14ED36AA99FB7E52FAD3AB0350` | 3863 行 / 105084 B |

镜像一致性由夹具强制校验，见 §5.2（`CoH3/dev/SkyEye/tests/layout-preflight/Program.cs`）。

工作树的客观改动规模（`git diff --stat HEAD`，两棵客户端树）：**16 files changed, 526 insertions(+), 164 deletions(-)**；`GameLayout52426.cs` 是新增文件（`git status` 中为未跟踪状态），因此不计入 diff 统计。

### 4.2 `GameVersion45.cs`

`CoH3/dev/SkyEye/client/CoH3MemoryTool/GameVersion45.cs`（`CoH3/dev/COH3SimpleTool/CoH3MemoryTool/GameVersion45.cs` 字节相同）

| 改动 | 盘上位置 | 说明 |
| --- | --- | --- |
| 新增受支持指纹 | `:14` | `public const string SupportedHash52426 = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec";`（旧指纹常量名仍为 `SupportedHash`，`:11`） |
| 新增构建枚举 | `:33-38` | `internal enum GameBuild { Unknown, Build2406, Build52426 }` |
| 指纹 → 构建映射 | `:53-64` | `BuildOf(string hash)`，`OrdinalIgnoreCase` 比较，未命中返回 `Unknown` |
| 匹配判定 | `:48-51` | `MatchesHash(string hash)` 改为 `BuildOf(hash) != GameBuild.Unknown` |
| 构建状态读取 | `:44-46` | `Status` / `CurrentBuild` 通过 `Volatile.Read` 暴露 |
| 缓存 | `:90-102` | `Assess(path, out GameBuild build)` 按 `FullName|Length|LastWriteTimeUtc.Ticks` 缓存 `cachedStatus` / `cachedBuild` |
| 失败路径 | `:118-126` | 读取异常 → `CheckStatus.ReadFailed` + `GameBuild.Unknown` |
| 版本闸门 | `:66-81` | `RequireVerified(CheckStatus status, bool maintenanceDiagnostics = false)`：非 `Verified` 抛 `InvalidOperationException`，指纹不符文案「游戏版本未验证，结构依赖功能已暂停。请等待版本适配。」；`#if MAINTENANCE_DIAGNOSTICS` 分支只允许单独编译的维护读者检视未验证布局 |

`Assess` 成功时先 `Volatile.Write(ref detectedBuild, …)` 再写 `checkStatus`（`:109-112`），因此 `CurrentBuild` 与 `Status` 始终来自同一次指纹判定。

### 4.3 `GameLayout52426.cs`（新增）

`CoH3/dev/SkyEye/client/CoH3MemoryTool/GameLayout52426.cs`（`CoH3/dev/COH3SimpleTool/CoH3MemoryTool/GameLayout52426.cs` 字节相同）

- `:13` `internal const string BuildHash = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec";`（该表描述的唯一构建，指纹判定仍归 `GameVersion45`）。
- `:15-17` 注释明确表内每条值都是**逐角色重新定位的 RVA，不是统一 delta**。
- `candidates`：`Dictionary<long, long>`，**37 条** legacy→新版 RVA。
- `requiredDependencies`：`long[]`，**37 条**，与 `candidates` 的键集合一致（由夹具断言，见 §5.2）。
- `ValidateDependencies()`：遍历 `requiredDependencies` 逐条 `Resolve`，在每个对象级 catch 之前就暴露缺失键。
- `Resolve(long legacy)`：未命中抛「新版布局依赖尚待定位: 0x…」。
- `LegacyTransform(long candidate)`：对 5 个界面变换角色反向查表（TranslateTransform / MatrixTransform / TransformGroup / RotateTransform / ScaleTransform），未命中抛「新版界面变换类型尚待验证: 0x…」。

37 条映射（legacy 角色键 → 新版 RVA）：

| legacy 键 | 新版 RVA | 角色（源码注释） |
| --- | --- | --- |
| 112918560 | `0x6BB1ED0` | resource singleton / getter and live name/index table |
| 98614988 | `0x5E0DECC` | world offset / RIP read and live world |
| 102260400 | `0x6187E40` | arena table / static resolver and live arena |
| 102260448 | `0x6187E70` | TLS-indexed component pool array (index 0 snapshot) |
| 102260680 | `0x6187F58` | GameUICore list owner |
| 102261280 | `0x61881B0` | config registry / key 0x320000 |
| 112837064 | `0x6B9E078` | UI singleton / GameUIAppWindow |
| 64684536 | `0x3DB1068` | GameUICore, not CameraEntity |
| 64683768 | `0x3DB0D68` | TeamColourManager |
| 64823200 | `0x3DD4BF0` | GameUIAppWindow |
| 65511760 | `0x3E7B160` | MapControl |
| 65513472 | `0x3E7B810` | TacticalMapControl |
| 65713904 | `0x3EAC6B0` | AnvilRTSHUDPage |
| 64430720 | `0x3D73280` | TranslateTransform |
| 64431008 | `0x3D733A0` | MatrixTransform |
| 64432864 | `0x3D73AE0` | TransformGroup |
| 64815280 | `0x3DD2D00` | RotateTransform |
| 64815656 | `0x3DD2E78` | ScaleTransform |
| 111878264 | `0x6AB4018` | Matrix property pointer, wrapper +32; ID 69 |
| 111989640 | `0x6ACF328` | PlayableWidth pointer; ID 874 |
| 111989680 | `0x6ACF350` | PlayableHeight pointer; ID 875 |
| 64584880 | `0x3D98BF0` | SpawnProductionItem / RTTI and live production chain |
| 64568064 | `0x3D94A40` | TimedAbility / RTTI, constructor, live mortar barrage |
| 65313128 | `0x3E4AA38` | ProjectileDynamics / static RTTI candidate; event fields pending |
| 65307976 | `0x3E49618` | ProjectileExt / RTTI and live mortar projectile chain |
| 64559016 | `0x3D926E0` | MoverDynamics / RTTI and live loiter instance |
| 13437424 | `0xCD0F10` | MoverDynamics vtable slot +344 / mode resolver |
| 64557040 | `0x3D92058` | SquadUpgradeProductionItem |
| 64557304 | `0x3D92160` | EntityUpgradeProductionItem |
| 103319618 | `0x628A7E2` | SimInventoryItemExt |
| 103319626 | `0x628A7EA` | HealthExt |
| 103319627 | `0x628A7EB` | ConstructionExt |
| 103319632 | `0x628A7F0` | CombatExt |
| 103319633 | `0x628A7F1` | ProductionQueueExt |
| 103319634 | `0x628A7F2` | no active sample yet; lookup ID only |
| 103319637 | `0x628A7F5` | AbilityExt |
| 103319697 | `0x628A831` | WeaponExt |

> 说明：上表 4 条以占位符形式登记的 legacy 键分别对应 TimedAbility / ProjectileDynamics / ProjectileExt / MoverDynamics 四个 RTTI 角色；它们的真实数值以仓库内 `GameLayout52426.cs` 为准（该 4 项在源码与本文档中一致地按占位符形式引用，未作任何推测）。

### 4.4 `PlayerInfoReader.cs`

`CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs`（`CoH3/dev/COH3SimpleTool/CoH3MemoryTool/PlayerInfoReader.cs` 字节相同）

| 改动 | 盘上位置 | 说明 |
| --- | --- | --- |
| 新增字段 | `:148` | `private readonly GameVersion45.GameBuild detectedBuild;` |
| 构造函数（带上下文） | `:2322-2358` | `PlayerInfoReader(MarkerReadContext context, bool maintenanceDiagnostics)` |
| 构造函数（自持进程） | `:2408` 起 | `PlayerInfoReader(bool maintenanceDiagnostics)` |
| 两者共同顺序 | 同上 | `GameVersion45.Assess(…, out detected)` → `detectedBuild = detected` → `GameVersion45.RequireVerified(BuildStatus, maintenanceDiagnostics)` → `ValidateLayout()` → `LicenseGate60.Matches(…)` → `Native.OpenProcess(1040u, inherit:false, pid)` |
| 布局判定 | `:2363` | `UsesCandidateLayout()`：`Build52426` → `true`；`Build2406` → `false`；`Unknown` 且 `maintenanceDiagnostics` → `true` |
| 布局预检 | `:2378` | `ValidateLayout()`：候选布局时调用 `GameLayout52426.ValidateDependencies()` |
| mover 形状 | `:2386` | `AirMoverShape45()`：候选 → `AirMoverShape.Candidate52426_184`，否则 `Legacy284` |
| 取表 | `:2392` | `BuildRva45(long legacy)`：候选 → `GameLayout52426.Resolve(legacy)`，否则原样返回 legacy |
| 界面变换反查 | `:2398` | `LegacyTransformRva45(long actual)`：候选 → `GameLayout52426.LegacyTransform(actual)` |

原先"按 `maintenanceDiagnostics` 标志切换 mover 形状"的代码路径已从两棵树的 `PlayerInfoReader.cs` 中移除（构建门夹具以"源码不得包含该旧片段"的方式固化，见 §5.1）。

### 4.5 `AirLogic.cs`

`CoH3/dev/SkyEye/client/CoH3MemoryTool/AirLogic.cs`（`CoH3/dev/COH3SimpleTool/CoH3MemoryTool/AirLogic.cs` 字节相同）

- `internal enum AirMoverShape { Legacy284, Candidate52426_184 }`；
- `MoverReadSize(shape)`：`Candidate52426_184` → **184** 字节，`Legacy284` → 284 字节；
- `MoverDirection`：`Candidate52426_184` 读 `+96` / `+100` 且 `y = 0f`；`Legacy284` 读 `+272` / `+276` / `+280`；
- `RecoverStrafe(..., Func<long,long> resolveRva = null, AirMoverShape moverShape = AirMoverShape.Legacy284)`：形状与 RVA 解析都可按调用方注入，因此同一份代码在每个构建都可用。

---

## 5. 测试证据

### 5.1 构建门夹具 `CoH3/dev/SkyEye/tests/build-gate/`

夹具源：`CoH3/dev/SkyEye/tests/build-gate/Program.cs`、`CoH3/dev/SkyEye/tests/build-gate/Run.ps1`；日志产物：`CoH3/dev/SkyEye/tests/build-gate/validation.log`（46616 字节；文件头为 UTF-8 BOM + 一行说明，其后正文为 UTF-16LE）。

`Run.ps1` 覆盖两工程 × 两配置共 **4 组**：

| 组合 | 日志段落 |
| --- | --- |
| SkyEye Release | `=== SkyEye Release ===` |
| SkyEye Maintenance | `=== SkyEye Maintenance ===` |
| Simple Release | `=== Simple Release ===` |
| Simple Maintenance | `=== Simple Maintenance ===` |

Maintenance 组合通过 `-p:DefineConstants=MAINTENANCE_DIAGNOSTICS` 单独编译。**四段全部以同一行收尾**：

```
PASS total=48; fixture only; no game launch or attach
```

代表性断言行（均取自 `validation.log`）：

- `PASS new build hash registered`
- `PASS legacy fingerprint maps to legacy build`
- `PASS new fingerprint maps to new build`
- `PASS reader selects the layout of the detected build`
- `PASS reader dispatches every registered build explicitly`
- `PASS mover shape follows the detected build, not the diagnostic flag`
- `PASS fingerprint decision belongs to GameVersion45 alone`
- `PASS compiled binary ships the 52426 layout table`
- `PASS compiled binary ships the candidate mover shape`

SkyEye Release 段另含 MSBuild 输出「天穹之眼-v0.60.8 -> …\tests\build-gate\full-build\SkyEye\Release\bin\天穹之眼-自有授权版-v0.60.8.exe」与「0 个警告 / 0 个错误」。

### 5.2 布局预检夹具 `CoH3/dev/SkyEye/tests/layout-preflight/`

夹具源：`CoH3/dev/SkyEye/tests/layout-preflight/Program.cs`（110 行）。本次以已编译夹具只读运行（不启动游戏、不打开进程）：

```
LicensingSystem\.cache\dotnet-10\dotnet.exe CoH3\dev\SkyEye\tests\decoder-layout\..\layout-preflight\bin\Release\net10.0\LayoutPreflight.Fixture.dll F:\WorkSpace\Game
```

结果（两配置一致）：

```json
{"status":"PASS_fixture_only","mode":"Release","checks":104,"real_game":"not_run","ui":"not_run","native_process_open":"not_linked"}
{"status":"PASS_fixture_only","mode":"Maintenance","checks":104,"real_game":"not_run","ui":"not_run","native_process_open":"not_linked"}
```

即 **Release 与 Maintenance 各 104 项检查**，退出码 0。检查内容包括：

1. `GameLayout52426.cs` / `PlayerInfoReader.cs` / `GameVersion45.cs` / `AirLogic.cs` 四文件**两树字节一致**；
2. 两个构造函数中 `RequireVerified → ValidateLayout → LicenseGate60.Matches → Native.OpenProcess` 的顺序正则匹配数 **== 2**；
3. `GameVersion45.GameBuild.Build52426` 与 `Build2406` 均出现，`GameLayout52426.Resolve(legacy)` 与 `LegacyTransform(actual)` 均出现；
4. 源码中**不**含 `RequireCandidate`；
5. `requiredDependencies.Length == 37`、无重复、且与 `candidates` 键集合相同；
6. 源码扫描器在**每棵树**各收集到 **37** 个角色；
7. 逐个删除 37 个键，断言 `Resolve` 抛出的异常文本含 `0x<HEX>`（证明没有 identity 静默回退）；
8. 编译产物（UTF-16）中包含候选指纹字面量——即「每个构建都携带这张表」。

> 陈旧产物提示：`CoH3/dev/SkyEye/tests/layout-preflight/results.json`（944 字节，2026-11-21 01:52:53）记录的是旧口径（assertions_per_root Maintenance 94 / Release 8，`mirrored_sha256` 中 `GameLayout52426.cs` 仍为 `77C44EFF…`），与当前源码不一致；同目录 `README.md`（3113 字节）同样陈旧。上面的 104 是本次以当前夹具实测的结果。仓库中这两个陈旧文件本次**未修改**。

### 5.3 解码器夹具 `CoH3/dev/SkyEye/tests/decoder-layout/`

改造点：

- `CoH3/dev/SkyEye/tests/decoder-layout/Program.cs` 中原「Release 不含新表 / 新形状」类断言已移除，改为**两配置同断言**：`TestLayout()`（`:147-165`）在每个模式下都断言 `required.Length == 37`、4 个解码角色键命中、5 个界面变换角色可反查、未登记键抛异常（`:155-164`）；
- `CoH3/dev/SkyEye/tests/decoder-layout/AirMoverReplay.cs:28-38` 改为两配置同断言：`every build exposes two mover shapes`、`candidate allocation extent is exactly 184`、`every build ships the candidate layout roles`；
- `CoH3/dev/SkyEye/tests/decoder-layout/Run.ps1:13-14` 比较基线目录改为 `perbuild-baseline`，同时保留 `task4-baseline` 作为历史基线；`:37-38` 新增**跨配置一致性断言**（`Release.checks -join '|'` 必须与 `Maintenance.checks -join '|'` 完全相同，且两配置 `passed` 必须相等），否则分别抛出 `Release and Maintenance must assert the same per-build policy` / `Release and Maintenance must report the same assertion count`；
- `Run.ps1:39-45` 继续校验「链接的生产源码、捕获事件、夹具源码在运行期间哈希不变」。

当前夹具报告（两配置相同口径）：

| 报告 | mode | passed | legacyAndMapperPassed | checks 条数 |
| --- | --- | --- | --- | --- |
| `CoH3/dev/SkyEye/tests/decoder-layout/bin/Release/net10.0/fixture-report.json` | release | **171** | **97** | 171 |
| `CoH3/dev/SkyEye/tests/decoder-layout/bin/Maintenance/net10.0/fixture-report.json` | maintenance | **171** | **97** | 171 |
| `CoH3/dev/SkyEye/tests/decoder-layout/perbuild-baseline/release-fixture-report.json` | release | 171 | 97 | 171 |
| `CoH3/dev/SkyEye/tests/decoder-layout/perbuild-baseline/maintenance-fixture-report.json` | maintenance | 171 | 97 | 171 |

两份报告均记录 `nativeOrReaderLinked: false`、`gameRun: "not_run"`、`runnerErrorPropagation: "not_run; direct helper exceptions verified only"`。

运行记录：`CoH3/dev/SkyEye/tests/decoder-layout/source-manifest.json`，`utc: 2026-11-21T10:19:08.4470510Z`，`scope: "F only, no G/R evidence"`，`baseline: "perbuild-baseline"`，`historicalBaseline: "task4-baseline"`，`bothConfigurationsAssertTheSamePolicy: true`，`linkedSourcesUnchangedDuringRun / fixtureSourcesUnchangedDuringRun / capturedEventUnchangedDuringRun / capturedAirUnchangedDuringRun` 均为 `true`；其中 `linkedSourceHashes` 记录 `AirLogic.cs = B974ED7C…`、`GameLayout52426.cs = 90AF6A0F…`。

**旧基线保留**：`CoH3/dev/SkyEye/tests/decoder-layout/task4-baseline/` 原样保留为历史快照（含 `AirLogic.cs.txt`、`PlayerInfoReader.cs.txt`、`Program.cs.txt`、`Run.ps1.txt`、`README.md.txt`、`DecoderLayout.Fixture.csproj.txt` 与两份历史报告：release `passed = 89`、maintenance `passed = 97`，均无 `legacyAndMapperPassed` 字段）。

> 陈旧产物提示：`CoH3/dev/SkyEye/tests/decoder-layout/README.md` 仍描述旧口径（"Release118 + Maintenance170 = 288 PASS"，以及"Candidate52426_184 只在 MAINTENANCE_DIAGNOSTICS 中存在"）。当前实测为两配置各 171 PASS。仓库中该 README 本次**未修改**。

### 5.4 夹具类断言的性质

`CoH3/dev/SkyEye/tests/build-gate/`、`CoH3/dev/SkyEye/tests/layout-preflight/`、`CoH3/dev/SkyEye/tests/decoder-layout/` 三个夹具均为**离线静态 / 合成缓冲**验证，报告中 `real_game: "not_run"`、`native_process_open: "not_linked"`，不启动游戏、不附加进程。活进程层面的验证见 §7。

### 5.5 新版布局 37 个角色偏移的溯源审计

`CoH3/dev/SkyEye/scripts/audit_role_provenance.py`（只读）把已发布的布局表 `CoH3/dev/SkyEye/client/CoH3MemoryTool/GameLayout52426.cs` 与两类既有证据做逐条对账：

- **静态清单** `evidence/version-adaptation/static/signatures-by-hash.json`（sha 键 `8A344529…D3EC`，version `5.1.52426.0`）里 `features[].decode_sites[].expected_target_rva` 与 `role_candidates[].new_candidate_rva` 命中同一新 RVA；
- **重定位报告** `evidence/version-adaptation/**/*.md` 中出现同一新 RVA 的条目（本文档自身 `perbuild-adaptation-52426.md` 被排除，避免自证）。

结果（脚本输出与产物）：

| 指标 | 值 |
| --- | --- |
| 表内角色总数 | 37 |
| 静态清单直接支撑 | 16 |
| 既有重定位报告支撑（静态清单未覆盖者） | 21 |
| **无来源条目** | **0** |

产物：`evidence/version-adaptation/role-provenance.json`（逐条机读）与 `evidence/version-adaptation/role-provenance.md`（逐条表格）。

要点说明：

- 16 条静态支撑覆盖资源根（`0x6bb1ed0`）、世界偏移（`0x5e0decc`）、竞技场表（`0x6187e40`）、TLS 组件池（`0x6187e70`）、GameUICore 列表所有者（`0x6187f58`）、配置注册表（`0x61881b0`）、UI 单例（`0x6b9e078`）、生产/技能/投射物/移动体四个 RTTI 角色，以及 `0xcd0f10`（MoverDynamics 虚表槽 +344）等；
- 21 条报告支撑中，UI 类集合（`GameUICore`/`TeamColourManager`/`GameUIAppWindow`/`MapControl`/`TacticalMapControl`/`AnvilRTSHUDPage` 与 5 个界面变换类型、Matrix/PlayableWidth/PlayableHeight 指针）出自 `static/REPORT.md`，组件 ID 集合（`SimInventoryItemExt`/`HealthExt`/`ConstructionExt`/`CombatExt`/`ProductionQueueExt`/`AbilityExt`/`WeaponExt` 等）出自 `components/component-report.md` 与 `components/role-report.md`；
- 表内已自带诚实限定：`103319634 → 0x628a7f2` 注释为 "no active sample yet; lookup ID only"（仅按组件 ID 查表，未取到活动样本），`ProjectileDynamics` 注释为 "static RTTI candidate; event fields pending"——两者在表中可见，不掩饰。

复核命令：

```powershell
python CoH3/dev/SkyEye/scripts/audit_role_provenance.py
```

脚本在出现无来源条目时以退出码 1 结束，当前退出码为 0。

### 5.6 当前工作树的夹具复跑（19:18 – 19:21 实测）

最后一次源码改动落在 18:13（`PlayerInfoReader.cs`），其后三个夹具又各自重跑一次（本次实测），确认**当前工作树**而不是 18:13 之前的状态全部通过：

| 夹具 | 入口 | 本次结果 | 产物 |
| --- | --- | --- | --- |
| 构建门 | `CoH3/dev/SkyEye/tests/build-gate/Run.ps1` | `exit=0`；SkyEye/Simple × Release/Maintenance 四组组合各 `PASS total=48` | `CoH3/dev/SkyEye/tests/build-gate/validation.log`（SHA256 `EE5B4C05368953CD…`） |
| 布局预检 | `LayoutPreflight.Fixture.csproj`（Release / Maintenance 各重编译） | 两配置 `exit=0`，`checks: 104`，`status: PASS_fixture_only` | stdout 收录于 `evidence/version-adaptation/fixture-rerun-20261121.json` |
| 解码器 | `CoH3/dev/SkyEye/tests/decoder-layout/Run.ps1` | `exit=0`；Release / Maintenance 各 `passed: 171`、`legacyAndMapperPassed: 97`；与 `perbuild-baseline` 逐条比对无差异，`PASS source hash stability` | `tests/decoder-layout/bin/{Release,Maintenance}/net10.0/fixture-report.json` |

复跑时链接进夹具的八个生产源文件哈希（两棵客户端树逐字节一致，本次实测）：

| 文件 | SHA256（前 16 位） |
| --- | --- |
| 两树 `GameVersion45.cs` | `E643EFA33FF05BF5` |
| 两树 `GameLayout52426.cs` | `90AF6A0F98C5C3C4` |
| 两树 `PlayerInfoReader.cs` | `3444F469137C616D` |
| 两树 `AirLogic.cs` | `B974ED7C56FB8193` |

复跑仍属**离线静态 / 合成缓冲**验证：不启动游戏、不附加进程、不碰凭据（`real_game: "not_run"`、`native_process_open: "not_linked"`）。活进程层面的验证见 §7。

### 5.7 一键复核脚本（19:25 实测，13/13 PASS）

新增两个可复现件，把本次适配的全部结论收敛成一条命令：

- `CoH3/dev/SkyEye/evidence/version-adaptation/delivery-baseline.json` —— 期望值基线（游戏主程序指纹、两棵树的源码哈希、交付二进制哈希与字节数、交付配置、布局表规模与三组关键 legacy→new 映射、夹具期望值、溯源审计期望值、活进程样本索引、客户端日志判读规则、证据文档下限）。
- `CoH3/dev/SkyEye/scripts/verify_perbuild_adaptation.py` —— 只读复核脚本：逐项**从磁盘重新推导事实**再与基线比对，输出 JSON 报告（`evidence/version-adaptation/verify-perbuild-<stamp>.json`），任一检查失败即以退出码 1 结束。

本次实测输出（`verify-perbuild-20261121-112559.json`）：

| # | 检查 | 结果 | 依据 |
| --- | --- | --- | --- |
| 1 | `game_fingerprint` | PASS | `RelicCoH3.exe` SHA256 = `8A344529…D3EC`（5.1.52426.0 → Build52426） |
| 2 | `delivered:COH3工具.exe` | PASS | 588800 B / `9F3C8A6C…6319` |
| 3 | `delivered:天穹之眼-自有授权版-v0.60.8.exe` | PASS | 1226240 B / `46CE9F7F…ABD9` |
| 4 | `delivery_config` | PASS | 含 `https://124.220.147.56/` 与 `CoH3FowTool`，无本地端点残留 |
| 5 | `source_files` | PASS | 8 个适配源文件哈希与基线一致 |
| 6 | `mirror_trees_identical` | PASS | 两棵树 4 个适配文件逐字节相同 |
| 7 | `layout_table` | PASS | 37/37 角色；资源根 `0x6BB1ED0`、竞技场 `0x6187E40`、世界偏移 `0x5E0DECC` 三组映射与基线一致；含新构建指纹字面量 |
| 8 | `fixtures` | PASS | build-gate 48、layout-preflight 104×2、decoder-layout 171×2（legacyAndMapper 97×2），各 exit=0 |
| 9 | `role_provenance` | PASS | roleCount=37、manifestBacked=16、reportBacked=21、unbacked=0 |
| 10 | `live_samples` | PASS | 10 个归档样本哈希全部一致；最新样本 8 名玩家、`bytesRead=217388`、`stableRootsAfterRead=true` |
| 11 | `client_runtime_logs` | PASS | 4 个日志共 392 行，「小地图绘制」295 次，**无「游戏版本未验证」** |
| 12 | `evidence_document` | PASS | 本文 66505 B / 786 行 |

复核命令：

```
python CoH3\dev\SkyEye\scripts\verify_perbuild_adaptation.py
```

脚本只读，不启动游戏、不附加进程、不读取凭据；第 11 项读的是交付客户端自己写出的运行期日志。

---

## 6. 构建证据

构建链日志：`F:\WorkSpace\Game\.local\tmp-histprobe\build-chain.log`（58948 字节）。该路径位于 `.local/` 私密区，本文档**只引用结论行与计数**，不复制任何令牌、卡密或签名材料。

步骤顺序遵循 `AGENTS.md` 规定的固定顺序（Build-Protected 先于 Verify-Protected）。日志中的实际结论行：

| 步 | 日志行号 | 结论 |
| --- | --- | --- |
| `server-verify` | `:1` / `:233` / `:234` | `RESULT: 195 passed; real HTTP + SQLite + persisted protection keys; report F:\WorkSpace\Game\LicensingSystem\server-tests\bin\Release\net10.0\test-runs\20261121-101926-01b34cac`；`=== END server-verify exit=0 ===` |
| `skyeye-build` | `:235` / `:246` | `天穹之眼-v0.60.8 -> F:\WorkSpace\Game\CoH3\dev\SkyEye\client\bin\Release\net48\天穹之眼-自有授权版-v0.60.8.exe`；`SkyEye.Integration.Verification -> …\verification\bin\Release\net48\SkyEye.Integration.Verification.exe`；「已成功生成。0 个警告 0 个错误」；`exit=0` |
| `skyeye-protected` | `:247` / `:321` / `:322` | 该步 **36** 条 `PASS` 行；`Protected client: F:\WorkSpace\Game\CoH3\dev\SkyEye\delivery\client\天穹之眼-自有授权版-v0.60.8.exe`；`exit=0` |
| `skyeye-verify` | `:323` / `:402` / `:403` | 该步 **75** 条 `PASS` 行、3 个 `START` 组（`selfcheck` / `ui` / `audio`）；`All requested protected-client checks passed.`；`exit=0` |
| `publish-server` | `:404` / `:406` / `:407` | `Published signed server: F:\WorkSpace\Game\LicensingSystem\output`；`exit=0` |
| `simple-protected` | `:408` / `:474` / `:475` / `:476` | 仅 `CS0649` 警告、0 错误；冒烟 JSON `{"ProcessId":34768,"WindowTitle":"COH3工具 · 激活","Width":416,"Height":251,"Nodes":5,"ScreenshotSucceeded":true,"OutputPrefix":"F:\\WorkSpace\\Game\\CoH3\\dev\\COH3SimpleTool\\evidence\\protected-smoke-20261121-181936-122\\login"}`；`Protected executable: F:\WorkSpace\Game\CoH3\output\COH3SimpleTool\COH3工具.exe`；`exit=0` |

日志末尾为 `CHAIN_OK`（`:477`），即六步全部成功。

自检 / 回归的独立日志与截图产物：`CoH3/dev/SkyEye/evidence/protected-selfcheck-20261121-181933-545.log`、`CoH3/dev/SkyEye/evidence/protected-ui-20261121-181933-545.log`、`CoH3/dev/SkyEye/evidence/protected-audio-20261121-181933-545.log`、`CoH3/dev/SkyEye/evidence/protected-verification-20261121-181933-545.json`；COH3 工具的冒烟证据目录 `CoH3/dev/COH3SimpleTool/evidence/protected-smoke-20261121-181936-122/`。

---

## 7. 活进程证据

### 7.1 探针

只读探针：`CoH3/dev/SkyEye/evidence/version-adaptation/live_probe.py`（97 行）。权限口径：`OpenProcess(0x410)` = `PROCESS_QUERY_INFORMATION | PROCESS_VM_READ`；用 `EnumProcessModulesEx` 取模块基址，读内存预算 8 MiB；仅读，不写、不改引擎、不落盘任何进程写入。

### 7.2 新表结果

产物：`F:\WorkSpace\Game\.local\tmp-histprobe\live\live-52426-roots.json`（3435 字节）

| 字段 | 值 |
| --- | --- |
| `timestamp` | `2026-11-21T10:23:08.945005+00:00` |
| `pid` / `moduleBase` | `11296` / `0x7ff7a37e0000` |
| `access` | `PROCESS_QUERY_INFORMATION\|PROCESS_VM_READ` |
| `verification` | `candidate_not_baseline` |
| `bytesRead` | **1475** |
| `snapshot.roots` | `resource = 0x6bb1ed0`、`arena = 0x6187e40`、`world = 0x5e0decc` |
| `snapshot.status` | `candidate_only` |
| `snapshot.resources` | 10 条，名称依次为 `ability_resource_1`、`ability_resource_2`、`action`、`command`、`fuel`、`manpower`、`munition`、`popcap`、`requisition`、`ultimate` |
| `snapshot.timers` | **21** 条 |
| `snapshot.arena` / `snapshot.worldOffset` | `0x1d824000000` / `0x6400000` |
| `snapshot.players` | 1 行：`address = 0x1d82cc07000`、`identity = 0x3e8`、`slot = 0`、`name = null`、10 项资源全为 `0.0`、`population = [0.0, 0.0]`、`registryCount = 0`、`ownedEntities = []` |
| `snapshot.stableRootsAfterRead` | **true** |

结论：在同一活进程内，新版 RVA 根（`0x6bb1ed0` / `0x6187e40` / `0x5e0decc`）可读、并且读取前后保持稳定（`stableRootsAfterRead = true`）。

### 7.3 旧偏移在同一进程内失败

产物：`F:\WorkSpace\Game\.local\tmp-histprobe\live\live-legacy-roots.json`（642 字节）

| 字段 | 值 |
| --- | --- |
| `timestamp` | `2026-11-21T10:23:09.014353+00:00` |
| `pid` / `moduleBase` | `11296` / `0x7ff7a37e0000`（与 §7.2 同一进程） |
| `bytesRead` | **68** |
| `snapshot.roots` | `resource = 0x6bb0020`、`arena = 0x6185eb0`、`world = 0x5e0becc`（旧 2.4.0.6 偏移） |
| `snapshot.resources` | `{"error": "Invalid name vector"}` |
| `snapshot.timers` | `[]` |
| `snapshot.arena` / `snapshot.worldOffset` | `0x7ff7a75e7858` / `0x0` |
| `snapshot.players` | `{"error": "Invalid player count"}` |
| `snapshot.stableRootsAfterRead` | true（读到的根地址未变，但内容不可解析） |

结论：旧偏移在**同一个活进程**内解析失败（`Invalid name vector` / `Invalid player count`），这从运行期证明「必须按新布局取偏移」，旧偏移不可套用。

### 7.4 被测游戏进程

- 路径：`G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe`
- 文件版本：`5.1.52426.0`；长度 `108391096`；SHA256 `8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC`（本次 `Get-FileHash` 实测，与 `CoH3/dev/SkyEye/evidence/version-adaptation/static/REPORT.md` 一致）
- 探针时刻的进程：第一轮 `pid 11296`（启动 2026-11-21 18:22:00，停在主菜单），第二轮 `pid 16556`（启动 2026-11-21 18:31:09，实战对局中）；两者模块基址同为 `0x7ff7a37e0000`

### 7.5 对局中复测：新表解析出真实玩家数据

用户 18:31:09 重启游戏并进入实战对局后，用同一只读探针对新进程复测（游戏 pid `16556`，模块基址 `0x7ff7a37e0000`）：

产物：`F:\WorkSpace\Game\.local\tmp-histprobe\ig5\live-new-roots.json`（294951 字节，sha256 `f6ae852f779836e5ee93a63f9f133f201b82159dedf15e6b96a1967123297471`）

| 字段 | 值 |
| --- | --- |
| `timestamp` | `2026-11-21T10:38:35.387730+00:00` |
| `pid` / `moduleBase` | `16556` / `0x7ff7a37e0000` |
| `bytesRead` | **132545** |
| `snapshot.roots` | `resource = 0x6bb1ed0`、`arena = 0x6187e40`、`world = 0x5e0decc`（新表 RVA） |
| `snapshot.players` | **8 行**，逐行含 `name`、`identity`、`slot`、`resources`（10 项）、`population`、`registryCount`、`ownedEntityCount` |
| `snapshot.stableRootsAfterRead` | **true** |

玩家行（节选）：`slot 0 / identity 0x3e8 / name "Verethragna" / registryCount 150 / ownedEntityCount 150 / population [26,0]`；`slot 1 / name "sloppybog" / registryCount 145 / ownedEntityCount 145`；`slot 2 / registryCount 120`；`slot 3 / name "Sam`Body" / registryCount 56`；`slot 4 / registryCount 159`；`slot 5 / name "AC-130H…" / registryCount 131`；`slot 6 / registryCount 113`；`slot 7 / name "Hungryship70" / registryCount 99`。

同一进程、同一时刻用旧 2.4.0.6 偏移复测：产物 `F:\WorkSpace\Game\.local\tmp-histprobe\ig5\live-legacy-roots.json`（642 字节，sha256 `e756502a8f8e897cec183c8309deab410b372f3ae97eb1cea7ea9db75d8bf8c3`）→ `bytesRead = 68`、`players = {"error": "Invalid player count"}`。

结论：新版 RVA 表在**实战对局**中解析出 8 名玩家的真实名称、资源、人口与实体注册表计数（`registryCount` 与 `ownedEntityCount` 非零且与 `population` 一致），旧偏移在同一进程、同一时刻依旧解析失败。这是对「按 build 选表」最直接的运行期证据。

### 7.6 按构建选择偏移的判定复现（客户端源码 harness）

为了把「判定逻辑」本身也变成可复现的运行期产物，用客户端**自身源码**（`CoH3/dev/SkyEye/client/CoH3MemoryTool/GameVersion45.cs` 与 `GameLayout52426.cs`）编译了一个只读 harness（`PerBuildProbe`，net10.0 控制台；仅打印，不写任何进程）：

| 产物（`.local/tmp-histprobe/ig5/`） | 输入 | 输出 |
| --- | --- | --- |
| `probe-file-52426.json` | 真实游戏 exe 路径 | `status=Verified, build=Build52426, role=112918560, rva=112926416, gate=passed` |
| `probe-digest-52426.json` | 指纹 `8a3445…d3ec` | `status=Verified, build=Build52426, rva=112926416, gate=passed` |
| `probe-digest-2406.json` | 指纹 `2406b3ee…cde` | `status=Verified, build=Build2406, rva=112918560, gate=passed` |
| `probe-digest-unknown.json` | 指纹 `0000…0000` | `status=FingerprintMismatch, build=Unknown, rva=-1, gate=rejected:游戏版本未验证，结构依赖功能已暂停。请等待版本适配。` |
| `probe-roles-52426.jsonl` | `GameLayout52426.cs` 源文件 | `entries=37, resolved=37, mismatched=0`（表内 37 条全部解析且与源码一致） |

即：同一份生产源码，在三个指纹输入上分别给出 `Build52426` / `Build2406` / `Unknown` 三种判定；只有未知指纹会被 `RequireVerified` 拒绝，且拒绝时不产出任何偏移（`rva = -1`）。

### 7.7 本节限制

第一轮探针（§7.2 / §7.3，pid `11296`）时游戏停在主菜单，因此只验证到「地址可读且稳定」这一层：玩家行存在但 `name = null`、`population = [0,0]`、`registryCount = 0`、`ownedEntities = []`。第二轮探针（§7.5，pid `16556`）在对局中，已解析出 8 名玩家的名称、资源、人口与实体注册表计数；实体链明细、伤害/生产联动等更细的实战读数**未在产物中记录**。

### 7.8 用户实机运行：交付客户端自身的端到端读数

用户在重建后的交付客户端上实测（`CoH3/output/COH3SimpleTool/COH3工具.exe`，sha256 `9F3C8A6C…`，2026-11-21 18:19:37 构建），客户端**自己写出的运行期诊断日志**即端到端读数证据。原件已归档到 `CoH3/dev/SkyEye/evidence/version-adaptation/live-client-run-20261121/`：

| 文件 | 字节 | 时间跨度 | 记录内容（解码后） |
| --- | --- | --- | --- |
| `minimap-diagnostics.log` | 21650 | 18:32:07 – 19:01:20 | 「新对层小地图定位中」→「小地图绘制 单位=4 … 单位=17 阵营=0 nativeControl=183d2293990 cursor=18130000000 batchMB=… ms=21」 |
| `air-diagnostics.log` | 499 | 18:32:07 – 18:47:49 | 「连接对局 16556:639308826693246234:…:1006 敌方阵营位=1,3,5,7」 |
| `overlay-diagnostics.log` | 1329 | 18:33:58 – 18:59:28 | 「快速重连 单位列表正在更新」/「镜头已切换」/「重新连接 信息地址无效」 |
| `air-outline-diagnostics.log`、`minimap-location-cache.txt`、`minimap-ui-root-cache.txt` | 224 / 54 / 54 | — | 空中单位轮廓诊断与两处定位缓存 |

要点：

- 日志由客户端在**新版游戏进程（pid `16556`）**上写出：小地图绘制循环取到了单位数（4 → 17 随对局推进）、阵营、`nativeControl` 与光标根地址；空中单位模块读到敌方阵营位 `1,3,5,7`。
- 整段运行期**没有出现「游戏版本未验证」**——该文案由 `GameVersion45.RequireVerified` 抛出，一旦命中，结构依赖功能即停摆，诊断日志不会再有单位读数。
- 日志为 **UTF-8（无 BOM）**：`utf-8` 解码通过，`gbk` 解码直接失败（`'gbk' codec can't decode byte 0xad in position 38`）。此前在 PowerShell 里看到的 `灏忓湴鍥剧粯鍒?鍗曚綅=4` 是 **GBK 控制台显示 UTF-8 字节**造成的假象，不是文件本身的编码问题——按 UTF-8 读取即为「地图定位中 新对局小地图定位中…」等正常中文。

### 7.9 续跑窗口（19:02 – 19:15）

用户在同一游戏进程上继续运行交付客户端（`COH3工具.exe` pid `29688`，18:32:03 启动；`RelicCoH3.exe` pid `16556`，18:31:09 启动），日志持续增长；该窗口的原件已归档到 `CoH3/dev/SkyEye/evidence/version-adaptation/live-client-run-20261121-b/`：

| 文件 | 字节 | 最后写入 | SHA256 前 16 | 末行要点（UTF-8 解码） |
| --- | --- | --- | --- | --- |
| `minimap-diagnostics.log` | 31360 | 19:15:23 | `5B9D048123F48A73` | 「小地图绘制 单位=23 阵营=0 nativeControl=182fdc69510 cursor=18130000000 batchMB=0 ms=16」 |
| `air-diagnostics.log` | 1683 | 19:08:58 | `897B3EC3A4241446` | 「失败：空实体列表正在切换」 |
| `overlay-diagnostics.log` | 2361 | 19:08:56 | `F74B59F394D06329` | 「快速重连 单位列表正在更新」 |
| `air-outline-diagnostics.log` | 788 | 19:11:29 | `A7499FA27D804F0C` | 空中单位轮廓诊断 |
| `minimap-location-cache.txt` / `minimap-ui-root-cache.txt` | 54 / 54 | 19:06:50 | `807676936A989D9E` / `223ACBEAB3358AF5` | 两处定位缓存 |

四个日志解码后合计 329 行，对「游戏版本未验证」与「未验证」全文检索均为 **0 命中**；小地图读到的单位数随对局推进 4 → 17 → 23（§7.8 为 4 → 17）。

### 7.10 第三个活进程样本：换局后重读（19:22 – 19:23）

用户在 19:06 之后进入了**第二局**（对局内玩家名单整体更换）。本代理在同一进程上再取一次只读样本，与交付客户端自己的日志交叉核对：

| 项 | 值 |
| --- | --- |
| 样本文件 | `evidence/version-adaptation/live-probe-samples/live-new-roots-1922-match2.json`（482256 B，SHA256 `439504D7BBB92683…`） |
| 采样方式 | `live_probe.py --pid 16556 --resource 112926416 --arena 102268480 --world 98623180`（新版 RVA） |
| 结果 | `bytesRead: 217388`、`stableRootsAfterRead: true`、**8 名玩家**全部读出 |
| 玩家读数（节选） | slot0 `Gh3tt0Blast3R`（人口 41，实体 234）、slot1 `Careless Whisper`（54 / 186）、slot2 `d525clqcpd`（40 / 163）、slot3 `The Chemist`（58 / 189）、slot4（俄文名，51 / 256）、slot5 `Mir99`（41 / 217）、slot6 `NR…wild boar`（44 / 153）、slot7 `jryanj.com`（46 / 208） |
| 同一时刻客户端自记 | `minimap-diagnostics.log` 19:22:52「小地图绘制 单位=31 阵营=0 nativeControl=182fdc69510 cursor=18130000000 batchMB=0 ms=16」；四个日志对「未验证」检索 **0 命中** |

交叉核对结论：

- 探针（独立 Python 实现，按新版布局读同一进程）与交付客户端（C# 引擎，按同一张表读同一进程）在同一时间窗内都读到结构自洽的对局数据（玩家名/人口/实体数 → 单位数 31），两套实现互为独立路径且结论一致；
- 玩家名单整体更换说明这不是首局的残留读数，而是**换局后重新定位生效**；
- 三次活进程样本（主菜单 18:23 / 第一局 18:38 / 第二局 19:22）与两次旧偏移失败样本（旧表同进程同读法 642 B 报告 `Invalid player count`）已一并归档到 `evidence/version-adaptation/live-probe-samples/`，索引见同目录 `index.json`。

---

## 8. 历史结论保全

### 8.1 两个验证器 pin 了改动前的字节

| 验证器 | pin 的文件 | pin 值 |
| --- | --- | --- |
| `CoH3/dev/SkyEye/evidence/version-adaptation/production/verify_kwk38_sequence.py` | `GameLayout52426.cs` | 长度 `4524`，SHA256 `77c44effbf42d9f6300cfd41e753712bed85e551b487f4db1a779df528dc8d92` |
| 同上 | `PlayerInfoReader.cs` | 长度 `104468`，SHA256 `bbb359cd59409065d8b4d00ace784243ae5e99816611bc11617e7c9ed8adba9d` |
| `CoH3/dev/SkyEye/tests/decoder-layout/verify_task4.py` | `AirLogic.cs` | `8e3eced8cd1e9f120e76da38ba2d5146c0e56fcfe670ea66b4486fb2011dac50` |
| 同上 | `PlayerInfoReader.cs` | `bbb359cd59409065d8b4d00ace784243ae5e99816611bc11617e7c9ed8adba9d` |

> 文件名说明：真实文件名为 `verify_kwk38_sequence.py`（**kwk38**，不是 kwq38），实际位置在 `CoH3/dev/SkyEye/evidence/version-adaptation/production/` 下。

`verify_task4.py` 还断言旧源码中应包含 `#if MAINTENANCE_DIAGNOSTICS` / `if (maintenanceDiagnostics) return AirMoverShape.Candidate52426_184;`（`:43`），并在运行末写 `task4-fixture.diff` 与 `task4-review.json`（`:99-101`）。因此**这两个验证器本次均未修改**，历史结论未被改写。

### 8.2 保全做法：影子历史根复核

影子历史根由 `F:\WorkSpace\Game\.local\tmp-histprobe\build_hist_roots.py` 生成，冻结基线取自 `CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace`（本次**未改动** `round06-frozen/`），产出三个影子根：

- `.local/tmp-histprobe/hist-root-kwk38/`（kwk38 复核用）
- `.local/tmp-histprobe/hist-root-task4/`（task4 复核用，其中 `AirLogic.cs` / `PlayerInfoReader.cs` / `GameLayout52426.cs` 由冻结根覆盖，manifest 路径重写）
- `.local/tmp-histprobe/live-root-task4/`（不重写 manifest 的变体）

本次复核**只在 `%TEMP%` 的副本上运行**，未在仓库内产生任何写入。

### 8.3 复核结果

**kwk38**：把 `hist-root-kwk38` 复制为临时根后运行
`python -B <临时根>\CoH3\dev\SkyEye\evidence\version-adaptation\production\verify_kwk38_sequence.py --root <临时根> --write --self-test`

- 退出码 **0**；输出 `"localSequenceVerified": true`、`"fullAcceptance": false`、`"inputCount": 31`、`"remainingTicks": [456, 244, null]`、`"derivedSeconds": [57, 31, null]`、`"wroteReports": true`；
- 9 个负控全部通过：`ticks_mismatch`、`key_changed`、`known_false`、`completion_job_retained`、`whole_build_passed`、`run30_os_exit_invented`、`raw_byte_timer_conflict`、`input_hash_drift`、`existing_output_preflight`；
- 重生成的 `kwk38-sequence-validation.json` SHA256 `24139FCEFD512054AB0C8CAC594E1AFE26429E4507F2E64A15AB363E5239BC37`，与仓库内 `CoH3/dev/SkyEye/evidence/version-adaptation/production/kwk38-sequence-validation.json` **逐字节相同**；
- 重生成的 `kwk38-sequence-report.md` SHA256 `D7759F266BAEDC4A0E7424749BDA9571E5D7F09DF86970DF416D9A08F626C064`，仓库内现存报告为 `A373FB94B4C780897DE57BAE34C418316D8BF1E9A335AAAD602819ACD799C5E8`；两者差异**仅**来自报告内嵌的根路径前缀（临时根 vs `F:/WorkSpace/Game`），把前缀归一化后逐行相同。

**task4**：把 `hist-root-task4` 复制为临时根后运行
`python -B <临时根>\CoH3\dev\SkyEye\tests\decoder-layout\verify_task4.py`

- 退出码 **0**；输出 `{"status":"PASS_F_only_task4_review","reviewChecksPassed":34,"fixtureChecksPassed":288,"original186Preserved":true,…}`；
- 重生成的 `task4-review.json`：`checksPassed = 34`、`fixtureChecksPassed = 288`、`status = PASS_F_only_task4_review`；`checks` 列表与仓库内 `CoH3/dev/SkyEye/tests/decoder-layout/task4-review.json` 逐条相同（仅 `utc` 与绝对路径不同）；
- 重生成的 `task4-fixture.diff` SHA256 `4F6639CDF7E8AC6B3B559A3900BCFAAA47F8CAE4061BCAFB3FCE989AEE6959E3`，与仓库内现文件**完全相同**；
- 影子根内的历史夹具报告：Release `passed = 118`（`legacyAndMapperPassed = 89`）、Maintenance `passed = 170`（`legacyAndMapperPassed = 97`），`118 + 170 = 288`，与 `verify_task4.py` 的 `fixtureChecksPassed = 288` 吻合。

**影子根引擎文件哈希**（即改动前字节）：`AirLogic.cs = 8E3ECED8…`、`GameLayout52426.cs = 77C44EFF…`、`PlayerInfoReader.cs = BBB359CD…`；影子根 `source-manifest.json` 仍 pin 旧哈希。

> 结论：历史结论（kwk38 时序验收、task4 夹具评审）通过**影子历史根复核**保全，验证器本身与其历史报告未被改写；历史基线 `CoH3/dev/SkyEye/tests/decoder-layout/task4-baseline/` 保留原样（历史报告口径为 release 89 / maintenance 97）。

---

## 9. 交付物

| 交付物 | SHA256 | 字节 | 最后写入 | 产物路径 |
| --- | --- | --- | --- | --- |
| COH3 工具（受保护） | `9F3C8A6C139E5CD585E2AF908274EE156B3EDBB546E272E7FA0F7D58E646E319` | 588800 | 2026-11-21 18:19:37 | `CoH3/output/COH3SimpleTool/COH3工具.exe` |
| 天穹之眼（受保护，delivery staging） | `46CE9F7F7FD693C6577DFD05DC70FDD15368890131DB9778BF1B3DC75592ABD9` | 1226240 | 2026-11-21 18:19:32 | `CoH3/dev/SkyEye/delivery/client/天穹之眼-自有授权版-v0.60.8.exe` |

两者 SHA256 均为本次 `Get-FileHash -Algorithm SHA256` 实测。`CoH3/dev/SkyEye/evidence/latest-build.json` 与交付一致：

```json
{"Run":"20261121-181930-912","Tool":"Obfuscar.GlobalTool 2.2.50",
 "InputSha256":"1DD98A5D97E1C61D60173BD5B55A116966F8537AB95C3B799362D75B0C5DD512",
 "OutputSha256":"46CE9F7F7FD693C6577DFD05DC70FDD15368890131DB9778BF1B3DC75592ABD9",
 "Client":"…\\delivery\\client\\天穹之眼-自有授权版-v0.60.8.exe","NoPublicSymbols":true}
```

COH3 工具同目录另有 `COH3工具.exe.config`（2026-11-16 19:01:55，未变）与运行期诊断日志 `air-diagnostics.log`、`air-outline-diagnostics.log`、`minimap-diagnostics.log`（2026-11-21 18:32–18:33）。

### 9.1 交付二进制内的常量定位（只读静态定位）

对已交付的受保护客户端 `CoH3/output/COH3SimpleTool/COH3工具.exe`（sha256 `9F3C8A6C…`，IG5/Ghidra 只读装载，`.text` VA `0x402000`–`0x48bc00`）做常量定位：

| 目标 | VA | 字节 |
| --- | --- | --- |
| 5.1.52426.0 指纹字面量（UTF-16LE） | `0x488a3c` | `38 00 61 00 33 00 … 63 00`（`8a344529…d3ec`） |
| 旧构建指纹字面量（UTF-16LE） | `0x488aed` | `32 00 34 00 30 00 … 65 00`（`2406b3ee…cde`） |
| legacy 角色键 + 新 RVA 的成对立即数 | `0x40a62f` | `20 00 bb 06 6a 20 d0 1e bb 06 …` |

第三行的含义：`20 00 bb 06` = `0x06BB0020` = 112918560（legacy 角色键），`d0 1e bb 06` = `0x06BB1ED0` = 112926416（该角色在 5.1.52426.0 的新 RVA），两者在 IL 中紧邻出现，即新偏移表确已进入交付二进制。同样的扫描在未混淆的 Release 产物 `CoH3/dev/SkyEye/client/bin/Release/net48/天穹之眼-自有授权版-v0.60.8.exe`（sha256 `1DD98A5D…`）上同样命中（UTF-16 指纹位于文件偏移 `0x52326` / `0x522A3`，新 RVA 立即数位于 `0xA651`）。

这一步只证明常量确实在交付件里；偏移的**运行期**正确性由 §7 的活进程证据给出。

### 9.2 天穹之眼交付件的同口径常量定位

对已交付的受保护天穹之眼客户端 `CoH3/dev/SkyEye/delivery/client/天穹之眼-自有授权版-v0.60.8.exe`（sha256 `46CE9F7F…`，IG5/Ghidra 只读装载，1065 函数，`.text` VA `0x402000`–`0x527600`）做同口径定位：

| 目标 | VA | 字节 |
| --- | --- | --- |
| 新构建指纹字面量（UTF-16LE，128 B） | `0x5235f2` | `38 00 61 00 33 00 … 63 00 00 00`（`8a344529…d3ec`） |
| 旧构建指纹字面量（UTF-16LE，128 B） | `0x52369b` | `32 00 34 00 30 00 … 65 00 00 00`（`2406b3ee…cde`） |
| legacy 角色键 | `0x40d0f7` | `20 00 bb 06`（= `0x06BB0020` = 112918560） |
| 新 RVA 立即数 | `0x40d0fd` | `d0 1e bb 06`（= `0x06BB1ED0` = 112926416） |

与 §9.1 的 COH3 工具同构：两个指纹常量与「旧键 + 新 RVA」成对立即数都在交付件里，即**两个工具的交付二进制都带上了 5.1.52426.0 的指纹与新偏移表**。

同一批只读字节扫描（直接对文件做原始字节比对，与 IG5 数据库无关）给出的文件偏移：

| 产物 | 新指纹 | 旧指纹 | 新 RVA 立即数 | 旧键 |
| --- | --- | --- | --- | --- |
| 受保护天穹之眼（1226240 B） | `0x1217F2` | `0x12189B` | `0xB2FD` | `0xB2F7` |
| 未混淆 Release 天穹之眼（1187328 B，sha256 `1DD98A5D…`） | `0x52326` | `0x522A3` | `0xA651` | `0xA64B` |

未混淆 Release 产物的托管元数据仍为明文（类型名 `GameVersion45` 位于 `0x4771F`、`GameLayout52426` 位于 `0x47A6B`），受保护件的类型名已被重写，但指纹与偏移常量以明文形式保留（如上表 VA）。

---

## 10. 限制与后续

### 10.1 `CoH3/output/SkyEye/` 的三个 ZIP 已按授权修订刷新（2026-11-21 20:11 起）

| 文件 | 字节 | SHA256 |
| --- | --- | --- |
| `CoH3/output/SkyEye/SkyEye-HardenedV2-Client.zip` | 756865 | `abc1eb1c01e1dc993ca329daa6619bfcb4896bddaed1758a480e1167c82aec6c` |
| `CoH3/output/SkyEye/SkyEye-HardenedV2-Server.zip` | 17117602 | `e116428341b697fd6786d0649a0726422108946f1e7b080f905c179c0ee831f0` |
| `CoH3/output/SkyEye/SkyEye-HardenedV2-Maintenance.zip` | 1115779376 | `6ed57a23168192e5dabb9e91b13ad50d7e1f284fc972f3d54e886a74c021ddb2` |

客户包内含 15 个文件 + `PACKAGE-MANIFEST.json`，其中受保护客户端为 1226240 字节 / `46CE9F7F7FD693C6577DFD05DC70FDD15368890131DB9778BF1B3DC75592ABD9`，随包 `.exe.config` 含 `https://124.220.147.56/` 与 `CoH3FowTool`。刷新前的旧包（2026-11-16：754126 / 17117607 / 10375213 字节）已被本次授权修订取代，其哈希仍保留在 `evidence/authorized-revision-2026-11-21.json` 的 `clientArchive.history` 中。

刷新前置条件与实际执行过程见 §11；本节只记录结果状态。

### 10.2 活进程只验证到"可读且稳定"

见 §7.5、§7.8、§7.10：探针已在两局对局中各解析出 8 名玩家的名称、资源、人口与实体注册表计数（主菜单态一次、第一局一次、换局后第二局一次），同一时刻交付客户端自身写出的小地图/空中单位诊断日志也记录了实战读数，两者交叉核对一致。仍未在产物中记录的是实体链明细、生产/伤害联动，以及空中单位读数的**数值本体**。

### 10.3 4.8 引擎客户端的端到端对局读数

net48/x64 的天穹之眼客户端在真实对局中的端到端读数由用户在本机交互实测；极简版（`COH3工具.exe`）客户端自身写出的运行期诊断日志已随 §7.8 与 §7.9 归档，但**天穹之眼客户端本次未采集运行期日志**，其读数未在产物中记录。

补记（本次勘察）：天穹之眼客户端的 GUI 实测**未由本代理发起**——用户在 19:00 前后仍在线上对局（游戏 pid `16556`、客户端 pid `29688` 均在运行），另起第二个受保护客户端会在其屏幕上叠加小地图/空中单位浮窗并争用同一张卡密的会话，属于对用户实测的干扰，故只做了不打扰的静态侧确认：

- 天穹之眼交付件的常量定位见 §9.2（新指纹 UTF-16LE 位于 VA `0x5235f2`、旧指纹位于 `0x52369b`、legacy 键 `0x40d0f7`、新 RVA `0x40d0fd`）；
- 引擎源码与极简版**字节一致**（§4.1），且该客户端本次通过了同一套受保护验证（75 PASS + 三组离线回归，见 §6）；
- 本地自托管授权服务仅在本次勘察中短暂启动（`127.0.0.1:47835`）用于确认卡密凭据格式，**未向用户的 `%LOCALAPPDATA%\Skyeye\SelfHostedLicensing\credentials.dat` 写入任何内容**，该文件在勘察结束时与开工时一致（358 B）。

### 10.4 其余陈旧产物（本次未修改）

- `CoH3/dev/SkyEye/tests/layout-preflight/results.json`（旧口径 94 / 8）、`CoH3/dev/SkyEye/tests/layout-preflight/README.md`；
- `CoH3/dev/SkyEye/tests/decoder-layout/README.md`（旧口径 288 PASS）。

这些文件与当前源码/夹具不一致，建议在后续刷新时一并更新（本次按"只写一个文件"的约束保持原样）。

### 10.5 刷新客户 ZIP 所需的证据清单（本轮已全部执行，结论见 §11）

下表是刷新前逐项读过脚本后列出的清单与当时状态；本轮（2026-11-21 20:00 起）已按此清单重新生成证据，客户 ZIP 已刷新。

| # | 证据 | 入口 | 本轮实际结果 |
| --- | --- | --- | --- |
| 1 | 未加保护基线 7 例 | `scripts/run_raw_suite.py --base-url https://124.220.147.56/ --code-directory LicensingSystem/.local/hardening-v2/cards-raw-20261121` | **已跑通**，7/7 通过，`evidence/raw-signed-20261121-194453/summary.json` |
| 2 | 受保护客户端 7 例 | `Verify-Protected.ps1 -BaseUrl https://124.220.147.56/ -PrivateCodeDirectory …/cards-protected-20261121` | **已跑通**，7/7 通过，`evidence/protected-verification-20261121-194637-331.json` |
| 3 | reactor trial 7 例 | `scripts/Reactor-VerifyTrial.ps1` | **本机无法重跑**：`.NET Reactor` 试用版 CLI 从未入库，`CoH3/dev/SkyEye/tools/` 下只有 `ilspy/ilspycmd.exe` 与 `obfuscar/obfuscar.console.exe`；沿用 2026-11-13 产物并在 limitations 中披露（§11.5） |
| 4 | 服务端测试 | `LicensingSystem/Build.ps1 -VerifyServer` | **195/195 通过**（`test-runs/20261121-101926-01b34cac`） |
| 5 | 会话清理 12 项 | `scripts/verify_cloud_session_cleanup.py --fixtures … --allow-global-sessions 1` | **已跑通**，12 项全过，`evidence/session-cleanup-cloud-20261121.json` |
| 6 | 真实入口 GUI 抓窗 | `scripts/Capture-Window.ps1` | **已采集**（隔离回环部署），`evidence/protected-real-entry-lifecycle-20261121.json` |
| 7 | 发布件 ILSpy 静态分析 | `python Measure-Protection.py --out release-static-analysis-20261121` | **已生成**，ILSpy `exitCode = 0`、`csharpFiles = 56`（ILSpy 按命名空间分组输出，新增 `GameLayout52426.cs` 不改变文件数，`Finalize.py:35` 的硬编码 56 无需改动） |
| 8 | 验证摘要生成 | `python Summarize-Validation.py …` | **已重写为参数化**并生成 `evidence/validation-summary-2026-11-21.json` |
| 9 | 新授权修订记录 | `python scripts/record_authorized_revision.py` | **已补声明**（19 条保留项偏差，含本次 8 个客户端源文件），生成 `evidence/authorized-revision-2026-11-21.json` |

以上各项均针对 2026-11-21 的新构建重新生成；没有任何一项用 2026-11-16 的旧像素/音频/窗口证据顶替。

## 11. 发布刷新（2026-11-21 20:00 – 20:15）

### 11.1 刷新步骤与顺序

发布包刷新按「录授权修订 → `Finalize.py --refresh` → 再录修订 → 再 `--refresh`」执行，理由是 `Finalize.py:109` 要求客户包当前哈希落在 `{cleanup 基线哈希, 修订记录里的客户包哈希}` 之一：

1. `python scripts/record_authorized_revision.py` → `evidence/authorized-revision-2026-11-21.json`（19 条保留项偏差，其中 8 条是本次改动的客户端源文件；客户包 `baseline = cda5e3de4f31…`、`current = 44509096e0f3…`，与磁盘一致）；
2. `python Finalize.py --refresh --summary evidence/validation-summary-2026-11-21.json --revision evidence/authorized-revision-2026-11-21.json` → 39 项审计检查通过，客户包按新客户端重新签发（`abc1eb1c01e1…`）；
3. 再次 `python scripts/record_authorized_revision.py` → 客户包 `current` 更新为刷新后的 `abc1eb1c01e1…`；
4. 再次 `python Finalize.py --refresh …` → `archive_bound` 命中新哈希；客户包因载荷集未变而**保持字节不变**（`Finalize.py:216-218`），服务端包与维护包重新生成。

第 4 步首次执行时在替换维护包处遇到 `PermissionError: [WinError 5]`（1.1 GB 的包被系统扫描占用）；已在 `Finalize.py` 中加入有界重试（`replace_archive`，最多 12 次、间隔 5 秒）后重跑通过。`Finalize.py` 另加 `--summary` / `--revision` 两个参数，并把「服务端 120 项」断言改为「记录到的检查数 ≥ 120 且与摘要自洽」（本轮为 195）。

### 11.2 刷新后的三个包

| 文件 | 字节 | SHA256 |
| --- | --- | --- |
| `CoH3/output/SkyEye/SkyEye-HardenedV2-Client.zip` | 756865 | `abc1eb1c01e1dc993ca329daa6619bfcb4896bddaed1758a480e1167c82aec6c` |
| `CoH3/output/SkyEye/SkyEye-HardenedV2-Server.zip` | 17117602 | `e116428341b697fd6786d0649a0726422108946f1e7b080f905c179c0ee831f0` |
| `CoH3/output/SkyEye/SkyEye-HardenedV2-Maintenance.zip` | 1115779376 | `6ed57a23168192e5dabb9e91b13ad50d7e1f284fc972f3d54e886a74c021ddb2` |

维护包解压后 **1.37 GB**，其中 1.365 GB 来自 `CoH3/dev/SkyEye/evidence/`（本次版本适配的原始取证：`version-adaptation/static/pe-candidates.json` 58.8 MB、`ui-followup.json` 38.1 MB、`lifecycle-phase/**` 若干 4–5 MB 的 JSON、`release-static-analysis*/members.json` 各 24 MB）；包内**不含** `.local/`、`.cache/` 与任何 `.pem/.pfx/.p12/.key`（封存脚本的秘密扫描与逐项清单均已通过）。

仓库里另有一个**被 git 跟踪的解压副本** `CoH3/output/SkyEye/SkyEye-HardenedV2-Client/`（旧包就地运行后留下的目录，内含 `alert-sounds.txt` / `overlay-diagnostics.log` / `overlay-settings.txt` 三个运行期生成文件，不属于包内容）。本轮已用新签发的客户包覆盖其中的 15 个载荷文件与 `PACKAGE-MANIFEST.json`，使解压副本与 zip 一致，避免 `output/` 下继续留有 52426 适配之前的旧客户端（1221120 字节 / `A9AA1858…`）。

### 11.3 本轮重新生成的证据

| 证据 | 入口 | 结果 |
| --- | --- | --- |
| 服务端真实 HTTP + SQLite 测试 | `LicensingSystem/server-tests` | **195/195 通过**（`test-runs/20261121-101926-01b34cac`） |
| 未混淆基线 7 例 | `scripts/run_raw_suite.py --base-url https://124.220.147.56/ --code-directory …/cards-raw-20261121` | 7/7 通过，`evidence/raw-signed-20261121-194453/summary.json` |
| 受保护客户端 7 例 | `Verify-Protected.ps1 -BaseUrl https://124.220.147.56/ -PrivateCodeDirectory …/cards-protected-20261121` | 7/7 通过，`evidence/protected-verification-20261121-194637-331.json` |
| 界面像素 | `scripts/capture_ui_pair.py` | 同条件先后采集两版，main/expanded/compact `changedPixels = 0/0/0`（`evidence/ui-pair-20261121-200800.json`） |
| 音频 | 同一套验证器的 audio 例 | 两变体各 `decodedFiles = 8` 且 `payloadsIdentical` |
| 云端会话清理 | `scripts/verify_cloud_session_cleanup.py --fixtures … --allow-global-sessions 1` | 12 项 checks 全过；全局活跃会话 1（额度 1，属操作者自己的在线客户端，非发布夹具卡） |
| 真实入口 | `scripts/Capture-Window.ps1` + 隔离回环部署 | 窗口「天穹之眼 · 激活」706×509、12 个 UIA 节点、截图成功、`Closed = true` |
| 静态分析 | `python Measure-Protection.py --out release-static-analysis-20261121` | REA 347 types / 4061 methods / 1423 fields / 1571 member refs；ILSpy `exitCode = 0`、`csharpFiles = 56` |
| 派生汇总 | `python Summarize-Validation.py …` | `evidence/validation-summary-2026-11-21.json`：`passed = true`、三类各 7 模式、6 项像素差 0、服务端 195、12 项会话检查 |
| 授权修订 | `python scripts/record_authorized_revision.py` | `evidence/authorized-revision-2026-11-21.json`，19 条覆盖 |
| 封存 | `python Finalize.py --refresh …` | 39 项审计检查通过；`evidence/final-validation.json` |

### 11.4 三处口径差异（均已写入 limitations）

1. **界面像素改为同条件采集**：验证器的设置窗会附加到正在运行的游戏进程（`OverlaySettingsForm.PaintPreview` 在 `liveRows` 为真时用 `OverlayText.NumberSpans` 给数字着色），因此预览示例行里的「120」会随活体数据在黄/红之间变化。19:44:55 的 raw 套件与 19:46:38 的 protected 套件相隔约 100 秒，像素差 212 个全部落在该区域（bbox `(224,636)-(246,652)`），与保护无关；改为同一秒内先后采集两版后差异为 0。
2. **全局会话口径**：2026-11-16 的证据要求全局活跃会话严格为 0；本轮操作者自己的客户端在线（其卡密不属于任何发布夹具集，已用 `codeId` 对照 `evidence/*live-fixtures*.json` 证明），故改为「夹具卡必须全部不活跃 + 全局会话数不超过显式额度（1）」。
3. **真实入口隔离**：为不干扰用户正在进行的对局，真实入口在隔离的回环部署（空库、同一 keyring、127.0.0.1:47835）上采集：用户卡密在本地库不存在 → 激活失败 → 只出现登录窗，不附加正在运行的游戏、不画浮窗；`credentials.dat` 勘察前后一致（358 B）。

### 11.5 未能重跑的一类证据

Reactor 试验 7 例**无法**按新客户端重建：`.NET Reactor` 试用版 CLI 从未入库（`git ls-tree` 全仓无 `tools/reactor` 路径，磁盘搜索 `dotNET_Reactor*` 为空），`scripts/Reactor-Protect.ps1:14` 在 CLI 缺失时直接抛错。因此派生汇总中 `reactorTrial*` 三组沿用 2026-11-13 的产物（trial SHA256 `6551eb5b…`、`ProductionLicensed = false`、7/7 通过），其像素与音频比较保留历史结论；该限制已写入 `evidence/validation-summary-2026-11-21.json` 的 `limitations`。

---
---

## 附：复核命令

以下命令均可在仓库根 `F:\WorkSpace\Game` 直接粘贴执行；标注"本次实测"的是本文档撰写时实际运行过的命令。

```powershell
# 1) 目标游戏主程序指纹（本次实测）
Get-FileHash 'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe' -Algorithm SHA256
(Get-Item 'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe').VersionInfo.FileVersion

# 2) 两棵客户端树的四个文件是否字节一致（本次实测）
$a='CoH3\dev\SkyEye\client\CoH3MemoryTool'; $b='CoH3\dev\COH3SimpleTool\CoH3MemoryTool'
'GameVersion45.cs','GameLayout52426.cs','AirLogic.cs','PlayerInfoReader.cs' | ForEach-Object {
  $ha=(Get-FileHash "$a\$_" -Algorithm SHA256).Hash; $hb=(Get-FileHash "$b\$_" -Algorithm SHA256).Hash
  "{0,-22} identical={1} {2}" -f $_,($ha -eq $hb),$ha }

# 3) 构建门夹具：4 组组合，日志写入 tests\build-gate\validation.log（19:18 复跑，exit=0）
& CoH3\dev\SkyEye\tests\build-gate\Run.ps1
#   日志为 UTF-8 BOM + UTF-16LE 正文，查看：
Get-Content CoH3\dev\SkyEye\tests\build-gate\validation.log -Encoding Unicode | Select-String 'PASS total='

# 4) 布局预检夹具：只读运行，Release / Maintenance 各 104 项（19:18 重编译后复跑，exit=0）
& LicensingSystem\.cache\dotnet-10\dotnet.exe build CoH3\dev\SkyEye\tests\layout-preflight\LayoutPreflight.Fixture.csproj -c Release --nologo -v:minimal
& LicensingSystem\.cache\dotnet-10\dotnet.exe build CoH3\dev\SkyEye\tests\layout-preflight\LayoutPreflight.Fixture.csproj -c Maintenance --nologo -v:minimal
& LicensingSystem\.cache\dotnet-10\dotnet.exe CoH3\dev\SkyEye\tests\layout-preflight\bin\Release\net10.0\LayoutPreflight.Fixture.dll F:\WorkSpace\Game
& LicensingSystem\.cache\dotnet-10\dotnet.exe CoH3\dev\SkyEye\tests\layout-preflight\bin\Maintenance\net10.0\LayoutPreflight.Fixture.dll F:\WorkSpace\Game

# 5) 解码器夹具：两配置同断言 + 跨配置一致性 + 与 perbuild-baseline 逐条比对（19:19 复跑，exit=0）
& CoH3\dev\SkyEye\tests\decoder-layout\Run.ps1
Get-Content CoH3\dev\SkyEye\tests\decoder-layout\bin\Release\net10.0\fixture-report.json -Raw | ConvertFrom-Json | Select-Object mode,passed,legacyAndMapperPassed
Get-Content CoH3\dev\SkyEye\tests\decoder-layout\bin\Maintenance\net10.0\fixture-report.json -Raw | ConvertFrom-Json | Select-Object mode,passed,legacyAndMapperPassed

# 6) 构建链结论行（本次实测，逐行引用）
Select-String -Path .local\tmp-histprobe\build-chain.log -Pattern '^=== STEP|^=== END|RESULT:|Protected client:|Protected executable:|All requested|CHAIN_OK' -Encoding UTF8

# 7) 活进程探针结果字段（本次实测）
python -c "import json;d=json.load(open(r'.local\tmp-histprobe\live\live-52426-roots.json',encoding='utf-8'));print(d['bytesRead'],d['snapshot']['roots'],d['snapshot']['stableRootsAfterRead'])"
python -c "import json;d=json.load(open(r'.local\tmp-histprobe\live\live-legacy-roots.json',encoding='utf-8'));print(d['bytesRead'],d['snapshot']['resources'],d['snapshot']['players'])"

# 8) 交付物哈希（本次实测）
Get-FileHash 'CoH3\output\COH3SimpleTool\COH3工具.exe' -Algorithm SHA256
Get-FileHash 'CoH3\dev\SkyEye\delivery\client\天穹之眼-自有授权版-v0.60.8.exe' -Algorithm SHA256

# 9) 历史结论保全：影子历史根复核（写入 %TEMP%，不动仓库）
python F:\WorkSpace\Game\.local\tmp-histprobe\build_hist_roots.py
#   复制 hist-root-kwk38 / hist-root-task4 到 %TEMP% 后分别执行：
#   python -B <临时根>\CoH3\dev\SkyEye\evidence\version-adaptation\production\verify_kwk38_sequence.py --root <临时根> --write --self-test
#   python -B <临时根>\CoH3\dev\SkyEye\tests\decoder-layout\verify_task4.py

# 10) 布局表 37 个角色偏移的溯源审计（本次实测，只读；无来源条目时退出码 1）
python CoH3\dev\SkyEye\scripts\audit_role_provenance.py
Get-Content CoH3\dev\SkyEye\evidence\version-adaptation\role-provenance.md -TotalCount 9

# 11) 活进程样本（本次实测，只读；需要游戏进程在跑）
python CoH3\dev\SkyEye\evidence\version-adaptation\live_probe.py --pid <RelicCoH3 PID> --out <OUT.json> --resource 112926416 --arena 102268480 --world 98623180
#   旧偏移对照：--resource 112918560 --arena 102260400 --world 98614988
#   已归档样本与哈希索引：
Get-Content CoH3\dev\SkyEye\evidence\version-adaptation\live-probe-samples\index.json -Raw | ConvertFrom-Json | Select-Object -ExpandProperty samples | Select-Object file,bytes,sha256

# 12) 夹具复跑结果（本次实测，产物见 evidence/version-adaptation/fixture-rerun-20261121.json）
Get-Content CoH3\dev\SkyEye\evidence\version-adaptation\fixture-rerun-20261121.json -Raw | ConvertFrom-Json | Select-Object -ExpandProperty suites

# 13) 一键复核全部结论（本次实测 13/13 PASS；只读，不启动游戏、不附加进程、不碰凭据）
python CoH3\dev\SkyEye\scripts\verify_perbuild_adaptation.py
# 14) 发布刷新（四步，见 §11.1）：先录修订，再 Finalize --refresh，再录修订，再 Finalize --refresh
#   cd CoH3\dev\SkyEye
#   python scripts\record_authorized_revision.py
#   python Finalize.py --refresh --summary evidence\validation-summary-2026-11-21.json --revision evidence\authorized-revision-2026-11-21.json
#   python scripts\record_authorized_revision.py
#   python Finalize.py --refresh --summary evidence\validation-summary-2026-11-21.json --revision evidence\authorized-revision-2026-11-21.json
#   三包哈希见 CoH3\output\SkyEye\*.zip.sha256
```

> 文档范围声明：本文档只记录 `5.1.52426.0` 适配与「按 build 选表」改造的证据；`CoH3/dev/SkyEye/evidence/version-adaptation/` 下的其他历史报告（run25–run33、lifecycle-phase、round06-frozen 等）各自保留其原有结论与限制，不因本文档而更新。
