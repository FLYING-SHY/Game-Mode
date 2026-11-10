param([string]$Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe')
$ErrorActionPreference = 'Stop'
$licensingRoot = Split-Path $PSScriptRoot -Parent
$config = Join-Path $licensingRoot '.local\server.json'
if (!(Test-Path $config)) { throw 'Run Initialize-Local.ps1 first; no real backend configuration exists.' }
& $Docker compose -f (Join-Path $PSScriptRoot 'compose.yaml') up -d postgres redis web worker
if ($LASTEXITCODE -ne 0) { throw 'Local licensing containers did not start.' }
$previous = $env:LICENSING_SERVER_CONFIG
try {
    # Scoped to this PowerShell process and its child, never a persistent/global environment change.
    $env:LICENSING_SERVER_CONFIG = $config
    & dotnet run --project (Join-Path $licensingRoot 'server\Licensing.Server.csproj') -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Local licensing adapter exited with a build or runtime error.' }
} finally { $env:LICENSING_SERVER_CONFIG = $previous }
