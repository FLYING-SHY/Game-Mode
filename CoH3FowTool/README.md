# CoH3FowTool 2.1 — Company of Heroes 3 本地显示工具（GUI + 授权）

针对 **CoH3 零售 build 26443338** 的地形迷雾与实体显示开关。当前方案已在本地 CPU 遭遇战中验证敌军模型、建筑、红色图标及小地图显示，并验证关闭后重新隐藏、再次开启后重新出现。

当前路径通过 **六处本地显示分支 + 两个 UI 开关**，由游戏自己的每帧更新同步渲染状态。无需 `-dev`，无需打开控制台；`reveal` / `apply` 不发送键盘输入或 PCMD 命令，也不应用旧版遥测抑制补丁。识别到旧版五处修改时，工具会将其恢复为原始代码。

## 2.1 发行版（GUI 单文件 + 授权）

| 项目 | 值 |
|---|---|
| 发行文件 | `dist/CoH3FowTool-2.1-win-x64.zip`（42,528,886 字节，SHA-256 `15cc47aec6b5be75122b34ff38b5f93ed04216b7a45ae598d3dac5b5120e672f`） |
| 内容 | `CoH3FowTool.exe`（48,080,992 字节，SHA-256 `e974cd8e731b51463802a5dcaad12bf533a54903e837c6bc4955b5ab0f18c9a5`）+ `使用说明.txt` |
| 界面 | 中文 WinForms，双击运行（UAC 自动请求管理员），无需 `.bat` |
| 授权 | 一码一次兑换；服务端绑定首次激活成功的设备；永久码仅激活联网、限时码每次启动联网核对 |
| 保护 | Obfuscar 2.2.50 混淆两个自家程序集后打进单文件 bundle；发行包不含源码/PDB/私钥/`.local` |

用户操作：输入授权码 → 激活 → 开启/关闭/还原。关闭、还原、状态查看不受授权限制。CLI 里 `apply/reveal/watch/file-apply` 已移除（返回码 3），防止绕过授权；`status/hide/revert/file-revert/selftest` 保留。

授权后端为本地 Keygen CE（见 [Licensing/README.md](<../Licensing/README.md>)），当前仅监听 `127.0.0.1`。公网部署（供他人激活）需云主机 + HTTPS 反代固定 Host，选项与费用列在 [Licensing/framework-evidence.md](<../Licensing/framework-evidence.md>)，本期不购买、不部署。

授权系统是仓库内独立目录 `../Licensing/`，所有游戏修改器共用同一套；本工具在授权系统中的产品 id 为 `CoH3FowTool`（见 `../Licensing/client/ProductIds.cs` 与 `AGENTS.md` 的多产品模型）。

## 快速使用

1. 启动游戏，进入本地遭遇战，等待战场与单位加载完成。
2. 以管理员身份运行发行版 `CoH3FowTool.exe`，默认进入菜单。
3. 按 `1` 开启；按 `3` 查看状态。正常开启时四个揭示值均为 `true`。
4. 按 `2` 关闭显示；需要同时还原代码时按 `4`。

菜单：`1` 开启、`2` 关闭、`3` 状态、`4` 关闭并还原、`8` 自检、`h` 帮助、`0` 退出。

默认游戏路径如下；安装在其他位置时使用 `--exe` 指定：

```text
G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe
```

```powershell
.\CoH3FowTool.exe --exe "D:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe" reveal
```

## 命令

```text
CoH3FowTool.exe [--exe <RelicCoH3.exe>] <命令>
```

| 命令 | 行为 |
|---|---|
| `reveal` / `apply` | 开启六处显示分支、设置揭示状态，并回读确认 |
| `hide` | 关闭四个揭示值，保留六处代码，便于再次开启 |
| `revert` | 关闭显示，并恢复本版六处及旧版五处代码 |
| `status` | 查看版本、磁盘/进程补丁状态和运行时显示状态 |
| `watch` | 每 2 秒检查并尝试开启显示；`Ctrl+C` 结束循环 |
| `selftest` | 复制游戏 exe 到临时目录，执行六项文件补丁自检 |
| `menu` | 交互菜单；无参数时默认执行 |
| `help` / `-h` / `--help` | 显示用法 |

常用操作：

```powershell
.\CoH3FowTool.exe status
.\CoH3FowTool.exe reveal
.\CoH3FowTool.exe hide
.\CoH3FowTool.exe revert
```

