"""Offline-only fake-clock/fake-window tests. No live backend is constructed."""
from __future__ import annotations

import argparse
from dataclasses import replace
from datetime import datetime, timedelta, timezone
import hashlib
import io
import json
from pathlib import Path
import sys
import tempfile
import threading
import unittest
from unittest.mock import patch

sys.dont_write_bytecode = True
import bounded_visual_capture as capture
from PIL import Image

ROOT = Path(__file__).resolve().parent


class FakeClock:
    def __init__(self):
        self.now = 0.0
        self.on_sleep = None

    def monotonic(self):
        return self.now

    def utc(self):
        return (datetime(2026, 1, 1, tzinfo=timezone.utc) + timedelta(seconds=self.now)).isoformat()

    def sleep(self, seconds):
        assert seconds >= 0
        self.now += seconds
        if self.on_sleep:
            self.on_sleep(self)


class FakeWindows:
    def __init__(self, config, clock):
        self.clock = clock
        self.rows = {
            101: capture.Window(101, 10, 11, (10, 10, 42, 34), "GAME", "FakeGame"),
            201: capture.Window(201, 20, 21, (10, 10, 42, 34), "", "FakeLayer"),
            202: capture.Window(202, 20, 21, (1, 1, 20, 20), "Settings", "FakePanel"),
        }
        self.fg = (101, 10)
        self.monitors = [(0, 0, 100, 100)]
        self.calls = []
        self.enumerations = 0
        self.duration = 0.0
        self.after_capture = None
        self.on_window = None
        self.error = None
        self.wrong_size = False
        self.abandoned_call = False

    def foreground(self):
        return self.fg

    def windows(self):
        self.enumerations += 1
        return list(self.rows.values())

    def window(self, hwnd):
        if self.on_window:
            self.on_window(self, hwnd)
        return self.rows.get(hwnd)

    def displays(self):
        return self.monitors

    def capture(self, target, image_format, desktop, guard):
        guard()
        self.calls.append((target.hwnd, image_format, desktop))
        if self.error:
            raise self.error
        before, utc = self.clock.now, self.clock.utc()
        self.clock.now += self.duration
        size = (target.size[0] - 1, target.size[1]) if self.wrong_size else target.size
        with Image.new("RGB", size, (20, 30, 40)) as image:
            data = io.BytesIO()
            image.save(data, "JPEG" if image_format == "jpeg" else "PNG")
        if self.after_capture:
            self.after_capture(self)
        return data.getvalue(), size, {"utcStart": utc, "utcEnd": self.clock.utc(),
                                       "monotonicStart": before, "monotonicEnd": self.clock.now}


