$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$repo = Split-Path -Parent (Split-Path -Parent $root)
$sdk = Join-Path $repo 'LicensingSystem\.cache\dotnet-10\dotnet.exe'
if (-not (Test-Path -LiteralPath $sdk)) { throw 'Install workspace .NET SDK 10 first.' }
$relative = 'verification\SkyEye.Integration.Verification.csproj'
& $sdk build (Join-Path $root $relative) -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw ('Build failed: ' + $relative) }
