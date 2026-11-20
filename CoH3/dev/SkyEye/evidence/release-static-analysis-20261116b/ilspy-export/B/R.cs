using System;
using System.Diagnostics;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using A;
using b;

namespace B;

internal static class R
{
	public static bool A(string P_0)
	{
		if (!global::A.p.A(P_0))
		{
			if (!string.IsNullOrEmpty(P_0) && P_0.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jQ(), StringComparison.Ordinal))
			{
				if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jq()) && !P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jR()))
				{
					return P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jr());
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool a(string P_0)
	{
		if (!string.IsNullOrEmpty(P_0) && P_0.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jQ()))
		{
			if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jr()))
			{
				return P_0.Substring(P_0.LastIndexOf('\\') + 1).Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dc());
			}
			return true;
		}
		return false;
	}

	public static bool A(float P_0, float P_1, float P_2, float P_3, float P_4, float P_5)
	{
		double num = P_0 - P_3;
		double num2 = P_1 - P_4;
		double num3 = P_2 - P_5;
		return num * num + num2 * num2 + num3 * num3 <= 0.0001;
	}

	public static bool A(byte[] P_0, byte[] P_1, byte[] P_2, long P_3, long P_4, long P_5, uint P_6, uint P_7, out q P_8, bool P_9 = false, bool P_10 = false, bool P_11 = false)
	{
		P_8 = null;
		if (P_0 == null || P_0.Length < 392 || P_1 == null || P_1.Length < 80 || P_2 == null || P_2.Length < 108)
		{
			return false;
		}
		if (BitConverter.ToUInt32(P_0, 104) != P_6 || BitConverter.ToUInt32(P_0, 108) != P_7 || (BitConverter.ToUInt32(P_0, 160) & 0x4000) != 0)
		{
			return false;
		}
		if (BitConverter.ToInt64(P_1, 0) != P_3 + 65313128 || BitConverter.ToUInt32(P_1, 36) + P_4 != P_5 || (BitConverter.ToUInt32(P_1, 64) != 0 && ((!P_9 && !P_10) || BitConverter.ToUInt32(P_1, 64) != 3)))
		{
			return false;
		}
		if (BitConverter.ToInt64(P_2, 0) != P_3 + 65307976 || BitConverter.ToUInt32(P_2, 8) + P_4 != P_5 || (P_1[16] != 1 && P_1[16] != 2))
		{
			return false;
		}
		float num = BitConverter.ToSingle(P_1, 20);
		float num2 = BitConverter.ToSingle(P_1, 24);
		float num3 = BitConverter.ToSingle(P_1, 28);
		float num4 = BitConverter.ToSingle(P_2, 104);
		float num5 = BitConverter.ToSingle(P_0, 44);
		float num6 = BitConverter.ToSingle(P_0, 48);
		float num7 = BitConverter.ToSingle(P_0, 52);
		if (!global::b.a.A(num, num2, num3) || !global::b.a.A(num5, num6, num7) || float.IsNaN(num4) || float.IsInfinity(num4) || num4 < 0f || num4 > 100f || (!P_10 && A(num5, num6, num7, num, num2, num3)))
		{
			return false;
		}
		uint num8 = BitConverter.ToUInt32(P_2, 20);
		uint num9 = BitConverter.ToUInt32(P_2, 24);
		if ((P_2[16] != 3 && (!P_11 || P_2[16] != 6)) || num8 == 0)
		{
			return false;
		}
		P_8 = new q
		{
			A = P_5,
			A = P_6,
			a = P_7,
			A = (((ulong)num8 << 32) | num9),
			A = num,
			a = num2,
			B = num3,
			b = num4
		};
		return true;
	}
}
internal static class r
{
	[Conditional("SKYEYE_PIPELINE_TRACE")]
	public static void A(string P_0, string P_1 = "")
	{
	}

	[Conditional("SKYEYE_PIPELINE_TRACE")]
	public static void A()
	{
	}
}
