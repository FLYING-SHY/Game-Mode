# KwK38 局部转换前中后证据核验

离线核验通过：同一游戏进程中的一辆 AK 250，其 KwK38 升级队列从 456/480 ticks 降至 244/480 ticks，完成后队列为空且实体、小队蓝图均为 20mm 型。结论仅覆盖这条局部原生转换序列。

游戏 PID `33656`，历史 driver 启动时间 `2026-11-21T03:42:27.6605831Z`；本地 slot1 / identity `1000003e9`。三次 C# 采样为相同记录 EXE hash 的独立短命 runner；每份各 open1 / Snapshot1。没有同一 reader 对象的连续证据，也没有生产 GUI 流程通过的结论。

| 阶段 | 原生画面人工关联 | raw / C# | 队列与蓝图 |
|---|---|---|---|
| run30 initial | 03:10 暂停 | [raw](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-initial-raw.json>) / [C#](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-30-fresh02-kwk38-start/maintenance-live.json>)：456/480 → ceil(ticks/8)=57秒 | queueCount=1；`halftrack_250_ak` |
| run31 mid | 03:36 暂停 | [raw](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-mid-raw.json>) / [C#](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-31-fresh02-kwk38-mid/maintenance-live.json>)：244/480 → ceil(ticks/8)=31秒 | queueCount=1；`halftrack_250_ak` |
| run32 complete | 04:13 暂停 | [raw](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-complete-raw.json>) / [C#](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-32-fresh02-kwk38-complete/maintenance-live.json>)：jobs=[]；remaining 未采得 | queueCount=0；`halftrack_250_20mm_ak` |

活动队列实体 `0x2940d04fe00`，C# key `2834896846336:2807501192`；initial/mid 共用 queue `0x2940a99ae80`、entry `0x2940bb82e00`、item `0x29407ef2fc0`、entryIdentity `4294968297`、entry generation `1`，totalTicks 均为 480。raw entry 的 +48/+52 float 已从 192B 字节直接重解，item vtable 从 64B 字节重解为 RVA `0x3D92058` / `SquadUpgradeProductionItem`；车辆队列类型 `SquadProductionQueueExt`，inventory lookup `0x628A7E2`。

C# key 后缀是源码 entity+96 的 generation；raw entry generation=1 是另一字段。这里仅将 key 的地址前缀独立对回 raw 实体，并检查两次 C# key 完全相同。完成后的 queue 地址变为 `0x2940a999680`；报告保留这一变化，不把完成后队列冒充原活动队列。

三次本地 `ProductionKnown=true`、`Kwk38Known=true`。完成时 raw entity 是 `ebps\races\afrika_korps\vehicles\halftrack_250_20mm_ak`；C# UnitsKnown 的小队路径以及 squad_markers 都为对应 `sbps` 路径。队列空项与转换蓝图共同构成完成证据，缺席项没有补写成剩余 0 ticks。

## 原生截图关联与计时界限

- [fresh02-250-conversion-hover-t.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-250-conversion-hover-t.png>)：03:07，KwK38型自动火炮改装 tooltip; nominal 60s; fuel15。来源：offline human image review; no OCR。
- [fresh02-kwk38-native-timer-start-hover.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-native-timer-start-hover.png>)：03:10，KwK38 tooltip identity; no remaining numeric countdown established。来源：lead native correlation and offline human image review。
- [fresh02-kwk38-initial-paused.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-initial-paused.png>)：03:10，paused; selected 250轻型运兵车。来源：lead native correlation and offline human image review。
- [fresh02-kwk38-mid-paused.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-mid-paused.png>)：03:36，paused; selected 250轻型运兵车。来源：lead native correlation and offline human image review。
- [fresh02-kwk38-complete-paused.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-complete-paused.png>)：04:13，paused; KwK38 completion notification and 250/9装甲半履带车 name。来源：lead native correlation and offline human image review。

截图文本、时间与转换提示是 lead 的原生关联及本次人工图片复核；机器仅固定截图 hash、字节数与 1936×1119 尺寸，没有 OCR。57→31秒属于源码算术换算；尚无原生剩余数字倒计时逐值一致性。tooltip 标称60s与480/8的算术相符也不扩大该结论。

## 源码对应

- 双引擎 [SkyEye Kwk38Upgrade.cs](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/Kwk38Upgrade.cs:8>) / [Simple Kwk38Upgrade.cs](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/Kwk38Upgrade.cs:8>) 完全同 hash：AK vehicles 前缀及 halftrack_250 前缀，IsResearch 精确匹配 `upgrade\afrika_korps\vehicle\20mm_halftrack_250_ak`；Status 仅在 Known 且活动 jobs 非空时生成升级文字。
- [PlayerInfoReader.cs:2702](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2702>) 从车辆分支调用 ReadProduction(vehicle:true)；[PlayerInfoReader.cs:2896](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2896>) 解析 squad inventory 与 +16 队列链接；[PlayerInfoReader.cs:2943](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2943>) 选择 SquadUpgrade guard，[PlayerInfoReader.cs:2960](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2960>) 过滤研究路径并以 entity+96 组成 key，随后追加 Kwk38Upgrading。
- [GameLayout52426.cs:51](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/GameLayout52426.cs:51>) 的相关角色映射为 SquadUpgrade→0x3D92058 / inventory→0x628A7E2；[ProductionJob.cs:15](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/ProductionJob.cs:15>) 将秒数委托给 [GrenadeUpgrade.cs:20](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/GrenadeUpgrade.cs:20>) 的 ceil(ticks/8)。

raw 三份记录的 layout sourceSha256 是 `cd72046d436678632e47840fecd47fe58f4574a54002e57e24b6d8092d44e2e7`；当前完整 layout 原始字节SHA是 `77c44effbf42d9f6300cfd41e753712bed85e551b487f4db1a779df528dc8d92`，这两个完整摘要值保留不同口径。[production_queue_probe.py:24](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/production_queue_probe.py:24>) 实际读取 UTF8 文本并用 regex 解析 mapping，[production_queue_probe.py:62](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/production_queue_probe.py:62>) 执行 sha256(text.encode('utf8'))，没有导入 SOURCE_SHA 常量。Python read_text 的通用换行将当前93处CRLF归一化：4524原始bytes变为4431文本bytes；归一化文本SHA恰为 raw 记录的 cd720…，所以这里不是 layout 内容漂移的证据。component/roots lookup 使用实际解析的 mapping；GUARDS 的候选RVA常量用于标签/coverage诊断。此核对不证明历史 source 闭包或编译 EXE 关联。历史 driver EXE SHA=`cd125bffa33e525cd4b94a753d65a6e4cbb199395e3860e57e85003db7665f79`；本次未读取可变的当前 EXE 或其他运行时 manifest，未导入执行 probe 的 ProcessReader。

## 退出码和整体验收状态

run30 的 [run30-driver-result.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-driver-result.json>) 保留 exitCode=null；[run30-driver-observation-correction.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-driver-observation-correction.json>) 记录 reportDeclaredExitCode=5、osExitCodeAcceptance=false 和原始 Reader reported failure。lead/task-14 记录的 wrapper exit1 作为原始工具观察保留；它不是 runner OS 退出码，OS 退出码仍为未知。run31/32 的 driver 均直接记录 LASTEXITCODE=5。

三份整报告持续为 `maintenanceOnly=true`、`buildVerifiedByThisRun=false`、`FingerprintMismatch`、`visualVerification=not_run`、report exitCode=5；局部核验不会倒写原报告。此结果不代表 F02 全矩阵、GUI、全功能或新增支持基线通过。

## 可重复离线入口

从仓库根运行（默认只检查，不创建文件）：

```powershell
python -B .\CoH3\dev\SkyEye\evidence\version-adaptation\production\verify_kwk38_sequence.py --self-test
```

`validate(root)` 返回确定的核验对象。`--write` 仅用于首轮创建指定 JSON 和本报告；任何目标已存在时退出，不覆写。入口以固定路径清单读取31项输入，按字节数+SHA逐项核验后再检查字段、字节、时序、文本一致性；不打开进程、GUI、游戏、授权或网络。没有修改引擎。

机器结果：[kwk38-sequence-validation.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/kwk38-sequence-validation.json>)。验证器：[verify_kwk38_sequence.py](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/verify_kwk38_sequence.py>)，SHA256 `3d0a73527a833754efd89b5a2d561750c337a36146023957cdd3df1176c61749`。

## 输入清单

| 输入 | bytes | SHA-256 |
|---|---:|---|
| [CoH3/dev/COH3SimpleTool/CoH3MemoryTool/Kwk38Upgrade.cs](<F:/WorkSpace/Game/CoH3/dev/COH3SimpleTool/CoH3MemoryTool/Kwk38Upgrade.cs>) | 920 | `1e56617e2213d1928f2129fe2db6bd97d075a625341036328002759047932e8b` |
| [CoH3/dev/SkyEye/client/CoH3MemoryTool/GameLayout52426.cs](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/GameLayout52426.cs>) | 4524 | `77c44effbf42d9f6300cfd41e753712bed85e551b487f4db1a779df528dc8d92` |
| [CoH3/dev/SkyEye/client/CoH3MemoryTool/GrenadeUpgrade.cs](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/GrenadeUpgrade.cs>) | 1286 | `6a80659f1360a8c578f07d00e22fcf1a69544f6777867b1cb159934733f338c1` |
| [CoH3/dev/SkyEye/client/CoH3MemoryTool/Kwk38Upgrade.cs](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/Kwk38Upgrade.cs>) | 920 | `1e56617e2213d1928f2129fe2db6bd97d075a625341036328002759047932e8b` |
| [CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs>) | 104468 | `bbb359cd59409065d8b4d00ace784243ae5e99816611bc11617e7c9ed8adba9d` |
| [CoH3/dev/SkyEye/client/CoH3MemoryTool/ProductionJob.cs](<F:/WorkSpace/Game/CoH3/dev/SkyEye/client/CoH3MemoryTool/ProductionJob.cs>) | 825 | `7d9dc6ce83868fc2b9a05cf3746b7b454885f4525e05f8fc0a19007cffaf567a` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-250-conversion-hover-t.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-250-conversion-hover-t.png>) | 3032928 | `c28fde01f133560449310e3b7515208603c0d9bffa592a705c2f8f37554211fd` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-complete-paused.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-complete-paused.png>) | 3133891 | `6e3af16cdd5b94dcaea7491533a6d39ad49a3b98abebfc77c5c88fcfcd96b67a` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-initial-paused.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-initial-paused.png>) | 3089380 | `ac9bbd447f8a6bee0cc1b173691beff8ee8661f6e6473beff9bf0705261e2b92` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-mid-paused.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-mid-paused.png>) | 3129145 | `4e19355e3b31157861500bbaadd13b71bea2d6f1fa65ff1ec1886edcb7c4dbaf` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-native-timer-start-hover.png](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/fresh02-kwk38-native-timer-start-hover.png>) | 3093438 | `f18e9b5eb2182d02a187c9607772a1c7d9e93f630d8ca7f2196b9cf0efc6a4e1` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-30-fresh02-kwk38-start/maintenance-live.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-30-fresh02-kwk38-start/maintenance-live.json>) | 10631 | `5376dfeaf421263aa6e46a30c85e0bbe4494a2bc0b0f3e2811cb539a13bb6fca` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-30-fresh02-kwk38-start/maintenance-live.txt](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-30-fresh02-kwk38-start/maintenance-live.txt>) | 8786 | `a701596ba5b37ba3a035b1325df701636b5debb217b3e15e144d8036cb62461c` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-31-fresh02-kwk38-mid/maintenance-live.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-31-fresh02-kwk38-mid/maintenance-live.json>) | 11008 | `763c19a2dd8a24517c29e2e8a0a2e13ffc13dbe06ba0357c4c148642a8c86e6a` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-31-fresh02-kwk38-mid/maintenance-live.txt](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-31-fresh02-kwk38-mid/maintenance-live.txt>) | 9163 | `a616045580276aaf1e1bab630b543129b28b529423f03b33f60bb88c0b495c19` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-32-fresh02-kwk38-complete/maintenance-live.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-32-fresh02-kwk38-complete/maintenance-live.json>) | 10960 | `f91dc5a38298a011ded3cdcf069a557989f65e76b99e5b199ad7af519fc161f9` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-32-fresh02-kwk38-complete/maintenance-live.txt](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/maintenance-run-32-fresh02-kwk38-complete/maintenance-live.txt>) | 9115 | `54496212be87a275b16c96b065df2e58df653a71229870f628727b31321d939b` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-complete-raw.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-complete-raw.json>) | 16468 | `c0649381dbab43fb9c7da126e4d65548e242d4a40a3e86dec196556e8a5b4b1e` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-initial-raw.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-initial-raw.json>) | 19733 | `2f28fa062af5d76579367c99b6e3926ed639831686cccfb63eb20c25d623f808` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-mid-raw.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-mid-raw.json>) | 18093 | `3c8ff4522fc53b66449342e733918f1c7323076267ac9040a2b853f2509c4392` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-start-driver.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/fresh02-kwk38-start-driver.json>) | 973 | `2f94dff4d5b5c5df8fad2f150e4ce7b6376f300678b0cf69c974cf12545ec58b` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/production_queue_probe.py](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/production_queue_probe.py>) | 11785 | `41781ea1bb1cb45bec9ac019054a0aa1d50b6f94cf6619372113f54166917e5a` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-driver-identity.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-driver-identity.json>) | 951 | `6994f95928e7ef9e4a397d08b67bc15e8b517a308093a1f70115b5539f37db29` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-driver-observation-correction.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-driver-observation-correction.json>) | 1477 | `7fba90a903d72959aad7b5a4ce4566c9e8ed9a5c86344d0ed058e68d2db6e017` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-driver-result.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-driver-result.json>) | 254 | `18d303c80b1dc98d0b901b1803273966c30fcd4bc2aad2b77aec43a8bb907f3b` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-stderr.txt](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-stderr.txt>) | 0 | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-stdout.txt](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run30-stdout.txt>) | 377 | `1df8a5066b83ddfef681617937029642ea96e833a9108a064560aee719e1785a` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run31-driver-result.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run31-driver-result.json>) | 835 | `7454c3e413c4d9cd9a38123ebd8a2fd57f63bb840dbcd5b1ae8d7e457d928d6a` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run31-stdout-stderr.txt](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run31-stdout-stderr.txt>) | 760 | `a3aa68d8f561007b905afd45b26514f4ffc84150fd162b78d91c26c66ddd2fb3` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run32-driver-result.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run32-driver-result.json>) | 849 | `1a0d8757c5e694a4ae7bc51f71744382e7f0a4515fdbbfde8242c5ee72784e3c` |
| [CoH3/dev/SkyEye/evidence/version-adaptation/production/run32-stdout-stderr.txt](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/production/run32-stdout-stderr.txt>) | 770 | `a6c5e0448941efed1c61780dba0255dba532d1bc818dfe3900fde3108fe8b26f` |

共 31 项 / 15724713 bytes。验证器自己的 hash 单列，输出文件不作为自己的输入。
