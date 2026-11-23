"""Capture the unprotected and protected settings-form renders back to back and compare them.

The verification UI check renders OverlaySettingsForm, which attaches to a running
RelicCoH3 process when one exists. When live rows are present the preview line paints
number colours taken from the live match, so a capture taken while the operator plays
differs from a capture taken in a quiet window. The protection itself is not the cause:
the same two builds render identical images under identical conditions.

This tool therefore captures both variants within the same second and records the
pixel comparison of main/expanded/compact together with the environment it observed.
"""
from pathlib import Path
import argparse, datetime, hashlib, json, subprocess, sys
from PIL import Image, ImageChops

R = Path(__file__).resolve().parents[1]
VERIFICATION = R / 'verification'

ap = argparse.ArgumentParser(description=__doc__)
ap.add_argument('--stamp', default=datetime.datetime.now().strftime('%Y%m%d-%H%M%S'))
ap.add_argument('--out', default=None, help='Evidence file under evidence/ (default: evidence/ui-pair-<stamp>.json).')
args = ap.parse_args()

variants = {
    'raw': VERIFICATION / 'bin' / 'Release' / 'net48' / 'SkyEye.Integration.Verification.exe',
    'release': VERIFICATION / 'bin' / 'Protected' / 'net48' / 'SkyEye.Integration.Verification.exe',
}
for name, exe in variants.items():
    if not exe.is_file():
        raise SystemExit('Missing verifier: ' + str(exe))

sandboxes = {}
for name, exe in variants.items():
    sandbox = VERIFICATION / 'sandbox' / ('ui-pair-' + args.stamp + '-' + name)
    if sandbox.exists():
        raise SystemExit('Sandbox already exists: ' + str(sandbox))
    completed = subprocess.run([str(exe), '--mode', 'ui', '--sandbox', str(sandbox)],
                               capture_output=True, text=True)
    if completed.returncode != 0:
        raise SystemExit('UI capture failed for ' + name + ': ' + completed.stdout + completed.stderr)
    sandboxes[name] = sandbox

pictures = []
for name in ['main', 'expanded', 'compact']:
    a = sandboxes['raw'] / (name + '.png')
    b = sandboxes['release'] / (name + '.png')
    with Image.open(a) as im_a, Image.open(b) as im_b:
        if im_a.size != im_b.size:
            raise SystemExit('Image size mismatch for ' + name)
        diff = ImageChops.difference(im_a.convert('RGB'), im_b.convert('RGB'))
        changed = sum(1 for pixel in diff.get_flattened_data() if pixel != (0, 0, 0))
        pictures.append({'name': name, 'size': list(im_a.size), 'changedPixels': changed,
                         'rawSha256': hashlib.sha256(a.read_bytes()).hexdigest(),
                         'releaseSha256': hashlib.sha256(b.read_bytes()).hexdigest()})

record = {
    'capturedAt': datetime.datetime.now(datetime.timezone.utc).isoformat(),
    'stamp': args.stamp,
    'method': 'unprotected and protected verifiers rendered back to back within the same second; both attached to the same environment',
    'rawVerifier': str(variants['raw']),
    'releaseVerifier': str(variants['release']),
    'sandboxes': {name: str(path) for name, path in sandboxes.items()},
    'pictures': pictures,
    'identical': all(item['changedPixels'] == 0 for item in pictures),
}
output = Path(args.out) if args.out else R / 'evidence' / ('ui-pair-' + args.stamp + '.json')
if output.exists():
    raise SystemExit('Refusing to overwrite existing pair evidence.')
output.write_text(json.dumps(record, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'out': str(output), 'identical': record['identical'],
                  'changedPixels': [item['changedPixels'] for item in pictures]}, ensure_ascii=True))
sys.exit(0 if record['identical'] else 1)
