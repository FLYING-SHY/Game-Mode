"""Reversible, exact-build probe for two local display flags (no simulation writes)."""
import argparse, ctypes as c, ctypes.wintypes as w, hashlib, json
from pathlib import Path

p = argparse.ArgumentParser()
p.add_argument('--pid', type=int, required=True)
p.add_argument('--base', type=lambda s:int(s, 0), required=True)
p.add_argument('--set', type=int, choices=[0, 1])
p.add_argument('--restore', action='store_true')
p.add_argument('--refresh', action='store_true')
p.add_argument('--entity-gate', choices=['enable', 'restore'])
a = p.parse_args()
exe = Path(r'G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe')
assert hashlib.sha256(exe.read_bytes()).hexdigest() == '21edb5c05c33e89fb5f9d88a3fc6738817abd80e0366f648b18ade999cc975ca', 'Unmatched executable'
k = c.WinDLL('kernel32', use_last_error=True)
k.OpenProcess.argtypes = [w.DWORD,w.BOOL,w.DWORD]; k.OpenProcess.restype=w.HANDLE
k.ReadProcessMemory.argtypes=[w.HANDLE,c.c_void_p,c.c_void_p,c.c_size_t,c.POINTER(c.c_size_t)]; k.ReadProcessMemory.restype=w.BOOL
k.WriteProcessMemory.argtypes=k.ReadProcessMemory.argtypes; k.WriteProcessMemory.restype=w.BOOL
k.CloseHandle.argtypes=[w.HANDLE]
k.VirtualProtectEx.argtypes=[w.HANDLE,c.c_void_p,c.c_size_t,w.DWORD,c.POINTER(w.DWORD)]; k.VirtualProtectEx.restype=w.BOOL
k.FlushInstructionCache.argtypes=[w.HANDLE,c.c_void_p,c.c_size_t]; k.FlushInstructionCache.restype=w.BOOL
h=k.OpenProcess(0x438 if (a.set is not None or a.restore or a.entity_gate) else 0x410,False,a.pid)
if not h: raise c.WinError(c.get_last_error())
def read(addr,size):
    b=c.create_string_buffer(size); n=c.c_size_t()
    if not k.ReadProcessMemory(h,addr,b,size,c.byref(n)) or n.value!=size: raise c.WinError(c.get_last_error())
    return b.raw
def write(addr,data):
    b=c.create_string_buffer(data); n=c.c_size_t()
    if not k.WriteProcessMemory(h,addr,b,len(data),c.byref(n)) or n.value!=len(data): raise c.WinError(c.get_last_error())
    assert read(addr,len(data))==data, 'Write verification failed'
try:
    assert read(a.base,2)==b'MZ', 'Invalid module base'
    if a.entity_gate:
        gates=[(0x1E9701,'740D'),(0x1874E8,'7409'),(0x187F42,'740D'),(0x1882E3,'740D'),(0x189E26,'7409'),(0x187EF0,'7409')]
        for rva,originalhex in gates:
            branch=a.base+rva
            original=bytes.fromhex(originalhex); patched=bytes.fromhex('90 90')
            live=read(branch,2)
            assert live in (original,patched), 'Unmatched entity visibility gate'
            desired=patched if a.entity_gate=='enable' else original
            oldprotect=w.DWORD()
            if not k.VirtualProtectEx(h,branch,2,0x40,c.byref(oldprotect)): raise c.WinError(c.get_last_error())
            try:
                write(branch,desired)
                if not k.FlushInstructionCache(h,branch,2): raise c.WinError(c.get_last_error())
            finally:
                unused=w.DWORD(); k.VirtualProtectEx(h,branch,2,oldprotect.value,c.byref(unused))
            print('Visibility gate',hex(rva),':',live.hex(),'->',read(branch,2).hex())
    addr=a.base+0x6A888A0
    flags=read(addr,2)
    assert all(x in (0,1) for x in flags), 'Unexpected display flags'
    manager=int.from_bytes(read(a.base+0x6B02CC0,8),'little')
    assert 0x10000<=manager<0x800000000000, 'Invalid display manager'
    refresh=read(manager+0x41,1)
    assert refresh[0] in (0,1), 'Unexpected refresh flag'
    world=int.from_bytes(read(a.base+0x6185F18,8),'little')
    assert 0x10000<=world<0x800000000000, 'Invalid presentation world'
    display=int.from_bytes(read(world+0x50,8),'little')
    assert 0x10000<=display<0x800000000000, 'Invalid FOW display object'
    displayflag=read(display+0x5E9,1)
    assert displayflag[0] in (0,1), 'Unexpected display task flag'
    print(json.dumps({'pid':a.pid,'base':hex(a.base),'flags_address':hex(addr),'ui_reveal_all':flags[0],'ui_reveal_entities':flags[1],'manager':hex(manager),'manager_flag41':refresh[0],'display':hex(display),'display_flag5e9':displayflag[0]}))
    backup=Path(__file__).with_name(f'ui_probe_{a.pid}.json')
    if a.set is not None:
        old=json.loads(backup.read_text(encoding='utf-8')) if backup.exists() else {'pid':a.pid,'base':a.base,'manager':manager,'flags':flags.hex(),'refresh':refresh.hex()}
        assert (old['pid'],old['base'],old['manager'])==(a.pid,a.base,manager), 'Backup context changed'
        old.setdefault('display',display); old.setdefault('displayflag',displayflag.hex())
        backup.write_text(json.dumps(old),encoding='utf-8')
        write(addr,bytes([a.set,a.set]))
        if a.refresh:
            write(manager+0x41,bytes([a.set]))
            write(display+0x5E9,bytes([a.set]))
        print('Display flag readback:',read(addr,2).hex(),'manager_flag41:',read(manager+0x41,1).hex(),'display_flag5e9:',read(display+0x5E9,1).hex())
    elif a.restore:
        old=json.loads(backup.read_text(encoding='utf-8'))
        assert (old['pid'],old['base'],old['manager'])==(a.pid,a.base,manager), 'Backup context changed'
        write(addr,bytes.fromhex(old['flags']))
        if a.refresh:
            write(manager+0x41,bytes.fromhex(old['refresh']))
            if 'display' in old:
                assert old['display']==display, 'Display object changed'
                write(display+0x5E9,bytes.fromhex(old['displayflag']))
        print('Restored display flags:',read(addr,2).hex(),'display_flag5e9:',read(display+0x5E9,1).hex())
finally: k.CloseHandle(h)
