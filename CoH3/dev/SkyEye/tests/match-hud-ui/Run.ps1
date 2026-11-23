$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$dotnet = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
$project = Join-Path $PSScriptRoot 'MatchHudUi.Fixture.csproj'
$report = Join-Path $PSScriptRoot 'fixture-result.json'
if (Test-Path -LiteralPath $report) { throw 'Preserve the existing HUD UI fixture report; use a separate run directory.' }
$sourceRoots = @('CoH3/dev/SkyEye/client/CoH3MemoryTool', 'CoH3/dev/SkyEye/client/Skyeye.QqProbe', 'CoH3/dev/COH3SimpleTool/CoH3MemoryTool')
$sourceFiles = @($sourceRoots | ForEach-Object { Get-ChildItem -LiteralPath (Join-Path $repo $_) -Filter '*.cs' -File -Recurse })
$sourceFiles += Get-Item -LiteralPath $project, (Join-Path $PSScriptRoot 'Program.cs'), (Join-Path $PSScriptRoot '../session-reset/Program.cs')
$before = @($sourceFiles | Get-FileHash -Algorithm SHA256)
$results = @()
foreach ($flavor in @('SkyEye', 'Simple')) {
    $obj = Join-Path $PSScriptRoot "obj/$flavor/"
    $out = Join-Path $PSScriptRoot "bin/$flavor/"
    & $dotnet build $project -c Release --nologo "-p:ClientFlavor=$flavor" "-p:BaseIntermediateOutputPath=$obj" "-p:OutputPath=$out" 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot "build-$flavor.log")
    if ($LASTEXITCODE -ne 0) { throw "HUD UI fixture build failed: $flavor" }
    $exe = Join-Path $out "MatchHudUi.$flavor.Fixture.exe"
    $log = Join-Path $PSScriptRoot "run-$flavor.log"
    & $exe 2>&1 | Tee-Object -FilePath $log
    if ($LASTEXITCODE -ne 0) { throw "HUD UI fixture failed: $flavor" }
    $results += [ordered]@{ client = $flavor; level = 'F'; exitCode = 0; executable = $exe; sha256 = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash; log = $log }
}
foreach ($entry in $before) {
    if ((Get-FileHash -LiteralPath $entry.Path -Algorithm SHA256).Hash -ne $entry.Hash) { throw ('HUD UI fixture source changed during run: ' + $entry.Path) }
}
[ordered]@{
    schema = 'match-hud-ui-fixture-v1'; utc = [datetime]::UtcNow.ToString('o'); status = 'PASS_fixture_only'; level = 'F'
    sdk = (& $dotnet --version); results = $results; sourceManifest = @($before | ForEach-Object { [ordered]@{ path = $_.Path; sha256 = $_.Hash } })
    scope = 'actual production forms/reset/publication with managed handle-free reader identity and no form constructors'
    guiValidation = 'not_run'; gameValidation = 'not_run'; normalLicenseModified = $false; productAcceptance = $false
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $report -Encoding UTF8
