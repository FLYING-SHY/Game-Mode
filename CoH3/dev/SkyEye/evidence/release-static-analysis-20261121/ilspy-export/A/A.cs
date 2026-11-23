using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
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
			string format = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.A();
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
	internal sealed class A
	{
		private string m_A;

		private readonly HashSet<string> m_A = new HashSet<string>();

		public static string A(global::B.O P_0)
		{
			return P_0.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + P_0.A;
		}

		public void A(string P_0)
		{
			if (this.m_A != P_0)
			{
				this.m_A.Clear();
				this.m_A = P_0;
			}
		}

		public bool a(global::B.O P_0)
		{
			if (P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA())
			{
				return !this.m_A.Contains(A(P_0));
			}
			return false;
		}

		public void A(global::B.O P_0, bool P_1)
		{
			if (P_1)
			{
				this.m_A.Remove(A(P_0));
			}
			else
			{
				this.m_A.Add(A(P_0));
			}
		}

		public IEnumerable<global::B.O> A(IEnumerable<global::B.O> P_0)
		{
			return P_0.Where(a);
		}
	}
	internal static class a
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.O, uint> A;

			public static Func<global::B.O, string> A;

			internal uint A(global::B.O P_0)
			{
				return P_0.A;
			}

			internal string a(global::B.O P_0)
			{
				return P_0.a;
			}
		}

		public static D A(IEnumerable<global::b.A> P_0, IEnumerable<global::B.O> P_1, float P_2, float P_3, double P_4)
		{
			Dictionary<uint, string> dictionary = P_1.ToDictionary((global::B.O o2) => o2.A, (global::B.O o2) => o2.a);
			HashSet<uint> hashSet = new HashSet<uint>();
			HashSet<uint> hashSet2 = new HashSet<uint>();
			HashSet<uint> hashSet3 = new HashSet<uint>();
			HashSet<uint> hashSet4 = new HashSet<uint>();
			foreach (global::b.A item in P_0)
			{
				if (!item.a && dictionary.TryGetValue(item.A, out var value) && !(value == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DL()))
				{
					bool flag = value == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba() || value == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl();
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
			return new D
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

		public static bool A(w P_0, PointF P_1, int P_2, int P_3, float P_4, out float P_5, out float P_6)
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
			if (global::b.b.A(P_5, P_4, P_6) && P_0.A(P_5, P_4, P_6, P_2, P_3, out var pointF) && Math.Abs(pointF.X - P_1.X) < 2f)
			{
				return Math.Abs(pointF.Y - P_1.Y) < 2f;
			}
			return false;
		}
	}
}
