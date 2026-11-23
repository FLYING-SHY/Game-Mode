using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;
using Microsoft.Win32;
using a;
using b;

namespace A
{
	internal static class K
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<L, bool> A;

			public static Func<L, bool> a;

			internal bool A(L P_0)
			{
				return !P_0.A;
			}

			internal bool a(L P_0)
			{
				if (!P_0.A)
				{
					return !P_0.a;
				}
				return false;
			}
		}

		private static readonly object m_A = new object();

		private static string m_A;

		private static DateTime m_A;

		public static void A(string P_0)
		{
			lock (K.m_A)
			{
				try
				{
					if (!(K.m_A == P_0) || !(DateTime.UtcNow - K.m_A < TimeSpan.FromSeconds(10.0)))
					{
						K.m_A = P_0;
						K.m_A = DateTime.UtcNow;
						string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bp());
						if (File.Exists(text) && new FileInfo(text).Length > 262144)
						{
							File.WriteAllText(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
						}
						File.AppendAllText(text, DateTime.Now.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + P_0 + Environment.NewLine);
					}
				}
				catch
				{
				}
			}
		}

		private static void A(List<y> P_0, PointF P_1, PointF P_2, Color P_3, float P_4)
		{
			int num = (int)Math.Ceiling(P_4 + 4f);
			P_0.Add(new y
			{
				A = new PointF[2] { P_1, P_2 },
				A = P_3,
				A = P_4,
				A = Rectangle.FromLTRB((int)Math.Floor(Math.Min(P_1.X, P_2.X)) - num, (int)Math.Floor(Math.Min(P_1.Y, P_2.Y)) - num, (int)Math.Ceiling(Math.Max(P_1.X, P_2.X)) + num, (int)Math.Ceiling(Math.Max(P_1.Y, P_2.Y)) + num)
			});
		}

		private static void A(List<global::a.u> P_0, PointF P_1, string P_2, Color P_3, global::a.W P_4)
		{
			Bitmap bitmap = P_4.a(new global::b.A
			{
				a = P_2,
				A = P_3
			});
			P_0.Add(new global::a.u
			{
				A = bitmap,
				A = new Rectangle((int)(P_1.X - (float)bitmap.Width / 2f), (int)(P_1.Y - (float)bitmap.Height / 2f), bitmap.Width, bitmap.Height)
			});
		}

		private static void A(global::a.W P_0, j P_1)
		{
			P_0.A(new global::b.c
			{
				d = P_1.B
			});
		}

		public static int A(List<global::a.u> P_0, List<y> P_1, IEnumerable<L> P_2, global::a.w P_3, j P_4, int P_5, int P_6, global::a.W P_7)
		{
			if (!P_4.A)
			{
				return 0;
			}
			A(P_7, P_4);
			int num = 0;
			foreach (L item in P_2.Where((L l2) => !l2.A))
			{
				if (item.a)
				{
					if (P_3.A(item.A, item.a + 0.15f, item.B, P_5, P_6, out var pointF))
					{
						A(P_1, new PointF(pointF.X - 9f, pointF.Y), new PointF(pointF.X + 9f, pointF.Y), item.A, (float)P_4.A);
						A(P_1, new PointF(pointF.X, pointF.Y - 9f), new PointF(pointF.X, pointF.Y + 9f), item.A, (float)P_4.A);
						A(P_0, new PointF(pointF.X, pointF.Y - 22f), item.a, item.A, P_7);
						num++;
					}
					continue;
				}
				float[][] array = g.A(item);
				int count = P_1.Count;
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					if (global::B.F.A(P_3, array[num2], array[(num2 + 1) % array.Length], P_5, P_6, out var pointF2, out var pointF3))
					{
						A(P_1, pointF2, pointF3, item.A, (float)P_4.A);
					}
				}
				float num3 = item.d + item.D / 2f;
				if (P_3.A(item.A + item.b * num3, item.a + 0.15f, item.B + item.C * num3, P_5, P_6, out var pointF4))
				{
					A(P_0, pointF4, item.a, item.A, P_7);
				}
				if (P_1.Count > count)
				{
					num++;
				}
			}
			return num;
		}

		public static void A(Graphics P_0, IEnumerable<L> P_1, global::a.w P_2, j P_3, int P_4, int P_5)
		{
			if (!P_3.A)
			{
				return;
			}
			P_0.SmoothingMode = SmoothingMode.AntiAlias;
			foreach (L item in P_1.Where((L l2) => !l2.A && !l2.a))
			{
				PointF[] array = g.A(P_2, g.A(item), P_4, P_5);
				if (array.Length >= 3)
				{
					using SolidBrush brush = new SolidBrush(Color.FromArgb(P_3.A * 255 / 100, item.A));
					P_0.FillPolygon(brush, array);
				}
			}
		}

		private static RectangleF A(int P_0, int P_1)
		{
			return new RectangleF((float)P_0 * 0.32f, (float)P_1 * 0.4f, (float)P_0 * 0.36f, (float)P_1 * 0.14f);
		}

		public static void A(List<global::a.u> P_0, List<y> P_1, int P_2, int P_3, j P_4, global::a.W P_5)
		{
			A(P_5, P_4);
			RectangleF rectangleF = A(P_2, P_3);
			PointF[] array = new PointF[4]
			{
				new PointF(rectangleF.Left, rectangleF.Top),
				new PointF(rectangleF.Right, rectangleF.Top),
				new PointF(rectangleF.Right, rectangleF.Bottom),
				new PointF(rectangleF.Left, rectangleF.Bottom)
			};
			for (int num = 0; num < 4; num++)
			{
				A(P_1, array[num], array[(num + 1) % 4], Color.Cyan, Math.Max(2f, (float)P_4.A));
			}
			A(P_0, new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top + rectangleF.Height / 2f), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BQ(), Color.Cyan, P_5);
		}

		public static void A(Graphics P_0, int P_1, int P_2, j P_3)
		{
			using SolidBrush brush = new SolidBrush(Color.FromArgb(P_3.A * 255 / 100, Color.Cyan));
			P_0.FillRectangle(brush, A(P_1, P_2));
		}
	}
	internal sealed class k : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public j A;

			public Action A;

			internal void A(bool P_0)
			{
				this.A.A = P_0;
				this.A();
			}

			internal void a(bool P_0)
			{
				this.A.a = P_0;
				this.A();
			}

			internal void B(bool P_0)
			{
				this.A.B = P_0;
				this.A();
			}

			internal void A(decimal P_0)
			{
				this.A.A = (int)P_0;
				this.A();
			}

			internal void a(decimal P_0)
			{
				this.A.a = (int)P_0;
				this.A();
			}

			internal void B(decimal P_0)
			{
				this.A.B = (int)P_0;
				this.A();
			}

			internal void b(decimal P_0)
			{
				this.A.b = (int)P_0;
				this.A();
			}

			internal void C(decimal P_0)
			{
				this.A.A = P_0;
				this.A();
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public Action<bool> A;

			public CheckBox A;

			internal void A(object P_0, EventArgs P_1)
			{
				this.A(this.A.Checked);
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public Action<decimal> A;

			public NumericUpDown A;

			internal void A(object P_0, EventArgs P_1)
			{
				this.A(this.A.Value);
			}
		}

		public k(j P_0, Action P_1)
		{
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bq();
			base.ClientSize = new Size(555, 475);
			Font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), 10f);
			base.StartPosition = FormStartPosition.CenterParent;
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(14),
				FlowDirection = FlowDirection.TopDown,
				WrapContents = false,
				AutoScroll = true
			};
			base.Controls.Add(flowLayoutPanel);
			bool flag = P_0.A;
			Action<bool> action = delegate(bool flag2)
			{
				P_0.A = flag2;
				P_1();
			};
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BR(), flag, action);
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Br(), P_0.a, delegate(bool flag2)
			{
				P_0.a = flag2;
				P_1();
			});
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BS(), P_0.B, delegate(bool flag2)
			{
				P_0.B = flag2;
				P_1();
			});
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bs(), P_0.A, 0m, 100m, 0, delegate(decimal num)
			{
				P_0.A = (int)num;
				P_1();
			});
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BT(), P_0.a, 0m, 100m, 0, delegate(decimal num)
			{
				P_0.a = (int)num;
				P_1();
			});
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bt(), P_0.B, 8m, 40m, 0, delegate(decimal num)
			{
				P_0.B = (int)num;
				P_1();
			});
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BU(), P_0.b, 6m, 24m, 0, delegate(decimal num)
			{
				P_0.b = (int)num;
				P_1();
			});
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bu(), P_0.A, 0.5m, 8m, 1, delegate(decimal num)
			{
				P_0.A = num;
				P_1();
			});
			flowLayoutPanel.Controls.Add(new Label
			{
				Width = 515,
				Height = 125,
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BV()
			});
		}

		private static void A(Control P_0, string P_1, bool P_2, Action<bool> P_3)
		{
			CheckBox A = new CheckBox
			{
				Text = P_1,
				Checked = P_2,
				AutoSize = true
			};
			P_0.Controls.Add(A);
			A.CheckedChanged += delegate
			{
				P_3(A.Checked);
			};
		}

		private static void A(Control P_0, string P_1, decimal P_2, decimal P_3, decimal P_4, int P_5, Action<decimal> P_6)
		{
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Width = 515,
				Height = 35
			};
			P_0.Controls.Add(flowLayoutPanel);
			flowLayoutPanel.Controls.Add(new Label
			{
				Text = P_1,
				Width = 355,
				Padding = new Padding(0, 5, 0, 0)
			});
			NumericUpDown A = new NumericUpDown
			{
				Minimum = P_3,
				Maximum = P_4,
				Value = P_2,
				DecimalPlaces = P_5,
				Increment = ((P_5 == 0) ? 1m : 0.5m),
				Width = 100
			};
			flowLayoutPanel.Controls.Add(A);
			A.ValueChanged += delegate
			{
				P_6(A.Value);
			};
		}
	}
}
namespace a
{
	internal sealed class K
	{
		private readonly string m_A;

