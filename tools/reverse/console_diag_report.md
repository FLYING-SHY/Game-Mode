# CoH3 build26443338 静态诊断结论

六个分支位于本地 UI 可见性查询和 FOW 纹理更新读者。它们控制是否读取本地 UI 揭示开关；检查到的函数体没有写入玩家 sim 揭示数组。父 agent 已分别反馈菜单控制台打开状态和六分支局部显示方案的实测成功；本子任务自身只读原始磁盘映像，没有访问游戏进程、发送输入、修改进程或原始 exe，也没有编辑父 agent 的工具源码。

本报告区分静态推导与父 agent 的运行反馈。所有地址均为十六进制 RVA；加载地址为 B，实际 VA=B+RVA。原始 image base 为 0x140000000。.text 内 raw offset=RVA-0xA00；.rdata 的换算不同。

## 映像及方法

```text
source: G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe
size: 108388536
sha256: 21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca
.text: RVA=0x1000 raw=0x600 size=0x3D52C00
REA CLI: F:\BaseEnv\nodejs\rea.cmd v6.8.0
Ghidra: 12.1.4
JDK: 25.0.3
```

技能目录已检查：本会话仅 Office、SEO、Windows ACL 技能，没有适用的逆向技能。使用了用户指定的 REA/Ghidra 和新建的磁盘静态分析脚本。新产物均以 console_diag_ 命名并位于限定目录。

## 六处分支审阅

| RVA | 原 raw offset | 原字节 | 当前读者及分支意义 |
|---|---|---|---|
| 0x1874E8 | 0x186AE8 | 74 09 | UI 可见性查询；1884A0 返回 false 时跳过全局 UI_A0 检查 |
| 0x187EF0 | 0x1874F0 | 74 09 | UI 对象 +0xF8 的 reveal-all getter；逐帧纹理更新使用此 getter |
| 0x187F42 | 0x187542 | 74 0D | UI 可见性查询；1884A0 返回 false 时跳过 UI_A0 |
| 0x1882E3 | 0x1878E3 | 74 0D | UI 可见性查询；1884A0 返回 false 时跳过 UI_A0；后续 UI_A2 分支未改变 |
| 0x189E26 | 0x189426 | 74 09 | UI 可见性查询；1884A0 返回 false 时跳过 UI_A0；后续 UI_A2 分支未改变 |
| 0x1E9701 | 0x1E8D01 | 74 0D | 实体可见性缓存到本地显示对象的 mask 更新；1884A0 返回 false 时跳过 UI_A1 |

父 agent 测试的替换都是 `90 90`。本子任务仅记录描述符，没有应用替换。NOP 后，上下文函数仍然执行，只是这六个读者继续检查 UI 开关；开关为 0 时仍走各自正常查询路径。这里适合使用“本地 UI 显示/可见性查询与纹理更新”的表述。其余四个查询函数可能还有其他本地 UI 消费者，本审阅没有穷尽所有调用方的交互副作用。

两个 UI 开关：

```text
UI object = B+0x6A887A8
UI_A0 / reveal-all = byte[B+0x6A888A0] = byte[UI object+0xF8]
UI_A1 / reveal entities = byte[B+0x6A888A1]
```

建议产品文案：**放开本地 UI 显示开关的上下文限制，并由引擎持续更新地形迷雾和实体显示。** 当前证据不支持将其描述为改变 sim 视野、AI 视野或玩家探索状态。

## 逐帧覆盖问题的完整链

`0xC964A0` 是包含 `FOWTextureUpdate` / `FOWTextureUpdateRevealExplored` 任务的更新函数：

```text
C964E3: call 187EB0(UI object)             ; explored getter
C964F2: call 187EE0(UI object)             ; all getter
C964F7: movzx ebx,al
C964FA: movzx edx,al
C96504: E8 47 01 00 00 -> C96650(manager, al)
```

未处理上下文分支时，`187EE0` 返回 `1884A0() && UI_A0`。所以即使 UI_A0 保持 1，对局中的上下文返回 0 仍会在下一次更新中提交 0，覆盖 manager+0x41 与 D+0x5E9。父 agent 先前反馈的“暂停有效、恢复运行后很快复原”与此控制流吻合。

`C96650` 用传入 DL 产生渲染任务，并执行：

```text
C96685: 40 86 7B 41      xchg byte [rbx+0x41],dil
```

因此 manager+0x41 是所请求状态的镜像值，最终值就是 DL，并非始终写 1 的 refresh 标志。

目标数据与任务：

```text
manager = u64(B+0x6B02CC0)
root = u64(B+0x6185F18)
D = u64(root+0x50)
payload = { D:u64, enabled:u8 }

1045700: 48 8B 11 0F B6 41 08 88 82 E9 05 00 00 C3
         D = u64(payload)
         byte[D+0x5E9] = byte[payload+8]
```

