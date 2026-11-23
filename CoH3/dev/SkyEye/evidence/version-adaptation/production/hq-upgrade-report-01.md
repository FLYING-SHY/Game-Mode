# 19:18本方HQ机械化支援中心：受控升级角色只读定位

## 结论

本次受控事件对应 **EntityUpgradeProductionItem**，不是SquadUpgradeProductionItem，也不是普通单位Spawn。当前HQ队列与条目字段可被现有结构读取，类型RVA映射正确；维护run10的production sample_absent来自现有产品仅选择Spawn/手雷/Kwk38这几类输出，不是本方队列被漏读或布局失配。

本次只新增本目录脚本和证据；不覆盖旧样本、不修改引擎、不构建、不控制UI、不推进游戏、不读取许可私密。已先查当前skills目录并重读本地game-trainer-development技能，加载karpathy-guidelines；使用已实测的ProcessReader只读Win32路径，未把REA静态能力当运行采样。

## 输入与受控动作证据

- lead报告：PID11192同一对局，暂停19:18，本方US HQ按G启动机械化支援中心，tooltip60s/100MP/30fuel，资源由fuel150→120、MP5302→5206。前后差值由lead操作记录提供，本探针不重新触发动作。
- 本agent实际读取 [hq-upgrade-paused.png](../hq-upgrade-paused.png)：画面19:18、明确“战术暂停已启用”、总部选中、战术队列有真实图标、机械化支援中心tooltip为技术升级/60s/100MP/30fuel，HUD5206MP/25mun/120fuel/CP3/人口4。截图SHA256 `283ec95150492f990cc2e57124984627a541521d82b85c8ebcf0f58f74652015`。
- [maintenance-run-10日志:7–24](../maintenance-run-10-hq-upgrade/maintenance-live.txt#L7-L24)同时含自己与敌方两行；slot0 ProductionKnown=true、Kwk38Known=true、GrenadeRemainingTicks=null、jobs空，因此“runner只取敌方”不成立。

## 只读探针与边界

从已读 [production_probe.py](production_probe.py) 复制成新 [hq_upgrade_probe.py](hq_upgrade_probe.py)，只改变输出文件名、记录当前trigger背景并增加player identity/resource raw与item64B raw/blueprint地址；不改旧脚本或旧JSON。

```powershell
& 'C:\Program Files\Python311\python.exe' 'F:\WorkSpace\Game\CoH3\dev\SkyEye\evidence\version-adaptation\production\hq_upgrade_probe.py'
```

实际exit0，UTC **2026-11-20T18:16:33.410150Z**（北京时间2026-11-21 02:16:33），2966读、请求/返回56,288 bytes、0失败、约10ms。句柄0x410；10秒/2MiB/20000次调用预算，单读≤65536、玩家≤16、owned entries≤4096/玩家、队列≤128。输出 [hq-upgrade-probe-01.json](hq-upgrade-probe-01.json)，roots结束复验稳定，HQ queue结束复验稳定。

## 实际完整链

| 节点 | 现场值 |
|---|---|
| module base / arena / world offset | 0x7FF7A37E0000 / 0x17C00000000 / 0x6400000 |
| 本方player | 0x17C08C08000，slot0，identity0x3E8 |
| HQ entity | 0x17C051F7C00，ebps\\races\\american\\buildings\\production\\hq_us |
| entity kind | ushort+76=429 |
| Health | lookup0x628A7EA，slot6，HealthExt0x17C0D3D27D0，health+108=3000 |
| production component | lookup0x628A7F1，slot11，ProductionQueueExt0x17C0C970B90；descriptor active=1，mode0 |
| queue link | ProductionQueueExt+52=0x2AF5000 → queue0x17C02AF5000 |
| queue data/count | queue+48=0x3D03700，queue+52=1 |
| first entry | 0x17C03D03700，原始192B保存在JSON |
| remaining/total | entry+48 float474，entry+52 float480 |
| item offset | entry+184 uint0xA9080 → item0x17C000A9080 |
| item类型 | .?AVEntityUpgradeProductionItem@@，vtable RVA0x3D92160，COL0x505A840，TD0x5F9ABC0 |
| blueprint pointer | item+16 Q=0x17CB29EA5F0，StringAt(blueprint+40)读取名称 |
| upgrade路径 | upgrade\\american\\research\\mechanized_support_center_us |

独立从保存的entryRaw与itemRaw64重新struct解包：entry长度确为192，+48/+52=474/480，+184=0xA9080；item首Q为0x7FF7A7572160，减base恰0x3D92160，+16指针与记录相同。item+8 uint原始值20可保留，但没有证据把它命名为通用production-kind枚举；角色身份用RTTI而非猜数字。item+4位于64位vtable指针高半部，不当作kind字段。

474/480按现有8tick/s换算：remaining=59.25s，ceil显示60s；total=60s，与tooltip60s一致。单一暂停快照只验证尺度相符，没有测倒计时随时间递减或完成触发。资源raw独立解包slot0 fuel120.091278、MP5206.10498、mun25、CP3，与整数HUD相符。

## 为什么run10没有production输出

- [ReadEntities:2682–2710](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2682-L2710)校验owner后遍历所有player的生产建筑，并未按敌我过滤ReadProduction。
- [ReadProduction:2918–2935](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2918-L2935)先辨Spawn，再以vehicle=false选择legacy64557304→0x3D92160，当前真实item精确命中EntityUpgrade。
- [ReadProduction:2937–2951](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2937-L2951)仅非vehicle且GrenadeUpgrade.IsResearch(path)时填GrenadeRemainingTicks；vehicle路径则只识别Kwk38Upgrade.IsResearch。
- [GrenadeUpgrade.cs:9–16](../../../client/CoH3MemoryTool/GrenadeUpgrade.cs#L9-L16)只列美国步枪兵手雷、英军手雷/枪榴弹三条路径，机械化支援中心不在列表。
- 非Spawn且不在这两个特定研究集合，本次不会向Producing/Kwk38Upgrading添加job，也没有通用EntityUpgrade列表。known可保持true而输出为空，符合当前功能契约；不是本次适配新增bug，也不应为验收擅自扩展产品功能。

## 受控链归类与覆盖边界

**本次可证明：**受控HQ技术升级出现在本方普通建筑ProductionQueueExt中，EntityUpgradeProductionItem的映射/blueprint+16/entry192B +48/+52/+184与当前现场对应。它不是Spawn，也没有被误映射成SquadUpgrade。

**仍未测：**SquadUpgradeProductionItem0x3D92058的真实事件（通常vehicle特别路径），PlayerUpgradeProductionItem另类，特定Grenade与Kwk38用户功能、升级倒计时推进/完成、工具可见输出、跨局/新进程与保护交付。当前探针coverage为EntityUpgrade observed，Spawn和SquadUpgrade sample_absent；前序Spawn证据保留在旧报告，本次不混用为新受控升级通过。

结论是单场景的角色/字段G观察，不是完整升级功能G/R通过；不登记新版hash。
