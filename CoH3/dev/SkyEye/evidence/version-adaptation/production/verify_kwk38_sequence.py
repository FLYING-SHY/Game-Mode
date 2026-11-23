"""Offline, hash-pinned verification of the run30/31/32 local KwK38 sequence.

No process, GUI, memory, credential, build, or network APIs are used.
Default invocation checks only. --write exclusively creates the two named reports;
--self-test additionally checks mutated in-memory negative controls. No overwrite.
"""
import argparse
import copy
import hashlib
import json
import math
import re
import struct
from datetime import datetime
from pathlib import Path

BASE = 'CoH3/dev/SkyEye/evidence/version-adaptation/'
PROD = BASE + 'production/'
SRC = 'CoH3/dev/SkyEye/client/CoH3MemoryTool/'
SELF = PROD + 'verify_kwk38_sequence.py'
OUTPUTS = (PROD + 'kwk38-sequence-validation.json', PROD + 'kwk38-sequence-report.md')
PID = 33656
START = '2026-11-21T03:42:27.6605831Z'
IDENTITY = '1000003e9'
ENTITY = '0x2940d04fe00'
KEY = '2834896846336:2807501192'
RESEARCH = r'upgrade\afrika_korps\vehicle\20mm_halftrack_250_ak'
VEHICLE = r'ebps\races\afrika_korps\vehicles\halftrack_250_ak'
CONVERTED = r'ebps\races\afrika_korps\vehicles\halftrack_250_20mm_ak'
READER_SHA = 'cd125bffa33e525cd4b94a753d65a6e4cbb199395e3860e57e85003db7665f79'
RAW_LAYOUT_SHA = 'cd72046d436678632e47840fecd47fe58f4574a54002e57e24b6d8092d44e2e7'
EXPECTED = {
    'CoH3/dev/COH3SimpleTool/CoH3MemoryTool/Kwk38Upgrade.cs': (920, '1e56617e2213d1928f2129fe2db6bd97d075a625341036328002759047932e8b'),
    SRC + 'GameLayout52426.cs': (4524, '77c44effbf42d9f6300cfd41e753712bed85e551b487f4db1a779df528dc8d92'),
    SRC + 'GrenadeUpgrade.cs': (1286, '6a80659f1360a8c578f07d00e22fcf1a69544f6777867b1cb159934733f338c1'),
    SRC + 'Kwk38Upgrade.cs': (920, '1e56617e2213d1928f2129fe2db6bd97d075a625341036328002759047932e8b'),
    SRC + 'PlayerInfoReader.cs': (104468, 'bbb359cd59409065d8b4d00ace784243ae5e99816611bc11617e7c9ed8adba9d'),
    SRC + 'ProductionJob.cs': (825, '7d9dc6ce83868fc2b9a05cf3746b7b454885f4525e05f8fc0a19007cffaf567a'),
    BASE + 'fresh02-250-conversion-hover-t.png': (3032928, 'c28fde01f133560449310e3b7515208603c0d9bffa592a705c2f8f37554211fd'),
    BASE + 'fresh02-kwk38-complete-paused.png': (3133891, '6e3af16cdd5b94dcaea7491533a6d39ad49a3b98abebfc77c5c88fcfcd96b67a'),
    BASE + 'fresh02-kwk38-initial-paused.png': (3089380, 'ac9bbd447f8a6bee0cc1b173691beff8ee8661f6e6473beff9bf0705261e2b92'),
    BASE + 'fresh02-kwk38-mid-paused.png': (3129145, '4e19355e3b31157861500bbaadd13b71bea2d6f1fa65ff1ec1886edcb7c4dbaf'),
    BASE + 'fresh02-kwk38-native-timer-start-hover.png': (3093438, 'f18e9b5eb2182d02a187c9607772a1c7d9e93f630d8ca7f2196b9cf0efc6a4e1'),
    BASE + 'maintenance-run-30-fresh02-kwk38-start/maintenance-live.json': (10631, '5376dfeaf421263aa6e46a30c85e0bbe4494a2bc0b0f3e2811cb539a13bb6fca'),
    BASE + 'maintenance-run-30-fresh02-kwk38-start/maintenance-live.txt': (8786, 'a701596ba5b37ba3a035b1325df701636b5debb217b3e15e144d8036cb62461c'),
    BASE + 'maintenance-run-31-fresh02-kwk38-mid/maintenance-live.json': (11008, '763c19a2dd8a24517c29e2e8a0a2e13ffc13dbe06ba0357c4c148642a8c86e6a'),
    BASE + 'maintenance-run-31-fresh02-kwk38-mid/maintenance-live.txt': (9163, 'a616045580276aaf1e1bab630b543129b28b529423f03b33f60bb88c0b495c19'),
    BASE + 'maintenance-run-32-fresh02-kwk38-complete/maintenance-live.json': (10960, 'f91dc5a38298a011ded3cdcf069a557989f65e76b99e5b199ad7af519fc161f9'),
    BASE + 'maintenance-run-32-fresh02-kwk38-complete/maintenance-live.txt': (9115, '54496212be87a275b16c96b065df2e58df653a71229870f628727b31321d939b'),
    PROD + 'fresh02-kwk38-complete-raw.json': (16468, 'c0649381dbab43fb9c7da126e4d65548e242d4a40a3e86dec196556e8a5b4b1e'),
    PROD + 'fresh02-kwk38-initial-raw.json': (19733, '2f28fa062af5d76579367c99b6e3926ed639831686cccfb63eb20c25d623f808'),
    PROD + 'fresh02-kwk38-mid-raw.json': (18093, '3c8ff4522fc53b66449342e733918f1c7323076267ac9040a2b853f2509c4392'),
    PROD + 'fresh02-kwk38-start-driver.json': (973, '2f94dff4d5b5c5df8fad2f150e4ce7b6376f300678b0cf69c974cf12545ec58b'),
    PROD + 'production_queue_probe.py': (11785, '41781ea1bb1cb45bec9ac019054a0aa1d50b6f94cf6619372113f54166917e5a'),
    PROD + 'run30-driver-identity.json': (951, '6994f95928e7ef9e4a397d08b67bc15e8b517a308093a1f70115b5539f37db29'),
    PROD + 'run30-driver-observation-correction.json': (1477, '7fba90a903d72959aad7b5a4ce4566c9e8ed9a5c86344d0ed058e68d2db6e017'),
    PROD + 'run30-driver-result.json': (254, '18d303c80b1dc98d0b901b1803273966c30fcd4bc2aad2b77aec43a8bb907f3b'),
    PROD + 'run30-stderr.txt': (0, 'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855'),
    PROD + 'run30-stdout.txt': (377, '1df8a5066b83ddfef681617937029642ea96e833a9108a064560aee719e1785a'),
    PROD + 'run31-driver-result.json': (835, '7454c3e413c4d9cd9a38123ebd8a2fd57f63bb840dbcd5b1ae8d7e457d928d6a'),
    PROD + 'run31-stdout-stderr.txt': (760, 'a3aa68d8f561007b905afd45b26514f4ffc84150fd162b78d91c26c66ddd2fb3'),
    PROD + 'run32-driver-result.json': (849, '1a0d8757c5e694a4ae7bc51f71744382e7f0a4515fdbbfde8242c5ee72784e3c'),
    PROD + 'run32-stdout-stderr.txt': (770, 'a6c5e0448941efed1c61780dba0255dba532d1bc818dfe3900fde3108fe8b26f'),
}


