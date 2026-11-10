param(
    [Parameter(Mandatory)][string]$InputDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$inputPath = (Resolve-Path -LiteralPath $InputDirectory).Path
$outputPath = [IO.Path]::GetFullPath($OutputDirectory)
if ($inputPath.TrimEnd('\') -eq $outputPath.TrimEnd('\')) {
    throw 'Protection output must be separate from the original publish directory.'
}
$modules = @('CoH3FowTool.dll', 'Licensing.Client.dll')
foreach ($module in $modules) {
    if (-not (Test-Path -LiteralPath (Join-Path $inputPath $module) -PathType Leaf)) {
        throw "Protection input missing: $module"
    }
}
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
$xmlInput = [Security.SecurityElement]::Escape($inputPath)
$xmlOutput = [Security.SecurityElement]::Escape($outputPath)
$configuration = @'
<Obfuscator>
  <Var name="InPath" value="__INPUT__" />
  <Var name="OutPath" value="__OUTPUT__" />
  <Var name="KeepPublicApi" value="true" />
  <Var name="HidePrivateApi" value="true" />
  <Var name="RenameProperties" value="false" />
  <Var name="RenameEvents" value="false" />
  <Var name="HideStrings" value="false" />
  <Var name="XmlMapping" value="true" />
  <Var name="RegenerateDebugInfo" value="false" />
  <Module file="$(InPath)\CoH3FowTool.dll" />
  <Module file="$(InPath)\Licensing.Client.dll">
    <!-- Obfuscar 2.2.50 strips parameter names before renaming methods.
         System.Text.Json reflects over both request and response constructors;
         skipMethods is required to keep these three transport contracts intact. -->
    <SkipType name="Licensing.ActivationRequest" skipMethods="true" skipFields="true" skipProperties="true" skipEvents="true" skipStringHiding="true" />
    <SkipType name="Licensing.ValidationRequest" skipMethods="true" skipFields="true" skipProperties="true" skipEvents="true" skipStringHiding="true" />
    <SkipType name="Licensing.LicenseApiResponse" skipMethods="true" skipFields="true" skipProperties="true" skipEvents="true" skipStringHiding="true" />
  </Module>
</Obfuscator>
'@
$configuration = $configuration.Replace('__INPUT__', $xmlInput).Replace('__OUTPUT__', $xmlOutput)
$configPath = Join-Path $outputPath 'obfuscar.xml'
[IO.File]::WriteAllText($configPath, $configuration, [Text.UTF8Encoding]::new($false))
Push-Location $projectRoot
try {
    dotnet tool run obfuscar.console -- $configPath
    if ($LASTEXITCODE -ne 0) { throw "Assembly protection failed (exit $LASTEXITCODE)." }
    foreach ($module in $modules) {
        $original = Join-Path $inputPath $module
        $protected = Join-Path $outputPath $module
        if (-not (Test-Path -LiteralPath $protected -PathType Leaf)) { throw "Protection output missing: $module" }
        if ((Get-FileHash -LiteralPath $original).Hash -eq (Get-FileHash -LiteralPath $protected).Hash) {
            throw "Assembly protection did not change $module."
        }
    }
}
finally { Pop-Location }
