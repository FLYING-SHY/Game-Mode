# CoH3 5.1.52426.0：只读静态适配候选

## 结论与证据等级

**结论：资源根 / 资源名称索引表、world-offset / arena 全局量有可复查的静态候选；关键 UI、相机、实体类的新版 RTTI → COL → vtable 身份已提取。全体结果仍为 S（static candidate），不是新版已支持声明。** 没有修改引擎，没有构建，没有启动或附加游戏，没有写入 original。

- 新目标：`G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe`，文件版本 `5.1.52426.0`，长度 `108391096`，独立实测 SHA256 `8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec`。
- 旧支持 SHA256：`2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde`。受限于本次证据目录和 CoH3 内发现，没有找到该 hash 的旧游戏 EXE / 确定位置；因此没有做旧 PE 与新 PE 的双样本对应证明。
- preferred image base `0x140000000`；所有本文地址均为 **RVA**，运行时 VA = 当前模块基址 + RVA。不要把 preferred VA 当运行时地址。
- 读取了仓库规则、karpathy-guidelines、game-trainer-development 与 engineering 指南；REA doctor 由其他 agent 负责，本调查只使用 Python `pefile`、`capstone`、`numpy` 处理磁盘文件。

## 1. 资源根与资源表（优先结果）

| 项目 | 旧 reader 依赖 | 新版候选 | 静态支持 |
|---|---:|---:|---|
| 资源 singleton 地址 | `0x6BB0020` | **`0x6BB1ED0`** | getter 函数 `[0x930C10,0x930CA3)` 返回该地址；独立初始化函数 `[0x930CB0,0x930CEC)` 也使用该地址 |
| 名称表 begin | root+840+32 | **`0x6BB2238` = root+872** | `0x2727FE` 的 RIP MOV |
| 名称表 end | root+840+40 | **`0x6BB2240` = root+880** | `0x272810` 的 RIP CMP |
| 条目大小 | 48 | **48 / `0x30`** | `0x27281E: add rax,0x30`；另外 end-begin 后常数除以48 |
| index 字段 | entry+40 | **entry+40 / `0x28`** | `0x272824: mov ecx,[rax+0x28]`，随后比较 `-1` |

可直接核算的字节：

```text
RVA 00930C32: 48 8D 05 97 12 28 06  lea rax,[rip+06281297] => 06BB1ED0
RVA 002727FE: 48 8B 05 33 FA 93 06  mov rax,[rip+0693FA33] => 06BB2238
RVA 00272810: 48 3B 05 29 FA 93 06  cmp rax,[rip+0693FA29] => 06BB2240
RVA 0027281E: 48 83 C0 30           add rax,30h
RVA 00272824: 8B 48 28              mov ecx,[rax+28h]
```

同一函数 `.pdata` 区间 `[0x271C60,0x27491C)` 内，`0x272880` 读取 end 减 begin，`0x27288E` 装入 `0x2AAAAAAAAAAAAAAB`，`imul` + `sar rdx,3` 实现有符号除48。循环比较的是 entry[0] 与一个全局键；**这并未直接证明 entry+8 的运行时字符串一定正确**。旧 reader 的 `StringAt(entry+8)`、SSO length+16/capacity+24、index<10 等仍需运行时交叉验证。

真实 NUL 边界资源字面量与 XREF：

| 字面量 | 新 RVA | 已提取的代码引用 |
|---|---:|---:|
| manpower | `0x476DF50` | `0x113139`、`0x2B2DC67` |
| munition | `0x476DF60` | `0x113199`、`0x2B2DCD7` |
| fuel | `0x476DF6C` | `0x1130A9`、`0x2B21F13` |
| command | `0x3DFB2F8` | 存在字符串本体；未由本次受限 RIP 预筛确定资源表关联 |

`0x462083F` 等还存在以 manpower 结尾的长字符串，不应当作独立名称；完整字符串边界已另列于 focused JSON。`shared_ability_1`、`shared_barrage_ability` 的精确 ASCII+NUL 搜索未命中；这不是名称已消失的证明，也不对编码 / 外部资源下结论。

