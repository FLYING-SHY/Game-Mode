$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$dotnet = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
& $dotnet build (Join-Path $PSScriptRoot 'MaintenanceLive.csproj') -c Release -v:minimal 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot 'build.log')
if ($LASTEXITCODE -ne 0) { throw "Maintenance live build failed: exit=$LASTEXITCODE" }
Write-Output (Join-Path $PSScriptRoot 'bin/Release/net48/SkyEye.Maintenance.Live.exe')
