"""Third stage: hunt for cheat-command constants / strings in the current exe."""
import struct

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

raw = open(P, "rb").read()


def find_all(pat, data=None):
    data = raw if data is None else data
    out = []
    i = data.find(pat)
    while i != -1:
        out.append(i)
        i = data.find(pat, i + 1)
    return out


def show(tag, pat):
    hits = find_all(pat)
    print("%-34s %-28s hits=%d  %s" % (
        tag, pat.hex(" "), len(hits), " ".join("0x%08X" % h for h in hits[:10])))
    return hits


# cheat command name hash used by the old build
show("old cheat cmd hash (movabs)", struct.pack("<Q", 0x5ff018d0c7448e70))
show("old cheat cmd hash (be)", struct.pack(">Q", 0x5ff018d0c7448e70))

for s in (b"revealed fow", b"MOD -- (CHEAT)", b"PCMD_CheatRevealAll",
          b"cheat_revealall", b"SimWorld::DoCommand",
          b"SimWorld::DoCommand - (sender %u) trying to run a cheat command (%s) from an untrusted source"):
    hits = find_all(s)
    print("%-34s %-28s hits=%d  %s" % (
        "string", repr(s[:28]), len(hits), " ".join("0x%08X" % h for h in hits[:10])))

# the reveal-all PCMD id 0x83 is built with mov edx,0x83 ; look for the pattern
# 'mov edx, 0x83'  = BA 83 00 00 00
show("mov edx,0x83", bytes.fromhex("BA 83 00 00 00"))
show("mov ecx,0x83", bytes.fromhex("B9 83 00 00 00"))
