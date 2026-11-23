"""Mini decoder: build the runtime type-record map from registration stubs.

Decodes the instruction stream of a stub, tracking register values for
`lea reg,[rip+X]` and emitting `slot -> value` for stores `mov [rip+Y], reg/imm`.
"""
import json
import re
import struct
import sys
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from p2_common import Img  # noqa: E402

img = Img()
DATA = img.data
SEC = img.sec
pdata = img.blob(".pdata")
nfun = len(pdata) // 12
beg = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[0::3].astype(np.int64)
end = np.frombuffer(pdata[:nfun * 12], dtype="<u4")[1::3].astype(np.int64)

REGS = {0xC0: "rax", 0xC1: "rcx", 0xC2: "rdx", 0xC3: "rbx", 0xC4: "rsp",
        0xC5: "rbp", 0xC6: "rsi", 0xC7: "rdi", 0xC0 | 0x40: "r8"}
REGN = {"rax": 0, "rcx": 1, "rdx": 2, "rbx": 3, "rsp": 4, "rbp": 5, "rsi": 6, "rdi": 7,
        "r8": 8, "r9": 9, "r10": 10, "r11": 11, "r12": 12, "r13": 13, "r14": 14, "r15": 15}


def func_of(rva):
    i = np.searchsorted(beg, rva, side="right") - 1
    if i >= 0 and beg[i] <= rva < end[i]:
        return int(beg[i]), int(end[i])
    return None


def str_at_rva(rva, limit=60):
    s = SEC.get(img.sec_of(rva))
    if s is None:
        return None
    o = s["roff"] + (rva - s["va"])
    e = DATA.find(b"\0", o, o + limit)
    if e > o:
        b = DATA[o:e]
        if len(b) > 1 and all(32 <= c < 127 for c in b):
            return b.decode("ascii")
    return None


