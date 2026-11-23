# Marker diagnosis: zero was a runner contract error

## Static root cause

Before lead's fix, the maintenance sampler called ReadSquadMarkers directly with snapshot MarkerSquads. New SquadMarkerSource instances default ColourKnown=false. TrackSquadMember fills identity/path/slot/members, not colour; ReadPlayerColours fills the **row**, not its sources. ReadSquadMarkers immediately skips a source when !source.ColourKnown, before reading any members, so zero output was not evidence of absent squads or invalid game positions.

- [TrackSquadMember](<../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3302-L3324>) captures squad+104 identity, member+96 generation, +124 squad offset, +88 owner.
- [ReadSquadMarkers](<../../../client/CoH3MemoryTool/PlayerInfoReader.cs#L3341-L3399>) gates colour first, then identity and member checks, HealthExt+108, entity positions +44/+48/+52; inner and outer catches conceal read/layout exceptions.
- [Production marker host](<../../../client/CoH3MemoryTool/SquadMarkerHost.cs#L120-L144>) explicitly binds each source's colour from its row, as do MinimapMarkerWindow and VerifyLive58. The original runner omitted this caller responsibility.
- Lead fixed the runner before this live probe. The JSON says runnerColourAssignmentPresentAtCapture=true; the preFixRunnerGateOutcome is a source-derived explanation of the earlier version, not a claim about the now-fixed runner heap.

## Read-only evidence

[Probe JSON](<markers-probe-01.json>): PID11192, access0x410, 1658 reads, 63858 requested/returned bytes, zero failed/short reads, 5.723ms measured. Arena/world and player registry headers remained stable.

Squad-only reconstruction: **12 sources, 36 members, all36 passed generation/squad-offset/owner/health/position checks;12 valid centres if real row colours are bound.** Each source's squad+104 identity matched before and after. Each member has raw128-byte snapshot/recheck, health component descriptor, exact x/y/z, boolean checks and centre. This is a Python reconstruction of the relevant reader path, **not a direct Snapshot call** and not a credential-bearing runner invocation.

The earlier run's11 squads and this12-source sample were captured at different times; no assertion of an atomic same-time comparison is made. Lead separately reported maintenance-run-05 observed12 actual markers after binding colours, matching the diagnostic result. No actual GUI/visual correctness was tested here.

Infrastructure49-specific marker collection is excluded from this squad-only script. Optional Direction/Movement internals are not tested by this read; null output does not mean absent gameplay events. Actual movement additionally needs the caller flag FlankReadMovement47 for infantry. Cross-session/lifecycle/visual tests remain not_run.

## Reproduce / limits

[Script](<markers_probe.py>) imports the existing exact-length ProcessReader without bytecode writes, fixes PID11192, and bounds requests to10s/2MiB/20000calls,16players,4096registry entries/player. It never logs in, reads credentials, writes game memory, resumes/controls the game or changes engine files. One synchronous OS read is not forcibly preempted.

```powershell
& 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe' .\CoH3\dev\SkyEye\evidence\version-adaptation\markers\markers_probe.py --self-test
& 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe' .\CoH3\dev\SkyEye\evidence\version-adaptation\markers\markers_probe.py
```

[Fixture log](<fixture.log>): finite/bounded positions, supported squad filtering, false colour eligibility suppresses valid positions. [Probe log](<probe.log>): single paused live read summary. Both exit0. No zero-source result is labelled absent: script uses unknown_no_sources in that case, retains failures, and exits3 on read/root errors.
