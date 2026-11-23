# 新版全功能验收矩阵（只读梳理，2026-11-21）

## 状态与原则

这是验收范围与源码调用链调查，不是执行结果。所有下列实际游戏用例本轮均为 `not_run`；之前能力调查的专窗截图仅证明已有暂停对局与截图路径可用。不得把空集合、无异常、授权通过、保护自检或UI开关变色当作某项功能生效。

用户已确认：两工具全功能；离线 AI 首轮；实际对局/恢复/跨局后登记；两套保护交付、验证记录及回滚；无 commit；冻结 original；隐私与 ProductId 不变。

源码由主代理并行适配，下面行号为本次读取时的锚点，后续编辑后可按方法名复查。本报告仅新建本文件，未改引擎、授权或界面代码，未启动客户端/构建，未操作游戏。

## A. 两个 UI 的主开关接线

仓库根为 F:/WorkSpace/Game，下列链接采用本报告位置相对路径。

| 功能 | SkyEye UI | SimpleTool UI | 数据链/实际作用 | 验收重点 |
|---|---|---|---|---|
| 顶部玩家信息 | [OverlaySettingsForm.cs:1435–1438](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1435-L1438) → preferences.Enabled | [ControlBarForm.cs:146–149](../../../COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs#L146-L149) → 同一偏好 | Poll → PlayerInfoReader.Snapshot → DisplayRows → EnemyOverlayWindow.UpdateRows；[EnemyOverlayWindow.cs:153–190](../../client/CoH3MemoryTool/EnemyOverlayWindow.cs#L153-L190) 按开关、授权、3秒新鲜度、前台窗口绘制 | 真实资源/人口/玩家归属与受控变化对照；关隐藏，开恢复，筛选/颜色/文本正确 |
| 主画面标记 | [OverlaySettingsForm.cs:1439–1442](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1439-L1442) → preferences.SquadMarkers.Enabled | [ControlBarForm.cs:150–153](../../../COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs#L150-L153) → 同一偏好 | Poll → squadOverlay.UpdateRows → SquadMarkerWindow；独立采样 ReadSquadMarkers / ReadMineCamera / ReadProjectileImpacts / ReadAirZones | 移动、朝向、镜头投影及开关闭环；不要把主开关当成所有独立图层总闸 |
| 小地图标记 | [OverlaySettingsForm.cs:1443–1446](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1443-L1446) → preferences.Minimap.Enabled | [ControlBarForm.cs:154–157](../../../COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs#L154-L157) → 同一偏好 | Poll → miniOverlay.UpdateRows → MinimapMarkerWindow.Frame → TryReadMinimap / ReadSquadMarkers；战术地图是另一个 Tactical51.Enabled | 地图世界/像素变换，移动终点、弹幕、V1；与战术地图开关分别验收 |

SkyEye Poll 主链见 [OverlaySettingsForm.cs:589–634](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L589-L634)；SimpleTool 同链见 [ControlBarForm.cs:356–393](../../../COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs#L356-L393)。两者都独立调用地雷、战场、小地图、空袭四个 UpdateRows，主画面开关并未包住所有这些调用。

SimpleTool CheckedChanged 调 set→Changed，并同步绑定见 [ControlBarForm.cs:191–207](../../../COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs#L191-L207)。SimpleTool 无参数面板不是引擎里没有对应图层：需分别记录“引擎支持/默认配置是否启用/UI是否暴露”。不要为验收而默默恢复已删的热键/声音/设置页面。

## B. 全功能/依赖/证据矩阵

所有行均需记录：游戏文件SHA256、客户端SHA256与UI方向、PID/startTime、MatchKey、触发前后时间、预期/实际、错误与截图/日志、关闭/恢复结果。稀有事件未出现应记 not_run，不记 passed。

| ID | 功能族/适用UI | 引擎或UI源码锚点 | 受控验收与负面对照 | 本轮状态 |
|---|---|---|---|---|
| F01 | 玩家基础情报；两UI | PlayerInfoReader.Snapshot；OverlayText；SkyEye [Info46](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1660-L1768) | 自己/队友/敌方归属、资源/人口/CP，生产/消耗导致读数变化；相同昵称不混人；敌方筛选与含队友开关 | not_run |
| F02 | 单位/建筑/战斗群/升级/生产；两引擎，SkyEye可细调 | Snapshot → UnitNames/BuildingNames/ProductionText/ChineseNames；同 Info46 | 单位生产完成、建筑建造完成、选择战斗群、升级完成前后；计时语义、名称与归属；不把名称表读通当各字段通过 | not_run |
| F03 | 玩家颜色、数值渐变/闪烁、字号布局；SkyEye，Simple用现有配置 | SkyEye Info46 1699–1768；ColourError；EnemyOverlayWindow | 游戏颜色跟随/手动回退分开；阈值越界/恢复；多玩家长文本不溢出；字体/位置保存再启 | not_run |
| F04 | 单位主画面名称/步兵圆环/载具方框；两UI | [World46:1771–1790](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1771-L1790)；SquadMarkerWindow.ReadSquadMarkers调用416行、相机340行 | 不同单位种类、遮挡/离屏、镜头平移/缩放、开关；投影位置与游戏单位关联 | not_run |
| F05 | 移动目标连线/终点；两引擎 | [World46:1791–1810](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1791-L1810)；ReadSquadMarkers | 下移动命令→目标变化→停止/到达后消失；防止把当前位置当目标；镜头变动仍吻合 | not_run |
| F06 | 朝向/机枪射界/实际小队与载具射程/备用示意；SkyEye设置，Simple引擎 | [World46:1811–1850](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1811-L1850) | 机枪转向/架设、小队/载具分别验证；实际射程与自定义备用长度分开，不将示意线报成真实射程 | not_run |
| F07 | 地雷/炸药，双方颜色；两引擎 | [World46:1851–1862](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1851-L1862)；MineOverlayWindow.Frame139行/ReadMineCamera164行 | 放置、清除/引爆，归属与颜色；功能关隐藏；没有地雷时为空不算通过 | not_run |
| F08 | 弹幕技能目标/范围；两引擎 | [Threat46:1883–1889](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1883-L1889)；MineOverlayWindow | 使用对应技能→目标/范围出现→结束清除；非弹幕事件负对照 | not_run |
| F09 | 逐发炮弹落点/范围/编号；两引擎 | [Threat46:1891–1918](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1891-L1918)；SquadMarkerWindow.ReadProjectileImpacts437行；ProjectilePipeline65 | 敌方/自己/队友三种过滤分别切；炮弹存在/消失、地形/镜头变化；重复启停不留旧弹道 | not_run |
| F10 | 飞机条形区域/主画面V1/小图V1/清空；两引擎 | [Threat46:1919–1952](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1919-L1952)；AirOverlayWindow.ReadAirZones375行、TryReadMinimap408行、ReadMineCamera424行 | 对应飞机与V1技能逐项；条形方向/宽长/时效；清空仅清当前，新事件仍出现；无对应技能记未测 | not_run |
| F11 | 小地图单位/移动/弹幕/V1；两UI | [Mini46:1955–2004](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L1955-L2004)；[MinimapMarkerWindow.Frame:235–277](../../client/CoH3MemoryTool/MinimapMarkerWindow.cs#L235-L277) | 比对世界点与小图点；尺寸/旋转/边界；布局改变、重新连接地图；关隐藏，开重新取新帧 | not_run |
| F12 | 战术地图标记；SkyEye明确开关，Simple仅继承偏好 | Mini46 Tactical51.Enabled1958行；TryReadTactical51；MinimapMarkerWindow.Frame260行 | 开/关战术地图与标记开关组合；关闭地图后清除，重开对齐；不能用只读到隐藏布局替代实际渲染 | not_run |
| F13 | SLD照明弹/支援指令提示；两UI | Simple [PollSupport49:422–448](../../../COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs#L422-L448)；SkyEye PollSupport49约2313行；ReadSupport49 | 新事件触发一次；首次连接只建baseline不补报历史；换局清seen；其他支援指令不误报 | not_run |
| F14 | 七类事件声音及MP3/WAV；仅SkyEye | [Sounds46:2006–2060](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L2006-L2060)；Poll alerts/armourAlerts→sounds.Play | 战斗群、基地、手雷、装甲生产、敌方弹幕、己方火炮就绪、SLD各自触发；开关/玩家选择/重复事件/静音与退出停止；解码一致不等于扬声器试听 | not_run |
| F15 | 复制热键（默认F8）/剪贴板/托盘；仅SkyEye | OverlaySettingsForm.RegisterHotKey201约353行，快捷页约2112–2153行 | 当前选中玩家文本、改键/冲突提示、前后台语义、退出注销、托盘恢复；勿泄露授权字段 | not_run |
| F16 | 包抄提醒（默认F9）/范围圆环；仅SkyEye | [Flank UI:788–858](../../client/CoH3MemoryTool/OverlaySettingsForm.cs#L788-L858)；UpdateFlank47、ReadSquadMarkers997行；FlankTests47 | 敌军接近/离开、朝向/移动条件与范围；一次触发/冷却/关停；换局不复用旧对象；图环与真实距离对照 | not_run |
| F17 | 刷新/位置采样/耗时统计；SkyEye调参，两引擎运行 | World46 1863–1875；SquadMarkerWindow约638行 | 记录实际绘制FPS、P95、位置采样率、相机/单位读取、数据年龄；设置目标率不等于实测稳定率 | not_run |
| F18 | 全局生命周期与故障恢复；两UI | SkyEye DropReader465行/Poll异常666–690；Simple [DropReader302–319](../../../COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs#L302-L319)、Poll异常395–418；Minimap epoch/token | 短读/切后台/最小化/场景切换/对局结束/下一局/游戏退出/新PID；旧标记及时隐藏、reader释放、自动重连、无旧局数据 | not_run |
| F19 | 持久化/三开关/退出与换码；两UI | SkyEye Changed、OverlayPreferences；Simple Toggle46/Changed与SimpleLicenseApp.ChangeLicense60 | 保存后重启一致；退出隐藏并下线；关闭优雅退出；不要改产品身份或导出凭据 | not_run |
| F20 | 裸版与保护版同一功能集、交付与回滚；两UI | Build-Protected、Verify-Protected、输出hash与包验证 | 关键对局场景裸/保护分别测；从最终目录启；依赖完整；旧发布身份及回滚清单保留；完成前不登记新hash | not_run |

注意：SimpleTool README明确删除热键/声音/设置/托盘/预览，故F14–F16的对应UI行为不属于SimpleTool恢复项；其 retained 引擎和现有默认图层仍需测试。SkyEye原README称六模块，但当前源码另有07包抄提醒，应以当前UI/源码可见功能为准。

## C. 现有测试入口与避免假阳性

1. **正式 SkyEye 入口不是 OverlayProgram。** [项目文件:6](../../client/天穹之眼-v0.60.8.csproj#L6) 指定 `CoH3MemoryTool.ControlsProgram`；[ControlsProgram.cs:14–30](../../client/CoH3MemoryTool/ControlsProgram.cs#L14-L30) 不解析args，直接启动 LicenseApplication60。正式EXE追加 `--self-test` 或 `--verify` 不会自动运行同名其它类的Main。
2. **SimpleTool正式入口同样没有测试参数。** [SimpleProgram.cs:14–29](../../../COH3SimpleTool/CoH3MemoryTool/SimpleProgram.cs#L14-L29) 仅正常授权应用启动。
3. **源码中的 OverlayProgram --self-test**：[OverlayProgram.cs:24–26](../../client/CoH3MemoryTool/OverlayProgram.cs#L24-L26)；覆盖格式、筛选、AlertTests、设置往返、窗口样式与UI预览等。但 [205–216行](../../client/CoH3MemoryTool/OverlayProgram.cs#L205-L216) 捕捉实时读失败后记 `LIVE READ UNAVAILABLE`，仍返回0。它需要专门选择入口编译/调用；不可把exit0等同真实游戏通过。
4. **源码中的 PlayerInfoProgram --verify**：[PlayerInfoProgram.cs:16–39](../../client/CoH3MemoryTool/PlayerInfoProgram.cs#L16-L39) 读 Snapshot 输出玩家信息；也是非正式StartupObject。旧文案含“supported hash”，不应在没有独立BuildStatus证据时照抄为新版认证结果。
5. **当前 VerifyLive58内部采样链**：[PlayerInfoReader.cs:253–315](../../client/CoH3MemoryTool/PlayerInfoReader.cs#L253-L315) 覆盖Snapshot/唯一自己/颜色、单位坐标、相机、独立marker连接、小图、战术布局、支援/弹道/空域计数。它很适合作为独立只读诊断，但0个支援/弹道/空域不证明事件功能有效；战术布局存在不证明图层实际显示；单次调用不证明跨局/新进程稳定。并行适配中新加维护诊断文字明确不等于build verification。
6. **受保护验证器**：[Verify-Protected.ps1:7–10](../../Verify-Protected.ps1#L7-L10) 本地三组selfcheck/ui/audio，联网追加protocol永久/定时、coordinator、network；[verification/Program.cs:33–60](../../verification/Program.cs#L33-L60) 对应mode分派。此套主要证明授权/保护/UI/音频，不替代F01–F20实际游戏验收。脚本会写测试sandbox和evidence，且读取前次保护构建；本轮没有执行。
7. **SimpleTool Build-Protected -Smoke**是构建/UI冒烟，不是全对局。既有 [README:54–58](../../../COH3SimpleTool/README.md#L54-L58) 明确历史真实回归停大厅，未测对局渲染。
8. AlertTests/FlankTests47是源码中的逻辑测试类；需要查清具体调用入口与产物。仅存在文件或能够编译，不宣称本轮执行通过。

## D. 用既有授权正常启动客户端（不读取私密数据）

- 现有输出可执行：`F:/WorkSpace/Game/CoH3/output/SkyEye/SkyEye-HardenedV2-Client/天穹之眼-自有授权版-v0.60.8.exe` 与 `F:/WorkSpace/Game/CoH3/output/COH3SimpleTool/COH3工具.exe`。这些为调查时发现的既有输出，未证明已含新适配；最终验证应绑定新构建hash，而不是凭路径当最新版。
- 先保留现有游戏单进程和对局，正常启动一个方向客户端，另一个方向等前者优雅退出后再测；两产品共享授权身份/凭据，避免并行抢会话造成额外in_use噪声。
- [LicenseCoordinator60.cs:67–81](../../client/CoH3MemoryTool/LicenseCoordinator60.cs#L67-L81)：默认由应用自身 LicenseStore60.Load 载入凭据，且仅 `credentials.AutoLogin && 非空Code` 时 wanted=true；启动Loop自行走正常身份/服务端校验。调查者无需打开凭据文件、打印卡码或修改存储。
- [LicenseApplication60.cs:27–80](../../client/CoH3MemoryTool/LicenseApplication60.cs#L27-L80) 正常显示登录、client.Start，输入事件走client.Submit；[SimpleLicenseApp.cs:27–76](../../../COH3SimpleTool/CoH3MemoryTool/SimpleLicenseApp.cs#L27-L76) 同机制。若既有记住且AutoLogin有效，等待正常登录；若出现登录页，用户可在UI自行输入已有卡码；不从文件/内存取码，不改授权。
- [SimpleLicenseApp.cs:131–151](../../../COH3SimpleTool/CoH3MemoryTool/SimpleLicenseApp.cs#L131-L151) active后ResumeProtected并显示控制条；失去授权则StopProtected。界面打开不等于LicenseGate允许，允许也不等于游戏读取/渲染通过。
- 保持 app.config/公钥/ProductId 原样；不发新卡、不运行服务部署、不改pin。网络/授权提示作为独立层记录，不能通过放宽校验来消除。
- 不把源码诊断入口当正常客户启动方法。正式客户端无参数启动即可；任何独立诊断产物需清楚标注“维护诊断/非build登记”。

## E. 推荐执行记录格式

| caseId | UI方向/裸或保护 | gameHash/clientHash | PID/startTime/MatchKey | 前置事件与负对照 | 预期 | 实际 | pass/fail/not_run | 恢复/跨局结果 | 原始证据 |
|---|---|---|---|---|---|---|---|---|---|

若首轮离线AI阵营/技能无法制造某事件，保留not_run并明确缺哪一项；不要让“全功能”退化成只测三个主开关或一次Snapshot。所有已确认的功能与生命周期证据完成后，再处理双份版本登记及保护交付。
