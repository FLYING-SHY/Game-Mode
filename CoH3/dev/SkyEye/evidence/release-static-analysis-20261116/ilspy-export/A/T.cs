using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
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
				return T.A(b2.A) + (this.A.A.Any(b2.A) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XE() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xd());
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
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cq(),
				1
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CR(),
				3
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cr(),
				2
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CS(),
				4
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cs(),
				1
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CT(),
				2
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ct(),
				3
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CU(),
				1
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cu(),
				2
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CV(),
				3
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cv(),
				4
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CW(),
				1
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cw(),
				2
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CX(),
				3
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cx(),
				1
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CY(),
				2
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cy(),
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
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cm(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CN(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cn(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CO()
			})[Math.Max(0, Math.Min(4, P_0))];
		}

		public static string A(global::B.N P_0)
		{
			if (!P_0.d)
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Co();
			}
			int count = ((P_0.b == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CP() || P_0.b == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cp() || P_0.b == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CQ()) ? 3 : 4);
			return string.Join(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA(), Enumerable.Range(1, count).Where(delegate(int num)
			{
				a a2 = new a();
				a2.A = num;
				return P_0.A.Any(a2.A);
			}).Select(delegate(int num)
			{
				B b2 = new B();
				b2.A = num;
				return A(b2.A) + (P_0.A.Any(b2.A) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XE() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xd());
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
			using FontFamily family = new FontFamily(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba());
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
