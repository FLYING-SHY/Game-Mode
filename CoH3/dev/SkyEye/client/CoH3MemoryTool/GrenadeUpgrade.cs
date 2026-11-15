using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal static class GrenadeUpgrade
{
	private static readonly HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "upgrade\\american\\research\\barracks\\grenade_riflemen_us", "upgrade\\british\\research\\barracks\\grenade_tommy_uk", "upgrade\\british\\research\\barracks\\rifle_grenade_tommy_uk" };

	public static bool IsResearch(string path)
	{
		if (path != null)
		{
			return paths.Contains(path.Replace('/', '\\'));
		}
		return false;
	}

	public static int SecondsRemaining(float ticks)
	{
		if (float.IsNaN(ticks) || float.IsInfinity(ticks) || ticks < 0f || ticks > 80000f)
		{
			throw new ArgumentOutOfRangeException("ticks");
		}
		return (int)Math.Ceiling((double)ticks / 8.0);
	}

	public static string Status(PlayerInfoRow row)
	{
		if (Completed(row))
		{
			return "有雷";
		}
		if (!row.ProductionKnown || !row.GrenadeRemainingTicks.HasValue)
		{
			return "";
		}
		return "手雷解锁中 " + SecondsRemaining(row.GrenadeRemainingTicks.Value) + "秒";
	}

	public static bool Completed(PlayerInfoRow row)
	{
		return row.Upgrades.Any((string p) => paths.Contains(p.Replace('/', '\\')));
	}
}
