# 2026-11-22 Round 2：会话清理与异步帧隔离

状态：源码修复及隔离验证完成；两客户端整体实机验收继续。没有把本轮 F 级检查登记成新的游戏已验证基线，也没有刷新现有交付包。

## 当前身份

2026-11-22 09:41（UTC+08）重新核对：`G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe`，版本 `5.1.52426.0`，108391096 字节，SHA256 `8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC`。当前游戏 PID 30504，启动时间 `2026-11-22T01:23:35.3993114Z`，主窗口 1379546。进程仍在运行；本轮尚未确认其具体对局阶段。

[源文件与运行身份清单](./round2-source-and-runtime.json)记录两树 323 个 C# 文件的 SHA256，以及本轮检查范围。它不包含卡密、私钥或授权数据库。

## 本轮生产改动

1. 两树新增 `ScenePublication<T>`，清理时撤销旧代际；普通刷新保留代际。Air/Squad worker 在代际变化时重建子 reader、清空连接缓存；最终呈现与 Squad 的排队绘制均受代际检查保护。清理返回后，旧帧的再次提交被拦截。已显示窗口的消隐仍依赖 worker 下一帧执行。
2. 两主窗体新增 `ClearSessionData()` 和 `AcceptSnapshot(List<PlayerInfoRow> fresh, string match)`。以已经接受的 `rowsMatch` 比较身份，先清旧状态再安装新行；成功 reader 与 PID 保持存活。DropReader 先脱离旧 reader，完整清理后再释放。support、两套 selection、alerts、armour、恢复预算和 SkyEye 包抄瞬态一并重置，保留持久偏好。
3. 适配复核脚本递归检查 suites/modes 的全部现存 exitCode，并检查全部 requiredSubstrings。报告明确只验证已记录材料的一致性；保留 `--no-write`。负控重现了旧脚本的两处假阳性并验证修正版失败。

## 证据

| 检查 | 结果 | 范围 |
| --- | --- | --- |
| ScenePublication 两树 | 各 26/26 | 实际 helper 的确定性并发；窗口接线静态检查 |
| Session-reset SkyEye | 78/78 | 实际生产清理方法，跳过 form 构造、track:false |
| Session-reset Simple | 66/66 | 同上，含同 key 新 reader 与 A→B |
| 完整 net48/x64 编译 | 4/4 | 两客户端 × Release/Maintenance |
| 编译产物 build-gate | 每组合 48/48，共 192 | 新旧 build 分派、门控及编译元数据 |
| verifier 纯内存回归 | 15/15 | 嵌套非零退出码、第二必需串、正例及 no-write |
| 既有材料无写入复核 | 13/13 | 当前旧交付、四对适配源文件和归档记录一致性 |
| original 全量漂移校验 | 25/25，0 漂移 | 冻结区内容未改动 |

SkyEye 完整编译 0 warning/error；Simple 每组合有 15 项 CS0649 未赋值字段 warning、0 error。原始构建日志保留在隔离构建目录。

- [SkyEye 会话测试 UTF-8 副本](./session-reset-SkyEye.log)
- [Simple 会话测试 UTF-8 副本](./session-reset-Simple.log)
- [会话测试与六文件 SHA256](../../../tests/session-reset/latest-result.json)
- [四组编译与 EXE SHA256](../../../tests/scene-publication/clients-build-report.json)
- [ScenePublication 复现说明](../../../tests/scene-publication/README.md)
- [verifier 负控说明](../../../tests/perbuild-verifier/README.md)

构建输出均在测试目录。当前已有的保护交付包和游戏 hash 分派保留原状态；本轮没有以这些编译结果替换客户包。Windows PowerShell 实际版本为 5.1.26100.9818；新构建脚本已加 UTF-8 BOM 解决中文路径误解码，UTF-16 测试日志另存为 UTF-8 副本供审阅。

## 证据边界与后续

本轮没有执行真实 Snapshot、native renderer、音频、热键或游戏 GUI 验收。历史 Run29/33 的战绩页残留仍然是开放问题：若同 PID/同 Match 的 Snapshot 持续成功，当前主 Poll 仍会刷新图层；本轮改动只解决清理和换局时的状态、异步提交竞争，不建立 Active/Result/Lobby 判定。

目前已有更新的 Simple 实机日志及正式包，旧 acceptance-current.md 中“没有新版交付/尚未登记 hash”的描述已经过时；它列出的未完成行为检查仍须逐项复核。复核脚本的 13/13 仅覆盖原记录指定材料，不会证明本轮新生命周期代码的 G/R/D。

下一轮的具体工作：

1. 修正维护模式的未知 build 选择：PlayerInfoReader.UsesCandidateLayout() 当前仍有 Unknown + maintenanceDiagnostics 回落至 52426 表的路径；未知 build 应在打开 native handle 前停止布局选择，并加隔离负控。正式 Release 的未知 hash 门控仍在。
2. 在当前 PID 获取可独立确认的游戏阶段、截图和同步 Snapshot/活动态轨迹，再进入正常对局→暂停→战绩→主菜单→新对局。保持真实授权入口，使用独立输出目录；不发明活动态偏移，不用静止时长冒充战绩页判据。
3. 将实际 UI、worker 连接、场景清理与源/EXE hash 绑定。完成 SkyEye 与 Simple 的关键功能及跨局验证后，再按 AGENTS 顺序生成保护版和复核交付。

原始样本未启动，original 未写入，没有 commit 或 push。宿主目标保持 active。
