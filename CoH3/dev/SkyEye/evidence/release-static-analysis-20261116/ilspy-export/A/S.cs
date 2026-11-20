using System;
using System.Drawing;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using b;

namespace A
{
	internal static class S
	{
		public static bool A(string P_0)
		{
			return R.A(P_0);
		}

		public static bool A(byte[] P_0)
		{
			if (P_0 == null || P_0.Length < 164)
			{
				return false;
			}
			if (P_0[76] == 2)
			{
				return false;
			}
			float num = BitConverter.ToSingle(P_0, 72);
			float num2 = BitConverter.ToSingle(P_0, 152);
			if (float.IsNaN(num) || float.IsInfinity(num) || float.IsNaN(num2) || float.IsInfinity(num2))
			{
				return false;
			}
			if (!(num > 0f) && !(num2 > 0f) && BitConverter.ToInt16(P_0, 160) <= 0 && (P_0[144] & 4) == 0)
			{
				return (P_0[80] & 8) != 0;
			}
			return true;
		}

		public static bool A(string P_0, byte[] P_1)
		{
			if (!p.C(P_0))
			{
				return A(P_1);
			}
			return p.A(P_1);
		}

		public static bool A(float P_0, float P_1, float P_2)
		{
			return global::b.a.A(P_0, P_1, P_2);
		}

		public static bool A(byte[] P_0, int P_1, out float P_2, out float P_3, out float P_4)
		{
			P_2 = (P_3 = (P_4 = 0f));
			int num = P_1 * 20;
			if (P_1 < 0 || P_0 == null || num > P_0.Length - 20)
			{
				return false;
			}
			if (P_0[num] != 1 && P_0[num] != 2)
			{
				return false;
			}
			P_2 = BitConverter.ToSingle(P_0, num + 4);
			P_3 = BitConverter.ToSingle(P_0, num + 8);
			P_4 = BitConverter.ToSingle(P_0, num + 12);
			return A(P_2, P_3, P_4);
		}

		public static string A(long P_0, uint P_1, long P_2, int P_3)
		{
			return P_0 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ax() + P_1 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ax() + P_2 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ax() + P_3;
		}
	}
	internal sealed class s
	{
		public string A;

		public string a;

		public string B;

		public string b;

		public string C;

		public float A;

		public float a;

		public float B;

		public Color A;

		public uint A;
	}
}
namespace a
{
	internal sealed class S
	{
		public global::A.k A;

		public bool A;

		public bool a;

		public float A;

		public int A;

		public uint A;
	}
	internal static class s
	{
		public static bool A(string P_0, out float P_1, out float P_2)
		{
			P_1 = (P_2 = 0f);
			if (!(P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.En()))
			{
				if (!(P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EO()))
				{
					if (!(P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Eo()))
					{
						if (!(P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EP()))
						{
							if (!(P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ep()))
							{
								if (P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EQ())
								{
									P_1 = 45f;
									P_2 = 5f;
									return true;
								}
								return false;
							}
							P_1 = 60f;
							P_2 = 5f;
							return true;
						}
						P_1 = 45f;
						P_2 = 5f;
						return true;
					}
					P_1 = 45f;
					P_2 = 5f;
					return true;
				}
				P_1 = 45f;
				P_2 = 5f;
				return true;
			}
			P_1 = 45f;
			P_2 = 5f;
			return true;
		}
	}
}
