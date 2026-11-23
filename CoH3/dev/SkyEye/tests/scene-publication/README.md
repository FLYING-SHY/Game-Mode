# Scene-publication generation fixture

Run from the repository root:

```powershell
& .\CoH3\dev\SkyEye\tests\scene-publication\Run.ps1
& .\CoH3\dev\SkyEye\tests\scene-publication\Build-Clients.ps1
```

`Run.ps1` source-links each tree's actual `ScenePublication<T>` into a net10.0 BCL-only fixture. Both trees pass 26 checks. It deterministically exercises a paused old frame, clear plus same-key replacement, rejection of the old commit, ordinary refresh, repeated clear, serialization of an already-started commit against clear, reentrant paint, callback exceptions, and mirrored source/wiring. It does not construct game readers, WinForms windows, or native renderers.

`Build-Clients.ps1` compiles the complete net48/x64 clients in Release and MAINTENANCE_DIAGNOSTICS into isolated `clients/{SkyEye,Simple}/{Release,Maintenance}` directories. It runs the existing 48-check build-gate fixture against each resulting binary and rejects production source changes during compilation. The result is recorded in `clients-build-report.json`. This script contains a UTF-8 BOM for the host's Windows PowerShell 5.1 path decoding; the helper-only script is ASCII.

Source before-copies and hashes are in `before/`. They are excluded from compilation by explicit compile items. No protected builds or release packages are produced here.

The production generation is advanced on every clear, including repeated clears and same-key reconnects. Normal snapshot refreshes keep the generation. Both worker windows drop their child reader/cache when the captured generation changes. Final native presentation and queued squad painting use `TryPresent` under the same gate. A clear waits for an already-started presentation, and an old frame cannot start another presentation after that clear returns.

These are F-level concurrency and compilation checks. A previously visible HWND may remain until the worker's next frame hides it. A result page with a still-successful Snapshot does not automatically trigger a clear: reliable match-activity detection, native GUI behavior, results-page cleanup, pause/resume, and real cross-match recovery still need actual-game validation. This fixture does not establish them.
