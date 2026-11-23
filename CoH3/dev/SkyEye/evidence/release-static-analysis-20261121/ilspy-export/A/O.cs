using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using a;
using b;

namespace A
{
	internal sealed class O : Form
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.O, bool> A;

			public static Func<global::B.O, uint> A;

			internal bool A(global::B.O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal uint a(global::B.O P_0)
			{
				return P_0.A;
			}
		}

		private readonly CheckedListBox m_A = new CheckedListBox();

		private readonly Func<global::B.O[]> m_A;

		private readonly global::a.A m_A;

		private readonly System.Windows.Forms.Timer m_A = new System.Windows.Forms.Timer();

		private bool m_A;

		public O(Func<global::B.O[]> P_0, global::a.A P_1)
		{
			this.m_A = P_0;
			this.m_A = P_1;
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bo();
			base.ClientSize = new Size(580, 350);
			MinimumSize = new Size(580, 350);
			Font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), 10f);
			base.StartPosition = FormStartPosition.CenterParent;
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(14),
				ColumnCount = 1,
				RowCount = 3
			};
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 78f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
			base.Controls.Add(tableLayoutPanel);
			tableLayoutPanel.Controls.Add(new Label
			{
				Dock = DockStyle.Fill,
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bP()
			}, 0, 0);
			this.m_A.Dock = DockStyle.Fill;
			this.m_A.CheckOnClick = true;
			this.m_A.IntegralHeight = false;
			tableLayoutPanel.Controls.Add(this.m_A, 0, 1);
			CheckedListBox checkedListBox = this.m_A;
			ItemCheckEventHandler value = delegate(object obj, ItemCheckEventArgs e2)
			{
				if (!this.m_A)
				{
					this.m_A.A(((Z)this.m_A.Items[e2.Index]).A, e2.NewValue == CheckState.Checked);
				}
			};
			checkedListBox.ItemCheck += value;
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill
			};
			tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 2);
			Button button = new Button
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bp(),
				AutoSize = true
			};
			Button button2 = new Button
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bQ(),
				AutoSize = true
			};
			Button button3 = new Button
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bq(),
				AutoSize = true
			};
			flowLayoutPanel.Controls.AddRange(new Control[3] { button, button2, button3 });
			button.Click += delegate
			{
				A(true);
			};
			button2.Click += delegate
			{
				A(false);
			};
			button3.Click += delegate
			{
				Close();
			};
			A();
			this.m_A.Interval = 1000;
			this.m_A.Tick += delegate
			{
				A();
			};
			this.m_A.Start();
		}

		private void A(bool P_0)
		{
			global::B.O[] array = this.m_A();
			foreach (global::B.O o2 in array)
			{
				this.m_A.A(o2, P_0);
			}
			A();
		}

		private void A()
		{
			global::B.O[] array = (from P_0 in this.m_A()
				where P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()
				orderby P_0.A
				select P_0).ToArray();
			this.m_A = true;
			try
			{
				bool flag = this.m_A.Items.Count == array.Length;
				int num = 0;
				while (flag && num < array.Length)
				{
					global::B.O o2 = ((Z)this.m_A.Items[num]).A;
					flag = o2.A == array[num].A && o2.A == array[num].A;
					num++;
				}
				if (!flag)
				{
					this.m_A.Items.Clear();
					global::B.O[] array2 = array;
					foreach (global::B.O o3 in array2)
					{
						this.m_A.Items.Add(new Z
						{
							A = o3
						}, this.m_A.a(o3));
					}
				}
				else
				{
					for (int num3 = 0; num3 < array.Length; num3++)
					{
						((Z)this.m_A.Items[num3]).A = array[num3];
						this.m_A.SetItemChecked(num3, this.m_A.a(array[num3]));
					}
				}
			}
			finally
			{
				this.m_A = false;
			}
		}

		protected override void Dispose(bool P_0)
		{
			if (P_0)
			{
				this.m_A.Dispose();
			}
			base.Dispose(P_0);
		}

		[CompilerGenerated]
		private void A(object P_0, ItemCheckEventArgs P_1)
		{
			if (!this.m_A)
			{
				this.m_A.A(((Z)this.m_A.Items[P_1.Index]).A, P_1.NewValue == CheckState.Checked);
			}
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			A(true);
		}

		[CompilerGenerated]
		private void a(object P_0, EventArgs P_1)
		{
			A(false);
		}

		[CompilerGenerated]
		private void B(object P_0, EventArgs P_1)
		{
			Close();
		}

		[CompilerGenerated]
		private void b(object P_0, EventArgs P_1)
		{
			A();
		}
	}
	internal sealed class o
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.O, bool> A;

			internal bool A(global::B.O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}
		}

		private string m_A;

		private readonly HashSet<string> m_A = new HashSet<string>();

		private readonly HashSet<string> a = new HashSet<string>();

		public void A()
		{
			this.m_A.Clear();
		}

		public List<global::a.e> A(string P_0, IEnumerable<global::B.O> P_1, global::a.A P_2)
		{
			if (this.m_A != P_0)
			{
				this.m_A = P_0;
				this.m_A.Clear();
				a.Clear();
			}
			List<global::a.e> list = new List<global::a.e>();
			foreach (global::B.O item2 in P_1.Where((global::B.O o2) => o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
			{
				string text = item2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + item2.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + item2.A;
				if (!item2.D)
				{
					this.m_A.Remove(text);
					continue;
				}
				bool flag = this.m_A.Contains(text);
				foreach (global::B.Q item3 in item2.a)
				{
					if (!string.IsNullOrEmpty(item3.a))
					{
						string item = text + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + item3.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + item3.A;
						if (a.Add(item) && flag && P_2.a(item2) && P.A(item3.A))
						{
							list.Add(new global::a.e
							{
								A = l.c,
								A = item2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.br() + global::b.k.A(item3.A)
							});
						}
					}
				}
				this.m_A.Add(text);
			}
			return list;
		}
	}
}
namespace a
{
	internal static class O
	{
		private static o m_A = new o();

		private static long m_A;

		public static bool A => Volatile.Read(ref O.m_A).A > l.A;

		public static long a => Volatile.Read(ref O.m_A).a;

		public static bool A(int P_0, DateTime P_1)
		{
			o o2 = Volatile.Read(ref O.m_A);
			if (O.A && o2.A == P_0)
			{
				return o2.A == P_1;
			}
			return false;
		}

		internal static bool A(long P_0, long P_1, long P_2, long P_3, F P_4)
		{
			long num = Math.Min(P_1, P_2) - P_0;
			if (P_4 == null || num <= 0 || num > 60 || P_2 < P_1)
			{
				return false;
			}
			long num2 = P_3 + num * 1000 - 1000;
			if (num2 <= l.A)
			{
				return false;
			}
			o o2 = Volatile.Read(ref O.m_A);
			long num3 = ((o2.A > l.A && o2.A == P_4.A && o2.A == P_4.A) ? o2.a : Interlocked.Increment(ref O.m_A));
			Volatile.Write(ref O.m_A, new o
			{
				A = num2,
				a = num3,
				A = P_4.A,
				A = P_4.A
			});
			return true;
		}

		public static void B()
		{
			Volatile.Write(ref O.m_A, new o
			{
				a = Interlocked.Increment(ref O.m_A)
			});
		}
	}
	internal sealed class o
	{
		public long A;

		public long a;

		public int A;

		public DateTime A;
	}
}
