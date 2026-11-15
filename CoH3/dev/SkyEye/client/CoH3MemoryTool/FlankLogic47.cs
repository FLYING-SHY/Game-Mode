using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CoH3MemoryTool;

internal static class FlankLogic47
{
	public static FlankResult47 Count(IEnumerable<SquadMarker> units, IEnumerable<PlayerInfoRow> players, float x, float z, double radius)
	{
		Dictionary<uint, string> dictionary = players.ToDictionary((PlayerInfoRow p) => p.Slot, (PlayerInfoRow p) => p.Relation);
		HashSet<uint> hashSet = new HashSet<uint>();
		HashSet<uint> hashSet2 = new HashSet<uint>();
		HashSet<uint> hashSet3 = new HashSet<uint>();
		HashSet<uint> hashSet4 = new HashSet<uint>();
		foreach (SquadMarker unit in units)
		{
			if (!unit.TextOnly && dictionary.TryGetValue(unit.Slot, out var value) && !(value == "中立"))
			{
				bool flag = value == "自己" || value == "队友";
				bool flag2 = Near(unit.X, unit.Z, x, z, radius);
				if (flag2)
				{
					(flag ? hashSet : hashSet2).Add(unit.Slot);
				}
				if (flag2 || (unit.Movement != null && Near(unit.Movement.X, unit.Movement.Z, x, z, radius)))
				{
					(flag ? hashSet3 : hashSet4).Add(unit.Slot);
				}
			}
		}
		FlankResult47 flankResult = new FlankResult47();
		flankResult.Friends = hashSet.Count;
		flankResult.Enemies = hashSet2.Count;
		flankResult.IncomingFriends = hashSet3.Count;
		flankResult.IncomingEnemies = hashSet4.Count;
		return flankResult;
	}

	private static bool Near(float a, float b, float x, float z, double r)
	{
		return (double)((a - x) * (a - x) + (b - z) * (b - z)) <= r * r;
	}

	public static bool Ground(MineCamera c, PointF point, int width, int height, float y, out float x, out float z)
	{
		double[] array = new double[16];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					array[i * 4 + j] += c.View[i * 4 + k] * c.Projection[k * 4 + j];
				}
			}
		}
		double num = point.X * 2f / (float)width - 1f;
		double num2 = 1f - point.Y * 2f / (float)height;
		double num3 = array[0] - num * array[3];
		double num4 = array[8] - num * array[11];
		double num5 = num * array[15] - array[12] - (double)y * (array[4] - num * array[7]);
		double num6 = array[1] - num2 * array[3];
		double num7 = array[9] - num2 * array[11];
		double num8 = num2 * array[15] - array[13] - (double)y * (array[5] - num2 * array[7]);
		double num9 = num3 * num7 - num4 * num6;
		x = (z = 0f);
		if (Math.Abs(num9) < 1E-08)
		{
			return false;
		}
		x = (float)((num5 * num7 - num4 * num8) / num9);
		z = (float)((num3 * num8 - num5 * num6) / num9);
		if (SquadMarkerLogic.ValidPosition(x, y, z) && c.Project(x, y, z, width, height, out var point2) && Math.Abs(point2.X - point.X) < 2f)
		{
			return Math.Abs(point2.Y - point.Y) < 2f;
		}
		return false;
	}
}
