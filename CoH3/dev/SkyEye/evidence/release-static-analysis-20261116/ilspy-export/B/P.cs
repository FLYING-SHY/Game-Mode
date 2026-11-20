using System;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
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
			throw new ArgumentException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jO());
		}
		global::a.f.A(P_1);
		global::a.f.A(P_2);
		if (P_2 <= 0f || P_1 > P_2)
		{
			throw new ArgumentOutOfRangeException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jo(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jP());
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
			return global::b.j.A(P_0.A) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xw() + ((P_0.A == 0) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xX() : (P_0.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DR())) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xx();
		}
	}

	public static string A(N P_0)
	{
		if (!P_0.D || P_0.a.Count == 0)
		{
			return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
		}
		return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jp() + string.Join(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DK(), from p2 in P_0.a.OrderBy((P p2) => global::b.j.A(p2.A)).ThenBy((P p2) => p2.a, StringComparer.Ordinal)
			select global::b.j.A(p2.A) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xw() + ((p2.A == 0) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xX() : (p2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DR())) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xx());
	}
}
