param([switch]$SelfTest)
$ErrorActionPreference = 'Stop'
function Convert-MaintenanceUiSource([string]$Text, $Spec, [string]$SourceName) {
    $matches = [regex]::Matches($Text, [regex]::Escape($Spec.Before)).Count
    $allConstructorSites = [regex]::Matches($Text, 'new\s+PlayerInfoReader\s*\(').Count
    if ($matches -ne 1 -or $allConstructorSites -ne 1) { throw ('Expected exactly one known reader creation in {0}, exact={1}, total={2}' -f $SourceName, $matches, $allConstructorSites) }
    if ($Text.Contains($Spec.After)) { throw ('Unexpected pre-existing maintenance factory in production source: ' + $SourceName) }
    $transformed = $Text.Replace($Spec.Before, $Spec.After)
    if ([regex]::Matches($transformed, 'new\s+PlayerInfoReader\s*\(').Count -ne 0 -or [regex]::Matches($transformed, [regex]::Escape($Spec.After)).Count -ne 1) { throw ('Transformation invariant failed: ' + $SourceName) }
    return $transformed
}
if ($SelfTest) {
    $fixture = @{ Before = 'new PlayerInfoReader()'; After = 'PlayerInfoReader.OpenForMaintenanceDiagnostics()' }
    if ((Convert-MaintenanceUiSource 'reader = new PlayerInfoReader();' $fixture 'fixture') -ne 'reader = PlayerInfoReader.OpenForMaintenanceDiagnostics();') { throw 'Exact transformation fixture failed' }
    $rejected = 0
    foreach ($sample in @('reader = null;', 'new PlayerInfoReader(); new PlayerInfoReader();', 'new PlayerInfoReader(); new PlayerInfoReader(context);', 'new PlayerInfoReader(); PlayerInfoReader.OpenForMaintenanceDiagnostics();')) {
        try { [void](Convert-MaintenanceUiSource $sample $fixture 'fixture') } catch { $rejected++ }
    }
    if ($rejected -ne 4) { throw 'Missing/multiple/unexpected/already-maintenance fixtures did not all fail' }
    Write-Host 'PASS generation fixtures: exact replacement; missing, multiple, extra constructor and existing factory fail closed (5/5). No game or authorization code invoked.'
    exit 0
}
$client = (Resolve-Path (Join-Path $PSScriptRoot '../../client')).Path
$generated = Join-Path $PSScriptRoot 'obj/maintenance-ui-generated'
[void][IO.Directory]::CreateDirectory($generated)
$specs = @(
    @{ File = 'OverlaySettingsForm.cs'; Before = 'new PlayerInfoReader()'; After = 'PlayerInfoReader.OpenForMaintenanceDiagnostics()' },
    @{ File = 'SquadMarkerWindow.cs'; Before = 'new PlayerInfoReader(markerScene.Context)'; After = 'PlayerInfoReader.OpenForMaintenanceDiagnostics(markerScene.Context)' },
    @{ File = 'AirOverlayWindow.cs'; Before = 'new PlayerInfoReader(markerScene.Context)'; After = 'PlayerInfoReader.OpenForMaintenanceDiagnostics(markerScene.Context)' }
)
$utf8 = New-Object Text.UTF8Encoding($false)
$records = @()
foreach ($spec in $specs) {
    $source = Join-Path $client ('CoH3MemoryTool/' + $spec.File)
    $beforeHash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
    $text = [IO.File]::ReadAllText($source)
    $transformed = Convert-MaintenanceUiSource $text $spec $source
    $destination = Join-Path $generated $spec.File
    [IO.File]::WriteAllText($destination, $transformed, $utf8)
    $afterHash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
    if ($beforeHash -ne $afterHash) { throw ('Production source changed during generation: ' + $source) }
    $records += [ordered]@{ source = $source; sourceSha256 = $beforeHash; generated = $destination; generatedSha256 = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash; replacements = 1; before = $spec.Before; after = $spec.After; productionSourceUnchanged = $true }
}
$report = [ordered]@{
    utc = [datetime]::UtcNow.ToString('O'); maintenanceOnly = $true; buildVerified = $false
    startupObject = 'CoH3MemoryTool.ControlsProgram'; compileSymbol = 'MAINTENANCE_DIAGNOSTICS'
    replacedFiles = $records; replacementCount = 3
    readerInternalReplacements = 0
    linkedLicenseCoordinatorSourceSha256 = (Get-FileHash -LiteralPath (Join-Path $client 'CoH3MemoryTool/LicenseCoordinator60.cs') -Algorithm SHA256).Hash
    authorization = 'Original ControlsProgram -> LicenseApplication60 -> LicenseCoordinator60; original Matches, pin, ProductId and credential handling. No synthetic grant.'
}
[IO.File]::WriteAllText((Join-Path $generated 'source-manifest.json'), ($report | ConvertTo-Json -Depth 8), $utf8)
Write-Host 'MAINTENANCE ONLY / UNVERIFIED: generated exactly 3 UI caller copies; production sources unchanged.'
