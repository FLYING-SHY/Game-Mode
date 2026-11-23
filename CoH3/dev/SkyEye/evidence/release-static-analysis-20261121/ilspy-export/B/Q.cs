using System;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using a;
using b;

namespace B;

internal sealed class Q
{
	public readonly string A;

	public readonly string a;

	public readonly float A;

	public readonly float a;

	public int A => global::a.g.A(this.A);

	public Q(string P_0, float P_1, float P_2, string P_3)
	{
		if (string.IsNullOrEmpty(P_0))
		{
			throw new ArgumentException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.js());
		}
		global::a.g.A(P_1);
		global::a.g.A(P_2);
		if (P_2 <= 0f || P_1 > P_2)
		{
			throw new ArgumentOutOfRangeException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jT(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jt());
		}
		this.A = P_0;
		this.A = P_1;
		a = P_2;
		this.a = P_3;
	}
}
internal static class q
{
	[Serializable]
	[CompilerGenerated]
	private sealed class A
	{
		public static readonly A A = new A();

		public static Func<Q, string> A;

		public static Func<Q, string> a;

		public static Func<Q, string> B;

		internal string A(Q P_0)
		{
			return global::b.k.A(P_0.A);
		}

		internal string a(Q P_0)
		{
			return P_0.a;
		}

		internal string B(Q P_0)
		{
			return global::b.k.A(P_0.A) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YC() + ((P_0.A == 0) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yc() : (P_0.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Du())) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YD();
		}
	}

	public static string A(O P_0)
	{
		if (!P_0.D || P_0.a.Count == 0)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}
		return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jU() + string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dk(), from q2 in P_0.a.OrderBy((Q q2) => global::b.k.A(q2.A)).ThenBy((Q q2) => q2.a, StringComparer.Ordinal)
			select global::b.k.A(q2.A) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YC() + ((q2.A == 0) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yc() : (q2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Du())) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YD());
	}
}
