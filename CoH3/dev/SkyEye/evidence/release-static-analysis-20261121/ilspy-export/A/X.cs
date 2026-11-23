using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;
using a;
using b;

namespace A
{
	internal sealed class X
	{
		public float A;

		public float a;

		public float B;

		public float b;

		public float C;

		public float c;

		public float D;

		public float d;

		public bool A;

		public bool a;

		public bool B;

		public bool b;

		public bool C;

		public bool c;
	}
	internal static class x
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<PointF, float> A;

			public static Func<PointF, float> a;

			public static Func<PointF, float> B;

			public static Func<PointF, float> b;

			internal float A(PointF P_0)
			{
				return P_0.X;
			}

			internal float a(PointF P_0)
			{
				return P_0.Y;
			}

			internal float B(PointF P_0)
			{
				return P_0.X;
			}

			internal float b(PointF P_0)
			{
				return P_0.Y;
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public string A;

			internal bool A(string P_0)
			{
				return this.A.Contains(P_0);
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public X A;

			public float A;

			internal float[] A(int P_0)
			{
				return new float[3]
				{
					this.A.A + this.A.b * this.A * (float)P_0 / 64f,
					this.A.a + 0.2f,
					this.A.B + this.A.C * this.A * (float)P_0 / 64f
				};
			}
		}

		public static bool A(X P_0, float P_1, float P_2, float P_3)
		{
			P_0.c = false;
			if (!A(P_1) || !A(P_2) || !A(P_3) || P_1 < -180f || P_2 > 180f || P_1 > P_2 || P_3 < 0f || P_3 > 180f)
			{
				return false;
			}
			P_1 = Math.Min(P_1, 0f - P_3);
			P_2 = Math.Max(P_2, P_3);
			if (P_2 - P_1 < 0.1f || P_2 - P_1 > 360f)
			{
				return false;
			}
			P_0.D = P_1;
			P_0.d = P_2;
			P_0.c = true;
			return true;
		}

		private static bool A(float P_0)
		{
			if (!float.IsNaN(P_0))
			{
				return !float.IsInfinity(P_0);
			}
			return false;
		}

		public static bool A(string P_0, string P_1)
		{
			if (!p.a(P_0) && !new string[11]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bH(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bW(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DA(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Da(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DB(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Db(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bY(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DC(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dc(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DD(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dd()
			}.Any((string value) => P_0.Contains(value)) && !P_1.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dc()) && !P_1.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DD()))
			{
				return P_1.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dd());
			}
			return true;
		}

		public static float A(X P_0, global::b.c P_1)
		{
			if (!P_0.b)
			{
				if (!P_0.a)
				{
					if (!P_0.A)
					{
						if (!P_1.D)
						{
							return P_1.a;
						}
						return P_0.c;
					}
					if (!P_1.c)
					{
						return P_1.B;
					}
					return P_0.c;
				}
				return P_1.b;
			}
			return P_0.c;
		}

		public static List<y> A(IEnumerable<global::b.A> P_0, global::a.w P_1, global::b.c P_2, int P_3, int P_4)
		{
			List<y> list = new List<y>();
			if (!P_2.C)
			{
				return list;
			}
			foreach (global::b.A item in P_0)
			{
				X A = item.A;
				if (A == null || !A.C)
				{
					continue;
				}
				float A2 = x.A(A, P_2);
				double num = Math.Atan2(A.b, A.C);
				x.A(list, item.A, P_1, P_3, P_4, from num6 in Enumerable.Range(0, 65)
					select new float[3]
					{
						A.A + A.b * A2 * (float)num6 / 64f,
						A.a + 0.2f,
						A.B + A.C * A2 * (float)num6 / 64f
					});
				if (!A.C || !A.B || A.b)
				{
					continue;
				}
				float num2 = 0f;
				float num3 = 0f;
				bool flag = false;
				if (P_2.d && A.c)
				{
					num2 = A.D;
					num3 = A.d;
					flag = true;
				}
				else if (A.B && P_2.E && P_2.C > 0)
				{
					num2 = (float)(-P_2.C) / 2f;
					num3 = (float)P_2.C / 2f;
					flag = true;
				}
				if (flag)
				{
					List<float[]> list2 = new List<float[]>();
					list2.Add(new float[3]
					{
						A.A,
						A.a + 0.2f,
						A.B
					});
					for (int num4 = 0; num4 <= 64; num4++)
					{
						double num5 = num + (double)(num2 + (num3 - num2) * (float)num4 / 64f) * Math.PI / 180.0;
						list2.Add(new float[3]
						{
							A.A + (float)Math.Sin(num5) * A.c,
							A.a + 0.2f,
							A.B + (float)Math.Cos(num5) * A.c
						});
					}
					list2.Add(new float[3]
					{
						A.A,
						A.a + 0.2f,
						A.B
					});
					x.A(list, item.A, P_1, P_3, P_4, list2);
				}
			}
			return list;
		}

		private static void A(List<y> P_0, Color P_1, global::a.w P_2, int P_3, int P_4, IEnumerable<float[]> P_5)
		{
			List<PointF> list = new List<PointF>();
			foreach (float[] item2 in P_5)
			{
				if (P_2.A(item2[0], item2[1], item2[2], P_3, P_4, out var item))
				{
					list.Add(item);
				}
				else
				{
					A(P_0, P_1, list);
				}
			}
			A(P_0, P_1, list);
		}

		private static void A(List<y> P_0, Color P_1, List<PointF> P_2)
		{
			if (P_2.Count >= 2)
			{
				float num = P_2.Min((PointF pointF) => pointF.X);
				float num2 = P_2.Min((PointF pointF) => pointF.Y);
				float num3 = P_2.Max((PointF pointF) => pointF.X);
				float num4 = P_2.Max((PointF pointF) => pointF.Y);
				P_0.Add(new y
				{
					A = P_1,
					A = P_2.ToArray(),
					A = Rectangle.FromLTRB((int)num - 16, (int)num2 - 16, (int)num3 + 17, (int)num4 + 17)
				});
			}
			P_2.Clear();
		}

		public static void A(Graphics P_0, IEnumerable<y> P_1, decimal P_2)
		{
			P_0.SmoothingMode = SmoothingMode.AntiAlias;
			foreach (y item in P_1)
			{
				using Pen pen = new Pen(item.A, (item.A > 0f) ? item.A : ((float)P_2));
				if (item.A)
				{
					if (item.a > 0f && item.B > 0f)
					{
						pen.DashPattern = global::B.F.A(item.a, item.B, pen.Width);
					}
					else
					{
						pen.DashStyle = DashStyle.Dash;
					}
				}
				P_0.DrawLines(pen, item.A);
			}
		}
	}
}
namespace a
{
	internal sealed class X : Form
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

			public static Func<global::B.O, IEnumerable<x>> A;

			public static Func<x, long> A;

			public static Func<IGrouping<long, x>, x> A;

			public static Func<global::B.O, IEnumerable<global::A.T>> A;

			public static Func<global::B.O, bool> A;

			public static Func<global::B.O, uint> A;

			internal IEnumerable<x> A(global::B.O P_0)
			{
				return P_0.A;
			}

			internal long A(x P_0)
			{
				return P_0.A;
			}

			internal x A(IGrouping<long, x> P_0)
			{
				return P_0.First();
			}

			internal IEnumerable<global::A.T> a(global::B.O P_0)
			{
				return P_0.A;
			}

			internal bool B(global::B.O P_0)
			{
				return !P_0.B;
			}

			internal uint b(global::B.O P_0)
			{
				return P_0.A;
			}
		}

		private readonly global::B.J m_A;

		private readonly Timer m_A = new Timer();

		private readonly Font m_A = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), 10f, FontStyle.Bold);

		private readonly global::A.S m_A = new global::A.S();

		private List<global::A.T> m_A = new List<global::A.T>();

		private global::B.n m_A;

		private List<x> m_A = new List<x>();

		private w m_A;

		private int m_A;

		private DateTime m_A;

		public string A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

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

		public X(global::B.J P_0, bool P_1 = true)
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
			this.m_A.Interval = 33;
			Timer timer = this.m_A;
			EventHandler value = delegate
			{
				a();
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

		public void A(global::B.n P_0, int P_1, IEnumerable<global::B.O> P_2)
		{
			this.m_A = P_0;
			this.m_A = P_1;
			this.m_A = (from x2 in P_2.SelectMany((global::B.O o2) => o2.A)
				group x2 by x2.A into source
				select source.First()).ToList();
			foreach (global::B.O item in P_2)
			{
				foreach (global::A.T item2 in item.A)
				{
					item2.A = (item.b ? item.A : Color.Red);
				}
			}
			this.m_A = this.m_A.A(P_0.C, P_2.SelectMany((global::B.O o2) => o2.A), DateTime.UtcNow, from o2 in P_2
				where !o2.B
				select o2.A);
			this.m_A = DateTime.UtcNow;
		}

		public void A()
		{
			this.m_A = null;
			this.m_A = null;
			this.A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.A();
			Hide();
		}

		private void a()
		{
			if (!O.A || this.m_A == null || ((!this.m_A.a || this.m_A.Count == 0) && (!this.m_A.B || this.m_A.Count == 0)) || DateTime.UtcNow - this.m_A > TimeSpan.FromSeconds(3.0))
			{
				Hide();
				return;
			}
			IntPtr intPtr = global::B.H.A();
			global::B.H.A(intPtr, out var num);
			if (num != this.m_A || A(intPtr))
			{
				Hide();
				return;
			}
			Point point = default(Point);
			if (!A(intPtr, out var a2) || !A(intPtr, ref point))
			{
				Hide();
				return;
			}
			if (global::b.J.A(this.m_A, this.m_A.C))
			{
				Hide();
				return;
			}
			try
			{
				this.m_A = this.m_A.F();
				this.A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				base.Bounds = new Rectangle(point.X, point.Y, a2.B, a2.b);
				if (!base.Visible)
				{
					Show();
				}
				Invalidate();
			}
			catch (Exception ex)
			{
				this.A = ex.Message;
				Hide();
			}
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			base.OnPaint(P_0);
			if (!O.A || this.m_A == null)
			{
				return;
			}
			P_0.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			foreach (x item in this.m_A)
			{
				if (!this.m_A.a)
				{
					break;
				}
				if (!this.m_A.A(item.A, item.a + 0.1f, item.B, base.ClientSize.Width, base.ClientSize.Height, out var pointF))
				{
					continue;
				}
				Color color = ((item.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()) ? this.m_A.A : ((item.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba() || item.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl()) ? this.m_A.a : Color.Silver));
				using (Pen pen = new Pen(Color.Black, 5f))
				{
					using Pen pen2 = new Pen(color, 3f);
					P_0.Graphics.DrawEllipse(pen, pointF.X - 8f, pointF.Y - 8f, 16f, 16f);
					P_0.Graphics.DrawEllipse(pen2, pointF.X - 8f, pointF.Y - 8f, 16f, 16f);
					P_0.Graphics.DrawLine(pen2, pointF.X - 4f, pointF.Y, pointF.X + 4f, pointF.Y);
					P_0.Graphics.DrawLine(pen2, pointF.X, pointF.Y - 4f, pointF.X, pointF.Y + 4f);
				}
				string text = ((item.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ew() : ((item.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DL()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DL() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EW())) + (item.A ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ex() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EX());
				TextRenderer.DrawText(P_0.Graphics, text, this.m_A, new Point((int)pointF.X + 11, (int)pointF.Y - 9), color, Color.Black, TextFormatFlags.NoPadding);
			}
			if (!this.m_A.B)
			{
				return;
			}
			foreach (global::A.T item2 in this.m_A)
			{
				if (!this.m_A.A(item2.A, item2.a + 0.2f, item2.B, base.ClientSize.Width, base.ClientSize.Height, out var pointF2))
				{
					continue;
				}
				float num = (float)this.m_A.A / 2f;
				float num2 = num * 30f / 22f;
				using (Pen pen3 = new Pen(Color.Black, 5f))
				{
					using Pen pen4 = new Pen(item2.A, 3f);
					P_0.Graphics.DrawEllipse(pen3, pointF2.X - num, pointF2.Y - num, num * 2f, num * 2f);
					P_0.Graphics.DrawEllipse(pen4, pointF2.X - num, pointF2.Y - num, num * 2f, num * 2f);
					P_0.Graphics.DrawLine(pen4, pointF2.X - num2, pointF2.Y, pointF2.X + num2, pointF2.Y);
					P_0.Graphics.DrawLine(pen4, pointF2.X, pointF2.Y - num2, pointF2.X, pointF2.Y + num2);
				}
				TextRenderer.DrawText(P_0.Graphics, item2.B, this.m_A, new Point((int)(pointF2.X + num + 5f), (int)pointF2.Y - 10), item2.A, Color.Black, TextFormatFlags.NoPadding);
			}
		}

		protected override void Dispose(bool P_0)
		{
			if (P_0)
			{
				this.m_A.Dispose();
				this.m_A.Dispose();
			}
			base.Dispose(P_0);
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			a();
		}
	}
	internal sealed class x
	{
		public string A;

		public string a;

		public float A;

		public float a;

		public float B;

		public long A;

		public bool A;
	}
}
