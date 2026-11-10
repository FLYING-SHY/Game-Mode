# 5DAAD0 返回对象与显示路径交叉核对

核对时间：2026-11-08 11:23–11:29（约5分半，有界收束）。只读原始磁盘映像、已有静态证据及 REA mini-PE。未执行游戏、访问游戏进程、补丁、输入或网络匹配。新增产物仅为 tools/reverse/online_diag_entity_crosscheck*。

结论：**是，AC 与 2.0 显示路径都以同一 5DAAD0 resolver 返回指针作为这些字段的对象基址。显示路径把该指针无调整地传给 1EA840，后者直接写 +0x69；AC 直接读返回指针的 +0x69/+0x6A。** 这是直接调用与数据流证据，强于仅见相同偏移的结构推测。静态证据保证相同 registry、相同 handle、相同映射状态时落到同一个槽位；两条路径在某一帧的具体实例地址相等未做运行验证。

## 原始字节与直接调用

源：G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe；size=108388536；本次重新计算 SHA256=21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca；base=0x140000000；下列地址均为 RVA，.text raw=RVA-0xA00。

| 路径 | 关键 RVA | 原始行为 |
|---|---|---|
| AC CEEE30 | CEEF27 | call1943A0 解析 sim entity；此处不是5DAAD0调用 |
| AC CEEE30 | CEEF2C/CEEF30/CEEF37/CEEF3E | rcx=*(this+8)；rdx=*(entity+0x88)；rcx=*(rcx+0xA8)；rcx=*rcx |
| AC CEEE30 | CEEF41 | E8 8A BB 8E FF → call5DAAD0 |
| AC CEEE30 | CEEF4F/CEEF54 | cmp word[rax+0x6A],0；signed jg |
| AC CEEE30 | CEEF56/CEEF5A | cmp byte[rax+0x69],0；jne |
| 显示 1E96E0 | 1E979B/1E979F/1E97A6/1E97AD | 同样从this+8、+0xA8首指针与entity+0x88组装两个参数 |
| 显示 1E96E0 | 1E97B0 | E8 1B 13 3F 00 → call5DAAD0 |
| 显示 1E96E0 | 1E97BA/1E97BE/1E97C0/1E97C3 | r8d=bl；dl=2；rcx=rax；call1EA840，没有指针增减/重新解析 |
| setter 1EA840 | 1EA846/1EA854 | 从同一this读取word+0x6A与byte+0x69 |
| setter 1EA840 | 1EA874 | 41 88 41 69 → mov byte[r9+0x69],al；r9保留入口rcx |

按汇编准确表示，两路径的 registry 参数为 `read64(read64(read64(this+8)+0xA8))`；参数2是 sim entity+0x88 的64位值。两个 this 分别属于两条路径，静态布局相同不自动证明它们运行时this+8相等。

5DAAD0 的主体也已查：5DAB16 对注册表+0x28的32位ID表做等值校验；5DAB1B–5DAB28从+0x10表取32位映射槽；5DAB39/5DAB40从registry+0x80指针数组取对象；5DAB62 `mov rax,rsi` 返回对象，无wrapper构造或基址调整。参数是带索引/身份校验的handle候选，不宜把整个entity+0x88值直接称为裸对象指针。共享resolver主体有一个跳到27A0A46的空槽冷分支，本次对象同一性结论不依赖该分支语义。

## +0x69 / +0x6A 的证据限度

确定的机械语义：令 `M=byte[obj+0x69]`、`C=signed int16[obj+0x6A]`、`E=(C>0 || M!=0)`。

