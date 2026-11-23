# 5.1.52426 候选布局 seam 独立静态审阅

## 审阅结论

**最新复核：下游decoder的旧版exact-guard路由缺口已由lead修复，五个reader调用显式传BuildRva45，双树保持镜像。尚须fixture覆盖与缺布局诊断传播；候选值的G/R不随修复自动通过。**

本文第1–6节保留初轮审阅快照与发现经过；其“七个guard未路由”的现状结论已被末尾第7节复核取代。第7节散列为最新复核基准。

本次只读源码、离线散列/依赖提取，不构建、不运行fixture、不操作游戏、不登记hash。重新加载 `karpathy-guidelines`，并沿用此前已重新读取的本地game-trainer-development技能，以当前源码调用链而非注释判定。

| 范围 | 结论 |
|---|---|
| 双树镜像 | 六对被审阅文件逐字节一致，见下面散列 |
| PlayerInfoReader本体RVA路由 | direct/global/lookup/property/transform共36依赖均有mapper键；未发现本体漏key |
| 下游pure decoder | **未全部路由**：AirLogic、ProjectileLogic、SupportCatalogue49共7个exact比较仍使用旧RVA，含mapper尚无的旧65313128 |
| rangebound | image+268435456是RTTI地址范围上界，不是exact类型RVA，不应送入mapper |
| LegacyTransform unknownthrow | fail-closed语义合理，与原MiniTransform default拒画一致；错误可见性另论 |
| Release维护入口 | 默认项目未定义维护宏；工厂、候选字典/解析/候选hash校验均在#if内；正常Main没有维护参数分派。此为源码默认配置结论，未检查新二进制 |
| 未知版本gate | 两个reader构造路径都在显式Native.OpenProcess前执行RequireVerified；维护例外再受独立候选hash与LicenseGate约束 |
| hidden catch | 当前reader本体36key齐全；但以后缺key/不支持类型会在部分逐对象catch被吞为少样本/零样本。现存decoder旧guard还可直接false/null，不经过异常通道 |

## 1. 两树逐字节比较

比较目录：SkyEye [客户端引擎](../../client/CoH3MemoryTool/) 与 SimpleTool [复制引擎](../../../COH3SimpleTool/CoH3MemoryTool/)。本次同名文件读bytes、SHA256，六对相等；不推出整棵树相同。

| 文件 | SHA256（两树相同） |
|---|---|
| [GameLayout52426.cs](../../client/CoH3MemoryTool/GameLayout52426.cs) | b5b29be06d90c33a59324a4b0429cd24eaf534350a2225d63f763ad1a707d7d0 |
| [PlayerInfoReader.cs](../../client/CoH3MemoryTool/PlayerInfoReader.cs) | eb19e87bddda81aaa036f030b7571b83644f312097d3be15f1e8eae78ce79e0a |
| [GameVersion45.cs](../../client/CoH3MemoryTool/GameVersion45.cs) | 2310f0e8e3a9671f5987190808fc01143fbc2e245d63a70990f58b7a33589640 |
| [AirLogic.cs](../../client/CoH3MemoryTool/AirLogic.cs) | 47bc0536ad81189776b2c2ce34b2741b945acbc0d300d19a4ed9a65880839c5d |
| [ProjectileLogic.cs](../../client/CoH3MemoryTool/ProjectileLogic.cs) | e88f0a981556c7f8fe4c423c3a0d309f9f70abb15acc5e3a4808e547a2c4f119 |
| [SupportCatalogue49.cs](../../client/CoH3MemoryTool/SupportCatalogue49.cs) | 2bf9c570658aa44815bb00134557138450d15425ee059317a451e25b0fc5e1ca |

这些散列锁定被审阅快照；并行后续修复可能改变行号和散列，本报告未将后来版本自动当作已复核。

## 2. 必要待改：下游decoder还有旧版exact guard

### P1：七个比较绕过BuildRva45

