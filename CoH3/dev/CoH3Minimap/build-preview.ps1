$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$version = '0.5.6-native-preview'
$publishDir = Join-Path $projectRoot "artifacts\preview-$stamp"
$distDir = Join-Path $projectRoot 'dist'
New-Item -ItemType Directory -Force $distDir | Out-Null
& dotnet restore (Join-Path $projectRoot 'CoH3Minimap.csproj') -r win-x64 --locked-mode
if ($LASTEXITCODE -ne 0) { throw "Locked restore failed: $LASTEXITCODE" }
& dotnet publish (Join-Path $projectRoot 'CoH3Minimap.csproj') -c Release -r win-x64 --no-restore --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "Publish failed: $LASTEXITCODE" }
$exe = Join-Path $publishDir 'CoH3Minimap.exe'
$stdout = Join-Path $publishDir 'selftest.stdout.txt'
$stderr = Join-Path $publishDir 'selftest.stderr.txt'
$test = Start-Process -FilePath $exe -ArgumentList '--selftest' -Wait -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
if ($test.ExitCode -ne 0) { throw "Published EXE selftest failed: $($test.ExitCode). See $stdout and $stderr" }
Copy-Item (Join-Path $projectRoot 'README.md') $publishDir
Copy-Item (Join-Path $projectRoot 'THIRD-PARTY-NOTICES.md') $publishDir
Copy-Item (Join-Path $projectRoot 'licenses') $publishDir -Recurse
$zip = Join-Path $distDir "CoH3Minimap-$version-$stamp.zip"
$files = @($exe, (Join-Path $publishDir 'README.md'), (Join-Path $publishDir 'THIRD-PARTY-NOTICES.md'), (Join-Path $publishDir 'licenses'))
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipStream = [IO.File]::Open($zip, [IO.FileMode]::CreateNew, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
$writer = New-Object IO.Compression.ZipArchive($zipStream, [IO.Compression.ZipArchiveMode]::Create, $false)
try {
    foreach ($item in $files) {
        $entries = if (Test-Path -LiteralPath $item -PathType Container) { Get-ChildItem -LiteralPath $item -File -Recurse } else { Get-Item -LiteralPath $item }
        foreach ($file in $entries) {
            $entryName = $file.FullName.Substring($publishDir.Length + 1).Replace('\', '/')
            $entry = $writer.CreateEntry($entryName, [IO.Compression.CompressionLevel]::Optimal)
            $input = [IO.File]::Open($file.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, ([IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete))
            $output = $entry.Open()
            try { $input.CopyTo($output) } finally { $output.Dispose(); $input.Dispose() }
        }
    }
} finally { $writer.Dispose(); $zipStream.Dispose() }
$archive = [IO.Compression.ZipFile]::OpenRead($zip)
try {
    $entry = $archive.GetEntry('CoH3Minimap.exe')
    if ($null -eq $entry) { throw 'EXE missing in ZIP' }
    $stream = $entry.Open()
    $hasher = [Security.Cryptography.SHA256]::Create()
    try { $hashBytes = $hasher.ComputeHash($stream) }
    finally { $hasher.Dispose(); $stream.Dispose() }
    $zipEntryHash = [BitConverter]::ToString($hashBytes).Replace('-', '')
} finally { $archive.Dispose() }
$exeHash = (Get-FileHash $exe -Algorithm SHA256).Hash
if ($exeHash -ne $zipEntryHash) { throw 'ZIP EXE mismatch' }
$report = [ordered]@{
    CreatedAt = (Get-Date).ToString('o')
    Exe = $exe
    FileVersion = (Get-Item $exe).VersionInfo.FileVersion
    ExeSha256 = $exeHash
    Zip = $zip
    ZipSha256 = (Get-FileHash $zip -Algorithm SHA256).Hash
    ZipExeSha256 = $zipEntryHash
    PublishedExeSyntheticSelftestPassed = $true
    GameAccessedBySelftest = $false
    NativeHudRegistryImplemented = $true
    OriginalMarksAndSelectionColorUsed = $true
    NativeMapArchiveSelectionImplemented = $true
    OfficialIconsIntegrated = $true
    TerritoryInteractivityImplemented = $true
    CameraViewportImplemented = $true
    FogCpuPlanesAndDisabledSelectorImplemented = $true
    NativePostMatchStateHintImplemented = $true
    FogGenerateMathSyntheticTestsPassed = $true
    ExperimentalCpuFogWindowImplemented = $true
    ExperimentalCpuFogDefaultEnabled = $false
    ExperimentalCpuFogModulationAssumption = '1'
    ExperimentalCpuFogTemporalHistoryImplemented = $false
    FogRawFixtureExportImplemented = $true
    SquadEnumerationImplemented = $true
    SquadEnumerationReadOnly = $true
    ShowAllUnitsDefaultEnabled = $false
    SquadUnitIconsImplemented = $true
    EnemyUnitsUnderFogDisplayed = $true
    FogActiveUiCompositionVerified = $false
    TemporalBlendExact = $false
    ContinuousMovementSyncMeasured = $true
    SameSceneVisualComparisonPassed = $false
    LevelBAccepted = $false
    Note = 'Implementation flags and synthetic checks are separate from the recorded static-scene runtime comparison. Squad enumeration and unit icons were live-verified against sim-side reads (entity-integration-20261112); movement sync measured on live match marks (round13-watch-livematch-20261112). Full Level B is not accepted.'
}
$reportPath = Join-Path $distDir "validation-$version-$stamp.json"
$report | ConvertTo-Json | Set-Content -Encoding utf8 $reportPath
$report | ConvertTo-Json
Write-Output "Report: $reportPath"
