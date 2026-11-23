param()
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$dotnet = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
$project = Join-Path $PSScriptRoot 'UnknownBuild.Fixture.csproj'
$expectedBefore = '3444F469137C616D382EE795967188F82E758A14ED36AA99FB7E52FAD3AB0350'
$expectedCurrent = '39DDE7A0F2BD80E2412FED3439870CF7A49A794A9AAB0F812A9AFEE6F60C65D2'
$expectedLayout = '90AF6A0F98C5C3C48DD9CAD8595377EE90990938753E68B142899B707C596D47'
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$inputHashes = @()
$results = @()
foreach ($flavor in @('SkyEye', 'Simple')) {
    $sourceRoot = if ($flavor -eq 'SkyEye') { 'CoH3/dev/SkyEye/client/CoH3MemoryTool' } else { 'CoH3/dev/COH3SimpleTool/CoH3MemoryTool' }
    foreach ($input in @(
        @{ path = (Join-Path $PSScriptRoot "before/$flavor/PlayerInfoReader.cs"); expected = $expectedBefore },
        @{ path = (Join-Path $repo "$sourceRoot/PlayerInfoReader.cs"); expected = $expectedCurrent },
        @{ path = (Join-Path $repo "$sourceRoot/GameLayout52426.cs"); expected = $expectedLayout }
    )) {
        $actual = (Get-FileHash -LiteralPath $input.path -Algorithm SHA256).Hash
        if ($actual -ne $input.expected) { throw "Frozen source changed: $($input.path)" }
        $inputHashes += [ordered]@{ path = $input.path; sha256 = $actual }
    }
    foreach ($configuration in @('Release', 'Maintenance')) {
        $work = Join-Path $PSScriptRoot "runs/$flavor/$configuration"
        New-Item -ItemType Directory -Force -Path $work | Out-Null
        $obj = Join-Path $work 'obj/'
        $out = Join-Path $work 'bin/'
        $buildLog = Join-Path $work 'build.log'
        # WinPS 5.1 wraps native stderr as error records; the process exit code decides success.
        $ErrorActionPreference = 'Continue'
        $buildOutput = & $dotnet build $project -c $configuration --nologo "-p:ClientFlavor=$flavor" "-p:BaseIntermediateOutputPath=$obj" "-p:MSBuildProjectExtensionsPath=$obj" "-p:OutputPath=$out" '-p:UseSharedCompilation=false' '-p:RestoreIgnoreFailedSources=true' '-nodeReuse:false' 2>&1
        $buildExit = $LASTEXITCODE
        $buildOutput | Out-File -LiteralPath $buildLog -Encoding utf8
        $buildOutput | ForEach-Object { Write-Output $_.ToString() }
        $ErrorActionPreference = 'Stop'
        if ($buildExit -ne 0) { throw "Unknown-build fixture build failed: $flavor/$configuration exit=$buildExit" }
        $exe = Join-Path $out "UnknownBuild.$flavor.Fixture.exe"
        $resultPath = Join-Path $work 'result.json'
        $runLog = Join-Path $work 'run.log'
        $ErrorActionPreference = 'Continue'
        $runOutput = & $exe $resultPath 2>&1
        $runExit = $LASTEXITCODE
        $runOutput | Out-File -LiteralPath $runLog -Encoding utf8
        $runOutput | ForEach-Object { Write-Output $_.ToString() }
        $ErrorActionPreference = 'Stop'
        if ($runExit -ne 0) { throw "Unknown-build fixture failed: $flavor/$configuration exit=$runExit" }
        $result = [System.IO.File]::ReadAllText($resultPath) | ConvertFrom-Json
        if ($result.passed -ne 50 -or $result.failed -ne 0) { throw "Unexpected result counts: $flavor/$configuration" }
        $results += [ordered]@{
            client = $flavor; configuration = $configuration; level = 'F'; build_exit_code = $buildExit; run_exit_code = $runExit
            passed = $result.passed; failed = $result.failed; scenarios = $result.scenarios
            reader_constructors_executed = 0; native_handles_created = 0
            executable = $exe; result = $resultPath; build_log = $buildLog; run_log = $runLog
        }
    }
}
foreach ($input in $inputHashes) {
    if ((Get-FileHash -LiteralPath $input.path -Algorithm SHA256).Hash -ne $input.sha256) { throw "Input drift during fixture: $($input.path)" }
}
[ordered]@{
    utc = [datetime]::UtcNow.ToString('o'); sdk = (& $dotnet --version); powershell = $PSVersionTable.PSVersion.ToString()
    level = 'F'; passed = ($results | ForEach-Object { $_.passed } | Measure-Object -Sum).Sum; failed = ($results | ForEach-Object { $_.failed } | Measure-Object -Sum).Sum
    results = $results; input_hashes = $inputHashes; reader_constructors_executed = 0; native_handles_created = 0
    game_validation = 'not_run'; repeat_session_validation = 'not_run'; delivery_validation = 'not_run'
    limitations = @('Full production source is linked; only five pure reader methods execute on uninitialized managed objects.', 'Constructor call order is checked from compiled IL; constructors, LicenseGate60 and native APIs do not execute.', 'No game attachment, gameplay observation or full client build is performed here.')
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'latest-result.json') -Encoding UTF8
Write-Output 'Unknown-build fixture complete: 200 passed, 0 failed; 4 isolated flavor/configuration builds; level=F.'
