using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;

namespace B;

internal static class V
{
	[CompilerGenerated]
	private static class A
	{
		public static Action A;
	}

	private static readonly object m_A = new object();

	private static DateTime m_A;

	private static bool m_A;

	private static List<Dictionary<string, object>> m_A = new List<Dictionary<string, object>>();

	private static int m_A;

	internal static List<Dictionary<string, object>> A(global::A.c P_0)
	{
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (global::A.b item in P_0.a)
		{
			if (global::A.C.A(item.A))
			{
				string value = (item.a.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.o()) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kd() : (item.a.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.O()) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KD() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kc()));
				list.Add(new Dictionary<string, object>
				{
					{
						C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KE(),
						item.A
					},
					{
						C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ke(),
						value
					}
				});
			}
		}
		return list;
	}

	internal static object[] A(List<Dictionary<string, object>> P_0, ref int P_1)
	{
		if (P_0.Count == 0)
		{
			P_1 = 0;
			return new object[0];
		}
		P_1 %= P_0.Count;
		int num = Math.Min(64, P_0.Count - P_1);
		object[] array = new object[num];
		for (int num2 = 0; num2 < num; num2++)
		{
			array[num2] = P_0[P_1 + num2];
		}
		P_1 = (P_1 + num) % P_0.Count;
		return array;
	}

	private static void A()
	{
		try
		{
			List<Dictionary<string, object>> list = A(global::A.C.b(null));
			lock (V.m_A)
			{
				V.m_A = list;
				if (V.m_A >= V.m_A.Count)
				{
					V.m_A = 0;
				}
			}
		}
		catch
		{
		}
		finally
		{
			lock (V.m_A)
			{
				V.m_A = false;
				V.m_A = DateTime.UtcNow.AddMinutes(5.0);
			}
		}
	}

	public static void A(Dictionary<string, object> P_0)
	{
		lock (V.m_A)
		{
			if (!V.m_A && DateTime.UtcNow >= V.m_A)
			{
				V.m_A = true;
				Task.Run((Action)A);
			}
			object[] array = A(V.m_A, ref V.m_A);
			if (array.Length != 0)
			{
				P_0[C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KF()] = array;
			}
		}
	}
}
internal sealed class v
{
	private int m_A;

	private DateTime m_A;

	public void A()
	{
		this.m_A = 0;
		this.m_A = DateTime.MinValue;
	}

	public bool A(bool P_0, DateTime P_1)
	{
		if (!P_0)
		{
			A();
			return false;
		}
		if (this.m_A++ == 0)
		{
			this.m_A = P_1;
		}
		if (this.m_A <= 8)
		{
			return P_1 - this.m_A < TimeSpan.FromSeconds(2.0);
		}
		return false;
	}
}
