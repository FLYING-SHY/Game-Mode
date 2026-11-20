using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using B;
using a;
using b;

namespace A
{
	internal sealed class Z : Form
	{
		private struct A
		{
			public int A;

			public int a;

			public int B;

			public int b;
		}

		private struct a
		{
			public int A;

			public int a;
		}

		private readonly Timer m_A = new Timer();

		private readonly global::B.I m_A;

		private string[] m_A = new string[0];

		private Color[] m_A = new Color[0];

		private global::B.g[][] m_A = new global::B.g[0][];

		private int m_A;

		private DateTime m_A = DateTime.MinValue;

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

		public int B => A(base.Handle, -20);

		[DllImport("user32.dll", EntryPoint = "GetClientRect")]
		private static extern bool A(IntPtr P_0, out A P_1);

		[DllImport("user32.dll", EntryPoint = "ClientToScreen")]
		private static extern bool A(IntPtr P_0, ref a P_1);

		[DllImport("user32.dll", EntryPoint = "IsIconic")]
		private static extern bool A(IntPtr P_0);

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		private static extern bool A(IntPtr P_0, IntPtr P_1, int P_2, int P_3, int P_4, int P_5, uint P_6);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
		private static extern int A(IntPtr P_0, int P_1);

		public Z(global::B.I P_0, bool P_1 = true)
		{
			this.m_A = P_0;
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			base.TopMost = true;
			base.StartPosition = FormStartPosition.Manual;
			base.AutoScaleMode = AutoScaleMode.None;
			BackColor = Color.FromArgb(1, 2, 3);
			base.TransparencyKey = BackColor;
			DoubleBuffered = true;
			a();
			this.m_A.Interval = 50;
			Timer timer = this.m_A;
			EventHandler value = delegate
			{
				b();
			};
			timer.Tick += value;
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
			Font font = Font;
			Font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), this.m_A.B, FontStyle.Bold, GraphicsUnit.Point);
			if (font != null && !font.Equals(Control.DefaultFont))
			{
				font.Dispose();
			}
			Color backColor = Color.FromArgb(1, 2, 3);
			while (backColor.ToArgb() == this.m_A.B.ToArgb() || backColor.ToArgb() == this.m_A.b.ToArgb())
			{
				backColor = Color.FromArgb(backColor.R + 1, 2, 3);
			}
			BackColor = backColor;
			base.TransparencyKey = BackColor;
			Invalidate();
		}

		public void A(int P_0, IEnumerable<global::B.N> P_1)
		{
			this.m_A = P_0;
			global::B.N[] array = P_1.ToArray();
			this.m_A = global::B.j.B(array, this.m_A);
			this.m_A = global::B.j.A(array, this.m_A);
			this.m_A = global::B.j.a(array, this.m_A);
			this.m_A = DateTime.UtcNow;
			Invalidate();
			b();
		}

		public void B()
		{
			this.m_A = new string[0];
			this.m_A = DateTime.MinValue;
			Hide();
		}

		private void b()
		{
			if (!global::a.N.A || !this.m_A.D || this.m_A.Length == 0 || DateTime.UtcNow - this.m_A > TimeSpan.FromSeconds(3.0))
			{
				Hide();
				return;
			}
			IntPtr intPtr = global::B.G.A();
			global::B.G.A(intPtr, out var num);
			if (num != this.m_A || A(intPtr))
			{
				Hide();
				return;
			}
			a a2 = default(a);
			if (!A(intPtr, out var a3) || !A(intPtr, ref a2) || a3.B < 200 || a3.b < 100)
			{
				Hide();
				return;
			}
			int num2 = Math.Max(100, (a3.B - a3.A) * this.m_A.C / 100);
			int num3;
			using (Graphics graphics = CreateGraphics())
			{
				num3 = global::B.h.A(graphics, this.m_A, Font, this.m_A.B, this.m_A.b, num2, false, this.m_A.a, this.m_A, this.m_A);
			}
			int num4 = Math.Min(this.m_A.b, Math.Max(0, a3.b - num3));
			base.Bounds = new Rectangle(a2.A + (a3.B - num2) / 2, a2.a + num4, num2, Math.Min(num3, a3.b - num4));
			if (!base.Visible)
			{
				Show();
			}
			A(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 19u);
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			base.OnPaint(P_0);
			global::B.h.A(P_0.Graphics, this.m_A, Font, this.m_A.B, this.m_A.b, base.ClientSize.Width, true, this.m_A.a, this.m_A, this.m_A);
		}

		protected override void Dispose(bool P_0)
		{
			if (P_0)
			{
				this.m_A.Stop();
				this.m_A.Dispose();
			}
			base.Dispose(P_0);
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			b();
		}
	}
	internal sealed class z
	{
		private string m_A;

		private readonly HashSet<string> m_A = new HashSet<string>();

		public static string A(global::B.N P_0)
		{
			return P_0.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq() + P_0.A;
		}

		public void A(string P_0)
		{
			if (this.m_A != P_0)
			{
				this.m_A.Clear();
				this.m_A = P_0;
			}
		}

		public bool a(global::B.N P_0)
		{
			if (P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz())
			{
				return !this.m_A.Contains(A(P_0));
			}
			return false;
		}

		public void A(global::B.N P_0, bool P_1)
		{
			if (P_1)
			{
				this.m_A.Remove(A(P_0));
			}
			else
			{
				this.m_A.Add(A(P_0));
			}
		}

		public IEnumerable<global::B.N> A(IEnumerable<global::B.N> P_0)
		{
			return P_0.Where(a);
		}
	}
}
namespace a
{
	internal static class Z
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.Z, bool> A;

