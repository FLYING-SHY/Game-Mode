"""Verify linked local evidence and release preservation; never open the game process."""
from pathlib import Path
from datetime import datetime
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[1]
RESEARCH = ROOT.parent / 'CoH3FowTool/tools/reverse/native_fow_20261111'
OUT = ROOT / 'artifacts/research-round2-20261112'
EXPECTED_SOURCE_SHA = '2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde'


def load(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    output = OUT / 'round2-verification.json'
    if output.exists():
        raise FileExistsError(output)
    report = OUT / 'ROUND2_REVIEW.md'
    text = report.read_text(encoding='utf-8')
    links = [(report.parent / target).resolve() for target in re.findall(r'\]\(<([^>]+)>\)', text)]
    assert links and all(path.is_file() for path in links), 'Missing local evidence link'
    comparison = load(OUT / 'native-paused-comparison.json')
    for path_key, digest_key in [('GameCapture', 'GameCaptureSha256'), ('ExternalCapture', 'ExternalCaptureSha256'), ('Output', 'OutputSha256')]:
        assert sha(Path(comparison[path_key])) == comparison[digest_key]
    assert comparison['PixelExactComparisonMeasured'] is False
    assert comparison['MovementSyncMeasured'] is False and comparison['LevelBAccepted'] is False
    analysis_path = ROOT / 'artifacts/probe-timeline-20261112/runtime-43932-20261112-0954-analysis.json'
    analysis = load(analysis_path)
    assert sha(Path(analysis['Source'])).upper() == analysis['SourceSha256']
    assert analysis['RequestedSamples'] == analysis['AttemptedSamples'] == analysis['CapturedSamples'] == 30
    assert analysis['ConsecutiveCaptureAttemptPairs'] == 29 and analysis['MarkPositionComparisons'] == 551
    assert analysis['MarkPositionChanges'] == 0 and all(value == 0 for value in analysis['Counts'].values())
    assert analysis['FogUiEnabledSamples'] == 30 and analysis['CaptureAttemptContinuityKnown'] is True
    for key in ['AtomicSnapshotClaimed', 'DynamicSyncErrorMeasured', 'FogCompositionVerified', 'LevelBAccepted']:
        assert analysis[key] is False
    roots_path = RESEARCH / 'runtime_resource_identity_20261112/known-roots-43932.json'
    roots = load(roots_path)
    assert roots['ImageSha256'] == EXPECTED_SOURCE_SHA
    assert roots['FactoryPointer'] - roots['ModuleBase'] == roots['FactoryModuleRva'] == 0x4E0C70
    assert roots['RootAndFactoryUnchanged'] is True
    assert roots['ReadOperations'] == 14 and roots['RequestedBytes'] == 320
    assert roots['ActiveFogSRVVerified'] is False and roots['AtomicSnapshotClaimed'] is False
    shader = load(RESEARCH / 'program_identity_20261112/verification.json')
    assert shader['Passed'] is True and len(shader['Checks']) == 54 and shader['NameCount'] == 109
    assert shader['CurrentDrawProgramVerified'] is False and shader['ActiveFogSRVVerified'] is False
    factory_path = RESEARCH / 'default_resource_factory_20261112/verification.json'
    factory = load(factory_path)
    assert factory['passed'] is True and factory['source']['sha256'] == EXPECTED_SOURCE_SHA
    assert factory['source_bytes'] == 275 and factory['literal_assertion_count'] == 39
    actual = factory['actual_REA']
    assert actual['declared_body_bytes'] == 274 and actual['source_bytes_missing_from_declared_body'] == 1
    assert actual['missing_original_bytes'] == 'cc' and actual['all_CFG_reachable_instruction_bytes_declared'] is True
    assert actual['selected_original_range_coverage_complete'] is False
    assert factory['parent_snapshot']['sha256'] == sha(roots_path)
    edge = factory['next_explicit_T_argument_edge']
    assert edge['site'] == '0x4e0d2d' and edge['target'] == '0x4e0980' and edge['RCX'] == 'T'
    release_path = ROOT / 'dist/validation-0.5.1-native-preview-20261112-041209.json'
    release = load(release_path)
    assert sha(Path(release['Exe'])).upper() == release['ExeSha256']
    assert sha(Path(release['Zip'])).upper() == release['ZipSha256']
    for key in ['FogActiveUiCompositionVerified', 'TemporalBlendExact', 'ContinuousMovementSyncMeasured', 'SameSceneVisualComparisonPassed', 'LevelBAccepted']:
        assert release[key] is False
    assert sorted(path.name for path in (ROOT / 'dist').iterdir() if path.is_file()) == sorted([Path(release['Zip']).name, release_path.name])
    result = {
        'VerifiedAt': datetime.now().astimezone().isoformat(), 'EvidenceConsistencyPassed': True,
        'Report': str(report), 'ReportSha256': sha(report), 'LocalReportLinksVerified': len(links),
        'ComparisonSha256': comparison['OutputSha256'], 'CurrentProbeSourceSha256': analysis['SourceSha256'],
        'SuccessfulSamples': 30, 'ConsecutiveAttemptPairs': 29, 'CoordinateComparisons': 551,
        'CoordinateChanges': 0, 'ClientShaderChecks': 54, 'DefaultFactoryOriginalBytes': 275,
        'DefaultFactoryReaDeclaredBytes': 274, 'DefaultFactoryReaMissingTrapBytes': 1,
        'ReleaseHashesUnchanged': True, 'OnlyLatestReleaseInDist': True,
        'CurrentDrawProgramVerified': False, 'FogCompositionVerified': False,
        'ContinuousMovementSyncMeasured': False, 'SameSceneVisualComparisonPassed': False,
        'LevelBAccepted': False, 'GameProcessOpenedByVerifier': False, 'GameWritten': False,
    }
    with output.open('x', encoding='utf-8') as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2)
        stream.write('\n')
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
