using System;
using System.Collections.Generic;

namespace CoH3MemoryTool;

internal sealed class TacticalMapPreferences51
{
	public bool Enabled = true;

	public int TextSize = 12;

	public IEnumerable<string> Serialize()
	{
		return new string[2]
		{
			"tacticalOn=" + (Enabled ? 1 : 0),
			"tacticalText=" + TextSize
		};
	}

	public bool Read(string line)
	{
		int num = line.IndexOf('=');
		if (num < 0)
		{
			return false;
		}
		string text = line.Substring(0, num);
		if (text != "tacticalOn" && text != "tacticalText")
		{
			return false;
		}
		if (int.TryParse(line.Substring(num + 1), out var result))
		{
			if (text == "tacticalOn")
			{
				Enabled = result != 0;
			}
			else
			{
				TextSize = Math.Max(8, Math.Min(40, result));
			}
		}
		return true;
	}
}
