# 5.1.52426.0 当前验收矩阵：candidate，不登记新 baseline

## 1. 口径、范围与结论

本文件初稿为截至所列工件快照的独立离线审阅；初稿审阅本身没有附加游戏或运行GUI/采样器。后续由lead按注明的实际运行工件追加增量，各段保留其历史范围与限制。适用游戏 SHA256：`8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC`。依据原始材料而非执行意图判定；后续生成的证据不自动并入本快照。

**结论：新版维持 `candidate / not verified`，未登记新基线，未生成新版保护交付。两客户端 Release 指纹门控各通过 27 项离线检查；这不代表新版功能通过。Run33 再次观察到战绩页残留；完整跨局、会话与缓存清理、全功能和正式交付验证均未完成。**

等级含义：S=静态身份/调用链/候选；F=隔离fixture；G=实际游戏受控场景；R=新进程/第二局/恢复重复；D=最终交付目录与保护版验证。等级和结果分开：`observed`不是`pass`；`sample_absent`仅表示此采样未取得合格样本；`not_run`表示用例未执行或缺闭环。编译、授权、unique signature和窗口存在均不替代游戏功能通过。

范围以[原始F01–F20矩阵](<test-matrix.md>)为准；其首轮全部not_run是历史起点，后续结果在本文件追加，不回写成当时已测。[阶段进度](<progress-20261121.md>)也是中间状态，decoder与事件后续证据见下文。

## 2. 核心证据台账与适用限制

| 证据 | 已明确事实 | 等级/结果与限制 |
|---|---|---|
| [静态报告](<static/REPORT.md>)、[signature校验](<static/signature-validation.json>) | 30 patterns、1122 raw hits，8类RTTI链/关键解码离线一致；0/multi/unique都不自动选址 | S及扫描器F PASS；不等于RVA运行全场景有效；升级/Loiter角色仍缺事件 |
| [run07 JSON](<maintenance-run-07-decoders/maintenance-live.json>)、[文本](<maintenance-run-07-decoders/maintenance-live.txt>) | UTC 2026-11-20 17:42:14起；真实lease active；resources2、units12、buildings2、production1、upgrades8、colours2、camera1、markers12、minimap1 | G observed；`FingerprintMismatch`、`buildVerifiedByThisRun=false`、`visualVerification=not_run`、exit5保留。一次snapshot，非跨feature原子时刻 |
| [生产诊断](<production/REPORT.md>)、[run04](<maintenance-run-04-live-production/maintenance-live.json>) | live SpawnProductionItem/jaeger；受控本方工程兵订单、资源扣减及剩余tick观察 | G部分；原13/240暂停样本不证时间进展；run07为85/200 ticks、ceil(/8)=11秒，视觉计时换算仍未校准 |
| [marker诊断](<markers/REPORT.md>) | 旧zero源于runner未绑ColourKnown；12 sources/36 members合法，修正runner后12 markers | S+F及G观察；不是无单位。run07显式开启并恢复FlankReadMovement47，有3条Movement、迫击炮与AA Direction；位置/目标时序尚未闭环 |
| [preflight结果](<../../tests/layout-preflight/results.json>)、[说明](<../../tests/layout-preflight/README.md>) | 两树各Maintenance94/Release8 PASS；37key逐个移除均抛含key异常；两构造在RequireCandidate后、原LicenseGate/OpenProcess前校验 | F PASS；只防缺映射在逐对象catch中消失；不保证所有读失败/结构错误可见；reader源顺序为静态断言，不执行native构造 |
| [decoder说明](<../../tests/decoder-layout/README.md>)、[Release结果](<../../tests/decoder-layout/bin/Release/net10.0/fixture-report.json>)、[Maintenance结果](<../../tests/decoder-layout/bin/Maintenance/net10.0/fixture-report.json>) | 最新118+170=288 PASS；原89+97=186断言完整保留；5入口/7guard/4角色、真实mortar及184B P47回放、异常传播和负对照 | F PASS；计数随新增Mover形状回放扩展。没有链接reader/native/GUI；helper异常到runner分类端到端仍not_run |
| [早期弹体诊断](<projectiles/projectile-report.md>) | 3个live ProjectileDynamics actor及backref真实吻合；2inactive、1不支持flamethrower | G对象身份observed；不是目标炮弹功能通过 |
| [事件02/03分析](<projectiles/event-report.md>)、[02 raw](<projectiles/event-30s-02.json>)、[03时间序列](<projectiles/event-30s-03.json>) | 02有1个mortar raw、mode16=2/state64=3、radius4.5，实际C#离线回放通过；03有55帧资源向量变化、稳定roots/registry、0target | 02 G采样+F回放；焦点/自动冻结使运动结论撤回。03证游戏数据推进，但炮弹candidate=0、targetTimeline空，运动/落点仍not_run |
| [地图视觉审阅](<visual-map-review.md>) | 当前战术760×760边界/中心及本方HUD整数值一致，小图可见皮肤比native边界内缩2px | G单场景观察性对照；无独立已知3D地标/地形高度，非world-to-screen全校准 |
| [维护GUI构建报告](<../../tests/maintenance-ui/build-report.json>) | UTC17:46构建hash `1E7A736DECFAE704CBD6C19F38A6BDEB50BE3B7776EEC1F82EC5B8F24775F001`；仅3生成调用点替换，正常入口/授权与配置保留 | 构建/F元数据通过，报告本身guiRun=not_run；后来GUI图为独立G证据。此构建早于最终preflight，不把运行中旧binary说成最新源已执行 |

