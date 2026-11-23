using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
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

			public static Func<L, string> A;

			public static Func<J, string> A;

			public static Func<J, double> A;

			public static Func<e, double> A;

			public static Func<e, L> A;

			internal string A(L P_0)
			{
				return P_0.A;
			}

			internal string A(J P_0)
			{
				return P_0.A;
			}

			internal double a(J P_0)
			{
				return P_0.A;
			}

			internal double A(e P_0)
			{
				return P_0.A;
			}

			internal L a(e P_0)
			{
				L l2 = P_0.A;
				return new L
				{
					A = l2.A,
					a = P_0.A.a,
					A = l2.A,
					A = l2.A,
					A = l2.A,
					A = l2.A - l2.C * P_0.A.b,
					a = l2.a,
					B = l2.B + l2.b * P_0.A.b,
					b = l2.b,
					C = l2.C,
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
			public Dictionary<string, J> A;

			internal bool A(string P_0)
			{
				return !this.A.ContainsKey(P_0);
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public J A;

			internal bool A(e P_0)
			{
				if (P_0.A.A == this.A.A && this.A.A >= P_0.A - 300.0 && this.A.A <= P_0.A + 6000.0 && P_0.A.Count < P_0.A.A)
				{
					return P_0.A.A.Any((string text) => this.A.a.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XG() + text, StringComparison.Ordinal));
				}
				return false;
			}

			internal bool A(string P_0)
			{
				return this.A.a.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XG() + P_0, StringComparison.Ordinal);
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
						return !h.A(this.A.A, this.A.A, this.A.A[P_0].A, this.A.A[P_0].a);
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

		public List<L> A(IEnumerable<L> P_0, IEnumerable<J> P_1, double P_2)
		{
			L[] array = P_0.ToArray();
			HashSet<string> hashSet = new HashSet<string>(array.Select((L l3) => l3.A));
			Dictionary<string, J> A = P_1.ToDictionary((J j2) => j2.A);
			L[] array2 = array;
			foreach (L l2 in array2)
			{
				if (!this.m_a.Contains(l2.A))
				{
					if (!this.m_A.TryGetValue(l2.A, out var value))
					{
						value = new e
						{
							A = l2,
							A = f.A(l2.a),
							A = P_2
						};
						this.m_A[l2.A] = value;
					}
					value.a = P_2;
				}
			}
			this.m_A = hashSet;
			foreach (J A2 in A.Values.OrderBy((J j2) => j2.A))
			{
				if (this.m_a.TryGetValue(A2.A, out var value2) && this.m_A.Values.Contains(value2))
				{
					continue;
				}
				e e2 = (from e3 in this.m_A.Values
					where e3.A.A == A2.A && A2.A >= e3.A - 300.0 && A2.A <= e3.A + 6000.0 && e3.A.Count < e3.A.A && e3.A.A.Any((string text2) => A2.a.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XG() + text2, StringComparison.Ordinal))
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
				bool flag = A3.A.Any((string key2) => A.ContainsKey(key2) && !A[key2].a && (!A[key2].A || !h.A(A3.A, A3.A, A[key2].A, A[key2].a)));
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
				L l3 = e3.A;
				return new L
				{
					A = l3.A,
					a = e3.A.a,
					A = l3.A,
					A = l3.A,
					A = l3.A,
					A = l3.A - l3.C * e3.A.b,
					a = l3.a,
					B = l3.B + l3.b * e3.A.b,
					b = l3.b,
					C = l3.C,
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
				return P_0.A.Any((string text) => this.A.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XG() + text, StringComparison.Ordinal));
			}

			internal bool A(string P_0)
			{
				return this.A.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XG() + P_0, StringComparison.Ordinal);
			}
		}

		private static readonly Dictionary<string, G> m_A;

		private static void A(string P_0, string P_1, float P_2, float P_3, float P_4, int P_5, params string[] aircraft)
		{
			f.m_A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aE() + P_0, new G
			{
				A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aE() + P_0,
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
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ae(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aF(), 5f, 42f, 0f, 1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.af());
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aG(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ag(), 12f, 23f, 7f, 2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aH());
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ah(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aI(), 5f, 47f, 0f, 2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ai());
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aJ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aj(), 3f, 47f, 0f, 1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aK());
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ak(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aj(), 7f, 47f, 0f, 2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aK());
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aL(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aj(), 7f, 47f, 0f, 4, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aK());
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.al(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aM(), 5f, 45f, 0f, 1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.am());
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aN(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.an(), 7f, 45f, 0f, 2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.am());
			f.m_A[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aO()].b = 2f;
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ao(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aP(), 0f, 0f, 0f, 1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ap());
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aQ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aq(), 0f, 0f, 0f, 2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ap());
			f.m_A[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aR()].A = true;
			f.m_A[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ar()].A = true;
		}

		public static G A(string P_0)
		{
			if (!f.m_A.TryGetValue(P_0 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), out var value))
			{
				return null;
			}
			return value;
		}

		public static bool a(string P_0)
		{
			return P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aS();
		}

		public static bool B(string P_0)
		{
			if (!global::a.s.A(P_0))
			{
				if (P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.@as(), StringComparison.Ordinal) && P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aT()))
				{
					return f.m_A.Values.Any((G g2) => g2.A.Any((string text) => P_0.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XG() + text, StringComparison.Ordinal)));
				}
				return false;
			}
			return true;
		}
	}
}
namespace a
{
	internal sealed class F
	{
		public int A;

