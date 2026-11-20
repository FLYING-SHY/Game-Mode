from pathlib import Path
import json
root=Path(__file__).resolve().parent/'iteration-1'
for case in sorted(root.glob('eval-*')):
    metadata=json.loads((case/'eval_metadata.json').read_text(encoding='utf-8'))
    for config in ['with_skill','without_skill']:
        run=case/config/'run-1'
        (run/'eval_metadata.json').write_text(json.dumps(metadata,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        # Agent completion events expose no exact end-to-end duration or token counts.
        # Times in execution.json describe fixture subprocesses, not the complete agent run.
        timing={'status':'not_reported_by_host','missing_fields':['total_tokens','duration_ms','total_duration_seconds'],'note':'Fixture subprocess durations are not full agent durations; no estimate substituted.'}
        (run/'timing.json').write_text(json.dumps(timing,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('6 reviewer prompts copied; unreported agent metrics explicitly null.')
