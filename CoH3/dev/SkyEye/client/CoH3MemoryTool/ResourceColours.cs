using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class ResourceColours
{
	public readonly ResourceColourRule Manpower = new ResourceColourRule
	{
		TargetValue = 1000m,
		FlashAbove = 1000m
	};

	public readonly ResourceColourRule Munition = new ResourceColourRule
	{
		TargetValue = 100m,
		FlashAbove = 100m
	};

	public readonly ResourceColourRule Fuel = new ResourceColourRule
	{
		TargetValue = 100m,
		FlashAbove = 100m
	};

	public readonly ResourceColourRule Population = new ResourceColourRule
	{
		TargetValue = 80m,
		FlashAbove = 80m
	};

	public IEnumerable<KeyValuePair<string, ResourceColourRule>> Rules()
	{
		yield return new KeyValuePair<string, ResourceColourRule>("manpower", Manpower);
		yield return new KeyValuePair<string, ResourceColourRule>("munition", Munition);
		yield return new KeyValuePair<string, ResourceColourRule>("fuel", Fuel);
		yield return new KeyValuePair<string, ResourceColourRule>("population", Population);
	}

	public bool Read(string line)
	{
		if (!line.StartsWith("resource.", StringComparison.Ordinal))
		{
			return false;
		}
		int num = line.IndexOf('=');
		if (num < 0)
		{
			return true;
		}
		string text = line.Substring(9, num - 9);
		foreach (KeyValuePair<string, ResourceColourRule> item in Rules())
		{
			if (item.Key == text)
			{
				item.Value.Decode(line.Substring(num + 1));
			}
		}
		return true;
	}

	public IEnumerable<string> Serialize()
	{
		return from r in Rules()
			select "resource." + r.Key + "=" + r.Value.Encode();
	}
}
