using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace CoH3MemoryTool;

internal static class AirEventSelfTests
{
    private sealed class Frame
    {
        public Frame() { }
        public long pollStartedMilliseconds { get; set; }
        public int decodedCount { get; set; }
        public object[] decoded { get; set; }
        public int droppedDecodedInThisPoll { get; set; }
    }

    private static Frame[] ReadFrames(AirEventEvidence evidence)
    {
        var serializer = new JavaScriptSerializer();
        return serializer.Deserialize<Frame[]>(serializer.Serialize(evidence.Frames));
    }

    private static bool AccountsFor(AirEventEvidence evidence, int calls)
    {
        var frames = ReadFrames(evidence);
        return calls == frames.Length + evidence.DroppedFrames && frames.Length <= AirEventEvidence.MaximumFrames &&
            evidence.TotalDecoded == evidence.RetainedDecoded + evidence.DroppedDecoded && evidence.RetainedDecoded <= AirEventEvidence.MaximumDecoded &&
            frames.Sum(frame => frame.decoded.Length) == evidence.RetainedDecoded &&
            frames.All(frame => frame.decoded.Length <= 16 && frame.decodedCount == frame.decoded.Length + frame.droppedDecodedInThisPoll) &&
            frames.Count(frame => frame.decodedCount == 0) <= 2 && frames.Count(frame => frame.decodedCount > 0) <= AirEventEvidence.MaximumFrames - 2 &&
            frames.Select(frame => frame.pollStartedMilliseconds).SequenceEqual(frames.Select(frame => frame.pollStartedMilliseconds).OrderBy(time => time));
    }

