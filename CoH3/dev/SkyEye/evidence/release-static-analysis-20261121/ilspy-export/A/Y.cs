using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using b;

namespace A
{
	internal sealed class Y : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public global::b.c A;

			public Action A;

			internal void A(decimal P_0)
			{
				this.A.a = (int)P_0;
				this.A();
			}

			internal void a(decimal P_0)
			{
				this.A.B = (int)P_0;
				this.A();
			}

			internal void B(decimal P_0)
			{
				this.A.b = (int)P_0;
				this.A();
			}

			internal void b(decimal P_0)
			{
				this.A.b = P_0;
				this.A();
			}

			internal void C(decimal P_0)
			{
				this.A.C = (int)P_0;
				this.A();
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public CheckBox A;

			public CheckBox a;

			public CheckBox B;

			public CheckBox b;

			public CheckBox C;

			public A A;

			internal void A(object P_0, EventArgs P_1)
			{
				this.A.A.C = this.A.Checked;
				this.A.A();
			}

			internal void a(object P_0, EventArgs P_1)
			{
				this.A.A.c = this.a.Checked;
				this.A.A();
			}

			internal void B(object P_0, EventArgs P_1)
			{
				this.A.A.D = this.B.Checked;
				this.A.A();
			}

			internal void b(object P_0, EventArgs P_1)
			{
				this.A.A.d = this.b.Checked;
				this.A.A();
			}

			internal void C(object P_0, EventArgs P_1)
			{
				this.A.A.E = this.C.Checked;
				this.A.A();
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

		public Y(global::b.c P_0, Action P_1)
		{
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DE();
			base.ClientSize = new Size(620, 550);
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
			CheckBox A = new CheckBox
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.De(),
				Checked = P_0.C,
				AutoSize = true
			};
			flowLayoutPanel.Controls.Add(A);
			A.CheckedChanged += delegate
			{
				P_0.C = A.Checked;
				P_1();
			};
			CheckBox a2 = new CheckBox
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DF(),
				Checked = P_0.c,
				AutoSize = true
			};
			flowLayoutPanel.Controls.Add(a2);
			a2.CheckedChanged += delegate
			{
				P_0.c = a2.Checked;
				P_1();
			};
			CheckBox B = new CheckBox
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Df(),
				Checked = P_0.D,
				AutoSize = true
			};
			flowLayoutPanel.Controls.Add(B);
			B.CheckedChanged += delegate
			{
				P_0.D = B.Checked;
				P_1();
			};
			decimal num = P_0.a;
			Action<decimal> action = delegate(decimal num2)
			{
				P_0.a = (int)num2;
				P_1();
			};
			Y.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DG(), num, 1m, 200m, action);
			Y.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dg(), P_0.B, 1m, 200m, delegate(decimal num2)
			{
				P_0.B = (int)num2;
				P_1();
			});
			Y.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DH(), P_0.b, 1m, 200m, delegate(decimal num2)
			{
				P_0.b = (int)num2;
				P_1();
			});
			Y.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dh(), P_0.b, 0.5m, 12m, delegate(decimal num2)
			{
				P_0.b = num2;
				P_1();
			});
			CheckBox b2 = new CheckBox
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DI(),
				Checked = P_0.d,
				AutoSize = true
			};
			flowLayoutPanel.Controls.Add(b2);
			b2.CheckedChanged += delegate
			{
				P_0.d = b2.Checked;
				P_1();
			};
			CheckBox C = new CheckBox
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Di(),
				Checked = P_0.E,
				AutoSize = true
			};
			flowLayoutPanel.Controls.Add(C);
			C.CheckedChanged += delegate
			{
				P_0.E = C.Checked;
				P_1();
			};
			Y.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DJ(), P_0.C, 0m, 180m, delegate(decimal num2)
			{
				P_0.C = (int)num2;
				P_1();
			});
			string text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dj();
			flowLayoutPanel.Controls.Add(new Label
			{
				Width = 580,
				Height = 100,
				Text = text + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DK()
			});
		}

		private static void A(Control P_0, string P_1, decimal P_2, decimal P_3, decimal P_4, Action<decimal> P_5)
		{
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Width = 585,
				Height = 32,
				WrapContents = false
			};
			P_0.Controls.Add(flowLayoutPanel);
			flowLayoutPanel.Controls.Add(new Label
			{
				Text = P_1,
				Width = 450,
				Padding = new Padding(0, 5, 0, 0)
			});
			NumericUpDown A = new NumericUpDown
			{
				Minimum = P_3,
				Maximum = P_4,
				Value = P_2,
				DecimalPlaces = ((P_3 < 1m) ? 1 : 0),
				Increment = ((P_3 < 1m) ? 0.5m : 1m),
				Width = 90
			};
			flowLayoutPanel.Controls.Add(A);
			A.ValueChanged += delegate
			{
				P_5(A.Value);
			};
		}
	}
	internal sealed class y
	{
		public bool A;

		public float A;

		public float a;

		public float B;

		public Color A;

		public PointF[] A;

		public Rectangle A;
	}
}
namespace a
{
	internal static class Y
	{
		public static long A(string P_0, int P_1, long P_2, long P_3)
		{
			string[] array = P_0.Trim().Split('|');
			if (array.Length != 4 || !int.TryParse(array[0], out var result) || !long.TryParse(array[1], out var result2) || !long.TryParse(array[2], out var result3) || !long.TryParse(array[3], out var result4) || result != P_1 || result2 != P_2 || result3 != P_3 || result4 < 65536 || result4 > 140737488355327L || result4 % 8 != 0L)
			{
				return 0L;
			}
			return result4;
		}
	}
	internal struct y
	{
		public double A;

