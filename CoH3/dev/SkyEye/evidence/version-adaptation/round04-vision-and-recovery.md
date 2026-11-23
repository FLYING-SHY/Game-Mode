# Round 04: vision attempt, recon cooldown, camera recovery review

Candidate status unchanged. Game PID31572, first fresh-process offline US/German AI match. No baseline promotion or protected delivery.

## Native actions and actual samples

- Initial frame03:41 paused; scout portrait selected3/3. Running rightclick950,430 then25seconds advanced to04:10; native queued movement arrow and enemy spotted notification. Another35seconds advanced to04:46; scout destroyed notification and local population0. Do not call this arrival or sustained target-vision PASS.
- maintenance-run-22-scout-vision: real licenseactive, resources local2356.4375/518.166931/293.3614/pop0, no local units; enemy seven unit samples, four buildings, one production job,13upgrades, colours2/camera1/markers7. No support/projectile/air sample; innerexit5.
- Native recon tooltip names“蚱蜢”侦察巡航, vision along flight path. Running left-button drag at950,430 to1150,430 followed by direction-confirm click caused native recon button cooldown95 visible at05:16 in round04-recon-drag-paused.png (previous04:46 no cooldown). This adds independent evidence that a recon order was accepted, unlike earlier unconfirmed targeting-only attempts. It does not prove P47 activated or flight geometry correct.
- maintenance-run-23-recon-drag: real licenseactive;20second window184polls/184all-sides-airqueries/0decoded,deadline,innerexit5. Recon is not named in current AirCatalogue; no inference of broken P47 layout from this zero sample.
- Each advance finally restored foreground-checked tactical pause. Jobs pwsh-85d7fe74448d4fee,pwsh-6af91cac6afc47b4,pwsh-851ad102a8314dc8,pwsh-aec42945732f48bd collected outerexit0.

## Camera recovery: source intent, not live PASS

Read-only subagent camera-recovery-review loaded karpathy-guidelines. Source findings:
- PlayerInfoReader.cs:1122-1155 rediscovers cameraCore when0;1160-1201 reads pointer before/after matrix, throws exact 镜头已切换。 at1193 on mismatch;1199 clears cameraCore=0 then rethrows. Same object next call may rediscover; no local loop in this method.
- PlayerInfoReader.cs:216-229 LocalIdentity58 calls ReadMineCamera;2757-2764 and2846-2859 Snapshot identity checks propagate errors.
- OverlaySettingsForm.cs:565-593 creates reader only if null, calls Snapshot, Success resets recovery state. Its666-682 catch computes sameMatch using reader.IsCurrentMatch55 again, so camera can fail again during guard. Source implements conditional same-reader fast retry or Drop/reopen, not mandatory whole-GUI restart. No live forced-recovery test claimed.
- SquadMarkerWindow.cs:333-347 FreshCamera attempts same-reader camera up to3times in-frame;571-592 records failed frames and hides after5. AirOverlayWindow.cs:424-455 clears/hides on failure; first2retain reader, thirdDrop, nextframe opensnew, success clears errors at444-445. This is design evidence only.

Prior run20 failure plus new-reader run21 success therefore remains new-instance recovery evidence. Same-reader/GUI recovery and cache cleanup still open. No engine/auth/private/original changes.
