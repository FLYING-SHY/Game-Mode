# Production HUD qualification observer

This is a standalone net48/x64 console diagnostic. It links the frozen production HUD helper directly. It does not link or construct PlayerInfoReader, product UI, licensing, or any license substitute. No production file is generated or modified by this project. Building it does not perform native sampling.

The only accepted live identity is PID30504, start `2026-11-22T01:23:35.3993114Z`, module SHA256 `8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec`. Every poll rechecks process start, MainModule path/base/size, and the complete module-file hash. A changed identity stops with exit 2. Process exit stops sampling. OpenProcess uses only `0x410` and its handle is closed in finally.

## Static inputs and derivation

[Build.ps1](<Build.ps1>) checks the frozen SkyEye/Simple reader/helper/context hashes, the current Reader's `UsesCandidateLayout` and `BuildRva45` branch, and the exact production helper-construction call. It extracts the singleton/root/HUD roles from the current layout table. It then checks those roles against the historical v4 raw role table and verifies `raw module base + derived type RVA == raw root/HUD vtable`. It emits no historical absolute control address.

Each build uses a new directory under `builds`, emits its generated pin source, and links the actual production helper with its default 256-read / 128-KiB / 12-ms budget. The project has no package references or external runtime dependencies beyond Windows .NET Framework 4.8. Build/source/executable SHA256 values are saved in that build's manifest, with source hashes checked again after compilation.

The diagnostic world string is explicitly `diagnostic-qualification-only:PID:startTicks:image`. It is not the product's world identity or Snapshot. Any minted epoch is labelled `diagnosticSoftwareEpoch`; `productEpoch` is null. Results always retain `productAcceptance=false`, `productGUI=not_run`, `ReaderConstructed=false`, and `classification=qualification-only`.

## Read and journal contract

Every delegate call is recorded in UTF-8 JSONL, including blocked calls. Counters distinguish logical attempts, budgeted requested reads/bytes, VQ precheck failures, actual RPM issues, RPM failure/short reads, and actual returned bytes. Canonical-address, positive-length/65536-byte, and overflow checks precede VirtualQueryEx. All covered regions must be committed/readable, without guard/noaccess protection. A VQ failure has `rpmIssued=false`, `nativeOK=null`, and its own query result/error; it is never labelled an RPM failure.

Issued RPM calls record actual native success, the freshly captured last-error value, requested/actual sizes, native elapsed time, and hex of every returned byte, including partial returns on failure. The original delegate error remains in the journal even when the production helper translates it to typed `ReadFault`. The walking address/cursor is read through reflection only; no helper field is written.

Raw buffers and metadata accumulate during each Resolve. Hex conversion, JSON serialization, and file writes occur after Resolve returns. Resolve timing still includes the observer's VQ, reflection, allocation, and bookkeeping overhead. It is not a measurement of unobserved product-only latency. Module hashing, journal flush cost, and total invocation cost are recorded separately. The helper's 12-ms check limits issuing further reads cooperatively; it does not preempt a native call.

Each poll records outcome, exception message, elapsed time, read deltas, and read-only helper queue/seen/candidate/walking/fault/limit statistics before and after the call. Ready results cross-check current exact-read journal bytes against the derived root/HUD type addresses, without an extra probe read or cached address hint.

## Bounds and output

At most 40 Resolve calls run, at actual Resolve start intervals of at least 250 ms. There is no catch-up burst after slow initialization or a slow poll. After the first Ready, the observer performs up to 10 further cached Resolve attempts and stops. The global bounds are 30 seconds, 12000 budgeted read requests/RPM calls, 8 MiB requested bytes, and 32 MiB aggregate journal/summary output. A final cap-denied logical call is still journaled and issues no RPM. Source pin hashing is limited to 32 MiB in aggregate (16 MiB per file), separately from the required game-module hash.

Output reservation is conservative and leaves 1 MiB for summary/failure metadata. Output must be a new directory inside this project's directory; existing outputs and reparse-point parent paths are rejected. Files use CreateNew. Any truncated, failed, or capped capture is explicitly partial. Unwritten issued-read records are drained in finally using a consumed-record index; serialization/I/O failure marks `journalComplete=false`. Handle close proceeds independently of log-drain success.

No Ready within the bounded observation is a diagnostic outcome with exit 0. Exit 2 is reserved for identity/source mismatch, argument/program/handle/output failure. The summary does not infer HUD absence or a game phase from unknown/budget/read-fault outcomes. It does not report a product acceptance result.

## Build only

From the repository root:

```powershell
& .\CoH3\dev\SkyEye\tests\match-hud-live\Build.ps1 -RepoRoot F:\WorkSpace\Game
```

The script only invokes `dotnet build`. Its final JSON identifies the new build manifest and executable. The build manifest must say `built_not_native_run`. All previous build directories remain untouched.

## Explicit native run for lead

Select the frozen build manifest reported in the handoff; verify its executable hash before running. The following command body is exact after setting `$ManifestPath` to that supplied manifest. It runs only when the lead invokes it:

```powershell
$pin = [IO.File]::ReadAllText($ManifestPath) | ConvertFrom-Json
if ($pin.status -ne 'built_not_native_run') { throw 'Select the reviewed build-only manifest' }
if ((Get-FileHash -LiteralPath $pin.executable.path -Algorithm SHA256).Hash -ne $pin.executable.sha256) { throw 'Executable hash changed' }
$runDir = 'F:\WorkSpace\Game\CoH3\dev\SkyEye\tests\match-hud-live\runs\' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0,8)
& $pin.executable.path --repo F:\WorkSpace\Game --pid 30504 --start-utc 2026-11-22T01:23:35.3993114Z --module-sha256 8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec --out $runDir
```

Inspect the new run's journal and summary before drawing a conclusion. In particular, correlate `walkReadFault`, cursor/queue progress, and the original precheck/RPM reasons to test whether optional vector pointers or rejected inline candidates poison a discovery pass. This project records that hypothesis without changing the production helper. Full UI acceptance, native game phase, stable cross-session recovery, and unobserved product latency remain separate measurements.
