using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using B;
using Microsoft.Win32;
using a;
using b;

namespace A
{
	internal static class M
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::a.E, bool> A;

			public static Func<global::a.E, bool> a;

			public static Func<global::a.E, bool> B;

			internal bool A(global::a.E P_0)
			{
				return P_0.A == L.a;
			}

			internal bool a(global::a.E P_0)
			{
				return P_0.A == L.B;
			}

			internal bool B(global::a.E P_0)
			{
				return P_0.A == L.b;
			}
		}

		private static void A(bool P_0, string P_1)
		{
			if (!P_0)
			{
				throw new Exception(P_1);
			}
		}

		public static void A(List<string> P_0)
		{
			global::B.N n2 = new global::B.N
			{
				A = 2u,
				A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BZ(),
				a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()
			};
			global::B.N n3 = new global::B.N
			{
				A = 3u,
				A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bA(),
				a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bA()
			};
			List<global::B.N> list = new List<global::B.N> { n2, n3 };
			z z2 = new z();
			z2.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba());
			m obj = new m();
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bB());
			n2.A.Add(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bb());
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count((global::a.E e2) => e2.A == L.a) == 1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bC());
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bc());
			global::b.J j2 = new global::b.J
			{
				A = 2,
				A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bD(),
				A = false
			};
			n2.A.Add(j2);
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bd());
			j2.A = true;
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bE());
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count((global::a.E e2) => e2.A == L.B) == 1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.be());
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bF());
			z2.A(n2, false);
			n2.A.Add(new global::b.J
			{
				A = 3,
				A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bf(),
				A = true
			});
			obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2);
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bG());
			P p2 = new P
			{
				A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bg(),
				a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bH(),
				a = true
			};
			p2.A[17u] = 360;
			n3.A.Add(p2);
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bh());
			p2.A[17u] = 4;
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bI());
			p2.A.Clear();
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count((global::a.E e2) => e2.A == L.b) == 1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bi());
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bJ());
			p2.A[17u] = 100;
			obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2);
			n3.A.Clear();
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ba(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bj());
			A(obj.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bK(), list, z2).Count == 0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bk());
			using (l l2 = new l())
			{
				foreach (L value in Enum.GetValues(typeof(L)))
				{
					if (!string.IsNullOrEmpty(l2.A(value)))
					{
						using (new Q(l2.A(value)))
						{
						}
					}
				}
			}
			P_0.Add(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bL());
		}
	}
	internal sealed class m
	{
		[CompilerGenerated]
		private sealed class A
		{
			public HashSet<string> A;

			public HashSet<string> a;

			public m A;

			internal bool A(string P_0)
			{
				return !this.A.Contains(P_0);
			}

			internal bool a(string P_0)
			{
				return !this.a.Contains(P_0);
			}

			internal bool B(string P_0)
			{
				return !this.A.m_A.Contains(P_0);
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public global::B.N A;

			public Func<KeyValuePair<string, uint>, bool> A;

			internal bool A(KeyValuePair<string, uint> P_0)
			{
				return P_0.Value == this.A.A;
			}
		}

		private readonly HashSet<string> m_A = new HashSet<string>();

		private readonly Dictionary<string, uint> m_A = new Dictionary<string, uint>();

		private readonly HashSet<uint> m_A = new HashSet<uint>();

		private string m_A;

		private bool m_A;

		private readonly Dictionary<uint, string> m_A = new Dictionary<uint, string>();

		private readonly HashSet<string> m_a = new HashSet<string>();

		private readonly Dictionary<string, int> m_A = new Dictionary<string, int>();

		private readonly Dictionary<string, Dictionary<uint, int>> m_A = new Dictionary<string, Dictionary<uint, int>>();

		public void A()
		{
			this.m_A = false;
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_a.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
		}

		public List<global::a.E> A(string P_0, List<global::B.N> P_1, z P_2)
		{
			if (this.m_A != P_0)
			{
				this.A();
				this.m_A = P_0;
			}
			List<global::a.E> list = new List<global::a.E>();
			HashSet<string> A = new HashSet<string>();
			HashSet<string> a2 = new HashSet<string>();
			HashSet<string> hashSet = new HashSet<string>();
			foreach (global::B.N A2 in P_1)
			{
				if (A2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz())
				{
					if (!A2.B)
					{
						foreach (KeyValuePair<string, uint> item in this.m_A.Where((KeyValuePair<string, uint> keyValuePair) => keyValuePair.Value == A2.A))
						{
							hashSet.Add(item.Key);
						}
					}
					foreach (s item2 in A2.A)
					{
						string text = item2.a ?? item2.A;
						this.m_A[text] = A2.A;
						hashSet.Add(text);
						if (this.m_A.Add(text) && this.m_A && P_2.a(A2))
						{
							list.Add(new global::a.E
							{
								A = L.D,
								A = A2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + item2.B + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bl()
							});
						}
					}
					if (global::a.f.a(A2) && this.m_A.Add(A2.A) && this.m_A && P_2.a(A2))
					{
						list.Add(new global::a.E
						{
							A = L.C,
							A = A2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bM()
						});
					}
					string text2 = U.A(A2);
					string value;
					bool flag = this.m_A.TryGetValue(A2.A, out value);
					if (this.m_A && flag && text2.Length > 0 && text2 != value && P_2.a(A2))
					{
						list.Add(new global::a.E
						{
							A = L.a,
							A = A2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bm() + U.a(A2)
						});
					}
					this.m_A[A2.A] = text2;
					foreach (global::b.J item3 in A2.A)
					{
						string text3 = A2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ax() + item3.A;
						A.Add(text3);
						if (!item3.A)
						{
							this.m_A.Remove(text3);
							continue;
						}
						this.m_A.TryGetValue(text3, out var value2);
						this.m_A[text3] = value2 + 1;
						if (!this.m_A)
						{
							this.m_a.Add(text3);
						}
						else if (value2 + 1 >= 2 && this.m_a.Add(text3) && P_2.a(A2))
						{
							list.Add(new global::a.E
							{
								A = L.B,
								A = A2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + T.A(item3.A) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bN()
							});
						}
					}
				}
				if (!(A2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bA()))
				{
					continue;
				}
				foreach (P item4 in A2.A)
				{
					a2.Add(item4.A);
					if (!item4.a)
					{
						this.m_A.Remove(item4.A);
						continue;
					}
					if (this.m_A && this.m_A.TryGetValue(item4.A, out var value3))
					{
						foreach (KeyValuePair<uint, int> item5 in value3)
						{
							if (item5.Value > 0 && (!item4.A.TryGetValue(item5.Key, out var value4) || value4 <= 0))
							{
								list.Add(new global::a.E
								{
									A = L.b,
									A = item4.A
								});
							}
						}
					}
					this.m_A[item4.A] = new Dictionary<uint, int>(item4.A);
				}
			}
			string[] array = this.m_A.Keys.Where((string item) => !A.Contains(item)).ToArray();
			foreach (string key in array)
			{
				this.m_A.Remove(key);
			}
			array = this.m_A.Keys.Where((string item) => !a2.Contains(item)).ToArray();
			foreach (string key2 in array)
			{
				this.m_A.Remove(key2);
			}
			this.m_A.IntersectWith(hashSet);
			array = this.m_A.Keys.Where((string item) => !this.m_A.Contains(item)).ToArray();
			foreach (string key3 in array)
			{
				this.m_A.Remove(key3);
			}
			this.m_A = true;
			return list;
		}
	}
}
namespace a
{
	internal sealed class M
	{
		public string Installation;

		public string Code;

		public bool Remember = true;

		public bool AutoLogin = true;
	}
	internal static class m
	{
		public static string A()
		{
			using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Eb());
			string text = registryKey?.GetValue(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EC()) as string;
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new PlatformNotSupportedException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ec());
			}
			return A(L.A, text);
		}

		internal static string A(string P_0, string P_1)
		{
			using SHA256 sHA = SHA256.Create();
			return BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(P_0 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ED() + P_1.Trim().ToLowerInvariant()))).Replace(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ci(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
		}
	}
}
