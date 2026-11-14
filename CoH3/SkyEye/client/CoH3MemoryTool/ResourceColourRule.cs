using System;
using System.Drawing;
using System.Globalization;

namespace CoH3MemoryTool;

internal sealed class ResourceColourRule
{
	public bool Enabled = true;

	public Color Zero = Color.White;

	public Color Target = Color.Gold;

	public Color Flash = Color.Red;

	public decimal TargetValue = 100m;

	public decimal FlashAbove = 100m;

	public decimal Frequency = 1m;

	public Color At(double value, double seconds)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			return Zero;
		}
		if (value > (double)FlashAbove)
		{
			if ((long)Math.Floor(Math.Max(0.0, seconds) * (double)Frequency * 2.0) % 2 != 0)
			{
				return Flash;
			}
			return Target;
		}
		double t = Math.Max(0.0, Math.Min(1.0, value / (double)Math.Max(1m, TargetValue)));
		return Color.FromArgb(255, Mix(Zero.R, Target.R, t), Mix(Zero.G, Target.G, t), Mix(Zero.B, Target.B, t));
	}

	private static int Mix(int a, int b, double t)
	{
		return (int)Math.Round((double)a + (double)(b - a) * t);
	}

	public string Encode()
	{
		return string.Join("|", Enabled ? "1" : "0", Zero.ToArgb().ToString(), Target.ToArgb().ToString(), Flash.ToArgb().ToString(), TargetValue.ToString(CultureInfo.InvariantCulture), FlashAbove.ToString(CultureInfo.InvariantCulture), Frequency.ToString(CultureInfo.InvariantCulture));
	}

	public void Decode(string text)
	{
		string[] array = text.Split('|');
		if (array.Length == 7 && (!(array[0] != "0") || !(array[0] != "1")) && int.TryParse(array[1], out var result) && int.TryParse(array[2], out var result2) && int.TryParse(array[3], out var result3) && decimal.TryParse(array[4], NumberStyles.Number, CultureInfo.InvariantCulture, out var result4) && decimal.TryParse(array[5], NumberStyles.Number, CultureInfo.InvariantCulture, out var result5) && decimal.TryParse(array[6], NumberStyles.Number, CultureInfo.InvariantCulture, out var result6) && !(result4 < 1m) && !(result4 > 1000000m) && !(result5 < 0m) && !(result5 > 1000000m) && !(result6 < 0.1m) && !(result6 > 5m))
		{
			Enabled = array[0] == "1";
			Zero = Color.FromArgb(255, Color.FromArgb(result));
			Target = Color.FromArgb(255, Color.FromArgb(result2));
			Flash = Color.FromArgb(255, Color.FromArgb(result3));
			TargetValue = result4;
			FlashAbove = result5;
			Frequency = result6;
		}
	}
}
