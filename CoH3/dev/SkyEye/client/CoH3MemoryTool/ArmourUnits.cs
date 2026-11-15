using System;
using System.Linq;

namespace CoH3MemoryTool;

internal static class ArmourUnits
{
	private static readonly string[] families = "armored_car archer bishop bison brummbar centaur chaffee char_b1 churchill command_churchill crusader cwt_15_quad_mount elefant grant greyhound halftrack_75mm halftrack_assault halftrack_quad halftrack_251_stuka halftrack_251_flak halftrack_251_mortar halftrack_7_flak halftrack_flame halftrack_stummel hellcat humber jagdpanzer king_tiger l6_40 m13_40 marder marmon_herrington matilda nashorn panther panzer_i panzer_ii panzer_iii panzer_iv panzerjager pershing pheasant_halftrack scott semovente sherman stug stuart sturmtiger tiger valentine wespe wirbelwind".Split(' ');

	public static bool Includes(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		string text = path.Replace('/', '\\').ToLowerInvariant();
		if (!text.StartsWith("sbps\\races\\") || !text.Contains("\\vehicles\\"))
		{
			return false;
		}
		string name = text.Substring(text.LastIndexOf('\\') + 1);
		if (name.EndsWith(".rgd"))
		{
			name = name.Substring(0, name.Length - 4);
		}
		return families.Any((string f) => name == f || name.StartsWith(f + "_", StringComparison.Ordinal));
	}
}
