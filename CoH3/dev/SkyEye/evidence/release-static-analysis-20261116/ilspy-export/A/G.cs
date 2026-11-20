using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
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

			public static Func<k, bool> A;

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

			internal bool A(k P_0)
			{
				return P_0.A;
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public global::a.v A;

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
			public k A;

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
			public global::a.y A;

			public int A;

			public int a;

			public Func<float[], PointF> A;

			internal PointF A(float[] P_0)
			{
				return this.A.A(P_0[0], P_0[2], this.A, a);
			}
		}

		private static A A(global::a.v P_0, float[] P_1)
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

		public static PointF[] A(global::a.v P_0, IEnumerable<float[]> P_1, int P_2, int P_3)
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

		public static float[][] A(k P_0)
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

		public static float[][] a(k P_0)
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

		public static void A(Graphics P_0, IEnumerable<k> P_1, global::a.v P_2, J P_3, int P_4, int P_5, Font P_6)
		{
			P_0.SmoothingMode = SmoothingMode.AntiAlias;
			P_0.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			foreach (k item in P_1)
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

		public static void A(Graphics P_0, IEnumerable<k> P_1, global::a.y P_2, J P_3, int P_4, int P_5, Font P_6)
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
				foreach (k item in P_1.Where((k k2) => k2.A))
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
						A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.at(), P_6, item.A, pointF);
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
		public static bool A(string P_0)
		{
			string text = P_0.Substring(P_0.LastIndexOf('\\') + 1);
			if (!text.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DT()) && !text.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Dt()))
			{
				return text.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DU());
			}
			return true;
		}

		public static bool a(string P_0)
		{
			if (!P_0.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.@as()) || !P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Du()))
			{
				return false;
			}
			string text = P_0.Substring(P_0.LastIndexOf('\\') + 1);
			if (!text.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DV()) && !text.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Dv()))
			{
				return A(P_0);
			}
			return true;
		}
	}
	internal interface g
	{
		e A();
	}
}
