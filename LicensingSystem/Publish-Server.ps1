param([switch]$NoBuild)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$sdk = Join-Path $root '.cache\dotnet-10\dotnet.exe'
if (-not (Test-Path -LiteralPath $sdk)) { throw 'Install workspace .NET SDK 10 first.' }
$project = Join-Path $root 'server\Licensing.Server.csproj'
if (-not $NoBuild) {
    & $sdk build $project -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Signed server build failed.' }
}
$output = Join-Path $root 'delivery\server'
& $sdk publish $project -c Release --no-build --no-restore --self-contained false -p:UseAppHost=false -o $output --nologo
if ($LASTEXITCODE -ne 0) { throw 'Signed server publish failed.' }
Copy-Item -LiteralPath (Join-Path $root 'Start-PublishedServer.ps1') -Destination (Join-Path $output 'Start-Server.ps1') -Force
$deployment = Join-Path $output 'deployment'
New-Item -ItemType Directory -Path $deployment -Force | Out-Null
foreach ($name in @('Get-DotNet10.ps1','New-DeploymentConfig.ps1','Caddyfile')) {
    Copy-Item -LiteralPath (Join-Path $root ('deployment\' + $name)) -Destination (Join-Path $deployment $name) -Force
}
Write-Output ('Published signed server: ' + $output)
