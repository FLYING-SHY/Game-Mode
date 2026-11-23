using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using a;
using b;

namespace B
{
	internal static class B
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

		public static List<b> A(IEnumerable<global::b.A> P_0, global::a.z P_1, C P_2, int P_3, int P_4)
		{
			List<b> list = new List<b>();
			if (!P_2.a || !P_2.B || P_1 == null || !P_1.A || P_3 <= 0 || P_4 <= 0)
			{
				return list;
			}
			foreach (global::b.A item in P_0.Where((global::b.A a2) => !a2.a))
			{
				e e2 = item.A;
				if (e2 != null && item.A && global::b.b.A(item.A, 0f, item.B) && global::b.b.A(e2.A, 0f, e2.B))
				{
					PointF pointF;
					bool flag = P_1.A(e2.A, e2.B, P_3, P_4, out pointF);
					list.Add(new b
					{
						A = P_1.A(item.A, item.B, P_3, P_4),
						a = (flag ? pointF : P_1.A(e2.A, e2.B, P_3, P_4)),
						A = item.A,
						A = item.A,
						A = flag
					});
				}
			}
			return list;
		}

		public static void A(Graphics P_0, IEnumerable<b> P_1, C P_2)
		{
			HashSet<Tuple<uint, int, int, int>> hashSet = new HashSet<Tuple<uint, int, int, int>>();
			float num = (float)P_2.C / 2f;
			foreach (b item in P_1)
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
	internal sealed class b
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
	internal sealed class B : IDisposable
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.O, bool> A;

			public static Func<global::B.O, bool> a;

			public static Func<global::B.O, bool> B;

			public static Func<global::B.O, bool> b;

			public static Func<global::B.O, bool> C;

			public static Func<global::B.O, global::B.T> A;

			public static Func<global::B.O, global::B.T> a;

			internal bool A(global::B.O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal bool a(global::B.O P_0)
			{
				return P_0.C;
			}

			internal bool B(global::B.O P_0)
			{
				return !P_0.C;
			}

			internal bool b(global::B.O P_0)
			{
				if (P_0.A.Count > 0)
				{
					return !P_0.b;
				}
				return false;
			}

			internal bool C(global::B.O P_0)
			{
				if (!(P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()) && !(P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba()))
				{
					return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl();
				}
				return true;
			}

			internal global::B.T c(global::B.O P_0)
			{
				return new global::B.T
				{
					A = P_0.A,
					A = P_0.A,
					A = P_0.a,
					A = (P_0.b ? P_0.A : Color.Silver)
				};
			}

			internal global::B.T D(global::B.O P_0)
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
			public c A;

			public B A;

			public ManualResetEvent A;

			internal void A()
			{
				bool flag = false;
				try
				{
					this.A.m_A = new E(this.A.A());
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

		private readonly c m_A;

		private readonly Thread m_A;

		private E m_A;

		private volatile string m_A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

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
					return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ks();
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
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KT();
			}
		}

		public B(c P_0, bool P_1 = true)
		{
			B A = this;
			this.m_A = P_0;
			if (!P_1)
			{
				this.m_A = new E(P_0, false);
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
						A.m_A = new E(P_0.A());
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
					Name = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kt()
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

		public void A(global::B.n P_0, int P_1, IEnumerable<global::B.O> P_2)
		{
			if (this.m_A == null || Volatile.Read(ref this.m_A) != 0)
			{
				return;
			}
			global::B.O[] source = P_2.Where((global::B.O o) => o.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()).ToArray();
			List<d> list = new List<d>();
			foreach (global::B.O item in source.Where((global::B.O o) => o.C))
			{
				foreach (d item2 in item.A)
				{
					d d2 = new d
					{
						A = item2.A,
						a = item2.a,
						A = item2.A,
						A = item2.A,
						A = item.b,
						A = item.A
					};
					foreach (C item3 in item2.A)
					{
						d2.A.Add(new C
						{
							A = item3.A,
							a = item3.a,
							A = item3.A,
							a = item3.a
						});
					}
					list.Add(d2);
				}
			}
			string text = (source.Any((global::B.O o) => !o.C) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ku() : (source.Any((global::B.O o) => o.A.Count > 0 && !o.b) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KU() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()));
			this.m_A.A(new global::a.v
			{
				A = P_0.i(),
				A = list.ToArray(),
				a = (from o in P_2
					where o.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA() || o.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba() || o.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl()
					select new global::B.T
					{
						A = o.A,
						A = o.A,
						A = o.a,
						A = (o.b ? o.A : Color.Silver)
					}).ToArray(),
				A = source.Select((global::B.O o) => new global::B.T
				{
					A = o.A,
					A = o.A,
					A = (o.b ? o.A : Color.Red)
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
				this.m_A.A((global::a.v)null);
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
	internal static class b
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
			if (global::a.i.A(P_0) == null)
			{
				if (!string.IsNullOrEmpty(P_0))
				{
					if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ik()) || !P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bS()))
					{
						if (P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bS(), StringComparison.Ordinal) && !P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jq()) && !P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jR()))
						{
							return !P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jr());
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
			return P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bs());
		}

		public static bool A(float P_0, float P_1, float P_2)
		{
			if (!float.IsNaN(P_0) && !float.IsNaN(P_1) && !float.IsNaN(P_2) && !float.IsInfinity(P_0) && !float.IsInfinity(P_1) && !float.IsInfinity(P_2) && Math.Abs(P_0) < 20000f && Math.Abs(P_2) < 20000f)
			{
				return Math.Abs(P_1) < 2000f;
			}
			return false;
		}

		public static global::b.A A(d P_0, List<float[]> P_1)
		{
			if (P_1.Count == 0 || !P_0.A || !A(P_0.A))
			{
				return null;
			}
			return new global::b.A
			{
				A = P_0.A,
				a = P_0.a,
				A = P_0.A,
				a = (global::a.i.A(P_0.A) ?? k.A(P_0.A)),
				a = (global::a.i.A(P_0.A) != null),
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
