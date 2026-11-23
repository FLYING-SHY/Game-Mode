using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
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
			BackColor = global::B.L.a;
			ForeColor = global::B.L.b;
			base.Height = 30;
			base.Width = 120;
			Font = global::a.J.A(9f);
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
			using (SolidBrush brush2 = new SolidBrush((this.m_A || A) ? global::B.L.B : global::B.L.a))
			{
				graphics.FillPolygon(brush2, global::B.L.A(base.Width, base.Height, 5));
			}
			global::B.L.A(graphics, base.ClientRectangle, (this.m_A || Focused) ? global::B.L.D : global::B.L.c);
			TextRenderer.DrawText(graphics, Text, Font, new Rectangle((base.Width >= 40) ? 8 : 0, 0, (base.Width < 40) ? base.Width : (base.Width - ((this.A == null) ? 16 : 35)), base.Height), base.Enabled ? global::B.L.b : global::B.L.C, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
			if (this.A != null)
			{
				using SolidBrush brush3 = new SolidBrush(this.A());
				graphics.FillRectangle(brush3, base.Width - 24, 8, 13, base.Height - 16);
			}
			if (A)
			{
				using (SolidBrush brush4 = new SolidBrush(global::B.L.D))
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
				global::B.L.A(P_1.Graphics, this.A.ClientRectangle, global::B.L.c);
			}
		}

		private readonly TextBox m_A = new TextBox();

		private readonly global::b.j m_A = new global::b.j();

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

		public d(global::a.N P_0)
		{
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.X();
			base.ClientSize = new Size(690, 470);
			MinimumSize = base.Size;
			MaximumSize = base.Size;
			base.StartPosition = FormStartPosition.CenterScreen;
			base.FormBorderStyle = FormBorderStyle.None;
			BackColor = global::B.L.A;
			ForeColor = global::B.L.b;
			Font = global::a.J.A();
			base.Icon = global::a.J.A;
			global::b.h h2 = new global::b.h
			{
				Dock = DockStyle.Fill
			};
			base.Controls.Add(h2);
			Label label = new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.x(),
				Font = global::a.J.A(23f, FontStyle.Bold),
				ForeColor = global::B.L.b,
				Bounds = new Rectangle(30, 26, 440, 47)
			};
			h2.Controls.Add(label);
			D d2 = new D
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Y(),
				Bounds = new Rectangle(618, 28, 40, 30)
			};
			EventHandler value = delegate
			{
				Close();
			};
			d2.Click += value;
			h2.Controls.Add(d2);
			h2.Controls.Add(new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.y(),
				Font = global::a.J.A(9f),
				ForeColor = global::B.L.C,
				Bounds = new Rectangle(34, 83, 550, 28)
			});
			Panel A = new Panel
			{
				Bounds = new Rectangle(32, 126, 626, 236),
				BackColor = global::B.L.a
			};
			h2.Controls.Add(A);
			A.Paint += delegate(object obj, PaintEventArgs e2)
			{
				global::B.L.A(e2.Graphics, A.ClientRectangle, global::B.L.c);
			};
			A.Controls.Add(new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Z(),
				ForeColor = global::B.L.C,
				Bounds = new Rectangle(22, 19, 560, 25)
			});
			this.m_A.Text = P_0.Code ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			this.m_A.UseSystemPasswordChar = true;
			this.m_A.BackColor = global::B.L.A;
			this.m_A.ForeColor = global::B.L.b;
			this.m_A.Font = global::a.J.A(12f);
			this.m_A.BorderStyle = BorderStyle.FixedSingle;
			this.m_A.SetBounds(22, 52, 580, 33);
			this.m_A.MaxLength = 80;
			A.Controls.Add(this.m_A);
			this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.z();
			this.m_A.A = P_0.Remember;
			this.m_A.SetBounds(20, 99, 220, 30);
			A.Controls.Add(this.m_A);
			this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aA();
			this.m_A.A = true;
			this.m_A.SetBounds(418, 99, 184, 36);
			A.Controls.Add(this.m_A);
			this.m_A.Click += delegate
			{
				this.A();
			};
			this.m_A.SetBounds(22, 151, 580, 70);
			this.m_A.ForeColor = global::B.L.d;
			this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aa();
			A.Controls.Add(this.m_A);
			h2.Controls.Add(new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aB(),
				ForeColor = global::B.L.C,
				Font = global::a.J.A(8f),
				Bounds = new Rectangle(34, 384, 620, 28)
			});
			h2.Controls.Add(new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ab(),
				ForeColor = global::B.L.C,
				Font = global::a.J.A(8f),
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
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aC();
				this.m_A.Focus();
				return;
			}
			Action<string, bool> action = this.m_A;
			if (action != null)
			{
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ac();
				action(this.m_A.Text, this.m_A.A);
			}
		}

		public void A(global::a.R P_0)
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
	internal sealed class D
	{
		public int A;

		public int a;

		public int B;

		public int b;

		public bool A
		{
			get
			{
				if (a <= this.A)
				{
					return b > B;
				}
				return true;
			}
		}
	}
	internal static class d
	{
		public static void A()
		{
			global::B.O[] array = new global::B.O[4]
			{
				new global::B.O
				{
					A = 1u,
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba()
				},
				new global::B.O
				{
					A = 2u,
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl()
				},
				new global::B.O
				{
					A = 3u,
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()
				},
				new global::B.O
				{
					A = 4u,
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()
				}
			};
			global::b.A[] array2 = new global::b.A[4]
			{
				new global::b.A
				{
					A = 1u
				},
				new global::b.A
				{
					A = 3u
				},
				new global::b.A
				{
					A = 3u
				},
				new global::b.A
				{
					A = 4u,
					A = 100f,
					A = new global::B.e
					{
						A = 0f,
						B = 0f
					}
				}
			};
			D d2 = a.A(array2, array, 0f, 0f, 40.0);
			if (d2.A != 1 || d2.a != 1 || d2.b != 2 || !d2.A)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dm());
			}
			if (a.A(new global::b.A[3]
			{
				array2[0],
				array2[1],
				new global::b.A
				{
					A = 2u
				}
			}, array, 0f, 0f, 40.0).A)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DN());
			}
			w w2 = new w();
			w2.A = new float[16]
			{
				1f, 0f, 0f, 0f, 0f, 0.5f, 0.866f, 0f, 0f, -0.866f,
				0.5f, 0f, 0f, -5f, 100f, 1f
			};
			w2.a = new float[16]
			{
				1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f,
				1f, 1f, 0f, 0f, 0f, 0f
			};
			w w3 = w2;
			if (!w3.A(2f, 0f, 3f, 1920, 1080, out var pointF) || !a.A(w3, pointF, 1920, 1080, 0f, out var num, out var num2) || (double)Math.Abs(num - 2f) > 0.001 || (double)Math.Abs(num2 - 3f) > 0.001)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dn());
			}
		}
	}
}
