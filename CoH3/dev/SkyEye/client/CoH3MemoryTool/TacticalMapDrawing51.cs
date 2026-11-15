using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace CoH3MemoryTool;

internal static class TacticalMapDrawing51
{
	public static void Draw(Graphics g, IEnumerable<SquadMarker> markers, MinimapFrame frame, MinimapPreferences shared, TacticalMapPreferences51 p, int width, int height, IEnumerable<BarrageRecord> barrages)
	{
		if (!p.Enabled || frame == null || !frame.Valid)
		{
			return;
		}
		MinimapPreferences minimapPreferences = new MinimapPreferences();
		minimapPreferences.VehicleNames = false;
		minimapPreferences.Enabled = true;
		minimapPreferences.Movement = shared.Movement;
		minimapPreferences.Barrages = shared.Barrages;
		minimapPreferences.InfantrySize = shared.InfantrySize;
		minimapPreferences.VehicleSize = shared.VehicleSize;
		minimapPreferences.MoveWidth = shared.MoveWidth;
		minimapPreferences.MoveCrossSize = shared.MoveCrossSize;
		minimapPreferences.BarrageSize = shared.BarrageSize;
		MinimapPreferences p2 = minimapPreferences;
		MinimapMarkerDrawing.Draw(g, markers, frame, p2, width, height, barrages);
		GraphicsState gstate = g.Save();
		try
		{
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddPolygon(frame.Corners(width, height));
				g.SetClip(graphicsPath, CombineMode.Intersect);
			}
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using Font font = new Font("Microsoft YaHei UI", p.TextSize, FontStyle.Bold, GraphicsUnit.Pixel);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Far;
			using StringFormat format = stringFormat;
			foreach (SquadMarker item in markers.Where((SquadMarker x) => !x.TextOnly && !string.IsNullOrEmpty(x.Name)))
			{
				if (!frame.Project(item.X, item.Z, width, height, out var point))
				{
					continue;
				}
				float num = (item.Vehicle ? shared.VehicleSize : shared.InfantrySize);
				point.Y -= num / 2f + 3f;
				using GraphicsPath graphicsPath2 = new GraphicsPath();
				graphicsPath2.AddString(item.Name, font.FontFamily, 1, p.TextSize, point, format);
				Pen pen = new Pen(Color.Black, 3f);
				pen.LineJoin = LineJoin.Round;
				using Pen pen2 = pen;
				using SolidBrush brush = new SolidBrush(item.Colour);
				g.DrawPath(pen2, graphicsPath2);
				g.FillPath(brush, graphicsPath2);
			}
		}
		finally
		{
			g.Restore(gstate);
		}
	}
}
