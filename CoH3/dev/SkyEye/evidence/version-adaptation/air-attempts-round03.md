# Round 03: bounded air diagnostic and native activation attempts

Status: candidate only; no air gameplay PASS, no baseline promotion, no protected delivery.

## Runner closure

Agent air-event-runner changed only tests/maintenance-live. Optional --air-events requires --event-seconds 1..60; default output unchanged. Uses actual ReadAirZones with all current Snapshot player rows, explicitly diagnostic rather than production enemy-only UI. Fixed MatchKey/LicenseEpoch checked before/after Snapshot, projectile and air reads; cancellation/deadline between calls. Air independently reports successful empty frames, retained128frames/256items/max16perframe with precise dropped counters. Zero decoded observations = sample_absent.

Build0warnings0errors; original20selftests,16eventselftests,new7airselftests pass; standalone --air-events returns2 before initialization. Lead verified EXE SHA25640E954B3C0404EA9362EFFE9287D2A7F7BAC20B1CCBDF7BDD1C4AD9347F1290F and reviewed EventSampling.cs. Handoff: ../../tests/maintenance-live/air-events-handoff.md (resolve from project if using this relative reference).

## Native game actions and evidence limits

PID31572 remains same fresh process. At02:22 paused, P47 strafe tooltip showed first-use-free and unlocked air center. Clicks selected red corridor; fresh01-air-aim-direction.png explicitly rejects headquarters-area targeting. Native minimap camera click200,817 moved camera to central river; fresh01-air-central-queued.png explicitly rejects fog-of-war and requires target vision. Neither is an activated event.

Attempted native recon (CtrlF7 button, client30,580) against central terrain. Green targeting corridor and rotation by left-button drag observed, but no independently confirmed activation/cooldown/plane/vision change. Thus filenames containing queued/confirm are action labels, NOT proof of accepted orders. Ten-second resume advanced02:22→02:33 and restored pause, no plane proof. Arrow-up camera attempt while paused did not visibly pan; minimap click did.

Lead surgically added120ms pointer-settle,40ms buttonhold and foreground recheck before mouse buttons to evidence/version-adaptation/game_input.py. No game-memory writes. This addresses potential input event ordering, not a proven root cause.

## Actual real-license sampling

- maintenance-run-19-air-recon: licenseactive;20second event window184polls/184airqueries,0decoded,deadline. Innerexit5, incomplete. Paused targeting then resume only; no accepted air trigger proven.
- maintenance-run-20-air-running-recon: licenseactive;48successful airqueries,0decoded;49polls then System.Exception: 镜头已切换。 Innerexit3, genuine failure retained, not sample_absent-only outcome. Stack: PlayerInfoReader.ReadMineCamera → LocalIdentity58 → Snapshot → SampleEvents → EventPolling.Run → Probe. Source PlayerInfoReader.cs:1191 checks Q(cameraCore+416)!=num6;1193 throws;1199 resets cameraCore=0 before rethrow. No catch/retry policy or engine layout changed to hide this error.
- maintenance-run-21-air-direction: newly opened real licensed reader after prior failure;184polls/184airqueries,0decoded,deadline,innerexit5. Snapshot/resources2,units8,buildings4,upgrades13,colours2,camera1,markers8 observed; this is next-reader recovery evidence only, not same-reader/UI recovery. No air event PASS.

All foreground-checked commands restored pause in finally. End fresh01-air-event03-paused.png at approximately03:35, central camera, no clients; process responds. Jobs pwsh-db8f10f76d75479c,pwsh-998cce4ad06a470c,pwsh-efc4a5832071406a,pwsh-9768057e7a094e39 all collected outerexit0; inner runner exits documented above. git diff --check clean.

## Next acceptance work

Obtain native vision via scout movement or independently confirmed recon, then accepted P47 strafe with cooldown/resource/plane evidence and simultaneous air sampling. Zero reads here cannot distinguish unactivated ability, uncatalogued recon, or missing layout. Investigate camera-change recovery separately without weakening identity checks. Continue second fresh process, remaining queue family, dynamic projectile and protected deliveries only after sufficient evidence. original/private/ProductId untouched.
