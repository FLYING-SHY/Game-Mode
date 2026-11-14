# 天穹之眼（SkyEye）

天穹之眼当前唯一维护版本：net48/x64 WinForms 客户端 + 编译内嵌的授权接入、验证工程与交付产物。授权系统本体（服务端/测试/SDK/私有状态）在独立目录 [LicensingSystem](<../../LicensingSystem/README.md>)；原版发布目录保持原样。目录布局于 2026-11-13 重构：授权代码独立为仓库根的 `LicensingSystem/`，本目录只保留此游戏的内容。

## 目录

- [client/](<client/>)：游戏客户端源码（含 `CoH3MemoryTool` 授权接入模块，`PinnedLeaseKey60.cs` 固定公钥、`LeaseSignature60.cs` 验签）。
- [verification/](<verification/>):独立验证器工程（自检、离线与联机七组回归的执行体源码）。
- [delivery/client/](<delivery/client/>):经实际回归混淆保护的可分发客户目录；不含源码、映射、验证器、PDB、私有配置或密钥。
- [授权服务端源码](<../../LicensingSystem/server/>)、[授权测试](<../../LicensingSystem/server-tests/>)：授权服务端源码与测试已移至授权系统目录（LicensingSystem/server、LicensingSystem/server-tests）。
- [tools/](<tools/>)、[scripts/](<scripts/>)：ILSpy（静态测量用）、回归辅助脚本。
- [evidence/](<evidence/>)、[reports/](<reports/>):回归证据、验收报告、清理与重构记录。

## 信任链

客户端每次激活和心跳发送新的 128 位随机 nonce。服务端用独立 RSA-3072 私钥，以 SHA-256 / PKCS#1 v1.5 对产品、设备、当前 Steam 身份、安装 GUID、nonce、会话令牌哈希、三个时间值、卡种和 kid 的规范串签名。客户端使用编译时固定的公钥验签，并在真实 Coordinator 放行 Gate 前再次要求验证标记。缺签名、错公钥、旧请求签名或签名字段被改动均不会产生新租约。

签名成功后仍执行原来的 `0 < delta <= 60` 校验、请求发起前单调时钟锚定、1 秒扣减及约 28 秒心跳。断网时只消耗已有租约，期限由服务端决定。签名保护服务端响应真实性；本地程序仍可能被修改，混淆用于增加修改和分析成本。

完整字节格式见 [signature-contract.md](signature-contract.md)。公钥不从版本接口获取，配置文件也不能替换公钥。服务端私钥和管理员令牌均不进入客户包或维护包。

## 构建和部署顺序

所有构建使用仓库内 `LicensingSystem/.cache/dotnet-10/dotnet.exe`（SDK 10）。独立部署先生成全新私有配置，或明确沿用已备份的同一签名密钥。

```powershell
# 1. 编译服务端并运行全部 120 项隔离测试
& .\LicensingSystem\Build.ps1 -VerifyServer

# 2. 创建私有配置；端口/域名按目标环境明确设置
& .\LicensingSystem\deployment\New-DeploymentConfig.ps1 -OutputFile C:\SkyEyePrivate\server.json -DataDirectory C:\SkyEyePrivate\data -ListenUrl http://127.0.0.1:47835

# 3. 离线初始化/复用私钥，只导出公钥并写入客户端编译期 pin（-PinFile 默认指向本游戏 pin）
& .\LicensingSystem\Initialize-Signing.ps1 -ConfigPath C:\SkyEyePrivate\server.json

# 4. 编译保护客户端并自检；随后执行七组链路回归
& .\CoH3\SkyEye\Build-Protected.ps1 -SelfCheck
& .\CoH3\SkyEye\Verify-Protected.ps1 -PrivateCodeDirectory <私有码目录>

# 5. 使用同一个私有配置运行签名服务
& .\LicensingSystem\Start-Server.ps1 -ConfigPath C:\SkyEyePrivate\server.json
```

更换服务端签名私钥后，必须重新固定公钥、构建、保护并分发客户端。只恢复数据库而丢失签名私钥会使旧客户端验签失败。备份数据库、整个 KeyDirectory 和私有配置；目标服务账户应独占这些目录的访问权限。Windows 本机验证使用当前用户、SYSTEM 和管理员 ACL；Linux 签名私钥设为 0600。跨账户部署需为实际服务账户明确设置权限。

默认客户端配置地址为 `http://127.0.0.1:47835/`；远程部署使用 HTTPS，并配置服务端 AllowedHosts 和实际反向代理来源。旧的 47831 无签名服务不会被此客户端降级接受。正式公网地址、证书、容器和第二台机器需要在相应环境完成验证。

## 发布区分

`delivery/client` 是经过本轮实际回归的可分发客户目录；不放源码、映射、验证器、PDB、私有配置或密钥。

.NET Reactor 试用工具、试用 EXE 和未混淆实验副本已清理，保留实验日志、测量报告及可配置的保护脚本。将来使用商业保护器时，从新的未混淆构建输入执行，并先通过 `-licensed` 检查。当前正式交付采用签名租约 + Obfuscar 保护。

回归基线：服务端 120/120；原始签名版、正式混淆版、Reactor 试验版各七组实际客户端回归；六次界面逐像素比较零差异；八个音频解码结果一致；布局重构后全量重验通过（客户端自检 + 三组离线 + 四组联机）。正式版仍能被 ILSpy 导出；详细测量与边界见 [验收报告](reports/protection-validation.md)。

## 最终产物与使用

- `SkyEye-HardenedV2-Client.zip`（CoH3 根）：分发客户的签名 + Obfuscar 客户端，不含源码或映射。
- `SkyEye-HardenedV2-Server.zip`：签名服务端发布目录（源/打包根在 `LicensingSystem/delivery/server`），不含本机私钥、数据库或管理员配置。
- `SkyEye-HardenedV2-Maintenance.zip`：自己保存的源码（含 LicensingSystem 授权源码）、开发映射、脚本及证据。Reactor Demo EXE 和第三方工具均排除。

本机后台位于 `http://127.0.0.1:47835/admin`，私有配置在 `LicensingSystem/.local/hardening-v2/server.json`。已编译的客户包固定了本机服务的公钥，需要与原签名密钥配对。新环境采用新密钥时，按前述顺序重新 pin、编译、保护后再分发客户包；单独生成新密钥不会自动适配旧客户 EXE。

发布服务使用 `delivery/server/Start-Server.ps1 -ConfigPath <私有配置>`；维护工程的可复现服务发布为 `LicensingSystem/Publish-Server.ps1 -NoBuild`；`Verify-Protected.ps1` 默认连接 47835，接收独立私有码目录执行七组回归。历史清理记录见 [cleanup-manifest.json](evidence/cleanup-manifest.json)，本次布局重构记录见 [refactor-manifest.json](evidence/refactor-manifest.json)。清理后可显式执行 `Finalize.py --refresh`，在保留原版、当前源码与客户包的校验通过后，原子刷新服务端和维护包；客户 ZIP 保持逐项一致。