- 1EA840 对调用方实际传入的bool 0/1执行：`M' = enabled ? (M|mask) : (M & ~mask)`。显示路径的mask是2（bit1）。因此+0x69是按原因位组合的mask，不是单一bool，也不是本片段中的sim玩家FOW数组。
- 1EA840 在E前后发生变化时，才于1EA899调用467100(obj,newE)。AC 在CEEF4F/CEEF56检查完全相同的两项条件；满足任意一项时跳过后续+0x60/+0x58位图读者。
- 466DD0 是另一条直接字段setter：466DD4/466DDB读取旧C/M；466DE4写word[obj+0x6A]=DX；466DFF在E变化时调用467100。该字段为16位且用有符号>0比较，不能仅凭word载入命名为无符号entity ID。
- 467100 在46722C/467230取本对象word+0x6A并加1；传入DL为0时4672FD把子节点值改为0；467304–46730A遍历[obj+0x18,obj+0x20)的指针列表，对每个指针调用466DD0(child,parentC+1或0)。支持+0x6A具有层级传播/计数性质的解释。

业务命名的推断：+0x69可描述为“显示抑制/隐藏原因mask候选”，+0x6A可描述为“继承的显示抑制层级/计数候选”。显示路径正常的可见性公式令mask2在不可见分支置位，且合并状态通知到显示组件，支持此方向。确切原始字段名、mask其他各bit定义、计数所有增减来源、完整可见性意义均未恢复。不要把+0x69直接称为“作弊flag”或把+0x6A称为“检测次数”；该调用片段只证明AC消费这些显示状态。

## REA 与 RTTI

REA实际使用：F:\BaseEnv\nodejs\rea.cmd 6.8.0；Ghidra12.1.4。对已有console_diag_entity_rw.exe执行decompile 0x1401E96E0，复用保留原始RVA的mini-PE；没有重新导入108MB全映像。新evidence_id=`ev_7e33dbb21459112f452c5b84ad12dc24c715f583d6f6c1f391f611b0e74364f3`，normalized_result是实际伪码，明确输出5DAAD0返回值传1EA840。该证据与既有ev_af2945758d0b957e75dcf762564561e8397b9a2a3dfbf25e3f7c8b0f13323c44及原始指令一致。本次还用pefile逐字节比较mini-PE与原exe：1E96E0长度0x107、冷分支26C6DE2长度0x15均相等。REA mini-PE未恢复完整类型及外部函数体，类型结论不来自反编译器命名。

RTTI具体查过原始映像的Entity相关TypeDescriptor候选，并对两个窄候选解析COL/vftable：

| 名称 | TypeDescriptor RVA | COL RVA | vftable RVA |
|---|---|---|---|
| .?AVBufferedEntity@@ | 5F6DED0 | 5063550 | 3D75FB0 |
| .?AVEntity@@ | 5F71338 | 5062678 / 50626A0 | 3D90090 / 3D90120 |

这些名称在原始映像中存在，但本次没有把5DAAD0返回槽绑定到其中某个vftable，也没有沿构造/发布链完成类型归属。RTTI不能作为已确认类名的证据；不把BufferedEntity或Entity当作最终命名。两路径共享对象布局由直接数据流证明，未借助上述候选名称。

## 证据文件

- [online_diag_entity_crosscheck_asm.txt](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_entity_crosscheck_asm.txt>)：AC、显示调用链、resolver、两个字段setter与通知传播的原始字节/RVA/raw反汇编。
- [online_diag_entity_crosscheck_rea.json](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_entity_crosscheck_rea.json>)：本次实际REA返回的Evidence envelope。
- [online_diag_entity_crosscheck_rea_decompiled.c](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_entity_crosscheck_rea_decompiled.c>)：本次REA normalized_result。
- [online_diag_entity_crosscheck_rtti.txt](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_entity_crosscheck_rtti.txt>)：上述两个候选的RTTI定位输出，仅作为已查内容，不冒充返回类型绑定。
- [online_diag_entity_crosscheck_manifest.json](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_entity_crosscheck_manifest.json>)：原始映像摘要、section表、关键RVA与类型候选清单。
- [online_diag_entity_crosscheck.py](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_entity_crosscheck.py>)：可重现磁盘反汇编/摘要脚本。

终点：对象桥接问题已用直接调用闭合；完整检测链、类型全图及任何绕过实现均未扩展。