| 源码位置 | 旧guard依赖 | 当前调用位置/后果 |
|---|---|---|
| [AirLogic.cs:24](../../client/CoH3MemoryTool/AirLogic.cs#L24) | actor==image+64559016 | reader:553 RecoverStrafe；新版actor会返回null |
| [AirLogic.cs:68](../../client/CoH3MemoryTool/AirLogic.cs#L68) | raw==image+64568064 | reader:382 Cast；新版TimedAbility会返回false |
| [AirLogic.cs:101](../../client/CoH3MemoryTool/AirLogic.cs#L101) | motion==image+65313128，ext==image+65307976 | reader:513 Rocket；任一旧guard失配即null |
| [ProjectileLogic.cs:56–60](../../client/CoH3MemoryTool/ProjectileLogic.cs#L56-L60) | actor==image+65313128，ext==image+65307976 | [reader:3194–3214](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3194-L3214)先用新版ProjectileExt预检，再用旧版ext guard重验；正常候选无法满足两套相异RVA |
| [SupportCatalogue49.cs:23](../../client/CoH3MemoryTool/SupportCatalogue49.cs#L23) | raw==image+64568064 | [reader:3429–3436](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3429-L3436)先通过新版TimedAbility预检，然后旧guard返回false |

以上是7个exact比较、4个独立legacy依赖。现有mapper已有64559016/64568064/65307976，却没有65313128。**需要把decoder的expected-vtable依赖纳入同一显式layout契约，并独立定位/验证ProjectileDynamics的角色；不要用平移、伪造image基址或跳过类型检查解决。** 此处不擅自指定尚未复核的新ProjectileDynamics值。

这些函数可直接返回false/null，不抛异常，因此即使runner顶层捕获异常，也会得到空集合而不是“布局错误”。[LiveSampling.cs:107–117](../../tests/maintenance-live/LiveSampling.cs#L107-L117)把support/projectiles/air空结果送入Count；[LiveSampling.cs:128–131](../../tests/maintenance-live/LiveSampling.cs#L128-L131)将count0标为sample_absent。现有报告的“不证明功能覆盖”警告是正确的，但不抵消上述确定的代码适配缺口。

### P2：区分缺布局与瞬态/无事件

[GameLayout52426.cs:62–66](../../client/CoH3MemoryTool/GameLayout52426.cs#L62-L66)缺key抛InvalidOperationException，fail-closed方向正确；但下列通道会将该异常与读取竞态混在一起：

- 弹体逐对象 [PlayerInfoReader.cs:3184–3231](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3184-L3231) 的空catch：可丢弃对象并最终返回空列表。
- 支援逐技能 [PlayerInfoReader.cs:3421–3462](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3421-L3462) 的空catch：同样可形成零列表。
- 移动 [PlayerInfoReader.cs:2096–2098](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2096-L2098) 空catch：Movement可能保持null，无法仅由null判定未移动。
- 生产不是纯吞零：[PlayerInfoReader.cs:2967–2976](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2967-L2976)把ProductionKnown/Kwk38Known设false；runner [LiveSampling.cs:68–73](../../tests/maintenance-live/LiveSampling.cs#L68-L73)检查known并判失败，已保留错误信号，虽未保留原始缺key消息。
- 地图catch保存消息：[PlayerInfoReader.cs:1495–1497](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L1495-L1497)、[3754–3767](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3754-L3767)；runner通常会判discovery_timeout并附MinimapStatus/TacticalStatus，不应当作干净的“未打开”。

当前本体36key已经齐全，所以不是报告一个正在发生的缺key；这是维护证据可信度缺口。新增依赖时应有明确的layout preflight或独立异常/诊断通道，让缺布局到达维护runner成为fail/blocked，而不是sample_absent；无需重写全部瞬态catch。

## 3. Reader本体路由覆盖：36/36

[BuildRva45:2355–2360](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2355-L2360)在维护reader调用Resolve，正常reader原样返回legacy。静态抽取直接BuildRva45字面量、Component第二参数（包含嵌套Resolve）、MiniPropertyId参数、vehicle三元guard及5种reverse-transform键，合并36个独立key；与 [mapper:22–60](../../client/CoH3MemoryTool/GameLayout52426.cs#L22-L60)集合相等，无missing/unused键。

- Component [2543–2576](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2543-L2576)：lookup、payload pool、arena table均经过BuildRva45；type*128/slot*24/mode*16是结构步长，不是RVA。
- MiniPropertyId [1281–1284](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L1281-L1284)：动态参数slot同样映射。Matrix/PlayableWidth/PlayableHeight三种slot均在字典中。
- 生产 [2910–2925](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2910-L2925)：Spawn/vehicle upgrade/ordinary upgrade分别映射，并非只处理数值字面量的简单替换。
- 小图/战术图控件、HUD页、UI根、五类Transform、属性slot、camera/colour manager/global registry等在reader本体已路由。
- Reverse transform [GameLayout52426.cs:69–73](../../client/CoH3MemoryTool/GameLayout52426.cs#L69-L73)只识别5个显式候选值后转回旧switch角色；不是范围猜测。

该覆盖只证明依赖被列入mapper，不证明字典值或结构字段已完成G/R。尤其TLS pool目前仍是index0镜像读取；mode1–8、loiter/升级事件和跨局验证未因封装而自动完成。

## 4. 非问题：rangebound、unknown transform与Release

### rangebound不该映射

[PlayerInfoReader.cs:2102–2122](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2102-L2122)的image+268435456是vtable/COL位于镜像附近的宽范围上界；后续num3是从RTTI COL读出的运行时RVA，再以image+num3+16读类型名。这不是旧build固定exact guard，不送BuildRva45是正确的。该宽上界也不等于真正PE SizeOfImage校验；是否收紧属于原有健壮性问题，不是本次遗漏重定位。

### LegacyTransform unknownthrow合理

[PlayerInfoReader.cs:1302–1366](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L1302-L1366)原有switch就只支持五种类型并在default抛错；新反向mapper不把未知类型当Identity、不回落旧RVA，保持“未知变换暂不绘制”的语义。需要关注的是上节诊断传播，不是删除throw。

### Release入口隔离成立（源码默认配置）

- 维护工厂在 [PlayerInfoReader.cs:2300–2311](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L2300-L2311) 的#if内；字典/候选hash校验/Resolve/LegacyTransform也均在 [GameLayout52426.cs:10–75](../../client/CoH3MemoryTool/GameLayout52426.cs#L10-L75) 的#if内。
- 正常public/context构造明确传false；private bool字段存在不等于存在release维护入口。
- [SkyEye项目](../../client/天穹之眼-v0.60.8.csproj#L1-L25)与 [SimpleTool项目](../../../COH3SimpleTool/COH3SimpleTool.csproj#L1-L25)没有定义MAINTENANCE_DIAGNOSTICS；工作区发现的Directory.Build.props只属于LicensingSystem子树，不影响CoH3默认项目。
- [ControlsProgram.cs:14–28](../../client/CoH3MemoryTool/ControlsProgram.cs#L14-L28)和 [SimpleProgram.cs:14–27](../../../COH3SimpleTool/CoH3MemoryTool/SimpleProgram.cs#L14-L27)没有解析args进入维护分支。
- 维护专用 [MaintenanceLive.csproj:10–19](../../tests/maintenance-live/MaintenanceLive.csproj#L10-L19)才定义宏并链接源码，位置在tests，不是client默认Compile范围。

这不证明已有output包中恰是当前源，也不防止操作者显式给普通项目注入DefineConstants；后者属于构建配置控制，需在D阶段检查实际编译常量/产物。

## 5. 版本gate在显式native-open之前

两条路径均成立：

```text
context ctor: Assess(2329) -> RequireVerified(2330)
 -> [maintenance RequireCandidate(2332)] -> LicenseGate.Matches(2334)
 -> Native.OpenProcess(2338)
normal ctor: Assess(2393) -> RequireVerified(2394)
 -> [maintenance RequireCandidate(2396)] -> LicenseGate.Matches(2398)
 -> Native.OpenProcess(2402)
```

[GameVersion45.cs:33–47](../../client/CoH3MemoryTool/GameVersion45.cs#L33-L47)在默认Release对非Verified抛异常；仅维护宏且maintenanceDiagnostics=true时放行，随后 [GameLayout52426.cs:11–19](../../client/CoH3MemoryTool/GameLayout52426.cs#L11-L19)重新SHA256限定当前候选hash。SupportedHash仍是旧值，没有登记新版。

这里“before open”指应用显式的Native.OpenProcess(0x410)读句柄；Process.StartTime/MainModule等元信息查询仍会由.NET/OS执行，不把它夸大为完全零系统进程访问。[GameIdentityProvider60.cs:29–34](../../client/CoH3MemoryTool/GameIdentityProvider60.cs#L29-L34)只Assess并读日志身份，不调用Native.OpenProcess，身份可显示不等于结构reader已经放行。

## 6. 低优先级文案与后续验收

[GameLayout52426.cs:57](../../client/CoH3MemoryTool/GameLayout52426.cs#L57)对ID18仍写“no active sample yet”；本次前序world-registry探针已有ProjectileExt活跃样本，详见 [role-report.md](components/role-report.md)。属于注释落后，不是RVA行为错误。

必要顺序：先补全decoder布局依赖与缺布局可观察性，再由维护编译/fixture与真实事件验证guard路径；最后才考虑G/R和双保护交付。当前审阅不宣布任何未测功能通过。

## 7. lead修复后的独立复核（取代初轮P1现状）

重新读取五个helper签名/七个guard、五处reader调用及mapper。确认：

- `ProjectileLogic.Decode(..., Func<long,long> resolveRva=null)`、`AirLogic.Cast/Rocket/RecoverStrafe(..., Func<long,long> resolveRva=null)`、`SupportCatalogue49.Active(..., Func<long,long> resolveRva=null)`都仅对exact-vtable使用`resolveRva?.Invoke(legacy) ?? legacy`。
- Reader:382/513/553/3214/3434五处明确传BuildRva45，未漏任一production调用。
- mapper新增 `{65313128,0x3E4AA38}`，标注ProjectileDynamics静态候选，event字段待验；覆盖由36增为37独立legacy角色。其余三个helper legacy依赖复用已有key。
- delegate异常不会被`??`转回legacy：`?.Invoke`抛出的异常仍向上传播；只有delegate本身为null才用legacy。没有全局状态，独立reader可各自传mapper。
- 默认release的BuildRva45返回legacy；旧fixture省略optional参数保持旧guard，不要求改造fixture就可编译。实际编译结果尚未由本审阅验证。
- 初轮P1旧guard绕过问题已在源码层修复；P2逐对象catch导致缺布局错误失去可见性仍成立。非维护路径没有因此开放维护入口。

五对文件最新逐bytes镜像一致，SHA256：

```text
GameLayout52426.cs 70ca2eab54ae21a2c62dfbcddf8aa928f625390ffe186b2e607581554963f3ba
PlayerInfoReader.cs a6d13354cd8f7b175c98a3b7942213596f1ca41757b4db2045ddf36b78579ba5
AirLogic.cs 22758734981407a68805937364b77dd33bda8cab545cf39c22767a1a9c1ef603
ProjectileLogic.cs e22bac88a7bbeda772067eb2bbe17fccd8d89b1b25e4b2e5653b78f035af7123
SupportCatalogue49.cs dc5cc8f6557ea5d10d26a06e33c066ddb7ddb5a1fc5b4b81d427d13ad4c60ebb
```

### 建议独立fixture（本次只提出，不构建/运行）

1. 每个helper构造一份所有非vtable guard均满足的旧buffer，省略delegate应通过；显式identity mapper应与默认完全一致。
2. candidate-buffer写入候选vtable，显式mapper应通过，而省略delegate应失败；同一body只更改expected-vtable以隔离字段逻辑。
3. Projectile/Rocket的actor、ext两个类型分别单独破坏，均应失败；修复其中一个不得掩盖另一个错误。对4个独立legacy角色收集delegate调用key断言，不仅断言输出非空。
4. mapper对一个key抛专门异常：直接helper应传播，测试reader/runner最终记录fail/blocked或明确layouterror，禁止仅留下sample_absent。加一个本来就无事件的负对照，保证它仍是sample_absent。
5. 两个实例使用不同mapper交错调用，确认没有跨实例候选污染；null/短buffer保持原有返回行为、不为无关短buffer硬调用mapper。
6. LegacyTransform五类candidate逐一反向得到legacy；未登记candidate应throw，不能Identity或旧RVA回退。常规release中actual legacy仍走原switch。
7. 静态依赖fixture遍历37个key且不得漏helper里的Resolve调用；编译release/maintenance两配置，检查release无维护工厂/候选表/候选hash字符串，并验证未知build在reader显式Native.OpenProcess之前抛出。
8. 候选ProjectileDynamics来自静态RTTI，不代替现场actor RTTI+backref+mode/owner/weapon字段复核；使用合成buffer的通过也不证明真实事件读链完整。
