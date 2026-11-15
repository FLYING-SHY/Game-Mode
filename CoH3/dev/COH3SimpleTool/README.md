# COH3简易工具（SkyEye 简易版）

英雄连3 本地信息工具 · 极简黑色界面。引擎复用 `CoH3\dev\SkyEye` 同源内存读取引擎（只读，不写入游戏内存），授权接入 LicensingSystem 自有授权服务（产品 `CoH3FowTool`，与 SkyEye 卡密通用）。

## 界面

- 登录页：激活码 + 记住 + 进入（系统标题栏，红字错误行）。
- 功能页：824x118 悬浮控制条（默认置顶，可拖动）
  - 三个开关：顶部玩家信息 / 主画面标记 / 小地图标记（参数自动保存到 exe 旁 `overlay-settings.txt`）
  - 连接状态：`○ 等待对局` / `● 对局已连接` / `⚠ 版本未验证`（悬停显示授权到期）
  - `退出` 按钮返回登录页换码；`—` 最小化；`×` 关闭并下线
- 无热键、无声音、无设置面板：启动游戏后自动挂接。

## 目录

```
COH3SimpleTool/
  COH3SimpleTool.csproj        # AssemblyName=COH3简易工具, net48/x64/WinForms
  CoH3MemoryTool/              # 引擎（复制自 SkyEye，黑灰配色改造）
    SimpleProgram.cs           # 入口（单实例互斥）
    SimpleLicenseApp.cs        # 授权状态机（SkyEye LicenseApplication60 同源）
    BlackLoginForm.cs          # 登录窗（全新黑壳）
    ControlBarForm.cs          # 控制条（全新黑壳，含 Poll 轮询引擎接线）
  Properties/AssemblyInfo.cs   # 版本 0.60.8.0（与服务端更新检查一致）
  Build-Protected.ps1          # 构建 + Obfuscar 2.2.50 保护 + -Smoke 冒烟
  tools/obfuscar/              # 固定版本保护工具
  # 交付产物在 ../output/COH3SimpleTool/（受保护 EXE + .exe.config，Build-Protected.ps1 直写该目录）
  evidence/                    # 截图 / latest-build.json 等证据
  artifacts/                   # 每次构建日志与私有改名映射（勿分发）
```

## 构建与保护

```powershell
# 依赖 LicensingSystem\.cache\dotnet-10 SDK（仓库级共享）
powershell -NoProfile -ExecutionPolicy Bypass -File .\Build-Protected.ps1 -Smoke
```

- 授权公钥 pin 预检（RSA-3072 modulus/kid）失败即中止，防止构建出无法验证租约的版本。
- Obfuscar 规则与 SkyEye 同链：HideStrings/HidePrivateApi/重命名/优化；保留 `LicenseCredentials60` 字段名（DPAPI JSON 兼容）与 `ControlBarForm.ReleaseCapture60`。
- `evidence\latest-build.json` 记录输入/输出 SHA256；`artifacts\<stamp>\private-renaming-map.xml` 为私有映射，仅维护用。

## 授权与凭据

- 服务端：`LicensingSystem`（默认 `http://127.0.0.1:47835/`，见 app.config）。
- 凭据文件与 SkyEye 共享：`%LocalAppData%\Skyeye\SelfHostedLicensing\credentials.dat`（同 DPAPI entropy），SkyEye 记住过的激活码在本工具自动带入。
- 激活需游戏运行中（读取当前 Steam 身份），与 SkyEye 行为一致。

## 与 SkyEye 的差异

- 砍除：F8 复制 / F9 包抄标记（flank 链）、8 个 MP3 声音警报与 NAudio 依赖、六个设置模块、托盘、预览区。
- 保留：四层标记引擎（顶部玩家信息/地雷/战场/小地图/空袭，开关与偏好文件控制）、SLD 照明弹状态字提示、断线快速重试。

## 验证状态

- 离线：裸构建 0 错误；登录页/控制条像素级黑灰审查；受保护版与裸版登录页 0 像素差异（`evidence\ui-login-raw\`、`evidence\protected-smoke-*\`）。
- 真实游戏回归（2026-11-15，`evidence\live-regression3-20261115-091831\`）：保存凭据自动登录 ✓、退出隐藏控制条并下线 ✓、隔离测试卡贴码激活（设备+Steam 绑定、服务端 activate/logout 事件齐全）✓、开关持久化往返 ✓、× 优雅关闭并下线 ✓。测试卡回归后已停用；对局内实时覆盖渲染未验证（游戏停在大厅）。
- 汇总：`evidence\regression-status.json`。
