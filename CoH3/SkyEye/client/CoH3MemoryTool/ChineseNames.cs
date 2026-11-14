using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CoH3MemoryTool;

internal static class ChineseNames
{
	private static readonly Dictionary<string, string> names = Load();

	private static Dictionary<string, string> Load()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("names-zh-hans.tsv");
		if (stream == null)
		{
			throw new InvalidOperationException("缺少简体中文名称表，请使用完整版本。");
		}
		using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			string[] array = text.Split('\t');
			if (array.Length == 2)
			{
				dictionary[array[0]] = array[1];
			}
		}
		return dictionary;
	}

	private static string Normalize(string s)
	{
		return (s ?? "").Replace('\\', '/').ToLowerInvariant();
	}

	public static bool IsActivation(string path)
	{
		return names.ContainsKey("A:" + Normalize(path));
	}

	public static bool IsSkill(string path)
	{
		string text = Normalize(path);
		string[] array = text.Split('/');
		if (array.Length >= 5)
		{
			if (!names.ContainsKey("N:" + text))
			{
				return names.ContainsKey("N:" + array[3] + "/" + Regex.Replace(array.Last(), "_(us|uk|ger|ak|dak)$", ""));
			}
			return true;
		}
		return false;
	}

	public static string GroupKey(PlayerInfoRow row)
	{
		return string.Join("|", from x in row.Upgrades.Where(IsActivation)
			orderby x
			select x);
	}

	public static string Group(PlayerInfoRow row)
	{
		List<string> list = new List<string>();
		foreach (string upgrade in row.Upgrades)
		{
			string[] array = Normalize(upgrade).Split('/');
			if (array.Length >= 5 && !(array[2] != "battlegroups") && names.TryGetValue("G:" + array[1] + "/" + array[3], out var value))
			{
				list.Add(value.EndsWith("战斗群") ? value.Substring(0, value.Length - 3) : value);
			}
		}
		if (list.Count != 0)
		{
			return string.Join(" / ", list.Distinct());
		}
		return "战斗群未知";
	}

	public static string Skill(string path)
	{
		string text = Normalize(path);
		if (names.TryGetValue("S:" + text, out var value))
		{
			return value;
		}
		string[] array = text.Split('/');
		string text2 = array.Last();
		string text3 = ((array.Length > 3) ? array[3] : "");
		if (names.TryGetValue("L:" + text3 + "/" + text2, out value))
		{
			return value;
		}
		string text4 = Regex.Replace(text2, "_(us|uk|ger|ak|dak)$", "");
		if (names.TryGetValue("L:" + text3 + "/" + text4, out value))
		{
			return value;
		}
		Match match = Regex.Match(text2, "(?:^|_)(left|right)_(\\d+)([ab]?)(?:_|$)");
		if (match.Success)
		{
			return ((match.Groups[1].Value == "left") ? "左" : "右") + match.Groups[2].Value + ((match.Groups[3].Value == "a") ? "甲" : ((match.Groups[3].Value == "b") ? "乙" : "")) + "技能（名称待识别）";
		}
		return "已解锁技能（名称待识别）";
	}
}
