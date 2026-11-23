$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$dotnet = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
$fixture = Join-Path $PSScriptRoot 'BuildGate.Fixture.csproj'
$log = Join-Path $PSScriptRoot 'validation.log'
'Static + fixture validation only; no game launch or attachment.' | Set-Content -LiteralPath $log -Encoding UTF8
$projects = @(
    @{ Name = 'SkyEye'; Project = 'CoH3/dev/SkyEye/client/天穹之眼-v0.60.8.csproj'; Source = 'CoH3/dev/SkyEye/client/CoH3MemoryTool'; Exe = '天穹之眼-自有授权版-v0.60.8.exe' },
    @{ Name = 'Simple'; Project = 'CoH3/dev/COH3SimpleTool/COH3SimpleTool.csproj'; Source = 'CoH3/dev/COH3SimpleTool/CoH3MemoryTool'; Exe = 'COH3工具.exe' }
)
foreach ($project in $projects) {
    foreach ($mode in @('Release', 'Maintenance')) {
        "=== $($project.Name) $mode ===" | Tee-Object -FilePath $log -Append
        $target = Join-Path $PSScriptRoot "full-build/$($project.Name)/$mode"
        $obj = "$target/obj/"
        $bin = "$target/bin/"
        $source = Join-Path $repo $project.Source
        $buildArgs = @('build', (Join-Path $repo $project.Project), '-c', 'Release', "-p:IntermediateOutputPath=$obj", "-p:MSBuildProjectExtensionsPath=$obj", "-p:OutputPath=$bin", '-v:minimal')
        if ($mode -eq 'Maintenance') { $buildArgs += '-p:DefineConstants=MAINTENANCE_DIAGNOSTICS' }
        & $dotnet @buildArgs 2>&1 | Tee-Object -FilePath $log -Append
        if ($LASTEXITCODE -ne 0) { throw "Client compilation failed: $($project.Name) $mode exit=$LASTEXITCODE" }
        & $dotnet run --project $fixture -c $mode "-p:GateSourceRoot=$source" -- $source (Join-Path $bin $project.Exe) 2>&1 | Tee-Object -FilePath $log -Append
        if ($LASTEXITCODE -ne 0) { throw "Fixture failed: $($project.Name) $mode exit=$LASTEXITCODE" }
    }
}