def decode_stub(fn_rva):
    """Return (base, slots) where slots = {slot_rva: value}."""
    fb = func_of(fn_rva)
    if fb is None:
        return None
    b, e = fb
    raw = DATA[img.rva2off(b):img.rva2off(b) + (e - b)]
    regs = {}
    slots = {}
    p = 0
    n = len(raw)
    while p < n:
        # lea reg, [rip+disp32]
        m = None
        if raw[p:p + 3] in (b"\x48\x8d\x05", b"\x4c\x8d\x05", b"\x48\x8d\x15",
                            b"\x4c\x8d\x15", b"\x48\x8d\x0d", b"\x4c\x8d\x0d",
                            b"\x48\x8d\x1d", b"\x4c\x8d\x1d", b"\x48\x8d\x25",
                            b"\x4c\x8d\x25", b"\x48\x8d\x2d", b"\x4c\x8d\x2d",
                            b"\x48\x8d\x35", b"\x4c\x8d\x35", b"\x48\x8d\x3d",
                            b"\x4c\x8d\x3d"):
            modrm = raw[p + 2]
            if modrm in (0x05, 0x0D, 0x15, 0x1D, 0x25, 0x2D, 0x35, 0x3D):
                disp = int.from_bytes(raw[p + 3:p + 7], "little", signed=True)
                t = b + p + 7 + disp
                ridx = ((raw[p + 2] >> 3) & 7) + (8 if raw[p] == 0x4C else 0)
                reg = "r" + str(ridx) if ridx >= 8 else \
                    {0: "rax", 1: "rcx", 2: "rdx", 3: "rbx", 4: "rsp",
                     5: "rbp", 6: "rsi", 7: "rdi"}[ridx]
                if reg:
                    regs[reg] = t
                m = 7
        elif raw[p:p + 2] == b"\x8d" and raw[p + 2] in (0x05, 0x0D, 0x15, 0x1D,
                                                         0x25, 0x2D, 0x35, 0x3D):
            disp = int.from_bytes(raw[p + 3:p + 7], "little", signed=True)
            t = b + p + 7 + disp
            reg = {0xC0: "rax", 0xC1: "rcx", 0xC2: "rdx", 0xC3: "rbx",
                   0xC6: "rsi", 0xC7: "rdi"}.get(raw[p + 2] & 0xF8)
            if reg:
                regs[reg] = t
            m = 7
        elif raw[p:p + 3] in (b"\x48\x89\x05", b"\x4c\x89\x05", b"\x48\x89\x0d",
                              b"\x4c\x89\x0d", b"\x48\x89\x15", b"\x4c\x89\x15",
                              b"\x48\x89\x1d", b"\x4c\x89\x1d", b"\x48\x89\x25",
                              b"\x4c\x89\x25", b"\x48\x89\x2d", b"\x4c\x89\x2d",
                              b"\x48\x89\x35", b"\x4c\x89\x35", b"\x48\x89\x3d",
                              b"\x4c\x89\x3d"):
            modrm = raw[p + 2]
            disp = int.from_bytes(raw[p + 3:p + 7], "little", signed=True)
            slot = b + p + 7 + disp
            ridx = ((raw[p + 2] >> 3) & 7) + (8 if raw[p] == 0x4C else 0)
            reg = "r" + str(ridx) if ridx >= 8 else \
                {0: "rax", 1: "rcx", 2: "rdx", 3: "rbx", 4: "rsp",
                 5: "rbp", 6: "rsi", 7: "rdi"}[ridx]
            slots[slot] = regs.get(reg, ("reg", reg)) if reg else ("reg?", None)
            m = 7
        elif raw[p:p + 2] == b"\x89" and raw[p + 2] in (0x05, 0x0D, 0x15, 0x1D,
                                                       0x25, 0x2D, 0x35, 0x3D):
            disp = int.from_bytes(raw[p + 3:p + 7], "little", signed=True)
            slot = b + p + 7 + disp
            reg = {0xC0: "eax", 0xC1: "ecx", 0xC2: "edx", 0xC3: "ebx",
                   0xC6: "esi", 0xC7: "edi"}.get(raw[p + 2] & 0xF8)
            slots[slot] = ("reg32", reg)
            m = 6
        elif raw[p:p + 3] == b"\x48\xc7\x05":
            disp = int.from_bytes(raw[p + 3:p + 7], "little", signed=True)
            slot = b + p + 11 + disp
            imm = int.from_bytes(raw[p + 7:p + 11], "little", signed=True)
            slots[slot] = ("imm", imm)
            m = 11
        elif raw[p:p + 2] == b"\xc7\x05":
            disp = int.from_bytes(raw[p + 2:p + 6], "little", signed=True)
            slot = b + p + 10 + disp
            imm = int.from_bytes(raw[p + 6:p + 10], "little", signed=True)
            slots[slot] = ("imm32", imm)
            m = 10
        elif raw[p:p + 3] == b"\x0f\x11\x05":
            disp = int.from_bytes(raw[p + 3:p + 7], "little", signed=True)
            slot = b + p + 7 + disp
            slots[slot] = ("xmm", None)
            m = 7
        elif raw[p:p + 3] == b"\x48\xb8":          # mov rax, imm64
            regs["rax"] = int.from_bytes(raw[p + 3:p + 11], "little")
            m = 10
        if m is None:
            p += 1
        else:
            p += m
    return b, e, slots


def describe(v):
    if isinstance(v, tuple):
        return f"{v[0]}"
    return "ptr"


def main():
    stubs = [int(x, 16) for x in sys.argv[1:]] or [0x68140, 0x68020]
    for fn in stubs:
        r = decode_stub(fn)
        if not r:
            print(f"{fn:08x}: no function")
            continue
        b, e, slots = r
        print(f"\n=== stub {b:08x}..{e:08x}  ({len(slots)} slots) ===")
        for slot in sorted(slots):
            v = slots[slot]
            sec = img.sec_of(slot)
            if isinstance(v, tuple) and v[0] == "imm":
                print(f"  +{slot - min(slots):#06x} slot {slot:08x} ({sec}) = imm {v[1]}")
            elif isinstance(v, int):
                s2 = img.sec_of(v)
                st = str_at_rva(v) if s2 == ".rdata" else None
                print(f"  +{slot - min(slots):#06x} slot {slot:08x} ({sec}) = {v:016x} "
                      f"{s2} {st or ''}")
            else:
                print(f"  +{slot - min(slots):#06x} slot {slot:08x} ({sec}) = {v}")


if __name__ == "__main__":
    main()
