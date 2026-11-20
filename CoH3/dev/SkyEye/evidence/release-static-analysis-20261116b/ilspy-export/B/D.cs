using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using A;
using B;
using a;

namespace B
{
	internal sealed class D : Panel
	{
		public readonly FlowLayoutPanel A;

		private readonly string m_A;

		private readonly string a;

		private bool m_A;

		public D(string P_0, string P_1)
		{
			this.m_A = P_1;
			a = P_0;
			BackColor = K.a;
			base.Width = 420;
			base.Margin = new Padding(0, 0, 0, 12);
			DoubleBuffered = true;
			this.A = new FlowLayoutPanel
			{
				Location = new Point(15, 48),
				Width = 390,
				Height = 1,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				WrapContents = true,
				FlowDirection = FlowDirection.LeftToRight,
				BackColor = K.a,
				Margin = Padding.Empty
			};
			base.Controls.Add(this.A);
			FlowLayoutPanel flowLayoutPanel = this.A;
			LayoutEventHandler value = delegate
			{
				A();
			};
			flowLayoutPanel.Layout += value;
			base.SizeChanged += delegate
			{
				A();
			};
		}

		public void A()
		{
			if (this.m_A)
			{
				return;
			}
			this.m_A = true;
			try
			{
				this.A.MaximumSize = new Size(Math.Max(280, base.Width - 30), 0);
				this.A.MinimumSize = new Size(Math.Max(280, base.Width - 30), 0);
				foreach (Control control in this.A.Controls)
				{
					if (control.Tag as string == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Do())
					{
						if (control is global::a.d d2)
						{
							d2.A(this.A.MinimumSize.Width);
						}
						else
						{
							control.Width = this.A.MinimumSize.Width;
						}
					}
				}
				this.A.PerformLayout();
				base.Height = this.A.Height + 63;
			}
			finally
			{
				this.m_A = false;
			}
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			base.OnPaint(P_0);
			Graphics graphics = P_0.Graphics;
			K.A(graphics, base.ClientRectangle, K.c);
			using (Pen pen = new Pen(K.c))
			{
				graphics.DrawLine(pen, 1, 38, base.Width - 3, 38);
				graphics.DrawLine(pen, 53, 10, 53, 29);
			}
			using (Font font = global::a.I.A(9f))
			{
				TextRenderer.DrawText(graphics, a, font, new Point(16, 12), K.C, TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
			}
			using (Font font2 = global::a.I.A(11f, FontStyle.Bold))
			{
				TextRenderer.DrawText(graphics, this.m_A, font2, new Point(67, 9), K.b, TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
			}
			using (Pen pen2 = new Pen(K.D))
			{
				graphics.DrawLine(pen2, base.Width - 42, 15, base.Width - 26, 15);
				graphics.DrawLine(pen2, base.Width - 42, 19, base.Width - 32, 19);
			}
			using Pen pen3 = new Pen(Color.FromArgb(28, 50, 66));
			graphics.DrawLine(pen3, 10, base.Height - 5, base.Width - 17, base.Height - 5);
		}

		[CompilerGenerated]
		private void A(object P_0, LayoutEventArgs P_1)
		{
			A();
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			A();
		}
	}
	internal sealed class d
	{
		public float A;

		public float a;

		public float B;
	}
}
namespace b
{
	internal sealed class D : Form
	{
		private struct A
		{
			public int A;

			public int a;

			public int B;

			public int b;
		}

		[CompilerGenerated]
		private sealed class a
		{
			public b A;

			public D A;

			public A A;

			internal bool A(global::B.S P_0)
			{
				if (P_0.A != null && !(P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()) && (!(P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA()) || !this.A.A.A))
				{
					if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL())
					{
						return this.A.A.a;
					}
					return false;
				}
				return true;
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public List<Rectangle> A;

			public List<global::a.t> A;

			public a A;

			internal void A(global::B.Z P_0, PointF P_1)
			{
				Bitmap bitmap = this.A.A.m_a.a(P_0);
				Rectangle item = global::B.e.A(bitmap.Size, P_1, this.A.A.B, this.A.A.b, this.A);
				this.A.Add(item);
				this.A.Add(new global::a.t
				{
					A = bitmap,
					A = item
				});
			}
		}

