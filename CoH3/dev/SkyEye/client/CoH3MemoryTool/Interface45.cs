using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class Interface45
{
	private static Icon icon;

	public static readonly Color Canvas = Color.FromArgb(8, 15, 24);

	public static readonly Color Paper = Color.FromArgb(13, 26, 38);

	public static readonly Color Ink = Color.FromArgb(220, 235, 241);

	public static readonly Color Muted = Color.FromArgb(104, 122, 138);

	public static readonly Color Line = Color.FromArgb(57, 87, 105);

	public static readonly Color Accent = Color.FromArgb(102, 207, 222);

	public static readonly Color Night = Color.FromArgb(20, 35, 49);

	public static Icon Icon
	{
		get
		{
			if (icon == null)
			{
				using Stream stream = typeof(Interface45).Assembly.GetManifestResourceStream("CoH3.InterfaceIcon");
				icon = new Icon(stream);
			}
			return icon;
		}
	}

	public static Font Font(float size = 10f, FontStyle style = FontStyle.Regular)
	{
		return new Font("Microsoft YaHei UI", size * 96f / 72f, style, GraphicsUnit.Pixel);
	}

	public static Label Text(string text, int height = 28, float size = 10f, Color? colour = null)
	{
		Label label = new Label();
		label.Text = text;
		label.Height = height;
		label.Dock = DockStyle.Top;
		label.ForeColor = colour ?? Ink;
		label.Font = Font(size);
		label.AutoEllipsis = true;
		label.TextAlign = ContentAlignment.MiddleLeft;
		return label;
	}
}
