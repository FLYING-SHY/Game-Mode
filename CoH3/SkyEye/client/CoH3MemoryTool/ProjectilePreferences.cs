using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoH3MemoryTool;

internal sealed class ProjectilePreferences
{
	public bool ShowOwn = true;

	public bool ShowAllies = true;

	public bool Enabled = true;

	public bool Rings = true;

	public int NumberSize = 16;

	public int TextSize = 12;

	public decimal RingWidth = 1.5m;

	public bool InfantryOn => false;

	public bool AnyEnabled
	{
		get
		{
			if (!Enabled)
			{
				return InfantryOn;
			}
			return true;
		}
	}

	public bool Visible(ProjectileImpact impact)
	{
		return Enabled;
	}

	public ProjectilePreferences Copy()
	{
		ProjectilePreferences projectilePreferences = new ProjectilePreferences();
		projectilePreferences.ShowOwn = ShowOwn;
		projectilePreferences.ShowAllies = ShowAllies;
		projectilePreferences.Enabled = Enabled;
		projectilePreferences.Rings = Rings;
		projectilePreferences.NumberSize = NumberSize;
		projectilePreferences.TextSize = TextSize;
		projectilePreferences.RingWidth = RingWidth;
		return projectilePreferences;
	}

	public IEnumerable<string> Serialize()
	{
		List<string> list = new List<string>();
		list.Add("shellOwn=" + (ShowOwn ? 1 : 0));
		list.Add("shellAllies=" + (ShowAllies ? 1 : 0));
		list.Add("shellImpacts=" + (Enabled ? 1 : 0));
		list.Add("shellRings=" + (Rings ? 1 : 0));
		list.Add("shellNumber=" + NumberSize);
		list.Add("shellText=" + TextSize);
		list.Add("shellRingWidth=" + RingWidth.ToString(CultureInfo.InvariantCulture));
		return list;
	}

	public bool Read(string line)
	{
		int num = line.IndexOf('=');
		if (num < 0)
		{
			return false;
		}
		string text = line.Substring(0, num);
		string text2 = line.Substring(num + 1);
		int result2;
		switch (text)
		{
		case "shellOwn":
			ShowOwn = text2 != "0";
			return true;
		case "shellAllies":
			ShowAllies = text2 != "0";
			return true;
		case "shellImpacts":
			if (int.TryParse(text2, out result2))
			{
				Enabled = result2 != 0;
			}
			return true;
		case "shellRings":
			if (int.TryParse(text2, out result2))
			{
				Rings = result2 != 0;
			}
			return true;
		case "shellStar":
		case "shellNumber":
			if (int.TryParse(text2, out result2))
			{
				NumberSize = Math.Max(8, Math.Min(72, result2));
			}
			return true;
		case "shellText":
			if (int.TryParse(text2, out result2))
			{
				TextSize = Math.Max(8, Math.Min(36, result2));
			}
			return true;
		case "shellRingWidth":
		{
			if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
			{
				RingWidth = Math.Max(0.5m, Math.Min(8m, result));
			}
			return true;
		}
		default:
			return false;
		}
	}
}
