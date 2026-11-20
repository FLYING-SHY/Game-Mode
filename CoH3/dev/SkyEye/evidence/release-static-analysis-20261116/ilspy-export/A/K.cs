using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;

namespace A
{
	internal sealed class K : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public J A;

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

		public K(J P_0, Action P_1)
		{
			Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BQ();
			base.ClientSize = new Size(555, 475);
			Font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), 10f);
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
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bq(), flag, action);
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BR(), P_0.a, delegate(bool flag2)
			{
				P_0.a = flag2;
				P_1();
			});
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Br(), P_0.B, delegate(bool flag2)
			{
				P_0.B = flag2;
				P_1();
			});
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BS(), P_0.A, 0m, 100m, 0, delegate(decimal num)
			{
				P_0.A = (int)num;
				P_1();
			});
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bs(), P_0.a, 0m, 100m, 0, delegate(decimal num)
			{
				P_0.a = (int)num;
				P_1();
			});
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BT(), P_0.B, 8m, 40m, 0, delegate(decimal num)
			{
				P_0.B = (int)num;
				P_1();
			});
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bt(), P_0.b, 6m, 24m, 0, delegate(decimal num)
			{
				P_0.b = (int)num;
				P_1();
			});
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BU(), P_0.A, 0.5m, 8m, 1, delegate(decimal num)
			{
				P_0.A = num;
				P_1();
			});
			flowLayoutPanel.Controls.Add(new Label
			{
				Width = 515,
				Height = 125,
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bu()
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
	internal sealed class k
	{
		public string A;

		public string a;

		public long A;

		public uint A;

		public Color A;

		public float A;

		public float a;

		public float B;

		public float b;

		public float C;

		public float c;

		public float D;

		public float d;

		public float E;

		public bool A;

		public bool a;
	}
}
namespace a
{
	internal static class K
	{
		public static bool A(this string[] P_0, string P_1)
		{
			for (int num = 0; num < P_0.Length; num++)
			{
				if (P_0[num] == P_1)
				{
					return true;
				}
			}
			return false;
		}
	}
	internal static class k
	{
		public static long A => checked((long)a());

		[DllImport("kernel32.dll", EntryPoint = "GetTickCount64")]
		private static extern ulong a();
	}
}
