using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;
using b;

namespace A
{
	internal static class P
	{
		[CompilerGenerated]
		private sealed class A
		{
			public string A;

			internal bool A(string P_0)
			{
				if (!(this.A == P_0))
				{
					return this.A.StartsWith(P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xg(), StringComparison.Ordinal);
				}
				return true;
			}
		}

		private static readonly string[] m_A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bt().Split(' ');

		public static bool A(string P_0)
		{
			if (string.IsNullOrEmpty(P_0))
			{
				return false;
			}
			string text = P_0.Replace('/', '\\').ToLowerInvariant();
			if (!text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bS()) || !text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bs()))
			{
				return false;
			}
			string A = text.Substring(text.LastIndexOf('\\') + 1);
			if (A.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bT()))
			{
				A = A.Substring(0, A.Length - 4);
			}
			return P.m_A.Any((string text2) => A == text2 || A.StartsWith(text2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xg(), StringComparison.Ordinal));
		}
	}
	internal sealed class p
	{
		public bool A;

		public string A;

		public string a;

		public long A;

		public bool a;

		public readonly Dictionary<uint, int> A = new Dictionary<uint, int>();

		public string A => this.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bU();

		public static bool A(string P_0)
		{
			return string.Equals(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bu(), StringComparison.OrdinalIgnoreCase);
		}

		public static bool a(string P_0)
		{
			if (!A(P_0))
			{
				if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bV()) && (P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bs()) || P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bv())))
				{
					if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bH()) && !P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bW()))
					{
						return P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bw());
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public static string B(string P_0)
		{
			string text = r.a(P_0);
			if (text != null)
			{
				return global::b.k.A(text);
			}
			if (!A(P_0))
			{
				if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bH()))
				{
					if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bW()))
					{
						if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bX()))
						{
							if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bx()))
							{
								if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bY()))
								{
									return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.by();
								}
								return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bZ();
							}
							return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bz();
						}
						return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CA();
					}
					return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ca();
				}
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bh();
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CB();
		}
	}
}
namespace a
{
	internal sealed class P : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public global::A.D A;

			internal void A(object P_0, KeyEventArgs P_1)
			{
				if (P_1.KeyCode == Keys.Return || P_1.KeyCode == Keys.Escape)
				{
					this.A.a();
				}
			}
		}

		public P(string P_0, string P_1, string P_2 = "确认")
		{
			Text = P_0;
			base.ClientSize = new Size(480, 232);
			base.StartPosition = FormStartPosition.CenterParent;
			base.FormBorderStyle = FormBorderStyle.None;
			BackColor = global::B.L.A;
			ForeColor = global::B.L.b;
			Font = J.A();
			base.Icon = J.A;
			global::b.h h2 = new global::b.h
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(26)
			};
			base.Controls.Add(h2);
			h2.Controls.Add(new Label
			{
				Text = P_0,
				Font = J.A(17f, FontStyle.Bold),
				ForeColor = global::B.L.D,
				Bounds = new Rectangle(26, 23, 420, 36)
			});
			h2.Controls.Add(new Label
			{
				Text = P_1,
				ForeColor = global::B.L.b,
				Bounds = new Rectangle(26, 80, 420, 66)
			});
			global::A.D A = new global::A.D
			{
				Text = P_2,
				A = true,
				Bounds = new Rectangle(330, 170, 122, 36)
			};
			global::A.D obj = A;
			EventHandler value = delegate
			{
				base.DialogResult = DialogResult.OK;
				Close();
			};
			obj.Click += value;
			h2.Controls.Add(A);
			base.KeyPreview = true;
			base.KeyDown += delegate(object obj2, KeyEventArgs e2)
			{
				if (e2.KeyCode == Keys.Return || e2.KeyCode == Keys.Escape)
				{
					A.a();
				}
			};
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}
	internal sealed class p
	{
		private readonly Dictionary<string, object> m_A;

		public readonly long A;

		[CompilerGenerated]
		private bool m_A;

		public string A => A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EH());

		public bool a
		{
			[CompilerGenerated]
			get
			{
				return this.m_A;
			}
			[CompilerGenerated]
			private set
			{
				this.m_A = flag;
			}
		}

		internal void A(K P_0)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if (P_0 == null || !P_0.A(this, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Eh(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EI(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ei()))
			{
				throw new HttpRequestException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EJ());
			}
			this.a = true;
		}

		public p(Dictionary<string, object> P_0, long P_1)
		{
			this.m_A = new Dictionary<string, object>(P_0);
			this.A = P_1;
		}

		public string A(string P_0)
		{
			if (!this.m_A.TryGetValue(P_0, out var value))
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
			return Convert.ToString(value, CultureInfo.InvariantCulture);
		}

		public long a(string P_0)
		{
			if (!long.TryParse(A(P_0), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				return 0L;
			}
			return result;
		}
	}
}
