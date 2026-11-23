using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace CoH3MemoryTool;

internal static class EventPollingSelfTests
{
    internal static int Run()
    {
        int passed = 0;
        Action<bool, string> check = (condition, name) => { if (!condition) throw new InvalidOperationException("FAIL " + name); passed++; Console.WriteLine("PASS " + name); };
        try
        {
            check(EventPolling.ParseSeconds("1", out int first) && first == 1 && EventPolling.ParseSeconds("60", out int last) && last == 60, "event duration accepts endpoints 1 and60");
            check(new[] { "0", "-1", "61", "abc", "2147483648" }.All(value => !EventPolling.ParseSeconds(value, out _)), "explicit zero/out-of-range/malformed duration rejected before auth");
            long now = 0; int calls = 0, rounds;
            string reason = EventPolling.Run(1, () => { calls++; return true; }, () => now, ms => now += ms, () => false, out rounds);
            check(reason == "deadline" && calls == 10 && rounds == 10 && now == 1000, "one second window runs ten100ms polls and stops at deadline");
            now = 0; calls = 0;
            reason = EventPolling.Run(1, () => { calls++; now += 35; return true; }, () => now, ms => now += ms, () => false, out rounds);
            check(reason == "deadline" && calls == 10 && now == 1000, "cadence subtracts query latency instead of adding100ms");
            calls = 0;
            reason = EventPolling.Run(1, () => { calls++; return true; }, () => 0, ms => { }, () => false, out rounds);
            check(reason == "round_cap" && calls == 11 && rounds == 11, "frozen clock hard round cap bounds polling");
            calls = 0;
            reason = EventPolling.Run(1, () => { calls++; return true; }, () => 0, ms => { }, () => true, out rounds);
            check(reason == "cancelled" && calls == 0 && rounds == 0, "pre-cancelled window starts no read");
            now = 0; calls = 0; bool cancelled = false;
            reason = EventPolling.Run(1, () => { calls++; cancelled = calls == 2; return true; }, () => now, ms => now += ms, () => cancelled, out rounds);
            check(reason == "cancelled" && calls == 2 && rounds == 2 && now == 100, "cancellation after query stops before sleep or another read");
            now = 0; int sleeps = 0;
            reason = EventPolling.Run(1, () => { now = 1200; return true; }, () => now, ms => sleeps++, () => false, out rounds);
            check(reason == "deadline" && rounds == 1 && sleeps == 0 && now == 1200, "slow synchronous read overruns cooperatively without another read");
            now = 999; int slept = 0;
            reason = EventPolling.Run(1, () => true, () => now, ms => { slept += ms; now += ms; }, () => false, out rounds);
            check(reason == "deadline" && slept == 1 && rounds == 1, "remaining budget clamps final sleep to1ms");
            reason = EventPolling.Run(1, () => false, () => 0, ms => { throw new Exception("unexpected sleep"); }, () => false, out rounds);
            check(reason == "stopped" && rounds == 1, "between-read stop prevents another poll");
            var sentinel = new InvalidOperationException("fixture epoch lost"); bool propagated = false;
            try { EventPolling.Run(1, () => { throw sentinel; }, () => 0, ms => { }, () => false, out rounds); }
            catch (InvalidOperationException ex) { propagated = ReferenceEquals(ex, sentinel); }
            check(propagated, "lease/dependency callback exception is propagated not absence");
            var empty = new EventEvidence(); empty.Add(new List<ProjectileImpact>(), "utc", 0, 1, "match", 7);
            check(empty.TotalDecoded == 0 && empty.Frames.Count == 0 && empty.NonzeroPolls == 0, "zero outputs do not fabricate retained event frames");
            var item = new ProjectileImpact { Entity = 0x42, Id = 2, Index = 3, SourceKey = 7, Slot = 1, Number = 4, Name = "mortar", Weapon = "weapon", X = 1, Y = 2, Z = 3, Radius = 4.5f };
            var capped = new EventEvidence();
            for (int i = 0; i < 17; i++) capped.Add(Enumerable.Repeat(item, 20).ToList(), "utc", i * 100, i * 100 + 1, "match", 7);
            check(capped.TotalDecoded == 340 && capped.RetainedDecoded == 256 && capped.Frames.Count == 16 && capped.DroppedDecoded == 84 && capped.DroppedFrames == 1, "decoded retention cap keeps exact dropped counters");
            var frameCapped = new EventEvidence();
            for (int i = 0; i < 256; i++) frameCapped.Add(new List<ProjectileImpact> { item }, "utc", i * 100, i * 100 + 1, "match", 7);
            check(frameCapped.Frames.Count == 128 && frameCapped.RetainedDecoded == 128 && frameCapped.DroppedFrames == 128 && frameCapped.DroppedDecoded == 128, "frame retention cap remains bounded independently of decoded cap");
            string json = new JavaScriptSerializer().Serialize(capped.Frames[0]);
            check(json.Contains("\"entity\":\"0x42\"") && json.Contains("\"sourceKey\":\"0x7\"") && json.Contains("\"Radius\":4.5") && json.Contains("\"epoch\":7") && json.Contains("\"pollStartedMilliseconds\":0"), "decoded identity/source/coordinates/epoch/timing serialize without reconstruction");
            check(BoundedPoll.ExitCodeFor(new[] { new LiveProbeResult { Status = "cancelled" } }) == 5, "cancelled event classification is incomplete exit5");
            Console.WriteLine("EVENT_SELF_TEST_ONLY passed=" + passed + "; virtual clock/data only; original20tests not rerun; no coordinator, credential store, process discovery, attach or GUI.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