历史证据 [root-probe-01.json](<../../adaptation-52426-20261121-004302/root-probe-01.json>) 曾报告：候选根下10个名称/index、2名玩家可读，状态为 `structure_candidate_read_success_not_full_validation`。本调查**只读了该历史文件，没有执行它对应的进程读取脚本**；此文件是单次既有观察，不提升本次静态等级。候选根由上述 getter / stride / index 代码独立复核，不只复述历史数值。

## 2. world-offset / arena

| 全局角色 | 旧 RVA | 新版 S 候选 | 复核点 |
|---|---:|---:|---|
| uint world offset | `0x5E0BECC` | **`0x5E0DECC`** | `0x17932A: 8B 3D 9C 4B C9 05` 读32位，紧随 `add rdi,[rcx+rax*8]`；另 `0x179966` 读32位后加arena并读取 `+0x100` |
| arena / resolver 表 | `0x6185EB0` | **`0x6187E40`** | 大量 LEA 指向该表，配合 TLS / mode 索引取得 arena 基址，再加32位对象offset |

两者分别有独立指令证据；**没有把某个 delta 批量应用到其他全局量**。原始 JSON 中精确目标引用的计数较大，仅代表字节预筛+单指令解码命中，不代表函数数量或所有引用均为可达代码。

## 3. 关键新版 RTTI / vtable 身份

下表是**新版类名身份**，不是已证明的旧→新替换表。每条已检查 x64 MSVC COL signature=1、pSelf=COL RVA、pTypeDescriptor 指向该名，以及 vtable[-1] 指向 COL。正文只列 object_offset=0 的主表，次级表详见 JSON。

| 新版类名（省略 `.?AV` 和末尾 `@@`） | TypeDescriptor RVA | COL RVA | 主 vtable RVA |
|---|---:|---:|---:|
| TranslateTransform@AVUI | `0x5EDCD40` | `0x50882B8` | **`0x3D73280`** |
| MatrixTransform@AVUI | `0x6072CA0` | `0x501BBB8` | **`0x3D733A0`** |
| TransformGroup@AVUI | `0x6079978` | `0x5017630` | **`0x3D73AE0`** |
| RotateTransform@AVUI | `0x5EDBE38` | `0x5087FC0` | **`0x3DD2D00`** |
| ScaleTransform@AVUI | `0x5EDBDD8` | `0x5088128` | **`0x3DD2E78`** |
| MapControl@WPFGUI | `0x5EDD550` | `0x500A0E8` | **`0x3E7B160`** |
| TacticalMapControl@WPFGUI | `0x5EDD578` | `0x5087368` | **`0x3E7B810`** |
| AnvilRTSHUDPage@WPFGUI | `0x5EE5F68` | `0x5085180` | **`0x3EAC6B0`** |
| WorldCameraInterface@WPFGUI | `0x60A6D10` | `0x50099B8` | **`0x3DBF138`** |
| CameraSwitchBoard@Camera | `0x6016A98` | `0x503B3A0` | **`0x3DB7728`** |
| CameraEntity@Camera（不同类，非旧cameraCore替换） | `0x6016A68` | `0x5261B58` | **`0x47CB750`** |
| GameUICore@UI@Relic（旧cameraCore角色候选） | `0x608FA38` | `0x5011E10` | **`0x3DB1068`** |
| TeamColourManager@UI@Relic（旧identity controller角色候选） | `0x6090540` | `0x5011AC8` | **`0x3DB0D68`** |
| Ability | `0x5E75BC0` | `0x5064FC0` | **`0x3DA7868`** |
| Entity | `0x5F73358` | `0x5063AD0` | **`0x3D910F8`** |
| ProjectileExt | `0x5F8B540` | `0x505DDB0` | **`0x3E49618`** |
| Player | `0x5F9A038` | `0x51B10C8` | **`0x47AA710`** |
| BufferedPlayer | `0x5F6FFD0` | `0x5064930` | **`0x3D89F50`** |

