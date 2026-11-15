param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$cache = Join-Path $Root '.cache'
$install = Join-Path $cache 'dotnet-10'
$dotnet = Join-Path $install 'dotnet.exe'
if (Test-Path -LiteralPath $dotnet) {
    $version = & $dotnet --version
    if ($LASTEXITCODE -eq 0 -and $version -match '^10\.') { Write-Output $dotnet; exit 0 }
    throw 'The workspace SDK directory exists but is not a usable .NET 10 SDK.'
}
if (Test-Path -LiteralPath $install) { throw 'SDK installation directory already exists; inspect it before retrying.' }
New-Item -ItemType Directory -Path $cache -Force | Out-Null
$metadataUrl = 'https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json'
$metadata = Invoke-RestMethod -Uri $metadataUrl -TimeoutSec 60
$sdk = $metadata.releases[0].sdk
$sdkFile = $sdk.files | Where-Object { $_.rid -eq 'win-x64' -and $_.name.EndsWith('.zip') } | Select-Object -First 1
if (-not $sdkFile -or $sdk.version -notmatch '^10\.') { throw 'Official .NET 10 metadata did not contain a Windows x64 SDK.' }
$cachedArchive = Join-Path $cache ('dotnet-sdk-' + $sdk.version + '-win-x64.zip')
$archive = $cachedArchive
$downloadUrl = 'https://dotnetcli.blob.core.windows.net/dotnet/Sdk/' + $sdk.version + '/dotnet-sdk-' + $sdk.version + '-win-x64.zip'
$verified = (Test-Path -LiteralPath $archive) -and (Get-FileHash -LiteralPath $archive -Algorithm SHA512).Hash -eq $sdkFile.hash
if (-not $verified) {
    # Azure Blob supports x-ms-range. Keep the prefix untouched until the combined archive is verified.
    $prefixLength = if (Test-Path -LiteralPath $cachedArchive) { (Get-Item -LiteralPath $cachedArchive).Length } else { 0 }
    $transferId = [guid]::NewGuid().ToString('N')
    $download = $cachedArchive + '.' + $transferId + '.download'
    $archive = $cachedArchive + '.' + $transferId + '.complete.zip'
    Add-Type -AssemblyName System.Net.Http
    $client = [System.Net.Http.HttpClient]::new()
    $client.Timeout = [TimeSpan]::FromMinutes(10)
    $cancellation = [System.Threading.CancellationTokenSource]::new()
    $cancellation.CancelAfter([TimeSpan]::FromMinutes(10))
    $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Get, $downloadUrl)
    if ($prefixLength -gt 0) {
        $headRequest = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Head, $downloadUrl)
        $headResponse = $null
        try {
            $headResponse = $client.SendAsync($headRequest, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead, $cancellation.Token).GetAwaiter().GetResult()
            $headResponse.EnsureSuccessStatusCode() | Out-Null
            $totalLength = $headResponse.Content.Headers.ContentLength
            if ($null -eq $totalLength -or $prefixLength -ge $totalLength) { throw 'The saved SDK prefix has no valid remaining byte range.' }
            $request.Headers.TryAddWithoutValidation('x-ms-range', ('bytes=' + $prefixLength + '-' + ($totalLength - 1))) | Out-Null
        } finally {
            if ($headResponse) { $headResponse.Dispose() }
            $headRequest.Dispose()
        }
    }
    $response = $null
    $transferFile = $null
    try {
        $response = $client.SendAsync($request, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead, $cancellation.Token).GetAwaiter().GetResult()
        $response.EnsureSuccessStatusCode() | Out-Null
        if ($prefixLength -gt 0 -and ([int]$response.StatusCode -ne 206 -or $response.Content.Headers.ContentRange.From -ne $prefixLength)) {
            throw 'The official server did not confirm the requested archive range; the saved prefix was preserved.'
        }
        $transferFile = [System.IO.File]::Open($download, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write)
        $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
        $stream.CopyToAsync($transferFile, 81920, $cancellation.Token).GetAwaiter().GetResult() | Out-Null
    } finally {
        if ($transferFile) { $transferFile.Dispose() }
        if ($response) { $response.Dispose() }
        $request.Dispose(); $client.Dispose(); $cancellation.Dispose()
    }
    $combined = [System.IO.File]::Open($archive, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write)
    try {
        if ($prefixLength -gt 0) {
            $prefix = [System.IO.File]::OpenRead($cachedArchive)
            try { $prefix.CopyTo($combined) } finally { $prefix.Dispose() }
        }
        $tail = [System.IO.File]::OpenRead($download)
        try { $tail.CopyTo($combined) } finally { $tail.Dispose() }
    } finally { $combined.Dispose() }
}
$actual = (Get-FileHash -LiteralPath $archive -Algorithm SHA512).Hash
if ($actual -ne $sdkFile.hash) { throw 'The downloaded SDK failed official SHA512 verification; source files were preserved.' }
Expand-Archive -LiteralPath $archive -DestinationPath $install
$actualVersion = & $dotnet --version
if ($LASTEXITCODE -ne 0 -or $actualVersion -ne $sdk.version) { throw 'Installed SDK version does not match official metadata.' }
[pscustomobject]@{
    InstalledAt = (Get-Date).ToUniversalTime().ToString('o')
    MetadataUrl = $metadataUrl; DownloadUrl = $downloadUrl; ArchivePath = $archive
    Version = $actualVersion; Sha512 = $actual; SdkPath = $dotnet; Verified = $true
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $cache 'dotnet-10-installation.json') -Encoding UTF8
Write-Output $dotnet
