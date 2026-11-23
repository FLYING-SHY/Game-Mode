# 暂停现场瞬态弹体：一次有界只读诊断

## 结果

**存在弹体路径对象，但本次没有可供ReadProjectileImpacts目标功能验证的炮弹样本。不是“扫描不到任何弹体”，而是两个inactive component与一个不支持的火焰喷射弹体被正常条件过滤。**

- [原始JSON](paused-1142-01.json)：UTC 2026-11-20T17:45:35.444443Z，PID11192，module base `0x7ff7a37e0000`，arena `0x17c00000000`，worldOffset `0x6400000`。
- registry `0x17c08c01000`，容量3229；完整检查3212个live entry，3条`ebps\\...\\projectiles\\...`路径；0读取异常，根与registry结束复验稳定。
- 718599 bytes、约0.032秒；只读OpenProcess(0x410)，单读≤65536，总预算8MiB，20秒deadline。未执行写内存、游戏调用、输入、恢复或暂停操作。画面11:42暂停由lead提供，本探针不读取UI确认该时间。
- JSON总体status为`observed_candidates_not_decode_verified`；目标mortar/允许weapon样本为`sample_absent`，二者分开表述。

## 三个对象与逐关过滤

| index / entity | blueprint | component ID18 | actor | 过滤结论 |
|---|---|---|---|---|
| 3135 / `0x17c05281600` | p_gun_20mm_axis | slot0, mode0, state0, value0xc97a8a0 | `0x17c05281800`，ProjectileDynamics | component inactive，原Component返回0；无需调用Decode |
| 3138 / `0x17c0529d400` | p_flamethrower_parent | slot0, mode0, state1, ext `0x17c0c97a970` | `0x17c052a7600`，ProjectileDynamics | ext/owner存在；weapon不在当前可预测列表，先被Weapon过滤；actor state64=1也不满足Decode所需0或特定弧线时3 |
| 3139 / `0x17c0529f400` | p_flamethrower_parent | slot0, mode0, state0, value0xc97a7d0 | `0x17c05281a00`，ProjectileDynamics | component inactive，原Component返回0 |

三者generation/index均与registry匹配，结束时generation仍相同；actor+36返回原entity。actor mode16均2、state64均1。

### 独立确认ProjectileDynamics候选身份

三个现场actor的RTTI均为：

```text
vtableRva=0x3E4AA38
name=.?AVProjectileDynamics@@
COL RVA=0x505ADB8
TypeDescriptor RVA=0x5F992D0
```

因此mapper中新加入的旧65313128→0x3E4AA38现在不仅有静态候选，还获得本次真实对象RTTI及backref一致性证据。但这些不是mortar飞行状态；不同state64的行为含义和完整预测解码仍待受控事件。

### 唯一active ProjectileExt详情

[JSON:100–160](paused-1142-01.json#L100-L160)：

- ext vtable `0x3E49618` / `.?AVProjectileExt@@`，COL0x505DDB0，TD0x5F8B540；ext+8 backref吻合entity。
- owner offset56 `0x8c09000` → player `0x17c08c09000`，slot1，identity `0x1000003e9`。本探针仅确认在players表内；自己/敌方关系沿用先前现场材料，本探针不重新读取身份日志或假定“非slot0一定敌人”。
- weapon `weapon\\german\\flame_throwers\\flamethrower_pioneer_us`；ProjectileLogic.Weapon当前只接tank字典、heavy_artillery、light_artillery、mortar类，该weapon不匹配。
- sourceType16=3，source20=1000008241，source24=3049，radius104=5；来源类型/非零id/半径/反向引用均满足相应检查。
- actor=(−88.59104,26.07598,59.14277)，body target=(−89.14555,23.96101,56.54527)，未到达目标；state64=1对非mortar/非arc仍失败。

## 与Reader每关的对应及未覆盖

1. Registry容量/offset/generation：按当前 [PlayerInfoReader.cs:3092–3141](../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3092-L3141) 读取与边界复核。
2. `shellSeen`首次warmup/跨poll缓存：本探针刻意不依赖该缓存，一次扫描全部live对象；**不是实际reader第一次调用的输出**，也未验证随后缓存时序。
3. blueprint路径、entity generation/index、body388、Component lookup、active bit、mode、RTTI：逐项记录；inactive与空缺不同。
4. ext owner：对全players表记录关系，未代替调用者enemyPlayers集合筛选。
5. weapon分类：依据当前ProjectileLogic.Weapon与AttackCatalogue67的四项tank字典；本次唯一active是flamethrower，明确在解码前过滤。
6. Decode字段：记录actor/ext exact类型、backref、mode16、state64、flags160、位置、source/radius等；只计算离线gate，不运行C# Decode。
7. `ValidPosition`仅做有限性/宽范围诊断，不冒称精确复刻原函数；本次早期weapon已失败，不影响其过滤结论。
8. Source-name、match epoch、地图/渲染、预测时间、去重和历史shellSequence未执行。零预测项不能证明这些通过或出错。

## 可重跑脚本与下一步

[projectile_probe.py](projectile_probe.py)接受PID与唯一tag，仅产生同目录JSON：

```powershell
& 'C:\Program Files\Python311\python.exe' 'F:\WorkSpace\Game\CoH3\dev\SkyEye\evidence\version-adaptation\projectiles\projectile_probe.py' --pid 11192 --tag resumed-mortar-01
```

由lead在现有scope内控制恢复/炮击后运行，使用新tag保留本次快照。遇到budget/根异常应视为inconclusive而非sample_absent；不要通过扩大无界扫描来补样本。预期找到允许weapon且active ProjectileExt后，再比较actor state64/position、owner敌方归属及真正ReadProjectileImpacts输出和画面落点。本次未请求或控制任何游戏动作。
