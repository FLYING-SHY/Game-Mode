"""Print every .pdata entry whose range contains a target RVA (handles chained EH regions)."""
import struct
import sys

import pefile

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()

secs = {}
for s in pe.sections:
    secs[s.Name.rstrip(b"\x00").decode()] = (s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData)

pva, pro, prs = secs[".pdata"]
pd = raw[pro:pro + prs]

targets = [int(a, 16) for a in sys.argv[1:]]
for t in targets:
    hits = []
    for i in range(0, len(pd) - 11, 12):
        begin, end, unwind = struct.unpack_from("<III", pd, i)
        if begin and end and begin <= t < end:
            hits.append((begin, end, unwind))
    print("target 0x%08X -> %d .pdata entries" % (t, len(hits)))
    for b, e, u in hits:
        print("    begin=0x%08X end=0x%08X (size=0x%X) unwind=0x%08X" % (b, e, e - b, u))
    exact = [h for h in hits if h[0] == t]
    print("    exact-start entry: %s" % ("YES" if exact else "NO"))
