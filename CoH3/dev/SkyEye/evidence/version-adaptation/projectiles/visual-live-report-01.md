# 实际炮击预测图层：显示、原生爆炸与消退

用例 `G-PROJ-VIS-01`，结果 `PASS_scene_display_sequence_only`。本次实际桌面先显示两组预测圈和编号；后续游戏自身出现对应场景的爆炸，图层先清掉编号 1，再清掉编号 2，其他单位标记继续更新。该结论限定于本场景的定性时序和实际桌面绘制；F09 全功能、逐弹身份和定量精度仍未完成，版本继续 candidate。

## 身份与采集边界

- 游戏 PID `31572`，启动 UTC `2026-11-20T18:44:29.8492330Z`，文件版本 `5.1.52426.0`，SHA256 `8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC`。
- 维护 SkyEye PID `23716`，正常入口与真实授权链；EXE SHA256 `3FE10E1295B6C5F6E5C49A22C3BBF2C55C3F421B52C09A7C83116AB7393F0E88`。完整绑定见[运行身份](<../round05-ui-rendering-identity.json>)。
- 原始[session](<visual-capture/live-01/session.json>) SHA256 `cfa8499596140018120cd849737ebe33f757fbadb70295bafb2856ac8b7c4eb8`；[完整性复核](<visual-capture/live-01-integrity.json>)检查全部 234 张图片的大小、SHA、窗口归属及组内顺序。
- 请求 60 秒、1000ms 节拍、80MiB；实际在 `byte_budget` 有界停止，39.062 秒、39 个完整组、234 张图片、83,253,861 bytes。第 40 组在写入前整体预算判断中丢弃，零部分写入、零缺失节拍；`CAPTURE_EXIT=3` 是该预算停止原因，非完整 60 秒。
- 每组顺序采集游戏 PrintWindow、四个独立层、实际桌面中的游戏窗口矩形。不同图片有独立起止 UTC，非原子同刻。桌面左侧另有独立桌面挂件；以下判断针对右侧炮位区域，不把挂件归为工具输出。

## 最短可复核时序

以下时间均为 **2026-11-21 UTC**，保留图片自身的采集区间。lead 已实际查看表中 group-001 desktop、group-002 game/world、group-006 desktop，早先还查看了 group-001/020/039 world；独立只读审阅另覆盖前 8 组 world 和关键 game/desktop，共 16 张。

| 图片 | UTC 起止 | 直接观察 |
|---|---|---|
| [group-001 world](<visual-capture/live-01/group-001-overlay-7801622.png>) | 01:13:27.062244–27.117899 | 两个红色预测椭圆，文字分别为“黄 / 1”“黄 / 2”。“黄”是名称文字，不代表像素颜色。 |
| [group-001 desktop](<visual-capture/live-01/group-001-desktop_game_region-527128.jpg>) | 01:13:27.118943–27.189034 | 两圈及编号确实叠在右侧两个阵地附近，此图该处没有明显亮爆。 |
| [group-002 game](<visual-capture/live-01/group-002-game-527128.jpg>) | 01:13:27.821848–27.866833 | 左侧阵地、此前编号 1 附近出现火焰、黑烟和放射状抛土。此游戏 PrintWindow 不含工具编号圈，可独立确认原生爆炸。 |
| [group-002 world](<visual-capture/live-01/group-002-overlay-7801622.png>) | 01:13:27.997621–28.051849 | 编号 1 及其圈消失，编号 2 与其他单位标记仍保留。 |
| [group-002 desktop](<visual-capture/live-01/group-002-desktop_game_region-527128.jpg>) | 01:13:28.052856–28.119151 | 实际桌面同样只剩编号 2，左侧阵地仍有爆炸烟尘。 |
| [group-005 desktop](<visual-capture/live-01/group-005-desktop_game_region-527128.jpg>) | 01:13:31.098973–31.162783 | 编号 2 仍在。 |
| [group-006 world](<visual-capture/live-01/group-006-overlay-7801622.png>) | 01:13:32.085169–32.140330 | 两个编号圈均已消失，其他单位标记仍有绘制。 |
| [group-006 desktop](<visual-capture/live-01/group-006-desktop_game_region-527128.jpg>) | 01:13:32.141835–32.214771 | 右侧阵地出现明显爆闪，桌面中无这两个编号圈。该组 game 图更早采于 01:13:31.815653–31.877585，未显示同一爆闪，故此处仅引用 desktop。 |

group-002～005 world 持续只有编号 2，group-006～008 已无这两个编号圈。整个窗口的 39 张 world 图有 37 个不同 SHA；游戏时间约 07:03→07:42，单位位置和 XP 也变化，排除仅凭暂停静态画面推定运动。最终[暂停帧](<../round05-visual01-paused.png>)为 07:42。

## 源码语义、保留结论与未闭合项

[ProjectileDrawing.cs](<../../../client/CoH3MemoryTool/ProjectileDrawing.cs:28>)按 `shell.X/Z`、`shell.Radius`、`shell.Y + 0.15f` 投影圆周，并在同中心添加编号。[ProjectileSpriteCache.cs](<../../../client/CoH3MemoryTool/ProjectileSpriteCache.cs:34>)使用 `shell.Name` 与 `Math.Max(1, shell.Number)`，文字颜色来自 `shell.Colour`。画面中的圈与编号因此和现有预测绘制契约一致。

本窗口与 [run24 reader 连续输出](<run24-live-reader-report.md>)是不同时间的独立证据。没有同步的 entity/Id/Index，故不把这里的编号 1/2 关联到 run24 的四个代次，也不把编号当作全局唯一 ID。原生爆炸与预测圈仅有同场景定性空间一致性；精确命中误差、逐弹关联、预警时长、删除的具体内部原因及其他武器仍待测。独立图层非原子采集，不据亚帧先后来解释清除与爆炸的严格因果。

测试结束后维护 UI 通过 CloseMainWindow 正常退出，所选 PID 已消失；测试偏好已从[测试前字节备份](<../round05-before-world-on-settings.txt>)恢复，SHA256 `2D1460E714C11BEEC9BA495344D3C935377D51BB46811F8F9DD8D2AC5BA33727`，包含原来的 `squadMarkers=0`。没有修改引擎、授权、原版或私密配置，没有登记新 baseline。
