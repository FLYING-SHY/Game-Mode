[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$RawAssembly,
    [ValidateSet('Trial','Production')][string]$Mode = 'Trial'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$toolRoot = Join-Path $root 'tools\reactor'
$cli = Join-Path $toolRoot 'extracted\{app}\dotNET_Reactor.Console.exe'
$project = Join-Path $toolRoot 'selective-no-rename.nrproj'
$inputFile = (Resolve-Path -LiteralPath $RawAssembly).Path
if ([IO.Path]::GetExtension($inputFile) -notin @('.dll','.exe')) { throw 'Expected a managed raw .dll or .exe.' }
if ($inputFile -match '(?i)(obfuscar|[\\/]reactor[\\/](trial|production)[\\/])') { throw 'Use the raw compiler output, before any obfuscator.' }
if (-not (Test-Path -LiteralPath $cli)) { throw "CLI missing: $cli" }
$stamp = (Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '-' + [guid]::NewGuid().ToString('N').Substring(0,8)
$destination = Join-Path $toolRoot ($Mode.ToLowerInvariant() + '\' + $stamp)
$evidence = Join-Path $root ('evidence\reactor-runs\' + $stamp)
New-Item -ItemType Directory -Path $destination,$evidence -Force | Out-Null
$outputFile = Join-Path $destination ([IO.Path]::GetFileName($inputFile))
$reactorArgs = @('-project',$project,'-file',$inputFile,'-targetfile',$outputFile,
    '-nodialog','-verbose','-obfuscation','0','-internalization','0',
    '-declarative_protection','1','-virtualization','1','-virtualization_protect_other_features','0',
    '-control_flow','0','-hide_calls','0','-anti_debug','0','-antitamp','0',
    '-suppressildasm','0','-necrobit','0','-nativeexe','0','-compression','0','-stringencryption','1')
if ($Mode -eq 'Production') { $reactorArgs += '-licensed' }
$inputHash = (Get-FileHash -LiteralPath $inputFile -Algorithm SHA256).Hash
$logPath = Join-Path $evidence 'cli.log'
# Capture output without turning native stderr into a terminating PowerShell error.
$priorPreference = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& $cli @reactorArgs > $logPath 2>&1
$cliExit = $LASTEXITCODE
$ErrorActionPreference = $priorPreference
$outputHash = $null
if (Test-Path -LiteralPath $outputFile) { $outputHash = (Get-FileHash -LiteralPath $outputFile -Algorithm SHA256).Hash }
$record = [ordered]@{Mode=$Mode;CLI=$cli;CLIVersion=(Get-Item -LiteralPath $cli).VersionInfo.FileVersion;
    Project=$project;Arguments=$reactorArgs;Input=$inputFile;InputSHA256=$inputHash;
    InputUnchanged=((Get-FileHash -LiteralPath $inputFile -Algorithm SHA256).Hash -eq $inputHash);
    Output=$outputFile;OutputSHA256=$outputHash;ExitCode=$cliExit;Log=$logPath;
    CheckedAt=(Get-Date).ToString('o');LicensedCliSucceeded=($Mode -eq 'Production' -and $cliExit -eq 0)}
$record | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidence 'run.json') -Encoding UTF8
Get-Content -LiteralPath $logPath
if ($cliExit -ne 0) { throw "Reactor exit $cliExit; evidence: $evidence" }
if (-not $outputHash) { throw "Reactor returned zero without target output; evidence: $evidence" }
Write-Output "Protected candidate: $outputFile"
Write-Output "Evidence: $evidence"
Write-Output 'Run application and structural verification separately; a successful CLI run does not prove feature coverage.'
