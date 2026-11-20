using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using B;
using a;
using b;

namespace B
{
	internal sealed class C : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public B A;

			public Action A;

			public CheckBox A;

			public Panel A;

			public CheckBox a;

			public NumericUpDown A;

			public NumericUpDown a;

			public CheckBox B;

			public NumericUpDown B;

			public NumericUpDown b;

			internal void A(object P_0, EventArgs P_1)
			{
				this.A.a = this.A.Checked;
				this.A();
				this.A.Invalidate();
			}

			internal void a(object P_0, EventArgs P_1)
			{
				this.A.B = this.a.Checked;
				this.A();
				this.A.Invalidate();
			}

			internal void B(object P_0, EventArgs P_1)
			{
				this.A.A = this.A.Value;
				this.A();
				this.A.Invalidate();
			}

			internal void b(object P_0, EventArgs P_1)
			{
				this.A.C = (int)this.a.Value;
				this.A();
				this.A.Invalidate();
			}

			internal void C(object P_0, EventArgs P_1)
			{
				this.A.b = this.B.Checked;
				this.A();
				this.A.Invalidate();
			}

			internal void c(object P_0, EventArgs P_1)
			{
				this.A.c = (int)this.B.Value;
				this.A();
				this.A.Invalidate();
			}

			internal void D(object P_0, EventArgs P_1)
			{
				this.A.b = (int)this.b.Value;
				this.A();
			}

			internal void A(object P_0, PaintEventArgs P_1)
			{
				global::a.y y2 = new global::a.y
				{
					A = this.A.Width / 2,
					a = this.A.Height / 2,
					B = this.A.Width - 20,
					b = this.A.Height - 20,
					C = this.A.Width,
					c = this.A.Height,
					D = 200f,
					d = 200f
				};
				Z[] array = new Z[2]
				{
					new Z
					{
						A = -50f,
						B = 0f,
						A = Color.FromArgb(255, 59, 48)
					},
					new Z
					{
						A = 50f,
						B = -45f,
						A = true,
						A = Color.FromArgb(160, 100, 255),
						A = new d
						{
							A = -10f,
							B = 45f
						}
					}
				};
				global::A.s[] array2 = new global::A.s[1]
				{
					new global::A.s
					{
						A = -55f,
						B = -45f,
						A = Color.Orange
					}
				};
				global::a.Z.A(P_1.Graphics, array, y2, new B
				{
					a = this.A.a,
					B = this.A.B,
					B = this.A.B,
					A = this.A.A,
					C = this.A.C,
					b = this.A.b,
					c = this.A.c
				}, this.A.Width, this.A.Height, array2);
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public bool A;

			public NumericUpDown A;

			public A A;

			internal void A(object P_0, EventArgs P_1)
			{
				if (this.A)
				{
					this.A.A.B = (int)this.A.Value;
				}
				else
				{
					this.A.A.a = (int)this.A.Value;
				}
				this.A.A();
				this.A.A.Invalidate();
			}
		}

