using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using B;
using b;

namespace A
{
	internal static class T
	{
		[CompilerGenerated]
		private sealed class A
		{
			public global::B.N A;

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
				return T.A(b2.A) + (this.A.A.Any(b2.A) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XE() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xd());
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public int A;

			internal bool A(global::b.J P_0)
			{
				return P_0.A == this.A;
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public int A;

			internal bool A(global::b.J P_0)
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
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cq(),
				1
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CR(),
				3
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cr(),
				2
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CS(),
				4
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cs(),
				1
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CT(),
				2
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ct(),
				3
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CU(),
				1
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cu(),
				2
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CV(),
				3
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cv(),
				4
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CW(),
				1
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cw(),
				2
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CX(),
				3
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cx(),
				1
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CY(),
				2
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cy(),
				3
			}
		};

		public static int A(string P_0)
		{
			if (!T.m_A.TryGetValue(P_0.Substring(P_0.LastIndexOf('\\') + 1), out var value))
			{
				return 0;
			}
			return value;
		}

		public static string A(int P_0)
		{
			return (new string[5]
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cm(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CN(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cn(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CO()
			})[Math.Max(0, Math.Min(4, P_0))];
		}

		public static string A(global::B.N P_0)
		{
			if (!P_0.d)
			{
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Co();
			}
			int count = ((P_0.b == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CP() || P_0.b == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cp() || P_0.b == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CQ()) ? 3 : 4);
			return string.Join(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA(), Enumerable.Range(1, count).Where(delegate(int num)
			{
				a a2 = new a();
				a2.A = num;
				return P_0.A.Any(a2.A);
			}).Select(delegate(int num)
			{
				B b2 = new B();
				b2.A = num;
				return A(b2.A) + (P_0.A.Any(b2.A) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XE() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xd());
			}));
		}
	}
	internal static class t
	{
		public static bool A(byte[] P_0, byte[] P_1)
		{
			if (P_0.Length != P_1.Length)
			{
				return false;
			}
			for (int num = 0; num < P_0.Length; num++)
			{
				if (P_0[num] != P_1[num])
				{
					return false;
				}
			}
			return true;
		}
	}
}
namespace a
{
	internal static class T
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.Z, bool> A;

			internal bool A(global::B.Z P_0)
			{
				if (P_0.A && !P_0.a)
				{
					return !string.IsNullOrEmpty(P_0.a);
				}
				return false;
			}
		}

		public static void A(Graphics P_0, IEnumerable<global::B.Z> P_1, y P_2, global::B.B P_3, int P_4, int P_5)
		{
			if (!P_3.A)
			{
				return;
			}
			using StringFormat format = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Far
			};
			using FontFamily family = new FontFamily(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba());
			foreach (global::B.Z item in P_1.Where((global::B.Z z2) => z2.A && !z2.a && !string.IsNullOrEmpty(z2.a)))
			{
				if (!P_2.A(item.A, item.B, P_4, P_5, out var origin))
				{
					continue;
				}
				origin.Y -= (float)P_3.B / 2f + 3f;
				using GraphicsPath graphicsPath = new GraphicsPath();
				graphicsPath.AddString(item.a, family, 1, P_3.A, origin, format);
				using Pen pen = new Pen(Color.Black, 2.5f)
				{
					LineJoin = LineJoin.Round
				};
				using SolidBrush brush = new SolidBrush(item.A);
				P_0.DrawPath(pen, graphicsPath);
				P_0.FillPath(brush, graphicsPath);
			}
		}
	}
	internal sealed class t
	{
		public Bitmap A;

		public Rectangle A;
	}
}