		internal const int A = 33494;

		private volatile b m_A;

		private volatile global::a.u m_A;

		private readonly bool m_A;

		private global::B.n m_A;

		private global::B.m m_A;

		private string m_A;

		private global::a.u m_A;

		private List<global::B.Z> m_A = new List<global::B.Z>();

		private List<global::a.t> m_A = new List<global::a.t>();

		private long m_A;

		private double m_A;

		private List<global::A.k> m_A = new List<global::A.k>();

		private readonly global::a.V m_A = new global::a.V();

		private string m_a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		private List<global::A.Y> m_A = new List<global::A.Y>();

		private List<global::B.q> m_A = new List<global::B.q>();

		private readonly global::B.u m_A = new global::B.u();

		private double m_a;

		private string m_B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		private readonly global::a.V m_a = new global::a.V();

		private readonly Font m_A = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), 10f, FontStyle.Bold);

		private readonly Stopwatch m_A = Stopwatch.StartNew();

		private double m_B;

		private double m_b;

		private double m_C;

		private double m_c;

		private double m_D;

		private double m_d;

		private double m_E;

		private double m_e;

		private int m_a;

		private int m_B;

		private int m_b;

		private int m_C;

		private readonly List<double> m_A = new List<double>();

		private bool m_a;

		private string m_b = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kv();

		public volatile string A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		public volatile string a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kW();

		public long a;

		private int m_c;

		public long A => Interlocked.Read(ref this.a);

		protected override bool a => true;

		protected override CreateParams B
		{
			get
			{
				CreateParams obj = base.CreateParams;
				obj.ExStyle |= 134742176;
				return obj;
			}
		}

		public int b => CreateParams.ExStyle;

		[DllImport("user32.dll", EntryPoint = "GetClientRect")]
		private static extern bool A(IntPtr P_0, out A P_1);

		[DllImport("user32.dll", EntryPoint = "ClientToScreen")]
		private static extern bool A(IntPtr P_0, ref Point P_1);

		[DllImport("user32.dll", EntryPoint = "IsIconic")]
		private static extern bool A(IntPtr P_0);

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		private static extern bool A(IntPtr P_0, IntPtr P_1, int P_2, int P_3, int P_4, int P_5, uint P_6);

		public void B()
		{
			Interlocked.Exchange(ref this.m_A, DateTime.UtcNow.AddSeconds(8.0).Ticks);
			if (this.m_A != null)
			{
				this.m_A.C();
			}
		}

		internal void b()
		{
			if (A(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 19u))
			{
				if (this.m_c != 0)
				{
					global::B.H.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kt(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kU());
				}
				this.m_c = 0;
				return;
			}
			int num = Math.Max(1, Marshal.GetLastWin32Error());
			if (num != this.m_c)
			{
				global::B.H.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ku(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kV() + num);
			}
			this.m_c = num;
		}

		public D(b P_0, bool P_1 = true)
		{
			this.m_A = P_0.A();
			this.m_A = P_1;
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			base.TopMost = true;
			base.StartPosition = FormStartPosition.Manual;
			base.AutoScaleMode = AutoScaleMode.None;
			BackColor = Color.FromArgb(1, 2, 3);
			base.TransparencyKey = BackColor;
			DoubleBuffered = true;
		}

		[Conditional("SKYEYE_PIPELINE_TRACE")]
		public void C()
		{
			_ = this.m_A;
		}

		protected override void OnHandleCreated(EventArgs P_0)
		{
			base.OnHandleCreated(P_0);
			if (this.m_A)
			{
				this.m_A = new global::B.n(base.Handle, 33494, 5);
			}
		}

		protected override void OnHandleDestroyed(EventArgs P_0)
		{
			if (this.m_A != null)
			{
				this.m_A.Dispose();
				this.m_A = null;
			}
			base.OnHandleDestroyed(P_0);
		}

