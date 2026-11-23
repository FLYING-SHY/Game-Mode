using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;

namespace B;

internal sealed class V
{
	public Bitmap A;

	public PointF A;
}
internal sealed class v : IDisposable
{
	private readonly Dictionary<string, V> m_A = new Dictionary<string, V>();

	private t m_A;

	private Font m_A;

	private Font a;

	public void A(t P_0)
	{
		if (this.m_A == null || this.m_A.A != P_0.A || this.m_A.a != P_0.a || this.m_A.Count > 512)
		{
			A();
			this.m_A = P_0.B();
			this.m_A = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), P_0.a, FontStyle.Bold);
			a = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), P_0.A, FontStyle.Bold);
		}
	}

	public V A(r P_0)
	{
		string text = (P_0.A ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jg()).Replace('\r', ' ').Replace('\n', ' ');
		string text2 = Math.Max(1, P_0.A).ToString(CultureInfo.InvariantCulture);
		string key = P_0.A.ToArgb() + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + text + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + text2;
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
		value = new V
		{
			A = image2,
			A = new PointF((float)num / 2f, num2)
		};
		this.m_A[key] = value;
		return value;
	}

	private void A()
	{
		foreach (V value in this.m_A.Values)
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
