param(
    [Parameter(Mandatory=$true)][string]$OutputFile,
    [Parameter(Mandatory=$true)][string]$DataDirectory,
    [string]$ListenUrl = 'http://127.0.0.1:47835',
    [string[]]$AllowedHosts = @('127.0.0.1','localhost','::1'),
    [string[]]$TrustedProxies = @(),
    [string]$ProductId = 'CoH3FowTool',
    [string]$ProductName = 'CoH3 SkyEye'
)
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath $OutputFile) { throw 'The configuration already exists; preserve its administrator token and key paths.' }
if ($DataDirectory -notmatch '^(?:[A-Za-z]:[\\/]|/|\\\\)') { throw 'DataDirectory must be an absolute path on the destination server.' }
$listen = $null
if (-not [Uri]::TryCreate($ListenUrl, [UriKind]::Absolute, [ref]$listen) -or $listen.Scheme -notin @('http','https')) { throw 'ListenUrl must be an absolute HTTP or HTTPS address.' }
$address = $null
if (-not [Net.IPAddress]::TryParse($listen.Host.Trim('[',']'), [ref]$address)) { throw 'ListenUrl must use an explicit listening IP address.' }
foreach ($hostName in $AllowedHosts) {
    if ([string]::IsNullOrWhiteSpace($hostName) -or $hostName.Contains('*')) { throw 'AllowedHosts must contain explicit host names.' }
}
foreach ($proxy in $TrustedProxies) {
    $proxyAddress = $null
    if (-not [Net.IPAddress]::TryParse($proxy, [ref]$proxyAddress)) { throw 'TrustedProxies must contain explicit IP addresses.' }
}
$random = [Security.Cryptography.RandomNumberGenerator]::Create()
try { $bytes = New-Object byte[] 32; $random.GetBytes($bytes) } finally { $random.Dispose() }
$adminToken = [BitConverter]::ToString($bytes).Replace('-','').ToLowerInvariant()
$data = $DataDirectory.TrimEnd([char[]]'\/')
$config = [ordered]@{
    ListenUrl = $ListenUrl
    DatabasePath = $data + '/leases.db'
    KeyDirectory = $data + '/keys'
    AdminToken = $adminToken
    ProductId = $ProductId
    ProductName = $ProductName
    CodePrefix = 'COH3'
    MinimumVersion = '0.60.8'
    LatestVersion = '0.60.8'
    DownloadUrl = ''
    VersionNotes = ''
    AllowedHosts = $AllowedHosts
    TrustedProxies = $TrustedProxies
    ForwardLimit = 1
}
$absoluteOutput = [IO.Path]::GetFullPath($OutputFile)
New-Item -ItemType Directory -Path (Split-Path -Parent $absoluteOutput) -Force | Out-Null
$config | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $absoluteOutput -Encoding UTF8
Write-Output ('Created private server configuration: ' + $absoluteOutput)