例如 MapControl / TacticalMapControl 均有多个次级 vtable，object_offset=208、216、344、352、360、368、504、512。扫描命中次级表时应减去相应 COL offset 才回到完整对象；只检查“类名相同”并不足以复用旧 `+472/+528` 字段路径。

旧 `MiniTransform` 的 case 行为与新版类名在语义上相符：translation、matrix、transform group、rotate、scale；只能作为候选优先级。新 Entity 的某个表恰好数值等于旧 guard，不代表该旧 guard 保持正确。相机尤其不要把 CameraEntity 和 CameraSwitchBoard 混为一类。

### 3.1 lead 运行时观察之后的独立静态交叉确认

lead 在本调查完成前回报另一 agent 的 `camera-snapshot-01.json` 观察。本调查未读取进程；据该新线索回查既有磁盘提取结果：

- camera root **`0x6187F58`**：`0x189E43: 48 8B 0D 0E E1 FF 05` 读取该全局，紧接 `0x189E4A: mov rcx,[rcx+0xB8]`。这独立支持旧 `Q(Q(image+root)+184)` 的表达式。
- GameUICore 主表 `0x3DB1068`；另有 COL `0x5011E60`、object_offset **40** 的次级表 `0x3EEF238`。它为“slotptr-40 找完整core”的观察提供静态类型解释。
- TeamColourManager 主表 `0x3DB0D68`，类型链见上表。
- 因而旧变量名 cameraCore **实际角色应优先核对 GameUICore，而不是凭名称选择 CameraEntity**。core+416矩阵、core+680 controller、+8 identity/+24 mode 属于 lead 另存的动态证据；本静态报告没有自行验证这些运行时字段。
- 本次额外确认 camera root 的新位置和类名，不把相邻的 payload global / identity registry global 顺带认定。

## 4. 完整旧全局/RVA依赖清单（36个唯一值，首轮历史快照）

**下表保留首次盘点时的状态，后续证据不回写成当时已知；当前追加状态见4.1、7、8、9节。** 36个值的统计范围是PlayerInfoReader，不覆盖后来发现的纯decoder常量（例如ProjectileDynamics旧guard65313128）。

每个值的所有源码行与原文均保存在 [pe-candidates.json](<pe-candidates.json>) 的 `old_dependencies`，来自只读 [PlayerInfoReader.cs](<../../../client/CoH3MemoryTool/PlayerInfoReader.cs>)。`0x10000000` 是地址范围上界，已从依赖清单剔除。

| 分组 | 旧 RVA / 角色 | 新版状态 |
|---|---|---|
| 资源、世界、arena | `0x6BB0020`、`0x5E0BECC`、`0x6185EB0` | 上述3个强S候选 |
| component payload / identity registry globals | `0x6185EE0`、`0x6186220` | 尚无独立新位置；不得据邻近delta填写 |
| 相机枚举 root | `0x6185FC8` | 新候选 `0x6187F58`，静态全局读取/+184表达式已确认，见3.1 |
| component lookup字节表 | `0x6288842`、`0x628884A`、`0x628884B`、`0x6288850`、`0x6288851`、`0x6288852`、`0x6288855`、`0x6288891` | 均待定位；调用类型/源码行已存JSON |
| UI singleton / PropertyId | `0x6B9C1C8`；`0x6AB2078`、`0x6ACD388`、`0x6ACD3B0` | 均待定位；名字表/RTTI不证明 PropertyId 的新版位置 |
| UI transform guards | `0x3D72280`、`0x3D723A0`、`0x3D72AE0`、`0x3DD00B0`、`0x3DD0228` | 已给语义相关新版RTTI，不标为已证实替换 |
| UI root/control guards | `0x3DD1FA0`、`0x3E7A150`、`0x3E7A800`、`0x3EAB6F0` | 已给 MapControl/TacticalMapControl/HUD 等候选，旧类名尚缺双样本证明 |
| 相机与identity guards | `0x3DAFEF8`、`0x3DB01F8` | 分别优先候选 TeamColourManager `0x3DB0D68` / GameUICore `0x3DB1068`；有独立新RTTI及lead的动态对象观察，不标为纯静态双样本映射证明 |
| entity/ability/projectile/health 等 guards | `0x3D90FF0`、`0x3D910F8`、`0x3D917A8`、`0x3D93B00`、`0x3D97CB0`、`0x3E48548` | 已给相关具名新表，仍需逐角色证明 |
| callable guard | `0xCD09F0` | 代码地址guard，非自动视作vtable；待定位 |

