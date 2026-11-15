using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;

namespace CoH3MemoryTool;

internal sealed class ProjectileSpriteCache : IDisposable
{
	private readonly Dictionary<string, ProjectileSprite> images = new Dictionary<string, ProjectileSprite>();

	private ProjectilePreferences style;

	private Font font;

	private Font numberFont;

	public void Configure(ProjectilePreferences p)
	{
		if (style == null || style.NumberSize != p.NumberSize || style.TextSize != p.TextSize || images.Count > 512)
		{
			Clear();
			style = p.Copy();
			font = new Font("Microsoft YaHei UI", p.TextSize, FontStyle.Bold);
			numberFont = new Font("Microsoft YaHei UI", p.NumberSize, FontStyle.Bold);
		}
	}

	public ProjectileSprite Get(ProjectileImpact shell)
	{
		string text = (shell.Name ?? "火炮").Replace('\r', ' ').Replace('\n', ' ');
		string text2 = Math.Max(1, shell.Number).ToString(CultureInfo.InvariantCulture);
		string key = shell.Colour.ToArgb() + ":" + text + ":" + text2;
		if (images.TryGetValue(key, out var value))
		{
			return value;
		}
		SizeF sizeF;
		SizeF sizeF2;
		using (Bitmap image = new Bitmap(1, 1))
		{
			using Graphics graphics = Graphics.FromImage(image);
			sizeF = graphics.MeasureString(text, font, 10000, StringFormat.GenericTypographic);
			sizeF2 = graphics.MeasureString(text2, numberFont, 10000, StringFormat.GenericTypographic);
		}
		int num = (int)Math.Ceiling(Math.Max(sizeF2.Width + 8f, sizeF.Width + 8f));
		int height = (int)Math.Ceiling(sizeF.Height + sizeF2.Height + 12f);
		float y = sizeF.Height + 6f + sizeF2.Height / 2f;
		Bitmap image2 = new Bitmap(num, height, PixelFormat.Format32bppPArgb);
		using (Graphics graphics2 = Graphics.FromImage(image2))
		{
			using SolidBrush brush = new SolidBrush(shell.Colour);
			graphics2.Clear(Color.Transparent);
			graphics2.SmoothingMode = SmoothingMode.AntiAlias;
			graphics2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			float num2 = ((float)num - sizeF.Width) / 2f;
			float num3 = ((float)num - sizeF2.Width) / 2f;
			float num4 = sizeF.Height + 6f;
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
				graphics2.DrawString(text, font, Brushes.Black, num2 + (float)point.X, 2 + point.Y, StringFormat.GenericTypographic);
				graphics2.DrawString(text2, numberFont, Brushes.Black, num3 + (float)point.X, num4 + (float)point.Y, StringFormat.GenericTypographic);
			}
			graphics2.DrawString(text, font, brush, num2, 2f, StringFormat.GenericTypographic);
			graphics2.DrawString(text2, numberFont, brush, num3, num4, StringFormat.GenericTypographic);
		}
		ProjectileSprite projectileSprite = new ProjectileSprite();
		projectileSprite.Image = image2;
		projectileSprite.Anchor = new PointF((float)num / 2f, y);
		value = projectileSprite;
		images[key] = value;
		return value;
	}

	private void Clear()
	{
		foreach (ProjectileSprite value in images.Values)
		{
			value.Image.Dispose();
		}
		images.Clear();
		if (font != null)
		{
			font.Dispose();
			font = null;
		}
		if (numberFont != null)
		{
			numberFont.Dispose();
			numberFont = null;
		}
		style = null;
	}

	public void Dispose()
	{
		Clear();
	}
}
