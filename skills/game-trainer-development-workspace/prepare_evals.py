from pathlib import Path
import json
root = Path(__file__).resolve().parent
skill = root.parent / 'game-trainer-development'
evals = json.loads((skill/'evals/evals.json').read_text(encoding='utf-8'))
for case in evals['evals']:
    directory = root/'iteration-1'/f"eval-{case['id']}-{case['name']}"
    directory.mkdir(parents=True, exist_ok=True)
    metadata = {'eval_id':case['id'],'eval_name':case['name'],'prompt':case['prompt'],'assertions':[]}
    (directory/'eval_metadata.json').write_text(json.dumps(metadata, ensure_ascii=False, indent=2),encoding='utf-8')
    for config in ['with_skill','without_skill']:
        (directory/config/'run-1'/'outputs').mkdir(parents=True,exist_ok=True)
print('Prepared 3 cases, 6 run directories; assertions pending.')
