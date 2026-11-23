from pathlib import Path
p=Path(__file__).resolve().parent
s=(p/'hq_upgrade_probe.py').read_text(encoding='utf8')
s=s.replace('Only PID 11192; no login/game control.','Explicit PID/output; no login/game control.')
s=s.replace("p.add_argument('--self-test', action='store_true'); a = p.parse_args()", "p.add_argument('--self-test', action='store_true'); p.add_argument('--pid',type=int); p.add_argument('--out'); a = p.parse_args()")
s=s.replace('    source = HERE.parents[2]', "    if not a.pid or not a.out: p.error('--pid and --out required')\n    source = HERE.parents[2]")
s=s.replace('ProcessReader(11192)','ProcessReader(a.pid)').replace("'pid': 11192", "'pid': a.pid")
s=s.replace('Paused 19:18 HQ upgrade event reported by lead; this probe does not control UI or independently establish trigger/time progression.', 'External game controls must be correlated independently; this probe does not control UI or establish progression.')
s=s.replace("out=HERE/'hq-upgrade-probe-01.json'",'out=pathlib.Path(a.out)')
out=p/'production_queue_probe.py'
assert not out.exists()
out.write_text(s,encoding='utf8')
