"""Normalize official aggregator defaults to the measurements actually available."""
from pathlib import Path
import json
root=Path(__file__).resolve().parent/'iteration-1'
p=root/'benchmark.json'
b=json.loads(p.read_text(encoding='utf-8'))
b['metadata'].update(runs_per_configuration=1,executor_model='Host child default; exact model not reported',analyzer_model='Host independent graders; exact model not reported')
for run in b['runs']:
    for key in ['time_seconds','tokens','tool_calls','errors']:
        run['result'].pop(key,None)
for config, stats in b['run_summary'].items():
    for key in ['time_seconds','tokens']:
        stats.pop(key,None)
b['notes']=[
'每题每配置仅一次；三题合计六次执行。这是方法与交接质量检查，不是跨游戏成功率估计。',
'宿主未给出完整代理任务耗时、token和总工具调用数，已移除聚合器用0填充的资源统计；模型测试耗时不能代替代理耗时。',
'无技能基线仍能使用既有通用技能与本机工具；两组都有相同公开输入。新技能组另读主技能及按需参考资料。',
'第二案例生成器与协议公开布局，属于已知材料下的字节发现验证，并非隐藏oracle盲测；评分者另以毒化ground truth检查定位算法独立性。',
'所有真实进程、真实游戏、跨实际会话与新机器交付验证仍为not_run。'
]
p.write_text(json.dumps(b,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
md=root/'benchmark.md'
if md.exists(): md.read_text(encoding='utf-8')
lines=['# 首轮对照评估','', '| 任务 | 使用技能 | 基线 |','|---|---:|---:|']
for eid in sorted(set(r['eval_id'] for r in b['runs'])):
    rows={r['configuration']:r['result'] for r in b['runs'] if r['eval_id']==eid}
    cell=lambda c:f"{rows[c]['passed']}/{rows[c]['total']}" if c in rows else 'missing'
    lines.append(f"| {eid} | {cell('with_skill')} | {cell('without_skill')} |")
lines += ['', *['- '+note for note in b['notes']], '', '完整证据和评分理由见各 run-1/grading.json，质性分析见 analysis.md。']
md.write_text('\n'.join(lines)+'\n',encoding='utf-8')
print('Normalized benchmark: one run per case/configuration; unknown resource metrics omitted.')
