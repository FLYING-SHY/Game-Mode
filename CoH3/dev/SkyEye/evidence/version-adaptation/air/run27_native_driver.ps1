param([Parameter(Mandatory = $true)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$ExpectedRunnerSha)
$ErrorActionPreference = 'Stop'
$repo = 'F:\WorkSpace\Game'
$airRoot = Join-Path $repo 'CoH3\dev\SkyEye\evidence\version-adaptation\air'
$gameInput = Join-Path $repo 'CoH3\dev\SkyEye\evidence\version-adaptation\game_input.py'
$pythonExe = 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe'
$runnerExe = Join-Path $repo 'CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe'
$output = Join-Path $repo 'CoH3\dev\SkyEye\evidence\version-adaptation\maintenance-run-27-p47-compact-flight'
$identityPath = Join-Path $airRoot 'run27-driver-identity.json'
$resultPath = Join-Path $airRoot 'run27-driver-result.json'
$stdoutPath = Join-Path $airRoot 'run27-stdout.txt'
$stderrPath = Join-Path $airRoot 'run27-stderr.txt'
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
$coreHashes = foreach ($name in @('AirLogic.cs', 'PlayerInfoReader.cs')) {
    $sourcePath = Join-Path $repo "CoH3\dev\SkyEye\client\CoH3MemoryTool\$name"
    $mirrorPath = Join-Path $repo "CoH3\dev\COH3SimpleTool\CoH3MemoryTool\$name"
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $sourcePath).Hash
    if ($hash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $mirrorPath).Hash) { throw 'Engine mirror drifted.' }
    [ordered]@{ source = $sourcePath; mirror = $mirrorPath; sha256 = $hash }
}
$arguments = @('--out', $output, '--wait-seconds', '120', '--event-seconds', '20', '--air-events', '--recovery-probe', '--event-prewarm-delay-ms', '4000')
$identity = [ordered]@{
    startedUtc = [datetime]::UtcNow.ToString('O'); gamePid = $game.Id; gameStartUtc = $game.StartTime.ToUniversalTime().ToString('O')
    gameExe = $game.MainModule.FileName; gameSha256 = $gameSha; moduleBase = $game.MainModule.BaseAddress.ToInt64(); moduleSize = $game.MainModule.ModuleMemorySize
    runner = $runnerExe; runnerSha256 = $runnerSha; arguments = $arguments; coreHashes = @($coreHashes)
    nativePrecondition = 'Game paused12:04 with accepted P47 order; native ammo504->454 and target capsule recorded separately. No UI/other reader active.'
    purpose = 'Existing reader initial air scan,4000ms prewarm gap while native cast advances, then actual ReadAirZones newly sees the plane. No cast decoder bypass or injected event.'
}
$identity | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $identityPath -Encoding utf8
$child = $null; $resumed = $false
$record = [ordered]@{ status = 'starting'; runnerPid = $null; childExitCode = $null; childExitCodeRecorded = $false; childKilledAtTimeout = $false; resumedUtc = $null; pausedUtc = $null; pauseExitCode = $null; finishedUtc = $null; error = $null }
try {
    $child = Start-Process -FilePath $runnerExe -ArgumentList $arguments -PassThru -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath
    $retainedProcessHandle = $child.Handle
    $record.runnerPid = $child.Id
    $warmup = [diagnostics.stopwatch]::StartNew()
    $ready = $false
    while ($warmup.ElapsedMilliseconds -lt 60000) {
        $child.Refresh()
        if ($child.HasExited) { throw 'Runner exited before the explicit prewarm marker.' }
        if ((Test-Path -LiteralPath $stdoutPath) -and (Select-String -LiteralPath $stdoutPath -SimpleMatch 'event_prewarm_delay started milliseconds=4000' -Quiet)) { $ready = $true; break }
        Start-Sleep -Milliseconds 50
    }
    if (-not $ready) { throw 'No prewarm marker within60000ms; game not resumed.' }
    & $pythonExe $gameInput --pid 31572 --key 32 --out (Join-Path $airRoot 'run27-native-resume.png')
    if ($LASTEXITCODE -ne 0) { throw 'Foreground-checked native resume failed.' }
    $resumed = $true; $record.resumedUtc = [datetime]::UtcNow.ToString('O')
    Start-Sleep -Seconds 5
    & $pythonExe (Join-Path $airRoot 'air_raw_snapshot.py') --pid 31572 --out (Join-Path $airRoot 'live-raw-11-p47-compact-middle.json')
    if ($LASTEXITCODE -ne 0) { throw 'Middle raw observation failed.' }
    Start-Sleep -Seconds 3
    & $pythonExe (Join-Path $airRoot 'air_raw_snapshot.py') --pid 31572 --out (Join-Path $airRoot 'live-raw-12-p47-compact-late.json')
    if ($LASTEXITCODE -ne 0) { throw 'Late raw observation failed.' }
    Start-Sleep -Seconds 2
    & $pythonExe $gameInput --pid 31572 --key 32 --out (Join-Path $airRoot 'run27-native-paused.png')
    $record.pauseExitCode = $LASTEXITCODE
    if ($LASTEXITCODE -ne 0) { throw 'Foreground-checked native pause failed.' }
    $resumed = $false; $record.pausedUtc = [datetime]::UtcNow.ToString('O')
    if (-not $child.WaitForExit(40000)) { $record.childKilledAtTimeout = $true; throw 'Owned diagnostic runner exceeded final40000ms wait.' }
    $child.Refresh()
    if ($null -ne $child.ExitCode) { $record.childExitCode = $child.ExitCode; $record.childExitCodeRecorded = $true }
    $record.status = 'completed_driver_only_consult_reader_report'
} catch {
    $record.status = 'driver_failure'
    $record.error = $_.Exception.ToString()
    throw
} finally {
    if ($resumed) {
        & $pythonExe $gameInput --pid 31572 --key 32 --out (Join-Path $airRoot 'run27-native-finally-paused.png')
        $record.pauseExitCode = $LASTEXITCODE
        if ($LASTEXITCODE -eq 0) { $record.pausedUtc = [datetime]::UtcNow.ToString('O') }
    }
    if ($null -ne $child) {
        $child.Refresh()
        if (-not $child.HasExited) {
            $currentChild = Get-Process -Id $child.Id -ErrorAction SilentlyContinue
            if ($null -ne $currentChild -and $currentChild.StartTime.ToUniversalTime() -eq $child.StartTime.ToUniversalTime()) {
                Stop-Process -Id $child.Id
                $record.childKilledAtTimeout = $true
                $child.WaitForExit(5000) | Out-Null
            }
        }
        if ($child.HasExited -and $null -ne $child.ExitCode) { $record.childExitCode = $child.ExitCode; $record.childExitCodeRecorded = $true }
    }
    $record.finishedUtc = [datetime]::UtcNow.ToString('O')
    $record.runnerSha256After = (Get-FileHash -Algorithm SHA256 -LiteralPath $runnerExe).Hash
    $record | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath $resultPath -Encoding utf8
    $record | ConvertTo-Json -Depth 7
}
