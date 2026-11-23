using System.Collections.Generic;

namespace CoH3MemoryTool;

internal sealed class LiveProbeResult
{
    public string Feature { get; set; }
    public string Status { get; set; }
    public string BuildStatus { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public string StartedUtc { get; set; }
    public int? SampleCount { get; set; }
    public object Evidence { get; set; }
    public List<string> Details { get; set; } = new List<string>();
    public string Error { get; set; }
}
