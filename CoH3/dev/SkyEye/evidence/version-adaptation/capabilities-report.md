# REA 与现有游戏 GUI 能力实测（2026-11-21）

## 范围

仅能力调查；没有启动/结束游戏，没有写游戏内存，没有注入，没有发送键鼠输入，没有修改源码/构建。唯一文件写入为本目录下的能力探针、报告、静态分析结果与游戏专窗截图。

已读取根 AGENTS.md、本仓库 game-trainer-development 技能、F:/WorkSpace/REA/README.md、F:/WorkSpace/REA/skills/reverse-engineer-anything/SKILL.md 及 references/runtime-observation.md。当前会话未暴露 REA MCP，使用既有 CLI，未运行 setup/update/install。

## 运行环境与命令结果

所有 REA 命令从 F:/WorkSpace/REA 调用 .\\rea-local.cmd。入口使用 .runtime/node-v24.22.3-win-x64/node.exe 与 scripts/rea.mjs。

| 实际命令 | 退出码与结果 | 结论 |
|---|---|---|
| `.\\rea-local.cmd --help` | 0；rea@6.8.0；CLI 命令目录含静态、capture-process、observe-native-ui | CLI 可运行，不等于所有能力均支持 Windows |
| `.\\rea-local.cmd doctor --json` | 1；healthy=false / environment_healthy=false；Ghidra 12.1.4、JDK 25.0.3、Windows native Job Object/private DACL/reparse-safe controls 均 healthy；Hopper/IDA/JADX/Kuna 等缺失，MCP/skill registrations 报 missing/config_drift | 整体非健康不代表现有静态功能失效；未实施配置修复 |
| `.\\rea-local.cmd inspect-pe-imports "G:\\SteamLibrary\\steamapps\\common\\Company of Heroes 3\\RelicCoH3.exe" --max-file-bytes 536870912 --json` | 0；rea-pe-imports 静态解析成功；完整 Evidence 保存为 capabilities-rea-pe-imports.json | 当前游戏文件的静态解析实测通过；不证明动态绑定/可达性 |
| `.\\rea-local.cmd observe-native-ui "G:\\SteamLibrary\\steamapps\\common\\Company of Heroes 3\\RelicCoH3.exe" --pid 11192 --window-id 4982018 --json` | 1；capability_unavailable / unsupported_provider；`Native macOS utilities require macOS.` | REA 原生窗口观察在当前 Windows 不可用 |
| `.\\rea-local.cmd capture-process "F:\\WorkSpace\\Game\\CoH3\\dev\\SkyEye\\evidence\\version-adaptation\\capabilities-process-scenario.json" --json` | 1；capability_unavailable / unsupported_provider | 用无害 cmd.exe /d /c exit 0 场景探测；未启动游戏。错误明确为 Windows PTY descendant cleanup 未支持，不是缺少安装包 |
| `.\\rea-local.cmd capabilities --json --filter-output 'native_ui,process_capture,providers,capabilities'` | 0；native-macos 相关 groups unavailable，unsupported_host；managed static groups available | 与上述实际错误一致 |
| `C:\\Program Files\\Python311\\python.exe capabilities-gui-probe.py`（脚本完整路径在本目录） | 0；Pillow + pywin32 PrintWindow(hwnd,2) 成功，返回1；已生成并人工查看游戏专窗截图 | Windows GUI 截图替代路线已实测，不依赖 REA |

capture-process 原始错误：

```text
Windows PTY process capture is unavailable because this adapter does not yet verify descendant cleanup. Use Linux (including WSL for Linux commands) or macOS. Reinstalling the PTY backend does not enable native Windows capture.
```

## 静态样本身份

- 文件：G:/SteamLibrary/steamapps/common/Company of Heroes 3/RelicCoH3.exe
- 字节数：108391096。
- SHA256：8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec。
- PE32+ / 64-bit / amd64。
- Evidence ID：ev_cba2cd5ace968d5a266558e4d5717332ce096d2a2d0b05aca8ecc5a393956f54。
- 这是文件身份与导入表证据；未通过该命令验证任何游戏对象、结构、实时值或功能效果。

## 游戏窗口与 GUI 能力

被动 Get-Process 与窗口查询：

- PID 11192，进程名 RelicCoH3，Responding=True。
- HWND 4982018，标题 `Company of Heroes 3 -print_turn_action_log_continuously -turn_soft_lock_detection_seconds 60`。
- 窗口可见、非最小化；查询时不是前台（foregroundHwnd=3607402）。
- 外框 (-1219,157)-(717,1276)，尺寸1936×1119。存在负屏幕坐标，后续禁止假设主屏正坐标。
- PrintWindow 成功获得真实游戏画面，截图显示局内时间11:13，并明确显示“战术暂停已启用”；因此这是已有暂停对局，不是大厅。不应结束或另起一局覆盖现场。

Python 模块：PIL、win32gui 可用；pyautogui、pywinauto、mss、pydirectinput 缺失。ctypes 能找到 user32.SendInput、SetForegroundWindow、GetClientRect 导出；这些输入/前台API本轮均未调用。

## 推荐现有 GUI 控制办法与限制

1. 截图直接使用当前 capabilities-gui-probe.py 路线：每次先重新确认 HWND 所属 PID，再 PrintWindow 精确捕获目标窗口；不要抓整个桌面。
2. 若主代理进行已获用户确认的 GUI 操作，可用既有 ctypes/Win32 + pywin32 实现：GetClientRect/ClientToScreen 获取当前客户区坐标，SetForegroundWindow 后确认实际前台 HWND/PID，再 SendInput 发送单次已声明操作，随后截图复核。多屏绝对鼠标定位应按虚拟桌面矩形处理，包含负坐标。
3. API 导出存在不等于操作已成功。前台切换可能受 Windows focus 规则影响；游戏是否接受合成输入、键位/鼠标坐标是否正确、本机输入完整性级别是否一致均未实测。本轮不把输入能力标为通过。
4. 保留当前战术暂停现场。首次操作建议由用户确认或仅实施已声明的单步菜单动作；若 focus/input 复核失败，用户接管具体点击即可，不必安装第三方库或修复 REA macOS/PTY功能。
5. agent-browser 仅浏览器自动化，不是原生 DirectX 游戏控制器。当前任务无需为截图与基本Win32输入安装新包。
6. 不推荐为只读测量而改走 Frida：其脚本在目标进程执行，超出本轮只读能力探测；REA静态/Ghidra健康也不能代替实际内存和对局验证。

## 独立产物

- capabilities-process-scenario.json：无害退出场景，只用于验证 Windows PTY 能力。
- capabilities-gui-probe.py：被动专窗截图和库/API可见性探针；不激活窗口、不发输入。
- capabilities-gui-probe.json：窗口、库、截图返回值记录。
- capabilities-game-window.png：已存在暂停对局的专窗截图。
- capabilities-rea-pe-imports.json：当前游戏文件的完整静态导入表 Evidence。

本轮未验证：Ghidra对当前大文件的完整项目导入/反编译；REA动态附加；键鼠输入效果；游戏内结构定位和跨局功能。这些不能由doctor/截图/导入表成功外推。
