using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;

namespace B;

internal sealed class U
{
	public Bitmap A;

	public PointF A;
}
internal sealed class u : IDisposable
{
	private readonly Dictionary<string, U> m_A = new Dictionary<string, U>();

	private s m_A;

	private Font m_A;

	private Font a;

	public void A(s P_0)
	{
		if (this.m_A == null || this.m_A.A != P_0.A || this.m_A.a != P_0.a || this.m_A.Count > 512)
		{
			A();
			this.m_A = P_0.B();
			this.m_A = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), P_0.a, FontStyle.Bold);
			a = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), P_0.A, FontStyle.Bold);
		}
	}

	public U A(q P_0)
	{
		string text = (P_0.A ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jC()).Replace('\r', ' ').Replace('\n', ' ');
		string text2 = Math.Max(1, P_0.A).ToString(CultureInfo.InvariantCulture);
		string key = P_0.A.ToArgb() + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + text + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + text2;
		if (this.m_A.TryGetValue(key, out var value))
		{
			return value;
		}
		SizeF sizeF;
		SizeF sizeF2;
		using (Bitmap image = new Bitmap(1, 1))
		{
			using Graphics graphics = Graphics.FromImage(image);
			sizeF = graphics.MeasureString(text, this.m_A, 10000, StringFormat.GenericTypographic);
			sizeF2 = graphics.MeasureString(text2, a, 10000, StringFormat.GenericTypographic);
		}
		int num = (int)Math.Ceiling(Math.Max(sizeF2.Width + 8f, sizeF.Width + 8f));
		int height = (int)Math.Ceiling(sizeF.Height + sizeF2.Height + 12f);
		float num2 = sizeF.Height + 6f + sizeF2.Height / 2f;
		Bitmap image2 = new Bitmap(num, height, PixelFormat.Format32bppPArgb);
		using (Graphics graphics2 = Graphics.FromImage(image2))
		{
			using SolidBrush brush = new SolidBrush(P_0.A);
			graphics2.Clear(Color.Transparent);
			graphics2.SmoothingMode = SmoothingMode.AntiAlias;
			graphics2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			float num3 = ((float)num - sizeF.Width) / 2f;
			float num4 = ((float)num - sizeF2.Width) / 2f;
			float num5 = sizeF.Height + 6f;
			Point[] array = new Point[4]
			{
				new Point(-1, 0),
				new Point(1, 0),
				new Point(0, -1),
				new Point(0, 1)
			};
			for (int num6 = 0; num6 < array.Length; num6++)
			{
				Point point = array[num6];
				graphics2.DrawString(text, this.m_A, Brushes.Black, num3 + (float)point.X, 2 + point.Y, StringFormat.GenericTypographic);
				graphics2.DrawString(text2, a, Brushes.Black, num4 + (float)point.X, num5 + (float)point.Y, StringFormat.GenericTypographic);
			}
			graphics2.DrawString(text, this.m_A, brush, num3, 2f, StringFormat.GenericTypographic);
			graphics2.DrawString(text2, a, brush, num4, num5, StringFormat.GenericTypographic);
		}
		value = new U
		{
			A = image2,
			A = new PointF((float)num / 2f, num2)
		};
		this.m_A[key] = value;
		return value;
	}

	private void A()
	{
		foreach (U value in this.m_A.Values)
		{
			value.A.Dispose();
		}
		this.m_A.Clear();
		if (this.m_A != null)
		{
			this.m_A.Dispose();
			this.m_A = null;
		}
		if (a != null)
		{
			a.Dispose();
			a = null;
		}
		this.m_A = null;
	}

	public void Dispose()
	{
		A();
	}
}