			internal bool A(global::B.Z P_0)
			{
				return !P_0.a;
			}
		}

		public static void A(Graphics P_0, IEnumerable<global::B.Z> P_1, y P_2, global::B.B P_3, int P_4, int P_5, IEnumerable<global::A.s> P_6 = null)
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
				global::B.A.A(P_0, global::B.A.A(P_1, P_2, P_3, P_4, P_5), P_3);
				foreach (global::B.Z item in P_1.Where((global::B.Z z2) => !z2.a))
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
				Y.A(P_0, P_6, P_2, P_3, P_4, P_5);
			}
			finally
			{
				P_0.Restore(gstate);
			}
		}
	}
	internal sealed class z : Form
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

			public static Func<global::B.N, bool> A;

			public static Func<global::B.N, IEnumerable<global::A.s>> A;

			public static Func<global::B.N, bool> a;

			public static Func<global::B.N, uint> A;

			public static Func<global::B.N, bool> B;

			public static Func<global::B.N, IEnumerable<global::b.c>> A;

			internal bool A(global::B.N P_0)
			{
				return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz();
			}

			internal IEnumerable<global::A.s> a(global::B.N P_0)
			{
				return P_0.A;
			}

			internal bool B(global::B.N P_0)
			{
				return !P_0.B;
			}

			internal uint b(global::B.N P_0)
			{
				return P_0.A;
			}

			internal bool C(global::B.N P_0)
			{
				return P_0.C;
			}

			internal IEnumerable<global::b.c> c(global::B.N P_0)
			{
				return P_0.A;
			}
		}

		private readonly bool m_A;

		private readonly global::B.B m_A;

		private readonly Timer m_A = new Timer();

		private readonly global::A.r m_A = new global::A.r();

		private List<global::A.s> m_A = new List<global::A.s>();

		private global::B.m m_A;

		private string m_A;

		private int m_A;

		private DateTime m_A;

		private global::b.c[] m_A = new global::b.c[0];

		private List<global::B.Z> m_A = new List<global::B.Z>();

		private y m_A;

		private y m_a;

		private readonly global::b.h m_A;

		private string m_a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();

		private DateTime m_a;

		public Func<B> A;

		public string B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();

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
				string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EW());
				if (File.Exists(text) && new FileInfo(text).Length > 262144)
				{
					File.WriteAllText(text, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
				}
				File.AppendAllText(text, DateTime.Now.ToString(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.az()) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + P_0 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + P_1 + Environment.NewLine);
			}
			catch
			{
			}
		}

		public z(global::B.B P_0, bool P_1 = true, global::b.h P_2 = null)
		{
			this.m_A = P_1;
			this.m_A = P_0;
			this.m_A = P_2 ?? new global::b.h
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
				this.m_A.F();
			}
			A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ew(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
			if (this.m_A)
			{
				this.m_A.Start();
			}
			Hide();
			this.B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EX();
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

		public void A(global::B.m P_0, int P_1, IEnumerable<global::B.N> P_2)
		{
			if (this.m_A != P_0 || this.m_A != P_0.b)
			{
				this.m_A = null;
				this.m_a = null;
				this.m_A.Clear();
				this.m_A.Clear();
				this.m_A.A();
			}
			this.m_A = P_0;
			this.m_A = P_0.b;
			this.m_A = P_1;
			global::B.N[] array = P_2.Where((global::B.N n4) => n4.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()).ToArray();
			global::B.N[] array2 = array;
			foreach (global::B.N n2 in array2)
			{
				foreach (global::A.s item in n2.A)
				{
					item.A = (n2.b ? n2.A : Color.Red);
				}
			}
			this.m_A = this.m_A.A(this.m_A, array.SelectMany((global::B.N n4) => n4.A), DateTime.UtcNow, from n4 in array
				where !n4.B
				select n4.A);
			global::B.N[] array3 = array.Where((global::B.N n4) => n4.C).ToArray();
			array2 = array3;
			foreach (global::B.N n3 in array2)
			{
				foreach (global::b.c item2 in n3.A)
				{
					item2.A = n3.b;
					item2.A = n3.A;
				}
			}
			this.m_A = array3.SelectMany((global::B.N n4) => n4.A).ToArray();
			this.m_A = DateTime.UtcNow;
		}

		public void b()
		{
			this.m_A = null;
			this.m_A = null;
			this.m_a = null;
			this.m_A = new global::b.c[0];
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.A();
			this.B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
			Hide();
		}

		private void C()
		{
			if (!N.A || (!this.m_A.a && !this.m_A.A) || this.m_A == null || DateTime.UtcNow - this.m_A > TimeSpan.FromSeconds(3.0))
			{
				Hide();
				return;
			}
			IntPtr intPtr = global::B.G.A();
			global::B.G.A(intPtr, out var num);
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
					A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ex(), this.B + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + this.m_A.a);
					Hide();
					return;
				}
				this.m_A = this.m_A.A(this.m_A, this.m_A);
				this.B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
				base.Bounds = new Rectangle(point.X, point.Y, a2.B, a2.b);
				if (this.m_A.Count == 0 && (!this.m_A.b || this.m_A.Count == 0) && (this.A == null || this.A() == null))
				{
					A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EY(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
					Hide();
					return;
				}
				A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ey(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EZ() + this.m_A.Count + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ez() + this.m_A.Count + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + this.m_A.a);
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
				A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eA(), ex.Message);
				Hide();
			}
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			base.OnPaint(P_0);
			if (N.A)
			{
				Z.A(P_0.Graphics, this.m_A, this.m_A, this.m_A, base.ClientSize.Width, base.ClientSize.Height, this.m_A);
				global::b.H.A(P_0.Graphics, this.m_A, this.m_a, this.m_A, this.m_A, base.ClientSize.Width, base.ClientSize.Height, this.m_A);
				B b2 = ((this.A == null) ? null : this.A());
				if (this.m_A.a)
				{
					global::a.a.A(P_0.Graphics, b2, this.m_A, base.ClientSize.Width, base.ClientSize.Height);
				}
				if (this.m_A.A)
				{
					global::a.a.A(P_0.Graphics, b2, this.m_a, base.ClientSize.Width, base.ClientSize.Height);
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
