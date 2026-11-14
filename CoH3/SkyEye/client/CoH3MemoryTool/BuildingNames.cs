using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal static class BuildingNames
{
	private static readonly Dictionary<string, int> tiers = new Dictionary<string, int>
	{
		{ "infanterie_kompanie_ger", 1 },
		{ "support_armory_ger", 3 },
		{ "mechanized_kompanie_ger", 2 },
		{ "panzer_armory_ger", 4 },
		{ "infanterie_support_ak", 1 },
		{ "mechanized_kompanie_ak", 2 },
		{ "panzer_kompanie_ak", 3 },
		{ "barracks_us", 1 },
		{ "weapon_support_center_us", 2 },
		{ "motor_pool_us", 3 },
		{ "tank_depot_us", 4 },
		{ "barracks_africa_uk", 1 },
		{ "motor_platoon_africa_uk", 2 },
		{ "armoured_platoon_africa_uk", 3 },
		{ "barracks_uk", 1 },
		{ "motor_platoon_uk", 2 },
		{ "armoured_platoon_uk", 3 }
	};

	public static int Tier(string path)
	{
		if (!tiers.TryGetValue(path.Substring(path.LastIndexOf('\\') + 1), out var value))
		{
			return 0;
		}
		return value;
	}

	public static string TierName(int tier)
	{
		return (new string[5] { "", "一本", "二本", "三本", "四本" })[Math.Max(0, Math.Min(4, tier))];
	}

	public static string Summary(PlayerInfoRow row)
	{
		if (!row.BuildingsKnown)
		{
			return "基地未知";
		}
		int count = ((row.Race == "afrika_korps" || row.Race == "british" || row.Race == "british_africa") ? 3 : 4);
		return string.Join(" ", from t in Enumerable.Range(1, count)
			where row.Buildings.Any((TrackedBuilding b) => b.Tier == t)
			select TierName(t) + (row.Buildings.Any((TrackedBuilding b) => b.Tier == t && b.Complete) ? "√" : "ing"));
	}
}
