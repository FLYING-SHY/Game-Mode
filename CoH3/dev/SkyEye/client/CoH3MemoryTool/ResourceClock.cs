using System.Diagnostics;

namespace CoH3MemoryTool;

internal static class ResourceClock
{
	private static readonly Stopwatch clock = Stopwatch.StartNew();

	public static double Seconds => clock.Elapsed.TotalSeconds;
}
