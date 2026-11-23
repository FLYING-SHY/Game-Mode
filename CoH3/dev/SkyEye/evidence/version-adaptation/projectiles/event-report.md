# 有界弹体事件采样01

## 实际运行

- [event-30s-01.json](event-30s-01.json)：UTC 2026-11-20T17:48:28.706225Z–17:48:48.374996Z（北京时间2026-11-21 01:48:28–48）。
- 实际20.219秒，36完整采样，25,889,632 bytes，总文件22,891 bytes，3个去重raw对象记录。
- 首版复用one-shot的20秒tick，故比声明的30秒总上限更早结束；stopReason如实为`20 second diagnostic budget exceeded`，没有冒称跑满30秒。
- 每轮完整3212live，3projectilePaths，过滤统计恒定：componentPresent失败2；weapon失败1；actorState64失败1。与此前暂停现场相同三实体，0个全记录gate通过样本。
- 本agent不控制UI。lead随后确认启动通知延迟，01采样全在恢复前；因此**明确不是“恢复后20秒没有炮弹”结论**。lead当前已经12:12暂停，下一次由lead在同一pwsh内恢复→执行采样→finally暂停/capture，避免跨消息延迟。

## raw与谓词

JSON按(entity,generation,gate-signature,weapon)去重保留body392B、actor80B、active ext208B十六进制，以及RTTI、owner/weapon/target/position。每轮保存过滤统计/错误，不记录每个位置变化，避免文件膨胀。

事件版有限坐标检查已按SquadMarkerLogic.ValidPosition的x/z<20000、y<2000及有限性条件设置；同时记录所有ProjectileLogic.Decode非infantry/non-playerSource分支的类型、backref、actor mode/state、destroyed flag、source、radius、not-arrived等gate。`allRecordedGatesPass`还要求weapon与owner在players表，而不等于实际C#调用执行；调用者enemyPlayers归属、shellSeen预热、时序/命名/绘制未代替验证。

## 后续脚本

[projectile_event_probe.py](projectile_event_probe.py)已修正tick为30秒，并增加`--pid/--tag`、已存在输出拒绝覆盖；未再次运行。当前版本将总read预算显式设为64MiB，以覆盖约61次当前registry读取；单读64KiB、约0.5秒间隔、最多61samples、raw去重记录JSON预算1.5MB。budget若先耗尽仍会早于30秒停止。已用Python compile()仅作语法验证，没有附加游戏或自动重复采样。

```powershell
& 'C:\Program Files\Python311\python.exe' 'F:\WorkSpace\Game\CoH3\dev\SkyEye\evidence\version-adaptation\projectiles\projectile_event_probe.py' --pid 11192 --tag event-30s-02
```

lead应在收到“已开始”时间后恢复，记录实际按键/游戏计时，再与JSON UTC范围对齐。未采到允许weapon时保持`sample_absent`/inconclusive，不把flamethrower和inactive组件升级为弹道功能通过。

## 采样02：真实mortar raw的独立分析

lead在同一pwsh中恢复→运行→finally暂停/capture；本agent仅读取其[event-30s-02.json](event-30s-02.json)，未再附加或操作游戏。

### 时间、覆盖与保存语义

- UTC 2026-11-20T17:51:36.874568Z–17:52:06.789757Z（北京时间2026-11-21 01:51:36.874568–01:52:06.789757）。elapsedSeconds=30.0，stopReason=deadline；55完整samples，39,615,111 bytes。
- 每次3226live、1条projectile path，0错误，candidateCount恒1，无failedGate。没有触发root/registry变动停止。
- 只保存1个去重raw记录，去重依据为实体地址、generation、gate集合和weapon组合。这不是55发炮弹，亦不意味着55次raw位置都相同：raw位置变化未作为去重key，后续帧没有保存位置。
- 不存在跨运行shellSeen缓存：脚本每轮从registry及内存重新读取，只有JSON保存层去重。但同一entity/gen跨55次存在，仍值得复核其实际运动/残留生命周期。

### 原始buffer独立解包（非复用JSON文字结论）

