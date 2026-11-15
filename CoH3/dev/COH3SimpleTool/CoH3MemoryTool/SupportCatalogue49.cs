using System;

namespace CoH3MemoryTool;

internal static class SupportCatalogue49
{
	public const string Root = "abilities\\races\\afrika_korps\\battlegroups\\";

	public static string Name(string p)
	{
		return p switch
		{
			"abilities\\races\\afrika_korps\\battlegroups\\kriegsmarine\\kriegsmarine_naval_bombardment_ak" => "海军介入", 
			"abilities\\races\\afrika_korps\\battlegroups\\italian_combined_arms\\left\\italian_combined_arms_covering_fire_ak" => "火炮掩护", 
			"abilities\\races\\afrika_korps\\battlegroups\\italian_infantry\\right\\italian_infantry_obice_barrage_ak" => "305炮击", 
			"abilities\\races\\afrika_korps\\battlegroups\\kriegsmarine\\kriegsmarine_flares_ak" => "SLD星爆照明弹", 
			_ => null, 
		};
	}

	public static bool Active(byte[] raw, long image, long identity)
	{
		if (raw != null && raw.Length >= 164 && BitConverter.ToInt64(raw, 0) == image + 64568064 && raw[76] != 2 && raw[20] == 6 && BitConverter.ToInt64(raw, 24) == identity)
		{
			return BitConverter.ToUInt32(raw, 68) != uint.MaxValue;
		}
		return false;
	}
}
