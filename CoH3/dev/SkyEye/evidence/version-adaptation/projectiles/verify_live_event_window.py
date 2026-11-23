"""Validate retained C# event outputs; no process access or synthetic game PASS."""
import argparse, collections, hashlib, json, math, pathlib, statistics


def analyse(report):
    result = next(r for r in report['results'] if r['Feature'] == 'event_window')
    assert report['licenseState'] == 'active', 'Real lease must be active'
    assert result['Status'] == 'observed', 'Need actual decoded outputs'
    evidence = result['Evidence']
    event = evidence.get('projectile', evidence)
    frames, polls = event['frames'], event['polls']
    assert event['stopReason'] == 'deadline', 'Incomplete event window'
    assert event['droppedFrames'] == event['droppedDecoded'] == 0, 'Retained sequence has gaps'
    assert len(frames) == event['nonzeroPolls'] == event['retainedFrames']
    assert all(p['status'] == 'completed' for p in polls)
    assert all(f['match'] == event['initialMatch'] and f['epoch'] == event['initialEpoch'] for f in frames)
    assert all(f['droppedDecodedInThisPoll'] == 0 and len(f['decoded']) == f['decodedCount'] for f in frames)
    assert sum(len(f['decoded']) for f in frames) == event['decodedObservationCount'] == event['retainedDecoded']
    assert sum(p['decodedCount'] or 0 for p in polls) == event['decodedObservationCount']
    groups = collections.defaultdict(list)
    for frame in frames:
        for item in frame['decoded']:
            assert all(math.isfinite(item[n]) for n in ('X', 'Y', 'Z', 'Radius'))
            groups[(item['entity'], item['Id'], item['Index'])].append((frame, item))
    identities = []
    for key, observations in sorted(groups.items(), key=lambda row: row[1][0][0]['pollStartedMilliseconds']):
        first, last = observations[0][0], observations[-1][0]
        identity = dict(zip(('entity', 'Id', 'Index'), key))
        identity.update(observations=len(observations), firstUtc=first['utc'], lastUtc=last['utc'],
                        firstMs=first['pollStartedMilliseconds'], lastMs=last['pollStartedMilliseconds'])
        for name in ('sourceKey', 'Slot', 'Number', 'Name', 'Weapon', 'Label', 'Radius'):
            values = list(dict.fromkeys(item[name] for _, item in observations))
            identity[name] = values[0] if len(values) == 1 else values
        identity['predictedImpactBounds'] = {n: [min(item[n] for _, item in observations), max(item[n] for _, item in observations)] for n in ('X', 'Y', 'Z')}
        identity['laterAllZeroPollMs'] = next((p['startedMilliseconds'] for p in polls if p['startedMilliseconds'] > identity['lastMs'] and p['decodedCount'] == 0), None)
        identities.append(identity)
    gaps = [b['startedMilliseconds'] - a['startedMilliseconds'] for a, b in zip(polls, polls[1:])]
    return dict(status='PASS_retained_reader_sequence_only', gameFeatureAccepted=False,
                limitations=['Predicted impact coordinates, not physical projectile positions.',
                             'Identity observations are not exact spawn/despawn or impact times.',
                             'No synchronized rendered overlay or ground impact comparison in this run.'],
                match=event['initialMatch'], epoch=event['initialEpoch'], polls=len(polls),
                decodedObservations=event['decodedObservationCount'], distinctRetainedIdentities=len(groups),
                elapsedMs=event['elapsedMilliseconds'], overrunMs=event['overrunMilliseconds'],
                startGapMedianMs=statistics.median(gaps) if gaps else None, startGapMaximumMs=max(gaps) if gaps else None,
                emptyPolls=sum(p['decodedCount'] == 0 for p in polls), identities=identities,
                airStatus=evidence.get('air', {}).get('status', 'not_requested'))


def self_test():
    item = dict(entity='0x10000', Id=3, Index=2, sourceKey='0x20000', Slot=1, Number=1, Name='fixture', Weapon='fixture', Label='fixture', Radius=4, X=1., Y=2., Z=3.)
    frame = dict(match='fixture', epoch=1, utc='fixture', pollStartedMilliseconds=100, decodedCount=1, decoded=[item], droppedDecodedInThisPoll=0)
    event = dict(stopReason='deadline', droppedFrames=0, droppedDecoded=0, nonzeroPolls=1, retainedFrames=1, retainedDecoded=1, decodedObservationCount=1, initialMatch='fixture', initialEpoch=1, frames=[frame], elapsedMilliseconds=300, overrunMilliseconds=0, polls=[dict(status='completed', decodedCount=c, startedMilliseconds=t) for t, c in ((0, 0), (100, 1), (200, 0))])
    report = dict(licenseState='active', results=[dict(Feature='event_window', Status='observed', Evidence=event)])
    assert analyse(report)['identities'][0]['laterAllZeroPollMs'] == 200
    event['droppedFrames'] = 1
    try: analyse(report)
    except AssertionError: pass
    else: raise AssertionError('Drop must invalidate complete-sequence analysis')
    event['droppedFrames'] = 0; frame['epoch'] = 2
    try: analyse(report)
    except AssertionError: pass
    else: raise AssertionError('Context mismatch must fail')
    print('SELF_TEST_PASS 3')


if __name__ == '__main__':
    p = argparse.ArgumentParser(); p.add_argument('--self-test', action='store_true'); p.add_argument('--input'); p.add_argument('--out'); args = p.parse_args()
    if args.self_test: self_test()
    else:
        if not args.input or not args.out: p.error('--input and --out required')
        source = pathlib.Path(args.input); data = source.read_bytes(); output = analyse(json.loads(data.decode('utf-8-sig')))
        output.update(input=str(source.resolve()), inputSha256=hashlib.sha256(data).hexdigest())
        pathlib.Path(args.out).write_text(json.dumps(output, ensure_ascii=False, indent=2), encoding='utf-8')
        print(json.dumps({k: output[k] for k in ('status', 'decodedObservations', 'distinctRetainedIdentities', 'polls', 'airStatus')}))
