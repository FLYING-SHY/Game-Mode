using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace CoH3MemoryTool;

internal static class MapUnitLabels52
{
	public static void Draw(Graphics g, IEnumerable<SquadMarker> markers, MinimapFrame frame, MinimapPreferences p, int width, int height)
	{
		if (!p.VehicleNames)
		{
			return;
		}
		StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Center;
		stringFormat.LineAlignment = StringAlignment.Far;
		using StringFormat format = stringFormat;
		using FontFamily family = new FontFamily("Microsoft YaHei UI");
		foreach (SquadMarker item in markers.Where((SquadMarker m) => m.Vehicle && !m.TextOnly && !string.IsNullOrEmpty(m.Name)))
		{
			if (!frame.Project(item.X, item.Z, width, height, out var point))
			{
				continue;
			}
			point.Y -= (float)p.VehicleSize / 2f + 3f;
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddString(item.Name, family, 1, p.VehicleTextSize, point, format);
			Pen pen = new Pen(Color.Black, 2.5f);
			pen.LineJoin = LineJoin.Round;
			using Pen pen2 = pen;
			using SolidBrush brush = new SolidBrush(item.Colour);
			g.DrawPath(pen2, graphicsPath);
			g.FillPath(brush, graphicsPath);
		}
	}
}
