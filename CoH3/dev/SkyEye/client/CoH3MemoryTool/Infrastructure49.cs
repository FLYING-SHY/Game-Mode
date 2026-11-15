using System.Linq;

namespace CoH3MemoryTool;

internal static class Infrastructure49
{
	public static string Name(string path)
	{
		if (string.IsNullOrEmpty(path) || (!path.StartsWith("ebps\\races\\") && !path.StartsWith("sbps\\races\\")))
		{
			return null;
		}
		string text = path.Substring(path.LastIndexOf('\\') + 1);
		if (new string[7] { "radio_beacon_uk", "siphon_radio_beacon_ak", "intel_radio_beacon_ak", "dak_bg04_beacon_acc_ak", "radio_beacon_pathfinders_us", "radio_beacon_paratroopers_us", "artillery_radio_beacon_us" }.Contains(text))
		{
			return UserUnitNames56.Resolve(path, "信标");
		}
		if (new string[3] { "partisan_tunnel_us", "partisan_tunnel_main_us", "partisan_tunnel_constructed_us" }.Contains(text))
		{
			return UserUnitNames56.Resolve(path, "隧道");
		}
		if (!(text == "propaganda_tower_ger"))
		{
			return null;
		}
		return UserUnitNames56.Resolve(path, "喇叭");
	}
}
