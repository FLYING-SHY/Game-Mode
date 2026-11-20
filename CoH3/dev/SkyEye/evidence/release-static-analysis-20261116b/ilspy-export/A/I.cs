using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using B;
using a;
using b;

namespace A
{
	internal sealed class I : Form
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

			public static Func<global::B.S, uint> A;

			public static Func<k, string> A;

			public static Func<k, bool> A;

			public static Func<k, bool> a;

			internal uint A(global::B.S P_0)
			{
				return P_0.A;
			}

			internal string A(k P_0)
			{
				return P_0.A;
			}

			internal bool a(k P_0)
			{
				return P_0.A;
			}

			internal bool B(k P_0)
			{
				return P_0.A;
			}
		}

		private const int m_A = 33508;

		private string m_A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		private readonly HashSet<string> m_A = new HashSet<string>();

		private long m_A;

		private readonly bool m_A;

		private volatile global::b.b m_A;

		private volatile global::a.u m_A;

		private global::B.n m_A;

		private readonly Stopwatch m_A = Stopwatch.StartNew();

		private readonly N m_A = new N();

		private global::B.m m_A;

		private string m_a;

		private List<k> m_A = new List<k>();

		private double m_A;

		private double m_a;

		private global::a.y m_A;

		private global::a.y m_a;

		private volatile global::b.h m_A;

		private Font m_A;

		private Font m_a;

		private int m_a;

		private int m_B;

		private int m_b;

		public volatile string A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

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

		[DllImport("user32.dll", EntryPoint = "GetClientRect")]
		private static extern bool A(IntPtr P_0, out A P_1);

		[DllImport("user32.dll", EntryPoint = "ClientToScreen")]
		private static extern bool A(IntPtr P_0, ref Point P_1);

		[DllImport("user32.dll", EntryPoint = "IsIconic")]
		private static extern bool A(IntPtr P_0);

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		private static extern bool A(IntPtr P_0, IntPtr P_1, int P_2, int P_3, int P_4, int P_5, uint P_6);

		private void A(string P_0)
		{
			try
			{
				string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ay());
				if (File.Exists(text) && new FileInfo(text).Length > 262144)
				{
					File.WriteAllText(text, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aZ());
				}
				File.AppendAllText(text, DateTime.Now.ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.az()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA() + P_0 + Environment.NewLine);
			}
			catch
			{
			}
		}

		private void a(string P_0)
		{
			if (!(this.m_A == P_0))
			{
				this.m_A = P_0;
				A(P_0);
			}
		}

		public void A()
		{
			Interlocked.Exchange(ref this.m_A, DateTime.UtcNow.AddSeconds(8.0).Ticks);
			if (this.m_A != null)
			{
				this.m_A.C();
			}
		}

		public I(global::b.b P_0, bool P_1 = true, global::b.h P_2 = null)
		{
			this.m_A = P_0;
			this.m_A = A(P_2);
			this.m_A = P_1;
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			base.TopMost = true;
			base.StartPosition = FormStartPosition.Manual;
			base.AutoScaleMode = AutoScaleMode.None;
		}

		protected override void OnHandleCreated(EventArgs P_0)
		{
			base.OnHandleCreated(P_0);
			if (this.m_A)
			{
				this.m_A = new global::B.n(base.Handle, 33508, 5);
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
			if (P_0.Msg == 33508)
			{
				try
				{
					c();
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

		protected override void OnPaintBackground(PaintEventArgs P_0)
		{
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
		}

		private static global::b.h A(global::b.h P_0)
		{
			return new global::b.h
			{
				A = (P_0?.A ?? false),
				A = (P_0?.A ?? 12)
			};
		}

		public void A(global::b.b P_0, global::b.h P_1 = null)
		{
			this.m_A = P_0;
			this.m_A = A(P_1);
			if (!P_0.A.A && base.IsHandleCreated)
			{
				try
				{
					BeginInvoke(new Action(B));
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

		public void a()
		{
			if (base.IsHandleCreated)
			{
				try
				{
					BeginInvoke(new Action(B));
				}
				catch (InvalidOperationException)
				{
				}
			}
		}

		private void B()
		{
			Interlocked.Exchange(ref this.m_A, 0L);
			this.m_A.Clear();
			this.m_A = 0.0;
			if (this.m_A != null)
			{
				this.m_A.D();
			}
			if (this.m_A.a != null)
			{
				this.m_A.a.Clear(Color.Transparent);
			}
			if (base.Visible)
			{
				Hide();
			}
		}

		public void A(global::a.u P_0)
		{
			this.m_A = P_0;
			if (this.m_A != null)
			{
				this.m_A.C();
			}
		}

		private void b()
		{
			if (base.Visible)
			{
				Hide();
			}
			if (this.m_A != null)
			{
				this.m_A.a = 5;
			}
		}

		private void C()
		{
			if (this.m_A != null)
			{
				this.m_A.Dispose();
			}
			this.m_A = null;
			this.m_a = null;
			this.m_A.Clear();
			this.m_A = null;
			this.m_a = null;
			this.m_A = (this.m_a = 0.0);
		}

		private void A(J P_0)
		{
			if (this.m_A == null || this.m_a != P_0.B)
			{
				if (this.m_A != null)
				{
					this.m_A.Dispose();
				}
				this.m_A = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), P_0.B, FontStyle.Bold);
				this.m_a = P_0.B;
			}
			if (this.m_a == null || this.m_B != P_0.b)
			{
				if (this.m_a != null)
				{
					this.m_a.Dispose();
				}
				this.m_a = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), P_0.b, FontStyle.Bold);
				this.m_B = P_0.b;
			}
		}

		private void c()
		{
			global::a.u u2 = this.m_A;
			J j2 = this.m_A.A;
			if (!global::a.N.A)
			{
				C();
				if (this.m_A.a != null)
				{
					this.m_A.a.Clear(Color.Transparent);
				}
				b();
				return;
			}
			if (u2 == null)
			{
				C();
				b();
				return;
			}
			if ((!j2.A && !j2.a && !j2.B) || DateTime.UtcNow - u2.A > TimeSpan.FromSeconds(3.0))
			{
				b();
				return;
			}
			IntPtr intPtr = global::B.G.A();
			global::B.G.A(intPtr, out var num);
			if (num != u2.A.A || A(intPtr))
			{
				b();
				return;
			}
			Point empty = Point.Empty;
			if (!A(intPtr, out var a2) || a2.B < 200 || a2.b < 100 || !A(intPtr, ref empty))
			{
				b();
				return;
			}
			try
			{
				if (this.m_A == null || this.m_a != u2.A.A)
				{
					C();
					this.m_A = new global::B.m(u2.A);
					this.m_a = u2.A.A;
					A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BB() + this.m_a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bb() + string.Join(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BC(), u2.A.Select((global::B.S P_0) => P_0.A)));
					this.m_A.Clear();
				}
				if (this.m_A != null)
				{
					this.m_A.a = this.m_A.E;
				}
				double totalMilliseconds = this.m_A.Elapsed.TotalMilliseconds;
				if (totalMilliseconds >= this.m_A)
				{
					this.m_A = this.m_A.A(this.m_a, u2.A ?? new global::B.S[0]);
					HashSet<string> hashSet = new HashSet<string>(this.m_A.Select((k P_0) => P_0.A));
					foreach (k item in this.m_A.Where((k P_0) => !this.m_A.Contains(P_0.A)))
					{
						A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bc() + item.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BD() + item.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bd() + (item.A.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aX()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Be() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BE()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BF() + item.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BC() + item.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BC() + item.B);
					}
					foreach (string item2 in this.m_A.Except(hashSet))
					{
						A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bf() + item2);
					}
					this.m_A.Clear();
					foreach (string item3 in hashSet)
					{
						this.m_A.Add(item3);
					}
					this.m_A += 33.333333333333336;
					if (this.m_A <= totalMilliseconds)
					{
						this.m_A = totalMilliseconds + 33.333333333333336;
					}
				}
				if (this.m_A.Count == 0 && DateTime.UtcNow.Ticks >= Interlocked.Read(ref this.m_A))
				{
					this.A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
					this.m_b = 0;
					if (base.Visible)
					{
						Hide();
					}
					return;
				}
				if (j2.B && this.m_A.Any((k P_0) => P_0.A) && totalMilliseconds >= this.m_a)
				{
					this.m_A.A(out this.m_A);
					if (this.m_A.A)
					{
						this.m_A.a(out this.m_a);
					}
					else
					{
						this.m_a = null;
					}
					this.m_a = totalMilliseconds + 100.0;
				}
				A(j2);
				this.m_A.A(a2.B, a2.b);
				this.m_A.a.Clear(Color.Transparent);
				if (!global::b.I.A(u2.A.A, u2.A.A))
				{
					global::a.v v2 = this.m_A.e();
					g.A(this.m_A.a, this.m_A.Where((k P_0) => P_0.A), v2, j2, a2.B, a2.b, this.m_A);
					j.A(this.m_A.a, this.m_A, v2, j2, a2.B, a2.b);
					if (DateTime.UtcNow.Ticks < Interlocked.Read(ref this.m_A))
					{
						j.A(this.m_A.a, a2.B, a2.b, j2);
					}
				}
				g.A(this.m_A.a, this.m_A, this.m_A, j2, a2.B, a2.b, this.m_a);
				if (this.m_A.A)
				{
					g.A(this.m_A.a, this.m_A, this.m_a, j2, a2.B, a2.b, this.m_a);
				}
				if (!base.Visible)
				{
					Show();
				}
				this.m_A.A(base.Handle, empty);
				A(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 19u);
				a(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BG());
				this.A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
				this.m_b = 0;
			}
			catch (Exception ex)
			{
				a(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bg() + ex.Message);
				this.A = ex.Message;
				this.m_A.Clear();
				this.m_A = null;
				if (++this.m_b >= 3)
				{
					C();
				}
				if (base.Visible)
				{
					Hide();
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
				C();
				this.m_A.Dispose();
				if (this.m_A != null)
				{
					this.m_A.Dispose();
				}
				if (this.m_a != null)
				{
					this.m_a.Dispose();
				}
			}
			base.Dispose(P_0);
		}

		[CompilerGenerated]
		private bool A(k P_0)
		{
			return !this.m_A.Contains(P_0.A);
		}
	}
	internal sealed class i
	{
		public string A;

		public string a;

		public long A;

		public double A;

		public k A;

		public bool A;

		public bool a;

		public float A;

		public float a;
	}
}
namespace a
{
	internal static class I
	{
		private static Icon m_A;

