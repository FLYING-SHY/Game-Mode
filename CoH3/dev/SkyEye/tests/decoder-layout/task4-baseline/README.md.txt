# Decoder layout fixtures — F only

独立 console fixture，链接当前 client 中的真实3个 helper 与纯数据/目录逻辑依赖，不修改正式源码，无 mock/helper复制，无 PlayerInfoReader/Native/授权协调器/GUI链接。项目依赖闭包共30个源码，含数据类型以及 helper 在编译时需要的 catalogue、position、names类；运行只调用被测 decoder 与布局mapper，不调用 UnitNames/Centre/授权/进程发现等无关路径。

## 执行

仓库根：

```powershell
& .\CoH3\dev\SkyEye\tests\decoder-layout\Run.ps1
```

使用仓库 .NET10 SDK。依次构建运行 Release 与 Maintenance 配置；后一配置显式定义 MAINTENANCE_DIAGNOSTICS。所有 bin/obj/报告/日志在本目录，30个链接源文件运行前后 SHA256相等才成功。

当前结果：**Release89/89 + Maintenance97/97 = 186 PASS**，两配置0warning0error。静态数据字段的预期 CS0649 在此独立工程禁用；未改正式工程警告设置。

- `release-build.log` / `release-test.log`
- `maintenance-build.log` / `maintenance-test.log`
- `bin/Release/net10.0/fixture-report.json` / `bin/Maintenance/net10.0/fixture-report.json`
- `source-manifest.json`：30个实际链接源 SHA256与稳定性声明

## 真 rawbytes 验证矩阵

5个入口：AirLogic.RecoverStrafe、AirLogic.Cast、AirLogic.Rocket、ProjectileLogic.Decode、SupportCatalogue49.Active。使用byte数组写入真实offset，填齐identity、generation/index、owner/backref、actor mode/state、位置、方向、半径和source等guard。验证不是字符串比对；正例还验证返回的坐标/方向/半径/Owner/SourceKey等payload。

每个入口都覆盖：

1. 旧vtable buffer省略delegate通过；显式identity mapper输出一致，精确收集key调用序列。
2. 同一buffer只改vtable的8字节：默认legacy拒绝；候选mapper接受。额外按非guard字节逐字节相等断言，隔离类型路由与字段解析。
3. 各类型独立写wrong值及legacy值，在候选mapper下均拒绝。Rocket和Projectile分别破坏actor、ext，其他guard保持正确，避免只测其中一项。
4. 每个key逐一抛独有异常，验证直接helper传播**同一异常对象**，未吞为false/null。
5. 两个不同MappingContext实例提供不同mapper，交错调用互不污染；错误context读取另一context的buffer拒绝。
6. null/短buffer不调用mapper，保留旧行为。注意Rocket原函数未做null guard，null body的既有行为是NullReferenceException；fixture明确测试此行为而未修改引擎。另测actor、motion、ext、targets各自最小长度边界。
7. 正确mapper下的禁用/无活动状态负对照仍false/null，不混同mapper异常。

覆盖4个独立角色key：64559016(MoverDynamics)、64568064(TimedAbility)、65313128(ProjectileDynamics)、65307976(ProjectileExt)。7个guard经5入口实测。

Maintenance 使用真实 GameLayout52426.Resolve；同时执行真实ValidateDependencies，检查37个required key唯一且与候选表完全相等。五种已登记transform逐项candidate→legacy反向匹配；未登记角色和transform必须throw。

Release验证实际GameLayout没有candidate字段/Resolve。**Release fixture自身仍有4个候选数值作为显式注入测试数据**，不将该测试EXE冒充正式release产物，也不以它替代正式门控/候选hash泄露测试。完整reader门控与Release维护工厂缺席由独立build-gate测试负责。

## 单个真实采样 rawbytes 离线回放

Run.ps1还读取 `../../evidence/version-adaptation/projectiles/event-30s-02.json` 的records[0]，保留原392B body、80B actor、208B ext，不改其任何原始字节做正例。每配置另10项验证：长度、双backref推导同一arena、双vtable推导同一image、真实Mortar路径判定、默认legacy拒绝、candidate mapper+mortar=true接受、返回payload与记录metadata一致、mortar=false拒绝state64=3、actor/ext分别独立wrongtype拒绝。

该JSON漏存顶层image/arena，因此由raw分别推导后与采样者提供的context核对。较早同PID的 `../../evidence/version-adaptation/projectiles/paused-1142-01.json` 有独立EnumProcessModulesEx模块根与Q(base+arenaRva)记录，可作旁证；本fixture不将其冒充02样本同时刻的独立根记录。输入路径/SHA256/原始长度/context与限制均写入fixture-report；输入文件运行前后hash一致才成功。原始样本仍标记event_observation_not_CSharp_execution，本次新增的是独立C#离线回放证据。

## 证据范围

这186个断言仅为F：合成buffer及单个已采样rawbytes离线回放验证guard路由与decoder字段逻辑，不证明候选RTTI的完整动态事件链、owner/weapon/mode全场景语义、屏幕绘制、跨局恢复或游戏功能正常。`source-manifest.json`固定本轮源码快照；后续引擎更改应重跑。

Direct helper mapper异常传播已实测；reader逐对象catch→runner fail/blocked/layouterror的端到端分类**not_run**，本工程没有链接reader或runner，未声称该链已覆盖。37项检查对照当前显式required表与字典，不声称独立解析了整个reader语法树。原始游戏、输出交付、凭据、ProductId、pin均未写入或运行。