队列不可用的锁内后备路径 `0x290F155` 也写 D+0x5E9。`C96610` 是平行的 fog-enabled 设置：manager+0x40 与 D+0x5E8。D+0x5E8/+0x5E9 存在打包 word 读者，如 `0x15715B1`、`0x60FEB8`，所以仅搜索单字节 +0x5E9 读者会漏掉消费链。

完整 REA 控制流保留了三个分支：普通纹理更新、reveal-all 时关闭 fog-enabled 更新、explored 的专用纹理任务。未见该更新函数派发 PCMD83 或调用玩家 sim 揭示数组 setter。

## 上下文许可判断与实体公式

1884A0 的 bool 结果只需看 AL；REA 返回类型推导为 ulonglong，返回 false 时上位寄存器可能保留旧值，不应把整个 RAX 当 bool。

```text
O = u32(B+0x5E0BECC)
Q = u64(B+0x6B02CF0)
if O == 0xFFFFFFFF or Q == 0: return false
V = u64(Q)
Info = u64(V+0x2B20)
kind = u32(Info+0x2F0)
modeFlags = byte[V+0x0D] != 0 || byte[V+0x04] != 0

A = TLS arena selected through GS:[0x58] -> TLS[0]+0x40 -> table B+0x6185E00
L = 2DE560(A+O)  ; via 18C920, arena-offset object resolution
return (L && byte[L+0x680] != 0)
    || (L && byte[L+0x691] in {5,7,8})
    || kind != 1
    || modeFlags
```

上述字段没有被赋予未经确认的游戏语义名称。真实函数在 Q 非空后直接读取 Info；静态观测工具应自行校验其指针，而不是把额外空指针检查误写成原始游戏逻辑。

实体函数 `0x1E96E0` 从缓存记录读取 `flags = u32(cache+0x48)`。原始逻辑：

```c
revealWanted = context1884A0() && UI_A1;
visible = (flags & 4) != 0 && ((flags & 0x10) != 0 || revealWanted);
setterArg3 = ((flags & 2) != 0 || visible) ? 0 : 1;
call1EA840(displayEntity, 2, setterArg3);
```

`1EA840` 更新本地显示对象 +0x69 的 mask，并在状态变化时触发后续显示通知。特别校正先前推测：缓存 bit2 会使 setterArg3=0；它并非此前猜测的“强制隐藏”。bit4、缓存命中、显示对象解析仍是其他条件，UI_A1=1 不等同于对任意无效/未创建对象强制生成模型。

## 新 layout 的结构校验

D 与 manager 在检查到的构造路径均没有虚表赋值，不应以 `[D]` 或 `[manager]` 进行虚表匹配。

D 的精确构造证据：

```text
4B32B3: B9 48 06 00 00                  allocate size 0x648
4B32D0: call memory-zero routine         length 0x648
4B32DB: E8 70 EA FF FF                  call 4B1D50
4B32E5: 48 89 46 50                     publish D at root+0x50
4B1D6E: 48 89 29                        zero first qword, not a vtable
4B1F73: 66 C7 86 E8 05 00 00 01 00      word[D+0x5E8]=1
```

manager 的精确构造证据：

```text
128F86E: B9 48 00 00 00                allocate size 0x48
128F87D: 48 89 05 3C 34 87 05           publish at B+0x6B02CC0
128F884: 48 C7 40 30 20 00 00 00        qword[manager+0x30]=32
128F88C: 48 C7 40 38 20 00 00 00        qword[manager+0x38]=32
128F894: 66 C7 40 40 01 00              bytes manager+0x40/+0x41=1/0
```

+0x30/+0x38 是尺寸参数；`4983BE` / `4983C8` 比较输入尺寸，失配会走重建路径。32 是初值，不宜作为整个会话都固定的类型签名。

最佳现有校验组合：匹配本 build 的磁盘/规范化代码身份和关键指令；检查 manager 至少可读 0x48、D 至少可读 0x648；四个状态字节均为 0 或 1；前后重读 root、root+0x50 与 manager global，确认仍是同一快照；恢复状态绑定 PID/对象生命周期。此组合是结构校验，不是 RTTI 身份证明，也没有提供跨线程原子快照保证。

## 原始控制台任务结论

菜单中的 Global 控制台无需 -dev。静态初始化 `4BD530` 无本地 dev 判断，读取 consoleKey 并创建/注册 Console。父 agent 在 no-dev PID18552 的菜单中反馈热键与关闭可逆成功；对局中该键曾无效，本子任务未完成 Global 对局输入拦截原因定位，并按最新指示停止追踪。

