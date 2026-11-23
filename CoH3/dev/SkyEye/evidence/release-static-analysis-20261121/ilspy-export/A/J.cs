using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;

namespace A
{
	internal sealed class J
	{
		public string A;

		public string a;

		public long A;

		public double A;

		public L A;

		public bool A;

		public bool a;

		public float A;

		public float a;
	}
	internal sealed class j
	{
		public bool A = true;

		public bool a = true;

		public bool B = true;

		public int A = 25;

		public int a = 30;

		public int B = 14;

		public int b = 10;

		public decimal A = 2m;

		public j A()
		{
			return (j)MemberwiseClone();
		}

		public IEnumerable<string> a()
		{
			return new string[8]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bh() + (this.A ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BI() + (this.a ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bi() + (this.B ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BJ() + this.A,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bj() + this.a,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BK() + B,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bk() + b,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BL() + this.A.ToString(CultureInfo.InvariantCulture)
			};
		}

		public bool A(string P_0)
		{
			int num = P_0.IndexOf('=');
			if (num < 0)
			{
				return false;
			}
			string text = P_0.Substring(0, num);
			string text2 = P_0.Substring(num + 1);
			if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bl())
			{
				if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
				{
					this.A = Math.Max(0.5m, Math.Min(8m, result));
				}
				return true;
			}
			if (!new string[7]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BM(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bm(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BN(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bn(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BO(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bo(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BP()
			}.Contains(text))
			{
				return false;
			}
			if (!int.TryParse(text2, out var result2))
			{
				return true;
			}
			if (text != null)
			{
				switch (text.Length)
				{
				case 5:
					switch (text[3])
					{
					case 'O':
						if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BM())
						{
							this.A = result2 != 0;
						}
						break;
					case 'V':
						if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bm())
						{
							this.a = result2 != 0;
						}
						break;
					}
					break;
				case 9:
					if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BN())
					{
						this.B = result2 != 0;
					}
					break;
				case 10:
					if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bn())
					{
						this.A = Math.Max(0, Math.Min(100, result2));
					}
					break;
				case 14:
					if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BO())
					{
						this.a = Math.Max(0, Math.Min(100, result2));
					}
					break;
				case 7:
					if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bo())
					{
						B = Math.Max(8, Math.Min(40, result2));
					}
					break;
				case 11:
					if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BP())
					{
						b = Math.Max(6, Math.Min(24, result2));
					}
					break;
				}
			}
			return true;
		}
	}
}
namespace a
{
	internal static class J
	{
		private static Icon m_A;

		public static readonly Color A = Color.FromArgb(8, 15, 24);

		public static readonly Color a = Color.FromArgb(13, 26, 38);

		public static readonly Color B = Color.FromArgb(220, 235, 241);

		public static readonly Color b = Color.FromArgb(104, 122, 138);

		public static readonly Color C = Color.FromArgb(57, 87, 105);

		public static readonly Color c = Color.FromArgb(102, 207, 222);

		public static readonly Color D = Color.FromArgb(20, 35, 49);

		public static Icon A
		{
			get
			{
				if (J.m_A == null)
				{
					using Stream stream = typeof(J).Assembly.GetManifestResourceStream(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dg());
					J.m_A = new Icon(stream);
				}
				return J.m_A;
			}
		}

		public static Font A(float P_0 = 10f, FontStyle P_1 = FontStyle.Regular)
		{
			return new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), P_0 * 96f / 72f, P_1, GraphicsUnit.Pixel);
		}

		public static Label A(string P_0, int P_1 = 28, float P_2 = 10f, Color? P_3 = null)
		{
			return new Label
			{
				Text = P_0,
				Height = P_1,
				Dock = DockStyle.Top,
				ForeColor = (P_3 ?? B),
				Font = A(P_2),
				AutoEllipsis = true,
				TextAlign = ContentAlignment.MiddleLeft
			};
		}
	}
	internal static class j
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.Q, string> A;

			public static Func<global::B.Q, string> a;

			internal string A(global::B.Q P_0)
			{
				return P_0.a;
			}

			internal string a(global::B.Q P_0)
			{
				return P_0.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Du();
			}
		}

		public static bool A(string P_0)
		{
			if (P_0 != null && P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dH(), StringComparison.OrdinalIgnoreCase))
			{
				return P_0.Substring(P_0.LastIndexOf('\\') + 1).StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dh(), StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		public static bool a(string P_0)
		{
			return string.Equals(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dI(), StringComparison.OrdinalIgnoreCase);
		}

		public static string A(global::B.O P_0)
		{
			if (!P_0.a || P_0.A.Count == 0)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.di() + string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cE(), from q2 in P_0.A.OrderBy((global::B.Q q2) => q2.a, StringComparer.Ordinal)
				select q2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Du());
		}
	}
}
