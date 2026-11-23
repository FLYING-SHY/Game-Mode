# Runtime vtable role mapping（只读）

2026-11-21 01:25（Asia/Shanghai），PID11192，image base `0x7FF7A37E0000`。沿已验证组件链与实体自身的字段读取，再独立读取RTTI，不使用批量delta映射。原始 [role-probe-01.json](role-probe-01.json)；脚本 [role_probe.py](role_probe.py)。

## 可供适配使用的实读候选

| 旧源码guard/role | 新RVA | 独立RTTI | runtime证据 | 状态 |
|---|---|---|---|---|
| 64584880，普通单位生产controller | 0x3D98BF0 | SpawnProductionItem | ProductionQueueExt→+52 queue→queue+48 entry→entry+184 controller→+16 blueprint；1个活跃单位生产项 | observed |
| 64568064，技能列表定时技能guard | 0x3D94A40 | TimedAbility | AbilityExt→+76 holder→holder+40 array/+44 count；炮击技能对象，+56有效ability blueprint | observed，真实作用在当前链中成立；旧版RTTI未单独反查 |
| 65307976，ID18弹体组件guard | 0x3E49618 | ProjectileExt | world registry活实体→新ID18查表→组件RTTI；1枚81mm迫击炮弹体 | observed |
| 64557040，vehicle升级controller | 未动态取得 | 静态同事候选SquadUpgradeProductionItem 0x3D92058 | vehicle queue仅3 empty、5 no_component | not_observed |
| 64557304，普通升级controller | 未动态取得 | 静态同事候选EntityUpgradeProductionItem 0x3D92160 | 普通队列只有Spawn项，未遇升级项 | not_observed |
| 64559016，ReadLoiter49的entity+388 body guard | 未动态取得 | 静态同事后续候选MoverDynamics 0x3D926E0；现场未遇loiter/aircraft对象 | mines样本是BuildingDynamics，不能替代飞机/徘徊body类 | not_observed |

前三行是当前路径、对象类型、RVA的独立运行时证据；并非证明所有下游字段或功能已通过。后3行保持未观察，不将静态候选直接当成可登记偏移。

## 1. 活跃单位生产

对象 `0x17C07820800`，vtable `image+0x3D98BF0`：

- 上游实体 `0x17C050BC200`，ProductionQueueExt `0x17C0C971D60`。
- queue `0x17C02ADAC00`，count=1，entry `0x17C03D8BF00`。
- controller+16→蓝图 `sbps\races\german\infantry\jaeger_ger`。
- entry+48/+52 两个原始float：13、240；本探针不对时间单位作额外推断。
- COL RVA `0x505A6B0`，TypeDescriptor RVA `0x5F9B110`，RTTI `.?AVSpawnProductionItem@@`。

