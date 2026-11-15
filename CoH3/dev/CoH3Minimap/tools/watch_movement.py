"""Read-only movement watch: polls with the released probe, and when native mark
movement is detected, captures a long sample plus window screenshots for the
movement-sync acceptance. Never writes to the game or sends input."""
import argparse, hashlib, json, subprocess, sys
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PY = sys.executable

def run_checked(cmd):
    r = subprocess.run(cmd, capture_output=True, text=True)
    if r.returncode != 0:
        raise RuntimeError(f"{cmd[0]} exited {r.returncode}: {r.stderr.strip()[:400]}")
    return r

def probe(out: Path, samples: int, exe: Path):
    if out.exists():
        raise FileExistsError(out)
    run_checked([str(exe), '--native-probe', '--samples', str(samples), '--output', str(out)])

def analyze(probe_json: Path, out: Path):
    if out.exists():
        raise FileExistsError(out)
    r = run_checked([PY, str(ROOT / 'tools' / 'analyze_native_continuity.py'), str(probe_json), '--output', str(out)])
    return json.loads(r.stdout)

def screenshots(pids):
    r = run_checked([PY, str(ROOT / 'tools' / 'capture_native_windows.py'), *map(str, pids)])
    return json.loads(r.stdout)

def viewport_of(pj: Path):
    """Return a stable JSON fingerprint of PerspectiveBox within one probe file, or None."""
    if not pj.exists():
        return None
    data = json.loads(pj.read_text(encoding='utf-8-sig'))
    boxes = [s.get('Map', {}).get('Geometry', {}).get('PerspectiveBox') for s in data.get('Samples', [])]
    boxes = [b for b in boxes if b is not None]
    if not boxes:
        return None
    first = json.dumps(boxes[0], sort_keys=True)
    if any(json.dumps(b, sort_keys=True) != first for b in boxes[1:]):
        return None  # not stable within this poll; do not claim a clean step
    return first

def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--exe', type=Path, required=True, help='released CoH3Minimap.exe (read-only probe)')
    ap.add_argument('--game-pid', type=int, required=True)
    ap.add_argument('--window-pids', type=int, nargs='+', required=True)
    ap.add_argument('--output-dir', type=Path, required=True)
    ap.add_argument('--max-wait-minutes', type=float, default=30.0)
    ap.add_argument('--poll-seconds', type=float, default=5.0)
    ap.add_argument('--poll-samples', type=int, default=6)
    ap.add_argument('--burst-samples', type=int, default=100)
    ap.add_argument('--watch-viewport', action='store_true',
                    help='also trigger on PerspectiveBox camera-viewport changes (pan works while paused)')
    a = ap.parse_args()
    out = a.output_dir
    out.mkdir(parents=True, exist_ok=False)
    started = datetime.now().astimezone().isoformat()
    polls, movement_poll_index = 0, None
    deadline = datetime.now().timestamp() + a.max_wait_minutes * 60

    try:
        previous_viewport = None
        viewport_changed = False
        while datetime.now().timestamp() < deadline:
            polls += 1
            pj = out / f'poll-{polls:03d}-probe.json'
            cj = out / f'poll-{polls:03d}-continuity.json'
            probe(pj, a.poll_samples, a.exe)
            summary = analyze(pj, cj)
            step = summary.get('LargestObservedCoordinateStep')
            changes = summary.get('MarkPositionChanges')
            viewport = viewport_of(pj) if a.watch_viewport else None
            viewport_changed = bool(a.watch_viewport and previous_viewport is not None and viewport is not None and viewport != previous_viewport)
            if viewport is not None:
                previous_viewport = viewport
            print(json.dumps(dict(poll=polls, captured=summary.get('CapturedSamples'), changes=changes, step=step,
                                  viewport_changed=viewport_changed)), flush=True)
            if (changes is not None and (changes > 0 or (step or 0.0) > 0.0)) or viewport_changed:
                movement_poll_index = polls
                break
        result = dict(StartedAt=started, Polls=polls, MovementDetected=movement_poll_index is not None,
                      MovementPollIndex=movement_poll_index, PollSeconds=a.poll_seconds, PollSamples=a.poll_samples,
                      ViewportWatchEnabled=bool(a.watch_viewport), ViewportTriggered=viewport_changed)
        if movement_poll_index is not None:
            pj = out / 'burst-probe.json'; cj = out / 'burst-continuity.json'
            probe(pj, a.burst_samples, a.exe)
            burst = analyze(pj, cj)
            shots = screenshots([a.game_pid, *a.window_pids])
            result.update(BurstProbe=str(pj), BurstContinuity=str(cj), BurstSummary=burst, Screenshots=shots,
                          MovementSyncMeasured=bool(burst.get('MarkPositionChanges') or 0) > 0 and bool((burst.get('LargestObservedCoordinateStep') or 0.0) > 0.0))
        else:
            result['Timeout'] = True
    finally:
        result['CompletedAt'] = datetime.now().astimezone().isoformat()
        result['GameWritten'] = False; result['GameFunctionsInvoked'] = False; result['GameInputSent'] = False
        rp = out / 'watch-result.json'
        with rp.open('x', encoding='utf-8') as f:
            json.dump(result, f, ensure_ascii=False, indent=2); f.write('\n')
        print(json.dumps(dict(Result=str(rp), MovementDetected=result['MovementDetected']), ensure_ascii=False))
    return 0 if result['MovementDetected'] else 2

if __name__ == '__main__':
    raise SystemExit(main())
