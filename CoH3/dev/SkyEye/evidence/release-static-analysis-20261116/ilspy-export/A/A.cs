using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using B;
using b;

namespace A
{
	[CompilerGenerated]
	internal sealed class A<A, a>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly A m_A;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly a m_A;

		public A A => this.A;

		public a a => this.A;

		[DebuggerHidden]
		public A(A P_0, a P_1)
		{
			this.A = P_0;
			this.A = P_1;
		}

		[DebuggerHidden]
		public override bool Equals(object P_0)
		{
			A<A, a> a2 = P_0 as A<A, a>;
			if (this != a2)
			{
				if (a2 != null && EqualityComparer<A>.Default.Equals(this.A, a2.A))
				{
					return EqualityComparer<a>.Default.Equals(this.A, a2.A);
				}
				return false;
			}
			return true;
		}

		[DebuggerHidden]
		public override int GetHashCode()
		{
			return (199105043 * -1521134295 + EqualityComparer<A>.Default.GetHashCode(this.A)) * -1521134295 + EqualityComparer<a>.Default.GetHashCode(this.A);
		}

		[DebuggerHidden]
		public override string ToString()
		{
			string format = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.A();
			object[] array = new object[2];
			A val = this.A;
			array[0] = ((val != null) ? val.ToString() : null);
			a val2 = this.A;
			array[1] = ((val2 != null) ? val2.ToString() : null);
			return string.Format(null, format, array);
		}
	}
	[CompilerGenerated]
	[a]
	internal sealed class a : Attribute
	{
	}
}
namespace a
{
	internal static class A
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.N, uint> A;

			public static Func<global::B.N, string> A;

			internal uint A(global::B.N P_0)
			{
				return P_0.A;
			}

			internal string a(global::B.N P_0)
			{
				return P_0.a;
			}
		}

		public static c A(IEnumerable<global::B.Z> P_0, IEnumerable<global::B.N> P_1, float P_2, float P_3, double P_4)
		{
			Dictionary<uint, string> dictionary = P_1.ToDictionary((global::B.N n2) => n2.A, (global::B.N n2) => n2.a);
			HashSet<uint> hashSet = new HashSet<uint>();
			HashSet<uint> hashSet2 = new HashSet<uint>();
			HashSet<uint> hashSet3 = new HashSet<uint>();
			HashSet<uint> hashSet4 = new HashSet<uint>();
			foreach (global::B.Z item in P_0)
			{
				if (!item.a && dictionary.TryGetValue(item.A, out var value) && !(value == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Dk()))
				{
					bool flag = value == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bA() || value == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DL();
					bool num = A(item.A, item.B, P_2, P_3, P_4);
					if (num)
					{
						(flag ? hashSet : hashSet2).Add(item.A);
					}
					if (num || (item.A != null && A(item.A.A, item.A.B, P_2, P_3, P_4)))
					{
						(flag ? hashSet3 : hashSet4).Add(item.A);
					}
				}
			}
			return new c
			{
				A = hashSet.Count,
				a = hashSet2.Count,
				B = hashSet3.Count,
				b = hashSet4.Count
			};
		}

		private static bool A(float P_0, float P_1, float P_2, float P_3, double P_4)
		{
			return (double)((P_0 - P_2) * (P_0 - P_2) + (P_1 - P_3) * (P_1 - P_3)) <= P_4 * P_4;
		}

		public static bool A(v P_0, PointF P_1, int P_2, int P_3, float P_4, out float P_5, out float P_6)
		{
			double[] array = new double[16];
			for (int num = 0; num < 4; num++)
			{
				for (int num2 = 0; num2 < 4; num2++)
				{
					for (int num3 = 0; num3 < 4; num3++)
					{
						array[num * 4 + num2] += P_0.A[num * 4 + num3] * P_0.a[num3 * 4 + num2];
					}
				}
			}
			double num4 = P_1.X * 2f / (float)P_2 - 1f;
			double num5 = 1f - P_1.Y * 2f / (float)P_3;
			double num6 = array[0] - num4 * array[3];
			double num7 = array[8] - num4 * array[11];
			double num8 = num4 * array[15] - array[12] - (double)P_4 * (array[4] - num4 * array[7]);
			double num9 = array[1] - num5 * array[3];
			double num10 = array[9] - num5 * array[11];
			double num11 = num5 * array[15] - array[13] - (double)P_4 * (array[5] - num5 * array[7]);
			double num12 = num6 * num10 - num7 * num9;
			P_5 = (P_6 = 0f);
			if (Math.Abs(num12) < 1E-08)
			{
				return false;
			}
			P_5 = (float)((num8 * num10 - num7 * num11) / num12);
			P_6 = (float)((num6 * num11 - num8 * num9) / num12);
			if (global::b.a.A(P_5, P_4, P_6) && P_0.A(P_5, P_4, P_6, P_2, P_3, out var pointF) && Math.Abs(pointF.X - P_1.X) < 2f)
			{
				return Math.Abs(pointF.Y - P_1.Y) < 2f;
			}
			return false;
		}
	}
	internal static class a
	{
		public static PointF[] A(B P_0, y P_1, int P_2, int P_3)
		{
			PointF[] array = new PointF[193];
			for (int num = 0; num < array.Length; num++)
			{
				double num2 = (double)num * Math.PI / 96.0;
				ref PointF reference = ref array[num];
				reference = P_1.A(P_0.A + (float)Math.Cos(num2) * P_0.B, P_0.a + (float)Math.Sin(num2) * P_0.B, P_2, P_3);
			}
			return array;
		}

		public static void A(Graphics P_0, B P_1, y P_2, int P_3, int P_4)
		{
			if (P_1 == null || P_2 == null || !P_2.A)
			{
				return;
			}
			GraphicsState gstate = P_0.Save();
			try
			{
				using (GraphicsPath graphicsPath = new GraphicsPath())
				{
					graphicsPath.AddPolygon(P_2.A(P_3, P_4));
					P_0.SetClip(graphicsPath, CombineMode.Intersect);
				}
				P_0.SmoothingMode = SmoothingMode.AntiAlias;
				using Pen pen = new Pen(P_1.A, P_1.b);
				P_0.DrawLines(pen, A(P_1, P_2, P_3, P_4));
				if (P_2.A(P_1.A, P_1.a, P_3, P_4, out var pointF))
				{
					P_0.DrawEllipse(pen, pointF.X - 2.5f, pointF.Y - 2.5f, 5f, 5f);
				}
			}
			finally
			{
				P_0.Restore(gstate);
			}
		}
	}
}
