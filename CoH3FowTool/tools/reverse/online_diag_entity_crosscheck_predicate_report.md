# 374AF68 封闭补充核对

2026-11-08 11:37–11:41，约4分钟（含落盘）；仅原始磁盘反汇编与已存在REA结果交叉读取。catalog仍无相关逆向skill，使用host文件工具、Python/Capstone/pefile；此次未新建REA导入，也未运行游戏、访问游戏进程、发送输入、打补丁或联网。父agent 11:39要求在两个query adapter的直接落点停止，已按该终点收束。

**确定结论：374AF68(P,entity) 是一个分层的实体状态/可见性允许谓词：entity状态位初筛 → 可提前决定true/false的特殊规则 → entity范围查询两类表。** “可见性允许”是根据调用上下文和范围查询所作业务概括；确切原始函数名、特殊flag业务名、两类表分别代表当前可见/已探索等含义仍未知。没有据此推断上传、封禁或处罚。

## 机械语义（原始RVA）

```c
bool predicate(P, entity) {
    flags = u32(entity+0xA0);
    if (flags & 0x04000000) return false; // bit26
    if (!(flags & 0x400)) {              // bit10
        if (!(flags & 8)) return false;  // bit3
        if (u32(entity+0x88)==0) return false;
    }
    effective = fn4FCB00(entity);
    optional = fn374AD7C(entity,effective,P); // 实际带out参数
    if (optional.has_value) return optional.value;

    root = fn183490(entity);
    if (!root) root = entity;
    bounds = fn374AC24(entity+0x150, qword(entity+0x38)); // 四float输出
    result = false;
    component = fn2D4D40(qword(root+0x38));
    if ((component && u8(component+0x45)!=0)
        || fn3B5940(root, address(0x6A97528),0)) {
        fn2496120(qword(P+8), &result, &bounds,1);
        if (result) return true;
    }
    fn2496188(qword(P+8), &result, &bounds,1);
    return result;
}
```

上述名称仅为地址标签；flags在374AF85加载。bit26在374AF91以`0F BA E0 1A`测试，并在374AF95以`0F 82 D0 00 00 00`跳false。bit10是374AF9B的`0F BA E0 0A`；bit3是374AFA1的`A8 08`。注意本函数里bit10置位会跳过bit3与+88测试；但外层374C0B0已单独要求u32(entity+88)!=0。

374AD7C的两个byte输出是可选结果：374AE7D `66 C7 03 00 01`形成{false,true}；374AED7 `66 C7 03 01 01`形成{true,true}；374AED1 `C6 43 01 00`形成has_value=false。374AFD0检查第二字节，非零就从374AFD6返回第一字节。因此它不是单一always-true前置条件。

特殊规则子函数中直接可见的分支：391CF0调用（RCX=P）返回非零会产生{true,true}；entity关联1525D2C与P不同且3B86F0(entity)返回对象+0x96非零会产生{false,true}；4C96A0及3B8480/188700参与另外两处否决；hash=0xE1CFB97362FA57C4的26D480模块查询与2E7870命中会产生{true,true}。这些callee的业务名称和hash对应类名未在本时间窗恢复，不继续展开。

## 两个query adapter及各一层落点

| 调用链 | 原始锚点 | 确定数据流 |
|---|---|---|
| 374AF68 → 2496120 | 374B044 `E8 D7 B0 D4 FE` | 输入qword[P+8]、bool输出、四float范围、count=1；若返回输出true则立即成功 |
| 2496120 → 62E230 | 249617C `E8 AF 80 19 FE` | 先187D00解析TLS arena内world+0x118对象；通过world+0x124的索引映射选表；落点从对象+index*12+0x148取arena偏移 |
| 374AF68 → 2496188 | 374B061 `E8 22 B1 D4 FE` | 同形式参数，提供最终bool输出 |
| 2496188 → 62E150 | 24961E4 `E8 67 7F 19 FE` | 同样索引映射；落点从对象+index*12+0x88取arena偏移 |

62E230在62E260读取+0x148表偏移，62E150在62E180读取+0x88表偏移；均由TLS arena加偏移并64字节对齐形成数据指针。两者用对象byte+7的倒数缩放四个float坐标，各自调用62E310并写bool输出。**确定存在两种表的范围查询；尚未绑定两种表的业务名称。** 62E310仅看到坐标裁剪/分块转换的有限前缀，不在本补充中继续追踪其完整网格链，报告结论也不依赖终端格子位的含义。

374AC24确定构造4float范围：读取输入+0/+4/+8/+C；条件满足时经62E7F0输出范围，退化分支374AD3C–374AD53将x写两次、y写两次，形成点范围。entity+0x150四float的精确原始类型名仍未知。

## 与现有REA的交叉证据

只读了父agent已有两份REA：

- [0x14374bab8.c](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_ac_samples_1/functions/0x14374bab8.c>) 第126–143行：显示对象+0x6A<1且+0x69==0，而374C0B0为false时，push 12-byte {this+558,code=0x800}到this+540/548/550容器。
- [0x14374c0b0.c](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_ac_predicates_1/functions/0x14374c0b0.c>) 第23行首个374AF68调用因mini-PE外部函数类型缺失未显示参数；原始374C0B0汇编明确RCX=P、RDX=entity在374C0D2调用374AF68，不能照伪码空参数误读。

父agent另行确认VisibilityStatusData RTTI vtable3DF1BA0 / TD5F6F8C8 / COL5062D08属于CEEE30统计记录。这是父agent提供的类型证据，本补充没有独立重验，不把它绑定为374AF68返回类型或显示对象类型。

原始字节证据：[online_diag_entity_crosscheck_predicate_asm.txt](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_entity_crosscheck_predicate_asm.txt>)；可重现脚本：[online_diag_entity_crosscheck_predicate_dump.py](<C:/Users/21642/Documents/deepseek-harness/default-workspace/CoH3FowTool/tools/reverse/online_diag_entity_crosscheck_predicate_dump.py>)。地址均为RVA；base0x140000000；.text raw=RVA-0xA00；脚本同时重新记录源exe SHA256。
