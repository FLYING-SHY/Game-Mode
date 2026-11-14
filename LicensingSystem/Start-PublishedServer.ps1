param([string]$ConfigPath = (Join-Path $PSScriptRoot '.local\server.json'))
$ErrorActionPreference = 'Stop'
$assembly = Join-Path $PSScriptRoot 'Licensing.Server.dll'
if (-not (Test-Path -LiteralPath $assembly)) { throw 'Run this script from the published server directory.' }
if (-not (Test-Path -LiteralPath $ConfigPath)) { throw 'Create a private configuration with deployment/New-DeploymentConfig.ps1 before starting this server.' }
$config = (Resolve-Path -LiteralPath $ConfigPath).Path
$candidates = @(
    (Join-Path $PSScriptRoot '..\..\.cache\dotnet-10\dotnet.exe'),
    (Join-Path $PSScriptRoot '.cache\dotnet-10\dotnet.exe')
)
$globalHost = Get-Command dotnet -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
if ($globalHost) { $candidates += $globalHost.Source }
$hostPath = $null
foreach ($candidate in $candidates) {
    if (Test-Path -LiteralPath $candidate) {
        $runtimes = & $candidate --list-runtimes
        if ($LASTEXITCODE -eq 0 -and ($runtimes -match '^Microsoft\.AspNetCore\.App 10\.')) { $hostPath = $candidate; break }
    }
}
if (-not $hostPath) {
    & (Join-Path $PSScriptRoot 'deployment\Get-DotNet10.ps1') -Root $PSScriptRoot | Out-Host
    $hostPath = Join-Path $PSScriptRoot '.cache\dotnet-10\dotnet.exe'
    if (-not (Test-Path -LiteralPath $hostPath)) { throw 'The local .NET 10 installation did not produce a runtime host.' }
}
$previousConfig = $env:LICENSING_SERVER_CONFIG
try {
    $env:LICENSING_SERVER_CONFIG = $config
    & $hostPath $assembly
    $serverExit = $LASTEXITCODE
} finally { $env:LICENSING_SERVER_CONFIG = $previousConfig }
exit $serverExit
