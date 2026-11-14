using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CoH3MemoryTool;

internal static class ProjectileDrawing
{
	private static void Flush(List<DirectionStroke> result, List<PointF> points, Color colour, float width)
	{
		if (points.Count < 2)
		{
			points.Clear();
			return;
		}
		int num = (int)Math.Ceiling(width + 3f);
		result.Add(new DirectionStroke
		{
			Colour = colour,
			Width = width,
			Points = points.ToArray(),
			Bounds = Rectangle.FromLTRB((int)Math.Floor(points.Min((PointF p) => p.X)) - num, (int)Math.Floor(points.Min((PointF p) => p.Y)) - num, (int)Math.Ceiling(points.Max((PointF p) => p.X)) + num, (int)Math.Ceiling(points.Max((PointF p) => p.Y)) + num)
		});
		points.Clear();
	}

	public static List<DirectionStroke> Rings(IEnumerable<ProjectileImpact> shells, MineCamera camera, ProjectilePreferences p, int width, int height)
	{
		List<DirectionStroke> result = new List<DirectionStroke>();
		if (!p.AnyEnabled || !p.Rings)
		{
			return result;
		}
		foreach (ProjectileImpact shell in shells)
		{
			if (!p.Visible(shell) || shell.Radius <= 0f)
			{
				continue;
			}
			List<PointF> list = new List<PointF>();
			for (int i = 0; i < 64; i++)
			{
				double num = (double)i * Math.PI / 32.0;
				double num2 = (double)(i + 1) * Math.PI / 32.0;
				float[] start = new float[3]
				{
					shell.X + shell.Radius * (float)Math.Cos(num),
					shell.Y + 0.15f,
					shell.Z + shell.Radius * (float)Math.Sin(num)
				};
				float[] end = new float[3]
				{
					shell.X + shell.Radius * (float)Math.Cos(num2),
					shell.Y + 0.15f,
					shell.Z + shell.Radius * (float)Math.Sin(num2)
				};
				if (!MovementDrawing.ClipSegment(camera, start, end, width, height, out var from, out var to))
				{
					Flush(result, list, shell.Colour, (float)p.RingWidth);
					continue;
				}
				if (list.Count > 0 && Math.Abs(list.Last().X - from.X) + Math.Abs(list.Last().Y - from.Y) > 1f)
				{
					Flush(result, list, shell.Colour, (float)p.RingWidth);
				}
				if (list.Count == 0)
				{
					list.Add(from);
				}
				list.Add(to);
			}
			Flush(result, list, shell.Colour, (float)p.RingWidth);
		}
		return result;
	}

	public static void AddNumbers(List<MarkerDrawCommand> commands, IEnumerable<ProjectileImpact> shells, MineCamera camera, ProjectilePreferences p, int width, int height, ProjectileSpriteCache cache)
	{
		if (!p.AnyEnabled)
		{
			return;
		}
		cache.Configure(p);
		foreach (ProjectileImpact shell in shells)
		{
			if (p.Visible(shell) && camera.Project(shell.X, shell.Y + 0.15f, shell.Z, width, height, out var point))
			{
				ProjectileSprite projectileSprite = cache.Get(shell);
				commands.Add(new MarkerDrawCommand
				{
					Image = projectileSprite.Image,
					Bounds = new Rectangle((int)Math.Round(point.X - projectileSprite.Anchor.X), (int)Math.Round(point.Y - projectileSprite.Anchor.Y), projectileSprite.Image.Width, projectileSprite.Image.Height)
				});
			}
		}
	}
}
