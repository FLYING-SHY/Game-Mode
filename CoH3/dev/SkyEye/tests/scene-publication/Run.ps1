$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$sdk = Join-Path $repo 'LicensingSystem/.cache/dotnet-10/dotnet.exe'
$project = Join-Path $PSScriptRoot 'ScenePublication.Fixture.csproj'
$engines = @{
    SkyEye = (Join-Path $repo 'CoH3/dev/SkyEye/client/CoH3MemoryTool')
    Simple = (Join-Path $repo 'CoH3/dev/COH3SimpleTool/CoH3MemoryTool')
}
foreach ($label in @('SkyEye', 'Simple')) {
    $intermediate = Join-Path $PSScriptRoot ('obj/' + $label + '/')
    $output = Join-Path $PSScriptRoot ('bin/' + $label + '/')
    & $sdk build $project -c Release --nologo "-p:EngineRoot=$($engines[$label])" "-p:BaseIntermediateOutputPath=$intermediate" "-p:OutputPath=$output" 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot ('build-' + $label + '.log'))
    if ($LASTEXITCODE -ne 0) { throw ('Fixture build failed: ' + $label) }
    & $sdk (Join-Path $output 'ScenePublication.Fixture.dll') $repo 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot ('run-' + $label + '.log'))
    if ($LASTEXITCODE -ne 0) { throw ('Fixture failed: ' + $label) }
}
