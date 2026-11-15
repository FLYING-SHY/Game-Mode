param([string]$BaseUrl='http://127.0.0.1:47835/', [string]$PrivateCodeDirectory, [switch]$LocalOnly)
$ErrorActionPreference='Stop'
$root=$PSScriptRoot
$verifier=Join-Path $root 'verification\bin\Protected\net48\SkyEye.Integration.Verification.exe'
if (-not (Test-Path -LiteralPath $verifier)) { throw 'Run Build-Protected.ps1 first.' }
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$cases=@(@{Mode='selfcheck';Name='selfcheck'},@{Mode='ui';Name='ui'},@{Mode='audio';Name='audio'})
if (-not $LocalOnly) {
    if (-not (Test-Path -LiteralPath $PrivateCodeDirectory -PathType Container)) { throw 'Provide a private directory of fresh isolated test cards.' }
    $cases+=@(@{Mode='protocol';Name='protocol-permanent';Kind='permanent'},@{Mode='protocol';Name='protocol-timed';Kind='timed'},@{Mode='coordinator';Name='coordinator';Kind='permanent'},@{Mode='network';Name='network';Kind='permanent'})
}
$rows=@()
foreach ($case in $cases) {
    $sandbox=Join-Path $root ('verification\sandbox\protected-' + $case.Name + '-' + $stamp)
    $arguments=@('--mode',$case.Mode,'--sandbox',$sandbox,'--base-url',$BaseUrl)
    if ($case.Kind) { $arguments+=@('--code-file',(Join-Path $PrivateCodeDirectory ($case.Name+'.txt')),'--expected-kind',$case.Kind) }
    $log=Join-Path $root ('evidence\protected-' + $case.Name + '-' + $stamp + '.log')
    & $verifier @arguments 2>&1 | Tee-Object -FilePath $log
    $rc=$LASTEXITCODE
    $rows+=@{Mode=$case.Mode;Name=$case.Name;ExitCode=$rc;Result=(Join-Path $sandbox 'result.json');Log=$log}
    if ($rc -ne 0) { throw ('Protected verification failed: '+$case.Name) }
}
$rows | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 (Join-Path $root ('evidence\protected-verification-'+$stamp+'.json'))
Write-Output 'All requested protected-client checks passed.'