class VisualCaptureTests(unittest.TestCase):
    def setUp(self):
        # All temporary files are created under this explicitly scoped directory.
        self.temp = tempfile.TemporaryDirectory(prefix=".fake-selftest-", dir=ROOT)
        self.base = Path(self.temp.name).resolve()
        assert self.base.parent == ROOT and self.base.name.startswith(".fake-selftest-")
        self.clock = FakeClock()
        self.config = capture.Config(10, 20, self.base / "new-run", seconds=1)
        self.backend = FakeWindows(self.config, self.clock)

    def tearDown(self):
        # Verify the absolute deletion target before TemporaryDirectory removes only our files.
        assert self.base.parent == ROOT and self.base.name.startswith(".fake-selftest-")
        self.temp.cleanup()

    def run_fake(self, **changes):
        config = replace(self.config, **changes)
        return capture.run_capture(config, lambda *_: self.backend, self.clock)

    def images(self):
        return list(self.config.out.glob("*.png")) + list(self.config.out.glob("*.jpg"))

    def first_group(self):
        return json.loads((self.config.out / "group-001.json").read_text(encoding="utf-8"))

    def test_import_does_not_construct_native_backend_or_parse_argv(self):
        # Loading a second copy would fail if WinDLL or argparse were used at import time.
        import importlib.util
        spec = importlib.util.spec_from_file_location("_offline_capture_import", ROOT / "bounded_visual_capture.py")
        module = importlib.util.module_from_spec(spec)
        sys.modules[spec.name] = module
        with patch.object(capture.c, "WinDLL", create=True, side_effect=AssertionError("live API called")), \
             patch.object(capture.argparse.ArgumentParser, "parse_args", side_effect=AssertionError("argparse called")):
            spec.loader.exec_module(module)
        sys.modules.pop(spec.name)

    def test_required_arguments_and_defaults(self):
        args = capture.build_parser().parse_args(["--game-pid", "10", "--overlay-pid", "20", "--out", "new"])
        self.assertEqual((args.seconds, args.cadence_ms, args.game_format), (20, 500, "jpeg"))
        self.assertFalse(args.desktop_game_region)
        with self.assertRaises(SystemExit), patch("sys.stderr", io.StringIO()):
            capture.build_parser().parse_args([])

    def test_invalid_configuration_before_backend(self):
        for changes in ({"seconds": 0}, {"seconds": 61}, {"seconds": float("nan")},
                        {"cadence_ms": 199}, {"game_pid": 0}, {"overlay_pid": 10}):
            with self.subTest(changes=changes), self.assertRaises(ValueError):
                self.run_fake(**changes)
        self.assertFalse(self.config.out.exists())
        self.assertEqual(self.backend.enumerations, 0)

    def test_existing_output_is_never_reused_and_no_backend_access(self):
        self.config.out.mkdir()
        marker = self.config.out / "keep.txt"
        marker.write_text("untouched", encoding="utf-8")
        factory = unittest.mock.Mock(side_effect=AssertionError("backend constructed"))
        with self.assertRaises(FileExistsError):
            capture.run_capture(self.config, factory, self.clock)
        factory.assert_not_called()
        self.assertEqual(marker.read_text(encoding="utf-8"), "untouched")

    def test_deadline_full_resolution_hashes_panels_and_exact_bytes(self):
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "deadline")
        self.assertEqual(report["groupsSaved"], 2)
        self.assertEqual(report["groupsDropped"], 0)
        group = self.first_group()
        self.assertEqual(group["overlayVisibleLayerCount"], 1)
        self.assertEqual(len(group["overlayExcludedPanels"]), 1)
        self.assertEqual([r["role"] for r in group["images"]], ["game", "overlay"])
        for record in group["images"]:
            data = (self.config.out / record["image"]).read_bytes()
            self.assertEqual(hashlib.sha256(data).hexdigest(), record["sha256"])
            with Image.open(io.BytesIO(data)) as image:
                self.assertEqual(image.size, (32, 24))
            self.assertEqual(record["bitmapSize"], [32, 24])
            self.assertIn("utcStart", record)
            self.assertIn("monotonicEnd", record)
        actual = sum(p.stat().st_size for p in self.config.out.iterdir())
        self.assertEqual(actual, report["totalBytesWritten"])
        self.assertLessEqual(actual, 80 * 1024 * 1024)
        self.assertTrue(report["initialValidation"]["valid"])
        self.assertTrue(report["finalValidation"]["valid"])

    def test_game_png_option(self):
        self.run_fake(game_format="png")
        self.assertTrue((self.config.out / "group-001-game-101.png").exists())

    def test_group_limit_exactly_120(self):
        report = self.run_fake(seconds=60, cadence_ms=200)
        self.assertEqual(report["stopReason"], "group_limit")
        self.assertEqual(report["groupsAttempted"], 120)
        self.assertEqual(report["groupsSaved"], 120)
        self.assertEqual(len(list(self.config.out.glob("group-*.json"))), 120)
        self.assertLess(self.clock.now, 60)

    def test_byte_budget_includes_final_report_and_stops_before_more_capture(self):
        limit = capture.REPORT_RESERVE + 100
        with patch.object(capture, "MAX_OUTPUT_BYTES", limit):
            report = self.run_fake()
        self.assertEqual(report["stopReason"], "byte_budget")
        self.assertEqual(report["groupsSaved"], 0)
        self.assertEqual(report["groupsDropped"], 1)
        self.assertEqual(report["imagesDropped"], 1)
        self.assertEqual(len(self.backend.calls), 1)
        self.assertEqual(len(self.images()), 0)
        self.assertEqual(report["totalBytesWritten"], (self.config.out / "session.json").stat().st_size)
        self.assertLessEqual(report["totalBytesWritten"], limit)

    def test_group_metadata_also_counts_against_budget(self):
        with patch.object(capture, "MAX_OUTPUT_BYTES", capture.REPORT_RESERVE + 1000):
            report = self.run_fake()
        self.assertEqual(report["stopReason"], "byte_budget")
        self.assertIn("candidateGroupBytes", report["stopDetail"])
        self.assertEqual(report["imagesDropped"], 2)
        self.assertEqual(len(self.images()), 0)

    def test_initial_unrelated_foreground_captures_nothing(self):
        self.backend.fg = (999, 999)
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "foreground_changed")
        self.assertEqual(report["groupsAttempted"], 0)
        self.assertEqual(self.backend.calls, [])
        self.assertEqual(self.backend.enumerations, 0)
        self.assertEqual(self.images(), [])

    def test_foreground_change_during_capture_discards_whole_group(self):
        self.backend.after_capture = lambda backend: setattr(backend, "fg", (999, 999))
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "foreground_changed")
        self.assertEqual(len(self.backend.calls), 1)
        self.assertEqual(report["groupsDropped"], 1)
        self.assertEqual(report["imagesDropped"], 1)
        self.assertEqual(self.images(), [])

    def test_foreground_change_during_wait_stops_before_next_capture(self):
        self.clock.on_sleep = lambda _: setattr(self.backend, "fg", (999, 999))
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "foreground_changed")
        self.assertEqual(report["groupsSaved"], 1)
        self.assertEqual(len(self.backend.calls), 2)

    def test_specified_overlay_foreground_is_accepted(self):
        self.backend.fg = (202, 20)
        self.assertEqual(self.run_fake()["groupsSaved"], 2)

    def test_reused_game_hwnd_with_other_pid_is_stopped(self):
        def swap(backend):
            backend.rows[101] = replace(backend.rows[101], pid=999)
        self.backend.after_capture = swap
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "window_invalid")
        self.assertEqual(report["stopDetail"]["actualPid"], 999)
        self.assertEqual(self.images(), [])

    def test_destroyed_overlay_hwnd_is_stopped(self):
        self.backend.after_capture = lambda backend: backend.rows.pop(201, None)
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "window_invalid")
        self.assertEqual(len(self.backend.calls), 1)
        self.assertEqual(self.images(), [])

    def test_overlay_pid_has_no_windows(self):
        self.backend.rows = {101: self.backend.rows[101]}
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "overlay_pid_no_windows")
        self.assertEqual(self.backend.calls, [])

    def test_no_visible_game(self):
        self.backend.rows[101] = replace(self.backend.rows[101], iconic=True)
        self.assertEqual(self.run_fake()["stopReason"], "no_game_window")
        self.assertEqual(self.backend.calls, [])

    def test_game_outside_real_display_is_not_captured(self):
        self.backend.monitors = [(0, 0, 30, 100)]
        self.assertEqual(self.run_fake()["stopReason"], "game_outside_display")
        self.assertEqual(self.backend.calls, [])

    def test_virtual_monitor_gap_is_not_a_display(self):
        self.backend.monitors = [(0, 0, 20, 100), (40, 0, 100, 100)]
        self.assertEqual(self.run_fake()["stopReason"], "game_outside_display")

    def test_deadline_during_bitmap_discards_frame(self):
        self.backend.duration = 1.1
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "deadline")
        self.assertEqual(report["groupsDropped"], 1)
        self.assertEqual(report["imagesDropped"], 1)
        self.assertEqual(self.images(), [])

    def test_non_drawable_layers_are_counted_but_not_captured(self):
        self.backend.rows[201] = replace(self.backend.rows[201], visible=False)
        report = self.run_fake()
        self.assertEqual(report["groupsSaved"], 2)
        group = self.first_group()
        self.assertEqual(group["overlayVisibleLayerCount"], 0)
        self.assertEqual(group["overlayNotDrawableCount"], 1)
        self.assertEqual(len(group["images"]), 1)

    def test_captioned_untitled_panel_is_excluded(self):
        self.backend.rows[203] = capture.Window(203, 20, 21, (0, 0, 10, 10), style=capture.WS_CAPTION)
        self.run_fake()
        self.assertEqual(len(self.first_group()["overlayExcludedPanels"]), 2)
        self.assertNotIn(203, [hwnd for hwnd, _, _ in self.backend.calls])

    def test_overlay_set_change_is_an_explicit_dropped_group(self):
        def add(backend):
            backend.rows[203] = capture.Window(203, 20, 21, (0, 0, 10, 10))
        self.backend.after_capture = add
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "overlay_set_changed")
        self.assertEqual(report["groupsDropped"], 1)
        self.assertEqual(report["imagesDropped"], 2)
        self.assertEqual(self.images(), [])

    def test_bitmap_dimension_error_is_explicit(self):
        self.backend.wrong_size = True
        self.assertEqual(self.run_fake()["stopReason"], "bitmap_size_mismatch")
        self.assertEqual(self.images(), [])

    def test_capture_failure_has_message_and_drop_count(self):
        self.backend.error = OSError("PrintWindow fake failure")
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "error")
        self.assertEqual(report["stopDetail"]["message"], "PrintWindow fake failure")
        self.assertEqual(report["groupsDropped"], 1)
        self.assertEqual(report["imagesCaptured"], 0)

    def test_cadence_overrun_records_exact_missed_slots(self):
        self.backend.duration = 0.16
        report = self.run_fake(cadence_ms=200)
        self.assertEqual(report["groupsSaved"], 2)
        self.assertEqual(report["groupsDropped"], 1)
        self.assertEqual(report["missedCadenceSlots"], 2)
        self.assertEqual([r["firstSlot"] for r in report["missedCadenceRanges"]], [1, 3])

    def test_desktop_is_opt_in_full_size_and_separately_labeled(self):
        self.run_fake(desktop_game_region=True)
        group = self.first_group()
        self.assertEqual(len(group["images"]), 3)
        record = group["images"][-1]
        self.assertEqual(record["role"], "desktop_game_region")
        self.assertEqual(record["method"], "ImageGrab.game_bbox.composited")
        self.assertEqual(record["bitmapSize"], [32, 24])
        self.assertIn("not atomic", record["compositionLimit"])

    def test_final_validation_detects_hwnd_loss_after_deadline(self):
        def expire(backend, hwnd):
            if self.clock.now >= 1 and hwnd == 101:
                backend.rows.pop(101, None)
        self.backend.on_window = expire
        report = self.run_fake()
        self.assertEqual(report["stopReason"], "window_invalid_at_end")
        self.assertFalse(report["finalValidation"]["valid"])

    def test_native_watchdog_with_fake_guard_never_loads_win32(self):
        backend = capture.WindowsBackend.__new__(capture.WindowsBackend)
        backend.abandoned_call = False
        backend._dpi = lambda: None
        release = threading.Event()
        checks = []

        def guard():
            checks.append(1)
            if len(checks) >= 4:
                raise capture.StopCapture("deadline")

        try:
            with self.assertRaises(capture.StopCapture):
                backend._bounded(lambda: release.wait(2), guard)
            self.assertTrue(backend.abandoned_call)
        finally:
            release.set()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()
    suite = unittest.defaultTestLoader.loadTestsFromTestCase(VisualCaptureTests)
    names = [test.id() for test in suite]
    result = unittest.TextTestRunner(verbosity=2).run(suite)
    report = {"schema": "skyeye-visual-capture-offline-selftest-v1", "utc": datetime.now(timezone.utc).isoformat(),
              "mode": "fake-clock/fake-window only", "liveBackendConstructed": False,
              "testsRun": result.testsRun, "failures": len(result.failures), "errors": len(result.errors),
              "passed": result.wasSuccessful(), "tests": names,
              "failureDetails": [{"test": test.id(), "traceback": text} for test, text in result.failures + result.errors]}
    if args.report:
        target = args.report.resolve()
        if target.parent != ROOT:
            parser.error("report must be directly inside the visual-capture directory")
        with target.open("x", encoding="utf-8") as stream:
            json.dump(report, stream, ensure_ascii=False, indent=2)
            stream.write("\n")
    return 0 if result.wasSuccessful() else 1


if __name__ == "__main__":
    raise SystemExit(main())
