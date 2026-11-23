# 新版 Component 解码链：只读动态证据

时间：2026-11-21 01:15–01:22（Asia/Shanghai）。PID 11192，image base `0x7FF7A37E0000`。目标为当前已存在的暂停对局。本报告不登记正式基线。

## 结论

1. **lookup 表列已取得独立跨 type 证据**，不是平移猜测：当前表根候选 `0x628A7E0`；八个旧 lookup 加 `0x1FA0`（8096），不是加 `0x2000`。前探测对旧位置及 `+0x2000` 在这34种 type 全读到 FF；随后在声明的约2MiB窗口搜索跨 type RTTI→slot一致列。
2. 全量复核为 **357 个拥有者注册实体、34 种 componentKind、8 列**。活跃条目都得到对应 RTTI 名，且 descriptor byte+18 全等于表列的 component ID，`idMismatch=[]`。所有成功解码都是 mode 0。
3. **payload pool 有镜像多解，暂停现场动态证据单独不足以选唯一 global。** 六个候选 global 对34种 type×8lookup 的实际选中描述符全部相同（每候选272checks，differenceCount=0）。只按邻近delta选址或按未用slot/padding差异排除镜像，会误判。
4. 静态同事补充原程序实际通过 **TLS index 选择 `Q(image+0x6187E70+index*8)`**，再加 entity+64 与 slot×24。故 `0x6187E70` 是有静态调用链支持的数组根；本动态脚本的 `Q(image+0x6187E70)` 只检查其 index 0。该根与TLS index选择是两件事。
5. `0x628A7F2`（旧ID18）在拥有者注册的357实体中全部FF；后续扩大到world注册表3181个live实体后，已发现1个活跃 ProjectileExt 并确认该列，见 [后续runtime角色报告](role-report.md)。本表保留第一次owned-only探测统计。mode1–8、线程切换、恢复/跨局/新进程尚待验证。

## 原解码布局与本次实读

源码 [PlayerInfoReader.cs:2521–2555](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2521-L2555) 实际是：

```text
kind = ushort(entity+76)
slot = byte(image+lookup+kind*128)
slot == 255 -> absent
D = Q(image+payloadGlobal) + uint(entity+64) + slot*24
mode = uint(D+0)
value = ulong(D+8)       // +8，不是+16
state = byte(D+16)
(state & 1)==0 -> inactive
mode==8 -> value
mode<=7 -> Q(image+modeRoots+mode*16) + uint(value)
```

本次 mode0 使用 `Q(image+0x6187E40)` 作相对地址arena，payload index0使用 `Q(image+0x6187E70)=0x17C53000000`。字节+18在活跃样本中保存 component ID，可与lookup列交叉校验。该额外不变量是实测发现，非宣称原源码已经检查它。

RTTI通过对象vtable→vtable[-1] COL→TypeDescriptor RVA→类型名读取；没有写入/调用游戏函数。

## 八列全量结果

| 旧lookup RVA | 新候选RVA | ID | 活跃 RTTI | active | absent FF | inactive |
|---|---|---:|---|---:|---:|---:|
| 0x6288842 | 0x628A7E2 | 2 | SimInventoryItemExt | 258 | 99 | 0 |
| 0x628884A | 0x628A7EA | 10 | HealthExt | 81 | 276 | 0 |
| 0x628884B | 0x628A7EB | 11 | ConstructionExt | 22 | 335 | 0 |
| 0x6288850 | 0x628A7F0 | 16 | CombatExt | 77 | 278 | 2 |
| 0x6288851 | 0x628A7F1 | 17 | ProductionQueueExt | 88 | 267 | 2 |
| 0x6288852 | 0x628A7F2 | 18 | 本场景无活跃样本 | 0 | 357 | 0 |
| 0x6288855 | 0x628A7F5 | 21 | AbilityExt | 64 | 293 | 0 |
| 0x6288891 | 0x628A831 | 81 | WeaponExt | 205 | 152 | 0 |

