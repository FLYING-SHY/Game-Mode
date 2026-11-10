# Game-Mode

游戏修改器单仓库：**一条通用授权系统 + 多个修改器产品**。

| 目录 | 角色 | 技术栈 |
| --- | --- | --- |
| [`Licensing/`](<Licensing/>) | **通用授权系统**（所有修改器共用）：授权码发码 / 兑换 / 设备绑定 / 限时到期 / 停用恢复 / 管理后台 / 客户端 SDK | .NET 9、ASP.NET Core Minimal API、SQLite、Keygen CE、RSA 签名 |
| [`CoH3FowTool/`](<CoH3FowTool/>) | 产品：Company of Heroes 3 本地显示工具（GUI + 授权接入） | .NET 9 / `net9.0-windows` / WinForms / x64 |
| [`docs/agents/`](<docs/agents/>) | Agent 工作流配置：issue tracker、triage 标签、domain docs 布局 | Markdown |

远程仓库：`git@github.com:FLYING-SHY/Game-Mode.git`

## 授权系统与产品的关系

授权系统完全独立于任何具体游戏：服务端、发码后台、客户端 SDK 都在 `Licensing/`，程序集与命名空间为 `Licensing.*`（不含产品前缀）。每个修改器以 **产品 id（`ProductId`）** 接入，彼此的授权码哈希空间、设备指纹空间与 Keygen 策略完全隔离。

```text
修改器 (CoH3FowTool/…)                     授权系统 (Licensing/…)
  LicenseClientOptions.ProductId  ──►  POST /api/licenses/activate { code, deviceId, productId }
  嵌入 productId 的发行配置        ──►  Products[] 产品目录 → Keygen product + permanent/timed policy
  本地验签（证书 metadata.product）  ◄──  CE 签名的 MACHINE FILE
```

当前登记产品：`CoH3FowTool`（见 `Licensing/.local/server.json` 的 `Products[]` 与 `Licensing/client/ProductIds.cs`）。

新增一个修改器接入同一套授权系统：

1. Keygen 侧建 product + permanent / timed 两条 policy；
2. `Licensing/.local/server.json` 增一条 `Products[]` 项（id、显示名、两条 policy id、码前缀）；
3. 修改器侧设置 `LicenseClientOptions.ProductId` 并在发行配置里带上 `productId`。

## 常用命令

```powershell
# 授权系统
dotnet build .\Licensing\tests\Licensing.Tests.csproj -c Release
dotnet run --project .\Licensing\tests\Licensing.Tests.csproj -c Release -- .\Licensing\.local\server.json

# 本地授权后端（需 Docker + 首次 Initialize-Local.ps1 引导 CE）
& .\Licensing\deployment\Initialize-Local.ps1
& .\Licensing\deployment\Start-Local.ps1

# 修改器
powershell -NoProfile -ExecutionPolicy Bypass -File .\CoH3FowTool\build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\CoH3FowTool\build.ps1 -Publish -PublicConfiguration ..\Licensing\.local\client-public.json
```

本机未安装 `pwsh`（PowerShell 7），Windows PowerShell 5.1 可直接运行上述脚本。

## 文档

- [`AGENTS.md`](<AGENTS.md>) — 仓库结构、构建命令、多产品模型、注意事项
- [`Licensing/README.md`](<Licensing/README.md>) — 授权系统用法、HTTP 接口、客户端 API、多产品模型
- [`Licensing/framework-evidence.md`](<Licensing/framework-evidence.md>) — 框架选型证据与真实集成检查记录
- [`CoH3FowTool/README.md`](<CoH3FowTool/README.md>) — CoH3 战争先锋修改器说明与发行流程
- [`CoH3FowTool/docs/licensing-gui-spec.md`](<CoH3FowTool/docs/licensing-gui-spec.md>) — 修改器 GUI 规格
- [`Licensing/docs/licensing-models-research.md`](<Licensing/docs/licensing-models-research.md>) — 通用授权模式调研

## 不入库内容

`bin/ obj/ dist/ publish/ publish-gui/ artifacts/`、`Licensing/.local/`（密钥、数据库、令牌）、`Licensing/.cache/`、`tools/**/out-*/`、`tools/reverse/*.png` —— 全部见 `.gitignore`。
