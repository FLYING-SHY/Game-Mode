param([Parameter(Mandatory = $true)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$ExpectedRunnerSha)
$ErrorActionPreference = 'Stop'
$repo = 'F:\WorkSpace\Game'
$evidence = Join-Path $repo 'CoH3\dev\SkyEye\evidence\version-adaptation'
$airRoot = Join-Path $evidence 'air'
$gameInput = Join-Path $evidence 'game_input.py'
$pythonExe = 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe'
$runnerExe = Join-Path $repo 'CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe'
$output = Join-Path $evidence 'maintenance-run-28-camera-recovery'
$identityPath = Join-Path $airRoot 'run28-driver-identity.json'
$resultPath = Join-Path $airRoot 'run28-driver-result.json'
$stdoutPath = Join-Path $airRoot 'run28-stdout.txt'
$stderrPath = Join-Path $airRoot 'run28-stderr.txt'
foreach ($path in @($output, $identityPath, $resultPath, $stdoutPath, $stderrPath)) {
    if (Test-Path -LiteralPath $path) { throw "Exclusive evidence path already exists: $path" }
}
$otherReaders = @(Get-Process | Where-Object { $_.ProcessName -like 'SkyEye.Maintenance*' -or $_.ProcessName -eq 'COH3工具' -or $_.ProcessName -like '天穹之眼*' })
if ($otherReaders.Count -ne 0) { throw 'Another selected-product UI or diagnostic reader is already running.' }
$game = Get-Process -Id 31572
if ($game.ProcessName -ne 'RelicCoH3' -or $game.StartTime.ToUniversalTime().ToString('O') -ne '2026-11-20T18:44:29.8492330Z') { throw 'Selected game identity changed.' }
$gameSha = (Get-FileHash -Algorithm SHA256 -LiteralPath $game.MainModule.FileName).Hash
if ($gameSha -ne '8A3445292255D652C8DBE9B53F24328AF159C161BE89836BA54EF070946DD3EC') { throw 'Selected game binary changed.' }
$runnerSha = (Get-FileHash -Algorithm SHA256 -LiteralPath $runnerExe).Hash
if ($runnerSha -ne $ExpectedRunnerSha) { throw 'Reviewed runner binary changed.' }
$arguments = @('--out', $output, '--wait-seconds', '120', '--event-seconds', '60', '--air-events', '--recovery-probe', '--event-prewarm-delay-ms', '1000')
[ordered]@{
    startedUtc = [datetime]::UtcNow.ToString('O'); gamePid = $game.Id; gameStartUtc = $game.StartTime.ToUniversalTime().ToString('O')
    gameExe = $game.MainModule.FileName; gameSha256 = $gameSha; runner = $runnerExe; runnerSha256 = $runnerSha; arguments = $arguments
    nativePrecondition = 'Game paused12:15 after run27; no other reader active.'
    purpose = 'Observe normal camera updates during native minimap pans. Recovery is recorded only after a naturally occurring reader camera exception; no injected exception or memory write.'
} | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath $identityPath -Encoding utf8
$child = $null; $resumed = $false
$record = [ordered]@{ status = 'starting'; runnerPid = $null; childExitCode = $null; childExitCodeRecorded = $false; childKilledAtTimeout = $false; resumedUtc = $null; pausedUtc = $null; pauseExitCode = $null; nativePans = @(); finishedUtc = $null; error = $null }
try {
    $child = Start-Process -FilePath $runnerExe -ArgumentList $arguments -PassThru -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath
    $retainedProcessHandle = $child.Handle
    $record.runnerPid = $child.Id
    $warmup = [diagnostics.stopwatch]::StartNew(); $ready = $false
    while ($warmup.ElapsedMilliseconds -lt 60000) {
        $child.Refresh()
        if ($child.HasExited) { throw 'Runner exited before the explicit prewarm marker.' }
        if ((Test-Path -LiteralPath $stdoutPath) -and (Select-String -LiteralPath $stdoutPath -SimpleMatch 'event_prewarm_delay started milliseconds=1000' -Quiet)) { $ready = $true; break }
        Start-Sleep -Milliseconds 50
    }
    if (-not $ready) { throw 'No prewarm marker within60000ms; game not resumed.' }
    & $pythonExe $gameInput --pid 31572 --key 32 --out (Join-Path $airRoot 'run28-native-resume.png')
    if ($LASTEXITCODE -ne 0) { throw 'Foreground-checked native resume failed.' }
    $resumed = $true; $record.resumedUtc = [datetime]::UtcNow.ToString('O')
    $points = @(@(140,900), @(260,930), @(220,800), @(90,820), @(140,900), @(260,930), @(220,800), @(90,820))
    for ($i = 0; $i -lt $points.Count; $i++) {
        Start-Sleep -Milliseconds 900
        $child.Refresh()
        if ($child.HasExited) { break }
        $capture = Join-Path $airRoot ('run28-native-pan-{0:00}.png' -f ($i + 1))
        & $pythonExe $gameInput --pid 31572 --left-click $points[$i][0] $points[$i][1] --out $capture
        if ($LASTEXITCODE -ne 0) { throw 'Foreground-checked native minimap pan failed.' }
        $record.nativePans += [ordered]@{ ordinal = $i + 1; clientX = $points[$i][0]; clientY = $points[$i][1]; finishedUtc = [datetime]::UtcNow.ToString('O'); capture = $capture }
    }
    if (-not $child.WaitForExit(60000)) { $record.childKilledAtTimeout = $true; throw 'Owned diagnostic runner exceeded final60000ms wait.' }
    $child.Refresh()
    if ($null -ne $child.ExitCode) { $record.childExitCode = $child.ExitCode; $record.childExitCodeRecorded = $true }
    $record.status = 'completed_driver_only_consult_reader_report'
} catch {
    $record.status = 'driver_failure'; $record.error = $_.Exception.ToString(); throw
} finally {
    if ($resumed) {
        & $pythonExe $gameInput --pid 31572 --key 32 --out (Join-Path $airRoot 'run28-native-paused.png')
        $record.pauseExitCode = $LASTEXITCODE
        if ($LASTEXITCODE -eq 0) { $record.pausedUtc = [datetime]::UtcNow.ToString('O') }
    }
    if ($null -ne $child) {
        $child.Refresh()
        if (-not $child.HasExited) {
            $owned = Get-Process -Id $child.Id -ErrorAction SilentlyContinue
            if ($null -ne $owned -and $owned.StartTime.ToUniversalTime() -eq $child.StartTime.ToUniversalTime()) {
                Stop-Process -Id $child.Id; $record.childKilledAtTimeout = $true; $child.WaitForExit(5000) | Out-Null
            }
        }
        if ($child.HasExited -and $null -ne $child.ExitCode) { $record.childExitCode = $child.ExitCode; $record.childExitCodeRecorded = $true }
    }
    $record.finishedUtc = [datetime]::UtcNow.ToString('O')
    $record.runnerSha256After = (Get-FileHash -Algorithm SHA256 -LiteralPath $runnerExe).Hash
    $record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $resultPath -Encoding utf8
    $record | ConvertTo-Json -Depth 8
}
