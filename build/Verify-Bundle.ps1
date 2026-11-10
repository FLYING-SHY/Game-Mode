param(
    [Parameter(Mandatory)][string]$Bundle,
    [Parameter(Mandatory)][string]$ProtectedDirectory
)
# Verifies that a .NET single-file bundle embeds the exact protected assemblies.
# Reads the bundle manifest at the end of the apphost, extracts the managed
# DLL entries, inflates them when compressed, and compares SHA256 hashes.
$ErrorActionPreference = 'Stop'
$data = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $Bundle).Path)
if ($data.Length -lt 64) { throw 'Bundle too small.' }
$stream = [IO.MemoryStream]::new($data)
$reader = [IO.BinaryReader]::new($stream)

function Read-SevenBit([byte[]]$d, [ref]$pos) {
    $result = [uint64]0; $shift = 0
    do {
        $b = $d[$pos.Value]; $pos.Value++
        $result = $result -bor ([uint64]($b -band 0x7f) -shl $shift)
        $shift += 7
    } while (($b -band 0x80) -ne 0)
    return $result
}

function Test-EntryOffset([long]$value) { $value -ge 0x400 -and $value -lt $data.Length }

# The bundler patches a 32-byte signature into the apphost; the 8 bytes that
# follow it hold the manifest offset. Search the whole file for that marker.
$signature = [byte[]](0x8b,0x12,0x02,0xb9,0x6a,0x61,0x20,0x38,0x72,0x7d,0x11,0x0c,
    0xed,0xa8,0x87,0x3c,0x8f,0x3f,0x0b,0x9d,0x46,0x62,0x1e,0x16,0x14,0x2f,0xf4,0x3a,0x6e,0x53,0x20,0x21)
$markerIndex = -1
for ($i = 0; $i -le $data.Length - 40; $i++) {
    $match = $true
    for ($j = 0; $j -lt 32; $j++) { if ($data[$i + $j] -ne $signature[$j]) { $match = $false; break } }
    if ($match) { $markerIndex = $i; break }
}
if ($markerIndex -lt 0) { throw 'Single-file bundle signature not found.' }
$headerOffset = [BitConverter]::ToInt64($data, $markerIndex + 32)
if (-not (Test-EntryOffset $headerOffset)) { throw "Manifest offset out of range: 0x$($headerOffset.ToString('X'))" }

# Layout variant A: version, bundleId, fileCount; variant B: version, fileCount, bundleId.
foreach ($variant in 'A','B') {
    $pos = [ref]([int]$headerOffset)
    try {
        $major = Read-SevenBit $data $pos; $minor = Read-SevenBit $data $pos
        if ($major -lt 1 -or $major -gt 8 -or $minor -gt 4) { continue }
        if ($variant -eq 'A') {
            $idLen = Read-SevenBit $data $pos
            if ($idLen -gt 256) { continue }
            $null = $idLen; $pos.Value += [int]$idLen
            $count = Read-SevenBit $data $pos
        } else {
            $count = Read-SevenBit $data $pos
            $idLen = Read-SevenBit $data $pos
            $pos.Value += [int]$idLen
        }
        if ($count -lt 20 -or $count -gt 5000) { continue }
        $entries = @()
        $valid = $true
        for ($i = 0; $i -lt $count; $i++) {
            if ($pos.Value + 25 -gt $data.Length) { $valid = $false; break }
            $offset = [BitConverter]::ToInt64($data, $pos.Value); $pos.Value += 8
            $size = [BitConverter]::ToInt64($data, $pos.Value); $pos.Value += 8
            $compressed = [BitConverter]::ToInt64($data, $pos.Value); $pos.Value += 8
            $type = $data[$pos.Value]; $pos.Value += 1
            $nameLen = [int](Read-SevenBit $data $pos)
            if ($nameLen -gt 512 -or $pos.Value + $nameLen -gt $data.Length) { $valid = $false; break }
            $name = [Text.Encoding]::UTF8.GetString($data, $pos.Value, $nameLen); $pos.Value += $nameLen
            $saneName = $name -match '^[A-Za-z0-9._\-/]+$'
            $saneSizes = ($size -ge 0 -and $size -lt $data.Length -and $compressed -ge 0 -and $compressed -lt $data.Length)
            if (-not (Test-EntryOffset $offset) -or -not $saneName -or -not $saneSizes -or $type -gt 5) { $valid = $false; break }
            $entries += [pscustomobject]@{ Offset = $offset; Size = $size; Compressed = $compressed; Type = $type; Name = $name }
        }
        if ($valid -and ($entries | Where-Object Name -eq 'CoH3.Licensing.Client.dll')) {
            Write-Host "Manifest parsed (variant $variant): v$major.$minor, $($entries.Count) entries."
            $results = @()
            foreach ($module in 'CoH3FowTool.dll','CoH3.Licensing.Client.dll') {
                $entry = $entries | Where-Object Name -eq $module | Select-Object -First 1
                if (-not $entry) { throw "Bundle does not contain $module." }
                $raw = New-Object byte[] $(if ($entry.Compressed -gt 0) { $entry.Compressed } else { $entry.Size })
                [Array]::Copy($data, $entry.Offset, $raw, 0, $raw.Length)
                if ($entry.Compressed -gt 0) {
                    $inflated = [IO.MemoryStream]::new()
                    $deflate = [IO.Compression.DeflateStream]::new([IO.MemoryStream]::new($raw), [IO.Compression.CompressionMode]::Decompress)
                    $deflate.CopyTo($inflated); $deflate.Dispose()
                    $payload = $inflated.ToArray()
                } else { $payload = $raw }
                $bundleSha = [BitConverter]::ToString([Security.Cryptography.SHA256]::HashData($payload)) -replace '-',''
                $protectedPath = Join-Path $ProtectedDirectory $module
                $protectedSha = (Get-FileHash -LiteralPath $protectedPath -Algorithm SHA256).Hash
                $results += [pscustomobject]@{
                    Module = $module
                    BundleSha256 = $bundleSha
                    ProtectedSha256 = $protectedSha
                    Match = ($bundleSha -eq $protectedSha)
                    Bytes = $payload.Length
                }
            }
            $results | ConvertTo-Json
            if ($results.Match -contains $false) { throw 'Bundle does not embed the protected assemblies.' }
            Write-Host 'Bundle embeds the protected assemblies.' -ForegroundColor Green
            return
        }
    } catch { continue }
}
throw 'Unable to parse the single-file bundle manifest; verify manually.'