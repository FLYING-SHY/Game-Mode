import pefile, capstone, struct, json, sys

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
pe = pefile.PE(P, fast_load=True)
raw = open(P, "rb").read()

secs = []
for s in pe.sections:
    secs.append(dict(name=s.Name.rstrip(b"\x00").decode(), va=s.VirtualAddress,
                     ro=s.PointerToRawData, rs=s.SizeOfRawData, vs=s.Misc_VirtualSize))

def va2off(va):
    for s in secs:
        if s["va"] <= va < s["va"] + max(s["rs"], s["vs"]):
            return s["ro"] + (va - s["va"])
    return None

def off2va(off):
    for s in secs:
        if s["ro"] <= off < s["ro"] + s["rs"]:
            return s["va"] + (off - s["ro"])
    return None

# --- parse .pdata for function boundaries ---
pdata = [s for s in secs if s["name"] == ".pdata"][0]
pd = raw[pdata["ro"]:pdata["ro"] + pdata["rs"]]
funcs = []
for i in range(0, len(pd) - 11, 12):
    begin, end, unwind = struct.unpack_from("<III", pd, i)
    if begin and end and end > begin:
        funcs.append((begin, end, unwind))
funcs.sort()
print("functions:", len(funcs))

# find function containing an RVA
import bisect
starts = [f[0] for f in funcs]
def func_of(rva):
    i = bisect.bisect_right(starts, rva) - 1
    if i >= 0 and funcs[i][0] <= rva < funcs[i][1]:
        return funcs[i]
    return None

if __name__ == "__main__":
    for a in sys.argv[1:]:
        rva = int(a, 16)
        f = func_of(rva)
        print(a, "->", f"0x{f[0]:X}-0x{f[1]:X} size=0x{f[1]-f[0]:X}" if f else None)
