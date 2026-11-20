using System;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using a;
using b;

namespace B;

internal sealed class P
{
	public readonly string A;

	public readonly string a;

	public readonly float A;

	public readonly float a;

	public int A => global::a.f.A(this.A);

	public P(string P_0, float P_1, float P_2, string P_3)
	{
		if (string.IsNullOrEmpty(P_0))
		{
			throw new ArgumentException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jO());
		}
		global::a.f.A(P_1);
		global::a.f.A(P_2);
		if (P_2 <= 0f || P_1 > P_2)
		{
			throw new ArgumentOutOfRangeException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jo(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jP());
		}
		this.A = P_0;
		this.A = P_1;
		a = P_2;
		this.a = P_3;
	}
}
internal static class p
{
	[Serializable]
	[CompilerGenerated]
	private sealed class A
	{
		public static readonly A A = new A();

		public static Func<P, string> A;

		public static Func<P, string> a;

		public static Func<P, string> B;

		internal string A(P P_0)
		{
			return global::b.j.A(P_0.A);
		}

		internal string a(P P_0)
		{
			return P_0.a;
		}

		internal string B(P P_0)
		{
			return global::b.j.A(P_0.A) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xw() + ((P_0.A == 0) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xX() : (P_0.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DR())) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xx();
		}
	}

	public static string A(N P_0)
	{
		if (!P_0.D || P_0.a.Count == 0)
		{
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
		}
		return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jp() + string.Join(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DK(), from p2 in P_0.a.OrderBy((P p2) => global::b.j.A(p2.A)).ThenBy((P p2) => p2.a, StringComparer.Ordinal)
			select global::b.j.A(p2.A) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xw() + ((p2.A == 0) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xX() : (p2.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DR())) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xx());
	}
}
