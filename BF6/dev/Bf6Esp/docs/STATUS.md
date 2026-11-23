# Bf6Esp 状态表

> 记录时间：2026-11-22（UTC+8）
> 目标游戏：Battlefield 6 / `bf6.exe` / BUILD-ID `BF6S-1.0.0.0-1F385AD9`
> 目标 build SHA-256：`1F385AD9AF30B8BA7F0C87CEEE92182710BB58F98708A2DE7F8D119042BAEFF4`
> 工具线：`BF6/dev/Bf6Esp/`　交付物：`BF6/output/Bf6Esp/`

---

## 0. 判定口径

本项目对每项能力分别给出三个独立结论，**不合并、不外推**：

| 结论 | 含义 | 判据 |
| --- | --- | --- |
| **读取成功** | 只读通道确实能读到目标进程的内存 | 能在目标进程上取回已知内容并比对一致 |
| **定位成功** | 目标数据（偏移/特征码）能唯一定位 | 偏移唯一命中且语义经交叉验证 |
| **实际游戏生效** | 在真实对局中表现正确 | 进真实对局后功能可见且数据正确 |

> **本沙箱内游戏无法启动**，因此「实际游戏生效」一律为「未验证」。
> 原因链（已逐条实测）：`bf6.exe` 直接启动 5 秒内退出，退出码 `-2147483645 = 0x80000003 (STATUS_BREAKPOINT)`；
> `EAAntiCheatService` 服务存在但处于 Stopped，当前会话 `elevated=False`，`Start-Service` 失败；
> `steam://rungameid/2807960` 拉起时 Steam 日志报 `Failed to start SteamService.exe ... (GLE 1223)`（用户未取消 UAC）。
> 即：**非管理员 + 未登录 EA/Steam 会话 + 无交互桌面**。
> 需要目标进程的结论（定位成功 / 实际游戏生效）只能标「未验证」，并在第 6 节给出用户侧一次性取证步骤。

---

## 1. 总览

| 能力 | 读取成功 | 定位成功 | 实际游戏生效 |
| --- | --- | --- | --- |
| 只读句柄（`PROCESS_VM_READ` + `PROCESS_QUERY_LIMITED_INFORMATION`） | ✅ 通道可用（自身进程等价验证） | ✅ 不涉及 | ❌ 未验证 |
| ReadProcessMemory 读取 | ✅ 通道可用（256 字节已知内容往返一致，失败计数 0） | ✅ 不涉及 | ❌ 未验证 |
| PE 头 / 节表解析 | ✅ 通道可用（自身模块 MZ + PE32+ 头解析正确） | ✅ 本 build 的 13 个节已从磁盘实测核对 | ❌ 未验证 |
| AOB 签名扫描（`??`/`?` 通配、按节扫描） | ✅ 代码就绪 | ✅ 档案已配 17 条真实锚点（含全文件唯一命中 1 次的 `RIFXRIFF`） | ❌ 未验证 |
| 运行时反射解析（MEC 式 TypeInfo/FieldInfo） | ✅ 代码就绪（合成夹具 7/7 字段、2/2 类型命中） | ❌ 未在真实进程上跑过（需活进程） | ❌ 未验证 |
| 实体数组定位（`.data` 里 `{指针,数量,容量}` 容器） | ✅ 代码就绪 | ❌ 未在真实进程上跑过 | ❌ 未验证 |
| 实体字段偏移统计推断（血量/队伍/位置/姓名，支持组件指针） | ✅ 代码就绪（合成夹具 3 种布局 36/36 命中） | ❌ 未在真实进程上跑过 | ❌ 未验证 |
| 投影矩阵定位 | ✅ 代码就绪 | ❌ 静态扫描结论：exe 内无可用矩阵，运行时才算 | ❌ 未验证 |
| 本地玩家 / 阵营 | ✅ 代码就绪 | ❌ 未在真实进程上跑过 | ❌ 未验证 |
| 世界坐标 → 屏幕投影 | ✅ 通道可用 | ✅ 数学实现完成（含相机背后/出屏判定，行/列向量两种约定自动判别） | ❌ 未验证 |
| 覆盖层窗口（分层透明 + 跟随前台窗口） | ✅ 代码就绪 | ✅ 不涉及 | ❌ 未验证 |
| ESP 绘制（方框/血条/距离/名字/下拉线/FOV 圈/离屏箭头） | ✅ 代码就绪 | ✅ 不涉及 | ❌ 未验证 |
| 设置持久化 + 全局热键 | ✅ 代码就绪 | ✅ 不涉及 | ❌ 未验证 |
| 授权接入（`ProductId=Bf6Esp`，公钥 pin 不变） | ✅ 服务端已注册并验证 | ✅ `/health` = ready，`/api/v1/version` 返回 `product_id: Bf6Esp` | ❌ 未做联机回归 |

