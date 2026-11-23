# Controlled infantry movement / stop observation

Game remains original PID11192, original offline AI match. Candidate only; not new baseline. No process writes. Foreground-checked game input and licensed maintenance reader were used separately.

## Evidence
- Before: [run10](<maintenance-run-10-hq-upgrade/maintenance-live.txt#L46>), local engineer position (-111.75,24.968071,91.75), Movement=null.
- Input: selected engineer4/4 via visible unit icon; opened tactical map, resumed, issued client right-click(822,738), then restored pause and closed tactical map. [command frame](<engineer-move-command.png>), [paused tactical frame](<engineer-move-map-paused.png>). The latter contains the tactical-pause command warning; a warning persisting after pause is not itself proof of failure/success of the preceding running-state input.
- After input: [run11](<maintenance-run-11-engineer-move/maintenance-live.txt#L46>) local engineer position(-111.393463,24.8977585,90.50346), Movement=(-100.547348,24.5778122,69.72629). Valid actual reader result, position changed and nonnull target appeared.
- Stop: brief resume, W (visible engineer stop command), restore pause. [stop command](<engineer-stop-command.png>), [paused result](<engineer-stop-paused.png>).
- After stop: [run12](<maintenance-run-12-engineer-stop/maintenance-live.txt#L46>) local engineer position(-110.755867,24.76283,88.42286), Movement=null. This is controlled stop clearing, not arrival.
- Both run11/run12 have license active, no feature read failures, incomplete aggregate exit5 due other sample_absent/not_run features. Those exit codes do not assert whole-suite success.

## Single-point tactical consistency
Earlier measured tactical transform: XX=-1.7302424e-7, XY=YX=-1.979166509, YY=1.7302424e-7, TX=959.999989, TY=539.999995. Projecting run11 target X=-100.547348,Z=69.72629 gives client(822.00006843230688,738.99995079470648), relative to issued click(822,738): error(+0.00006843230688,+0.99995079470648) pixels. This is one actual command-target/map-transform consistency observation within about1pixel. Navigation snapping/rasterization may contribute. Not a universal terrain/world-camera/DPI/multi-map validation.

## Remaining
Rendering of the movement line/end marker, endpoint arrival and automatic disappearance, vehicle movement and alternate component modes, second match/fresh game process remain unverified. This observation neither upgrades the candidate hash nor constitutes protected-delivery acceptance.
