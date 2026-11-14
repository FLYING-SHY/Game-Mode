# V2 跨端签名只读审查

结论：在本次审查的客户端真实发送路径、Coordinator 授权入口、服务签名入口及公钥发布脚本中，未发现可造成错误授权的重大缺口。此结论来自源码审查；本轮没有构建、请求运行服务或读取私钥/私有配置。

## 观察到的事实

| 项目 | 依据 | 结果 |
| --- | --- | --- |
| 签名格式 | [服务签名](../../../LicensingSystem/server/LeaseSigner.cs#L65)、[客户端验签](../client/CoH3MemoryTool/LeaseSignature60.cs#L48) | 两端按相同顺序组成 13 行 UTF8 文本，以 LF 连接且无尾 LF；RSA3072 / SHA256 / PKCS1 v1.5 一致。签名覆盖 product、active、device、Steam、installation、nonce、有效 token 哈希、三个时间、kind、kid。 |
| nonce 新鲜性 | [发送路径](../client/CoH3MemoryTool/LicenseApi60.cs#L45)、[随机数](../client/CoH3MemoryTool/LeaseSignature60.cs#L40) | 每次实际 Call 都复制 payload，使用 16 字节 CSPRNG 新建 nonce 并替换原值；验签上下文在发送前捕获。 |
| 幂等 | [激活](../../../LicensingSystem/server/LicenseService.cs#L94) | request_id 的稳定负载哈希排除 nonce；重试复用原会话/lease，按本次 nonce 重新签名。缺失或格式错误 nonce 在存储变更前返回错误。 |
| 有效 token | [客户端 token 选择](../client/CoH3MemoryTool/LeaseSignature60.cs#L55)、[心跳签名](../../../LicensingSystem/server/LicenseService.cs#L197) | 激活要求响应 token；心跳优先采用响应 token，缺失时取本次 Bearer。目前服务回传已验证的 Bearer，Coordinator 在缺失 token 时保留现有会话 token。 |
| Gate marker | [响应标记](../client/CoH3MemoryTool/LicenseReply60.cs#L15)、[Coordinator](../client/CoH3MemoryTool/LicenseCoordinator60.cs#L339) | marker 仅在固定 pin 验签成功后置位，数据字典防御性复制。扫描客户端发现唯一 Grant 调用先检查 marker；同时检查 generation 和发送前后游戏身份。 |
| 时间约束 | [Gate](../client/CoH3MemoryTool/LicenseGate60.cs#L33)、[Coordinator](../client/CoH3MemoryTool/LicenseCoordinator60.cs#L403) | 保持 0<delta<=60、request-start 单调截止时间、1 秒扣减和 28 秒心跳。 |
| 公钥发布 | [初始化脚本](../../../LicensingSystem/Initialize-Signing.ps1#L11)、[构建脚本](../Build-Protected.ps1#L10)、[项目引用](../verification/SkyEye.Integration.Verification.csproj#L10) | 离线导出后只更新源代码 Modulus/Exponent/Kid 三个常量，再编译并混淆。签名相关类没有出现在混淆跳过规则中；未见 HTTP 公钥引导。 |
| 密钥生命周期 | [持久密钥](../../../LicensingSystem/server/LeaseSigner.cs#L22)、[离线导出](../../../LicensingSystem/server/Program.cs#L19) | PKCS8 文件按配置目录复用；初始文件完整写入后原子发布且不覆盖；并发初始化复用胜出文件；已存在损坏密钥终止初始化且保持原样。导出仅包含三个公钥字段，立即退出。 |

## 非阻断观察及推断

- **已补充并只读确认：** main 在[构建脚本](../Build-Protected.ps1#L4)编译/保护前增加 pin 预检：Modulus/Exponent/Kid 三常量存在且非空，modulus/exponent 可解码为 base64，modulus 为 384 字节、exponent 非空，kid 必须精确匹配 SHA256(modulus) 前 16 位小写十六进制。原“构建无 pin 预检”观察已解决。本次仅重新读取脚本，没有构建；main 说明测试过的客户端二进制未变。
- **观察：** 客户端要求 Steam 文本正好 17 位，服务端接受 UInt64 范围的 1–20 位十进制文本。正常 17 位身份一致；超出客户端域的服务端输入会被客户端请求上下文校验挡住。
- **观察：** Windows 私钥及临时文件继承 KeyDirectory ACL。Unix 在写入临时文件之后设置 0600，因此首次创建期间的保密性依赖受限目录/umask。部署备份应保留签名 key 与原数据保护 keyring。
- **推断：** 服务端不维护已用 nonce 集合，与当前合同一致。重放防护依靠客户端每次 send 的新随机 nonce，以及按该请求上下文验签；捕获的响应在新 nonce 下不通过。
- **观察：** 无效签名进入连接失败重试路径，已有有效 lease 可维持到其原单调截止时间；无效回复没有进入 Grant，也不会延长截止时间。

## 本次没有验证

- 保护后程序集的真实执行等价性、商业保护器效果。
- 对运行中 V2 服务的新 HTTP 探测、重放注入或集成场景。
- 部署目录实际 ACL、私钥内容、私有配置。
- 最终交付归档内容及可执行文件哈希链。

机器可读版本：[signature-cross-review.json](../evidence/signature-cross-review.json)。本轮仅新增这两份证据文件，没有修改源码、构建或操作服务。
