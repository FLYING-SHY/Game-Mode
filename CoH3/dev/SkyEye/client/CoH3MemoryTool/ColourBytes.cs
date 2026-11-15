namespace CoH3MemoryTool;

internal static class ColourBytes
{
	public static bool AsSpanEqual(this byte[] a, byte[] b)
	{
		if (a.Length == b.Length && a[0] == b[0])
		{
			return a[1] == b[1];
		}
		return false;
	}
}
