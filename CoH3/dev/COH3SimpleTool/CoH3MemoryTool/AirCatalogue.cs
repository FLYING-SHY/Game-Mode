using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal static class AirCatalogue
{
	private static readonly Dictionary<string, AirDefinition> list;

	private static void Add(string path, string name, float width, float length, float offset, int count, params string[] aircraft)
	{
		list.Add("abilities\\races\\" + path, new AirDefinition
		{
			Path = "abilities\\races\\" + path,
			Name = name,
			HalfWidth = width,
			Length = length,
			Offset = offset,
			Planes = count,
			Aircraft = aircraft
		});
	}

	static AirCatalogue()
	{
		list = new Dictionary<string, AirDefinition>(StringComparer.Ordinal);
		Add("german\\battlegroups\\luftwaffe\\luftwaffe_right_1b_strafing_run_ger", "斯图卡扫射巡航", 5f, 42f, 0f, 1, "stuka_ju87_strafe");
		Add("german\\battlegroups\\luftwaffe\\luftwaffe_right_3a_fragmentation_bombs_ger", "破片炸弹", 12f, 23f, 7f, 2, "stuak_ju87_dive_bomber");
		Add("afrika_korps\\battlegroups\\italian_combined_arms\\left\\italian_combined_arms_vehicle_strafe_ak", "扫射巡航", 5f, 47f, 0f, 2, "stuka_ju87_combined_arms_strafe");
		Add("afrika_korps\\battlegroups\\elite_forces\\elite_forces_stuka_ace_rank_1_ak", "王牌中队", 3f, 47f, 0f, 1, "stuka_ace_ju87_strafe_ak");
		Add("afrika_korps\\battlegroups\\elite_forces\\elite_forces_stuka_ace_rank_2_ak", "王牌中队", 7f, 47f, 0f, 2, "stuka_ace_ju87_strafe_ak");
		Add("afrika_korps\\battlegroups\\elite_forces\\elite_forces_stuka_ace_rank_3_ak", "王牌中队", 7f, 47f, 0f, 4, "stuka_ace_ju87_strafe_ak");
		Add("american\\aircraft\\strafing_run_p47_us", "P47扫射", 5f, 45f, 0f, 1, "p47_thunderbolt_air_support_center_us");
		Add("american\\aircraft\\strafing_run_p47_double_us", "P47双机扫射", 7f, 45f, 0f, 2, "p47_thunderbolt_air_support_center_us");
		list["abilities\\races\\american\\aircraft\\strafing_run_p47_double_us"].Lateral = 2f;
		Add("american\\aircraft\\dive_bombing_run_p47_us", "P47轰炸目标", 0f, 0f, 0f, 1, "p47_thunderbolt_dive_bomber_us");
		Add("american\\aircraft\\dive_bombing_run__double_p47_us", "P47双机轰炸目标", 0f, 0f, 0f, 2, "p47_thunderbolt_dive_bomber_us");
		list["abilities\\races\\american\\aircraft\\dive_bombing_run_p47_us"].Point = true;
		list["abilities\\races\\american\\aircraft\\dive_bombing_run__double_p47_us"].Point = true;
	}

	public static AirDefinition Find(string path)
	{
		if (!list.TryGetValue(path ?? "", out var value))
		{
			return null;
		}
		return value;
	}

	public static bool V1(string path)
	{
		return path == "ebps\\gameplay\\projectiles\\races\\german\\p_artillery_off_map_rocket_v1";
	}

	public static bool Aircraft(string path)
	{
		if (!Loiter49.Supported(path))
		{
			if (path.StartsWith("ebps\\races\\", StringComparison.Ordinal) && path.Contains("\\aircraft\\"))
			{
				return list.Values.Any((AirDefinition d) => d.Aircraft.Any((string n) => path.EndsWith("\\" + n, StringComparison.Ordinal)));
			}
			return false;
		}
		return true;
	}
}
