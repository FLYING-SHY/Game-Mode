using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Management;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using b;

namespace A
{
	internal static class W
	{
		public static string A => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cm(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cN());

		public static string A(l P_0)
		{
			string path = P_0 switch
			{
				l.B => CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cn(), 
				l.a => CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cO(), 
				l.c => CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.co(), 
				l.A => CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cP(), 
				l.b => CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cp(), 
				l.D => CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cQ(), 
				l.C => CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cq(), 
				_ => throw new ArgumentOutOfRangeException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cR()), 
			};
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cm(), path);
		}
	}
	internal static class w
	{
		private static readonly object m_A = new object();

		private static bool m_A;

		private static string m_A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		private static string a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		internal static string A(string P_0)
		{
			P_0 = (P_0 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).Trim();
			if (P_0.Length == 0 || P_0.Length > 64)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
			string text = P_0;
			for (int num = 0; num < text.Length; num++)
			{
				if (char.IsControl(text[num]))
				{
					return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				}
			}
			string text2 = P_0.ToLowerInvariant().Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CJ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D())
				.Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CK(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
			if (text2.Length != 0)
			{
				if (text2 == null)
				{
					goto IL_0175;
				}
				switch (text2.Length)
				{
				case 13:
					break;
				case 15:
					goto IL_00f3;
				case 18:
					goto IL_0129;
				case 7:
					goto IL_013b;
				case 4:
					goto IL_014a;
				case 2:
					goto IL_0159;
				case 3:
					goto IL_0168;
				default:
					goto IL_0175;
				}
				char c2 = text2[0];
				if (c2 != 'd')
				{
					if (c2 != 'n' || !(text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cs()))
					{
						goto IL_0175;
					}
				}
				else if (!(text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cS()))
				{
					goto IL_0175;
				}
			}
			goto IL_01c4;
			IL_014a:
			if (!(text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cU()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_0168:
			if (!(text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cV()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_00f3:
			if (!(text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cr()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_0129:
			if (!(text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cT()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_0159:
			if (!(text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cu()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_0175:
			bool flag = true;
			bool flag2 = true;
			text = text2;
			foreach (char c3 in text)
			{
				flag = flag && c3 == '0';
				flag2 = flag2 && c3 == 'f';
			}
			if (flag || flag2)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
			return P_0;
			IL_013b:
			if (!(text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ct()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_01c4:
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}

		private static void A()
		{
			lock (w.m_A)
			{
				if (w.m_A)
				{
					return;
				}
				w.m_A = true;
				string[] array = new string[2]
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cv(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cW()
				};
				foreach (string text in array)
				{
					try
					{
						EnumerationOptions options = new EnumerationOptions
						{
							Timeout = TimeSpan.FromSeconds(2.0),
							ReturnImmediately = false
						};
						using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cw(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cX() + text, options);
						using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
						foreach (ManagementBaseObject item in managementObjectCollection)
						{
							using (item)
							{
								string text2 = A(Convert.ToString(item[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cx()], CultureInfo.InvariantCulture));
								if (text2.Length == 0)
								{
									continue;
								}
								w.m_A = text2;
								a = ((text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cv()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cy() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cY());
								return;
							}
						}
					}
					catch
					{
					}
				}
			}
		}

		public static void A(Dictionary<string, object> P_0)
		{
			A();
			if (w.m_A.Length > 0)
			{
				P_0[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cZ()] = w.m_A;
				P_0[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cz()] = a;
			}
		}
	}
}
namespace a
{
	internal sealed class W : IDisposable
	{
		private readonly Dictionary<string, Bitmap> m_A = new Dictionary<string, Bitmap>();

		private global::b.c m_A;

		private Font m_A;

		public void A(global::b.c P_0)
		{
			if (this.m_A.Count > 1024 || this.m_A == null || this.m_A.c != P_0.c || this.m_A.D != P_0.D || this.m_A.d != P_0.d || !(this.m_A.C == P_0.C))
			{
				A();
				this.m_A = P_0.A();
				this.m_A = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), P_0.d, FontStyle.Bold);
			}
		}

		public Bitmap A(global::b.A P_0)
		{
			string key = (P_0.a ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EV() : (P_0.A ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Eu() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EU())) + P_0.A.ToArgb() + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_0.a;
			if (this.m_A.TryGetValue(key, out var value))
			{
				return value;
			}
			float num = (P_0.A ? this.m_A.D : this.m_A.c);
			SizeF sizeF;
			using (Bitmap image = new Bitmap(1, 1))
			{
				using Graphics graphics = Graphics.FromImage(image);
				sizeF = graphics.MeasureString(P_0.a ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), this.m_A, 10000, StringFormat.GenericTypographic);
			}
			int num2 = (int)Math.Ceiling(Math.Max(num + (float)this.m_A.C + 8f, sizeF.Width + 8f));
			int num3 = (int)Math.Ceiling(Math.Max(num + (float)this.m_A.C + 8f, sizeF.Height + 8f));
			value = new Bitmap(num2, num3, PixelFormat.Format32bppPArgb);
			using (Graphics graphics2 = Graphics.FromImage(value))
			{
				using Pen pen = new Pen(P_0.A, (float)this.m_A.C);
				using SolidBrush brush = new SolidBrush(P_0.A);
				graphics2.Clear(Color.Transparent);
				graphics2.SmoothingMode = SmoothingMode.AntiAlias;
				graphics2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				RectangleF rect = new RectangleF(((float)num2 - num) / 2f, ((float)num3 - num) / 2f, num, num);
				if (!P_0.a)
				{
					if (P_0.A)
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
				for (int num6 = 0; num6 < array.Length; num6++)
				{
					Point point = array[num6];
					graphics2.DrawString(P_0.a ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), this.m_A, Brushes.Black, num4 + (float)point.X, num5 + (float)point.Y, StringFormat.GenericTypographic);
				}
				graphics2.DrawString(P_0.a ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), this.m_A, brush, num4, num5, StringFormat.GenericTypographic);
			}
			this.m_A.Add(key, value);
			return value;
		}

		public Bitmap a(global::b.A P_0)
		{
			string key = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ev() + P_0.A.ToArgb() + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_0.a;
			if (this.m_A.TryGetValue(key, out var value))
			{
				return value;
			}
			string[] array = (P_0.a ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).Split('\n');
			float num = 0f;
			float num2;
			using (Bitmap image = new Bitmap(1, 1))
			{
				using Graphics graphics = Graphics.FromImage(image);
				num2 = (float)Math.Ceiling(this.m_A.GetHeight(graphics) + 2f);
				string[] array2 = array;
				foreach (string text in array2)
				{
					num = Math.Max(num, graphics.MeasureString(text, this.m_A, 10000, StringFormat.GenericTypographic).Width);
				}
			}
			value = new Bitmap(Math.Max(1, (int)Math.Ceiling(num + 8f)), Math.Max(1, (int)Math.Ceiling(num2 * (float)array.Length + 8f)), PixelFormat.Format32bppPArgb);
			using (Graphics graphics2 = Graphics.FromImage(value))
			{
				using SolidBrush brush = new SolidBrush(P_0.A);
				graphics2.Clear(Color.Transparent);
				graphics2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				for (int num4 = 0; num4 < array.Length; num4++)
				{
					float num5 = ((float)value.Width - graphics2.MeasureString(array[num4], this.m_A, 10000, StringFormat.GenericTypographic).Width) / 2f;
					float num6 = 4f + (float)num4 * num2;
					Point[] array3 = new Point[4]
					{
						new Point(-1, 0),
						new Point(1, 0),
						new Point(0, -1),
						new Point(0, 1)
					};
					for (int num7 = 0; num7 < array3.Length; num7++)
					{
						Point point = array3[num7];
						graphics2.DrawString(array[num4], this.m_A, Brushes.Black, num5 + (float)point.X, num6 + (float)point.Y, StringFormat.GenericTypographic);
					}
					graphics2.DrawString(array[num4], this.m_A, brush, num5, num6, StringFormat.GenericTypographic);
				}
			}
			this.m_A.Add(key, value);
			return value;
		}

		private void A()
		{
			foreach (Bitmap value in this.m_A.Values)
			{
				value.Dispose();
			}
			this.m_A.Clear();
			this.m_A = null;
			if (this.m_A != null)
			{
				this.m_A.Dispose();
				this.m_A = null;
			}
		}

		public void a()
		{
			A();
		}

		public void Dispose()
		{
			A();
		}
	}
	internal sealed class w
	{
		public float[] A;

		public float[] a;

		public bool A(float P_0, float P_1, float P_2, int P_3, int P_4, out PointF P_5)
		{
			float num = P_0 * this.A[0] + P_1 * this.A[4] + P_2 * this.A[8] + this.A[12];
			float num2 = P_0 * this.A[1] + P_1 * this.A[5] + P_2 * this.A[9] + this.A[13];
			float num3 = P_0 * this.A[2] + P_1 * this.A[6] + P_2 * this.A[10] + this.A[14];
			float num4 = num * a[0] + num2 * a[4] + num3 * a[8] + a[12];
			float num5 = num * a[1] + num2 * a[5] + num3 * a[9] + a[13];
			float num6 = num * a[3] + num2 * a[7] + num3 * a[11] + a[15];
			P_5 = default(PointF);
			if (num6 < 0.05f)
			{
				return false;
			}
			float num7 = num4 / num6;
			float num8 = num5 / num6;
			if (float.IsNaN(num7) || float.IsNaN(num8) || Math.Abs(num7) > 1f || Math.Abs(num8) > 1f)
			{
				return false;
			}
			P_5 = new PointF((num7 + 1f) * (float)P_3 / 2f, (1f - num8) * (float)P_4 / 2f);
			return true;
		}
	}
}
