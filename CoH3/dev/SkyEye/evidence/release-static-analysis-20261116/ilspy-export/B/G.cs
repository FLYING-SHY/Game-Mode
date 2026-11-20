using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using B;
using a;

namespace B
{
	internal static class G
	{
		public struct A
		{
			public IntPtr A;

			public IntPtr a;

			public uint A;

			public UIntPtr A;

			public uint a;

			public uint B;

			public uint b;
		}

		[DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
		public static extern o A(uint P_0, bool P_1, int P_2);

		[DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
		public static extern bool A(IntPtr P_0);

		[DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
		private static extern bool A(o P_0, IntPtr P_1, byte[] P_2, UIntPtr P_3, out UIntPtr P_4);

		public static bool a(o P_0, IntPtr P_1, byte[] P_2, UIntPtr P_3, out UIntPtr P_4)
		{
			if (!global::a.N.A)
			{
				P_4 = UIntPtr.Zero;
				return false;
			}
			return A(P_0, P_1, P_2, P_3, out P_4);
		}

		[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx", SetLastError = true)]
		public static extern UIntPtr A(o P_0, IntPtr P_1, out A P_2, UIntPtr P_3);

		[DllImport("user32.dll", EntryPoint = "RegisterHotKey", SetLastError = true)]
		public static extern bool A(IntPtr P_0, int P_1, uint P_2, uint P_3);

		[DllImport("user32.dll", EntryPoint = "UnregisterHotKey")]
		public static extern bool A(IntPtr P_0, int P_1);

		[DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
		public static extern IntPtr A();

		[DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
		public static extern uint A(IntPtr P_0, out uint P_1);
	}
	internal sealed class g
	{
		public int A;

		public int a;

		public double A;

		public X A;
	}
}
namespace b
{
	internal static class G
	{
		public static double A(string P_0)
		{
			if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LH()))
			{
				if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lh()))
				{
					return 60.0;
				}
				return 13.5;
			}
			return 34.0;
		}
	}
	internal sealed class g : Panel
	{
		public g()
		{
			DoubleBuffered = true;
			BackColor = global::B.K.A;
		}

		protected override void OnPaintBackground(PaintEventArgs P_0)
		{
			base.OnPaintBackground(P_0);
			using (Pen pen = new Pen(Color.FromArgb(14, 29, 41)))
			{
				for (int m = 20; m < base.Width; m += 48)
				{
					P_0.Graphics.DrawLine(pen, m, 0, m, base.Height);
				}
				for (int n = 20; n < base.Height; n += 48)
				{
					P_0.Graphics.DrawLine(pen, 0, n, base.Width, n);
				}
			}
			global::B.K.A(P_0.Graphics, new Rectangle(0, 0, base.Width, base.Height), global::B.K.c);
			using Pen pen2 = new Pen(global::B.K.D);
			P_0.Graphics.DrawLine(pen2, 12, 4, 70, 4);
			P_0.Graphics.DrawLine(pen2, base.Width - 12, base.Height - 5, base.Width - 70, base.Height - 5);
		}
	}
}
