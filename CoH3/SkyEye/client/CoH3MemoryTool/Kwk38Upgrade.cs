using System;
using System.Linq;

namespace CoH3MemoryTool;

internal static class Kwk38Upgrade
{
	public static bool IsVehicle(string path)
	{
		if (path != null && path.StartsWith("ebps\\races\\afrika_korps\\vehicles\\", StringComparison.OrdinalIgnoreCase))
		{
			return path.Substring(path.LastIndexOf('\\') + 1).StartsWith("halftrack_250", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static bool IsResearch(string path)
	{
		return string.Equals(path, "upgrade\\afrika_korps\\vehicle\\20mm_halftrack_250_ak", StringComparison.OrdinalIgnoreCase);
	}

	public static string Status(PlayerInfoRow row)
	{
		if (!row.Kwk38Known || row.Kwk38Upgrading.Count == 0)
		{
			return "";
		}
		return "250/9升级：" + string.Join(" / ", from j in row.Kwk38Upgrading.OrderBy((ProductionJob j) => j.Key, StringComparer.Ordinal)
			select j.RemainingSeconds + "秒");
	}
}
