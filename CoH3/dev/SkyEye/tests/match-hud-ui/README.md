# HUD context / production UI fixture

This directory contains isolated F-level builds and records of the normal Release login entry points. No result here establishes game features, cross-session behavior, or final delivery acceptance.

## Reproducible checks (repository root)

```powershell
& .\CoH3\dev\SkyEye\tests\match-hud-ui\Run.ps1
& .\CoH3\dev\SkyEye\tests\match-hud-ui\Build-Clients.ps1
```

The scripts use separate `obj` / `bin` directories, pin the production sources before and after the operation, and stop when an existing result file would be replaced. Preserve an earlier run instead of overwriting its report. Both commands were run using .NET SDK 10.0.404. The initial fixture compile error (ambiguous `Label`) was fixed by an explicit WinForms alias; the four-client build script now has a UTF-8 BOM for Windows PowerShell 5.1.

- [Fixture result](<fixture-result.json>): SkyEye 54 and Simple 49 explicit checks, plus shared session-reset checks (76 / 60). The fixture links the real production forms and invokes their shared cleanup path without form constructors, window handles, renderer threads, game-process handles, or changes to the normal license gate.
- [Fixture source](<Program.cs>): four typed HUD outcomes retain the reader and discovery progress, clear transient state, advance scene publications, reject late presentation, and allow a later qualified key. Compiled IL checks verify typed handlers and the local typed catch around qualification in the ordinary error path. These checks do not execute production polling against the game.
- [Build report](<clients-build-report.json>): SkyEye and Simple in Release / Maintenance, four build exits and four 48-check gate fixture exits are zero. Simple retains its existing CS0649 warnings. Maintenance factory presence is checked separately from Release. Binaries are test artifacts under `clients/`.

## Normal Release entry observations

Both newly built Release executables were launched through their standard application entry points. The pre-existing masked activation field was submitted through the normal UI event path. No credential field value was read or exported.

| Client | Observation record | Selected-window screenshot | Visible result |
|---|---|---|---|
| Simple | [Record](<simple-normal-login-observation.json>) | [Screenshot](<simple-normal-window-02.png>) | 本次登录已解除，请重新确认激活码。 |
| SkyEye | [Record](<skyeye-normal-login-observation.json>) | [Screenshot](<skyeye-normal-window-02.png>) | 本次登录已解除，请重新确认激活码。 |

The root agent inspected both screenshots and sanitized UI Automation status labels. The screenshot sidecars, launch records, submit records, executable hashes, and image hashes bind each observation to its isolated Release build. No network response was captured; the records do not infer a particular server-side rejection condition from the label. Neither application reached its protected control view. Product Reader execution, feature validation, overlay composition, cross-session validation, and delivery acceptance remain `not_run` / `false`.

The historical cross-match native measurements have a separate [read-only observation index](<../../evidence/version-adaptation/session-lifecycle-20261122/round3-live/round3-v4-findings.md>). They use the capture-time Reader source pin and retain `phaseDecision=not_evaluated` / `productAcceptance=false`; they do not substitute for these normal product entry points.
