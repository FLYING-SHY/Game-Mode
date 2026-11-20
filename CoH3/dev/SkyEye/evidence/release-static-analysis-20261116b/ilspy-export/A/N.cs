using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using B;

namespace A
{
	internal sealed class N : IDisposable
	{
		private struct A
		{
			public uint A;

			public int A;

			public int a;

			public ushort A;

			public ushort a;

			public uint a;

			public uint B;

			public int B;

			public int b;

			public uint b;

			public uint C;
		}

		private struct a
		{
			public byte A;

			public byte a;

			public byte B;

			public byte b;
		}

		private IntPtr m_A;

		private IntPtr m_a;

		private IntPtr m_B;

		private IntPtr m_b;

		[CompilerGenerated]
		private Bitmap m_A;

		[CompilerGenerated]
		private Graphics m_A;

		public Bitmap A
		{
			[CompilerGenerated]
			get
			{
				return this.m_A;
			}
			[CompilerGenerated]
			private set
			{
				this.m_A = bitmap;
			}
		}

		public Graphics a
		{
			[CompilerGenerated]
			get
			{
				return this.m_A;
			}
			[CompilerGenerated]
			private set
			{
				this.m_A = graphics;
			}
		}

		[DllImport("user32.dll", EntryPoint = "GetDC")]
		private static extern IntPtr A(IntPtr P_0);

		[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
		private static extern int A(IntPtr P_0, IntPtr P_1);

		[DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
		private static extern IntPtr a(IntPtr P_0);

		[DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
		private static extern bool B(IntPtr P_0);

		[DllImport("gdi32.dll", EntryPoint = "CreateDIBSection")]
		private static extern IntPtr A(IntPtr P_0, ref A P_1, uint P_2, out IntPtr P_3, IntPtr P_4, uint P_5);

		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		private static extern IntPtr a(IntPtr P_0, IntPtr P_1);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		private static extern bool b(IntPtr P_0);

		[DllImport("user32.dll", EntryPoint = "UpdateLayeredWindow", SetLastError = true)]
		private static extern bool A(IntPtr P_0, IntPtr P_1, ref Point P_2, ref Size P_3, IntPtr P_4, ref Point P_5, int P_6, ref a P_7, int P_8);

		public void A(int P_0, int P_1)
		{
			if (this.A != null && this.A.Width == P_0 && this.A.Height == P_1)
			{
				return;
			}
			B();
			IntPtr intPtr = A(IntPtr.Zero);
			try
			{
				this.m_A = a(intPtr);
				A a2 = new A
				{
					A = (uint)Marshal.SizeOf(typeof(A)),
					A = P_0,
					a = -P_1,
					A = 1,
					a = 32
				};
				this.m_a = A(intPtr, ref a2, 0u, out this.m_b, IntPtr.Zero, 0u);
				if (this.m_A == IntPtr.Zero || this.m_a == IntPtr.Zero)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bn());
				}
				this.m_B = a(this.m_A, this.m_a);
				this.A = new Bitmap(P_0, P_1, P_0 * 4, PixelFormat.Format32bppPArgb, this.m_b);
				this.a = Graphics.FromImage(this.A);
			}
			catch
			{
				B();
				throw;
			}
			finally
			{
				A(IntPtr.Zero, intPtr);
			}
		}

		public void A(IntPtr P_0, Point P_1)
		{
			this.a.Flush(FlushIntention.Sync);
			Size size = this.A.Size;
			Point empty = Point.Empty;
			a a2 = new a
			{
				B = byte.MaxValue,
				b = 1
			};
			if (!A(P_0, IntPtr.Zero, ref P_1, ref size, this.m_A, ref empty, 0, ref a2, 2))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		private void B()
		{
			if (this.a != null)
			{
				this.a.Dispose();
				this.a = null;
			}
			if (this.A != null)
			{
				this.A.Dispose();
				this.A = null;
			}
			if (this.m_A != IntPtr.Zero && this.m_B != IntPtr.Zero)
			{
				a(this.m_A, this.m_B);
			}
			if (this.m_a != IntPtr.Zero)
			{
				b(this.m_a);
			}
			if (this.m_A != IntPtr.Zero)
			{
				B(this.m_A);
			}
			this.m_A = (this.m_a = (this.m_B = (this.m_b = IntPtr.Zero)));
		}

		public void Dispose()
		{
			B();
		}
	}
	internal sealed class n : Form
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.N, bool> A;

			public static Func<global::B.N, uint> A;

			internal bool A(global::B.N P_0)
			{
				return P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz();
			}

			internal uint a(global::B.N P_0)
			{
				return P_0.A;
			}
		}

		private readonly CheckedListBox m_A = new CheckedListBox();

		private readonly Func<global::B.N[]> m_A;

		private readonly z m_A;

		private readonly System.Windows.Forms.Timer m_A = new System.Windows.Forms.Timer();

		private bool m_A;

