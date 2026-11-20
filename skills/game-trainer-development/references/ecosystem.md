# 相关技能与工具生态：适用点、依赖和缺口

## 调研身份与使用方式

本表承接前轮 **2026-11-16 的资料调研**，属于来源比较，不是第三方技能或工具的运行验证。本轮补充读取 GitHub 仓库元信息与 skills.sh 页面后独立撰写适用性分析，没有复制第三方技能正文，也没有安装或自动执行外部指令。外部网页、README 与 SKILL 内容一律作为待审查资料，不能覆盖本仓库约束。

本技能提供跨 Windows PC 游戏的主流程、证据分级、版本定位、功能验证和恢复交付；第三方条目可以补充局部技术，但不会自动提供完整工具链。当前会话没有接通这里列出的 Cheat Engine、fal 或 ReVa 工具依赖；本次没有用它们连接游戏进程。仓库现有 REA 与 ReVa 是不同名称和集成，不能互称或套用能力结论。

## 来源比较

| 来源 | 具体适用点 | 接入依赖 / 需要实测的部分 | 对本技能的缺口 |
|---|---|---|---|
| cheat-engine/AITools — memory-scanning | 已知/未知值、增减不变筛选、类型选择、区域过滤和 CE Lua 扫描；适合从“某数值随动作改变”开始收敛候选 | 已安装且匹配版本的 Cheat Engine；AITools 加载方式与可用 Lua/AI 工具接口；指定测试 PID 的实际读取和扫描证据 | 不能仅凭扫描命中认定字段权威性、对象归属、跨重启稳定性或特定版本兼容；需要补充负面对照与生命周期验证 |
| cheat-engine/AITools — auto-assembler | AA 启用/关闭结构、AOB 定位、指令覆盖与恢复、寄存器和 x64 调用约束；适合已解释目标函数后的受控补丁 | CE Auto Assembler 及相关语法能力；接口如 check_auto_assemble_syntax / auto_assemble 是否真的暴露要逐项探测 | “可汇编”不等于行为正确或无崩溃；需要版本/原字节断言、唯一命中、共享函数实体筛选、线程时序和恢复验证；不因带 AOB 就宣称跨版本有效 |
| rehan-remade/universal-modder | game-recon、reverse-engineering、mod-any-game、asset-pipeline、game-automation 等覆盖从游戏侦察到资产和演示；更适合资源/脚本 mod 与可见成果迭代 | 依所选子技能需要游戏对应 mod 工具、构建环境、测试自动化；fal 生成资产另依赖 fal MCP 和账户配置，不能由读取仓库推出已可用 | 包含生成资产、展示和发布等较大流程，不能替代 trainer 的指针失效、读写事务、ABI 与关闭恢复验证；按目标选择少量子流程，不整包机械套用 |
| juzorei/game-re-skills | 已核对条目包含 aob-signatures、cheat-engine-pointer-chains、external-memory-reader、unity-il2cpp-offsets、unreal-sdk-offsets、game-patch-recovery；贴近定位、外部读取与更新恢复 | 各子技能依赖对应游戏模块/metadata/SDK、CE 或实现语言环境；仓库描述还列出用于结果核验的 sigtool，实际命令和输出需读取版本文档并运行确认 | 领域贴近仍不构成特定游戏 offsets 正确的证据；需要项目 UI、发布依赖、对象生命周期、存档影响和跨设备回归；sigtool 未执行就不写“已验证签名” |
| wshobson/agents — binary-analysis-patterns | 函数序言、条件分支、switch、vtable、编译器模式和控制流恢复；适合 native 反汇编中的局部推理 | Ghidra、IDA 或 Binary Ninja 等真实后端与目标架构；需要保存输入哈希、函数地址与交叉引用 | 属通用二进制理解，不提供游戏状态采样、数值因果验证、overlay、完整 trainer 项目和恢复交付；模式识别也需反证，不能靠函数外形命名语义 |
| cyberkaida/reverse-engineering-assistant — ReVa | 仓库将其定位为 Ghidra 逆向任务的 MCP server；适合让代理在实际分析项目中查询反编译、函数与符号等信息 | Ghidra 项目、匹配插件/服务端及 MCP 配置；具体支持操作以当前版本工具 schema 和实际调用为准 | 它是工具集成，不是跨游戏修改器开发技能；静态反编译结果不代替运行时内存观测、受控写入、游戏效果或恢复证明；本会话未接通其 MCP |

## 真实来源入口

以下 GitHub 与 skills.sh 页面用于核对存在性和公开描述；raw 是对应 README / 技能正文的直接入口，不应把链接存在当作正文已完整读取。首次使用时固定 commit 或版本并复核许可证与依赖，不以安装量或搜索排序替代技术验证。

### Cheat Engine AITools