初稿及早期GUI证据指向同一game PID11192、start 2026-11-21 00:43:18、既有离线AI对局；其中GUI JSON的pid32392是维护客户端PID，反复重开reader/runner/client不算fresh游戏进程。后续另取得game PID31572和33656两次fresh进程身份及第二实际场景，见3.9；这项增量不追认早期GUI为R通过。

## 3. 新增GUI工件：亲眼所见与未推定事项

已读取六份ui JSON：[all-on](<ui-all-on-01.json>)、[minimap-off](<ui-minimap-off.json>)、[world-off](<ui-world-off.json>)、[tactical-on](<ui-tactical-on.json>)、[world-mortar-on](<ui-world-mortar-on.json>)、[world-observed-off](<ui-world-observed-off.json>)。它们记录UTC、同一客户端PID32392、hwnd/rect及PNG路径，不含feature pass断言、game start/MatchKey或客户端hash绑定。tag是操作标签，不当成操作成功证明。

本审阅实际用read_image查看以下PNG（未查看的其他PNG不据名推断）：

| 已看图 | 可直接观察 | 仍未证明 |
|---|---|---|
| [world层9636558](<ui-world-mortar-on-9636558.png>) | 黑底独立透明层截图上有红色突击掷弹兵圆环/文字、摩托方框 | 与同刻游戏单位逐点精确对齐、镜头移动/遮挡/退出清理 |
| [mine层265346](<ui-world-mortar-on-265346.png>)、[world-off后同层](<ui-world-observed-off-265346.png>) | 两枚红色“敌雷”符号；两图同SHA256，world主标记关闭后该独立层仍保留 | 不是总开关失败的充分证据：原矩阵说明mine独立更新。未验证放雷/引爆、己方颜色、mine自身关开 |
| [mini层199812](<ui-world-mortar-on-199812.png>) | 左下有红点/三角分布 | 与单位逐点对应、移动目标线、边界裁剪、弹幕/V1 |
| [tactical层199812](<ui-tactical-on-199812.png>)、[游戏战术画面](<ui-tactical-current.png>) | 独立层有中央战术标记/单位名称及左下小图点；游戏画面确为战术地图并有暂停提示 | 两张不是同刻合成截图；未做逐点像素校准/地图关开清除与恢复时序 |
| [顶部信息层855314](<ui-world-mortar-on-855314.png>) | 有敌方资源/人口/CP、战斗群/单位文本；`pioneer_autobuild_ger`仍显示原key | 不是完整汉化/所有字段正确；未做同刻敌方HUD对照、阈值闪烁/排版全场景 |
| [minimap-off面板](<ui-minimap-off-2033744.png>)、[world-off面板](<ui-world-observed-off-2033744.png>) | 前者小图开关off/world on；后者world off/mini on；两者顶部信息on且显示待适配 | 开关状态本身不是feature结果。JSON窗口列表变化支持窗体出现/消失观察，不代替像素与恢复新帧验证 |

以上为维护SkyEye GUI的G observed，不是正式SkyEye保护版或SimpleTool实际UI回归。黑底独立层截图用于证明“有绘制”，不证明用户实际桌面合成效果、透明/点击穿透、多屏/DPI或真实运动。面板中声音/空袭开关亮着同样不证明已产生对应事件或声音。

### 3.1 输入坐标与暂停条件纠正（lead后续核查，保持旧记录）

lead提供的现场条件：virtual display `[0,0,2560,1440]`，原game rect `[-1219,157,717,1276]`多数位于屏幕外；旧client鼠标坐标换算为负screen坐标后会被clamp到0。因此旧scout选择/战术图鼠标move的操作意图不构成成功输入证据，相关移动用例继续not_run。先前F1+W键HQ生产另有真实队列/扣资源证据，不随鼠标问题一并撤回。

lead已将game移到`[0,30,1936,1149]`而保持client1920×1080。本审阅实际查看[scout-visible-display-selected.png](<scout-visible-display-selected.png>)，可见16:01暂停、侦察小队3/3被选中；实际查看[scout-verified-order.png](<scout-verified-order.png>)，可见战术暂停下禁止地图命令提示。因此这是有效选择+输入前置条件纠正，不是移动通过。event04由lead串行恢复/发令/采样正在执行，本文件尚未读取其结果，不预判。

旧UI面板位于primary display，其开关点击不受上述game负坐标问题同等影响。world-on JSON含hwnd9636558、world-observed-off JSON无此hwnd，结合已看面板world off，支持“该主标记窗消失”的观察；mine265346独立存留，不将两者合并成一个总闸。地图/单位运动、重新开启获得新帧及退出清理仍须后续闭环。

### 3.2 增量run08–12：授权、实际移动/停止与HQ技术角色

