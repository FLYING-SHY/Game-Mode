"""Read-only hypothesis test for old/new CoH3 roots. Never registers a baseline."""
import argparse, ctypes as c, struct, json, datetime
from ctypes import wintypes as w
p=argparse.ArgumentParser(); p.add_argument('--pid',type=int,required=True); p.add_argument('--out',required=True); a=p.parse_args()
k=c.WinDLL('kernel32',use_last_error=True); ps=c.WinDLL('psapi',use_last_error=True)
k.OpenProcess.argtypes=[w.DWORD,w.BOOL,w.DWORD]; k.OpenProcess.restype=w.HANDLE
k.ReadProcessMemory.argtypes=[w.HANDLE,c.c_void_p,c.c_void_p,c.c_size_t,c.POINTER(c.c_size_t)]; k.ReadProcessMemory.restype=w.BOOL
k.CloseHandle.argtypes=[w.HANDLE]
ps.EnumProcessModulesEx.argtypes=[w.HANDLE,c.POINTER(w.HMODULE),w.DWORD,c.POINTER(w.DWORD),w.DWORD]; ps.EnumProcessModulesEx.restype=w.BOOL
h=k.OpenProcess(0x410,False,a.pid)
if not h: raise c.WinError(c.get_last_error())
def read(addr,n):
    b=c.create_string_buffer(n); done=c.c_size_t()
    if not k.ReadProcessMemory(h,addr,b,n,c.byref(done)) or done.value!=n: raise OSError(f'ReadProcessMemory failed at {addr:#x}, size {n}, error {c.get_last_error()}')
    return b.raw
def q(addr): return struct.unpack('<Q',read(addr,8))[0]
def u(addr): return struct.unpack('<I',read(addr,4))[0]
def s(addr):
    length,capacity=struct.unpack('<QQ',read(addr+16,16))
    if length>1024 or capacity<length: raise ValueError(f'invalid string length={length} capacity={capacity}')
    return read(addr if capacity<16 else q(addr),length).decode('utf-8',errors='strict') if length else ''
def test(root,arena_rva,world_rva):
    result={'resourceRootRva':hex(root),'arenaRva':hex(arena_rva),'worldRva':hex(world_rva)}
    try:
        begin,end=q(base+root+872),q(base+root+880)
        result['resourceVector']=[hex(begin),hex(end)]
        if end<begin or end-begin>6144 or (end-begin)%48: raise ValueError('invalid resource vector dimensions')
        result['resources']=[{'name':s(ptr+8),'index':u(ptr+40)} for ptr in range(begin,end,48)]
        arena=q(base+arena_rva); offset=u(base+world_rva); world=arena+offset
        result.update(arena=hex(arena),worldOffset=hex(offset),world=hex(world))
        players=arena+u(world+252); count=u(players+16); vector=arena+u(players+12)
        result['playerCount']=count
        if not 0<count<=16: raise ValueError('invalid player count')
        result['players']=[]
        for i in range(count):
            player=arena+u(vector+i*4)
            result['players'].append({'address':hex(player),'identity':hex(q(player+8)),'slot':u(player+12),'rawResources':list(struct.unpack('<10f',read(player+1704,40))),'populationParts':[struct.unpack('<f',read(player+1256,4))[0],struct.unpack('<f',read(player+1304,4))[0]]})
        result['status']='structure_candidate_read_success_not_full_validation'
    except Exception as e: result['error']=str(e); result['status']='failed'
    return result
try:
    mods=(w.HMODULE*2048)(); needed=w.DWORD()
    if not ps.EnumProcessModulesEx(h,mods,c.sizeof(mods),c.byref(needed),3): raise c.WinError(c.get_last_error())
    base=mods[0]
    result={'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'imageBase':hex(base),'access':'PROCESS_VM_READ | PROCESS_QUERY_INFORMATION','baselineStatus':'unverified','old':test(112918560,102260400,98614988),'candidate':test(112926416,102268480,98623180)}
    with open(a.out,'w',encoding='utf-8') as f: json.dump(result,f,ensure_ascii=False,indent=2,allow_nan=False)
    print(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False))
finally: k.CloseHandle(h)
