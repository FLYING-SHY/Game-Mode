using System;

namespace CoH3MemoryTool;

internal static class EventPolling
{
    internal static bool ParseSeconds(string value, out int seconds) => int.TryParse(value, out seconds) && seconds >= 1 && seconds <= 60;

    // Cooperative budget: a synchronous query is never forcibly interrupted.
    internal static string Run(int seconds, Func<bool> sample, Func<long> elapsed, Action<int> sleep, Func<bool> cancelled, out int rounds)
    {
        if (seconds < 1 || seconds > 60) throw new ArgumentOutOfRangeException(nameof(seconds));
        rounds = 0;
        long budget = seconds * 1000L;
        while (rounds < seconds * 10 + 1)
        {
            if (cancelled()) return "cancelled";
            long started = elapsed();
            if (started >= budget) return "deadline";
            rounds++;
            if (!sample()) return "stopped";
            if (cancelled()) return "cancelled";
            long now = elapsed();
            if (now >= budget) return "deadline";
            int delay = (int)Math.Min(budget - now, Math.Max(0, 100 - (now - started)));
            if (delay > 0) sleep(delay);
        }
        return "round_cap";
    }
}