		public C(B P_0, global::b.b P_1, Action P_2)
		{
			Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.el();
			base.ClientSize = new Size(650, 570);
			MinimumSize = new Size(650, 570);
			Font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), 10f);
			base.StartPosition = FormStartPosition.CenterParent;
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(14),
				ColumnCount = 1,
				RowCount = 7
			};
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			base.Controls.Add(tableLayoutPanel);
			tableLayoutPanel.Controls.Add(new Label
			{
				Dock = DockStyle.Fill,
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eM()
			}, 0, 0);
			CheckBox A = new CheckBox
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.em(),
				AutoSize = true,
				Checked = P_0.a
			};
			tableLayoutPanel.Controls.Add(A, 0, 1);
			TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 3,
				RowCount = 2
			};
			for (int num = 0; num < 3; num++)
			{
				tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
			}
			tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 2);
			CheckBox a2 = new CheckBox
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eN(),
				AutoSize = true,
				Checked = P_0.B
			};
			tableLayoutPanel.Controls.Add(a2, 0, 3);
			TableLayoutPanel tableLayoutPanel3 = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 2
			};
			for (int num2 = 0; num2 < 2; num2++)
			{
				tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			}
			tableLayoutPanel.Controls.Add(tableLayoutPanel3, 0, 4);
			tableLayoutPanel3.Controls.Add(new Label
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.en(),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, 0, 0);
			NumericUpDown A2 = new NumericUpDown
			{
				Minimum = 0.5m,
				Maximum = 8m,
				DecimalPlaces = 1,
				Increment = 0.5m,
				Value = P_0.A,
				Width = 100
			};
			tableLayoutPanel3.Controls.Add(A2, 0, 1);
			tableLayoutPanel3.Controls.Add(new Label
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eO(),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, 1, 0);
			NumericUpDown a3 = new NumericUpDown
			{
				Minimum = 2m,
				Maximum = 40m,
				Value = P_0.C,
				Width = 100
			};
			tableLayoutPanel3.Controls.Add(a3, 1, 1);
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				WrapContents = false
			};
			tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 5);
			CheckBox B = new CheckBox
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eo(),
				AutoSize = true,
				Checked = P_0.b,
				Padding = new Padding(0, 4, 0, 0)
			};
			flowLayoutPanel.Controls.Add(B);
			flowLayoutPanel.Controls.Add(new Label
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eP(),
				AutoSize = true,
				Padding = new Padding(12, 7, 0, 0)
			});
			NumericUpDown B2 = new NumericUpDown
			{
				Minimum = 4m,
				Maximum = 60m,
				Value = P_0.c,
				Width = 80
			};
			flowLayoutPanel.Controls.Add(B2);
			Panel A3 = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = Color.FromArgb(38, 45, 43)
			};
			tableLayoutPanel.Controls.Add(A3, 0, 6);
			A.CheckedChanged += delegate
			{
				P_0.a = A.Checked;
				P_2();
				A3.Invalidate();
			};
			a2.CheckedChanged += delegate
			{
				P_0.B = a2.Checked;
				P_2();
				A3.Invalidate();
			};
			A2.ValueChanged += delegate
			{
				P_0.A = A2.Value;
				P_2();
				A3.Invalidate();
			};
			a3.ValueChanged += delegate
			{
				P_0.C = (int)a3.Value;
				P_2();
				A3.Invalidate();
			};
			B.CheckedChanged += delegate
			{
				P_0.b = B.Checked;
				P_2();
				A3.Invalidate();
			};
			B2.ValueChanged += delegate
			{
				P_0.c = (int)B2.Value;
				P_2();
				A3.Invalidate();
			};
			bool[] array = new bool[2] { false, true };
			foreach (bool flag in array)
			{
				bool A4 = flag;
				int column = (A4 ? 1 : 0);
				tableLayoutPanel2.Controls.Add(new Label
				{
					Text = (A4 ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eQ() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ep()),
					Dock = DockStyle.Fill,
					TextAlign = ContentAlignment.MiddleLeft
				}, column, 0);
				NumericUpDown A5 = new NumericUpDown
				{
					Minimum = (A4 ? 3 : 2),
					Maximum = (A4 ? 60 : 40),
					Value = (A4 ? P_0.B : P_0.a),
					Width = 100
				};
				tableLayoutPanel2.Controls.Add(A5, column, 1);
				A5.ValueChanged += delegate
				{
					if (A4)
					{
						P_0.B = (int)A5.Value;
					}
					else
					{
						P_0.a = (int)A5.Value;
					}
					P_2();
					A3.Invalidate();
				};
			}
			tableLayoutPanel2.Controls.Add(new Label
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eq(),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, 2, 0);
			NumericUpDown b2 = new NumericUpDown
			{
				Minimum = 1m,
				Maximum = 60m,
				Value = P_0.b,
				Width = 100
			};
			tableLayoutPanel2.Controls.Add(b2, 2, 1);
			b2.ValueChanged += delegate
			{
				P_0.b = (int)b2.Value;
				P_2();
			};
			A3.Paint += delegate(object obj, PaintEventArgs e2)
			{
				global::a.y y2 = new global::a.y
				{
					A = A3.Width / 2,
					a = A3.Height / 2,
					B = A3.Width - 20,
					b = A3.Height - 20,
					C = A3.Width,
					c = A3.Height,
					D = 200f,
					d = 200f
				};
				Z[] array2 = new Z[2]
				{
					new Z
					{
						A = -50f,
						B = 0f,
						A = Color.FromArgb(255, 59, 48)
					},
					new Z
					{
						A = 50f,
						B = -45f,
						A = true,
						A = Color.FromArgb(160, 100, 255),
						A = new d
						{
							A = -10f,
							B = 45f
						}
					}
				};
				global::A.s[] array3 = new global::A.s[1]
				{
					new global::A.s
					{
						A = -55f,
						B = -45f,
						A = Color.Orange
					}
				};
				global::a.Z.A(e2.Graphics, array2, y2, new B
				{
					a = P_0.a,
					B = P_0.B,
					B = P_0.B,
					A = P_0.A,
					C = P_0.C,
					b = P_0.b,
					c = P_0.c
				}, A3.Width, A3.Height, array3);
			};
		}
	}
	internal sealed class c
	{
		private bool m_A;

		[CompilerGenerated]
		private bool m_a;

		[CompilerGenerated]
		private long m_A;

		[CompilerGenerated]
		private long m_a;

		public bool A
		{
			[CompilerGenerated]
			get
			{
				return this.m_a;
			}
			[CompilerGenerated]
			private set
			{
				this.m_a = flag;
			}
		}

		public long a
		{
			[CompilerGenerated]
			get
			{
				return this.m_A;
			}
			[CompilerGenerated]
			private set
			{
				this.m_A = num;
			}
		}

		public long B
		{
			[CompilerGenerated]
			get
			{
				return this.m_a;
			}
			[CompilerGenerated]
			private set
			{
				this.m_a = num;
			}
		}

		public void b()
		{
			A = false;
			long num = (this.B = 65536L);
			long num3 = num;
			a = num3;
			this.m_A = false;
		}

		public void B(long P_0)
		{
			long num = (a = Math.Max(65536L, P_0));
			long num3 = num;
			this.B = num3;
			A = true;
			this.m_A = false;
		}

		public bool b(long P_0)
		{
			a = Math.Max(65536L, P_0);
			if (this.m_A && a >= this.B)
			{
				a = this.B;
				this.m_A = false;
				return true;
			}
			return false;
		}

		public bool C()
		{
			a = 65536L;
			if (this.B == 65536)
			{
				this.m_A = false;
				return true;
			}
			this.m_A = true;
			return false;
		}
	}
}
namespace b
{
	internal sealed class C : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public b A;

