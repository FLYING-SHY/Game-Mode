namespace CoH3MemoryTool;

internal static class LicenseArray60
{
	public static bool Contains60(this string[] source, string value)
	{
		foreach (string text in source)
		{
			if (text == value)
			{
				return true;
			}
		}
		return false;
	}
}
