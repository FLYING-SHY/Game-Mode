using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
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

			public static Func<Z, bool> A;

			internal bool A(Z P_0)
			{
				return !P_0.a;
			}
		}

		public static List<a> A(IEnumerable<Z> P_0, global::a.y P_1, B P_2, int P_3, int P_4)
		{
			List<a> list = new List<a>();
			if (!P_2.a || !P_2.B || P_1 == null || !P_1.A || P_3 <= 0 || P_4 <= 0)
			{
				return list;
			}
			foreach (Z item in P_0.Where((Z z2) => !z2.a))
			{
				d d2 = item.A;
				if (d2 != null && item.A && global::b.a.A(item.A, 0f, item.B) && global::b.a.A(d2.A, 0f, d2.B))
				{
					PointF pointF;
					bool flag = P_1.A(d2.A, d2.B, P_3, P_4, out pointF);
					list.Add(new a
					{
						A = P_1.A(item.A, item.B, P_3, P_4),
						a = (flag ? pointF : P_1.A(d2.A, d2.B, P_3, P_4)),
						A = item.A,
						A = item.A,
						A = flag
					});
				}
			}
			return list;
		}

		public static void A(Graphics P_0, IEnumerable<a> P_1, B P_2)
		{
			HashSet<Tuple<uint, int, int, int>> hashSet = new HashSet<Tuple<uint, int, int, int>>();
			float num = (float)P_2.C / 2f;
			foreach (a item in P_1)
			{
				using Pen pen = new Pen(item.A, (float)P_2.A);
				pen.DashStyle = DashStyle.Solid;
				P_0.DrawLine(pen, item.A, item.a);
				if (item.A && hashSet.Add(Tuple.Create(item.A, item.A.ToArgb(), (int)Math.Round(item.a.X), (int)Math.Round(item.a.Y))))
				{
					P_0.DrawLine(pen, item.a.X - num, item.a.Y - num, item.a.X + num, item.a.Y + num);
					P_0.DrawLine(pen, item.a.X - num, item.a.Y + num, item.a.X + num, item.a.Y - num);
				}
			}
		}
	}
	internal sealed class a
	{
		public PointF A;

		public PointF a;

		public Color A;

		public uint A;

		public bool A;
	}
}
namespace b
{
	internal sealed class A : IDisposable
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.N, bool> A;

			public static Func<global::B.N, bool> a;

			public static Func<global::B.N, bool> B;

			public static Func<global::B.N, bool> b;

			public static Func<global::B.N, bool> C;

			public static Func<global::B.N, global::B.S> A;

			public static Func<global::B.N, global::B.S> a;

			internal bool A(global::B.N P_0)
			{
				return P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz();
			}

			internal bool a(global::B.N P_0)
			{
				return P_0.C;
			}

			internal bool B(global::B.N P_0)
			{
				return !P_0.C;
			}

			internal bool b(global::B.N P_0)
			{
				if (P_0.A.Count > 0)
				{
					return !P_0.b;
				}
				return false;
			}

			internal bool C(global::B.N P_0)
			{
				if (!(P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()) && !(P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA()))
				{
					return P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL();
				}
				return true;
			}

			internal global::B.S c(global::B.N P_0)
			{
				return new global::B.S
				{
					A = P_0.A,
					A = P_0.A,
					A = P_0.a,
					A = (P_0.b ? P_0.A : Color.Silver)
				};
			}

			internal global::B.S D(global::B.N P_0)
			{
				return new global::B.S
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
			public b A;

			public global::b.A A;

			public ManualResetEvent A;

			internal void A()
			{
				bool flag = false;
				try
				{
					this.A.m_A = new D(this.A.A());
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

		private readonly b m_A;

		private readonly Thread m_A;

		private D m_A;

		private volatile string m_A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		private int m_A;

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
					return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KO();
				}
				return this.m_A;
			}
		}

		public string a
		{
			get
			{
				if (this.m_A != null)
				{
					return this.m_A.a;
				}
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ko();
			}
		}

