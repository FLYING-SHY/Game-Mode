import re, sys
p = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
data = open(p, 'rb').read()
pat = re.compile(rb"[ -~]{5,}")
keys = sys.argv[1:]
for m in pat.finditer(data):
    s = m.group().decode('ascii', 'ignore')
    low = s.lower()
    if any(k in low for k in keys):
        print(f"{m.start():08X}\t{s}")
