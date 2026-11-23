$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$dotnet = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'GenerateSources.ps1') -SelfTest 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot 'generation-self-test.log')
if ($LASTEXITCODE -ne 0) { throw 'Generation fixtures failed' }
& $dotnet build (Join-Path $PSScriptRoot 'MaintenanceSimpleUi.csproj') -c Release --nologo 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Maintenance UI build failed' }
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Verify-Offline.ps1') 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot 'offline-verification.log')
if ($LASTEXITCODE -ne 0) { throw 'Offline metadata verification failed' }
Write-Host (Join-Path $PSScriptRoot 'bin/Release/net48/COH3SimpleTool.Maintenance.UI.Unverified.exe')
Write-Host 'BUILD ONLY: no GUI launch, login, game discovery or attachment performed.'
