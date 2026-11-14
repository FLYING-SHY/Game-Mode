#!/usr/bin/env python3
"""只读 squad 枚举诊断：打印当前对局 sim World 槽位、每队玩家 ID 与坐标范围。"""
import ctypes, ctypes.wintypes as wt, struct, sys, math

TH32CS_SNAPMODULE = 0x8
k32 = ctypes.WinDLL("kernel32", use_last_error=True)

class MODULEENTRY32W(ctypes.Structure):
    _fields_ = [("dwSize", ctypes.c_ulong), ("th32ModuleID", ctypes.c_ulong),
                ("th32ProcessID", ctypes.c_ulong), ("GlblcntUsage", ctypes.c_ulong),
                ("ProccntUsage", ctypes.c_ulong), ("modBaseAddr", ctypes.c_void_p),
                ("modBaseSize", ctypes.c_ulong), ("hModule", ctypes.c_void_p),
                ("szModule", ctypes.c_wchar * 256), ("szExePath", ctypes.c_wchar * 260)]

def find_game():
    snap = k32.CreateToolhelp32Snapshot(2 | TH32CS_SNAPMODULE, 0)  # TH32CS_SNAPPROCESS=2
    class PE(ctypes.Structure):
        _fields_ = [("dwSize", ctypes.c_ulong), ("cntUsage", ctypes.c_ulong),
                    ("th32ProcessID", ctypes.c_ulong), ("th32DefaultHeapID", ctypes.c_size_t),
                    ("th32ModuleID", ctypes.c_ulong), ("cntThreads", ctypes.c_ulong),
                    ("th32ParentProcessID", ctypes.c_ulong), ("pcPriClassBase", ctypes.c_long),
                    ("dwFlags", ctypes.c_ulong), ("szExeFile", ctypes.c_wchar * 260)]
    p = PE(); p.dwSize = ctypes.sizeof(PE)
    pid = None
    if k32.Process32FirstW(snap, ctypes.byref(p)):
        while True:
            if p.szExeFile.lower() == "reliccoh3.exe":
                pid = p.th32ProcessID
            if not k32.Process32NextW(snap, ctypes.byref(p)):
                break
    k32.CloseHandle(snap)
    return pid

def module_base(pid):
    snap = k32.CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pid)
    m = MODULEENTRY32W(); m.dwSize = ctypes.sizeof(MODULEENTRY32W)
    base = None
    if k32.Module32FirstW(snap, ctypes.byref(m)):
        while True:
            if m.szModule.lower() == "reliccoh3.exe":
                base = m.modBaseAddr
                break
            if not k32.Module32NextW(snap, ctypes.byref(m)):
                break
    k32.CloseHandle(snap)
    return base

class Reader:
    def __init__(self, pid):
        self.h = k32.OpenProcess(0x0410, False, pid)  # PROCESS_QUERY_INFORMATION|PROCESS_VM_READ
        if not self.h:
            raise OSError("OpenProcess failed")
    def u64(self, a): return struct.unpack("<Q", self.read(a, 8))[0]
    def u32(self, a): return struct.unpack("<I", self.read(a, 4))[0]
    def f32(self, a): return struct.unpack("<f", self.read(a, 4))[0]
    def read(self, a, n):
        buf = ctypes.create_string_buffer(n)
        got = ctypes.c_size_t()
        if not k32.ReadProcessMemory(self.h, ctypes.c_void_p(a), buf, n, ctypes.byref(got)) or got.value != n:
            raise OSError(f"RPM failed @ {a:#x}")
        return buf.raw

def main():
    pid = int(sys.argv[1]) if len(sys.argv) > 1 else find_game()
    if not pid:
        print("RelicCoH3.exe not found"); return 2
    print(f"game pid={pid}")
    B = module_base(pid)
    if not B:
        print("module base not found"); return 2
    print(f"base={B:#x}")
    r = Reader(pid)
    # TLS 槽链（与 NativeSquadReader 一致）
    seg_base_ptr = B + 0x6185EB0
    segBase = r.u64(seg_base_ptr)
    print(f"segBase={segBase:#x} (from B+0x6185EB0)")
    sessOff = r.u32(B + 0x5E0BECC)
    print(f"sessOff={sessOff:#x}")
    worldOff = r.u32(segBase + sessOff + 0x100)
    World = segBase + worldOff
    print(f"worldOff={worldOff:#x} World={World:#x}")
    count = r.u32(World + 0xC)
    handleOff = r.u32(World + 0x08)
    validOff = r.u32(World + 0x14)
    print(f"count={count} handleOff={handleOff:#x} validOff={validOff:#x}")
    if count > 4096:
        print("count out of bound, abort"); return 2
    squads = []
    for i in range(count):
        if r.u32(segBase + validOff + 4 * i) == 0:
            continue
        h = r.u32(segBase + handleOff + 8 * i)
        sq = segBase + h
        x, y, z = r.f32(sq + 0x2C), r.f32(sq + 0x30), r.f32(sq + 0x34)
        pl = segBase + r.u32(sq + 0x58)
        pid_id = r.u32(pl + 8)
        squads.append((pid_id, x, y, z, sq))
    print(f"valid squads={len(squads)}")
    from collections import Counter
    print("player ids:", dict(Counter(s[0] for s in squads)))
    for pid_id, x, y, z, sq in squads:
        print(f"  player={pid_id} pos=({x:.1f},{y:.1f},{z:.1f}) squad={sq:#x}")
    return 0

if __name__ == "__main__":
    sys.exit(main())
