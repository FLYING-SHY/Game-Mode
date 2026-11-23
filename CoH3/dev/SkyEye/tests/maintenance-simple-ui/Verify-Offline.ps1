$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Run with Windows PowerShell 5.1 for net48 reflection-only inspection.' }
$exe = Join-Path $PSScriptRoot 'bin/Release/net48/COH3SimpleTool.Maintenance.UI.Unverified.exe'
$manifestPath = Join-Path $PSScriptRoot 'obj/maintenance-simple-ui-generated/source-manifest.json'
$manifest = [IO.File]::ReadAllText($manifestPath) | ConvertFrom-Json
if ($manifest.replacementCount -ne 3 -or $manifest.readerInternalReplacements -ne 0 -or @($manifest.replacedFiles).Count -ne 3) { throw 'Source manifest replacement invariant failed' }
foreach ($record in $manifest.replacedFiles) {
    if ((Get-FileHash -LiteralPath $record.source -Algorithm SHA256).Hash -ne $record.sourceSha256) { throw ('Production source changed after generation: ' + $record.source) }
    if ((Get-FileHash -LiteralPath $record.generated -Algorithm SHA256).Hash -ne $record.generatedSha256) { throw ('Generated source changed after generation: ' + $record.generated) }
    if (-not $record.productionSourceUnchanged -or $record.replacements -ne 1) { throw 'Source mutation/replacement invariant failed' }
}
foreach ($record in $manifest.replacedFiles) {
    $sourceText = [IO.File]::ReadAllText($record.source)
    $generatedText = [IO.File]::ReadAllText($record.generated)
    if ($generatedText -cne $sourceText.Replace($record.before, $record.after)) { throw 'Generated source has edits beyond the exact factory replacement' }
}
foreach ($inputRecord in $manifest.sourceInputs) {
    if ((Get-FileHash -LiteralPath $inputRecord.path -Algorithm SHA256).Hash -ne $inputRecord.sha256) { throw ('Original source/resource changed during build: ' + $inputRecord.path) }
}
if ($manifest.environmentBypassAdded -ne $false -or @($manifest.sourceInputs).Count -lt 7) { throw 'Input/source-only transformation audit missing' }
$assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($exe)
if ($assembly.GetName().Name -ne 'COH3SimpleTool.Maintenance.UI.Unverified') { throw 'Unexpected assembly identity' }
if ($assembly.EntryPoint.DeclaringType.FullName -ne 'CoH3MemoryTool.SimpleProgram') { throw 'Original controls startup entry not preserved' }
$expectedResources = @('CoH3.InterfaceIcon', 'names-zh-hans.tsv', 'CoH3.BarrageCatalogue.tsv')
$resources = @($assembly.GetManifestResourceNames())
foreach ($name in $expectedResources) { if ($resources -cnotcontains $name) { throw ('Missing exact embedded resource logical name: ' + $name) } }
$reader = $assembly.GetType('CoH3MemoryTool.PlayerInfoReader', $true)
$flags = [Reflection.BindingFlags]'NonPublic,Public,Static'
$factories = @($reader.GetMethods($flags) | Where-Object { $_.Name -eq 'OpenForMaintenanceDiagnostics' })
if ($factories.Count -ne 2) { throw 'Expected both maintenance factory overloads in this isolated assembly' }
$client = (Resolve-Path (Join-Path $PSScriptRoot '../../../COH3SimpleTool')).Path
$configSourceHash = (Get-FileHash -LiteralPath (Join-Path $client 'app.config') -Algorithm SHA256).Hash
$configOutputHash = (Get-FileHash -LiteralPath ($exe + '.config') -Algorithm SHA256).Hash
if ($configSourceHash -ne $configOutputHash) { throw 'App config bytes differ from linked original' }
$report = [ordered]@{
    utc = [datetime]::UtcNow.ToString('O'); maintenanceOnly = $true; buildVerified = $false; guiRun = 'not_run'; authorizationRun = 'not_run'; gameAttach = 'not_run'
    assembly = $exe; assemblySha256 = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash
    startup = $assembly.EntryPoint.DeclaringType.FullName; exactReplacementCount = 3; originalUiSourcesUnchanged = $true; maintenanceFactoryCount = $factories.Count
    embeddedResources = $resources; expectedResources = $expectedResources; originalAppConfigBytesMatch = $true
    originalAppConfigSha256 = $configSourceHash; sourceManifest = $manifestPath
    auditedOriginalInputCount = @($manifest.sourceInputs).Count; environmentBypassAdded = $false
    originalAuthorization = 'SimpleProgram -> SimpleLicenseApp -> original LicenseCoordinator60 + BlackLoginForm; no replacement auth or new grants'
    verified = @('source hashes unchanged', 'exact 3 generated caller replacements', 'reader internal source linked unchanged', 'original SimpleProgram startup', 'maintenance compile-only factories', 'exact resource logical names', 'original app config bytes')
    limitations = @('Reflection-only metadata checks do not execute Main or initialize the game reader.', 'Actual activation, overlay rendering, window ownership and close/revoke behavior require lead-controlled GUI validation.', 'All candidate layouts remain unverified for formal release.')
}
$utf8 = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'build-report.json'), ($report | ConvertTo-Json -Depth 8), $utf8)
Write-Host 'PASS offline metadata/resources/config/source-hash checks. MAINTENANCE ONLY / UNVERIFIED. GUI, auth and game attachment NOT RUN.'