		public A(b P_0, bool P_1 = true)
		{
			global::b.A A = this;
			this.m_A = P_0;
			if (!P_1)
			{
				this.m_A = new D(P_0, false);
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
						A.m_A = new D(P_0.A());
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
					Name = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KP()
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

		public void B()
		{
			if (this.m_A != null && Volatile.Read(ref this.m_A) == 0)
			{
				this.m_A.A(this.m_A.A());
			}
		}

		public void A(global::B.m P_0, int P_1, IEnumerable<global::B.N> P_2)
		{
			if (this.m_A == null || Volatile.Read(ref this.m_A) != 0)
			{
				return;
			}
			global::B.N[] source = P_2.Where((global::B.N n) => n.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()).ToArray();
			List<c> list = new List<c>();
			foreach (global::B.N item in source.Where((global::B.N n) => n.C))
			{
				foreach (c item2 in item.A)
				{
					c c2 = new c
					{
						A = item2.A,
						a = item2.a,
						A = item2.A,
						A = item2.A,
						A = item.b,
						A = item.A
					};
					foreach (B item3 in item2.A)
					{
						c2.A.Add(new B
						{
							A = item3.A,
							a = item3.a,
							A = item3.A,
							a = item3.a
						});
					}
					list.Add(c2);
				}
			}
			string text = (source.Any((global::B.N n) => !n.C) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KQ() : (source.Any((global::B.N n) => n.A.Count > 0 && !n.b) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kp() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()));
			this.m_A.A(new global::a.u
			{
				A = P_0.I(),
				A = list.ToArray(),
				a = (from n in P_2
					where n.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz() || n.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA() || n.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL()
					select new global::B.S
					{
						A = n.A,
						A = n.A,
						A = n.a,
						A = (n.b ? n.A : Color.Silver)
					}).ToArray(),
				A = source.Select((global::B.N n) => new global::B.S
				{
					A = n.A,
					A = n.A,
					A = (n.b ? n.A : Color.Red)
				}).ToArray(),
				A = DateTime.UtcNow,
				A = text
			});
		}

		public void b()
		{
			if (this.m_A != null)
			{
				this.m_A.B();
			}
		}

		public void C()
		{
			if (this.m_A != null)
			{
				this.m_A.c();
			}
		}

		public void c()
		{
			if (this.m_A != null && Volatile.Read(ref this.m_A) == 0)
			{
				this.m_A.A((global::a.u)null);
			}
		}

		public void D()
		{
			Dispose();
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
		private void d()
		{
			this.m_A.Close();
			Application.ExitThread();
		}
	}
	internal static class a
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<float[], double> A;

			public static Func<float[], double> a;

			public static Func<float[], double> B;

			internal double A(float[] P_0)
			{
				return P_0[0];
			}

			internal double a(float[] P_0)
			{
				return P_0[1];
			}

			internal double B(float[] P_0)
			{
				return P_0[2];
			}
		}

		public static bool A(string P_0)
		{
			if (global::a.h.A(P_0) == null)
			{
				if (!string.IsNullOrEmpty(P_0))
				{
					if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iG()) || !P_0.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.br()))
					{
						if (P_0.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.br(), StringComparison.Ordinal) && !P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jM()) && !P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jm()))
						{
							return !P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jN());
						}
						return false;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public static bool a(string P_0)
		{
			return P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bS());
		}

		public static bool A(float P_0, float P_1, float P_2)
		{
			if (!float.IsNaN(P_0) && !float.IsNaN(P_1) && !float.IsNaN(P_2) && !float.IsInfinity(P_0) && !float.IsInfinity(P_1) && !float.IsInfinity(P_2) && Math.Abs(P_0) < 20000f && Math.Abs(P_2) < 20000f)
			{
				return Math.Abs(P_1) < 2000f;
			}
			return false;
		}

		public static global::B.Z A(c P_0, List<float[]> P_1)
		{
			if (P_1.Count == 0 || !P_0.A || !A(P_0.A))
			{
				return null;
			}
			return new global::B.Z
			{
				A = P_0.A,
				a = P_0.a,
				A = P_0.A,
				a = (global::a.h.A(P_0.A) ?? j.A(P_0.A)),
				a = (global::a.h.A(P_0.A) != null),
				A = P_0.A,
				A = a(P_0.A),
				A = P_0.A,
				A = (float)((IEnumerable<float[]>)P_1).Average((Func<float[], double>)((float[] array) => array[0])),
				a = (float)((IEnumerable<float[]>)P_1).Average((Func<float[], double>)((float[] array) => array[1])),
				B = (float)((IEnumerable<float[]>)P_1).Average((Func<float[], double>)((float[] array) => array[2]))
			};
		}
	}
}
