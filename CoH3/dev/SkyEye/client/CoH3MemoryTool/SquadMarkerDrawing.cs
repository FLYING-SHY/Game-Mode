using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class SquadMarkerDrawing
{
	public static void Draw(Graphics g, PointF point, bool vehicle, string name, Color colour, SquadMarkerPreferences p, Font font)
	{
		float num = (vehicle ? p.VehicleSize : p.InfantrySize);
		RectangleF rect = new RectangleF(point.X - num / 2f, point.Y - num / 2f, num, num);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (Pen pen = new Pen(colour, (float)p.Stroke))
		{
			if (vehicle)
			{
				g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
			}
			else
			{
				g.DrawEllipse(pen, rect);
			}
		}
		TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
		Size size = TextRenderer.MeasureText(g, name ?? "", font, new Size(32767, 32767), flags);
		Rectangle rectangle = new Rectangle((int)Math.Round(point.X - (float)size.Width / 2f), (int)Math.Round(point.Y - (float)size.Height / 2f), size.Width, size.Height);
		Point[] array = new Point[4]
		{
			new Point(-1, 0),
			new Point(1, 0),
			new Point(0, -1),
			new Point(0, 1)
		};
		foreach (Point pos in array)
		{
			Rectangle bounds = rectangle;
			bounds.Offset(pos);
			TextRenderer.DrawText(g, name, font, bounds, Color.Black, flags);
		}
		TextRenderer.DrawText(g, name, font, rectangle, colour, flags);
	}
}
