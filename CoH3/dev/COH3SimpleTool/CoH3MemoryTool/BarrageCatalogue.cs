using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CoH3MemoryTool;

internal static class BarrageCatalogue
{
	private static readonly Dictionary<string, HashSet<string>> units;

	private static readonly Dictionary<string, string> squads;

	public static IEnumerable<string> Units => units.Keys;

	static BarrageCatalogue()
	{
		units = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		squads = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CoH3.BarrageCatalogue.tsv");
		if (stream == null)
		{
			throw new InvalidOperationException("缺少火炮技能列表。");
		}
		using StreamReader streamReader = new StreamReader(stream);
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			string[] array = text.Split('\t');
			if (array.Length == 3)
			{
				if (!units.TryGetValue(array[0], out var value))
				{
					value = (units[array[0]] = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
				}
				value.Add(array[1]);
				squads[array[0]] = array[2];
			}
		}
	}

	public static bool Supported(string unit)
	{
		return units.ContainsKey(unit);
	}

	public static bool Ability(string unit, string ability)
	{
		if (units.TryGetValue(unit, out var value))
		{
			return value.Contains(ability);
		}
		return false;
	}

	public static string Squad(string unit)
	{
		if (!squads.TryGetValue(unit, out var value))
		{
			return unit;
		}
		return value;
	}
}
