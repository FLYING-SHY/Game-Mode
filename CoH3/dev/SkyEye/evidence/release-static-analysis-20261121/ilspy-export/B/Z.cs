using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;

namespace B;

internal static class Z
{
	private sealed class A
	{
		public string A;

		public Color A;

		public int A;
	}

	[CompilerGenerated]
	private sealed class a
	{
		public string A;

		public List<A> A;

		public Color A;

		public Graphics A;

		public Font A;

		public int A;

		public int a;

		public int B;

		public Rectangle A;

		public bool A;

		internal void A()
		{
			if (this.A.Length != 0)
			{
				this.A.Add(new A
				{
					A = this.A,
					A = this.A,
					A = TextRenderer.MeasureText(this.A, this.A, this.A, new Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width
				});
				this.A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
		}

		internal void A(A P_0)
		{
			if (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dt())
			{
				this.A = 0;
				a += B;
			}
			else
			{
				if (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YG())
				{
					return;
				}
				if (this.A > 0 && this.A + P_0.A > this.A.Width)
				{
					this.A = 0;
					a += B;
				}
				if (this.A == 0 && string.IsNullOrWhiteSpace(P_0.A))
				{
					return;
				}
				if (this.A)
				{
					Rectangle rectangle = new Rectangle(this.A.X + this.A, this.A.Y + a, Math.Max(1, P_0.A), B);
					Point[] array = new Point[4]
					{
						new Point(-1, 0),
						new Point(1, 0),
						new Point(0, -1),
						new Point(0, 1)
					};
					foreach (Point pos in array)
					{
						Rectangle bounds = rectangle;
						bounds.Offset(pos);
						TextRenderer.DrawText(this.A, P_0.A, this.A, bounds, Color.Black, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
					}
					TextRenderer.DrawText(this.A, P_0.A, this.A, rectangle, P_0.A, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
				}
				this.A += P_0.A;
			}
		}
	}

	private const TextFormatFlags m_A = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

	public static int A(Graphics P_0, string P_1, Font P_2, Rectangle P_3, Color P_4, h[] P_5, bool P_6, double P_7)
	{
		List<A> A = new List<A>();
		TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(P_1);
		string A2 = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		Color A3 = P_4;
		Action action = delegate
		{
			if (A2.Length != 0)
			{
				A.Add(new A
				{
					A = A2,
					A = A3,
					A = TextRenderer.MeasureText(P_0, A2, P_2, new Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width
				});
				A2 = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
		};
		while (textElementEnumerator.MoveNext())
		{
			string textElement = textElementEnumerator.GetTextElement();
			int elementIndex = textElementEnumerator.ElementIndex;
			Color color = P_4;
			foreach (h h2 in P_5)
			{
				if (h2.A.A && elementIndex >= h2.A && elementIndex < h2.A + h2.a)
				{
					color = h2.A.A(h2.A, P_7);
					break;
				}
			}
			bool flag = textElement.Length == 1 && ((textElement[0] >= 'a' && textElement[0] <= 'z') || (textElement[0] >= 'A' && textElement[0] <= 'Z') || (textElement[0] >= '0' && textElement[0] <= '9') || CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kn().IndexOf(textElement[0]) >= 0);
			if (flag && A2.Length > 0 && color.ToArgb() == A3.ToArgb())
			{
				A2 += textElement;
				continue;
			}
			action();
			A3 = color;
			A2 = textElement;
			if (!flag)
			{
				action();
			}
		}
		action();
		int B = TextRenderer.MeasureText(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KO(), P_2, new Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Height;
		int A4 = 0;
		int a2 = 0;
		Action<A> action2 = delegate(A a3)
		{
			if (a3.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dt())
			{
				A4 = 0;
				a2 += B;
			}
			else if (!(a3.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YG()))
			{
				if (A4 > 0 && A4 + a3.A > P_3.Width)
				{
					A4 = 0;
					a2 += B;
				}
				if (A4 != 0 || !string.IsNullOrWhiteSpace(a3.A))
				{
					if (P_6)
					{
						Rectangle rectangle = new Rectangle(P_3.X + A4, P_3.Y + a2, Math.Max(1, a3.A), B);
						Point[] array = new Point[4]
						{
							new Point(-1, 0),
							new Point(1, 0),
							new Point(0, -1),
							new Point(0, 1)
						};
						foreach (Point pos in array)
						{
							Rectangle bounds = rectangle;
							bounds.Offset(pos);
							TextRenderer.DrawText(P_0, a3.A, P_2, bounds, Color.Black, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
						}
						TextRenderer.DrawText(P_0, a3.A, P_2, rectangle, a3.A, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
					}
					A4 += a3.A;
				}
			}
		};
		foreach (A item in A)
		{
			if (item.A <= P_3.Width)
			{
				action2(item);
				continue;
			}
			TextElementEnumerator textElementEnumerator2 = StringInfo.GetTextElementEnumerator(item.A);
			while (textElementEnumerator2.MoveNext())
			{
				string textElement2 = textElementEnumerator2.GetTextElement();
				action2(new A
				{
					A = textElement2,
					A = item.A,
					A = TextRenderer.MeasureText(P_0, textElement2, P_2, new Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width
				});
			}
		}
		return a2 + B;
	}
}
internal sealed class z : Form
{
	[CompilerGenerated]
	private sealed class A
	{
		public global::A.M A;

		public CheckBox A;

		public z A;

		internal void A(object P_0, EventArgs P_1)
		{
			this.A.A = this.A.Checked;
			this.A.B();
		}
	}

	[CompilerGenerated]
	private sealed class a
	{
		public global::A.l A;

		public Label A;

		public A A;

		internal void A(object P_0, EventArgs P_1)
		{
			using OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yg(),
				Title = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YH()
			};
			if (openFileDialog.ShowDialog(this.A.A) == DialogResult.OK)
			{
				try
				{
					this.A.A.A(this.A, openFileDialog.FileName);
					this.A.Text = Path.GetFileName(openFileDialog.FileName);
					return;
				}
				catch (Exception ex)
				{
					MessageBox.Show(this.A.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yh() + ex.Message, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YI());
					return;
				}
			}
		}

		internal void a(object P_0, EventArgs P_1)
		{
			this.A.A.A(this.A, true);
		}

		internal void B(object P_0, EventArgs P_1)
		{
			try
			{
				this.A.A.A(this.A, null);
				this.A.Text = this.A.A.a(this.A);
			}
			catch (Exception)
			{
				MessageBox.Show(this.A.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yi(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YI());
			}
		}
	}

	public z(global::A.M P_0)
	{
		z A = this;
		Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ko();
		base.ClientSize = new Size(790, 480);
		Font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), 10f);
		base.StartPosition = FormStartPosition.CenterParent;
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(14),
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoScroll = true
		};
		base.Controls.Add(flowLayoutPanel);
		CheckBox A2 = new CheckBox
		{
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hN(),
			Checked = P_0.A,
			AutoSize = true
		};
		flowLayoutPanel.Controls.Add(A2);
		A2.CheckedChanged += delegate
		{
			P_0.A = A2.Checked;
			P_0.B();
		};
		flowLayoutPanel.Controls.Add(new Label
		{
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KP(),
			AutoSize = true,
			Margin = new Padding(0, 8, 0, 8)
		});
		foreach (global::A.l value in Enum.GetValues(typeof(global::A.l)))
		{
			global::A.l A3 = value;
			FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
			{
				Width = 710,
				Height = 52,
				WrapContents = false
			};
			flowLayoutPanel.Controls.Add(flowLayoutPanel2);
			flowLayoutPanel2.Controls.Add(new Label
			{
				Text = ((A3 == global::A.l.D) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KR() : ((A3 == global::A.l.a) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hq() : ((A3 == global::A.l.B) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hQ() : ((A3 == global::A.l.c) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kq() : ((A3 == global::A.l.C) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KQ() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kp()))))),
				Width = 112,
				Height = 38,
				Padding = new Padding(0, 8, 0, 0)
			});
			Button button = new Button
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kr(),
				AutoSize = true
			};
			Button button2 = new Button
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GU(),
				AutoSize = true
			};
			Button button3 = new Button
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KS(),
				AutoSize = true
			};
			Label A4 = new Label
			{
				Text = P_0.a(A3),
				AutoSize = true,
				Padding = new Padding(0, 8, 0, 0)
			};
			flowLayoutPanel2.Controls.Add(button);
			flowLayoutPanel2.Controls.Add(button2);
			flowLayoutPanel2.Controls.Add(button3);
			flowLayoutPanel2.Controls.Add(A4);
			button.Click += delegate
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Filter = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yg(),
					Title = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YH()
				};
				if (openFileDialog.ShowDialog(A) == DialogResult.OK)
				{
					try
					{
						P_0.A(A3, openFileDialog.FileName);
						A4.Text = Path.GetFileName(openFileDialog.FileName);
						return;
					}
					catch (Exception ex)
					{
						MessageBox.Show(A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yh() + ex.Message, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YI());
						return;
					}
				}
			};
			button2.Click += delegate
			{
				P_0.A(A3, true);
			};
			button3.Click += delegate
			{
				try
				{
					P_0.A(A3, null);
					A4.Text = P_0.a(A3);
				}
				catch (Exception)
				{
					MessageBox.Show(A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yi(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YI());
				}
			};
		}
	}
}