SCAR 子模式有独立 dev flag 限制：

```text
B40B10: 80 B8 40 01 00 00 00      cmp byte [app+0x140],0
B40B17: 0F 84 1D 01 00 00         je skip SCAR registration
SCAR tag 0x53434152, consoleKey_SCAR, fallback Control+Shift+2
Global tag 0x474C5541, consoleKey, fallback Control+Shift+1
```

Global 菜单可用这一结论仅覆盖已观察场景；SCAR 的注册分支仍受上述 dev flag 控制。App.ini 配置本身也不证明运行可用。

可供只读观察的 Console 状态：

```text
M = u64(B+0x6BA4430)
begin = u64(M+0)
end   = u64(M+8)
for each 8-byte element in [begin,end):
    C = u64(element)
    choose object with u64(C) == B+0x3E3A5C8
opened = byte[C+0x10]
drawing = byte[C+0x11]
animation = float[C+0x18]       // 0 .. 0.5
```

直接 getter `14F6680` 原字节 `0F B6 41 10 C3`，属于 Console 虚表 +0x28。类型 getter `14F9170` 原字节 `B8 4C 53 4E 43 C3`，返回 CNSL=0x434E534C。绘制 thunk `9E94D0` 原字节 `48 83 C1 08 E9 07 00 00 00` 证明 renderer 内嵌在 C+8。真正的打开 functor `14E0550` 同时写 renderer+8/+9=1，因此打开意图 C+0x10 和绘制活动 C+0x11 应分开观察；关闭动画结束前后两者可能短暂不同。RenderInterface+8 是帧内 Begin/End 标记，不是持久打开位。

父 agent 反馈的运行校验值：B=0x7FF751670000，C=0x2479AA9D130，opened/drawing=true，animation=0.5；此地址仅是当时对象实例，重启后应重新定位。

## cheat_revealall 链与未满足条件

PCMD83 sim handler `3437BF8` 保留三重判断：

```text
3437C04: 3B 42 24                    u32(cmd+8) == u32(P+0x24)
3437C07: 0F 85 B5 00 00 00           fail -> return
3437C0D: 80 7A 48 FF                 byte[P+0x48] == 0xFF
3437C11: 0F 85 AB 00 00 00           fail -> return
3437C17: 80 7A 60 00                 byte[P+0x60] != 0
3437C1B: 0F 84 A1 00 00 00           fail -> return
```

P 是 dispatch 的 `1943A0(handle)` 返回的 arena 对象，不是传给 handler 的临时栈块。本静态子任务没有读取这些运行字段，所以没有确认哪个运行条件当前失败。

通过后，`3437C45` 打日志，`3437C6B` 调 `34342F4` 派发 GE_PlayerCheat=0x580000；UI subscriber `390C688` 是通知路径，StatLogging subscriber `39FADBC` 记录作弊统计，二者不承担最终 sim FOW 状态写入。

真正状态写位于后续 `33A1C68`：先更新 CRC，然后用 `6EC2E0(playerId*)` 求玩家索引，并在 `33A1CA6` 执行 `C6 44 38 6C 01`，即 `byte[module+0x6C+index]=1`。重复调用在该字节上是幂等置 1，不是 toggle；CRC 仍可能继续变化。

其上游 `26D480(hash=0x5FF018D0C7448E70)` 还要求 BasicFogOfWarModule 存在、启用。模块虚表 RVA=0x3E964C0，hash 位于 module+8，enable getter `14FDCF0` 原字节 `0F B6 41 68 C3`，即 module+0x68 非零。若模块查找返回空，日志和事件可以成功而实际数组不写。玩家索引是否有效也是运行状态问题。UI_A0/A1 路径与这条 sim 数组路径是两套机制。

## 有效证据产物及限制

机器可读地址/原始字节/REA evidence id：

- [console_diag_manifest.json](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/console_diag_manifest.json>)
- [console_diag_final_review.txt](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/console_diag_final_review.txt>)
- [console_diag_structure_shape.txt](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/console_diag_structure_shape.txt>)

已复核的 REA/Ghidra 输出：

- [console_diag_tick_rw_decompiled.c](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/console_diag_tick_rw_decompiled.c>) — 完整逐帧更新，包含冷分支。evidence `ev_06b7774bb36f36c57d9af98883ef2b3255aa5aa8521b6e1ec9a436eb603619ea`。
- [console_diag_entity_rw_decompiled.c](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/console_diag_entity_rw_decompiled.c>) — 实体缓存与显示 mask。evidence `ev_af2945758d0b957e75dcf762564561e8397b9a2a3dfbf25e3f7c8b0f13323c44`。
- [console_diag_context_rw_decompiled.c](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/console_diag_context_rw_decompiled.c>) — 1884A0 的实际条件。evidence `ev_c1abcdb3748aade46d2f697717ef405c28743ecae491110346d08c1953eedc09`。
- [console_diag_simcase_decompiled.c](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/console_diag_simcase_decompiled.c>) — PCMD83 三个条件。
- [console_diag_foweffect_decompiled.c](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/console_diag_foweffect_decompiled.c>) — CRC 后的真实揭示字节写入。

