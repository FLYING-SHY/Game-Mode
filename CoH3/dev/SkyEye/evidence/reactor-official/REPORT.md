# .NET Reactor 7.7.1.0 工具准备实测

2026-11-13。此报告记录工具准备阶段；后续主代理已生成真实客户端Trial，本代理独立验证详见 evidence/reactor-client-trial/REPORT.md。工具准备阶段范围仅 tools/reactor、evidence/reactor-*、scripts/Reactor-*，未处理业务源码/客户端；后续按主代理指示只创建verification独立bin副本与sandbox测试目录。未安装系统关联、修改 PATH、创建账号、购买或更改任何许可证；安装器未执行。

## 来源与完整性

- 官方下载页：https://www.eziriz.com/reactor_download.htm ：7.7.1.0，12-Nov-2026，Windows 64-bit，41.2 MB，GUI+CLI，Demo/Nag Screen。
- 实际下载：https://www.eziriz.com/download.php?download=dotnet_reactor_setup_7_7_1_0.exe
- 安装器：tools/reactor/dotnet_reactor_setup_7_7_1_0.exe，43,210,992 bytes。
- SHA256：A9243424503486F9992CB2D4E09A35CFB3A7766DF2EBF99FF6519B10323CDCA9。
- Authenticode：Valid，Eziriz e.K.，签名证书指纹 A7BD528F613B78DDDAEA9C3BC0DC16E14DB0F712，DigiCert 时间戳。详见 installer-integrity.json。
- Inno Setup 解包器：jrathlev InnoUnpacker Windows GUI 发布中的 innounp-2.zip；实际 innounp 2.71.3。来源 https://github.com/jrathlev/InnoUnpacker-Windows-GUI/releases/download/ui_2_2_13/innounp-2.zip 。SHA256 F8E45C9676BEBC7D7570108AB572673B541FCF7A7D3D24A04319AC6B5315B4E8，对照 GitHub release asset digest。CLI 是提取的官方原件，未修改。

## 独立目录部署结果

CLI 绝对路径：

```powershell
$cli = 'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2\tools\reactor\extracted\{app}\dotNET_Reactor.Console.exe'
& $cli -help
```

文件版本 7.7.1.0，大小 26648 bytes。必须保留相邻依赖，不建议只复制这个小启动器。提取目录中的 `{app}` 和 `{userdocs}` 均为字面目录名，未读取真实用户目录。`-help` 实际返回0。官方GUI为相邻 dotNET_Reactor.exe，本轮仅验证CLI。

提取过程：

```powershell
& 'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2\tools\reactor\unpacker\innounp.exe' -x -b -y '-dF:\WorkSpace\Game\CoH3\SkyEye-HardenedV2\tools\reactor\extracted' 'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2\tools\reactor\dotnet_reactor_setup_7_7_1_0.exe'
```

## 可执行命令/项目模板

使用 raw 编译产物，禁止叠加 Obfuscar 输出；脚本对明显 obfuscar / reactor trial/production 路径做防误用检查，但它不能凭文件名证明 raw provenance，应由构建链提供干净输入。

```powershell
# 自建无业务 fixture，已实际执行成功；真实客户端时替换为干净 raw DLL 的绝对路径。
& 'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2\scripts\Reactor-Protect.ps1' -RawAssembly 'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2\tools\reactor\probe\bin\Release\net9.0\ReactorProbe.dll' -Mode Trial

# 正式模式强制官方 license guard；当前 Demo 实测101。
& 'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2\scripts\Reactor-Protect.ps1' -RawAssembly 'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2\tools\reactor\probe\bin\Release\net9.0\ReactorProbe.dll' -Mode Production
```

项目模板 tools/reactor/selective-no-rename.nrproj 已经CLI实际加载。CLI覆盖参数如下，参数名以实际7.7.1.0帮助为准：

```text
-project <template.nrproj> -file <raw.dll> -targetfile <independent-trial-dir/raw.dll>
-nodialog -verbose -obfuscation 0 -internalization 0
-declarative_protection 1 -virtualization 1 -virtualization_protect_other_features 0
-control_flow 0 -hide_calls 0 -anti_debug 0 -antitamp 0
-suppressildasm 0 -necrobit 0 -nativeexe 0 -compression 0 -stringencryption 1
```