---

## 2. 三个结论的证据

### 2.1 读取成功 — 有证据（等价路径）

`Bf6Esp.exe --selftest` 用**与正式流程完全相同**的只读路径（只申请 `PROCESS_VM_READ | PROCESS_QUERY_LIMITED_INFORMATION`）读取自身进程：

```
[成功] 只读句柄已取得（仅 PROCESS_VM_READ | PROCESS_QUERY_LIMITED_INFORMATION）。
[成功] DOS 头魔数校验（MZ）。
[成功] PE 头解析：ImageBase=0x140000000 SizeOfImage=0x20000 节数=2（PE32+）
[成功] 已知内容往返比对（地址 0x26C1752D0A8，256 字节）。
[信息] 读取失败计数：0
=== 自检通过：只读通道可用 ===
```

这验证的是**读取通道本身**（句柄申请、ReadProcessMemory 封装、PE 解析走同一条代码路径），
**不等于**已在 `bf6.exe` 上验证过 —— 这一点仍记为「未验证」。

### 2.2 定位成功 — 静态部分达成，动态部分未验证

#### 2.2.1 静态侧：已定位的部分

| 对象 | 结论 | 证据 |
| --- | --- | --- |
| PE 节表（13 个节的 RVA / 虚拟大小 / 原始大小） | ✅ 已实测 | `analysis/p14_profile_data.json`（imageBase 0x140000000，fileSize 199,230,312） |
| `.data` 模块注册表 magic `RIFXRIFF` | ✅ 全文件唯一命中（1 次），RVA `0x9AF49A0` | `analysis/p14_profile_data.json` |
| `typeinfo` 段记录标记 `u32 0x00080089` | ✅ 命中 23,701 次，与记录条数一致 | `analysis/p14_profile_data.json` |
| 明文反射名（类型名/属性名）的 RVA | ✅ 已提取 40+ 条（`ClientSoldierEntity` 1 次命中、`CurrentTeamId` 1 次、`OwningTeam` 1 次、`positionZ` 1 次等） | `analysis/p13_names.json`、`analysis/p14_profile_data.json` |
| 档案 `bf6-1.0.0.0.json` | ✅ 已回填真实段表 + 17 条签名 + 15 条 RVA 条目 | `profiles/bf6-1.0.0.0.json` |

#### 2.2.2 静态侧：已排除的路线（有价值的负结论）

对 `bf6.exe`（199 MB）做穷尽式静态分析后确认：

1. **成员名在磁盘上不存在**。exe 内有类型名（23,701 条类型记录）、有成员偏移（`fieldinf` 段内 14,815 个偏移小整数），
   但没有成员名——扫 13,672 个名字指针表找「名字 + 偏移」配对，命中 **0**。
2. **类型名不是名字摘要**。`typeinfo` 记录 +0x00 的 32 位 key 对 2499 条已验证锚点试过约 70 种摘要
   （djb2 add/xor 扫种子 0..2²⁰、fnv1a、sdbm、jenkins oat、crc32、murmur2/3、xxh32/64、siphash24、36 乘数 × 19 初值 × 4 运算序 × 11 名形）
   **全部 0 命中**；并给出结构性证明（ADD 族同末字节名的 key 差 gcd=1 ⇒ 任何 M>1 不可能；XOR 族违反「低 8 位相同」⇒ 整个 32 位多项式哈希族被排除）。
   进一步实测：exe 里**没有任何代码写静态 typeinfo 记录**（286,731 条 store 型 RIP 引用里，「目标 +4 == 标记 0x00080089」命中 0/3590、0/3174）
   ⇒ **key 是构建期烘焙值**，不存在可逆向的运行时 hash 例程。
