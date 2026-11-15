using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class Interface45
{
	private static Icon icon;

	public static readonly Color Canvas = Color.FromArgb(20, 20, 20);

	public static readonly Color Paper = Color.FromArgb(26, 26, 26);

	public static readonly Color Ink = Color.FromArgb(235, 235, 235);

	public static readonly Color Muted = Color.FromArgb(138, 138, 138);

	public static readonly Color Line = Color.FromArgb(42, 42, 42);

	public static readonly Color Accent = Color.FromArgb(255, 255, 255);

	public static readonly Color Night = Color.FromArgb(28, 28, 28);

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
