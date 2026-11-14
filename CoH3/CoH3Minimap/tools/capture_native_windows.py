import ctypes as c
from ctypes import wintypes as w
from pathlib import Path
from datetime import datetime
import hashlib
import json
import sys
from PIL import Image

u = c.WinDLL('user32', use_last_error=True)
g = c.WinDLL('gdi32', use_last_error=True)
try:
    u.SetProcessDpiAwarenessContext(c.c_void_p(-4))
except Exception:
    pass
H = c.c_void_p
u.GetWindowDC.argtypes = [H]; u.GetWindowDC.restype = H
u.ReleaseDC.argtypes = [H, H]
u.PrintWindow.argtypes = [H, H, w.UINT]; u.PrintWindow.restype = w.BOOL
u.GetWindowRect.argtypes = [H, c.POINTER(w.RECT)]
u.GetWindowThreadProcessId.argtypes = [H, c.POINTER(w.DWORD)]
u.IsWindowVisible.argtypes = [H]; u.IsWindowVisible.restype = w.BOOL
g.CreateCompatibleDC.argtypes = [H]; g.CreateCompatibleDC.restype = H
g.CreateCompatibleBitmap.argtypes = [H, c.c_int, c.c_int]; g.CreateCompatibleBitmap.restype = H
g.SelectObject.argtypes = [H, H]; g.SelectObject.restype = H
g.DeleteObject.argtypes = [H]; g.DeleteDC.argtypes = [H]
class BIH(c.Structure):
    _fields_ = [('size',w.DWORD),('width',w.LONG),('height',w.LONG),('planes',w.WORD),('bits',w.WORD),('compression',w.DWORD),('imageSize',w.DWORD),('x',w.LONG),('y',w.LONG),('used',w.DWORD),('important',w.DWORD)]
g.GetDIBits.argtypes = [H,H,w.UINT,w.UINT,H,c.POINTER(BIH),w.UINT]
g.GetDIBits.restype = c.c_int
CALLBACK = c.WINFUNCTYPE(w.BOOL, H, w.LPARAM)
u.EnumWindows.argtypes = [CALLBACK, w.LPARAM]

pids = set(map(int, sys.argv[1:]))
windows = []
@CALLBACK
def visit(hwnd, _):
    pid = w.DWORD()
    u.GetWindowThreadProcessId(hwnd, c.byref(pid))
    if pid.value in pids and u.IsWindowVisible(hwnd):
        rect = w.RECT()
        u.GetWindowRect(hwnd, c.byref(rect))
        if rect.right - rect.left > 200 and rect.bottom - rect.top > 200:
            windows.append((int(pid.value), hwnd, rect))
    return True
u.EnumWindows(visit, 0)
out = Path(__file__).resolve().parents[1] / 'artifacts'
stamp = datetime.now().strftime('%Y%m%d-%H%M%S-%f')
results = []
for pid, hwnd, rect in windows:
    width, height = rect.right - rect.left, rect.bottom - rect.top
    dc = u.GetWindowDC(hwnd)
    mem = g.CreateCompatibleDC(dc)
    bitmap = g.CreateCompatibleBitmap(dc, width, height)
    old = g.SelectObject(mem, bitmap)
    ok = u.PrintWindow(hwnd, mem, 2)
    g.SelectObject(mem, old)
    try:
        if not ok:
            raise OSError('PrintWindow failed for requested process window')
        header = BIH(c.sizeof(BIH), width, -height, 1, 32, 0, width * height * 4, 0, 0, 0, 0)
        raw = c.create_string_buffer(width * height * 4)
        lines = g.GetDIBits(dc, bitmap, 0, height, raw, c.byref(header), 0)
        if lines != height:
            raise OSError('GetDIBits did not return the full window')
        image = Image.frombytes('RGB', (width, height), raw.raw, 'raw', 'BGRX')
        path = out / f'native-window-{pid}-{stamp}.png'
        image.save(path)
        results.append({'pid':pid,'path':str(path),'size':[width,height], 'sha256':hashlib.sha256(path.read_bytes()).hexdigest(), 'unique_colors':len(image.getcolors(width * height) or [])})
    finally:
        g.DeleteObject(bitmap); g.DeleteDC(mem); u.ReleaseDC(hwnd, dc)
print(json.dumps(results, ensure_ascii=False, indent=2))
