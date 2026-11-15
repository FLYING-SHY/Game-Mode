$ErrorActionPreference='Stop'
$exe=Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'delivery\client') -Filter '*.exe' -File | Select-Object -First 1
$process=Start-Process -FilePath $exe.FullName -WorkingDirectory $exe.DirectoryName -PassThru
$record=[ordered]@{Exe=$exe.FullName;Sha256=(Get-FileHash -LiteralPath $exe.FullName).Hash;Pid=$process.Id;Started=(Get-Date).ToString('o');WindowCaptured=$false;Closed=$false}
try {
    $deadline=(Get-Date).AddSeconds(15)
    do { $process.Refresh();if($process.HasExited){throw 'Protected client exited before showing its window.'};if($process.MainWindowHandle -ne 0){break};Start-Sleep -Milliseconds 100 } while((Get-Date) -lt $deadline)
    if($process.MainWindowHandle -eq 0){throw 'Protected client did not show a window within 15 seconds.'}
    $capture=Join-Path $PSScriptRoot 'scripts\Capture-Window.ps1'
    & $capture -TargetProcessId $process.Id -OutputPrefix (Join-Path $PSScriptRoot 'evidence\protected-real-entry')
    $record.WindowCaptured=$true
} finally {
    $process.Refresh()
    if(-not $process.HasExited){$null=$process.CloseMainWindow();$record.Closed=$process.WaitForExit(10000)}else{$record.Closed=$true}
    $record | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 (Join-Path $PSScriptRoot 'evidence\protected-real-entry-lifecycle.json')
}
if(-not $record.Closed){throw 'The protected client window did not close normally.'}
