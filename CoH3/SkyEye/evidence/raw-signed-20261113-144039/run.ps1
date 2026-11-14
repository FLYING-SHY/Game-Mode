$ErrorActionPreference = 'Stop'
$v2 = 'F:\WorkSpace\Game\CoH3\SkyEye-HardenedV2'
$copy = "$v2\artifacts\raw-signed-test\20261113-144039"
$evidence = "$v2\evidence\raw-signed-20261113-144039"
$exe = "$copy\SkyEye.Integration.Verification.exe"
$cards = 'F:\WorkSpace\Game\Licensing\.local\hardening-v2\cards-raw'
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath "$copy\天穹之眼-自有授权版-v0.60.8.exe").Hash
$results = @()
foreach ($case in @('selfcheck','ui','audio','protocol-permanent','protocol-timed','coordinator','network')) {
    $mode = if ($case.StartsWith('protocol-')) { 'protocol' } else { $case }
    $sandbox = "$v2\sandbox\raw-v2-20261113-144039-$case"
    $argsList = @('--mode',$mode,'--sandbox',$sandbox)
    if ($case -in @('protocol-permanent','protocol-timed','coordinator','network')) {
        $argsList += @('--base-url','http://127.0.0.1:47835/','--code-file',"$cards\$case.txt",'--expected-kind',$(if ($case -eq 'protocol-timed') {'timed'} else {'permanent'}))
    }
    $started = [datetime]::UtcNow
    & $exe @argsList 2>&1 | Tee-Object -FilePath "$evidence\$case.log"
    $exit = $LASTEXITCODE
    $results += @{case=$case;exitCode=$exit;elapsedMs=([datetime]::UtcNow-$started).TotalMilliseconds;sandbox="$v2\verification\sandbox\raw-v2-20261113-144039-$case"}
    @{clientSHA256=$hash;privateCopy=$copy;baseUrl='http://127.0.0.1:47835/';sandboxJunction="$v2\sandbox";sandboxTarget="$v2\verification\sandbox";results=$results} | ConvertTo-Json -Depth 6 | Set-Content "$evidence\summary.json"
    if ($exit -ne 0) { Write-Output "CASE_FAILED=$case"; exit $exit }
}
Write-Output "ALL_SEVEN_CASES_PASS CLIENT_SHA256=$hash"
