using System;
using System.Collections.Generic;
using System.Drawing;

namespace CoH3MemoryTool;

internal static class MinimapBarrageDrawing
{
	public static PointF[] Star(PointF centre, float size)
	{
		List<PointF> list = new List<PointF>();
		float num = size / 2f;
		double[] array = new double[3]
		{
			Math.PI / 2.0,
			Math.PI / 6.0,
			Math.PI * 5.0 / 6.0
		};
		foreach (double num2 in array)
		{
			float num3 = (float)Math.Cos(num2) * num;
			float num4 = (float)Math.Sin(num2) * num;
			list.Add(new PointF(centre.X - num3, centre.Y - num4));
			list.Add(new PointF(centre.X + num3, centre.Y + num4));
		}
		return list.ToArray();
	}

	public static void Draw(Graphics g, IEnumerable<BarrageRecord> barrages, MinimapFrame frame, MinimapPreferences p, int width, int height)
	{
		if (!p.Barrages || barrages == null)
		{
			return;
		}
		foreach (BarrageRecord barrage in barrages)
		{
			if (!frame.Project(barrage.X, barrage.Z, width, height, out var point))
			{
				continue;
			}
			PointF[] array = Star(point, p.BarrageSize);
			using Pen pen = new Pen(Color.Black, 3.5f);
			using Pen pen2 = new Pen(barrage.Colour, 1.5f);
			for (int i = 0; i < array.Length; i += 2)
			{
				g.DrawLine(pen, array[i], array[i + 1]);
			}
			for (int j = 0; j < array.Length; j += 2)
			{
				g.DrawLine(pen2, array[j], array[j + 1]);
			}
		}
	}
}