- [run08](<maintenance-run-08-events/maintenance-live.json>)为`licenseState=in_use`、AUTHORIZATION_TIMEOUT、readerOpenAttempts=0、snapshotAttempts=0、exit3。lead确认同产品SkyEye GUI32392持有会话；这是授权并发条件，不计G读取失败或功能通过。关闭该客户端后，[run09](<maintenance-run-09-events-single-client/maintenance-live.json>)真实active、单reader，30秒276 polls（总snapshotAttempts277），实际C# Snapshot/ReadProjectileImpacts decoded0、event_window=sample_absent、exit5。零输出不证明无弹体或预测路径通过。
- [HQ升级报告](<production/hq-upgrade-report-01.md>)独立解包真实192B entry：remaining474/total480，item为EntityUpgradeProductionItem `0x3D92160`、blueprint `upgrade\\american\\research\\mechanized_support_center_us`。这是新增G角色/字段观察，撤回“EntityUpgrade只有静态身份”的旧现状。474/480按8ticks/s为59.25/60秒，与tooltip60s尺度相符，但没有倒计时到完成闭环。run10 ProductionKnown=true/jobs空的原因是现有产品输出契约只列Spawn、Grenade、Kwk38，不泛化输出所有tech；不应为验收增加通用技术树功能。SquadUpgrade/Kwk38与特定Grenade事件仍未测。
- 实际读取[run10](<maintenance-run-10-hq-upgrade/maintenance-live.json>)、[run11](<maintenance-run-11-engineer-move/maintenance-live.json>)、[run12](<maintenance-run-12-engineer-stop/maintenance-live.json>)本方engineer同路径三条：run10位置`(-111.75,24.968071,91.75)`、Movement=null；run11位置`(-111.393463,24.8977585,90.50346)`、Movement=`(-100.547348,24.5778122,69.72629)`；run12位置`(-110.755867,24.76283,88.42286)`、Movement=null。lead操作为地图右键+短resume，再W停止；**不是自然到达**。这是G命令→位移/目标出现→主动停止目标消失的单例数据闭环，非只有“字段有值”。
- 本审阅亲看[移动命令图](<engineer-move-command.png>)（可见地图工程兵附近白色目标线）、[移动后暂停图](<engineer-move-world-paused.png>)（19:19工程队4/4、队列有图标）、[停止暂停图](<engineer-stop-paused.png>)（19:20、队列空）。暂停提示只描述截图当刻，不足以反驳先resume发令再pause的串行操作；判定结合上述实际坐标/Movement变化，而非据warning单独判命令失败。
- 独立复算原战术affine：`u=-1.7302424e-7*x-1.979166509*z+959.999989`，`v=-1.979166509*x+1.7302424e-7*z+539.999995`，将run11目标X/Z代入得client`(822.0000684,738.9999508)`。lead记录右键client`(822,738)`，差约`(0,+1px)`，支持一次指令目标与地图affine在±1px一致；可能含navgrid/snapping，不升级为全地形/world-to-screen/任意旋转校准。主/小图工具连线的实际动态绘制仍缺闭环。操作顺序及局限另见[受控movement报告](<movement-control-01.md>)，与本次独立复算一致。

### 3.3 Simple维护UI：实际绘制、两个开关闭环与同局重开

读取[initial](<simple-ui-initial.json>)、[top-off](<simple-top-off.json>)、[world-off](<simple-world-off.json>)、[mini-off](<simple-mini-off.json>)、[restored](<simple-restored.json>)。客户端PID32148，lead记录正常授权自动通过；不是新游戏PID。

- top HWND200036：initial有、top-off无、restored有。本审阅亲看[restored顶部PNG](<simple-restored-200036.png>)：敌方79MP/116mun/227fuel/人口56/CP0与单位、已解锁文本实际绘制。故“顶部层隐藏→恢复且有文本”是本轮G局部闭环，不只开关变色。
- mini HWND265322：initial有、mini-off无、restored有；亲看[restored小图PNG](<simple-restored-265322.png>)有红点/三角。支持小图层隐藏→恢复绘制，仍未验证每个单位对应/动态目标/旋转边缘。
- 亲看[restored控制条PNG](<simple-restored-330702.png>)，三开关均恢复on、label待适配；其底部同时显示“数据更新中，正在快速重试: 镜头已切换。”，所以不把截图推断成持续无错读取或恢复性能通过。world在此机位未取得已绘制主窗口的正例，world toggle状态变化不是主画面绘制闭环。
- [normal-close记录](<simple-normal-close.json>)：UTC18:25:18，PID32148 normalCloseRequested=true、processStillRunning=false、selected-PID可见窗探针无窗；exe SHA256 `55283309B3F8DEEE9C89B3DFA0D4C060E611665766C72B32B18B8A2ED0D7FDE4`。正常关闭后进程/可见窗清理observed，不声称句柄/所有后台任务/服务端租约释放都已测。
- [重开JSON](<simple-reopened.json>)的client PID30868有新top2886206/mini396390；本审阅亲看[新top PNG](<simple-reopened-2886206.png>)有同一暂停现场79/116/227/人口56等文本，lead确认三开关恢复。记同局客户端退出重开R小项observed；**游戏仍PID11192，不抵扣第二局或两fresh-game-process**。[第二次正常关闭记录](<simple-reopened-normal-close.json>) UTC18:29:16确认PID30868 processStillRunning=false、gamePid11192、freshGameProcess=false。

