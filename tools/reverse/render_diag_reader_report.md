# display +69/+6A 读取者与表现链：封闭静态核对

2026-11-08 12:03–12:11，约8分钟（含落盘；末次RTTI字节导出12:11:40）。当前catalog无适用逆向skill；使用host文件工具、Python/Capstone/pefile/numpy。此次无新REA导入。仅原始磁盘读取，仅新增render_diag_reader_*；无游戏进程访问、游戏输入、补丁、网络或匹配操作。

源文件：G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe。每份输出独立记录SHA256=21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca；base=0x140000000；下列地址均为RVA，证据包含raw与原始字节。

**结论：确定有读取两字段的纯getter，以及Mesh/FX/Splat独立组件状态通道；本时间窗未证明有能直接提交完整实体绘制、且完全独立于显示对象状态的入口。** “单独状态函数不调用1EA840/不写display mask”与“已经具备独立draw入口”是不同证据强度。

## 1. 纯getter与直接调用者

令C=signed16[obj+0x6A]，M=byte[obj+0x69]，H=(C>0 || M!=0)。

- **1F4980–1F4992，19字节叶函数，返回H。** 原始字节：`66 83 79 6a 00 7f 09 80 79 69 00 75 03 32 c0 c3 b0 01 c3`。只读取两个字段，无call、无对象写入、无栈帧。
- **EFB3E0–EFB3F9，26字节叶wrapper，obj=[this+0xE0]，返回!H。** 原始`48 8b 81 e0 00 00 00`后读取+6A/+69。原始RTTI将此函数绑定到GameDcaContext vftable3D905D8的slot+0x50：指针槽3D90628 → COL5039408 → TD5E65280 → `.?AVGameDcaContext@@`，COL signature=1、selfRVA匹配、offset=0。
- 全.text直接E8/E9枚举对1F4980只发现**472AA8 `E8 D3 1E D8 FF`**；对EFB3E0无直接E8/E9调用。后者有上述vtable引用，不能据此宣称无人调用。10E08C0的已反汇编前缀在10E08EC比较slot+50与EFB3E0，10E08FC起内联同一逻辑。

472AA8所在函数.pdata范围471EC0–472C0B。关键数据流：472A7C call4DBD40返回对象 → r9=rax → 472AA5 rcx=r9 → 472AA8 getter。H=true时472AB1令xmm8=公共float常量，并经16A7AE0/28A170产生另一个float候选；结果在472B70写局部记录+0x38，并在472B79取目标vtable+0x20，472B90去虚拟化调用C845F0或472BA3间接调用。相似记录+2C/+30/+34也通过同slot更新。**这已证明getter影响表现参数更新；未证明该slot是GPU draw submission或实体剔除入口。** 不把该调用者命名为已确认通用renderer。

## 2. 467100对具体组件的通知绑定

已有上游证据：1EA840修改M后，H变化调用467100；466DD0修改C后，H变化同样调用467100。此次原始确认467100从[obj+30,obj+38)遍历16字节条目，组件=entry+8，取vtable+0x68，在467186传DL=H、46718D call rax。以下为原始绝对函数指针、合法COL/TD与类名字符串交叉绑定：

| slot+68函数 | 类 | vftable / COL / TD |
|---|---|---|
| D1EAD0 | MeshComponent | 3D883D8 / 5070868 / 5F338C0 |
| D1EAD0 | StaticMeshComponent | 3D884D8 / 5070728 / 5F33A50 |
| D1EAD0 | SkinnedMeshComponent | 3DED560 / 5070750 / 5F33A20 |
| D1EAD0 | MeshCollectionComponent | 3E096A0 / 5070890 / 5F338E8 |
| 1174D90 | FXControllerComponent | 3E15A78 / 50709F8 / 5F331F8 |
| 1376760 | SplatComponent | 3DE64C8 / 502A0C0 / 604A5B8 |

**Mesh通道：** D1EAEA写component+E9 bit0=H；发生变化时经vtable+E8通知，特定实现包含D1EB2B→60D1D0；component+E8==1、+E9 bit0==0、bit2==1、component+C0==0、display+6C bit2等条件决定D1EB71→60AF80，已有component+C0则D1EB79→60AEB0。60AEB0有限前缀遍历component+D0/D8条目、调用60B3E0，并把component+C0加入管理对象双缓冲队列（60AF29 call60CFE0、60AF35写队列项）。这支持资源/表现管理候选；本次没有追到最终图形API提交。60D1D0当前看到25E320/25F350空间数据更新，不能简单称它draw函数。