旧 Component 解码还依赖：`ushort(entity+76)`、`byte(image+lookup+type*128)`、`Q(image+payloadGlobal)+U(entity+64)+slot*24`；payload mode0..7 走 `Q(image+arenaTable+mode*16)+(uint)payload`，mode8为直接pointer。**lookup 的数值和type/slot/mode语义均未因根候选而获得新版有效性证明。**

### 4.1 后续追加证据状态（不覆盖上面的历史盘点）

| 依赖组 | 当前追加候选/证据 | 仍保留的限制 |
|---|---|---|
| component payload / identity registry | TLS-indexed pool anchor`0x6187E70`，colour registry`0x61881B0`，见7节 | pool是表锚，不等价固定外部线程选择；registry要保留hash/node/type多重校验 |
| component lookup | 静态Health`0x628A7EA`；agent1报告34种type/357实体独立映射，见[lookup-probe-01.json](<../components/lookup-probe-01.json>) | 静态只独立确认Health与另一索引；其他列不是由统一delta推得 |
| UI singleton / PropertyId | UI`0x6B9E078`；Matrix slot`0x6AB4018`，PlayableWidth/Height slots`0x6ACF328`/`0x6ACF350` | wrapper+32才是pointer slot；同名注册有多组，须对照property ID/类型/值 |
| UI root/control | GameUIAppWindow`0x3DD4BF0`、MapControl`0x3E7B160`、TacticalMapControl`0x3E7B810`、HUD`0x3EAC6B0` | lead另有动态对象/BFS证据；静态类身份不证明地图几何全部正确 |
| 七个剩余guard | TimedAbility、Spawn、Squad/EntityUpgrade、Mover、ProjectileExt、Mover callable，完整见8节与compact JSON | 升级、Loiter仍未观测；无旧PE双样本证明 |
| 后来发现的纯decoder额外guard | 旧`65313128`的ProjectileDynamics候选`0x3E4AA38`，见8.1 | 先前36值盘点只覆盖PlayerInfoReader，零样本不能排除decoder guard失配 |

全体新增结果仍按维护候选处理；后续观察由相应agent生成，不将这些S结果改写成验证通过或正式支持。

## 5. Signature 候选及限制

[focused-evidence.json](<focused-evidence.json>) 保存4个候选 signature、完整字节窗口、.pdata函数界限、掩码规则、可执行节内原始字节命中列表。

- `0x930C32` 的函数内13字节模式 `48 8D 05 ?? ?? ?? ?? 48 83 C4 30 5B C3` **有44个原始命中**；必须解码 displacement 验证目标 `0x6BB1ED0`，并检查 getter 初始化上下文，不能只取第一个命中。
- `0x2727FE` 的46字节资源遍历模式当前文件原始命中1个。
- `0x272880` 的44字节资源表计数模式当前文件原始命中1个。
- `0x17932A` 的45字节world-offset上下文模式当前文件原始命中1个。
- 唯一命中仅说明该文件内模式选择性；不证明内存对象、会话生命周期、计时器单位、玩家归属、地图几何、字段布局或下一次更新稳定性。
- 模式仅掩码 RIP disp 与直接跳转/call immediates；不是通用跨版本扫描器。指令预筛覆盖 optional REX + 常见6/7字节RIP MOV/LEA/CMP/MOVSXD，漏掉其他寻址/指令是预期限制，不能以未命中断言不存在。
- .data 超出 raw_size 的虚拟部分属于运行时初始化/BSS：JSON 的 `new_file_same_rva_bytes` 对这种位置返回 null，不把其他节的磁盘字节误认成根内容。

