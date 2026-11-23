using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using a;

namespace A
{
	internal sealed class G
	{
		public string A;

		public string a;

		public float A;

		public float a;

		public float B;

		public float b;

		public int A;

		public string[] A;

		public bool A;
	}
	internal static class g
	{
		private struct A
		{
			public double A;

			public double a;

			public double B;

			public static A A(A P_0, A P_1, double P_2)
			{
				return new A
				{
					A = P_0.A + (P_1.A - P_0.A) * P_2,
					a = P_0.a + (P_1.a - P_0.a) * P_2,
					B = P_0.B + (P_1.B - P_0.B) * P_2
				};
			}
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class a
		{
			public static readonly a A = new a();

			public static Func<A, bool> A;

			public static Func<A, double> A;

			public static Func<A, double> a;

			public static Func<A, double> B;

			public static Func<A, double> b;

			public static Func<A, double> C;

			public static Func<L, bool> A;

			internal bool A(A P_0)
			{
				if (!double.IsNaN(P_0.A) && !double.IsNaN(P_0.a) && !double.IsNaN(P_0.B) && !double.IsInfinity(P_0.A) && !double.IsInfinity(P_0.a))
				{
					return double.IsInfinity(P_0.B);
				}
				return true;
			}

			internal double a(A P_0)
			{
				return P_0.B - 0.05;
			}

			internal double B(A P_0)
			{
				return P_0.A + P_0.B;
			}

			internal double b(A P_0)
			{
				return P_0.B - P_0.A;
			}

			internal double C(A P_0)
			{
				return P_0.a + P_0.B;
			}

			internal double c(A P_0)
			{
				return P_0.B - P_0.a;
			}

			internal bool A(L P_0)
			{
				return P_0.A;
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public global::a.w A;

			public int A;

			public int a;

			internal A A(float[] P_0)
			{
				return g.A(this.A, P_0);
			}

			internal PointF A(A P_0)
			{
				return new PointF((float)((1.0 + P_0.A / P_0.B) * (double)this.A / 2.0), (float)((1.0 - P_0.a / P_0.B) * (double)a / 2.0));
			}
		}

		[CompilerGenerated]
		private sealed class b
		{
			public L A;

			internal float[] A(int P_0)
			{
				double num = (double)P_0 * Math.PI / 32.0;
				return new float[3]
				{
					this.A.A + this.A.E * (float)Math.Cos(num),
					this.A.a + 0.15f,
					this.A.B + this.A.E * (float)Math.Sin(num)
				};
			}
		}

		[CompilerGenerated]
		private sealed class C
		{
			public global::a.z A;

			public int A;

			public int a;

			public Func<float[], PointF> A;

			internal PointF A(float[] P_0)
			{
				return this.A.A(P_0[0], P_0[2], this.A, a);
			}
		}

		private static A A(global::a.w P_0, float[] P_1)
		{
			double num = P_1[0];
			double num2 = P_1[1];
			double num3 = P_1[2];
			double num4 = num * (double)P_0.A[0] + num2 * (double)P_0.A[4] + num3 * (double)P_0.A[8] + (double)P_0.A[12];
			double num5 = num * (double)P_0.A[1] + num2 * (double)P_0.A[5] + num3 * (double)P_0.A[9] + (double)P_0.A[13];
			double num6 = num * (double)P_0.A[2] + num2 * (double)P_0.A[6] + num3 * (double)P_0.A[10] + (double)P_0.A[14];
			return new A
			{
				A = num4 * (double)P_0.a[0] + num5 * (double)P_0.a[4] + num6 * (double)P_0.a[8] + (double)P_0.a[12],
				a = num4 * (double)P_0.a[1] + num5 * (double)P_0.a[5] + num6 * (double)P_0.a[9] + (double)P_0.a[13],
				B = num4 * (double)P_0.a[3] + num5 * (double)P_0.a[7] + num6 * (double)P_0.a[11] + (double)P_0.a[15]
			};
		}

		public static PointF[] A(global::a.w P_0, IEnumerable<float[]> P_1, int P_2, int P_3)
		{
			List<A> list = P_1.Select((float[] array2) => A(P_0, array2)).ToList();
			if (list.Any((A a3) => double.IsNaN(a3.A) || double.IsNaN(a3.a) || double.IsNaN(a3.B) || double.IsInfinity(a3.A) || double.IsInfinity(a3.a) || double.IsInfinity(a3.B)))
			{
				return new PointF[0];
			}
			Func<A, double>[] array = new Func<A, double>[5]
			{
				(A a3) => a3.B - 0.05,
				(A a3) => a3.A + a3.B,
				(A a3) => a3.B - a3.A,
				(A a3) => a3.a + a3.B,
				(A a3) => a3.B - a3.a
			};
			foreach (Func<A, double> func in array)
			{
				if (list.Count < 3)
				{
					return new PointF[0];
				}
				List<A> list2 = new List<A>();
				A a2 = list.Last();
				double num2 = func(a2);
				foreach (A item in list)
				{
					double num3 = func(item);
					if (num2 < 0.0 != num3 < 0.0)
					{
						list2.Add(g.A.A(a2, item, num2 / (num2 - num3)));
					}
					if (num3 >= 0.0)
					{
						list2.Add(item);
					}
					a2 = item;
					num2 = num3;
				}
				list = list2;
			}
			return list.Select((A a3) => new PointF((float)((1.0 + a3.A / a3.B) * (double)P_2 / 2.0), (float)((1.0 - a3.a / a3.B) * (double)P_3 / 2.0))).ToArray();
		}

		public static float[][] A(L P_0)
		{
			float num = P_0.A + P_0.b * P_0.d;
			float num2 = P_0.B + P_0.C * P_0.d;
			float num3 = num + P_0.b * P_0.D;
			float num4 = num2 + P_0.C * P_0.D;
			float num5 = (0f - P_0.C) * P_0.c;
			float num6 = P_0.b * P_0.c;
			float num7 = P_0.a + 0.15f;
			return new float[4][]
			{
				new float[3]
				{
					num + num5,
					num7,
					num2 + num6
				},
				new float[3]
				{
					num3 + num5,
					num7,
					num4 + num6
				},
				new float[3]
				{
					num3 - num5,
					num7,
					num4 - num6
				},
				new float[3]
				{
					num - num5,
					num7,
					num2 - num6
				}
			};
		}

		public static float[][] a(L P_0)
		{
			return Enumerable.Range(0, 64).Select(delegate(int num2)
			{
				double num = (double)num2 * Math.PI / 32.0;
				return new float[3]
				{
					P_0.A + P_0.E * (float)Math.Cos(num),
					P_0.a + 0.15f,
					P_0.B + P_0.E * (float)Math.Sin(num)
				};
			}).ToArray();
		}

		public static void A(Graphics P_0, string P_1, Font P_2, Color P_3, PointF P_4)
		{
			SizeF sizeF = P_0.MeasureString(P_1, P_2, 10000, StringFormat.GenericTypographic);
			float num = P_4.X - sizeF.Width / 2f;
			float num2 = P_4.Y - sizeF.Height / 2f;
			using SolidBrush brush = new SolidBrush(P_3);
			Point[] array = new Point[4]
			{
				new Point(-1, 0),
				new Point(1, 0),
				new Point(0, -1),
				new Point(0, 1)
			};
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				Point point = array[num3];
				P_0.DrawString(P_1, P_2, Brushes.Black, num + (float)point.X, num2 + (float)point.Y, StringFormat.GenericTypographic);
			}
			P_0.DrawString(P_1, P_2, brush, num, num2, StringFormat.GenericTypographic);
		}

