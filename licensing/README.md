# 本地授权服务与 .NET 客户端

本目录包含真实 Keygen CE 后端的本机部署、薄业务适配层、受保护管理员网页和 WinForms 可复用的 .NET 9 客户端。Keygen 负责策略、许可证、设备、停用、RSA 签名；SQLite 适配层只负责一次性兑换码、事务绑定和幂等恢复。不存在内存授权后端回退。

已在本机运行官方 CE 容器并完成 32 项真实 HTTP / SQLite 集成检查，以及管理员浏览器发码、停用、恢复、退出验收。第二台物理电脑、正式 HTTPS 部署和游戏运行验收另行进行；本目录测试未启动游戏或访问游戏进程。框架文档、实际版本差异和测试证据见 [框架证据](framework-evidence.md)。

## 文件与依赖

| 入口 | 用途 |
| --- | --- |
| [客户端项目](client/CoH3.Licensing.Client.csproj) | 纯 .NET 9 / 标准 RSA API；主程序只引用此项目 |
| [后端项目](server/CoH3.Licensing.Server.csproj) | ASP.NET Core、Microsoft.Data.Sqlite 9.0.17、Keygen JSON:API 适配 |
| [真实集成检查](tests/Program.cs) | 启动真实 loopback Kestrel，调用真实 CE API，使用独立 SQLite |
| [初始化脚本](deployment/Initialize-Local.ps1) | 创建本地随机凭据、初始化 CE、创建产品和两种策略、输出公钥配置 |
| [启动脚本](deployment/Start-Local.ps1) | 复用已有本地数据并启动容器和适配层 |
| [Compose 配置](deployment/compose.yaml) | 独立 `coh3-licensing-local` 项目；官方 CE 镜像锁定 digest |

需要 .NET 9 SDK、Docker CLI 和已正常运行的 Linux 容器引擎。实测 SDK 9.0.315 / runtime 9.0.17、Docker 29.6.2。脚本默认 Docker CLI 路径为 `C:\Program Files\Docker\Docker\resources\bin\docker.exe`，可通过 `-Docker` 参数覆盖。NuGet 缓存只写入本目录的 `.cache/packages`；脚本不修改全局环境变量或服务。

## 本机启动

在项目根目录执行：

```powershell
# 仅首次；已有 .local/server.json 时初始化脚本直接退出，保留现有账户/数据。
& .\licensing\deployment\Initialize-Local.ps1

# 复用数据启动；前台保持运行，Ctrl+C 停止 .NET 适配层。
& .\licensing\deployment\Start-Local.ps1
```

PowerShell 执行策略阻止脚本时，可用单次进程参数执行，不改变全局策略：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\licensing\deployment\Start-Local.ps1
```

本地地址：

- `http://127.0.0.1:47831/health`：实际查询 CE 策略；正常返回 `framework: Keygen CE, mode: real-api`。
- `http://127.0.0.1:47831/`：**管理页入口**，自动跳转到登录页 `/admin/login`（根路径以前是 404，已加跳转）。
- `http://127.0.0.1:47831/admin/login`：管理员登录页，需要管理员令牌。
- `http://127.0.0.1:47831/admin`：受保护管理入口，未登录转到登录页。
- `127.0.0.1:47830`：CE API，仅适配层及本地初始化使用；要求配置的 `Host: api.keygen.localhost`。

访问管理页前必须先让适配层在跑（上面的 Start-Local.ps1 保持窗口不关）。若提示连接被拒绝，说明进程没起或已退出，先跑 `Start-Local.ps1` 或访问 `/health` 确认。管理员令牌已另存到 `.local/admin-token.txt`（仅本机，供复制粘贴）。

Postgres / Redis 没有宿主端口；CE 的宿主绑定为 `127.0.0.1:47830:3000`，适配层只接受字面量 loopback IP。所有容器、网络和卷使用上述独立 Compose 项目前缀。停止本项目容器且保留数据：

```powershell
& 'C:\Program Files\Docker\Docker\resources\bin\docker.exe' compose -f .\licensing\deployment\compose.yaml stop
```