3. **`.vlizer` 不是加密数据**（11.2 MB，块熵 5.15–7.99，1626 个首字节为 `E9` 的跳转桩）而是**代码**；压缩/加密假设被推翻。
4. **`Data\layout.toc` 主体是高熵（6.549）加密/压缩数据**，zlib/gzip/xz/zstd/lz4/bzip2 魔数与单字节 XOR 后 zlib 全部失败；
   `.cas` 数据包（1.56 GB 实扫）里同样 0 命中成员名。
   ⇒ **推论：Frostbite 的成员名最可能就在 `layout.toc` 里，游戏启动时解析后放进进程内存。**
5. **静态数据里没有投影/视图矩阵**（滑动窗口扫 `.data`/`.rdata`/`ctr`/`.rodata` 全是重定位噪声）⇒ 矩阵只能在运行时算出来。

#### 2.2.3 动态侧：已实现、待实机验证

因为「按名取偏移」在 BF6 上不成立，运行时定位改成**不依赖名字**的三条路：

1. **运行时反射解析**（`client/Esp/RuntimeReflection60.cs`）：按 MEC 布局扫活进程内存里的 TypeInfo/FieldInfo 链
   （info 头 `+0x00 name*` / `+0x08 flags` / `+0x0A size` / `+0x18 align` / `+0x1A fieldCount` / `+0x20 super*` / `+0x28|+0x30 fieldArray*`，
   FieldInfo 0x18 字节 = `+0x00 name*` / `+0x08 flags` / `+0x0A offset` / `+0x10 type*`），并有「描述符数组」兜底通道。
2. **实体容器启发式**（`RuntimeLocator60.FindEntityArrays`）：在 `.data` 里找 `{指针, 数量, 容量}` 三元组容器。
3. **字段统计推断**（`EntityFieldProfiler60`）：**不靠名字**，靠多实体数值分布反推——
   血量=取值∈[0.5,1000] 且多数实体取同一个值、存在另一个恒定槽作最大血量；
   队伍=int∈[0,64] 且取值种类≤8（非全零）；位置=连续三 float 有限且跨实体至少两轴变化，
   **或**落在 4×4 变换矩阵的平移分量（行主序 [12,13,14] / 列主序 [3,7,11]）；
   姓名=8 字节对齐处的指针指向可打印 ASCII。
   并且支持 **Frostbite ECS 组件布局**：实体里只存组件指针时，位置/血量分别从变换组件、血量组件对象里取（组件指针槽 + 内层偏移）。

合成夹具自检（`--selftest-reflect` / `--selftest-locate` / `--selftest-entity`）全部通过，
但**尚未在真实 `bf6.exe` 上跑过**，故「定位成功」在动态侧记为**未验证**。

### 2.3 实际游戏生效 — 未验证

没有任何在真实对局中的观测记录。GUI 可以启动，覆盖层窗口、绘制、投影等逻辑**尚未在游戏画面上叠加过**。

---

## 3. 已完成 / 未完成清单

### 已完成

