# LicensingSystem — 通用授权系统

跨游戏共用的授权服务端与私有状态，按 ProductId 隔离多产品；根产品为 `CoH3FowTool`（CoH3 天穹之眼与 COH3工具共用），战地 6 工具使用 `Bf6Esp`，账号绑定型产品 `ZeppPortal` 通过 `Products[]` 追加。游戏侧的接入代码在各游戏目录内（当前为 [CoH3/dev/SkyEye](<../CoH3/dev/SkyEye/README.md>)，客户端授权模块编译内嵌于游戏程序）。

## 目录

- [server/](<server/>)
  授权服务端源码（ASP.NET Core Minimal API + SQLite）：卡密、设备绑定、会话租约、RSA-3072/SHA256 响应签名、管理后台。
- [server-tests/](<server-tests/>)
  隔离测试（`Build.ps1 -VerifyServer` 共 267 项；数据库/密钥环使用临时副本）。
- [deployment/](<deployment/>)
  部署辅助：[Get-DotNet10.ps1](<deployment/Get-DotNet10.ps1>)（SDK 获取）、[New-DeploymentConfig.ps1](<deployment/New-DeploymentConfig.ps1>)（生成全新私有配置，默认监听 `http://127.0.0.1:47835`，可用 `-AllowedHosts` / `-TrustedProxies` 指定公网入口与反代来源）、[Caddyfile](<deployment/Caddyfile>)（域名反代模板；线上实例实际使用 nginx，配置见服务器上的 `/opt/licensing/DEPLOYMENT.md`）。
- [.cache/](<.cache/>)
  `dotnet-10/`：构建与运行使用的 .NET SDK 10.0.404（含安装校验证明 `dotnet-10-installation.json`）；`packages/`：server / server-tests 的 NuGet 还原目标（`Directory.Build.props` 指定）。两者都是构建必需，不是可随意删除的缓存。
- [.local/](<.local/>)
  私有状态：`hardening-v2/` 当前签名服务的配置、数据库、签名密钥与测试夹具；另有历史授权数据库、管理员配置及 `retained-private-state/`。私有目录不进入任何交付包，也不入库。

## 构建、运行、管理

仓库根目录执行：

```powershell
# 构建服务端并运行全部 267 项测试
& .\LicensingSystem\Build.ps1 -VerifyServer

# 初始化/校验租约签名密钥（公钥固定在游戏客户端 PinnedLeaseKey60.cs，-PinFile 可覆盖）
& .\LicensingSystem\Initialize-Signing.ps1 -ConfigPath .\LicensingSystem\.local\hardening-v2\server.json

# 启动服务
& .\LicensingSystem\Start-Server.ps1 -ConfigPath .\LicensingSystem\.local\hardening-v2\server.json
```

管理入口 `http://127.0.0.1:47835/admin`（本机），线上实例为 `https://124.220.147.56/admin`；管理员令牌取自对应私有配置的 `AdminToken`；客户端不持有它。服务端发布包由 [Publish-Server.ps1](<Publish-Server.ps1>) 生成到 [output/](<output/>)（供 Server.zip 打包，包含本地化启动 wrapper 与 deployment 辅助脚本）。

## 线上实例（124.220.147.56）

2026-11-16 起生产授权服务运行在该公网地址：应用只监听回环 `http://127.0.0.1:47835`，nginx 终结 TLS（Let's Encrypt shortlived IP 证书）后反代回环；`AllowedHosts` 含 `124.220.147.56`，`TrustedProxies` 为 `127.0.0.1`。systemd 单元 `licensing.service` 以独立账户 `licensing` 运行 `/opt/dotnet/dotnet /opt/licensing/Licensing.Server.dll`；私有配置 `/opt/licensing/server.local.json`，数据库与密钥目录 `/opt/licensing/data`。`/usr/local/bin/licensing-backup.sh` 每日 03:17 做 SQLite 在线备份并保留 14 份于 `/opt/licensing-backups`；acme.sh 定时续签短期证书。恢复或迁移时必须同时带走 `server.local.json`、`data/leases.db` 与 `data/keys` 中的租约签名私钥，否则旧客户端的公钥 pin 会验签失败。

签名字段、nonce 与规范串以游戏侧[签名契约](<../CoH3/dev/SkyEye/signature-contract.md>)为准。

## 多产品与授权模型

- **产品隔离**：`ServerOptions` 继承 `ProductOptions`（产品 id、名称、卡密前缀、数据库路径、密钥目录、版本与下载地址），根产品沿用顶层配置字段；`Products[]` 只填根产品之外的产品。启动时校验产品 id 唯一、数据库路径互异、密钥目录互不重叠且不与数据库路径嵌套，违反直接拒绝启动。
- **运行时分流**：[ProductCatalog.cs](<server/ProductCatalog.cs>) 按 ProductId 解析到对应 `LicenseService` 与 Data Protection 密钥环（Windows 上 DPAPI 加密密钥文件）；管理后台通过 `?productId=` 选定产品，未指定时落到根产品。
- **设备绑定型**（`CoH3FowTool`、`Bf6Esp`）：卡密绑定设备指纹，签发带签名的租约，激活 / 心跳 / 退出接口保持原样。
- **账号绑定型**（`ZeppPortal`，见 [LicenseService.Zepp.cs](<server/LicenseService.Zepp.cs>)）：卡密核销到十进制上游 UID，不签发设备租约；`POST /internal/zepp/entitlement` 查询账号授权，`POST /internal/zepp/redeem` 核销卡密（要求 36 位 UUID `request_id` 幂等；写锁串行化后事务内只允许一个赢家；时长卡顺延，永久会员与时长卡互斥）。这两个接口使用独立的 `ZeppBackendToken` Bearer 鉴权，该令牌不得等于管理员令牌。
- **测试**：[server-tests/ProductTests.cs](<server-tests/ProductTests.cs>) 覆盖产品隔离、路径校验、账号核销幂等与并发唯一赢家等场景，`Build.ps1 -VerifyServer` 共 267 项。

## 新增一个游戏

1. 服务端 `Products[]` 增加产品项（或按现有配置结构扩展），并在客户端设置 `LicenseClientOptions.ProductId`；
2. 产品 id 决定授权码哈希空间（`{productId}|code-v1|`）、设备指纹前缀（`{productId}|device-v1|`）与管理后台的卡密归属；
3. 每个游戏一个目录：游戏源码、客户端授权接入（内嵌编译）与该游戏的交付产物放在该游戏目录内，本目录只放授权系统本体。

## 备份

一起保留当前配置指定的 DatabasePath、整个 KeyDirectory 与 `server.json`。运行中的 SQLite 先正常停服或使用在线备份，避免遗漏 WAL 事务。Data Protection 密钥用于解密卡密；租约签名私钥用于匹配客户 EXE 内固定的公钥。新环境更换私钥时，需在游戏侧重新固定公钥并重新编译分发客户端。