		protected override void WndProc(ref Message P_0)
		{
			if (P_0.Msg == 33494)
			{
				try
				{
					e();
					return;
				}
				finally
				{
					if (this.m_A != null)
					{
						this.m_A.c();
					}
				}
			}
			if (P_0.Msg == 132)
			{
				P_0.Result = new IntPtr(-1);
			}
			else if (P_0.Msg == 33)
			{
				P_0.Result = new IntPtr(3);
			}
			else
			{
				base.WndProc(ref P_0);
			}
		}

		public void A(b P_0)
		{
			this.m_A = P_0;
			if (!P_0.A.A && base.IsHandleCreated)
			{
				try
				{
					BeginInvoke(new Action(D));
				}
				catch (InvalidOperationException)
				{
				}
			}
			if (this.m_A != null)
			{
				this.m_A.C();
			}
		}

		public void c()
		{
			if (base.IsHandleCreated)
			{
				try
				{
					BeginInvoke(new Action(D));
				}
				catch (InvalidOperationException)
				{
				}
			}
		}

		private void D()
		{
			Interlocked.Exchange(ref this.m_A, 0L);
			this.m_A.Clear();
			this.m_A = 0.0;
			if (this.m_A != null)
			{
				this.m_A.D();
			}
			this.m_A.Clear();
			this.m_A.Clear();
			if (base.Visible)
			{
				Hide();
			}
			Invalidate();
		}

		public void A(global::a.u P_0)
		{
			this.m_A = P_0;
			if (this.m_A != null)
			{
				this.m_A.C();
			}
		}

		private void A(bool P_0 = true)
		{
			if (base.Visible)
			{
				Hide();
			}
			if (this.m_A != null)
			{
				this.m_A.a = (P_0 ? 5 : this.m_A.E);
			}
			this.m_d = 0.0;
			this.m_a = (this.m_B = 0);
			this.m_E = (this.m_e = 0.0);
			this.m_A.Clear();
			this.m_D = this.m_A.Elapsed.TotalMilliseconds;
		}

		private void d()
		{
			if (this.m_A != null)
			{
				this.m_A.Dispose();
			}
			this.m_A = null;
			this.m_A = null;
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.a();
			this.m_A = 0.0;
			this.m_a.a();
			this.m_B = (this.m_a = 0.0);
			this.m_B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
			this.m_C = 0;
		}

		private global::a.v E()
		{
			Exception ex = null;
			for (int m = 0; m < 3; m++)
			{
				try
				{
					return this.m_A.e();
				}
				catch (Exception ex2)
				{
					ex = ex2;
				}
			}
			throw ex;
		}

