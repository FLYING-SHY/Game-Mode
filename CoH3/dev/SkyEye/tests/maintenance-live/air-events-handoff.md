# Air event sampling handoff — task-3 retention fix and diagnostic fields

## Scope and trigger

All writes for this revision were confined to `F:\WorkSpace\Game\CoH3\dev\SkyEye\tests\maintenance-live\` (source, related tests/docs and local build output). No game/auth/GUI launch, process reads, engine edits, private/original writes, commit or push. Task-3 was read and claimed before modifications; karpathy-guidelines was reloaded.

Parent-reported live run25 (`maintenance-run-25-p47-prewarmed/maintenance-live.json`): 551 air queries / 83 positive polls, but128 leading empty frames filled retention, retainedDecoded=0, droppedFrames=423 and droppedDecoded=83. This worker did not open or rerun that external live evidence; it supplied the regression requirement.

## Diff summary

- `AirEventEvidence.cs`: unchanged128-frame /256-decoded /16-per-frame budgets. Always reserve2 slots for the first successful empty read and latest successful empty read (one slot if there has only been one empty read). Subsequent empty reads replace the old latest empty and increment droppedFrames. Up to126 earliest positive frames use the remaining capacity. Removing the old latest empty before appending preserves Add/read order; positive geometry is not displaced by leading empty polls. Positive overflow drops the entire frame and counts all its items. Existing per-frame/total decoded truncation remains unchanged, including positive metadata frames with decoded=[] after the decoded budget is exhausted.
- `AirEventSelfTests.cs`: expanded7 checks to18. Added long leading-empty → positive → trailing-empty fixture, interleaved chronological replacement, positive saturation before late empty reads, empty-only first/latest retention, decoded truncation with empty replacement, final accounting across fixtures, and600 mixed polls with invariants checked after each Add.
- `EventSampling.cs`: retention fix changed only the enabled-air explanatory Details string. Parent subsequently explicitly requested raw diagnostic fields; the sole source-line change for that supplement adds `item.Key, item.Owner, item.Offset` to the existing anonymous air payload at line74. These public ReadAirZones outputs are passed through without reconstruction: Key supports later flight: fallback versus cast-key diagnosis, Owner preserves the reader's attribution value, and Offset preserves its geometry value. Actual call routing, default projectile output, fixed MatchKey/LicenseEpoch/Allowed guards and cancellation/deadline logic are unchanged.
- `README.md`: new retention policy, exact accounting semantics,18-test coverage and551-poll fixture result; now also explains Key/Owner/Offset in opt-in `air.frames[].decoded`.
- `air-events-handoff.md`: current evidence and binary hash; this replaces the previous handoff after the parent's explicit continuation requests.

The retention fix changed no JSON field names or types. The follow-up adds only Key/Owner/Offset to the opt-in air decoded object; existing fields are preserved. `--air-events` remains optional/default false; all-sides maintenance diagnostic scope remains distinct from production enemy-only UI. No-event remains sample_absent, never gameplay PASS. All three unchanged selftest suites were rerun after the diagnostic-field supplement; no new live sampling was performed.

For N successful AirEventEvidence.Add calls:

- `N == retainedFrames + droppedFrames` (droppedFrames includes replaced empty frames).
- `decodedObservationCount == retainedDecoded + droppedDecoded`.
- `retainedDecoded == sum(frames[].decoded.Length)`; each frame retains at most16 decoded items.
- Empty replacement contributes0 to droppedDecoded. Retained frames stay in read/time order, with at most2 decodedCount=0 frames and126 positive frames.

## Verification

- Regression before fix: normal build succeeded, then new air selftest failed as expected at `FAIL long leading empties retain only first and latest and count replacements` (native exit1, captured in `air-retention-regression-before.log`).
- Final `Build.ps1`: success,0 warnings,0 errors (`build.log`).
- `--self-test`: exit0, passed20 (`self-test.log`).
- `--event-self-test`: exit0, passed16 (`event-self-test.log`).
- `--air-event-self-test`: exit0, passed18 (`air-event-self-test.log`). Total54 checks.
- Synthetic551-poll fixture:400 leading empties,83 one-item positive polls,68 trailing empties →85 retained frames /83 retained decoded, droppedFrames=466, droppedDecoded=0. Retained times:0,400..482,550. This is a structural regression fixture, not a replay of run25 or new live evidence.
- Positive-cap fixture:130 positives followed by3 empty reads →128 retained frames /126 decoded, droppedFrames=5, droppedDecoded=4, first/latest empty times130/132.
- Mixed truncation fixture:400 empty /17 positive polls of20 items /3 empty →19 frames /256 decoded, droppedFrames=401, droppedDecoded=84.
-600 mixed synthetic polls satisfy accounting, chronological order and all budgets after every Add.

The untracked runner directory has no Git baseline. Retention-fix before-edit copies are in `artifacts/air-retention-fix-before/` with `.before` suffixes; `air-retention-fix.diff` preserves that earlier five-file review diff. The subsequent diagnostic-field supplement has separate before-edit copies in `artifacts/air-payload-fields-before/`; `air-payload-fields.diff` is the final three-file review diff (EventSampling, README and handoff). Both diffs were reviewed, with no whitespace errors.

## Final binary

Executable: `F:\WorkSpace\Game\CoH3\dev\SkyEye\tests\maintenance-live\bin\Release\net48\SkyEye.Maintenance.Live.exe`

Matching intermediate: `F:\WorkSpace\Game\CoH3\dev\SkyEye\tests\maintenance-live\obj\Release\net48\SkyEye.Maintenance.Live.exe`

Both SHA256: `BB3D856A2E5683F4C14492E8A9FD385A47204010C93D61CE1BB56798D61EA564`

Handoff is final: no further runtime/source edits, rebuilds or live launch by this worker after delivery. Parent will review before any actual sampling. This worker collected only offline runner/build evidence.
