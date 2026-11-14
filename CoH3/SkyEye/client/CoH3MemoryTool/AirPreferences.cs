using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class AirPreferences
{
	public bool Enabled = true;

	public bool V1 = true;

	public bool MiniV1 = true;

	public int Opacity = 25;

	public int MiniOpacity = 30;

	public int TextSize = 14;

	public int MiniTextSize = 10;

	public decimal Width = 2m;

	public AirPreferences Copy()
	{
		return (AirPreferences)MemberwiseClone();
	}

	public IEnumerable<string> Serialize()
	{
		return new string[8]
		{
			"airOn=" + (Enabled ? 1 : 0),
			"airV1=" + (V1 ? 1 : 0),
			"airMiniV1=" + (MiniV1 ? 1 : 0),
			"airOpacity=" + Opacity,
			"airMiniOpacity=" + MiniOpacity,
			"airText=" + TextSize,
			"airMiniText=" + MiniTextSize,
			"airWidth=" + Width.ToString(CultureInfo.InvariantCulture)
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
		string s = line.Substring(num + 1);
		if (text == "airWidth")
		{
			if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
			{
				Width = Math.Max(0.5m, Math.Min(8m, result));
			}
			return true;
		}
		if (!new string[7] { "airOn", "airV1", "airMiniV1", "airOpacity", "airMiniOpacity", "airText", "airMiniText" }.Contains(text))
		{
			return false;
		}
		if (!int.TryParse(s, out var result2))
		{
			return true;
		}
		switch (text)
		{
		case "airOn":
			Enabled = result2 != 0;
			break;
		case "airV1":
			V1 = result2 != 0;
			break;
		case "airMiniV1":
			MiniV1 = result2 != 0;
			break;
		case "airOpacity":
			Opacity = Math.Max(0, Math.Min(100, result2));
			break;
		case "airMiniOpacity":
			MiniOpacity = Math.Max(0, Math.Min(100, result2));
			break;
		case "airText":
			TextSize = Math.Max(8, Math.Min(40, result2));
			break;
		case "airMiniText":
			MiniTextSize = Math.Max(6, Math.Min(24, result2));
			break;
		}
		return true;
	}
}
