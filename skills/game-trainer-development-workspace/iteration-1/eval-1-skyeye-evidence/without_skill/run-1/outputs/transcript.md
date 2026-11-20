# case1 / without_skill / run-1 实际执行简记

本记录按执行顺序整理实际工具与结果，不记录推测 token、总耗时或工具数量。命令工作目录均为仓库根 `F:\WorkSpace\Game`。所有落盘产物位于本次 outputs；没有执行样本、游戏、构建、测试或服务，也没有修改原项目。

## 输入与 skill

1. 查看会话提供的 available_skills 目录。调用 `skill("skill-creator")`，因本任务是既有 skill 的独立基线评估；不展开该 skill 的参考文件、不运行评分/聚合/查看器步骤，它们属于父评估任务，且本次明确禁止读取 assertions 和其他运行输出。
2. `read F:\WorkSpace\Game\AGENTS.md` 成功，确认 original 冻结、dev/source 与 output 布局和静态/动态边界。
3. 为避免显示 assertions，未用 read 返回整个评估 JSON，首次运行：

```powershell
$m = Get-Content -LiteralPath 'F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-1-skyeye-evidence\eval_metadata.json' -Raw | ConvertFrom-Json
$m.prompt
```

结果失败：本机默认编码把中文误解码，`ConvertFrom-Json` 返回 `传入的对象无效，应为“:”或“}”。 (438)`。错误输出显示了原 JSON 的损坏文本；未对 assertions 作检查或使用。

4. 修正读取编码后成功：

```powershell
$m = Get-Content -LiteralPath 'F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-1-skyeye-evidence\eval_metadata.json' -Encoding UTF8 -Raw | ConvertFrom-Json
$m.prompt
```

只请求/输出 `prompt`，任务要求只读分析原版和当前客户端，至少检查五个指定文件，不运行样本/游戏、不改项目、不访问授权私有数据，并给出精确出处及验证限制。没有访问或评估 assertions 字段。

5. 调用 `skill("karpathy-guidelines")` 用于代码审查约束：明确假设、保持原项目零改动、基于可复核证据得出结论。没有读取新建的 game-trainer-development/SKILL.md 或任何该 skill references，没有读取 with_skill 输出。

## 发现、读取与静态检索

- `glob` 在游戏树搜索 `**/AGENTS.md`：无子目录规则文件。
- `glob` 在 SkyEye 查五个指定文件：全部找到；列举原版目录文件确认 EXE、配置、说明、声音、缓存和诊断日志。
- 用 `read` 读取维护源码全部 Native、GameVersion45；PlayerInfoReader 的 1..250、1080..1201、1540..1774、1840..2014、2290..2449、2700..2809 行；SquadMarkerWindow 的 1..260、285..665 行；全部 MineCamera、OverlayDiagnostics55；OverlaySettingsForm 的 2180..2194 行。未把未读的 PlayerInfoReader 全部内容声称为人工逐行审计。
- 用 `read` 读取完整既有 protection-validation 报告、原版先看这里、软件使用说明、SHA256SUMS；原版 overlay-diagnostics 仅读前 35 行。
- `grep` 当前客户端 C#，实际模式之一：`WriteProcessMemory|VirtualProtectEx|CreateRemoteThread|OpenProcess\(|VirtualAllocEx|DllImport|InstallPatch|patch`。返回 P/Invoke、OpenProcess 命中；未返回常见远程写入/注入/补丁 API。进一步检索定位函数、image 偏移、GameVersion45、VirtualQuery、MatchKey 等，按命中精读上下文。
- 另一次 `grep` 为 `GameVersion45.Unverified|File\.(Write|Copy|Move|Delete)|Process\.Start`。发现本地设置、日志、缓存等写入和更新链接启动入口；没有读取 LicenseStore 所存的任何真实凭据或私有目录。
- 当前会话未暴露 REA 专用 MCP/skill；`glob` 仓库已有 ILSpy 工具目录，找到 ilspycmd 9.1.0.7988。未下载或安装工具。

## 原版只读类型导出

调用 `pwsh` 执行下列命令；运行的是 ILSpy 工具，传入原版 EXE 作为解析数据，没有启动原版 EXE。ILSpy 打印四次更新版本提示，未执行更新。

