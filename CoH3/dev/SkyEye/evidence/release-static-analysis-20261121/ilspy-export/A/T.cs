using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;
using b;

namespace A
{
	internal sealed class T
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
	internal static class t
	{
		[CompilerGenerated]
		private sealed class A
		{
			public global::B.O A;

			internal bool A(int P_0)
			{
				a a2 = new a
				{
					A = P_0
				};
				return this.A.A.Any(a2.A);
			}

			internal string a(int P_0)
			{
				B b2 = new B
				{
					A = P_0
				};
				return t.A(b2.A) + (this.A.A.Any(b2.A) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xi() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XI());
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public int A;

			internal bool A(global::b.K P_0)
			{
				return P_0.A == this.A;
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public int A;

			internal bool A(global::b.K P_0)
			{
				if (P_0.A == this.A)
				{
					return P_0.A;
				}
				return false;
			}
		}

		private static readonly Dictionary<string, int> m_A = new Dictionary<string, int>
		{
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CR(),
				1
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cr(),
				3
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CS(),
				2
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cs(),
				4
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CT(),
				1
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ct(),
				2
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CU(),
				3
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cu(),
				1
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CV(),
				2
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cv(),
				3
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CW(),
				4
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cw(),
				1
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CX(),
				2
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cx(),
				3
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CY(),
				1
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cy(),
				2
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CZ(),
				3
			}
		};

		public static int A(string P_0)
		{
			if (!t.m_A.TryGetValue(P_0.Substring(P_0.LastIndexOf('\\') + 1), out var value))
			{
				return 0;
			}
			return value;
		}

		public static string A(int P_0)
		{
			return (new string[5]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CN(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cn(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CO(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Co()
			})[Math.Max(0, Math.Min(4, P_0))];
		}

		public static string A(global::B.O P_0)
		{
			if (!P_0.d)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CP();
			}
			int count = ((P_0.b == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cp() || P_0.b == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CQ() || P_0.b == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cq()) ? 3 : 4);
			return string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba(), Enumerable.Range(1, count).Where(delegate(int num)
			{
				a a2 = new a();
				a2.A = num;
				return P_0.A.Any(a2.A);
			}).Select(delegate(int num)
			{
				B b2 = new B();
				b2.A = num;
				return A(b2.A) + (P_0.A.Any(b2.A) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xi() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XI());
			}));
		}
	}
}
namespace a
{
	internal sealed class T
	{
		public global::A.L A;

		public bool A;

		public bool a;

		public float A;

		public int A;

		public uint A;
	}
	internal static class t
	{
		public static bool A(string P_0, out float P_1, out float P_2)
		{
			P_1 = (P_2 = 0f);
			if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ER()))
			{
				if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Er()))
				{
					if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ES()))
					{
						if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Es()))
						{
							if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ET()))
							{
								if (P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Et())
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
