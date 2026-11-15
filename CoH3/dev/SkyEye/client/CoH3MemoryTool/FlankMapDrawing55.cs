using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CoH3MemoryTool;

internal static class FlankMapDrawing55
{
	public static PointF[] Points(FlankMapRange55 r, MinimapFrame f, int width, int height)
	{
		PointF[] array = new PointF[193];
		for (int i = 0; i < array.Length; i++)
		{
			double num = (double)i * Math.PI / 96.0;
			ref PointF reference = ref array[i];
			reference = f.Point(r.X + (float)Math.Cos(num) * r.Radius, r.Z + (float)Math.Sin(num) * r.Radius, width, height);
		}
		return array;
	}

	public static void Draw(Graphics g, FlankMapRange55 r, MinimapFrame f, int width, int height)
	{
		if (r == null || f == null || !f.Valid)
		{
			return;
		}
		GraphicsState gstate = g.Save();
		try
		{
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddPolygon(f.Corners(width, height));
				g.SetClip(graphicsPath, CombineMode.Intersect);
			}
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using Pen pen = new Pen(r.Colour, r.Width);
			g.DrawLines(pen, Points(r, f, width, height));
			if (f.Project(r.X, r.Z, width, height, out var point))
			{
				g.DrawEllipse(pen, point.X - 2.5f, point.Y - 2.5f, 5f, 5f);
			}
		}
		finally
		{
			g.Restore(gstate);
		}
	}
}
