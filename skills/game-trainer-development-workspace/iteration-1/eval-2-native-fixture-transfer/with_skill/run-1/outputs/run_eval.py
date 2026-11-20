import datetime
import hashlib
import json
import os
from pathlib import Path
import platform
import subprocess
import sys
import time

OUT = Path(__file__).resolve().parent
ROOT = Path(r'F:\WorkSpace\Game')
LAB = ROOT / 'skills/game-trainer-development/scripts/fixture_lab.py'
FIXTURE = OUT / 'fixture'
TMP = OUT / 'temp'
TMP.mkdir(exist_ok=True)
env = dict(os.environ, TMP=str(TMP), TEMP=str(TMP), TMPDIR=str(TMP), PYTHONDONTWRITEBYTECODE='1')
commands = [
    ('create', [sys.executable, str(LAB), 'create', '--output', str(FIXTURE)]),
    ('resolver', [sys.executable, str(OUT / 'resolver.py'), '--directory', str(FIXTURE), '--output', str(OUT)]),
    ('verify', [sys.executable, str(LAB), 'verify', '--directory', str(FIXTURE), '--result', str(OUT / 'candidates.json')]),
    ('self-test', [sys.executable, str(LAB), 'self-test']),
]
runs = []
for name, argv in commands:
    started = datetime.datetime.now(datetime.timezone.utc).isoformat()
    tick = time.perf_counter()
    p = subprocess.run(argv, cwd=ROOT, env=env, capture_output=True, text=True, encoding='utf-8')
    duration = time.perf_counter() - tick
    (OUT / (name + '.stdout.log')).write_text(p.stdout, encoding='utf-8')
    (OUT / (name + '.stderr.log')).write_text(p.stderr, encoding='utf-8')
    runs.append({'name': name, 'argv': argv, 'cwd': str(ROOT), 'started_utc': started,
                 'elapsed_seconds_measured': duration, 'exit_code': p.returncode,
                 'stdout': name + '.stdout.log', 'stderr': name + '.stderr.log'})
    print(name, 'exit', p.returncode, flush=True)
    if p.returncode:
        print(p.stdout, p.stderr)
        break
inputs = [ROOT / 'AGENTS.md', LAB, ROOT / 'skills/game-trainer-development/SKILL.md',
          ROOT / 'skills/game-trainer-development/references/fixture-lab.md',
          ROOT / 'skills/game-trainer-development/references/engine-routes.md',
          OUT / 'resolver.py']
identity = [{'path': str(p), 'bytes': p.stat().st_size,
             'sha256': hashlib.sha256(p.read_bytes()).hexdigest()} for p in inputs]
(OUT / 'execution.json').write_text(json.dumps({'python': sys.version, 'executable': sys.executable,
    'platform': platform.platform(), 'inputs': identity, 'runs': runs,
    'token_usage': 'not exposed; not measured'}, indent=2) + '\n', encoding='utf-8')
lines = ['# case2 with_skill 执行简记', '',
         '读取会话技能目录，加载 karpathy-guidelines；读取根 AGENTS.md、game-trainer-development/SKILL.md、fixture-lab.md、engine-routes.md 与 fixture_lab.py。只使用 eval_metadata.json 的 prompt；未读取其他运行输出。', '',
         '以下命令与退出码来自实际 subprocess 执行；完整 stdout/stderr 单独保存。self-test 的临时目录通过 TMP/TEMP/TMPDIR 限定在 outputs/temp。', '']
for r in runs:
    lines.extend(['```text', subprocess.list2cmdline(r['argv']), '```',
                  f"开始 UTC：{r['started_utc']}；实测耗时：{r['elapsed_seconds_measured']:.6f} 秒；退出码：{r['exit_code']}。", ''])
lines.extend(['resolver 只向发现阶段提供 manifest 中的容器元信息（session/build、base、文件名、SHA256），扫描 AOB 并解码两个 displacement；逐字节扫描三帧 int32 候选，扫描 manager 后所有可读 uint64 指针槽，依靠变化序列和结构交集消歧。先保存候选，之后独立函数才读 ground truth 核对。', '',
              '输入材料公开了生成器和答案布局，因此属于已知协议/材料的模型方法验证，并非真正盲测。未执行真实进程或游戏测试。token 数未暴露，未估造。'])
(OUT / 'transcript.md').write_text('\n'.join(lines) + '\n', encoding='utf-8')
raise SystemExit(0 if len(runs) == 4 and all(r['exit_code'] == 0 for r in runs) else 1)
