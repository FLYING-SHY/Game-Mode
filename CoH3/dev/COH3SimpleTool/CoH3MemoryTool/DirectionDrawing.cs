using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace CoH3MemoryTool;

internal static class DirectionDrawing
{
	public static bool SetArc(DirectionData d, float left, float right, float fireConeHalf)
	{
		d.ArcValid = false;
		if (!Finite(left) || !Finite(right) || !Finite(fireConeHalf) || left < -180f || right > 180f || left > right || fireConeHalf < 0f || fireConeHalf > 180f)
		{
			return false;
		}
		left = Math.Min(left, 0f - fireConeHalf);
		right = Math.Max(right, fireConeHalf);
		if (right - left < 0.1f || right - left > 360f)
		{
			return false;
		}
		d.ArcLeft = left;
		d.ArcRight = right;
		d.ArcValid = true;
		return true;
	}

	private static bool Finite(float value)
	{
		if (!float.IsNaN(value))
		{
			return !float.IsInfinity(value);
		}
		return false;
	}

	public static bool Artillery(string squad, string weapon)
	{
		if (!ArtilleryState.IsSupported(squad) && !new string[11]
		{
			"bishop", "wespe", "stuka", "whizbang", "scott", "hummel", "priest", "sexton", "howitzer", "mortar",
			"artillery"
		}.Any((string name) => squad.Contains(name)) && !weapon.Contains("howitzer") && !weapon.Contains("mortar"))
		{
			return weapon.Contains("artillery");
		}
		return true;
	}

	public static float Length(DirectionData d, SquadMarkerPreferences p)
	{
		if (!d.AntiTankGun)
		{
			if (!d.Artillery)
			{
				if (!d.Team)
				{
					if (!p.AutoVehicleRange)
					{
						return p.VehicleLength;
					}
					return d.Range;
				}
				if (!p.AutoTeamRange)
				{
					return p.TeamLength;
				}
				return d.Range;
			}
			return p.ArtilleryLength;
		}
		return d.Range;
	}

	public static List<DirectionStroke> Build(IEnumerable<SquadMarker> markers, MineCamera camera, SquadMarkerPreferences p, int width, int height)
	{
		List<DirectionStroke> list = new List<DirectionStroke>();
		if (!p.Directions)
		{
			return list;
		}
		foreach (SquadMarker marker in markers)
		{
			DirectionData d = marker.Direction;
			if (d == null || !d.Ready)
			{
				continue;
			}
			float length = Length(d, p);
			double num = Math.Atan2(d.DX, d.DZ);
			Add(list, marker.Colour, camera, width, height, from i in Enumerable.Range(0, 65)
				select new float[3]
				{
					d.X + d.DX * length * (float)i / 64f,
					d.Y + 0.2f,
					d.Z + d.DZ * length * (float)i / 64f
				});
			if (!d.Ready || !d.MachineGun || d.AntiTankGun)
			{
				continue;
			}
			float num2 = 0f;
			float num3 = 0f;
			bool flag = false;
			if (p.AutoSectors && d.ArcValid)
			{
				num2 = d.ArcLeft;
				num3 = d.ArcRight;
				flag = true;
			}
			else if (d.MachineGun && p.Sectors && p.SectorAngle > 0)
			{
				num2 = (float)(-p.SectorAngle) / 2f;
				num3 = (float)p.SectorAngle / 2f;
				flag = true;
			}
			if (flag)
			{
				List<float[]> list2 = new List<float[]>();
				list2.Add(new float[3]
				{
					d.X,
					d.Y + 0.2f,
					d.Z
				});
				for (int num4 = 0; num4 <= 64; num4++)
				{
					double num5 = num + (double)(num2 + (num3 - num2) * (float)num4 / 64f) * Math.PI / 180.0;
					list2.Add(new float[3]
					{
						d.X + (float)Math.Sin(num5) * d.Range,
						d.Y + 0.2f,
						d.Z + (float)Math.Cos(num5) * d.Range
					});
				}
				list2.Add(new float[3]
				{
					d.X,
					d.Y + 0.2f,
					d.Z
				});
				Add(list, marker.Colour, camera, width, height, list2);
			}
		}
		return list;
	}

	private static void Add(List<DirectionStroke> output, Color c, MineCamera camera, int width, int height, IEnumerable<float[]> world)
	{
		List<PointF> list = new List<PointF>();
		foreach (float[] item in world)
		{
			if (camera.Project(item[0], item[1], item[2], width, height, out var point))
			{
				list.Add(point);
			}
			else
			{
				Flush(output, c, list);
			}
		}
		Flush(output, c, list);
	}

	private static void Flush(List<DirectionStroke> output, Color c, List<PointF> points)
	{
		if (points.Count >= 2)
		{
			float num = points.Min((PointF p) => p.X);
			float num2 = points.Min((PointF p) => p.Y);
			float num3 = points.Max((PointF p) => p.X);
			float num4 = points.Max((PointF p) => p.Y);
			output.Add(new DirectionStroke
			{
				Colour = c,
				Points = points.ToArray(),
				Bounds = Rectangle.FromLTRB((int)num - 16, (int)num2 - 16, (int)num3 + 17, (int)num4 + 17)
			});
		}
		points.Clear();
	}

	public static void Draw(Graphics g, IEnumerable<DirectionStroke> paths, decimal width)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (DirectionStroke path in paths)
		{
			using Pen pen = new Pen(path.Colour, (path.Width > 0f) ? path.Width : ((float)width));
			if (path.Dashed)
			{
				if (path.DashLength > 0f && path.DashGap > 0f)
				{
					pen.DashPattern = MovementDrawing.DashPattern(path.DashLength, path.DashGap, pen.Width);
				}
				else
				{
					pen.DashStyle = DashStyle.Dash;
				}
			}
			g.DrawLines(pen, path.Points);
		}
	}
}
