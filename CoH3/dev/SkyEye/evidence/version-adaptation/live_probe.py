"""Bounded read-only CoH3 layout hypothesis probe; never registers a build."""
import argparse, ctypes as c, datetime, hashlib, json, pathlib, struct
from ctypes import wintypes as w

class ProcessReader:
    def __init__(self, pid):
        self.k = c.WinDLL('kernel32', use_last_error=True)
        self.k.OpenProcess.argtypes = [w.DWORD, w.BOOL, w.DWORD]
        self.k.OpenProcess.restype = w.HANDLE
        self.k.ReadProcessMemory.argtypes = [w.HANDLE, c.c_void_p, c.c_void_p, c.c_size_t, c.POINTER(c.c_size_t)]
        self.k.ReadProcessMemory.restype = w.BOOL
        self.k.CloseHandle.argtypes = [w.HANDLE]
        self.handle = self.k.OpenProcess(0x410, False, pid)
        if not self.handle: raise c.WinError(c.get_last_error())
        ps = c.WinDLL('psapi', use_last_error=True)
        ps.EnumProcessModulesEx.argtypes = [w.HANDLE, c.POINTER(w.HMODULE), w.DWORD, c.POINTER(w.DWORD), w.DWORD]
        ps.EnumProcessModulesEx.restype = w.BOOL
        mods = (w.HMODULE * 2048)(); needed = w.DWORD()
        if not ps.EnumProcessModulesEx(self.handle, mods, c.sizeof(mods), c.byref(needed), 3):
            self.close(); raise c.WinError(c.get_last_error())
        self.base = mods[0]
        self.bytes_read = 0
    def close(self):
        if self.handle: self.k.CloseHandle(self.handle); self.handle = None
    def read(self, address, length):
        if not 0x10000 <= address < 0x7fffffffffff or not 0 <= length <= 65536:
            raise ValueError('Invalid or unbounded read')
        if self.bytes_read + length > 8 * 1024 * 1024: raise ValueError('8 MiB probe budget exceeded')
        b = c.create_string_buffer(length); done = c.c_size_t()
        if not self.k.ReadProcessMemory(self.handle, address, b, length, c.byref(done)) or done.value != length:
            raise OSError(f'ReadProcessMemory({address:#x},{length}) error={c.get_last_error()} actual={done.value}')
        self.bytes_read += length
        return b.raw
    def q(self, a): return struct.unpack('<Q', self.read(a, 8))[0]
    def u(self, a): return struct.unpack('<I', self.read(a, 4))[0]
    def f(self, a): return struct.unpack('<f', self.read(a, 4))[0]
    def string(self, a):
        size, cap = struct.unpack('<QQ', self.read(a + 16, 16))
        if not 0 <= size <= 1024 or cap < size: raise ValueError('Invalid string dimensions')
        return self.read(a if cap < 16 else self.q(a), size).decode('utf8') if size else ''

def probe(r, roots):
    b = r.base
    result = {'roots': {k: hex(v) for k, v in roots.items()}, 'status': 'candidate_only'}
    def attempt(name, fn):
        try: result[name] = fn()
        except Exception as e: result[name] = {'error': str(e)}
    def names(offset, limit):
        begin, end = r.q(b + roots['resource'] + offset + 32), r.q(b + roots['resource'] + offset + 40)
        if not 0 <= end - begin <= limit or (end-begin) % 48: raise ValueError('Invalid name vector')
        return [{'name': r.string(p + 8), 'index': r.u(p+40)} for p in range(begin, end, 48)]
    attempt('resources', lambda: names(840, 6144))
    attempt('timers', lambda: names(560, 12288))
    arena = r.q(b+roots['arena']); world_offset = r.u(b+roots['world']); world = arena+world_offset
    result.update(arena=hex(arena), worldOffset=hex(world_offset))
    def players():
        pvec = arena+r.u(world+252); count = r.u(pvec+16); start = arena+r.u(pvec+12)
        if not 0 < count <= 16: raise ValueError('Invalid player count')
        rows = []
        for i in range(count):
            p = arena+r.u(start+i*4)
            n = p+1640; nlen = r.u(n+12)
            name = r.read(arena+r.u(n+8), nlen*2).decode('utf-16-le').rstrip('\0') if 0 < nlen <= 256 and r.u(n+4)==0xffffffff else None
            row = {'address':hex(p),'identity':hex(r.q(p+8)), 'slot':r.u(p+12), 'name':name,
                   'resources':list(struct.unpack('<10f',r.read(p+1704,40))), 'population':[r.f(p+1256),r.f(p+1304)]}
            registry = arena+r.u(p+1792); length = r.u(registry+40); data = arena+r.u(registry+36)
            if length > 4096: raise ValueError('Invalid entity count')
            row['registryCount'] = length; entities=[]
            for j in range(length):
                off = r.u(data+j*4)
                if off == 0xffffffff: continue
                ent = arena+off
                if arena+r.u(ent+88) != p: continue
                blueprint = r.q(ent+56)
                entity = {'address':hex(ent),'vtableRva':hex(r.q(ent)-b), 'blueprint':r.string(blueprint+40),
                          'generation':r.u(ent+96),'componentKind':struct.unpack('<H',r.read(ent+76,2))[0]}
                entities.append(entity)
            row['ownedEntities'] = entities; rows.append(row)
        return rows
    attempt('players', players)
    result['stableRootsAfterRead'] = r.q(b+roots['arena'])==arena and r.u(b+roots['world'])==world_offset
    return result

if __name__ == '__main__':
    p=argparse.ArgumentParser(); p.add_argument('--pid',type=int,required=True); p.add_argument('--out',required=True)
    p.add_argument('--resource',type=lambda x:int(x,0),default=112926416)
    p.add_argument('--arena',type=lambda x:int(x,0),default=102268480)
    p.add_argument('--world',type=lambda x:int(x,0),default=98623180)
    a=p.parse_args(); r=ProcessReader(a.pid)
    try:
        report={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'moduleBase':hex(r.base),
                'access':'PROCESS_QUERY_INFORMATION|PROCESS_VM_READ','verification':'candidate_not_baseline',
                'snapshot':probe(r,{'resource':a.resource,'arena':a.arena,'world':a.world})}
        report['bytesRead']=r.bytes_read
        pathlib.Path(a.out).write_text(json.dumps(report,ensure_ascii=False,indent=2,allow_nan=False),encoding='utf8')
        print(json.dumps({'output':a.out,'pid':a.pid,'bytesRead':r.bytes_read,'stableRootsAfterRead':report['snapshot']['stableRootsAfterRead'],'players':[{k:v for k,v in row.items() if k!='ownedEntities'} | {'ownedEntityCount':len(row['ownedEntities'])} for row in report['snapshot']['players']] if isinstance(report['snapshot']['players'],list) else report['snapshot']['players']},ensure_ascii=False,allow_nan=False))
    finally: r.close()