## 6. 复现及交付文件

```powershell
python CoH3/dev/SkyEye/evidence/version-adaptation/static/investigate_pe.py
python CoH3/dev/SkyEye/evidence/version-adaptation/static/focus_evidence.py
python CoH3/dev/SkyEye/evidence/version-adaptation/static/verify_evidence.py
```

- [investigate_pe.py](<investigate_pe.py>)：PE节、RTTI/COL/vtable、资源ASCII字符串、RIP候选、旧reader依赖的只读提取器。
- [focus_evidence.py](<focus_evidence.py>)：.pdata边界、重点反汇编、NUL边界字符串、重点RTTI、signature原始命中计数。
- [verify_evidence.py](<verify_evidence.py>)：磁盘hash、RIP目标、RTTI指针链、signature命中和清单数量断言。
- [pe-candidates.json](<pe-candidates.json>)：完整提取明细。
- [focused-evidence.json](<focused-evidence.json>)：建议先读的重点证据。
- [rtti-priority.json](<rtti-priority.json>)：类名预筛得到的优先vtable池（3912项，包含次级表，不是3912个独立类）。

## 7. 后续 UI / Property / Component 静态复核

本节新增独立磁盘证据保存于 [ui-followup.json](<ui-followup.json>)，由 [ui_followup.py](<ui_followup.py>) 生成。6项RIP目标重算、wrapper+32写入字节、component pool indexed指令断言为 PASS。lead 转发的动态结果仅作为旁证，非本agent执行结果。

| 角色 | 新候选 RVA | 可复查静态证据 |
|---|---:|---|
| UI singleton | `0x6B9E078` | `0x6A613D: 48 8B 1D 34 7F 4F 06`；随后比较对象+0x70/+0x74 screen dimensions；`0x14C9708` 将指针写入该global |
| 根窗口 vtable | `0x3DD4BF0` | GameUIAppWindow@WPFGUI，TD `0x60A10C0`，COL `0x500CBE0`；与lead回报父链根RTTI一致 |
| ColourConfig registry | `0x61881B0` | `0x405576: 48 8B 05 33 2C D8 05`；同函数+0x298 mask/+0x280 buckets/+0x270 sentinel，node+0x10比`0x320000`、node+0x18/+0x20为8字节vector、对象+0x48再比同key |
| Matrix property **pointer slot** | `0x6AB4018` | `0x1411BAD` 引用字符串Matrix `0x3EB5EA0`，调用`0x965A10`，`0x1411BC2`传wrapper `0x6AB3FF8` 到setter `0x9C5E30`，setter写wrapper+32 |
| PlayableWidth property **pointer slot** | `0x6ACF328` | `0xDF4566: 48 8D 15 8B 38 01 03`引用字符串`0x3E07DF8`；`0xDF457B: 48 8D 0D 86 AD CD 05`传wrapper `0x6ACF308` 到同setter，+32为slot |
| PlayableHeight property **pointer slot** | `0x6ACF350` | `0xDF4647: 48 8D 15 D2 36 01 03`引用字符串`0x3E07D20`；`0xDF465C: 48 8D 0D CD AC CD 05`传wrapper `0x6ACF330`，+32为slot |