初始化创建 `.local/keygen.env`、`.local/postgres.env`、`.local/server.json`、`.local/client-public.json` 以及只运行一次 setup 的标记。这些运行数据均被 [忽略规则](.gitignore) 排除。CE setup 会重建数据库，因此脚本通过 `setup.complete` 阻止再次执行；保留该标记以及现有卷。

`server.json` 含管理令牌和受限于产品的 CE API token，只留在服务端。管理员在本地读取其中 `AdminToken` 字段（或直接复制 `.local/admin-token.txt` 的内容），在登录页口令框输入。管理员会话有效 4 小时、不自动续期，cookie 为 HttpOnly / SameSite=Strict，登录和所有管理写操作均验证防跨站请求 token，登录入口按 IP 限速。在 Windows 上，管理员 cookie 密钥使用当前账户的 DPAPI 加密。客户端只取得下述两个公开字段：

```json
{
  "serviceUri": "http://127.0.0.1:47831/",
  "verificationPublicKeyPem": "-----BEGIN RSA PUBLIC KEY-----\n...\n-----END RSA PUBLIC KEY-----"
}
```

此配置用于发行构建时嵌入服务地址和信任公钥。生产 GUI 固定嵌入的信任根，不从用户可编辑的环境变量或外部 JSON 接受替代根。客户端 DLL 没有私钥、管理 token 或产品 token。

## 约定与恢复行为

- **一码绑定一个设备**：随机 160 位兑换码仅生成时完整显示；数据库存 SHA256、码尾和记录。SQLite `BEGIN IMMEDIATE` 串行化兑换；CE strict / maxMachines=1 / NO_OVERAGE 再限制设备数。返回激活成功前先持久化设备、期限和完整原生证书。
- **丢响应与中断**：同设备重试永久码可逐字恢复已提交证书；其他设备返回 `AlreadyRedeemed`。若远端设备已创建而适配层进程在提交前中断，重试从真实 CE `machine.created` 恢复，保持最早设备及期限。不同设备也被远端记录阻止。
- **永久**：CE `license.expiry=null`，原生机器证书 `meta.ttl=null, meta.expiry=null`。首次激活联网成功并保存证书后，新启动只做本地验签、应用/授权种类检查、设备匹配，零 HTTP。已签发永久许可没有远程撤回能力。
- **限时**：从真实首次设备激活时间起算天数。适配层同步设定 `license.expiry=machine.created+days` 后再签发，避免 CE 原生首次激活到期更新的异步延迟。每个新进程启动都联网验证现有绑定，不再次消耗码；失联或停用时禁止新启用。运行中只用签名服务器时间加单调计时检查到期，没有七天缓存、定期联网或刷新任务。已运行实例的停用在下次启动联网校验时生效。
- **设备**：默认对 Windows 64 位注册表的 `MachineGuid` 做应用域分隔 SHA256，原值不上传。该标识对应 Windows 安装实例；系统重装可能改变，克隆安装可能相同，不表示硬件可信证明。真实第二台设备与重装场景尚待现场验收。
- **恢复备份**：兑换 SQLite、CE Postgres 卷和 CE 加密环境配置需要成套保留，才能恢复对应账户密钥和原设备记录；仅备份客户端证书不能重建服务器发码记录。永久客户端在后端故障时仍本地工作。

适配层的部署模型是一个 SQLite 数据文件与同一 CE 账户。使用多个适配层实例时必须共享同一个可靠的 SQLite 文件；独立副本数据库不构成受支持的横向扩容方案。激活时事务跨越 CE 网络调用，面向本项目的小规模人工发码使用。

## 客户端公共接口

定义见 [状态与选项](client/LicenseModels.cs) 和 [LicenseClient](client/LicenseClient.cs)：

```csharp
using CoH3.Licensing;

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
using var licensing = new LicenseClient(new LicenseClientOptions
{
    BaseUri = new Uri(EmbeddedServiceUri),
    VerificationPublicKeyPem = EmbeddedVerificationPublicKeyPem,
    LicenseFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CoH3FowTool", "license.lic")
}, http);

// 每个新 GUI 进程启动一次。永久本地，限时在线。
LicenseStatus startup = await licensing.ValidateStartupAsync(cancellationToken);

// 用户输入码时调用；成功后以临时文件 + 原子替换保存证书。
LicenseStatus activation = await licensing.ActivateAsync(code, cancellationToken);

// 启用前以及运行中的本地到期检查；此方法不访问网络。
LicenseStatus current = licensing.CheckCurrentLicense();
if (current.CanEnable) { /* 允许启用 */ }
```

