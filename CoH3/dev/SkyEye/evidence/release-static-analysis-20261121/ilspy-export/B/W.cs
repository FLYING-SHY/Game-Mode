using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;

namespace B;

internal static class W
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
				string value = (item.a.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.o()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KI() : (item.a.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.O()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kh() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KH()));
				list.Add(new Dictionary<string, object>
				{
					{
						CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ki(),
						item.A
					},
					{
						CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KJ(),
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
			lock (W.m_A)
			{
				W.m_A = list;
				if (W.m_A >= W.m_A.Count)
				{
					W.m_A = 0;
				}
			}
		}
		catch
		{
		}
		finally
		{
			lock (W.m_A)
			{
				W.m_A = false;
				W.m_A = DateTime.UtcNow.AddMinutes(5.0);
			}
		}
	}

	public static void A(Dictionary<string, object> P_0)
	{
		lock (W.m_A)
		{
			if (!W.m_A && DateTime.UtcNow >= W.m_A)
			{
				W.m_A = true;
				Task.Run((Action)A);
			}
			object[] array = A(W.m_A, ref W.m_A);
			if (array.Length != 0)
			{
				P_0[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kj()] = array;
			}
		}
	}
}
internal sealed class w
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
