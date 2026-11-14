param([switch]$VerifyServer)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$sdk = Join-Path $root '.cache\dotnet-10\dotnet.exe'
if (-not (Test-Path -LiteralPath $sdk)) { throw 'Install workspace .NET SDK 10 first.' }
foreach ($relative in @('server\Licensing.Server.csproj','server-tests\Licensing.Tests.csproj')) {
    & $sdk build (Join-Path $root $relative) -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw ('Build failed: ' + $relative) }
}
if ($VerifyServer) {
    & $sdk (Join-Path $root 'server-tests\bin\Release\net10.0\Licensing.Tests.dll')
    if ($LASTEXITCODE -ne 0) { throw 'Signed server regression failed.' }
}
