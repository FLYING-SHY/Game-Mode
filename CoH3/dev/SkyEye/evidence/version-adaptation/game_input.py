"""Foreground-checked game input. Fails before keys if focus acquisition fails."""
import argparse,sys,pathlib,runpy,time,ctypes as c
from ctypes import wintypes as w
p=argparse.ArgumentParser();p.add_argument('--pid',type=int,required=True);p.add_argument('--key',type=int);clicks=p.add_mutually_exclusive_group();clicks.add_argument('--right-click',nargs=2,type=int);clicks.add_argument('--left-click',nargs=2,type=int);clicks.add_argument('--left-drag',nargs=4,type=int);p.add_argument('--out',required=True);a=p.parse_args()
sys.argv=['window_probe.py','--pid',str(a.pid)]
root=pathlib.Path(__file__).resolve().parent
d=runpy.run_path(str(root.parent/'adaptation-52426-20261121-004302'/'window_probe.py'))
u=d['u'];u.GetForegroundWindow.restype=w.HWND
h,t,r=max(d['windows'],key=lambda x:(x[2][2]-x[2][0])*(x[2][3]-x[2][1]))
u.SetForegroundWindow(h)
if u.GetForegroundWindow()!=h:
    # Join the current foreground input queue only while requesting normal activation.
    # An obscured title bar may belong to another application, so never click it blindly.
    k=c.WinDLL('kernel32',use_last_error=True)
    k.GetCurrentThreadId.restype=w.DWORD
    u.AttachThreadInput.argtypes=[w.DWORD,w.DWORD,w.BOOL];u.AttachThreadInput.restype=w.BOOL
    current=k.GetCurrentThreadId(); foreground=u.GetForegroundWindow()
    owner=w.DWORD(); thread=u.GetWindowThreadProcessId(foreground,c.byref(owner))
    attached=bool(thread and thread!=current and u.AttachThreadInput(current,thread,True))
    try:
        u.BringWindowToTop.argtypes=[w.HWND];u.BringWindowToTop(h)
        u.SetForegroundWindow(h)
    finally:
        if attached:u.AttachThreadInput(current,thread,False)
    time.sleep(.15)
if u.GetForegroundWindow()!=h:raise RuntimeError('Selected game did not acquire foreground; no key sent')
if a.key is not None:
    u.keybd_event(a.key,0,0,0);u.keybd_event(a.key,0,2,0);time.sleep(.45)
if a.right_click or a.left_click:
    point=w.POINT(*(a.right_click or a.left_click));u.ClientToScreen(h,c.byref(point))
    vx,vy,vw,vh=[u.GetSystemMetrics(i) for i in (76,77,78,79)]
    if not(vx<=point.x<vx+vw and vy<=point.y<vy+vh):raise RuntimeError('Click lies outside active virtual display; no mouse input sent')
    u.SetCursorPos(point.x,point.y)
    # Let the game observe the pointer move before resolving the click's world/UI target.
    time.sleep(.12)
    if u.GetForegroundWindow()!=h:raise RuntimeError('Game lost foreground before click; no mouse button sent')
    down,up=(8,16) if a.right_click else (2,4)
    u.mouse_event(down,0,0,0,0);time.sleep(.04);u.mouse_event(up,0,0,0,0);time.sleep(.25)
if a.left_drag:
    start=w.POINT(*a.left_drag[:2]);end=w.POINT(*a.left_drag[2:])
    u.ClientToScreen(h,c.byref(start));u.ClientToScreen(h,c.byref(end))
    vx,vy,vw,vh=[u.GetSystemMetrics(i) for i in (76,77,78,79)]
    if not all(vx<=p.x<vx+vw and vy<=p.y<vy+vh for p in (start,end)):raise RuntimeError('Drag lies outside active virtual display; no mouse input sent')
    u.SetCursorPos(start.x,start.y);time.sleep(.12)
    if u.GetForegroundWindow()!=h:raise RuntimeError('Game lost foreground before drag; no mouse button sent')
    u.mouse_event(2,0,0,0,0)
    try:
        time.sleep(.3)
        for step in range(1,21):
            if u.GetForegroundWindow()!=h:raise RuntimeError('Game lost foreground during drag')
            u.SetCursorPos(start.x+(end.x-start.x)*step//20,start.y+(end.y-start.y)*step//20);time.sleep(.03)
    finally:u.mouse_event(4,0,0,0,0)
    time.sleep(.25)
d['capture_window'](h,r[2]-r[0],r[3]-r[1]).save(a.out)
print('FOREGROUND_CHECKED',h,'KEY',a.key,'OUT',a.out)
