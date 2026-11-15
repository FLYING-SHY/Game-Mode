from pathlib import Path
import hashlib
import json
import re
import zipfile

ROOT = Path(__file__).resolve().parents[1]
report_path = ROOT / 'dist' / 'validation-0.5-native-preview-20261112-004209.json'
report = json.loads(report_path.read_text(encoding='utf-8-sig'))
exe = Path(report['Exe'])
archive_path = Path(report['Zip'])
sha = lambda data: hashlib.sha256(data).hexdigest().upper()
assert sha(exe.read_bytes()) == report['ExeSha256']
assert sha(archive_path.read_bytes()) == report['ZipSha256']
expected = {'CoH3Minimap.exe': exe, 'README.md': ROOT / 'README.md', 'THIRD-PARTY-NOTICES.md': ROOT / 'THIRD-PARTY-NOTICES.md'}
expected.update({'licenses/' + p.name: p for p in (ROOT / 'licenses').iterdir() if p.is_file()})
with zipfile.ZipFile(archive_path) as archive:
    assert set(archive.namelist()) == set(expected)
    assert archive.testzip() is None
    for name, source in expected.items():
        assert archive.read(name) == source.read_bytes(), name
    for name in ['README.md', 'THIRD-PARTY-NOTICES.md']:
        for target in re.findall(r'\]\(([^)]+)\)', archive.read(name).decode('utf-8')):
            if not target.startswith(('https://', 'http://', '#')):
                assert target in archive.namelist(), (name, target)
isolated = ROOT / 'artifacts' / 'isolated-release-20261112-0048'
assert sha((isolated / 'CoH3Minimap.exe').read_bytes()) == report['ExeSha256']
assert len(list(isolated.glob('*.dll'))) == 0
assert (isolated / 'selftest.stdout.txt').read_text(encoding='utf-8-sig').count('PASS') == 13
assert (isolated / 'selftest.stderr.txt').stat().st_size == 0
comparison = Path(report['StaticSceneComparisonImage'])
assert sha(comparison.read_bytes()) == report['StaticSceneComparisonImageSha256']
assert report['FogActiveUiCompositionVerified'] is False
assert report['SameSceneVisualComparisonPassed'] is False
assert report['LevelBAccepted'] is False
report.update(ArchiveFileCountVerified=len(expected), ZipCrcVerified=True, AllArchiveFilesMatchSources=True, PackagedMarkdownLinksVerified=True, IsolatedExeSelftestExitCode=0, IsolatedExeNoSidecars=True)
report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'archive_files_verified': len(expected), 'license_files_verified': len(expected) - 3, 'zip_crc_verified': True, 'all_archive_files_match_sources': True, 'standalone_exe_selftest': '13 PASS', 'live_samples': report['PublishedExeLiveProbeCapturedSamples'], 'level_b_accepted': False}, indent=2))
