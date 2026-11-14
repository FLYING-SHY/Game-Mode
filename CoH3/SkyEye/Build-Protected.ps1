param([switch]$SelfCheck)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$pinSource = [IO.File]::ReadAllText((Join-Path $root 'client\CoH3MemoryTool\PinnedLeaseKey60.cs'))
$pinValues = @{}
foreach ($pinName in @('Modulus','Exponent','Kid')) {
    $pinMatch = [regex]::Match($pinSource, ('\bconst\s+string\s+' + $pinName + '\s*=\s*"([^"]*)"'))
    if (-not $pinMatch.Success -or -not $pinMatch.Groups[1].Value) { throw 'Initialize-Signing.ps1 must pin a public key before release protection.' }
    $pinValues[$pinName] = $pinMatch.Groups[1].Value
}
$pinModulus = [Convert]::FromBase64String($pinValues.Modulus)
$pinExponent = [Convert]::FromBase64String($pinValues.Exponent)
$pinSha = [Security.Cryptography.SHA256]::Create()
try { $actualKid = [BitConverter]::ToString($pinSha.ComputeHash($pinModulus)).Replace('-','').ToLowerInvariant().Substring(0,16) } finally { $pinSha.Dispose() }
if ($pinModulus.Length -ne 384 -or $pinExponent.Length -eq 0 -or $pinValues.Kid -cne $actualKid) { throw 'The pinned RSA-3072 public key and kid do not agree.' }
$repo = Split-Path -Parent (Split-Path -Parent $root)
$sdk = Join-Path $repo 'LicensingSystem\.cache\dotnet-10\dotnet.exe'
if (-not (Test-Path -LiteralPath $sdk)) { throw 'Install workspace SDK 10 using LicensingSystem/deployment/Get-DotNet10.ps1.' }
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$run = Join-Path $root ('artifacts\' + $stamp)
New-Item -ItemType Directory -Path $run -Force | Out-Null
$project = Join-Path $root 'verification\SkyEye.Integration.Verification.csproj'
& $sdk build $project -c Release --nologo 2>&1 | Tee-Object -FilePath (Join-Path $run 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Client/verifier compilation failed.' }
$tool = Join-Path $root 'tools\obfuscar\obfuscar.console.exe'
if (-not (Test-Path -LiteralPath $tool)) {
    & $sdk tool install Obfuscar.GlobalTool --version 2.2.50 --tool-path (Split-Path -Parent $tool)
    if ($LASTEXITCODE -ne 0) { throw 'Pinned Obfuscar installation failed.' }
}
$inputPath = Join-Path $root 'verification\bin\Release\net48'
$outputPath = Join-Path $root 'verification\bin\Protected\net48'
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
$client = Get-ChildItem -LiteralPath (Join-Path $root 'client\bin\Release\net48') -Filter '*.exe' -File | Select-Object -First 1
$escape = { param($value) [System.Security.SecurityElement]::Escape([string]$value) }
$inXml = & $escape $inputPath
$outXml = & $escape $outputPath
$mapXml = & $escape (Join-Path $run 'private-renaming-map.xml')
$clientXml = & $escape (Join-Path $inputPath $client.Name)
$verifierXml = & $escape (Join-Path $inputPath 'SkyEye.Integration.Verification.exe')
$referenceXml = & $escape 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
$naudioXml = & $escape (Join-Path $root 'client')
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
  <AssemblySearchPath path="$naudioXml" />
  <Module file="$clientXml">
    <SkipField type="CoH3MemoryTool.LicenseCredentials60" rx=".*" />
    <SkipMethod type="CoH3MemoryTool.ActivationForm60" name="ReleaseCapture60" />
  </Module>
  <Module file="$verifierXml">
    <SkipField type="*" rx=".*" />
    <SkipProperty type="*" rx=".*" />
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
Copy-Item -LiteralPath (Join-Path $inputPath 'SkyEye.Integration.Verification.exe.config') -Destination $outputPath -Force
Copy-Item -LiteralPath (Join-Path $inputPath ($client.Name + '.config')) -Destination $outputPath -Force
$delivery = Join-Path $root 'delivery\client'
New-Item -ItemType Directory -Path $delivery -Force | Out-Null
$assetsRoot = Join-Path $root 'client\bin\Release\net48'
Get-ChildItem -LiteralPath $assetsRoot -Recurse -File | Where-Object { $_.Extension -in @('.config','.mp3','.wav','.txt','.html','.ico') -and $_.Name -notlike '*.log*' } | ForEach-Object {
    $relative = $_.FullName.Substring($assetsRoot.Length).TrimStart('\')
    $target = Join-Path $delivery $relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
    Copy-Item -LiteralPath $_.FullName -Destination $target -Force
    $verificationAsset = Join-Path $outputPath $relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $verificationAsset) -Force | Out-Null
    Copy-Item -LiteralPath $_.FullName -Destination $verificationAsset -Force
}
Copy-Item -LiteralPath (Join-Path $outputPath $client.Name) -Destination $delivery -Force
$summary = [ordered]@{Run=$stamp;Tool='Obfuscar.GlobalTool 2.2.50';InputSha256=(Get-FileHash -LiteralPath (Join-Path $inputPath $client.Name)).Hash;OutputSha256=(Get-FileHash -LiteralPath (Join-Path $delivery $client.Name)).Hash;Client=(Join-Path $delivery $client.Name);PrivateMapping=(Join-Path $run 'private-renaming-map.xml');ProtectedVerifier=(Join-Path $outputPath 'SkyEye.Integration.Verification.exe');NoPublicSymbols=$true}
$summary | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 (Join-Path $run 'build-summary.json')
$summary | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 (Join-Path $root 'evidence\latest-build.json')
if ($SelfCheck) {
    $sandbox = Join-Path $root ('verification\sandbox\protected-selfcheck-' + $stamp)
    & (Join-Path $outputPath 'SkyEye.Integration.Verification.exe') --mode selfcheck --sandbox $sandbox 2>&1 | Tee-Object -FilePath (Join-Path $run 'selfcheck.log')
    if ($LASTEXITCODE -ne 0) { throw 'Protected client selfcheck failed.' }
}
Write-Output ('Protected client: ' + $summary.Client)
