using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using B;
using a;
using b;

namespace A
{
	internal static class X
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
			public w A;

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

		public static bool A(w P_0, float P_1, float P_2, float P_3)
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
			if (!P.a(P_0) && !new string[11]
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bg(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bv(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cz(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DA(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Da(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DB(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bx(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Db(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DC(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dc(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DD()
			}.Any((string value) => P_0.Contains(value)) && !P_1.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DC()) && !P_1.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dc()))
			{
				return P_1.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DD());
			}
			return true;
		}

		public static float A(w P_0, global::b.b P_1)
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

		public static List<Y> A(IEnumerable<global::B.Z> P_0, global::a.v P_1, global::b.b P_2, int P_3, int P_4)
		{
			List<Y> list = new List<Y>();
			if (!P_2.C)
			{
				return list;
			}
			foreach (global::B.Z item in P_0)
			{
				w A = item.A;
				if (A == null || !A.C)
				{
					continue;
				}
				float A2 = X.A(A, P_2);
				double num = Math.Atan2(A.b, A.C);
				X.A(list, item.A, P_1, P_3, P_4, from num6 in Enumerable.Range(0, 65)
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
					X.A(list, item.A, P_1, P_3, P_4, list2);
				}
			}
			return list;
		}

		private static void A(List<Y> P_0, Color P_1, global::a.v P_2, int P_3, int P_4, IEnumerable<float[]> P_5)
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

		private static void A(List<Y> P_0, Color P_1, List<PointF> P_2)
		{
			if (P_2.Count >= 2)
			{
				float num = P_2.Min((PointF pointF) => pointF.X);
				float num2 = P_2.Min((PointF pointF) => pointF.Y);
				float num3 = P_2.Max((PointF pointF) => pointF.X);
				float num4 = P_2.Max((PointF pointF) => pointF.Y);
				P_0.Add(new Y
				{
					A = P_1,
					A = P_2.ToArray(),
					A = Rectangle.FromLTRB((int)num - 16, (int)num2 - 16, (int)num3 + 17, (int)num4 + 17)
				});
			}
			P_2.Clear();
		}

		public static void A(Graphics P_0, IEnumerable<Y> P_1, decimal P_2)
		{
			P_0.SmoothingMode = SmoothingMode.AntiAlias;
			foreach (Y item in P_1)
			{
				using Pen pen = new Pen(item.A, (item.A > 0f) ? item.A : ((float)P_2));
				if (item.A)
				{
					if (item.a > 0f && item.B > 0f)
					{
						pen.DashPattern = global::B.E.A(item.a, item.B, pen.Width);
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
	internal sealed class x : Form
	{
		[CompilerGenerated]
		private sealed class A
		{
			public global::b.b A;

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

		public x(global::b.b P_0, Action P_1)
		{
			Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dd();
			base.ClientSize = new Size(620, 550);
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
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DE(),
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
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.De(),
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
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DF(),
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
			x.A(flowLayoutPanel, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Df(), num, 1m, 200m, action);
			x.A(flowLayoutPanel, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DG(), P_0.B, 1m, 200m, delegate(decimal num2)
			{
				P_0.B = (int)num2;
				P_1();
			});
			x.A(flowLayoutPanel, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dg(), P_0.b, 1m, 200m, delegate(decimal num2)
			{
				P_0.b = (int)num2;
				P_1();
			});
			x.A(flowLayoutPanel, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DH(), P_0.b, 0.5m, 12m, delegate(decimal num2)
			{
				P_0.b = num2;
				P_1();
			});
			CheckBox b2 = new CheckBox
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dh(),
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
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DI(),
				Checked = P_0.E,
				AutoSize = true
			};
			flowLayoutPanel.Controls.Add(C);
			C.CheckedChanged += delegate
			{
				P_0.E = C.Checked;
				P_1();
			};
			x.A(flowLayoutPanel, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Di(), P_0.C, 0m, 180m, delegate(decimal num2)
			{
				P_0.C = (int)num2;
				P_1();
			});
			string text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DJ();
			flowLayoutPanel.Controls.Add(new Label
			{
				Width = 580,
				Height = 100,
				Text = text + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dj()
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
}
namespace a
{
	internal static class X
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
	internal struct x
	{
		public double A;

		public double a;

		public double B;

		public double b;

		public double C;

		public double c;

		public static x A => new x
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

		public static x A(double P_0, double P_1)
		{
			return new x
			{
				A = 1.0,
				b = 1.0,
				C = P_0,
				c = P_1
			};
		}

		public x A(x P_0)
		{
			return new x
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

		public static x A(byte[] P_0)
		{
			uint num = BitConverter.ToUInt32(P_0, 28);
			switch (num)
			{
			default:
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Eu());
			case 0u:
				return x.A;
			case 1u:
			case 2u:
			case 3u:
			case 4u:
			case 5u:
			case 6u:
			case 7u:
			{
				x result = x.A;
				if ((num & 4) != 0)
				{
					result = new x
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
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EV());
				}
				return result;
			}
			}
		}

		public static y A(float P_0, float P_1, float P_2, float P_3, float P_4, float P_5, x P_6)
		{
			x x2 = new x
			{
				A = P_2 / P_0,
				b = P_3 / P_1,
				C = P_2 / 2f,
				c = P_3 / 2f
			};
			x x3 = x2.A(P_6);
			if (!x3.a)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ev());
			}
			PointF pointF = x3.a(0.0, 0.0);
			return new y
			{
				A = true,
				A = x3,
				D = P_0,
				d = P_1,
				C = P_4,
				c = P_5,
				A = pointF.X,
				a = pointF.Y,
				B = (float)(Math.Sqrt(x3.A * x3.A + x3.B * x3.B) * (double)P_0),
				b = (float)(Math.Sqrt(x3.a * x3.a + x3.b * x3.b) * (double)P_1),
				E = (float)(Math.Atan2(x3.B, x3.A) * 180.0 / Math.PI)
			};
		}
	}
}
