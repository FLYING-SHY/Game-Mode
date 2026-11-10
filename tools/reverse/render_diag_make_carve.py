"""Create a separate static-render analysis carve using the verified unwind/RW builder."""
from pathlib import Path
import re

root = Path(__file__).resolve().parent
text = (root / 'online_diag_ac_carve.py').read_text(encoding='utf-8')
seeds = """SEEDS = {
    'fog_block_constructor': 0x4B1D50,
    'reveal_task_enqueue': 0xC96650,
    'reveal_task_execute': 0x1045700,
    'fog_field_packet_reader': 0x60F900,
    'field_pack_snapshot': 0x15713E8,
}
"""
text, count = re.subn(r'SEEDS = \{.*?\n\}\n', seeds, text, count=1, flags=re.S)
assert count == 1
old = """# Keep the target vtable and RTTI, and resolve its pointers against copied code.
for rva in (0x3D7D290, 0x5029FA8, 0x604A890):
    add_span(rva, 0x100)
# Preserve addresses of original writable globals as writable in the analysis PE.
assert permissions[0x5E0BECC & -PAGE] & 0x80000000
assert permissions[0x6185E00 & -PAGE] & 0x80000000
"""
assert old in text
text = text.replace(old, '# Original section permissions are retained for each included data page.\n', 1)
text = text.replace("SEEDS['periodic_update']", "SEEDS['fog_block_constructor']")
text = text.replace("ROOT/'online_diag_ac_rw2.exe'", "ROOT/'render_diag_texture_rw.exe'")
text = text.replace("ROOT/'online_diag_ac_carve2.json'", "ROOT/'render_diag_texture_carve.json'")
text = text.replace('Carve read-only AC methods', 'Carve read-only rendering methods', 1)
output = root / 'render_diag_texture_carve.py'
assert not output.exists(), 'Preserve existing analysis generator'
output.write_text(text, encoding='utf-8')
print('Created:', output)
