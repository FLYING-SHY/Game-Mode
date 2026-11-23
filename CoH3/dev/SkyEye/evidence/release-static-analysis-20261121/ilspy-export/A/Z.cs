using System;
using System.Collections.Generic;
using System.Drawing;
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
	internal sealed class Z
	{
		public global::B.O A;

		public override string ToString()
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA() + (A.A + 1) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dk() + A.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dk() + u.a(A);
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

		private struct a
		{
			public int A;

			public int a;
		}

		private readonly Timer m_A = new Timer();

		private readonly global::B.J m_A;

		private string[] m_A = new string[0];

		private Color[] m_A = new Color[0];

		private global::B.h[][] m_A = new global::B.h[0][];

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

		public z(global::B.J P_0, bool P_1 = true)
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
			Font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), this.m_A.B, FontStyle.Bold, GraphicsUnit.Point);
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

		public void A(int P_0, IEnumerable<global::B.O> P_1)
		{
			this.m_A = P_0;
			global::B.O[] array = P_1.ToArray();
			this.m_A = global::B.k.B(array, this.m_A);
			this.m_A = global::B.k.A(array, this.m_A);
			this.m_A = global::B.k.a(array, this.m_A);
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
			if (!global::a.O.A || !this.m_A.D || this.m_A.Length == 0 || DateTime.UtcNow - this.m_A > TimeSpan.FromSeconds(3.0))
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
				num3 = global::B.i.A(graphics, this.m_A, Font, this.m_A.B, this.m_A.b, num2, false, this.m_A.a, this.m_A, this.m_A);
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
			global::B.i.A(P_0.Graphics, this.m_A, Font, this.m_A.B, this.m_A.b, base.ClientSize.Width, true, this.m_A.a, this.m_A, this.m_A);
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
}
namespace a
{
	internal static class Z
	{
		public static PointF[] A(PointF P_0, float P_1)
		{
			List<PointF> list = new List<PointF>();
			float num = P_1 / 2f;
			double[] array = new double[3]
			{
				Math.PI / 2.0,
				Math.PI / 6.0,
				Math.PI * 5.0 / 6.0
			};
			foreach (double num3 in array)
			{
				float num4 = (float)Math.Cos(num3) * num;
				float num5 = (float)Math.Sin(num3) * num;
				list.Add(new PointF(P_0.X - num4, P_0.Y - num5));
				list.Add(new PointF(P_0.X + num4, P_0.Y + num5));
			}
			return list.ToArray();
		}

		public static void A(Graphics P_0, IEnumerable<global::A.T> P_1, z P_2, global::B.C P_3, int P_4, int P_5)
		{
			if (!P_3.b || P_1 == null)
			{
				return;
			}
			foreach (global::A.T item in P_1)
			{
				if (!P_2.A(item.A, item.B, P_4, P_5, out var pointF))
				{
					continue;
				}
				PointF[] array = A(pointF, P_3.c);
				using Pen pen = new Pen(Color.Black, 3.5f);
				using Pen pen2 = new Pen(item.A, 1.5f);
				for (int num = 0; num < array.Length; num += 2)
				{
					P_0.DrawLine(pen, array[num], array[num + 1]);
				}
				for (int num2 = 0; num2 < array.Length; num2 += 2)
				{
					P_0.DrawLine(pen2, array[num2], array[num2 + 1]);
				}
			}
		}
	}
	internal sealed class z
	{
		public bool A;

		public y A;

		public float A;

		public float a;

		public float B;

		public float b;

		public float C;

		public float c;

		public float D;

		public float d;

		public float E;

		public bool A
		{
			get
			{
				if (D >= 8f && d >= 8f && B >= 10f && b >= 10f && C > 0f)
				{
					return c > 0f;
				}
				return false;
			}
		}

		public PointF A(float P_0, float P_1, int P_2, int P_3)
		{
			if (this.A)
			{
				PointF pointF = this.A.a(P_0, P_1);
				return new PointF(pointF.X * (float)P_2 / C, pointF.Y * (float)P_3 / c);
			}
			double num = (double)E * Math.PI / 180.0;
			double num2 = P_0 / D * B;
			double num3 = (0f - P_1) / d * b;
			return new PointF((float)(((double)this.A + num2 * Math.Cos(num) - num3 * Math.Sin(num)) * (double)P_2 / (double)C), (float)(((double)a + num2 * Math.Sin(num) + num3 * Math.Cos(num)) * (double)P_3 / (double)c));
		}

		public bool A(float P_0, float P_1, int P_2, int P_3, out PointF P_4)
		{
			P_4 = default(PointF);
			if (!this.A || !global::b.b.A(P_0, 0f, P_1) || Math.Abs(P_0) > D / 2f || Math.Abs(P_1) > d / 2f)
			{
				return false;
			}
			P_4 = A(P_0, P_1, P_2, P_3);
			return true;
		}

		public PointF[] A(int P_0, int P_1)
		{
			return new PointF[4]
			{
				A((0f - D) / 2f, (0f - d) / 2f, P_0, P_1),
				A(D / 2f, (0f - d) / 2f, P_0, P_1),
				A(D / 2f, d / 2f, P_0, P_1),
				A((0f - D) / 2f, d / 2f, P_0, P_1)
			};
		}
	}
}