def require(condition, label):
    if not condition:
        raise ValueError(label)


def one(rows, predicate, label):
    matches = [r for r in rows if predicate(r)]
    require(len(matches) == 1, label)
    return matches[0]


def stamp(value):
    return datetime.fromisoformat(value.replace('Z', '+00:00'))


def decode(data):
    encoding = 'utf-16' if data.startswith((b'\xff\xfe', b'\xfe\xff')) else 'utf-8-sig'
    return data.decode(encoding)


def check_blob(name, data):
    size, sha = EXPECTED[name]
    require(len(data) == size and hashlib.sha256(data).hexdigest() == sha, 'input_drift:' + name)


def load_inputs(root):
    blobs = {name: (root / name).read_bytes() for name in EXPECTED}
    for name, data in blobs.items():
        check_blob(name, data)
    docs = {name: json.loads(decode(data)) for name, data in blobs.items() if name.endswith('.json')}
    return blobs, docs


def feature(report, name):
    return one(report['results'], lambda r: r['Feature'] == name, 'unique_feature:' + name)


def local(rows):
    return one(rows, lambda r: r['Slot'] == 1, 'unique_local_slot')


def check_sequence(blobs, docs):
    stages, checks = [], []
    start_driver = docs[PROD + 'fresh02-kwk38-start-driver.json']
    identity_driver = docs[PROD + 'run30-driver-identity.json']
    end_driver = docs[PROD + 'run30-driver-result.json']
    correction = docs[PROD + 'run30-driver-observation-correction.json']
    require((start_driver['pid'], start_driver['gameStartUtc']) == (PID, START), 'start_driver_identity')
    require((identity_driver['gamePid'], identity_driver['gameStartUtc']) == (PID, START), 'run30_game_identity')
    require(identity_driver['readerPid'] == end_driver['readerPid'] == correction['readerPid'] == 31592, 'run30_runner_identity')
    require(identity_driver['readerSha256'].lower() == READER_SHA, 'run30_recorded_reader_hash')
    require(end_driver['gamePid'] == PID and end_driver['gameStartUnchanged'] is True and end_driver['reportExists'] is True, 'run30_end_identity')
    require(end_driver['exitCode'] is None and correction['driverRecordedExitCode'] is None, 'run30_os_exit_unknown')
    require(correction['reportDeclaredExitCode'] == 5 and correction['osExitCodeAcceptance'] is False, 'run30_correction_retained')
    require(correction['reportSha256'] == EXPECTED[BASE + 'maintenance-run-30-fresh02-kwk38-start/maintenance-live.json'][1], 'run30_correction_report_hash')
    require(correction['driverReportedFailure'] == 'Reader reported failure: ', 'run30_wrapper_failure_retained')
    require(blobs[PROD + 'run30-stderr.txt'] == b'', 'run30_empty_runner_stderr_is_not_wrapper_success')
    require([a['action'] for a in start_driver['nativeActions']] == ['native_click_kwk38', 'resume', 'pause'], 'start_actions')
    checks.append('driver identity, recorded binary hash, original run30 failure and unknown OS exit preserved')
    for n, raw_stage, run_stage, remaining in ((30, 'initial', 'start', 456), (31, 'mid', 'mid', 244), (32, 'complete', 'complete', None)):
        raw_path = PROD + f'fresh02-kwk38-{raw_stage}-raw.json'
        report_path = BASE + f'maintenance-run-{n}-fresh02-kwk38-{run_stage}/maintenance-live.json'
        raw, report = docs[raw_path], docs[report_path]
        require(raw['pid'] == PID and raw['sourceSha256'] == RAW_LAYOUT_SHA, f'run{n}_raw_identity_layout')
        require(raw['stableRootsAfterRead'] is True and raw['readFailures'] == [], f'run{n}_raw_stability')
        player = one(raw['players'], lambda r: r['slot'] == 1, 'raw_local_slot')
        require(player['identity'] == '0x' + IDENTITY, f'run{n}_raw_local_identity')
        entity = one(player['productionEntities'], lambda r: r['entity'] == ENTITY, 'raw_target_entity')
        require(entity['vehicle'] is True and entity['health'] > 0, f'run{n}_raw_live_vehicle')
        require(entity['queueComponent']['rtti']['name'] == '.?AVSquadProductionQueueExt@@', f'run{n}_squad_queue')
        require(entity['queueComponent']['lookupRva'] == '0x628a7e2', f'run{n}_inventory_lookup')
        require(report['maintenanceOnly'] is True and report['buildVerifiedByThisRun'] is False, f'run{n}_maintenance_limits')
        require(report['visualVerification'] == 'not_run' and report['exitCode'] == 5, f'run{n}_visual_exit_limits')
        require(report['readerOpenAttempts'] == report['snapshotAttempts'] == 1 and report['eventWindowSeconds'] == 0, f'run{n}_one_short_runner')
        require(all(r['BuildStatus'] == 'FingerprintMismatch' for r in report['results'] if r['BuildStatus'] is not None), f'run{n}_build_mismatch_retained')
        res = local(feature(report, 'snapshot/resources')['Evidence'])
        require(res['identity'] == IDENTITY and res['IsLocalPlayer'] is True and res['Race'] == 'afrika_korps', f'run{n}_csharp_identity')
        production = feature(report, 'production')
        row = local(production['Evidence'])
        require(row['ProductionKnown'] is True and row['Kwk38Known'] is True and row['tickRatePerSecond'] == 8, f'run{n}_known_tick_flags')
        require(row['GrenadeRemainingTicks'] is None, f'run{n}_not_grenade')
        units = local(feature(report, 'units')['Evidence'])
        squad_path = (VEHICLE if remaining is not None else CONVERTED).replace('ebps', 'sbps', 1)
        require(units['UnitsKnown'] is True and {'Key': squad_path, 'Value': 1} in units['units'], f'run{n}_squad_blueprint')
        markers = [m for m in feature(report, 'squad_markers')['Evidence'] if m['Slot'] == 1 and m['Path'] == squad_path]
        require(len(markers) == 1, f'run{n}_marker_blueprint')
        if remaining is not None:
            job = one(row['jobs'], lambda j: j['Path'] == RESEARCH, 'unique_kwk38_job')
            require(job == {'Path': RESEARCH, 'Key': KEY, 'RemainingTicks': remaining, 'TotalTicks': 480, 'RemainingSeconds': math.ceil(remaining / 8)}, f'run{n}_csharp_job_values')
            require(entity['blueprint'] == VEHICLE and entity['queueCount'] == 1, f'run{n}_raw_initial_vehicle')
            require(entity['itemBlueprint'] == RESEARCH and entity['itemRtti']['name'] == '.?AVSquadUpgradeProductionItem@@', f'run{n}_research_type')
            require(entity['itemRtti']['vtableRva'] == '0x3d92058', f'run{n}_upgrade_vtable')
            require(entity['entry']['remainingTicks'] == remaining and entity['entry']['totalTicks'] == 480, f'run{n}_raw_csharp_ticks')
            entry = bytes.fromhex(entity['entryRaw'])
            item = bytes.fromhex(entity['itemRaw64'])
            require(len(entry) == 192 and len(item) == 64, f'run{n}_raw_extents')
            require(struct.unpack_from('<ff', entry, 48) == (remaining, 480), f'run{n}_entry_raw_ticks')
            require(struct.unpack_from('<Q', entry, 0)[0] == entity['entry']['entryIdentity'] == 4294968297, f'run{n}_entry_identity')
            require(struct.unpack_from('<I', entry, 4)[0] == entity['entry']['generation'] == 1, f'run{n}_entry_generation')
            require(int(raw['arena'], 16) + struct.unpack_from('<I', entry, 184)[0] == int(entity['itemAddress'], 16), f'run{n}_item_pointer')
            require(struct.unpack_from('<Q', item, 0)[0] - int(raw['moduleBase'], 16) == 0x3D92058, f'run{n}_raw_vtable')
            require(entity['timerInvariant'] is True and entity['stableQueueRecheck'] is True and entity['derivedRemainingSeconds'] == math.ceil(remaining / 8), f'run{n}_raw_seconds_stability')
            require(int(KEY.split(':')[0]) == int(entity['entity'], 16), f'run{n}_key_entity_prefix')
            require(production['Status'] == 'observed', f'run{n}_production_observed')
        else:
            require(row['jobs'] == [] and production['Status'] == 'sample_absent', 'run32_csharp_queue_empty')
            require(entity['blueprint'] == CONVERTED and entity['queueCount'] == 0 and entity['status'] == 'empty_queue', 'run32_raw_converted_empty')
            require('entry' not in entity and 'itemBlueprint' not in entity, 'run32_remaining_is_absent_not_zero')
        txt = decode(blobs[report_path.replace('.json', '.txt')])
        lines = txt.splitlines()
        require(lines[0] == 'MAINTENANCE ONLY; read success is not build/visual verification; exitCode=5', f'run{n}_text_limits')
        at = one(list(enumerate(lines)), lambda pair: pair[1].startswith('production: '), 'text_production_header')[0]
        require(json.loads(lines[at + 2]) == production['Evidence'], f'run{n}_json_text_production_agree')
        stdout_path = PROD + ('run30-stdout.txt' if n == 30 else f'run{n}-stdout-stderr.txt')
        require(f"production: {production['Status']} samples={production['SampleCount']}" in decode(blobs[stdout_path]), f'run{n}_stdout_agrees')
        if n == 30:
            require(stamp(start_driver['nativeActions'][-1]['utc']) < stamp(raw['utc']) < stamp(identity_driver['readerStartUtc']) < stamp(report['snapshotCapturedUtc']) < stamp(report['utc']) < stamp(end_driver['utc']), 'run30_chronology')
            require(correction['productionEvidence'] == row, 'run30_correction_production_agrees')
        else:
            driver = docs[PROD + f'run{n}-driver-result.json']
            require((driver['gamePid'], driver['gameStartUtc'], driver['osExitCode']) == (PID, START, 5), f'run{n}_direct_exit_identity')
            require(driver['readerSha256'].lower() == READER_SHA and 'direct LASTEXITCODE' in driver['scope'], f'run{n}_recorded_reader_direct_exit')
            require([a['action'] for a in driver['nativeActions']] == ['resume', 'pause'], f'run{n}_native_progression')
            times = [a['utc'] for a in driver['nativeActions']] + [raw['utc'], driver['startedUtc'], report['snapshotCapturedUtc'], report['utc'], driver['finishedUtc']]
            require(all(stamp(a) < stamp(b) for a, b in zip(times, times[1:])), f'run{n}_chronology')
        checks.append(f'run{n}: raw bytes, C# JSON, text, stdout, native-action driver chronology, local identity and limits agree')
        stages.append({'run': n, 'stage': raw_stage, 'rawPath': raw_path, 'reportPath': report_path, 'rawUtc': raw['utc'], 'snapshotCapturedUtc': report['snapshotCapturedUtc'], 'arena': raw['arena'], 'moduleBase': raw['moduleBase'], 'playerAddress': player['playerAddress'], 'entity': ENTITY, 'entityBlueprint': entity['blueprint'], 'squadBlueprint': squad_path, 'queueAddress': entity['queueAddress'], 'queueCount': entity['queueCount'], 'entryAddress': entity.get('entryAddress'), 'itemAddress': entity.get('itemAddress'), 'key': KEY if remaining is not None else None, 'remainingTicks': remaining, 'totalTicks': 480 if remaining is not None else None, 'derivedSeconds': math.ceil(remaining / 8) if remaining is not None else None, 'productionKnown': row['ProductionKnown'], 'kwk38Known': row['Kwk38Known']})
    for field in ('arena', 'moduleBase', 'playerAddress', 'entity'):
        require(len({s[field] for s in stages}) == 1, 'stable_cross_stage_' + field)
    for field in ('queueAddress', 'entryAddress', 'itemAddress', 'key', 'totalTicks'):
        require(stages[0][field] == stages[1][field], 'same_active_queue_' + field)
    require(all(stamp(a['snapshotCapturedUtc']) < stamp(b['rawUtc']) for a, b in zip(stages, stages[1:])), 'cross_stage_order')
    checks.append('active queue identity/total preserved; 456 > 244; completion is missing job plus converted entity/squad, never a synthesized zero timer')
    sky = decode(blobs[SRC + 'Kwk38Upgrade.cs'])
    simple = decode(blobs['CoH3/dev/COH3SimpleTool/CoH3MemoryTool/Kwk38Upgrade.cs'])
    require(sky == simple and RESEARCH.replace('\\', '\\\\') in sky, 'dual_kwk38_source_identical_exact_path')
    require('StartsWith("halftrack_250", StringComparison.OrdinalIgnoreCase)' in sky and '!row.Kwk38Known || row.Kwk38Upgrading.Count == 0' in sky, 'kwk38_prefix_status_semantics')
    reader = decode(blobs[SRC + 'PlayerInfoReader.cs'])
    for source_line in ('vehicle ? 64557040 : 64557304', 'vehicle && Kwk38Upgrade.IsResearch(path)', 'entity + ":" + U(entity + 96)', 'row.Kwk38Upgrading.Add(productionJob2)', 'Component(Resolve(arena, U(entity + 124)), 103319618L)'):
        require(source_line in reader, 'reader_source:' + source_line)
    layout = decode(blobs[SRC + 'GameLayout52426.cs']).replace('\r\n', '\n').replace('\r', '\n')
    mapping = {int(k): int(v, 16) for k, v in re.findall(r'\{\s*(\d+),\s*0x([0-9A-Fa-f]+)\s*\}', layout)}
    require(mapping[64557040] == 0x3D92058 and mapping[103319618] == 0x628A7E2, 'relevant_current_layout_mappings')
    require(hashlib.sha256(layout.encode('utf8')).hexdigest() == RAW_LAYOUT_SHA, 'raw_digest_matches_current_normalized_layout_text')
    probe = decode(blobs[PROD + 'production_queue_probe.py'])
    for source_line in ("text = source.read_text(encoding='utf8')", "hashlib.sha256(text.encode('utf8')).hexdigest()", 'mapping = {int(k): int(v, 16)', 'index = read(b+mapping[role]+kind*128, 1)[0]', 'from live_probe import ProcessReader'):
        require(source_line in probe, 'probe_hash_and_mapping_mechanism:' + source_line)
    require('SOURCE_SHA' not in probe, 'probe_has_no_imported_source_sha_constant')
    checks.append('probe computes normalized UTF8 text SHA and parses current source mapping; raw digest equals current LF-normalized layout text, separately pinned from CRLF byte SHA')
    require('GrenadeUpgrade.SecondsRemaining(RemainingTicks)' in decode(blobs[SRC + 'ProductionJob.cs']), 'remaining_seconds_delegation')
    require('Math.Ceiling((double)ticks / 8.0)' in decode(blobs[SRC + 'GrenadeUpgrade.cs']), 'ticks_divisor_and_ceiling')
    checks.append('current dual Kwk38 filters identical; relevant reader key/queue/type and ceil(ticks/8) source semantics present')
    for name, data in blobs.items():
        if name.endswith('.png'):
            require(data.startswith(b'\x89PNG\r\n\x1a\n') and struct.unpack('>II', data[16:24]) == (1936, 1119), 'png_dimensions:' + name)
    checks.append('five native PNG hashes and full-resolution dimensions bound; no OCR or machine countdown comparison')
    return stages, checks


