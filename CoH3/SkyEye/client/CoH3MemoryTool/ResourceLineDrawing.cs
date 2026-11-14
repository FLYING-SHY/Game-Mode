using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class ResourceLineDrawing
{
	private sealed class Run
	{
		public string Text;

		public Color Colour;

		public int Width;
	}

	private const TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

	public static int Draw(Graphics g, string text, Font font, Rectangle box, Color baseColour, NumberSpan[] spans, bool paint, double seconds)
	{
		List<Run> runs = new List<Run>();
		TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(text);
		string pending = "";
		Color pendingColour = baseColour;
		Action action = delegate
		{
			if (pending.Length != 0)
			{
				runs.Add(new Run
				{
					Text = pending,
					Colour = pendingColour,
					Width = TextRenderer.MeasureText(g, pending, font, new Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width
				});
				pending = "";
			}
		};
		while (textElementEnumerator.MoveNext())
		{
			string textElement = textElementEnumerator.GetTextElement();
			int elementIndex = textElementEnumerator.ElementIndex;
			Color color = baseColour;
			foreach (NumberSpan numberSpan in spans)
			{
				if (numberSpan.Rule.Enabled && elementIndex >= numberSpan.Start && elementIndex < numberSpan.Start + numberSpan.Length)
				{
					color = numberSpan.Rule.At(numberSpan.Value, seconds);
					break;
				}
			}
			bool flag = textElement.Length == 1 && ((textElement[0] >= 'a' && textElement[0] <= 'z') || (textElement[0] >= 'A' && textElement[0] <= 'Z') || (textElement[0] >= '0' && textElement[0] <= '9') || "._:/%+-".IndexOf(textElement[0]) >= 0);
			if (flag && pending.Length > 0 && color.ToArgb() == pendingColour.ToArgb())
			{
				pending += textElement;
				continue;
			}
			action();
			pendingColour = color;
			pending = textElement;
			if (!flag)
			{
				action();
			}
		}
		action();
		int lineHeight = TextRenderer.MeasureText(g, "国Ag", font, new Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Height;
		int x = 0;
		int y = 0;
		Action<Run> action2 = delegate(Run run)
		{
			if (run.Text == "\n")
			{
				x = 0;
				y += lineHeight;
			}
			else if (!(run.Text == "\r"))
			{
				if (x > 0 && x + run.Width > box.Width)
				{
					x = 0;
					y += lineHeight;
				}
				if (x != 0 || !string.IsNullOrWhiteSpace(run.Text))
				{
					if (paint)
					{
						Rectangle rectangle = new Rectangle(box.X + x, box.Y + y, Math.Max(1, run.Width), lineHeight);
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
							TextRenderer.DrawText(g, run.Text, font, bounds, Color.Black, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
						}
						TextRenderer.DrawText(g, run.Text, font, rectangle, run.Colour, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
					}
					x += run.Width;
				}
			}
		};
		foreach (Run item in runs)
		{
			if (item.Width <= box.Width)
			{
				action2(item);
				continue;
			}
			TextElementEnumerator textElementEnumerator2 = StringInfo.GetTextElementEnumerator(item.Text);
			while (textElementEnumerator2.MoveNext())
			{
				string textElement2 = textElementEnumerator2.GetTextElement();
				action2(new Run
				{
					Text = textElement2,
					Colour = item.Colour,
					Width = TextRenderer.MeasureText(g, textElement2, font, new Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width
				});
			}
		}
		return y + lineHeight;
	}
}
