using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CoH3MemoryTool;

internal static class MovementDrawing
{
	private struct ClipPoint
	{
		public double X;

		public double Y;

		public double W;
	}

	private sealed class Destination
	{
		public SquadMarker Marker;

		public PointF Point;
	}

	public static float[] DashPattern(float length, float gap, float width)
	{
		return new float[2]
		{
			Math.Max(0.01f, length / width),
			Math.Max(0.01f, gap / width)
		};
	}

	private static ClipPoint ClipCoordinates(MineCamera c, float[] point)
	{
		double num = point[0];
		double num2 = point[1];
		double num3 = point[2];
		double num4 = num * (double)c.View[0] + num2 * (double)c.View[4] + num3 * (double)c.View[8] + (double)c.View[12];
		double num5 = num * (double)c.View[1] + num2 * (double)c.View[5] + num3 * (double)c.View[9] + (double)c.View[13];
		double num6 = num * (double)c.View[2] + num2 * (double)c.View[6] + num3 * (double)c.View[10] + (double)c.View[14];
		return new ClipPoint
		{
			X = num4 * (double)c.Projection[0] + num5 * (double)c.Projection[4] + num6 * (double)c.Projection[8] + (double)c.Projection[12],
			Y = num4 * (double)c.Projection[1] + num5 * (double)c.Projection[5] + num6 * (double)c.Projection[9] + (double)c.Projection[13],
			W = num4 * (double)c.Projection[3] + num5 * (double)c.Projection[7] + num6 * (double)c.Projection[11] + (double)c.Projection[15]
		};
	}

	private static bool Bound(double a, double b, ref double low, ref double high)
	{
		if (double.IsNaN(a) || double.IsInfinity(a) || double.IsNaN(b) || double.IsInfinity(b))
		{
			return false;
		}
		if (a < 0.0 && b < 0.0)
		{
			return false;
		}
		if (a >= 0.0 && b >= 0.0)
		{
			return true;
		}
		double val = a / (a - b);
		if (a < 0.0)
		{
			low = Math.Max(low, val);
		}
		else
		{
			high = Math.Min(high, val);
		}
		return low <= high;
	}

	public static bool ClipSegment(MineCamera camera, float[] start, float[] end, int width, int height, out PointF from, out PointF to)
	{
		from = (to = (to = default(PointF)));
		ClipPoint clipPoint = ClipCoordinates(camera, start);
		ClipPoint clipPoint2 = ClipCoordinates(camera, end);
		double low = 0.0;
		double high = 1.0;
		if (!Bound(clipPoint.W - 0.05, clipPoint2.W - 0.05, ref low, ref high) || !Bound(clipPoint.X + clipPoint.W, clipPoint2.X + clipPoint2.W, ref low, ref high) || !Bound(clipPoint.W - clipPoint.X, clipPoint2.W - clipPoint2.X, ref low, ref high) || !Bound(clipPoint.Y + clipPoint.W, clipPoint2.Y + clipPoint2.W, ref low, ref high) || !Bound(clipPoint.W - clipPoint.Y, clipPoint2.W - clipPoint2.Y, ref low, ref high))
		{
			return false;
		}
		double num = clipPoint.W + (clipPoint2.W - clipPoint.W) * low;
		double num2 = clipPoint.W + (clipPoint2.W - clipPoint.W) * high;
		if (num < 0.049999 || num2 < 0.049999)
		{
			return false;
		}
		from = new PointF((float)((1.0 + (clipPoint.X + (clipPoint2.X - clipPoint.X) * low) / num) * (double)width / 2.0), (float)((1.0 - (clipPoint.Y + (clipPoint2.Y - clipPoint.Y) * low) / num) * (double)height / 2.0));
		to = new PointF((float)((1.0 + (clipPoint.X + (clipPoint2.X - clipPoint.X) * high) / num2) * (double)width / 2.0), (float)((1.0 - (clipPoint.Y + (clipPoint2.Y - clipPoint.Y) * high) / num2) * (double)height / 2.0));
		return true;
	}

	private static DirectionStroke Stroke(Color colour, PointF a, PointF b, float width)
	{
		DirectionStroke directionStroke = new DirectionStroke();
		directionStroke.Colour = colour;
		directionStroke.Width = width;
		directionStroke.Points = new PointF[2] { a, b };
		directionStroke.Bounds = Rectangle.FromLTRB((int)Math.Min(a.X, b.X) - 16, (int)Math.Min(a.Y, b.Y) - 16, (int)Math.Max(a.X, b.X) + 17, (int)Math.Max(a.Y, b.Y) + 17);
		return directionStroke;
	}

