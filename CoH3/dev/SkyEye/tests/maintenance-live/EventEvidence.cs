using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

// Retains decoded public method outputs, never raw process memory or credentials.
internal sealed class EventEvidence
{
    internal const int MaximumFrames = 128;
    internal const int MaximumDecoded = 256;
    internal readonly List<object> Frames = new List<object>();
    internal long TotalDecoded;
    internal int NonzeroPolls;
    internal int RetainedDecoded;
    internal int DroppedFrames;
    internal long DroppedDecoded;

    internal void Add(List<ProjectileImpact> items, string utc, long startedMs, long finishedMs, string match, long epoch)
    {
        TotalDecoded += items.Count;
        if (items.Count == 0) return;
        NonzeroPolls++;
        int take = Math.Min(16, Math.Min(items.Count, MaximumDecoded - RetainedDecoded));
        if (Frames.Count >= MaximumFrames || take <= 0) { DroppedFrames++; DroppedDecoded += items.Count; return; }
        Frames.Add(new
        {
            utc, pollStartedMilliseconds = startedMs, readFinishedMilliseconds = finishedMs, match, epoch,
            decodedCount = items.Count,
            decoded = items.Take(take).Select(item => new
            {
                entity = "0x" + item.Entity.ToString("x"), item.Id, item.Index, item.Slot,
                sourceKey = "0x" + item.SourceKey.ToString("x"), item.Number, item.Name, item.Weapon, item.Label,
                argb = item.Colour.ToArgb().ToString("x8"), item.X, item.Y, item.Z, item.Radius
            }).ToArray(),
            droppedDecodedInThisPoll = items.Count - take
        });
        RetainedDecoded += take;
        DroppedDecoded += items.Count - take;
    }
}