		private readonly string a;

		private readonly string B;

		private readonly string b;

		private readonly string C;

		private readonly string c;

		private readonly bool m_A;

		internal K(Dictionary<string, object> P_0, string P_1, bool P_2)
		{
			this.m_A = M.A;
			a = A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dJ()).ToUpperInvariant();
			B = A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dj());
			if (!Guid.TryParse(A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dK()), out var result))
			{
				throw new ArgumentException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dk());
			}
			b = result.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dL()).ToLowerInvariant();
			C = A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dl());
			c = P_1;
			this.m_A = P_2;
			if (!Regex.IsMatch(a, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dM()) || !Regex.IsMatch(B, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dm()) || !Regex.IsMatch(C, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dN()))
			{
				throw new ArgumentException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dn());
			}
		}

		private static string A(Dictionary<string, object> P_0, string P_1)
		{
			if (!P_0.TryGetValue(P_1, out var value))
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
			return Convert.ToString(value, CultureInfo.InvariantCulture);
		}

		internal static string A(byte[] P_0)
		{
			using SHA256 sHA = SHA256.Create();
			return BitConverter.ToString(sHA.ComputeHash(P_0)).Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CJ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).ToLowerInvariant();
		}

		internal static string A()
		{
			byte[] array = new byte[16];
			using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
			{
				randomNumberGenerator.GetBytes(array);
			}
			return BitConverter.ToString(array).Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CJ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).ToLowerInvariant();
		}

		internal bool A(p P_0, string P_1, string P_2, string P_3)
		{
			try
			{
				if (P_0.A != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dO() || !Regex.IsMatch(P_3 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.@do()) || P_0.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dP()) != P_3)
				{
					return false;
				}
				byte[] array = Convert.FromBase64String(P_1);
				byte[] array2 = Convert.FromBase64String(P_2);
				if (array.Length != 384 || (array[0] & 0x80) == 0 || array2.Length == 0 || A(array).Substring(0, 16) != P_3)
				{
					return false;
				}
				string text = P_0.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dp());
				if (text.Length == 0 && !this.m_A)
				{
					text = c;
				}
				if (!Regex.IsMatch(text ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dQ()))
				{
					return false;
				}
				string text2 = P_0.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dq());
				if (text2 != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dR() && text2 != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dr())
				{
					return false;
				}
				string[] array3 = new string[3];
				string[] array4 = new string[3]
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dS(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ds(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dT()
				};
				for (int num = 0; num < array4.Length; num++)
				{
					string text3 = P_0.A(array4[num]);
					if (!long.TryParse(text3, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result) || text3 != result.ToString(CultureInfo.InvariantCulture))
					{
						return false;
					}
					array3[num] = text3;
				}
				string text4 = string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dt(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dU(), this.m_A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dO(), a, B, b, C, A(Encoding.UTF8.GetBytes(text)), array3[0], array3[1], array3[2], text2, P_3);
				byte[] array5 = Convert.FromBase64String(P_0.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.du()));
				if (array5.Length != 384)
				{
					return false;
				}
				using RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
				rSACryptoServiceProvider.PersistKeyInCsp = false;
				rSACryptoServiceProvider.ImportParameters(new RSAParameters
				{
					Modulus = array,
					Exponent = array2
				});
				return rSACryptoServiceProvider.VerifyData(Encoding.UTF8.GetBytes(text4), CryptoConfig.MapNameToOID(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dV()), array5);
			}
			catch (Exception ex) when (ex is FormatException || ex is CryptographicException || ex is ArgumentException)
			{
				return false;
			}
		}
	}
	internal sealed class k : ApplicationContext
	{
		[CompilerGenerated]
		private sealed class A
		{
			public R A;

			public k A;

			internal void A()
			{
				this.A.A(this.A);
			}
		}

		private readonly m m_A;

		private readonly global::A.d m_A;

		private global::B.K m_A;

		private readonly Timer m_A = new Timer();

		private bool m_A;

		private bool m_a;

		private bool m_B;

		private long m_A;

		private global::b.l m_A;

		public k(m P_0 = null)
		{
			this.m_A = P_0 ?? new m();
			this.m_A = new global::A.d(this.m_A.A);
			_ = this.m_A.Handle;
			this.m_A.A(delegate(string text, bool flag)
			{
				this.m_A.A(text, flag);
			});
			this.m_A.FormClosing += A;
			SystemEvents.PowerModeChanged += A;
			this.m_A.A(delegate(R r2)
			{
				if (!this.m_A && !this.m_A.IsDisposed)
				{
					try
					{
						this.m_A.BeginInvoke((Action)delegate
						{
							A(r2);
						});
					}
					catch (InvalidOperationException)
					{
					}
				}
			});
			this.m_A.Interval = 100;
			this.m_A.Tick += delegate
			{
				bool flag = O.A;
				if (flag != this.m_B)
				{
					this.m_B = flag;
					if (this.m_A != null)
					{
						if (flag)
						{
							this.m_A.l();
						}
						else
						{
							this.m_A.M();
						}
					}
				}
				if (!flag)
				{
					global::A.q.A();
				}
			};
			this.m_A.Start();
			this.m_A.Show();
			this.m_A.a();
		}

		private void A()
		{
			if (this.m_A == null)
			{
				this.m_A = new global::B.K();
				this.m_A.FormClosing += A;
				this.m_A.b(delegate
				{
					this.m_A.B();
					this.m_A.M();
					this.m_A.Show();
					this.m_A.Activate();
				});
			}
		}

		private void A(R P_0)
		{
			if (this.m_A || this.m_a || !this.m_A.A(P_0))
			{
				return;
			}
			this.m_A.A(P_0);
			if (this.m_A != null && (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dv() || P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dW() || P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dw() || P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dX()))
			{
				this.m_A.A();
			}
			if (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dx())
			{
				if (!O.A || this.m_A == null)
				{
					return;
				}
				if (this.m_A == null)
				{
					using (global::b.l l2 = new global::b.l(P_0, false))
					{
						this.m_A = l2;
						try
						{
							l2.ShowDialog(this.m_A);
							return;
						}
						finally
						{
							this.m_A = null;
						}
					}
				}
				this.m_A.A(P_0);
			}
			else if (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dO() || P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dv())
			{
				this.m_A = ((P_0.A > 0) ? P_0.A : this.m_A);
				A();
				if (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dO())
				{
					this.m_A.A(this.m_A, P_0.D);
					this.m_A.l();
					this.m_B = true;
				}
				else
				{
					this.m_A.M();
					this.m_B = false;
				}
				if (this.m_A.Visible)
				{
					this.m_A.Hide();
					this.m_A.Show();
					this.m_A.Activate();
				}
			}
			else if (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dW() || P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dX())
			{
				this.m_a = true;
				O.B();
				if (this.m_A != null)
				{
					this.m_A.M();
				}
				Form owner = ((this.m_A != null && this.m_A.Visible) ? ((Form)this.m_A) : ((Form)this.m_A));
				if (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dX())
				{
					using global::b.l l3 = new global::b.l(P_0, true);
					l3.ShowDialog(owner);
				}
				else
				{
					using P p2 = new P(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dY(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dY(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dy());
					p2.ShowDialog(owner);
				}
				a();
			}
			else if (P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dw() || P_0.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dZ())
			{
				if (this.m_A != null)
				{
					this.m_A.M();
					this.m_A.Hide();
				}
				this.m_A.Show();
			}
			else if (!O.A && this.m_A != null)
			{
				this.m_A.M();
			}
		}

		private void A(object P_0, FormClosingEventArgs P_1)
		{
			if (!this.m_A)
			{
				P_1.Cancel = true;
				a();
			}
		}

		private void A(object P_0, PowerModeChangedEventArgs P_1)
		{
			if (P_1.Mode == PowerModes.Resume)
			{
				this.m_A.b();
			}
		}

		private async void a()
		{
			if (this.m_A)
			{
				return;
			}
			this.m_A = true;
			if (this.m_A != null)
			{
				this.m_A.A();
			}
			SystemEvents.PowerModeChanged -= A;
			this.m_A.Stop();
			O.B();
			if (this.m_A != null)
			{
				this.m_A.M();
			}
			this.m_A.Hide();
			if (this.m_A != null)
			{
				this.m_A.Hide();
			}
			try
			{
				await this.m_A.E();
			}
			finally
			{
				if (this.m_A != null)
				{
					this.m_A.Close();
				}
				this.m_A.Close();
				this.m_A.Dispose();
				ExitThread();
			}
		}

		protected override void ExitThreadCore()
		{
			this.m_A.Dispose();
			base.ExitThreadCore();
		}

		[CompilerGenerated]
		private void A(string P_0, bool P_1)
		{
			this.m_A.A(P_0, P_1);
		}

		[CompilerGenerated]
		private void a(R P_0)
		{
			if (this.m_A || this.m_A.IsDisposed)
			{
				return;
			}
			try
			{
				this.m_A.BeginInvoke((Action)delegate
				{
					A(P_0);
				});
			}
			catch (InvalidOperationException)
			{
			}
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			bool flag = O.A;
			if (flag != this.m_B)
			{
				this.m_B = flag;
				if (this.m_A != null)
				{
					if (flag)
					{
						this.m_A.l();
					}
					else
					{
						this.m_A.M();
					}
				}
			}
			if (!flag)
			{
				global::A.q.A();
			}
		}

		[CompilerGenerated]
		private void B()
		{
			this.m_A.B();
			this.m_A.M();
			this.m_A.Show();
			this.m_A.Activate();
		}
	}
}
