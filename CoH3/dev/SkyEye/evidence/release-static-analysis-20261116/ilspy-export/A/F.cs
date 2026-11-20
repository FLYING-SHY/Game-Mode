using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using B;
using a;

namespace A
{
	internal sealed class F
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<k, string> A;

			public static Func<i, string> A;

			public static Func<i, double> A;

			public static Func<e, double> A;

			public static Func<e, k> A;

			internal string A(k P_0)
			{
				return P_0.A;
			}

			internal string A(i P_0)
			{
				return P_0.A;
			}

			internal double a(i P_0)
			{
				return P_0.A;
			}

			internal double A(e P_0)
			{
				return P_0.A;
			}

			internal k a(e P_0)
			{
				k k2 = P_0.A;
				return new k
				{
					A = k2.A,
					a = P_0.A.a,
					A = k2.A,
					A = k2.A,
					A = k2.A,
					A = k2.A - k2.C * P_0.A.b,
					a = k2.a,
					B = k2.B + k2.b * P_0.A.b,
					b = k2.b,
					C = k2.C,
					c = P_0.A.A,
					D = P_0.A.a,
					d = P_0.A.B,
					a = P_0.A.A
				};
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public Dictionary<string, i> A;

			internal bool A(string P_0)
			{
				return !this.A.ContainsKey(P_0);
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public i A;

			internal bool A(e P_0)
			{
				if (P_0.A.A == this.A.A && this.A.A >= P_0.A - 300.0 && this.A.A <= P_0.A + 6000.0 && P_0.A.Count < P_0.A.A)
				{
					return P_0.A.A.Any((string text) => this.A.a.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xb() + text, StringComparison.Ordinal));
				}
				return false;
			}

			internal bool A(string P_0)
			{
				return this.A.a.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xb() + P_0, StringComparison.Ordinal);
			}
		}

		[CompilerGenerated]
		private sealed class b
		{
			public e A;

			public a A;

			internal bool A(string P_0)
			{
				if (this.A.A.ContainsKey(P_0) && !this.A.A[P_0].a)
				{
					if (this.A.A[P_0].A)
					{
						return !H.A(this.A.A, this.A.A, this.A.A[P_0].A, this.A.A[P_0].a);
					}
					return true;
				}
				return false;
			}
		}

		private readonly Dictionary<string, e> m_A = new Dictionary<string, e>();

		private readonly Dictionary<string, e> m_a = new Dictionary<string, e>();

		private HashSet<string> m_A = new HashSet<string>();

		private readonly HashSet<string> m_a = new HashSet<string>();

		public void A()
		{
			this.m_A.Clear();
			this.m_a.Clear();
			this.m_A.Clear();
			this.m_a.Clear();
		}

		public bool A(string P_0)
		{
			return this.m_a.ContainsKey(P_0);
		}

		public List<k> A(IEnumerable<k> P_0, IEnumerable<i> P_1, double P_2)
		{
			k[] array = P_0.ToArray();
			HashSet<string> hashSet = new HashSet<string>(array.Select((k k3) => k3.A));
			Dictionary<string, i> A = P_1.ToDictionary((i i2) => i2.A);
			k[] array2 = array;
			foreach (k k2 in array2)
			{
				if (!this.m_a.Contains(k2.A))
				{
					if (!this.m_A.TryGetValue(k2.A, out var value))
					{
						value = new e
						{
							A = k2,
							A = f.A(k2.a),
							A = P_2
						};
						this.m_A[k2.A] = value;
					}
					value.a = P_2;
				}
			}
			this.m_A = hashSet;
			foreach (i A2 in A.Values.OrderBy((i i2) => i2.A))
			{
				if (this.m_a.TryGetValue(A2.A, out var value2) && this.m_A.Values.Contains(value2))
				{
					continue;
				}
				e e2 = (from e3 in this.m_A.Values
					where e3.A.A == A2.A && A2.A >= e3.A - 300.0 && A2.A <= e3.A + 6000.0 && e3.A.Count < e3.A.A && e3.A.A.Any((string text2) => A2.a.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xb() + text2, StringComparison.Ordinal))
					orderby e3.A descending
					select e3).FirstOrDefault();
				if (e2 != null)
				{
					e2.A.Add(A2.A);
					e2.A = true;
					this.m_a[A2.A] = e2;
				}
			}
			string[] array3 = this.m_A.Keys.ToArray();
			foreach (string text in array3)
			{
				e A3 = this.m_A[text];
				bool flag = A3.A.Any((string key2) => A.ContainsKey(key2) && !A[key2].a && (!A[key2].A || !H.A(A3.A, A3.A, A[key2].A, A[key2].a)));
				bool flag2 = A3.A.Count >= A3.A.A || P_2 - A3.A > 6000.0;
				if ((A3.A && !flag && flag2) || (!A3.A && P_2 - A3.A > 6000.0))
				{
					this.m_A.Remove(text);
					this.m_a.Add(text);
				}
			}
			array3 = this.m_a.Keys.Where((string key2) => !A.ContainsKey(key2)).ToArray();
			foreach (string key in array3)
			{
				this.m_a.Remove(key);
			}
			return this.m_A.Values.Select(delegate(e e3)
			{
				k k3 = e3.A;
				return new k
				{
					A = k3.A,
					a = e3.A.a,
					A = k3.A,
					A = k3.A,
					A = k3.A,
					A = k3.A - k3.C * e3.A.b,
					a = k3.a,
					B = k3.B + k3.b * e3.A.b,
					b = k3.b,
					C = k3.C,
					c = e3.A.A,
					D = e3.A.a,
					d = e3.A.B,
					a = e3.A.A
				};
			}).ToList();
		}
	}
	internal static class f
	{
		[CompilerGenerated]
		private sealed class A
		{
			public string A;

			public Func<string, bool> A;

			internal bool A(G P_0)
			{
				return P_0.A.Any((string text) => this.A.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xb() + text, StringComparison.Ordinal));
			}

			internal bool A(string P_0)
			{
				return this.A.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xb() + P_0, StringComparison.Ordinal);
			}
		}

		private static readonly Dictionary<string, G> m_A;

		private static void A(string P_0, string P_1, float P_2, float P_3, float P_4, int P_5, params string[] aircraft)
		{
			f.m_A.Add(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aE() + P_0, new G
			{
				A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aE() + P_0,
				a = P_1,
				A = P_2,
				a = P_3,
				B = P_4,
				A = P_5,
				A = aircraft
			});
		}

		static f()
		{
			f.m_A = new Dictionary<string, G>(StringComparer.Ordinal);
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ae(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aF(), 5f, 42f, 0f, 1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.af());
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aG(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ag(), 12f, 23f, 7f, 2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aH());
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ah(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aI(), 5f, 47f, 0f, 2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ai());
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aJ(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aj(), 3f, 47f, 0f, 1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aK());
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ak(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aj(), 7f, 47f, 0f, 2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aK());
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aL(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aj(), 7f, 47f, 0f, 4, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aK());
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.al(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aM(), 5f, 45f, 0f, 1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.am());
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aN(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.an(), 7f, 45f, 0f, 2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.am());
			f.m_A[C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aO()].b = 2f;
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ao(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aP(), 0f, 0f, 0f, 1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ap());
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aQ(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aq(), 0f, 0f, 0f, 2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ap());
			f.m_A[C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aR()].A = true;
			f.m_A[C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ar()].A = true;
		}

		public static G A(string P_0)
		{
			if (!f.m_A.TryGetValue(P_0 ?? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D(), out var value))
			{
				return null;
			}
			return value;
		}

		public static bool a(string P_0)
		{
			return P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aS();
		}

		public static bool B(string P_0)
		{
			if (!global::a.r.A(P_0))
			{
				if (P_0.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.@as(), StringComparison.Ordinal) && P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.aT()))
				{
					return f.m_A.Values.Any((G g2) => g2.A.Any((string text) => P_0.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xb() + text, StringComparison.Ordinal)));
				}
				return false;
			}
			return true;
		}
	}
}
namespace a
{
	internal static class F
	{
		public const string A = "2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde";

