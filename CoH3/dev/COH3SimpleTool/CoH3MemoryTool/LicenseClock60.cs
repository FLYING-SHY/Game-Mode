using System.Runtime.InteropServices;

namespace CoH3MemoryTool;

internal static class LicenseClock60
{
	public static long Milliseconds => checked((long)GetTickCount64());

	[DllImport("kernel32.dll")]
	private static extern ulong GetTickCount64();
}
