# Keygen CE 复用评估与实际验证记录

研究 / 验证日期：2026-11-09。目标是复用成熟许可证、设备与签名实现，同时满足一次性全球兑换、永久无 TTL、限时首次激活起算和启动联网规则。实现范围仅为本目录，未修改或启动游戏。

## 结论与职责

采用官方 **Keygen CE 自托管 API**，没有另写许可证签名算法或模拟 CE 后端。Keygen 承担 product / policy / license / machine、受保护策略、设备限制、验证、停用和原生 RSA 机器文件签名。业务适配层仅补一次性人用兑换码与 SQLite 事务、同步首次激活期限、受保护管理员页面及客户端所需的精简 HTTP 接口。

部署使用官方 `keygen/api` 镜像，锁定 digest：

```text
sha256:4a5550da52bb2ee9b37ba245ddfe5c3809e062efe802c5354510dfe4217e0102
```

配套为 Postgres 17.5、Redis 7.4.1；独立 Compose 项目 `coh3-licensing-local`。适配层 .NET 9 / Microsoft.Data.Sqlite 9.0.17，客户端只依赖标准 .NET API。

官方依据：[自托管 CE 文档](https://keygen.sh/docs/self-hosting/)、[官方 Compose](https://github.com/keygen-sh/keygen-api/blob/master/compose.yaml)、[官方许可证与设备 API](https://keygen.sh/docs/api/)。官方源代码 master 用于解释行为；可复现的实际运行版本以以上镜像 digest 为准。

## 能力与适配取舍

| 需求 | 官方能力 / 实际行为 | 实现决策 |
| --- | --- | --- |
| 永久许可证没有到期日 | Policy `duration=null` / license `expiry=null` 支持永不过期 | 永久策略 duration=null，签名时再次验证 license expiry 为 null |
| 永久机器文件不定期刷新 | Machine checkout 默认约一个月；文档明确 ttl 可为 null，此时永久且不可撤回；非 null 最少 3600 秒 | 实际请求 `POST machines/{id}/actions/check-out?ttl=&include=license&algorithm=base64%2Brsa-sha256`；真实 CE 生成 `meta.ttl=null, meta.expiry=null`，没有自定义票据 |
| 签名明确区分永久与限时 | 原生机器文件可包含 license；签名覆盖 included license 元数据 | 签入 `metadata.product=<ProductId>, kind=permanent/timed, days`（旧的 `coh3App`/`coh3Kind` 键继续兼容）；客户端不从空 expiry 猜类型 |
| 限时从首次激活起算 | Policy `expirationBasis=FROM_FIRST_ACTIVATION` 原生支持，但官方文档说明通过异步事件更新，可能延迟几分钟 | 两策略 duration=null / FROM_CREATION；适配层在真实 machine 创建成功后，同步 PATCH `license.expiry=machine.created+days`，验证成功后再 checkout 和提交本地记录 |
| 一码全球一次、设备上限一台 | strict / maxMachines=1 / NO_OVERAGE 限制设备，原生许可证可多次由同设备验证 | 额外持久兑换码 hash；本地 `BEGIN IMMEDIATE` 防并发；远端机器创建是可恢复绑定点。相同设备恢复，异机 409 |
| 丢响应、进程中断恢复 | Keygen machine.created 和 fingerprint 在数据库持久化 | 远端已有同设备 machine 时恢复原首次激活时间；永久本地已提交时返回原证书逐字一致。远端先创建后本地未提交的窗口也测试过 |
| 用户持码不能自行改设备 | protected policy + authenticationStrategy NONE | 客户端不持有 CE API token；只通过适配层提交人用兑换码和摘要设备 ID；只给服务端产品 token |
| 限时每次新进程启动联网 | CE 原生在线 license validation + 新机器文件 | 客户端读取签名类型，限时 POST validate；永久跳过 HTTP。运行中只查单调时间至签名 expiry，不开刷新任务 |
| 停用与离线的解释状态 | CE suspend / reinstate 和错误码 | 映射为 Suspended / Expired / BackendUnavailable；客户端网络失败为 NetworkUnavailable，CanEnable=false；已运行限时实例按确认到期日工作 |

官方出处：[Policy expirationBasis](https://keygen.sh/docs/api/policies/#policies-object-attrs-expirationBasis)、[Policy API](https://keygen.sh/docs/api/policies/)、[Machine check-out 与 ttl](https://keygen.sh/docs/api/machines/#machines-actions-check-out)、[加密与签名格式](https://keygen.sh/docs/api/cryptography/)。

原生源码进一步证实 ttl / 签名行为：[AbstractCheckoutService](https://github.com/keygen-sh/keygen-api/blob/master/app/services/abstract_checkout_service.rb)、[MachineCheckoutService](https://github.com/keygen-sh/keygen-api/blob/master/app/services/machine_checkout_service.rb)。对于 ttl=null，原生 `expires_at=nil`；未加密快照保存 Base64 编码 JSON 为 enc，使用 `machine/{enc}` 作为 RSA PKCS1 SHA256 签名输入。客户端通过标准 `RSA.VerifyData` 验签，要求算法恰为 `base64+rsa-sha256`、公钥至少 2048 位。

**首次激活的持久点** 是 CE 创建绑定机器成功的服务器时间，不是发码时间或客户端本地时间。远端已创建而后续响应丢失时，时间不会向后重置。这保留了可恢复性并防止通过重试延后期限；成功响应仍要等原生验证、签名和本地事务提交全部完成后发送。

## 当前 CE 版本的实际差异

初始化最初按旧假设读取账户 `data.attributes.publicKey`，真实响应没有该属性。实际官方 [AccountSerializer](https://github.com/keygen-sh/keygen-api/blob/master/app/serializers/account_serializer.rb) 与当前 CE 一致：RSA 公钥位于 `data.meta.keys.rsa2048`，值为 Base64 编码 PEM。初始化脚本已根据真实响应解码，并验证 PEM 格式；不是生成本地替代私钥。

机器文件默认 TTL 与永久授权不等价：即使 license 永久，默认 checkout 文件仍约一个月到期，因此必须显式请求空 ttl 并验证返回的 ttl / expiry 都为 null。当前真实 API 接受空查询参数作为 nullable integer，已在永久、限时两种签名中实际验签确认。

原生 FROM_FIRST_ACTIVATION 的异步更新不适合返回后立刻被当成永久许可的分支。当前适配器不用这个异步字段来决定种类；显式签名种类和同步 expiry 两项都验证，不接受 timed+expiry=null。

## 环境与部署观察

首次只读 Docker info 发现 Linux engine 管道不存在，未将其描述为后端已完成。父侧正常启动 Docker Desktop 后继续；官方镜像第一次 pull 遇到 EOF，正常重试下载成功。实际没有依赖 Docker 模拟器或内存状态。

所有新资源有独立项目前缀，既有容器没有被修改。CE 只映射 `127.0.0.1:47830`；适配层仅 `127.0.0.1:47831`；Postgres / Redis 无宿主端口。初始化用一次性管理员 API token 创建产品、策略和产品 token，随后撤销临时管理员 API token；后续失败路径也执行 finally 撤销。运行凭据均留 `.local`，RSA 私钥由 CE 账户存储，客户端只获得公开 PEM。

管理页面使用标准 ASP.NET cookie 认证 / Antiforgery；Windows 的 cookie 密钥额外用 DPAPI 加密。`Host` 过滤接受字面量 loopback IP，CE 则要求固定配置的 `api.keygen.localhost`。这是一套本地模板；外部 HTTPS 反代必须正确重写两个不同上游 Host，并配置可信转发、HTTPS cookie、域名、证书和备份后再部署。

## 已执行的真实集成检查

执行命令：

```powershell
dotnet run --project .\Licensing\tests\Licensing.Tests.csproj -c Release -- .\Licensing\.local\server.json
```

已完成首轮 26 项、补强后 32 项 PASS。最终在独立输出目录重新构建（0 警告、0 错误），包含 Windows DPAPI 和 NO_OVERAGE 健康检查补强后，32 项再次通过。最终记录：`.local/test-runs/20261109-142941-164dd0/results.json`。这里的时间戳为 UTC。测试源码见 [Program.cs](tests/Program.cs)，不是全 mock 测试。

授权系统改为通用多产品架构（`ProductId` 隔离授权码哈希空间、设备指纹空间与 Keygen 策略）后，在同一套真实 CE 容器上重跑：**33 项 PASS**（新增第 33 项「授权码不能跨产品 id 兑换」，返回 `UnknownProduct`），0 警告 0 错误。记录：`.local/test-runs/20261110-081103-24b5b8/results.json`。最终构建和执行命令：

```powershell
dotnet build .\Licensing\tests\Licensing.Tests.csproj -c Release --artifacts-path .\Licensing\.local\final-build
dotnet .\Licensing\.local\final-build\bin\Licensing.Tests\release\Licensing.Tests.dll .\Licensing\.local\server.json
```

主要实测：

1. `/health` 通过真实 CE 查询并验证两策略；匿名管理 API 401、匿名页面跳转、缺失 CSRF 400、正确令牌建立会话。
2. 同一码两个不同测试设备摘要并发激活，仅一个成功，另一个 AlreadyRedeemed；同机重试得到逐字相同原始证书。
3. 真实原生机器文件签名包含应用 / 种类，ttl=null；复制到不同设备摘要、本地篡改签名、替换信任公钥分别被阻止。
4. 真实限时激活包含明确种类及首次机器时间 + 2 天的 expiry；以测试时钟推进 3 天模拟运行跨到期，检查失败且 HTTP 计数不变。测试只模拟流逝时间，没有模拟服务端状态。
5. 限时新启动恰有一个实际 HTTP 验证；真实 CE suspend 后下次启动返回 Suspended 并清理旧启用状态；reinstate 后恢复；永久码不出现远程撤回操作。
6. 在真实激活响应已由服务器提交后，传输层有意丢弃响应并抛网络错误；客户端没有许可证文件；同机重试恢复逐字相同证书。
7. 通过真实 CE API 先创建机器、不写本地兑换绑定，重现本地提交前中断；异机仍被阻止，原机恢复成功，首次时间和期限保持不变。
8. 停止实际 Kestrel 后，永久新客户端启动成功且零 HTTP；限时新客户端返回 NetworkUnavailable；重新启动并复用原 SQLite 后仍恢复原证书且阻止异机。
9. 缺失设备的畸形激活请求是解释性 400，管理员列表实际显示持久设备 / 激活 / 期限。

设备测试注入的是两个不同 SHA256 摘要，用于真实 API 并发与证书匹配验证，不冒充已经在两台物理电脑运行。当前设备默认源是 Windows 安装 MachineGuid，经 SHA256 后传输；系统克隆/重装和硬件证明不属于这次已测结论。

## 管理员浏览器验收

已使用独立 agent-browser / Chromium 会话，在实际 `127.0.0.1:47831` 页面完成：匿名跳转、令牌登录、选择限时并填 3 天、页面生成码、刷新后仅显示码尾、点击停用后显示“已停用 / 恢复”、恢复、退出登录。既有父侧测试码未被停用。

本地截图 `.local/admin-acceptance.png` 不含管理 token、私钥或完整兑换码。测试会话已退出并关闭。管理 API 的登录、发永久 / 天数码、列表、停用则同时有自动化 HTTP 覆盖。

## 待部署 / 待现场

- 正式 HTTPS 端点、域名 / 证书 / 可信反代、正式运行凭据、备份恢复演练尚未公开部署。云主机选项（小型 VPS / 容器平台 / 家用机内网穿透三类）与费用区间已在质询轮口头列出，本期不购买、不部署。
- 第二台物理 PC 上的原生 MachineGuid 设备匹配、系统重装 / 克隆行为待现场验收。
- 本目录只验客户端和授权服务，不运行主 GUI 的游戏功能，不启动或修改游戏进程。
- 32 项检查使用完整 CE + Kestrel + SQLite；受保护发行 DLL 的实际 HTTP 验证由父侧独立执行并记录，不能用这里的普通编译 DLL 结果替代。

## 父侧补充验证（2026-11-09 收尾）

- 保护客户端回归：`tests/ProtectedClient` 引用**实际混淆 DLL**（SHA-256 `8804C52357C126B388750AA7EB78DE0D206D81E4961D00BDA0A1C0FA77F7829E`）对真实服务 11/11 PASS，覆盖 JSON 反序列化（P1 修复后）、永久/限时激活与恢复、异机拒绝、时钟偏移、断连不删证。记录 `tests/ProtectedClient/artifacts/run2/protected-client-tests.json`。
- 发行 bundle 内容核验：`build/scan_bundle_streams.py` 全偏移 raw-deflate 扫描发行 EXE（SHA-256 `E974CD8E731B51463802A5DCAAD12BF533A54903E837C6BC4955B5AB0F18C9A5`），205 条内嵌 PE 流中混淆版两 DLL 精确命中、未混淆版无流无原始字节；混淆客户端哈希与上述回归引用一致。记录 `artifacts/bundle-scan.txt`、`artifacts/bundle-verification.md`。
- 用户包：`dist/CoH3FowTool-2.1-win-x64.zip`（42,528,886 字节，SHA-256 `15CC47AEC6B5BE75122B34FF38B5F93ED04216B7A45AE598D3DAC5B5120E672F`），仅含发行 EXE 与中文使用说明，解包逐文件哈希回读一致；不含源码/PDB/`.local`/管理凭据。
