"""Compare native probe observations, never infer simulation or rendered-frame synchronization."""
import argparse
from datetime import datetime
import hashlib
import json
import math
from pathlib import Path
import re


def finite(value):
    return type(value) in (int, float) and math.isfinite(value)


def scene(sample):
    location, model = sample.get('Location') or {}, sample.get('Map') or {}
    geometry, minimap = model.get('Geometry') or {}, model.get('Minimap') or {}
    addresses = [location.get(key) for key in ('ApplicationAddress', 'HudAddress', 'MapAddress')]
    addresses += [model.get('ModelAddress'), model.get('MarksCollectionAddress')]
    size = [geometry.get(key) for key in ('PlayableWidth', 'PlayableHeight')]
    path = minimap.get('AssetPath')
    if not all(type(value) is int and value > 0 for value in addresses):
        return None
    if location['MapAddress'] != model['ModelAddress'] or not isinstance(path, str) or not path:
        return None
    if not all(finite(value) and value > 0 for value in size):
        return None
    return (*addresses, model.get('ModelVtableRva'), path, *size)


def marks(sample):
    # No persistent entity ID is claimed. Do not match a reused model address to a new payload/type.
    found, duplicate_addresses, invalid = {}, set(), 0
    for mark in (sample.get('Map') or {}).get('Marks') or []:
        address = mark.get('ModelAddress')
        if type(address) is not int or address <= 0:
            invalid += 1
            continue
        if address in found:
            duplicate_addresses.add(address)
        found[address] = mark
    for address in duplicate_addresses:
        del found[address]
    records = {}
    for address, mark in found.items():
        payload, kind = mark.get('PayloadAddress'), mark.get('ModelType')
        if type(payload) is not int or payload <= 0 or not isinstance(kind, str) or not kind:
            invalid += 1
            continue
        records[(address, payload, kind)] = mark
    return records, len(duplicate_addresses), invalid


def position(mark):
    values = [mark.get('PositionX'), mark.get('PositionY')]
    return values if all(finite(value) for value in values) else None


def visual(mark):
    color, angle = mark.get('SelectionColor') or {}, mark.get('CachedAngle') or {}
    values = [mark.get('RawScale'), *(color.get(key) for key in ('R', 'G', 'B', 'A')), angle.get('Degrees')]
    handle = mark.get('IconHandle')
    if type(handle) is not int or not all(finite(value) for value in values):
        return None
    return [handle, *values]


def camera(sample):
    points = ((sample.get('Map') or {}).get('Geometry') or {}).get('PerspectiveBox')
    if not isinstance(points, list) or len(points) != 4:
        return None
    values = [point.get(key) for point in points for key in ('X', 'Y')]
    return values if all(finite(value) for value in values) else None


def image(sample, field):
    data = sample.get(field) or {}
    width, height, digest = data.get('Width'), data.get('Height'), data.get('Sha256')
    if not all(type(value) is int and value > 0 for value in (width, height)):
        return None
    if field == 'FogPlanes':
        if not isinstance(digest, list) or len(digest) != 3:
            return None
        hashes = digest
    else:
        hashes = [digest]
    if not all(isinstance(value, str) and re.fullmatch(r'[0-9a-fA-F]{64}', value) for value in hashes):
        return None
    return [width, height, *(value.upper() for value in hashes),
            *([data.get('RawConfiguration')] if field == 'FogPlanes' else [])]


def timestamp(sample):
    try:
        value = datetime.fromisoformat(sample['CapturedAt'])
        return value if value.tzinfo is not None else None
    except (KeyError, TypeError, ValueError):
        return None


def probe_timeline(report, samples, requested):
    """Validate the complete v2 attempt journal; legacy reports have unknown capture gaps."""
    if report.get('ProbeSchemaVersion') is None:
        return None
    if type(report.get('ProbeSchemaVersion')) is not int or report['ProbeSchemaVersion'] != 2:
        raise ValueError('Unsupported ProbeSchemaVersion')
    attempts, frequency = report.get('Attempts'), report.get('MonotonicClockFrequency')
    if report.get('MonotonicClock') != 'Stopwatch.ElapsedTicks' or type(frequency) is not int or frequency <= 0:
        raise ValueError('Invalid monotonic clock metadata')
    if not isinstance(attempts, list) or len(attempts) > requested:
        raise ValueError('Invalid Attempts array')
    if (type(report.get('AttemptedSamples')) is not int or type(report.get('UnattemptedSamples')) is not int or
            report['AttemptedSamples'] != len(attempts) or report['UnattemptedSamples'] != requested - len(attempts)):
        raise ValueError('AttemptedSamples/UnattemptedSamples disagree with the attempt journal')
    captured_ids, previous_end = [], -1
    for index, attempt in enumerate(attempts):
        if not isinstance(attempt, dict) or type(attempt.get('AttemptIndex')) is not int or attempt['AttemptIndex'] != index:
            raise ValueError('AttemptIndex must be consecutive and zero based')
        start, end = attempt.get('StartedMonotonicTicks'), attempt.get('CompletedMonotonicTicks')
        if type(start) is not int or type(end) is not int or start < 0 or start < previous_end or end < start:
            raise ValueError('Monotonic capture intervals overlap or run backwards')
        previous_end = end
        if attempt.get('Outcome') == 'Captured' and attempt.get('Reason') is None:
            captured_ids.append(index)
        elif attempt.get('Outcome') != 'Discarded' or not isinstance(attempt.get('Reason'), str) or not attempt['Reason']:
            raise ValueError('Invalid attempt outcome/reason')
    sample_ids = [sample.get('AttemptIndex') for sample in samples]
    if any(type(value) is not int for value in sample_ids) or sample_ids != captured_ids:
        raise ValueError('Captured sample AttemptIndex values do not match the attempt journal')
    return attempts, frequency


