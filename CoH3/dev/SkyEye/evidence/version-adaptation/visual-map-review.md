# 正常授权维护采样：地图与HUD只读视觉审阅

## 结论与等级

- **当前单一窗口、暂停场景的观察性对照通过**：战术地图边界/中心与native affine相符，HUD本方资源显示与原始采样相符；地图朝向没有出现90°错置或上下/左右颠倒的可见矛盾。
- 小地图native frame包围盒比可见灰色地图边线每侧外扩约2px，记录为皮肤/绘制边界差异，不用肉眼中心估计覆盖该差异。
- Camera矩阵只完成数值不变量与1920×1080宽高比一致性检查；没有独立地形高度/世界坐标锚点，**尚未完成3D点→屏幕的数值校准**。
- 这是已有实际游戏材料的只读观察审阅，不是全功能G/R、跨局/新进程或交付验收。原始维护日志仍为 `FingerprintMismatch`、`buildVerifiedByThisRun=false`、`visualVerification=not_run`、exitCode=5；本报告是单独的人工视觉对照，不回写其状态，不新增hash登记。

## 1. 输入、能力与坐标约定

重新读取工作区 `game-trainer-development` 技能和engineering资料，按“读取成功、视觉一致、完整功能生效分别报告”执行。使用read/read_image查看既有工件，使用现有Python311/Pillow/numpy仅做离线PNG像素与矩阵计算；未接触游戏句柄、输入或引擎源码写入。