使用 `hide` 或 `revert` 前先结束正在运行的 `watch`，否则下一轮检查会重新开启显示。结束工具或 `watch` 本身不会自动撤销游戏中的改动；使用 `hide` / `revert`，或退出游戏进程来结束内存状态。

以下兼容命令保留，但不属于上述内存使用流程：

| 命令 | 行为 |
|---|---|
| `file-apply` | 游戏退出后，将六处显示代码写入磁盘，并保留对应的 `.bak` |
| `file-revert` | 游戏退出后，从目标 exe 对应的 `.bak` 恢复磁盘文件 |
| `launch-opts --add` / `--remove` | 管理 Steam 的 `-dev` 启动项；当前显示方案无需此项 |

`file-apply` 只设置代码，不设置对局中的 UI 开关；进入对局后仍需运行 `reveal`。`revert` 恢复的是运行中进程，磁盘修改由 `file-revert` 恢复。默认内存流程保持磁盘游戏 exe 原样。

## 已验证的版本与画面

| 项目 | 已验证值 |
|---|---|
| 游戏版本 | `3.0.0.52071` |
| Steam app / build | `1677280` / `26443338` |
| 原始 exe 大小 | `108,388,536` 字节 |
| 原始 SHA-256 | `21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca` |
| 架构 / 首选 image base | PE32+ x64 / `0x140000000` |
| 实测场景 | 本地遭遇战，BLINDER 街巷，CPU 标准 |
| 实测时间 | 2026-11-08；同次测试进程 PID `18552`，PID 仅作记录 |

以下由最终自包含发行程序在新对局中取得，镜头相同，关闭/开启时间分别为 `03:41` / `03:44`：

| `hide`：恢复隐藏 | `reveal`：重新显示 |
|---|---|
| ![关闭后的战场与小地图](<tools/reverse/fow_release_enemy_off.png>) | ![开启后的敌方基地与小地图](<tools/reverse/fow_release_enemy_on.png>) |

关闭时四个揭示值为 `false`，敌军模型与建筑隐藏；再次开启后四值为 `true`，模型、图标及小地图信息重新出现。前一场遭遇战也验证了步兵模型与红色血条。最终程序另验证了单次开启后从 `00:22` 推进到 `01:51` 时四值仍为 `true`，期间没有运行 `watch` 或重发命令。测试结束执行 `revert`，本版六处与旧版五处代码全部恢复原字节，四值全部为 `false`，磁盘原始 SHA-256 保持不变。具体记录见 [发行版验证记录](<tools/reverse/validation_2_0.md>)。

**验证范围仅为上述本地遭遇战。公开匹配、天梯、联机同步及服务端行为尚未实测；其他版本、地图和完整对局生命周期也未作覆盖性验证。** 文件、导入表或字符串扫描只提供静态样本观察，本次没有得出反作弊或完整性检查机制不存在的结论。

**2026-11-08 后续 REA 查核：** 已找到自带 `AntiCheatWorldService` 读取显示路径所影响的同类对象状态，并在显示结果与另一实体允许条件不一致时追加内部记录的分支。直接没有写模拟或遥测字段，不等于显示变化不被其他消费者观察。当前公开匹配的启用、实际触发、上传与处罚仍未验证；2.0 尚未实现在线免检测／免封。本轮没有改动发布程序，完整证据与限制见 [检测相关逆向报告](tools/reverse/online_diag_ac_report.md)。

`selftest` 验证的是文件补丁流程，不代替游戏画面验证。它的六项为：原始定位、写入成功、补后定位、目标字节一致性、备份恢复、恢复后逐字节一致。原游戏 exe 与正在运行的游戏不参与这些临时副本写入。

## 地址表与实现

下列 RVA 仅适用于上面的已验证 exe。运行时地址为 **模块实际基址 `B` + RVA**，并非固定使用首选 image base。工具按唯一锚点定位，并核对预期字节；新六处和旧五处已知修改归一后，还会严格校验原始 SHA-256。

### 六处当前显示分支

每处将两字节条件分支替换为 `90 90`，共 12 字节。放开上下文限制后仍然读取 UI 开关，因此 `hide` 保留代码时仍可关闭显示。定义见 [PatchCatalog.cs](<src/PatchCatalog.cs>)。