def analyze(report):
    samples = report.get('Samples')
    if not isinstance(samples, list):
        raise ValueError('Samples must be an array')
    captured, requested = len(samples), report.get('RequestedSamples')
    if (type(report.get('CapturedSamples')) is not int or report['CapturedSamples'] != captured or
            type(requested) is not int or requested < captured):
        raise ValueError('RequestedSamples/CapturedSamples disagree with the sample array')
    timeline = probe_timeline(report, samples, requested)
    names = ('MarkIdentityTransitions', 'MarkPositionTransitions', 'MarkVisualTransitions',
             'CameraOrMapGeometryTransitions', 'CameraViewportTransitions', 'MapAngleTransitions',
             'TerritoryTransitions', 'InteractivityTransitions', 'FogPlaneTransitions')
    counts, compared = dict.fromkeys(names, 0), dict.fromkeys(names, 0)
    skipped = dict(SceneChanged=0, SceneIdentityMissing=0, TimestampMissingOrNotIncreasing=0,
                   CaptureAttemptGap=0, MonotonicTimeNotIncreasing=0)
    indexed = [marks(sample) for sample in samples]
    comparable = position_comparisons = position_changes = 0
    largest_step = 0.0

    def compare(name, a, b):
        if a is None or b is None:
            return False
        compared[name] += 1
        changed = a != b
        counts[name] += changed
        return changed

    for index, (before, after) in enumerate(zip(samples, samples[1:])):
        if timeline is not None:
            attempts, _ = timeline
            a_id, b_id = before['AttemptIndex'], after['AttemptIndex']
            if b_id != a_id + 1:
                skipped['CaptureAttemptGap'] += 1
                continue
            if attempts[b_id]['CompletedMonotonicTicks'] <= attempts[a_id]['CompletedMonotonicTicks']:
                skipped['MonotonicTimeNotIncreasing'] += 1
                continue
        else:
            a_time, b_time = timestamp(before), timestamp(after)
            if a_time is None or b_time is None or b_time <= a_time:
                skipped['TimestampMissingOrNotIncreasing'] += 1
                continue
        a_scene, b_scene = scene(before), scene(after)
        if a_scene is None or b_scene is None:
            skipped['SceneIdentityMissing'] += 1
            continue
        if a_scene != b_scene:
            skipped['SceneChanged'] += 1
            continue
        comparable += 1
        a, b = indexed[index][0], indexed[index + 1][0]
        # Ambiguous/invalid records make identity-set comparisons incomplete.
        if indexed[index][1:] == (0, 0) and indexed[index + 1][1:] == (0, 0):
            compare('MarkIdentityTransitions', a.keys(), b.keys())
        position_values, visual_values = [], []
        for key in a.keys() & b.keys():
            ap, bp = position(a[key]), position(b[key])
            if ap is not None and bp is not None:
                position_comparisons += 1
                changed = ap != bp
                position_changes += changed
                position_values.append(changed)
                largest_step = max(largest_step, math.hypot(bp[0] - ap[0], bp[1] - ap[1]))
            av, bv = visual(a[key]), visual(b[key])
            if av is not None and bv is not None:
                visual_values.append(av != bv)
        if position_values:
            compared['MarkPositionTransitions'] += 1
            counts['MarkPositionTransitions'] += any(position_values)
        if visual_values:
            compared['MarkVisualTransitions'] += 1
            counts['MarkVisualTransitions'] += any(visual_values)
        a_angle = ((before.get('Map') or {}).get('Geometry') or {}).get('AngleDegrees')
        b_angle = ((after.get('Map') or {}).get('Geometry') or {}).get('AngleDegrees')
        angle_known = finite(a_angle) and finite(b_angle)
        angle_changed = compare('MapAngleTransitions', a_angle if angle_known else None, b_angle if angle_known else None)
        a_camera, b_camera = camera(before), camera(after)
        camera_changed = compare('CameraViewportTransitions', a_camera, b_camera)
        if angle_known and a_camera is not None and b_camera is not None:
            compared['CameraOrMapGeometryTransitions'] += 1
            counts['CameraOrMapGeometryTransitions'] += angle_changed or camera_changed
        for field, name in (('Territory', 'TerritoryTransitions'), ('Interactivity', 'InteractivityTransitions'),
                            ('FogPlanes', 'FogPlaneTransitions')):
            compare(name, image(before, field), image(after, field))
    post_match = sum((sample.get('MatchState') or {}).get('IsGameOverSpectatingMap') is True for sample in samples)
    fog_enabled = sum((sample.get('FogPlanes') or {}).get('UiDisabled') is False for sample in samples)
    first, last = (timestamp(samples[0]), timestamp(samples[-1])) if samples else (None, None)
    duration = (last - first).total_seconds() if first and last and last >= first else None
    attempted = capture_window = consecutive_pairs = None
    if timeline is not None:
        attempts, frequency = timeline
        attempted = len(attempts)
        capture_window = ((attempts[-1]['CompletedMonotonicTicks'] - attempts[0]['StartedMonotonicTicks']) / frequency
                          if attempts else 0)
        consecutive_pairs = sum(b['AttemptIndex'] == a['AttemptIndex'] + 1 for a, b in zip(samples, samples[1:]))
        duration = ((attempts[samples[-1]['AttemptIndex']]['CompletedMonotonicTicks'] -
                     attempts[samples[0]['AttemptIndex']]['CompletedMonotonicTicks']) / frequency if samples else None)
    if not samples:
        observation = 'NoCapturedSamples'
    elif post_match == captured:
        observation = 'PostMatchOnly'
    elif comparable == 0:
        observation = 'NoComparableCapturedPairs'
    elif any(counts.values()):
        observation = 'SourceChangesObserved'
    else:
        observation = 'NoComparableSourceChangesObserved'
    return dict(RequestedSamples=requested, AttemptedSamples=attempted, CapturedSamples=captured,
                DiscardedSamples=attempted - captured if attempted is not None else None,
                UnattemptedSamples=requested - attempted if attempted is not None else None,
                UncapturedRequestedSamples=requested - captured,
                Diagnostics=report.get('Diagnostics', []), ObservedDurationSeconds=duration,
                TimingBasis='ProbeMonotonicCaptureIntervals' if timeline is not None else 'LegacyWallClock',
                CaptureWindowSeconds=capture_window, ConsecutiveCaptureAttemptPairs=consecutive_pairs,
                AdjacentCapturedPairs=max(0, captured - 1), ComparedTransitions=comparable,
                SkippedPairs=skipped, Counts=counts, MetricComparedPairs=compared,
                MarkPositionComparisons=position_comparisons, MarkPositionChanges=position_changes,
                LargestObservedCoordinateStep=largest_step,
                DuplicateAddressSamples=sum(item[1] > 0 for item in indexed),
                InvalidMarkerRecords=sum(item[2] for item in indexed),
                NativeMarkCounts=sorted({len((sample.get('Map') or {}).get('Marks') or []) for sample in samples}),
                MissingMeasurements={key: sum(image(sample, key) is None for sample in samples)
                                     for key in ('Territory', 'Interactivity', 'FogPlanes')},
                PostMatchSamples=post_match, FogUiEnabledSamples=fog_enabled,
                FogUiDisabledSamples=sum((sample.get('FogPlanes') or {}).get('UiDisabled') is True for sample in samples),
                UniqueMarkEpochs=len({mark.get('ModelEpoch') for item in indexed for mark in item[0].values()}),
                Observation=observation, GameWritten=report.get('GameWritten'),
                GameFunctionsInvoked=report.get('GameFunctionsInvoked'), GameProcessOpenedByAnalyzer=False,
                CaptureAttemptContinuityKnown=timeline is not None, AtomicSnapshotClaimed=False,
                DynamicSyncErrorMeasured=False, FogCompositionVerified=False, LevelBAccepted=False,
                IdentityMethod='Same scene plus model address, payload address and model type; not a persistent entity ID.',
                Note='V2 compares consecutive capture attempts in the same scene; legacy capture gaps remain unknown. '
                     'An attempt includes native reads and diagnostic hashing, not a GPU frame. Pointer reuse with identical fields, '
                     'GPU output, frame latency, simulation state and synchronization with game images are not established. '
                     'Epoch-only updates and missing measurements do not count as content changes.')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('--output', type=Path, help='Create a new JSON report; existing files are preserved.')
    args = parser.parse_args()
    raw = args.source.read_bytes()
    result = analyze(json.loads(raw.decode('utf-8-sig')))
    result = dict(Source=str(args.source.resolve()), SourceSha256=hashlib.sha256(raw).hexdigest().upper(), **result)
    rendered = json.dumps(result, ensure_ascii=False, indent=2, allow_nan=False) + '\n'
    if args.output:
        with args.output.open('x', encoding='utf-8') as stream:
            stream.write(rendered)
    print(rendered, end='')


if __name__ == '__main__':
    main()