		public static void A(Graphics P_0, IEnumerable<L> P_1, global::a.w P_2, j P_3, int P_4, int P_5, Font P_6)
		{
			P_0.SmoothingMode = SmoothingMode.AntiAlias;
			P_0.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			foreach (L item in P_1)
			{
				if (item.A ? (!P_3.a) : (!P_3.A))
				{
					continue;
				}
				PointF[] array = A(P_2, item.A ? a(item) : A(item), P_4, P_5);
				if (array.Length < 3)
				{
					continue;
				}
				using (Pen pen = new Pen(item.A, (float)P_3.A))
				{
					if (!item.A)
					{
						using SolidBrush brush = new SolidBrush(Color.FromArgb(P_3.A * 255 / 100, item.A));
						P_0.FillPolygon(brush, array);
					}
					P_0.DrawPolygon(pen, array);
				}
				float num = item.d + item.D / 2f;
				if (P_2.A(item.A + (item.A ? 0f : (item.b * num)), item.a + 0.15f, item.B + (item.A ? 0f : (item.C * num)), P_4, P_5, out var pointF))
				{
					A(P_0, item.a, P_6, item.A, pointF);
				}
			}
		}

		public static void A(Graphics P_0, IEnumerable<L> P_1, global::a.z P_2, j P_3, int P_4, int P_5, Font P_6)
		{
			if (!P_3.B || P_2 == null || !P_2.A)
			{
				return;
			}
			GraphicsState gstate = P_0.Save();
			try
			{
				using (GraphicsPath graphicsPath = new GraphicsPath())
				{
					graphicsPath.AddPolygon(P_2.A(P_4, P_5));
					P_0.SetClip(graphicsPath, CombineMode.Intersect);
				}
				P_0.SmoothingMode = SmoothingMode.AntiAlias;
				P_0.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				foreach (L item in P_1.Where((L l2) => l2.A))
				{
					PointF[] points = (from array in a(item)
						select P_2.A(array[0], array[2], P_4, P_5)).ToArray();
					using (SolidBrush brush = new SolidBrush(Color.FromArgb(P_3.a * 255 / 100, item.A)))
					{
						using Pen pen = new Pen(item.A, 1.5f);
						P_0.FillPolygon(brush, points);
						P_0.DrawPolygon(pen, points);
					}
					if (P_2.A(item.A, item.B, P_4, P_5, out var pointF))
					{
						A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.at(), P_6, item.A, pointF);
					}
				}
			}
			finally
			{
				P_0.Restore(gstate);
			}
		}
	}
}
namespace a
{
	internal static class G
	{
		internal enum A
		{
			A,
			a,
			B,
			b
		}

