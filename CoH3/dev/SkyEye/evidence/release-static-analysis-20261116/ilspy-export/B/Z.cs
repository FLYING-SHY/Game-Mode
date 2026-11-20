using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using b;

namespace B;

internal sealed class Z
{
	public long A;

	public long a;

	public d A;

	public global::A.w A;

	public string A;

	public string a;

	public uint A;

	public bool A;

	public bool a;

	public Color A;

	public float A;

	public float a;

	public float B;
}
internal static class z
{
	public static void A(Graphics P_0, PointF P_1, bool P_2, string P_3, Color P_4, global::b.b P_5, Font P_6)
	{
		float num = (P_2 ? P_5.D : P_5.c);
		RectangleF rect = new RectangleF(P_1.X - num / 2f, P_1.Y - num / 2f, num, num);
		P_0.SmoothingMode = SmoothingMode.AntiAlias;
		using (Pen pen = new Pen(P_4, (float)P_5.C))
		{
			if (P_2)
			{
				P_0.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
			}
			else
			{
				P_0.DrawEllipse(pen, rect);
			}
		}
		TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
		Size size = TextRenderer.MeasureText(P_0, P_3 ?? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D(), P_6, new Size(32767, 32767), flags);
		Rectangle rectangle = new Rectangle((int)Math.Round(P_1.X - (float)size.Width / 2f), (int)Math.Round(P_1.Y - (float)size.Height / 2f), size.Width, size.Height);
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
			TextRenderer.DrawText(P_0, P_3, P_6, bounds, Color.Black, flags);
		}
		TextRenderer.DrawText(P_0, P_3, P_6, rectangle, P_4, flags);
	}
}
