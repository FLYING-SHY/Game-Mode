param([switch]$NoBuild)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$dotnet = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
$project = Join-Path $PSScriptRoot 'SessionReset.Fixture.csproj'
$results = @()
foreach ($flavor in @('SkyEye', 'Simple')) {
    $obj = Join-Path $PSScriptRoot "obj/$flavor/"
    $out = Join-Path $PSScriptRoot "bin/$flavor/"
    if (-not $NoBuild) {
        & $dotnet build $project -c Release --nologo "-p:ClientFlavor=$flavor" "-p:BaseIntermediateOutputPath=$obj" "-p:OutputPath=$out" 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot "build-$flavor.log")
        if ($LASTEXITCODE -ne 0) { throw "Session-reset fixture build failed: $flavor" }
    }
    $exe = Join-Path $out "SessionReset.$flavor.Fixture.exe"
    $log = Join-Path $PSScriptRoot "run-$flavor.log"
    & $exe 2>&1 | Tee-Object -FilePath $log
    if ($LASTEXITCODE -ne 0) { throw "Session-reset fixture failed: $flavor" }
    $results += [ordered]@{ client = $flavor; level = 'F'; exit_code = $LASTEXITCODE; executable = $exe; log = $log }
}
$inputPaths = @(
    'CoH3/dev/SkyEye/client/CoH3MemoryTool/OverlaySettingsForm.cs',
    'CoH3/dev/COH3SimpleTool/CoH3MemoryTool/ControlBarForm.cs',
    'CoH3/dev/SkyEye/client/CoH3MemoryTool/EnemySelection.cs',
    'CoH3/dev/COH3SimpleTool/CoH3MemoryTool/EnemySelection.cs',
    'CoH3/dev/SkyEye/client/CoH3MemoryTool/ArmourProductionTracker.cs',
    'CoH3/dev/COH3SimpleTool/CoH3MemoryTool/ArmourProductionTracker.cs'
)
$hashes = @($inputPaths | ForEach-Object {
    $path = Join-Path $repo $_
    [ordered]@{ path = $path; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
})
[ordered]@{ utc = [datetime]::UtcNow.ToString('o'); results = $results; source_inputs = $hashes; game_validation = 'not_run'; gui_validation = 'not_run' } |
    ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'latest-result.json') -Encoding UTF8
