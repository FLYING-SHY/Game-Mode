"""Pure-memory regression for the per-build material-consistency verifier.

Run with python -B to avoid bytecode files. No game, process, network, fixture
files, baseline, history, or release artifacts are modified by these tests.
"""

import importlib.util
import io
import json
import sys
import unittest
from contextlib import ExitStack, redirect_stdout
from pathlib import Path
from unittest.mock import patch


HERE = Path(__file__).resolve().parent


def load_verifier(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


CURRENT = load_verifier("current_perbuild_verifier", HERE.parents[1] / "scripts" / "verify_perbuild_adaptation.py")
BEFORE = load_verifier("before_perbuild_verifier", HERE / "before" / "verify_perbuild_adaptation.py")
LOG_NAMES = ["minimap-diagnostics.log", "air-diagnostics.log", "air-outline-diagnostics.log", "overlay-diagnostics.log"]
CHECK_NAMES = ["check_game", "check_delivered", "check_config", "check_sources", "check_layout", "check_fixtures", "check_provenance", "check_live_samples", "check_runtime_logs", "check_document"]


def fixture_data():
    return {"suites": {
        "build-gate": {"exitCode": 0},
        "layout-preflight": {
            "Release": {"exitCode": 0, "checks": 104},
            "Maintenance": {"exitCode": 0, "checks": 104},
        },
        "decoder-layout": {"modes": [{"exitCode": 0}, {"exitCode": 0}]},
    }}


def fixture_result(data, verifier=CURRENT):
    baseline = {"fixtures": {"rel": "in-memory.json", "expect": {
        "suites.layout-preflight.Release.checks": 104,
        "suites.layout-preflight.Maintenance.checks": 104,
    }}}
    checks = verifier.Checks()
    with patch.object(Path, "exists", return_value=True), patch.object(Path, "read_text", return_value=json.dumps(data)):
        verifier.check_fixtures(checks, Path("in-memory"), baseline)
    return checks.rows[0]


def log_result(logs, required=("draw", "visible"), verifier=CURRENT):
    baseline = {"clientRuntimeLogs": {
        "dir": "in-memory", "files": LOG_NAMES,
        "forbiddenSubstrings": ["not-verified", "unsupported"],
        "requiredSubstrings": list(required),
    }}
    checks = verifier.Checks()
    with patch.object(Path, "exists", lambda path: path.name in logs), patch.object(verifier, "decode_log", side_effect=lambda path: logs[path.name]):
        verifier.check_runtime_logs(checks, Path("in-memory"), baseline)
    return checks.rows[0]


def run_main(no_write, passes=True):
    output = io.StringIO()
    arguments = ["verify_perbuild_adaptation.py", "--root", "in-memory"]
    if no_write:
        arguments.append("--no-write")

    def fake_check(checks, *_):
        checks.add("in-memory", passes, "no external I/O")

    with ExitStack() as stack:
        stack.enter_context(patch.object(sys, "argv", arguments))
        stack.enter_context(patch.object(Path, "exists", return_value=True))
        stack.enter_context(patch.object(Path, "read_text", return_value="{}"))
        writer = stack.enter_context(patch.object(Path, "write_text"))
        for name in CHECK_NAMES:
            stack.enter_context(patch.object(CURRENT, name, side_effect=fake_check))
        stack.enter_context(redirect_stdout(output))
        status = CURRENT.main()
    return status, output.getvalue(), writer


class FixtureTests(unittest.TestCase):
    def test_positive_nested_modes(self):
        self.assertEqual(fixture_result(fixture_data())["status"], "PASS")

    def test_top_level_nonzero_exit_code(self):
        data = fixture_data()
        data["suites"]["build-gate"]["exitCode"] = 3
        row = fixture_result(data)
        self.assertEqual(row["status"], "FAIL")
        self.assertIn("suites.build-gate.exitCode=3", row["detail"])

    def test_nested_release_exit_code_regression(self):
        data = fixture_data()
        data["suites"]["layout-preflight"]["Release"]["exitCode"] = 7
        self.assertEqual(fixture_result(data, BEFORE)["status"], "PASS")
        row = fixture_result(data)
        self.assertEqual(row["status"], "FAIL")
        self.assertIn("suites.layout-preflight.Release.exitCode=7", row["detail"])

    def test_nested_list_mode_exit_code(self):
        data = fixture_data()
        data["suites"]["decoder-layout"]["modes"][1]["exitCode"] = 2
        row = fixture_result(data)
        self.assertEqual(row["status"], "FAIL")
        self.assertIn("suites.decoder-layout.modes[1].exitCode=2", row["detail"])

    def test_expected_assertion_count_still_checked(self):
        data = fixture_data()
        data["suites"]["layout-preflight"]["Release"]["checks"] = 103
        self.assertEqual(fixture_result(data)["status"], "FAIL")

    def test_multiple_exit_failures_reported(self):
        data = fixture_data()
        data["suites"]["layout-preflight"]["Maintenance"]["exitCode"] = 4
        data["suites"]["decoder-layout"]["modes"][0]["exitCode"] = 5
        row = fixture_result(data)
        self.assertEqual(row["status"], "FAIL")
        self.assertIn("Maintenance.exitCode=4", row["detail"])
        self.assertIn("modes[0].exitCode=5", row["detail"])


class RuntimeLogTests(unittest.TestCase):
    def test_all_required_strings_across_logs(self):
        logs = dict.fromkeys(LOG_NAMES, "")
        logs[LOG_NAMES[0]] = "draw\ndraw"
        logs[LOG_NAMES[1]] = "visible"
        row = log_result(logs)
        self.assertEqual(row["status"], "PASS")
        self.assertIn("minimap draws=2", row["detail"])

    def test_second_required_string_regression(self):
        logs = dict.fromkeys(LOG_NAMES, "")
        logs[LOG_NAMES[0]] = "draw"
        self.assertEqual(log_result(logs, verifier=BEFORE)["status"], "PASS")
        row = log_result(logs)
        self.assertEqual(row["status"], "FAIL")
        self.assertIn("missing required substring 'visible'", row["detail"])

    def test_first_required_string_missing(self):
        logs = dict.fromkeys(LOG_NAMES, "visible")
        self.assertEqual(log_result(logs)["status"], "FAIL")

    def test_forbidden_substring_still_checked(self):
        logs = dict.fromkeys(LOG_NAMES, "draw visible")
        logs[LOG_NAMES[-1]] = "unsupported"
        self.assertEqual(log_result(logs)["status"], "FAIL")

    def test_missing_log_still_checked(self):
        logs = {name: "draw visible" for name in LOG_NAMES[:-1]}
        self.assertEqual(log_result(logs)["status"], "FAIL")

    def test_empty_required_spec_fails_without_exception(self):
        logs = dict.fromkeys(LOG_NAMES, "draw")
        row = log_result(logs, required=())
        self.assertEqual(row["status"], "FAIL")
        self.assertIn("no requiredSubstrings configured", row["detail"])


class ReportTests(unittest.TestCase):
    def test_no_write_has_no_report_write(self):
        status, output, writer = run_main(no_write=True)
        self.assertEqual(status, 0)
        writer.assert_not_called()
        self.assertIn("not written (--no-write)", output)
        self.assertIn("full G/R/D acceptance not assessed", output)

    def test_written_report_declares_consistency_scope(self):
        status, output, writer = run_main(no_write=False)
        self.assertEqual(status, 0)
        writer.assert_called_once()
        report = json.loads(writer.call_args.args[0])
        self.assertTrue(report["passed"])
        self.assertEqual(report["verificationScope"], "recorded_material_consistency_only")
        self.assertIn("full game/cross-session/delivery acceptance is not assessed", report["acceptanceClaim"])
        self.assertIn("recorded-material checks passed", output)

    def test_failed_checks_keep_nonzero_exit(self):
        status, _, writer = run_main(no_write=True, passes=False)
        self.assertEqual(status, 1)
        writer.assert_not_called()


if __name__ == "__main__":
    unittest.main(verbosity=2)
