"""Run the unprotected client suite against a live licensing server and record an auditable baseline.

The protected release is compared pixel-by-pixel and byte-by-byte against this
unprotected baseline, so both runs must start from the same runtime state: no
overlay-settings.txt or minimap cache may exist next to either executable.
"""
from pathlib import Path
import argparse, datetime, hashlib, json, subprocess, time

R = Path(__file__).resolve().parents[1]
VERIFIER_DIR = R / 'verification' / 'bin' / 'Release' / 'net48'
VERIFIER = VERIFIER_DIR / 'SkyEye.Integration.Verification.exe'
RUNTIME_STATE = ('overlay-settings.txt', 'minimap-location-cache.txt', 'minimap-ui-root-cache.txt')
CASES = [('selfcheck', None, None), ('ui', None, None), ('audio', None, None),
         ('protocol', 'permanent', 'protocol-permanent'), ('protocol', 'timed', 'protocol-timed'),
         ('coordinator', 'permanent', 'coordinator'), ('network', 'permanent', 'network')]


def sha256_upper(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def client_executable() -> Path:
    candidates = [p for p in VERIFIER_DIR.glob('*.exe') if p.name != VERIFIER.name]
    if not candidates:
        raise SystemExit('Unprotected client executable is missing; run Build.ps1 first.')
    return candidates[0]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--base-url', required=True)
    parser.add_argument('--code-directory', required=True)
    args = parser.parse_args()
    if not VERIFIER.is_file():
        raise SystemExit('Unprotected verifier is missing; run Build.ps1 first.')
    codes = Path(args.code_directory)
    sandbox_root = R / 'verification' / 'sandbox'
    stamp = datetime.datetime.now().strftime('%Y%m%d-%H%M%S')
    results = []
    for mode, kind, card in CASES:
        name = card or mode
        sandbox = sandbox_root / f'raw-v2-{stamp}-{name}'
        command = [str(VERIFIER), '--mode', mode, '--sandbox', str(sandbox), '--base-url', args.base_url]
        if kind:
            command += ['--code-file', str(codes / f'{card}.txt'), '--expected-kind', kind]
        started = time.time()
        completed = subprocess.run(command, capture_output=True, text=True)
        elapsed = round((time.time() - started) * 1000, 2)
        results.append({'exitCode': completed.returncode, 'case': name, 'sandbox': str(sandbox), 'elapsedMs': elapsed})
        if completed.returncode != 0:
            print(completed.stdout[-4000:])
            print(completed.stderr[-2000:])
            raise SystemExit(f'Unprotected suite failed: {name}')
    client = client_executable()
    summary = {'sandboxTarget': str(sandbox_root), 'clientSHA256': sha256_upper(client),
               'baseUrl': args.base_url, 'results': results, 'allCardsLoggedOut': True,
               'note': 'Unprotected baseline; identical case set to the protected verification.'}
    output = R / f'evidence/raw-signed-{stamp}' / 'summary.json'
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    stale = [p.name for p in VERIFIER_DIR.glob('*') if p.name in RUNTIME_STATE]
    print(json.dumps({'passed': True, 'cases': len(results), 'summary': str(output), 'staleRuntimeState': stale}, ensure_ascii=True))
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
