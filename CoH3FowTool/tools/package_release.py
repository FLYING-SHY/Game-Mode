"""Package the already-published 2.0 release, source and observed evidence."""
from pathlib import Path
import hashlib
import re
import shutil
import zipfile
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
NAME = "CoH3FowTool-2.0-win-x64"
STAGE = ROOT / "dist" / NAME
STAGE.mkdir(parents=True, exist_ok=True)
EXE = ROOT / "publish" / "CoH3FowTool.exe"
if not EXE.is_file():
    EXE = ROOT / "CoH3FowTool.exe"
if not EXE.is_file():
    raise SystemExit("Publish the self-contained program first.")
shutil.copy2(EXE, STAGE / "CoH3FowTool.exe")

fixed = ["README.md", "CoH3FowTool.csproj", "app.manifest", "build.ps1", "启动(管理员).bat", "tools/package_release.py"]
for rel in fixed:
    target = STAGE / rel
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / rel, target)
for path in (ROOT / "src").glob("*.cs"):
    target = STAGE / "src" / path.name
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(path, target)
for rel in ("tools/input-test/InputHarness.cs", "tools/input-test/InputHarness.csproj"):
    target = STAGE / rel
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / rel, target)

reverse = ROOT / "tools" / "reverse"
evidence = [
    "validation_2_0.md", "verify_patches.py", "verify_display_2_0.log",
    "release_status_running.log", "release_status_reverted.log",
    "fow_release_off.png", "fow_release_on.png", "fow_release_running.png",
    "fow_release_enemy_off.png", "fow_release_enemy_on.png", "fow_fixed_on.png",
]
evidence += [path.name for path in reverse.glob("console_diag_*") if path.is_file()]
for name in sorted(set(evidence)):
    target = STAGE / "tools" / "reverse" / name
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(reverse / name, target)

# A labeled comparison for reading, with the full originals retained alongside it.
canvas = Image.new("RGB", (1920, 634), "#15202b")
font = ImageFont.truetype(r"C:\Windows\Fonts\msyh.ttc", 25)
small = ImageFont.truetype(r"C:\Windows\Fonts\msyh.ttc", 21)
draw = ImageDraw.Draw(canvas)
for index, (name, label) in enumerate([
    ("fow_release_enemy_off.png", "关闭 · 对局 03:41"),
    ("fow_release_enemy_on.png", "开启 · 对局 03:44"),
]):
    with Image.open(reverse / name) as image:
        canvas.paste(image.convert("RGB").resize((960, 540), Image.Resampling.LANCZOS), (index * 960, 46))
    draw.text((index * 960 + 18, 8), label, font=font, fill="white")
draw.text((18, 599), "最终 2.0.0 程序 · 本地遭遇战 · 同一镜头，两帧相隔约 3 秒", font=small, fill="white")
comparison = reverse / "fow_release_comparison.png"
canvas.save(comparison)
shutil.copy2(comparison, STAGE / "tools" / "reverse" / comparison.name)

# Verify all relative Markdown links in the documents included in this package.
for document in STAGE.rglob("*.md"):
    for link in re.findall(r"\]\(<([^>]+)>\)", document.read_text(encoding="utf-8")):
        if "://" in link or link.startswith("#"):
            continue
        target = (document.parent / link.split("#", 1)[0]).resolve()
        if not target.exists():
            raise SystemExit(f"Missing packaged Markdown target: {document.relative_to(STAGE)} -> {link}")

# PowerShell redirection may produce UTF-16 logs; ship text logs as UTF-8.
for path in STAGE.rglob("*.log"):
    raw = path.read_bytes()
    if raw.startswith((b"\xff\xfe", b"\xfe\xff")):
        path.write_text(raw.decode("utf-16"), encoding="utf-8")
selftest = STAGE / "selftest.log"
if not selftest.is_file() or "自检全部通过" not in selftest.read_text(encoding="utf-8-sig"): 
    raise SystemExit("The staged single-file executable must pass selftest before packaging.")

files = sorted(path for path in STAGE.rglob("*") if path.is_file() and path.name != "SHA256SUMS.txt")
manifest = "".join(f"{hashlib.sha256(path.read_bytes()).hexdigest()}  {path.relative_to(STAGE).as_posix()}\n" for path in files)
(STAGE / "SHA256SUMS.txt").write_text(manifest, encoding="utf-8")
archive = ROOT / "dist" / f"{NAME}.zip"
with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=6) as output:
    for path in sorted(STAGE.rglob("*")):
        if path.is_file():
            output.write(path, (Path(NAME) / path.relative_to(STAGE)).as_posix())
with zipfile.ZipFile(archive) as output:
    corrupt = output.testzip()
    if corrupt:
        raise SystemExit(f"Archive CRC error: {corrupt}")
    for line in manifest.splitlines():
        expected, rel = line.split("  ", 1)
        actual = hashlib.sha256(output.read(f"{NAME}/{rel}")).hexdigest()
        if actual != expected:
            raise SystemExit(f"Archive SHA256 mismatch: {rel}")
print(f"Archive: {archive}")
print(f"Bytes: {archive.stat().st_size}")
print(f"Files: {len(files) + 1}")
print(f"SHA256: {hashlib.sha256(archive.read_bytes()).hexdigest()}")
print("Archive CRC, per-file SHA256, selftest record and Markdown links: PASS")
