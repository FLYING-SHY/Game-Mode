using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using B;
using a;
using b;

namespace A
{
	internal sealed class O
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.N, bool> A;

			internal bool A(global::B.N P_0)
			{
				return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz();
			}
		}

		private string m_A;

		private readonly HashSet<string> m_A = new HashSet<string>();

		private readonly HashSet<string> a = new HashSet<string>();

		public void A()
		{
			this.m_A.Clear();
		}

		public List<global::a.E> A(string P_0, IEnumerable<global::B.N> P_1, z P_2)
		{
			if (this.m_A != P_0)
			{
				this.m_A = P_0;
				this.m_A.Clear();
				a.Clear();
			}
			List<global::a.E> list = new List<global::a.E>();
			foreach (global::B.N item2 in P_1.Where((global::B.N n2) => n2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
			{
				string text = item2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq() + item2.a + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq() + item2.A;
				if (!item2.D)
				{
					this.m_A.Remove(text);
					continue;
				}
				bool flag = this.m_A.Contains(text);
				foreach (global::B.P item3 in item2.a)
				{
					if (!string.IsNullOrEmpty(item3.a))
					{
						string item = text + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq() + item3.a + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq() + item3.A;
						if (a.Add(item) && flag && P_2.a(item2) && o.A(item3.A))
						{
							list.Add(new global::a.E
							{
								A = L.c,
								A = item2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bR() + global::b.j.A(item3.A)
							});
						}
					}
				}
				this.m_A.Add(text);
			}
			return list;
		}
	}
	internal static class o
	{
		[CompilerGenerated]
		private sealed class A
		{
			public string A;

			internal bool A(string P_0)
			{
				if (!(this.A == P_0))
				{
					return this.A.StartsWith(P_0 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XC(), StringComparison.Ordinal);
				}
				return true;
			}
		}

		private static readonly string[] m_A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bT().Split(' ');

		public static bool A(string P_0)
		{
			if (string.IsNullOrEmpty(P_0))
			{
				return false;
			}
			string text = P_0.Replace('/', '\\').ToLowerInvariant();
			if (!text.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.br()) || !text.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bS()))
			{
				return false;
			}
			string A = text.Substring(text.LastIndexOf('\\') + 1);
			if (A.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bs()))
			{
				A = A.Substring(0, A.Length - 4);
			}
			return o.m_A.Any((string text2) => A == text2 || A.StartsWith(text2 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XC(), StringComparison.Ordinal));
		}
	}
}
namespace a
{
	internal sealed class O : Form
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

		public O(string P_0, string P_1, string P_2 = "确认")
		{
			Text = P_0;
			base.ClientSize = new Size(480, 232);
			base.StartPosition = FormStartPosition.CenterParent;
			base.FormBorderStyle = FormBorderStyle.None;
			BackColor = global::B.K.A;
			ForeColor = global::B.K.b;
			Font = I.A();
			base.Icon = I.A;
			global::b.g g2 = new global::b.g
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(26)
			};
			base.Controls.Add(g2);
			g2.Controls.Add(new Label
			{
				Text = P_0,
				Font = I.A(17f, FontStyle.Bold),
				ForeColor = global::B.K.D,
				Bounds = new Rectangle(26, 23, 420, 36)
			});
			g2.Controls.Add(new Label
			{
				Text = P_1,
				ForeColor = global::B.K.b,
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
			g2.Controls.Add(A);
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
	internal sealed class o
	{
		private readonly Dictionary<string, object> m_A;

		public readonly long A;

		[CompilerGenerated]
		private bool m_A;

		public string A => A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ed());

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

		internal void A(J P_0)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if (P_0 == null || !P_0.A(this, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EE(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ee(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EF()))
			{
				throw new HttpRequestException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ef());
			}
			this.a = true;
		}

		public o(Dictionary<string, object> P_0, long P_1)
		{
			this.m_A = new Dictionary<string, object>(P_0);
			this.A = P_1;
		}

		public string A(string P_0)
		{
			if (!this.m_A.TryGetValue(P_0, out var value))
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
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
