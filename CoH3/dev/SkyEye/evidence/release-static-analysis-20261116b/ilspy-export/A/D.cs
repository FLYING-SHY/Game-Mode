using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using A;
using B;
using a;
using b;

namespace A
{
	internal sealed class D : Control
	{
		public Func<Color> A;

		private bool m_A;

		[CompilerGenerated]
		private bool m_a;

		public bool A
		{
			[CompilerGenerated]
			get
			{
				return this.m_a;
			}
			[CompilerGenerated]
			set
			{
				this.m_a = flag;
			}
		}

		public void a()
		{
			if (base.Enabled)
			{
				OnClick(EventArgs.Empty);
			}
		}

		protected override void OnKeyDown(KeyEventArgs P_0)
		{
			if (P_0.KeyCode == Keys.Space || P_0.KeyCode == Keys.Return)
			{
				a();
				P_0.Handled = true;
				P_0.SuppressKeyPress = true;
			}
			base.OnKeyDown(P_0);
		}

		public D()
		{
			base.TabStop = true;
			base.AccessibleRole = AccessibleRole.PushButton;
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			BackColor = global::B.K.a;
			ForeColor = global::B.K.b;
			base.Height = 30;
			base.Width = 120;
			Font = global::a.I.A(9f);
			base.Margin = new Padding(0, 2, 8, 4);
			Cursor = Cursors.Hand;
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
		}

		protected override void OnMouseEnter(EventArgs P_0)
		{
			this.m_A = true;
			Invalidate();
			base.OnMouseEnter(P_0);
		}

		protected override void OnMouseLeave(EventArgs P_0)
		{
			this.m_A = false;
			Invalidate();
			base.OnMouseLeave(P_0);
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			Graphics graphics = P_0.Graphics;
			using (SolidBrush brush = new SolidBrush(BackColor))
			{
				graphics.FillRectangle(brush, base.ClientRectangle);
			}
			using (SolidBrush brush2 = new SolidBrush((this.m_A || A) ? global::B.K.B : global::B.K.a))
			{
				graphics.FillPolygon(brush2, global::B.K.A(base.Width, base.Height, 5));
			}
			global::B.K.A(graphics, base.ClientRectangle, (this.m_A || Focused) ? global::B.K.D : global::B.K.c);
			TextRenderer.DrawText(graphics, Text, Font, new Rectangle((base.Width >= 40) ? 8 : 0, 0, (base.Width < 40) ? base.Width : (base.Width - ((this.A == null) ? 16 : 35)), base.Height), base.Enabled ? global::B.K.b : global::B.K.C, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
			if (this.A != null)
			{
				using SolidBrush brush3 = new SolidBrush(this.A());
				graphics.FillRectangle(brush3, base.Width - 24, 8, 13, base.Height - 16);
			}
			if (A)
			{
				using (SolidBrush brush4 = new SolidBrush(global::B.K.D))
				{
					graphics.FillRectangle(brush4, 0, 7, 2, base.Height - 14);
				}
			}
		}
	}
	internal sealed class d : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public Panel A;

			internal void A(object P_0, PaintEventArgs P_1)
			{
				global::B.K.A(P_1.Graphics, this.A.ClientRectangle, global::B.K.c);
			}
		}

		private readonly TextBox m_A = new TextBox();

		private readonly global::b.i m_A = new global::b.i();

		private readonly Label m_A = new Label();

		private readonly D m_A = new D();

		[CompilerGenerated]
		private Action<string, bool> m_A;

		[SpecialName]
		[CompilerGenerated]
		public void A(Action<string, bool> P_0)
		{
			Action<string, bool> action = this.m_A;
			Action<string, bool> action2;
			do
			{
				action2 = action;
				Action<string, bool> value = (Action<string, bool>)Delegate.Combine(action2, P_0);
				action = Interlocked.CompareExchange(ref this.m_A, value, action2);
			}
			while ((object)action != action2);
		}

		[SpecialName]
		[CompilerGenerated]
		public void a(Action<string, bool> P_0)
		{
			Action<string, bool> action = this.m_A;
			Action<string, bool> action2;
			do
			{
				action2 = action;
				Action<string, bool> value = (Action<string, bool>)Delegate.Remove(action2, P_0);
				action = Interlocked.CompareExchange(ref this.m_A, value, action2);
			}
			while ((object)action != action2);
		}

