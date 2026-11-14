using System;
using System.Collections.Generic;

namespace CoH3MemoryTool;

internal static class AttackCatalogue67
{
	private static readonly Dictionary<string, string> tankWeapons = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		{ "weapon\\german\\ballistic_weapon\\tank_gun\\380mm_rocket_launcher_sturmtiger_ger", "突击虎" },
		{ "weapon\\german\\ballistic_weapon\\tank_gun\\75mm_semovente_artillery_barrage_ger", "塞莫凡提" },
		{ "weapon\\afrika_korps\\ballistic_weapon\\tank_gun\\75mm_stug_iii_short_ak", "三突D" },
		{ "weapon\\afrika_korps\\ballistic_weapon\\tank_gun\\75mm_barrage_stug_iii_short_ak", "三突D" }
	};

	public static bool Tank(string path)
	{
		if (path != null)
		{
			return tankWeapons.ContainsKey(path);
		}
		return false;
	}

	public static bool Bomb(string path)
	{
		return path == "weapon\\american\\explosive_weapon\\heavy_artillery\\500lb_bomb_p47_us";
	}

	public static bool Arc(string path)
	{
		if (!Tank(path))
		{
			return Bomb(path);
		}
		return true;
	}

	public static string Name(string path)
	{
		if (path == null || !tankWeapons.TryGetValue(path, out var value))
		{
			if (!Bomb(path))
			{
				return null;
			}
			return "P47炸弹";
		}
		return value;
	}

	public static bool EarlyBarrage(string ability)
	{
		if (!(ability == "abilities\\races\\german\\vehicles\\siegebreakers_sturmtiger_shot_ger"))
		{
			return ability == "abilities\\races\\german\\vehicles\\barrage_semovente_ger";
		}
		return true;
	}

	public static bool Assigned(byte[] raw)
	{
		if (raw != null && raw.Length >= 164 && raw[76] != 2 && (raw[144] & 1) != 0 && BitConverter.ToUInt32(raw, 12) != 0 && BitConverter.ToUInt32(raw, 12) <= 64)
		{
			return BitConverter.ToUInt32(raw, 8) != uint.MaxValue;
		}
		return false;
	}
}
