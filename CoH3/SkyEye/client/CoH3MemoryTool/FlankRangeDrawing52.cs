using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace CoH3MemoryTool;

internal static class FlankRangeDrawing52
{
	public static Color OwnColour(IEnumerable<PlayerInfoRow> rows)
	{
		return rows.FirstOrDefault((PlayerInfoRow p) => p.Relation == "自己" && p.GameColourKnown)?.GameColour ?? Color.White;
	}

	public static void Draw(Graphics g, MineCamera camera, float x, float y, float z, float radius, int width, int height, Color colour, float stroke, bool enabled)
	{
		if (!enabled || camera == null)
		{
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using Pen pen = new Pen(colour, stroke);
		int num = 192;
		float[] array = null;
		for (int i = 0; i <= num; i++)
		{
			double num2 = (double)i * Math.PI * 2.0 / (double)num;
			float[] array2 = new float[3]
			{
				x + (float)Math.Cos(num2) * radius,
				y + 0.3f,
				z + (float)Math.Sin(num2) * radius
			};
			if (array != null && MovementDrawing.ClipSegment(camera, array, array2, width, height, out var from, out var to))
			{
				g.DrawLine(pen, from, to);
			}
			array = array2;
		}
	}
}
