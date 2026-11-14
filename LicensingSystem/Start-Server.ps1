param([Parameter(Mandatory=$true)][string]$ConfigPath)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$sdk = Join-Path $root '.cache\dotnet-10\dotnet.exe'
$server = Join-Path $root 'server\bin\Release\net10.0\Licensing.Server.dll'
if (-not (Test-Path -LiteralPath $ConfigPath)) { throw 'The private configuration must exist.' }
if (-not (Test-Path -LiteralPath $server)) { throw 'Build the signed server first.' }
$previous = $env:LICENSING_SERVER_CONFIG
try {
    $env:LICENSING_SERVER_CONFIG = (Resolve-Path -LiteralPath $ConfigPath).Path
    & $sdk $server
    if ($LASTEXITCODE -ne 0) { throw 'Signed server exited with an error.' }
} finally { $env:LICENSING_SERVER_CONFIG = $previous }