精确证据：[role-probe-01.json:2292–2311](role-probe-01.json#L2292-L2311)。对应源码调查位置为 [PlayerInfoReader.cs:2846–2925](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2846-L2925)；lead并行改动可能使行号漂移，请以ReadProduction方法名核对。

普通生产路径覆盖：67 empty、11 no_component、1 observed。vehicle路径覆盖：3 empty、5 no_component。没有修改队列或向游戏输入，缺席的升级生产项不补造。

## 2. Ability不止一个vtable

检查2个player级列表及64个AbilityExt派生的holder列表，共138个独立对象：

| RTTI | RVA | 独立对象数 |
|---|---|---:|
| AlwaysOnAbility | 0x3DA77B8 | 136 |
| ToggledAbility | 0x3E97398 | 1 |
| TimedAbility | 0x3D94A40 | 1 |

**不把所有技能对象统一视为TimedAbility。** 旧64568064 guard出现在炮击/空袭的具体循环，使用TimedAbility候选应仍保留蓝图与活动状态检查。

当前TimedAbility对象：

- `0x17C03E10200`，来自实体 `0x17C05021C00` 的AbilityExt `0x17C0C991C70`，列表index1。
- `abilities\races\german\team_weapons\barrage_81mm_mortar_ger`。
- state+12=1，target offset+8=`0x10EC40`。
- COL `0x5064E80`，TD `0x5F6F3A8`。

运行时 [role-probe-01.json:2274–2291](role-probe-01.json#L2274-L2291) 与独立静态 [rtti-priority.json:27421–27435](../static/rtti-priority.json#L27421-L27435) 对类名和RVA一致。

## 3. ID18的覆盖缺口已闭合

前次357个owned entities没有ID18，原因是该列表未覆盖world registry的非owned瞬态对象。

本次world registry：capacity3229，live3181，全部检查header+104 generation、+108 index。发现：

- entity `0x17C0527B000`，registry index3001。
- 蓝图 `ebps\gameplay\projectiles\common\artillery\p_artillery_mortar_81mm`。
- 新lookup `0x628A7F2`（root+ID18）解出component `0x17C0C979790`。
- RTTI `.?AVProjectileExt@@`，vtable RVA `0x3E49618`，COL `0x505DDB0`，TD `0x5F8B540`。

证据：[role-probe-01.json:2420–2434](role-probe-01.json#L2420-L2434)。因此 [组件报告](component-report.md) 的ID18 owned-only缺席，不应被误读为全场景没有弹体；它现已有活跃类型确认。弹道轨迹、伤害或落点仍未做时间变化/渲染验证。

## 4. ReadLoiter49与mine的标签纠正

旧64559016在本次读取源码中位于 **ReadLoiter49** 的entity+388 body校验（旧调查行1029–1055），不是独立mine类型guard。不应因同样从entity+388取对象就把所有Dynamics类混同。

现场共发现14个包含mine的蓝图实体，但其中只有5个+388非FF，全部RTTI `BuildingDynamics`，vtable `0x3DBA160`；5个+36 owner backref均等源实体；4个state+64=1，1个state+64为2913796097。该事实既不能替代loiter类身份，也不证明旧guard应更换为BuildingDynamics。

样例：[role-probe-01.json:2384–2419](role-probe-01.json#L2384-L2419)。现场未遇aircraft/loiter路径，保持not_observed。

后续agent-3静态线索：MoverDynamics vtable `0x3D926E0`（TD `0x5F98DB8`，COL `0x505AFE8`）；vtable+344函数 `0xCD0F10` 从this+0x40读取mode，mode0转 `0xCD0F60`、mode1走TLS/arena。Building/Attachment/ProjectileDynamics同slot为其它函数，故MoverDynamics较符合旧movement函数role。此为静态候选，+36归属、resolver及plane字段还待其补证，本runtime报告不把它升为observed。

## 静态升级候选仅作待办线索

agent-3按RTTI主类给出：

- `EntityUpgradeProductionItem` vtable `0x3D92160`，TD `0x5F9ABC0`，COL `0x505A840`。
- `SquadUpgradeProductionItem` vtable `0x3D92058`，TD `0x5F9B1B0`，COL `0x505A5E8`。
- `PlayerUpgradeProductionItem` vtable `0x3E57008`；本地已核读静态 [rtti-priority.json:28435–28449](../static/rtti-priority.json#L28435-L28449)。

普通路径来自ProductionQueueExt，vehicle路径来自squad SimInventoryItem+16，故Entity/Squad升级候选有路径依据；本轮未遇活跃对象，不能把该推断升级为runtime mapping完成。PlayerUpgrade也不能拿来补普通/vehicle guard。

## 预算与局限

- OpenProcess `0x410`；ReadProcessMemory每次≤65536bytes，整个探针789379bytes，errors=[]，stableArenaWorld=true。没有写游戏内存、调用游戏函数、切前台、按键或更改源码。
- 只写本components目录，游戏现场始终保留。读注册表上限65536容量、最多4096 live对象，本次3181 live未触及上限；不会把扫描截断当对象缺席。
- RTTI是对象身份证据；生产队列一次性内容、TimedAbility状态、ProjectileExt存在不等价于功能、恢复、跨局、新进程验收。
- 脚本使用pool index0镜像；TLS多镜像局限沿用 [组件报告](component-report.md)。所有成功组件解码仍为mode0，mode1–8未覆盖。