合计795个active组件，全部 mode0、ID一致；4个inactive不是读取异常。统计见 [components-verified-01.json:12–419](components-verified-01.json#L12-L419)。

注意：旧lookup尾数627并非Combat，它实读为Construction；旧632才为Combat。禁止按调用点用途臆断RTTI类名。最初lookup搜索将Combat旧值暂配627，造成报告delta 8101；该中间字段已被本轮八列直接解码更正，最终以本表为准。

## 跨 type 样例（slot不是全局常量）

| kind / 实体 | SimInventory | Health | Construction | Combat | Production | Ability | Weapon |
|---|---:|---:|---:|---:|---:|---:|---:|
| 429 HQ_US / 0x17C051F7C00 | FF | 6 | 7 | 10 | 11 | 12 | FF |
| 500 Scout / 0x17C051FFC00 | 1 | 8 | FF | 12 | 13 | 15 | FF |
| 612 Pioneer / 0x17C0521AC00 | 1 | 8 | FF | 12 inactive | 13 inactive | 15 | FF |
| 1158 FlakWeapon / 0x17C05252600 | 1 | 4 | FF | FF | FF | FF | 8 |

精确逐实体descriptor、slot、mode、地址、RTTI在 [components-verified-01.json](components-verified-01.json)，示例分别从 [429:585](components-verified-01.json#L585)、[500:1525](components-verified-01.json#L1525)、[612:6218](components-verified-01.json#L6218)、[1158:28009](components-verified-01.json#L28009) 附近开始。

kind612的四个inactive描述符分别来自 Pioneer 与 Grenadier，两者Combat/Production state byte都是0，但slot与ID仍匹配。该现象解释初轮以“有活跃RTTI”推断absent时的一个type mismatch：**无活跃RTTI不等于slot==FF**。

## pool歧义与静态交叉证据

动态候选：`0x6187DD0`、`0x6187DD8`、`0x6187E70`、`0x6187E78`、`0x6187E80`、`0x6187E88`。它们指向不同pool reserve，当前实际选中descriptor内容和目标组件地址相同。

早期32slot整块比较在kind1158出现差异，进一步只比lookup选中slot后差异归零。早期差异包含未使用分配区；它**不构成**排除其它pool的依据。最终证据见 [components-verified-01.json:420–469](components-verified-01.json#L420-L469)。

静态agent-3报告的主路径：

```text
0x2EB5D9  r14 = imagebase
0x2EB5E0  slot = byte[image + 0x628A7EA + kind*128]
0x2EB5F7  rax = gs:[0x58]
0x2EB600  rcx = [rax]
0x2EB603  ecx = [rbx+rcx] ; rbx=0x40, TLS index
0x2EB606  eax = [entity+0x40]
0x2EB609  rcx = [image + rcx*8 + 0x6187E70]
             rcx += eax
             rax = slot*3
             rcx += rax*8
             call 0x820980 ; descriptor resolve
```

原始静态记录由同事维护于 [ui-followup.json](../static/ui-followup.json)。上段是同事给出的静态证据摘要，不声称本动态探针测得当前线程TLS index；更不能把所有线程index当0。

## 探测过程、预算与错误

- [component_probe.py](component_probe.py)：arena/world实体枚举、候选global小窗口、slot RTTI探查。读取322381bytes；包含故意候选的读失败，错误列表最多保留200条，不代表总失败次数。初探96slot可越到相邻分配，故它不是最终实体组件边界证明。
- [lookup_probe.py](lookup_probe.py)：声明窗口start/end、≤65536bytes分块读取、跨type列匹配；读取2566504bytes，errors=[]。Health/Ability/Weapon各在窗口内唯一候选，分别19/13/10个阳性type、34type全匹配。额外MovingExt列 `0x628A7ED` 有8阳性type匹配。
- [verify_components.py](verify_components.py)：新鲜枚举357实体，**只读取lookup指定slot**，每条检查RTTI与component ID；复读两次统计一致。最终读取142443bytes，errors=[]，stableArenaWorld=true。
- 读取均复用 [live_probe.py:5–36](../live_probe.py#L5-L36) 的OpenProcess(0x410)、地址/每读65536bytes上限与每探针8MiB预算。不同探针预算分别计数，不是整个会话累计8MiB。
- 所有输出只在本目录。未更改引擎/注册hash/构建/启动游戏/发送按键/调用游戏代码/写游戏内存；未读取私密授权数据或original。

## 尚待验证

- 当前动态只覆盖PID11192、同一暂停场景；根前后稳定不等价于跨进程/跨局稳定。
- ID18的owned-only覆盖缺口已由后续world registry活跃ProjectileExt补齐，见 [runtime角色报告](role-report.md)；弹道内部字段和运动行为仍待真实时间变化验证。
- 所有active都是mode0；1–7 arena分支与8直接指针分支是现有源码机制，当前未动态覆盖。
- TLS index如何绑定读线程/对局更新时序、镜像pool随恢复是否分歧，尚无动态验证。本报告不将index0硬登记为通用线程选择。
- 组件存在不代表内部字段沿用、事件/渲染正确。Health+108、Ability内部timer、Weapon/Production指针字段等还需要各自验证及对局恢复/跨局证据。
- 最终版本登记仍需lead规定的真实对局全功能、恢复、跨局及交付回归；这里没有更改该验收门槛。
