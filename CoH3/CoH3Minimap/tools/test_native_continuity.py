"""Offline regression cases for false motion/content evidence; no game process is accessed."""
from copy import deepcopy
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

from analyze_native_continuity import analyze


def sample(index=0):
    return dict(CapturedAt=f'2026-11-12T04:08:{index:02d}+08:00',
                Location=dict(ApplicationAddress=0x10000, HudAddress=0x20000, MapAddress=0x30000),
                Map=dict(ModelAddress=0x30000, ModelVtableRva=0x3DF4060, MarksCollectionAddress=0x40000,
                         Minimap=dict(AssetPath='data:scenarios/map.rrtex', CacheEpoch=index),
                         Geometry=dict(PlayableWidth=480, PlayableHeight=480, AngleDegrees=-60,
                                       PerspectiveBox=[dict(X=0, Y=0), dict(X=1, Y=0), dict(X=0, Y=1), dict(X=1, Y=1)]),
                         Marks=[dict(ModelAddress=0x50000, PayloadAddress=0x60000, ModelType='SquadMarkModel',
                                     PositionX=10, PositionY=20, ModelEpoch=index, IconHandle=123, RawScale=1,
                                     SelectionColor=dict(R=1, G=0, B=0, A=1, CacheState=1),
                                     CachedAngle=dict(Degrees=0, CacheEpoch=index))]),
                MatchState=dict(IsGameOverSpectatingMap=True, CacheEpoch=index, GlobalEpoch=index),
                Territory=dict(Width=482, Height=482, Sha256='A' * 64),
                Interactivity=dict(Width=480, Height=480, Sha256='B' * 64),
                FogPlanes=dict(Width=240, Height=240, RawConfiguration=0, UiDisabled=True,
                               Sha256=['C' * 64, 'D' * 64, 'E' * 64]))


def report(*samples):
    return dict(Samples=list(samples), RequestedSamples=len(samples), CapturedSamples=len(samples),
                GameWritten=False, GameFunctionsInvoked=False)


def timed_report(*observations, requested=None):
    samples, attempts = [], []
    for index, item in enumerate(observations):
        captured = item is not None
        attempts.append(dict(AttemptIndex=index, StartedMonotonicTicks=index * 100,
                             CompletedMonotonicTicks=index * 100 + 25,
                             Outcome='Captured' if captured else 'Discarded',
                             Reason=None if captured else 'NativeRootOrModelUnavailableOrChanged'))
        if captured:
            item = deepcopy(item)
            item['AttemptIndex'] = index
            samples.append(item)
    requested = len(attempts) if requested is None else requested
    return dict(report(*samples), ProbeSchemaVersion=2, RequestedSamples=requested,
                AttemptedSamples=len(attempts), UnattemptedSamples=requested - len(attempts),
                MonotonicClock='Stopwatch.ElapsedTicks', MonotonicClockFrequency=1000, Attempts=attempts)