| 输入 | 身份/位置 |
|---|---|
| 正常授权采样 | [maintenance-live.txt:1–9](maintenance-run-03-tactical/maintenance-live.txt#L1-L9)，UTC 2026-11-20T17:32:23.217Z起；active真实lease，无synthetic grant |
| 原始JSON | [maintenance-live.json](maintenance-run-03-tactical/maintenance-live.json)，snapshot UTC 17:32:23.2404615Z，非跨feature原子快照 |
| 战术地图截图 | [tactical-open-01.png](tactical-open-01.png)，1936×1119，SHA256 `d0ce24bca0b3d29aff6c1efb190b20f3f55ffe101f327a8ff409b636168042aa` |
| 常规HUD截图 | [controlled-paused.png](controlled-paused.png)，1936×1119，SHA256 `c3941c7588f74c87f7395cf7e058b65523347f86687fbb1d00ea6473d622b082`；画面显示11:28战术暂停 |

日志的RootWidth/Height是1920×1080客户区。按委派给出的border left=8、top=31，本审阅统一使用：

```text
screenshotX = clientX + 8
screenshotY = clientY + 31
clientX = screenshotX - 8
clientY = screenshotY - 31
```

不是桌面绝对坐标；未向负坐标显示器的桌面位置再加偏移。1936×1119与1920×1080的差值为左右各8、上31下8，尺寸自洽。截图与采样为同一暂停现场的不同时间材料；没有合并成同一原子时刻。

## 2. 战术地图：边界、中心、旋转

输入见 [maintenance-live.txt:68–71](maintenance-run-03-tactical/maintenance-live.txt#L68-L71)。

```text
Centre=(960,540), PixelSize≈760×760, WorldSize=384×384
Angle=-90.00001°, NativeTransform=true
u=-1.730242418e-7*x -1.97916650896*z +959.999989429
v=-1.97916650896*x +1.730242418e-7*z +539.999994835
```

原生仿射应用方式与 [MinimapFrame.cs:42–48](../../client/CoH3MemoryTool/MinimapFrame.cs#L42-L48) 一致；本窗口width/RootWidth、height/RootHeight均为1。World的角点取±192，顺序依据 [MinimapFrame.cs:66–74](../../client/CoH3MemoryTool/MinimapFrame.cs#L66-L74)。

| 项目 | affine预测客户区 | 预测截图坐标 | 原PNG观察 |
|---|---:|---:|---|
| 左边 | 580 | 588 | x588灰边RGB=(128,128,128) |
| 右几何边 | 1340 | 1348 | 最后灰边像素x1347；x1348为外侧抗锯齿/阴影 |
| 上边 | 160 | 191 | y191灰边RGB=(128,128,128) |
| 下几何边 | 920 | 951 | 最后灰边像素y950；y951为外侧抗锯齿/阴影 |
| 中心/世界(0,0) | (960,540) | (968,571) | 对称边界的几何中心一致；没有要求地物/图标落在中心 |

按像素占用区间 `[588,1348) × [191,951)` 理解，灰色方框恰为760×760；不要把右/下最后像素和排他几何边的1px差当作坐标错位。浮点角点距离上述整数几何边最大约 `7.41e-5px`，远小于1px栅格化误差。实际可见边线比较以约±1px光栅/抗锯齿容差为限，不推导亚像素视觉精度。

**旋转对照：**截图罗盘N在左侧而非上侧；affine的+worldX沿屏幕上方、+worldZ沿左方，两个主轴长度≈1.9791665px/world-unit且近似正交，对应Angle≈-90°和屏幕Y向下的约定。384×1.9791665≈760，尺寸自洽。此结论只是当前朝向一致；没有旋转动作前后对照，也没有标注已知世界坐标的独立地标，所以不是绝对轴命名或任意旋转的动态验证。

## 3. 常规小地图边界与padding

输入见 [maintenance-live.txt:62–65](maintenance-run-03-tactical/maintenance-live.txt#L62-L65)：Centre=(201,837)，268×268，Angle=-90.00001°，native scale≈0.697916645。

- 预测客户区包围盒 `[67,335) × [703,971)`；截图 `[75,343) × [734,1002)`，中心(209,868)。
- 原PNG灰边像素：左x77、右x340，上y736、下y999，均取到RGB=(128,128,128)。其几何占用区间 `[77,341) × [736,1000)`，264×264。
- 因而可见灰色边线相对native frame每侧内缩2px，中心仍是(209,868)。按照最后像素数值比较时右/下会表现为3px差，来自排他边界与最后像素的约定，而非不对称偏移。
- 当前小地图只能支持中心/朝向/尺度量级与HUD皮肤相符；frame是UI控件变换边界，不必等于最内层绘制线。世界边缘投影到frame边界与可见内线之间有约2px余量，边缘标记裁剪/点击/覆盖层观感仍要另测，不直接改引擎缩放以贴线。

## 4. 资源HUD逐项对照

原始资源见 [maintenance-live.txt:7–9](maintenance-run-03-tactical/maintenance-live.txt#L7-L9)。在常规HUD截图可读：

| 本方字段 | 日志原始值 | 截图显示 | 判断 |
|---|---:|---:|---|
| Manpower | 3487.125 | 3487 | 整数显示一致 |
| Munition | 25 | 25 | 一致 |
| Fuel | 95.28898 | 95 | 整数显示一致 |
| CP | 1 | 右侧1 | 一致 |
| Population | 4 | 4/100 | 当前人口4一致，100上限未由该采样单独读取 |

Manpower/Fuel的小数到整数，本次只能说明显示值相符，未单独判定游戏对任意值使用floor、round还是截断。截图增长率+270、+6，VP492/1000、计时11:28不是该资源快照已验证的字段。敌方资源HUD没有显示，故98.90886/89.49805/233.731262/人口40等保持只读观测，不列为视觉通过。

## 5. Camera：数值一致，不夸大为3D校准

原始矩阵见 [maintenance-live.txt:37–39](maintenance-run-03-tactical/maintenance-live.txt#L37-L39)。把16-float矩阵按现有row-vector约定用于离线计算：

- View旋转3×3 determinant≈1.0000000387，`R*R^T-I`最大绝对误差≈1.12e-7，有限、近似正交。
- Projection `m22/m11≈1.7777778914`，与1920/1080≈1.7777777778一致；竖直FOV≈59.999997°，near≈0.1。
- 反算camera world≈(78.0593,45.14785,-160.0000)。这是矩阵推导值，不是截图独立测量位置。
- 地图白色视口轮廓的朝向、位置与相机在地图一侧向内看的现象不冲突；但要数值比较其四角，必须知道地形高度/相机视口轮廓生成规则。简单假设整个地图ground y=0会给出不同的落点，此假设没有证据，故没有将它作为失败或用拟合高度作为通过证据。
- 尚无已知(x,y,z)地物锚点与同刻屏幕点，因此没有宣称world-to-screen、遮挡、远近裁剪或所有单位标记对齐通过。此次`squad_markers`为sample_absent，亦不证明渲染准确。

## 6. 未测与交接边界

1. 不同分辨率、DPI、窗口模式、全屏、多显示器迁移/负桌面坐标。
2. 旋转、地图缩放、拖移后的native affine与可见边线变化；地图关闭/重开或切局布局恢复。
3. 已知地形高度/世界坐标锚点的3D投影，单位图标逐项位置、边缘裁剪、点击穿透。
4. 战术地图/小地图各层功能开关、移动目标时序、暂停恢复、换局、新进程和保护交付。
5. 正式采样日志的独立人工审核标记若需要维护，应另有显式记录，本次不篡改原JSON的visualVerification/not_run或版本状态。

本次只新增此审阅报告，不改engine、授权配置、游戏状态、构建或hash。
