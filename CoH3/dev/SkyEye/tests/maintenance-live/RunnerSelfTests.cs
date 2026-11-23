using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.Script.Serialization;

namespace CoH3MemoryTool;

internal static class RunnerSelfTests
{
    internal static int Run()
    {
        int passed = 0;
        Action<bool, string> check = (condition, name) => { if (!condition) throw new InvalidOperationException("FAIL " + name); Console.WriteLine("PASS " + name); passed++; };
        try
        {
            long now = 0;
            int calls = 0;
            int attempts;
            bool found = BoundedPoll.Until(() => ++calls == 3, () => now, delay => now += delay, out attempts);
            check(found && calls == 3 && attempts == 3 && now == 100, "initial asynchronous false is retried, then succeeds");
            now = 0; calls = 0;
            found = BoundedPoll.Until(() => { calls++; return false; }, () => now, delay => now += delay, out attempts);
            check(!found && now == 5000 && calls == 100 && attempts == 100, "absent discovery stops at 5 second budget");
            found = BoundedPoll.Until(() => { calls++; return false; }, () => 0, delay => { }, out attempts);
            check(!found && attempts == 101, "frozen clock still stops at 101 rounds");
            calls = 0;
            found = BoundedPoll.Until(() => { calls++; return true; }, () => 5000, delay => { throw new Exception("unexpected sleep"); }, out attempts);
            check(!found && calls == 0 && attempts == 0, "already expired budget starts no query");
            now = 4999; int slept = 0;
            found = BoundedPoll.Until(() => false, () => now, delay => { slept += delay; now += delay; }, out attempts);
            check(!found && slept == 1 && attempts == 1, "sleep never extends beyond remaining budget");
            var zero = new LiveProbeResult { Status = "pending" };
            MaintenanceLiveContext.Count(zero, 0);
            check(zero.Status == "sample_absent" && zero.SampleCount == 0, "zero event count is sample_absent");
            var observed = new LiveProbeResult { Status = "pending" };
            MaintenanceLiveContext.Count(observed, 4);
            check(observed.Status == "observed" && observed.SampleCount == 4, "positive count observed without whole feature pass");
            check(BoundedPoll.ExitCodeFor(new[] { zero }) == 5, "sample absent produces incomplete exit code");
            check(BoundedPoll.ExitCodeFor(new[] { new LiveProbeResult { Status = "not_run" } }) == 5, "not_run is incomplete");
            check(BoundedPoll.ExitCodeFor(new[] { new LiveProbeResult { Status = "discovery_timeout" } }) == 5, "discovery timeout is inconclusive not layout failure");
            check(BoundedPoll.ExitCodeFor(new[] { zero, new LiveProbeResult { Status = "fail" } }) == 3, "failure outranks missing samples");
            check(BoundedPoll.ExitCodeFor(new[] { new LiveProbeResult { Status = "blocked" } }) == 3, "dependency failure remains failure");
            check(BoundedPoll.ExitCodeFor(new[] { observed }) == 0, "observed-only aggregation");
            check(BoundedPoll.ExitCodeFor(new List<LiveProbeResult>()) == 3, "empty result set fails");
            var json = new JavaScriptSerializer().Serialize(new LiveProbeResult { Feature = "fixture", Status = "sample_absent", SampleCount = 0, Evidence = new { remainingTicks = 80, remainingSeconds = 10, tickRate = 8, view = new[] { 1f, 0f }, local = true } });
            check(json.Contains("\"remainingTicks\":80") && json.Contains("\"remainingSeconds\":10") && json.Contains("\"SampleCount\":0") && json.Contains("\"local\":true"), "granular evidence survives report serialization");
            var marker = new SquadMarkerSource { Squad = 42, Identity = 7, Path = "sbps\\races\\german\\infantry\\jaeger_ger", Slot = 1 };
            check(!marker.ColourKnown, "new snapshot marker source defaults to unknown colour");
            var knownRow = new PlayerInfoRow { GameColourKnown = true, GameColour = Color.FromArgb(255, 23, 45, 67) };
            knownRow.MarkerSquads.Add(marker);
            var unknownRow = new PlayerInfoRow { GameColourKnown = false, GameColour = Color.FromArgb(255, 90, 80, 70) };
            var unknownMarker = new SquadMarkerSource { ColourKnown = true, Colour = Color.Red };
            unknownRow.MarkerSquads.Add(unknownMarker);
            MaintenanceLiveContext.BindMarkerColours(new[] { knownRow, unknownRow });
            check(marker.ColourKnown && marker.Colour.ToArgb() == knownRow.GameColour.ToArgb() && marker.Squad == 42 && marker.Identity == 7, "colour binding enables known sources with exact row colour without changing identity");
            check(!unknownMarker.ColourKnown && unknownMarker.Colour.ToArgb() == unknownRow.GameColour.ToArgb(), "unknown row colour remains unknown and clears stale known flag");
            knownRow.GameColourKnown = false;
            MaintenanceLiveContext.BindMarkerColours(new[] { knownRow });
            check(!marker.ColourKnown, "known to unknown transition never retains stale marker colour eligibility");
            check(MaintenanceLiveContext.SampleInfantryMovement, "maintenance marker sampling explicitly opts infantry into movement reads");
            Console.WriteLine("SELF_TEST_ONLY; no coordinator, credential store, game discovery, process open, or attach. passed=" + passed);
            return 0;
        }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }
}
