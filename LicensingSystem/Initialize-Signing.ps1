param(
    [Parameter(Mandatory=$true)][string]$ConfigPath,
    [string]$PinFile
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$repo = Split-Path -Parent $root
$sdk = Join-Path $root '.cache\dotnet-10\dotnet.exe'
$server = Join-Path $root 'server\bin\Release\net10.0\Licensing.Server.dll'
if (-not $PinFile) { $PinFile = Join-Path $repo 'CoH3\SkyEye\client\CoH3MemoryTool\PinnedLeaseKey60.cs' }
if (-not (Test-Path -LiteralPath $server)) { throw 'Compile server/Licensing.Server.csproj first.' }
if (-not (Test-Path -LiteralPath $PinFile)) { throw 'The public-key pin source file is missing.' }
if (-not (Test-Path -LiteralPath $ConfigPath)) { throw 'Provide an existing private deployment configuration.' }
$previous = $env:LICENSING_SERVER_CONFIG
New-Item -ItemType Directory -Path (Join-Path $root 'evidence') -Force | Out-Null
$publicPath = Join-Path $root ('evidence\lease-public-key-' + [Guid]::NewGuid().ToString('N') + '.json')
try {
    $env:LICENSING_SERVER_CONFIG = (Resolve-Path -LiteralPath $ConfigPath).Path
    & $sdk $server --export-lease-public-key $publicPath
    if ($LASTEXITCODE -ne 0) { throw 'Offline public-key export failed.' }
} finally { $env:LICENSING_SERVER_CONFIG = $previous }
$key = Get-Content -LiteralPath $publicPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([Convert]::FromBase64String($key.modulus).Length -ne 384 -or $key.kid -notmatch '^[0-9a-f]{16}$') { throw 'Unexpected RSA public-key format.' }
$content = [IO.File]::ReadAllText($PinFile)
foreach ($entry in @(@('Modulus',[string]$key.modulus),@('Exponent',[string]$key.exponent),@('Kid',[string]$key.kid))) {
    $pattern = '(\bconst\s+string\s+' + $entry[0] + '\s*=\s*)"[^"]*"'
    if ([regex]::Matches($content,$pattern).Count -ne 1) { throw ('Exactly one public pin constant is required: ' + $entry[0]) }
    $content = [regex]::Replace($content,$pattern,('${1}"' + $entry[1] + '"'))
}
[IO.File]::WriteAllText($PinFile,$content,[Text.UTF8Encoding]::new($false))
Write-Output ('Pinned public key ' + $key.kid + '. Rebuild and re-protect the client before distribution.')
Write-Output 'Keep the private server key and its directory out of all client and maintenance archives.'