输出在 tools/reactor/trial/<stamp>，证据在 evidence/reactor-runs/<stamp>，包含CLI参数、版本、退出码、原始输入前后SHA256、输出SHA256及控制台日志。脚本只输出指定主DLL；现有verification需要按其原有依赖部署方式装配runtimeconfig/deps/native依赖，然后运行功能测试。脚本保持rename关闭，便于verification原名引用。

选择性方法Attribute由主代理在最终raw源中加入；本代理未更改客户端：

```csharp
[System.Reflection.Obfuscation(Feature = "virtualization", Exclude = false)]
// Place directly on each selected existing method; preserve its existing signature/body.
```

候选：CoH3MemoryTool.LeaseSignature60.Verify、LicenseReply60.VerifyActive；Gate.Grant仅低频再考虑。Allowed热路径不添加。Attribute只是选方法，全局仍须启用 declarative_protection/virtualization；不提供Attribute时不应把CLI成功解释为目标方法已虚拟化。当前配置还关闭保护器内部helper虚拟化，以减少启动开销。不要加入 -allow_virtualization_warnings 来掩盖被选方法未虚拟化；默认此类问题退出1。

## 实测证据与边界

- 自建 tools/reactor/probe .NET9 classlib，仅 Verify 带virtualization Attribute，Allowed不带；dotnet build成功，0warning/0error。
- Trial实际退出0：evidence/reactor-runs/20261113-143916-874-3e5b6445/run.json 和 cli.log。控制台包含 `Code Virtualization...Done`、`Successfully Protected!` 及 `.NET Reactor Demo Version. The protected file(s) will stop working after 14 days.`。
- 该候选输出：tools/reactor/trial/20261113-143916-874-3e5b6445/ReactorProbe.dll。证明独立目录CLI与模板能处理 .NET9 fixture；未执行生成的fixture、未做反编译/运行时结构核验，尚不证明业务保护覆盖或业务兼容性。
- Production实际退出101：evidence/reactor-runs/20261113-143928-857-2683b96d/run.json，日志 `.NET Reactor is not licensed!`；独立 -licensed 证据 licensed-guard.json/log。PowerShell wrapper因此抛出错误并返回1，底层Reactor原始退出码101保存在JSON。
- 真实客户端待最终raw样本；本轮没有真实客户端保护成功声明。

## Demo限制（官方与运行时）

1. 下载页明确 Demo/Nag Screen。
2. 官方协议 Trial/Demo 条款：处理产物会显示nag和/或自生成日起14天后停止工作；不得向第三方分发处理产物。https://www.eziriz.com/help/license_agreement/index.html ，保存为 license.html。
3. 官方注册说明：未注册处理的应用/库显示 `This software is protected by an unregistered version of .NET Reactor`；部署受保护软件需要注册。https://www.eziriz.com/help/ordering/why_register/index.html ，保存为 why-register.html。
4. 实际CLI进一步明确14天后停止工作；未测试nag弹框的展示时机。
5. `-nodialog` 仅抑制保护过程的进度窗口，不移除受保护产物的Demo限制；Production使用 -licensed fail closed。未进行许可证绕过。

## 官方文档保存

- https://www.eziriz.com/help/command_line_parameters/index.html → cli.html，实际帮助cli-help-utf8.txt。
- https://www.eziriz.com/help/definitions/declarative_protection/index.html → declarative.html。
- https://www.eziriz.com/help/definitions/code_virtualization/index.html → virtualization.html。
- https://www.eziriz.com/help/howto/single_file_publishing/index.html → single-file.html。
- 安装包SDK原始项目转码示例 sdk-project-utf8.xml，仅用于核对XML键；它包含官方样例license锁，因此实际模板未复制这些锁或其样例密钥。

Integrity-linked virtualization需Anti Tampering与普通磁盘程序集，且更贵；本模板使用标准selective virtualization，未启用这类变体。最终单文件发布应按官方步骤先保护托管程序集再进入后续发布链，并以最终交付文件另做验证。
