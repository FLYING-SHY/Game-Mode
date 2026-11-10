# Windows 桌面软件发行授权模式调研

核实与访问日期：**2026-11-09**。范围为通用商业软件授权模式；【已证事实】表示官方资料明确说明的行为，【推荐决策】表示调研时讨论的产品规则。

**后续决策更新（2026-11-09 21:37）**：用户已选择简化为“永久授权仅首次联网，之后本机验证；限时授权每次启动联网，运行中检查到期时间”。先前统一七天刷新的推荐及下文规则列表作为调研时的历史快照保留；当前产品规则以[GUI 与授权最终规格](licensing-gui-spec.md)为准。官方资料结论不受此产品策略调整影响。

**结论【已证事实】：讨论中的流程组合了已有的设备激活、长期授权记录、签名离线凭证与定期联网验证模式。它有成熟官方案例，但这些案例不足以证明市面所有或多数软件采用同一套实现。** Microsoft 365 Apps 使用账户授权与周期联网；JetBrains 同时提供账户、离线激活码和许可证服务器等入口；Keygen 提供设备激活及离线签名文件。[Microsoft Learn](https://learn.microsoft.com/en-us/microsoft-365-apps/licensing-activation/overview-licensing-activation-microsoft-365-apps)、[JetBrains](https://www.jetbrains.com/help/idea/register.html)、[Keygen](https://keygen.sh/docs/activating-machines/)（访问：2026-11-09）。

## 官方事实及对本方案的含义

| 主题 | 已证事实 | 推荐决策与解释 |
|---|---|---|
| 一次兑换与长期 license | Microsoft 的兑换产品密钥只兑换一次，兑换后通过账户安装产品、管理权益。Keygen 则将创建 license、验证 license、激活设备分为不同动作；license key 并非天然都是一次性兑换码。[Microsoft](https://support.microsoft.com/en-us/microsoft-365-activation-licensing/get-help-with-product-key-errors-on-office-com-setup#bkmk_errors1)、[Keygen](https://keygen.sh/docs/activating-machines/)（访问：2026-11-09） | **一码首次联网兑换**是本产品的业务规则。兑换完成后保留长期授权记录与设备关系；后续刷新验证已有授权，不再次消耗兑换码。 |
| 设备激活与机器码 | Keygen 激活需要 license ID 与设备 fingerprint；IP、主机名等为可选属性，指纹可由客户端查询设备信息生成。[Keygen](https://keygen.sh/docs/activating-machines/#how-to-identify-a-device)（访问：2026-11-09） | 采用后台自动绑定设备、不绑 IP；用户只输入兑换码。**用户无需填写机器码**是交互选择，底层仍存在设备标识。 |
| 账户与席位 | Microsoft 365 Apps 按用户分配授权，并允许该用户激活多台设备；JetBrains 允许从账户获取授权，也提供离线激活码和 Floating License Server / License Vault。[Microsoft Learn](https://learn.microsoft.com/en-us/microsoft-365-apps/licensing-activation/overview-licensing-activation-microsoft-365-apps)、[JetBrains](https://www.jetbrains.com/help/idea/register.html)（访问：2026-11-09） | 账户席位、设备名额和兑换次数应分别理解。账户系统并非本产品已确定的必需入口；当前保持“一码兑换并绑定设备”的体验。 |
| 永久与限时 | Keygen 区分永久和限时许可；其策略支持 `duration=null` 表示无到期日，也支持 `expirationBasis=FROM_FIRST_ACTIVATION` 从首次设备激活后设置到期日，默认却是创建时起算。[Keygen 策略](https://keygen.sh/docs/api/policies/#policies-object-attrs-expirationBasis)（访问：2026-11-09） | 保留永久、限时两种；限时从首次成功激活起算，以服务端记录为准。这是可配置能力，并非各软件默认行为。 |
| 离线签名与请求应答 | Keygen 能将 license 或 machine 导出为签名文件，内容包含授权/设备快照及有效期；联网设备可代取文件，再转交离线设备。[Keygen 离线许可](https://keygen.sh/docs/choosing-a-licensing-model/offline-licenses/)、[文件格式](https://keygen.sh/docs/api/cryptography/#cryptographic-lic)（访问：2026-11-09） | 完全离线激活可采用“设备请求 → 联网端取得签名许可 → 离线端导入验证”。本产品已选择首次联网兑换，日常可自动领取凭证，无需让用户手工搬运请求文件。 |
| 最多 7 天离线 | Keygen 离线文件的 TTL 可按用途调整，默认及建议起点为 30 天，并明确文件到期日与 license 到期日分开。Microsoft 365 Apps 的常规周期也是至少每 30 天联网一次。[Keygen TTL](https://keygen.sh/docs/api/cryptography/#cryptographic-lic)、[Microsoft Learn](https://learn.microsoft.com/en-us/microsoft-365-apps/licensing-activation/overview-licensing-activation-microsoft-365-apps)（访问：2026-11-09） | **7 天是本产品自选规则，不是统一行业标准。** 从最近一次服务端成功签发/刷新的凭证时间起计算；永久授权同样遵守刷新周期，限时授权还受其自身到期日约束。 |
| 签名防伪的边界 | Keygen 私钥留在服务端，客户端使用公钥验证；签名用于确认数据来源及完整性。官方另行明确，客户端程序仍可能被修改。[Keygen 密钥](https://keygen.sh/docs/api/cryptography/)、[客户端修改](https://keygen.sh/docs/api/security/#security-crack-prevention)（访问：2026-11-09） | 私钥不进入用户端。签名授权文件提供防伪与修改检测，**不代表客户端程序不可修改**，也不意味着签名内容自动保密。 |
| 停用、撤销与换绑 | Keygen 的离线许可是签发时的快照；TTL 让更新、续期、到期或停用等变化在后续快照中传播。设备创建/删除分别对应激活/停用。[Keygen 快照一致性](https://keygen.sh/docs/api/cryptography/#cryptographic-lic)、[设备生命周期](https://keygen.sh/docs/activating-machines/)（访问：2026-11-09） | 管理员控制停用与换绑。**旧设备离线时，新状态要等它联网或旧凭证到期才生效**；立即给新设备换绑，可能在旧凭证剩余期限内出现两台设备都可用的窗口。按 7 天规则，最长延迟取决于旧凭证尚余的有效时间。 |

## 调研时确认的产品规则（历史快照）

以下均为【推荐决策／已确认产品选择】，不是对所有厂商实现的归纳：

- 用户端为 GUI 单 exe；首次输入一码并联网兑换，后台自动绑定设备，不要求用户填写机器码，不绑定 IP。
- 维护长期授权记录；永久与限时两种，限时从首次成功激活起算。
- 激活后允许最多 7 天离线，再联网刷新；离线凭证有效期同时受授权自身到期日约束。
- 停用、撤销和换绑由管理员控制，并接受离线窗口造成的状态传播延迟。
- 用户端、管理端和云端授权服务分离；签名私钥与管理凭据留在可信服务端。Keygen 官方同样要求客户端避免内置管理员、环境或产品级秘密 API token。[Keygen 权限与凭据](https://keygen.sh/docs/api/security/#security-secret-tokens)（访问：2026-11-09）。

“单 exe”“7 天”“仅管理员换绑”分别属于发行形式、离线策略与权限策略；上述官方案例证明这些能力可以组合，未证明存在统一的行业实现。本次仅新增调研文档，未实施授权程序、构建、部署或更改发布包。