		public double a;

		public double B;

		public double b;

		public double C;

		public double c;

		public static y A => new y
		{
			A = 1.0,
			b = 1.0
		};

		public bool a
		{
			get
			{
				double[] array = new double[6] { this.A, this.a, B, b, C, c };
				foreach (double value in array)
				{
					if (double.IsNaN(value) || double.IsInfinity(value) || Math.Abs(value) > 1000000.0)
					{
						return false;
					}
				}
				return Math.Abs(this.A * b - this.a * B) > 1E-09;
			}
		}

		public static y A(double P_0, double P_1)
		{
			return new y
			{
				A = 1.0,
				b = 1.0,
				C = P_0,
				c = P_1
			};
		}

		public y A(y P_0)
		{
			return new y
			{
				A = P_0.A * this.A + P_0.a * B,
				a = P_0.A * this.a + P_0.a * b,
				B = P_0.B * this.A + P_0.b * B,
				b = P_0.B * this.a + P_0.b * b,
				C = P_0.A * C + P_0.a * c + P_0.C,
				c = P_0.B * C + P_0.b * c + P_0.c
			};
		}

		public PointF a(double P_0, double P_1)
		{
			return new PointF((float)(this.A * P_0 + this.a * P_1 + C), (float)(B * P_0 + b * P_1 + c));
		}

		public static y A(byte[] P_0)
		{
			uint num = BitConverter.ToUInt32(P_0, 28);
			switch (num)
			{
			default:
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EY());
			case 0u:
				return y.A;
			case 1u:
			case 2u:
			case 3u:
			case 4u:
			case 5u:
			case 6u:
			case 7u:
			{
				y result = y.A;
				if ((num & 4) != 0)
				{
					result = new y
					{
						A = BitConverter.ToSingle(P_0, 0),
						B = BitConverter.ToSingle(P_0, 4),
						a = BitConverter.ToSingle(P_0, 8),
						b = BitConverter.ToSingle(P_0, 12),
						C = BitConverter.ToSingle(P_0, 16),
						c = BitConverter.ToSingle(P_0, 20)
					};
				}
				else
				{
					if ((num & 2) != 0)
					{
						result.A = BitConverter.ToSingle(P_0, 0);
						result.b = BitConverter.ToSingle(P_0, 12);
					}
					if ((num & 1) != 0)
					{
						result.C = BitConverter.ToSingle(P_0, 16);
						result.c = BitConverter.ToSingle(P_0, 20);
					}
				}
				if (!result.a)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ey());
				}
				return result;
			}
			}
		}

		public static z A(float P_0, float P_1, float P_2, float P_3, float P_4, float P_5, y P_6)
		{
			y y2 = new y
			{
				A = P_2 / P_0,
				b = P_3 / P_1,
				C = P_2 / 2f,
				c = P_3 / 2f
			};
			y y3 = y2.A(P_6);
			if (!y3.a)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EZ());
			}
			PointF pointF = y3.a(0.0, 0.0);
			return new z
			{
				A = true,
				A = y3,
				D = P_0,
				d = P_1,
				C = P_4,
				c = P_5,
				A = pointF.X,
				a = pointF.Y,
				B = (float)(Math.Sqrt(y3.A * y3.A + y3.B * y3.B) * (double)P_0),
				b = (float)(Math.Sqrt(y3.a * y3.a + y3.b * y3.b) * (double)P_1),
				E = (float)(Math.Atan2(y3.B, y3.A) * 180.0 / Math.PI)
			};
		}
	}
}
