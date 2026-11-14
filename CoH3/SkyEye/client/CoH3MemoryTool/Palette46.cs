using System.Drawing;
using System.Drawing.Drawing2D;

namespace CoH3MemoryTool;

internal static class Palette46
{
	public static Color Back = Color.FromArgb(8, 15, 24);

	public static Color Panel = Color.FromArgb(13, 26, 38);

	public static Color Raised = Color.FromArgb(20, 40, 54);

	public static Color Ink = Color.FromArgb(220, 235, 241);

	public static Color Muted = Color.FromArgb(126, 156, 174);

	public static Color Line = Color.FromArgb(57, 87, 105);

	public static Color Light = Color.FromArgb(102, 207, 222);

	public static Color Amber = Color.FromArgb(235, 190, 101);

	public static Point[] Shape(int w, int h, int cut = 9)
	{
		return new Point[6]
		{
			new Point(cut, 0),
			new Point(w - 1, 0),
			new Point(w - 1, h - cut - 1),
			new Point(w - cut - 1, h - 1),
			new Point(0, h - 1),
			new Point(0, cut)
		};
	}

	public static void Frame(Graphics g, Rectangle r, Color colour)
	{
		using Pen pen = new Pen(colour);
		GraphicsState gstate = g.Save();
		g.TranslateTransform(r.X, r.Y);
		g.DrawPolygon(pen, Shape(r.Width, r.Height));
		g.Restore(gstate);
	}
}