		public static readonly Color A = Color.FromArgb(8, 15, 24);

		public static readonly Color a = Color.FromArgb(13, 26, 38);

		public static readonly Color B = Color.FromArgb(220, 235, 241);

		public static readonly Color b = Color.FromArgb(104, 122, 138);

		public static readonly Color C = Color.FromArgb(57, 87, 105);

		public static readonly Color c = Color.FromArgb(102, 207, 222);

		public static readonly Color D = Color.FromArgb(20, 35, 49);

		public static Icon A
		{
			get
			{
				if (I.m_A == null)
				{
					using Stream stream = typeof(I).Assembly.GetManifestResourceStream(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dD());
					I.m_A = new Icon(stream);
				}
				return I.m_A;
			}
		}

		public static Font A(float P_0 = 10f, FontStyle P_1 = FontStyle.Regular)
		{
			return new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), P_0 * 96f / 72f, P_1, GraphicsUnit.Pixel);
		}

		public static Label A(string P_0, int P_1 = 28, float P_2 = 10f, Color? P_3 = null)
		{
			return new Label
			{
				Text = P_0,
				Height = P_1,
				Dock = DockStyle.Top,
				ForeColor = (P_3 ?? B),
				Font = A(P_2),
				AutoEllipsis = true,
				TextAlign = ContentAlignment.MiddleLeft
			};
		}
	}
	internal static class i
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.P, string> A;

			public static Func<global::B.P, string> a;

			internal string A(global::B.P P_0)
			{
				return P_0.a;
			}

			internal string a(global::B.P P_0)
			{
				return P_0.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DR();
			}
		}

		public static bool A(string P_0)
		{
			if (P_0 != null && P_0.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dd(), StringComparison.OrdinalIgnoreCase))
			{
				return P_0.Substring(P_0.LastIndexOf('\\') + 1).StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dE(), StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		public static bool a(string P_0)
		{
			return string.Equals(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.de(), StringComparison.OrdinalIgnoreCase);
		}

		public static string A(global::B.N P_0)
		{
			if (!P_0.a || P_0.A.Count == 0)
			{
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
			}
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dF() + string.Join(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cd(), from p2 in P_0.A.OrderBy((global::B.P p2) => p2.a, StringComparer.Ordinal)
				select p2.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DR());
		}
	}
}