class ContinuityTests(unittest.TestCase):
    def test_epochs_alone_are_not_content_changes(self):
        result = analyze(report(sample(), sample(1), sample(2)))
        self.assertEqual(result['UniqueMarkEpochs'], 3)
        self.assertTrue(all(value == 0 for value in result['Counts'].values()))
        self.assertEqual(result['MarkPositionComparisons'], 2)
        self.assertEqual(result['Observation'], 'PostMatchOnly')

    def test_matching_records_can_change_coordinates_without_acceptance(self):
        a, b = sample(), sample(1)
        b['Map']['Marks'][0].update(PositionX=13, PositionY=24)
        b['MatchState']['IsGameOverSpectatingMap'] = False
        result = analyze(report(a, b))
        self.assertEqual(result['Counts']['MarkPositionTransitions'], 1)
        self.assertEqual(result['LargestObservedCoordinateStep'], 5)
        self.assertEqual(result['Observation'], 'SourceChangesObserved')
        for flag in ('DynamicSyncErrorMeasured', 'FogCompositionVerified', 'LevelBAccepted', 'CaptureAttemptContinuityKnown'):
            self.assertIs(result[flag], False)

    def test_payload_or_type_replacement_is_not_coordinate_motion(self):
        for field, value in (('PayloadAddress', 0x70000), ('ModelType', 'VehicleMarkModel')):
            with self.subTest(field=field):
                a, b = sample(), sample(1)
                b['Map']['Marks'][0].update({field: value, 'PositionX': 200})
                result = analyze(report(a, b))
                self.assertEqual(result['Counts']['MarkIdentityTransitions'], 1)
                self.assertEqual(result['Counts']['MarkPositionTransitions'], 0)
                self.assertEqual(result['MarkPositionComparisons'], 0)

    def test_scene_root_map_path_and_dimensions_break_comparison(self):
        for changed in ('root', 'path', 'dimensions'):
            with self.subTest(changed=changed):
                a, b = sample(), sample(1)
                b['Map']['Marks'][0]['PositionX'] = 200
                if changed == 'root':
                    b['Location']['HudAddress'] += 8
                elif changed == 'path':
                    b['Map']['Minimap']['AssetPath'] = 'data:scenarios/other.rrtex'
                else:
                    b['Map']['Geometry']['PlayableWidth'] = 384
                result = analyze(report(a, b))
                self.assertEqual(result['SkippedPairs']['SceneChanged'], 1)
                self.assertEqual(result['ComparedTransitions'], 0)
                self.assertEqual(result['Counts']['MarkPositionTransitions'], 0)

    def test_unreadable_layer_recovery_is_not_a_texture_change(self):
        a, b = sample(), sample(1)
        for key in ('Territory', 'Interactivity', 'FogPlanes'):
            a[key] = None
        result = analyze(report(a, b))
        for key in ('TerritoryTransitions', 'InteractivityTransitions', 'FogPlaneTransitions'):
            self.assertEqual(result['Counts'][key], 0)
            self.assertEqual(result['MetricComparedPairs'][key], 0)
        self.assertEqual(result['FogUiDisabledSamples'], 1)

    def test_content_hash_changes_and_fog_configuration_are_observations(self):
        a, b = sample(), sample(1)
        b['Territory']['Sha256'] = 'F' * 64
        b['FogPlanes']['RawConfiguration'] = 1
        result = analyze(report(a, b))
        self.assertEqual(result['Counts']['TerritoryTransitions'], 1)
        self.assertEqual(result['Counts']['FogPlaneTransitions'], 1)
        self.assertFalse(result['FogCompositionVerified'])

    def test_duplicate_addresses_and_nonfinite_coordinates_are_excluded(self):
        a, b = sample(), sample(1)
        b['Map']['Marks'].append(deepcopy(b['Map']['Marks'][0]))
        result = analyze(report(a, b))
        self.assertEqual(result['DuplicateAddressSamples'], 1)
        self.assertEqual(result['MetricComparedPairs']['MarkIdentityTransitions'], 0)
        self.assertEqual(result['MarkPositionComparisons'], 0)
        b = sample(1)
        b['Map']['Marks'][0]['PositionX'] = 'NaN'
        self.assertEqual(analyze(report(a, b))['MarkPositionComparisons'], 0)

    def test_rotation_and_camera_changes_are_separate(self):
        a, b = sample(), sample(1)
        b['Map']['Geometry']['AngleDegrees'] = -90
        result = analyze(report(a, b))
        self.assertEqual(result['Counts']['MapAngleTransitions'], 1)
        self.assertEqual(result['Counts']['CameraViewportTransitions'], 0)
        self.assertEqual(result['Counts']['MarkPositionTransitions'], 0)
        b['Map']['Geometry']['PerspectiveBox'][0]['X'] = 2
        self.assertEqual(analyze(report(a, b))['Counts']['CameraViewportTransitions'], 1)

    def test_unknown_state_does_not_imply_running_or_active_fog(self):
        a, b = sample(), sample(1)
        for item in (a, b):
            item.pop('MatchState')
            item['FogPlanes']['UiDisabled'] = None
        result = analyze(report(a, b))
        self.assertEqual(result['PostMatchSamples'], 0)
        self.assertEqual(result['FogUiEnabledSamples'], 0)
        self.assertEqual(result['Observation'], 'NoComparableSourceChangesObserved')
        self.assertFalse(result['LevelBAccepted'])

    def test_timestamps_missing_geometry_and_report_mismatch_fail_conservatively(self):
        a, b = sample(), sample(1)
        b['CapturedAt'] = a['CapturedAt']
        self.assertEqual(analyze(report(a, b))['SkippedPairs']['TimestampMissingOrNotIncreasing'], 1)
        b = sample(1)
        b['Map']['Geometry'] = None
        self.assertEqual(analyze(report(a, b))['SkippedPairs']['SceneIdentityMissing'], 1)
        data = report(a, b)
        data['CapturedSamples'] = 3
        with self.assertRaisesRegex(ValueError, 'disagree'):
            analyze(data)

    def test_v2_separates_discarded_unattempted_and_absent_game(self):
        result = analyze(timed_report(sample(), None, requested=5))
        self.assertEqual((result['AttemptedSamples'], result['CapturedSamples'], result['DiscardedSamples'],
                          result['UnattemptedSamples']), (2, 1, 1, 3))
        self.assertTrue(result['CaptureAttemptContinuityKnown'])
        empty = analyze(timed_report(requested=5))
        self.assertEqual((empty['AttemptedSamples'], empty['DiscardedSamples'], empty['UnattemptedSamples']), (0, 0, 5))
        self.assertEqual(empty['Observation'], 'NoCapturedSamples')
        self.assertFalse(empty['LevelBAccepted'])

    def test_v2_does_not_compare_across_a_discarded_attempt(self):
        b = sample(2)
        b['Map']['Marks'][0]['PositionX'] = 200
        result = analyze(timed_report(sample(), None, b))
        self.assertEqual(result['SkippedPairs']['CaptureAttemptGap'], 1)
        self.assertEqual(result['MarkPositionComparisons'], 0)
        self.assertEqual(result['ConsecutiveCaptureAttemptPairs'], 0)

    def test_v2_monotonic_timing_survives_wall_clock_adjustment(self):
        b = sample(1)
        b['CapturedAt'] = '2026-11-11T23:59:59+08:00'
        result = analyze(timed_report(sample(), b))
        self.assertEqual(result['TimingBasis'], 'ProbeMonotonicCaptureIntervals')
        self.assertEqual(result['ObservedDurationSeconds'], 0.1)
        self.assertEqual(result['CaptureWindowSeconds'], 0.125)
        self.assertEqual(result['ComparedTransitions'], 1)
        self.assertEqual(result['MarkPositionComparisons'], 1)
        self.assertFalse(result['DynamicSyncErrorMeasured'])

    def test_v2_invalid_or_inconsistent_journal_is_rejected(self):
        mutations = [lambda d: d.update(ProbeSchemaVersion=3),
                     lambda d: d.update(MonotonicClockFrequency=0),
                     lambda d: d.update(AttemptedSamples=3),
                     lambda d: d.update(UnattemptedSamples=1),
                     lambda d: d['Attempts'][1].update(StartedMonotonicTicks=24),
                     lambda d: d['Attempts'][1].update(CompletedMonotonicTicks=99),
                     lambda d: d['Attempts'][1].update(AttemptIndex=0),
                     lambda d: d['Attempts'][1].update(Outcome='Discarded', Reason='changed'),
                     lambda d: d['Samples'][1].update(AttemptIndex=0)]
        for mutate in mutations:
            with self.subTest(mutate=mutate):
                data = timed_report(sample(), sample(1))
                mutate(data)
                with self.assertRaises(ValueError):
                    analyze(data)

    def test_v2_equal_monotonic_end_is_not_a_timed_transition(self):
        data = timed_report(sample(), sample(1))
        data['Attempts'][0].update(StartedMonotonicTicks=0, CompletedMonotonicTicks=0)
        data['Attempts'][1].update(StartedMonotonicTicks=0, CompletedMonotonicTicks=0)
        result = analyze(data)
        self.assertEqual(result['SkippedPairs']['MonotonicTimeNotIncreasing'], 1)
        self.assertEqual(result['MarkPositionComparisons'], 0)

    def test_legacy_does_not_invent_attempt_or_discard_counts(self):
        data = report(sample(), sample(1))
        data['RequestedSamples'] = 5
        result = analyze(data)
        self.assertEqual(result['UncapturedRequestedSamples'], 3)
        self.assertIsNone(result['AttemptedSamples'])
        self.assertIsNone(result['DiscardedSamples'])
        self.assertIsNone(result['UnattemptedSamples'])
        self.assertFalse(result['CaptureAttemptContinuityKnown'])

    def test_cli_does_not_modify_source_or_overwrite_existing_report(self):
        with tempfile.TemporaryDirectory() as directory:
            source, output = Path(directory) / 'probe.json', Path(directory) / 'summary.json'
            original = json.dumps(report(sample(), sample(1))).encode()
            source.write_bytes(original)
            command = [sys.executable, '-B', str(Path(__file__).with_name('analyze_native_continuity.py')),
                       str(source), '--output', str(output)]
            # Explicit files avoid Windows sandbox child stdio-pipe restrictions.
            with (Path(directory) / 'stdout.txt').open('w') as stdout, (Path(directory) / 'stderr.txt').open('w') as stderr:
                self.assertEqual(subprocess.run(command, stdout=stdout, stderr=stderr).returncode, 0)
                first_output = output.read_bytes()
                self.assertNotEqual(subprocess.run(command, stdout=stdout, stderr=stderr).returncode, 0)
            self.assertEqual(output.read_bytes(), first_output)
            self.assertEqual(source.read_bytes(), original)
            self.assertEqual(json.loads(first_output)['CapturedSamples'], 2)


if __name__ == '__main__':
    unittest.main(verbosity=2)
