# HUD match context — isolated F slice

This directory contains the task-6 production-helper fixture, original source backups, exact baseline diffs, and the frozen production-source hash manifest.

## What changed

The 52426 reader now builds its product `MatchKey` from the existing private world identity and a positively validated `(current root, HUD, software epoch)` token. The original world-key format remains the product key for the legacy build. The existing build dispatch, 37-role layout, build/layout/license/open ordering, and licensing implementation are unchanged.

The main reader's `Snapshot` is the only discovery/mint caller. It checks the chosen token before reading rows and again before committing `MatchKey`. `MarkerContext`, independent marker-reader construction, `IsCurrentMatch55`, world gates, maps, support commands, and the public camera result validate the expected token without adopting a replacement HUD. A clone-bound guard also prevents discovery after its expected token retires.

The token has a process-local monotonic `Interlocked` epoch and a shared `Volatile`/`Interlocked` retirement latch. Normal refresh, pause flags, and transient HUD reads preserve the token. A confirmed cached HUD type mismatch or a valid changed current root retires that token; a later HUD at the same address receives a new epoch. New reader instances also receive a new global epoch.

`MatchHudUnavailableException(MatchHudOutcome outcome, string message)` derives directly from `InvalidOperationException`. Its outcomes are `Resolving`, `Retired`, `ReadFault`, and `Ambiguous`. Qualification faults propagate through both map `TryRead` methods and `IsCurrentMatch55`, so the UI can clear publications and preserve the reader's resolving queue rather than enter ordinary data recovery. The UI edits belong to the lead task and are outside this slice.

## Positive witnesses and limits

The guard uses the existing detected-build roles for the UI singleton, root type, and HUD type. Discovery starts from the live singleton's root; it does not trust the old minimap root cache. It walks inline pointer candidates at offsets 144..568 and the existing vector504/header112/count124 shape, with count at most 512. Each accepted edge has a child `parent136` witness. HUD candidates are target leaves; their large content subtrees are not scanned.

Before adopting an initial token, the existing non-HUD frontier must finish without a known read fault or node-cap truncation. Multiple positively validated candidates, including candidates at different depths and branches, produce `Ambiguous`. This concerns the supported frontier, not a claim that the full native UI tree has been reconstructed. An empty frontier, unsupported edge, partial read, or exhausted budget never becomes an absence/phase proof.

Strict validation reads the expected HUD type, live root type/address and root parent0, at most 64 ancestors, bidirectional membership witnesses, ancestor UI vtable range, and repeats the relevant child parent/HUD/current-root reads. Flags remain display geometry and never decide match semantics.

Default per-operation bounds are 256 exact reads, 131072 bytes, 12 ms, 128 discovery nodes, and 1024 candidate steps. A discovery pass retains at most 6000 known addresses. Per-turn budget exhaustion keeps its queue/cursor for continuation. A failing node is put at the queue tail, allowing other branches to advance; a pass containing known faults stays unknown and is retried. These limits bound the helper's issued read operations, not native syscall latency.

Unknown qualification invalidates the existing minimap epoch and clears map controls without changing the HUD epoch. Async minimap publication also checks its captured minimap epoch, product key, and token reference. The two geometry readers require the specified HUD and root addresses. The public camera API has a strict result gate; a private camera core is used only by bootstrap identity/colour reads enclosed by Snapshot qualification.

## Run the F fixture

```powershell
& .\LicensingSystem\.cache\dotnet-10\dotnet.exe run `
  --project .\CoH3\dev\SkyEye\tests\match-hud-context\MatchHudContext.Fixture.csproj `
  --configuration Release -- F:\WorkSpace\Game

& .\LicensingSystem\.cache\dotnet-10\dotnet.exe run `
  --project .\CoH3\dev\SkyEye\tests\match-hud-context\MatchHudContext.Fixture.csproj `
  --configuration Release `
  -p:EngineRoot=F:\WorkSpace\Game\CoH3\dev\COH3SimpleTool\CoH3MemoryTool `
  -p:BaseIntermediateOutputPath=obj-simple/ -p:OutputPath=bin-simple/ `
  -- F:\WorkSpace\Game
```

The fixture links the actual production helper/context and supplies a deterministic byte-addressed memory delegate. It never constructs `PlayerInfoReader`, opens a process, links P/Invoke or licensing, supplies a fake license, or opens a GUI. Reader integration is checked by explicit production-source seams; it is not a runtime reader/injection test.

Covered cases: same-world/same-root second HUD; stable pause/flags; short-read and read-fault recovery; shared clone epoch and retirement; same-address revival blocked for old clones; new-reader epoch; live-root replacement and parent/type uncertainty; inline496 and vector witnesses; missing witnesses/empty graph; overlarge vector; different-depth dual HUD; common root/path budget; retained cursor across budget turns; faulty-node fairness; 64-ancestor limit; both production mirrors; shared product-key call sites; snapshot end-before-commit ordering; strict context/clone/current-check hooks; map/async/camera/support hooks; legacy key/support behavior; unchanged constructor preflight/license/open ordering.

## Evidence level and remaining G

The native evidence that motivated the change is historical: two rounds in PID30504 reused the complete world key and live root while HUD instances changed; paused HUD/minimap/tactical chains remained positively connected; results reads of the old controls returned exact bodies with changed first QWORDs. This fixture does not reproduce or extend those native measurements.

The new production behavior still needs G after the lead builds it: actual paused round acceptance, immediate results clearing, same-PID second-round recovery, independent clone rejection, minimap/tactical recovery after unknown reads, and steady-state camera/HUD qualification cost. Real UI-tree edges or types outside the supported frontier may leave discovery resolving; the F tests deliberately do not label that a successful game adaptation. Full client builds, UI integration fixtures, protection, publication, native G, and delivery are owned by the lead.

Original reader backups in [baseline](<baseline/>) retain SHA256 `39DDE7A0F2BD80E2412FED3439870CF7A49A794A9AAB0F812A9AFEE6F60C65D2`; original context SHA256 is `BF777F7A02831F5E36738EB421D36F35A05C17E08B4E5320A66F68E6FE05188E`. Frozen production hashes are in [the manifest](<frozen-source-sha256.json>). The exact reader delta against that original is in [the SkyEye diff](<SkyEye.PlayerInfoReader.cs.diff>) and [the Simple diff](<Simple.PlayerInfoReader.cs.diff>).
