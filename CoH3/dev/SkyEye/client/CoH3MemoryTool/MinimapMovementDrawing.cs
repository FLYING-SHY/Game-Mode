using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace CoH3MemoryTool;

internal static class MinimapMovementDrawing
{
	public static List<MinimapMovementGuide> Build(IEnumerable<SquadMarker> markers, MinimapFrame frame, MinimapPreferences p, int width, int height)
	{
		List<MinimapMovementGuide> list = new List<MinimapMovementGuide>();
		if (!p.Enabled || !p.Movement || frame == null || !frame.Valid || width <= 0 || height <= 0)
		{
			return list;
		}
		foreach (SquadMarker item in markers.Where((SquadMarker m) => !m.TextOnly))
		{
			MovementData movement = item.Movement;
			if (movement != null && item.Vehicle && SquadMarkerLogic.ValidPosition(item.X, 0f, item.Z) && SquadMarkerLogic.ValidPosition(movement.X, 0f, movement.Z))
			{
				PointF point;
				bool flag = frame.Project(movement.X, movement.Z, width, height, out point);
				list.Add(new MinimapMovementGuide
				{
					Start = frame.Point(item.X, item.Z, width, height),
					End = (flag ? point : frame.Point(movement.X, movement.Z, width, height)),
					Colour = item.Colour,
					Slot = item.Slot,
					ShowCross = flag
				});
			}
		}
		return list;
	}

	public static void Draw(Graphics g, IEnumerable<MinimapMovementGuide> guides, MinimapPreferences p)
	{
		HashSet<Tuple<uint, int, int, int>> hashSet = new HashSet<Tuple<uint, int, int, int>>();
		float num = (float)p.MoveCrossSize / 2f;
		foreach (MinimapMovementGuide guide in guides)
		{
			using Pen pen = new Pen(guide.Colour, (float)p.MoveWidth);
			pen.DashStyle = DashStyle.Solid;
			g.DrawLine(pen, guide.Start, guide.End);
			if (guide.ShowCross && hashSet.Add(Tuple.Create(guide.Slot, guide.Colour.ToArgb(), (int)Math.Round(guide.End.X), (int)Math.Round(guide.End.Y))))
			{
				g.DrawLine(pen, guide.End.X - num, guide.End.Y - num, guide.End.X + num, guide.End.Y + num);
				g.DrawLine(pen, guide.End.X - num, guide.End.Y + num, guide.End.X + num, guide.End.Y - num);
			}
		}
	}
}
