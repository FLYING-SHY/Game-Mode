"""Passive capability probe. Never activates windows or sends input."""
import ctypes
import importlib.util
import json
from pathlib import Path
import win32gui
import win32ui
import win32process
from PIL import Image

root = Path(__file__).resolve().parent
hwnd = 4982018
thread_id, pid = win32process.GetWindowThreadProcessId(hwnd)
if pid != 11192:
    raise RuntimeError(f'Window PID changed: {pid}')
rect = win32gui.GetWindowRect(hwnd)
left, top, right, bottom = rect
width, height = right-left, bottom-top
result = {'pid': pid, 'hwnd': hwnd, 'title': win32gui.GetWindowText(hwnd),
          'rect': rect, 'visible': bool(win32gui.IsWindowVisible(hwnd)),
          'minimized': bool(win32gui.IsIconic(hwnd)),
          'foregroundHwnd': win32gui.GetForegroundWindow(),
          'libraries': {m: bool(importlib.util.find_spec(m)) for m in
                        ['PIL', 'pyautogui', 'pywinauto', 'mss', 'win32gui', 'pydirectinput']},
          'inputExportsPresentNotInvoked': {name: bool(getattr(ctypes.windll.user32, name, None)) for name in
                                          ['SendInput', 'SetForegroundWindow', 'GetClientRect']}}
window_dc = win32gui.GetWindowDC(hwnd)
source = win32ui.CreateDCFromHandle(window_dc)
memory = source.CreateCompatibleDC()
bitmap = win32ui.CreateBitmap()
bitmap.CreateCompatibleBitmap(source, width, height)
old = memory.SelectObject(bitmap)
try:
    captured = ctypes.windll.user32.PrintWindow(hwnd, memory.GetSafeHdc(), 2)
    result['printWindowReturn'] = captured
    if captured:
        image = Image.frombuffer('RGB', (width, height), bitmap.GetBitmapBits(True), 'raw', 'BGRX', 0, 1)
        image.save(root / 'capabilities-game-window.png')
        result['imageExtrema'] = image.getextrema()
        result['screenshot'] = 'capabilities-game-window.png'
finally:
    memory.SelectObject(old)
    win32gui.DeleteObject(bitmap.GetHandle())
    memory.DeleteDC()
    source.DeleteDC()
    win32gui.ReleaseDC(hwnd, window_dc)
(root / 'capabilities-gui-probe.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(result, ensure_ascii=False, indent=2))