### 3.4 实际炮击画面：显示、原生爆炸与消退（lead增量）

[实际画面报告](<projectiles/visual-live-report-01.md>)绑定游戏PID31572和正常授权维护UI23716；[完整性记录](<projectiles/visual-capture/live-01-integrity.json>)核对39个完整组、234张图、全部大小/SHA/窗口归属。采集39.062秒后按80MiB预算停止，零部分写入，保留确切丢弃计数。lead亲看关键desktop、game和world图，独立只读审阅覆盖前8组关键帧。

实际桌面group-001同时显示“黄1/2”预测圈；group-002原生game图在此前1号圈附近出现火焰/烟尘，其后world和desktop仅剩2号；group-006 world两圈均清除，desktop显示右侧爆闪，其他单位标记仍更新。记`G-PROJ-VIS-01=PASS_scene_display_sequence_only`：取得真实桌面绘制、原生爆炸与图形消退的定性场景闭环。不同图片依次采集，非原子同刻；无同步逐弹身份，不将该窗口的编号关联到独立run24的entity/Id/Index，不推定精确命中误差、预警时长或删除内部原因。

UI正常关闭后PID23716消失，测试偏好从字节备份恢复SHA`2D1460E714C11BEEC9BA495344D3C935377D51BB46811F8F9DD8D2AC5BA33727`（含原`squadMarkers=0`）。该增量仍不是F09全功能、保护版或新baseline通过。

### 3.5 实际P47施放：计数修复、payload与镜头异常（lead增量）

[run25–26报告](<air/run25-26-air-evidence.md>)记录两次原生US空中支援中心施放。run25真实551次air查询有83次正观测，但旧策略已被128个最早空帧占满，全部83条正payload被丢弃，故[run25复核](<air/run25-reader-counter-validation.json>)只接受计数。维护sampler随后保留最早/最新空帧和最多126正帧，128帧/256decoded/每帧16上限保持；54项基础/event/air隔离校验通过。

[run26](<maintenance-run-26-p47-retained/maintenance-live.json>)使用修正后的真实授权C# reader，428次完整air查询保留68帧/66decoded，360个替换空帧、0decoded丢失。[逐项复核](<air/run26-reader-prefix-validation.json>)将66条输出与raw07原始cast的owner、ability、instance3481、path、target bytes组成的key精确对上；XYZ `(34.4120636,20.2007885,1.86473608)`、方向 `(0.999969959,0.00774675328)`、半宽5/长45/offset0一致。只有一个非`flight:` key，属于一次P47的timed-cast重复观测；ordinal12–77正例后ordinal78空读。该维护诊断显式使用all-sides，生产enemy-only GUI尚未据此通过。

完整60秒窗口在46.780秒、ordinal429因`System.Exception: 镜头已切换。`失败，维持`fail / stopReason=exception / air invalidated / exitCode3`；已成功的历史前缀不升级为完整窗口通过。随后同一reader读取有效小地图只证明地图路径继续可用，它绕开镜头重发现，不当作Snapshot/生产GUI恢复。driver的OS子进程exitCode未留存，外层exit0不替代报告状态。

本节当时还确认MoverDynamics静态分配184B，而旧两处consumer读284B并取+272/+280。raw04/05实际位移及raw08近正交cast均支持+96/+100为平面方向，旧字段位于分配边界之外。后续候选修正与新事件验证见3.6；本节run26的完整窗口失败结论保持。

### 3.6 实际flight fallback及完整有界窗口（lead增量）

[run27报告](<air/run27-flight-live-report.md>)及[逐项复核](<air/run27-flight-window-validation.json>)绑定冻结的正常授权runner、game PID31572/start与两树镜像核心。Mover形状显式选择184B/+96/+100，RecoverStrafe与ReadLoiter49共用；只在维护编译且maintenanceDiagnostics=true启用。隔离fixture扩为118+170=288PASS，全部旧186断言保留；双树四配置guard144、preflight204及runner87检查通过。正式版本登记仍待后续G/R/D决策。

真实C#20.014秒窗口184查询全部完成，event_window=observed、stopReason=deadline；air保留46frames/44decoded、138替换空帧、0decoded丢失。44次正观测只有一个`flight:2188088899584:1000008712` key，目标XYZ `(34.41206,20.2007885,1.8647356)`、方向 `(-0.158670262,0.9873316)`、半宽5/长45一致。ordinal7/645ms首正例，ordinal50/5331ms末正例，ordinal51/5439ms归零。native推进约10.982秒后恢复暂停；窗口剩余部分是暂停下读取，不写成20秒游戏模拟推进。

