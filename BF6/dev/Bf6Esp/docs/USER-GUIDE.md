# Bf6Esp 用户手册

> Battlefield 6 外部只读 ESP 覆盖层（人物框 / 血条 / 距离 / 名字）。
> 交付目录：`BF6/output/Bf6Esp/`

---

## 1. 交付内容

| 文件 | 说明 |
| --- | --- |
| `Bf6Esp.exe` | 主程序（.NET Framework 4.8 / x64）。双击启动图形界面 |
| `Bf6Esp.exe.config` | 运行时配置 |
| `overlay-settings.txt` | 覆盖层设置（与 exe 同目录，可直接编辑） |
| `profiles/bf6-1.0.0.0.json` | 当前游戏 build 的偏移档案（含 build 哈希绑定） |
| `BUILD-INFO.json` | 构建时间、源文件清单与 SHA-256、目标游戏 build 哈希 |

**不需要**管理员权限启动程序本体；**不需要**任何驱动、注入或补丁。

---

## 2. 系统要求

- Windows 10 / 11 x64（Windows 7 需 .NET Framework 4.8）
- .NET Framework 4.8（Win10 1903+ 自带）
- Battlefield 6（Steam 版，`bf6.exe`；单机版 `SP\bf6.exe` 为不同 build，当前档案未覆盖）
- 已授权（见第 4 节）

---

## 3. 快速上手

1. 启动 Battlefield 6，进入一局对局（**必须进对局**：大厅里没有实体，叠加层不会有内容）。
2. 双击 `Bf6Esp.exe`。
3. 输入授权码完成激活（授权码由授权服务签发）。
4. 主窗口显示「运行中」后，按设置里的热键（默认 `End`）开关覆盖层。
5. 覆盖层会叠加在游戏窗口上：敌方为红框、友方为绿框、中立为黄框。

> 覆盖层是**独立进程的独立窗口**，游戏客户端里不会出现任何第三方模块。

---

## 4. 授权激活

- 授权状态由 `LicenseGate60` 决定；**未授权时覆盖层不启动、也不允许读游戏内存**（诊断命令不受影响）。
- 凭据保存在 `%LOCALAPPDATA%\Bf6Esp\SelfHostedLicensing`（DPAPI 保护），换机器需要重新激活。
- 界面会显示中文状态：未授权 / 已授权 / 已过期 / 设备不匹配 / 使用中 / 需要更新。

---

## 5. 命令行诊断

| 命令 | 作用 | 退出码 |
| --- | --- | --- |
| `Bf6Esp.exe` | 启动图形界面（激活 + 覆盖层） | 0 |
| `Bf6Esp.exe --probe` | 快速探测：游戏是否运行、档案是否匹配、签名命中数、读取失败次数 | 恒 0 |
| `Bf6Esp.exe --calibrate` | 只读校准：模块信息 + 签名命中 + 启发式发现 + 运行时反射 + 运行时布局 | 3 = 游戏未运行 |
| `Bf6Esp.exe --calibrate --out D:\find.json` | 指定校准报告输出路径 | 同上 |
| `Bf6Esp.exe --selftest` | 只读通道自检（读自身进程 PE 头 + 256 字节往返比对） | 失败为 1 |
| `Bf6Esp.exe --selftest-reflect` | 反射解析器自检（合成夹具，字段名→偏移还原） | 失败为 1 |
| `Bf6Esp.exe --selftest-locate` | 运行时定位器自检（对自身进程跑完整定位流程） | 失败为 1 |
| `Bf6Esp.exe --selftest-entity` | 实体字段推断自检（三种布局：连续 float / 列主序矩阵 / ECS 组件指针） | 失败为 1 |
| `Bf6Esp.exe --help` | 帮助 | 0 |

退出码总表：`0` 正常 / `2` 参数错误 / `3` 校准时游戏未运行 / `4` 无法只读打开进程 / `5` PE 头解析失败 / `6` 报告写入失败。

> 所有诊断命令**只读**：句柄权限固定为 `PROCESS_VM_READ | PROCESS_QUERY_LIMITED_INFORMATION`，程序内不存在任何写入游戏内存的 API。

---

## 6. 设置项（`overlay-settings.txt`）

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

改完保存即生效（下次启动读取）。

---

## 7. 常见问题

**Q：启动了但没有方框。**
按顺序排查：

1. `--probe` 看「游戏进程」是否找到（只认 `bf6.exe` / `bf6Trial.exe`）；
2. 是否**进了对局**（大厅/主菜单没有实体）；
3. 是否处于**观战/加载中**（实体数组为空）；
4. `--selftest-locate` 是否 0（定位器自身是否正常）；
5. 窗口是否被拉伸成非 16:9（投影矩阵按视口宽高比打分，比例异常时可能选错矩阵）。

**Q：方框位置跳动 / 与人物错位。**
说明投影矩阵或实体位置被误判。走第 8 节的取证流程回传校准报告。

**Q：把队友也框成敌人。**
`only_enemies=true` 依赖队伍字段；队伍推断失败时会退化为全显示。可在设置里先关 `only_enemies` 观察颜色是否分阵营。

**Q：游戏更新后失效。**
这是设计内的：档案按 build 哈希绑定，偏移不写死在代码里。按第 8 节重新取证即可，通常不需要改代码。

**Q：需要管理员权限吗？**
程序本体不需要。只有当游戏以管理员身份运行时，普通权限的进程读不到它的内存——此时用同等权限的会话运行本程序即可。

**Q：会被反作弊发现吗？**
本工具不做注入、不写内存、不改文件、不加载驱动（见 `docs/ARCHITECTURE.md` §2）。是否使用、以及由此产生的一切后果，由使用者自行判断。

---

## 8. 一次性取证步骤（维护者 / 反馈问题用）

1. 以**与游戏相同权限**的会话启动游戏并进入对局；
2. 执行 `Bf6Esp.exe --calibrate --out %USERPROFILE%\Desktop\bf6-calib.json`；
3. 把生成的 `bf6-calib.json` 回传（里面只有内存读取结果与元数据，无任何写入、无个人数据）；
4. 依据报告里的 `resolved`（实体列表 / 投影矩阵 / 本地玩家地址）与 `reflectionFields`（字段名→偏移）固化偏移，再进游戏复验。

---

## 9. 目录与依赖

- 依赖 .NET Framework 4.8（系统自带）。
- 不依赖任何第三方运行时、不需要 Python、不需要管理员、不需要驱动。
- 设置与凭据位置：`overlay-settings.txt`（exe 同目录）、`%LOCALAPPDATA%\Bf6Esp\`（授权凭据）。
