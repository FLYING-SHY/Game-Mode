using System;
using System.Linq;
using System.Web.Script.Serialization;

namespace CoH3MemoryTool;

internal static class RecoveryProbeSelfTests
{
#if RECOVERY_FIXTURE
    private static int Main() => Run();
#endif

    internal static int Run()
    {
        int passed = 0;
        Action<bool, string> check = (condition, name) => { if (!condition) throw new InvalidOperationException("FAIL " + name); passed++; Console.WriteLine("PASS " + name); };
        try
        {
            check(RecoveryProbe.IsCameraFailure(new Exception("镜头已切换。")) && RecoveryProbe.IsCameraFailure(new Exception("镜头正在切换。")), "source camera diagnostics qualify the optional trigger");
            check(!RecoveryProbe.IsCameraFailure(new Exception("fixture unrelated failure")) && !RecoveryProbe.IsCameraFailure(null), "unrelated errors and no error do not qualify");
            var none = RecoveryProbe.Run(null, () => throw new Exception("unexpected context read"), () => throw new Exception("unexpected camera read"), () => throw new Exception("unexpected snapshot read"), () => 0, () => "utc", ms => throw new Exception("unexpected sleep"), () => false);
            check(none.Status == "not_triggered" && none.Attempts.Count == 0 && none.DirectCameraAttempts == 0 && none.SnapshotAttempts == 0, "no natural error means not_triggered with no callback or read");

            var f = new Fixture { CameraFailures = 1 };
            string naturalFixtureError = null;
            try { f.Camera(); } catch (Exception ex) { naturalFixtureError = ex.ToString(); }
            f.Trigger.Error = naturalFixtureError;
            var original = new LiveProbeResult { Feature = "event_window", Status = "fail", Error = naturalFixtureError, Evidence = new { air = new { status = "invalidated" } } };
            int cameraBeforeMap = f.CameraCalls;
            f.Map();
            check(f.MapCalls == 1 && f.CameraCalls == cameraBeforeMap && f.SnapshotCalls == 0, "fake map success after camera failure contributes no camera or Snapshot evidence");
            var e = f.Run();
            check(e.CameraAndSnapshotRecovered && e.Status == "observed" && e.Attempts.Count == 1 && e.DirectCameraAttempts == 1 && e.SnapshotAttempts == 1, "same fake reader later completes camera and Snapshot pair");
            check(e.StartContext.SameReader && e.EndContext.SameReader && e.Attempts[0].Camera.Before.Valid && e.Attempts[0].Camera.After.Valid && e.Attempts[0].Snapshot.Before.Valid && e.Attempts[0].Snapshot.After.Valid && e.ReaderOpenAttemptsAtTrigger == 1, "same object and original match epoch guard each side of both calls");
            string originalJson = new JavaScriptSerializer().Serialize(original);
            check(original.Status == "fail" && original.Error == naturalFixtureError && originalJson.Contains("\"status\":\"invalidated\"") && BoundedPoll.ExitCodeFor(new[] { original, new LiveProbeResult { Status = e.Status } }) == 3, "successful recovery preserves original event fail air invalidated and exit3");
            string json = new JavaScriptSerializer().Serialize(e);
            check(json.Contains("\"View\":[") && json.Contains("\"Projection\":[") && json.Contains("\"identity\":\"fixture-local\"") && json.Contains("\"StartedUtc\":") && json.Contains("\"FinishedUtc\":") && json.Contains("\"InitialEpoch\":7"), "camera matrices local identity UTC and original epoch survive serialization");

            f = new Fixture { CameraFailures = 1 }; e = f.Run();
            check(e.CameraAndSnapshotRecovered && e.Attempts.Count == 2 && e.DirectCameraAttempts == 2 && e.SnapshotAttempts == 1 && e.Attempts[0].Snapshot == null && e.Attempts[0].Camera.Error != null, "failed camera is logged skips Snapshot and retries on same instance");
            f = new Fixture { CameraFailures = 20, FrozenClock = true }; e = f.Run();
            check(!e.CameraAndSnapshotRecovered && e.StopReason == "attempt_cap" && e.Attempts.Count == 3 && f.CameraCalls == 3 && f.SnapshotCalls == 0, "frozen clock still limits total attempts to3");
            f = new Fixture { SnapshotFailures = 20 }; e = f.Run();
            check(e.DirectCameraAttempts == 3 && e.SnapshotAttempts == 3 && e.Attempts.All(a => a.Camera.Status == "observed" && a.Snapshot.Status == "fail") && !e.CameraAndSnapshotRecovered, "three failed Snapshots retain partial camera evidence without claiming pair recovery");

            f = new Fixture { Match = "changed" }; e = f.Run();
            check(e.Status == "blocked" && e.DirectCameraAttempts == 0 && !e.StartContext.Valid, "preexisting match change stops before any read");
            f = new Fixture { Epoch = 8 }; e = f.Run();
            check(e.Status == "blocked" && e.DirectCameraAttempts == 0 && e.InitialEpoch == 7, "preexisting epoch change is compared against original event epoch");
            f = new Fixture { CameraAction = x => x.Match = "changed" }; e = f.Run();
            check(e.Status == "blocked" && e.Attempts[0].Camera.Status == "invalidated" && e.SnapshotAttempts == 0, "match change during camera invalidates result and stops before Snapshot");
            f = new Fixture { CameraAction = x => x.Epoch++ }; e = f.Run();
            check(e.Status == "blocked" && e.SnapshotAttempts == 0 && !e.Attempts[0].Camera.After.Valid, "epoch change during camera stops continuation");
            f = new Fixture { SnapshotAction = x => x.Epoch++ }; e = f.Run();
            check(e.Status == "blocked" && !e.CameraAndSnapshotRecovered && e.Attempts.Count == 1 && e.Attempts[0].Snapshot.Status == "invalidated", "epoch change during successful Snapshot prevents recovery claim");
            f = new Fixture { CameraFailures = 1, CameraAction = x => x.Allowed = false }; e = f.Run();
            check(e.Status == "blocked" && e.DirectCameraAttempts == 1 && e.Attempts[0].Camera.Error != null && !e.Attempts[0].Camera.After.Allowed, "guard also runs after failed camera and preserves its exception");
            f = new Fixture { ContextReader = new object() }; e = f.Run();
            check(e.Status == "blocked" && !e.StartContext.SameReader && e.DirectCameraAttempts == 0, "replacement object with same match and epoch is rejected");
            f = new Fixture { OpenAttempts = 2 }; e = f.Run();
            check(e.Status == "blocked" && e.DirectCameraAttempts == 0, "reader reopen count change stops probe");
            f = new Fixture { Allowed = false }; e = f.Run();
            check(e.Status == "blocked" && e.Attempts.Count == 0, "disallowed lease stops before read");
            f = new Fixture { Cancelled = true }; e = f.Run();
            check(e.Status == "cancelled" && e.Attempts.Count == 0, "pre-cancellation starts no recovery attempt");
            f = new Fixture { CameraAction = x => x.Cancelled = true }; e = f.Run();
            check(e.Status == "cancelled" && e.DirectCameraAttempts == 1 && e.SnapshotAttempts == 0, "cancellation between camera and Snapshot stops next call");
            f = new Fixture { Now = 2000 }; e = f.Run();
            check(e.StopReason == "deadline" && e.DirectCameraAttempts == 0, "expired budget starts no query");
            f = new Fixture { CameraAction = x => x.Now = 2001 }; e = f.Run();
            check(e.StopReason == "deadline" && e.SnapshotAttempts == 0 && e.OverrunMilliseconds == 1 && !e.CameraAndSnapshotRecovered, "slow camera overrun is recorded and prevents next read");
            f = new Fixture { SnapshotAction = x => x.Now = 2500 }; e = f.Run();
            check(e.CameraAndSnapshotRecovered && e.OverrunMilliseconds == 500 && e.Attempts.Count == 1, "completed guarded pair may overrun cooperative budget without extra attempt");
            f = new Fixture { Now = 1999, CameraFailures = 20 }; e = f.Run();
            check(e.StopReason == "deadline" && f.Slept == 1 && e.DirectCameraAttempts == 1, "last retry gap clamps to remaining1ms");

            check(EventPrewarm.ParseMilliseconds("0", out int zero) && zero == 0 && EventPrewarm.ParseMilliseconds("5000", out int maximum) && maximum == 5000 && new[] { "-1", "5001", "bad", "2147483648" }.All(v => !EventPrewarm.ParseMilliseconds(v, out _)), "prewarm parser accepts only0 through5000");
            long now = 0; int waits = 0, guards = 0;
            bool done = EventPrewarm.Wait(4000, () => guards++, () => now, ms => { now += ms; waits++; }, () => false);
            check(done && now == 4000 && waits == 80 && guards == 81, "prewarm uses bounded50ms slices and a final guard");
            now = 0; waits = 0;
            done = EventPrewarm.Wait(5000, () => { }, () => now, ms => { now += ms; waits++; }, () => now >= 50);
            check(!done && now == 50 && waits == 1, "prewarm cooperative cancellation prevents completion");
            now = 0; bool changed = false;
            try { EventPrewarm.Wait(4000, () => { if (now >= 50) throw new InvalidOperationException("fixture context changed"); }, () => now, ms => now += ms, () => false); }
            catch (InvalidOperationException ex) { changed = ex.Message == "fixture context changed"; }
            check(changed && now == 50, "prewarm propagates original context failure before events");
            waits = 0; bool capped = false;
            try { EventPrewarm.Wait(4000, () => { }, () => 0, ms => waits++, () => false); }
            catch (InvalidOperationException) { capped = true; }
            check(capped && waits == 101, "prewarm frozen clock is capped at101 wait rounds");
            now = 0;
            done = EventPrewarm.Wait(1, () => { }, () => now, ms => now += ms, () => false);
            check(done && now == 1, "prewarm final sleep clamps to remaining1ms");
            done = EventPrewarm.Wait(0, () => { }, () => 0, ms => throw new Exception("unexpected zero-delay sleep"), () => false);
            check(done, "zero prewarm completes without sleep");
            Console.WriteLine("RECOVERY_SELF_TEST_ONLY passed=" + passed + "; synthetic delegates and virtual time only; no engine, coordinator, credential store, process discovery, attach or GUI.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private sealed class Fixture
    {
        internal readonly RecoveryProbeTrigger Trigger;
        internal object ContextReader;
        internal string Match = "fixture-match";
        internal long Epoch = 7;
        internal bool Allowed = true, Cancelled, FrozenClock;
        internal int OpenAttempts = 1, CameraFailures, SnapshotFailures, CameraCalls, SnapshotCalls, MapCalls, Slept;
        internal long Now;
        internal Action<Fixture> CameraAction, SnapshotAction;

        internal Fixture()
        {
            ContextReader = this;
            Trigger = new RecoveryProbeTrigger { Reader = this, ReaderOpenAttempts = 1, Match = "fixture-match", Epoch = 7, Utc = "2026-01-01T00:00:00.0000000Z", Error = "synthetic fixture event camera failure" };
        }

        internal object Camera()
        {
            CameraCalls++;
            CameraAction?.Invoke(this);
            if (CameraFailures-- > 0) throw new Exception("镜头已切换。");
            return new { View = new float[16], Projection = new float[16] };
        }

        private object Snapshot()
        {
            SnapshotCalls++;
            SnapshotAction?.Invoke(this);
            if (SnapshotFailures-- > 0) throw new Exception("fixture Snapshot failure");
            return new { rowCount = 1, local = new { identity = "fixture-local", Slot = 0 } };
        }

        internal void Map() { MapCalls++; }

        internal RecoveryProbeEvidence Run() => RecoveryProbe.Run(Trigger,
            () => new RecoveryProbeContext { Reader = ContextReader, ReaderOpenAttempts = OpenAttempts, Match = Match, Epoch = Epoch, Allowed = Allowed },
            Camera, Snapshot, () => Now, () => new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Now).ToString("O"),
            ms => { Slept += ms; if (!FrozenClock) Now += ms; }, () => Cancelled);
    }
}
