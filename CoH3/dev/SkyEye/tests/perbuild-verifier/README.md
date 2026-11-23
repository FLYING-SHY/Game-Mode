# Per-build verifier regression (2026-11-22)

This suite verifies the material-consistency checker only. It does not launch or attach to a game, use the network, read private licensing state, or establish actual-game / cross-session / delivery acceptance.

## Changed behavior

- Inspect every present `exitCode` below `suites`, including nested mode dictionaries and lists. A non-zero exit code fails the fixture check even when recorded assertion counts match.
- Require every configured `requiredSubstrings` entry to occur somewhere across the configured runtime logs. Existing forbidden-string and missing-file checks remain active. An empty required-string specification fails explicitly.
- JSON reports declare `verificationScope = recorded_material_consistency_only` and an explicit acceptance limitation. The console summary states the same limitation.
- `--no-write` preserves report-write suppression and now prints `report: not written (--no-write)`.

No baseline, historical acceptance record, game hash policy, build dispatch, or release artifact is changed by this work.

## Before snapshot

`before/verify_perbuild_adaptation.py` is the exact script copied before editing.

SHA256: `1ABEB47C015AF72A0FB88B66F62DCD1DDB2EFE0A262BD07B6213EDDCF9F9486F`.

Two regression tests load this preserved snapshot and reproduce its old false PASS results before checking the corrected implementation:

1. Nested `layout-preflight.Release.exitCode = 7`, assertion counts still 104: before PASS, corrected FAIL.
2. One `draw` string, three other empty logs, required `visible` absent: before PASS, corrected FAIL.

## Reproduction

From the repository root:

```powershell
python -B CoH3/dev/SkyEye/tests/perbuild-verifier/test_perbuild_verifier.py
python -B CoH3/dev/SkyEye/scripts/verify_perbuild_adaptation.py --root F:\WorkSpace\Game --no-write
```

All data used by the regression suite is in memory. `Path.exists`, evidence reads, runtime-log decoding, and report writes are mocked where exercised. Running with `-B` avoids bytecode files. The normal report-writing branch is tested through a mock; it does not create a report.

Observed on 2026-11-22: **15 tests passed**. Coverage includes positive nested modes, top-level and nested failures, list-mode failures, multiple failures, expected-count mismatch, required strings distributed across logs, missing first/second required strings, forbidden strings, missing logs, empty requirements, report scope, no-write suppression, and non-zero failure exit.

The no-write check immediately after this change returned **13/13 recorded-material checks passed**, with 1,309 log lines and 941 minimap draws. This records the materials present at that instant; it is not a new G/R/D acceptance result. Later source changes may intentionally differ from the existing baseline, which this task leaves unchanged.
