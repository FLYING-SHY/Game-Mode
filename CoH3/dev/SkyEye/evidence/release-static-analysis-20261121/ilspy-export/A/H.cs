using System;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using b;

namespace A
{
	internal enum H
	{
		A,
		a
	}
	internal static class h
	{
		internal static int A(H P_0 = H.A)
		{
			return P_0 switch
			{
				H.a => 184, 
				H.A => 284, 
				_ => throw new ArgumentOutOfRangeException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aU()), 
			};
		}

		internal static void A(byte[] P_0, H P_1, out float P_2, out float P_3, out float P_4)
		{
			switch (P_1)
			{
			case H.a:
				P_2 = BitConverter.ToSingle(P_0, 96);
				P_3 = 0f;
				P_4 = BitConverter.ToSingle(P_0, 100);
				break;
			default:
				throw new ArgumentOutOfRangeException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aU());
			case H.A:
				P_2 = BitConverter.ToSingle(P_0, 272);
				P_3 = BitConverter.ToSingle(P_0, 276);
				P_4 = BitConverter.ToSingle(P_0, 280);
				break;
			}
		}

		public static bool A(L P_0, G P_1, float P_2, float P_3)
		{
			if (P_0.a || (P_1 != null && P_1.A) || !global::b.b.A(P_2, 0f, P_3))
			{
				return false;
			}
			float num = P_1?.a ?? P_0.D;
			float num2 = P_1?.B ?? P_0.d;
			return (P_2 - P_0.A) * P_0.b + (P_3 - P_0.B) * P_0.C > num2 + num + 10f;
		}

		public static L A(string P_0, byte[] P_1, byte[] P_2, long P_3, long P_4, long P_5, uint P_6, uint P_7, Func<long, long> P_8 = null, H P_9 = H.A)
		{
			if (P_1 == null || P_1.Length < 392 || P_2 == null || P_2.Length < A(P_9) || BitConverter.ToUInt32(P_1, 104) != P_6 || BitConverter.ToUInt32(P_1, 108) != P_7 || (BitConverter.ToUInt32(P_1, 160) & 0x4000) != 0)
			{
				return null;
			}
			if (BitConverter.ToInt64(P_2, 0) != P_3 + (P_8?.Invoke(64559016L) ?? 64559016) || BitConverter.ToUInt32(P_2, 36) + P_4 != P_5 || BitConverter.ToUInt32(P_2, 64) != 1 || P_2[16] != 2)
			{
				return null;
			}
			float num = BitConverter.ToSingle(P_2, 20);
			float num2 = BitConverter.ToSingle(P_2, 24);
			float num3 = BitConverter.ToSingle(P_2, 28);
			A(P_2, P_9, out var num4, out var value, out var num5);
			float num6 = BitConverter.ToSingle(P_1, 48);
			double num7 = Math.Sqrt(num4 * num4 + num5 * num5);
			if (!global::b.b.A(num, num2, num3) || double.IsNaN(num7) || double.IsInfinity(num7) || num7 < 0.95 || num7 > 1.01 || float.IsNaN(value) || float.IsInfinity(value) || (double)Math.Abs(value) > 0.1 || float.IsNaN(num6) || float.IsInfinity(num6) || num6 - num2 < 8f)
			{
				return null;
			}
			bool flag = P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.au();
			bool flag2 = P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aV();
			bool flag3 = P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.av();
			if (!flag && !flag2 && !flag3)
			{
				return null;
			}
			num4 = (float)((double)num4 / num7);
			num5 = (float)((double)num5 / num7);
			G g2 = f.A(flag3 ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aX() : (flag ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aw() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aW()));
			float num8 = (flag ? 2.5f : 0f);
			return new L
			{
				A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ax() + P_5 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_6,
				a = g2.a,
				A = num + num4 * num8,
				a = num2,
				B = num3 + num5 * num8,
				b = num4,
				C = num5,
				c = g2.A,
				D = g2.a,
				d = g2.B
			};
		}

		public static bool A(byte[] P_0, byte[] P_1, long P_2, long P_3, out float P_4, out float P_5, out float P_6, out float P_7, out float P_8, bool P_9 = false, Func<long, long> P_10 = null)
		{
			P_4 = (P_5 = (P_6 = (P_7 = (P_8 = 0f))));
			if (P_0 == null || P_0.Length < 148 || P_1 == null || P_1.Length != 20 || BitConverter.ToInt64(P_0, 0) != P_2 + (P_10?.Invoke(64568064L) ?? 64568064) || P_0[76] == 2 || P_0[20] != 6 || BitConverter.ToInt64(P_0, 24) != P_3 || (P_0[144] & 1) == 0)
			{
				return false;
			}
			if (!s.A(P_1, 0, out P_4, out P_5, out P_6))
			{
				return false;
			}
			if (P_9)
			{
				P_7 = 0f;
				P_8 = 1f;
				return true;
			}
			float num = BitConverter.ToSingle(P_0, 40);
			float value = BitConverter.ToSingle(P_0, 44);
			float num2 = BitConverter.ToSingle(P_0, 48);
			double num3 = Math.Sqrt(num * num + num2 * num2);
			if (double.IsNaN(num3) || double.IsInfinity(num3) || num3 < 0.95 || num3 > 1.05 || (double)Math.Abs(value) > 0.02)
			{
				return false;
			}
			P_7 = (float)((double)num / num3);
			P_8 = (float)((double)num2 / num3);
			return true;
		}

		public static L A(byte[] P_0, byte[] P_1, byte[] P_2, long P_3, long P_4, long P_5, uint P_6, uint P_7, Func<long, long> P_8 = null)
		{
			if (P_0.Length < 392 || P_1.Length < 68 || P_2.Length < 108 || BitConverter.ToUInt32(P_0, 104) != P_6 || BitConverter.ToUInt32(P_0, 108) != P_7 || (BitConverter.ToUInt32(P_0, 160) & 0x4000) != 0)
			{
				return null;
			}
			if (BitConverter.ToInt64(P_1, 0) != P_3 + (P_8?.Invoke(65313128L) ?? 65313128) || BitConverter.ToUInt32(P_1, 36) + P_4 != P_5 || BitConverter.ToUInt32(P_1, 64) != 3 || BitConverter.ToInt64(P_2, 0) != P_3 + (P_8?.Invoke(65307976L) ?? 65307976) || BitConverter.ToUInt32(P_2, 8) + P_4 != P_5 || (P_1[16] != 1 && P_1[16] != 2))
			{
				return null;
			}
			float num = BitConverter.ToSingle(P_1, 20);
			float num2 = BitConverter.ToSingle(P_1, 24);
			float num3 = BitConverter.ToSingle(P_1, 28);
			float num4 = BitConverter.ToSingle(P_2, 104);
			if (!global::b.b.A(num, num2, num3) || float.IsNaN(num4) || float.IsInfinity(num4) || num4 <= 0f || num4 > 100f || global::B.S.A(BitConverter.ToSingle(P_0, 44), BitConverter.ToSingle(P_0, 48), BitConverter.ToSingle(P_0, 52), num, num2, num3))
			{
				return null;
			}
			uint num5 = BitConverter.ToUInt32(P_2, 56);
			if (num5 == uint.MaxValue)
			{
				return null;
			}
			return new L
			{
				A = P_5 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_6,
				a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.at(),
				A = P_4 + num5,
				A = true,
				A = num,
				a = num2,
				B = num3,
				E = num4
			};
		}
	}
}
namespace a
{
	internal static class H
	{
		public static bool A(string P_0)
		{
			string text = P_0.Substring(P_0.LastIndexOf('\\') + 1);
			if (!text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dw()) && !text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DX()))
			{
				return text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dx());
			}
			return true;
		}

		public static bool a(string P_0)
		{
			if (!P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.@as()) || !P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DY()))
			{
				return false;
			}
			string text = P_0.Substring(P_0.LastIndexOf('\\') + 1);
			if (!text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dy()) && !text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DZ()))
			{
				return A(P_0);
			}
			return true;
		}
	}
	internal interface h
	{
		F A();
	}
}
