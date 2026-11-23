# Production diagnosis — PID 11192, paused single-session sample

## Result

- Exact read-only probe: 2269 reads / 42871 requested and returned bytes, zero failed/short reads, stable arena/world and active queue rechecks.
- Local American HQ, German HQ and Infanterie Kompanie queues are empty: `sample_absent`, not verified production coverage.
- German Mechanized Kompanie has one queue entry. Queue `0x17c02adac00`, first entry `0x17c03d8bf00`, item `0x17c07820800`.
- Item RTTI `.?AVSpawnProductionItem@@`, vtable RVA `0x3D98BF0`, COL `0x505A6B0`, type descriptor `0x5F9B110`; blueprint `sbps\\races\\german\\infantry\\jaeger_ger`.
- Entry +48/+52 = 13 / 240 raw ticks. Existing conversion ceil(13/8)=2 seconds. Paused target means no time progression or on-screen timer equivalence was tested.
- SquadUpgradeProductionItem / EntityUpgradeProductionItem: `sample_absent`; no vehicle/upgrade event observed. Their mapping remains static candidate evidence supplied by agent-3, not dynamic verification from this probe.

## Source path and hidden failure

Current source references (lead may subsequently move lines):

1. `CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2649-2702`: ReadEntities walks player+1792 registry, validates entity+88 owner, recognizes building blueprint `\\buildings\\production\\`, and invokes ReadProduction. Vehicle special case is Africa Korps halftrack_250 only.
2. `CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2868-2906`: HealthExt lookup, health+108 > 0; ProductionQueueExt+52 (or squad inventory+16) gives queue offset; queue+48 data/+52 length; first 192-byte entry; entry+184 points to ProductionItem. Count zero returns before all type guards, hence empty local queues can look healthy while enemy nonempty queue fails.
3. `CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2910`: `Q(num10) == image + BuildRva45(64584880)` must evaluate the map lookup for every non-vehicle nonempty queue, before deciding if item is a spawn. Missing key deterministically throws even when live reads and item are valid.
4. `CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2924`: `BuildRva45(vehicle ? 64557040 : 64557304)` runs unconditionally after the spawn branch. Adding only SpawnProductionItem therefore still discards a valid spawn if the normal-upgrade guard is absent; list commits happen later at 2954-2964.
5. `CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2967-2976`: broad catch discards the exception and only sets ProductionKnown=false or Kwk38Known=false. Runner sees a generic availability failure, not the first failing guard/read address.
6. `CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs:2458-2468`: ReadProcessMemory must return exactly n bytes. False or short read throws the same generic game-changing exception, which ReadProduction also swallows. This diagnostic script instead retains exact requested address, length, API error and actual byte count on failure.

The initially inspected candidate dictionary lacked all three production guards, sufficient to explain run-01. Before the live probe, lead added them. The JSON records the **post-fix source SHA256** and `firstMissingGuard=null`; that is not a claim that run-01 used the new dictionary. No inner exception from run-01 was captured. Zero short reads now does not retrospectively rule out an additional transient failure during run-01.

Agent-3 static identities: SpawnProductionItem `0x3D98BF0`; vehicle SquadUpgradeProductionItem `0x3D92058`; normal EntityUpgradeProductionItem `0x3D92160`. PlayerUpgradeProductionItem `0x3E57008` is a distinct alternative, not silently substituted. Full old/new upgrade-role equivalence still lacks a live upgrade event.

## Reproduce

From repository root; existing bundled Python, no build/authentication required:

```powershell
& 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe' .\CoH3\dev\SkyEye\evidence\version-adaptation\production\production_probe.py --self-test
& 'E:\Cache\AI\Deepseek\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe' .\CoH3\dev\SkyEye\evidence\version-adaptation\production\production_probe.py
```

Self-test confirms entry offsets and exact-length rejection of 0/191/193-byte entries; no process opened. Live mode only accepts the hardcoded delegated PID 11192 and uses existing ProcessReader with access 0x410. Reads are bounded to 10 seconds, 2 MiB requested bytes, 20000 calls, 16 players, 4096 registry entries/player and 128 queue length. Imports suppress bytecode writes; output stays in this production evidence directory. It does not write game memory, launch/resume/control the game, log in, inspect credentials, or change engine/runner files. An individual synchronous OS read is not forcibly preempted by the budget.

Artifacts: `production_probe.py`, `production-probe-01.json`, `probe.log`, `fixture.log` in this directory. JSON contains raw 192-byte entry, every selected component descriptor/RTTI and stable rechecks. Dynamic evidence is G/single paused sample only; R and D not_run. Lead separately reported post-fix maintenance-run-03 production observed 1 with EXIT5; that run was not executed by this diagnostic agent.