- [x] `client/` 完整工程：net48 / x64 / WinExe / 无外部包 / 零错误编译（22 个 CS0649 警告均为 DataContract POCO 字段，属预期）
- [x] 只读进程与内存层（句柄、读取、PE 解析、AOB 扫描、区间枚举、失败计数）
- [x] 档案层（POCO、JSON 读写、build 哈希匹配、签名回退解析、「版本不匹配」提示）+ 档案目录查找修复（大小写不敏感导致 `client\Profiles` 挡住真档案目录）
- [x] 随包档案 `profiles/bf6-1.0.0.0.json`：真实段表 + 17 条签名锚点 + 15 条 RVA 条目，**运行时目标留空（0 = 未知，不是偏移 0）**
- [x] 运行时反射解析器（`RuntimeReflection60`）+ 实体容器启发式 + 字段统计推断器（含组件指针寻址）
- [x] 运行时布局对象 `RuntimeLayout60`（字段偏移 + 组件指针槽）与 `RuntimeEntitySource`（内联/组件两种寻址统一解析）
- [x] ESP 数据层（实体快照、`IEntitySource` 抽象 + 三个实现、优雅降级）
- [x] 投影、渲染、覆盖层窗口、设置面板、全局热键
- [x] CLI：`--probe` / `--calibrate [--out]` / `--selftest` / `--selftest-reflect` / `--selftest-locate` / `--selftest-entity` / `--help`，退出码语义化
- [x] 授权栈移植与适配（`ProductId=Bf6Esp`，公钥 pin 与签名域保持不变，GUI 中文状态）
- [x] 授权系统已注册 `Bf6Esp`：服务端实例 `127.0.0.1:47836`，`/health`=ready、`/api/v1/version` 返回 `product_id: Bf6Esp`，与 CoH3 哈希空间隔离
- [x] `Build.ps1`（离线构建 + 发布 + `BUILD-INFO.json`）与中文成功/失败摘要
- [x] 文档：`ARCHITECTURE.md`、`STATUS.md`、`USER-GUIDE.md`

### 未完成（需要运行中的游戏 / 实机验证）

- [ ] 在真实 `bf6.exe` 上跑 `--calibrate`，产出 `profiles/calibration-<UTC>.json`（本沙箱做不到，见第 6 节）
- [ ] 用校准报告确认：实体数组地址、投影矩阵约定（行/列向量）、本地玩家与阵营字段
- [ ] 真实对局验证：方框位置、血量比例、距离、阵营配色、FOV 圈、离屏箭头
- [ ] 联机回归：激活 → 心跳 → 进入覆盖层（服务端已就绪，客户端未跑通）
- [ ] 真实签名模式串：目前 17 条锚点里 2 条是唯一定位锚点，其余为「名字字符串存在性」锚点；真正的功能定位依赖运行时推断

---

## 4. 验收记录（本次交付）

| 验收项 | 结果 | 说明 |
| --- | --- | --- |
| `dotnet build` 零错误 | ✅ | `已成功生成。0 个错误`（22 警告，均为 DataContract 字段 CS0649） |
| `Build.ps1` 产出 `Bf6Esp.exe` | ✅ | `F:\WorkSpace\Game\BF6\output\Bf6Esp\Bf6Esp.exe`（158,720 bytes）+ `BUILD-INFO.json`（源文件 71 个） |
| `--selftest` | ✅ | 退出码 0；只读通道可用 |
| `--selftest-reflect` | ✅ | 退出码 0；7/7 字段偏移命中、2/2 类型还原 |
| `--selftest-locate` | ✅ | 退出码 0；反射 11 类型/2 字段，定位流水线不抛异常 |
| `--selftest-entity` | ✅ | 退出码 0；3 种布局各 12/12 命中（位置 x/y/z + 组件槽、血量 + 组件槽、最大血量 + 组件槽、队伍 + 组件槽、姓名 + 组件槽） |
| `--probe` 干净退出 | ✅ | 退出码 0；游戏未运行 → 明确中文提示，版本校验与签名命中跳过 |
| `--calibrate`（游戏未运行） | ✅ | 退出码 3，中文提示「未检测到 Battlefield 6 进程…请先启动游戏后重试。」，无崩溃、不落盘 |
| 写入范围之外无改动 | ✅ | `BF6\original\` 冻结区未写入；`git` 无 commit / push |
| 交付包不含源码 / PDB / 私钥 | ✅ | `output\Bf6Esp` 只有 exe + config + 设置模板 + 档案 + BUILD-INFO.json |
| 三份文档齐备 | ✅ | `docs/ARCHITECTURE.md`（含运行时定位层数据流图 + §5 校准工作流 + §6 更新适配）、`docs/STATUS.md`（本表）、`docs/USER-GUIDE.md`（用户手册：上手 / 授权 / 命令行 / 设置 / FAQ / 取证步骤） |

### 命令行输出留档

```
$ Bf6Esp.exe --probe
=== Bf6Esp 探测模式（外部只读） ===
[信息] 授权状态：未授权（覆盖层不会启动；探测只读，不受影响）
[信息] 档案目录：F:\WorkSpace\Game\BF6\dev\Bf6Esp\profiles；档案数量：1
[信息] 游戏进程：未运行。未检测到 Battlefield 6 进程（bf6.exe / bf6Trial.exe）。
[信息] 版本校验：跳过（游戏未运行，无法计算当前 build 的 SHA-256）。
[信息] 签名命中：跳过（游戏未运行）。
=== 探测结束 ===
（退出码 0）

