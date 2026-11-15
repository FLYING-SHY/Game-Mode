# 第二轮加固验收报告

本轮在独立工程实现了服务端签名短租约、客户端固定公钥验签和正式混淆发布，并对 .NET Reactor 7.7.1.0 进行了独立虚拟化试验。以下结果来自本机真实执行和静态证据。

## 交付结果

| 产物 | 本轮采用的保护 | 用途 |
|---|---|---|
| 客户端发布包 | RSA-3072 签名租约校验 + Obfuscar 2.2.50 元数据重命名和字符串隐藏 | 分发客户 |
| 签名服务端发布包 | ASP.NET Core 10 / SQLite / 持久 RSA 私钥 | 部署服务 |
| 维护包 | 两端源码、构建脚本、私有重命名映射、验证器和运行证据 | 自己维护 |
| Reactor 试验记录 | 两个验签方法选择性虚拟化的历史实验 | 日志与测量保留；试用工具、EXE 及实验验证器已清理 |

正式客户端 SHA256：`636206fdd6dc6eeda979d5c3d1fc21432e6aaffbbbad899299a835e28ce1ce6b`。
原始签名输入 SHA256：`a2bd412924ce669666afc4fcaa68b96fa5a6ae29b2bf6a32d89ef1466f20ef10`。
Reactor 试验 SHA256：`6551eb5bf91cbb34d579590e4ea70c04fac55f1a00be6af22eb3b3f348eb7f25`。

## 签名链与拒绝条件

规范串见 [签名契约](../signature-contract.md)，共 13 行，以 LF 分隔且末尾没有 LF。服务端使用 RSA-3072 / SHA-256 / PKCS#1 v1.5 签署产品、active 状态、设备、当前 Steam 身份、安装 GUID、请求 nonce、有效会话令牌的 SHA256、服务时间、租约截止、卡密截止、卡种和 kid。

每次实际激活或心跳发送均生成新的 128 位随机 nonce；网络重试保留业务 request_id，但换新 nonce。客户端公钥固定在编译产物中，没有网络公钥引导或无签名降级路径。

API 层先验证签名，再产生只读回复快照与验证标记；真实 Coordinator 的唯一 Grant 入口还要求该标记。测试覆盖改签名、改 kid、改三个时间、卡种、token、产品、设备、Steam、安装 GUID、nonce、错误密钥、空 pin、未签名 HTTP active 和直接构造未验证回复。

已有租约的本地截止、60 秒上限、1 秒扣减、约 28 秒心跳及断网失效规则保持。签名失败不会延长旧租约。签名证明响应来自持有服务端私钥的一方；本地代码被修改的风险仍存在。

私钥采用持久存储、原子首次发布及重启复用；并发初始化选择同一个已发布密钥，损坏的既有密钥会使服务启动失败。公钥由离线 CLI 导出后固定，构建保护入口额外检查 modulus 为 384 字节、exponent 非空和 kid 与公钥摘要一致。

## 实际回归

服务端隔离 HTTP + SQLite 回归 **120/120 通过**，包括原 80 项业务回归与 40 项签名相关回归。原始结果见 [服务端测试](../evidence/server-signature-tests/results.json)。

| 组别 | 原始签名版 | 正式混淆版 | Reactor 试验版 |
|---|---|---|---|
| selfcheck：验签负向、配置、DPAPI、Gate、资源、PInvoke | 通过 | 通过 | 通过 |
| UI：七模块、布局、开关和数值边界 | 通过 | 通过 | 通过 |
| audio：八个 MP3 的实际解码与缓存 | 通过 | 通过 | 通过 |
| 永久卡真实协议 | 通过 | 通过 | 通过 |
| 限时卡真实协议 | 通过 | 通过 | 通过 |
| Coordinator：续租、恢复、身份变化、暂停登出 | 通过 | 通过 | 通过 |
| network：真实等待租约耗尽 | 通过 | 通过 | 通过 |

三版各七组，退出码全部为 0；逐进程 result.json 的 passed 也逐项核对。原始版见 [raw 汇总](../evidence/raw-signed-20261113-144039/summary.json)，正式版见 [正式回归清单](../evidence/protected-verification-20261113-144238-873.json)，商业试验见 [七组汇总](../evidence/reactor-client-trial/seven-case-summary.json)。

正式混淆版激活发送时刻 107 ms，首次心跳发送时刻 28117 ms，间隔 **28.010 秒**。断网包装器产生传输故障后，Gate 在进程启动后 **59178 ms** 关闭，恢复测试传输后登出。使用真实时钟等待，故障仅限验证器传输边界，未停止服务或修改系统网络。

Reactor 版首次心跳发送时刻 28211 ms；断网组 Gate 在 59170 ms 关闭，59178 ms 收到登出回复。上述单次观测用于证明节奏和失效行为，不是统计性能基准。

UI 以原始签名版为基准，分别对正式版及试验版的 main、expanded、compact 三张截图逐像素比较，六次 changedPixels 均为 0。两种保护版的八个解码 WAV 长度及 SHA256 均与原始版完全一致；未进行扬声器试听。见 [派生核验汇总](../evidence/validation-summary.json)。

12 张完成联调的卡经真实管理员会话查询，观察时有效会话数全部为 0。该查询证明观察时的服务器状态；Protocol 的 finally await 正常返回由其退出码和控制流证明，日志没有被夸大为每次都打印了独立 logout 回复。见 [会话清理](../evidence/session-cleanup.json)。

正式客户目录的真实 EXE 已打开 706×509 激活窗口，采集后正常关闭本次启动的进程。界面提示先启动游戏并停留在大厅；未据此宣称已经进入实际游戏。见 [入口生命周期](../evidence/protected-real-entry-lifecycle.json) 和 [截图](../evidence/protected-real-entry.png)。

