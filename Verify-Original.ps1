# Verify-Original.ps1 — original/ 冻结区漂移校验
# 用法: powershell -File Verify-Original.ps1  （或 pwsh）
# 退出码: 0 = 一致; 1 = 发现漂移(MISSING/CHANGED/EXTRA); 2 = 清单缺失
# 清单格式: "<sha256小写>  <相对仓库根的正斜杠路径>"，UTF-8。
# 白名单: CoH3/original/MANIFEST.sha256 自身不参与哈希校验（在 EXTRA 检查中亦跳过）。
param(
    [string]$Root = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'
$manifestRel = 'CoH3/original/MANIFEST.sha256'
$originalDirRel = 'CoH3/original'
$manifestPath = Join-Path $Root ($manifestRel -replace '/', '\')
$originalDir  = Join-Path $Root ($originalDirRel -replace '/', '\')

if (-not (Test-Path -LiteralPath $manifestPath)) {
    Write-Warning "manifest not found: $manifestRel"
    exit 2
}

$entries = @{}
Get-Content -LiteralPath $manifestPath -Encoding UTF8 | ForEach-Object {
    $line = $_.TrimEnd()
    if ($line) {
        $parts = $line -split '  ', 2
        if ($parts.Count -ne 2 -or -not $parts[0] -or -not $parts[1]) {
            Write-Warning "malformed manifest line: $line"
            exit 2
        }
        $entries[$parts[1]] = $parts[0].ToLowerInvariant()
    }
}

$fail = 0
foreach ($rel in ($entries.Keys | Sort-Object)) {
    $abs = Join-Path $Root ($rel -replace '/', '\')
    if (-not (Test-Path -LiteralPath $abs)) {
        Write-Host "MISSING  $rel"
        $fail++
    } else {
        $actual = (Get-FileHash -LiteralPath $abs -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne $entries[$rel]) {
            Write-Host "CHANGED  $rel"
            $fail++
        }
    }
}

$present = Get-ChildItem -LiteralPath $originalDir -Recurse -File | ForEach-Object {
    $_.FullName.Substring($Root.Length + 1).Replace('\','/')
}
foreach ($p in $present) {
    if ($p -eq $manifestRel) { continue }
    if (-not $entries.ContainsKey($p)) {
        Write-Host "EXTRA    $p"
        $fail++
    }
}

if ($fail -eq 0) {
    Write-Host ("original/ VERIFIED: {0} files, no drift" -f $entries.Count)
    exit 0
} else {
    Write-Host ("original/ DRIFT: {0} problem(s) — see MISSING/CHANGED/EXTRA above" -f $fail)
    exit 1
}
