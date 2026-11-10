# 10E08C0 / EFB3E0 内联状态查询：分支极性补充

2026-11-08 12:16，2分钟内封闭补充。沿用已匹配host Python/Capstone工具，无适用逆向skill，无新REA导入。仅磁盘读取，新增render_diag_reader_*。

源exe SHA256=21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca；base0x140000000；地址均RVA，.text raw=RVA-0xA00。完整连续对齐字节窗口10E08C0–10E0C0F见同目录render_diag_reader_dump_10E08C0_350.txt，内含source路径、hash、每条指令raw和hex。起点10E08C0确为.pdata函数起点；单条.pdata短范围并非逻辑控制流边界，本次显式保留后续分支落点。

## 指针相等路径的精确原始指令

```text
RVA       raw       bytes                         instruction
10E08C0   10DFEC0   40 55                         push rbp
10E08C2   10DFEC2   56                            push rsi
10E08C3   10DFEC3   57                            push rdi
10E08C4   10DFEC4   48 8d 6c 24 b9                lea rbp,[rsp-47h]
10E08C9   10DFEC9   48 81 ec a0 00 00 00          sub rsp,A0h
10E08D0   10DFED0   48 8b 05 e9 06 d1 04          mov rax,qword ptr [rip+4D106E9h]
10E08D7   10DFED7   48 33 c4                      xor rax,rsp
10E08DA   10DFEDA   48 89 45 27                   mov [rbp+27h],rax
10E08DE   10DFEDE   48 8b 72 08                   mov rsi,[rdx+8]
10E08E2   10DFEE2   48 8b f9                      mov rdi,rcx
10E08E5   10DFEE5   48 8b 06                      mov rax,[rsi]
10E08E8   10DFEE8   4c 8b 40 50                   mov r8,[rax+50h]
10E08EC   10DFEEC   48 8d 05 ed aa e1 ff          lea rax,[rip-1E5513h] ; RVA EFB3E0
10E08F3   10DFEF3   4c 3b c0                      cmp r8,rax
10E08F6   10DFEF6   0f 85 22 02 00 00             jne 10E0B1E
10E08FC   10DFEFC   48 8b 86 e0 00 00 00          mov rax,[rsi+E0h]
10E0903   10DFF03   66 83 78 6a 00                cmp word ptr [rax+6Ah],0
10E0908   10DFF08   0f 8f b5 02 00 00             jg 10E0BC3
10E090E   10DFF0E   80 78 69 00                   cmp byte ptr [rax+69h],0
10E0912   10DFF12   0f 85 ab 02 00 00             jne 10E0BC3
10E0918   10DFF18   b0 01                         mov al,1
10E091A   10DFF1A   84 c0                         test al,al
10E091C   10DFF1C   0f 84 d5 01 00 00             je 10E0AF7
10E0922   10DFF22   0f 57 c0                      xorps xmm0,xmm0
10E0925   10DFF25   0f 2f 47 28                   comiss xmm0,[rdi+28h]
10E0929   10DFF29   0f 83 ac 02 00 00             jae 10E0BDB

10E0BC3   10E01C3   32 c0                         xor al,al
10E0BC5   10E01C5   e9 50 fd ff ff                jmp 10E091A

10E0AF7   10E00F7   48 8b 4d 27                   mov rcx,[rbp+27h]
10E0AFB   10E00FB   48 33 cc                      xor rcx,rsp
10E0AFE   10E00FE   e8 dd 9b 1d 01                call 22BA6E0
10E0B03   10E0103   48 81 c4 a0 00 00 00          add rsp,A0h
10E0B0A   10E010A   5f                            pop rdi
10E0B0B   10E010B   5e                            pop rsi
10E0B0C   10E010C   5d                            pop rbp
10E0B0D   10E010D   c3                            ret
```

## 准确极性

令Q=qword[第二参数+8]（进入RSI），D=qword[Q+E0]；C=signed16[D+6A]，M=byte[D+69]，H=(C>0 || M!=0)。

当Q的vtable+50函数指针等于EFB3E0：

- C>0，或C<=0而M!=0：跳10E0BC3计算AL=0，再10E091A→10E091C跳10E0AF7清理退出。
- C<=0且M==0：10E0918计算AL=1；10E091C不跳，落入10E0922，继续检查第一参数对象+28/+38/+3C/+40等额外条件。

因此内联查询值是**AL=!H**。它不是“AL=true就退出”或“AL=H”。`mov al,1`与`xor al,al`计算的是被内联getter的逻辑返回值；不是10E08C0当场执行RET，也不足以把10E08C0整个函数标成返回bool的API。

指针不等时从10E08F6跳10E0B1E，存在另一实现的去虚拟化/冷块路径。本补充只解释要求的EFB3E0相等分支，不把不等路径合并为同一字段公式。

已证实的角色是GameDcaContext slot+50状态查询及调用方筛选。未由此证明直接draw、render提交或某个确定剔除阶段，也未证明这里的D与父agent另一条+5E8来源是同一对象。
