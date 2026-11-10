"""Fast call-site mapper.

Scans .text for E8 (rel32) call opcodes with a regex instead of a full
capstone disassembly, then filters candidates against the .pdata function
table so false positives (rel32 bytes inside immediates) are dropped.

Usage:  python calls.py <target_rva_hex> [more_targets...]
        python calls.py --dump <target_rva_hex>
"""
import re, struct, sys, bisect, json, os

P = r"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe"
CACHE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "callmap.json")


def load():
    import pefile
    pe = pefile.PE(P, fast_load=True)
    raw = open(P, "rb").read()
    secs = []
    for s in pe.sections:
        secs.append((s.Name.rstrip(b"\x00").decode(), s.VirtualAddress,
                     s.PointerToRawData, s.SizeOfRawData, s.Misc_VirtualSize))
    text = [x for x in secs if x[0] == ".text"][0]
    code = raw[text[2]:text[2] + text[3]]
    tva = text[1]

    # function starts from .pdata
    pd = [x for x in secs if x[0] == ".pdata"][0]
    pdb = raw[pd[2]:pd[2] + pd[3]]
    funcs = []
    for i in range(0, len(pdb) - 11, 12):
        b, e, u = struct.unpack_from("<III", pdb, i)
        if b and e and e > b:
            funcs.append(b)
    funcs = sorted(set(funcs))
    return code, tva, funcs


def build_map():
    code, tva, funcs = load()
    fset = set(funcs)
    calls = {}
    for m in re.finditer(rb"\xe8", code):
        i = m.start()
        if i + 5 > len(code):
            continue
        disp = struct.unpack_from("<i", code, i + 1)[0]
        site = tva + i
        target = site + 5 + disp
        if target in fset:
            calls.setdefault(target, []).append(site)
    return calls


def main():
    code, tva, funcs = load()
    fset = set(funcs)
    starts = funcs

    if os.path.exists(CACHE):
        try:
            calls = {int(k): v for k, v in json.load(open(CACHE)).items()}
        except Exception:
            calls = build_map()
            json.dump({str(k): v for k, v in calls.items()}, open(CACHE, "w"))
    else:
        calls = build_map()
        json.dump({str(k): v for k, v in calls.items()}, open(CACHE, "w"))

    for a in sys.argv[1:]:
        t = int(a, 16)
        sites = calls.get(t, [])
        print(f"\n== call sites of 0x{t:X}: {len(sites)}")
        for s in sites[:40]:
            i = bisect.bisect_right(starts, s) - 1
            owner = starts[i] if i >= 0 else 0
            print(f"   0x{s:X}   (in func 0x{owner:X})")
        if len(sites) > 40:
            print(f"   ... +{len(sites)-40} more")


if __name__ == "__main__":
    main()
