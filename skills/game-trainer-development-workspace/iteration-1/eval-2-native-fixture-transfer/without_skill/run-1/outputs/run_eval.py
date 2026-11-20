"""Run the requested commands and retain actual stdout, stderr, exits and elapsed time."""
import datetime
import json
import os
from pathlib import Path
import subprocess
import sys
import time

out = Path(__file__).resolve().parent
lab = Path(r'F:\WorkSpace\Game\skills\game-trainer-development\scripts\fixture_lab.py')
(out/'temp').mkdir(exist_ok=True)
env = dict(os.environ, TMP=str(out/'temp'), TEMP=str(out/'temp'), PYTHONDONTWRITEBYTECODE='1')
commands = [
    ('create', [sys.executable, str(lab), 'create', '--output', str(out/'fixture')]),
    ('resolver', [sys.executable, str(out/'resolver.py'), '--directory', str(out/'fixture'), '--output', str(out)]),
    ('verify', [sys.executable, str(lab), 'verify', '--directory', str(out/'fixture'), '--result', str(out/'candidates.json')]),
    ('self-test', [sys.executable, str(lab), 'self-test']),
]
records = []
for name, command in commands:
    started = datetime.datetime.now(datetime.timezone.utc).isoformat()
    tick = time.perf_counter()
    result = subprocess.run(command, capture_output=True, text=True, env=env, cwd=out)
    elapsed = time.perf_counter()-tick
    (out/(name+'.stdout.log')).write_text(result.stdout, encoding='utf-8')
    (out/(name+'.stderr.log')).write_text(result.stderr, encoding='utf-8')
    parsed = json.loads(result.stdout)
    (out/(name+'.json')).write_text(json.dumps(parsed, indent=2)+'\n', encoding='utf-8')
    record = dict(step=name, command=command, started_utc=started, elapsed_seconds=elapsed,
                  exit_code=result.returncode, stdout_file=name+'.stdout.log', stderr_file=name+'.stderr.log')
    records.append(record)
    (out/'execution.json').write_text(json.dumps(dict(python=sys.version, executable=sys.executable, runs=records), indent=2)+'\n', encoding='utf-8')
    print(json.dumps(record), flush=True)
    if result.returncode:
        print(result.stdout, result.stderr)
        raise SystemExit(result.returncode)
print('All four commands completed with exit code 0.')
