using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoH3MemoryTool;

internal sealed class MinimapPreferences
{
	public bool VehicleNames = true;

	public int VehicleTextSize = 10;

	public bool Enabled = true;

	public bool Movement = true;

	public bool Barrages = true;

	public int InfantrySize = 6;

	public int VehicleSize = 10;

	public int RefreshRate = 10;

	public int MoveCrossSize = 8;

	public int BarrageSize = 12;

	public decimal MoveWidth = 1m;

	public IEnumerable<string> Serialize()
	{
		return new string[11]
		{
			"miniVehicleNames=" + (VehicleNames ? 1 : 0),
			"miniVehicleText=" + VehicleTextSize,
			"miniEnemy=" + (Enabled ? 1 : 0),
			"miniInfantry=" + InfantrySize,
			"miniVehicle=" + VehicleSize,
			"miniRefresh=" + RefreshRate,
			"miniMovement=" + (Movement ? 1 : 0),
			"miniMoveWidth=" + MoveWidth.ToString(CultureInfo.InvariantCulture),
			"miniMoveCross=" + MoveCrossSize,
			"miniBarrages=" + (Barrages ? 1 : 0),
			"miniBarrageSize=" + BarrageSize
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
		switch (text)
		{
		case "miniVehicleNames":
		case "miniVehicleText":
		{
			if (int.TryParse(s, out var result2))
			{
				if (text == "miniVehicleNames")
				{
					VehicleNames = result2 != 0;
				}
				else
				{
					VehicleTextSize = Math.Max(6, Math.Min(30, result2));
				}
			}
			return true;
		}
		case "miniMoveWidth":
		{
			if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var result3))
			{
				MoveWidth = Math.Max(0.5m, Math.Min(8m, result3));
			}
			return true;
		}
		default:
		{
			if (text != "miniEnemy" && text != "miniInfantry" && text != "miniVehicle" && text != "miniRefresh" && text != "miniMovement" && text != "miniMoveCross" && text != "miniBarrages" && text != "miniBarrageSize")
			{
				return false;
			}
			if (!int.TryParse(s, out var result))
			{
				return true;
			}
			switch (text)
			{
			case "miniEnemy":
				Enabled = result != 0;
				break;
			case "miniBarrages":
				Barrages = result != 0;
				break;
			case "miniBarrageSize":
				BarrageSize = Math.Max(4, Math.Min(60, result));
				break;
			case "miniMovement":
				Movement = result != 0;
				break;
			case "miniMoveCross":
				MoveCrossSize = Math.Max(2, Math.Min(40, result));
				break;
			case "miniInfantry":
				InfantrySize = Math.Max(2, Math.Min(40, result));
				break;
			case "miniVehicle":
				VehicleSize = Math.Max(3, Math.Min(60, result));
				break;
			default:
				RefreshRate = Math.Max(1, Math.Min(60, result));
				break;
			}
			return true;
		}
		}
	}
}
