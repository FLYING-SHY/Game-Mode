using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CoH3MemoryTool;

internal static class MovementLabelLayout
{
	public static Rectangle Place(Size size, PointF anchor, int width, int height, IEnumerable<Rectangle> occupied)
	{
		int x = Math.Max(4, Math.Min(Math.Max(4, width - size.Width - 4), (int)Math.Round(anchor.X - (float)size.Width / 2f)));
		int top = Math.Max(4, Math.Min(Math.Max(4, height - size.Height - 4), (int)Math.Round(anchor.Y - (float)size.Height)));
		Rectangle[] array = occupied.ToArray();
		List<int> list = new List<int>();
		list.Add(top);
		List<int> list2 = list;
		Rectangle[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Rectangle rectangle = array2[i];
			list2.Add(rectangle.Top - size.Height - 4);
			list2.Add(rectangle.Bottom + 4);
		}
		Rectangle area = default(Rectangle);
		foreach (int item in from n in list2.Where((int n) => n >= 4 && n + size.Height <= height - 4).Distinct()
			orderby (!((float)(n + size.Height) <= anchor.Y)) ? 1 : 0, Math.Abs(n - top), n
			select n)
		{
			ref Rectangle reference = ref area;
			reference = new Rectangle(x, item, size.Width, size.Height);
			if (!array.Any((Rectangle r) => Rectangle.Inflate(r, 2, 2).IntersectsWith(area)))
			{
				return area;
			}
		}
		return new Rectangle(x, top, size.Width, size.Height);
	}
}
