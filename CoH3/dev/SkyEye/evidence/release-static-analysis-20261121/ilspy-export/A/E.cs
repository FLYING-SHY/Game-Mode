using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;

namespace A
{
	internal static class E
	{
		public static int A()
		{
			List<string> list = new List<string>();
			try
			{
				using (global::B.n n2 = new global::B.n())
				{
					list.AddRange(n2.D());
				}
				File.WriteAllLines(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aD(), list);
				return 0;
			}
			catch (Exception ex)
			{
				list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ad() + ex);
				File.WriteAllLines(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aD(), list);
				return 1;
			}
		}
	}
	internal sealed class e
	{
		public L A;

		public G A;

		public double A;

		public double a;

		public bool A;

		public readonly HashSet<string> A = new HashSet<string>();
	}
}
namespace a
{
	internal sealed class E : FlowLayoutPanel
	{
		public readonly FlowLayoutPanel A;

		public readonly global::A.D A;

		public bool A;

		private readonly string m_A;

		public bool A
		{
			get
			{
				return this.A.Visible;
			}
			set
			{
				this.A.Visible = flag;
				this.A.Text = (this.A ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D() : (this.m_A + (flag ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Do() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DO())));
				PerformLayout();
			}
		}

		public void A(int P_0)
		{
			MinimumSize = new Size(P_0, 0);
			MaximumSize = new Size(P_0, 0);
			this.A.Width = P_0;
			this.A.MinimumSize = new Size(P_0, 0);
			this.A.MaximumSize = new Size(P_0, 0);
			foreach (Control control in this.A.Controls)
			{
				if (control.Tag as string == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DP())
				{
					if (control is E e2)
					{
						e2.A(P_0);
					}
					else
					{
						control.Width = P_0;
					}
				}
			}
			PerformLayout();
		}

		public E(string P_0)
		{
			this.m_A = P_0;
			base.Tag = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DP();
			base.Width = 390;
			base.FlowDirection = FlowDirection.TopDown;
			base.WrapContents = false;
			AutoSize = true;
			AutoSizeMode = AutoSizeMode.GrowAndShrink;
			base.Margin = new Padding(0, 3, 0, 3);
			this.A = new global::A.D
			{
				Name = P_0,
				Text = P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DO(),
				Width = 390,
				Margin = Padding.Empty
			};
			this.A = new FlowLayoutPanel
			{
				Visible = false,
				Width = 390,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				WrapContents = true,
				Margin = new Padding(0, 6, 0, 4)
			};
			base.Controls.Add(this.A);
			base.Controls.Add(this.A);
			global::A.D obj = this.A;
			EventHandler value = delegate
			{
				this.A = !this.A;
			};
			obj.Click += value;
			base.SizeChanged += delegate
			{
				this.A.Width = base.Width;
				this.A.MinimumSize = new Size(base.Width, 0);
				this.A.MaximumSize = new Size(base.Width, 0);
			};
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			this.A = !this.A;
		}

		[CompilerGenerated]
		private void a(object P_0, EventArgs P_1)
		{
			this.A.Width = base.Width;
			this.A.MinimumSize = new Size(base.Width, 0);
			this.A.MaximumSize = new Size(base.Width, 0);
		}
	}
	internal sealed class e
	{
		public global::A.l A;

		public string A;
	}
}
