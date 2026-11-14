using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoH3MemoryTool;

internal static class OverlayText
{
	public static Color[] Colours(IEnumerable<PlayerInfoRow> rows, OverlayPreferences p)
	{
		return (from x in rows
			where x.Relation == "敌方" || p.ShowAllPlayers
			orderby x.Slot
			select x).SelectMany((PlayerInfoRow x) => (!p.UseGameColours || !x.GameColourKnown) ? new Color[2] { p.Color, p.SkillColor } : new Color[2] { x.GameColour, x.GameColour }).ToArray();
	}

	public static NumberSpan[][] NumberSpans(IEnumerable<PlayerInfoRow> rows, OverlayPreferences preferences)
	{
		List<NumberSpan[]> list = new List<NumberSpan[]>();
		foreach (PlayerInfoRow item in from x in rows
			where x.Relation == "敌方" || preferences.ShowAllPlayers
			orderby x.Slot
			select x)
		{
			List<NumberSpan> list2 = new List<NumberSpan>();
			First49(item, preferences, list2);
			list.Add(list2.ToArray());
			list.Add(new NumberSpan[0]);
		}
		return list.ToArray();
	}

	private static string Clean(string s)
	{
		return Regex.Replace(s ?? "", "[\\x00-\\x1F\\x7F]", " ").Trim();
	}

	public static string[] Lines(IEnumerable<PlayerInfoRow> rows, OverlayPreferences preferences = null)
	{
		List<string> list = new List<string>();
		foreach (PlayerInfoRow item in from x in rows
			where x.Relation == "敌方" || (preferences != null && preferences.ShowAllPlayers)
			orderby x.Slot
			select x)
		{
			ChineseNames.Group(item);
			BuildingNames.Summary(item);
			ProductionText.Summary(item);
			GrenadeUpgrade.Status(item);
			Kwk38Upgrade.Status(item);
			string text = Clean(item.Name);
			if (text.StartsWith("CPU - "))
			{
				text = "电脑 - " + text.Substring(6);
			}
			list.Add(First49(item, preferences, new List<NumberSpan>()));
			List<string> list2 = new List<string>();
			foreach (string item2 in item.Upgrades.Where((string x) => x.Contains("\\battlegroups\\")))
			{
				string[] array = item2.Split('\\');
				if (array.Length >= 5 && ChineseNames.IsSkill(item2))
				{
					list2.Add(ChineseNames.Skill(item2));
				}
			}
			List<string> list3 = new List<string>();
			if (preferences == null || preferences.ShowSkills)
			{
				list3.Add("已解锁  " + ((list2.Count == 0) ? "未读到节点" : string.Join(" · ", list2.Distinct())));
			}
			if (preferences != null && preferences.ShowUnits)
			{
				list3.Add("单位  " + UnitNames.Summary(item));
			}
			list.Add(string.Join("\n", list3));
		}
		return list.ToArray();
	}

	public static string First49(PlayerInfoRow row, OverlayPreferences settings, List<NumberSpan> spans)
	{
		bool[] array = ((settings == null) ? Enumerable.Repeat(element: true, 10).ToArray() : settings.Info49);
		string text = Clean(row.Name);
		if (text.StartsWith("CPU - "))
		{
			text = "电脑 - " + text.Substring(6);
		}
		string text2 = text;
		string[] array2 = new string[5] { "  人 ", "  弹 ", "  油 ", "  人口 ", "  CP " };
		double[] array3 = new double[5]
		{
			Math.Floor(row.Manpower),
			Math.Floor(row.Munition),
			Math.Floor(row.Fuel),
			row.Population,
			Math.Floor(row.CP)
		};
		ResourceColourRule[] array4 = ((settings == null) ? new ResourceColourRule[4] : new ResourceColourRule[4]
		{
			settings.Resources.Manpower,
			settings.Resources.Munition,
			settings.Resources.Fuel,
			settings.Resources.Population
		});
		for (int i = 0; i < 5; i++)
		{
			if (array[i])
			{
				string text3 = array3[i].ToString((i == 3) ? "0.#" : "0", CultureInfo.CurrentCulture);
				text2 += array2[i];
				if (i < 4 && array4[i] != null && array4[i].Enabled)
				{
					spans.Add(new NumberSpan
					{
						Start = text2.Length,
						Length = text3.Length,
						Value = array3[i],
						Rule = array4[i]
					});
				}
				text2 += text3;
			}
		}
		string[] array5 = new string[5]
		{
			ChineseNames.Group(row),
			BuildingNames.Summary(row),
			ProductionText.Summary(row),
			GrenadeUpgrade.Status(row),
			Kwk38Upgrade.Status(row)
		};
		for (int j = 0; j < 5; j++)
		{
			if (array[5 + j] && !string.IsNullOrEmpty(array5[j]))
			{
				text2 = text2 + "  |  " + array5[j];
			}
		}
		return text2;
	}
}