关键更正：wrapper本体首Q是vtable，并非DependencyProperty实例。`0x9C5E45`读[rcx+32]、`0x9C5E52`读[rbx+32]、`0x9C5E56: 48 89 47 20`写[rdi+32]。所以外读应Q(pointer slot)再按对象字段取ID；错误地Q(wrapper)+184可产生看似ID的代码/字符串垃圾。lead另回报：Matrix slot得到DependencyProperty id69，Width/Height slot得到id874/875、BoxedFloat384/384；对应证据为 [property-snapshot-01.json](<../property-snapshot-01.json>)。**PlayableWidth/Height有多组同名字面量注册引用，仅名字不唯一确定角色；此组选择需结合lead的MapControl属性观察。**

Colour registry的完整结构和旧reader2165-2195相符：hash immediate `0x4CF5E27F9DA5A7B7`，node key `3276800`、vector单元素和对象类型key再检。lead回报 [colour-snapshot-01.json](<../colour-snapshot-01.json>) 与此相符。UI root动态旁证为 [ui-snapshot-01.json](<../ui-snapshot-01.json>)；这些报告由lead采集，本调查没有打开进程。

### Component pool：TLS索引不可丢弃

`0x2EB5E0` 实际使用lookup displacement `0x628A7EA`；`0x2EB609: 49 8B 8C CE 70 7E 18 06` 读取 `Q(image + tlsIndex*8 + 0x6187E70)`，加 `U(entity+64)` 后加 `slot*24`，调用descriptor resolver `0x820980`。第二条独立路径`0x1829EA`使用lookup `0x628A7E8`，`0x1829F7`同样使用pool表 `0x6187E70`。

`tlsIndex = DWORD(Q(Q(gs:[0x58])) + 0x40)`（此处表达其代码链，不等价外部线程选择方案）。helper `0x820980` 读DWORD[desc]为mode、Q(desc+8)为payload；mode8直接返回payload，否则取 `Q(image+0x6187E40+(tlsIndex+mode*2)*8) + uint(payload)`。因此 **`0x6187E70` 是TLS-indexed表锚，不应仅因多个表槽当前镜像而选用其他地址；外读Q(anchor)只对应index0快照，尚需证明所需线程/arena关系。** 本次追踪了编码为base+index+disp的指令，补足RIP预筛会漏掉这些寻址的限制。

旧→新lookup列的跨实体动态验证由agent1负责，见components证据；本agent只独立证明Health lookup实际机器码displacement `0x628A7EA` 与pool/resolver形态，并未把+8096批量推到其他无证据依赖。

后续动态验收仍应分两次全新会话，逐项记录资源名/index、实际UI资源值、玩家归属、arena/world生命周期和fail-closed行为；本次没有执行该步骤。资源读取修复也不等价于小地图/相机/技能/实体路径全部适配。

## 8. 剩余角色 guard 的独立静态补充

可优先复查 [compactguard-evidence.json](<compactguard-evidence.json>)（约24KB，完整sig/mask/raw命中数、解码规则、RTTI与字段指令）；扩展证据为 [role-guards.json](<role-guards.json>)，复现脚本为 [role_guards.py](<role_guards.py>)。脚本只读同一SHA256的PE，6条构造/安装RIP目标断言、COL self/TypeDescriptor/vtable[-1]断言、MoverDynamics vtable+344函数指针断言全部通过。没有附加或读取进程。

| 旧依赖 | 新版候选 RVA | 静态角色与证据等级 |
|---|---|---|
| `64568064 / 0x3D93B00` | `0x3D94A40` | TimedAbility，TD`0x5F6F3A8`、COL`0x5064E80`，继承Ability；agent1另报告运行时active mortar能力同类 |
| `64584880 / 0x3D97CB0` | `0x3D98BF0` | SpawnProductionItem，TD`0x5F9B110`、COL`0x505A6B0`，继承ProductionItem；agent1/agent2另报告敌jaeger生产对象同类 |
| `64557040 / 0x3D90FF0` | `0x3D92058` | **候选** SquadUpgradeProductionItem；与vehicle从SquadSimInventoryItem队列取项语义相符，暂无升级对象动态证据 |
| `64557304 / 0x3D910F8` | `0x3D92160` | **候选** EntityUpgradeProductionItem；与normal从ProductionQueueExt取项语义相符，暂无升级对象动态证据 |
| `64559016 / 0x3D917A8` | `0x3D926E0` | **候选** MoverDynamics；TD`0x5F98DB8`、COL`0x505AFE8`，继承EntityDynamics；Loiter路径尚未观测 |
| `65307976 / 0x3E48548` | `0x3E49618` | ProjectileExt，TD`0x5F8B540`、COL`0x505DDB0`，继承EntityExtension/IComponent；agent1另报告world registry实际ProjectileExt对象 |
| `13437424 / 0xCD09F0` | `0xCD0F10` | **函数候选，不是vtable**；确为MoverDynamics主vtable+344指针所指函数 |