		private static readonly object m_A = new object();

		private static string m_a;

		private static int m_A;

		public static bool A => Volatile.Read(ref F.m_A) != 0;

		internal static bool A(string P_0)
		{
			return string.Equals(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DP(), StringComparison.OrdinalIgnoreCase);
		}

		public static void a(string P_0)
		{
			lock (F.m_A)
			{
				try
				{
					FileInfo fileInfo = new FileInfo(P_0);
					string text = fileInfo.FullName + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq() + fileInfo.Length + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq() + fileInfo.LastWriteTimeUtc.Ticks;
					if (text == F.m_a)
					{
						return;
					}
					string text2;
					using (FileStream inputStream = File.OpenRead(P_0))
					{
						using SHA256 sHA = SHA256.Create();
						text2 = BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ci(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D()).ToLowerInvariant();
					}
					Volatile.Write(ref F.m_A, (!A(text2)) ? 1 : 0);
					F.m_a = text;
				}
				catch
				{
					Volatile.Write(ref F.m_A, 1);
					F.m_a = null;
				}
			}
		}
	}
	internal static class f
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<string, bool> A;

			internal bool A(string P_0)
			{
				return f.m_A.Contains(P_0.Replace('/', '\\'));
			}
		}

		private static readonly HashSet<string> m_A = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Dr(),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DS(),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ds()
		};

		public static bool A(string P_0)
		{
			if (P_0 != null)
			{
				return f.m_A.Contains(P_0.Replace('/', '\\'));
			}
			return false;
		}

		public static int A(float P_0)
		{
			if (float.IsNaN(P_0) || float.IsInfinity(P_0) || P_0 < 0f || P_0 > 80000f)
			{
				throw new ArgumentOutOfRangeException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Dp());
			}
			return (int)Math.Ceiling((double)P_0 / 8.0);
		}

		public static string A(global::B.N P_0)
		{
			if (a(P_0))
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DQ();
			}
			if (!P_0.D || !P_0.A.HasValue)
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
			}
			return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Dq() + A(P_0.A.Value) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DR();
		}

		public static bool a(global::B.N P_0)
		{
			return P_0.A.Any((string text) => f.m_A.Contains(text.Replace('/', '\\')));
		}
	}
}
