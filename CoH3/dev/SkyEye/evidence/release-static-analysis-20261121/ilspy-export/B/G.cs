using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using b;

namespace B
{
	internal static class G
	{
		public static bool A(byte[] P_0, byte[] P_1)
		{
			uint num = BitConverter.ToUInt32(P_0, 64);
			uint num2 = BitConverter.ToUInt32(P_1, 144);
			if (num == 0 && BitConverter.ToUInt32(P_0, 8) == 0 && P_0[16] != 5 && P_0[76] == 2)
			{
				if (num2 != 2 && num2 != 3)
				{
					return num2 == 4;
				}
				return true;
			}
			return false;
		}

		public static e a(byte[] P_0, byte[] P_1)
		{
			if (!A(P_0, P_1))
			{
				return null;
			}
			float num = BitConverter.ToSingle(P_0, 80);
			float num2 = BitConverter.ToSingle(P_0, 84);
			float num3 = BitConverter.ToSingle(P_0, 88);
			if (!global::b.b.A(num, num2, num3))
			{
				return null;
			}
			return new e
			{
				A = num,
				a = num2,
				B = num3
			};
		}
	}
	internal sealed class g : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public global::b.c A;

			public Action A;

			internal void A(decimal P_0)
			{
				this.A.A = (int)P_0;
				this.A();
			}

			internal void a(decimal P_0)
			{
				this.A.A = P_0;
				this.A();
			}

			internal void B(decimal P_0)
			{
				this.A.a = P_0;
				this.A();
			}

			internal void b(decimal P_0)
			{
				this.A.B = P_0;
				this.A();
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public CheckBox A;

			public A A;

			internal void A(object P_0, EventArgs P_1)
			{
				this.A.A.a = this.A.Checked;
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

		public g(global::b.c P_0, Action P_1)
		{
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eu();
			base.ClientSize = new Size(510, 350);
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
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eV(),
				Checked = P_0.a,
				AutoSize = true
			};
			flowLayoutPanel.Controls.Add(A);
			A.CheckedChanged += delegate
			{
				P_0.a = A.Checked;
				P_1();
			};
			decimal num = P_0.A;
			Action<decimal> action = delegate(decimal num2)
			{
				P_0.A = (int)num2;
				P_1();
			};
			g.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ev(), num, 8m, 160m, 0, action);
			g.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eW(), P_0.A, 0.5m, 12m, 1, delegate(decimal num2)
			{
				P_0.A = num2;
				P_1();
			});
			g.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ew(), P_0.a, 1m, 100m, 1, delegate(decimal num2)
			{
				P_0.a = num2;
				P_1();
			});
			g.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eX(), P_0.B, 1m, 100m, 1, delegate(decimal num2)
			{
				P_0.B = num2;
				P_1();
			});
			flowLayoutPanel.Controls.Add(new Label
			{
				Width = 470,
				Height = 105,
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ex()
			});
		}

		private static void A(Control P_0, string P_1, decimal P_2, decimal P_3, decimal P_4, int P_5, Action<decimal> P_6)
		{
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Width = 470,
				Height = 35
			};
			P_0.Controls.Add(flowLayoutPanel);
			flowLayoutPanel.Controls.Add(new Label
			{
				Text = P_1,
				Width = 280,
				Padding = new Padding(0, 5, 0, 0)
			});
			NumericUpDown A = new NumericUpDown
			{
				Minimum = P_3,
				Maximum = P_4,
				Value = P_2,
				DecimalPlaces = P_5,
				Increment = ((P_5 == 0) ? 1m : 0.5m),
				Width = 90
			};
			flowLayoutPanel.Controls.Add(A);
			A.ValueChanged += delegate
			{
				P_6(A.Value);
			};
		}
	}
}
namespace b
{
	internal static class G
	{
		public const string A = "abilities\\races\\afrika_korps\\battlegroups\\";

		public static string A(string P_0)
		{
			if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LI()))
			{
				if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Li()))
				{
					if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LJ()))
					{
						if (P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lj())
						{
							return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hR();
						}
						return null;
					}
					return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LL();
				}
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lk();
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LK();
		}

		public static bool A(byte[] P_0, long P_1, long P_2, Func<long, long> P_3 = null)
		{
			if (P_0 != null && P_0.Length >= 164 && BitConverter.ToInt64(P_0, 0) == P_1 + (P_3?.Invoke(64568064L) ?? 64568064) && P_0[76] != 2 && P_0[20] == 6 && BitConverter.ToInt64(P_0, 24) == P_2)
			{
				return BitConverter.ToUInt32(P_0, 68) != uint.MaxValue;
			}
			return false;
		}
	}
	internal sealed class g
	{
		public string A;

		public string a;

		public string B;

		public uint A;

		public float A;

		public float a;

		public float B;

		public DateTime A;

		public bool A;
	}
}
