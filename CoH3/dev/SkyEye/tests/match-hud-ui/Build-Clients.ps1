$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$sdk = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
$fixture = Join-Path $repo 'CoH3/dev/SkyEye/tests/build-gate/BuildGate.Fixture.csproj'
$report = Join-Path $PSScriptRoot 'clients-build-report.json'
if (Test-Path -LiteralPath $report) { throw 'Preserve the existing HUD client build report; choose a separate run directory.' }
$projects = @(
    @{ Name = 'SkyEye'; Project = 'CoH3/dev/SkyEye/client/天穹之眼-v0.60.8.csproj'; Source = 'CoH3/dev/SkyEye/client/CoH3MemoryTool'; Exe = '天穹之眼-自有授权版-v0.60.8.exe' },
    @{ Name = 'Simple'; Project = 'CoH3/dev/COH3SimpleTool/COH3SimpleTool.csproj'; Source = 'CoH3/dev/COH3SimpleTool/CoH3MemoryTool'; Exe = 'COH3工具.exe' }
)
$sourceFiles = @($projects | ForEach-Object { Get-ChildItem -LiteralPath (Join-Path $repo $_.Source) -Filter '*.cs' -File })
$sourceFiles += @($projects | ForEach-Object { Get-Item -LiteralPath (Join-Path $repo $_.Project) })
$before = @($sourceFiles | Get-FileHash -Algorithm SHA256)
$results = @()
foreach ($project in $projects) {
    $source = Join-Path $repo $project.Source
    foreach ($mode in @('Release', 'Maintenance')) {
        $target = Join-Path $PSScriptRoot ('clients/' + $project.Name + '/' + $mode)
        New-Item -ItemType Directory -Path $target -Force | Out-Null
        $obj = Join-Path $target 'obj/'
        $bin = Join-Path $target 'bin/'
        $buildArgs = @('build', (Join-Path $repo $project.Project), '-c', 'Release', "-p:IntermediateOutputPath=$obj", "-p:MSBuildProjectExtensionsPath=$obj", "-p:OutputPath=$bin", '--nologo', '-v:minimal')
        if ($mode -eq 'Maintenance') { $buildArgs += '-p:DefineConstants=MAINTENANCE_DIAGNOSTICS' }
        & $sdk @buildArgs 2>&1 | Tee-Object -FilePath (Join-Path $target 'build.log')
        if ($LASTEXITCODE -ne 0) { throw ('HUD client build failed: ' + $project.Name + ' ' + $mode) }
        $gateBin = Join-Path $target 'gate/bin/'
        $gateObj = Join-Path $target 'gate/obj/'
        & $sdk build $fixture -c $mode "-p:GateSourceRoot=$source" "-p:IntermediateOutputPath=$gateObj" "-p:MSBuildProjectExtensionsPath=$gateObj" "-p:OutputPath=$gateBin" --nologo -v:minimal 2>&1 | Tee-Object -FilePath (Join-Path $target 'gate-build.log')
        if ($LASTEXITCODE -ne 0) { throw ('HUD gate fixture build failed: ' + $project.Name + ' ' + $mode) }
        & $sdk (Join-Path $gateBin 'BuildGate.Fixture.dll') $source (Join-Path $bin $project.Exe) 2>&1 | Tee-Object -FilePath (Join-Path $target 'gate-run.log')
        if ($LASTEXITCODE -ne 0) { throw ('HUD gate fixture failed: ' + $project.Name + ' ' + $mode) }
        $results += [ordered]@{
            client = $project.Name; mode = $mode; buildExitCode = 0; gateExitCode = 0
            executable = (Join-Path $bin $project.Exe); sha256 = (Get-FileHash -LiteralPath (Join-Path $bin $project.Exe) -Algorithm SHA256).Hash
        }
    }
}
foreach ($entry in $before) {
    if ((Get-FileHash -LiteralPath $entry.Path -Algorithm SHA256).Hash -ne $entry.Hash) { throw ('Source changed during HUD build: ' + $entry.Path) }
}
[ordered]@{
    schema = 'match-hud-client-build-v1'; utc = [datetime]::UtcNow.ToString('O'); status = 'PASS_build_and_gate_fixture_only'
    sdk = (& $sdk --version); results = $results; sourceManifest = @($before | ForEach-Object { [ordered]@{ path = $_.Path; sha256 = $_.Hash } })
    gui = 'not_run'; realGame = 'not_run'; protected = 'not_built'; delivery = 'not_updated'; productAcceptance = $false
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $report -Encoding utf8