## 发布服务与部署

发布目录中的服务在 47836 临时端口实际启动，使用相同签名密钥与独立测试数据库；真实受保护客户端完成激活、幂等、冲突、绑定、续租和登出协议检查。见 [健康检查](../evidence/published-server-health.json) 与 [真实协议日志](../evidence/published-protected-protocol.log)。临时服务随后关闭。

本机 V2 签名服务及客户端默认使用 47835。加固验收时旧 47831 服务独立保留；后续 2026-11-13 清理中已停止旧服务，并删除旧服务源码与启动入口。原有授权业务数据和密钥仍保留在私有目录，未迁移正式卡密。缺少有效签名的服务端响应不会产生新租约。

更换签名密钥必须重新导出固定公钥、编译、保护并重新验证客户端。维护包不含本机私钥、管理员令牌或测试完整卡密；现有已编译客户包需要与本机保留的原签名密钥配对。新环境生成新密钥后，应从维护工程重新固定公钥并构建对应客户包。

数据库、整个 KeyDirectory 和私有配置需一起备份。Windows 本轮目录 ACL 限制为当前用户、SYSTEM 和管理员；Unix 创建签名私钥后设为 0600，首次临时写入还依赖受限父目录和服务账户 umask。后者属于部署前提，未宣称已在 Linux 动态验证。

## 商业保护实际结果

官方包签名 Valid，签名人 Eziriz e.K.；提取后的 Console 实际报告版本 7.7.1.0。只处理未混淆输入，选择性标记 `LeaseSignature60.Verify` 和 `LicenseReply60.VerifyActive`；未把高频绘制、预测或 Gate 热路径送入虚拟机。

REA 与 ILSpy 直接比较：Verify 从 699 字节 / 290 条 CIL 变为 147 字节 / 39 条；VerifyActive 从 46 / 16 变为 85 / 24。两个入口实际变为虚拟机 dispatch，相关原始主体不再直接以原样 CIL 出现。辅助方法仍暴露 RSA VerifyData 等 API，类型名刻意保留，结果不等于全程序语义隐藏。详细证据见 [商业试验报告](../evidence/reactor-client-trial/REPORT.md)。

当前工具为 Demo，保护文件 14 天失效且存在试用发布限制；Production 路线通过 `-licensed` 检查，当前实际返回 Exit101。试用通过不等于已取得商业发布许可。因此交付采用已通过回归的签名 + Obfuscar 版，试用 EXE、工具安装包、提取工具和试验验证器均排除出所有交付压缩包。商业工具脚本和文字/JSON 对照证据保留在维护包。

## 正式版逆向检查

REA 对正式版重新读取，coverage complete：341 types、4032 methods、1399 fields、1562 member refs。ILSpy 实际 project export **成功**，生成 56 个 C# 文件。元数据重命名和字符串隐藏提高阅读及修改成本，未阻止托管反编译。见 [正式版静态分析](../evidence/release-static-analysis/summary.json)。

独立只读跨端审查未发现导致错误授权的重大缺口；canonical、nonce、验证标记、token fallback、密钥生命周期和构建 pin 链均有源码锚点。Steam 输入域客户端更严格等兼容性差异和 Unix 权限前提仍保留在 [审查报告](signature-cross-review.md)。

## 验证边界

本轮覆盖本机 Windows x64、.NET Framework 4.8 客户端、.NET 10 服务、真实 HTTP 与 SQLite、实际公钥签名及短租约节奏。尚未覆盖实际游戏内完整对局、长期压力、第二台机器、公网 HTTPS、容器部署、代码签名、混淆后全部游戏内路径、任意本地补丁对抗或生产迁移。

封存程序通过清理清单验证旧源码/归档在删除前的基线及删除后的状态，并核对原版、当前源码、客户发布和签名材料的保留哈希；同时检查客户包无源码/映射/PDB、所有包无 Demo 客户端及本机秘密，逐项解压复核大小和 SHA256。最终结果见 [最终校验](../evidence/final-validation.json) 及压缩包外置 `archive-verification`。

## 2026-11-13 旧版清理与再验收

按精确清单删除 107 个旧目录或文件目标，共 4506 个文件、2,091,090,074 字节。删除对象包括旧恢复版、初次授权版、第一轮加固、旧交付包、重复授权源码、旧服务入口、试用工具及旧临时产物。原版和当前源码/发布/签名材料共 284 个保留文件逐项哈希核对；旧源码备份中 3 个独有私有数据文件单独保留。见 [删除清单](../evidence/cleanup-manifest.json)。

清理后实际重新编译客户端、服务端与验证器，均为零编译警告和错误；隔离后端真实 HTTP + SQLite 测试再次 **120/120 通过**，正式受保护客户端 **7/7 组通过**。未混淆重建输入哈希与已保护输入一致，客户 EXE 和客户 ZIP 保持原哈希。新一轮三张界面图与原正式版逐像素零差异，八个解码音频载荷一致。见 [清理后验收](../evidence/cleanup-validation.json)、[后端结果](../evidence/cleanup-server-tests-results.json) 与 [七组清单](../evidence/protected-verification-20261113-163401-399.json)。

迁移后的 ILSpy 9.1.0.7988 可实际运行，17 个现有 PowerShell 脚本语法检查通过。再次移除了回归重新生成的 82 个临时文件；保留结构化结果、日志与图片。当前 47835 服务健康，旧 47831 端口关闭。服务端包和维护包以 `Finalize.py --refresh` 原子刷新；客户包逐项核对后保留。
