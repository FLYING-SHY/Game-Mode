"""External game-window capture/control only; no process writes or injection."""
import argparse, ctypes as c, json, pathlib, time
from ctypes import wintypes as w
from PIL import Image

def capture_window(hwnd, width, height):
    g=c.WinDLL('gdi32',use_last_error=True)
    u.GetWindowDC.argtypes=[w.HWND]; u.GetWindowDC.restype=w.HDC
    u.ReleaseDC.argtypes=[w.HWND,w.HDC]
    u.PrintWindow.argtypes=[w.HWND,w.HDC,w.UINT]; u.PrintWindow.restype=w.BOOL
    g.CreateCompatibleDC.argtypes=[w.HDC]; g.CreateCompatibleDC.restype=w.HDC
    g.CreateCompatibleBitmap.argtypes=[w.HDC,c.c_int,c.c_int]; g.CreateCompatibleBitmap.restype=w.HBITMAP
    g.SelectObject.argtypes=[w.HDC,w.HANDLE]; g.SelectObject.restype=w.HANDLE
    g.DeleteObject.argtypes=[w.HANDLE]; g.DeleteDC.argtypes=[w.HDC]
    g.GetDIBits.argtypes=[w.HDC,w.HBITMAP,w.UINT,w.UINT,c.c_void_p,c.c_void_p,w.UINT]
    class Header(c.Structure):
        _fields_=[('size',w.DWORD),('width',w.LONG),('height',w.LONG),('planes',w.WORD),('bits',w.WORD),('compression',w.DWORD),('sizeImage',w.DWORD),('x',w.LONG),('y',w.LONG),('used',w.DWORD),('important',w.DWORD)]
    source=u.GetWindowDC(hwnd); dc=g.CreateCompatibleDC(source); bitmap=g.CreateCompatibleBitmap(source,width,height); old=g.SelectObject(dc,bitmap)
    try:
        if not u.PrintWindow(hwnd,dc,2): raise OSError('PrintWindow failed')
        g.SelectObject(dc,old)
        header=Header(); header.size=c.sizeof(header); header.width=width; header.height=-height; header.planes=1; header.bits=32
        pixels=c.create_string_buffer(width*height*4)
        if g.GetDIBits(dc,bitmap,0,height,pixels,c.byref(header),0)!=height: raise OSError('GetDIBits failed')
        return Image.frombuffer('RGB',(width,height),pixels.raw,'raw','BGRX',0,1)
    finally:
        g.SelectObject(dc,old); g.DeleteObject(bitmap); g.DeleteDC(dc); u.ReleaseDC(hwnd,source)
u=c.WinDLL('user32',use_last_error=True)
u.SetProcessDPIAware()
u.GetWindowThreadProcessId.argtypes=[w.HWND,c.POINTER(w.DWORD)]
u.GetWindowRect.argtypes=[w.HWND,c.POINTER(w.RECT)]
u.IsWindowVisible.argtypes=[w.HWND]
u.GetWindowTextW.argtypes=[w.HWND,w.LPWSTR,c.c_int]
u.SetForegroundWindow.argtypes=[w.HWND]
u.PostMessageW.argtypes=[w.HWND,w.UINT,w.WPARAM,w.LPARAM]
parser=argparse.ArgumentParser(); parser.add_argument('--pid',type=int,required=True); parser.add_argument('--out'); parser.add_argument('--click',nargs=2,type=int); parser.add_argument('--key',type=int); a=parser.parse_args()
windows=[]
@c.WINFUNCTYPE(w.BOOL,w.HWND,w.LPARAM)
def visit(hwnd,_):
    pid=w.DWORD(); u.GetWindowThreadProcessId(hwnd,c.byref(pid))
    if pid.value==a.pid and u.IsWindowVisible(hwnd):
        text=c.create_unicode_buffer(512); u.GetWindowTextW(hwnd,text,512)
        rect=w.RECT(); u.GetWindowRect(hwnd,c.byref(rect))
        windows.append((hwnd,text.value,(rect.left,rect.top,rect.right,rect.bottom)))
    return True
u.EnumWindows(visit,0)
print(json.dumps([{'hwnd':h,'title':t,'rect':r} for h,t,r in windows],ensure_ascii=False))
if not windows: raise SystemExit('No visible window for selected game process')
hwnd,title,rect=max(windows,key=lambda item:(item[2][2]-item[2][0])*(item[2][3]-item[2][1]))
if a.click or a.key is not None:
    u.SetForegroundWindow(hwnd); time.sleep(.2)
if a.click:
    x,y=a.click; point=w.POINT(x,y); u.ClientToScreen(hwnd,c.byref(point)); u.SetCursorPos(point.x,point.y)
    u.mouse_event(2,0,0,0,0); u.mouse_event(4,0,0,0,0); time.sleep(.4)
if a.key is not None:
    u.keybd_event(a.key,0,0,0); u.keybd_event(a.key,0,2,0); time.sleep(.4)
if a.out:
    capture_window(hwnd,rect[2]-rect[0],rect[3]-rect[1]).save(a.out); print(str(pathlib.Path(a.out).resolve()))