	public static string Summary(IEnumerable<SquadMarker> markers)
	{
		return string.Join("\n", from g in (from m in markers
				group m by m.Name ?? "").OrderBy((IGrouping<string, SquadMarker> g) => g.Key, StringComparer.Ordinal)
			select g.Key + ((g.Count() > 1) ? ("x" + g.Count()) : ""));
	}

	public static List<DirectionStroke> Build(IEnumerable<SquadMarker> markers, MineCamera camera, SquadMarkerPreferences prefs, int width, int height, Action<SquadMarker, PointF> label)
	{
		List<DirectionStroke> list = new List<DirectionStroke>();
		if (!prefs.Movement)
		{
			return list;
		}
		List<Destination> list2 = new List<Destination>();
		float width2 = (float)prefs.MoveWidth;
		float num = (float)prefs.MoveCrossSize / 2f;
		foreach (SquadMarker marker2 in markers)
		{
			MovementData movement = marker2.Movement;
			if (movement != null)
			{
				if (ClipSegment(camera, new float[3]
				{
					marker2.X,
					marker2.Y + 0.2f,
					marker2.Z
				}, new float[3]
				{
					movement.X,
					movement.Y + 0.2f,
					movement.Z
				}, width, height, out var from, out var to))
				{
					DirectionStroke directionStroke = Stroke(marker2.Colour, from, to, width2);
					directionStroke.Dashed = true;
					directionStroke.DashLength = (float)prefs.MoveDashLength;
					directionStroke.DashGap = (float)prefs.MoveDashGap;
					list.Add(directionStroke);
				}
				if (camera.Project(movement.X, movement.Y + 0.2f, movement.Z, width, height, out var point))
				{
					list2.Add(new Destination
					{
						Marker = marker2,
						Point = point
					});
				}
			}
		}
		List<Destination> list3 = new List<Destination>(from d in list2
			orderby d.Marker.Slot, d.Point.Y, d.Point.X
			select d);
		float reach = Math.Max(36, prefs.MoveCrossSize + 12);
		while (list3.Count > 0)
		{
			List<Destination> list4 = new List<Destination>();
			list4.Add(list3[0]);
			List<Destination> list5 = list4;
			list3.RemoveAt(0);
			bool flag;
			do
			{
				flag = false;
				for (int num2 = list3.Count - 1; num2 >= 0; num2--)
				{
					Destination candidate = list3[num2];
					if (candidate.Marker.Slot == list5[0].Marker.Slot && !(candidate.Marker.Colour != list5[0].Marker.Colour) && list5.Any((Destination d) => (d.Marker.Movement.X - candidate.Marker.Movement.X) * (d.Marker.Movement.X - candidate.Marker.Movement.X) + (d.Marker.Movement.Z - candidate.Marker.Movement.Z) * (d.Marker.Movement.Z - candidate.Marker.Movement.Z) <= 64f || (Math.Abs(d.Point.X - candidate.Point.X) <= reach && Math.Abs(d.Point.Y - candidate.Point.Y) <= reach)))
					{
						list5.Add(candidate);
						list3.RemoveAt(num2);
						flag = true;
					}
				}
			}
			while (flag);
			List<PointF> list6 = new List<PointF>();
			foreach (Destination item in list5)
			{
				PointF goal = item.Point;
				if (!list6.Any((PointF p) => Math.Abs(p.X - goal.X) + Math.Abs(p.Y - goal.Y) < 1f))
				{
					list6.Add(goal);
					list.Add(Stroke(item.Marker.Colour, new PointF(goal.X - num, goal.Y - num), new PointF(goal.X + num, goal.Y + num), width2));
					list.Add(Stroke(item.Marker.Colour, new PointF(goal.X - num, goal.Y + num), new PointF(goal.X + num, goal.Y - num), width2));
				}
			}
			if (label != null)
			{
				SquadMarker marker = list5[0].Marker;
				label(new SquadMarker
				{
					Name = Summary(list5.Select((Destination d) => d.Marker)),
					Colour = marker.Colour,
					Slot = marker.Slot,
					Vehicle = true
				}, new PointF(list5.Average((Destination d) => d.Point.X), list5.Min((Destination d) => d.Point.Y) - num - 4f));
			}
		}
		return list;
	}
}
