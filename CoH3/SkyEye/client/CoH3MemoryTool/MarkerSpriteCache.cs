using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace CoH3MemoryTool;

internal sealed class MarkerSpriteCache : IDisposable
{
	private readonly Dictionary<string, Bitmap> sprites = new Dictionary<string, Bitmap>();

	private SquadMarkerPreferences style;

	private Font font;

	public void Configure(SquadMarkerPreferences p)
	{
		if (sprites.Count > 1024 || style == null || style.InfantrySize != p.InfantrySize || style.VehicleSize != p.VehicleSize || style.TextSize != p.TextSize || !(style.Stroke == p.Stroke))
		{
			Clear();
			style = p.Copy();
			font = new Font("Microsoft YaHei UI", p.TextSize, FontStyle.Bold);
		}
	}

	public Bitmap Get(SquadMarker marker)
	{
		string key = (marker.TextOnly ? "t" : (marker.Vehicle ? "v" : "i")) + marker.Colour.ToArgb() + ":" + marker.Name;
		if (sprites.TryGetValue(key, out var value))
		{
			return value;
		}
		float num = (marker.Vehicle ? style.VehicleSize : style.InfantrySize);
		SizeF sizeF;
		using (Bitmap image = new Bitmap(1, 1))
		{
			using Graphics graphics = Graphics.FromImage(image);
			sizeF = graphics.MeasureString(marker.Name ?? "", font, 10000, StringFormat.GenericTypographic);
		}
		int num2 = (int)Math.Ceiling(Math.Max(num + (float)style.Stroke + 8f, sizeF.Width + 8f));
		int num3 = (int)Math.Ceiling(Math.Max(num + (float)style.Stroke + 8f, sizeF.Height + 8f));
		value = new Bitmap(num2, num3, PixelFormat.Format32bppPArgb);
		using (Graphics graphics2 = Graphics.FromImage(value))
		{
			using Pen pen = new Pen(marker.Colour, (float)style.Stroke);
			using SolidBrush brush = new SolidBrush(marker.Colour);
			graphics2.Clear(Color.Transparent);
			graphics2.SmoothingMode = SmoothingMode.AntiAlias;
			graphics2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			RectangleF rect = new RectangleF(((float)num2 - num) / 2f, ((float)num3 - num) / 2f, num, num);
			if (!marker.TextOnly)
			{
				if (marker.Vehicle)
				{
					graphics2.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
				}
				else
				{
					graphics2.DrawEllipse(pen, rect);
				}
			}
			float num4 = ((float)num2 - sizeF.Width) / 2f;
			float num5 = ((float)num3 - sizeF.Height) / 2f;
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
				graphics2.DrawString(marker.Name ?? "", font, Brushes.Black, num4 + (float)point.X, num5 + (float)point.Y, StringFormat.GenericTypographic);
			}
			graphics2.DrawString(marker.Name ?? "", font, brush, num4, num5, StringFormat.GenericTypographic);
		}
		sprites.Add(key, value);
		return value;
	}

	public Bitmap MovementLabel(SquadMarker marker)
	{
		string key = "move:" + marker.Colour.ToArgb() + ":" + marker.Name;
		if (sprites.TryGetValue(key, out var value))
		{
			return value;
		}
		string[] array = (marker.Name ?? "").Split('\n');
		float num = 0f;
		float num2;
		using (Bitmap image = new Bitmap(1, 1))
		{
			using Graphics graphics = Graphics.FromImage(image);
			num2 = (float)Math.Ceiling(font.GetHeight(graphics) + 2f);
			string[] array2 = array;
			foreach (string text in array2)
			{
				num = Math.Max(num, graphics.MeasureString(text, font, 10000, StringFormat.GenericTypographic).Width);
			}
		}
		value = new Bitmap(Math.Max(1, (int)Math.Ceiling(num + 8f)), Math.Max(1, (int)Math.Ceiling(num2 * (float)array.Length + 8f)), PixelFormat.Format32bppPArgb);
		using (Graphics graphics2 = Graphics.FromImage(value))
		{
			using SolidBrush brush = new SolidBrush(marker.Colour);
			graphics2.Clear(Color.Transparent);
			graphics2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			for (int j = 0; j < array.Length; j++)
			{
				float num3 = ((float)value.Width - graphics2.MeasureString(array[j], font, 10000, StringFormat.GenericTypographic).Width) / 2f;
				float num4 = 4f + (float)j * num2;
				Point[] array3 = new Point[4]
				{
					new Point(-1, 0),
					new Point(1, 0),
					new Point(0, -1),
					new Point(0, 1)
				};
				for (int k = 0; k < array3.Length; k++)
				{
					Point point = array3[k];
					graphics2.DrawString(array[j], font, Brushes.Black, num3 + (float)point.X, num4 + (float)point.Y, StringFormat.GenericTypographic);
				}
				graphics2.DrawString(array[j], font, brush, num3, num4, StringFormat.GenericTypographic);
			}
		}
		sprites.Add(key, value);
		return value;
	}

	private void Clear()
	{
		foreach (Bitmap value in sprites.Values)
		{
			value.Dispose();
		}
		sprites.Clear();
		style = null;
		if (font != null)
		{
			font.Dispose();
			font = null;
		}
	}

	public void Reset()
	{
		Clear();
	}

	public void Dispose()
	{
		Clear();
	}
}
