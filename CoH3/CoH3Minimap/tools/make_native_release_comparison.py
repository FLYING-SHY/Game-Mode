from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'artifacts'
game_path = ART / 'native-window-35148-20261112-004347-624493.png'
app_path = ART / 'native-window-36056-20261112-004347-624493.png'
release_report = ROOT / 'dist' / 'validation-0.5-native-preview-20261112-004209.json'
probe_path = ART / 'native-release-live-20261112-0043.json'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


assert sha(app_path) == 'DF1CC857804757048E4CBDED8AB620ADAE4811FA5AED3A6125F2BF53CDFEFB0E'
assert sha(game_path) == '4B32A4A84860B2757ACE933AF536748320E40B7FA3450AB8D82D79E0743EEA27'
canvas = Image.new('RGB', (1160, 700), '#16191c')
draw = ImageDraw.Draw(canvas)
font = ImageFont.truetype(r'C:\Windows\Fonts\msyh.ttc', 23)
small = ImageFont.truetype(r'C:\Windows\Fonts\msyh.ttc', 18)
draw.text((24, 12), '游戏原生小地图（裁切）', font=font, fill='white')
draw.text((600, 12), '外部 0.5 单文件发行窗口', font=font, fill='white')
with Image.open(game_path) as game:
    assert game.size == (2560, 1440)
    crop = game.crop((0, 840, 560, 1400)).resize((520, 520), Image.Resampling.LANCZOS)
    canvas.paste(crop, (24, 55))
with Image.open(app_path) as app:
    assert app.size == (400, 448)
    canvas.paste(app.convert('RGB').resize((520, 582), Image.Resampling.LANCZOS), (600, 55))
draw.text((24, 648), '同次采集，仅裁切与缩放；静止场景。未验证活跃迷雾与连续移动同步。', font=small, fill='#c4c8cc')
output = ART / 'native-release-comparison-20261112.png'
canvas.save(output)
probe = json.loads(probe_path.read_text(encoding='utf-8-sig'))
report = json.loads(release_report.read_text(encoding='utf-8-sig'))
assert probe['CapturedSamples'] == probe['RequestedSamples'] == 3
assert not probe['GameWritten'] and not probe['GameFunctionsInvoked'] and not probe['HeapScanned']
report['PublishedExeLiveProbeCapturedSamples'] = 3
report['PublishedExeLiveProbeRequestedSamples'] = 3
report['PublishedExeLiveProbePath'] = str(probe_path)
report['PublishedExeWindowProcessId'] = 36056
report['PublishedExeStaticSceneFeaturesVisuallyReviewed'] = ['native map identity', 'rotation', '39 native marks', 'territory red/blue and boundaries', 'camera viewport']
report['StaticSceneComparisonImage'] = str(output)
report['StaticSceneComparisonImageSha256'] = sha(output)
report['OriginalWindowImages'] = [{'Path': str(p), 'Sha256': sha(p)} for p in [game_path, app_path]]
report['ContinuousMovementSyncMeasured'] = False
report['FogActiveUiCompositionVerified'] = False
report['SameSceneVisualComparisonPassed'] = False
report['LevelBAccepted'] = False
release_report.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'comparison': str(output), 'sha256': sha(output), 'release_report': str(release_report), 'live_samples': 3, 'full_level_b_accepted': False}, indent=2))
