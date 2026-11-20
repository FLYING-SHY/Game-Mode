using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
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
				A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BZ(),
				a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()
			};
			global::B.N n3 = new global::B.N
			{
				A = 3u,
				A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA(),
				a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA()
			};
			List<global::B.N> list = new List<global::B.N> { n2, n3 };
			z z2 = new z();
			z2.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba());
			m obj = new m();
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bB());
			n2.A.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bb());
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count((global::a.E e2) => e2.A == L.a) == 1, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bC());
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bc());
			global::b.J j2 = new global::b.J
			{
				A = 2,
				A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bD(),
				A = false
			};
			n2.A.Add(j2);
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bd());
			j2.A = true;
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bE());
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count((global::a.E e2) => e2.A == L.B) == 1, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.be());
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bF());
			z2.A(n2, false);
			n2.A.Add(new global::b.J
			{
				A = 3,
				A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bf(),
				A = true
			});
			obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2);
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bG());
			P p2 = new P
			{
				A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bg(),
				a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bH(),
				a = true
			};
			p2.A[17u] = 360;
			n3.A.Add(p2);
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bh());
			p2.A[17u] = 4;
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bI());
			p2.A.Clear();
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count((global::a.E e2) => e2.A == L.b) == 1, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bi());
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bJ());
			p2.A[17u] = 100;
			obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2);
			n3.A.Clear();
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ba(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bj());
			A(obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bK(), list, z2).Count == 0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bk());
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
			P_0.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bL());
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
				if (A2.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz())
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
								A = A2.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA() + item2.B + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bl()
							});
						}
					}
					if (global::a.f.a(A2) && this.m_A.Add(A2.A) && this.m_A && P_2.a(A2))
					{
						list.Add(new global::a.E
						{
							A = L.C,
							A = A2.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bM()
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
							A = A2.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bm() + U.a(A2)
						});
					}
					this.m_A[A2.A] = text2;
					foreach (global::b.J item3 in A2.A)
					{
						string text3 = A2.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + item3.A;
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
								A = A2.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA() + T.A(item3.A) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bN()
							});
						}
					}
				}
				if (!(A2.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA()))
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
			using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Eb());
			string text = registryKey?.GetValue(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EC()) as string;
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new PlatformNotSupportedException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ec());
			}
			return A(L.A, text);
		}

		internal static string A(string P_0, string P_1)
		{
			using SHA256 sHA = SHA256.Create();
			return BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(P_0 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ED() + P_1.Trim().ToLowerInvariant()))).Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ci(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
		}
	}
}