| 名称 | RVA | 文件偏移 | 原始 → 开启 | 用途 |
|---|---|---|---|---|
| `TerrainFrame` | `0x00187EF0` | `0x001874F0` | `74 09` → `90 90` | 每帧地形揭示 getter |
| `EntityDisplay` | `0x001E9701` | `0x001E8D01` | `74 0D` → `90 90` | 本地实体可见性缓存到显示掩码 |
| `VisibilityA` | `0x001874E8` | `0x00186AE8` | `74 09` → `90 90` | 本地 UI 可见性查询 |
| `VisibilityB` | `0x00187F42` | `0x00187542` | `74 0D` → `90 90` | 本地 UI 可见性查询 |
| `VisibilityC` | `0x001882E3` | `0x001878E3` | `74 0D` → `90 90` | 本地 UI 可见性查询 |
| `VisibilityD` | `0x00189E26` | `0x00189426` | `74 09` → `90 90` | 本地 UI 可见性查询 |

### 四个揭示值与对象定位

```text
M = read_u64(B + 0x06B02CC0)
R = read_u64(B + 0x06185F18)
D = read_u64(R + 0x50)
```

| 值 | 地址 | 开启 / 关闭 |
|---|---|---|
| UI 地形全揭示 | `B + 0x06A888A0` | `1` / `0` |
| UI 实体揭示 | `B + 0x06A888A1` | `1` / `0` |
| manager 全揭示镜像 | `M + 0x41` | `1` / `0` |
| 渲染数据全揭示 | `D + 0x5E9` | `1` / `0` |

[FowDisplay.cs](<src/FowDisplay.cs>) 读取 manager `0x48` 字节和 D `0x648` 字节，检查布尔形状并前后复查对象指针。`M+0x40`、`D+0x5E8` 是引擎维护的普通迷雾状态，另作布尔检查，不属于上表四个揭示值。

M、D 的已审阅构造路径均未赋虚表；当前校验依据分配大小、结构字段与指针稳定性，不将首 qword 当虚表。指针前后相同也不等于跨线程原子快照。

### 引擎持续更新链

```text
C964A0  每帧 FOW 纹理更新
  -> 187EE0  读取 UI 地形全揭示开关
  -> C96650  提交渲染任务，并将 M+0x41 设置为传入值
  -> 1045700 渲染任务将 D+0x5E9 设置为该值
```

这解释了早期仅写 D 在暂停中有效、恢复运行后又被覆盖的现象。当前 `TerrainFrame` 分支让每帧读取遵循 UI 开关，渲染状态由该链持续维护。

实体路径在 `0x001E96E0` 结合可见性缓存与 UI 实体开关，再调用 `0x001EA840` 更新本地显示掩码。它仍保留其他缓存条件；本次没有穷尽所有实体类别和所有 UI 查询调用者。

### 旧五处仅作恢复

| 旧名称 | RVA | 原修改长度 |
|---|---|---|
| `CheatGate` | `0x03908BAE` | 17 字节 |
| `RevealAllGate` | `0x0390920C` | 2 字节 |
| `DoCommandTrust` | `0x004820FD` | 2 字节 |
| `TelemetryUsedCheats` | `0x001D8F81` | 4 字节 |
| `TelemetryDebugUsed` | `0x001D8FAA` | 4 字节 |

这些位置由 `BuildLegacy()` 识别，当前 `reveal` / `apply` 清理已有旧修改，`revert` 一并恢复。遥测两处恢复为原代码与应用遥测抑制补丁是不同操作；当前默认路线不启用后者。

## 技术证据与旧结论更正

完整地址、原始字节、推导及限制见 [静态诊断报告](<tools/reverse/console_diag_report.md>) 与 [机器清单](<tools/reverse/console_diag_manifest.json>)。报告中的生产代码审阅注明了快照时间，当前使用流程以本 README 为准。

- sim 的 `PCMD 0x83` 仍有玩家 ID、对象 `+0x48==0xFF`、`+0x60!=0` 三重检查，以及组件存在/启用条件。当前显示功能不走这条命令链。
- `0x033A1C68` 同时更新 CRC 并写入真实 FOW 状态：`0x033A1CA6` 的 `C6 44 38 6C 01` 将组件的玩家揭示字节置 1。早期“只有 CRC 记账”的解释已被此证据修正。
- 已定位的 `GE_PlayerCheat` UI/统计订阅者负责通知与使用统计；将该 UI 事件视作实际揭雾处理器的旧解释已修正。
- 无 `-dev` 的菜单 Global 控制台已观察到可打开；SCAR 子模式有单独的 dev flag 注册条件。对局内 Global 热键阻断原因未完整定位，当前使用流程不依赖它。

