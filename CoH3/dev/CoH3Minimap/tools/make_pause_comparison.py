"""Build a labelled static comparison from two existing window captures; no game access."""
from pathlib import Path
import hashlib
import json

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / 'artifacts/native-window-43932-20261112-100250-823635.png'
EXTERNAL = ROOT / 'artifacts/native-window-3532-20261112-100250-823635.png'
OUTPUT = ROOT / 'artifacts/research-round2-20261112/native-paused-comparison.png'
CROP = (18, 660, 420, 1062)  # Whole native map area, selected in the original 1936x1119 capture.


def main():
    metadata_path = OUTPUT.with_suffix('.json')
    if OUTPUT.exists() or metadata_path.exists():
        raise FileExistsError('Comparison output already exists')
    with Image.open(GAME) as source_game, Image.open(EXTERNAL) as source_external:
        if source_game.size != (1936, 1119) or source_external.size != (400, 448):
            raise ValueError('Unexpected source image size')
        native = source_game.crop(CROP).convert('RGB').resize((400, 400), Image.Resampling.LANCZOS)
        external = source_external.convert('RGB')
    image = Image.new('RGB', (1000, 650), '#16191c')
    draw = ImageDraw.Draw(image)
    title = ImageFont.truetype('msyh.ttc', 24)
    label = ImageFont.truetype('msyh.ttc', 19)
    note = ImageFont.truetype('msyh.ttc', 18)
    draw.text((24, 18), '当前暂停场景：原版与 0.5.1 预览', font=title, fill='#f0f0f0')
    draw.text((24, 65), '游戏原生：迷雾开启', font=label, fill='#e8e8e8')
    draw.text((540, 65), '外部：19/19 标记，迷雾尚未接入', font=label, fill='#e8e8e8')
    image.paste(native, (24, 104))
    image.paste(external, (540, 104))
    draw.text((24, 568), '游戏画面明确显示“战术暂停已启用”（08:55）。', font=note, fill='#dddddd')
    draw.text((24, 602), '只作静止绘制对照；未测移动误差，完整 Level B 尚未通过。', font=note, fill='#dddddd')
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    with OUTPUT.open('xb') as stream:
        image.save(stream, format='PNG')
    metadata = {
        'GameCapture': str(GAME), 'GameCaptureSha256': hashlib.sha256(GAME.read_bytes()).hexdigest(),
        'ExternalCapture': str(EXTERNAL), 'ExternalCaptureSha256': hashlib.sha256(EXTERNAL.read_bytes()).hexdigest(),
        'GameCropPixels': list(CROP), 'NativeResizePixels': [400, 400],
        'ImageEdits': ['Crop native minimap area', 'Uniform resize', 'Add labels; source pixels otherwise unchanged'],
        'Output': str(OUTPUT), 'OutputSha256': hashlib.sha256(OUTPUT.read_bytes()).hexdigest(),
        'PausedTextVisible': True, 'DisplayedGameTime': '08:55', 'ExternalDisplayedMarks': '19/19',
        'PixelExactComparisonMeasured': False, 'MovementSyncMeasured': False,
        'FogCompositionVerified': False, 'SameSceneVisualComparisonPassed': False, 'LevelBAccepted': False,
        'RuntimeProcessAccessed': False, 'GameWritten': False, 'GameFunctionsInvoked': False,
    }
    with metadata_path.open('x', encoding='utf-8') as stream:
        json.dump(metadata, stream, ensure_ascii=False, indent=2)
        stream.write('\n')
    print(json.dumps(metadata, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