**FX通道：** 46731D是1174D90的去虚拟化分支：DL=!H，RCX=component+C0，46732B call1174DA0。原函数1174D90同样先xor dl,1再this+=C0并尾调。1174DA0写内部对象+A4=visible，再R8B=!visible、RDX=内部对象+90尾调4AAAE0。4AAAE0解析RDX句柄（4AAAED call4AD2B0），4AAAF7 `80 A0 1D 01 00 00 FE`与4AAAFE `08 98 1D 01 00 00`只更新解析对象+11D bit0。**这些短函数不读写display+69/+6A、不调用1EA840；确定存在单独组件状态入口，最终draw行为仍未知。**

**Splat通道：** 1376760在1376774写component+DC=(H && byte[component+140]==0)，检查component+C0及+140后可能跳冷块2A83988。冷块未展开，未证明独立提交。

## 3. FX准备链的另一个直接字段消费者

4A9690从[component+8]取得display，4A96BC/4A96C4读取C/M，将!H作为第5个byte参数在4A96E1传入4A95D0。后者4A9668取该参数作为R8B，4A9674 call4ABB80；4ABCB2写内部对象+A4，再4ABCC0 call4ABD90，4ABE33根据+A4产生!visible并在4ABE43 call4AAAE0，汇合到上述FX状态位。

4A9810在4A983A安装vftable3D7CED0；原始COL5038DA0 / TD6018548，类名`.?AVFXMarkerUpdate@?A0x701f61a5@@`。因此4A9690/4A95D0保守定位为FX准备链，不泛称通用实体render提交。该链普通路径仍读取display可见状态。

## 时间边界内Mesh原始补齐

`render_diag_reader_dump_D1EAD0_B0.txt`保存D1EAD0完整0xB0字节、直接callee 60AEB0完整0xC8、60AF80主.pdata范围完整0x457、60D1D0完整0x4D（冷块跳转不再展开）。**参数DL=H，即hidden/suppressed极性，非visible。** D1EAEA写component+E9 bit0=H；H==0才可能满足创建路径，此外还要求+E8==1、+E9 bit2、display+6C bit2等条件。

一层内确定写入：60AEB0在60AF42清component+C0，60AF6F清component+104，并在60AEDB清条目+8；60AF80在60B0B9写component+C0=新建候选对象，60B0D3写条目+8=已保存关联值，分支60B27D清component+1A1、60B3CB清component+105。60B09F/60B278/60B373调用同一管理函数510330；其语义与最终GPU提交本次未展开。60D1D0通过25E320/25F350处理component+90/+EC空间数据。

**471EC0中1F4980调用与上述Mesh路径是否属于同一场景图对象链，未完成对象同一性证明，明确未知。** 也未基于任意+5E8位移匹配认定同一对象。原始getter、组件RTTI与通知数据流分别记录，不相互代替对象同一性证据。

## 证据终点与未知项

- 已证实：两种getter、一个直接getter调用者、Mesh/FX/Splat通知的RTTI类型、FX独立组件状态位入口。
- 未证实：完全不依赖显示对象mask/层级的实体模型draw入口；GPU draw submission；常规culling函数确切名称；候选函数全调用图与所有副作用；对象生命周期/线程约束；最终视觉效果。这里只能支撑架构判断。
- 搜索是特定编码的word+6A/byte+69近邻筛选，随后人工核对原始指令；不是全部编译形式或全部读取者的穷举。E8/E9枚举不覆盖虚调用、函数指针、内联和其他section。
- 首个472A30窗口从指令中间开始，开头伪反汇编不采用；最终D1EAD0证据文件另含从472A20开始的对齐窗口。

## 文件

- `render_diag_reader_probe.py`：重现scan、xref、dump、ptr、type五种只读原始PE查询。
- `render_diag_reader_scan.txt`：两字段近邻扫描与getter初始字节。
- `render_diag_reader_xref.txt`：getter、467100、4A9690的直接E8/E9引用。
- `render_diag_reader_ptr_1F4980.txt`：GameDcaContext的slot+50绑定。
- `render_diag_reader_ptr_1174D90.txt`：Mesh/FX/Splat的slot+68与COL/TD绑定。
- `render_diag_reader_type_3D7CED0.txt`：FXMarkerUpdate类型绑定。
- `render_diag_reader_dump.txt`：467100组件遍历、通知/层级传播以及1F4710空间范围更新。
- `render_diag_reader_dump_472A30_1D0.txt`：FX准备链与getter调用者初始窗口，忽略开头不对齐字节。
- `render_diag_reader_dump_1174D90_150.txt`：FX短setter、4AAAE0、Splat与FX汇合链。
- `render_diag_reader_dump_D1EAD0_1A0.txt`：Mesh回调/管理前缀、472A20对齐调用者、两getter完整原始字节。

全部文件目录：C:\Users\21642\Documents\deepseek-harness\default-workspace\CoH3FowTool\tools\reverse\。
