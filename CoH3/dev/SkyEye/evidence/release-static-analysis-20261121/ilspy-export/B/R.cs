using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using A;
using a;

namespace B;

internal static class R
{
	[Serializable]
	[CompilerGenerated]
	private sealed class A
	{
		public static readonly A A = new A();

		public static Func<PointF, float> A;

		public static Func<PointF, float> a;

		public static Func<PointF, float> B;

		public static Func<PointF, float> b;

		internal float A(PointF P_0)
		{
			return P_0.X;
		}

		internal float a(PointF P_0)
		{
			return P_0.Y;
		}

		internal float B(PointF P_0)
		{
			return P_0.X;
		}

		internal float b(PointF P_0)
		{
			return P_0.Y;
		}
	}

	private static void A(List<global::A.y> P_0, List<PointF> P_1, Color P_2, float P_3)
	{
		if (P_1.Count < 2)
		{
			P_1.Clear();
			return;
		}
		int num = (int)Math.Ceiling(P_3 + 3f);
		P_0.Add(new global::A.y
		{
			A = P_2,
			A = P_3,
			A = P_1.ToArray(),
			A = Rectangle.FromLTRB((int)Math.Floor(P_1.Min((PointF pointF) => pointF.X)) - num, (int)Math.Floor(P_1.Min((PointF pointF) => pointF.Y)) - num, (int)Math.Ceiling(P_1.Max((PointF pointF) => pointF.X)) + num, (int)Math.Ceiling(P_1.Max((PointF pointF) => pointF.Y)) + num)
		});
		P_1.Clear();
	}

	public static List<global::A.y> A(IEnumerable<r> P_0, global::a.w P_1, t P_2, int P_3, int P_4)
	{
		List<global::A.y> list = new List<global::A.y>();
		if (!P_2.a || !P_2.b)
		{
			return list;
		}
		foreach (r item3 in P_0)
		{
			if (!P_2.A(item3) || item3.b <= 0f)
			{
				continue;
			}
			List<PointF> list2 = new List<PointF>();
			for (int num = 0; num < 64; num++)
			{
				double num2 = (double)num * Math.PI / 32.0;
				double num3 = (double)(num + 1) * Math.PI / 32.0;
				float[] array = new float[3]
				{
					item3.A + item3.b * (float)Math.Cos(num2),
					item3.a + 0.15f,
					item3.B + item3.b * (float)Math.Sin(num2)
				};
				float[] array2 = new float[3]
				{
					item3.A + item3.b * (float)Math.Cos(num3),
					item3.a + 0.15f,
					item3.B + item3.b * (float)Math.Sin(num3)
				};
				if (!F.A(P_1, array, array2, P_3, P_4, out var item, out var item2))
				{
					A(list, list2, item3.A, (float)P_2.A);
					continue;
				}
				if (list2.Count > 0 && Math.Abs(list2.Last().X - item.X) + Math.Abs(list2.Last().Y - item.Y) > 1f)
				{
					A(list, list2, item3.A, (float)P_2.A);
				}
				if (list2.Count == 0)
				{
					list2.Add(item);
				}
				list2.Add(item2);
			}
			A(list, list2, item3.A, (float)P_2.A);
		}
		return list;
	}

	public static void A(List<global::a.u> P_0, IEnumerable<r> P_1, global::a.w P_2, t P_3, int P_4, int P_5, v P_6)
	{
		if (!P_3.a)
		{
			return;
		}
		P_6.A(P_3);
		foreach (r item in P_1)
		{
			if (P_3.A(item) && P_2.A(item.A, item.a + 0.15f, item.B, P_4, P_5, out var pointF))
			{
				V v2 = P_6.A(item);
				P_0.Add(new global::a.u
				{
					A = v2.A,
					A = new Rectangle((int)Math.Round(pointF.X - v2.A.X), (int)Math.Round(pointF.Y - v2.A.Y), v2.A.Width, v2.A.Height)
				});
			}
		}
	}
}
internal sealed class r
{
	public long A;

	public uint A;

	public uint a;

	public uint B;

	public ulong A;

	public int A;

	public string A;

	public string a;

	public string B;

	public Color A;

	public float A;

	public float a;

	public float B;

	public float b;
}
