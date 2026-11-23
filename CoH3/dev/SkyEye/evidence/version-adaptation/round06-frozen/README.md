# Round06 候选构建及源码有限快照

本目录在生命周期源码后续修改前保存两套当前维护界面的工件和指定源依赖。复制于 2026-11-21 06:42:45–06:42:46 UTC；独立复核完成于 06:44:40 UTC。状态保持 `maintenanceOnly=true`、`buildVerified=false`。

[冻结清单](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/freeze-manifest.json>)逐项记录 363 个去重文件的原绝对路径、仓库相对路径、快照路径、字节数、SHA-256、角色及哈希来源；合计 4,147,010 字节。所有快照保留原仓库相对结构，位于[workspace](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/>)内。

| 快照范围 | 文件数 | 字节数 |
| --- | ---: | ---: |
| [Simple 原输入](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/COH3SimpleTool/>) | 153 | 704,310 |
| [SkyEye 当前源码及资源](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/client/>) | 186 | 1,367,038 |
| [Simple task15 构建工件及定义](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/>) | 10 | 667,985 |
| [SkyEye 构建工件及定义](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/tests/maintenance-ui/>) | 10 | 1,276,862 |
| [指定 KwK、phase、air 探针源码](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/evidence/version-adaptation/>) | 4 | 130,815 |

## 历史绑定与当前源码快照

SkyEye 的历史清单记录了三组原始与生成调用方的 SHA-256，以及 Coordinator 的 SHA-256。快照保留这些绑定，并根据当前项目的 Include/Exclude 规则记录 177 个编译输入、4 个嵌入资源和应用清单的复制时字节。项目派生记录标为 `skyeye:current_project_derived_*`，对应 `build_time_source_binding=false`。 [SkyEye 原构建清单副本](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/tests/maintenance-ui/obj/maintenance-ui-generated/source-manifest.json>)

Simple task15 的[原构建清单副本](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/obj/maintenance-simple-ui-generated/source-manifest.json>)保留全部 153 个 `sourceInputs` 的构建时 SHA，并额外核对三组原始/生成 caller 和 Coordinator。两个构建项目、生成器、离线校验脚本、原报告、EXE、EXE.config 及六个生成副本均在快照内。

| 构建工件 | SHA-256 |
| --- | --- |
| [SkyEye EXE](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/tests/maintenance-ui/bin/Release/net48/SkyEye.Maintenance.UI.Unverified.exe>) | `439DB8B3469464323C0E55CE706166E813817C9F9609ED941E4A8CA33478FEAA` |
| [SkyEye 原报告](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/tests/maintenance-ui/build-report.json>) | `F3B9306A9019E2F5C2C57B02FEE4D6EF8F057BC5CF90EE748D9F0AF1C5E44C8F` |
| [Simple task15 EXE](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/bin/Release/net48/COH3SimpleTool.Maintenance.UI.Unverified.exe>) | `A169466A2CCF6B37354CE04562CE31C89CA7FA4AC9941F299E545B8BC65F6563` |
| [Simple task15 原报告](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/tests/maintenance-simple-ui/task15/build-report.json>) | `0757C53414A6FFD9FF47EE502E8C464D27AEBCC346ED4FB2E92F331D07C21C03` |

两份 EXE.config 及对应原 app.config 的字节 SHA 都是 `603BA077967B612C1E1A9B5F809AE718C5F6ADDF0DFF753BC06CC63B585CD441`。它们是项目明示的公共端点、ProductId 和运行时配置；凭据实例、授权状态及用户运行设置未进入快照。两个名为 LicenseCredentials60 的编译类型仅声明字段，没有 Installation/Code 实例值。

## 历史验证器依赖的保留方式

[KwK 验证器副本](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/evidence/version-adaptation/production/verify_kwk38_sequence.py>)及其 `EXPECTED` 中六个 `.cs` 依赖均按原 size/SHA 保留。其他历史 raw、driver、截图等输入保留原有位置，本快照仅闭合这次指定的源依赖。

[原 phase 探针副本](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/evidence/version-adaptation/lifecycle-phase/phase_probe.py>)、[隔离自测副本](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/workspace/CoH3/dev/SkyEye/evidence/version-adaptation/lifecycle-phase/phase_probe_selftest.py>)及其全部三个 `SOURCE_PINS` 均已保存，包含 air raw 探针。KwK/phase 的源码 pin 记录为历史验证器的精确依赖，不作为 UI 构建时全源绑定。

后续读取历史源码时，以原仓库相对路径 `repo_relative_path` 定位 `snapshot_root / "workspace" / repo_relative_path`，同时核对冻结清单 SHA 和历史验证器原有 expected SHA。旧报告、验证器字节、绝对路径规则及 expected 映射均保持原样；未来 resolver 如选择快照，应继续保留历史 expected 值。

## 独立复核结果

[复核报告](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/freeze-verification.json>)由复制命令完成后的独立读取产生：363 次源文件哈希、363 次复制品哈希，共 726 次，变化/缺失/额外文件均为 0；181 项既有 expected SHA 绑定全部匹配，六个生成副本均等于原文本恰好一次构造替换。复制后 host glob 的 365 项成员集合精确对应 363 个复制文件和最初两份清单；此说明和复核报告随后新增。

| 审计文件 | 字节数 | SHA-256 |
| --- | ---: | --- |
| [freeze-manifest.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/freeze-manifest.json>) | 406,361 | `7E60BA59D5B71DB45FA74536AEB895D5ABE6C4C6A28A103001F7DB7889758900` |
| [freeze-verification.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/freeze-verification.json>) | 3,433 | `4198A7439697C87DA656EAFCA2663A01F07ADC040A81C6AE807D22744092A80E` |
| [preflight.json](<F:/WorkSpace/Game/CoH3/dev/SkyEye/evidence/version-adaptation/round06-frozen/preflight.json>) | 1,839 | `A96A2F34450C7E976822926B1BD382EE64540484877BD61070ABAA7B59B1EEFD` |

文件复制使用新目录独占预留及 `xb` 创建；源文件和旧证据写入次数均为 0。此次仅做静态字节与清单核对，`processQueries=0`，GUI、授权、游戏附加及构建均为 `not_run`。

快照范围不包含 SDK/Framework 二进制、SDK 隐式生成源码、声音、构建包装脚本、运行日志、用户设置、授权私有数据或冻结原版区。保留两份原报告及 EXE 哈希只固定已有证据；候选布局、生产 GUI、游戏功能与正式基线的验收状态沿用原结论。
