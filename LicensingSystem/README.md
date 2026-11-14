# LicensingSystem — 通用授权系统

跨游戏共用的授权服务端与私有状态，按 ProductId 隔离多产品；当前产品为 `CoH3FowTool`。游戏侧的接入代码在各游戏目录内（当前为 [CoH3/SkyEye](<../CoH3/SkyEye/README.md>)，客户端授权模块编译内嵌于游戏程序）。

## 目录

- [server/](<server/>)
  授权服务端源码（ASP.NET Core Minimal API + SQLite）：卡密、设备绑定、会话租约、RSA-3072/SHA256 响应签名、管理后台。
- [server-tests/](<server-tests/>)
  隔离测试（`Build.ps1 -VerifyServer` 共 120 项；数据库/密钥环使用临时副本）。
- [deployment/](<deployment/>)
  部署辅助：[Get-DotNet10.ps1](<deployment/Get-DotNet10.ps1>)（SDK 获取）、[New-DeploymentConfig.ps1](<deployment/New-DeploymentConfig.ps1>)（生成全新私有配置，默认监听 `http://127.0.0.1:47835`）、[Caddyfile](<deployment/Caddyfile>)（域名反代模板）。
- [.cache/](<.cache/>)
  `dotnet-10/`：构建与运行使用的 .NET SDK 10.0.404（含安装校验证明 `dotnet-10-installation.json`）；`packages/`：server / server-tests 的 NuGet 还原目标（`Directory.Build.props` 指定）。两者都是构建必需，不是可随意删除的缓存。
- [.local/](<.local/>)
  私有状态：`hardening-v2/` 当前签名服务的配置、数据库、签名密钥与测试夹具；另有历史授权数据库、管理员配置及 `retained-private-state/`。私有目录不进入任何交付包，也不入库。

## 构建、运行、管理

仓库根目录执行：

```powershell
# 构建服务端并运行全部 120 项测试
& .\LicensingSystem\Build.ps1 -VerifyServer

# 初始化/校验租约签名密钥（公钥固定在游戏客户端 PinnedLeaseKey60.cs，-PinFile 可覆盖）
& .\LicensingSystem\Initialize-Signing.ps1 -ConfigPath .\LicensingSystem\.local\hardening-v2\server.json

# 启动服务
& .\LicensingSystem\Start-Server.ps1 -ConfigPath .\LicensingSystem\.local\hardening-v2\server.json
```

管理入口 `http://127.0.0.1:47835/admin`，管理员令牌取自对应私有配置的 `AdminToken`；客户端不持有它。服务端发布包由 [Publish-Server.ps1](<Publish-Server.ps1>) 生成到 [delivery/server/](<delivery/server/>)（供 Server.zip 打包，包含本地化启动 wrapper 与 deployment 辅助脚本）。

签名字段、nonce 与规范串以游戏侧[签名契约](<../CoH3/SkyEye/signature-contract.md>)为准。

## 新增一个游戏

1. 服务端 `Products[]` 增加产品项（或按现有配置结构扩展），并在客户端设置 `LicenseClientOptions.ProductId`；
2. 产品 id 决定授权码哈希空间（`{productId}|code-v1|`）、设备指纹前缀（`{productId}|device-v1|`）与管理后台的卡密归属；
3. 每个游戏一个目录：游戏源码、客户端授权接入（内嵌编译）与该游戏的交付产物放在该游戏目录内，本目录只放授权系统本体。

## 备份

一起保留当前配置指定的 DatabasePath、整个 KeyDirectory 与 `server.json`。运行中的 SQLite 先正常停服或使用在线备份，避免遗漏 WAL 事务。Data Protection 密钥用于解密卡密；租约签名私钥用于匹配客户 EXE 内固定的公钥。新环境更换私钥时，需在游戏侧重新固定公钥并重新编译分发客户端。