同窗raw11/12精确对应entity `0x1fd7442f800`、generation1000008712、index2963，沿原攻击方向从目标前98.821m飞至目标后42.732m；TimedAbility已phase2/instanceFFFFFFFF，其原始目标、方向、owner仍与C#条带吻合。后续暂停raw13中同代飞机仍存在，位于目标后170.081m，明确标注为窗口之后状态。raw12的actor已改为离场目标，但C#保留条带仍是原攻击目标。本例取得实际飞行补救输出、完整reader窗口及归零；不将44观测算作44次施放。

本例仍为all-current-row维护诊断、本方slot0；enemy-only GUI、Loiter/V1和完整飞机功能待测。没有自然camera异常，same_reader_recovery=not_triggered，零恢复读取；OS子进程及报告exit5一致，其他缺样与not_run保留。本节当时的维护UI已另行重建、0warning/0error、生成5checks与离线校验通过；后续GUI执行另见3.8，不回写为run27的生产GUI验证。

### 3.7 Run28自然镜头异常后的有限同reader恢复

[原始run28](<maintenance-run-28-camera-recovery/maintenance-live.json>)、[恢复报告](<air/run28-recovery-live-report.md>)及[87项离线复核](<air/run28-recovery-validation.json>)记录game PID31572上的一次自然`System.Exception: 镜头已切换。`。配置60秒，事件计时10586ms时ordinal98的Snapshot异常，只有97轮完成后续查询；维持`event_window=fail / stopReason=exception / air invalidated / exitCode3`，driver实际子进程exit3也保留。air的97次查询均为零，保留最早/最新2空帧、95替换空帧；23条projectile observation不计作23个独立事件。

