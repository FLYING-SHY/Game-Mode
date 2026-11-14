using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class SquadMarkerPreferences
{
	public AirPreferences Air = new AirPreferences();

	public ProjectilePreferences Projectiles = new ProjectilePreferences();

	public bool ATSectors = true;

	public bool Movement = true;

	public int MoveCrossSize = 24;

	public decimal MoveWidth = 2m;

	public decimal MoveDashLength = 8m;

	public decimal MoveDashGap = 5m;

	public bool Enabled = true;

	public bool ShowMetrics;

	public bool Directions = true;

	public bool AutoTeamRange = true;

	public bool AutoVehicleRange = true;

	public bool AutoSectors = true;

	public bool Sectors;

	public int VehicleLength = 20;

	public int TeamLength = 45;

	public int ArtilleryLength = 25;

	public int SectorAngle;

	public decimal DirectionWidth = 2m;

	public int InfantrySize = 40;

	public int VehicleSize = 48;

	public int TextSize = 12;

	public int FrameRate = 60;

	public int PositionRate = 20;

	public decimal Stroke = 2m;

	public SquadMarkerPreferences Copy()
	{
		SquadMarkerPreferences squadMarkerPreferences = new SquadMarkerPreferences();
		squadMarkerPreferences.ATSectors = ATSectors;
		squadMarkerPreferences.Air = Air.Copy();
		squadMarkerPreferences.Projectiles = Projectiles.Copy();
		squadMarkerPreferences.Enabled = Enabled;
		squadMarkerPreferences.ShowMetrics = ShowMetrics;
		squadMarkerPreferences.Movement = Movement;
		squadMarkerPreferences.MoveCrossSize = MoveCrossSize;
		squadMarkerPreferences.MoveWidth = MoveWidth;
		squadMarkerPreferences.MoveDashLength = MoveDashLength;
		squadMarkerPreferences.MoveDashGap = MoveDashGap;
		squadMarkerPreferences.Directions = Directions;
		squadMarkerPreferences.AutoTeamRange = AutoTeamRange;
		squadMarkerPreferences.AutoVehicleRange = AutoVehicleRange;
		squadMarkerPreferences.AutoSectors = AutoSectors;
		squadMarkerPreferences.Sectors = Sectors;
		squadMarkerPreferences.VehicleLength = VehicleLength;
		squadMarkerPreferences.TeamLength = TeamLength;
		squadMarkerPreferences.ArtilleryLength = ArtilleryLength;
		squadMarkerPreferences.SectorAngle = SectorAngle;
		squadMarkerPreferences.DirectionWidth = DirectionWidth;
		squadMarkerPreferences.InfantrySize = InfantrySize;
		squadMarkerPreferences.VehicleSize = VehicleSize;
		squadMarkerPreferences.TextSize = TextSize;
		squadMarkerPreferences.FrameRate = FrameRate;
		squadMarkerPreferences.PositionRate = PositionRate;
		squadMarkerPreferences.Stroke = Stroke;
		return squadMarkerPreferences;
	}

	public IEnumerable<string> Serialize()
	{
		return new string[24]
		{
			"atSectors=" + (ATSectors ? 1 : 0),
			"moveDashLength=" + MoveDashLength.ToString(CultureInfo.InvariantCulture),
			"moveDashGap=" + MoveDashGap.ToString(CultureInfo.InvariantCulture),
			"movementOn=" + (Movement ? 1 : 0),
			"moveCross=" + MoveCrossSize,
			"moveWidth=" + MoveWidth.ToString(CultureInfo.InvariantCulture),
			"directionOn=" + (Directions ? 1 : 0),
			"directionAuto=" + (AutoTeamRange ? 1 : 0),
			"directionVehicleAuto=" + (AutoVehicleRange ? 1 : 0),
			"directionAutoSectors=" + (AutoSectors ? 1 : 0),
			"directionSectors=" + (Sectors ? 1 : 0),
			"directionVehicle=" + VehicleLength,
			"directionTeam=" + TeamLength,
			"directionArtillery=" + ArtilleryLength,
			"directionAngle=" + SectorAngle,
			"directionWidth=" + DirectionWidth.ToString(CultureInfo.InvariantCulture),
			"markerFrames=" + FrameRate,
			"markerPositions=" + PositionRate,
			"markerMetrics=" + (ShowMetrics ? 1 : 0),
			"squadMarkers=" + (Enabled ? 1 : 0),
			"squadCircle=" + InfantrySize,
			"squadSquare=" + VehicleSize,
			"squadText=" + TextSize,
			"squadStroke=" + Stroke.ToString(CultureInfo.InvariantCulture)
		}.Concat(Projectiles.Serialize()).Concat(Air.Serialize());
	}

	public bool Read(string line)
	{
		if (Air.Read(line) || Projectiles.Read(line))
		{
			return true;
		}
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
		case "atSectors":
			ATSectors = text2 != "0";
			return true;
		case "moveDashLength":
		{
			if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result5))
			{
				MoveDashLength = Math.Max(1m, Math.Min(100m, result5));
			}
			return true;
		}
		case "moveDashGap":
		{
			if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result4))
			{
				MoveDashGap = Math.Max(1m, Math.Min(100m, result4));
			}
			return true;
		}
		case "movementOn":
			if (int.TryParse(text2, out result2))
			{
				Movement = result2 != 0;
			}
			return true;
		case "moveCross":
			if (int.TryParse(text2, out result2))
			{
				MoveCrossSize = Math.Max(8, Math.Min(160, result2));
			}
			return true;
		case "moveWidth":
		{
			if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result6))
			{
				MoveWidth = Math.Max(0.5m, Math.Min(12m, result6));
			}
			return true;
		}
		case "directionOn":
			if (int.TryParse(text2, out result2))
			{
				Directions = result2 != 0;
			}
			return true;
		case "directionAutoSectors":
			if (int.TryParse(text2, out result2))
			{
				AutoSectors = result2 != 0;
			}
			return true;
		case "directionVehicleAuto":
			if (int.TryParse(text2, out result2))
			{
				AutoVehicleRange = result2 != 0;
			}
			return true;
		case "directionAuto":
			if (int.TryParse(text2, out result2))
			{
				AutoTeamRange = result2 != 0;
			}
			return true;
		case "directionSectors":
			if (int.TryParse(text2, out result2))
			{
				Sectors = result2 != 0;
			}
			return true;
		case "directionVehicle":
			if (int.TryParse(text2, out result2))
			{
				VehicleLength = Math.Max(1, Math.Min(200, result2));
			}
			return true;
		case "directionTeam":
			if (int.TryParse(text2, out result2))
			{
				TeamLength = Math.Max(1, Math.Min(200, result2));
			}
			return true;
		case "directionArtillery":
			if (int.TryParse(text2, out result2))
			{
				ArtilleryLength = Math.Max(1, Math.Min(200, result2));
			}
			return true;
		case "directionAngle":
			if (int.TryParse(text2, out result2))
			{
				SectorAngle = Math.Max(0, Math.Min(180, result2));
			}
			return true;
		case "directionWidth":
		{
			if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result3))
			{
				DirectionWidth = Math.Max(0.5m, Math.Min(12m, result3));
			}
			return true;
		}
		case "markerFrames":
			if (int.TryParse(text2, out result2))
			{
				FrameRate = Math.Max(10, Math.Min(144, result2));
			}
			return true;
		case "markerPositions":
			if (int.TryParse(text2, out result2))
			{
				PositionRate = Math.Max(5, Math.Min(144, result2));
			}
			return true;
		case "markerMetrics":
			if (int.TryParse(text2, out result2))
			{
				ShowMetrics = result2 != 0;
			}
			return true;
		case "squadMarkers":
			if (int.TryParse(text2, out result2))
			{
				Enabled = result2 != 0;
			}
			return true;
		case "squadCircle":
			if (int.TryParse(text2, out result2))
			{
				InfantrySize = Math.Max(8, Math.Min(200, result2));
			}
			return true;
		case "squadSquare":
			if (int.TryParse(text2, out result2))
			{
				VehicleSize = Math.Max(8, Math.Min(200, result2));
			}
			return true;
		case "squadText":
			if (int.TryParse(text2, out result2))
			{
				TextSize = Math.Max(8, Math.Min(36, result2));
			}
			return true;
		case "squadStroke":
		{
			if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
			{
				Stroke = Math.Max(0.5m, Math.Min(12m, result));
			}
			return true;
		}
		default:
			return false;
		}
	}
}