$ Bf6Esp.exe --calibrate
=== Bf6Esp 校准模式（外部只读） ===
[错误] 未检测到 Battlefield 6 进程（bf6.exe / bf6Trial.exe），请先启动游戏后重试。
（退出码 3）

$ Bf6Esp.exe --selftest-entity
--- 场景 A：位置为连续三个 float（内联） ---
[成功] 位置 x：期望 0x10，实际 0x10。 … （12/12）
--- 场景 B：位置在列主序 4x4 变换矩阵内（内联） ---
[成功] 位置 x：期望 0x5C，实际 0x5C。 … （12/12）
--- 场景 C：位置与血量都在组件对象里（实体只存指针） ---
[成功] 位置组件指针槽：期望 0x8，实际 0x8。
[成功] 血量组件指针槽：期望 0x10，实际 0x10。
=== 实体字段推断自检通过 ===
（退出码 0）
```

---

## 5. 已知限制

1. 本机 PowerShell 执行策略会拦截 `F:\WorkSpace\Game` 下的未签名脚本，需用 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File …\Build.ps1` 运行；
2. `Build.ps1` 会保留输出目录里已存在的 `overlay-settings.txt`（避免覆盖用户设置），首次发布才写默认模板；
3. `profiles/` 里没有随包的 `calibration-*.json`（那是运行 `--calibrate` 才生成的产物）；
4. 校准报告体积可能较大（启发式候选上千条），后续可在 `CalibrationRunner60` 中按命中数排序并截断；
5. 运行时字段推断依赖「同一偏移在所有实体上是同一种数据」这一前提；若 BF6 的实体表里混装了异构对象（载具 / 武器 / 道具），需要按实体类型过滤后再推断；
6. 实体数组启发式只扫 `.data` 段；若目标容器落在 `.rdata` 或堆上，需要扩扫范围（`RuntimeLocator60` 已预留堆扫描预算参数）。

---

## 6. 下一步：需要用户在本机执行的一次性取证

本沙箱非管理员 + 未登录 EA/Steam，无法拉起 `bf6.exe`（退出码 `0x80000003`）。请在**自己的交互式管理员会话**里：

1. 用管理员身份启动 Steam 并登录 → 启动 `Battlefield 6`，进一局（确保能看到其他玩家）；
2. 用**管理员** PowerShell 执行：
   ```powershell
   cd F:\WorkSpace\Game\BF6\output\Bf6Esp
   .\Bf6Esp.exe --calibrate
   ```
3. 把生成的 `profiles\calibration-*.json` 回传；
4. 依据报告把运行时定位结果固化进 `profiles/bf6-1.0.0.0.json` 的 `offsets` / `structs`（或直接固化为新的 build 档案），
   之后进游戏用覆盖层逐项验收（方框/血条/距离/阵营色）。

报告里最关键的四个字段：`resolved.EntityList` / `resolved.EntityCount` / `resolved.ViewProjectionMatrix` / `resolved.LocalPlayer`，
以及 `reflectionFields`（活进程里解析出的成员名与偏移表）。

普通用户的日常操作与常见问题见 `docs/USER-GUIDE.md`。
