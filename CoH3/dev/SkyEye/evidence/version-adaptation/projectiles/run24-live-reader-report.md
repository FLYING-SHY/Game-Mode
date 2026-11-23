# Run24：真实 reader 的 Wespe 连续观测

状态：通过本次 retained reader sequence 复核；全功能/画面预测验收仍待完成。未提升基线。

## 真实采样

- 游戏：PID31572 / first fresh process，离线 US 对 German AI。窗口527128，client1920×1080；输入操作逐次确认游戏前台，finally恢复战术暂停。
- 维护 runner：真实签名租约 `license_state=active`；实际 C# Snapshot / ReadProjectileImpacts（敌方来源），以及独立 all-sides ReadAirZones 诊断。没有 Python raw 回放替代 C# 执行。
- 50秒事件窗：459 polls / 459 projectile queries / 459 air queries；elapsed50006ms、overrun6ms；start间隔中位109ms、最大112ms。deadline正常终止。
- 128次炮弹输出、128个非零帧、4个不同(entity,Id,Index)；331次零输出；0 dropped frames / 0 dropped items。air输出0，仍为sample_absent。runner总体exit5，其他未采样功能继续待验证。
- 固定MatchKey `31572:639308258698492330:2188107579392:2188149555200:1000`，epoch1；全部输出帧与之相符。

## 代次、预测点与后续空输出

四个对象都属于同一来源 `0x3b9aea1500000c4e`、Slot1、名字“黄”，武器 `weapon\german\explosive_weapon\heavy_artillery\105mm_wespe_ger`，预测Radius8。复用registry Index1712但entity与Id变化：

| entity | Id | Number | 次数 | 事件窗起止ms | 下一次全零poll ms | 预测X/Y/Z |
|---|---|---|---|---|---|---|
| 0x1fd7441b800 | 1000008446 | 1 | 33 | 34699–38197 | 38306 | -99.56421 / 24.5662422 / 73.38151 |
| 0x1fd7441d400 | 1000008447 | 2 | 34 | 38744–42346 | 42455 | -81.78934 / 24.5663681 / 81.11048 |
| 0x1fd744c7000 | 1000008450 | 3 | 33 | 42894–46383 | 46495 | -92.84718 / 24.5558052 / 80.76666 |
| 0x1fd7441e200 | 1000008451 | 4 | 28 | 47041–49991 | 窗口结束前尚无 | -92.7956543 / 24.2330666 / 54.9527664 |

- 每个对象的预测点在本次留存观测中稳定，前三个之后存在全零poll，第四个到窗口结束仍被读出。
- 这是产品预测点、编号与generation复用/清除行为的证据；坐标不是弹体当前飞行位置，reader缺失也不直接等于物理爆炸时刻。尚无同步屏幕预测圈或游戏实际命中对照。
- 游戏从06:05恢复，到采样后暂停截图显示07:02；原生“炮位遭到攻击”提示保留。暂停前后PNG不单独证明每枚炮弹命中。

## 可复核工件

- [真实 runner JSON](<../maintenance-run-24-artillery-50s/maintenance-live.json>)、[文本](<../maintenance-run-24-artillery-50s/maintenance-live.txt>)。
- [序列复核脚本](<verify_live_event_window.py>)、[结果](<run24-reader-sequence-validation.json>)：3个fixture自检通过；真实结果 `PASS_retained_reader_sequence_only`、`gameFeatureAccepted=false`。JSON记录输入SHA256 `2e4f02e991e2d3763d5cfe7d3cac05ad7575b6ff05bddad074117339bf6c6243`。
- [恢复前游戏帧](<../round05-artillery-window-resume.png>)、[暂停后游戏帧](<../round05-artillery-window-paused.png>)。

## 同轮空袭raw负样本

[raw快照01](<../air/live-raw-01.json>)在07:02暂停后实际只读：4928 attempts、486501bytes、0errors、完整扫描且根与描述符稳定。两玩家技能74/54项，registry3156项；没有catalogued P47/recon技能或飞机。该瞬时负样本与尚未证实P47激活一致，不扩展为整个事件窗缺失结论。技能列表主要为player/battlegroup项；entity空袭可能由飞机恢复路径采集，仍待原生成功施放和实际C#输出。

原始/授权私密数据与ProductId保持原样，游戏仅原生输入，无进程写入；本报告只登记实际证据。
