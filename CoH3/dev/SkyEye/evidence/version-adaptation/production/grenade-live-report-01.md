# Supported US grenade upgrade — actual countdown to completion

Status: bounded G evidence PASS for this upgrade path; overall adaptation remains candidate. No engine/source baseline change in this round.

## Scenario and native UI
- Game PID31572, first fresh process, offline US vs standard German AI, high resources1000VP, no mod.
- HQ F1; T queued air support center. Its60s/100MP/30fuel tooltip is `../fresh01-barracks-tooltip.png` despite historical misleading filename. After bounded64s resume, actual air support center completed, confirmed `../fresh01-z-actual-tooltip.png`; enables native air abilities for later tests.
- Barracks verified by correct screen-coordinate hover in `../fresh01-z-tooltip-corrected.png`: Z,120MP15fuel, tooltip explicitly says double-click enables automatic construction. Earlier manual placement attempts did not deduct construction resources and are not completed-building evidence. Double-click native Z button did deduct120MP15fuel and queued a construction icon (`../fresh01-barracks-auto-queued.png`). Bounded40s resume then pause; F2 selects completed barracks (`../fresh01-barracks-controls.png`).
- `../fresh01-grenade-tooltip.png`: 手榴弹装备包,25s,100MP10fuel; T triggers. No riflemen squad was built before research. We had air center active; this test does not establish whether it was a prerequisite.
- Research queued while paused initially gave null countdown (`../maintenance-run-15-grenade-start/maintenance-live.txt`). One-second bounded resume activates queue; paused screenshot and runner16 capture active research.

## Correlated raw/reader sequence
| Phase | Actual licensed report | Raw queue | Remaining ticks / total | Derived seconds |
|---|---|---|---|---|
| active | ../maintenance-run-16-grenade-active/maintenance-live.txt:22-29 | grenade-queue-01.json |187/200|24|
| after8s bounded resume | ../maintenance-run-17-grenade-middle/maintenance-live.txt:22-29 | grenade-queue-02.json |118/200|15|
| after18s further resume | ../maintenance-run-18-grenade-complete/maintenance-live.txt:22-29 | grenade-queue-03-complete.json | queue count0, remaining null | null |

- Same queue `0x1fd71d92000`; active/middle same entry `0x1fd72efff00`, item `0x1fd6f2ed300`.
- EntityUpgradeProductionItem RTTI `.?AVEntityUpgradeProductionItem@@`, vtableRva0x3d92160/COL0x505a840/TD0x5f9abc0. Exact blueprint `upgrade\american\research\barracks\grenade_riflemen_us`, host `ebps\races\american\buildings\production\barracks_us`, owner slot0.
- Both active raw captures have stable queue/root rechecks, no read failures; total200ticks agrees with25s tooltip at inherited8ticks/s. Raw reader ticks match licensed reader exactly. Actual elapsed intervals include input/capture overhead; no claim of exact8.000s interval.
- Completed report adds exact grenade blueprint to local player's completed `Upgrades`, previously absent; `ProductionKnown=true`, countdownnull. `../fresh01-grenade-complete-paused.png` at02:22 explicitly shows 手榴弹装备包已完成 plus empty tactical queue and green completed tick.
- `verify_grenade_sequence.py` asserts same queue/entry/item, raw and product-reader equality, decreasing187→118, total200, completed list transition and emptyqueue; `grenade-sequence-validation.json` PASS.

## Probe and verification hygiene
- `production_queue_probe.py` is a separately named derived historical bounded probe with explicit --pid/--out. Historical hq_upgrade_probe.py/output preserved. Access0x410 read-only,10s/2MiB/20000read bounds preserved. Selftest passes192B layout plus lengths0/191/193 rejection.
- Raw captures1398/1488/1533reads,26497/28134/28662successful bytes,zero failures/stable roots. No injection/process writes.
- Two initial inline generator commands failed before file creation because PowerShell native argument quote parsing; resolved by file-based Python generator `create_queue_probe.py`. No engine edits.
- All actual runnerslicenseactive/innerexit5 because other feature event samples remain absent. All background jobs collected, including pwsh-d181c3e69174449d/pwsh-7cfb0f1119c34419/pwsh-9e5765b82e8e4bdf/pwsh-7bba7eaa761d480e/pwsh-283a336b7f6b4380/pwsh-f441dd0d66934b83. git diff --check clean; mirrored GameLayout hashes equal.

## Limits and next state
- UI numeric countdown agreement, sound alert and rendered upgrade text were not independently tested; do not promote these from raw/reader evidence.
- Kwk38 uses AK250 SimInventoryItemExt→SquadUpgradeProductionItem and remains pending in separate AK scenario.
- Dynamic projectiles, support/air, second fresh game process, full lifecycle and dual protected delivery still pending. SupportedHash unchanged.
- Game PID31572 paused02:22 in normal view with completed barracks selected. No maintenance clients running. Air center native abilities now unlocked (left HUD Ctrl+F7/F8/F9) for subsequent event sampling.
