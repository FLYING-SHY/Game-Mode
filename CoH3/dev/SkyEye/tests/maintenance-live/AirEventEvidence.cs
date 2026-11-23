using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

// All-sides maintenance diagnostics, not production enemy-only UI routing.
// Reserve two slots for first/latest successful empty reads; zero output is never PASS.
internal sealed class AirEventEvidence
{
    internal const int MaximumFrames = 128;
    internal const int MaximumDecoded = 256;
    internal readonly List<object> Frames = new List<object>();
    internal long TotalDecoded;
    internal int NonzeroPolls;
    internal int RetainedDecoded;
    internal int DroppedFrames;
    internal long DroppedDecoded;
    private bool hasFirstEmptyFrame;
    private object latestEmptyFrame;
    private int nonzeroFrames;

    internal void Add(List<object> items, string utc, long startedMs, long finishedMs, string match, long epoch)
    {
        TotalDecoded += items.Count;
        if (items.Count > 0) NonzeroPolls++;
        int take = Math.Min(16, Math.Min(items.Count, MaximumDecoded - RetainedDecoded));
        if (items.Count > 0 && nonzeroFrames >= MaximumFrames - 2) { DroppedFrames++; DroppedDecoded += items.Count; return; }
        var frame = new
        {
            utc, pollStartedMilliseconds = startedMs, readFinishedMilliseconds = finishedMs, match, epoch,
            decodedCount = items.Count, decoded = items.Take(take).ToArray(),
            droppedDecodedInThisPoll = items.Count - take
        };
        if (items.Count == 0)
        {
            if (!hasFirstEmptyFrame) hasFirstEmptyFrame = true;
            else
            {
                if (latestEmptyFrame != null) { Frames.Remove(latestEmptyFrame); DroppedFrames++; }
                latestEmptyFrame = frame;
            }
        }
        else nonzeroFrames++;
        // Removing the previous latest empty before appending keeps retained polls in time order.
        Frames.Add(frame);
        RetainedDecoded += take;
        DroppedDecoded += items.Count - take;
    }
}
