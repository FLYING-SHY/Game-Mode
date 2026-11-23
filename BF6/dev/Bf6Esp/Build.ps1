<#
.SYNOPSIS
	构建 Bf6Esp 客户端（net48 / x64 / WinExe）并发布到 BF6\output\Bf6Esp。

.DESCRIPTION
	1) 使用本机 .NET SDK（优先 LicensingSystem\.cache\dotnet-10\dotnet.exe，其次 PATH 上的 dotnet）离线构建
	   BF6\dev\Bf6Esp\client\Bf6Esp.csproj；
	2) 把 Bf6Esp.exe（及 .exe.config）、覆盖层设置模板、profiles\*.json 复制到 BF6\output\Bf6Esp\；
	3) 生成 BUILD-INFO.json：构建时间、源文件清单 + 每文件 SHA-256、目标游戏 build 哈希。

	本脚本只写 BF6\dev\Bf6Esp\ 与 BF6\output\Bf6Esp\，不会触碰 BF6\original\ 冻结区。

.EXAMPLE
	& 'F:\WorkSpace\Game\BF6\dev\Bf6Esp\Build.ps1'
#>
[CmdletBinding()]
param(
	[string]$Configuration = 'Release',
	[string]$OutputDir = '',
	[switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

function Write-Section([string]$Text) {
	Write-Host ''
	Write-Host ('=== ' + $Text + ' ===') -ForegroundColor Cyan
}

function Write-Info([string]$Text) {
	Write-Host ('[信息] ' + $Text) -ForegroundColor Gray
}

function Write-Ok([string]$Text) {
	Write-Host ('[成功] ' + $Text) -ForegroundColor Green
}

function Write-Fail([string]$Text) {
	Write-Host ('[失败] ' + $Text) -ForegroundColor Red
}

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$clientDir = Join-Path $root 'client'
$csproj = Join-Path $clientDir 'Bf6Esp.csproj'
$profilesDir = Join-Path $root 'profiles'
$settingsTemplate = Join-Path $clientDir 'overlay-settings.txt'

if ([string]::IsNullOrEmpty($OutputDir)) {
	# <repo>\BF6\dev\Bf6Esp -> <repo>\BF6\output\Bf6Esp
	$gameRoot = Split-Path -Parent (Split-Path -Parent $root)
	$OutputDir = Join-Path $gameRoot 'output\Bf6Esp'
}

Write-Section 'Bf6Esp 构建'
Write-Info ('源码目录: ' + $clientDir)
Write-Info ('输出目录: ' + $OutputDir)

# ---------------------------------------------------------------- 1. 定位 dotnet
$dotnet = $null
$cachedDotnet = 'F:\WorkSpace\Game\LicensingSystem\.cache\dotnet-10\dotnet.exe'
if (Test-Path -LiteralPath $cachedDotnet) {
	$dotnet = $cachedDotnet
	Write-Info ('使用本地 SDK: ' + $dotnet)
}
else {
	$cmd = Get-Command 'dotnet' -ErrorAction SilentlyContinue
	if ($null -ne $cmd) {
		$dotnet = $cmd.Source
		Write-Info ('使用 PATH 上的 dotnet: ' + $dotnet)
	}
	else {
		Write-Fail '未找到 dotnet（既没有 ' + $cachedDotnet + '，PATH 上也没有 dotnet）。'
		exit 1
	}
}

if (-not (Test-Path -LiteralPath $csproj)) {
	Write-Fail ('找不到工程文件: ' + $csproj)
	exit 1
}

# ---------------------------------------------------------------- 2. 构建
if (-not $SkipBuild) {
	Write-Section '编译'
	$buildArgs = @('build', $csproj, '-c', $Configuration, '--nologo', '-v', 'minimal')
	& $dotnet @buildArgs
	$buildExit = $LASTEXITCODE
	if ($buildExit -ne 0) {
		Write-Host ''
		Write-Fail ('编译失败（dotnet build 退出码 ' + $buildExit + '）。')
		exit $buildExit
	}
	Write-Ok '编译成功（0 error）。'
}
else {
	Write-Info '已指定 -SkipBuild，跳过编译步骤。'
}

# ---------------------------------------------------------------- 3. 收集产物
$binDir = Join-Path $clientDir ('bin\' + $Configuration + '\net48')
$exeSource = Join-Path $binDir 'Bf6Esp.exe'
if (-not (Test-Path -LiteralPath $exeSource)) {
	Write-Fail ('编译产物不存在: ' + $exeSource)
	exit 1
}

Write-Section '发布'
if (-not (Test-Path -LiteralPath $OutputDir)) {
	New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Copy-Item -LiteralPath $exeSource -Destination (Join-Path $OutputDir 'Bf6Esp.exe') -Force
Write-Ok '已发布 Bf6Esp.exe'

$configSource = Join-Path $binDir 'Bf6Esp.exe.config'
if (Test-Path -LiteralPath $configSource) {
	Copy-Item -LiteralPath $configSource -Destination (Join-Path $OutputDir 'Bf6Esp.exe.config') -Force
	Write-Ok '已发布 Bf6Esp.exe.config'
}

$settingsTarget = Join-Path $OutputDir 'overlay-settings.txt'
if (Test-Path -LiteralPath $settingsTemplate) {
	if (Test-Path -LiteralPath $settingsTarget) {
		Write-Info '输出目录已有 overlay-settings.txt，保留现有用户设置（未覆盖）。'
	}
	else {
		Copy-Item -LiteralPath $settingsTemplate -Destination $settingsTarget -Force
		Write-Ok '已发布 overlay-settings.txt（默认模板）。'
	}
}
else {
	Write-Info '未找到设置模板 client\overlay-settings.txt，跳过。'
}

$outputProfiles = Join-Path $OutputDir 'profiles'
if (-not (Test-Path -LiteralPath $outputProfiles)) {
	New-Item -ItemType Directory -Path $outputProfiles -Force | Out-Null
}
if (Test-Path -LiteralPath $profilesDir) {
	$profileFiles = @(Get-ChildItem -LiteralPath $profilesDir -Filter '*.json' -File -ErrorAction SilentlyContinue)
	if ($profileFiles.Count -gt 0) {
		foreach ($file in $profileFiles) {
			Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $outputProfiles $file.Name) -Force
		}
		Write-Ok ('已发布 profiles\*.json（' + $profileFiles.Count + ' 个）。')
	}
	else {
		Write-Info 'profiles 目录内没有 .json，未发布档案。'
	}
}
else {
	Write-Info '没有 profiles 目录，未发布档案。'
}

# ---------------------------------------------------------------- 4. BUILD-INFO.json
Write-Section '生成 BUILD-INFO.json'

$sourceFiles = @()
$scanRoots = @(
	@{ Path = $clientDir; Label = 'client' },
	@{ Path = $profilesDir; Label = 'profiles' },
	@{ Path = (Join-Path $root 'docs'); Label = 'docs' }
)
foreach ($scan in $scanRoots) {
	if (-not (Test-Path -LiteralPath $scan.Path)) {
		continue
	}
	$items = @(Get-ChildItem -LiteralPath $scan.Path -Recurse -File | Where-Object {
		$_.FullName -notmatch '\\bin\\' -and $_.FullName -notmatch '\\obj\\'
	})
	foreach ($item in $items) {
		$relative = $item.FullName.Substring($root.Length).TrimStart('\', '/')
		$sourceFiles += [ordered]@{
			path   = $relative.Replace('\', '/')
			size   = [int64]$item.Length
			sha256 = (Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
		}
	}
}
$buildScriptPath = Join-Path $root 'Build.ps1'
if (Test-Path -LiteralPath $buildScriptPath) {
	$sourceFiles += [ordered]@{
		path   = 'Build.ps1'
		size   = [int64](Get-Item -LiteralPath $buildScriptPath).Length
		sha256 = (Get-FileHash -LiteralPath $buildScriptPath -Algorithm SHA256).Hash.ToLowerInvariant()
	}
}

# 目标游戏 build 哈希：从随包档案里取（档案与 build 一一对应）
$gameBuild = [ordered]@{
	exe        = $null
	sha256     = $null
	moduleSize = $null
	profile    = $null
}
if (Test-Path -LiteralPath $profilesDir) {
	$profileFiles = @(Get-ChildItem -LiteralPath $profilesDir -Filter '*.json' -File -ErrorAction SilentlyContinue | Sort-Object Name)
	foreach ($file in $profileFiles) {
		try {
			$parsed = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
			if ($null -ne $parsed.buildSha256) {
				$gameBuild.exe = $parsed.gameExe
				$gameBuild.sha256 = $parsed.buildSha256
				$gameBuild.moduleSize = $parsed.moduleSize
				$gameBuild.profile = $file.Name
				break
			}
		}
		catch {
			Write-Info ('跳过无法解析的档案: ' + $file.Name)
		}
	}
}

$dotnetVersion = ''
try {
	$dotnetVersion = (& $dotnet --version) | Select-Object -First 1
}
catch {
	$dotnetVersion = 'unknown'
}

$buildInfo = [ordered]@{
	product        = 'Bf6Esp'
	tool           = 'Battlefield 6 外部只读 ESP 覆盖层'
	buildUtc       = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
	configuration  = $Configuration
	targetFramework = 'net48'
	platform       = 'x64'
	dotnet         = $dotnet
	dotnetVersion  = [string]$dotnetVersion
	gameBuild      = $gameBuild
	sourceCount    = $sourceFiles.Count
	sources        = $sourceFiles
}

$buildInfoPath = Join-Path $OutputDir 'BUILD-INFO.json'
$json = $buildInfo | ConvertTo-Json -Depth 6
[System.IO.File]::WriteAllText($buildInfoPath, $json, (New-Object System.Text.UTF8Encoding($false)))
Write-Ok ('已生成 BUILD-INFO.json（源文件 ' + $sourceFiles.Count + ' 个）。')

# ---------------------------------------------------------------- 5. 收尾
Write-Host ''
Write-Section '完成'
Get-ChildItem -LiteralPath $OutputDir -Recurse -File | ForEach-Object {
	$rel = $_.FullName.Substring($OutputDir.Length).TrimStart('\', '/')
	Write-Host ('  ' + $rel.PadRight(28) + ' ' + ([int64]$_.Length).ToString().PadLeft(12) + ' bytes')
}
Write-Host ''
if ($null -ne $gameBuild.sha256) {
	Write-Ok ('目标游戏 build: ' + $gameBuild.sha256 + '（档案 ' + $gameBuild.profile + '）')
}
Write-Ok ('交付目录: ' + $OutputDir)
Write-Host ''
exit 0
