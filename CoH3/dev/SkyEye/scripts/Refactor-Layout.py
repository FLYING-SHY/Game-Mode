# One-shot layout refactor finisher (2026-11-13, m03410): completes the
# licensing extraction after the interrupted first apply. Idempotent: every
# step is skipped when already applied. Writes evidence\refactor-manifest.json
# whose prefix_map is the authoritative old-path -> new-path mapping consumed
# by Finalize.py afterwards.
#
# HISTORICAL ONE-SHOT: describes the pre-refactor layout; must not run again.

import hashlib
import json
import os
import re
import subprocess
import sys
from pathlib import Path

GAME = Path(r'F:\WorkSpace\Game')
V2 = GAME / 'CoH3' / 'SkyEye-HardenedV2'
SKY = GAME / 'CoH3' / 'SkyEye'
LIC = GAME / 'Licensing'
LS = GAME / 'LicensingSystem'

CONFIG_FILES = [
    LS / '.local' / 'hardening-v2' / 'server.json',
    LS / '.local' / 'hardening-v2' / 'published-server.json',
]
CONFIG_FIELDS = ['DatabasePath', 'KeyDirectory']
OLD_PREFIX_RE = re.compile(r'F:(?:\\|/)+WorkSpace(?:\\|/)+Game(?:\\|/)+Licensing(?=(?:\\|/))')

EXPECTED_COUNTS = {
    'server': 74,
    'server-tests': 71,
    'packages': 82,
    'delivery-server': 36,
    'local-private': 62,
    'deployment': 3,
}


def die(msg):
    print('REFETCH-FAIL: ' + msg)
    sys.exit(1)


def move(src, dst, kind):
    if dst.exists():
        die('destination exists: ' + str(dst))
    if not src.exists():
        die('missing source: ' + str(src))
    os.rename(src, dst)
    print('moved: %s -> %s' % (src, dst))
    return {'from': str(src), 'to': str(dst), 'kind': kind}


def count_files(root):
    if not root.is_dir():
        die('missing tree: ' + str(root))
    return sum(1 for p in root.rglob('*') if p.is_file())


def sha256(path):
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(1 << 20), b''):
            h.update(chunk)
    return h.hexdigest()


