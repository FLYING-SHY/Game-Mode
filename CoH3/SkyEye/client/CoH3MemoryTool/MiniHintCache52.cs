namespace CoH3MemoryTool;

internal static class MiniHintCache52
{
	public static long Parse(string text, int pid, long started, long image)
	{
		string[] array = text.Trim().Split('|');
		if (array.Length != 4 || !int.TryParse(array[0], out var result) || !long.TryParse(array[1], out var result2) || !long.TryParse(array[2], out var result3) || !long.TryParse(array[3], out var result4) || result != pid || result2 != started || result3 != image || result4 < 65536 || result4 > 140737488355327L || result4 % 8 != 0)
		{
			return 0L;
		}
		return result4;
	}
}
