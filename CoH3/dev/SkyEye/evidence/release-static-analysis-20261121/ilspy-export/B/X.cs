using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;

namespace B;

internal static class X
{
	private static readonly Stopwatch m_A = Stopwatch.StartNew();

	public static double A => X.m_A.Elapsed.TotalSeconds;
}
internal sealed class x : Form
{
	[CompilerGenerated]
	private sealed class A
	{
		public Action A;

		public J A;

		public x A;

		public Panel A;

		internal void A(object P_0, PaintEventArgs P_1)
		{
			string text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yd();
			List<h> list = new List<h>();
			foreach (KeyValuePair<string, Y> item in this.A.A.A())
			{
				Y value = item.Value;
				string text2 = ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fo()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YF() : ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FP()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ye() : ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fp()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YE() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HC())));
				text = text + text2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba();
				string text3 = (value.a + 1m).ToString(CultureInfo.InvariantCulture);
				list.Add(new h
				{
					A = text.Length,
					a = text3.Length,
					A = (double)value.a + 1.0,
					A = value
				});
				text = text + text3 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yf();
			}
			Z.A(P_1.Graphics, text, this.A.Font, new Rectangle(8, 8, Math.Max(20, this.A.Width - 16), this.A.Height - 16), Color.White, list.ToArray(), true, X.A);
		}

		internal void A(object P_0, EventArgs P_1)
		{
			this.A.Invalidate();
		}
	}

	[CompilerGenerated]
	private sealed class a
	{
		public Y A;

		public CheckBox A;

		public A A;

		internal void A(object P_0, EventArgs P_1)
		{
			this.A.A = this.A.Checked;
			this.A.A();
		}

		internal void A(Color P_0)
		{
			this.A.A = P_0;
			this.A.A();
		}

		internal void A(decimal P_0)
		{
			this.A.A = P_0;
			this.A.A();
		}

		internal void a(Color P_0)
		{
			this.A.a = P_0;
			this.A.A();
		}

		internal void a(decimal P_0)
		{
			this.A.a = P_0;
			this.A.A();
		}

		internal void B(Color P_0)
		{
			this.A.B = P_0;
			this.A.A();
		}

		internal void B(decimal P_0)
		{
			this.A.B = P_0;
			this.A.A();
		}
	}

	[CompilerGenerated]
	private sealed class B
	{
		public Button A;

		public x A;

		public Action<Color> A;

		internal void A(object P_0, EventArgs P_1)
		{
			using ColorDialog colorDialog = new ColorDialog
			{
				Color = this.A.BackColor,
				FullOpen = true
			};
			if (colorDialog.ShowDialog(this.A) == DialogResult.OK)
			{
				this.A.BackColor = colorDialog.Color;
				this.A(colorDialog.Color);
			}
		}
	}

	[CompilerGenerated]
	private sealed class b
	{
		public Action<decimal> A;

		public NumericUpDown A;

		internal void A(object P_0, EventArgs P_1)
		{
			this.A(this.A.Value);
		}
	}

	private readonly Timer m_A = new Timer();

	public x(J P_0, Action P_1)
	{
		x A = this;
		Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KK();
		base.ClientSize = new Size(1020, 460);
		MinimumSize = new Size(1020, 460);
		Font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), 10f);
		base.StartPosition = FormStartPosition.CenterParent;
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(14),
			ColumnCount = 1,
			RowCount = 3
		};
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 86f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 235f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		base.Controls.Add(tableLayoutPanel);
		tableLayoutPanel.Controls.Add(new Label
		{
			Dock = DockStyle.Fill,
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kk()
		}, 0, 0);
		TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 8,
			RowCount = 5
		};
		tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 1);
		int[] array = new int[8] { 55, 95, 115, 125, 115, 125, 115, 130 };
		foreach (int num2 in array)
		{
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, num2));
		}
		string[] array2 = new string[8]
		{
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KL(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kl(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hj(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HL(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HK(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hl(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hk(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KM()
		};
		foreach (string text in array2)
		{
			tableLayoutPanel2.Controls.Add(new Label
			{
				Text = text,
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, tableLayoutPanel2.Controls.Count, 0);
		}
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 43f));
		int num3 = 1;
		foreach (KeyValuePair<string, Y> item in P_0.A.A())
		{
			Y A2 = item.Value;
			int num4 = num3++;
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
			CheckBox A3 = new CheckBox
			{
				Checked = A2.A,
				Dock = DockStyle.Fill
			};
			tableLayoutPanel2.Controls.Add(A3, 0, num4);
			A3.CheckedChanged += delegate
			{
				A2.A = A3.Checked;
				P_1();
			};
			string text2 = ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fo()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ha() : ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FP()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HB() : ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fp()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hb() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HC())));
			tableLayoutPanel2.Controls.Add(new Label
			{
				Text = text2,
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, 1, num4);
			this.A(tableLayoutPanel2, 2, num4, A2.A, delegate(Color color)
			{
				A2.A = color;
				P_1();
			});
			x.A(tableLayoutPanel2, 3, num4, 1m, 1000000m, A2.A, 0, delegate(decimal num5)
			{
				A2.A = num5;
				P_1();
			});
			this.A(tableLayoutPanel2, 4, num4, A2.a, delegate(Color color)
			{
				A2.a = color;
				P_1();
			});
			x.A(tableLayoutPanel2, 5, num4, 0m, 1000000m, A2.a, 0, delegate(decimal num5)
			{
				A2.a = num5;
				P_1();
			});
			this.A(tableLayoutPanel2, 6, num4, A2.B, delegate(Color color)
			{
				A2.B = color;
				P_1();
			});
			x.A(tableLayoutPanel2, 7, num4, 0.1m, 5m, A2.B, 1, delegate(decimal num5)
			{
				A2.B = num5;
				P_1();
			});
		}
		Panel A4 = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(38, 45, 43)
		};
		tableLayoutPanel.Controls.Add(A4, 0, 2);
		A4.Paint += delegate(object obj, PaintEventArgs e2)
		{
			string text3 = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yd();
			List<h> list = new List<h>();
			foreach (KeyValuePair<string, Y> item2 in P_0.A.A())
			{
				Y value = item2.Value;
				string text4 = ((item2.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fo()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YF() : ((item2.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FP()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ye() : ((item2.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fp()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YE() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HC())));
				text3 = text3 + text4 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba();
				string text5 = (value.a + 1m).ToString(CultureInfo.InvariantCulture);
				list.Add(new h
				{
					A = text3.Length,
					a = text5.Length,
					A = (double)value.a + 1.0,
					A = value
				});
				text3 = text3 + text5 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yf();
			}
			Z.A(e2.Graphics, text3, A.Font, new Rectangle(8, 8, Math.Max(20, A4.Width - 16), A4.Height - 16), Color.White, list.ToArray(), true, X.A);
		};
		this.m_A.Interval = 50;
		this.m_A.Tick += delegate
		{
			A4.Invalidate();
		};
		this.m_A.Start();
	}

	private void A(TableLayoutPanel P_0, int P_1, int P_2, Color P_3, Action<Color> P_4)
	{
		Button A = new Button
		{
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Km(),
			BackColor = P_3,
			Dock = DockStyle.Fill
		};
		P_0.Controls.Add(A, P_1, P_2);
		A.Click += delegate
		{
			using ColorDialog colorDialog = new ColorDialog
			{
				Color = A.BackColor,
				FullOpen = true
			};
			if (colorDialog.ShowDialog(this) == DialogResult.OK)
			{
				A.BackColor = colorDialog.Color;
				P_4(colorDialog.Color);
			}
		};
	}

	private static void A(TableLayoutPanel P_0, int P_1, int P_2, decimal P_3, decimal P_4, decimal P_5, int P_6, Action<decimal> P_7)
	{
		NumericUpDown A = new NumericUpDown
		{
			Minimum = P_3,
			Maximum = P_4,
			Value = P_5,
			DecimalPlaces = P_6,
			Increment = ((P_6 == 0) ? 1m : 0.1m),
			Dock = DockStyle.Fill,
			ThousandsSeparator = true
		};
		P_0.Controls.Add(A, P_1, P_2);
		A.ValueChanged += delegate
		{
			P_7(A.Value);
		};
	}

	protected override void Dispose(bool P_0)
	{
		if (P_0)
		{
			this.m_A.Dispose();
		}
		base.Dispose(P_0);
	}
}
