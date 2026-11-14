# 真实客户端 Reactor Trial 独立验证

2026-11-13；使用主代理最终 raw client 的既有 Trial 输出，未编辑 client/server/verification 源码。后续按主代理新增指示，仅读新建 cards-reactor-trial 中四个指定测试卡文件并完成真实四组联机；完整七组均通过。本代理新增/覆盖的 verification 内容仅为主代理明确要求的 verification/bin/ReactorTrial/net48 副本与各独有 verification/sandbox/reactor-* 测试目录。Trial exe仅留本地工具/验证目录，未加入分发包。

## 实际产物身份

- raw: client/bin/Release/net48/天穹之眼-自有授权版-v0.60.8.exe，1,182,208 bytes，SHA256 A2BD412924CE669666AFC4FCAA68B96FA5A6AE29B2BF6A32D89EF1466F20EF10。
- trial: tools/reactor/trial/20261113-144033-423-c4bc273d/天穹之眼-自有授权版-v0.60.8.exe，1,572,352 bytes，SHA256 6551EB5BF91CBB34D579590E4EA70C04FAC55F1A00BE6AF22EB3B3F348EB7F25。
- 对 verification/bin/ReactorTrial/net48 中实际运行的客户端重新计算SHA256，与trial一致。见 identity-summary.json、verifier-client-hash.json。
- 主代理保护参数证据：evidence/reactor-runs/20261113-144033-423-c4bc273d/run.json。rename关闭；只对 LeaseSignature60.Verify 与 LicenseReply60.VerifyActive 添加选择Attribute；未叠Obfuscar。

## 运行时验证

复制 verification/bin/Release/net48 全部内容到 verification/bin/ReactorTrial/net48，再覆盖相同文件名的trial客户端。副本仍保持verifier的三层相对目录规则，因此其sandbox allowlist仍解析到 verification/sandbox。

可复现入口：scripts/Reactor-VerifyTrial.ps1。每个模式独立PID、独有sandbox，30,000ms deadline；只在超时情况下采集该PID窗体及子控件文字，再停止该脚本自身启动的超时进程。本次无超时，也无进程被停止。

| 模式 | PID | ExitCode | verifier elapsedMs | 判定 |
| --- | ---: | ---: | ---: | --- |
| selfcheck | 35460 | 0 | 390 | passed:true |
| ui | 42064 | 0 | 744 | passed:true |
| audio | 16356 | 0 | 106 | passed:true |

证据：runtime-results.json、各 *-stdout.log / *-stderr.log、selfcheck-result.json、ui-result.json、audio-result.json。每项的stderr均为空。sandbox前缀 verification/sandbox/reactor-20261113-144411-762-。

selfcheck实测包括RSA3072/SHA256 PKCS1 activation签名fixture；heartbeat有效Bearer回退；拒绝篡改token、product、lease_signature/kid、时间、license_kind、request device/steam/installation/nonce；拒绝错误RSA key；空production pin fail closed；不可伪造verification marker；真实HTTP unsigned active拒绝；coordinator在GateGrant前拒绝未验证active；原Gate lease/identity/revocation；嵌入资源与PInvoke检查。详见selfcheck-stdout.log。这里的真实HTTP用verifier隔离模拟流程，未使用私有码连接正式授权服务。

UI保留509 controls、7 modules、12 checks：主界面、展开/折叠、numeric边界与rounding、七个audio selectors等通过。main.png、expanded.png、compact.png 为此次真实截图。

Audio八个内嵌音频资源均完成隔离缓存复用与RIFF/WAVE解码检查，见audio-audio-result.json；audiblePlaybackTested:false，未声称已听音确认。

本次三个verifier流程均快速完成，未发生nag阻塞；没有运行主应用登录/正式联机/长期运行来穷举nag展示时机。Demo的14天限制与分发限制依旧成立。

首轮runner用 Start-Process -PassThru 得到空ExitCode，虽verifier报告passed:true，本代理未据此宣称进程成功。修复为显式 Diagnostics.Process 与异步stdout/stderr捕获后重新运行，以上为第二轮有真实ExitCode的最终数据。首轮隔离sandbox保留，未删除。

## REA 与 ILSpy 实际方法证据

已读 F:/WorkSpace/REA/skills/reverse-engineer-anything/SKILL.md。使用已有兼容Node与REA CLI直接静态读取二进制，不加载执行目标：

```powershell
$node='F:\WorkSpace\REA\.runtime\node-v24.22.3-win-x64\node.exe'
$rea='F:\WorkSpace\REA\scripts\rea.mjs'
& $node $rea inspect-managed-members '<raw-or-trial-exe>' --type CoH3MemoryTool.LeaseSignature60 --type CoH3MemoryTool.LicenseReply60 --format json
$ilspy='F:\WorkSpace\Game\CoH3\SkyEye-v0.60.8-Recovered\tools\ilspy\ilspycmd.exe'
& $ilspy -t CoH3MemoryTool.LeaseSignature60 '<raw-or-trial-exe>'
& $ilspy -t CoH3MemoryTool.LicenseReply60 '<raw-or-trial-exe>'
```

完整证据：raw-members.json、trial-members.json、trial-artifact.json、selected-methods.json；ILSpy导出 raw-LeaseSignature60.cs / trial-LeaseSignature60.cs / raw-LicenseReply60.cs / trial-LicenseReply60.cs。工具版本另存ilspy-version.txt。