		private void e()
		{
			b A = this.m_A;
			global::a.u u = this.m_A;
			if (!global::a.N.A)
			{
				d();
				this.A();
				return;
			}
			if (u == null)
			{
				d();
				this.A();
				return;
			}
			if ((!A.B && !A.A.a && !A.A.A && DateTime.UtcNow.Ticks > Interlocked.Read(ref this.m_A)) || DateTime.UtcNow - u.A > TimeSpan.FromSeconds(3.0))
			{
				this.A();
				return;
			}
			IntPtr intPtr = global::B.G.A();
			global::B.G.A(intPtr, out var num);
			if (num != u.A.A || global::b.D.A(intPtr))
			{
				this.A();
				return;
			}
			if (I.A(u.A.A, u.A.A))
			{
				this.A(false);
				return;
			}
			Point point = default(Point);
			if (!global::b.D.A(intPtr, out var A2) || A2.B < 200 || A2.b < 100 || !global::b.D.A(intPtr, ref point))
			{
				this.A();
				return;
			}
			try
			{
				if (this.m_A == null || this.m_A != u.A.A)
				{
					d();
					this.m_A = new global::B.m(u.A);
					this.m_A = u.A.A;
				}
				this.m_A = u;
				if (this.m_A != null)
				{
					this.m_A.a = A.E;
				}
				this.m_a.A(A);
				if (this.m_b != A.e)
				{
					this.m_b = A.e;
					this.m_B = 0.0;
				}
				if (!A.B)
				{
					this.m_A.Clear();
					this.m_B = 0.0;
				}
				else if (this.m_A.Elapsed.TotalMilliseconds >= this.m_B)
				{
					double totalMilliseconds = this.m_A.Elapsed.TotalMilliseconds;
					this.m_A = this.m_A.A(this.m_A, this.m_A.A);
					this.m_C = this.m_A.Elapsed.TotalMilliseconds - totalMilliseconds;
					this.m_B++;
					double num2 = 1000.0 / (double)A.e;
					this.m_B += num2;
					if (this.m_B <= totalMilliseconds)
					{
						this.m_B = totalMilliseconds + num2;
					}
				}
				if (!A.A.a)
				{
					this.m_A.Clear();
					this.m_a = 0.0;
					this.m_B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
				}
				else if (this.m_A.Elapsed.TotalMilliseconds >= this.m_a)
				{
					double totalMilliseconds2 = this.m_A.Elapsed.TotalMilliseconds;
					try
					{
						this.m_A = this.m_A.A(this.m_A, (this.m_A.a ?? this.m_A.A ?? new global::B.S[0]).Where((global::B.S P_0) => P_0.A == null || P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz() || (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA() && A.A.A) || (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL() && A.A.a)), A.A.A);
						this.m_B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
					}
					catch (Exception ex)
					{
						this.m_A.Clear();
						this.m_B = ex.Message;
					}
					this.m_a += 33.333333333333336;
					if (this.m_a <= totalMilliseconds2)
					{
						this.m_a = totalMilliseconds2 + 33.333333333333336;
					}
				}
				if (!A.A.A)
				{
					this.m_A.Clear();
					this.m_A = 0.0;
				}
				else if (this.m_A.Elapsed.TotalMilliseconds >= this.m_A)
				{
					try
					{
						this.m_A = this.m_A.A(this.m_A, this.m_A.A ?? new global::B.S[0]);
					}
					catch (Exception ex2)
					{
						this.m_A.Clear();
						global::A.j.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kw() + ex2.Message);
					}
					this.m_A = this.m_A.Elapsed.TotalMilliseconds + 33.333333333333336;
				}
				double totalMilliseconds3 = this.m_A.Elapsed.TotalMilliseconds;
				global::a.v v = E();
				this.m_c = this.m_A.Elapsed.TotalMilliseconds;
				this.m_b = this.m_c - totalMilliseconds3;
				List<global::A.Y> list = global::A.X.A(this.m_A, v, A, A2.B, A2.b);
				List<global::a.t> A3 = new List<global::a.t>();
				foreach (global::B.Z item2 in this.m_A)
				{
					if (v.A(item2.A, item2.a + (item2.a ? 3f : (item2.A ? 1.2f : 0.8f)), item2.B, A2.B, A2.b, out var pointF))
					{
						Bitmap bitmap = this.m_a.A(item2);
						A3.Add(new global::a.t
						{
							A = bitmap,
							A = new Rectangle((int)Math.Round(pointF.X - (float)bitmap.Width / 2f), (int)Math.Round(pointF.Y - (float)bitmap.Height / 2f), bitmap.Width, bitmap.Height)
						});
					}
				}
				List<Rectangle> A4 = new List<Rectangle>();
				list.AddRange(global::B.E.A(this.m_A, v, A, A2.B, A2.b, delegate(global::B.Z P_0, PointF P_1)
				{
					Bitmap bitmap2 = this.m_a.a(P_0);
					Rectangle item = global::B.e.A(bitmap2.Size, P_1, A2.B, A2.b, A4);
					A4.Add(item);
					A3.Add(new global::a.t
					{
						A = bitmap2,
						A = item
					});
				}));
				list.AddRange(global::B.Q.A(this.m_A, v, A.A, A2.B, A2.b));
				global::B.Q.A(A3, this.m_A, v, A.A, A2.B, A2.b, this.m_A);
				int num3 = global::A.j.A(A3, list, this.m_A, v, A.A, A2.B, A2.b, this.m_A);
				string text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kX() + this.m_A.Count + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kx() + num3;
				if (text != this.m_a)
				{
					this.m_a = text;
					global::A.j.A(text);
				}
				if (DateTime.UtcNow.Ticks < Interlocked.Read(ref this.m_A))
				{
					global::A.j.A(A3, list, A2.B, A2.b, A.A, this.m_A);
				}
				if (A3.Count == 0 && list.Count == 0 && !A.b)
				{
					this.m_A = A3;
					this.m_A = list;
					this.A = ((this.m_B.Length > 0) ? this.m_B : this.m_A.A);
					this.m_C = 0;
					this.A(false);
					return;
				}
				Rectangle rect = global::b.D.A(A2.B, A2.b);
				using Region region = new Region();
				region.MakeEmpty();
				foreach (global::A.Y item3 in this.m_A)
				{
					region.Union(item3.A);
				}
				foreach (global::A.Y item4 in list)
				{
					region.Union(item4.A);
				}
				this.m_A = list;
				foreach (global::a.t item5 in this.m_A)
				{
					region.Union(item5.A);
				}
				foreach (global::a.t item6 in A3)
				{
					region.Union(item6.A);
				}
				if (A.b || this.m_a)
				{
					region.Union(rect);
				}
				this.m_a = A.b;
				this.m_A = A3;
				Rectangle rectangle = new Rectangle(point.X, point.Y, A2.B, A2.b);
				bool num4 = base.Bounds != rectangle;
				bool flag = !base.Visible;
				if (num4)
				{
					base.Bounds = rectangle;
				}
				this.A = ((this.m_B.Length > 0) ? this.m_B : this.m_A.A);
				if (flag)
				{
					Show();
				}
				b();
				if (num4 || flag)
				{
					Invalidate();
				}
				else
				{
					Invalidate(region);
				}
				Update();
				this.m_C = 0;
			}
			catch (Exception ex3)
			{
				this.A = ex3.Message;
				this.m_C++;
				if (this.m_A == null)
				{
					this.A();
					return;
				}
				if (this.m_C >= 5 || ex3.Message.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kY()))
				{
					this.m_A.Clear();
					if (base.Visible)
					{
						Hide();
					}
				}
				if (this.m_A != null)
				{
					this.m_A.a = A.E;
				}
			}
		}

		private static Rectangle A(int P_0, int P_1)
		{
			return new Rectangle(Math.Max(0, P_0 - 555), Math.Max(0, P_1 - 330), 545, 95);
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			double totalMilliseconds = this.m_A.Elapsed.TotalMilliseconds;
			base.OnPaint(P_0);
			if (!global::a.N.A)
			{
				return;
			}
			global::A.X.A(P_0.Graphics, this.m_A, this.m_A.b);
			foreach (global::a.t item in this.m_A)
			{
				if (item.A.IntersectsWith(P_0.ClipRectangle))
				{
					P_0.Graphics.DrawImageUnscaled(item.A, item.A.Location);
				}
			}
			if (this.m_A.b)
			{
				Rectangle rectangle = A(base.ClientSize.Width, base.ClientSize.Height);
				P_0.Graphics.FillRectangle(Brushes.Black, rectangle);
				P_0.Graphics.DrawString(this.m_b, this.m_A, Brushes.White, rectangle);
			}
			if (this.m_A.Count != 0 || this.m_A.Count != 0 || this.m_A.b)
			{
				Interlocked.Increment(ref this.a);
				double totalMilliseconds2 = this.m_A.Elapsed.TotalMilliseconds;
				if (this.m_d > 0.0)
				{
					this.m_A.Add(totalMilliseconds2 - this.m_d);
				}
				this.m_d = totalMilliseconds2;
				this.m_a++;
				this.m_E += totalMilliseconds2 - totalMilliseconds;
				this.m_e += Math.Max(0.0, totalMilliseconds2 - this.m_c);
				if (totalMilliseconds2 - this.m_D >= 1000.0)
				{
					this.m_A.Sort();
					double num = ((this.m_A.Count == 0) ? 0.0 : this.m_A[Math.Min(this.m_A.Count - 1, (int)((double)this.m_A.Count * 0.95))]);
					this.m_b = string.Format(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ky(), (double)(this.m_a * 1000) / (totalMilliseconds2 - this.m_D), this.m_A.E, num, this.m_b, this.m_C, this.m_e / (double)this.m_a, this.m_E / (double)this.m_a, (double)(this.m_B * 1000) / (totalMilliseconds2 - this.m_D), this.m_A.e);
					this.a = this.m_b;
					this.m_D = totalMilliseconds2;
					this.m_a = (this.m_B = 0);
					this.m_E = (this.m_e = 0.0);
					this.m_A.Clear();
				}
			}
		}

		protected override void Dispose(bool P_0)
		{
			if (P_0)
			{
				if (this.m_A != null)
				{
					this.m_A.Dispose();
					this.m_A = null;
				}
				d();
				this.m_a.Dispose();
				this.m_A.Dispose();
				this.m_A.Dispose();
				this.m_A.Dispose();
			}
			base.Dispose(P_0);
		}
	}
	internal sealed class d : Panel
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.D, int> A;

			internal int A(global::B.D P_0)
			{
				return P_0.Height + 12;
			}
		}

		private bool m_A;

		private readonly VScrollBar m_A = new VScrollBar();

		public d()
		{
			Dock = DockStyle.Fill;
			AutoScroll = false;
			base.Padding = new Padding(0, 0, 8, 4);
			base.Margin = Padding.Empty;
			BackColor = global::B.K.A;
			DoubleBuffered = true;
			this.m_A.Dock = DockStyle.Right;
			this.m_A.Width = 14;
			this.m_A.Visible = false;
			base.Controls.Add(this.m_A);
			VScrollBar vScrollBar = this.m_A;
			EventHandler value = delegate
			{
				a();
			};
			vScrollBar.ValueChanged += value;
			base.SizeChanged += delegate
			{
				B();
			};
			base.ControlAdded += delegate(object P_0, ControlEventArgs P_1)
			{
				P_1.Control.SizeChanged += delegate
				{
					B();
				};
				B();
			};
			base.Layout += delegate
			{
				B();
			};
		}

		private int A()
		{
			return base.Controls.OfType<global::B.D>().Sum((global::B.D P_0) => P_0.Height + 12) + base.Padding.Bottom;
		}

		private void a()
		{
			int num = 0;
			foreach (global::B.D item in base.Controls.OfType<global::B.D>())
			{
				item.Location = new Point(0, num - this.m_A.Value);
				num += item.Height + 12;
			}
		}

		public void B()
		{
			if (this.m_A)
			{
				return;
			}
			this.m_A = true;
			try
			{
				for (int m = 0; m < 2; m++)
				{
					foreach (global::B.D item in base.Controls.OfType<global::B.D>())
					{
						item.Width = Math.Max(312, base.ClientSize.Width - base.Padding.Horizontal - (this.m_A.Visible ? this.m_A.Width : 0) - 2);
						item.A();
					}
					this.m_A.Visible = A() > base.ClientSize.Height;
				}
				this.m_A.Minimum = 0;
				this.m_A.LargeChange = Math.Max(1, base.ClientSize.Height);
				this.m_A.SmallChange = 36;
				this.m_A.Maximum = Math.Max(0, A() - 1);
				this.m_A.Value = Math.Min(this.m_A.Value, Math.Max(0, A() - base.ClientSize.Height));
				a();
				this.m_A.BringToFront();
			}
			finally
			{
				this.m_A = false;
			}
		}

		protected override void OnMouseWheel(MouseEventArgs P_0)
		{
			if (this.m_A.Visible)
			{
				this.m_A.Value = Math.Max(0, Math.Min(Math.Max(0, this.m_A.Maximum - this.m_A.LargeChange + 1), this.m_A.Value - P_0.Delta / 120 * 72));
			}
			base.OnMouseWheel(P_0);
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			a();
		}

		[CompilerGenerated]
		private void a(object P_0, EventArgs P_1)
		{
			B();
		}

		[CompilerGenerated]
		private void A(object P_0, ControlEventArgs P_1)
		{
			P_1.Control.SizeChanged += delegate
			{
				B();
			};
			B();
		}

		[CompilerGenerated]
		private void B(object P_0, EventArgs P_1)
		{
			B();
		}

		[CompilerGenerated]
		private void A(object P_0, LayoutEventArgs P_1)
		{
			B();
		}
	}
}