完整构造函数：

```csharp
LicenseClient(LicenseClientOptions options, HttpClient httpClient,
    Func<string>? deviceFingerprint = null, TimeProvider? timeProvider = null);
Task<LicenseStatus> ActivateAsync(string code, CancellationToken ct = default);
Task<LicenseStatus> ValidateStartupAsync(CancellationToken ct = default);
LicenseStatus CheckCurrentLicense();
```

`deviceFingerprint` / `TimeProvider` 是测试注入点；发行 GUI 使用默认设备算法和系统单调计时。传入的 `HttpClient` 由调用方管理生命周期。客户端将许可和确认时间作为单个不可变状态原子发布，网络检查开始时立即切换为禁止启用；线程读取不会混合两份授权状态。

`LicenseStatus` 提供 `CanEnable, Code, Message, Kind, ExpiresAtUtc, DeviceId`。`Kind` 为 Unknown / Permanent / Timed，来自签名元数据而非猜测空期限。常见 `Code`：Valid、NotActivated、ConfigurationRequired、Checking、InvalidCode、AlreadyRedeemed、DeviceMismatch、InvalidLicense、Expired、Suspended、NetworkUnavailable、BackendUnavailable、StorageError、InvalidResponse。错误仍允许 GUI 提供关闭、还原和状态查看。

## HTTP 接口

| 请求 | 正常响应 / 说明 |
| --- | --- |
| `POST /api/licenses/activate`，`{code,deviceId}` | `{code:"Valid",message,certificate}`；证书为 CE 原生 MACHINE FILE |
| `POST /api/licenses/validate`，`{certificate,deviceId}` | 在线确认限时证书；同样返回真实 CE 新签名快照 |
| `POST /admin/login`，表单 `token,__RequestVerificationToken` | 建立管理员 cookie，会话令牌只由服务端核验 |
| `GET /api/admin/codes` | 最近 1000 条码尾、种类、设备、首次激活、期限和停用记录 |
| `POST /api/admin/codes`，`{kind:"permanent",days:null}` 或 `{kind:"timed",days:30}` | 返回仅本次显示的完整码；days 支持 1–36500 |
| `POST /api/admin/codes/{id}/suspension`，`{suspended:true/false}` | 仅限时；调用真实 CE suspend / reinstate |
| `POST /api/admin/logout` | 注销管理员会话 |

管理 API 除登录外要求会话；所有管理 POST 还要求页面生成的 `X-CSRF-TOKEN`。设备值是 64 位大写十六进制 SHA256。失败响应为 `{code,message,certificate:null}`，不包含 CE 原始错误体或凭据。

## 验证与待部署事项

在项目根目录执行：

```powershell
dotnet build .\licensing\server\CoH3.Licensing.Server.csproj -c Release
dotnet run --project .\licensing\tests\CoH3.Licensing.Tests.csproj -c Release -- .\licensing\.local\server.json
```

测试会直接创建真实 CE 许可证、真实设备和独立 SQLite / license 文件，保留本地证据，不碰运行中的业务数据库。缺配置时退出码 2 并显示 NOT RUN，不以全 mock 代替后端完成。报告写入 `.local/test-runs/<UTC-time>-<id>/results.json`。用可注入单调时间推进检查到期；丢响应测试只在实际服务器提交后丢弃真实 HTTP 响应，不伪造 CE 状态。

当前所有监听都仅限本机，没有公开部署。未来 HTTPS 反向代理需要保持后端绑定 loopback，固定重写适配层上游 Host 为字面量 loopback、固定 CE 上游 Host 为配置的 CE 域名，并显式配置可信代理与 HTTPS cookie/转发方案；当前 Host 过滤不会接受公网域名直传。公网域名、证书、正式密钥/备份、代理部署和第二台物理机验证仍是待部署/待现场事项。