REA evidence identities：

- raw members：ev_642892150fde533011d0f422711f82217ae59e3726b1181fcb60c596ecf902ec。
- trial members：ev_38afd8b35515936bd662faaf00095b6351af2099ec0569ced2e35fc8d35b5061。
- trial artifact：ev_ff766d8cd4128cd60cabe441c33b5c4b51f5cae5db33169ae6a0dd57890f23df。

| 方法 | raw CIL bytes / instructions | trial CIL bytes / instructions |
| --- | ---: | ---: |
| LeaseSignature60.Verify | 699 / 290 | 147 / 39 |
| LicenseReply60.VerifyActive | 46 / 16 | 85 / 24 |

raw Verify可直接看见canonical数据构造、UTF8、SHA256/RSA VerifyData与异常处理。trial Verify入口已变为参数对象数组 + `hoMuqZRhpg6UCtZcEnM.ehWtVS8XY6(150, array, array2, flag)`，返回VM结果；VerifyActive变为 `hoMuqZRhpg6UCtZcEnM.ehWtVS8XY6(116, array, array2, null)`。该结构与CIL变化支持所选方法实际被虚拟化，超过单独CLI成功日志的证据强度。

边界：trial同一type仍新增可反编译辅助方法，例如直接暴露 `RSACryptoServiceProvider.VerifyData` 调用以及异常类型；方法原名也有意保留。结论是两个选定入口的原CIL主体已被VM调度替代，并非所有语义和加密API都隐藏、也非不可逆保证。未对VM做完整逆向或恢复成本量化。

## 发行状态

这是Demo评估，不是已注册发布版。Reactor原始输出声明14天后停止工作，官方协议还含nag/第三方分发限制；-Mode Production 强制 -licensed 在本环境返回101。通过当前七组功能和静态检查不改变这个状态。四组隔离卡联机结果如下；不把此Demo产物列为可分发版。

## 后续补全：真实四组联机与完整七组聚合

主代理明确允许仅读 F:/WorkSpace/Game/Licensing/.local/hardening-v2/cards-reactor-trial 中 protocol-permanent.txt、protocol-timed.txt、coordinator.txt、network.txt；仅经 --code-file 传给现有verifier。未读取该目录其他卡内容、未读旧.private/.local账号或密钥。BaseUrl为 http://127.0.0.1:47835/ 。继续使用原 verification/bin/ReactorTrial/net48 副本，未build或更换二进制；online-client-hash.json 再次确认trial SHA256一致。

复现脚本 scripts/Reactor-VerifyOnline.ps1；四个不同卡并发启动，Diagnostics.Process保留真实进程句柄、异步捕获输出。protocol每组30秒上限，coordinator/network各120秒上限。各case sandbox前缀 verification/sandbox/reactor-online-20261113-144939-131-。

| 模式 | PID | ExitCode | elapsedMs | 结果 |
| --- | ---: | ---: | ---: | --- |
| protocol-permanent | 10984 | 0 | 158 | passed:true |
| protocol-timed | 23572 | 0 | 123 | passed:true |
| coordinator | 14816 | 0 | 29882 | passed:true |
| network | 27620 | 0 | 59181 | passed:true |

证据 online-20261113-144939-131/ 下每case的stdout、stderr、verifier-result以及results.json；顶层online-results.json便于聚合。四组stderr均空，未超时、未停止任何进程；没有nag阻塞。输出保存前检查/替换对应新卡字符串；额外对13个联机证据文件扫描四张卡完整值，命中数0，见seven-case-summary.json。未在报告记录卡、session_token或私钥。

- 两组protocol均通过真实active签名标记、永久/计时kind、同request幂等、不同payload冲突、并发会话in_use、authenticated heartbeat与logout、不同设备拒绝、重新激活仍保留原expires_at。
- coordinator：activate回复116ms；首heartbeat HTTP28211ms、回复28218ms，距激活request约28.1秒；强制resume后再验证，换current-game identity时29525ms收到logout回复并重新activate，pause在29790ms收到logout回复，29879ms通过resume/identity/pause检查。最后await CloseAsync返回并检查gate关闭后进程Exit0。
- network：activate回复112ms；人为网络传输故障后28189ms首次network_wait，再以真实5秒级重试；59170ms通过实际租约deadline关闭gate（源码还断言从activation起至少57000ms），并确认network_wait；finally恢复Offline=false、await CloseAsync，在59178ms收到实际logout回复，59181ms完成。59.17秒与整秒服务端lease时间戳+本地单调时钟转换一致，不是把等待人为缩短为毫秒fixture。

finally证据边界：network明确在finally关闭路径打印logout回复；coordinator已经pause/logout，finally完成关闭与gate断言，但无须假定又发送一次logout。protocol源码的finally直接await API logout而不打印REPLY事件；两组最终PASS/Exit0证明该finally没有抛异常并返回，协议日志还记录了前序显式logout检查；独立服务端audit/session终态由主代理另查。聚合字段使用LogoutReplyObserved而非把任意logout日志错误标成finally网络观测。

完整seven-case-summary.json已合并本地三组与联机四组：Count=7、PassedCount=7、AllPassed=true，每项ExitCode=0。这里比较的是同一未注册Reactor Trial的完整七组兼容性，不改变官方14天/nag/分发限制。
