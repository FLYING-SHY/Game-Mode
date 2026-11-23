# Verify-Original.ps1 — original/ 冻结区漂移校验（所有游戏）
# 用法: powershell -File Verify-Original.ps1  （或 pwsh）
# 退出码: 0 = 全部一致; 1 = 发现漂移(MISSING/CHANGED/EXTRA); 2 = 清单缺失或格式错误
# 清单格式: "<sha256小写>  <相对仓库根的正斜杠路径>"，UTF-8。
# 每个 <Game>/original/MANIFEST.sha256 自身不参与哈希校验（在 EXTRA 检查中亦跳过）。
param(
    [string]$Root = $PSScriptRoot,
    [string[]]$Game = @()
)

$ErrorActionPreference = 'Stop'

# 找出所有 <Game>/original 冻结区（只认仓库根下的一层游戏目录）。
$games = @()
Get-ChildItem -LiteralPath $Root -Directory | Sort-Object Name | ForEach-Object {
    $originalDir = Join-Path $_.FullName 'original'
    if (Test-Path -LiteralPath $originalDir -PathType Container) {
        $manifest = Join-Path $originalDir 'MANIFEST.sha256'
        if (Test-Path -LiteralPath $manifest -PathType Leaf) {
            $games += [pscustomobject]@{
                Name = $_.Name
                ManifestRel = "$($_.Name)/original/MANIFEST.sha256"
                ManifestPath = $manifest
                OriginalDir = $originalDir
            }
        }
    }
}

if ($Game.Count -gt 0) {
    $games = @($games | Where-Object { $Game -contains $_.Name })
    if ($games.Count -eq 0) {
        Write-Warning "no frozen original/ zone found for: $($Game -join ', ')"
        exit 2
    }
}

if ($games.Count -eq 0) {
    Write-Warning "no <Game>/original/MANIFEST.sha256 found under $Root"
    exit 2
}

$fail = 0
$verified = 0
$totalFiles = 0

foreach ($zone in $games) {
    $entries = @{}
    Get-Content -LiteralPath $zone.ManifestPath -Encoding UTF8 | ForEach-Object {
        $line = $_.TrimEnd()
        if ($line) {
            $parts = $line -split '  ', 2
            if ($parts.Count -ne 2 -or -not $parts[0] -or -not $parts[1]) {
                Write-Warning "malformed manifest line in $($zone.ManifestRel): $line"
                exit 2
            }
            $entries[$parts[1]] = $parts[0].ToLowerInvariant()
        }
    }

    $zoneFail = 0
    foreach ($rel in ($entries.Keys | Sort-Object)) {
        $abs = Join-Path $Root ($rel -replace '/', '\')
        if (-not (Test-Path -LiteralPath $abs)) {
            Write-Host "MISSING  $rel"
            $zoneFail++
        } else {
            $actual = (Get-FileHash -LiteralPath $abs -Algorithm SHA256).Hash.ToLowerInvariant()
            if ($actual -ne $entries[$rel]) {
                Write-Host "CHANGED  $rel"
                $zoneFail++
            }
        }
    }

    $present = Get-ChildItem -LiteralPath $zone.OriginalDir -Recurse -File | ForEach-Object {
        $_.FullName.Substring($Root.Length + 1).Replace('\','/')
    }
    foreach ($p in $present) {
        if ($p -eq $zone.ManifestRel) { continue }
        if (-not $entries.ContainsKey($p)) {
            Write-Host "EXTRA    $p"
            $zoneFail++
        }
    }

    if ($zoneFail -eq 0) {
        Write-Host ("original/ VERIFIED [{0}]: {1} files, no drift" -f $zone.Name, $entries.Count)
        $verified++
        $totalFiles += $entries.Count
    } else {
        Write-Host ("original/ DRIFT [{0}]: {1} problem(s) — see MISSING/CHANGED/EXTRA above" -f $zone.Name, $zoneFail)
        $fail++
    }
}

if ($fail -eq 0) {
    Write-Host ("original/ VERIFIED: {0} zone(s), {1} file(s), no drift" -f $verified, $totalFiles)
    exit 0
} else {
    exit 1
}
