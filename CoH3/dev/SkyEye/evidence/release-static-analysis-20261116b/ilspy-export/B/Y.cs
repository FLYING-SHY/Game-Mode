using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using A;

namespace B;

internal static class Y
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
				this.A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
			}
		}

		internal void A(A P_0)
		{
			if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dQ())
			{
				this.A = 0;
				a += B;
			}
			else
			{
				if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ya())
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

	public static int A(Graphics P_0, string P_1, Font P_2, Rectangle P_3, Color P_4, g[] P_5, bool P_6, double P_7)
	{
		List<A> A = new List<A>();
		TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(P_1);
		string A2 = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
				A2 = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
			}
		};
		while (textElementEnumerator.MoveNext())
		{
			string textElement = textElementEnumerator.GetTextElement();
			int elementIndex = textElementEnumerator.ElementIndex;
			Color color = P_4;
			foreach (g g2 in P_5)
			{
				if (g2.A.A && elementIndex >= g2.A && elementIndex < g2.A + g2.a)
				{
					color = g2.A.A(g2.A, P_7);
					break;
				}
			}
			bool flag = textElement.Length == 1 && ((textElement[0] >= 'a' && textElement[0] <= 'z') || (textElement[0] >= 'A' && textElement[0] <= 'Z') || (textElement[0] >= '0' && textElement[0] <= '9') || _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KJ().IndexOf(textElement[0]) >= 0);
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
		int B = TextRenderer.MeasureText(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kj(), P_2, new Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Height;
		int A4 = 0;
		int a2 = 0;
		Action<A> action2 = delegate(A a3)
		{
			if (a3.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dQ())
			{
				A4 = 0;
				a2 += B;
			}
			else if (!(a3.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ya()))
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
internal sealed class y : Form
{
	[CompilerGenerated]
	private sealed class A
	{
		public global::A.l A;

		public CheckBox A;

		public y A;

		internal void A(object P_0, EventArgs P_1)
		{
			this.A.A = this.A.Checked;
			this.A.B();
		}
	}

	[CompilerGenerated]
	private sealed class a
	{
		public global::A.L A;

		public Label A;

		public A A;

		internal void A(object P_0, EventArgs P_1)
		{
			using OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YB(),
				Title = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yb()
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
					MessageBox.Show(this.A.A, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YC() + ex.Message, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yc());
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
				MessageBox.Show(this.A.A, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YD(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yc());
			}
		}
	}

	public y(global::A.l P_0)
	{
		y A = this;
		Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KK();
		base.ClientSize = new Size(790, 480);
		Font = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), 10f);
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
			Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.hj(),
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
			Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kk(),
			AutoSize = true,
			Margin = new Padding(0, 8, 0, 8)
		});
		foreach (global::A.L value in Enum.GetValues(typeof(global::A.L)))
		{
			global::A.L A3 = value;
			FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
			{
				Width = 710,
				Height = 52,
				WrapContents = false
			};
			flowLayoutPanel.Controls.Add(flowLayoutPanel2);
			flowLayoutPanel2.Controls.Add(new Label
			{
				Text = ((A3 == global::A.L.D) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Km() : ((A3 == global::A.L.a) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.hN() : ((A3 == global::A.L.B) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.hm() : ((A3 == global::A.L.c) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KM() : ((A3 == global::A.L.C) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kl() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KL()))))),
				Width = 112,
				Height = 38,
				Padding = new Padding(0, 8, 0, 0)
			});
			Button button = new Button
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KN(),
				AutoSize = true
			};
			Button button2 = new Button
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Gq(),
				AutoSize = true
			};
			Button button3 = new Button
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kn(),
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
					Filter = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YB(),
					Title = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yb()
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
						MessageBox.Show(A, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YC() + ex.Message, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yc());
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
					MessageBox.Show(A, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YD(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yc());
				}
			};
		}
	}
}
