param(
    [switch]$Publish,
    [string]$PublicConfiguration = '',
    [string]$OutputDirectory = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $root
try {
    $configurationArguments = @()
    if ($PublicConfiguration) {
        $configurationPath = (Resolve-Path -LiteralPath $PublicConfiguration).Path
        $configurationArguments = @("-p:ClientPublicConfiguration=$configurationPath")
    }
    if ($Publish) {
        if (-not $PublicConfiguration) {
            throw 'Publishing requires -PublicConfiguration with the service URI and verification PUBLIC key.'
        }
        $stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fffffff'
        $work = Join-Path $root "artifacts\gui-build\$stamp"
        $stage = Join-Path $work 'original'
        $protected = Join-Path $work 'protected'
        $destination = if ($OutputDirectory) { [IO.Path]::GetFullPath($OutputDirectory) } else { Join-Path $root 'publish-gui' }
        $legacyPublish = [IO.Path]::GetFullPath((Join-Path $root 'publish')).TrimEnd('\')
        if ($destination.TrimEnd('\') -eq $legacyPublish) {
            throw 'Use a separate output directory; publish contains the verified 2.0 release.'
        }
        dotnet tool restore
        if ($LASTEXITCODE -ne 0) { throw "Tool restore failed (exit $LASTEXITCODE)" }
        dotnet publish .\CoH3FowTool.csproj -c Release -r win-x64 --self-contained true --nologo `
            -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false @configurationArguments -o $stage
        if ($LASTEXITCODE -ne 0) { throw "Stage publish failed (exit $LASTEXITCODE)" }
        & (Join-Path $root 'build\ProtectAssemblies.ps1') -InputDirectory $stage -OutputDirectory $protected
        dotnet publish .\CoH3FowTool.csproj -c Release -r win-x64 --self-contained true --no-build --no-restore --nologo `
            -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
            -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false `
            "-p:ProtectedAssemblyDirectory=$protected" @configurationArguments -o $destination
        if ($LASTEXITCODE -ne 0) { throw "Single-file publish failed (exit $LASTEXITCODE)" }
        $out = Join-Path $destination 'CoH3FowTool.exe'
        $files = @(Get-ChildItem -LiteralPath $destination -Recurse -File)
        if ($files.Count -ne 1 -or $files[0].Name -ne 'CoH3FowTool.exe') {
            throw 'Client output must contain exactly one EXE; use a clean, dedicated output directory.'
        }
        $report = [ordered]@{
            createdAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
            executable = $out
            sha256 = (Get-FileHash -LiteralPath $out -Algorithm SHA256).Hash
            protector = 'Obfuscar.GlobalTool 2.2.50'
            originalAssemblies = $stage
            protectedAssemblies = $protected
            publicConfiguration = $configurationPath
            scope = 'Build evidence only; no game or second-device execution.'
        }
        $report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $work 'build-report.json') -Encoding utf8
        Write-Host "Build evidence: $(Join-Path $work 'build-report.json')"
    }
    else {
        dotnet build .\CoH3FowTool.csproj -c Release --nologo @configurationArguments
        if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)" }
        $out = Join-Path $root 'bin\Release\net9.0-windows\CoH3FowTool.exe'
    }
    if (-not (Test-Path -LiteralPath $out -PathType Leaf)) { throw "Output missing: $out" }
    Write-Host "Built: $out" -ForegroundColor Green
}
finally { Pop-Location }