		public DateTime A;

		public string A;

		public string a;

		public string B;

		public string A => this.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + this.A.Ticks + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + this.A;
	}
	internal static class f
	{
		internal const string A = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec";

		private static readonly Dictionary<long, long> m_A = new Dictionary<long, long>
		{
			{ 112918560L, 112926416L },
			{ 98614988L, 98623180L },
			{ 102260400L, 102268480L },
			{ 102260448L, 102268528L },
			{ 102260680L, 102268760L },
			{ 102261280L, 102269360L },
			{ 112837064L, 112844920L },
			{ 64684536L, 64688232L },
			{ 64683768L, 64687464L },
			{ 64823200L, 64834544L },
			{ 65511760L, 65515872L },
			{ 65513472L, 65517584L },
			{ 65713904L, 65717936L },
			{ 64430720L, 64434816L },
			{ 64431008L, 64435104L },
			{ 64432864L, 64436960L },
			{ 64815280L, 64826624L },
			{ 64815656L, 64827000L },
			{ 111878264L, 111886360L },
			{ 111989640L, 111997736L },
			{ 111989680L, 111997776L },
			{ 64584880L, 64588784L },
			{ 64568064L, 64571968L },
			{ 65313128L, 65317432L },
			{ 65307976L, 65312280L },
			{ 64559016L, 64562912L },
			{ 13437424L, 13438736L },
			{ 64557040L, 64561240L },
			{ 64557304L, 64561504L },
			{ 103319618L, 103327714L },
			{ 103319626L, 103327722L },
			{ 103319627L, 103327723L },
			{ 103319632L, 103327728L },
			{ 103319633L, 103327729L },
			{ 103319634L, 103327730L },
			{ 103319637L, 103327733L },
			{ 103319697L, 103327793L }
		};

		private static readonly long[] m_A = new long[37]
		{
			112918560L, 98614988L, 102260400L, 102260448L, 102260680L, 102261280L, 112837064L, 64684536L, 64683768L, 64823200L,
			65511760L, 65513472L, 65713904L, 64430720L, 64431008L, 64432864L, 64815280L, 64815656L, 111878264L, 111989640L,
			111989680L, 64584880L, 64568064L, 65313128L, 65307976L, 64559016L, 13437424L, 64557040L, 64557304L, 103319618L,
			103319626L, 103319627L, 103319632L, 103319633L, 103319634L, 103319637L, 103319697L
		};

		internal static void A()
		{
			long[] array = f.m_A;
			for (int num = 0; num < array.Length; num++)
			{
				A(array[num]);
			}
		}

		internal static long A(long P_0)
		{
			if (!f.m_A.TryGetValue(P_0, out var value))
			{
				throw new InvalidOperationException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dp() + P_0.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DQ()));
			}
			return value;
		}

		internal static long a(long P_0)
		{
			long[] array = new long[5] { 64430720L, 64431008L, 64432864L, 64815280L, 64815656L };
			foreach (long num2 in array)
			{
				if (f.m_A[num2] == P_0)
				{
					return num2;
				}
			}
			throw new InvalidOperationException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dq() + P_0.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DQ()));
		}
	}
}
