"""PrintWindow 截图：python shot_window.py <pid> <out.png>"""
import ctypes, sys
from ctypes import wintypes as wt
from PIL import Image

u = ctypes.windll.user32
g32 = ctypes.windll.gdi32
u.SetProcessDPIAware()


class RECT(ctypes.Structure):
    _fields_ = [("L", ct := ctypes.c_long), ("T", ct), ("Rt", ct), ("B", ct)]


pid = int(sys.argv[1])
out = sys.argv[2]
found = {}


@ctypes.WINFUNCTYPE(wt.BOOL, wt.HWND, wt.LPARAM)
def enum_cb(hwnd, _):
    p = wt.DWORD()
    u.GetWindowThreadProcessId(hwnd, ctypes.byref(p))
    if p.value == pid and u.IsWindowVisible(hwnd) and u.GetAncestor(hwnd, 2) == hwnd:  # GA_ROOT=2
        found[hwnd] = True
    return True


u.EnumWindows(enum_cb, 0)
h = next(iter(found)) if found else 0
print("hwnd", h)
if not h:
    sys.exit(1)
r = RECT()
u.GetWindowRect(h, ctypes.byref(r))
w, ht = r.Rt - r.L, r.B - r.T
print("rect", r.L, r.T, w, ht)
hdc = u.GetWindowDC(h)
mdc = g32.CreateCompatibleDC(hdc)
hb = g32.CreateCompatibleBitmap(hdc, w, ht)
g32.SelectObject(mdc, hb)
u.PrintWindow(h, mdc, 2)
bits = (ctypes.c_char * (w * ht * 4))()
g32.GetBitmapBits(hb, w * ht * 4, bits)
img = Image.frombuffer("RGBA", (w, ht), bytes(bits), "raw", "RGBA", 0, 1)
img.save(out)
print("saved", out, w, ht)
