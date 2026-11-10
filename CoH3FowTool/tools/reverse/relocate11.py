"""Find callers of the console-command registration function and all
references to the +0x680 cheat-block flag on command objects."""
import struct

import capstone
import pefile

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"

pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()

secs = {}
for s in pe.sections:
    secs[s.Name.rstrip(b"\x00").decode()] = (
        s.VirtualAddress, s.PointerToRawData, s.SizeOfRawData)
tva, tro, trs = secs[".text"]
text = raw[tro:tro + trs]

md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)

REG_FN = 0x00B63410
target = 0x140000000 + REG_FN

print("=== callers of console registration fn 0x%X ===" % REG_FN)
hits = []
for i in range(len(text) - 5):
    if text[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", text, i + 1)[0]
    rva = tva + i
    if 0x140000000 + rva + 5 + rel == target:
        hits.append(rva)
for h in hits:
    print("  call at rva 0x%08X (file 0x%08X)" % (h, tro + (h - tva)))

print()
print("=== raw occurrences of disp32 0x680 in .text ===")
pat = b"\x80\x06\x00\x00"
pos = 0
while True:
    j = text.find(pat, pos)
    if j < 0:
        break
    rva = tva + j
    # find instruction start: back up a few bytes, then disassemble a window
    start = max(0, j - 12)
    chunk = text[start:j + 24]
    base = 0x140000000 + tva + start
    best = None
    for ins in md.disasm(chunk, base):
        if ins.address <= 0x140000000 + rva < ins.address + ins.size:
            best = ins
            break
    if best:
        print("  rva 0x%08X  %s %s" % (rva, best.mnemonic, best.op_str))
    pos = j + 1