### 构造与字段证据

- TimedAbility：`0x7646D7: 48 8D 05 62 03 63 03` 解码到`0x3D94A40`，后续`mov [r14],rax`、`mov word [r14+0xA0],bp`；`0x76568F`析构先装回同表，再调用派生/基类清理。其虚表+40指向`0x328A7F8`，直接访问+72(float)、+152(float)、+160(short)、+144(byte)，与旧reader活动字段位置交集明确。**该函数对+144使用not/and2；旧BarrageLogic还涉及bit4、+76与+80，故不等价证明整个Running谓词。** Blueprint指针+56由虚表+8函数`0x903190: mov rcx,[rcx+0x38]`再次支持；目标vector+8/+12 stride20尚无本段独立完整结构证明。
- SpawnProductionItem：`0x7E1E97: 48 8D 05 52 6D 5B 03` 解码到`0x3D98BF0`，`0x7E1E9E: mov [rcx],rax`，`0x7E1EA1: mov [rcx+0x10],rdx`，明确保留+16指针形态。
- 升级三类都继承IUpgradeProductionItem→ProductionItem，factory在`0x54C783`/`0x54C7C4`写+16指针，并分别安装SquadUpgrade/EntityUpgrade表。另有PlayerUpgradeProductionItem `0x3E57008`（TD`0x5F9AC20`、COL`0x505A7F0`），不是默认升级表的可靠替代。无旧PE做同类RTTI对照，也无当前升级对象，因此两项旧→新映射仍是角色候选。
- MoverDynamics：`0x707A9D: 48 8D 05 3C AC 68 03` 安装表`0x3D926E0`；+64保存mode，+172/+176初始化为无效offset。其vtable+344=`image+0xCD0F10`：读取DWORD[this+64]，mode0跳`0xCD0F60`并通过TLS arena解析[this+172]；mode1块`0xCD0F1D`通过TLS arena解析[this+176]再读状态+132。其他Building/Attachment/ProjectileDynamics的同槽为`0x109C9E0`，NavigationDynamics为`0x342C140`，可作角色排除参照。**Loiter所读+272/+280与飞行target行为尚未独立静态证实，矿物/建筑的state64=1也不等于飞机。**

### 签名使用约束

本次重要模式完整sig与等长mask均在compact JSON：`0xCD0F1D`（56字节）1个raw命中；`0x328A7F8`（64字节、函数内前缀）1个raw命中；`0xCD0F10`短模式6个raw命中；Spawn构造安装14字节模式15个raw命中；升级安装14字节各12个raw命中。短安装模式通过`instruction_rva + 7 + signed disp32`解码后再检查RTTI/COL角色，绝不把短模式本身当唯一识别。计数仅覆盖可执行section的文件raw bytes，不证明可达性、实时对象类别或旧→新语义等价。没有以统一delta填充任何缺失依赖。

### 8.1 纯decoder额外guard：ProjectileDynamics（第八项）

