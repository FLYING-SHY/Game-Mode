namespace CoH3MemoryTool;

internal static class LoiterWeaponCatalogue49
{
	public static bool Shape(string path, out float length, out float width)
	{
		length = (width = 0f);
		switch (path)
		{
		case "weapon\\afrika_korps\\small_arms\\machine_gun\\heavy_machine_gun\\stuka_anti_vehicle_strafe_ak":
			length = 45f;
			width = 5f;
			return true;
		case "weapon\\afrika_korps\\small_arms\\machine_gun\\heavy_machine_gun\\stuka_combined_arms_strafe_ak":
			length = 45f;
			width = 5f;
			return true;
		case "weapon\\german\\small_arms\\machine_gun\\heavy_machine_gun\\30mm_ju87_ger":
			length = 45f;
			width = 5f;
			return true;
		case "weapon\\german\\small_arms\\machine_gun\\heavy_machine_gun\\stuka_strafing_run":
			length = 45f;
			width = 5f;
			return true;
		case "weapon\\german\\small_arms\\machine_gun\\heavy_machine_gun\\stuka_strafing_run_foggia_ger":
			length = 60f;
			width = 5f;
			return true;
		case "weapon\\german\\small_arms\\machine_gun\\heavy_machine_gun\\stuka_strafing_run_jericho_trumpet_ger":
			length = 45f;
			width = 5f;
			return true;
		default:
			return false;
		}
	}
}
