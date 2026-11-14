"""Offline cross-check of the creation-chain review and the 13:42 probe."""
from pathlib import Path
from datetime import datetime
import argparse
import hashlib
import json
import re
import pefile

PROJECT = Path(__file__).resolve().parents[1]
REVERSE = PROJECT.parent / 'CoH3FowTool/tools/reverse/native_fow_20261111'
UPSTREAM = REVERSE / 'device_creation_upstream_20261112'
PUBLICATION = REVERSE / 'device_global_publication_20261112'
LIVE = PROJECT / 'artifacts/research-live-20261112-134243'


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def load(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=LIVE / 'review-verification.json')
    args = parser.parse_args()
    checks = {}

    def check(name, condition):
        checks[name] = bool(condition)
        if not condition:
            raise AssertionError(name)

    u = load(UPSTREAM / 'upstream-verification.json')
    v = load(PUBLICATION / 'verification.json')
    raw = load(LIVE / 'native-43932.json')
    analysis = load(LIVE / 'native-analysis.json')
    release = load(PROJECT / 'dist/validation-0.5.1-native-preview-20261112-041209.json')
    source = Path(v['source']['path'])
    check('fixed_original_source', source.stat().st_size == 108389560 and digest(source) == '2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde')
    check('frozen_46_checks', u['Passed'] is True and len(u['Checks']) == 46 and all(x is True for x in u['Checks'].values()))
    check('frozen_publication_proof_identity', digest(PUBLICATION / 'verification.json') == 'c1996e0b1ca63792063c4466c93e593bfd625a913ba862a3cdcc14c7b1b14ba0')
    check('frozen_427_checks', v['status'] == 'PASS' and v['check_count'] == len(v['checks']) == 427 and all(x['passed'] is True for x in v['checks']))
    check('raw_CFG_scope', v['scope']['original_bytes'] == 976 and v['scope']['original_instructions'] == 216 and v['scope']['original_ranges'] == 2)
    rea = v['actual_REA']
    check('true_REA_gap_retained', rea['declared_bytes'] == 124 and rea['missing_original_bytes'] == 852 and rea['missing_normal_CFG_reachable_instructions'] == 185 and rea['original_range_coverage_complete'] is False)
    flags = v['semantic_flags']
    check('per_path_copy_not_global_reread', flags['SamePerPathPublishedValueAndCallRCX'] is True and flags['GAtCallbackTimeEqualsPublishedValue'] is False and flags['CurrentDrawProgramVerified'] is False and flags['SRVBindingVerified'] is False)
    rea_paths = {'mini_SHA256': 'publication_function.exe', 'raw_CLI_SHA256': 'rea_a2e250.cli.json', 'index_SHA256': 'rea_a2e250/decompile-index.json', 'C_SHA256': 'rea_a2e250/functions/0x140a2e250.c'}
    check('original_REA_files_unchanged', all(digest(PUBLICATION / p) == rea[k] for k, p in rea_paths.items()))
    check('publication_artifacts_unchanged', all(digest(PUBLICATION / p) == h for p, h in v['artifact_SHA256'].items()))
    check('snapshot_immutable', digest(UPSTREAM / 'callback-root-43932.json') == 'bec0ec94d2b064b191e1dbf43c3ec59a1bf334e9fdb393a5285214f8a63edfe2')
    literals = {0x13B41A6: '488D05038A67FF', 0x13B41AD: '488905A442DD04', 0xA2CBBA: '488BD9', 0xA2CBC1: 'E84A0E0000', 0xA2CBC6: '488BCB', 0xA2CBC9: 'E842EAFFFF', 0xA2B616: '488BD9', 0xA2B619: '4881C1B03D0300', 0xA2B620: 'E80BC0DAFF', 0xA2E42E: '48893DBBA27505', 0xA2E435: '488B051CA07505', 0xA2E43C: '498BD6', 0xA2E43F: '488BCF', 0xA2E442: 'FFD0'}
    original = pefile.PE(str(source), fast_load=True)
    check('14_original_instruction_literals', all(original.get_data(rva, len(bytes.fromhex(b))) == bytes.fromhex(b) for rva, b in literals.items()))
    check('raw_capture_identity', raw['ProbeSchemaVersion'] == 2 and raw['ProcessId'] == 43932 and raw['ImageSha256'].lower() == v['source']['sha256'])
    check('analysis_bound_to_raw', analysis['SourceSha256'].lower() == digest(LIVE / 'native-43932.json'))
    samples = raw['Samples']
    indices = [x['AttemptIndex'] for x in samples]
    discarded = [x for x in raw['Attempts'] if x['Outcome'] == 'Discarded']
    check('20_attempts_18_captures', len(raw['Attempts']) == 20 and len(samples) == 18 and analysis['RequestedSamples'] == analysis['AttemptedSamples'] == 20 and analysis['UnattemptedSamples'] == 0)
    check('exact_discarded_attempts', [x['AttemptIndex'] for x in discarded] == [0, 18] and all(x['Reason'] == 'NativeRootOrModelUnavailableOrChanged' for x in discarded))
    check('16_consecutive_pairs', sum(b == a + 1 for a, b in zip(indices, indices[1:])) == analysis['ConsecutiveCaptureAttemptPairs'] == 16 and analysis['SkippedPairs']['CaptureAttemptGap'] == 1)
    check('no_observed_coordinate_changes', analysis['MarkPositionComparisons'] == 304 and analysis['MarkPositionChanges'] == 0 and analysis['NativeMarkCounts'] == [19] and all(x == 0 for x in analysis['Counts'].values()))
    check('selector_and_state_are_distinct', analysis['FogUiEnabledSamples'] == 18 and analysis['UniqueMarkEpochs'] == 18 and all(x['MatchState']['IsGameOverSpectatingMap'] is False for x in samples))
    check('new_capture_clock', samples[0]['CapturedAt'] == '2026-11-12T13:42:43.8074417+08:00' and samples[-1]['CapturedAt'] == '2026-11-12T13:42:45.7726184+08:00' and analysis['CaptureWindowSeconds'] == 2.0996919)
    check('analysis_is_not_acceptance', all(analysis[k] is False for k in ['AtomicSnapshotClaimed', 'DynamicSyncErrorMeasured', 'FogCompositionVerified', 'LevelBAccepted', 'GameWritten', 'GameFunctionsInvoked', 'GameProcessOpenedByAnalyzer']))
    dev = PROJECT / 'artifacts/probe-timeline-20261112'
    check('development_build_identity', digest(dev / 'CoH3Minimap.exe') == '132343922be3dad701ca5f0ac21c389d0da14f60b0407ff500379e44306a6222' and digest(dev / 'CoH3Minimap.dll') == '1287b547df32e126ae5b3d58333bd9401a1a2c7f5609e2769d970710de8038f3')
    check('sources_no_newer_than_development_DLL', all(p.stat().st_mtime <= (dev / 'CoH3Minimap.dll').stat().st_mtime for p in (PROJECT / 'src').glob('*.cs')))
    check('published_files_unchanged', digest(Path(release['Exe'])).upper() == release['ExeSha256'].upper() and digest(Path(release['Zip'])).upper() == release['ZipSha256'].upper())
    check('only_current_release_zip', sorted(p.name for p in (PROJECT / 'dist').glob('*.zip')) == [Path(release['Zip']).name])
    check('five_release_acceptance_flags_remain_false', all(release[k] is False for k in ['FogActiveUiCompositionVerified', 'TemporalBlendExact', 'ContinuousMovementSyncMeasured', 'SameSceneVisualComparisonPassed', 'LevelBAccepted']))
    docs = [UPSTREAM / 'CREATION_UPSTREAM_REPORT.md', PUBLICATION / 'DEVICE_GLOBAL_PUBLICATION_REPORT.md', LIVE / 'CURRENT_OBSERVATION.md']
    link_counts = {}
    for doc in docs:
        links = re.findall(r'\[[^\]]+\]\((?:<)?([^)>]+)(?:>)?\)', doc.read_text(encoding='utf-8'))
        link_counts[doc.name] = len(links)
        check('links_' + doc.stem, bool(links) and all((doc.parent / p.split('#', 1)[0]).resolve().is_file() for p in links))
    readme = (PROJECT / 'README.md').read_text(encoding='utf-8')
    check('README_has_both_new_reviews', 'device_creation_upstream_20261112/CREATION_UPSTREAM_REPORT.md' in readme and 'research-live-20261112-134243/CURRENT_OBSERVATION.md' in readme and '2026-11-12 13:28+08:00' in readme)
    text = docs[0].read_text(encoding='utf-8')
    check('publication_update_consistent', '852 B / 185' in text and '427' in text and '仍由独立任务核验' not in text and '完成 A2E250 全部原冷块' not in text)
    check('observation_intervals_kept_separate', '分开的观察区间' in docs[2].read_text(encoding='utf-8'))
    evidence = [*docs, PROJECT / 'README.md', Path(__file__), UPSTREAM / 'upstream-verification.json', UPSTREAM / 'callback-root-43932.json', PUBLICATION / 'verification.json', LIVE / 'native-43932.json', LIVE / 'native-analysis.json', dev / 'CoH3Minimap.exe', dev / 'CoH3Minimap.dll', Path(release['Exe']), Path(release['Zip'])]
    result = dict(VerifiedAt=datetime.now().astimezone().isoformat(), Passed=True, Checks=checks, LocalLinks=link_counts, OriginalInstructionLiterals=len(literals), FileSha256={str(p): digest(p) for p in evidence}, GameProcessOpenedByVerifier=False, Frozen46And427ProofsRerun=False, ExistingAnalyzerRegressionTestsRerun=False, ApplicationSelfTestsRerun=False, LevelBAccepted=False)
    with args.output.open('x', encoding='utf-8') as f:
        json.dump(result, f, ensure_ascii=False, indent=2)
        f.write('\n')
    print(json.dumps(dict(Passed=True, Checks=len(checks), LocalLinks=link_counts, OriginalInstructions=len(literals), ExistingReleaseUnchanged=True, Output=str(args.output)), indent=2))


if __name__ == '__main__':
    main()
