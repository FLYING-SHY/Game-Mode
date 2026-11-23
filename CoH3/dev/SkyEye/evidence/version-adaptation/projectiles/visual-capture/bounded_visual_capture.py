"""Bounded, explicit-PID window evidence. Importing this module performs no Win32 calls."""
from __future__ import annotations

import argparse
import ctypes as c
from ctypes import wintypes as w
from dataclasses import dataclass
from datetime import datetime, timezone
import hashlib
import io
import json
import os
from pathlib import Path
import sys
import threading
import time

MAX_GROUPS = 120
MAX_OUTPUT_BYTES = 80 * 1024 * 1024
REPORT_RESERVE = 64 * 1024
POLL_SECONDS = 0.05
WS_CAPTION = 0x00C00000


def json_bytes(value):
    return (json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False) + "\n").encode("utf-8")


class StopCapture(Exception):
    def __init__(self, reason, detail=None):
        super().__init__(reason)
        self.reason = reason
        self.detail = detail or {}


class RealClock:
    monotonic = staticmethod(time.monotonic)
    sleep = staticmethod(time.sleep)

    @staticmethod
    def utc():
        return datetime.now(timezone.utc).isoformat()


@dataclass(frozen=True)
class Config:
    game_pid: int
    overlay_pid: int
    out: Path
    seconds: float = 20
    cadence_ms: int = 500
    game_format: str = "jpeg"
    desktop_game_region: bool = False

    def validate(self):
        if self.game_pid <= 0 or self.overlay_pid <= 0 or self.game_pid == self.overlay_pid:
            raise ValueError("game-pid and overlay-pid must be distinct positive integers")
        if not 1 <= self.seconds <= 60:
            raise ValueError("seconds must be in [1, 60]")
        if self.cadence_ms < 200:
            raise ValueError("cadence-ms must be >= 200")
        if self.game_format not in ("png", "jpeg"):
            raise ValueError("game-format must be png or jpeg")


@dataclass(frozen=True)
class Window:
    hwnd: int
    pid: int
    tid: int
    rect: tuple[int, int, int, int]
    title: str = ""
    class_name: str = ""
    style: int = 0
    visible: bool = True
    iconic: bool = False
    cloaked: bool = False

    @property
    def size(self):
        return self.rect[2] - self.rect[0], self.rect[3] - self.rect[1]

    @property
    def drawable(self):
        return self.visible and not self.iconic and not self.cloaked and min(self.size) > 0

    @property
    def panel(self):
        # Matches the maintained SkyEye forms; fail closed for titled/captioned windows.
        return bool(self.title) or (self.style & WS_CAPTION) == WS_CAPTION

    def identity(self):
        return self.hwnd, self.pid, self.tid, self.class_name

    def metadata(self):
        return {"hwnd": self.hwnd, "pid": self.pid, "tid": self.tid,
                "windowRect": self.rect, "title": self.title,
                "className": self.class_name, "style": self.style}


def inside_display(rect, displays):
    left, top, right, bottom = rect
    return right > left and bottom > top and any(
        left >= d[0] and top >= d[1] and right <= d[2] and bottom <= d[3]
        for d in displays
    )