def main():
    if V2.exists() or not SKY.is_dir():
        die('unexpected pre-state: rename not applied')
    print('state: rename applied (SkyEye)')

    moves = [{'from': str(V2), 'to': str(SKY), 'kind': 'dir',
              'note': 'applied in first interrupted run; resumed'}]

    already = [
        (SKY / 'server', LS / 'server'),
        (SKY / 'server-tests', LS / 'server-tests'),
        (SKY / '.cache' / 'packages', LS / '.cache' / 'packages'),
        (SKY / 'Directory.Build.props', LS / 'Directory.Build.props'),
        (SKY / 'Start-Server.ps1', LS / 'Start-Server.ps1'),
        (SKY / 'Initialize-Signing.ps1', LS / 'Initialize-Signing.ps1'),
        (SKY / 'Publish-Server.ps1', LS / 'Publish-Server.ps1'),
        (SKY / 'scripts' / 'Start-PublishedServer.ps1', LS / 'Start-PublishedServer.ps1'),
        (SKY / 'delivery' / 'server', LS / 'delivery' / 'server'),
        (LIC / '.cache' / 'dotnet-10', LS / '.cache' / 'dotnet-10'),
        (LIC / '.cache' / 'dotnet-10-installation.json', LS / '.cache' / 'dotnet-10-installation.json'),
    ]
    for src, dst in already:
        if src.exists() and dst.exists():
            die('both sides exist: ' + str(src))
        if dst.exists():
            print('resume: already moved ' + dst.name if src.is_dir() else dst.name)
            continue
        if src.is_dir():
            dst.parent.mkdir(parents=True, exist_ok=True)
        moves.append(move(src, dst, 'dir' if src.is_dir() else 'file'))

    remaining = [
        (LIC / '.local', LS / '.local', 'dir'),
        (LIC / 'deployment', LS / 'deployment', 'dir'),
        (LIC / '.gitignore', LS / '.gitignore', 'file'),
        (LIC / 'README.md', LS / 'README.md', 'file'),
    ]
    for src, dst, kind in remaining:
        if dst.exists() and not src.exists():
            print('resume: already moved ' + dst.name)
            moves.append({'from': str(src), 'to': str(dst), 'kind': kind,
                          'note': 'applied in first interrupted run; resumed'})
            continue
        if dst.exists():
            leftover = list(dst.iterdir()) if dst.is_dir() else None
            if leftover is None or leftover:
                die('destination exists and is not an empty skeleton: ' + str(dst))
            dst.rmdir()
            print('cleanup: removed empty skeleton ' + str(dst))
        moves.append(move(src, dst, kind))

    # junction recreated exactly once
    junction = SKY / 'sandbox'
    junction_target = SKY / 'verification' / 'sandbox'
    if junction.exists():
        print('resume: junction already present')
    else:
        import _winapi
        _winapi.CreateJunction(str(junction_target), str(junction))
        resolved = os.readlink(junction)
        if 'SkyEye' not in resolved:
            die('junction recreated with unexpected target: ' + resolved)
        print('recreated: sandbox junction (resolved: ' + resolved + ')')

    # remove empty shells
    for empty in (SKY / '.cache', LIC / '.cache', LIC):
        if not empty.exists():
            continue
        leftover = list(empty.iterdir())
        if leftover:
            die('expected empty shell has leftovers: ' + str(empty) + ' -> ' + repr(leftover[:5]))
        empty.rmdir()
    print('shells removed')

    # verify moved trees by file counts (all moves were same-volume renames:
    # content bytes cannot change; counts catch truncation/loss)
    count_map = {
        'server': LS / 'server',
        'server-tests': LS / 'server-tests',
        'packages': LS / '.cache' / 'packages',
        'delivery-server': LS / 'delivery' / 'server',
        'local-private': LS / '.local',
        'deployment': LS / 'deployment',
    }
    for name, root in count_map.items():
        n = count_files(root)
        if n != EXPECTED_COUNTS[name]:
            die('count mismatch for %s: %d != %d' % (name, n, EXPECTED_COUNTS[name]))
        print('verified %-16s %d files' % (name, n))
    if not (SKY / 'client').is_dir() or not (SKY / 'verification').is_dir() or not (SKY / 'evidence').is_dir():
        die('game tree incomplete after rename')
    if (SKY / 'server').exists() or (SKY / 'server-tests').exists():
        die('server trees still inside game project')

    sdk = LS / '.cache' / 'dotnet-10'
    sdk_files, sdk_bytes = 0, 0
    for p in sdk.rglob('*'):
        if p.is_file():
            sdk_files += 1
            sdk_bytes += p.stat().st_size
    if (sdk_files, sdk_bytes) != (5578, 812144112):
        die('SDK stats mismatch: %d/%d' % (sdk_files, sdk_bytes))
    ver = subprocess.run([str(sdk / 'dotnet.exe'), '--version'], capture_output=True, text=True, timeout=120)
    if ver.returncode != 0 or not ver.stdout.strip().startswith('10.'):
        die('SDK dotnet --version failed: rc=%s out=%r' % (ver.returncode, ver.stdout[:80]))
    print('verified SDK: %d files, %d bytes, dotnet %s' % (sdk_files, sdk_bytes, ver.stdout.strip()))

    # rewrite private config absolute paths
    config_changes = []
    for cfg in CONFIG_FILES:
        if not cfg.is_file():
            die('missing config: ' + str(cfg))
        text = cfg.read_text(encoding='utf-8-sig')
        before = json.loads(text)
        new_text, n = OLD_PREFIX_RE.subn(lambda m: m.group(0) + 'System', text)
        if n == 0 and 'LicensingSystem' in text and not OLD_PREFIX_RE.search(text):
            print('resume: config already rewritten ' + cfg.name)
            continue
        if n == 0:
            die('no path replaced in ' + str(cfg))
        if OLD_PREFIX_RE.search(new_text):
            die('old licensing prefix still present in ' + str(cfg))
        after = json.loads(new_text)
        updated = []
        for field in CONFIG_FIELDS:
            b, a = str(before[field]), str(after[field])
            if b == a or 'LicensingSystem' not in a:
                die('field %s not migrated in %s' % (field, cfg.name))
            updated.append(field)
        if str(before.get('AdminToken')) != str(after.get('AdminToken')):
            die('AdminToken changed unexpectedly in ' + cfg.name)
        cfg.write_text(new_text, encoding='utf-8')
        config_changes.append({'file': cfg.name, 'replacements': n, 'fields_updated': updated})
        print('config: %s -> %d prefix replacements (fields: %s)' % (cfg.name, n, ','.join(updated)))

    prefix_map = [
        {'from': str(V2 / 'server'), 'to': str(LS / 'server')},
        {'from': str(V2 / 'server-tests'), 'to': str(LS / 'server-tests')},
        {'from': str(V2 / '.cache' / 'packages'), 'to': str(LS / '.cache' / 'packages')},
        {'from': str(V2 / 'Directory.Build.props'), 'to': str(LS / 'Directory.Build.props')},
        {'from': str(V2 / 'Start-Server.ps1'), 'to': str(LS / 'Start-Server.ps1')},
        {'from': str(V2 / 'Initialize-Signing.ps1'), 'to': str(LS / 'Initialize-Signing.ps1')},
        {'from': str(V2 / 'Publish-Server.ps1'), 'to': str(LS / 'Publish-Server.ps1')},
        {'from': str(V2 / 'scripts' / 'Start-PublishedServer.ps1'), 'to': str(LS / 'Start-PublishedServer.ps1')},
        {'from': str(V2 / 'delivery' / 'server'), 'to': str(LS / 'delivery' / 'server')},
        {'from': str(V2), 'to': str(SKY)},
        {'from': str(LIC), 'to': str(LS)},
    ]
    manifest = {
        'created': '2026-11-13',
        'reason': 'm03410 licensing extracted to standalone project; game dir renamed SkyEye',
        'game_root': str(GAME),
        'moves': moves,
        'prefix_map': prefix_map,
        'verified_counts': {k: EXPECTED_COUNTS[k] for k in count_map},
        'sdk': {'files': sdk_files, 'bytes': sdk_bytes, 'dotnet_version': ver.stdout.strip()},
        'config_changes': config_changes,
        'junction_recreated': {'link': str(junction), 'target': str(junction_target)},
    }
    out = SKY / 'evidence' / 'refactor-manifest.json'
    out.write_text(json.dumps(manifest, indent=1), encoding='utf-8')
    print('manifest: ' + str(out))
    print('REFACTOR COMPLETE')


if __name__ == '__main__':
    main()
