namespace CoH3MemoryTool;

internal static class SupportSchedule49
{
	public static double Seconds(string path)
	{
		if (!path.Contains("obice_barrage"))
		{
			if (!path.Contains("naval_bombardment"))
			{
				return 60.0;
			}
			return 13.5;
		}
		return 34.0;
	}
}
