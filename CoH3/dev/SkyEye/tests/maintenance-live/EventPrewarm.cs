using System;

namespace CoH3MemoryTool;

internal static class EventPrewarm
{
    internal static bool ParseMilliseconds(string value, out int milliseconds) =>
        int.TryParse(value, out milliseconds) && milliseconds >= 0 && milliseconds <= 5000;

    internal static bool Wait(int milliseconds, Action guard, Func<long> elapsedMilliseconds,
        Action<int> sleep, Func<bool> cancelled)
    {
        if (milliseconds < 0 || milliseconds > 5000) throw new ArgumentOutOfRangeException(nameof(milliseconds));
        for (int round = 0; round < 101; round++)
        {
            guard();
            if (cancelled()) return false;
            long remaining = milliseconds - elapsedMilliseconds();
            if (remaining <= 0) return true;
            sleep((int)Math.Min(50, remaining));
        }
        throw new InvalidOperationException("Prewarm delay did not finish within101 cooperative wait rounds; do not start event reads.");
    }
}
