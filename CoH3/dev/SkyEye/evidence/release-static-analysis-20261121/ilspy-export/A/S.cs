using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using b;

namespace A
{
	internal sealed class S
	{
		private sealed class A
		{
			public T A;

			public DateTime A;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class a
		{
			public static readonly a A = new a();

			public static Func<T, string> A;

			public static Func<IGrouping<string, T>, T> A;

			public static Func<T, string> a;

			public static Func<KeyValuePair<string, A>, string> A;

			public static Func<A, T> A;

			internal string A(T P_0)
			{
				return P_0.A;
			}

			internal T A(IGrouping<string, T> P_0)
			{
				return P_0.First();
			}

			internal string a(T P_0)
			{
				return P_0.A;
			}

			internal string A(KeyValuePair<string, A> P_0)
			{
				KeyValuePair<string, A> keyValuePair = P_0;
				return keyValuePair.Key;
			}

			internal T A(A P_0)
			{
				return P_0.A;
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public HashSet<string> A;

			public HashSet<uint> A;

			public DateTime A;

			internal bool A(KeyValuePair<string, A> P_0)
			{
				HashSet<string> hashSet = this.A;
				KeyValuePair<string, A> keyValuePair = P_0;
				if (!hashSet.Contains(keyValuePair.Key))
				{
					HashSet<uint> hashSet2 = this.A;
					keyValuePair = P_0;
					if (hashSet2.Contains(keyValuePair.Value.A.A))
					{
						DateTime dateTime = this.A;
						keyValuePair = P_0;
						return dateTime - keyValuePair.Value.A >= TimeSpan.FromSeconds(3.0);
					}
					return true;
				}
				return false;
			}
		}

		private readonly Dictionary<string, A> m_A = new Dictionary<string, A>();

		private string m_A;

		public void A()
		{
			this.m_A.Clear();
			this.m_A = null;
		}

		public List<T> A(string P_0, IEnumerable<T> P_1, DateTime P_2, IEnumerable<uint> P_3 = null)
		{
			if (this.m_A != P_0)
			{
				this.A();
				this.m_A = P_0;
			}
			HashSet<uint> A = new HashSet<uint>(P_3 ?? Enumerable.Empty<uint>());
			T[] array = (from t3 in P_1
				group t3 by t3.A into source
				select source.First()).ToArray();
			HashSet<string> A2 = new HashSet<string>(array.Select((T t3) => t3.A));
			T[] array2 = array;
			foreach (T t2 in array2)
			{
				this.m_A[t2.A] = new A
				{
					A = t2,
					A = P_2
				};
			}
			string[] array3 = this.m_A.Where(delegate(KeyValuePair<string, A> keyValuePair2)
			{
				HashSet<string> hashSet = A2;
				KeyValuePair<string, A> keyValuePair = keyValuePair2;
				if (!hashSet.Contains(keyValuePair.Key))
				{
					HashSet<uint> hashSet2 = A;
					keyValuePair = keyValuePair2;
					if (hashSet2.Contains(keyValuePair.Value.A.A))
					{
						DateTime dateTime = P_2;
						keyValuePair = keyValuePair2;
						return dateTime - keyValuePair.Value.A >= TimeSpan.FromSeconds(3.0);
					}
					return true;
				}
				return false;
			}).Select(delegate(KeyValuePair<string, A> keyValuePair2)
			{
				KeyValuePair<string, A> keyValuePair = keyValuePair2;
				return keyValuePair.Key;
			}).ToArray();
			foreach (string key in array3)
			{
				this.m_A.Remove(key);
			}
			return this.m_A.Values.Select((A a2) => a2.A).ToList();
		}
	}
	internal static class s
	{
		public static bool A(string P_0)
		{
			return r.A(P_0);
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
			if (!Q.C(P_0))
			{
				return A(P_1);
			}
			return Q.A(P_1);
		}

		public static bool A(float P_0, float P_1, float P_2)
		{
			return global::b.b.A(P_0, P_1, P_2);
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
			return P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_1 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_3;
		}
	}
}
namespace a
{
	internal sealed class S
	{
		internal int A;

		internal DateTime A;

		internal string A;

		internal uint A;

		internal long A;

		internal ulong? A;
	}
	internal static class s
	{
		public static bool A(string P_0)
		{
			if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EP()) && !(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ep()))
			{
				return P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EQ();
			}
			return true;
		}

		public static void A(T P_0, bool P_1, float P_2, float P_3, float P_4, float P_5, float P_6, float P_7, float P_8, float P_9, float P_10, string P_11, string P_12)
		{
			if (P_0.A != null && global::A.h.A(P_0.A, null, P_5, P_6))
			{
				P_0.A = null;
			}
			if (!P_1 && !P_0.A)
			{
				return;
			}
			if (!P_1 && P_3 > P_0.A + 8f)
			{
				P_0.a = true;
				return;
			}
			if (P_0.A != null && !P_0.a && P_3 <= P_0.A + 3f)
			{
				double num = Math.Sqrt(P_7 * P_7 + P_8 * P_8);
				if (num > 0.9 && num < 1.1)
				{
					P_0.A.A = P_2;
					P_0.A.a = P_3;
					P_0.A.B = P_4;
					P_0.A.b = (float)((double)P_7 / num);
					P_0.A.C = (float)((double)P_8 / num);
				}
			}
			if ((P_1 && !P_0.A) || (!P_1 && P_0.a && P_3 <= P_0.A + 3f))
			{
				double num2 = Math.Sqrt(P_7 * P_7 + P_8 * P_8);
				if (!(num2 < 0.9) && !(num2 > 1.1) && !(P_9 < 1f) && !(P_9 > 200f) && !(P_10 <= 0f) && !(P_10 > 100f))
				{
					float num3 = (float)((double)P_7 / num2);
					float num4 = (float)((double)P_8 / num2);
					P_0.A = true;
					P_0.a = false;
					P_0.A = P_3;
					P_0.A++;
					P_0.A = new global::A.L
					{
						A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Eq() + P_11 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_0.A,
						a = P_12,
						A = P_2,
						a = P_3,
						B = P_4,
						b = num3,
						C = num4,
						D = P_9,
						c = P_10
					};
				}
			}
		}
	}
}