[raw记录:620–716](event-30s-02.json#L620-L716)包含body392B、actor80B、ext208B，长度均与预期一致。使用Python struct从hex重新解包得到：

| 项目 | 值 |
|---|---|
| entity / id / index | 0x17C052E7000 / 1000008637 / 3087 |
| blueprint | ebps\gameplay\projectiles\common\artillery\p_artillery_mortar_81mm |
| actor | 0x17C0524EE00，RTTI ProjectileDynamics，vtable RVA0x3E4AA38 |
| actor模式 | mode16=2，state64=3；mortar=true，故state3属于当前helper允许分支 |
| ext | 0x17C0C97BCF0，RTTI ProjectileExt，vtable RVA0x3E49618 |
| root交叉核算 | actorVtable−0x3E4AA38与extVtable−0x3E49618都为base0x7FF7A37E0000；entity−actor.backref36为arena0x17C00000000；ext.backref8亦回entity |
| body flags | 0x19E，0x4000 destroyed位未置 |
| owner | offset0x8C09000 → player0x17C08C09000，slot1，identity0x1000003E9 |
| weapon | weapon\german\explosive_weapon\mortar\81mm_mortar_barrage_ger；Weapon=true，Mortar=true，Arc=false |
| ext来源 | sourceType16=3，source20=1000008229（非零），source24=3061 |
| radius | 4.5，有限且在0..100 |
| generation | registry generation与body+104相同，body+108等于index，单轮结束复验相同 |

owner在players表是独立事实；先前身份快照把slot1标为敌方，但02不读取LocalAccount日志重新判敌我。本条作为真实fixture时可直接使用保存的owner，正式reader还须传入匹配enemyPlayers集合。

### “已到达/卡住缓存误选”判断

首帧body+44当前位置为`(-61.1252213,58.1234932,42.8874550)`，actor+20预测位置为`(-91.3385239,24.5234928,62.0009003)`；三维距离平方=`2407.127470924799`，距离约`49.0624854`。`ProjectileLogic.Arrived`阈值是平方距离≤0.0001，故**首raw明确未到达，非“已到达仍通过”**。位置均有限，x/z<20000且y<2000，满足实际ValidPosition阈值。

然而55次仅保留相同gate与entity/gen，不保留每帧位置，故此JSON不足以证明炮弹轨迹确实推进，亦不足以排除“游戏仍暂停/对象滞留/读取的是恒定预测落点”等情形。actor+20本来就是预测位置，不可单凭其恒定断言弹体卡死。要证明动态，需要body+44位置序列、游戏计时或同窗画面证据；本次不重新采样。

### Helper真实raw fixture交接

已向agent-2发消息指向同一JSON `records[0]` 的rawBodyHex/rawActorHex/rawExtHex，建议实际调用当前：

```text
ProjectileLogic.Decode(body,actor,ext,
 image=0x7FF7A37E0000,arena=0x17C00000000,
 entity=0x17C052E7000,id=1000008637,index=3087,
 out impact,mortar=true,infantry=false,playerSource=false,
 resolveRva=候选显式mapper)
```

预期候选mapper通过；省略delegate用旧RVA应失败。fixture还应逐独立破坏actor/ext vtable、sourceType/sourceID、backref、state64、already-arrived坐标等，证明通过来自完整guard而非只比较某一个类型。

当前证据等级：真实现场原始对象/类型/归属与helper谓词的观察证据；实际C# helper执行由agent-2的fixture给出。它们都不等于可见预测标记的动态G/R通过，也未证明恢复/跨局。

## 焦点控制纠正（lead后续现场核查）

lead进一步核查旧window_probe的SetForegroundWindow未检查返回，混合UI控制可能让space没有送达游戏。其提供的现场观察为：event02末截图游戏12:13、没有战术暂停文字；后来focus-checked-state截图15:03仍运行；使用新game_input.py强制验证foreground并点击标题栏后，15:30截图明确暂停。

据此修正解释：event02可能大量时间处于焦点丢失自动冻结，不能按最初“同一pwsh恢复→采样→暂停”操作意图推定游戏连续运动。55次相同entity/gate记录仅保留对象/谓词证据，**运动与预测标记验证为not_run**，不宣称运动成功或失败。

此纠正来自lead的后续控制与截图核查，本agent未重新附加或操作UI；未覆盖/修改event-30s-02.json或其raw。后续由lead串行使用foreground-checked控制，实际按键前后核验前台窗口和游戏时间，再对齐采样UTC。采样脚本本身是只读采集，不负责确保游戏获得焦点。

## 采样03脚本增强（尚未由本agent运行）

应lead要求，事件脚本已加入每sample至多10个targetTimeline记录，优先全gate通过候选，包含entity、id/generation、index、body当前位置、actor预测位置、state64/mode16、sourceId、weapon及稳定标记。这些小记录不受raw去重影响，因而可实际比较同一实体的body位置变化；目标位置本身可能恒定，不应拿它当运动判据。

每轮另外保存独立枚举的base、读取的arena/world/registry根，以及players+1704的10个float、同位raw uint和hex资源快照。resource root+560经源码核对是技能timer名称/索引表而非游戏时钟；只保存至多16条name/index的uint/float位模式及原hex、向量总数，明确标为metadata，不能以其静止判定游戏冻结。

约束仍为30秒、64MiB总read、单读64KiB、0.5秒间隔、最多61次sample；raw去重预算1.5MB，最终JSON4MiB硬上限。若输出超限先裁尾raw再裁尾samples，并置outputTrimmed=true，不悄悄伪称完整保留。根/registry/技能timer向量变动即停止。仅compile()检查语法，没有附加/运行。准确command与tag event-30s-03已交lead，由其foreground-checked串行控制。

## 采样03独立结果：资源证明推进，弹体运动仍not_run

本节仅离线读取lead执行生成的[event-30s-03.json](event-30s-03.json)，未修改或运行任何采样脚本，未附加游戏。

### 范围与稳定性

- 首sample UTC **2026-11-20T17:59:47.290136Z**，末sample **18:00:17.184405Z**；北京时间2026-11-21 **01:59:47.290136–02:00:17.184405**。采样间跨度29.894269秒，运行总计30.0秒，stopReason=deadline。
- 55个完整samples，40,056,226 bytes，总JSON294,634 bytes；outputTrimmed=false，错误条数0。
- 55次base均0x7FF7A37E0000，arena均0x17C00000000，worldOffset均0x6400000，registry均0x17C08C01000；全部stableRootsAndRegistry=true。
- Registry live数量在3248–3259变化（3258共45次、3254共4次、3259共3次、3248共2次、3257共1次）；根稳定不等于对象数固定，本次数量变化也是运行状态变化旁证。
- timer名称向量55次均root0x7FF7AA392100、begin0x17C96EF08A0、end0x17C96EF0C90；其稳定只说明名称/索引元数据稳定，不是冻结证据。

### 资源raw字节独立解包

对55次、两玩家的rawHex逐一struct解包10float，并与JSON rawFloat逐项对照，全部一致。两玩家各有55种不同10-float向量，54个相邻区间全部发生资源变化；不是复用同一缓存快照。

下面保持原始数组索引，不把未由本次名称表重新确认的字段强行命名：

| slot | rawFloat索引 | 首值 | 末值 | 差值 | 相邻变化 |
|---|---:|---:|---:|---:|---|
| 0 | 2 | 107.87446594 | 181.67379761 | +73.79933167 | 54次均上升 |
| 0 | 4 | 123.61747742 | 127.11662292 | +3.49914551 | 54次均上升 |
| 0 | 5 | 4286.18212891 | 4420.94775391 | +134.76562500 | 54次均上升 |
| 1 | 2 | 837.86926270 | 229.66809082 | −608.20117188 | 53上升、1下降 |
| 1 | 3 | 1 | 0 | −1 | 3个不同值，1升1降 |
| 1 | 4 | 266.49423218 | 282.89315796 | +16.39892578 | 54次均上升 |
| 1 | 5 | 88.70742798 | 180.53193665 | +91.82450867 | 54次均上升 |
| 1 | 6 | 50.20187378 | 73.02169800 | +22.81982422 | 53上升、1下降 |

slot0其余索引0/1/3/6/7/8/9不变；slot1索引0/1/7/8/9不变。单个不变资源本来就可能不增长或支出，不拿它推定冻结。持续多字段变化、敌方一次支出型下降和registry数量变化相互支持：**03采样窗口内游戏数据确实推进，不再是02那种仅同一谓词缓存证据。** 不把这些数值当精确游戏时钟或把wall-clock30秒等同恰好30秒游戏tick。

lead另提供foreground-checked串行控制的截图观察：恢复约15:30，暂停16:01且明确显示战术暂停。该画面事实由lead报告，本agent未声称独立读过对应截图；它与本次资源推进证据一致。

### 无目标弹体：保留未测结论

55次candidateCount=0、targetTimeline均空、records共0；coverage没有任何projectilePaths阳性计数。这是当前500ms左右采样粒度下未观察到符合路径前筛的弹体，不是发现弹体后被某个Decode guard过滤，也不排除短于间隔的瞬态漏采。

因此本次可确认稳定根下资源/registry数据推进，但**弹体位置progression、预测轨迹/落点、可见运动标记仍为not_run（目标sample_absent）**。不把“游戏在运行而没采到炮弹”改写成弹道功能通过或失败。03不推翻02真实raw的静态谓词/fixture价值，亦不把02升级为运动证据。