    internal static int Run()
    {
        int passed = 0;
        Action<bool, string> check = (condition, name) => { if (!condition) throw new InvalidOperationException("FAIL " + name); passed++; Console.WriteLine("PASS " + name); };
        try
        {
            var rows = new[]
            {
                new PlayerInfoRow { Slot = 1, Relation = "自己" },
                new PlayerInfoRow { Slot = 2, Relation = "友方" },
                new PlayerInfoRow { Slot = 3, Relation = "敌方" }
            };
            check(MaintenanceLiveContext.EventPlayers(rows, false).Select(p => p.Slot).SequenceEqual(new uint[] { 3 }), "projectile routing remains enemy-only");
            check(MaintenanceLiveContext.EventPlayers(rows, true).Select(p => p.Slot).SequenceEqual(new uint[] { 1, 2, 3 }), "air diagnostic routing includes local allied enemy current rows");
            var empty = new AirEventEvidence();
            empty.Add(new List<object>(), "utc", 0, 1, "match", 7);
            check(empty.TotalDecoded == 0 && empty.NonzeroPolls == 0 && empty.Frames.Count == 1, "successful zero air output retains empty frame not a gameplay PASS");
            object zone = new { Slot = 1, Name = "fixture", X = 1f, Y = 2f, Z = 3f, DX = 4f, DZ = 5f, HalfWidth = 6f, Length = 7f, Radius = 8f, Rocket = false, Point = true };
            var leading = new AirEventEvidence();
            for (int i = 0; i < 400; i++) leading.Add(new List<object>(), "utc", i, i + 1, "match", 7);
            check(leading.Frames.Count == 2 && leading.DroppedFrames == 398 && leading.RetainedDecoded == 0 && leading.DroppedDecoded == 0, "long leading empties retain only first and latest and count replacements");
            for (int i = 400; i < 483; i++) leading.Add(new List<object> { zone }, "utc", i, i + 1, "match", 7);
            for (int i = 483; i < 551; i++) leading.Add(new List<object>(), "utc", i, i + 1, "match", 7);
            check(leading.TotalDecoded == 83 && leading.NonzeroPolls == 83 && leading.RetainedDecoded == 83 && leading.Frames.Count == 85 && leading.DroppedFrames == 466 && leading.DroppedDecoded == 0, "551 polls with83 late positives retain actual decoded geometry after leading empties");
            check(ReadFrames(leading).Select(frame => frame.pollStartedMilliseconds).SequenceEqual(new long[] { 0 }.Concat(Enumerable.Range(400, 83).Select(i => (long)i)).Concat(new long[] { 550 })), "leading positive trailing sequence keeps first latest empty and chronological positives");
            var interleaved = new AirEventEvidence();
            for (int i = 0; i < 5; i++) interleaved.Add(i % 2 == 0 ? new List<object>() : new List<object> { zone }, "utc", i, i + 1, "match", 7);
            check(interleaved.DroppedFrames == 1 && ReadFrames(interleaved).Select(frame => frame.pollStartedMilliseconds).SequenceEqual(new long[] { 0, 1, 3, 4 }), "latest empty replacement preserves interleaved positive time order");
            var capped = new AirEventEvidence();
            for (int i = 0; i < 17; i++) capped.Add(Enumerable.Repeat(zone, 20).ToList(), "utc", i * 100, i * 100 + 1, "match", 7);
            check(capped.TotalDecoded == 340 && capped.RetainedDecoded == 256 && capped.Frames.Count == 17 && capped.DroppedDecoded == 84 && capped.DroppedFrames == 0, "air decoded cap retains frame shape with exact decoded drops");
            var cappedFrames = ReadFrames(capped);
            check(cappedFrames.Take(16).All(frame => frame.decoded.Length == 16 && frame.droppedDecodedInThisPoll == 4) && cappedFrames[16].decoded.Length == 0 && cappedFrames[16].decodedCount == 20 && cappedFrames[16].droppedDecodedInThisPoll == 20, "per-frame16 and total256 caps preserve decodedCount after geometry budget exhausted");
            var frames = new AirEventEvidence();
            for (int i = 0; i < 130; i++) frames.Add(new List<object> { zone }, "utc", i, i + 1, "match", 7);
            check(frames.Frames.Count == 126 && frames.RetainedDecoded == 126 && frames.DroppedFrames == 4 && frames.DroppedDecoded == 4 && frames.TotalDecoded == 130, "positive frame cap reserves two empty slots with exact drops");
            for (int i = 130; i < 133; i++) frames.Add(new List<object>(), "utc", i, i + 1, "match", 7);
            check(frames.Frames.Count == 128 && frames.DroppedFrames == 5 && frames.RetainedDecoded == 126 && frames.DroppedDecoded == 4, "first and latest empty slots remain available after positive saturation");
            check(ReadFrames(frames).Select(frame => frame.pollStartedMilliseconds).SequenceEqual(Enumerable.Range(0, 126).Select(i => (long)i).Concat(new long[] { 130, 132 })), "positive saturation retains earliest positives and first latest late empty frames");
            var emptyFrames = new AirEventEvidence();
            for (int i = 0; i < 130; i++) emptyFrames.Add(new List<object>(), "utc", i, i + 1, "match", 7);
            check(emptyFrames.Frames.Count == 2 && emptyFrames.DroppedFrames == 128 && emptyFrames.DroppedDecoded == 0 && emptyFrames.NonzeroPolls == 0, "empty-only window has two frames and exact replacement count");
            check(ReadFrames(emptyFrames).Select(frame => frame.pollStartedMilliseconds).SequenceEqual(new long[] { 0, 129 }), "empty-only window retains first and latest timestamps");
            var mixed = new AirEventEvidence();
            for (int i = 0; i < 400; i++) mixed.Add(new List<object>(), "utc", i, i + 1, "match", 7);
            for (int i = 400; i < 417; i++) mixed.Add(Enumerable.Repeat(zone, 20).ToList(), "utc", i, i + 1, "match", 7);
            for (int i = 417; i < 420; i++) mixed.Add(new List<object>(), "utc", i, i + 1, "match", 7);
            check(mixed.TotalDecoded == 340 && mixed.RetainedDecoded == 256 && mixed.DroppedDecoded == 84 && mixed.Frames.Count == 19 && mixed.DroppedFrames == 401, "empty replacements and decoded truncation use independent exact ledgers");
            string json = new JavaScriptSerializer().Serialize(capped.Frames[0]);
            check(json.Contains("\"HalfWidth\":6") && json.Contains("\"Point\":true") && json.Contains("\"epoch\":7") && json.Contains("\"pollStartedMilliseconds\":0") && json.Contains("\"droppedDecodedInThisPoll\":4"), "air structural fields timing epoch and per-frame drops serialize without schema changes");
            check(AccountsFor(empty, 1) && AccountsFor(leading, 551) && AccountsFor(interleaved, 5) && AccountsFor(capped, 17) && AccountsFor(frames, 133) && AccountsFor(emptyFrames, 130) && AccountsFor(mixed, 420), "all fixtures satisfy frame decoded accounting bounds and chronological order");
            var ledger = new AirEventEvidence();
            bool exactAtEveryPoll = true;
            for (int i = 0; i < 600; i++)
            {
                ledger.Add(i % 11 == 0 ? Enumerable.Repeat(zone, 20).ToList() : i > 400 ? new List<object> { zone } : new List<object>(), "utc", i, i + 1, "match", 7);
                exactAtEveryPoll &= AccountsFor(ledger, i + 1);
            }
            check(exactAtEveryPoll, "600 mixed polls satisfy accounting and all bounds after every add");
            Console.WriteLine("AIR_EVENT_SELF_TEST_ONLY passed=" + passed + "; synthetic routing/retention only; no game, coordinator, credentials, auth grant or GUI.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
