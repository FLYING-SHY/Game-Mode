using System;
using System.Collections.Generic;

namespace CoH3MemoryTool;

internal sealed class ArtilleryState
{
	public bool UsesSharedBarrage;

	public string Key;

	public string Name;

	public long Entity;

	public bool CooldownsKnown;

	public readonly Dictionary<uint, int> Cooldowns = new Dictionary<uint, int>();

	public string ReadyMessage => Name + "共享弹幕技能冷却结束";

	public static bool IsStuka(string p)
	{
		return string.Equals(p, "ebps\\races\\afrika_korps\\vehicles\\halftrack_251_stuka_ak", StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsSupported(string p)
	{
		if (!IsStuka(p))
		{
			if (!p.Contains("\\crew\\") && (p.Contains("\\vehicles\\") || p.Contains("\\emplacements\\")))
			{
				if (!p.Contains("bishop") && !p.Contains("wespe"))
				{
					return p.Contains("howitzer_bl_5_5");
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static string UnitName(string p)
	{
		string text = BarrageCatalogue.Squad(p);
		if (text != null)
		{
			return UnitNames.Name(text);
		}
		if (!IsStuka(p))
		{
			if (!p.Contains("bishop"))
			{
				if (!p.Contains("wespe"))
				{
					if (!p.Contains("210"))
					{
						if (!p.Contains("5_5"))
						{
							if (!p.Contains("priest"))
							{
								return "中重型火炮";
							}
							return "牧师自行火炮";
						}
						return "5.5英寸榴弹炮";
					}
					return "210毫米榴弹炮";
				}
				return "黄蜂自行火炮";
			}
			return "主教自行火炮";
		}
		return "陆地斯图卡";
	}
}
