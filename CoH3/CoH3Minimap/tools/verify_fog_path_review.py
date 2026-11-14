"""Offline QA for the new static Fog-path review; no process/game access."""
from pathlib import Path
from datetime import datetime
import hashlib,json,re
ROOT=Path(__file__).resolve().parents[1]
HERE=ROOT/'artifacts/research-fog-path-20261112'
REVERSE=ROOT.parent/'CoH3FowTool/tools/reverse/native_fow_20261111'
report=HERE/'FOG_PATH_REVIEW.md'
array_report=REVERSE/'descriptor_arrays_20261112/DESCRIPTOR_ARRAYS_REPORT.md'
checks={}
def check(name,condition):
    checks[name]=bool(condition)
    if not condition:raise AssertionError(name)
def digest(path):
    h=hashlib.sha256()
    with path.open('rb') as f:
        for data in iter(lambda:f.read(1048576),b''):h.update(data)
    return h.hexdigest()
def load(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def links(path):
    found=[]
    for target in re.findall(r'\[[^\]\n]+\]\(([^)\n]+)\)',path.read_text(encoding='utf-8')):
        target=target.strip().strip('<>').split('#',1)[0]
        if not target or re.match(r'^(https?://|mailto:)',target):continue
        local=(path.parent/target).resolve()
        check('local_link_'+str(len(found))+'_'+path.name,local.is_file())
        found.append(dict(Target=str(local),Sha256=digest(local)))
    return found
text=report.read_text(encoding='utf-8');array_text=array_report.read_text(encoding='utf-8')
review_links=links(report);array_links=links(array_report)
check('review_has_all_ten_local_stages',len(review_links)==14 and '201F10' in text and '200130' in text and '2016D0' in text)
check('current_scope_distinguished',all(s in text for s in ('当前帧','Level B','均未验收','前端 R+28','当前执行路径未闭合')))
v=load(REVERSE/'descriptor_arrays_20261112/verification.json')
check('arrays_stored_checks_pass',v['Passed'] and len(v['Checks'])==105 and all(x is True for x in v['Checks'].values()))
check('arrays_coverage',v['OriginalRanges']==11 and v['OriginalBytes']==690 and v['OriginalInstructions']==159 and v['ActualReaDeclaredBodyComplete'] and not v['MissingDeclaredInstructions'])
check('arrays_acceptance_stays_false',all(v[k] is False for k in ('InterfaceIdentityVerified','HardwareSlotMappingVerified','CurrentDrawProgramVerified','SRVBindingVerified','ActiveFogSRVVerified','FogCompositionVerified','MovementSyncMeasured','LevelBAccepted')))
asm=(REVERSE/'descriptor_arrays_20261112/original.asm').read_text(encoding='utf-8')
addresses={int(line.split()[0],16) for line in asm.splitlines() if line.strip()}
check('printed_array_instruction_locations',all(a in addresses for a in (0x201f10,0x201f3d,0x201f1e,0x201f19,0x201f2b,0x202061,0x202082,0x202088,0x20208b)))
c=(REVERSE/'descriptor_arrays_20261112/decompiled/functions/0x140201f10.c').read_text(encoding='utf-8')
check('quoted_truncation_line_is_real','*plVar7 = (ulonglong)(uint)(iVar13 * iVar4) + param_4;' in c and 'zero_extend32' in array_text)
validation_path=ROOT/'dist/validation-0.5.1-native-preview-20261112-041209.json'
validation=load(validation_path);exe=Path(validation['Exe']);archive=Path(validation['Zip'])
check('same_release_exe_sha',digest(exe).upper()==validation['ExeSha256']=='9B89AF4D3D2DE3F0803A1E314E2F5221E7AAA932891C19C0D3899F630EC20DFB')
check('same_release_zip_sha',digest(archive).upper()==validation['ZipSha256'])
check('document_release_hashes',validation['ExeSha256'] in text.upper() and validation['ZipSha256'] in text.upper())
check('only_existing_current_zip',list((ROOT/'dist').glob('*.zip'))==[archive])
false_keys=('FogActiveUiCompositionVerified','TemporalBlendExact','ContinuousMovementSyncMeasured','SameSceneVisualComparisonPassed','LevelBAccepted')
check('release_five_acceptance_fields_false',all(validation[k] is False and k in text for k in false_keys))
observation=ROOT/'artifacts/research-live-20261112-1204/CURRENT_OBSERVATION.md'
obs=observation.read_text(encoding='utf-8')
check('same_timestamped_observation',all(s in obs and s in text for s in ('12:02:44','12:07:44','551','29','19','08:55')))
check('readme_links_new_review','artifacts/research-fog-path-20261112/FOG_PATH_REVIEW.md' in (ROOT/'README.md').read_text(encoding='utf-8'))
result=dict(CreatedAt=datetime.now().astimezone().isoformat(),EvidenceConsistencyPassed=all(checks.values()),Checks=checks,
 Report=str(report),ReportSha256=digest(report),ArrayReport=str(array_report),ArrayReportSha256=digest(array_report),
 ReviewLinks=review_links,ArrayReportLinks=array_links,ArraysStoredCheckCount=105,ArraysOriginalBytes=690,ArraysOriginalInstructions=159,
 ReleaseExeSha256=digest(exe),ReleaseZipSha256=digest(archive),ReleaseValidationSha256=digest(validation_path),
 Acceptance={k:validation[k] for k in false_keys},ApplicationRebuilt=False,PublishedSelftestsRerun=False,
 RuntimeProcessAccessed=False,GameWritten=False)
with (HERE/'review-verification.json').open('x',encoding='utf-8') as f:json.dump(result,f,ensure_ascii=False,indent=2);f.write('\n')
print(json.dumps(dict(Passed=result['EvidenceConsistencyPassed'],Checks=len(checks),ReviewLinks=len(review_links),ArrayLinks=len(array_links),ReleaseUnchanged=True,Acceptance=result['Acceptance']),indent=2))
