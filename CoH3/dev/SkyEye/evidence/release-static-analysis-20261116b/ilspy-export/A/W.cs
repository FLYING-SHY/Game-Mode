using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using A;
using B;
using b;

namespace A
{
	internal static class W
	{
		private static readonly object m_A = new object();

		private static bool m_A;

		private static string m_A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		private static string a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		internal static string A(string P_0)
		{
			P_0 = (P_0 ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()).Trim();
			if (P_0.Length == 0 || P_0.Length > 64)
			{
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
			}
			string text = P_0;
			for (int num = 0; num < text.Length; num++)
			{
				if (char.IsControl(text[num]))
				{
					return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
				}
			}
			string text2 = P_0.ToLowerInvariant().Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()).Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ci(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D())
				.Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cj(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
			if (text2.Length != 0)
			{
				if (text2 == null)
				{
					goto IL_0175;
				}
				switch (text2.Length)
				{
				case 13:
					break;
				case 15:
					goto IL_00f3;
				case 18:
					goto IL_0129;
				case 7:
					goto IL_013b;
				case 4:
					goto IL_014a;
				case 2:
					goto IL_0159;
				case 3:
					goto IL_0168;
				default:
					goto IL_0175;
				}
				char c2 = text2[0];
				if (c2 != 'd')
				{
					if (c2 != 'n' || !(text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cS()))
					{
						goto IL_0175;
					}
				}
				else if (!(text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cr()))
				{
					goto IL_0175;
				}
			}
			goto IL_01c4;
			IL_014a:
			if (!(text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ct()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_0168:
			if (!(text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cu()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_00f3:
			if (!(text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cR()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_0129:
			if (!(text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cs()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_0159:
			if (!(text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cU()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_0175:
			bool flag = true;
			bool flag2 = true;
			text = text2;
			foreach (char c3 in text)
			{
				flag = flag && c3 == '0';
				flag2 = flag2 && c3 == 'f';
			}
			if (flag || flag2)
			{
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
			}
			return P_0;
			IL_013b:
			if (!(text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cT()))
			{
				goto IL_0175;
			}
			goto IL_01c4;
			IL_01c4:
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
		}

		private static void A()
		{
			lock (W.m_A)
			{
				if (W.m_A)
				{
					return;
				}
				W.m_A = true;
				string[] array = new string[2]
				{
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cV(),
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cv()
				};
				foreach (string text in array)
				{
					try
					{
						EnumerationOptions options = new EnumerationOptions
						{
							Timeout = TimeSpan.FromSeconds(2.0),
							ReturnImmediately = false
						};
						using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cW(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cw() + text, options);
						using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
						foreach (ManagementBaseObject item in managementObjectCollection)
						{
							using (item)
							{
								string text2 = A(Convert.ToString(item[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cX()], CultureInfo.InvariantCulture));
								if (text2.Length == 0)
								{
									continue;
								}
								W.m_A = text2;
								a = ((text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cV()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cY() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cx());
								return;
							}
						}
					}
					catch
					{
					}
				}
			}
		}

		public static void A(Dictionary<string, object> P_0)
		{
			A();
			if (W.m_A.Length > 0)
			{
				P_0[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cy()] = W.m_A;
				P_0[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cZ()] = a;
			}
		}
	}
	internal sealed class w
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
}
namespace a
{
	internal sealed class W : Form
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

			public static Func<global::B.N, IEnumerable<w>> A;

			public static Func<w, long> A;

			public static Func<IGrouping<long, w>, w> A;

			public static Func<global::B.N, IEnumerable<global::A.s>> A;

			public static Func<global::B.N, bool> A;

			public static Func<global::B.N, uint> A;

			internal IEnumerable<w> A(global::B.N P_0)
			{
				return P_0.A;
			}

			internal long A(w P_0)
			{
				return P_0.A;
			}

			internal w A(IGrouping<long, w> P_0)
			{
				return P_0.First();
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
		}

		private readonly global::B.I m_A;

		private readonly Timer m_A = new Timer();

		private readonly Font m_A = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), 10f, FontStyle.Bold);

		private readonly global::A.r m_A = new global::A.r();

		private List<global::A.s> m_A = new List<global::A.s>();

		private global::B.m m_A;

		private List<w> m_A = new List<w>();

		private v m_A;

		private int m_A;

		private DateTime m_A;

		public string A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

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

		public W(global::B.I P_0, bool P_1 = true)
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

		public void A(global::B.m P_0, int P_1, IEnumerable<global::B.N> P_2)
		{
			this.m_A = P_0;
			this.m_A = P_1;
			this.m_A = (from w2 in P_2.SelectMany((global::B.N n2) => n2.A)
				group w2 by w2.A into source
				select source.First()).ToList();
			foreach (global::B.N item in P_2)
			{
				foreach (global::A.s item2 in item.A)
				{
					item2.A = (item.b ? item.A : Color.Red);
				}
			}
			this.m_A = this.m_A.A(P_0.b, P_2.SelectMany((global::B.N n2) => n2.A), DateTime.UtcNow, from n2 in P_2
				where !n2.B
				select n2.A);
			this.m_A = DateTime.UtcNow;
		}

		public void A()
		{
			this.m_A = null;
			this.m_A = null;
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.A();
			Hide();
		}

		private void a()
		{
			if (!N.A || this.m_A == null || ((!this.m_A.a || this.m_A.Count == 0) && (!this.m_A.B || this.m_A.Count == 0)) || DateTime.UtcNow - this.m_A > TimeSpan.FromSeconds(3.0))
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
			if (!A(intPtr, out var a2) || !A(intPtr, ref point))
			{
				Hide();
				return;
			}
			if (global::b.I.A(this.m_A, this.m_A.b))
			{
				Hide();
				return;
			}
			try
			{
				this.m_A = this.m_A.e();
				this.A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
			if (!N.A || this.m_A == null)
			{
				return;
			}
			P_0.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			foreach (w item in this.m_A)
			{
				if (!this.m_A.a)
				{
					break;
				}
				if (!this.m_A.A(item.A, item.a + 0.1f, item.B, base.ClientSize.Width, base.ClientSize.Height, out var pointF))
				{
					continue;
				}
				Color color = ((item.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()) ? this.m_A.A : ((item.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA() || item.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL()) ? this.m_A.a : Color.Silver));
				using (Pen pen = new Pen(Color.Black, 5f))
				{
					using Pen pen2 = new Pen(color, 3f);
					P_0.Graphics.DrawEllipse(pen, pointF.X - 8f, pointF.Y - 8f, 16f, 16f);
					P_0.Graphics.DrawEllipse(pen2, pointF.X - 8f, pointF.Y - 8f, 16f, 16f);
					P_0.Graphics.DrawLine(pen2, pointF.X - 4f, pointF.Y, pointF.X + 4f, pointF.Y);
					P_0.Graphics.DrawLine(pen2, pointF.X, pointF.Y - 4f, pointF.X, pointF.Y + 4f);
				}
				string text = ((item.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ET() : ((item.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dk()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dk() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Es())) + (item.A ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EU() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Et());
				TextRenderer.DrawText(P_0.Graphics, text, this.m_A, new Point((int)pointF.X + 11, (int)pointF.Y - 9), color, Color.Black, TextFormatFlags.NoPadding);
			}
			if (!this.m_A.B)
			{
				return;
			}
			foreach (global::A.s item2 in this.m_A)
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
	internal sealed class w
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
