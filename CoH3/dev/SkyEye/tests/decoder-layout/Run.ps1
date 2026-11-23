$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$dotnet = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
$project = Join-Path $PSScriptRoot 'DecoderLayout.Fixture.csproj'
$engine = (Resolve-Path (Join-Path $PSScriptRoot '../../client/CoH3MemoryTool')).Path
$captured = (Resolve-Path (Join-Path $PSScriptRoot '../../evidence/version-adaptation/projectiles/event-30s-02.json')).Path
$capturedHash = (Get-FileHash -LiteralPath $captured -Algorithm SHA256).Hash
$airDirectory = (Resolve-Path (Join-Path $PSScriptRoot '../../evidence/version-adaptation/air')).Path
$airBefore = @('live-raw-04-p47-middle.json','live-raw-05-p47-late.json','live-raw-07-p47-early.json','live-raw-08-p47-middle.json','live-raw-09-p47-late.json' | ForEach-Object { $path = Join-Path $airDirectory $_; [ordered]@{ path = $path; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash } })
$fixtureBefore = @('Program.cs','AirMoverReplay.cs','Run.ps1','DecoderLayout.Fixture.csproj' | ForEach-Object { $path = Join-Path $PSScriptRoot $_; [ordered]@{ path = $path; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash } })
# Per-build dispatch baseline: the layout table now ships in every build, so both configurations
# must assert the same policy. The task-4 baseline stays untouched as the historical record.
$baselineDirectory = 'perbuild-baseline'
$historicalBaseline = 'task4-baseline'
$perBuildPreservation = @()
$reports = @{}
[xml]$xml = [IO.File]::ReadAllText($project)
$spec = @($xml.Project.ItemGroup.Compile | Where-Object { $_.Include })[0].Include
$sourceFiles = @($spec.Split(';') | ForEach-Object { $_.Replace('$(EngineRoot)', $engine) })
$before = @($sourceFiles | ForEach-Object { [ordered]@{ path = $_; sha256 = (Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash } })
foreach ($mode in @('Release', 'Maintenance')) {
    $isMaintenance = ($mode -eq 'Maintenance').ToString().ToLowerInvariant()
    & $dotnet build $project -c $mode ('-p:Maintenance=' + $isMaintenance) --nologo 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot ($mode.ToLowerInvariant() + '-build.log'))
    if ($LASTEXITCODE -ne 0) { throw ($mode + ' fixture build failed') }
    & $dotnet (Join-Path $PSScriptRoot ('bin/' + $mode + '/net10.0/DecoderLayout.Fixture.dll')) --captured-projectile $captured --captured-air $airDirectory 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot ($mode.ToLowerInvariant() + '-test.log'))
    if ($LASTEXITCODE -ne 0) { throw ($mode + ' fixtures failed') }
    $baseline = Get-Content -LiteralPath (Join-Path $PSScriptRoot ($baselineDirectory + '/' + $mode.ToLowerInvariant() + '-fixture-report.json')) -Raw | ConvertFrom-Json
    $current = Get-Content -LiteralPath (Join-Path $PSScriptRoot ('bin/' + $mode + '/net10.0/fixture-report.json')) -Raw | ConvertFrom-Json
    if ($current.legacyAndMapperPassed -ne $baseline.legacyAndMapperPassed) { throw ($mode + ' original assertion count changed') }
    if ($current.passed -ne $baseline.passed) { throw ($mode + ' total assertion count changed') }
    for ($i = 0; $i -lt $baseline.legacyAndMapperPassed; $i++) {
        if ($current.checks[$i] -cne $baseline.checks[$i]) { throw ($mode + ' original assertion changed at ' + $i) }
    }
    $reports[$mode] = $current
    $perBuildPreservation += [ordered]@{ mode = $mode; originalPassed = $baseline.passed; originalNamesAndOrderPreserved = $true; currentPassed = $current.passed }
}
if (($reports['Release'].checks -join '|') -cne ($reports['Maintenance'].checks -join '|')) { throw 'Release and Maintenance must assert the same per-build policy' }
if ($reports['Release'].passed -ne $reports['Maintenance'].passed) { throw 'Release and Maintenance must report the same assertion count' }
foreach ($record in $before) {
    if ((Get-FileHash -LiteralPath $record.path -Algorithm SHA256).Hash -ne $record.sha256) { throw ('Linked production source changed during fixture run; rerun required: ' + $record.path) }
}
if ((Get-FileHash -LiteralPath $captured -Algorithm SHA256).Hash -ne $capturedHash) { throw 'Captured event JSON changed during run; rerun required' }
foreach ($record in @($airBefore) + @($fixtureBefore)) {
    if ((Get-FileHash -LiteralPath $record.path -Algorithm SHA256).Hash -ne $record.sha256) { throw ('Fixture source or air input changed during run: ' + $record.path) }
}
$report = [ordered]@{ utc = [datetime]::UtcNow.ToString('O'); scope = 'F only, no G/R evidence'; baseline = $baselineDirectory; historicalBaseline = $historicalBaseline; bothConfigurationsAssertTheSamePolicy = $true; linkedSourceHashes = $before; linkedSourcesUnchangedDuringRun = $true; fixtureSourceHashes = $fixtureBefore; fixtureSourcesUnchangedDuringRun = $true; capturedEvent = $captured; capturedEventSha256 = $capturedHash; capturedEventUnchangedDuringRun = $true; capturedAirInputs = $airBefore; capturedAirUnchangedDuringRun = $true; perBuildAssertions = $perBuildPreservation; configurations = @('Release', 'Maintenance') }
$utf8 = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'source-manifest.json'), ($report | ConvertTo-Json -Depth 5), $utf8)
Write-Host 'PASS source hash stability. All artifacts remain in decoder-layout; original production sources untouched.'