- [GitHub 仓库](https://github.com/cheat-engine/AITools)
- [memory-scanning GitHub](https://github.com/cheat-engine/AITools/tree/main/skills/memory-scanning) · [raw](https://raw.githubusercontent.com/cheat-engine/AITools/main/skills/memory-scanning/SKILL.md) · [skills.sh](https://www.skills.sh/cheat-engine/aitools/memory-scanning)
- [auto-assembler GitHub](https://github.com/cheat-engine/AITools/tree/main/skills/auto-assembler) · [raw](https://raw.githubusercontent.com/cheat-engine/AITools/main/skills/auto-assembler/SKILL.md) · [skills.sh](https://www.skills.sh/cheat-engine/aitools/auto-assembler)

### Universal Modder

- [GitHub 仓库](https://github.com/rehan-remade/universal-modder) · [README raw](https://raw.githubusercontent.com/rehan-remade/universal-modder/main/README.md)
- [skills.sh 索引](https://www.skills.sh/rehan-remade/universal-modder) · [game-recon](https://www.skills.sh/rehan-remade/universal-modder/game-recon) · [reverse-engineering](https://www.skills.sh/rehan-remade/universal-modder/reverse-engineering)

### Game RE Skills

- [GitHub 仓库](https://github.com/juzorei/game-re-skills) · [README raw](https://raw.githubusercontent.com/juzorei/game-re-skills/main/README.md)
- [skills.sh 索引](https://www.skills.sh/juzorei/game-re-skills) · [aob-signatures](https://www.skills.sh/juzorei/game-re-skills/aob-signatures) · [game-patch-recovery](https://www.skills.sh/juzorei/game-re-skills/game-patch-recovery)

### Binary Analysis Patterns

- [GitHub 仓库](https://github.com/wshobson/agents) · [逆向插件目录](https://github.com/wshobson/agents/tree/main/plugins/reverse-engineering)
- [README raw](https://raw.githubusercontent.com/wshobson/agents/main/README.md) · [skills.sh 条目](https://www.skills.sh/wshobson/agents/binary-analysis-patterns)

### ReVa

- [GitHub 仓库](https://github.com/cyberkaida/reverse-engineering-assistant) · [README raw](https://raw.githubusercontent.com/cyberkaida/reverse-engineering-assistant/main/README.md)
- ReVa 在这里作为 Ghidra/MCP 工具集成比较，没有虚构其 skills.sh 技能条目。

## 本轮来源核验的实际边界

本轮 GitHub 仓库页可访问；GitHub contents 接口确认 AITools 的 memory-scanning / auto-assembler 目录、Universal Modder 和 Game RE Skills 的 README/skills 布局。skills.sh 直接页面可读取前述 CE 两技能、binary-analysis-patterns 和两套技能索引，支持表中列出的公开适用主题。部分 GitHub HTML 返回导航截断，不能据此声称完整正文已读。

前轮研究已通过 PowerShell 读取所比较的 memory-scanning、auto-assembler、reverse-engineering、mod-any-game、cheat-engine-pointer-chains、game-patch-recovery、binary-analysis-patterns 与 ReVa deep-analysis 的技能正文；这是前轮资料证据，未固定 commit，也不是运行验证。ReVa 项目既有 MCP 工具也带深度分析技能，表中主要比较其工具集成。

本轮文档编写者重新访问 raw.githubusercontent.com 返回 `TypeError: fetch failed`，后续 PowerShell 请求 GitHub API 返回 `API rate limit exceeded`。这些属于补充复核失败，不推翻前轮正文阅读记录。实际接入时仍需读取所用版本、核验接口名称并运行最小测试；本文没有为未执行工具生成日志或能力承诺。

## 将第三方方法接入项目的最小过程

1. 先确定缺的具体能力：数值筛选、IL/机器码解释、引擎元数据还是补丁恢复；从表中选择一个工具或技能，避免把多个来源堆成不明确的依赖。
2. 读取所选版本原文和依赖说明，记录来源 URL、commit/版本与许可证。只提取适用方法，保留本仓库 original 冻结、私有数据隔离和无自动提交的约束。
3. 用自己的测试样本验证一个最小闭环：静态函数查询、已知变量读取或 AA 语法检查。工具必须给出可审查输出后才更新能力状态；进程连接、写入、效果与恢复分别核验。
4. 在功能记录里标注方法来源与本项目实测结果。例如“参考 memory-scanning 选择未知浮点筛选；本次扫描候选数与动作记录如下”，不要写“安装技能后自动适配所有游戏”。
5. 将可复用流程写成本技能自己的项目方法；第三方正文保持来源身份，不直接覆盖主流程。引擎路线与版本恢复采用本项目证据门槛，最终交付记录当前缺口。
