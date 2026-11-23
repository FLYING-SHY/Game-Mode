"""Exact bounded read-only production-path evidence. Explicit PID/output; no login/game control."""
import argparse, ctypes, datetime, hashlib, json, math, pathlib, re, struct, sys, time
sys.dont_write_bytecode = True
HERE = pathlib.Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parent))
from live_probe import ProcessReader

GUARDS = {64584880: ('SpawnProductionItem', 0x3D98BF0), 64557040: ('SquadUpgradeProductionItem', 0x3D92058), 64557304: ('EntityUpgradeProductionItem', 0x3D92160)}

def parse_entry(raw):
    if len(raw) != 192: raise ValueError('Production entry short read: expected192 actual%d' % len(raw))
    return {'remainingTicks': struct.unpack_from('<f', raw, 48)[0], 'totalTicks': struct.unpack_from('<f', raw, 52)[0], 'itemOffset': struct.unpack_from('<I', raw, 184)[0], 'entryIdentity': struct.unpack_from('<Q', raw)[0], 'generation': struct.unpack_from('<I', raw, 4)[0]}

def main():
    p = argparse.ArgumentParser(); p.add_argument('--self-test', action='store_true'); p.add_argument('--pid',type=int); p.add_argument('--out'); a = p.parse_args()
    if a.self_test:
        raw = bytearray(192); struct.pack_into('<ff', raw, 48, 80, 160); struct.pack_into('<I', raw, 184, 0x1234)
        assert parse_entry(raw)['remainingTicks'] == 80 and parse_entry(raw)['itemOffset'] == 0x1234
        for n in [0, 191, 193]:
            try: parse_entry(bytes(n)); raise AssertionError('short/overread accepted')
            except ValueError: pass
        print('PASS entry-layout fixture; PASS exact-length guard 0/191/193; no process opened'); return
    if not a.pid or not a.out: p.error('--pid and --out required')
    source = HERE.parents[2] / 'client' / 'CoH3MemoryTool' / 'GameLayout52426.cs'
    text = source.read_text(encoding='utf8')
    mapping = {int(k): int(v, 16) for k, v in re.findall(r'\{\s*(\d+),\s*0x([0-9A-Fa-f]+)\s*\}', text)}
    started = time.perf_counter()
    r = ProcessReader(a.pid); b = r.base
    requested = 0; read_calls = 0; failures = []; stage = 'initialize'
    original_read = r.read
    def read(address, length):
        nonlocal requested, read_calls
        if time.perf_counter() - started > 10: raise TimeoutError('10 second diagnostic budget exceeded')
        requested += length; read_calls += 1
        if requested > 2 * 1024 * 1024 or read_calls > 20000: raise ValueError('diagnostic byte/call budget exceeded')
        try: return original_read(address, length)
        except Exception as ex:
            failures.append({'stage': stage, 'address': hex(address), 'requestedLength': length, 'error': str(ex)})
            raise
    r.read = read
    def resolve(arena, offset):
        if offset == 0xffffffff: raise ValueError('arena offset unavailable FFFFFFFF')
        return arena + offset
    def rtti(obj):
        vt = r.q(obj); info = {'vtable': hex(vt), 'vtableRva': hex(vt - b)}
        if not b <= vt < b + 0x7000000: return info
        col = r.q(vt - 8); td = r.u(col + 12)
        info.update(colRva=hex(col-b), typeDescriptorRva=hex(td), name=read(b+td+16, 192).split(b'\0')[0].decode('ascii', errors='replace'))
        return info
    def component(entity, role, arena):
        kind = struct.unpack('<H', read(entity+76, 2))[0]
        index = read(b+mapping[role]+kind*128, 1)[0]
        info = {'roleLegacy': hex(role), 'lookupRva': hex(mapping[role]), 'kind': kind, 'index': index}
        if index == 255: return 0, info
        descriptor = r.q(b+mapping[102260448]) + r.u(entity+64) + index*24
        raw = read(descriptor, 24); mode = struct.unpack_from('<I', raw)[0]; value = struct.unpack_from('<Q', raw, 8)[0]; state = raw[16]
        info.update(descriptor=hex(descriptor), raw=raw.hex(), mode=mode, value=hex(value), state=state)
        if not state & 1 or not value: return 0, info
        if mode > 8: raise ValueError('Component mode out of bounds: %d' % mode)
        address = value if mode == 8 else r.q(b+mapping[102260400]+mode*16)+(value&0xffffffff)
        info.update(address=hex(address), rtti=rtti(address)); return address, info
    report = {'utc': datetime.datetime.now(datetime.timezone.utc).isoformat(), 'pid': a.pid, 'moduleBase': hex(b), 'access': '0x410 PROCESS_QUERY_INFORMATION|PROCESS_VM_READ', 'scope': 'production paths only; no game inputs, writes, resume, login or credential reads', 'source': str(source), 'sourceSha256': hashlib.sha256(text.encode('utf8')).hexdigest(), 'guards': [{'legacy': hex(k), 'presentInCurrentMap': k in mapping, 'currentMappedRva': hex(mapping[k]) if k in mapping else None, 'staticCandidateName': v[0], 'staticCandidateRva': hex(v[1]), 'candidateStatus': 'not_promoted_by_this_probe'} for k,v in GUARDS.items()], 'players': [], 'limitations': ['External game controls must be correlated independently; this probe does not control UI or establish progression.', 'RTTI/path equality is structural evidence, not proof of full production behavior.']}
    try:
        arena = r.q(b+mapping[102260400]); world_offset = r.u(b+mapping[98614988]); world = resolve(arena, world_offset)
        registry = resolve(arena, r.u(world+252)); count = r.u(registry+16); data = resolve(arena, r.u(registry+12))
        if not 0 < count <= 16: raise ValueError('player count out of bounds')
        report.update(arena=hex(arena), worldOffset=hex(world_offset))
        for i in range(count):
            player = resolve(arena, r.u(data+i*4)); slot = r.u(player+12)
            er = resolve(arena, r.u(player+1792)); ec = r.u(er+40); ed = resolve(arena, r.u(er+36))
            if ec > 4096: raise ValueError('entity count out of bounds')
            row = {'slot': slot, 'playerAddress': hex(player), 'productionEntities': [], 'ownedEntityCount': 0, 'identity':hex(r.q(player+8)), 'resourceRaw':read(player+1704,40).hex()}; report['players'].append(row)
            seen = set()
            for j in range(ec):
                off = r.u(ed+j*4)
                if off == 0xffffffff: continue
                entity = resolve(arena, off)
                if resolve(arena, r.u(entity+88)) != player: continue
                row['ownedEntityCount'] += 1
                blueprint = r.q(entity+56); path = r.string(blueprint+40)
                vehicle = path.lower().startswith('ebps\\races\\afrika_korps\\vehicles\\') and path.rsplit('\\',1)[-1].lower().startswith('halftrack_250')
                if not vehicle and '\\buildings\\production\\' not in path: continue
                item = {'entity': hex(entity), 'blueprint': path, 'vehicle': vehicle, 'status': 'pending'}; row['productionEntities'].append(item)
                try:
                    stage = 'health component'; health, info = component(entity, 103319626, arena); item['healthComponent'] = info
                    if not health: item['status'] = 'health_component_absent'; continue
                    hp = r.f(health+108); item['health'] = hp
                    if hp <= 0: item['status'] = 'inactive_health'; continue
                    stage = 'production component'
                    owner = resolve(arena, r.u(entity+124)) if vehicle else entity
                    queue_component, info = component(owner, 103319618 if vehicle else 103319633, arena); item['queueComponent'] = info
                    if not queue_component: item['status'] = 'queue_component_absent'; continue
                    link_offset = 16 if vehicle else 52; queue_offset = r.u(queue_component+link_offset); item['queueOffset'] = hex(queue_offset)
                    if queue_offset == 0xffffffff: item['status'] = 'queue_link_absent'; continue
                    queue = resolve(arena, queue_offset)
                    if queue in seen: item['status'] = 'duplicate_queue'; continue
                    seen.add(queue)
                    length = r.u(queue+52); start = r.u(queue+48); item.update(queueAddress=hex(queue), queueCount=length, queueDataOffset=hex(start))
                    if length > 128: raise ValueError('production count out of bounds')
                    if not length: item['status'] = 'empty_queue'; continue
                    stage = '192-byte first queue entry'; entry_address = resolve(arena, start); raw = read(entry_address, 192); entry = parse_entry(raw); item.update(entryAddress=hex(entry_address), entryRaw=raw.hex(), entry=entry)
                    stage = 'production item'; target = resolve(arena, entry['itemOffset']); type_info = rtti(target); item.update(itemAddress=hex(target), itemRtti=type_info)
                    item_blueprint = r.q(target+16); item['itemBlueprint'] = r.string(item_blueprint+40); item['itemRaw64']=read(target,64).hex();item['itemBlueprintAddress']=hex(item_blueprint)
                    ticks = entry['remainingTicks']; total = entry['totalTicks']; item['timerInvariant'] = math.isfinite(ticks) and math.isfinite(total) and 0 <= ticks <= total <= 80000 and total > 0
                    if item['timerInvariant']: item['derivedRemainingSeconds'] = math.ceil(ticks/8)
                    item['stableQueueRecheck'] = r.u(queue_component+link_offset)==queue_offset and r.u(queue+52)==length and r.u(queue+48)==start and r.u(entry_address+184)==entry['itemOffset'] and r.q(entry_address)==entry['entryIdentity']
                    guard_order = [64557040] if vehicle else [64584880, 64557304]
                    missing = [key for key in guard_order if key not in mapping]
                    item['sourceGuardEvaluationOrder'] = [hex(key) for key in guard_order]
                    item['firstMissingGuard'] = hex(missing[0]) if missing else None
                    item['sourceOutcome'] = 'Resolve throws before any row commit; catch sets '+('Kwk38Known=false' if vehicle else 'ProductionKnown=false') if missing else 'No missing guard in captured source; runtime comparison still requires verification'
                    item['status'] = 'nonempty_queue_observed'
                except Exception as ex: item.update(status='read_error', stage=stage, error=repr(ex))
        report['stableRootsAfterRead'] = r.q(b+mapping[102260400])==arena and r.u(b+mapping[98614988])==world_offset
        items = [item for row in report['players'] for item in row['productionEntities']]
        report['coverage'] = {name: 'observed' if any(item.get('itemRtti', {}).get('vtableRva') == hex(rva) for item in items) else 'sample_absent' for name, rva in GUARDS.values()}
        report['status'] = 'read_error' if any(item['status'] == 'read_error' for item in items) else 'observed_nonempty_queue' if any(item['status'] == 'nonempty_queue_observed' for item in items) else 'sample_absent'
    except Exception as ex: report.update(status='probe_error', error=repr(ex), stage=stage)
    finally:
        report.update(readCalls=read_calls, requestedBytes=requested, successfulBytes=r.bytes_read, readFailures=failures, elapsedMilliseconds=round((time.perf_counter()-started)*1000))
        r.close()
        out=pathlib.Path(a.out); out.write_text(json.dumps(report,ensure_ascii=False,indent=2,allow_nan=False),encoding='utf8')
    print(json.dumps({'out':str(out),'readCalls':read_calls,'bytes':r.bytes_read,'readFailures':len(failures),'stableRoots':report.get('stableRootsAfterRead'), 'queues': [{'slot':row['slot'],'status':item['status'],'path':item['blueprint'],'count':item.get('queueCount'),'rtti':item.get('itemRtti'),'itemBlueprint':item.get('itemBlueprint'),'firstMissingGuard':item.get('firstMissingGuard')} for row in report['players'] for item in row['productionEntities']]},ensure_ascii=False))

    return 3 if report.get('status') in ('probe_error', 'read_error') or failures or not report.get('stableRootsAfterRead') else 0

if __name__=='__main__': sys.exit(main())