REA 使用独立 mini-PE，保留原始 RVA，不是在完整原始映像上构建的全程序数据库。外部函数、类型和部分导入并未完整重建；关键 bool、字段、分支与写入均以原始汇编复核。最终 *_rw 样本保留原始节的可写属性并映射所需 BSS 页，避免把运行时可变全局错误当常量。

早期非 rw 的 context 反编译曾将初值为 FFFFFFFF 的全局错误常量折叠为 false；早期 entity-gate JSON 是未识别函数地址的错误 envelope；两者已被本节的 *_rw 证据替代。命令 exit code 0 并不保证 REA JSON 是成功 envelope。早期仅热区的 tickgate 反编译也已由含冷分支版本取代。

父 agent 在六处分支测试中反馈敌军建筑/步兵模型、红图标与 minimap 图标出现。持续运行、关/开循环、关卡切换和旧补丁回滚由父 agent 执行，本报告不把该后续测试当作已完成。

## 生产代码只读审阅

审阅快照：2026-11-08 09:58 左右，父 agent 新版代码；主代码没有由本子任务编辑。以下结论按该快照记录，父 agent 后续修复需以更新后的文件为准。

通过静态核对的部分：

- [FowDisplay.cs:30](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/src/FowDisplay.cs#L30>) 的 manager 读取 0x48、D 读取 0x648、UI flags 长度 2、manager+0x40/0x41 与 D+0x5E8/0x5E9 的布尔检查均匹配构造和消费证据；三个指针前后检查与当前对象发布路径一致。
- [PatchCatalog.cs:179](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/src/PatchCatalog.cs#L179>) 六个分支及预期原字节与本报告清单一致；BuildLegacy 只用于识别/还原旧五处。
- [RuntimeState.cs:20](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/src/RuntimeState.cs#L20>) 使用新六处+旧五处归一后校验原始摘要；[Program.cs:195](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/src/Program.cs#L195>) 核对运行进程 exe 路径。
- [FowDisplay.cs:71](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/src/FowDisplay.cs#L71>) 单批代码写入有预检、逐项回读、异常倒序回滚；正常关闭按当前命令含义写四个显示状态为 0，revert 再恢复代码。

**P1：字段状态失败会阻止 revert 进行代码恢复。**

[Program.cs:267](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/src/Program.cs#L267>) 先调用 SetFlags(false)、Wait(false)，到第 272 行才调用 SetCode(...,false)。若字段读取、写入或等待因对局切换等抛出异常，外层 catch 直接返回 1，十一处代码恢复均未执行。第 263-264 行也只把 InvalidOperationException 当作无 display；ReadProcessMemory 的 Win32Exception 会直接跳到外层 catch。

建议把固定 UI flags 清理、仍属于同一对象的动态字段清理、代码恢复拆成分别尝试的步骤，最后聚合失败。即使 display 消失，也应尝试已经校验通过的代码还原，且回读原始字节后再报告完成。

**P2：reveal 的命令级失败没有恢复此前成功的六处分支；对象换代时字段回滚又会跳过固定 UI flags。**

[Program.cs:226](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/src/Program.cs#L226>) 的顺序是还原旧五处、开启新六处、SetFlags、Wait。SetFlags/Wait 失败后，第 239-242 行仅打印并返回 1，前面成功的六处 NOP 会保留。

[FowDisplay.cs:128](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/src/FowDisplay.cs#L128>) 在 manager/display 改变时会跳过全部字段回滚，包括两个固定地址的 UI 开关。例如前两次写入已令 UI_A0/A1=1，第三次检查发生对象换代，就会留下一组已经变化的全局开关；跳过回滚本身没有单独的部分状态报告。

建议保留命令开始前的新六处代码状态，在后续失败时恢复这一批；旧五处仍保持已还原。固定 UI flags 与依赖对象身份的字段分别处理；动态字段只在相同对象时回退。若对象已消失，应明确报告哪些固定/代码状态已恢复、哪些对象字段被跳过，而非宣称整个操作已回滚。

这些是异常恢复路径问题。正常 Read 布局、off/on 设置目标与四值等待未发现明显偏移或 bool 极性错误。这里没有执行故障注入或运行新命令；持续与回滚实测仍由父 agent 完成。
