using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace CoH3MemoryTool;

internal static class AirRender42
{
	private static readonly object gate = new object();

	private static string last;

	private static DateTime lastTime;

	public static void Log(string text)
	{
		lock (gate)
		{
			try
			{
				if (!(last == text) || !(DateTime.UtcNow - lastTime < TimeSpan.FromSeconds(10.0)))
				{
					last = text;
					lastTime = DateTime.UtcNow;
					string text2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "air-outline-diagnostics.log");
					if (File.Exists(text2) && new FileInfo(text2).Length > 262144)
					{
						File.WriteAllText(text2, "");
					}
					File.AppendAllText(text2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + text + Environment.NewLine);
				}
			}
			catch
			{
			}
		}
	}

	private static void Edge(List<DirectionStroke> paths, PointF a, PointF b, Color colour, float width)
	{
		int num = (int)Math.Ceiling(width + 4f);
		paths.Add(new DirectionStroke
		{
			Points = new PointF[2] { a, b },
			Colour = colour,
			Width = width,
			Bounds = Rectangle.FromLTRB((int)Math.Floor(Math.Min(a.X, b.X)) - num, (int)Math.Floor(Math.Min(a.Y, b.Y)) - num, (int)Math.Ceiling(Math.Max(a.X, b.X)) + num, (int)Math.Ceiling(Math.Max(a.Y, b.Y)) + num)
		});
	}

	private static void Label(List<MarkerDrawCommand> commands, PointF centre, string text, Color colour, MarkerSpriteCache cache)
	{
		Bitmap bitmap = cache.MovementLabel(new SquadMarker
		{
			Name = text,
			Colour = colour
		});
		commands.Add(new MarkerDrawCommand
		{
			Image = bitmap,
			Bounds = new Rectangle((int)(centre.X - (float)bitmap.Width / 2f), (int)(centre.Y - (float)bitmap.Height / 2f), bitmap.Width, bitmap.Height)
		});
	}

	private static void Configure(MarkerSpriteCache cache, AirPreferences p)
	{
		cache.Configure(new SquadMarkerPreferences
		{
			TextSize = p.TextSize
		});
	}

	public static int Add(List<MarkerDrawCommand> commands, List<DirectionStroke> paths, IEnumerable<AirZone> zones, MineCamera camera, AirPreferences p, int width, int height, MarkerSpriteCache cache)
	{
		if (!p.Enabled)
		{
			return 0;
		}
		Configure(cache, p);
		int num = 0;
		foreach (AirZone item in zones.Where((AirZone z) => !z.Rocket))
		{
			if (item.Point)
			{
				if (camera.Project(item.X, item.Y + 0.15f, item.Z, width, height, out var point))
				{
					Edge(paths, new PointF(point.X - 9f, point.Y), new PointF(point.X + 9f, point.Y), item.Colour, (float)p.Width);
					Edge(paths, new PointF(point.X, point.Y - 9f), new PointF(point.X, point.Y + 9f), item.Colour, (float)p.Width);
					Label(commands, new PointF(point.X, point.Y - 22f), item.Name, item.Colour, cache);
					num++;
				}
				continue;
			}
			float[][] array = AirDrawing.Rectangle(item);
			int count = paths.Count;
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				if (MovementDrawing.ClipSegment(camera, array[num2], array[(num2 + 1) % array.Length], width, height, out var from, out var to))
				{
					Edge(paths, from, to, item.Colour, (float)p.Width);
				}
			}
			float num3 = item.Offset + item.Length / 2f;
			if (camera.Project(item.X + item.DX * num3, item.Y + 0.15f, item.Z + item.DZ * num3, width, height, out var point2))
			{
				Label(commands, point2, item.Name, item.Colour, cache);
			}
			if (paths.Count > count)
			{
				num++;
			}
		}
		return num;
	}

	public static void Fill(Graphics g, IEnumerable<AirZone> zones, MineCamera camera, AirPreferences p, int width, int height)
	{
		if (!p.Enabled)
		{
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (AirZone item in zones.Where((AirZone z) => !z.Rocket && !z.Point))
		{
			PointF[] array = AirDrawing.Polygon(camera, AirDrawing.Rectangle(item), width, height);
			if (array.Length >= 3)
			{
				using SolidBrush brush = new SolidBrush(Color.FromArgb(p.Opacity * 255 / 100, item.Colour));
				g.FillPolygon(brush, array);
			}
		}
	}

	private static RectangleF TestRect(int width, int height)
	{
		return new RectangleF((float)width * 0.32f, (float)height * 0.4f, (float)width * 0.36f, (float)height * 0.14f);
	}

	public static void Test(List<MarkerDrawCommand> commands, List<DirectionStroke> paths, int width, int height, AirPreferences p, MarkerSpriteCache cache)
	{
		Configure(cache, p);
		RectangleF rectangleF = TestRect(width, height);
		PointF[] array = new PointF[4]
		{
			new PointF(rectangleF.Left, rectangleF.Top),
			new PointF(rectangleF.Right, rectangleF.Top),
			new PointF(rectangleF.Right, rectangleF.Bottom),
			new PointF(rectangleF.Left, rectangleF.Bottom)
		};
		for (int i = 0; i < 4; i++)
		{
			Edge(paths, array[i], array[(i + 1) % 4], Color.Cyan, Math.Max(2f, (float)p.Width));
		}
		Label(commands, new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top + rectangleF.Height / 2f), "空袭显示自检", Color.Cyan, cache);
	}

	public static void TestFill(Graphics g, int width, int height, AirPreferences p)
	{
		using SolidBrush brush = new SolidBrush(Color.FromArgb(p.Opacity * 255 / 100, Color.Cyan));
		g.FillRectangle(brush, TestRect(width, height));
	}
}
