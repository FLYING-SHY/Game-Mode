# Layout preflight fixture (offline only)

Purpose: maintenance readers validate the explicit 37-role dependency inventory after the candidate-file hash check and before licensing/native OpenProcess. Missing mappings throw with the legacy key before per-object catches can convert that failure into apparent absence. This does not make every runtime/structure error observable.

Production edits are limited to mirrored `GameLayout52426.cs` and `PlayerInfoReader.cs`. No hash was registered, no release entry was opened, no normal licensing check was bypassed, and no candidate values were promoted to verified.

## Reproduce

Run from repository root using the installed .NET 10 SDK:

```powershell
$env:DOTNET_CLI_HOME = "$PWD/CoH3/dev/SkyEye/tests/layout-preflight/.cli"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$dotnet = "$PWD/LicensingSystem/.cache/dotnet-10/dotnet.exe"
$project = 'CoH3/dev/SkyEye/tests/layout-preflight/LayoutPreflight.Fixture.csproj'
foreach ($root in @("$PWD/CoH3/dev/SkyEye/client/CoH3MemoryTool", "$PWD/CoH3/dev/COH3SimpleTool/CoH3MemoryTool")) {
    foreach ($config in @('Maintenance', 'Release')) {
        & $dotnet run --project $project --configuration $config "-p:LayoutSourceRoot=$root" -- "$PWD"
        if ($LASTEXITCODE -ne 0) { throw 'fixture failed' }
    }
}
```

The fixture links only GameLayout52426 and BCL test code: no PlayerInfoReader, process P/Invoke, real game, licensing client, or UI is executable in this test. Reader construction order and unchanged licensing gate presence are checked against source, not exercised through a game process.

## Assertions

- Both production source pairs are byte-identical.
- Both maintenance constructor branches call RequireCandidate -> ValidateDependencies -> existing LicenseGate -> Native.OpenProcess in that order.
- Maintenance has 37 unique required keys matching the candidate dictionary and source-scanned reader/helper dependencies.
- Source scanner covers BuildRva45 literal/ternary arguments, component lookup call arguments, MiniPropertyId slots, transform case roles, and all same-directory helper files using `resolveRva?.Invoke`. A synthetic future helper dependency is detected as outside the required inventory. This is a targeted textual guard, not a general C# semantic analyzer for arbitrary aliases or indirect constants.
- Complete mappings pass. Removing each of the 37 keys independently causes InvalidOperationException containing that key's hex value; every mutation is restored in finally.
- Release contains no maintenance methods/fields and no UTF-16 candidate hash in the compiled fixture assembly.

## Observed results

Both SkyEye and COH3SimpleTool source roots: Maintenance PASS (94 assertions each), Release PASS (8 assertions each), exit 0. The only test-scaffolding failure during development was an overly narrow constructor-order regex; it was corrected and all four final runs passed.

No real game/UI was run; no full client or maintenance-live project was rebuilt. Preflight proves mapping presence only, not candidate RVA accuracy, runtime event presence, or G/R acceptance.
