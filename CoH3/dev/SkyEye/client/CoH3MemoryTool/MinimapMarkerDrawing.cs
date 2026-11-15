using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace CoH3MemoryTool;

internal static class MinimapMarkerDrawing
{
	public static void Draw(Graphics g, IEnumerable<SquadMarker> markers, MinimapFrame frame, MinimapPreferences p, int width, int height, IEnumerable<BarrageRecord> barrages = null)
	{
		if (!p.Enabled || frame == null || !frame.Valid)
		{
			return;
		}
		GraphicsState gstate = g.Save();
		try
		{
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddPolygon(frame.Corners(width, height));
				g.SetClip(graphicsPath, CombineMode.Intersect);
			}
			g.SmoothingMode = SmoothingMode.AntiAlias;
			MinimapMovementDrawing.Draw(g, MinimapMovementDrawing.Build(markers, frame, p, width, height), p);
			foreach (SquadMarker item in markers.Where((SquadMarker m) => !m.TextOnly))
			{
				if (!frame.Project(item.X, item.Z, width, height, out var point))
				{
					continue;
				}
				using SolidBrush brush = new SolidBrush(item.Colour);
				using Pen pen = new Pen(Color.Black, 1f);
				float num = (item.Vehicle ? p.VehicleSize : p.InfantrySize);
				if (item.Vehicle)
				{
					float num2 = num / 2f;
					PointF[] points = new PointF[3]
					{
						new PointF(point.X, point.Y - num2),
						new PointF(point.X - num2, point.Y + num2),
						new PointF(point.X + num2, point.Y + num2)
					};
					g.FillPolygon(brush, points);
					g.DrawPolygon(pen, points);
				}
				else
				{
					RectangleF rect = new RectangleF(point.X - num / 2f, point.Y - num / 2f, num, num);
					g.FillEllipse(brush, rect);
					g.DrawEllipse(pen, rect);
				}
			}
			MinimapBarrageDrawing.Draw(g, barrages, frame, p, width, height);
		}
		finally
		{
			g.Restore(gstate);
		}
	}
}
