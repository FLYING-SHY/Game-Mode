"""Bounded read of two statically proven territory CPU buffers; never invokes game code."""
import ctypes as c
from ctypes import wintypes as w
from pathlib import Path
from datetime import datetime
import hashlib
import json
import struct
import sys
from PIL import Image

pid = int(sys.argv[1]); base = int(sys.argv[2], 0)
k = c.WinDLL('kernel32', use_last_error=True)
k.OpenProcess.argtypes = [w.DWORD, w.BOOL, w.DWORD]; k.OpenProcess.restype = w.HANDLE
k.ReadProcessMemory.argtypes = [w.HANDLE,c.c_void_p,c.c_void_p,c.c_size_t,c.POINTER(c.c_size_t)]; k.ReadProcessMemory.restype = w.BOOL
k.CloseHandle.argtypes = [w.HANDLE]
h = k.OpenProcess(0x1010, False, pid)
if not h: raise OSError(c.get_last_error(), 'OpenProcess(read-only)')
def read(address, size):
    if not (0x10000 <= address <= 0x7fffffffffff) or not (0 < size <= 16 * 1024 * 1024):
        raise ValueError('Address or size outside bounded probe limits')
    buf = c.create_string_buffer(size); got = c.c_size_t()
    if not k.ReadProcessMemory(h, c.c_void_p(address), buf, size, c.byref(got)) or got.value != size:
        raise OSError(c.get_last_error(), 'ReadProcessMemory')
    return buf.raw
u64 = lambda data, off: struct.unpack_from('<Q', data, off)[0]
u32 = lambda data, off: struct.unpack_from('<I', data, off)[0]
out = Path(__file__).resolve().parents[1] / 'artifacts'
stamp = datetime.now().strftime('%Y%m%d-%H%M%S-%f')
try:
    if read(base, 2) != b'MZ': raise ValueError('Module base is not a PE image')
    root = u64(read(base + 0x6B02D78, 8), 0)
    before = read(root, 0x108)
    result = {'pid':pid,'base':hex(base),'root':hex(root),'headerHex':before.hex(),
              'dirty':before[0],'nextWriteSlot':u64(before,0x48),'enableReady':read(base+0x6B02D80,2).hex(),
              'gameWritten':False,'gameFunctionsInvoked':False,'formatVerified':False,'slots':[]}
    for index in range(2):
        off = 0x98 + index * 0x38
        width,height,total = (u32(before,off+x) for x in (0x14,0x18,0x1C))
        ptr,count = u64(before,off+0x20),u64(before,off+0x28)
        row = {'slot':index,'width':width,'height':height,'totalBytes':total,'pointer':hex(ptr),'countField':count}
        if width > 0 and height > 0 and width * height * 4 == total and total <= 16*1024*1024:
            pixels = read(ptr, total)
            path = out / f'cpu-territory-slot{index}-{stamp}.bin'
            with path.open('xb') as stream: stream.write(pixels)
            row.update(rawPath=str(path),sha256=hashlib.sha256(pixels).hexdigest(),
                       distinctPixels=len(set(struct.iter_unpack('<I',pixels))))
            for order in ('RGBA','BGRA'):
                image = Image.frombytes('RGBA',(width,height),pixels,'raw',order)
                image.save(out / f'cpu-territory-slot{index}-{order}-{stamp}.png')
        result['slots'].append(row)
    interactivity = u64(read(base + 0x6B02D88, 8), 0)
    ih = read(interactivity, 0x38)
    iw, iht, itotal = (u32(ih, off) for off in (0x14, 0x18, 0x1C))
    iptr = u64(ih, 0x20)
    ir = {'root':hex(interactivity),'width':iw,'height':iht,'totalBytes':itotal,'pointer':hex(iptr),'countField':u64(ih,0x28)}
    if iw > 0 and iht > 0 and iw * iht * 4 == itotal and itotal <= 16*1024*1024:
        pixels = read(iptr,itotal)
        path = out / f'cpu-interactivity-{stamp}.bin'
        with path.open('xb') as stream: stream.write(pixels)
        ir.update(rawPath=str(path),sha256=hashlib.sha256(pixels).hexdigest(),
                  distinctPixels=len(set(struct.iter_unpack('<I',pixels))),
                  alphaValues=sorted(set(pixels[3::4])))
        for order in ('RGBA','BGRA'):
            Image.frombytes('RGBA',(iw,iht),pixels,'raw',order).save(out / f'cpu-interactivity-{order}-{stamp}.png')
        ir['imageHeaderUnchanged'] = ih == read(interactivity,0x38)
        ir['rootUnchanged'] = interactivity == u64(read(base+0x6B02D88,8),0)
    result['interactivity'] = ir
    after = read(root,0x108)
    result['headerUnchanged'] = before == after
    result['rootUnchanged'] = root == u64(read(base+0x6B02D78,8),0)
    output = out / f'cpu-territory-probe-{stamp}.json'
    output.write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(result,ensure_ascii=False,indent=2))
finally:
    k.CloseHandle(h)
