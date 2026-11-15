using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class OverlayDrawing
{
	public const TextFormatFlags Flags = TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;

	public static int Draw(Graphics g, string[] lines, Font font, Color color, Color skillColor, int width, bool paint, int detailSize = 0, Color[] lineColours = null, NumberSpan[][] numberSpans = null, double animationSeconds = -1.0)
	{
		using Font font2 = new Font(font.FontFamily, (detailSize > 0) ? ((float)detailSize) : font.Size, font.Style);
		int num = 5;
		for (int i = 0; i < lines.Length; i++)
		{
			Font font3 = ((i % 2 == 0) ? font : font2);
			if (string.IsNullOrEmpty(lines[i]))
			{
				continue;
			}
			bool flag = numberSpans != null && i < numberSpans.Length && numberSpans[i].Length > 0;
			Color baseColour = ((lineColours != null && i < lineColours.Length) ? lineColours[i] : ((i % 2 == 0) ? color : skillColor));
			int num2 = (flag ? ResourceLineDrawing.Draw(g, lines[i], font3, new Rectangle(7, num, Math.Max(20, width - 14), 32767), baseColour, numberSpans[i], paint, (animationSeconds < 0.0) ? ResourceClock.Seconds : animationSeconds) : TextRenderer.MeasureText(g, lines[i], font3, new Size(Math.Max(20, width - 14), 32767), TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height);
			Rectangle rectangle = new Rectangle(7, num, Math.Max(20, width - 14), num2);
			if (paint && !flag)
			{
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
					TextRenderer.DrawText(g, lines[i], font3, bounds, Color.Black, TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
				}
				TextRenderer.DrawText(g, lines[i], font3, rectangle, (lineColours != null && i < lineColours.Length) ? lineColours[i] : ((i % 2 == 0) ? color : skillColor), TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			}
			num += num2 + ((i % 2 != 1) ? 1 : 7);
		}
		return num + 3;
	}
}