REA 采用 **6.8.0 + Ghidra 12.1.4 / JDK 25**，结合 Capstone、PE 节表与 `.pdata`，在保持原 RVA 的迷你 PE 上复核。有效的三份当前伪代码为：

- [每帧渲染更新](<tools/reverse/console_diag_tick_rw_decompiled.c>)
- [实体显示](<tools/reverse/console_diag_entity_rw_decompiled.c>)
- [上下文条件](<tools/reverse/console_diag_context_rw_decompiled.c>)

复核样本保留原节的读写属性、相关 BSS 映射和冷分支。早期把可写全局页重建为只读会造成错误常量折叠，缺失冷分支也会截断控制流；这些旧产物不作为当前证明。所有伪代码仍需与原始字节交叉核对。

## 构建与交付

运行环境为 Windows x64。**自包含发行版无需预装 .NET**；从源码构建需要 .NET 9 SDK。

### 2.1 GUI + 授权（当前发行）

```powershell
.\build.ps1 -Publish -PublicConfiguration ..\Licensing\.local\client-public.json
```

两段式流水线：普通 stage 发布 → [build/ProtectAssemblies.ps1](<build/ProtectAssemblies.ps1>) 用 Obfuscar 2.2.50（`dotnet tool` 清单固定）混淆 `CoH3FowTool.dll` 与 `Licensing.Client.dll`（三个 System.Text.Json DTO 用 `SkipType` 保参数名）→ [build/ProtectedBundle.targets](<build/ProtectedBundle.targets>) 在 `PrepareForBundle` 后把 bundle 内两文件替换为混淆版 → `--no-build` 单文件发布到 `publish-gui\`。原 `publish\`（2.0）拒绝覆盖；每次运行留档 `artifacts\gui-build\<时间戳>\` 并写 `build-report.json`。

交付产物：

```text
publish-gui/CoH3FowTool.exe        # 2.1 GUI 单文件（内嵌混淆程序集）
dist/CoH3FowTool-2.1-win-x64.zip   # 用户包（exe + 使用说明.txt）
dist/CoH3FowTool-2.0-win-x64.zip   # 2.0 全量包（源码+报告，保留）
```

发行 EXE 的内嵌内容已核验：[build/scan_bundle_streams.py](<build/scan_bundle_streams.py>) 在每个字节偏移尝试 raw-deflate 解压，205 条内嵌 PE 流中混淆版两个 DLL 精确命中（SHA-256 一致），未混淆版本无压缩流亦无原始字节。混淆客户端 DLL 与 `tests/ProtectedClient` 11/11 真实回归引用的完全同哈希。记录见 [artifacts/bundle-verification.md](<artifacts/bundle-verification.md>)。

### 2.0 命令行版（保留）

```powershell
dotnet publish .\CoH3FowTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish
```

开发构建输出到 `bin/Release/net9.0-windows/`。包内附有源码、逆向报告、验证记录和截图；自包含程序已在仅放置单 exe 的独立目录通过六项自检。

## 常见状态与恢复

| 现象 | 处理 |
|---|---|
| 找不到游戏或显示对象尚未建立 | 核对 `--exe`，进入战场并等待加载；再执行 `status` |
| 访问进程失败 | 以管理员身份运行，并确认工具与游戏对应同一 exe 路径 |
| 锚点、原始字节或归一摘要不匹配 | 保留完整输出与实际版本信息，等待重新适配该版本 |
| `hide` 后又自动开启 | 结束仍在运行的 `watch`，再执行 `hide` |
| 操作报错或对象正在切换 | 运行 `status` 核对当前实际状态；稳定进入战场后再关闭或还原 |
| 只想关闭画面效果 | `hide`；四个揭示值应为 `false`，六处代码可以继续保留 |
| 要恢复进程代码 | `revert`，随后用 `status` 核对；退出游戏也会结束该进程的内存修改 |
| 曾使用磁盘模式 | 游戏退出后执行 `file-revert`；核对恢复文件的大小和 SHA-256 |
| 激活失败 / 网络不可用 | 永久码激活那一次、限时码每次启动需联网；后端未启动时先跑 `..\Licensing\deployment\Start-Local.ps1` |
| 换电脑使用 | 授权码绑定首启设备，一次性兑换；另发新码，旧机不自动作废 |
| 许可证异常但想关掉效果 | 关闭/还原/状态不受授权限制，直接操作即可 |

游戏更新后重新核对版本、唯一锚点、原始字节、调用/消费链及真实画面；找到相同字节模式本身不代表仍有相同语义。
