using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;

namespace B;

internal sealed class T
{
	public sealed class A
	{
		public ulong A;

		public string A;

		public bool A;

		public int A;

		public double A;

		public double a;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class a
	{
		public static readonly a A = new a();

		public static Func<KeyValuePair<ulong, A>, ulong> A;

		internal ulong A(KeyValuePair<ulong, A> P_0)
		{
			KeyValuePair<ulong, A> keyValuePair = P_0;
			return keyValuePair.Key;
		}
	}

	[CompilerGenerated]
	private sealed class B
	{
		public double A;

		internal bool A(KeyValuePair<ulong, A> P_0)
		{
			KeyValuePair<ulong, A> keyValuePair = P_0;
			if (!keyValuePair.Value.A)
			{
				double num = this.A;
				keyValuePair = P_0;
				return num - keyValuePair.Value.A > 120000.0;
			}
			return false;
		}
	}

	private readonly Dictionary<ulong, A> m_A = new Dictionary<ulong, A>();

	public IEnumerable<A> A => this.m_A.Values.ToArray();

	public A A(ulong P_0)
	{
		if (!this.m_A.TryGetValue(P_0, out var value))
		{
			Dictionary<ulong, A> dictionary = this.m_A;
			A obj = new A
			{
				A = P_0
			};
			A result = obj;
			dictionary[P_0] = obj;
			return result;
		}
		return value;
	}

	public void A(ulong P_0, string P_1)
	{
		A a2 = A(P_0);
		if (!string.IsNullOrEmpty(P_1))
		{
			if (!a2.A || a2.A != P_1)
			{
				a2.A = 0;
			}
			a2.A = true;
			a2.A = P_1;
		}
		else
		{
			a2.A = false;
		}
	}

	public int A(ulong P_0, double P_1)
	{
		A a2 = A(P_0);
		if (!a2.A && P_1 - a2.A > 12000.0)
		{
			a2.A = 0;
		}
		a2.A = P_1;
		return ++a2.A;
	}

	public void A(double P_0)
	{
		ulong[] array = this.m_A.Where(delegate(KeyValuePair<ulong, A> keyValuePair2)
		{
			KeyValuePair<ulong, A> keyValuePair = keyValuePair2;
			if (!keyValuePair.Value.A)
			{
				double num2 = P_0;
				keyValuePair = keyValuePair2;
				return num2 - keyValuePair.Value.A > 120000.0;
			}
			return false;
		}).Select(delegate(KeyValuePair<ulong, A> keyValuePair2)
		{
			KeyValuePair<ulong, A> keyValuePair = keyValuePair2;
			return keyValuePair.Key;
		}).ToArray();
		foreach (ulong key in array)
		{
			this.m_A.Remove(key);
		}
	}

	public void a()
	{
		this.m_A.Clear();
	}
}
internal sealed class t : Form
{
	[CompilerGenerated]
	private sealed class A
	{
		public s A;

		public Action A;

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
			this.A.A = P_0;
			this.A();
		}
	}

	[CompilerGenerated]
	private sealed class a
	{
		public CheckBox A;

		public CheckBox a;

		public A A;

		internal void A(object P_0, EventArgs P_1)
		{
			this.A.A.B = this.A.Checked;
			this.A.A();
		}

		internal void a(object P_0, EventArgs P_1)
		{
			this.A.A.b = this.a.Checked;
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

	public t(s P_0, Action P_1)
	{
		Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.HX();
		base.ClientSize = new Size(510, 335);
		Font = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), 10f);
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
			Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jz(),
			AutoSize = true,
			Checked = P_0.B
		};
		flowLayoutPanel.Controls.Add(A);
		A.CheckedChanged += delegate
		{
			P_0.B = A.Checked;
			P_1();
		};
		CheckBox a2 = new CheckBox
		{
			Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KA(),
			AutoSize = true,
			Checked = P_0.b
		};
		flowLayoutPanel.Controls.Add(a2);
		a2.CheckedChanged += delegate
		{
			P_0.b = a2.Checked;
			P_1();
		};
		decimal num = P_0.A;
		Action<decimal> action = delegate(decimal num2)
		{
			P_0.A = (int)num2;
			P_1();
		};
		t.A(flowLayoutPanel, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ka(), num, 8m, 72m, 0, action);
		t.A(flowLayoutPanel, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KB(), P_0.a, 8m, 36m, 0, delegate(decimal num2)
		{
			P_0.a = (int)num2;
			P_1();
		});
		t.A(flowLayoutPanel, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kb(), P_0.A, 0.5m, 8m, 1, delegate(decimal num2)
		{
			P_0.A = num2;
			P_1();
		});
		flowLayoutPanel.Controls.Add(new Label
		{
			Width = 475,
			Height = 105,
			Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KC()
		});
	}

	private static void A(Control P_0, string P_1, decimal P_2, decimal P_3, decimal P_4, int P_5, Action<decimal> P_6)
	{
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Width = 475,
			Height = 35
		};
		P_0.Controls.Add(flowLayoutPanel);
		flowLayoutPanel.Controls.Add(new Label
		{
			Text = P_1,
			Width = 310,
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
