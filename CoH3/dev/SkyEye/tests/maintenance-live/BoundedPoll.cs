using System;

namespace CoH3MemoryTool;

internal static class BoundedPoll
{
    internal static bool Until(Func<bool> query, Func<long> elapsedMilliseconds, Action<int> sleep, out int attempts)
    {
        attempts = 0;
        while (attempts < 101 && elapsedMilliseconds() < 5000)
        {
            attempts++;
            if (query()) return true;
            long remaining = 5000 - elapsedMilliseconds();
            if (remaining <= 0) break;
            sleep((int)Math.Min(50, remaining));
        }
        return false;
    }

    internal static int ExitCodeFor(System.Collections.Generic.IEnumerable<LiveProbeResult> results)
    {
        bool any = false;
        bool incomplete = false;
        foreach (var result in results)
        {
            any = true;
            if (result.Status == "fail" || result.Status == "blocked") return 3;
            if (result.Status != "observed" && result.Status != "pass") incomplete = true;
        }
        return !any ? 3 : incomplete ? 5 : 0;
    }
}