独立`same_reader_recovery=observed`：同一个实际reader直接`ReadMineCamera`1次、随后`Snapshot`1次，在探针自身6ms内成功，首轮即停止；配置上限3轮/合作式2000ms预算。readerOpenAttempts始终1，六组SameReader/Valid=true、原Match与epoch1及Allowed=true不变；[ReferenceEquals守卫](<../../tests/maintenance-live/RecoveryProbe.cs#L114>)支持对象同一性，identity hash仅作标签。camera返回两组各16个有限float矩阵，Snapshot返回2行、本方slot0/identity3e8；该次本方地址与原Match对应。总Snapshot尝试100次=初始1+事件98+恢复1，探针显式camera计数不包含Snapshot内部读取。

复核只接受`readerRecoveryAccepted=true`；`originalEventWindowAccepted / productionGuiRecoveryAccepted / gameFeatureAccepted / protectedDeliveryAccepted`均false。原窗口失败未被改为成功；随后地图是独立探针。没有camera指针前后原始dump，单次恢复不证明生产GUI重试或跨局重连。原始报告仍`FingerprintMismatch`、`buildVerifiedByThisRun=false`、`visualVerification=not_run`。

### 3.8 Run29有限GUI隐藏/恢复、战绩页残留与主菜单清理

[身份记录](<gui-run29-identity.json>)绑定维护UI PID24288/start `2026-11-21T03:16:36.5587055Z`、SHA256 `F06F3BD1625A45E314B20F4B6AB05EEF1C4DD871C15BCD0B062AB9A10D9946FA`和game PID31572。[图层报告](<gui-run29-layer-live-report.md>)与[235项离线一致性检查](<gui-run29-layer-validation.json>)给出三个有限状态：`guiHideRestore=PASS_limited_GUI_window_capture`、`resultPageResidual=unresolved`、`menuCleanup=observed`。原world-on为开关仍关的失败attempt，world-enabled亮开关时也尚无世界层，首个正例是enemy-view。

[enemy-view](<gui-run29-enemy-view.json>)中世界HWND3869816绘出八轮/框/实线与虚线，小图2689582绘出红点/三角/路径；[off](<gui-run29-world-off.json>)时世界层退出该次可见集合，[restored](<gui-run29-world-restored.json>)时返回，世界PNG与正例SHA256相同。小图在三批中保留且PNG相同。这支持隐藏/恢复观察；暂停12:31下的分窗PrintWindow不证明刷新新帧、桌面合成、运动/投影精度或same-reader。辅助986248为全黑帧，不据此接受mine功能。

[名为after-match-menu的记录](<gui-run29-after-match-menu.json>)实际关联[战绩/比赛数据页](<lifecycle02-first-menu-after-match.png>)；world/top仍留存与正例相同的旧图，面板仍有旧敌方统计，**战绩页残留继续open**。后续[真正主菜单](<lifecycle02-lobby-before-restart.png>)的[可见集合](<gui-run29-lobby-before-restart.json>)只有panel5113760，敌方列表空、提示悬浮层已隐藏。此项仅观察到显示清理，不证明reader释放、窗口销毁、托管对象同一性或epoch/cache清空，也不关闭战绩页残留。

### 3.9 两次fresh游戏身份与第二实际场景的raw/GUI新值

[first identity](<fresh-process-01-identity.json>)记录game PID31572/start `2026-11-20T18:44:29.8492330Z`；[正常退出记录](<lifecycle02-game-normal-exit.json>)显示其在10秒内退出、游戏进程列表为空，UI24288继续运行。[second identity](<fresh-process-02-identity.json>)记录新game PID33656/start `2026-11-21T03:42:27.6605831Z`、oldGame31572Absent=true；两次game SHA256均为本文件所列值，moduleBase均`0x7ff7a37e0000`、size120369152。该second identity的scope仍是`process_identity_only_match_not_yet_sampled`，不把随后场景倒填为身份采集时已完成。

第二实际场景由后续[设置图](<fresh02-final-skirmish-settings.png>)、[00:06战术暂停图](<fresh02-initial-paused.png>)及[raw roots](<fresh02-live-roots.json>)另行支持。raw在2026-11-21T04:15:47.370682+00:00记录PID33656、arena `0x29407e00000`、worldOffset `0x6400000`、stableRootsAfterRead=true；player slot0/address `0x29410a08000`/identity `0x3e8`具有US实体，slot1/address `0x29410a09000`/identity `0x1000003e9`具有AK实体。该probe仍标记`candidate_not_baseline / candidate_only`。

[同GUI采集](<gui-run29-fresh02-initial.json>)在2026-11-21T04:15:46.917667+00:00记录UI24288及原top/panel HWND；[新顶部文本](<gui-run29-fresh02-initial-2230404.png>)显示敌方US工程兵与侦察兵、资源1114/500/300、人口4/CP5，与raw slot0的1114.29736328125/500/300.6295166015625、population4/command5的显示整数相符。本方AK暂停HUD1420/500/315、人口4/CP12也与raw slot1相符。这是保存记录支持的同GUI进程新玩家值观察，非原子同帧，也没有GUI reader对象/重连分支跟踪。

已有两次fresh身份和第二实际场景，不再记作只有旧PID反复采样；但第二场景这里只有独立raw+GUI，**未取得该场景的实际C# Snapshot/MatchKey/epoch提交证据**。完整跨局重连、旧缓存/标记清除和R用例仍待测。该[初始面板](<gui-run29-fresh02-initial-5113760.png>)底部仍显示旧“地雷标记：信息地址无效。”，新资源值不抵消这条残留。

### 3.10 Task10 地雷旧错误文本的一行源码修复

仅在[SkyEye ClearData](<../../client/CoH3MemoryTool/MineOverlayWindow.cs#L127>)及[Simple ClearData](<../../../COH3SimpleTool/CoH3MemoryTool/MineOverlayWindow.cs#L127>)各插入`LastError = "";`，位于camera置空之后、mines.Clear之前；UpdateRows/RefreshFrame、读取与活动异常路径保持原逻辑。两源逐字节一致，SHA256均`3c61f6bd4a3b7df1887128dbd57434e6d77814a0e2ad1d3751dd2def1f713d03`，task-only diff各+1/-0，逆补丁恢复编辑前字节与hash，diff-check通过。

此项只修复 `ClearData` 时清空旧错误文本。Task10 当时的 UI24288 尚未包含修复；后续 Run33 已运行包含修复的候选，未取得先触发同类错误再清理的回归，故该回归仍为 `not_run`。

### 3.11 Run33 有限界面观察与正常关闭

[Run33 身份](<gui-run33-identity.json>)绑定 SkyEye 维护界面 PID7672、游戏 PID33656 和候选二进制。默认 [F8](<gui-run33-copy-result.json>) 在游戏居前时复制了所选敌方文本；未测改键、冲突和退出后按键注销。[面板居前](<gui-run33-panel-foreground.json>)与[游戏最小化](<gui-run33-minimized.json>)时只见设置面板，[恢复](<gui-run33-after-minimize.json>)后图层重现。这些是窗口可见集合的观察，不证明 reader 释放或重新连接。

[原生战绩页](<gui-run33-game-after-leave.png>)之后，[延迟窗口采集](<gui-run33-results-delayed.json>)仍有世界层和顶部层，`resultPageResidual=unresolved`。[正常关闭记录](<gui-run33-maintenance-close.json>)确认 UI7672 收到 `CloseMainWindow` 并退出；不是战绩页自动清理、热键注销或新局重连的证据。

### 3.12 正式 Release 指纹门控离线复测

[双客户端 Release 夹具](<release-gate-check-01/result.json>)各通过 27 项检查：确认旧支持指纹未变，未知指纹和读取失败停止结构依赖功能，Release 不因维护参数放行；另以源代码断言确认两个构造均在打开读取句柄前完成版本门控。只验证夹具和源码顺序；未构建完整 Release 客户端，未附加游戏，未验证新指纹或保护交付。

## 4. F01–F20 当前功能矩阵

| ID | 当前可引用状态 | 尚待完成的具体闭环 |
|---|---|---|
| F01 基础玩家信息 | G observed；本方HUD一次对照、两玩家raw、资源03连续变化 | 敌我/队友筛选与受控消耗多场景、开关恢复、双UI |
| F02 单位/建筑/战斗群/生产/升级 | G observed：spawn/HQ EntityUpgrade；新增[US Grenade闭环](<production/grenade-live-report-01.md>)raw+产品reader187→118/200ticks→queue0/完成列表，UI完成通知；[KwK38局部序列](<production/kwk38-sequence-report.md>)同实体/队列456→244→queue0，车辆蓝图切换为halftrack_250_20mm_ak | 全F02仍未完成；KwK38三次为独立短命reader，57→31是tick换算、完成后remaining缺席，非生产GUI倒计时证据；Grenade工具可见文本/数值倒计时/声音及其他单位类别仍待测 |
| F03 颜色/渐变/闪烁/布局 | G colours2、红色文本渲染observed | 手动回退、阈值越界/恢复、长文本/字体保存重启 |
| F04 主标记 | G圆环/方框/名字实际绘制observed | 同刻点位/地形校准、平移缩放/离屏/遮挡、开关恢复无旧帧 |
| F05 移动目标 | G单例闭环：本方工程兵null→命令后位移/目标→W停止后null；目标地图投影与指令点约1px一致 | 自然到达自动消失、车辆/其他mode、工具连线/终点实际动态绘制；单点非全地图校准 |
| F06 朝向/射界/射程 | G mortar Range300/AA50 Direction observed | MG架设/转向、ArcValid、实际与备用示意区分、画面方向/长度校准 |
| F07 地雷/炸药 | G两枚“敌雷”渲染observed | 受控放置/清除/引爆、双方归属颜色、独立开关隐藏恢复 |
| F08 弹幕目标范围 | S TimedAbility+G存在对象，F guard路径 | 技能施放→范围显现→结束清除与负对照，地图/主画面均留证 |
| F09 逐发预测 | G mortar raw+F真实bytes Decode PASS；[run24 reader](<projectiles/run24-live-reader-report.md>)50s/459polls、4个Wespe代次/128观测；新增[实际画面限定闭环](<projectiles/visual-live-report-01.md>)：桌面1/2圈→原生爆炸→先1后2清除，其他标记继续更新 | reader与画面为独立时间窗口，仍缺同步逐弹ID/定量命中误差/预警时长及其他武器；不把画面编号关联到run24身份，产品坐标是预测点 |
| F10 飞机/V1 | S角色/F288；[run26](<air/run25-26-air-evidence.md>)历史timed-cast66条/窗口fail保留；[run27 flight](<air/run27-flight-live-report.md>)取得真实184B fallback、184查询/44同key观测→归零及完整20s reader窗口；[run28](<air/run28-recovery-live-report.md>)另观察到自然镜头异常后的同reader camera+Snapshot单次恢复 | enemy-only GUI条带；**Loiter/V1**不同事件、方向/宽长/生命周期、清空后新事件、主/小图；run28未使原event window或production GUI通过，本方all-sides诊断不替代这些项目 |
| F11 小图 | G native affine、点/三角绘制与off状态observed | 关→开新帧、移动/弹幕/V1、边界/旋转/布局重连 |
| F12 战术地图 | G打开布局/标记层observed | 关图清除/重开对齐、标记开关组合、实际同刻点位 |
| F13 SLD/支援指令 | F Active路由PASS；G run07 support=sample_absent | **真实支援/SLD新事件触发一次**、首次连接baseline不补报、换局seen清空与非目标负对照 |
| F14 七声音 | 本轮只有UI可见；游戏触发/试听not_run | SkyEye逐类真实事件、开关/重复/静音/退出停止；SimpleTool无此UI恢复项 |
| F15 热键/托盘/剪贴板 | [Run33 默认 F8](<gui-run33-copy-result.json>)有限通过：剪贴板内容与所选文本相符 | 改键、冲突、退出注销、托盘恢复均未验证（仅SkyEye） |
| F16 包抄 | reader采样flag观察≠提醒通过 | 接近/离开/朝向、冷却、F9与距离圈、换局对象清除（仅SkyEye UI） |
| F17 性能 | 单次读耗时与采样预算有记录 | 真实FPS/P95/数据年龄、多线程负荷、长时间位置采样率 |
| F18 生命周期 | 两次fresh进程身份；Run28有限同reader恢复；Run29主菜单显示清理；Run33面板居前、最小化、恢复与正常退出有记录。Run30–32是第二场景的独立C# Snapshot | 独立快照不证明生产GUI跨局提交、释放/自动重连或epoch与缓存清理；Run33战绩页world/top残留仍未修复 |
| F19 偏好/三开关/退出 | SkyEye与Simple均有有限历史窗口证据；[Run33 正常关闭](<gui-run33-maintenance-close.json>)确认SkyEye维护进程退出；Run33候选包含task10的ClearData改动 | 未复现旧错误后验证清空，Simple当前候选未运行；战绩页自动清理、更多偏好持久化、授权租约释放仍未验证 |
| F20 双工具裸/保护交付 | 新版D `not_run`；[Release门控](<release-gate-check-01/result.json>)各27项离线检查通过；Simple task15 [隔离候选元数据](<../../tests/maintenance-simple-ui/task15/compiled-metadata.json>)为reflection-only，Main未执行、auth/gameAttach/guiRun均`not_run` | 未登记新指纹，未构建或发布新版双保护交付；门控夹具不代表正式二进制已验收 |

## 5. 阻止登记新 baseline 的明确缺项

1. **R证据未完成：**已有两次fresh游戏进程身份、第二场景的raw/GUI观察及Run30–32独立C# Snapshot。未记录生产GUI跨局提交、epoch变化、自动重连和旧缓存清理。Run28的一次同reader恢复、Run29的主菜单显示清理，以及Run33正常关闭均不替代这些检查。
2. **升级功能闭环仍不足：**[US Grenade真实闭环](<production/grenade-live-report-01.md>)与[自动复核](<production/grenade-sequence-validation.json>)已PASS原始/产品reader限定范围：同queue/entry/item187→118/200ticks→queue0，精确grenade_riflemen_us进入本方完成列表，游戏02:22实际显示手榴弹装备包已完成；不是只有HQ泛化tech。[KwK38局部序列](<production/kwk38-sequence-report.md>)现已通过独立离线核验：AK250同实体/队列456→244→完成后queue0，entity/squad蓝图切换到20mm型；仍不是同reader连续读取或生产GUI流程。UI数字倒计时/声音/工具可见文本、其他单位类别仍待对照；泛化tech本来不输出，不擅自扩展验收契约。
3. **空袭与支援闭环仍不完整：**[实际P47 run25–26](<air/run25-26-air-evidence.md>)已取得原生施放、真实all-sides C# timed-cast历史payload及后续空读，不再记作完全未覆盖；run25保留缺陷已修正并通过54项自检。run26窗口因镜头切换明确fail，其后同reader地图成功绕开camera，不替代Snapshot/GUI恢复。后续[run27](<air/run27-flight-live-report.md>)取得184B结构下真实flight fallback及完整20.014秒reader窗口，44条同key观测后归零；[run28](<air/run28-recovery-live-report.md>)在另一次自然镜头异常后，同reader直接camera+Snapshot各一次/6ms有限恢复，但原event window仍fail、air invalidated、exit3，且production GUI recovery未验收。故真实enemy-only GUI绘制、Loiter/V1/SLD、首次baseline/重复去重/清空后新事件仍待闭合；早期[零样本尝试](<air-attempts-round03.md>)的原生总部/视野局限继续保留。
4. **弹体逐弹关联与定量精度仍缺：**02 raw的G身份+F回放保留，冻结限制不扩大到run24。[run24实际C#输出](<projectiles/run24-live-reader-report.md>)有50秒/459polls、4个Wespe代次/128观测与前三个后的全零poll；[复核](<projectiles/run24-reader-sequence-validation.json>)无丢帧/丢项。[新实际画面](<projectiles/visual-live-report-01.md>)已限定闭合桌面预测圈、原生爆炸与两圈依次清除，不再记为完全未覆盖。两个窗口无同步身份，仍需逐弹entity/Id/Index关联、命中误差、预警时长及更多武器；不把编号当全局ID或由非原子图片推定精确因果。
5. **图层状态与清理未验收：**[Run29](<gui-run29-layer-live-report.md>)观察到有限的隐藏/恢复和主菜单显示清理；暂停下相同PNG不证明新帧、投影精度或reader释放。[Run33](<gui-run33-results-delayed.json>)再次观察到战绩页残留。含ClearData改动的候选已启动，但旧错误触发后的清理回归仍未执行。
6. **两UI全功能覆盖不足：**Simple维护UI已有top/mini开关绘制闭环、正常退出及同局client重开，不再写成完全未测；world绘制正例、其余事件/生命周期与双保护版关键场景仍缺。SkyEye专有声音/热键/托盘/包抄未因SimpleTool无面板而从范围消失。
7. **最终源码与binary闭包：**早期GUI构建早于preflight，保留其局限。此前lead重建的维护SkyEye有0warning/0error、5生成fixtures及离线来源/资源/config校验通过；[运行身份](<skyeye-current-source-identity.json>)绑定client PID32780、SHA256 `3FE10E1295B6C5F6E5C49A22C3BBF2C55C3F421B52C09A7C83116AB7393F0E88`、GameLayout SHA `77C44EFFBF42D9F6300CFD41E753712BED85E551B487F4DB1A779DF528DC8D92` 与同一game PID11192。当时源码已实际读取并绘制顶部文本；[战术图开](<skyeye-current-tactical-1838374.png>)有中央单位文字，[关](<skyeye-current-map-closed-1838374.png>)清空中央且保留小图，再开图恢复同暂停分布（两个开图层SHA相同，非游戏推进证明）。已正常关闭且进程退出。后续run29身份另绑定UI24288及其binary；task10的双树ClearData修复发生在该binary之后，Run33候选包含该修复但未取得触发后的回归证据。以上不替代全功能、保护版或端到端错误分类验证。
8. **新版 D 未执行：**未登记新的支持指纹，未生成新版保护交付，现有交付目录保持不变。双客户端 Release 离线夹具结果不替代完整客户端验收。

后续每例记录caseID、两工具方向/裸或保护、game/client SHA256、PID/startTime/MatchKey、前后UTC及实际游戏计时/foreground确认、事件正例/负对照、预期/实际、pass/fail/not_run、清理恢复与原始PNG/JSON。观察性证据可继续积累；只有完整用例的预期、实际与恢复都闭合时才写pass。
