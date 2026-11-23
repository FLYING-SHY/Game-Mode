[CmdletBinding()]
param([string]$RepoRoot = 'F:\WorkSpace\Game')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path.TrimEnd('\')
$scope = $PSScriptRoot
$project = Join-Path $scope 'MatchHudLive.csproj'
$engine = Join-Path $RepoRoot 'CoH3\dev\SkyEye\client\CoH3MemoryTool'
$readerPath = Join-Path $engine 'PlayerInfoReader.cs'
$layoutPath = Join-Path $engine 'GameLayout52426.cs'
$helperPath = Join-Path $engine 'MatchHudContext.cs'
$rawPath = Join-Path $RepoRoot 'CoH3\dev\SkyEye\evidence\version-adaptation\lifecycle-phase\v4\live-round3\pid30504-second-running-ref-paused-001.json'
$modulePin = '8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec'
$frozen = @{
    'PlayerInfoReader.cs' = '8AE3E8BF3C3D9E90924D71799EBEB39E07256DB05F8D44073BB89F9A17C48545'
    'MatchHudContext.cs' = '1BC671A1641B6BA2D46F8F91B11D5790DB8E346D55D2262070F06EBF607D7878'
    'MarkerReadContext.cs' = 'C10952282B89964316796525F331E1E0519642F8A443C5BDC24D042C518C3D29'
}
function Sha([string]$path) { (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() }
function HexNumber([string]$value) { [Convert]::ToInt64($value.Replace('0x', ''), 16) }
function CsString([string]$value) { '@"' + $value.Replace('"', '""') + '"' }
foreach ($tree in @($engine, (Join-Path $RepoRoot 'CoH3\dev\COH3SimpleTool\CoH3MemoryTool'))) {
    foreach ($name in $frozen.Keys) {
        if ((Sha (Join-Path $tree $name)) -ne $frozen[$name].ToLowerInvariant()) { throw "Frozen production pin changed: $tree\$name" }
    }
}
$reader = [IO.File]::ReadAllText($readerPath)
$layout = [IO.File]::ReadAllText($layoutPath)
if ($reader -notmatch 'if \(detectedBuild == GameVersion45\.GameBuild\.Build52426\)\s*\{\s*return true;' -or
    $reader -notmatch 'private long BuildRva45\(long legacy\)\s*\{\s*if \(UsesCandidateLayout\(\)\) return GameLayout52426\.Resolve\(legacy\);\s*return legacy;\s*\}') { throw 'Reader static selector differs from reviewed 52426 route' }
$guardCall = 'new MatchHudGuard(Read, image + BuildRva45(112837064), image + BuildRva45(64823200), image + BuildRva45(65713904), image)'
if (-not $reader.Contains($guardCall) -or -not $layout.Contains($modulePin)) { throw 'Reviewed helper role call or layout fingerprint changed' }
$roles = [ordered]@{}
foreach ($entry in @(@('RootSlotRva',112837064), @('RootTypeRva',64823200), @('HudTypeRva',65713904))) {
    $matches = [regex]::Matches($layout, '\{\s*' + $entry[1] + '\s*,\s*(0x[0-9a-fA-F]+)\s*\}')
    if ($matches.Count -ne 1) { throw "Role missing/ambiguous in current layout: $($entry[0])" }
    $roles[$entry[0]] = HexNumber $matches[0].Groups[1].Value
}
$rawHash = Sha $rawPath
$raw = [IO.File]::ReadAllText($rawPath) | ConvertFrom-Json
if ($raw.pid -ne 30504 -or $raw.expectedStartUtc -ne '2026-11-22T01:23:35.3993114Z' -or $raw.module.sha256 -ne $modulePin) { throw 'Historical raw process/module pin differs' }
$rawBase = HexNumber $raw.module.base
if ((HexNumber $raw.roles.uiSingleton.candidateRva) -ne $roles.RootSlotRva -or
    (HexNumber $raw.roles.rootVtable.candidateRva) -ne $roles.RootTypeRva -or
    (HexNumber $raw.roles.hudVtable.candidateRva) -ne $roles.HudTypeRva) { throw 'Static selector roles disagree with raw role table' }
$expectedRoot = $rawBase + $roles.RootTypeRva
$expectedHud = $rawBase + $roles.HudTypeRva
$rawRoot = HexNumber $raw.samples[0].hud.rootVtable
$hudProofs = @($raw.samples[0].hud.proofs | Where-Object { $_.kind -eq 'hud' -and $_.initial.PSObject.Properties['controlVtable'] -and (HexNumber $_.initial.controlVtable) -eq $expectedHud })
if ($rawRoot -ne $expectedRoot -or $hudProofs.Count -ne 1) { throw 'Raw module base + selector type arithmetic did not match one fresh HUD and current root' }
# Historical control pointers are deliberately not emitted into the executable.
$raw = $null

$buildId = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + ([guid]::NewGuid().ToString('N').Substring(0,8))
$buildDir = Join-Path $scope ('builds\' + $buildId)
if (Test-Path -LiteralPath $buildDir) { throw 'Build output already exists' }
New-Item -ItemType Directory -Path $buildDir | Out-Null
$generated = Join-Path $buildDir 'SourcePins.g.cs'
$paths = @($readerPath,$layoutPath,$helperPath,(Join-Path $engine 'MarkerReadContext.cs'),$project,(Join-Path $scope 'Program.cs'),$PSCommandPath,(Join-Path $scope 'README.md'),$rawPath)
foreach ($name in $frozen.Keys) { $paths += Join-Path $RepoRoot ('CoH3\dev\COH3SimpleTool\CoH3MemoryTool\' + $name) }
$pins = @($paths | ForEach-Object { [ordered]@{ path = $_; sha256 = Sha $_; bytes = (Get-Item -LiteralPath $_).Length } })
$totalSourceBytes = 0L
foreach ($sourcePin in $pins) { $totalSourceBytes += $sourcePin['bytes'] }
if ($totalSourceBytes -gt 32MB) { throw 'Source 32MiB cap' }
$pinLines = @($pins | ForEach-Object { '        new string[] { ' + (CsString $_.path) + ', ' + (CsString $_.sha256) + ' },' }) -join "`n"
$source = @"
// Generated from frozen sources and historical raw type arithmetic; no absolute control hints.
internal static class SourcePins
{
    internal const string Repo = $(CsString $RepoRoot);
    internal const string GeneratedPath = $(CsString $generated);
    internal const string RawReference = $(CsString $rawPath);
    internal const string RawReferenceSha256 = $(CsString $rawHash);
    internal const long RootSlotRva = $($roles.RootSlotRva)L;
    internal const long RootTypeRva = $($roles.RootTypeRva)L;
    internal const long HudTypeRva = $($roles.HudTypeRva)L;
    internal static readonly string[][] Files = new string[][]
    {
$pinLines
    };
}
"@
[IO.File]::WriteAllText($generated, $source, [Text.UTF8Encoding]::new($false))
$dotnet = Join-Path $RepoRoot 'LicensingSystem\.cache\dotnet-10\dotnet.exe'
$log = Join-Path $buildDir 'build.log'
& $dotnet build $project --configuration Release "-p:GeneratedPins=$generated" "-p:BaseIntermediateOutputPath=$buildDir\obj\" "-p:MSBuildProjectExtensionsPath=$buildDir\obj\" "-p:OutputPath=$buildDir\bin\" 2>&1 | Tee-Object -FilePath $log
$buildExit = $LASTEXITCODE
$after = @($pins | ForEach-Object { [ordered]@{ path = $_.path; sha256 = Sha $_.path; beforeSha256 = $_.sha256; match = (Sha $_.path) -eq $_.sha256 } })
if (@($after | Where-Object { -not $_.match }).Count -gt 0) { throw 'Source changed during build' }
$exe = Join-Path $buildDir 'bin\net48\MatchHud.Qualification.Only.exe'
$manifest = [ordered]@{
    status = $(if ($buildExit -eq 0) { 'built_not_native_run' } else { 'build_failed' })
    buildExitCode = $buildExit; nativeRun = $false; productAcceptance = $false; ReaderConstructed = $false
    framework = 'net48'; platform = 'x64'; helperLinked = $helperPath
    selectorSource = $readerPath; layoutSource = $layoutPath; roles = $roles
    rawReference = [ordered]@{ path = $rawPath; sha256 = $rawHash; moduleBase = ('0x{0:x}' -f $rawBase); rootVtable = ('0x{0:x}' -f $expectedRoot); hudVtable = ('0x{0:x}' -f $expectedHud); arithmeticMatch = $true; absoluteControlHintsEmitted = $false }
    sourcePinsBefore = $pins; sourcePinsAfter = $after
    generatedPins = [ordered]@{ path = $generated; sha256 = Sha $generated }
    executable = $(if (Test-Path -LiteralPath $exe) { [ordered]@{ path = $exe; sha256 = Sha $exe; bytes = (Get-Item -LiteralPath $exe).Length } } else { $null })
    buildLog = $log
}
$manifestPath = Join-Path $buildDir 'build-manifest.json'
[IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 12), [Text.UTF8Encoding]::new($false))
if ($buildExit -ne 0) { throw "Build failed ($buildExit), inspect $log" }
$manifest | ConvertTo-Json -Depth 12
