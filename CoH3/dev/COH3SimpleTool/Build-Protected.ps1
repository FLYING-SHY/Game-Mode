param([switch]$Smoke)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$pinSource = [IO.File]::ReadAllText((Join-Path $root 'CoH3MemoryTool\PinnedLeaseKey60.cs'))
$pinValues = @{}
foreach ($pinName in @('Modulus','Exponent','Kid')) {
    $pinMatch = [regex]::Match($pinSource, ('\bconst\s+string\s+' + $pinName + '\s*=\s*"([^"]*)"'))
    if (-not $pinMatch.Success -or -not $pinMatch.Groups[1].Value) { throw 'The pinned lease public key is missing; cannot protect.' }
    $pinValues[$pinName] = $pinMatch.Groups[1].Value
}
$pinModulus = [Convert]::FromBase64String($pinValues.Modulus)
$pinExponent = [Convert]::FromBase64String($pinValues.Exponent)
$pinSha = [Security.Cryptography.SHA256]::Create()
try { $actualKid = [BitConverter]::ToString($pinSha.ComputeHash($pinModulus)).Replace('-','').ToLowerInvariant().Substring(0,16) } finally { $pinSha.Dispose() }
if ($pinModulus.Length -ne 384 -or $pinExponent.Length -eq 0 -or $pinValues.Kid -cne $actualKid) { throw 'The pinned RSA-3072 public key and kid do not agree.' }
$repo = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $root))
$sdk = Join-Path $repo 'LicensingSystem\.cache\dotnet-10\dotnet.exe'
if (-not (Test-Path -LiteralPath $sdk)) { throw 'Install workspace SDK 10 using LicensingSystem/deployment/Get-DotNet10.ps1.' }
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$run = Join-Path $root ('artifacts\' + $stamp)
New-Item -ItemType Directory -Path $run -Force | Out-Null
& $sdk build (Join-Path $root 'COH3SimpleTool.csproj') -c Release --nologo 2>&1 | Tee-Object -FilePath (Join-Path $run 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
$tool = Join-Path $root 'tools\obfuscar\obfuscar.console.exe'
if (-not (Test-Path -LiteralPath $tool)) {
    $shared = Join-Path $repo 'CoH3\dev\SkyEye\tools\obfuscar\obfuscar.console.exe'
    if (Test-Path -LiteralPath $shared) {
        New-Item -ItemType Directory -Path (Split-Path -Parent $tool) -Force | Out-Null
        Copy-Item -LiteralPath $shared -Destination $tool -Force
    } else {
        & $sdk tool install Obfuscar.GlobalTool --version 2.2.50 --tool-path (Split-Path -Parent $tool)
        if ($LASTEXITCODE -ne 0) { throw 'Pinned Obfuscar installation failed.' }
    }
}
$inputPath = Join-Path $root 'bin\Release\net48'
$outputPath = Join-Path $root 'bin\Protected\net48'
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
$project = [xml][IO.File]::ReadAllText((Join-Path $root 'COH3SimpleTool.csproj'))
$exeName = $project.SelectSingleNode('/Project/PropertyGroup/AssemblyName').InnerText + '.exe'
if (-not (Test-Path -LiteralPath (Join-Path $inputPath $exeName) -PathType Leaf)) { throw 'Built executable not found: ' + $exeName }
$escape = { param($value) [System.Security.SecurityElement]::Escape([string]$value) }
$inXml = & $escape $inputPath
$outXml = & $escape $outputPath
$mapXml = & $escape (Join-Path $run 'private-renaming-map.xml')
$exeXml = & $escape (Join-Path $inputPath $exeName)
$referenceXml = & $escape 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
$config = @"
<Obfuscator>
  <Var name="InPath" value="$inXml" />
  <Var name="OutPath" value="$outXml" />
  <Var name="LogFile" value="$mapXml" />
  <Var name="XmlMapping" value="true" />
  <Var name="KeepPublicApi" value="false" />
  <Var name="HidePrivateApi" value="true" />
  <Var name="RenameProperties" value="true" />
  <Var name="RenameEvents" value="true" />
  <Var name="RenameFields" value="true" />
  <Var name="KeepProperties" value="true" />
  <Var name="HideStrings" value="true" />
  <Var name="ReuseNames" value="true" />
  <Var name="UseUnicodeNames" value="false" />
  <Var name="RegenerateDebugInfo" value="false" />
  <Var name="OptimizeMethods" value="true" />
  <Var name="SuppressIldasm" value="true" />
  <AssemblySearchPath path="$referenceXml" />
  <Module file="$exeXml">
    <SkipField type="CoH3MemoryTool.LicenseCredentials60" rx=".*" />
    <SkipMethod type="CoH3MemoryTool.ControlBarForm" name="ReleaseCapture60" />
  </Module>
</Obfuscator>
"@
$configPath = Join-Path $run 'obfuscar.xml'
[System.IO.File]::WriteAllText($configPath, $config, [System.Text.UTF8Encoding]::new($false))
$previousRoot = $env:DOTNET_ROOT
try {
    $env:DOTNET_ROOT = Split-Path -Parent $sdk
    & $tool $configPath 2>&1 | Tee-Object -FilePath (Join-Path $run 'obfuscar.log')
    if ($LASTEXITCODE -ne 0) { throw 'Obfuscation failed; no release produced.' }
} finally { $env:DOTNET_ROOT = $previousRoot }
Copy-Item -LiteralPath (Join-Path $inputPath ($exeName + '.config')) -Destination $outputPath -Force
$runtimeState = @('overlay-settings.txt','minimap-location-cache.txt','minimap-ui-root-cache.txt','alert-sounds.txt','air-diagnostics.log','air-outline-diagnostics.log','minimap-diagnostics.log','overlay-diagnostics.log')
$delivery = Join-Path $repo 'CoH3\output\COH3SimpleTool'
New-Item -ItemType Directory -Path $delivery -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $outputPath $exeName) -Destination $delivery -Force
Copy-Item -LiteralPath (Join-Path $outputPath ($exeName + '.config')) -Destination $delivery -Force
# Runtime preferences and diagnostics are user state, never part of a delivery.
Get-ChildItem -LiteralPath $delivery -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -in $runtimeState } | Remove-Item -Force
$summary = [ordered]@{Run=$stamp;Tool='Obfuscar.GlobalTool 2.2.50';InputSha256=(Get-FileHash -LiteralPath (Join-Path $inputPath $exeName)).Hash;OutputSha256=(Get-FileHash -LiteralPath (Join-Path $delivery $exeName)).Hash;Executable=(Join-Path $delivery $exeName);PrivateMapping=(Join-Path $run 'private-renaming-map.xml');NoPublicSymbols=$true}
$summary | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 (Join-Path $run 'build-summary.json')
$summary | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 (Join-Path $root 'evidence\latest-build.json')
if ($Smoke) {
    $capture = Join-Path $repo 'CoH3\dev\SkyEye\scripts\Capture-Window.ps1'
    $evidence = Join-Path $root ('evidence\protected-smoke-' + $stamp)
    New-Item -ItemType Directory -Path $evidence -Force | Out-Null
    $p = Start-Process -FilePath (Join-Path $delivery $exeName) -PassThru
    try {
        $deadline = (Get-Date).AddSeconds(20)
        do { Start-Sleep -Milliseconds 500; $p.Refresh() } while ($p.MainWindowHandle -eq [IntPtr]::Zero -and -not $p.HasExited -and (Get-Date) -lt $deadline)
        if ($p.HasExited) { throw ('Protected exe exited during smoke test with code ' + $p.ExitCode + '.') }
        if ($p.MainWindowHandle -eq [IntPtr]::Zero) { throw 'Protected exe produced no main window.' }
        $expectedTitle = [IO.Path]::GetFileNameWithoutExtension($exeName)
        if ($p.MainWindowTitle -notin @($expectedTitle, ($expectedTitle + ' · 激活'))) { throw ('Unexpected window title: ' + $p.MainWindowTitle) }
        & $capture -TargetProcessId $p.Id -OutputPrefix (Join-Path $evidence 'login') | Tee-Object -FilePath (Join-Path $evidence 'capture.json')
    } finally {
        if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }
    }
    # The smoke run exercises the delivered binary in place; drop the state it produced.
    Get-ChildItem -LiteralPath $delivery -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -in $runtimeState } | Remove-Item -Force
}
Write-Output ('Protected executable: ' + $summary.Executable)
