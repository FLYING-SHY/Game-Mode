using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal static class SquadMarkerLogic
{
	public static bool Supported(string path)
	{
		if (Infrastructure49.Name(path) == null)
		{
			if (!string.IsNullOrEmpty(path))
			{
				if (!path.Contains("bunker") || !path.StartsWith("sbps\\races\\"))
				{
					if (path.StartsWith("sbps\\races\\", StringComparison.Ordinal) && !path.Contains("\\buildings\\") && !path.Contains("\\base_defense_"))
					{
						return !path.Contains("\\howitzer_base_");
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool Vehicle(string path)
	{
		return path.Contains("\\vehicles\\");
	}

	public static bool ValidPosition(float x, float y, float z)
	{
		if (!float.IsNaN(x) && !float.IsNaN(y) && !float.IsNaN(z) && !float.IsInfinity(x) && !float.IsInfinity(y) && !float.IsInfinity(z) && Math.Abs(x) < 20000f && Math.Abs(z) < 20000f)
		{
			return Math.Abs(y) < 2000f;
		}
		return false;
	}

	public static SquadMarker Centre(SquadMarkerSource squad, List<float[]> positions)
	{
		if (positions.Count == 0 || !squad.ColourKnown || !Supported(squad.Path))
		{
			return null;
		}
		SquadMarker squadMarker = new SquadMarker();
		squadMarker.Source = squad.Squad;
		squadMarker.Identity = squad.Identity;
		squadMarker.Path = squad.Path;
		squadMarker.Name = Infrastructure49.Name(squad.Path) ?? UnitNames.Name(squad.Path);
		squadMarker.TextOnly = Infrastructure49.Name(squad.Path) != null;
		squadMarker.Slot = squad.Slot;
		squadMarker.Vehicle = Vehicle(squad.Path);
		squadMarker.Colour = squad.Colour;
		squadMarker.X = (float)((IEnumerable<float[]>)positions).Average((Func<float[], double>)((float[] p) => p[0]));
		squadMarker.Y = (float)((IEnumerable<float[]>)positions).Average((Func<float[], double>)((float[] p) => p[1]));
		squadMarker.Z = (float)((IEnumerable<float[]>)positions).Average((Func<float[], double>)((float[] p) => p[2]));
		return squadMarker;
	}
}
