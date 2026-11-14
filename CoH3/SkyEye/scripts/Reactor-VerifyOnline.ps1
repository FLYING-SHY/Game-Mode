$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$target=Join-Path $root 'verification\bin\ReactorTrial\net48'
$ev=Join-Path $root 'evidence\reactor-client-trial'
$cards='F:\WorkSpace\Game\LicensingSystem\.local\hardening-v2\cards-reactor-trial'
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$runEvidence=Join-Path $ev ('online-'+$stamp)
New-Item -ItemType Directory -Force -Path $runEvidence | Out-Null
$cases=@(
 @{Name='protocol-permanent';Mode='protocol';Kind='permanent';TimeoutMs=30000},
 @{Name='protocol-timed';Mode='protocol';Kind='timed';TimeoutMs=30000},
 @{Name='coordinator';Mode='coordinator';Kind='permanent';TimeoutMs=120000},
 @{Name='network';Mode='network';Kind='permanent';TimeoutMs=120000}
)
$jobs=@()
foreach($case in $cases) {
 $sandbox=Join-Path $root ('verification\sandbox\reactor-online-'+$stamp+'-'+$case.Name)
 $card=Join-Path $cards ($case.Name+'.txt')
 if(-not(Test-Path -LiteralPath $card)){throw 'Missing dedicated online fixture file.'}
 $info=New-Object Diagnostics.ProcessStartInfo
 $info.FileName=Join-Path $target 'SkyEye.Integration.Verification.exe'
 $info.Arguments='--mode '+$case.Mode+' --base-url http://127.0.0.1:47835/ --expected-kind '+$case.Kind+' --timeout-seconds 110 --sandbox "'+$sandbox+'" --code-file "'+$card+'"'
 $info.WorkingDirectory=$target
 $info.UseShellExecute=$false
 $info.RedirectStandardOutput=$true
 $info.RedirectStandardError=$true
 $proc=New-Object Diagnostics.Process
 $proc.StartInfo=$info
 $null=$proc.Start()
 $jobs+=@{Case=$case;Process=$proc;Start=Get-Date;Sandbox=$sandbox;Card=$card;OutTask=$proc.StandardOutput.ReadToEndAsync();ErrTask=$proc.StandardError.ReadToEndAsync()}
}
$rows=@()
foreach($item in $jobs) {
 $proc=$item.Process
 $remaining=[Math]::Max(0,$item.Case.TimeoutMs-((Get-Date)-$item.Start).TotalMilliseconds)
 $exited=$proc.WaitForExit([int]$remaining)
 $window=$null
 if(-not $exited){$proc.Refresh();$window=$proc.MainWindowTitle;Stop-Process -Id $proc.Id -Force;$proc.WaitForExit(5000)|Out-Null}
 $stdout=$item.OutTask.GetAwaiter().GetResult()
 $stderr=$item.ErrTask.GetAwaiter().GetResult()
 # Read only this explicitly authorized new fixture file to prevent secret persistence in evidence.
 $secret=[IO.File]::ReadAllText($item.Card).Trim()
 if($secret.Length -gt 0){$stdout=$stdout.Replace($secret,'[REDACTED]');$stderr=$stderr.Replace($secret,'[REDACTED]')}
 $outPath=Join-Path $runEvidence ($item.Case.Name+'-stdout.log')
 $errPath=Join-Path $runEvidence ($item.Case.Name+'-stderr.log')
 [IO.File]::WriteAllText($outPath,$stdout,[Text.UTF8Encoding]::new($false))
 [IO.File]::WriteAllText($errPath,$stderr,[Text.UTF8Encoding]::new($false))
 $report=$null
 $reportPath=Join-Path $item.Sandbox 'result.json'
 if(Test-Path -LiteralPath $reportPath){
  $text=[IO.File]::ReadAllText($reportPath)
  if($secret.Length -gt 0){$text=$text.Replace($secret,'[REDACTED]')}
  [IO.File]::WriteAllText((Join-Path $runEvidence ($item.Case.Name+'-result.json')),$text,[Text.UTF8Encoding]::new($false))
  $report=$text|ConvertFrom-Json
 }
 $rows+=[ordered]@{Case=$item.Case.Name;PID=$proc.Id;TimeoutMs=$item.Case.TimeoutMs;TimedOut=(-not $exited);ExitCode=$proc.ExitCode;ElapsedMs=$report.elapsedMs;VerifierPassed=$report.passed;Passed=($exited -and $proc.ExitCode -eq 0 -and $report.passed);WindowAtTimeout=$window;Sandbox=$item.Sandbox;Stdout=$outPath;Stderr=$errPath;LogoutReplyObserved=($stdout -match 'REPLY.*api/v1/logout');CleanupControlFlowCompleted=($exited -and $proc.ExitCode -eq 0 -and $report.passed);BaseUrl='http://127.0.0.1:47835/'}
}
$rows|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $runEvidence 'results.json') -Encoding UTF8
$rows|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $ev 'online-results.json') -Encoding UTF8
$rows|ConvertTo-Json -Depth 5
