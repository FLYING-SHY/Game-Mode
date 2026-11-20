from pathlib import Path
import json
root = Path(__file__).resolve().parent
path = root.parent/'game-trainer-development/evals/evals.json'
data = json.loads(path.read_text(encoding='utf-8'))
expectations = {
1: [
'准确解释OpenProcess 0x410及ReadProcessMemory/VirtualQueryEx，并不把窗口RestoreLayer66称为游戏补丁恢复。',
'解释模块相对定位与至少两种结构/读取校验，并给出可复查源码位置。',
'说明扫描对象与预算/缓存/epoch机制，而非误称通用AOB或未知初值扫描器。',
'解释覆盖层绘制或窗口机制，并区分CoH3特定布局和可迁移工程方法。',
'准确说明游戏build哈希机制与存量验证报告未覆盖完整对局的限制，不虚构本轮动态验证。'
],
2: [
'存在可运行定位脚本，从镜像扫描指令、正确按指令末端加有符号RIP位移求根，而非从答案表取布局。',
'候选按两个build分别给出模块相对根、指针链和字段，verify实际通过四session和每session三帧。',
'通过受控变化排除两个同起值decoy，定位脚本不硬编码答案字段/链偏移。',
'保存真实self-test与verify日志，0/1/多命中及恢复冲突均按预期处理。',
'明确这是离线模型且资料公开，未宣称真实进程、真实游戏或盲测验证通过。'
],
3: [
'交付实际运行的离线测试脚本及真实结果，而非只有测试计划。',
'0命中停止采用；多候选不直接选首个，使用指令目标/结构语义验证或报告歧义。',
'代码正确计算有符号RIP目标并验证预期指令/边界，含可执行断言。',
'会话身份失效有实际测试；正常补丁启用禁用恢复有实际测试。',
'恢复前检查已安装字节，外部改动引发冲突并保留现场，代码与测试一致。',
'保持build身份检查并列出尚未执行的真实游戏/跨会话验证，不以fixture替代实测结论。'
]}
for case in data['evals']:
    case['expectations'] = expectations[case['id']]
    case['assertions'] = expectations[case['id']]
    p = root/'iteration-1'/f"eval-{case['id']}-{case['name']}"/'eval_metadata.json'
    metadata = json.loads(p.read_text(encoding='utf-8'))
    metadata['assertions'] = case['expectations']
    p.write_text(json.dumps(metadata,ensure_ascii=False,indent=2),encoding='utf-8')
path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Recorded 16 substantive expectations per configuration across 3 cases.')