def validate(root):
    root = Path(root).resolve()
    blobs, docs = load_inputs(root)
    stages, checks = check_sequence(blobs, docs)
    return {
        'schemaVersion': 1,
        'scope': 'offline audit of one local AK KwK38 native conversion sequence; candidate maintenance only',
        'localSequenceVerified': True,
        'fullAcceptance': False,
        'newBaseline': False,
        'sameReaderObjectProven': False,
        'productionGuiFlowVerified': False,
        'nativeCountdownAgreementVerified': False,
        'game': {'pid': PID, 'startUtc': START, 'localSlot': 1, 'localIdentity': IDENTITY, 'startBinding': 'historical driver records; raw captures have pid but no process start time'},
        'reader': {'recordedExeSha256': READER_SHA, 'binaryHashSource': 'run30 identity and run31/32 driver results; current mutable EXE is not read', 'instances': 'three separate short-lived runner invocations; one reader open / one Snapshot per report', 'rawReportedNormalizedLayoutTextSha256': RAW_LAYOUT_SHA, 'currentLayoutByteSha256': EXPECTED[SRC + 'GameLayout52426.cs'][1], 'currentNormalizedLayoutTextSha256': RAW_LAYOUT_SHA, 'currentLayoutByteCount': 4524, 'currentLayoutNormalizedTextByteCount': 4431, 'currentLayoutCrlfCount': 93, 'probeSource': PROD + 'production_queue_probe.py', 'probeHashMechanism': 'lines24-26 read UTF8 text with universal newline conversion and parse mapping; line62 hashes text.encode(utf8), not raw bytes or an imported SOURCE_SHA constant', 'probeMappingMechanism': 'component/roots lookups use the parsed mapping; fixed GUARDS role labels/candidate RVAs are diagnostic coverage constants', 'historicalSourceClosureOrBinaryEqualityClaimed': False},
        'stages': stages,
        'checks': checks,
        'historicalExitCodes': {'run30': {'wrapperExitCode': 1, 'wrapperExitCodeProvenance': 'lead task-14 original tool observation; not an OS exit code and not reconstructed from the JSON', 'driverRecordedExitCode': None, 'osExitCode': None, 'osExitCodeAccepted': False, 'reportDeclaredExitCode': 5, 'originalFailureRetained': 'Reader reported failure: '}, 'run31': {'osExitCode': 5, 'method': 'driver records direct LASTEXITCODE'}, 'run32': {'osExitCode': 5, 'method': 'driver records direct LASTEXITCODE'}},
        'wholeReportLimits': {'maintenanceOnly': True, 'buildVerifiedByThisRun': False, 'BuildStatus': 'FingerprintMismatch', 'visualVerification': 'not_run', 'reportExitCode': 5, 'notPromoted': ['F02 complete matrix', 'production GUI', 'same-object recovery', 'full feature acceptance', 'new supported baseline']},
        'nativeHumanObservations': [
            {'path': BASE + 'fresh02-250-conversion-hover-t.png', 'gameTime': '03:07', 'observation': 'KwK38型自动火炮改装 tooltip; nominal 60s; fuel15', 'basis': 'offline human image review; no OCR'},
            {'path': BASE + 'fresh02-kwk38-native-timer-start-hover.png', 'gameTime': '03:10', 'observation': 'KwK38 tooltip identity; no remaining numeric countdown established', 'basis': 'lead native correlation and offline human image review'},
            {'path': BASE + 'fresh02-kwk38-initial-paused.png', 'gameTime': '03:10', 'observation': 'paused; selected 250轻型运兵车', 'basis': 'lead native correlation and offline human image review'},
            {'path': BASE + 'fresh02-kwk38-mid-paused.png', 'gameTime': '03:36', 'observation': 'paused; selected 250轻型运兵车', 'basis': 'lead native correlation and offline human image review'},
            {'path': BASE + 'fresh02-kwk38-complete-paused.png', 'gameTime': '04:13', 'observation': 'paused; KwK38 completion notification and 250/9装甲半履带车 name', 'basis': 'lead native correlation and offline human image review'},
        ],
        'limitations': [
            'Human screenshot annotations are not asserted by this machine checker; only file identity and dimensions are machine checked.',
            '57 and 31 seconds are ceil(ticks/8) arithmetic; native remaining-digit equality and countdown precision are not established.',
            'After capture has queueCount=0 and no entry; remainingTicks/totalTicks are absent, not measured zero.',
            'Raw entry generation1 and entryIdentity4294968297 are separate from C# entity-generation key suffix2807501192; only key prefix is independently tied to raw entity address.',
            'Completion changes the raw queue address; same active queue is asserted for initial/mid only.',
            'Historical driver startUTC binds the captures to the game process; raw files do not independently record startUTC.',
            'Raw sourceSha256 is SHA256 of UTF8 text after universal-newline normalization, computed from the read source rather than an imported SOURCE_SHA constant. Current layout raw-byte SHA77c44 differs from text SHAcd720 because 93 CRLF pairs normalize to LF; current normalized full-text digest equals the raw reported value. This does not attest a historical source closure or compiled EXE.',
            'The inspected probe parses the read layout mapping and uses it for component/roots lookups; its fixed GUARDS candidate-RVA labels are diagnostic coverage constants. The probe source itself is hash-pinned without importing or executing its ProcessReader.',
            'No current runtime manifest or mutable compiled executable is an input; recorded EXE hashes are historical evidence only.',
            'Snapshot features share one captured snapshot timestamp, not atomic cross-feature state; separate raw and runner captures were made during the recorded pause.',
        ],
        'inputCount': len(EXPECTED),
        'inputBytes': sum(len(data) for data in blobs.values()),
        'inputs': [{'path': name, 'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest()} for name, data in sorted(blobs.items())],
        'verifier': {'path': SELF, 'bytes': (root / SELF).stat().st_size, 'sha256': hashlib.sha256((root / SELF).read_bytes()).hexdigest()},
    }


def report_markdown(result, root):
    def link(name, label=None, line=None):
        target = (root / name).as_posix() + (':' + str(line) if line else '')
        return '[' + (label or Path(name).name) + '](<' + target + '>)'
    lines = [
        '# KwK38 局部转换前中后证据核验', '',
        '离线核验通过：同一游戏进程中的一辆 AK 250，其 KwK38 升级队列从 456/480 ticks 降至 244/480 ticks，完成后队列为空且实体、小队蓝图均为 20mm 型。结论仅覆盖这条局部原生转换序列。', '',
        f'游戏 PID `{PID}`，历史 driver 启动时间 `{START}`；本地 slot1 / identity `{IDENTITY}`。三次 C# 采样为相同记录 EXE hash 的独立短命 runner；每份各 open1 / Snapshot1。没有同一 reader 对象的连续证据，也没有生产 GUI 流程通过的结论。', '',
        '| 阶段 | 原生画面人工关联 | raw / C# | 队列与蓝图 |', '|---|---|---|---|',
    ]
    for stage, timer in zip(result['stages'], ('03:10', '03:36', '04:13')):
        number = f"{stage['remainingTicks']}/480 → ceil(ticks/8)={stage['derivedSeconds']}秒" if stage['remainingTicks'] is not None else 'jobs=[]；remaining 未采得'
        lines.append(f"| run{stage['run']} {stage['stage']} | {timer} 暂停 | {link(stage['rawPath'], 'raw')} / {link(stage['reportPath'], 'C#')}：{number} | queueCount={stage['queueCount']}；`{stage['entityBlueprint'].split(chr(92))[-1]}` |")
    lines += ['', f'活动队列实体 `{ENTITY}`，C# key `{KEY}`；initial/mid 共用 queue `0x2940a99ae80`、entry `0x2940bb82e00`、item `0x29407ef2fc0`、entryIdentity `4294968297`、entry generation `1`，totalTicks 均为 480。raw entry 的 +48/+52 float 已从 192B 字节直接重解，item vtable 从 64B 字节重解为 RVA `0x3D92058` / `SquadUpgradeProductionItem`；车辆队列类型 `SquadProductionQueueExt`，inventory lookup `0x628A7E2`。', '',
        'C# key 后缀是源码 entity+96 的 generation；raw entry generation=1 是另一字段。这里仅将 key 的地址前缀独立对回 raw 实体，并检查两次 C# key 完全相同。完成后的 queue 地址变为 `0x2940a999680`；报告保留这一变化，不把完成后队列冒充原活动队列。', '',
        '三次本地 `ProductionKnown=true`、`Kwk38Known=true`。完成时 raw entity 是 `ebps\\races\\afrika_korps\\vehicles\\halftrack_250_20mm_ak`；C# UnitsKnown 的小队路径以及 squad_markers 都为对应 `sbps` 路径。队列空项与转换蓝图共同构成完成证据，缺席项没有补写成剩余 0 ticks。', '',
        '## 原生截图关联与计时界限', '',
    ]
    for item in result['nativeHumanObservations']:
        lines.append(f"- {link(item['path'])}：{item['gameTime']}，{item['observation']}。来源：{item['basis']}。")
    lines += ['', '截图文本、时间与转换提示是 lead 的原生关联及本次人工图片复核；机器仅固定截图 hash、字节数与 1936×1119 尺寸，没有 OCR。57→31秒属于源码算术换算；尚无原生剩余数字倒计时逐值一致性。tooltip 标称60s与480/8的算术相符也不扩大该结论。', '',
        '## 源码对应', '',
        f"- 双引擎 {link(SRC + 'Kwk38Upgrade.cs', 'SkyEye Kwk38Upgrade.cs', 8)} / {link('CoH3/dev/COH3SimpleTool/CoH3MemoryTool/Kwk38Upgrade.cs', 'Simple Kwk38Upgrade.cs', 8)} 完全同 hash：AK vehicles 前缀及 halftrack_250 前缀，IsResearch 精确匹配 `{RESEARCH}`；Status 仅在 Known 且活动 jobs 非空时生成升级文字。",
        f"- {link(SRC + 'PlayerInfoReader.cs', 'PlayerInfoReader.cs:2702', 2702)} 从车辆分支调用 ReadProduction(vehicle:true)；{link(SRC + 'PlayerInfoReader.cs', 'PlayerInfoReader.cs:2896', 2896)} 解析 squad inventory 与 +16 队列链接；{link(SRC + 'PlayerInfoReader.cs', 'PlayerInfoReader.cs:2943', 2943)} 选择 SquadUpgrade guard，{link(SRC + 'PlayerInfoReader.cs', 'PlayerInfoReader.cs:2960', 2960)} 过滤研究路径并以 entity+96 组成 key，随后追加 Kwk38Upgrading。",
        f"- {link(SRC + 'GameLayout52426.cs', 'GameLayout52426.cs:51', 51)} 的相关角色映射为 SquadUpgrade→0x3D92058 / inventory→0x628A7E2；{link(SRC + 'ProductionJob.cs', 'ProductionJob.cs:15', 15)} 将秒数委托给 {link(SRC + 'GrenadeUpgrade.cs', 'GrenadeUpgrade.cs:20', 20)} 的 ceil(ticks/8)。", '',
        f"raw 三份记录的 layout sourceSha256 是 `{RAW_LAYOUT_SHA}`；当前完整 layout 原始字节SHA是 `{result['reader']['currentLayoutByteSha256']}`，这两个完整摘要值保留不同口径。{link(PROD + 'production_queue_probe.py', 'production_queue_probe.py:24', 24)} 实际读取 UTF8 文本并用 regex 解析 mapping，{link(PROD + 'production_queue_probe.py', 'production_queue_probe.py:62', 62)} 执行 sha256(text.encode('utf8'))，没有导入 SOURCE_SHA 常量。Python read_text 的通用换行将当前93处CRLF归一化：4524原始bytes变为4431文本bytes；归一化文本SHA恰为 raw 记录的 cd720…，所以这里不是 layout 内容漂移的证据。component/roots lookup 使用实际解析的 mapping；GUARDS 的候选RVA常量用于标签/coverage诊断。此核对不证明历史 source 闭包或编译 EXE 关联。历史 driver EXE SHA=`{READER_SHA}`；本次未读取可变的当前 EXE 或其他运行时 manifest，未导入执行 probe 的 ProcessReader。", '',
        '## 退出码和整体验收状态', '',
        f"run30 的 {link(PROD + 'run30-driver-result.json')} 保留 exitCode=null；{link(PROD + 'run30-driver-observation-correction.json')} 记录 reportDeclaredExitCode=5、osExitCodeAcceptance=false 和原始 Reader reported failure。lead/task-14 记录的 wrapper exit1 作为原始工具观察保留；它不是 runner OS 退出码，OS 退出码仍为未知。run31/32 的 driver 均直接记录 LASTEXITCODE=5。", '',
        '三份整报告持续为 `maintenanceOnly=true`、`buildVerifiedByThisRun=false`、`FingerprintMismatch`、`visualVerification=not_run`、report exitCode=5；局部核验不会倒写原报告。此结果不代表 F02 全矩阵、GUI、全功能或新增支持基线通过。', '',
        '## 可重复离线入口', '',
        '从仓库根运行（默认只检查，不创建文件）：', '', '```powershell',
        'python -B .\\CoH3\\dev\\SkyEye\\evidence\\version-adaptation\\production\\verify_kwk38_sequence.py --self-test',
        '```', '',
        f"`validate(root)` 返回确定的核验对象。`--write` 仅用于首轮创建指定 JSON 和本报告；任何目标已存在时退出，不覆写。入口以固定路径清单读取{result['inputCount']}项输入，按字节数+SHA逐项核验后再检查字段、字节、时序、文本一致性；不打开进程、GUI、游戏、授权或网络。没有修改引擎。", '',
        f"机器结果：{link(OUTPUTS[0])}。验证器：{link(SELF)}，SHA256 `{result['verifier']['sha256']}`。", '',
        '## 输入清单', '', '| 输入 | bytes | SHA-256 |', '|---|---:|---|',
    ]
    for item in result['inputs']:
        lines.append(f"| {link(item['path'], item['path'])} | {item['bytes']} | `{item['sha256']}` |")
    lines += ['', f"共 {result['inputCount']} 项 / {result['inputBytes']} bytes。验证器自己的 hash 单列，输出文件不作为自己的输入。", '']
    return '\n'.join(lines)


def self_test(root):
    blobs, docs = load_inputs(root)
    report30 = BASE + 'maintenance-run-30-fresh02-kwk38-start/maintenance-live.json'
    report32 = BASE + 'maintenance-run-32-fresh02-kwk38-complete/maintenance-live.json'
    cases = [
        ('ticks_mismatch', lambda d: local(feature(d[report30], 'production')['Evidence'])['jobs'][0].update(RemainingTicks=455)),
        ('key_changed', lambda d: local(feature(d[report30], 'production')['Evidence'])['jobs'][0].update(Key='2834896846336:2')),
        ('known_false', lambda d: local(feature(d[report30], 'production')['Evidence']).update(Kwk38Known=False)),
        ('completion_job_retained', lambda d: local(feature(d[report32], 'production')['Evidence'])['jobs'].append({'Path': RESEARCH})),
        ('whole_build_passed', lambda d: d[report32].update(buildVerifiedByThisRun=True)),
        ('run30_os_exit_invented', lambda d: d[PROD + 'run30-driver-result.json'].update(exitCode=5)),
        ('raw_byte_timer_conflict', lambda d: local_raw_entry(d).update(remainingTicks=455)),
    ]
    passed = []
    for label, change in cases:
        mutant = copy.deepcopy(docs)
        change(mutant)
        try:
            check_sequence(blobs, mutant)
        except ValueError:
            passed.append(label)
        else:
            raise ValueError('negative_control_accepted:' + label)
    try:
        name = PROD + 'fresh02-kwk38-initial-raw.json'
        check_blob(name, blobs[name] + b' ')
    except ValueError:
        passed.append('input_hash_drift')
    else:
        raise ValueError('negative_control_accepted:input_hash_drift')
    try:
        ensure_outputs_new([root / SELF])
    except FileExistsError:
        passed.append('existing_output_preflight')
    else:
        raise ValueError('negative_control_accepted:existing_output_preflight')
    return passed


def local_raw_entry(docs):
    player = one(docs[PROD + 'fresh02-kwk38-initial-raw.json']['players'], lambda r: r['slot'] == 1, 'raw_local_slot')
    return one(player['productionEntities'], lambda r: r['entity'] == ENTITY, 'raw_target_entity')['entry']


def ensure_outputs_new(paths):
    for path in paths:
        if path.exists():
            raise FileExistsError('output_exists:' + str(path))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[6])
    parser.add_argument('--write', action='store_true', help='exclusively create the two named reports')
    parser.add_argument('--self-test', action='store_true', help='also check mutated in-memory negative controls')
    args = parser.parse_args()
    root = args.root.resolve()
    result = validate(root)
    tests = self_test(root) if args.self_test else []
    if args.write:
        paths = [root / name for name in OUTPUTS]
        ensure_outputs_new(paths)
        payloads = [json.dumps(result, ensure_ascii=False, indent=2) + '\n', report_markdown(result, root)]
        for path, text in zip(paths, payloads):
            with path.open('x', encoding='utf-8', newline='\n') as handle:
                handle.write(text)
    print(json.dumps({'localSequenceVerified': result['localSequenceVerified'], 'fullAcceptance': False, 'inputCount': result['inputCount'], 'remainingTicks': [s['remainingTicks'] for s in result['stages']], 'derivedSeconds': [s['derivedSeconds'] for s in result['stages']], 'negativeControlsPassed': tests, 'wroteReports': args.write}, ensure_ascii=True))


if __name__ == '__main__':
    main()
