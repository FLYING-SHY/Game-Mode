using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;

namespace CoH3MemoryTool;

internal static class AirDrawing
{
	private struct CPoint
	{
		public double X;

		public double Y;

		public double W;

		public static CPoint Mix(CPoint a, CPoint b, double t)
		{
			return new CPoint
			{
				X = a.X + (b.X - a.X) * t,
				Y = a.Y + (b.Y - a.Y) * t,
				W = a.W + (b.W - a.W) * t
			};
		}
	}

	private static CPoint Clip(MineCamera c, float[] p)
	{
		double num = p[0];
		double num2 = p[1];
		double num3 = p[2];
		double num4 = num * (double)c.View[0] + num2 * (double)c.View[4] + num3 * (double)c.View[8] + (double)c.View[12];
		double num5 = num * (double)c.View[1] + num2 * (double)c.View[5] + num3 * (double)c.View[9] + (double)c.View[13];
		double num6 = num * (double)c.View[2] + num2 * (double)c.View[6] + num3 * (double)c.View[10] + (double)c.View[14];
		return new CPoint
		{
			X = num4 * (double)c.Projection[0] + num5 * (double)c.Projection[4] + num6 * (double)c.Projection[8] + (double)c.Projection[12],
			Y = num4 * (double)c.Projection[1] + num5 * (double)c.Projection[5] + num6 * (double)c.Projection[9] + (double)c.Projection[13],
			W = num4 * (double)c.Projection[3] + num5 * (double)c.Projection[7] + num6 * (double)c.Projection[11] + (double)c.Projection[15]
		};
	}

	public static PointF[] Polygon(MineCamera camera, IEnumerable<float[]> vertices, int width, int height)
	{
		List<CPoint> list = vertices.Select((float[] p) => Clip(camera, p)).ToList();
		if (list.Any((CPoint p) => double.IsNaN(p.X) || double.IsNaN(p.Y) || double.IsNaN(p.W) || double.IsInfinity(p.X) || double.IsInfinity(p.Y) || double.IsInfinity(p.W)))
		{
			return new PointF[0];
		}
		Func<CPoint, double>[] array = new Func<CPoint, double>[5]
		{
			(CPoint p) => p.W - 0.05,
			(CPoint p) => p.X + p.W,
			(CPoint p) => p.W - p.X,
			(CPoint p) => p.Y + p.W,
			(CPoint p) => p.W - p.Y
		};
		foreach (Func<CPoint, double> func in array)
		{
			if (list.Count < 3)
			{
				return new PointF[0];
			}
			List<CPoint> list2 = new List<CPoint>();
			CPoint cPoint = list.Last();
			double num2 = func(cPoint);
			foreach (CPoint item in list)
			{
				double num3 = func(item);
				if (num2 < 0.0 != num3 < 0.0)
				{
					list2.Add(CPoint.Mix(cPoint, item, num2 / (num2 - num3)));
				}
				if (num3 >= 0.0)
				{
					list2.Add(item);
				}
				cPoint = item;
				num2 = num3;
			}
			list = list2;
		}
		return list.Select((CPoint p) => new PointF((float)((1.0 + p.X / p.W) * (double)width / 2.0), (float)((1.0 - p.Y / p.W) * (double)height / 2.0))).ToArray();
	}

	public static float[][] Rectangle(AirZone a)
	{
		float num = a.X + a.DX * a.Offset;
		float num2 = a.Z + a.DZ * a.Offset;
		float num3 = num + a.DX * a.Length;
		float num4 = num2 + a.DZ * a.Length;
		float num5 = (0f - a.DZ) * a.HalfWidth;
		float num6 = a.DX * a.HalfWidth;
		float num7 = a.Y + 0.15f;
		return new float[4][]
		{
			new float[3]
			{
				num + num5,
				num7,
				num2 + num6
			},
			new float[3]
			{
				num3 + num5,
				num7,
				num4 + num6
			},
			new float[3]
			{
				num3 - num5,
				num7,
				num4 - num6
			},
			new float[3]
			{
				num - num5,
				num7,
				num2 - num6
			}
		};
	}

	public static float[][] Circle(AirZone a)
	{
		return Enumerable.Range(0, 64).Select(delegate(int i)
		{
			double num = (double)i * Math.PI / 32.0;
			return new float[3]
			{
				a.X + a.Radius * (float)Math.Cos(num),
				a.Y + 0.15f,
				a.Z + a.Radius * (float)Math.Sin(num)
			};
		}).ToArray();
	}

	public static void Label(Graphics g, string text, Font font, Color colour, PointF centre)
	{
		SizeF sizeF = g.MeasureString(text, font, 10000, StringFormat.GenericTypographic);
		float num = centre.X - sizeF.Width / 2f;
		float num2 = centre.Y - sizeF.Height / 2f;
		using SolidBrush brush = new SolidBrush(colour);
		Point[] array = new Point[4]
		{
			new Point(-1, 0),
			new Point(1, 0),
			new Point(0, -1),
			new Point(0, 1)
		};
		for (int i = 0; i < array.Length; i++)
		{
			Point point = array[i];
			g.DrawString(text, font, Brushes.Black, num + (float)point.X, num2 + (float)point.Y, StringFormat.GenericTypographic);
		}
		g.DrawString(text, font, brush, num, num2, StringFormat.GenericTypographic);
	}

	public static void World(Graphics g, IEnumerable<AirZone> zones, MineCamera camera, AirPreferences p, int width, int height, Font font)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		foreach (AirZone zone in zones)
		{
			if (zone.Rocket ? (!p.V1) : (!p.Enabled))
			{
				continue;
			}
			PointF[] array = Polygon(camera, zone.Rocket ? Circle(zone) : Rectangle(zone), width, height);
			if (array.Length < 3)
			{
				continue;
			}
			using (Pen pen = new Pen(zone.Colour, (float)p.Width))
			{
				if (!zone.Rocket)
				{
					using SolidBrush brush = new SolidBrush(Color.FromArgb(p.Opacity * 255 / 100, zone.Colour));
					g.FillPolygon(brush, array);
				}
				g.DrawPolygon(pen, array);
			}
			float num = zone.Offset + zone.Length / 2f;
			if (camera.Project(zone.X + (zone.Rocket ? 0f : (zone.DX * num)), zone.Y + 0.15f, zone.Z + (zone.Rocket ? 0f : (zone.DZ * num)), width, height, out var point))
			{
				Label(g, zone.Name, font, zone.Colour, point);
			}
		}
	}

	public static void Minimap(Graphics g, IEnumerable<AirZone> zones, MinimapFrame frame, AirPreferences p, int width, int height, Font font)
	{
		if (!p.MiniV1 || frame == null || !frame.Valid)
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
			g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			foreach (AirZone item in zones.Where((AirZone z) => z.Rocket))
			{
				PointF[] points = (from v in Circle(item)
					select frame.Point(v[0], v[2], width, height)).ToArray();
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(p.MiniOpacity * 255 / 100, item.Colour)))
				{
					using Pen pen = new Pen(item.Colour, 1.5f);
					g.FillPolygon(brush, points);
					g.DrawPolygon(pen, points);
				}
				if (frame.Project(item.X, item.Z, width, height, out var point))
				{
					Label(g, "V1", font, item.Colour, point);
				}
			}
		}
		finally
		{
			g.Restore(gstate);
		}
	}
}