```powershell
$outDir = 'F:\WorkSpace\Game\skills\game-trainer-development-workspace\iteration-1\eval-1-skyeye-evidence\without_skill\run-1\outputs'
New-Item -ItemType Directory -Force -Path "$outDir\original-static" | Out-Null
$env:DOTNET_ROOT = 'F:\WorkSpace\Game\LicensingSystem\.cache\dotnet-10'
$env:DOTNET_ROLL_FORWARD = 'Major'
$toolPath = 'F:\WorkSpace\Game\CoH3\dev\SkyEye\tools\ilspy\ilspycmd.exe'
$samplePath = 'F:\WorkSpace\Game\CoH3\original\天穹之眼-v0.60.8\天穹之眼-v0.60.8.exe'
Get-FileHash -LiteralPath $samplePath -Algorithm SHA256
foreach ($typeName in @('Native','PlayerInfoReader','SquadMarkerWindow','GameVersion45')) {
    & $toolPath -t "CoH3MemoryTool.$typeName" $samplePath -o "$outDir\original-static"
    if ($LASTEXITCODE -ne 0) { throw "ilspycmd failed for $typeName with exit $LASTEXITCODE" }
}
Get-ChildItem -LiteralPath "$outDir\original-static" | Select-Object Name,Length
```

成功：四个导出均未触发非零退出检查。实际文件大小为 GameVersion45 1373、Native 1688、PlayerInfoReader 101482、SquadMarkerWindow 17087 字节。样本 SHA256 为 `F89B09C2BFF2751C5E7904D3E302B6966ABB7C8CC16D56A98BE09582AAD4E2D0`，与原清单匹配。

随后 `read` 导出 Native 和 GameVersion45 全文，并 `grep` 四个导出类型的读取权限、定位偏移、Gate、窗口样式和绘图入口；命中均与维护源码相同。

## 四类比较与原版哈希复核

实际比较命令核心为：

```powershell
$sourceDir = 'F:\WorkSpace\Game\CoH3\dev\SkyEye\client\CoH3MemoryTool'
$rows = foreach ($typeName in @('Native','PlayerInfoReader','SquadMarkerWindow','GameVersion45')) {
    $sourcePath = Join-Path $sourceDir "$typeName.cs"
    $exportPath = Join-Path "$outDir\original-static" "CoH3MemoryTool.$typeName.decompiled.cs"
    $sourceText = [IO.File]::ReadAllText($sourcePath).Replace("`r`n","`n").TrimEnd()
    $exportText = [IO.File]::ReadAllText($exportPath).Replace("`r`n","`n").TrimEnd()
    [pscustomobject]@{
        type=$typeName
        normalized_text_equal=($sourceText -ceq $exportText)
        source_sha256=(Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLower()
        export_sha256=(Get-FileHash -LiteralPath $exportPath -Algorithm SHA256).Hash.ToLower()
    }
}
$rows | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath "$outDir\static-comparison.json" -Encoding UTF8
$rows | Format-Table type,normalized_text_equal
Get-FileHash -LiteralPath $samplePath -Algorithm SHA256
```

结果四个 `normalized_text_equal` 都是 True；再次读取样本 SHA256 与首次一致。这里只证明原版 EXE 哈希前后相同，没有声称全 original 树逐文件前后校验。

向父 agent 发出中间结果，说明四类证据从维护源码推断升级为原版直接静态核对，但不代表整程序完全相同。

## 文档产出与收尾

- `write` 创建本次 [report.md](<F:/WorkSpace/Game/skills/game-trainer-development-workspace/iteration-1/eval-1-skyeye-evidence/without_skill/run-1/outputs/report.md>)，含定位链、只读与补丁证据、覆盖层、迁移执行步骤、保护报告边界和实际失败。
- 调用 `skill("discernment-nudge")`；用户已明确要求验证、精确出处和限制，按该 skill 自身例外不再附加反问。
- `write` 创建本记录；最终列举 outputs 并解析比较 JSON 核对交付。最终结果通过 `send_message` 发给父 agent。

没有后台作业，没有授权范围扩张、提交或推送。没有可用的完成通知 token/耗时数据，未编造相关测量。
