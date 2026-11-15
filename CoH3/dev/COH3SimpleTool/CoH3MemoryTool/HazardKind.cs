namespace CoH3MemoryTool;

internal static class HazardKind
{
	public static bool Demolition(string path)
	{
		string text = path.Substring(path.LastIndexOf('\\') + 1);
		if (!text.StartsWith("demo_charge_") && !text.StartsWith("demolition_charge_"))
		{
			return text.Contains("_demo_charge_");
		}
		return true;
	}

	public static bool Supported(string path)
	{
		if (!path.StartsWith("ebps\\races\\") || !path.Contains("\\buildings\\defensive\\"))
		{
			return false;
		}
		string text = path.Substring(path.LastIndexOf('\\') + 1);
		if (!text.Contains("mine") && !text.Contains("booby_trap"))
		{
			return Demolition(path);
		}
		return true;
	}
}