lead后续独立下游审计发现 [ProjectileLogic.cs:56](<../../../client/CoH3MemoryTool/ProjectileLogic.cs#L56>)、[AirLogic.cs:101](<../../../client/CoH3MemoryTool/AirLogic.cs#L101>) 中还有旧`65313128 / 0x3E49968`（不在最初PlayerInfoReader的36值清单）。其新版静态角色候选为 **ProjectileDynamics主表`0x3E4AA38`**，TD`0x5F992D0`、COL`0x505ADB8`、CHD`0x51AF910`，继承EntityDynamics。已追加到compact与hash manifest，不能因旧guard失配导致零sample而断言实际对象不存在。

- `0x10E25A2: 48 8D 05 8F 84 D6 02` → `0x3E4AA38`；随后写[this]。`0x10E25AC`将[this+64]初始化为2，这并非decoder要求mode0/3已满足的证据。
- 该表+456指向`0xA38920`。其中`0xA3897B: mov ecx,[rbx+0x24]`并与TLS arena相加取得entity；`0xA38999: lea rcx,[rbx+0x10]`调用`0x42B610`检查target，后续从同+16调用`0x42D290`读取3D点。支持backref+36、target+16的角色/字段形态。
- 该表+520指向`0x342DFA8`，`0x342E01B: 89 43 40`可写[this+64]，所以构造默认值不是对象整个生命周期的不变量。但mode0/3的具体活体状态、ext来源枚举、归属/半径和完整Decode谓词尚未由本调查证明。
- 同槽+344仍为`0x109C9E0`，不是MoverDynamics的`0xCD0F10`。两种dynamics角色不能因都有target/backref字段而混用。

## 9. 按hash的signature manifest与离线校验

首读 [signature-index.json](<signature-index.json>)（紧凑可读索引），完整机器清单为 [signatures-by-hash.json](<signatures-by-hash.json>)，约95KB。其唯一build键是新版SHA256大写值；旧支持hash仅作历史元数据，**没有fallback匹配或旧RVA默认值**。涵盖资源/root/world、arena/components、camera、UI/PropertyId/colour registry、最初七个guard及后来发现的第八个ProjectileDynamics。

- **30个pattern，全部1122个raw命中均保留，逐命中的解码目标也保留。** 复用既有15个签名记录，仅新增15个小范围关键指令模式；没有重扫RTTI或重写巨大的UI引用证据。
- 每条包含完整`sig`、等长`mask`（x固定、?通配）、expected RVA、全部raw hits、命中数、关键指令、逐decode-site的规则/expected target，以及每个raw hit实际解码出的target列表。
- 0 hit记`zero_hit`；multi记`multi_hit`；single也只记`unique_raw_candidate`。**三种均`selected_rva=null`，绝不自动取首或宣称运行有效。** 有些命中可在指令中间，解码只是附加约束，不构成可达性证明。
- 原先探索的极宽模式（例如仅LEA/call产生数万命中）保留在 [broad-pattern-exploration.json](<broad-pattern-exploration.json>)，不是首选定位模式；原证据没有删除。主manifest改用更完整的同一函数上下文，仍显示实际多命中而非隐藏它。

复现：

```powershell
python CoH3/dev/SkyEye/evidence/version-adaptation/static/build_signature_manifest.py
python CoH3/dev/SkyEye/evidence/version-adaptation/static/verify_signature_manifest.py
# 可指定磁盘目标；未知hash在扫描前退出，不选择候选：
python CoH3/dev/SkyEye/evidence/version-adaptation/static/verify_signature_manifest.py --target 'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe'
```

[build_signature_manifest.py](<build_signature_manifest.py>) 只整理已有证据与关键磁盘指令；[verify_signature_manifest.py](<verify_signature_manifest.py>) 独立重算hash、可执行节内全部重叠raw匹配、每个解码目标、8条RTTI链与Mover virtual+344。产物 [signature-validation.json](<signature-validation.json>) 为 **PASS_offline_internal_consistency_only**：30 patterns、1122 raw hits、0项失败；内置fixture的0hit、多hit及single-case全部通过，均无candidate选择。没有进程API、patch、引擎写入或构建；此PASS不是动态游戏功能验收。
