using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;
using a;
using b;

namespace B
{
	internal static class A
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::b.A, bool> A;

			internal bool A(global::b.A P_0)
			{
				return !P_0.a;
			}
		}

		public static void A(Graphics P_0, IEnumerable<global::b.A> P_1, global::a.z P_2, C P_3, int P_4, int P_5, IEnumerable<global::A.T> P_6 = null)
		{
			if (!P_3.a || P_2 == null || !P_2.A)
			{
				return;
			}
			GraphicsState gstate = P_0.Save();
			try
			{
				using (GraphicsPath graphicsPath = new GraphicsPath())
				{
					graphicsPath.AddPolygon(P_2.A(P_4, P_5));
					P_0.SetClip(graphicsPath, CombineMode.Intersect);
				}
				P_0.SmoothingMode = SmoothingMode.AntiAlias;
				B.A(P_0, B.A(P_1, P_2, P_3, P_4, P_5), P_3);
				foreach (global::b.A item in P_1.Where((global::b.A a2) => !a2.a))
				{
					if (!P_2.A(item.A, item.B, P_4, P_5, out var pointF))
					{
						continue;
					}
					using SolidBrush brush = new SolidBrush(item.A);
					using Pen pen = new Pen(Color.Black, 1f);
					float num = (item.A ? P_3.B : P_3.a);
					if (item.A)
					{
						float num2 = num / 2f;
						PointF[] points = new PointF[3]
						{
							new PointF(pointF.X, pointF.Y - num2),
							new PointF(pointF.X - num2, pointF.Y + num2),
							new PointF(pointF.X + num2, pointF.Y + num2)
						};
						P_0.FillPolygon(brush, points);
						P_0.DrawPolygon(pen, points);
					}
					else
					{
						RectangleF rect = new RectangleF(pointF.X - num / 2f, pointF.Y - num / 2f, num, num);
						P_0.FillEllipse(brush, rect);
						P_0.DrawEllipse(pen, rect);
					}
				}
				global::a.Z.A(P_0, P_6, P_2, P_3, P_4, P_5);
			}
			finally
			{
				P_0.Restore(gstate);
			}
		}
	}
	internal sealed class a : Form
	{
		private struct A
		{
			public int A;

			public int a;

			public int B;

			public int b;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class a
		{
			public static readonly a A = new a();

			public static Func<O, bool> A;

			public static Func<O, IEnumerable<global::A.T>> A;

			public static Func<O, bool> a;

			public static Func<O, uint> A;

			public static Func<O, bool> B;

			public static Func<O, IEnumerable<global::b.d>> A;

			internal bool A(O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal IEnumerable<global::A.T> a(O P_0)
			{
				return P_0.A;
			}

			internal bool B(O P_0)
			{
				return !P_0.B;
			}

			internal uint b(O P_0)
			{
				return P_0.A;
			}

			internal bool C(O P_0)
			{
				return P_0.C;
			}

			internal IEnumerable<global::b.d> c(O P_0)
			{
				return P_0.A;
			}
		}

		private readonly bool m_A;

		private readonly C m_A;

		private readonly Timer m_A = new Timer();

		private readonly global::A.S m_A = new global::A.S();

		private List<global::A.T> m_A = new List<global::A.T>();

		private n m_A;

		private string m_A;

		private int m_A;

		private DateTime m_A;

		private global::b.d[] m_A = new global::b.d[0];

		private List<global::b.A> m_A = new List<global::b.A>();

		private global::a.z m_A;

		private global::a.z m_a;

		private readonly global::b.i m_A;

		private string m_a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		private DateTime m_a;

		public Func<global::a.b> A;

		public string B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		protected override bool A => true;

		protected override CreateParams a
		{
			get
			{
				CreateParams obj = base.CreateParams;
				obj.ExStyle |= 134742176;
				return obj;
			}
		}

		public int B => this.m_A.Interval;

		[DllImport("user32.dll", EntryPoint = "GetClientRect")]
		private static extern bool A(IntPtr P_0, out A P_1);

		[DllImport("user32.dll", EntryPoint = "ClientToScreen")]
		private static extern bool A(IntPtr P_0, ref Point P_1);

		[DllImport("user32.dll", EntryPoint = "IsIconic")]
		private static extern bool A(IntPtr P_0);

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		private static extern bool A(IntPtr P_0, IntPtr P_1, int P_2, int P_3, int P_4, int P_5, uint P_6);

		private void A(string P_0, string P_1 = "")
		{
			if (P_0 == this.m_a && DateTime.UtcNow < this.m_a)
			{
				return;
			}
			this.m_a = P_0;
			this.m_a = DateTime.UtcNow.AddSeconds(10.0);
			try
			{
				string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ez());
				if (File.Exists(text) && new FileInfo(text).Length > 262144)
				{
					File.WriteAllText(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
				}
				File.AppendAllText(text, DateTime.Now.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + P_1 + Environment.NewLine);
			}
			catch
			{
			}
		}

		public a(C P_0, bool P_1 = true, global::b.i P_2 = null)
		{
			this.m_A = P_1;
			this.m_A = P_0;
			this.m_A = P_2 ?? new global::b.i
			{
				A = false
			};
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			base.TopMost = true;
			base.StartPosition = FormStartPosition.Manual;
			base.AutoScaleMode = AutoScaleMode.None;
			BackColor = Color.FromArgb(1, 2, 3);
			base.TransparencyKey = BackColor;
			DoubleBuffered = true;
			B();
			this.m_A.Tick += delegate
			{
				C();
			};
			if (P_1)
			{
				this.m_A.Start();
			}
		}

		protected override void WndProc(ref Message P_0)
		{
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

		public void a()
		{
			this.m_A = null;
			this.m_a = null;
			this.m_A.Clear();
			if (this.m_A != null)
			{
				this.m_A.f();
			}
			A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eA(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
			if (this.m_A)
			{
				this.m_A.Start();
			}
			Hide();
			this.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ea();
		}

		public void B()
		{
			if (this.m_A && !this.m_A.Enabled)
			{
				this.m_A.Start();
			}
			this.m_A.Interval = Math.Max(1, 1000 / this.m_A.b);
			if (!this.m_A.a && !this.m_A.A)
			{
				Hide();
			}
			Invalidate();
		}

		public void A(n P_0, int P_1, IEnumerable<O> P_2)
		{
			if (this.m_A != P_0 || this.m_A != P_0.C)
			{
				this.m_A = null;
				this.m_a = null;
				this.m_A.Clear();
				this.m_A.Clear();
				this.m_A.A();
			}
			this.m_A = P_0;
			this.m_A = P_0.C;
			this.m_A = P_1;
			O[] array = P_2.Where((O o4) => o4.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()).ToArray();
			O[] array2 = array;
			foreach (O o2 in array2)
			{
				foreach (global::A.T item in o2.A)
				{
					item.A = (o2.b ? o2.A : Color.Red);
				}
			}
			this.m_A = this.m_A.A(this.m_A, array.SelectMany((O o4) => o4.A), DateTime.UtcNow, from o4 in array
				where !o4.B
				select o4.A);
			O[] array3 = array.Where((O o4) => o4.C).ToArray();
			array2 = array3;
			foreach (O o3 in array2)
			{
				foreach (global::b.d item2 in o3.A)
				{
					item2.A = o3.b;
					item2.A = o3.A;
				}
			}
			this.m_A = array3.SelectMany((O o4) => o4.A).ToArray();
			this.m_A = DateTime.UtcNow;
		}

		public void b()
		{
			this.m_A = null;
			this.m_A = null;
			this.m_a = null;
			this.m_A = new global::b.d[0];
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.A();
			this.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			Hide();
		}

		private void C()
		{
			if (!global::a.O.A || (!this.m_A.a && !this.m_A.A) || this.m_A == null || DateTime.UtcNow - this.m_A > TimeSpan.FromSeconds(3.0))
			{
				Hide();
				return;
			}
			IntPtr intPtr = H.A();
			H.A(intPtr, out var num);
			if (num != this.m_A || A(intPtr))
			{
				Hide();
				return;
			}
			Point point = default(Point);
			if (!A(intPtr, out var a2) || a2.B < 200 || a2.b < 100 || !A(intPtr, ref point))
			{
				Hide();
				return;
			}
			try
			{
				this.m_A = null;
				this.m_a = null;
				bool num2 = this.m_A.a && this.m_A.A(out this.m_A);
				bool flag = this.m_A.A && this.m_A.a(out this.m_a);
				if (!num2 && !flag)
				{
					this.B = (this.m_A.a ? this.m_A.B : this.m_A.C);
					A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eB(), this.B + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + this.m_A.a);
					Hide();
					return;
				}
				this.m_A = this.m_A.A(this.m_A, this.m_A);
				this.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				base.Bounds = new Rectangle(point.X, point.Y, a2.B, a2.b);
				if (this.m_A.Count == 0 && (!this.m_A.b || this.m_A.Count == 0) && (this.A == null || this.A() == null))
				{
					A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eb(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
					Hide();
					return;
				}
				A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eC(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ec() + this.m_A.Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eD() + this.m_A.Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + this.m_A.a);
				if (!base.Visible)
				{
					Show();
				}
				A(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 19u);
				Invalidate();
			}
			catch (Exception ex)
			{
				this.m_A = null;
				this.m_a = null;
				this.m_A.Clear();
				this.B = ex.Message;
				A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ed(), ex.Message);
				Hide();
			}
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			base.OnPaint(P_0);
			if (global::a.O.A)
			{
				global::B.A.A(P_0.Graphics, this.m_A, this.m_A, this.m_A, base.ClientSize.Width, base.ClientSize.Height, this.m_A);
				global::b.I.A(P_0.Graphics, this.m_A, this.m_a, this.m_A, this.m_A, base.ClientSize.Width, base.ClientSize.Height, this.m_A);
				global::a.b b2 = ((this.A == null) ? null : this.A());
				if (this.m_A.a)
				{
					global::a.B.A(P_0.Graphics, b2, this.m_A, base.ClientSize.Width, base.ClientSize.Height);
				}
				if (this.m_A.A)
				{
					global::a.B.A(P_0.Graphics, b2, this.m_a, base.ClientSize.Width, base.ClientSize.Height);
				}
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
		private void A(object P_0, EventArgs P_1)
		{
			C();
		}
	}
}
namespace b
{
	internal sealed class A
	{
		public long A;

		public long a;

		public global::B.e A;

		public global::A.X A;

		public string A;

		public string a;

		public uint A;

		public bool A;

		public bool a;

		public Color A;

		public float A;

		public float a;

		public float B;
	}
	internal static class a
	{
		public static void A(Graphics P_0, PointF P_1, bool P_2, string P_3, Color P_4, c P_5, Font P_6)
		{
			float num = (P_2 ? P_5.D : P_5.c);
			RectangleF rect = new RectangleF(P_1.X - num / 2f, P_1.Y - num / 2f, num, num);
			P_0.SmoothingMode = SmoothingMode.AntiAlias;
			using (Pen pen = new Pen(P_4, (float)P_5.C))
			{
				if (P_2)
				{
					P_0.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
				}
				else
				{
					P_0.DrawEllipse(pen, rect);
				}
			}
			TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
			Size size = TextRenderer.MeasureText(P_0, P_3 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), P_6, new Size(32767, 32767), flags);
			Rectangle rectangle = new Rectangle((int)Math.Round(P_1.X - (float)size.Width / 2f), (int)Math.Round(P_1.Y - (float)size.Height / 2f), size.Width, size.Height);
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
				TextRenderer.DrawText(P_0, P_3, P_6, bounds, Color.Black, flags);
			}
			TextRenderer.DrawText(P_0, P_3, P_6, rectangle, P_4, flags);
		}
	}
}
