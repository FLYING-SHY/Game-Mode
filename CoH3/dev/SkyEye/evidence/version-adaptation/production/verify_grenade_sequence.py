"""Reconcile actual queue raws and actual licensed reader reports; never attaches."""
import json
from pathlib import Path
p=Path(__file__).resolve().parent
root=p.parent
def feature(n,name):
    report=json.loads((root/n/'maintenance-live.json').read_text(encoding='utf8'))
    assert report['licenseState']=='active' and report['exitCode']==5
    return next(r['Evidence'] for r in report['results'] if r['Feature']==name)
def queue(name):
    r=json.loads((p/name).read_text(encoding='utf8'))
    assert r['pid']==31572 and r['stableRootsAfterRead'] and not r['readFailures']
    return next(item for row in r['players'] if row['slot']==0 for item in row['productionEntities'] if item['blueprint'].endswith('\\barracks_us'))
names=['maintenance-run-16-grenade-active','maintenance-run-17-grenade-middle','maintenance-run-18-grenade-complete']
path='upgrade\\american\\research\\barracks\\grenade_riflemen_us'
raw=[queue(n) for n in ['grenade-queue-01.json','grenade-queue-02.json','grenade-queue-03-complete.json']]
rows=[next(r for r in feature(n,'production') if r['Slot']==0) for n in names]
assert raw[0]['queueAddress']==raw[1]['queueAddress']==raw[2]['queueAddress']
assert raw[0]['entryAddress']==raw[1]['entryAddress']
assert raw[0]['itemAddress']==raw[1]['itemAddress']
for q,r in zip(raw[:2],rows[:2]):
    assert q['stableQueueRecheck'] and q['timerInvariant'] and q['itemBlueprint']==path
    assert q['itemRtti']['vtableRva']=='0x3d92160'
    assert q['entry']['remainingTicks']==r['GrenadeRemainingTicks']
    assert q['entry']['totalTicks']==200
    assert r['ProductionKnown']
assert rows[0]['GrenadeRemainingTicks']>rows[1]['GrenadeRemainingTicks']>0
assert raw[2]['queueCount']==0 and rows[2]['GrenadeRemainingTicks'] is None and rows[2]['ProductionKnown']
assert path not in next(r for r in feature(names[0],'upgrades') if r['Slot']==0)['items']
assert path in next(r for r in feature(names[2],'upgrades') if r['Slot']==0)['items']
out={'status':'pass','scope':'actual US grenade queue decreasing to completion; not full build acceptance','pid':31572,'blueprint':path,'queue':raw[0]['queueAddress'],'entry':raw[0]['entryAddress'],'item':raw[0]['itemAddress'],'remainingTicks':[r['GrenadeRemainingTicks'] for r in rows],'totalTicks':200,'derivedSeconds':[r['grenadeRemainingSeconds'] for r in rows],'completedPlayerUpgrade':True,'limitations':['UI countdown numerical agreement not independently read','Sound/production overlay not verified','Kwk38 SquadUpgrade still absent','Other event families keep aggregate exit5']}
(p/'grenade-sequence-validation.json').write_text(json.dumps(out,indent=2,ensure_ascii=False),encoding='utf8')
print(json.dumps(out,ensure_ascii=False))
