using System;
using System.Collections.Generic;
using System.Drawing;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using B;
using b;

namespace A
{
	internal sealed class Y
	{
		public bool A;

		public float A;

		public float a;

		public float B;

		public Color A;

		public PointF[] A;

		public Rectangle A;
	}
	internal sealed class y
	{
		public global::B.N A;

		public override string ToString()
		{
			return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz() + (A.A + 1) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DK() + A.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DK() + U.a(A);
		}
	}
}
namespace a
{
	internal static class Y
	{
		public static PointF[] A(PointF P_0, float P_1)
		{
			List<PointF> list = new List<PointF>();
			float num = P_1 / 2f;
			double[] array = new double[3]
			{
				Math.PI / 2.0,
				Math.PI / 6.0,
				Math.PI * 5.0 / 6.0
			};
			foreach (double num3 in array)
			{
				float num4 = (float)Math.Cos(num3) * num;
				float num5 = (float)Math.Sin(num3) * num;
				list.Add(new PointF(P_0.X - num4, P_0.Y - num5));
				list.Add(new PointF(P_0.X + num4, P_0.Y + num5));
			}
			return list.ToArray();
		}

		public static void A(Graphics P_0, IEnumerable<global::A.s> P_1, y P_2, global::B.B P_3, int P_4, int P_5)
		{
			if (!P_3.b || P_1 == null)
			{
				return;
			}
			foreach (global::A.s item in P_1)
			{
				if (!P_2.A(item.A, item.B, P_4, P_5, out var pointF))
				{
					continue;
				}
				PointF[] array = A(pointF, P_3.c);
				using Pen pen = new Pen(Color.Black, 3.5f);
				using Pen pen2 = new Pen(item.A, 1.5f);
				for (int num = 0; num < array.Length; num += 2)
				{
					P_0.DrawLine(pen, array[num], array[num + 1]);
				}
				for (int num2 = 0; num2 < array.Length; num2 += 2)
				{
					P_0.DrawLine(pen2, array[num2], array[num2 + 1]);
				}
			}
		}
	}
	internal sealed class y
	{
		public bool A;

		public x A;

		public float A;

		public float a;

		public float B;

		public float b;

		public float C;

		public float c;

		public float D;

		public float d;

		public float E;

		public bool A
		{
			get
			{
				if (D >= 8f && d >= 8f && B >= 10f && b >= 10f && C > 0f)
				{
					return c > 0f;
				}
				return false;
			}
		}

		public PointF A(float P_0, float P_1, int P_2, int P_3)
		{
			if (this.A)
			{
				PointF pointF = this.A.a(P_0, P_1);
				return new PointF(pointF.X * (float)P_2 / C, pointF.Y * (float)P_3 / c);
			}
			double num = (double)E * Math.PI / 180.0;
			double num2 = P_0 / D * B;
			double num3 = (0f - P_1) / d * b;
			return new PointF((float)(((double)this.A + num2 * Math.Cos(num) - num3 * Math.Sin(num)) * (double)P_2 / (double)C), (float)(((double)a + num2 * Math.Sin(num) + num3 * Math.Cos(num)) * (double)P_3 / (double)c));
		}

		public bool A(float P_0, float P_1, int P_2, int P_3, out PointF P_4)
		{
			P_4 = default(PointF);
			if (!this.A || !global::b.a.A(P_0, 0f, P_1) || Math.Abs(P_0) > D / 2f || Math.Abs(P_1) > d / 2f)
			{
				return false;
			}
			P_4 = A(P_0, P_1, P_2, P_3);
			return true;
		}

		public PointF[] A(int P_0, int P_1)
		{
			return new PointF[4]
			{
				A((0f - D) / 2f, (0f - d) / 2f, P_0, P_1),
				A(D / 2f, (0f - d) / 2f, P_0, P_1),
				A(D / 2f, d / 2f, P_0, P_1),
				A((0f - D) / 2f, d / 2f, P_0, P_1)
			};
		}
	}
}