		public d(global::a.M P_0)
		{
			Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.X();
			base.ClientSize = new Size(690, 470);
			MinimumSize = base.Size;
			MaximumSize = base.Size;
			base.StartPosition = FormStartPosition.CenterScreen;
			base.FormBorderStyle = FormBorderStyle.None;
			BackColor = global::B.K.A;
			ForeColor = global::B.K.b;
			Font = global::a.I.A();
			base.Icon = global::a.I.A;
			global::b.g g2 = new global::b.g
			{
				Dock = DockStyle.Fill
			};
			base.Controls.Add(g2);
			Label label = new Label
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.x(),
				Font = global::a.I.A(23f, FontStyle.Bold),
				ForeColor = global::B.K.b,
				Bounds = new Rectangle(30, 26, 440, 47)
			};
			g2.Controls.Add(label);
			D d2 = new D
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Y(),
				Bounds = new Rectangle(618, 28, 40, 30)
			};
			EventHandler value = delegate
			{
				Close();
			};
			d2.Click += value;
			g2.Controls.Add(d2);
			g2.Controls.Add(new Label
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.y(),
				Font = global::a.I.A(9f),
				ForeColor = global::B.K.C,
				Bounds = new Rectangle(34, 83, 550, 28)
			});
			Panel A = new Panel
			{
				Bounds = new Rectangle(32, 126, 626, 236),
				BackColor = global::B.K.a
			};
			g2.Controls.Add(A);
			A.Paint += delegate(object obj, PaintEventArgs e2)
			{
				global::B.K.A(e2.Graphics, A.ClientRectangle, global::B.K.c);
			};
			A.Controls.Add(new Label
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Z(),
				ForeColor = global::B.K.C,
				Bounds = new Rectangle(22, 19, 560, 25)
			});
			this.m_A.Text = P_0.Code ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
			this.m_A.UseSystemPasswordChar = true;
			this.m_A.BackColor = global::B.K.A;
			this.m_A.ForeColor = global::B.K.b;
			this.m_A.Font = global::a.I.A(12f);
			this.m_A.BorderStyle = BorderStyle.FixedSingle;
			this.m_A.SetBounds(22, 52, 580, 33);
			this.m_A.MaxLength = 80;
			A.Controls.Add(this.m_A);
			this.m_A.Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.z();
			this.m_A.A = P_0.Remember;
			this.m_A.SetBounds(20, 99, 220, 30);
			A.Controls.Add(this.m_A);
			this.m_A.Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aA();
			this.m_A.A = true;
			this.m_A.SetBounds(418, 99, 184, 36);
			A.Controls.Add(this.m_A);
			this.m_A.Click += delegate
			{
				this.A();
			};
			this.m_A.SetBounds(22, 151, 580, 70);
			this.m_A.ForeColor = global::B.K.d;
			this.m_A.Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aa();
			A.Controls.Add(this.m_A);
			g2.Controls.Add(new Label
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aB(),
				ForeColor = global::B.K.C,
				Font = global::a.I.A(8f),
				Bounds = new Rectangle(34, 384, 620, 28)
			});
			g2.Controls.Add(new Label
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ab(),
				ForeColor = global::B.K.C,
				Font = global::a.I.A(8f),
				Bounds = new Rectangle(34, 414, 620, 36)
			});
			this.m_A.KeyDown += delegate(object obj, KeyEventArgs e2)
			{
				if (e2.KeyCode == Keys.Return)
				{
					this.A();
					e2.SuppressKeyPress = true;
				}
			};
			MouseEventHandler value2 = delegate(object obj, MouseEventArgs e2)
			{
				if (e2.Button == MouseButtons.Left)
				{
					ReleaseCapture60();
					d.A(base.Handle, 161, new IntPtr(2), IntPtr.Zero);
				}
			};
			label.MouseDown += value2;
		}

		[DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
		private static extern bool ReleaseCapture60();

		[DllImport("user32.dll", EntryPoint = "SendMessageW")]
		private static extern IntPtr A(IntPtr P_0, int P_1, IntPtr P_2, IntPtr P_3);

		private void A()
		{
			if (string.IsNullOrWhiteSpace(this.m_A.Text))
			{
				this.m_A.Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aC();
				this.m_A.Focus();
				return;
			}
			Action<string, bool> action = this.m_A;
			if (action != null)
			{
				this.m_A.Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ac();
				action(this.m_A.Text, this.m_A.A);
			}
		}

		public void A(global::a.Q P_0)
		{
			if (!P_0.A && !string.IsNullOrEmpty(P_0.a))
			{
				this.m_A.Text = P_0.a;
			}
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			Close();
		}

		[CompilerGenerated]
		private void a(object P_0, EventArgs P_1)
		{
			A();
		}

		[CompilerGenerated]
		private void A(object P_0, KeyEventArgs P_1)
		{
			if (P_1.KeyCode == Keys.Return)
			{
				A();
				P_1.SuppressKeyPress = true;
			}
		}

		[CompilerGenerated]
		private void A(object P_0, MouseEventArgs P_1)
		{
			if (P_1.Button == MouseButtons.Left)
			{
				ReleaseCapture60();
				A(base.Handle, 161, new IntPtr(2), IntPtr.Zero);
			}
		}
	}
}
namespace a
{
	internal static class D
	{
		public static void A()
		{
			global::B.N[] array = new global::B.N[4]
			{
				new global::B.N
				{
					A = 1u,
					a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA()
				},
				new global::B.N
				{
					A = 2u,
					a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL()
				},
				new global::B.N
				{
					A = 3u,
					a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()
				},
				new global::B.N
				{
					A = 4u,
					a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()
				}
			};
			global::B.Z[] array2 = new global::B.Z[4]
			{
				new global::B.Z
				{
					A = 1u
				},
				new global::B.Z
				{
					A = 3u
				},
				new global::B.Z
				{
					A = 3u
				},
				new global::B.Z
				{
					A = 4u,
					A = 100f,
					A = new global::B.d
					{
						A = 0f,
						B = 0f
					}
				}
			};
			c c2 = global::a.A.A(array2, array, 0f, 0f, 40.0);
			if (c2.A != 1 || c2.a != 1 || c2.b != 2 || !c2.A)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DM());
			}
			if (global::a.A.A(new global::B.Z[3]
			{
				array2[0],
				array2[1],
				new global::B.Z
				{
					A = 2u
				}
			}, array, 0f, 0f, 40.0).A)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dm());
			}
			v v2 = new v();
			v2.A = new float[16]
			{
				1f, 0f, 0f, 0f, 0f, 0.5f, 0.866f, 0f, 0f, -0.866f,
				0.5f, 0f, 0f, -5f, 100f, 1f
			};
			v2.a = new float[16]
			{
				1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f,
				1f, 1f, 0f, 0f, 0f, 0f
			};
			v v3 = v2;
			if (!v3.A(2f, 0f, 3f, 1920, 1080, out var pointF) || !global::a.A.A(v3, pointF, 1920, 1080, 0f, out var num, out var num2) || (double)Math.Abs(num - 2f) > 0.001 || (double)Math.Abs(num2 - 3f) > 0.001)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DN());
			}
		}
	}
	internal sealed class d : FlowLayoutPanel
	{
		public readonly FlowLayoutPanel A;

		public readonly global::A.D A;

		public bool A;

		private readonly string m_A;

		public bool A
		{
			get
			{
				return this.A.Visible;
			}
			set
			{
				this.A.Visible = flag;
				this.A.Text = (this.A ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D() : (this.m_A + (flag ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DO() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dn())));
				PerformLayout();
			}
		}

		public void A(int P_0)
		{
			MinimumSize = new Size(P_0, 0);
			MaximumSize = new Size(P_0, 0);
			this.A.Width = P_0;
			this.A.MinimumSize = new Size(P_0, 0);
			this.A.MaximumSize = new Size(P_0, 0);
			foreach (Control control in this.A.Controls)
			{
				if (control.Tag as string == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Do())
				{
					if (control is d d2)
					{
						d2.A(P_0);
					}
					else
					{
						control.Width = P_0;
					}
				}
			}
			PerformLayout();
		}

		public d(string P_0)
		{
			this.m_A = P_0;
			base.Tag = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Do();
			base.Width = 390;
			base.FlowDirection = FlowDirection.TopDown;
			base.WrapContents = false;
			AutoSize = true;
			AutoSizeMode = AutoSizeMode.GrowAndShrink;
			base.Margin = new Padding(0, 3, 0, 3);
			this.A = new global::A.D
			{
				Name = P_0,
				Text = P_0 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dn(),
				Width = 390,
				Margin = Padding.Empty
			};
			this.A = new FlowLayoutPanel
			{
				Visible = false,
				Width = 390,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				WrapContents = true,
				Margin = new Padding(0, 6, 0, 4)
			};
			base.Controls.Add(this.A);
			base.Controls.Add(this.A);
			global::A.D obj = this.A;
			EventHandler value = delegate
			{
				this.A = !this.A;
			};
			obj.Click += value;
			base.SizeChanged += delegate
			{
				this.A.Width = base.Width;
				this.A.MinimumSize = new Size(base.Width, 0);
				this.A.MaximumSize = new Size(base.Width, 0);
			};
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			this.A = !this.A;
		}

		[CompilerGenerated]
		private void a(object P_0, EventArgs P_1)
		{
			this.A.Width = base.Width;
			this.A.MinimumSize = new Size(base.Width, 0);
			this.A.MaximumSize = new Size(base.Width, 0);
		}
	}
}
