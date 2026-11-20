# 跨路线第二案例：离线 native64 仿真镜像

本实验使用 Python 标准库生成、读取普通二进制文件和内存中的 bytearray。它是**仿真内存镜像**，没有真实进程读写、进程枚举、附加、系统内存 API 或真实游戏操作。模块开头是 `SIM\0`，并非可执行 PE/ELF。它用于衡量从参考工具分析到 native 风格地址定位的方法迁移；通过结果仅为 F（fixture）级证据，不代表线上功能或实际游戏验证。

使用 Python 3.9+。从仓库根目录执行脚本；将生成目录放到独立临时目录，保持 original、真实游戏、外部进程和密钥完全不参与。脚本仅提供 `create`、`verify`、`self-test` 三个子命令。

## 固定数据集与假设

`create --output DIR` 生成相同内容的 25 个固定文件：`manifest.json`，四个 session 各三份 module 镜像（unique/zero/multi）与三份 heap 帧。重复生成覆盖相同文件名，文件内容、地址和 SHA256 保持确定性，不随机生成，也不清理目录中的其他文件。

- Build A 与 B 分别使用模块基址 `0x140000000` / `0x180000000`，root RVA `0x180` / `0x240`，链偏移 `0x20` / `0x30`，health 偏移 `0x38` / `0x58`。
- 每 build 有两个 session：A0/A1、B0/B1。同 build 模块基址、RVA 与布局稳定，heap 基址与 actor 地址随 session 改变；旧 session 的绝对地址失效。
- 所有指针是 little-endian uint64，health 是 little-endian int32。每帧目标序列为 `[100,73,91]`。两个 decoy 起初同为 100，后续分别为 `[100,100,100]` 和 `[100,91,73]`；只扫描第一帧会得到三个等值候选。
- 链语义：`manager = u64(module_base + root_rva)`；对 `ptr_offsets` 中每个偏移执行 `object = u64(object + offset)`；最终字段地址为 `object + health_offset`。本例只有一级 manager→actor 解引用。每次读取限定单个镜像区域，越界与短读直接失败，不拼接不同区域。

AOB 模式为：

```text
48 8B 81 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9
```

首条 7 字节指令含可变字段 displacement，第二条 7 字节指令含可变 RIP-relative displacement。若命中 RVA 为 `hit`，root 绝对地址是 `module_base + hit + 14 + i32(module[hit+10:hit+14])`；不能把 wildcard 覆盖的字节当成稳定地址。unique 有 1 命中，zero 将首字节改为 `49` 得到 0 命中，multi 在另一 RVA 放第二份匹配并重新编码其 RIP 位移，得到 2 命中。只有唯一命中可接受；0/多命中分别报告失配与歧义，不默认取第一个。

## Manifest ground truth

manifest 是测试答案表，验证器读取它来判断候选，并验证镜像 SHA256。它不是定位结果；从 manifest 复制 root、布局和地址只完成 oracle/接线检查，不构成逆向成功。脚本内 `oracle_candidate()` 仅供 self-test 证明验证器的正常与失败分支。

manifest 顶层字段：

| 字段 | 类型与含义 |
|---|---|
| `schema` | 整数 1 |
| `model` | 固定 `offline-simulated-native64` |
| `pointer_size` / `byte_order` | 8 / `little` |
| `pattern` | 上述 AOB 文本，`??` 表示一个 wildcard 字节 |
| `sessions` | 四个 session 记录 |

每个 session 记录：`id`、`build`（字符串）；`module_base`、`heap_base`、`module_size`、`heap_size`（JSON 整数）；`pointer_root: {rva,address}`；`field_layout: {ptr_offsets:[整数],health_offset:整数}`；`health_address`、`decoy_addresses`（两个地址）；`expected_values`（三帧）；`frames`（按时间排列的三个 `{file,sha256}`）；`aob`（键 unique/zero/multi，每个值为 `{file,sha256}`）。SHA256 为原始文件字节的小写 hex。所有文件名是目录内 basename；加载器拒绝路径分隔符并核对 hash。

## Verify 输入 schema

`verify --directory DIR --result JSON` 的 `--result` 是**候选 JSON 输入文件**，报告输出 stdout。JSON 使用 UTF-8，可带 BOM。所有地址/偏移可用 JSON 整数或 `"0x..."` 字符串；`schema` 必须为整数 1。输入结构：

```json
{
  "schema": 1,
  "candidates": [
    {
      "build": "A",
      "root_rva": "0x180",
      "ptr_offsets": ["0x20"],
      "health_offset": "0x38",
      "sessions": ["A0", "A1"],
      "resolved_addresses": {
        "A0": "0x200000438",
        "A1": "0x2002004b8"
      }
    },
    {
      "build": "B",
      "root_rva": "0x240",
      "ptr_offsets": ["0x30"],
      "health_offset": "0x58",
      "sessions": ["B0", "B1"]
    }
  ]
}
```

