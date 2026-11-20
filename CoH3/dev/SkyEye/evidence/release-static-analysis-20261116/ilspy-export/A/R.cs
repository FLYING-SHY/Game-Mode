using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;

namespace A
{
	internal static class R
	{
		private static readonly Dictionary<string, HashSet<string>> m_A;

		private static readonly Dictionary<string, string> m_A;

		public static IEnumerable<string> A => R.m_A.Keys;

		static R()
		{
			R.m_A = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			R.m_A = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cl());
			if (stream == null)
			{
				throw new InvalidOperationException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CM());
			}
			using StreamReader streamReader = new StreamReader(stream);
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split('\t');
				if (array.Length == 3)
				{
					if (!R.m_A.TryGetValue(array[0], out var value))
					{
						HashSet<string> hashSet = (R.m_A[array[0]] = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
						value = hashSet;
					}
					value.Add(array[1]);
					R.m_A[array[0]] = array[2];
				}
			}
		}

		public static bool A(string P_0)
		{
			return R.m_A.ContainsKey(P_0);
		}

		public static bool A(string P_0, string P_1)
		{
			if (R.m_A.TryGetValue(P_0, out var value))
			{
				return value.Contains(P_1);
			}
			return false;
		}

		public static string a(string P_0)
		{
			if (!R.m_A.TryGetValue(P_0, out var value))
			{
				return P_0;
			}
			return value;
		}
	}
	internal sealed class r
	{
		private sealed class A
		{
			public s A;

			public DateTime A;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class a
		{
			public static readonly a A = new a();

			public static Func<s, string> A;

			public static Func<IGrouping<string, s>, s> A;

			public static Func<s, string> a;

			public static Func<KeyValuePair<string, A>, string> A;

			public static Func<A, s> A;

			internal string A(s P_0)
			{
				return P_0.A;
			}

			internal s A(IGrouping<string, s> P_0)
			{
				return P_0.First();
			}

			internal string a(s P_0)
			{
				return P_0.A;
			}

			internal string A(KeyValuePair<string, A> P_0)
			{
				KeyValuePair<string, A> keyValuePair = P_0;
				return keyValuePair.Key;
			}

			internal s A(A P_0)
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

		public List<s> A(string P_0, IEnumerable<s> P_1, DateTime P_2, IEnumerable<uint> P_3 = null)
		{
			if (this.m_A != P_0)
			{
				this.A();
				this.m_A = P_0;
			}
			HashSet<uint> A = new HashSet<uint>(P_3 ?? Enumerable.Empty<uint>());
			s[] array = (from s3 in P_1
				group s3 by s3.A into source
				select source.First()).ToArray();
			HashSet<string> A2 = new HashSet<string>(array.Select((s s3) => s3.A));
			s[] array2 = array;
			foreach (s s2 in array2)
			{
				this.m_A[s2.A] = new A
				{
					A = s2,
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
}
namespace a
{
	internal sealed class R
	{
		internal int A;

		internal DateTime A;

		internal string A;

		internal uint A;

		internal long A;

		internal ulong? A;
	}
	internal static class r
	{
		public static bool A(string P_0)
		{
			if (!(P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.El()) && !(P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EM()))
			{
				return P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Em();
			}
			return true;
		}

		public static void A(S P_0, bool P_1, float P_2, float P_3, float P_4, float P_5, float P_6, float P_7, float P_8, float P_9, float P_10, string P_11, string P_12)
		{
			if (P_0.A != null && global::A.H.A(P_0.A, null, P_5, P_6))
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
					P_0.A = new global::A.k
					{
						A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EN() + P_11 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ax() + P_0.A,
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
