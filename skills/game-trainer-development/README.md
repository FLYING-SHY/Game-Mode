# Windows 游戏修改器开发 skill

版本：0.1.0（首轮评审版）。维护源是本目录；对照评估在相邻 `game-trainer-development-workspace/`，不进入安装包。

## 使用

把整个 `game-trainer-development` 目录放到你的 Agent 支持的 skills 目录，或导入交付的 `.skill` 包（ZIP 格式，具体安装方式由宿主决定）。不依赖本仓库盘符。当前会话是否自动发现新技能取决于宿主的重新加载机制；未声称已经全局安装。

可用提示词：

- “使用 game-trainer-development，调查这个游戏目录，先实现一个资源显示功能。”
- “分析这个参考工具的实现，给出定位证据，复现为独立程序。”
- “这张 CE 表重启后失效，验证指针根并整理跨会话测试。”
- “游戏升级后 AOB 两处命中，检查新 build 并修复定位。”

提交游戏路径、目标功能和已有材料即可。技能先检查工具能力与原版保护规则，再执行最小功能切片。

## 内容

- `SKILL.md`：四种入口、阶段产出、证据等级与交付协议。
- `references/skyeye-case-study.md`：原版与源码证据对应，读内存与覆盖层案例。
- `references/engine-routes.md`：native / Mono / IL2CPP / Unreal 的工具与分析分支。
- `references/engineering.md`：会话、指针/AOB、补丁恢复、更新与打包。
- `references/ecosystem.md`：公开技能来源、依赖和覆盖缺口。
- `references/fixture-lab.md` 与 `scripts/fixture_lab.py`：native 风格离线虚拟内存第二案例。
- `assets/investigation-template.md`：开发记录模板。
- `evals/evals.json`：仓库维护用验证任务，标准打包脚本排除 evals。

## 证据边界

天穹之眼提供 .NET/x64 外部读取和覆盖层的静态依据，不是写内存补丁案例。案例中的源码链接只在本仓库布局下可打开；独立安装包保留证据摘录和相对出处，不附带原版或项目源码。第二案例是 Python 创建的离线内存镜像，不是另一款真实游戏。Mono、IL2CPP、Unreal 为研究路线，首版没有宣称全部实际游戏验证通过。

有无 skill 对照各运行一次，共三种任务；其作用是检查遗漏与方法迁移，不构成跨游戏成功率统计。评分以具体结果与日志为证据；评审页面供使用者检查质量与反馈。

## 维护

新增游戏只新增适配资料与该 build 的证据，不把偏移写入通用主流程。修改流程后重跑有关验证，保留旧版对照。源文件 UTF-8，实验 Python 3.10+ 仅使用标准库。第三方技能作为引用资料，未打包其源码或宣称接入其 MCP。