顶层必须有 `schema`、`candidates`。每个候选必须有 `build`、`root_rva`、`ptr_offsets`、`health_offset`、`sessions`；`resolved_addresses` 可省略，省略时由验证器重新解析每个 session。若填写其某个 session 项，验证器同时核对该绝对地址，可用于发现缓存旧会话地址的错误。完整通过需要 A/B 各一项，不能重复 build，且每项包含其两个不同 session，不能只用一帧或一个 session。每个 session 的三帧都必须解析为 ground truth 字段地址、匹配变化序列并区别于两条 decoy 序列。

上例是公开 oracle 的协议演示，不应作为 blind 定位成绩。真正的方法迁移测试应先隐藏 manifest：从 module/heap 镜像与受控变化记录中得到候选，记录 AOB 命中、RIP 计算、逐级解引用和排除 decoy 的证据，然后交给持有 manifest 的验证者。

## Verify 输出 schema 与退出码

成功处理后的 stdout 是 JSON：

| 字段 | 内容 |
|---|---|
| `schema` | 1 |
| `model` | `offline-simulated-native64` |
| `passed` | 所有 build 候选、AOB 情形、patch model 检查都通过时为 true |
| `candidates` | 每项含 `build`、`passed`、`sessions`；失败解析另含 `error` 字符串 |
| `candidates[].sessions[]` | `id`、解析出的整数 `address`、三帧 `values`、两条 `decoy_values`、`passed` |
| `aob` | 12 项，每个 session × mode 一项 |
| `aob[]` | `session`、`mode`、整数 `count`、`status`（unique/zero/multiple）、`decoded_roots`（绝对地址数组）、`accepted`、`passed` |
| `patch_model` | 见下文 |

AOB 的 `accepted=false` 与 `passed=true` 可以同时出现：zero/multi 正确检出并停止采用，正是预期行为。这里 AOB 行验证测试素材与唯一性处理，不评分候选作者是否独立实现扫描器；候选评分针对提交的 module-relative root/ptr chain/field。

patch model 在独立 bytearray 模拟 `0f8511000000 → 909090909090` 的 enable/disable，输出布尔 `enabled`、`disabled_restored`、`restore_conflict`、`conflict_preserved`，以及 `original_hex`、`installed_hex`、`conflicting_hex`。正常关闭恢复保存的原字节；第二次启用后模拟第三方将首字节改为 CC，若当前字节与已装补丁不同就停止恢复、记录 conflict 并原样保留现场。验证不改 module/heap 文件，不实际安装补丁，也不模拟线程、页面权限或 trampoline。

退出码：create 完成、verify 全通过、self-test 全通过为 0；验证未通过或预期输入/文件错误为 1。顶层文件/JSON/schema 错误输出 `{ "passed": false, "error": "..." }`，无成功报告的其余字段。候选局部错误保留在候选记录中；不要求错误输入具有完整正常输出结构。

## 可复制执行示例

在仓库根目录 PowerShell 执行。以下仅把 fixture 与候选写入系统临时目录；示例明确使用公开答案做协议接线检查。

```powershell
python .\skills\game-trainer-development\scripts\fixture_lab.py self-test
$lab = Join-Path $env:TEMP 'game-trainer-fixture-lab'
python .\skills\game-trainer-development\scripts\fixture_lab.py create --output $lab
@'
{"schema":1,"candidates":[{"build":"A","root_rva":"0x180","ptr_offsets":["0x20"],"health_offset":"0x38","sessions":["A0","A1"]},{"build":"B","root_rva":"0x240","ptr_offsets":["0x30"],"health_offset":"0x58","sessions":["B0","B1"]}]}
'@ | Set-Content -Encoding UTF8 (Join-Path $lab 'candidate.json')
python .\skills\game-trainer-development\scripts\fixture_lab.py verify --directory $lab --result (Join-Path $lab 'candidate.json')
$LASTEXITCODE
```

self-test 在自动清理的 TemporaryDirectory 中生成固定素材，证明两 build/两 session、排除 decoy、0/1/multiple 与 RIP 解码、末字节边界、短读、区域外读、负长度、旧 session 绝对地址、恢复冲突、正常启停与确定性。它是验证器回归，不是无提示独立解题评测。

## 实测记录

2026-11-16，在工作区通过 `python skills/game-trainer-development/scripts/fixture_lab.py self-test` 执行：退出码 0，`passed: true`，11 项 checks 均为 true（`two_builds_two_sessions_decoys`、`aob_0_1_multiple_rip`、`normal_enable_disable`、`restore_conflict`、`last_byte_boundary`、`short_read`、`outside_image`、`negative_size`、`stale_session`、`decoy_rejected`、`deterministic`）。该记录仅表明离线仿真模型回归通过。
