import json
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
d = json.load(open(HERE / "p2_memxref.json", encoding="utf-8"))

for n in ["Health", "TeamId", "PersonaId", "HealthComponent", "LocalPlayer", "ClientPlayer"]:
    for r in d.get(n, []):
        c = r["code_rva"]
        o = img.rva2off(c)
        after = DATA[o:o + 40]
        # find the string the ref points at
        s = img.cstr_at_rva(r["target_str_rva"]) if r["target_str_rva"] < 0x100000000 else "?"
        print(f"{n:<18} code 0x{c:08x} form={r['form']} strrva=0x{r['target_str_rva']:x} '{s}'")
        print("     ", " ".join(f"{x:02x}" for x in after))