class WindowsBackend:
    """Only window metadata, PrintWindow, optional ImageGrab, and this process's DPI state."""

    def __init__(self, config, clock):
        if os.name != "nt":
            raise OSError("Live capture requires Windows")
        self.allowed = (config.game_pid, config.overlay_pid)
        self.clock = clock
        self.abandoned_call = False
        self.u = c.WinDLL("user32", use_last_error=True)
        self.g = c.WinDLL("gdi32", use_last_error=True)
        self.dwm = c.WinDLL("dwmapi", use_last_error=True)
        self.enum_callback = c.WINFUNCTYPE(w.BOOL, w.HWND, w.LPARAM)
        self.monitor_callback = c.WINFUNCTYPE(w.BOOL, w.HANDLE, w.HDC, c.POINTER(w.RECT), w.LPARAM)
        self._bind(self.u, "SetThreadDpiAwarenessContext", [w.HANDLE], w.HANDLE)
        self._bind(self.u, "EnumWindows", [self.enum_callback, w.LPARAM], w.BOOL)
        self._bind(self.u, "IsWindow", [w.HWND], w.BOOL)
        self._bind(self.u, "IsWindowVisible", [w.HWND], w.BOOL)
        self._bind(self.u, "IsIconic", [w.HWND], w.BOOL)
        self._bind(self.u, "GetWindowThreadProcessId", [w.HWND, c.POINTER(w.DWORD)], w.DWORD)
        self._bind(self.u, "GetForegroundWindow", [], w.HWND)
        self._bind(self.u, "GetWindowRect", [w.HWND, c.POINTER(w.RECT)], w.BOOL)
        self._bind(self.u, "GetWindowTextW", [w.HWND, w.LPWSTR, c.c_int], c.c_int)
        self._bind(self.u, "GetClassNameW", [w.HWND, w.LPWSTR, c.c_int], c.c_int)
        self._bind(self.u, "GetWindowLongW", [w.HWND, c.c_int], w.LONG)
        self._bind(self.u, "EnumDisplayMonitors", [w.HDC, c.c_void_p, self.monitor_callback, w.LPARAM], w.BOOL)
        self._bind(self.u, "GetWindowDC", [w.HWND], w.HDC)
        self._bind(self.u, "ReleaseDC", [w.HWND, w.HDC], c.c_int)
        self._bind(self.u, "PrintWindow", [w.HWND, w.HDC, w.UINT], w.BOOL)
        self._bind(self.g, "CreateCompatibleDC", [w.HDC], w.HDC)
        self._bind(self.g, "CreateCompatibleBitmap", [w.HDC, c.c_int, c.c_int], w.HBITMAP)
        self._bind(self.g, "SelectObject", [w.HDC, w.HANDLE], w.HANDLE)
        self._bind(self.g, "DeleteObject", [w.HANDLE], w.BOOL)
        self._bind(self.g, "DeleteDC", [w.HDC], w.BOOL)
        self._bind(self.g, "GetDIBits", [w.HDC, w.HBITMAP, w.UINT, w.UINT,
                                         c.c_void_p, c.c_void_p, w.UINT], c.c_int)
        self._bind(self.dwm, "DwmGetWindowAttribute", [w.HWND, w.DWORD, c.c_void_p, w.DWORD], c.c_long)
        self._dpi()

    @staticmethod
    def _bind(lib, name, args, result):
        function = getattr(lib, name)
        function.argtypes, function.restype = args, result

    def _dpi(self):
        # Changes only this tool's calling thread. Physical coordinates are mandatory.
        if not self.u.SetThreadDpiAwarenessContext(c.c_void_p(-4)):
            raise OSError("SetThreadDpiAwarenessContext(PER_MONITOR_AWARE_V2) failed")

    def foreground(self):
        hwnd = self.u.GetForegroundWindow()
        pid = w.DWORD()
        if hwnd:
            self.u.GetWindowThreadProcessId(hwnd, c.byref(pid))
        return int(hwnd or 0), int(pid.value)

    def window(self, hwnd):
        if not self.u.IsWindow(hwnd):
            return None
        pid = w.DWORD()
        tid = int(self.u.GetWindowThreadProcessId(hwnd, c.byref(pid)))
        if not tid:
            return None
        if pid.value not in self.allowed:
            # Do not fetch title/geometry/style from a reused, unrelated HWND.
            return Window(int(hwnd), int(pid.value), tid, (0, 0, 0, 0), visible=False)
        rect = w.RECT()
        if not self.u.GetWindowRect(hwnd, c.byref(rect)):
            return None
        title, name = c.create_unicode_buffer(512), c.create_unicode_buffer(256)
        self.u.GetWindowTextW(hwnd, title, len(title))
        self.u.GetClassNameW(hwnd, name, len(name))
        cloaked = w.DWORD()
        hr = self.dwm.DwmGetWindowAttribute(hwnd, 14, c.byref(cloaked), c.sizeof(cloaked))
        if hr != 0:
            raise OSError("DwmGetWindowAttribute(DWMWA_CLOAKED) failed")
        return Window(int(hwnd), int(pid.value), tid,
                      (rect.left, rect.top, rect.right, rect.bottom), title.value, name.value,
                      int(self.u.GetWindowLongW(hwnd, -16)) & 0xFFFFFFFF,
                      bool(self.u.IsWindowVisible(hwnd)), bool(self.u.IsIconic(hwnd)), bool(cloaked.value))

    def windows(self):
        rows, errors = [], []

        @self.enum_callback
        def visit(hwnd, _):
            try:
                pid = w.DWORD()
                self.u.GetWindowThreadProcessId(hwnd, c.byref(pid))
                if pid.value in self.allowed:
                    row = self.window(int(hwnd))
                    if row is not None and row.pid in self.allowed:
                        rows.append(row)
            except Exception as exc:
                errors.append(exc)
                return False
            return True

        ok = self.u.EnumWindows(visit, 0)
        if errors:
            raise errors[0]
        if not ok:
            raise OSError("EnumWindows failed")
        return rows

    def displays(self):
        rows = []

        @self.monitor_callback
        def visit(_monitor, _dc, rect, _data):
            r = rect.contents
            rows.append((r.left, r.top, r.right, r.bottom))
            return True

        if not self.u.EnumDisplayMonitors(None, None, visit, 0):
            raise OSError("EnumDisplayMonitors failed")
        return rows

    def _print_window(self, target):
        from PIL import Image

        class Header(c.Structure):
            _fields_ = [("size", w.DWORD), ("width", w.LONG), ("height", w.LONG),
                        ("planes", w.WORD), ("bits", w.WORD), ("compression", w.DWORD),
                        ("sizeImage", w.DWORD), ("x", w.LONG), ("y", w.LONG),
                        ("used", w.DWORD), ("important", w.DWORD)]

        width, height = target.size
        source = dc = bitmap = old = None
        try:
            source = self.u.GetWindowDC(target.hwnd)
            if not source:
                raise OSError("GetWindowDC failed")
            dc = self.g.CreateCompatibleDC(source)
            if not dc:
                raise OSError("CreateCompatibleDC failed")
            bitmap = self.g.CreateCompatibleBitmap(source, width, height)
            if not bitmap:
                raise OSError("CreateCompatibleBitmap failed")
            old = self.g.SelectObject(dc, bitmap)
            if not old or old == c.c_void_p(-1).value:
                raise OSError("SelectObject failed")
            # Deterministic black background for color-key/transparent layer evidence.
            pixels = c.create_string_buffer(width * height * 4)
            # A fresh compatible bitmap is not guaranteed to be initialized. Clear its DC.
            self._bind(self.g, "PatBlt", [w.HDC, c.c_int, c.c_int, c.c_int, c.c_int, w.DWORD], w.BOOL)
            if not self.g.PatBlt(dc, 0, 0, width, height, 0x00000042):  # BLACKNESS
                raise OSError("PatBlt(BLACKNESS) failed")
            if not self.u.PrintWindow(target.hwnd, dc, 2):  # PW_RENDERFULLCONTENT
                raise OSError("PrintWindow failed")
            self.g.SelectObject(dc, old)
            header = Header()
            header.size, header.width, header.height = c.sizeof(header), width, -height
            header.planes, header.bits = 1, 32
            if self.g.GetDIBits(dc, bitmap, 0, height, pixels, c.byref(header), 0) != height:
                raise OSError("GetDIBits failed")
            return Image.frombytes("RGB", (width, height), pixels.raw, "raw", "BGRX", 0, 1)
        finally:
            if dc and old and old != c.c_void_p(-1).value:
                self.g.SelectObject(dc, old)
            if bitmap:
                self.g.DeleteObject(bitmap)
            if dc:
                self.g.DeleteDC(dc)
            if source:
                self.u.ReleaseDC(target.hwnd, source)

    def _bounded(self, function, guard):
        # PrintWindow has no timeout. The daemon never writes files or starts another capture.
        # On expiry/foreground loss the caller abandons it and the standalone CLI exits.
        done, result = threading.Event(), []

        def work():
            try:
                self._dpi()
                guard()
                result.append((True, function()))
            except BaseException as exc:
                result.append((False, exc))
            finally:
                done.set()

        guard()
        threading.Thread(target=work, name="bounded-window-bitmap", daemon=True).start()
        try:
            while not done.wait(POLL_SECONDS):
                guard()
            guard()
        except BaseException:
            self.abandoned_call = not done.is_set()
            raise
        ok, value = result[0]
        if not ok:
            raise value
        return value

    def capture(self, target, image_format, desktop, guard):
        def work():
            from PIL import ImageGrab
            utc, mono = self.clock.utc(), self.clock.monotonic()
            if desktop:
                image = ImageGrab.grab(bbox=target.rect, include_layered_windows=True, all_screens=True)
            else:
                image = self._print_window(target)
            try:
                output = io.BytesIO()
                if image_format == "jpeg":
                    image.convert("RGB").save(output, "JPEG", quality=95, subsampling=0)
                else:
                    image.save(output, "PNG")
                return output.getvalue(), image.size, {
                    "utcStart": utc, "utcEnd": self.clock.utc(),
                    "monotonicStart": mono, "monotonicEnd": self.clock.monotonic(),
                }
            finally:
                image.close()

        return self._bounded(work, guard)


