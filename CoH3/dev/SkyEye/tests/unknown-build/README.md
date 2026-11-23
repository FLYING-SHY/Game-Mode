# Unknown-build layout stop — isolated F fixture

Task-4 changes only the two production PlayerInfoReader selectors. Build52426 keeps candidate layout, Build2406 keeps legacy layout. Unknown and any unregistered enum now throw InvalidOperationException from UsesCandidateLayout, including the ValidateLayout path, regardless of maintenanceDiagnostics.

Run from the repository root with Windows PowerShell 5.1:

```powershell
& .\CoH3\dev\SkyEye\tests\unknown-build\Run.ps1
```

The runner uses LicensingSystem/.cache/dotnet-10/dotnet.exe (observed SDK 10.0.404), .NET Framework 4.8 reference assemblies and an x64 .NET Framework runtime. All CLI home/package/build outputs are isolated under this directory. Compile items are explicitly enabled only for the fixture and the selected production tree; before/**/*.cs is explicitly removed. Run.ps1 has a UTF-8 BOM for Windows PowerShell 5.1.

The actual production source is linked for each flavor; no copy of the selector implementation is tested. FormatterServices.GetUninitializedObject creates managed Reader storage without executing constructors or field initializers. Only detectedBuild and maintenanceDiagnostics are set by reflection. Each of Build2406, Build52426, Unknown and enum 123456 is tested with maintenance false and true.

Per variant, 48 check groups execute the real UsesCandidateLayout, ValidateLayout, AirMoverShape45, BuildRva45, LegacyTransformRva45 and check untouched process/handle/resource fields. Known-build RVA dispatch traverses every production registered role; five independent transform vectors cover all reverse-transform roles. Two additional check groups decode both private constructors' compiled IL and require RequireVerified < ValidateLayout < LicenseGate60.Matches < Native.OpenProcess. The runner requires exactly 50 passes and zero failures per variant.

Latest execution: SkyEye Release 50/0; SkyEye Maintenance 50/0; Simple Release 50/0; Simple Maintenance 50/0. Total 200 check groups passed, zero failed, 32 scenario combinations. All four fixture build and run exit codes are 0. See latest-result.json and runs/<flavor>/<configuration>/{result.json,build.log,run.log}.

## Before/current evidence

Both before/PlayerInfoReader snapshots were copied before editing. Two production files have the same selector-only diff in before-current.diff. Precise current source locations in both trees: selector line 2363, explicit exception line 2374, ValidateLayout line 2377 (selector call 2379); constructor order is lines 2337/2338/2339/2343 and 2428/2429/2430/2434. GameLayout52426 sources are unchanged.

- Before Reader SHA256: 3444F469137C616D382EE795967188F82E758A14ED36AA99FB7E52FAD3AB0350
- Frozen/current Reader SHA256: 39DDE7A0F2BD80E2412FED3439870CF7A49A794A9AAB0F812A9AFEE6F60C65D2
- Unchanged GameLayout52426 SHA256: 90AF6A0F98C5C3C48DD9CAD8595377EE90990938753E68B142899B707C596D47

The runner validates all six inputs before its builds and checks for drift after all four runs. Production freeze and hashes were sent to lead and phase-probe-current before any fixture execution. Neither production file changed after freeze.

## Limits and host history

Evidence level is F only: actual pure production methods executed offline, plus constructor IL inspection. Reader constructors, LicenseGate60 and native APIs never execute; there is no game attachment or native handle creation. Game validation, repeated game sessions and delivery validation remain not_run. The root agent owns full-client builds. This fixture does not build or overwrite any other fixture's outputs.

Attempt 1 stopped at WinPS 5.1 native stderr handling (NativeCommandError). Attempt 2 completed all four builds and 200 checks, then stopped at OrderedDictionary/Measure-Object -Property aggregation. Run.ps1 now captures native output with explicit UTF-8 logs, checks native exit codes and projects dictionary values before summing. The next complete invocation exited 0; attempt-1-host.log and attempt-2-host.log retain the exact host errors. Existing production uninitialized-field CS0649 warnings appeared during the initial compilation; the final invocation was incremental. No product C# adjustment was made for either host issue.