		internal enum a
		{
			A,
			a,
			B
		}

		public const string A = "2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde";

		public const string a = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec";

		private static readonly object m_A = new object();

		private static string m_B;

		private static A m_A = G.A.A;

		private static a m_A = G.a.A;

		private static int m_A;

		private static int m_a;

		internal static A A => (A)Volatile.Read(ref G.m_A);

		internal static a a => (a)Volatile.Read(ref G.m_a);

		internal static bool A(string P_0)
		{
			return a(P_0) != G.a.A;
		}

		internal static a a(string P_0)
		{
			if (string.Equals(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DR(), StringComparison.OrdinalIgnoreCase))
			{
				return G.a.a;
			}
			if (string.Equals(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dr(), StringComparison.OrdinalIgnoreCase))
			{
				return G.a.B;
			}
			return G.a.A;
		}

		internal static void A(A P_0, bool P_1 = false)
		{
			string message;
			switch (P_0)
			{
			default:
				message = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DS();
				break;
			case G.A.B:
				message = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ds();
				break;
			case G.A.a:
				return;
			}
			throw new InvalidOperationException(message);
		}

		public static A B(string P_0)
		{
			a a2;
			return A(P_0, out a2);
		}

		internal static A A(string P_0, out a P_1)
		{
			lock (G.m_A)
			{
				try
				{
					FileInfo fileInfo = new FileInfo(P_0);
					string text = fileInfo.FullName + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + fileInfo.Length + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + fileInfo.LastWriteTimeUtc.Ticks;
					if (text == G.m_B)
					{
						P_1 = G.m_A;
						return G.m_A;
					}
					string text2;
					using (FileStream inputStream = File.OpenRead(P_0))
					{
						using SHA256 sHA = SHA256.Create();
						text2 = BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CJ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).ToLowerInvariant();
					}
					a a2 = a(text2);
					Volatile.Write(ref G.m_a, (int)a2);
					A value = ((a2 != G.a.A) ? G.A.a : G.A.B);
					Volatile.Write(ref G.m_A, (int)value);
					G.m_B = text;
					G.m_A = value;
					G.m_A = a2;
					P_1 = a2;
				}
				catch
				{
					Volatile.Write(ref G.m_A, 3);
					Volatile.Write(ref G.m_a, 0);
					G.m_B = null;
					G.m_A = G.A.b;
					G.m_A = G.a.A;
					P_1 = G.a.A;
				}
				return G.A;
			}
		}
	}
	internal static class g
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<string, bool> A;

			internal bool A(string P_0)
			{
				return g.m_A.Contains(P_0.Replace('/', '\\'));
			}
		}

		private static readonly HashSet<string> m_A = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DV(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dv(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DW()
		};

		public static bool A(string P_0)
		{
			if (P_0 != null)
			{
				return g.m_A.Contains(P_0.Replace('/', '\\'));
			}
			return false;
		}

		public static int A(float P_0)
		{
			if (float.IsNaN(P_0) || float.IsInfinity(P_0) || P_0 < 0f || P_0 > 80000f)
			{
				throw new ArgumentOutOfRangeException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DT());
			}
			return (int)Math.Ceiling((double)P_0 / 8.0);
		}

		public static string A(global::B.O P_0)
		{
			if (a(P_0))
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dt();
			}
			if (!P_0.D || !P_0.A.HasValue)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DU() + A(P_0.A.Value) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Du();
		}

		public static bool a(global::B.O P_0)
		{
			return P_0.A.Any((string text) => g.m_A.Contains(text.Replace('/', '\\')));
		}
	}
}
