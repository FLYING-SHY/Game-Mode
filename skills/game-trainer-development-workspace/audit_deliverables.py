"""Audit owned skill resources without modifying originals or the skill tree."""
from pathlib import Path
import json, re, hashlib, zipfile, sys
root = Path(__file__).resolve().parent
skill = root.parent/'game-trainer-development'
checks = {}
text = (skill/'SKILL.md').read_text(encoding='utf-8')
checks['main_under_500_lines'] = len(text.splitlines()) < 500
required = sorted(set(re.findall(r'`((?:references|scripts|assets)/[^`]+)`',text)))
checks['resources_exist'] = all((skill/p).is_file() for p in required)
checks['python_source_compiles'] = True
for p in skill.rglob('*.py'):
    compile(p.read_text(encoding='utf-8'),str(p),'exec')
checks['utf8_documents'] = True
for p in skill.rglob('*.md'):
    p.read_text(encoding='utf-8')
missing=[]
for p in skill.rglob('*.md'):
    for target in re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')):
        if target.startswith(('https://','http://','#')): continue
        target=target.split('#')[0].strip('<>')
        if target and not (p.parent/target).exists(): missing.append({'source':str(p.relative_to(skill)), 'target':target})
checks['repository_document_links_exist'] = not missing
archive=root/'dist/game-trainer-development.skill'
archive_info=None
if archive.exists():
    with zipfile.ZipFile(archive) as z:
        files=z.namelist()
        checks['archive_crc'] = z.testzip() is None
        checks['archive_entrypoint'] = 'game-trainer-development/SKILL.md' in files
        checks['archive_excludes_evals_and_caches'] = all('/evals/' not in n and '__pycache__' not in n and not n.endswith('.pyc') for n in files)
        checks['archive_matches_sources'] = all(z.read(n)==(skill.parent/n).read_bytes() for n in files)
        archive_info={'path':str(archive),'sha256':hashlib.sha256(archive.read_bytes()).hexdigest(),'files':files,'size':archive.stat().st_size}
report={'passed':all(checks.values()),'checks':checks,'required_resources':required,'missing_links':missing,'archive':archive_info}
(root/'artifact-audit.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
sys.exit(0 if report['passed'] else 1)
