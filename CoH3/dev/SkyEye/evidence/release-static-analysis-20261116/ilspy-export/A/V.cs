using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using B;
using a;
using b;

namespace A
{
	internal static class V
	{
		[DllImport("user32.dll", EntryPoint = "SetProcessDPIAware")]
		private static extern bool A();

		[STAThread]
		public static int A(string[] P_0)
		{
			A();
			q.A();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			bool createdNew;
			using (new Mutex(initiallyOwned: true, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ck(), out createdNew))
			{
				if (!createdNew)
				{
					MessageBox.Show(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cL(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cl());
					return 1;
				}
				Application.Run(new global::a.j());
			}
			return 0;
		}
	}
	internal static class v
	{
		public static string A => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cM(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cm());

		public static string A(L P_0)
		{
			string path = P_0 switch
			{
				L.B => C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cN(), 
				L.a => C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cn(), 
				L.c => C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cO(), 
				L.A => C4318269_002D39F2_002D4549_002D851A_002D418926B36435.co(), 
				L.b => C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cP(), 
				L.D => C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cp(), 
				L.C => C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cQ(), 
				_ => throw new ArgumentOutOfRangeException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cq()), 
			};
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cM(), path);
		}
	}
}
namespace a
{
	internal sealed class V : IDisposable
	{
		private readonly Dictionary<string, Bitmap> m_A = new Dictionary<string, Bitmap>();

		private global::b.b m_A;

		private Font m_A;

		public void A(global::b.b P_0)
		{
			if (this.m_A.Count > 1024 || this.m_A == null || this.m_A.c != P_0.c || this.m_A.D != P_0.D || this.m_A.d != P_0.d || !(this.m_A.C == P_0.C))
			{
				A();
				this.m_A = P_0.A();
				this.m_A = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), P_0.d, FontStyle.Bold);
			}
		}

		public Bitmap A(global::B.Z P_0)
		{
			string key = (P_0.a ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Er() : (P_0.A ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ER() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Eq())) + P_0.A.ToArgb() + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ax() + P_0.a;
			if (this.m_A.TryGetValue(key, out var value))
			{
				return value;
			}
			float num = (P_0.A ? this.m_A.D : this.m_A.c);
			SizeF sizeF;
			using (Bitmap image = new Bitmap(1, 1))
			{
				using Graphics graphics = Graphics.FromImage(image);
				sizeF = graphics.MeasureString(P_0.a ?? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D(), this.m_A, 10000, StringFormat.GenericTypographic);
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
					graphics2.DrawString(P_0.a ?? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D(), this.m_A, Brushes.Black, num4 + (float)point.X, num5 + (float)point.Y, StringFormat.GenericTypographic);
				}
				graphics2.DrawString(P_0.a ?? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D(), this.m_A, brush, num4, num5, StringFormat.GenericTypographic);
			}
			this.m_A.Add(key, value);
			return value;
		}

		public Bitmap a(global::B.Z P_0)
		{
			string key = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ES() + P_0.A.ToArgb() + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ax() + P_0.a;
			if (this.m_A.TryGetValue(key, out var value))
			{
				return value;
			}
			string[] array = (P_0.a ?? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D()).Split('\n');
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
	internal sealed class v
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