		public n(Func<global::B.N[]> P_0, z P_1)
		{
			this.m_A = P_0;
			this.m_A = P_1;
			Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bO();
			base.ClientSize = new Size(580, 350);
			MinimumSize = new Size(580, 350);
			Font = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), 10f);
			base.StartPosition = FormStartPosition.CenterParent;
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(14),
				ColumnCount = 1,
				RowCount = 3
			};
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 78f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
			base.Controls.Add(tableLayoutPanel);
			tableLayoutPanel.Controls.Add(new Label
			{
				Dock = DockStyle.Fill,
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bo()
			}, 0, 0);
			this.m_A.Dock = DockStyle.Fill;
			this.m_A.CheckOnClick = true;
			this.m_A.IntegralHeight = false;
			tableLayoutPanel.Controls.Add(this.m_A, 0, 1);
			CheckedListBox checkedListBox = this.m_A;
			ItemCheckEventHandler value = delegate(object obj, ItemCheckEventArgs e2)
			{
				if (!this.m_A)
				{
					this.m_A.A(((y)this.m_A.Items[e2.Index]).A, e2.NewValue == CheckState.Checked);
				}
			};
			checkedListBox.ItemCheck += value;
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill
			};
			tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 2);
			Button button = new Button
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bP(),
				AutoSize = true
			};
			Button button2 = new Button
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bp(),
				AutoSize = true
			};
			Button button3 = new Button
			{
				Text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bQ(),
				AutoSize = true
			};
			flowLayoutPanel.Controls.AddRange(new Control[3] { button, button2, button3 });
			button.Click += delegate
			{
				A(true);
			};
			button2.Click += delegate
			{
				A(false);
			};
			button3.Click += delegate
			{
				Close();
			};
			A();
			this.m_A.Interval = 1000;
			this.m_A.Tick += delegate
			{
				A();
			};
			this.m_A.Start();
		}

		private void A(bool P_0)
		{
			global::B.N[] array = this.m_A();
			foreach (global::B.N n2 in array)
			{
				this.m_A.A(n2, P_0);
			}
			A();
		}

		private void A()
		{
			global::B.N[] array = (from P_0 in this.m_A()
				where P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()
				orderby P_0.A
				select P_0).ToArray();
			this.m_A = true;
			try
			{
				bool flag = this.m_A.Items.Count == array.Length;
				int num = 0;
				while (flag && num < array.Length)
				{
					global::B.N n2 = ((y)this.m_A.Items[num]).A;
					flag = n2.A == array[num].A && n2.A == array[num].A;
					num++;
				}
				if (!flag)
				{
					this.m_A.Items.Clear();
					global::B.N[] array2 = array;
					foreach (global::B.N n3 in array2)
					{
						this.m_A.Items.Add(new y
						{
							A = n3
						}, this.m_A.a(n3));
					}
				}
				else
				{
					for (int num3 = 0; num3 < array.Length; num3++)
					{
						((y)this.m_A.Items[num3]).A = array[num3];
						this.m_A.SetItemChecked(num3, this.m_A.a(array[num3]));
					}
				}
			}
			finally
			{
				this.m_A = false;
			}
		}

		protected override void Dispose(bool P_0)
		{
			if (P_0)
			{
				this.m_A.Dispose();
			}
			base.Dispose(P_0);
		}

		[CompilerGenerated]
		private void A(object P_0, ItemCheckEventArgs P_1)
		{
			if (!this.m_A)
			{
				this.m_A.A(((y)this.m_A.Items[P_1.Index]).A, P_1.NewValue == CheckState.Checked);
			}
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			A(true);
		}

		[CompilerGenerated]
		private void a(object P_0, EventArgs P_1)
		{
			A(false);
		}

		[CompilerGenerated]
		private void B(object P_0, EventArgs P_1)
		{
			Close();
		}

		[CompilerGenerated]
		private void b(object P_0, EventArgs P_1)
		{
			A();
		}
	}
}
namespace a
{
	internal static class N
	{
		private static n m_A = new n();

		private static long m_A;

		public static bool A => Volatile.Read(ref N.m_A).A > k.A;

		public static long a => Volatile.Read(ref N.m_A).a;

		public static bool A(int P_0, DateTime P_1)
		{
			n n2 = Volatile.Read(ref N.m_A);
			if (N.A && n2.A == P_0)
			{
				return n2.A == P_1;
			}
			return false;
		}

		internal static bool A(long P_0, long P_1, long P_2, long P_3, e P_4)
		{
			long num = Math.Min(P_1, P_2) - P_0;
			if (P_4 == null || num <= 0 || num > 60 || P_2 < P_1)
			{
				return false;
			}
			long num2 = P_3 + num * 1000 - 1000;
			if (num2 <= k.A)
			{
				return false;
			}
			n n2 = Volatile.Read(ref N.m_A);
			long num3 = ((n2.A > k.A && n2.A == P_4.A && n2.A == P_4.A) ? n2.a : Interlocked.Increment(ref N.m_A));
			Volatile.Write(ref N.m_A, new n
			{
				A = num2,
				a = num3,
				A = P_4.A,
				A = P_4.A
			});
			return true;
		}

		public static void B()
		{
			Volatile.Write(ref N.m_A, new n
			{
				a = Interlocked.Increment(ref N.m_A)
			});
		}
	}
	internal sealed class n
	{
		public long A;

		public long a;

		public int A;

		public DateTime A;
	}
}