			public Action A;

			public NumericUpDown A;

			public NumericUpDown a;

			public CheckBox A;

			public CheckBox a;

			public Panel A;

			internal void A(object P_0, EventArgs P_1)
			{
				this.A.E = (int)this.A.Value;
				this.A();
			}

			internal void a(object P_0, EventArgs P_1)
			{
				this.A.e = (int)this.a.Value;
				this.A();
			}

			internal void B(object P_0, EventArgs P_1)
			{
				this.A.b = this.A.Checked;
				this.A();
			}

			internal void b(object P_0, EventArgs P_1)
			{
				this.A.B = this.a.Checked;
				this.A();
				this.A.Invalidate();
			}

			internal void A(object P_0, PaintEventArgs P_1)
			{
				using Font font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), this.A.d, FontStyle.Bold);
				global::B.z.A(P_1.Graphics, new PointF((float)this.A.Width * 0.28f, (float)this.A.Height * 0.62f), false, j.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.sd()), Color.FromArgb(235, 65, 65), this.A, font);
				global::B.z.A(P_1.Graphics, new PointF((float)this.A.Width * 0.73f, (float)this.A.Height * 0.62f), true, j.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.TE()), Color.FromArgb(160, 100, 255), this.A, font);
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public int A;

			public NumericUpDown A;

			public A A;

			internal void A(object P_0, EventArgs P_1)
			{
				if (this.A == 0)
				{
					this.A.A.c = (int)this.A.Value;
				}
				else if (this.A == 1)
				{
					this.A.A.D = (int)this.A.Value;
				}
				else if (this.A == 2)
				{
					this.A.A.C = this.A.Value;
				}
				else
				{
					this.A.A.d = (int)this.A.Value;
				}
				this.A.A();
				this.A.A.Invalidate();
			}
		}

		public C(b P_0, Action P_1, Func<string> P_2 = null)
		{
			Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ko();
			base.ClientSize = new Size(720, 565);
			MinimumSize = new Size(720, 565);
			Font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), 10f);
			base.StartPosition = FormStartPosition.CenterParent;
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(14),
				ColumnCount = 1,
				RowCount = 6
			};
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			base.Controls.Add(tableLayoutPanel);
			tableLayoutPanel.Controls.Add(new Label
			{
				Dock = DockStyle.Fill,
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kP()
			}, 0, 0);
			CheckBox a2 = new CheckBox
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kp(),
				Checked = P_0.B,
				AutoSize = true
			};
			tableLayoutPanel.Controls.Add(a2, 0, 1);
			TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 4,
				RowCount = 2
			};
			int[] array = new int[4] { 120, 150, 150, 170 };
			foreach (int num in array)
			{
				tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, num));
			}
			tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 2);
			string[] array2 = new string[4]
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kQ(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kq(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hk(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HL()
			};
			decimal[] array3 = new decimal[4] { P_0.c, P_0.D, P_0.C, P_0.d };
			Panel A = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = Color.FromArgb(38, 45, 43)
			};
			tableLayoutPanel.Controls.Add(A, 0, 5);
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill
			};
			tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 3);
			flowLayoutPanel.Controls.Add(new Label
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kR(),
				AutoSize = true,
				Padding = new Padding(0, 5, 0, 0)
			});
			NumericUpDown A2 = new NumericUpDown
			{
				Minimum = 10m,
				Maximum = 144m,
				Value = P_0.E,
				Width = 62
			};
			flowLayoutPanel.Controls.Add(A2);
			A2.ValueChanged += delegate
			{
				P_0.E = (int)A2.Value;
				P_1();
			};
			flowLayoutPanel.Controls.Add(new Label
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kr(),
				AutoSize = true,
				Padding = new Padding(10, 5, 0, 0)
			});
			NumericUpDown a3 = new NumericUpDown
			{
				Minimum = 5m,
				Maximum = 144m,
				Value = P_0.e,
				Width = 62
			};
			flowLayoutPanel.Controls.Add(a3);
			a3.ValueChanged += delegate
			{
				P_0.e = (int)a3.Value;
				P_1();
			};
			FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.TopDown,
				WrapContents = false
			};
			tableLayoutPanel.Controls.Add(flowLayoutPanel2, 0, 4);
			CheckBox A3 = new CheckBox
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kS(),
				Checked = P_0.b,
				AutoSize = true
			};
			flowLayoutPanel2.Controls.Add(A3);
			A3.CheckedChanged += delegate
			{
				P_0.b = A3.Checked;
				P_1();
			};
			flowLayoutPanel2.Controls.Add(new Label
			{
				AutoSize = true,
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ks()
			});
			flowLayoutPanel2.Controls.Add(new Label
			{
				AutoSize = true,
				MaximumSize = new Size(610, 0),
				Text = ((P_2 == null) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kT() : P_2())
			});
			a2.CheckedChanged += delegate
			{
				P_0.B = a2.Checked;
				P_1();
				A.Invalidate();
			};
			for (int num2 = 0; num2 < 4; num2++)
			{
				int A4 = num2;
				tableLayoutPanel2.Controls.Add(new Label
				{
					Text = array2[num2],
					Dock = DockStyle.Fill,
					TextAlign = ContentAlignment.MiddleLeft
				}, num2, 0);
				NumericUpDown numericUpDown = new NumericUpDown
				{
					Minimum = ((num2 == 2) ? 0.5m : 8m)
				};
				NumericUpDown numericUpDown2 = numericUpDown;
				numericUpDown2.Maximum = num2 switch
				{
					3 => 36, 
					2 => 12, 
					_ => 200, 
				};
				numericUpDown.DecimalPlaces = ((num2 == 2) ? 1 : 0);
				numericUpDown.Increment = ((num2 == 2) ? 0.5m : 1m);
				numericUpDown.Value = array3[num2];
				numericUpDown.Width = 95;
				NumericUpDown A5 = numericUpDown;
				tableLayoutPanel2.Controls.Add(A5, num2, 1);
				A5.ValueChanged += delegate
				{
					if (A4 == 0)
					{
						P_0.c = (int)A5.Value;
					}
					else if (A4 == 1)
					{
						P_0.D = (int)A5.Value;
					}
					else if (A4 == 2)
					{
						P_0.C = A5.Value;
					}
					else
					{
						P_0.d = (int)A5.Value;
					}
					P_1();
					A.Invalidate();
				};
			}
			A.Paint += delegate(object obj, PaintEventArgs e2)
			{
				using Font font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), P_0.d, FontStyle.Bold);
				global::B.z.A(e2.Graphics, new PointF((float)A.Width * 0.28f, (float)A.Height * 0.62f), false, j.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.sd()), Color.FromArgb(235, 65, 65), P_0, font);
				global::B.z.A(e2.Graphics, new PointF((float)A.Width * 0.73f, (float)A.Height * 0.62f), true, j.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.TE()), Color.FromArgb(160, 100, 255), P_0, font);
			};
		}
	}
	internal sealed class c
	{
		public long A;

		public long a;

		public string A;

		public uint A;

		public bool A;

		public Color A;

		public readonly List<B> A = new List<B>();
	}
}
