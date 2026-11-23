using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using a;
using b;

namespace A
{
	internal sealed class I : IDisposable
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.O, bool> A;

			public static Func<global::B.O, global::B.T> A;

			internal bool A(global::B.O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal global::B.T a(global::B.O P_0)
			{
				return new global::B.T
				{
					A = P_0.A,
					A = P_0.A,
					A = (P_0.b ? P_0.A : Color.Red)
				};
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public global::b.c A;

			public I A;

			public ManualResetEvent A;

			internal void A()
			{
				bool flag = false;
				try
				{
					this.A.m_A = new i(this.A.A(), true, this.A.m_A);
					_ = this.A.m_A.Handle;
					flag = true;
					this.A.Set();
					Application.Run();
				}
				catch (Exception ex)
				{
					this.A.m_A = ex.Message;
					if (!flag)
					{
						this.A.Set();
					}
				}
				finally
				{
					if (this.A.m_A != null)
					{
						this.A.m_A.Dispose();
					}
				}
			}
		}

		private readonly global::b.c m_A;

		private readonly global::b.i m_A;

		private readonly Thread m_A;

		private i m_A;

		private int m_A;

		private volatile string m_A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		public string A
		{
			get
			{
				if (this.m_A.Length <= 0)
				{
					if (this.m_A != null)
					{
						return this.m_A.A;
					}
					return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				}
				return this.m_A;
			}
		}

		public I(global::b.c P_0, bool P_1 = true, global::b.i P_2 = null)
		{
			I A = this;
			this.m_A = P_0;
			this.m_A = P_2 ?? new global::b.i
			{
				A = false
			};
			if (!P_1)
			{
				this.m_A = new i(P_0.A(), false, this.m_A);
				return;
			}
			ManualResetEvent A2 = new ManualResetEvent(initialState: false);
			try
			{
				this.m_A = new Thread((ThreadStart)delegate
				{
					bool flag = false;
					try
					{
						A.m_A = new i(P_0.A(), true, A.m_A);
						_ = A.m_A.Handle;
						flag = true;
						A2.Set();
						Application.Run();
					}
					catch (Exception ex)
					{
						A.m_A = ex.Message;
						if (!flag)
						{
							A2.Set();
						}
					}
					finally
					{
						if (A.m_A != null)
						{
							A.m_A.Dispose();
						}
					}
				})
				{
					IsBackground = true,
					Name = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ay()
				};
				this.m_A.SetApartmentState(ApartmentState.STA);
				this.m_A.Start();
				A2.WaitOne();
			}
			finally
			{
				if (A2 != null)
				{
					((IDisposable)A2).Dispose();
				}
			}
		}

		public void a()
		{
			if (this.m_A != null && Volatile.Read(ref this.m_A) == 0)
			{
				this.m_A.A(this.m_A.A(), this.m_A);
			}
		}

		public void A(global::B.n P_0, int P_1, IEnumerable<global::B.O> P_2)
		{
			if (this.m_A != null && Volatile.Read(ref this.m_A) == 0)
			{
				this.m_A.A(new global::a.v
				{
					A = P_0.i(),
					A = (from o2 in P_2
						where o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()
						select new global::B.T
						{
							A = o2.A,
							A = o2.A,
							A = (o2.b ? o2.A : Color.Red)
						}).ToArray(),
					A = DateTime.UtcNow
				});
			}
		}

		public void B()
		{
			if (this.m_A != null)
			{
				this.m_A.A();
			}
		}

		public void b()
		{
			if (this.m_A != null)
			{
				this.m_A.a();
			}
		}

		public void C()
		{
			if (this.m_A != null && Volatile.Read(ref this.m_A) == 0)
			{
				this.m_A.A((global::a.v)null);
			}
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref this.m_A, 1) != 0 || this.m_A == null)
			{
				return;
			}
			if (this.m_A == null)
			{
				this.m_A.Dispose();
				return;
			}
			try
			{
				this.m_A.BeginInvoke((Action)delegate
				{
					this.m_A.Close();
					Application.ExitThread();
				});
			}
			catch (InvalidOperationException)
			{
			}
			if (Thread.CurrentThread != this.m_A)
			{
				this.m_A.Join();
			}
		}

		[CompilerGenerated]
		private void c()
		{
			this.m_A.Close();
			Application.ExitThread();
		}
	}
	internal sealed class i : Form
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

			public static Func<global::B.T, uint> A;

			public static Func<L, string> A;

			public static Func<L, bool> A;

			public static Func<L, bool> a;

			internal uint A(global::B.T P_0)
			{
				return P_0.A;
			}

			internal string A(L P_0)
			{
				return P_0.A;
			}

			internal bool a(L P_0)
			{
				return P_0.A;
			}

			internal bool B(L P_0)
			{
				return P_0.A;
			}
		}

		private const int m_A = 33508;

		private string m_A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		private readonly HashSet<string> m_A = new HashSet<string>();

		private long m_A;

		private readonly bool m_A;

		private volatile global::b.c m_A;

		private volatile global::a.v m_A;

		private global::B.o m_A;

		private readonly Stopwatch m_A = Stopwatch.StartNew();

		private readonly n m_A = new n();

		private global::B.n m_A;

		private string m_a;

		private List<L> m_A = new List<L>();

		private double m_A;

		private double m_a;

		private global::a.z m_A;

		private global::a.z m_a;

		private volatile global::b.i m_A;

		private Font m_A;

		private Font m_a;

		private int m_a;

		private int m_B;

		private int m_b;

		public volatile string A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

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
				string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aZ());
				if (File.Exists(text) && new FileInfo(text).Length > 262144)
				{
					File.WriteAllText(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.az());
				}
				File.AppendAllText(text, DateTime.Now.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + P_0 + Environment.NewLine);
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

		public i(global::b.c P_0, bool P_1 = true, global::b.i P_2 = null)
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
				this.m_A = new global::B.o(base.Handle, 33508, 5);
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

		private static global::b.i A(global::b.i P_0)
		{
			return new global::b.i
			{
				A = (P_0?.A ?? false),
				A = (P_0?.A ?? 12)
			};
		}

		public void A(global::b.c P_0, global::b.i P_1 = null)
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
				this.m_A.d();
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

		public void A(global::a.v P_0)
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

		private void A(j P_0)
		{
			if (this.m_A == null || this.m_a != P_0.B)
			{
				if (this.m_A != null)
				{
					this.m_A.Dispose();
				}
				this.m_A = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), P_0.B, FontStyle.Bold);
				this.m_a = P_0.B;
			}
			if (this.m_a == null || this.m_B != P_0.b)
			{
				if (this.m_a != null)
				{
					this.m_a.Dispose();
				}
				this.m_a = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), P_0.b, FontStyle.Bold);
				this.m_B = P_0.b;
			}
		}

		private void c()
		{
			global::a.v v2 = this.m_A;
			j j2 = this.m_A.A;
			if (!global::a.O.A)
			{
				C();
				if (this.m_A.a != null)
				{
					this.m_A.a.Clear(Color.Transparent);
				}
				b();
				return;
			}
			if (v2 == null)
			{
				C();
				b();
				return;
			}
			if ((!j2.A && !j2.a && !j2.B) || DateTime.UtcNow - v2.A > TimeSpan.FromSeconds(3.0))
			{
				b();
				return;
			}
			IntPtr intPtr = global::B.H.A();
			global::B.H.A(intPtr, out var num);
			if (num != v2.A.A || A(intPtr))
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
				if (this.m_A == null || this.m_a != v2.A.A)
				{
					C();
					this.m_A = new global::B.n(v2.A);
					this.m_a = v2.A.A;
					A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bb() + this.m_a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BC() + string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bc(), v2.A.Select((global::B.T P_0) => P_0.A)));
					this.m_A.Clear();
				}
				if (this.m_A != null)
				{
					this.m_A.a = this.m_A.E;
				}
				double totalMilliseconds = this.m_A.Elapsed.TotalMilliseconds;
				if (totalMilliseconds >= this.m_A)
				{
					this.m_A = this.m_A.A(this.m_a, v2.A ?? new global::B.T[0]);
					HashSet<string> hashSet = new HashSet<string>(this.m_A.Select((L P_0) => P_0.A));
					foreach (L item in this.m_A.Where((L P_0) => !this.m_A.Contains(P_0.A)))
					{
						A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BD() + item.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bd() + item.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BE() + (item.A.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ax()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BF() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Be()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bf() + item.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bc() + item.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bc() + item.B);
					}
					foreach (string item2 in this.m_A.Except(hashSet))
					{
						A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BG() + item2);
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
					this.A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
					this.m_b = 0;
					if (base.Visible)
					{
						Hide();
					}
					return;
				}
				if (j2.B && this.m_A.Any((L P_0) => P_0.A) && totalMilliseconds >= this.m_a)
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
				if (!global::b.J.A(v2.A.A, v2.A.A))
				{
					global::a.w w2 = this.m_A.F();
					g.A(this.m_A.a, this.m_A.Where((L P_0) => P_0.A), w2, j2, a2.B, a2.b, this.m_A);
					K.A(this.m_A.a, this.m_A, w2, j2, a2.B, a2.b);
					if (DateTime.UtcNow.Ticks < Interlocked.Read(ref this.m_A))
					{
						K.A(this.m_A.a, a2.B, a2.b, j2);
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
				a(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bg());
				this.A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				this.m_b = 0;
			}
			catch (Exception ex)
			{
				a(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BH() + ex.Message);
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
		private bool A(L P_0)
		{
			return !this.m_A.Contains(P_0.A);
		}
	}
}
namespace a
{
	internal interface I : IDisposable
	{
		Task<p> A(string P_0, Dictionary<string, object> P_1, string P_2, CancellationToken P_3);
	}
	internal static class i
	{
		public static string A(string P_0)
		{
			if (string.IsNullOrEmpty(P_0) || (!P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.@as()) && !P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bS())))
			{
				return null;
			}
			string text = P_0.Substring(P_0.LastIndexOf('\\') + 1);
			if (new string[7]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dz(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dA(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.da(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dB(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.db(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dC(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dc()
			}.Contains(text))
			{
				return global::b.M.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dD());
			}
			if (new string[3]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dd(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dE(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.de()
			}.Contains(text))
			{
				return global::b.M.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dF());
			}
			if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.df()))
			{
				return null;
			}
			return global::b.M.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dG());
		}
	}
}