def run_capture(config, backend_factory=WindowsBackend, clock=None):
    """Create a fresh directory; injected fake backend/clock keep all offline checks inert."""
    config.validate()
    clock = clock or RealClock()
    out = Path(config.out).resolve()
    # This must precede backend construction: never touch a window on an output collision.
    out.mkdir(exist_ok=False)
    started, utc = clock.monotonic(), clock.utc()
    deadline = started + config.seconds
    interval = config.cadence_ms / 1000
    summary = {
        "schema": "skyeye-window-evidence-v1", "utcStart": utc,
        "monotonicStart": started, "deadlineMonotonic": deadline,
        "config": {"gamePid": config.game_pid, "overlayPid": config.overlay_pid,
                   "seconds": config.seconds, "cadenceMs": config.cadence_ms,
                   "gameFormat": config.game_format, "desktopGameRegion": config.desktop_game_region},
        "limits": {"groups": MAX_GROUPS, "totalBytes": MAX_OUTPUT_BYTES,
                   "finalReportReserveBytes": REPORT_RESERVE},
        "groupsAttempted": 0, "groupsSaved": 0, "groupsDropped": 0,
        "imagesCaptured": 0, "imagesSaved": 0, "imagesDropped": 0,
        "droppedGroupsByReason": {}, "missedCadenceSlots": 0, "missedCadenceRanges": [],
        "groups": [], "dataBytesWritten": 0, "writeFailures": [],
        "captureSemantics": "Sequential full-window PrintWindow; layers alone prove drawing, not desktop composition.",
    }
    backend = game = None
    last_expected = []
    attempt_open, frame_images = False, 0
    phase, slot, stop_reason, stop_detail = "initialization", 0, "error", {}

    def foreground_check():
        hwnd, pid = backend.foreground()
        if pid not in (config.game_pid, config.overlay_pid):
            raise StopCapture("foreground_changed", {"hwnd": hwnd, "pid": pid, "phase": phase})
        return {"hwnd": hwnd, "pid": pid}

    def deadline_check():
        if clock.monotonic() >= deadline:
            raise StopCapture("deadline", {"phase": phase})

    def validate_window(expected, require_drawable=False, geometry=False):
        actual = backend.window(expected.hwnd)
        if actual is None or actual.identity() != expected.identity():
            raise StopCapture("window_invalid", {
                "hwnd": expected.hwnd, "expectedPid": expected.pid,
                "actualPid": None if actual is None else actual.pid, "phase": phase})
        if require_drawable and not actual.drawable:
            raise StopCapture("window_not_visible", {"hwnd": expected.hwnd, "phase": phase})
        if geometry and actual.rect != expected.rect:
            raise StopCapture("window_geometry_changed", {"hwnd": expected.hwnd, "phase": phase,
                                                           "before": expected.rect, "after": actual.rect})
        return actual

    def guard():
        deadline_check()
        foreground_check()
        if game is not None:
            current = validate_window(game, True, True)
            if not inside_display(current.rect, backend.displays()):
                raise StopCapture("game_outside_display", {"windowRect": current.rect, "phase": phase})
        for target in last_expected:
            if target.hwnd != (game.hwnd if game else 0):
                validate_window(target, attempt_open and target.drawable, attempt_open)

    try:
        backend = backend_factory(config, clock)
        guard()
        windows = backend.windows()
        candidates = [r for r in windows if r.pid == config.game_pid and r.drawable]
        if not candidates:
            raise StopCapture("no_game_window")
        game = sorted(candidates, key=lambda r: (-(r.size[0] * r.size[1]), r.hwnd))[0]
        overlay_rows = [r for r in windows if r.pid == config.overlay_pid]
        if not overlay_rows:
            raise StopCapture("overlay_pid_no_windows")
        last_expected = [game, overlay_rows[0]]
        summary["selectedGame"] = game.metadata()
        summary["initialDisplays"] = backend.displays()
        guard()
        summary["initialValidation"] = {"valid": True, "foreground": foreground_check(),
                                        "checkedWindows": [r.metadata() for r in last_expected]}
        while True:
            phase = "between_groups"
            if summary["groupsAttempted"] >= MAX_GROUPS:
                raise StopCapture("group_limit")
            # Waiting is polled so foreground loss is noticed within 50 ms.
            due = started + slot * interval
            while clock.monotonic() < due:
                guard()
                clock.sleep(max(0, min(POLL_SECONDS, due - clock.monotonic(), deadline - clock.monotonic())))
            guard()
            windows = backend.windows()
            # Pin the same game HWND for the entire session; never switch to a new window.
            game = validate_window(game, True, False)
            overlay_rows = [r for r in windows if r.pid == config.overlay_pid]
            if not overlay_rows:
                raise StopCapture("overlay_pid_no_windows")
            layers = sorted((r for r in overlay_rows if r.drawable and not r.panel), key=lambda r: r.hwnd)
            # Hidden/panel anchor still validates the explicitly selected overlay PID when no layer is visible.
            last_expected = [game] + layers + [overlay_rows[0]]
            attempt_open, frame_images = True, 0
            summary["groupsAttempted"] += 1
            attempt = summary["groupsAttempted"]
            group_start, group_utc = clock.monotonic(), clock.utc()
            foreground_before = foreground_check()
            guard()
            prefix = f"group-{attempt:03d}"
            files, records = [], []
            targets = [("game", game, config.game_format, False)]
            targets += [("overlay", target, "png", False) for target in layers]
            if config.desktop_game_region:
                targets.append(("desktop_game_region", game, config.game_format, True))
            for role, target, image_format, desktop in targets:
                phase = f"capture:{role}:{target.hwnd}"
                guard()
                data, size, stamps = backend.capture(target, image_format, desktop, guard)
                frame_images += 1
                summary["imagesCaptured"] += 1
                guard()
                if tuple(size) != target.size:
                    raise StopCapture("bitmap_size_mismatch", {"expected": target.size, "actual": size,
                                                               "phase": phase})
                suffix = "jpg" if image_format == "jpeg" else "png"
                filename = f"{prefix}-{role}-{target.hwnd}.{suffix}"
                record = dict(target.metadata(), role=role, bitmapSize=size, image=filename,
                              imageBytes=len(data), sha256=hashlib.sha256(data).hexdigest(), **stamps)
                record["method"] = "ImageGrab.game_bbox.composited" if desktop else "PrintWindow.PW_RENDERFULLCONTENT"
                if desktop:
                    record["compositionLimit"] = "Actual desktop pixels within game rect; includes any occluders; pre/post foreground checks are not atomic with capture."
                records.append(record)
                files.append((filename, data))
                # Include each image and a fixed report reserve before collecting the next bitmap.
                if sum(len(payload) for _, payload in files) + summary["dataBytesWritten"] + REPORT_RESERVE > MAX_OUTPUT_BYTES:
                    raise StopCapture("byte_budget", {"phase": phase, "candidateImageBytes": sum(len(p) for _, p in files),
                                                       "availableDataBytes": MAX_OUTPUT_BYTES - REPORT_RESERVE - summary["dataBytesWritten"]})
            phase = "group_post_validation"
            guard()
            latest = backend.windows()
            latest_layers = sorted(r.hwnd for r in latest if r.pid == config.overlay_pid and r.drawable and not r.panel)
            if latest_layers != [r.hwnd for r in layers]:
                raise StopCapture("overlay_set_changed", {"before": [r.hwnd for r in layers], "after": latest_layers})
            group = {"attempt": attempt, "cadenceSlot": slot, "utcStart": group_utc, "utcEnd": clock.utc(),
                     "monotonicStart": group_start, "monotonicEnd": clock.monotonic(),
                     "scheduledMonotonic": due, "foregroundBefore": foreground_before,
                     "foregroundAfter": foreground_check(), "game": game.metadata(),
                     "overlayWindowCount": len(overlay_rows), "overlayVisibleLayerCount": len(layers),
                     "overlayExcludedPanels": [r.metadata() for r in overlay_rows if r.drawable and r.panel],
                     "overlayNotDrawableCount": sum(not r.drawable for r in overlay_rows), "images": records}
            metadata_name = f"{prefix}.json"
            files.append((metadata_name, json_bytes(group)))
            group_bytes = sum(len(payload) for _, payload in files)
            available = MAX_OUTPUT_BYTES - REPORT_RESERVE - summary["dataBytesWritten"]
            if group_bytes > available:
                raise StopCapture("byte_budget", {"candidateGroupBytes": group_bytes, "availableDataBytes": available,
                                                   "manifestReserveBytes": REPORT_RESERVE})
            phase = "group_commit"
            # No image files are opened before the entire group fits and validation has passed.
            # File writes also count partial bytes if an OS write fails; never silently discard a file.
            written_names = []
            try:
                for filename, data in files:
                    guard()
                    path = out / filename
                    with path.open("xb") as stream:
                        written = stream.write(data)
                        if written != len(data):
                            raise OSError("short file write")
                    written_names.append(filename)
                    summary["dataBytesWritten"] += len(data)
            except BaseException:
                # Keep explicit partial evidence rather than delete files or pretend a group committed.
                actual = sum(p.stat().st_size for p in out.iterdir() if p.is_file())
                summary["dataBytesWritten"] = actual
                summary["writeFailures"].append({"attempt": attempt, "completedFiles": written_names,
                                                  "partialGroup": True})
                raise
            summary["groups"].append({"attempt": attempt, "cadenceSlot": slot, "metadata": metadata_name,
                                       "images": len(records), "bytes": group_bytes})
            summary["groupsSaved"] += 1
            summary["imagesSaved"] += frame_images
            attempt_open = False
            slot += 1
            # Preserve exact cadence slots skipped by capture/encoding; do not burst-catch-up.
            next_slot = int((clock.monotonic() - started) // interval) + 1
            if clock.monotonic() > started + slot * interval and next_slot > slot:
                summary["missedCadenceRanges"].append({"firstSlot": slot, "count": next_slot - slot,
                                                       "reason": "capture_or_write_overrun"})
                summary["missedCadenceSlots"] += next_slot - slot
                slot = next_slot
    except StopCapture as exc:
        stop_reason, stop_detail = exc.reason, exc.detail
    except KeyboardInterrupt:
        stop_reason, stop_detail = "interrupted", {"phase": phase}
    except Exception as exc:
        stop_reason, stop_detail = "error", {"phase": phase, "type": type(exc).__name__, "message": str(exc)[:1024]}
    finally:
        if attempt_open:
            summary["groupsDropped"] += 1
            summary["imagesDropped"] += frame_images
            summary["droppedGroupsByReason"][stop_reason] = 1
        final_validation = {"valid": False, "checkedWindowCount": 0}
        if backend is not None:
            try:
                final_hwnd, final_pid = backend.foreground()
                final_validation["foreground"] = {"hwnd": final_hwnd, "pid": final_pid}
                checked = [validate_window(r).identity() for r in last_expected]
                final_validation["checkedWindowCount"] = len(checked)
                final_validation["identitiesSha256"] = hashlib.sha256(json_bytes(checked)).hexdigest()
                final_validation["valid"] = bool(last_expected)
                if not final_validation["valid"] and stop_reason in ("deadline", "group_limit"):
                    stop_reason = "window_invalid_at_end"
            except Exception as exc:
                final_validation["reason"] = getattr(exc, "reason", type(exc).__name__)
                final_validation["detail"] = getattr(exc, "detail", {"message": str(exc)[:512]})
                if stop_reason in ("deadline", "group_limit"):
                    stop_reason = "window_invalid_at_end"
            summary["nativeCallAbandoned"] = bool(getattr(backend, "abandoned_call", False))
        summary.update(utcEnd=clock.utc(), monotonicEnd=clock.monotonic(), stopReason=stop_reason,
                       stopDetail=stop_detail, finalValidation=final_validation)
        summary["totalBytesWritten"] = summary["dataBytesWritten"]
        # Fixed-point length includes the final report's own length field.
        for _ in range(10):
            report_data = json_bytes(summary)
            total = summary["dataBytesWritten"] + len(report_data)
            if total == summary["totalBytesWritten"]:
                break
            summary["totalBytesWritten"] = total
        report_data = json_bytes(summary)
        if len(report_data) > REPORT_RESERVE or summary["dataBytesWritten"] + len(report_data) > MAX_OUTPUT_BYTES:
            raise RuntimeError("Final report exceeds reserved byte budget")
        with (out / "session.json").open("xb") as stream:
            if stream.write(report_data) != len(report_data):
                raise OSError("short session report write")
    return summary


def build_parser():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--game-pid", type=int, required=True)
    parser.add_argument("--overlay-pid", type=int, required=True)
    parser.add_argument("--out", type=Path, required=True, help="New directory; parent must already exist")
    parser.add_argument("--seconds", type=float, default=20, help="Capture deadline in seconds, 1..60 (default 20)")
    parser.add_argument("--cadence-ms", type=int, default=500, help="Minimum 200 (default 500)")
    parser.add_argument("--game-format", choices=("jpeg", "png"), default="jpeg")
    parser.add_argument("--desktop-game-region", action="store_true", help="Opt in to composited game-rect pixels, with foreground guards")
    return parser


def main(argv=None):
    parser = build_parser()
    args = parser.parse_args(argv)
    config = Config(**vars(args))
    try:
        config.validate()
        report = run_capture(config)
    except (ValueError, OSError) as exc:
        print(json.dumps({"stopReason": "setup_error", "type": type(exc).__name__, "message": str(exc)}), file=sys.stderr)
        return 2
    print(json.dumps({"out": str(config.out.resolve()), "stopReason": report["stopReason"],
                      "groupsSaved": report["groupsSaved"], "groupsDropped": report["groupsDropped"],
                      "imagesDropped": report["imagesDropped"], "totalBytesWritten": report["totalBytesWritten"]}))
    return 0 if report["stopReason"] in ("deadline", "group_limit") and report["groupsSaved"] else 3


if __name__ == "__main__":
    raise SystemExit(main())
